using BudgetControl.Models;
using BudgetControl.Services;

namespace BudgetControl.ViewModels
{
    /// <summary>
    /// ViewModel pour l'ajout et la modification d'une transaction (dépense ou revenu).
    /// Paramètres de navigation Shell : ?type=Depense|Revenu et optionnel &amp;id=xxx
    /// </summary>
    [QueryProperty(nameof(TransactionId),   "id")]
    [QueryProperty(nameof(TypeTransaction), "type")]
    public class AddTransactionViewModel : BaseViewModel
    {
        private int      _transactionId;
        private string   _typeTransaction       = "Depense";
        private decimal  _montant;
        private string   _description           = string.Empty;
        private int      _categorieIndex        = 1; // Alimentation par défaut
        private DateTime _date                  = DateTime.Now;
        private Transaction? _existingTransaction;

        // Propriétés de navigation QueryProperty

        public int TransactionId
        {
            get => _transactionId;
            set { SetProperty(ref _transactionId, value); _ = LoadExistingAsync(); }
        }

        public string TypeTransaction
        {
            get => _typeTransaction;
            set
            {
                SetProperty(ref _typeTransaction, value);
                OnPropertyChanged(nameof(IsDepense));
                OnPropertyChanged(nameof(PageTitle));
                Title = PageTitle;
            }
        }

        // Champs du formulaire

        public decimal Montant
        {
            get => _montant;
            set => SetProperty(ref _montant, value);
        }

        /// <summary>Texte bindé sur l'Entry, converti en decimal.</summary>
        public string MontantText
        {
            get => _montant == 0 ? string.Empty : _montant.ToString("F2");
            set
            {
                if (decimal.TryParse(value?.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var v))
                    Montant = v;
            }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>Index du Picker catégorie (0-based sur CategoriesLabels).</summary>
        public int CategorieIndex
        {
            get => _categorieIndex;
            set
            {
                SetProperty(ref _categorieIndex, value);
                OnPropertyChanged(nameof(CategorieSelectionnee));
                OnPropertyChanged(nameof(CategorieLabel));
            }
        }

        /// <summary>Clé interne de la catégorie sélectionnée.</summary>
        public string CategorieSelectionnee
            => CategorieIndex >= 0 && CategorieIndex < Categories.Count
                ? Categories[CategorieIndex]
                : "Alimentation";

        /// <summary>Label affiché dans le bouton catégorie.</summary>
        public string CategorieLabel
            => CategorieIndex >= 0 && CategorieIndex < CategoriesLabels.Count
                ? CategoriesLabels[CategorieIndex]
                : "Choisir une catégorie";

        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        public bool   IsDepense  => TypeTransaction == "Depense";
        public string PageTitle  => _transactionId > 0
            ? (IsDepense ? "Modifier dépense" : "Modifier revenu")
            : (IsDepense ? "Ajouter dépense"  : "Ajouter revenu");

        // Listes pour le Picker catégorie 

        public List<string> Categories { get; } = new()
        {
            "Sante", "Alimentation", "Epargne",
            "Transport", "Loisirs", "Shopping", "Logement", "FraisFixes"
        };

        public List<string> CategoriesLabels { get; } = new()
        {
            "🏥 Santé", "🛒 Alimentation", "💰 Épargne",
            "🚗 Transport", "🎮 Loisirs", "🛍️ Shopping", "🏠 Logement", "📄 Frais fixes"
        };

        // Commandes

        public RelayCommand SauvegarderCommand      { get; }
        public RelayCommand AnnulerCommand          { get; }
        public RelayCommand ChoisirCategorieCommand { get; }

        public AddTransactionViewModel()
        {
            SauvegarderCommand      = new RelayCommand(async () => await SauvegarderAsync());
            AnnulerCommand          = new RelayCommand(async () => await Shell.Current.GoToAsync(".."));
            ChoisirCategorieCommand = new RelayCommand(async () => await ChoisirCategorieAsync());
            Title = PageTitle;
        }

        private async Task ChoisirCategorieAsync()
        {
            var result = await Shell.Current.DisplayActionSheet(
                "Choisir une catégorie", "Annuler", null,
                CategoriesLabels.ToArray());

            if (result is null || result == "Annuler") return;

            var idx = CategoriesLabels.IndexOf(result);
            if (idx >= 0)
            {
                CategorieIndex = idx;
                OnPropertyChanged(nameof(CategorieLabel));
            }
        }

        /// <summary>Charge une transaction existante si on est en mode édition.</summary>
        private async Task LoadExistingAsync()
        {
            if (TransactionId <= 0) return;

            var userId = AuthService.Instance.CurrentUser?.Id ?? 0;
            var all    = await DatabaseService.Instance.GetAllTransactionsAsync(userId);
            _existingTransaction = all.FirstOrDefault(t => t.Id == TransactionId);

            if (_existingTransaction is null) return;

            Montant      = _existingTransaction.Montant;
            Description  = _existingTransaction.Description ?? string.Empty;
            Date         = _existingTransaction.Date;

            var catIdx = Categories.IndexOf(_existingTransaction.Categorie);
            CategorieIndex = catIdx >= 0 ? catIdx : 1;

            OnPropertyChanged(nameof(PageTitle));
        }

        private async Task SauvegarderAsync()
        {
            if (IsBusy) return;

            if (Montant <= 0)
            {
                await Shell.Current.DisplayAlert("Erreur", "Le montant doit être supérieur à 0.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var userId      = AuthService.Instance.CurrentUser?.Id ?? 0;
                var transaction = _existingTransaction ?? new Transaction { UserId = userId };

                transaction.Type        = TypeTransaction;
                transaction.Montant     = Montant;
                transaction.Description = Description;
                transaction.Date        = Date;
                transaction.Categorie   = IsDepense ? CategorieSelectionnee : string.Empty;

                await DatabaseService.Instance.SaveTransactionAsync(transaction);
                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
