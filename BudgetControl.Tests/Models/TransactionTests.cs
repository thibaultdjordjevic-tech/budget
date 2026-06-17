using BudgetControl.Models;

namespace BudgetControl.Tests.Models;

public class TransactionTests
{
    // ────────────────────────────────────────────────
    //  Valeurs par défaut
    // ────────────────────────────────────────────────

    [Fact]
    public void NouvelleTransaction_TypeParDefautEstDepense()
    {
        var t = new Transaction();
        Assert.Equal("Depense", t.Type);
    }

    [Fact]
    public void NouvelleTransaction_CategorieParDefautEstVide()
    {
        var t = new Transaction();
        Assert.Equal(string.Empty, t.Categorie);
    }

    [Fact]
    public void NouvelleTransaction_DescriptionParDefautEstVide()
    {
        var t = new Transaction();
        Assert.Equal(string.Empty, t.Description);
    }

    // ────────────────────────────────────────────────
    //  IsDepense
    // ────────────────────────────────────────────────

    [Fact]
    public void IsDepense_QuandTypeEstDepense_RetourneVrai()
    {
        var t = new Transaction { Type = "Depense" };
        Assert.True(t.IsDepense);
    }

    [Fact]
    public void IsDepense_QuandTypeEstRevenu_RetourneFaux()
    {
        var t = new Transaction { Type = "Revenu" };
        Assert.False(t.IsDepense);
    }

    // ────────────────────────────────────────────────
    //  CouleurType
    // ────────────────────────────────────────────────

    [Fact]
    public void CouleurType_QuandDepense_RetourneRouge()
    {
        var t = new Transaction { Type = "Depense" };
        Assert.Equal("#E57373", t.CouleurType);
    }

    [Fact]
    public void CouleurType_QuandRevenu_RetourneVert()
    {
        var t = new Transaction { Type = "Revenu" };
        Assert.Equal("#81C784", t.CouleurType);
    }

    // ────────────────────────────────────────────────
    //  MontantFormate
    // ────────────────────────────────────────────────

    [Fact]
    public void MontantFormate_QuandDepense_CommenceParSigneMoins()
    {
        var t = new Transaction { Type = "Depense", Montant = 42.5m };
        Assert.StartsWith("-", t.MontantFormate);
    }

    [Fact]
    public void MontantFormate_QuandRevenu_CommenceParSignePlus()
    {
        var t = new Transaction { Type = "Revenu", Montant = 1500m };
        Assert.StartsWith("+", t.MontantFormate);
    }

    [Fact]
    public void MontantFormate_QuandDepense_ContientLeMontantEnDevise()
    {
        var t = new Transaction { Type = "Depense", Montant = 42.5m };
        // On compare avec la même expression pour rester indépendant de la culture machine
        Assert.Equal($"-{42.5m:C2}", t.MontantFormate);
    }

    [Fact]
    public void MontantFormate_QuandRevenu_ContientLeMontantEnDevise()
    {
        var t = new Transaction { Type = "Revenu", Montant = 1500m };
        Assert.Equal($"+{1500m:C2}", t.MontantFormate);
    }

    [Fact]
    public void MontantFormate_MontantZero_EstFormate()
    {
        var t = new Transaction { Type = "Depense", Montant = 0m };
        Assert.Equal($"-{0m:C2}", t.MontantFormate);
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
        var t = new Transaction { Categorie = categorie };
        Assert.Equal(labelAttendu, t.CategorieLabel);
    }

    [Fact]
    public void CategorieLabel_QuandCategorieVide_AfficheRevenu()
    {
        var t = new Transaction { Categorie = string.Empty };
        Assert.Equal("💵 Revenu", t.CategorieLabel);
    }

    [Fact]
    public void CategorieLabel_QuandCategorieInconnue_RetourneLaValeurBrute()
    {
        var t = new Transaction { Categorie = "Inconnu" };
        Assert.Equal("Inconnu", t.CategorieLabel);
    }

    // ────────────────────────────────────────────────
    //  CategorieIcone
    // ────────────────────────────────────────────────

    [Theory]
    [InlineData("Sante",        "🏥")]
    [InlineData("Alimentation", "🛒")]
    [InlineData("Epargne",      "💰")]
    [InlineData("Transport",    "🚗")]
    [InlineData("Loisirs",      "🎮")]
    [InlineData("Shopping",     "🛍️")]
    [InlineData("Logement",     "🏠")]
    [InlineData("FraisFixes",   "📄")]
    public void CategorieIcone_RetourneLEmoji(string categorie, string iconeAttendue)
    {
        var t = new Transaction { Categorie = categorie };
        Assert.Equal(iconeAttendue, t.CategorieIcone);
    }

    [Fact]
    public void CategorieIcone_QuandCategorieInconnue_RetourneArgent()
    {
        var t = new Transaction { Categorie = "Inconnu" };
        Assert.Equal("💵", t.CategorieIcone);
    }

    // ────────────────────────────────────────────────
    //  CategorieNom
    // ────────────────────────────────────────────────

    [Theory]
    [InlineData("Sante",        "Santé")]
    [InlineData("Alimentation", "Alimentation")]
    [InlineData("Epargne",      "Épargne")]
    [InlineData("Transport",    "Transport")]
    [InlineData("Loisirs",      "Loisirs")]
    [InlineData("Shopping",     "Shopping")]
    [InlineData("Logement",     "Logement")]
    [InlineData("FraisFixes",   "Frais fixes")]
    public void CategorieNom_RetourneNomSansEmoji(string categorie, string nomAttendu)
    {
        var t = new Transaction { Categorie = categorie };
        Assert.Equal(nomAttendu, t.CategorieNom);
    }

    [Fact]
    public void CategorieNom_QuandCategorieInconnueEtDescriptionVide_RetourneRevenu()
    {
        var t = new Transaction { Categorie = "Inconnu", Description = string.Empty };
        Assert.Equal("Revenu", t.CategorieNom);
    }

    [Fact]
    public void CategorieNom_QuandCategorieInconnueMaisDescriptionRenseignee_RetourneLaDescription()
    {
        var t = new Transaction { Categorie = "Inconnu", Description = "Virement salaire" };
        Assert.Equal("Virement salaire", t.CategorieNom);
    }
}
