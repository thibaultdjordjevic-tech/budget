using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gestion.Services;

namespace Gestion.ViewModels
{
    //ViewModel pour la page d'inscription
    public class RegisterViewModel : BaseViewModel
    {
        // Service d'authentification
        private readonly AuthService _auth;
        // Propriétés pour l'email et le mot de passe saisies par l'utilisateur
        public string Email { get; set; }
        public string Password { get; set; }
        // Commande liée au bouton d'inscription
        public ICommand RegisterCommand { get; }
        // Constructeur qui initialise le service d'authentification et la commande
        public RegisterViewModel(AuthService auth)
        {
            _auth = auth;
            RegisterCommand = new Command(async () => await Register());
        }
        // Méthode pour gérer l'inscription de l'utilisateur
        private async Task Register()
        {
            // Tentative d'inscription de l'utilisateur
            var success = await _auth.RegisterAsync(Email, Password);
            // Message d'erreur si le compte existe déjà
            if (!success)
            {
                await Shell.Current.DisplayAlert(
                    "Erreur",
                    "Compte déjà existant",
                    "OK"
                );
                return;
            }
            // Message de succès
            await Shell.Current.DisplayAlert(
                "Succès",
                "Compte créé",
                "OK"
            );
            // Navigation de retour à la page précédente (connexion)
            await Shell.Current.GoToAsync("..");
        }
    }
}
