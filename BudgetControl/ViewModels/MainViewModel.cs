using System.Collections.ObjectModel;
using BudgetControl.Models;
using BudgetControl.Services;

namespace BudgetControl.ViewModels
{
    /// <summary>
    /// ViewModel du tableau de bord — affiche le solde global et les dernières transactions
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private decimal _soldeGlobal;
        private decimal _totalDepenses;
        private decimal _totalRevenus;
        private int _moisSelectionne = DateTime.Now.Month;
        private int _anneeSelectionnee = DateTime.Now.Year;

        public decimal SoldeGlobal
        {
            get => _soldeGlobal;
            set
            {
                SetProperty(ref _soldeGlobal, value);
                OnPropertyChanged(nameof(SoldeFormate));
                OnPropertyChanged(nameof(CouleurSolde));
            }
        }

        public decimal TotalDepenses
        {
            get => _totalDepenses;
            set { SetProperty(ref _totalDepenses, value); OnPropertyChanged(nameof(TotalDepensesFormate)); }
        }

        public decimal TotalRevenus
        {
            get => _totalRevenus;
            set { SetProperty(ref _totalRevenus, value); OnPropertyChanged(nameof(TotalRevenusFormate)); }
        }

        public string SoldeFormate      => $"{SoldeGlobal:C2}";
        public string TotalDepensesFormate => $"-{TotalDepenses:C2}";
        public string TotalRevenusFormate  => $"+{TotalRevenus:C2}";
        public string CouleurSolde      => SoldeGlobal >= 0 ? "#06D6A0" : "#FF6B6B";

        public int MoisSelectionne
        {
            get => _moisSelectionne;
            set { SetProperty(ref _moisSelectionne, value); OnPropertyChanged(nameof(PeriodeLabel)); }
        }

        public int AnneeSelectionnee
        {
            get => _anneeSelectionnee;
            set { SetProperty(ref _anneeSelectionnee, value); OnPropertyChanged(nameof(PeriodeLabel)); }
        }

        public string PeriodeLabel => new DateTime(AnneeSelectionnee, MoisSelectionne, 1)
                                          .ToString("MMMM yyyy");

        public string NomUtilisateur => AuthService.Instance.CurrentUser?.Prenom ?? "Utilisateur";

        /// <summary>
        /// Les 5 dernières transactions du mois
        /// </summary>
        public ObservableCollection<Transaction> DernieresTransactions { get; } = new();

        // Commandes
        public RelayCommand LoadDataCommand  { get; }
        public RelayCommand MoisPrecedentCommand { get; }
        public RelayCommand MoisSuivantCommand   { get; }
        public RelayCommand LogoutCommand        { get; }
        public RelayCommand NavigateDepensesCommand   { get; }
        public RelayCommand NavigateRevenusCommand    { get; }
        public RelayCommand NavigateBudgetsCommand    { get; }
        public RelayCommand NavigateSyntheseCommand   { get; }

        public MainViewModel()
        {
            Title = "Accueil";

            LoadDataCommand          = new RelayCommand(async () => await LoadDataAsync());
            MoisPrecedentCommand     = new RelayCommand(async () => await ChangerMoisAsync(-1));
            MoisSuivantCommand       = new RelayCommand(async () => await ChangerMoisAsync(+1));
            LogoutCommand            = new RelayCommand(LogoutAsync);
            NavigateDepensesCommand = new RelayCommand(async () => await Shell.Current.GoToAsync("//DepensesPage"));
            NavigateRevenusCommand = new RelayCommand(async () => await Shell.Current.GoToAsync("//RevenusPage"));
            NavigateBudgetsCommand = new RelayCommand(async () => await Shell.Current.GoToAsync("//BudgetsPage"));
            NavigateSyntheseCommand = new RelayCommand(async () => await Shell.Current.GoToAsync("//SynthesePage"));
        
        }

        public async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var userId = AuthService.Instance.CurrentUser?.Id ?? 0;
                var transactions = await DatabaseService.Instance
                    .GetTransactionsAsync(userId, MoisSelectionne, AnneeSelectionnee);

                TotalRevenus  = transactions.Where(t => t.Type == "Revenu").Sum(t => t.Montant);
                TotalDepenses = transactions.Where(t => t.Type == "Depense").Sum(t => t.Montant);
                SoldeGlobal   = TotalRevenus - TotalDepenses;

                DernieresTransactions.Clear();
                foreach (var t in transactions.Take(5))
                    DernieresTransactions.Add(t);

                OnPropertyChanged(nameof(NomUtilisateur));
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ChangerMoisAsync(int delta)
        {
            var date = new DateTime(AnneeSelectionnee, MoisSelectionne, 1).AddMonths(delta);
            AnneeSelectionnee = date.Year;
            MoisSelectionne   = date.Month;
            await LoadDataAsync();
        }

        private void LogoutAsync()
        {
            AuthService.Instance.Logout();
            Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
