using BudgetControl.Models;

namespace BudgetControl.Tests.Models;

public class UserTests
{
    // ────────────────────────────────────────────────
    //  Valeurs par défaut
    // ────────────────────────────────────────────────

    [Fact]
    public void NouvelUtilisateur_EmailParDefautEstVide()
    {
        var u = new User();
        Assert.Equal(string.Empty, u.Email);
    }

    [Fact]
    public void NouvelUtilisateur_PasswordHashParDefautEstVide()
    {
        var u = new User();
        Assert.Equal(string.Empty, u.PasswordHash);
    }

    [Fact]
    public void NouvelUtilisateur_NomParDefautEstVide()
    {
        var u = new User();
        Assert.Equal(string.Empty, u.Nom);
    }

    [Fact]
    public void NouvelUtilisateur_PrenomParDefautEstVide()
    {
        var u = new User();
        Assert.Equal(string.Empty, u.Prenom);
    }

    [Fact]
    public void NouvelUtilisateur_CreatedAtEstApproximativementMaintenant()
    {
        var avant = DateTime.Now.AddSeconds(-1);
        var u = new User();
        var apres = DateTime.Now.AddSeconds(1);

        Assert.InRange(u.CreatedAt, avant, apres);
    }

    // ────────────────────────────────────────────────
    //  Affectation des propriétés
    // ────────────────────────────────────────────────

    [Fact]
    public void User_EmailEstBienAssigne()
    {
        var u = new User { Email = "test@exemple.fr" };
        Assert.Equal("test@exemple.fr", u.Email);
    }

    [Fact]
    public void User_NomEtPrenomSontBienAssignes()
    {
        var u = new User { Nom = "Dupont", Prenom = "Marie" };
        Assert.Equal("Dupont", u.Nom);
        Assert.Equal("Marie", u.Prenom);
    }

    [Fact]
    public void User_PasswordHashEstBienAssigne()
    {
        var hash = "abc123def456";
        var u = new User { PasswordHash = hash };
        Assert.Equal(hash, u.PasswordHash);
    }

    [Fact]
    public void User_DeuxUtilisateursDistinctsNonLies()
    {
        var u1 = new User { Email = "alice@exemple.fr", Nom = "Alice" };
        var u2 = new User { Email = "bob@exemple.fr",   Nom = "Bob" };

        Assert.NotEqual(u1.Email, u2.Email);
        Assert.NotEqual(u1.Nom,   u2.Nom);
    }
}
