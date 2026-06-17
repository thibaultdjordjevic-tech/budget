using BudgetControl.Models;

namespace BudgetControl.Tests.Models;

public class BudgetTests
{
    // ────────────────────────────────────────────────
    //  Valeurs par défaut
    // ────────────────────────────────────────────────

    [Fact]
    public void NouveauBudget_MontantParDefautEstZero()
    {
        var b = new Budget();
        Assert.Equal(0m, b.Montant);
    }

    [Fact]
    public void NouveauBudget_MoisParDefautEstMoisCourant()
    {
        var b = new Budget();
        Assert.Equal(DateTime.Now.Month, b.Mois);
    }

    [Fact]
    public void NouveauBudget_AnneeParDefautEstAnneeCourante()
    {
        var b = new Budget();
        Assert.Equal(DateTime.Now.Year, b.Annee);
    }

    [Fact]
    public void NouveauBudget_CategorieParDefautEstVide()
    {
        var b = new Budget();
        Assert.Equal(string.Empty, b.Categorie);
    }

    // ────────────────────────────────────────────────
    //  IsAnnuel
    // ────────────────────────────────────────────────

    [Fact]
    public void IsAnnuel_QuandMoisEstZero_RetourneVrai()
    {
        var b = new Budget { Mois = 0 };
        Assert.True(b.IsAnnuel);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void IsAnnuel_QuandMoisEstDifferentDeZero_RetourneFaux(int mois)
    {
        var b = new Budget { Mois = mois };
        Assert.False(b.IsAnnuel);
    }

    // ────────────────────────────────────────────────
    //  CategorieLabel
    // ────────────────────────────────────────────────

    [Theory]
    [InlineData("Sante",        "🏥 Santé")]
    [InlineData("Alimentation", "🛒 Alimentation")]
    [InlineData("Epargne",      "💰 Épargne")]
    [InlineData("Transport",    "🚗 Transport")]
    [InlineData("Loisirs",      "🎮 Loisirs")]
    [InlineData("Shopping",     "🛍️ Shopping")]
    [InlineData("Logement",     "🏠 Logement")]
    [InlineData("FraisFixes",   "📄 Frais fixes")]
    public void CategorieLabel_RetourneLabelAvecEmoji(string categorie, string labelAttendu)
    {
        var b = new Budget { Categorie = categorie };
        Assert.Equal(labelAttendu, b.CategorieLabel);
    }

    [Fact]
    public void CategorieLabel_QuandCategorieInconnue_RetourneLaValeurBrute()
    {
        var b = new Budget { Categorie = "Autre" };
        Assert.Equal("Autre", b.CategorieLabel);
    }

    // ────────────────────────────────────────────────
    //  IconeName
    // ────────────────────────────────────────────────

    [Theory]
    [InlineData("Sante",        "heart")]
    [InlineData("Alimentation", "cart")]
    [InlineData("Epargne",      "piggybank")]
    [InlineData("Transport",    "car")]
    [InlineData("Loisirs",      "gamecontroller")]
    [InlineData("Shopping",     "bag")]
    [InlineData("Logement",     "house")]
    [InlineData("FraisFixes",   "doc_text")]
    public void IconeName_RetourneLIconeCorrecte(string categorie, string iconeAttendue)
    {
        var b = new Budget { Categorie = categorie };
        Assert.Equal(iconeAttendue, b.IconeName);
    }

    [Fact]
    public void IconeName_QuandCategorieInconnue_RetourneCircle()
    {
        var b = new Budget { Categorie = "Inconnu" };
        Assert.Equal("circle", b.IconeName);
    }

    // ────────────────────────────────────────────────
    //  CategorieType enum
    // ────────────────────────────────────────────────

    [Fact]
    public void CategorieType_ToutesLesValeursSontDefinies()
    {
        var valeurs = Enum.GetValues<CategorieType>();
        Assert.Equal(8, valeurs.Length);
    }

    [Theory]
    [InlineData("Sante",        CategorieType.Sante)]
    [InlineData("Alimentation", CategorieType.Alimentation)]
    [InlineData("Epargne",      CategorieType.Epargne)]
    [InlineData("Transport",    CategorieType.Transport)]
    [InlineData("Loisirs",      CategorieType.Loisirs)]
    [InlineData("Shopping",     CategorieType.Shopping)]
    [InlineData("Logement",     CategorieType.Logement)]
    [InlineData("FraisFixes",   CategorieType.FraisFixes)]
    public void CategorieType_NomCorrespondAALaCle(string nomAttendu, CategorieType valeur)
    {
        Assert.Equal(nomAttendu, valeur.ToString());
    }
}
