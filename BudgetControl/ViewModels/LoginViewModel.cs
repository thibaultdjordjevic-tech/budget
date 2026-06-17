using BudgetControl.Services;

namespace BudgetControl.ViewModels
{
    /// <summary>
    /// ViewModel pour la page de connexion / inscription.
    /// Gère les deux modes (connexion / création de compte) sur une même page.
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        // Champs de saisie
        private string _email        = string.Empty;
        private string _password     = string.Empty;
        private string _nom          = string.Empty;
        private string _prenom       = string.Empty;
        private bool   _isRegisterMode;
        private string _errorMessage = string.Empty;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string Nom
        {
            get => _nom;
            set => SetProperty(ref _nom, value);
        }

        public string Prenom
        {
            get => _prenom;
            set => SetProperty(ref _prenom, value);
        }

        public bool IsRegisterMode
        {
            get => _isRegisterMode;
            set
            {
                SetProperty(ref _isRegisterMode, value);
                OnPropertyChanged(nameof(PageTitle));
                OnPropertyChanged(nameof(SwitchModeLabel));
                OnPropertyChanged(nameof(ActionButtonLabel));
                ErrorMessage = string.Empty;
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Libellés dynamiques
        public string PageTitle         => IsRegisterMode ? "Créer un compte"  : "Connexion";
        public string ActionButtonLabel => IsRegisterMode ? "S'inscrire"        : "Se connecter";
        public string SwitchModeLabel   => IsRegisterMode
            ? "Déjà un compte ? Se connecter"
            : "Pas de compte ? S'inscrire";

        // Commandes
        public RelayCommand LoginOrRegisterCommand { get; }
        public RelayCommand SwitchModeCommand      { get; }

        public LoginViewModel()
        {
            Title                  = "Budget Control";
            LoginOrRegisterCommand = new RelayCommand(async () => await LoginOrRegisterAsync());
            SwitchModeCommand      = new RelayCommand(() => IsRegisterMode = !IsRegisterMode);
        }

        private async Task LoginOrRegisterAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                (bool success, string message) result = IsRegisterMode
                    ? await AuthService.Instance.RegisterAsync(Email, Password, Nom, Prenom)
                    : await AuthService.Instance.LoginAsync(Email, Password);

                if (result.success)
                    await Shell.Current.GoToAsync("//MainPage");
                else
                    ErrorMessage = result.message;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur inattendue : {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
