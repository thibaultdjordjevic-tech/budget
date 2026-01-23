using Gestion.Helpers;
using Gestion.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion.Services
{
    // Service pour gérer l'authentification et les inscriptions des utilisateurs
    public class AuthService
    {
        // Connexion à la base de données SQLite
        private readonly SQLiteAsyncConnection _db;
        // Constructeur qui récupère la connexion SQLite depuis DatabaseService
        public AuthService(DatabaseService database)
        {
            _db = database.Database; //Connexion stocké pour l'utiliser dans les méthodes
        }
        // Méthode pour connecter un utilisateur avec son email et son mot de passe
        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _db.Table<User>()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            return user.PasswordHash == password
                ? user
                : null;
        }
        // Méthode pour inscrire un nouvel utilisateur avec son email et son mot de passe
        public async Task<bool> RegisterAsync(string email, string password)
        {
            var user = await _db.Table<User>()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
                return false;

            await _db.InsertAsync(new User
            {
                Email = email,
                PasswordHash = password
            });

            return true;
        }
    }
}
