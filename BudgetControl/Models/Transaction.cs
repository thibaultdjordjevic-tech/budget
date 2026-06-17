using SQLite;

namespace BudgetControl.Models
{
    /// <summary>
    /// Représente une transaction financière : dépense (Type="Depense") ou revenu (Type="Revenu").
    /// Les propriétés calculées ([Ignore]) ne sont pas stockées en base.
    /// </summary>
    [Table("Transactions")]
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public int UserId { get; set; }

        /// <summary>"Depense" ou "Revenu".</summary>
        [NotNull]
        public string Type { get; set; } = "Depense";

        [NotNull]
        public decimal Montant { get; set; }

        /// <summary>Clé interne de la catégorie (ex : "Alimentation"). Vide pour les revenus.</summary>
        public string Categorie { get; set; } = string.Empty;

        /// <summary>Description saisie par l'utilisateur (facultative).</summary>
        public string Description { get; set; } = string.Empty;

        [NotNull]
        public DateTime Date { get; set; } = DateTime.Now;

        // Propriétés calculées (non persistées)

        [Ignore]
        public bool IsDepense => Type == "Depense";

        /// <summary>Montant formaté avec signe : "-45,00 €" ou "+1 500,00 €".</summary>
        [Ignore]
        public string MontantFormate => IsDepense
            ? $"-{Montant:C2}"
            : $"+{Montant:C2}";

        /// <summary>Couleur selon le type : rouge pour dépense, vert pour revenu.</summary>
        [Ignore]
        public string CouleurType => IsDepense ? "#E57373" : "#81C784";

        /// <summary>Libellé lisible de la catégorie avec icône emoji.</summary>
        [Ignore]
        public string CategorieLabel => Categorie switch
        {
            "Sante"        => "🏥 Santé",
            "Alimentation" => "🛒 Alimentation",
            "Epargne"      => "💰 Épargne",
            "Transport"    => "🚗 Transport",
            "Loisirs"      => "🎮 Loisirs",
            "Shopping"     => "🛍️ Shopping",
            "Logement"     => "🏠 Logement",
            "FraisFixes"   => "📄 Frais fixes",
            _              => string.IsNullOrEmpty(Categorie) ? "💵 Revenu" : Categorie
        };

        /// <summary>Icône seule (emoji) de la catégorie — utilisé dans DepensesPage.</summary>
        [Ignore]
        public string CategorieIcone => Categorie switch
        {
            "Sante"        => "🏥",
            "Alimentation" => "🛒",
            "Epargne"      => "💰",
            "Transport"    => "🚗",
            "Loisirs"      => "🎮",
            "Shopping"     => "🛍️",
            "Logement"     => "🏠",
            "FraisFixes"   => "📄",
            _              => "💵"
        };

        /// <summary>Nom lisible de la catégorie sans icône — utilisé dans DepensesPage.</summary>
        [Ignore]
        public string CategorieNom => Categorie switch
        {
            "Sante"        => "Santé",
            "Alimentation" => "Alimentation",
            "Epargne"      => "Épargne",
            "Transport"    => "Transport",
            "Loisirs"      => "Loisirs",
            "Shopping"     => "Shopping",
            "Logement"     => "Logement",
            "FraisFixes"   => "Frais fixes",
            _              => string.IsNullOrEmpty(Description) ? "Revenu" : Description
        };
    }
}
