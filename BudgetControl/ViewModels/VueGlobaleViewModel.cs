using System.Collections.ObjectModel;
using BudgetControl.Models;
using BudgetControl.Services;

namespace BudgetControl.ViewModels
{
    public class VueGlobaleViewModel : BaseViewModel
    {
        private decimal _totalRevenus;
        private decimal _totalDepenses;
        private decimal _solde;
        private int     _moisSelectionne   = DateTime.Now.Month;
        private int     _anneeSelectionnee = DateTime.Now.Year;

        public decimal TotalRevenus
        {
            get => _totalRevenus;
            set { SetProperty(ref _totalRevenus, value); OnPropertyChanged(nameof(ProgressRevenus)); }
        }

        public decimal TotalDepenses
        {
            get => _totalDepenses;
            set { SetProperty(ref _totalDepenses, value); OnPropertyChanged(nameof(ProgressDepenses)); }
        }

        public decimal Solde
        {
            get => _solde;
            set { SetProperty(ref _solde, value); OnPropertyChanged(nameof(CouleurSolde)); }
        }

        public string CouleurSolde => Solde >= 0 ? "#81C784" : "#E57373";

        public double ProgressRevenus
        {
            get
            {
                var max = Math.Max(TotalRevenus, TotalDepenses);
                return max == 0 ? 0 : (double)(TotalRevenus / max);
            }
        }

        public double ProgressDepenses
        {
            get
            {
                var max = Math.Max(TotalRevenus, TotalDepenses);
                return max == 0 ? 0 : (double)(TotalDepenses / max);
            }
        }

        public int MoisSelectionne
        {
            get => _moisSelectionne;
            set { SetProperty(ref _moisSelectionne, value); OnPropertyChanged(nameof(PeriodeLabel)); _ = ChargerAsync(); }
        }

        public int AnneeSelectionnee
        {
            get => _anneeSelectionnee;
            set { SetProperty(ref _anneeSelectionnee, value); OnPropertyChanged(nameof(PeriodeLabel)); _ = ChargerAsync(); }
        }

        public string PeriodeLabel
            => new DateTime(_anneeSelectionnee, _moisSelectionne, 1).ToString("MMMM yyyy");

        public ObservableCollection<StatCategorie> StatsCategories { get; } = new();

        public RelayCommand RefreshCommand       { get; }
        public RelayCommand MoisPrecedentCommand { get; }
        public RelayCommand MoisSuivantCommand   { get; }

        public VueGlobaleViewModel()
        {
            Title                = "Vue globale";
            RefreshCommand       = new RelayCommand(async () => await ChargerAsync());
            MoisPrecedentCommand = new RelayCommand(async () => await ChangerMoisAsync(-1));
            MoisSuivantCommand   = new RelayCommand(async () => await ChangerMoisAsync(+1));
        }

        private async Task ChangerMoisAsync(int delta)
        {
            var date = new DateTime(_anneeSelectionnee, _moisSelectionne, 1).AddMonths(delta);
            _anneeSelectionnee = date.Year;
            _moisSelectionne   = date.Month;
            OnPropertyChanged(nameof(AnneeSelectionnee));
            OnPropertyChanged(nameof(MoisSelectionne));
            OnPropertyChanged(nameof(PeriodeLabel));
            await ChargerAsync();
        }

        public async Task ChargerAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var userId = AuthService.Instance.CurrentUser?.Id ?? 0;

                TotalRevenus  = await DatabaseService.Instance.GetTotalRevenusAsync(userId, _moisSelectionne, _anneeSelectionnee);
                TotalDepenses = await DatabaseService.Instance.GetTotalDepensesAsync(userId, _moisSelectionne, _anneeSelectionnee);
                Solde         = TotalRevenus - TotalDepenses;

                var statsRaw = await DatabaseService.Instance
                    .GetDepensesParCategorieAsync(userId, _moisSelectionne, _anneeSelectionnee);

                var couleurs = new[]
                {
                    "#E57373","#81C784","#64B5F6","#FFB74D",
                    "#BA68C8","#4DB6AC","#90A4AE","#F06292"
                };

                StatsCategories.Clear();
                int idx = 0;
                foreach (var kv in statsRaw.OrderByDescending(x => x.Value))
                {
                    var fakeTx = new Transaction { Categorie = kv.Key };
                    StatsCategories.Add(new StatCategorie
                    {
                        Nom         = fakeTx.CategorieLabel,
                        Icone       = GetIcone(kv.Key),
                        Total       = kv.Value,
                        Couleur     = couleurs[idx % couleurs.Length],
                        Pourcentage = TotalDepenses > 0
                            ? (double)(kv.Value / TotalDepenses * 100) : 0
                    });
                    idx++;
                }
            }
            finally { IsBusy = false; }
        }

        private static string GetIcone(string cat) => cat switch
        {
            "Sante"        => "🏥",
            "Alimentation" => "🛒",
            "Epargne"      => "💰",
            "Transport"    => "🚗",
            "Loisirs"      => "🎮",
            "Shopping"     => "🛍️",
            "Logement"     => "🏠",
            "FraisFixes"   => "📄",
            _              => "💳"
        };
    }

    public class StatCategorie
    {
        public string  Nom         { get; set; } = string.Empty;
        public string  Icone       { get; set; } = string.Empty;
        public decimal Total       { get; set; }
        public decimal? Budget     { get; set; }
        public string  Couleur     { get; set; } = "#90A4AE";
        public double  Pourcentage { get; set; }

        public string Label => Budget.HasValue
            ? $"{Total:C2} / {Budget.Value:C2}"
            : $"{Total:C2}";

        public double ProgressValue => Budget.HasValue && Budget.Value > 0
            ? Math.Min((double)(Total / Budget.Value), 1.0)
            : 0.0;

        public bool DepaseBudget => Budget.HasValue && Total > Budget.Value;
    }
}
