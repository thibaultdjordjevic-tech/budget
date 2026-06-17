using SQLite;
using BudgetControl.Models;

namespace BudgetControl.Services
{
    /// <summary>
    /// Service de gestion de la base de données SQLite locale.
    /// Toutes les données sont séparées par utilisateur (UserId).
    /// Les mots de passe sont hachés avant stockage (via AuthService).
    /// </summary>
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
        private static DatabaseService? _instance;

        public static DatabaseService Instance => _instance ??= new DatabaseService();

        private DatabaseService() { }

        /// <summary>
        /// Initialise la connexion SQLite et crée les tables si elles n'existent pas.
        /// </summary>
        private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
        {
            if (_database is not null) return _database;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "budgetcontrol.db");
            _database = new SQLiteAsyncConnection(dbPath);

            await _database.CreateTableAsync<User>();
            await _database.CreateTableAsync<Transaction>();
            await _database.CreateTableAsync<Budget>();

            return _database;
        }

        // ─── UTILISATEURS ──────────────────────────────────────────────────────────

        /// <summary>Recherche un utilisateur par son email (insensible à la casse).</summary>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var db = await GetDatabaseAsync();
            return await db.Table<User>()
                           .Where(u => u.Email == email)
                           .FirstOrDefaultAsync();
        }

        /// <summary>Recherche un utilisateur par son identifiant.</summary>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            var db = await GetDatabaseAsync();
            return await db.Table<User>().Where(u => u.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>Insère ou met à jour un utilisateur.</summary>
        public async Task<int> SaveUserAsync(User user)
        {
            var db = await GetDatabaseAsync();
            return user.Id == 0
                ? await db.InsertAsync(user)
                : await db.UpdateAsync(user);
        }

        // TRANSACTIONS

        /// <summary>
        /// Retourne les transactions d'un utilisateur pour un mois/année donnés,
        /// triées par date décroissante.
        /// </summary>
        public async Task<List<Transaction>> GetTransactionsAsync(int userId, int mois, int annee)
        {
            var db  = await GetDatabaseAsync();
            var all = await db.Table<Transaction>()
                              .Where(t => t.UserId == userId)
                              .ToListAsync();

            // Filtre côté client (SQLite-NET ne supporte pas les propriétés DateTime.Month)
            return all.Where(t => t.Date.Month == mois && t.Date.Year == annee)
                      .OrderByDescending(t => t.Date)
                      .ToList();
        }

        /// <summary>Retourne toutes les transactions d'un utilisateur, sans filtre.</summary>
        public async Task<List<Transaction>> GetAllTransactionsAsync(int userId)
        {
            var db = await GetDatabaseAsync();
            return await db.Table<Transaction>()
                           .Where(t => t.UserId == userId)
                           .OrderByDescending(t => t.Date)
                           .ToListAsync();
        }

        /// <summary>Insère ou met à jour une transaction.</summary>
        public async Task<int> SaveTransactionAsync(Transaction transaction)
        {
            var db = await GetDatabaseAsync();
            return transaction.Id == 0
                ? await db.InsertAsync(transaction)
                : await db.UpdateAsync(transaction);
        }

        /// <summary>Supprime une transaction.</summary>
        public async Task<int> DeleteTransactionAsync(Transaction transaction)
        {
            var db = await GetDatabaseAsync();
            return await db.DeleteAsync(transaction);
        }

        // CALCULS AGRÉGÉS

        /// <summary>Total des revenus pour un utilisateur sur une période.</summary>
        public async Task<decimal> GetTotalRevenusAsync(int userId, int mois, int annee)
        {
            var transactions = await GetTransactionsAsync(userId, mois, annee);
            return transactions.Where(t => t.Type == "Revenu").Sum(t => t.Montant);
        }

        /// <summary>Total des dépenses pour un utilisateur sur une période.</summary>
        public async Task<decimal> GetTotalDepensesAsync(int userId, int mois, int annee)
        {
            var transactions = await GetTransactionsAsync(userId, mois, annee);
            return transactions.Where(t => t.Type == "Depense").Sum(t => t.Montant);
        }

        /// <summary>
        /// Retourne le total des dépenses regroupé par catégorie pour une période.
        /// Clé = string interne de la catégorie (ex: "Alimentation"), Valeur = total.
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetDepensesParCategorieAsync(
            int userId, int mois, int annee)
        {
            var transactions = await GetTransactionsAsync(userId, mois, annee);
            return transactions
                .Where(t => t.Type == "Depense")
                .GroupBy(t => t.Categorie)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Montant));
        }

        // BUDGETS PAR CATÉGORIE

        /// <summary>Retourne tous les budgets d'un utilisateur pour un mois/année.</summary>
        public async Task<List<Budget>> GetBudgetsAsync(int userId, int mois, int annee)
        {
            var db = await GetDatabaseAsync();
            return await db.Table<Budget>()
                           .Where(b => b.UserId == userId
                                    && b.Mois   == mois
                                    && b.Annee  == annee)
                           .ToListAsync();
        }

        /// <summary>Retourne le budget mensuel pour une catégorie précise.</summary>
        public async Task<Budget?> GetBudgetAsync(int userId, string categorie, int mois, int annee)
        {
            var db = await GetDatabaseAsync();
            return await db.Table<Budget>()
                           .Where(b => b.UserId    == userId
                                    && b.Categorie == categorie
                                    && b.Mois      == mois
                                    && b.Annee     == annee)
                           .FirstOrDefaultAsync();
        }

        /// <summary>Retourne les budgets annuels par défaut (Mois=0) pour un utilisateur.</summary>
        public async Task<List<Budget>> GetBudgetsAnnuelsAsync(int userId, int annee)
        {
            var db = await GetDatabaseAsync();
            return await db.Table<Budget>()
                           .Where(b => b.UserId == userId && b.Mois == 0 && b.Annee == annee)
                           .ToListAsync();
        }

        /// <summary>Retourne le budget annuel par défaut pour une catégorie précise (Mois=0).</summary>
        public async Task<Budget?> GetBudgetAnnuelAsync(int userId, string categorie, int annee)
        {
            var db = await GetDatabaseAsync();
            return await db.Table<Budget>()
                           .Where(b => b.UserId    == userId
                                    && b.Categorie == categorie
                                    && b.Mois      == 0
                                    && b.Annee     == annee)
                           .FirstOrDefaultAsync();
        }

        /// <summary>Insère ou met à jour un budget.</summary>
        public async Task<int> SaveBudgetAsync(Budget budget)
        {
            var db = await GetDatabaseAsync();
            return budget.Id == 0
                ? await db.InsertAsync(budget)
                : await db.UpdateAsync(budget);
        }

        /// <summary>Supprime un budget.</summary>
        public async Task<int> DeleteBudgetAsync(Budget budget)
        {
            var db = await GetDatabaseAsync();
            return await db.DeleteAsync(budget);
        }
    }
}
