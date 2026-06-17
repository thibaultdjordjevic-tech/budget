using System.Collections.ObjectModel;
using BudgetControl.Models;
using BudgetControl.Services;

namespace BudgetControl.ViewModels
{
    /// <summary>
    /// Données d'un segment de graphique camembert
    /// </summary>
    public class ChartSegment
    {
        public string Label      { get; set; } = string.Empty;
        public decimal Valeur    { get; set; }
        public string Couleur    { get; set; } = "#90A4AE";
        public string Pourcentage { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel de la page de synthèse — solde global + répartition par catégorie
    /// </summary>
    public class SyntheseViewModel : BaseViewModel
    {
        private decimal _solde;
        private decimal _totalDepenses;
        private decimal _totalRevenus;
        private int _mois = DateTime.Now.Month;
        private int _annee = DateTime.Now.Year;

        public decimal Solde          { get => _solde;         set { SetProperty(ref _solde, value);         OnPropertyChanged(nameof(SoldeFormate)); OnPropertyChanged(nameof(CouleurSolde)); } }
        public decimal TotalDepenses  { get => _totalDepenses; set { SetProperty(ref _totalDepenses, value); OnPropertyChanged(nameof(TotalDepensesFormate)); OnPropertyChanged(nameof(ProgressDepenses)); OnPropertyChanged(nameof(ProgressRevenus)); } }
        public decimal TotalRevenus   { get => _totalRevenus;  set { SetProperty(ref _totalRevenus, value);  OnPropertyChanged(nameof(TotalRevenusFormate));  OnPropertyChanged(nameof(ProgressRevenus));  OnPropertyChanged(nameof(ProgressDepenses)); } }

        public string SoldeFormate         => $"{Solde:C2}";
        public string TotalDepensesFormate => $"{TotalDepenses:C2}";
        public string TotalRevenusFormate  => $"{TotalRevenus:C2}";
        public string CouleurSolde         => Solde >= 0 ? "#06D6A0" : "#FF6B6B";

        /// <summary>Progression normalée pour la barre Revenus (0.0 – 1.0).</summary>
        public double ProgressRevenus
        {
            get
            {
                var max = Math.Max((double)TotalRevenus, (double)TotalDepenses);
                return max > 0 ? Math.Min((double)TotalRevenus / max, 1.0) : 0.0;
            }
        }

        /// <summary>Progression normalisée pour la barre Dépenses (0.0 – 1.0).</summary>
        public double ProgressDepenses
        {
            get
            {
                var max = Math.Max((double)TotalRevenus, (double)TotalDepenses);
                return max > 0 ? Math.Min((double)TotalDepenses / max, 1.0) : 0.0;
            }
        }

        public int Mois  { get => _mois;  set { SetProperty(ref _mois, value);  OnPropertyChanged(nameof(PeriodeLabel)); } }
        public int Annee { get => _annee; set { SetProperty(ref _annee, value); OnPropertyChanged(nameof(PeriodeLabel)); } }
        public string PeriodeLabel => new DateTime(Annee, Mois, 1).ToString("MMMM yyyy");

        /// <summary>
        /// Segments pour le graphique camembert des dépenses par catégorie
        /// </summary>
        public ObservableCollection<ChartSegment> SegmentsDepenses { get; } = new();

        public RelayCommand LoadCommand          { get; }
        public RelayCommand MoisPrecedentCommand { get; }
        public RelayCommand MoisSuivantCommand   { get; }

        private static readonly string[] _couleursCat =
        {
            "#4361EE","#FF6B6B","#06D6A0","#FFB627",
            "#5E35B1","#26C6DA","#FF7043","#AB47BC"
        };

        public SyntheseViewModel()
        {
            Title = "Synthèse";
            LoadCommand          = new RelayCommand(async () => await LoadAsync());
            MoisPrecedentCommand = new RelayCommand(async () => await ChangerMoisAsync(-1));
            MoisSuivantCommand   = new RelayCommand(async () => await ChangerMoisAsync(+1));
        }

        public async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var userId       = AuthService.Instance.CurrentUser?.Id ?? 0;
                var transactions = await DatabaseService.Instance.GetTransactionsAsync(userId, Mois, Annee);

                TotalRevenus  = transactions.Where(t => t.Type == "Revenu").Sum(t => t.Montant);
                TotalDepenses = transactions.Where(t => t.Type == "Depense").Sum(t => t.Montant);
                Solde         = TotalRevenus - TotalDepenses;

                // Répartition par catégorie
                SegmentsDepenses.Clear();
                var depenses = transactions.Where(t => t.Type == "Depense").ToList();
                var groupes  = depenses.GroupBy(t => t.Categorie)
                                       .OrderByDescending(g => g.Sum(t => t.Montant))
                                       .ToList();

                for (int i = 0; i < groupes.Count; i++)
                {
                    var g       = groupes[i];
                    var montant = g.Sum(t => t.Montant);
                    var pct     = TotalDepenses > 0 ? (montant / TotalDepenses * 100) : 0;

                    SegmentsDepenses.Add(new ChartSegment
                    {
                        Label       = g.First().CategorieLabel,
                        Valeur      = montant,
                        Couleur     = _couleursCat[i % _couleursCat.Length],
                        Pourcentage = $"{pct:F1}%"
                    });
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ChangerMoisAsync(int delta)
        {
            var date = new DateTime(Annee, Mois, 1).AddMonths(delta);

            Annee = date.Year;
            Mois = date.Month;

            await LoadAsync();
        }
    }
}
