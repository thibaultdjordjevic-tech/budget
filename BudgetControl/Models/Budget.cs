using SQLite;

namespace BudgetControl.Models
{
    /// <summary>
    /// Catégories de dépenses définies dans le cahier des charges
    /// </summary>
    public enum CategorieType
    {
        Sante,
        Alimentation,
        Epargne,
        Transport,
        Loisirs,
        Shopping,
        Logement,
        FraisFixes
    }

    /// <summary>
    /// Budget alloué par l'utilisateur pour une catégorie donnée
    /// </summary>
    [Table("Budgets")]
    public class Budget
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public int UserId { get; set; }

        [NotNull]
        public string Categorie { get; set; } = string.Empty;

        public decimal Montant { get; set; } = 0;

        public int Mois { get; set; } = DateTime.Now.Month;
        public int Annee { get; set; } = DateTime.Now.Year;

        /// <summary>Mois = 0 signifie budget annuel par défaut (tous les mois).</summary>
        [Ignore]
        public bool IsAnnuel => Mois == 0;

        // Propriété calculée (non stockée)
        [Ignore]
        public string CategorieLabel => Categorie switch
        {
            "Sante"         => "🏥 Santé",
            "Alimentation"  => "🛒 Alimentation",
            "Epargne"       => "💰 Épargne",
            "Transport"     => "🚗 Transport",
            "Loisirs"       => "🎮 Loisirs",
            "Shopping"      => "🛍️ Shopping",
            "Logement"      => "🏠 Logement",
            "FraisFixes"    => "📄 Frais fixes",
            _               => Categorie
        };

        [Ignore]
        public string IconeName => Categorie switch
        {
            "Sante"         => "heart",
            "Alimentation"  => "cart",
            "Epargne"       => "piggybank",
            "Transport"     => "car",
            "Loisirs"       => "gamecontroller",
            "Shopping"      => "bag",
            "Logement"      => "house",
            "FraisFixes"    => "doc_text",
            _               => "circle"
        };
    }
}
