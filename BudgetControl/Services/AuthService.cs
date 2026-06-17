using System.Security.Cryptography;
using System.Text;
using BudgetControl.Models;

namespace BudgetControl.Services
{
    /// <summary>
    /// Gère l'authentification et la session utilisateur.
    /// Les mots de passe sont hachés avec SHA-256 + sel aléatoire (pas de dépendance BCrypt).
    /// La session est persistée via Preferences.
    /// </summary>
    public class AuthService
    {
        private static AuthService? _instance;
        public static AuthService Instance => _instance ??= new AuthService();

        private const string SessionKey = "logged_user_id";

        public User? CurrentUser { get; private set; }
        public bool IsLoggedIn => CurrentUser is not null;

        private AuthService() { }

        /// <summary>Restaure la session au démarrage de l'application.</summary>
        public async Task RestoreSessionAsync()
        {
            int userId = Preferences.Get(SessionKey, 0);
            if (userId <= 0) return;
            CurrentUser = await DatabaseService.Instance.GetUserByIdAsync(userId);
        }

        /// <summary>Inscrit un nouvel utilisateur.</summary>
        public async Task<(bool success, string message)> RegisterAsync(
            string email, string password, string nom, string prenom)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "Email et mot de passe requis.");
            if (password.Length < 6)
                return (false, "Le mot de passe doit contenir au moins 6 caractères.");
            if (!email.Contains('@'))
                return (false, "Adresse email invalide.");

            var normalizedEmail = email.ToLower().Trim();
            if (await DatabaseService.Instance.GetUserByEmailAsync(normalizedEmail) is not null)
                return (false, "Un compte avec cet email existe déjà.");

            var user = new User
            {
                Email        = normalizedEmail,
                PasswordHash = HashPassword(password),
                Nom          = nom.Trim(),
                Prenom       = prenom.Trim(),
                CreatedAt    = DateTime.Now
            };

            await DatabaseService.Instance.SaveUserAsync(user);
            CurrentUser = user;
            Preferences.Set(SessionKey, user.Id);
            return (true, "Compte créé avec succès.");
        }

        /// <summary>Connecte un utilisateur existant.</summary>
        public async Task<(bool success, string message)> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "Email et mot de passe requis.");

            var user = await DatabaseService.Instance.GetUserByEmailAsync(email.ToLower().Trim());
            if (user is null || !VerifyPassword(password, user.PasswordHash))
                return (false, "Email ou mot de passe incorrect.");

            CurrentUser = user;
            Preferences.Set(SessionKey, user.Id);
            return (true, "Connexion réussie.");
        }

        /// <summary>Déconnecte l'utilisateur courant.</summary>
        public void Logout()
        {
            CurrentUser = null;
            Preferences.Remove(SessionKey);
        }

        // Hachage SHA-256 + sel aléatoire

        private static string HashPassword(string password)
        {
            byte[] salt   = RandomNumberGenerator.GetBytes(16);
            string saltB64 = Convert.ToBase64String(salt);
            byte[] hash   = SHA256.HashData(Encoding.UTF8.GetBytes(password + saltB64));
            return $"{saltB64}:{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash?.Split(':');
            if (parts?.Length != 2) return false;
            byte[] hash   = SHA256.HashData(Encoding.UTF8.GetBytes(password + parts[0]));
            return Convert.ToBase64String(hash) == parts[1];
        }
    }
}
