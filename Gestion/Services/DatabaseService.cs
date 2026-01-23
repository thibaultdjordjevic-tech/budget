using Gestion.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion.Services
{
    // Service pour gérer la base de données SQLite et la connexion
    public class DatabaseService
    {
        public SQLiteAsyncConnection Database { get; }

        public DatabaseService()
        {
            // Définition du chemin de la base de données SQLite
            var path = Path.Combine(
                FileSystem.AppDataDirectory,
                "budget.db3"
            );
            // Initialisation de la connexion à la base de données SQLite
            Database = new SQLiteAsyncConnection(path);
            // Création de la table User si elle n'existe pas déjà
            Database.CreateTableAsync<User>().Wait();
        }
    }
}
