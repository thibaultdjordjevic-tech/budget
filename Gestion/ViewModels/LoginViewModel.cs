using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gestion.Services;

namespace Gestion.ViewModels
{
    //ViewModel pour la page de connexion
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _auth; // Service d'authentification
        // Propriétés pour l'email et le mot de passe saisies par l'utilisateur
        public string Email { get; set; }
        public string Password { get; set; }
        // Commandes liées aux boutons de la vue
        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        // Constructeur qui initialise le service d'authentification et les commandes
        public LoginViewModel(AuthService auth)
        {
            _auth = auth;
            // Initialisation des commandes avec les méthodes associées
            LoginCommand = new Command(async () => await Login()); // Méthode de connexion
            GoToRegisterCommand = new Command(async () =>
                await Shell.Current.GoToAsync("///RegisterPage")); // Navigation vers la page d'inscription
        }

        private async Task Login()
        {
            //Verification si l'utilisateur existe
            var user = await _auth.LoginAsync(Email, Password);
            //Message erreur
            if (user == null)
            {
                await Shell.Current.DisplayAlert(
                    "Erreur",
                    "Email ou mot de passe incorrect",
                    "OK"
                );
                return;
            }
            //Stockage de l'ID utilisateur dans les préférences
            Preferences.Set("UserId", user.Id);
            //Navigation vers la page principale (Menu)
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
