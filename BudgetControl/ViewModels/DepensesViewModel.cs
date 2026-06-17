using System.Collections.ObjectModel;
using BudgetControl.Models;
using BudgetControl.Services;

namespace BudgetControl.ViewModels
{
    public class DepensesViewModel : BaseViewModel
    {
        private Transaction? _depenseSelectionnee;
        private Transaction  _formulaire          = new() { Type = "Depense", Date = DateTime.Now };
        private string       _categorieSelectionnee = "Alimentation";
        private bool         _isFormVisible;
        private int          _mois              = DateTime.Now.Month;
        private int          _annee             = DateTime.Now.Year;
        private string       _filtreCategorie   = "Toutes";
        private DateTime     _filtreDebut       = new(DateTime.Now.Year, DateTime.Now.Month, 1);
        private DateTime     _filtreFin         = DateTime.Now;

        private static readonly List<string> _categoriesLabels = new()
        {
            "🏥 Santé", "🛒 Alimentation", "💰 Épargne",
            "🚗 Transport", "🎮 Loisirs", "🛍️ Shopping", "🏠 Logement", "📄 Frais fixes"
        };

        public ObservableCollection<Transaction> Depenses { get; } = new();

        public List<string> CategoriesSaisie { get; } = new()
        {
            "Sante", "Alimentation", "Epargne",
            "Transport", "Loisirs", "Shopping", "Logement", "FraisFixes"
        };

        public Transaction? DepenseSelectionnee
        {
            get => _depenseSelectionnee;
            set { SetProperty(ref _depenseSelectionnee, value); if (value != null) OuvrirEdition(value); }
        }

        public Transaction Formulaire
        {
            get => _formulaire;
            set => SetProperty(ref _formulaire, value);
        }

        public string CategorieSelectionnee
        {
            get => _categorieSelectionnee;
            set
            {
                SetProperty(ref _categorieSelectionnee, value);
                Formulaire.Categorie = value;
                OnPropertyChanged(nameof(CategorieLabel));
            }
        }

        public string CategorieLabel => _categorieSelectionnee switch
        {
            "Sante"        => "🏥 Santé",
            "Alimentation" => "🛒 Alimentation",
            "Epargne"      => "💰 Épargne",
            "Transport"    => "🚗 Transport",
            "Loisirs"      => "🎮 Loisirs",
            "Shopping"     => "🛍️ Shopping",
            "Logement"     => "🏠 Logement",
            "FraisFixes"   => "📄 Frais fixes",
            _              => "Choisir une catégorie"
        };

        public bool IsFormVisible
        {
            get => _isFormVisible;
            set => SetProperty(ref _isFormVisible, value);
        }

        public string FiltreCategorie
        {
            get => _filtreCategorie;
            set { SetProperty(ref _filtreCategorie, value); _ = LoadAsync(); }
        }

        public DateTime FiltreDebut
        {
            get => _filtreDebut;
            set { SetProperty(ref _filtreDebut, value); _ = LoadAsync(); }
        }

        public DateTime FiltreFin
        {
            get => _filtreFin;
            set { SetProperty(ref _filtreFin, value); _ = LoadAsync(); }
        }

        public int Mois  { get => _mois;  set { SetProperty(ref _mois, value);  _ = LoadAsync(); } }
        public int Annee { get => _annee; set { SetProperty(ref _annee, value); _ = LoadAsync(); } }

        public string  PeriodeLabel  => new DateTime(Annee, Mois, 1).ToString("MMMM yyyy");
        public decimal TotalDepenses => Depenses.Sum(t => t.Montant);
        public string  TotalFormate  => $"{TotalDepenses:C2}";

        public RelayCommand LoadCommand             { get; }
        public RelayCommand NouvelleDepenseCommand  { get; }
        public RelayCommand SauvegarderCommand      { get; }
        public RelayCommand SupprimerCommand        { get; }
        public RelayCommand AnnulerCommand          { get; }
        public RelayCommand MoisPrecedentCommand    { get; }
        public RelayCommand MoisSuivantCommand      { get; }
        public RelayCommand ChoisirCategorieCommand { get; }

        public DepensesViewModel()
        {
            Title                  = "Mes dépenses";
            LoadCommand            = new RelayCommand(async () => await LoadAsync());
            NouvelleDepenseCommand = new RelayCommand(NouvelleDepense);
            SauvegarderCommand     = new RelayCommand(async () => await SauvegarderAsync());
            SupprimerCommand       = new RelayCommand(async () => await SupprimerAsync());
            ChoisirCategorieCommand = new RelayCommand(async () => await ChoisirCategorieAsync());

            AnnulerCommand = new RelayCommand(() =>
            {
                IsFormVisible = false;
                ResetFormulaire();
                _depenseSelectionnee = null;
                OnPropertyChanged(nameof(DepenseSelectionnee));
            });

            MoisPrecedentCommand = new RelayCommand(async () => await ChangerMoisAsync(-1));
            MoisSuivantCommand   = new RelayCommand(async () => await ChangerMoisAsync(+1));
        }

        public async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var userId = AuthService.Instance.CurrentUser?.Id ?? 0;

                List<Transaction> all;
                if (_filtreDebut != new DateTime(Annee, Mois, 1) || _filtreFin != DateTime.Now)
                {
                    var toutes = await DatabaseService.Instance.GetAllTransactionsAsync(userId);
                    all = toutes
                        .Where(t => t.Date >= FiltreDebut && t.Date <= FiltreFin.AddDays(1))
                        .ToList();
                }
                else
                {
                    all = await DatabaseService.Instance.GetTransactionsAsync(userId, Mois, Annee);
                }

                var depenses = all.Where(t => t.Type == "Depense");
                if (FiltreCategorie != "Toutes")
                    depenses = depenses.Where(t => t.Categorie == FiltreCategorie);

                Depenses.Clear();
                foreach (var d in depenses.OrderByDescending(t => t.Date))
                    Depenses.Add(d);

                OnPropertyChanged(nameof(TotalDepenses));
                OnPropertyChanged(nameof(TotalFormate));
                OnPropertyChanged(nameof(PeriodeLabel));
            }
            finally { IsBusy = false; }
        }

        private void NouvelleDepense()
        {
            ResetFormulaire();
            _depenseSelectionnee = null;
            OnPropertyChanged(nameof(DepenseSelectionnee));
            IsFormVisible = true;
        }

        private void OuvrirEdition(Transaction t)
        {
            Formulaire = new Transaction
            {
                Id          = t.Id,
                Montant     = t.Montant,
                Date        = t.Date,
                Description = t.Description,
                Categorie   = t.Categorie,
                Type        = "Depense",
                UserId      = t.UserId
            };
            CategorieSelectionnee = t.Categorie;
            IsFormVisible = true;
        }

        private async Task ChoisirCategorieAsync()
        {
            var result = await Shell.Current.DisplayActionSheet(
                "Catégorie", "Annuler", null,
                _categoriesLabels.ToArray());

            if (result is null || result == "Annuler") return;

            var idx = _categoriesLabels.IndexOf(result);
            if (idx >= 0)
                CategorieSelectionnee = CategoriesSaisie[idx];
        }

        private async Task SauvegarderAsync()
        {
            if (Formulaire.Montant <= 0)
            {
                await Shell.Current.DisplayAlert("Erreur", "Le montant doit être supérieur à 0.", "OK");
                return;
            }
            if (string.IsNullOrEmpty(Formulaire.Categorie))
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez sélectionner une catégorie.", "OK");
                return;
            }

            Formulaire.UserId = AuthService.Instance.CurrentUser?.Id ?? 0;
            Formulaire.Type   = "Depense";
            await DatabaseService.Instance.SaveTransactionAsync(Formulaire);

            IsFormVisible = false;
            ResetFormulaire();
            _depenseSelectionnee = null;
            OnPropertyChanged(nameof(DepenseSelectionnee));
            await LoadAsync();
        }

        private async Task SupprimerAsync()
        {
            if (_depenseSelectionnee is null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirmation", "Supprimer cette dépense ?", "Oui", "Annuler");
            if (!confirm) return;

            await DatabaseService.Instance.DeleteTransactionAsync(_depenseSelectionnee);
            IsFormVisible = false;
            ResetFormulaire();
            _depenseSelectionnee = null;
            OnPropertyChanged(nameof(DepenseSelectionnee));
            await LoadAsync();
        }

        private async Task ChangerMoisAsync(int delta)
        {
            var date = new DateTime(Annee, Mois, 1).AddMonths(delta);
            Annee       = date.Year;
            Mois        = date.Month;
            FiltreDebut = new DateTime(date.Year, date.Month, 1);
            FiltreFin   = FiltreDebut.AddMonths(1).AddDays(-1);
        }

        private void ResetFormulaire()
        {
            Formulaire = new Transaction
            {
                Type      = "Depense",
                Date      = DateTime.Now,
                UserId    = AuthService.Instance.CurrentUser?.Id ?? 0,
                Categorie = "Alimentation"
            };
            CategorieSelectionnee = "Alimentation";
        }
    }
}
