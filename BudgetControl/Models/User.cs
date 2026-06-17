using SQLite;

namespace BudgetControl.Models
{
    /// <summary>
    /// Représente un utilisateur de l'application
    /// </summary>
    [Table("Users")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull, Unique]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mot de passe haché avec SHA-256 + sel aléatoire
        /// </summary>
        [NotNull]
        public string PasswordHash { get; set; } = string.Empty;

        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
