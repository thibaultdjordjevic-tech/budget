using System.Collections.ObjectModel;
using System.Globalization;
using BudgetControl.Models;
using BudgetControl.Services;

namespace BudgetControl.ViewModels
{
    public class BudgetResume
    {
        public string  Categorie      { get; set; } = string.Empty;
        public string  CategorieLabel { get; set; } = string.Empty;
        public string  CategorieIcone { get; set; } = string.Empty;
        public string  CategorieNom   { get; set; } = string.Empty;
        public decimal BudgetPrevu    { get; set; }
        public decimal Depenses       { get; set; }

        public bool EstAnnuel { get; set; }

        public decimal Restant       => BudgetPrevu - Depenses;
        public double  Progression   => BudgetPrevu > 0 ? Math.Min((double)(Depenses / BudgetPrevu), 1.0) : 0;
        public string  CouleurBarre  => Progression >= 1 ? "#FF6B6B" : Progression >= 0.8 ? "#FFB627" : "#06D6A0";
        public string  RestantFormate => Restant >= 0
            ? $"Reste : {Restant:C0}"
            : $"Dépassé : {Math.Abs(Restant):C0}";

        public bool   HasBudget    => BudgetPrevu > 0;
        public string BadgeLabel   => !HasBudget ? "" : EstAnnuel ? "défaut annuel" : "ce mois";
        public string BadgeCouleur => EstAnnuel ? "#8B949E" : "#4361EE";
    }

    public class BudgetsViewModel : BaseViewModel
    {
        private int _mois  = DateTime.Now.Month;
        private int _annee = DateTime.Now.Year;

        public ObservableCollection<BudgetResume> Budgets { get; } = new();

        public int Mois
        {
            get => _mois;
            set { SetProperty(ref _mois, value); OnPropertyChanged(nameof(PeriodeLabel)); _ = LoadAsync(); }
        }

        public int Annee
        {
            get => _annee;
            set { SetProperty(ref _annee, value); OnPropertyChanged(nameof(PeriodeLabel)); _ = LoadAsync(); }
        }

        public string PeriodeLabel => new DateTime(Annee, Mois, 1).ToString("MMMM yyyy");

        public RelayCommand                   LoadCommand           { get; }
        public RelayCommand<BudgetResume>     ModifierBudgetCommand { get; }
        public RelayCommand                   MoisPrecedentCommand  { get; }
        public RelayCommand                   MoisSuivantCommand    { get; }

        private static readonly List<(string key, string icone, string nom)> _categories = new()
        {
            ("Sante",        "🏥", "Santé"),
            ("Alimentation", "🛒", "Alimentation"),
            ("Epargne",      "💰", "Épargne"),
            ("Transport",    "🚗", "Transport"),
            ("Loisirs",      "🎮", "Loisirs"),
            ("Shopping",     "🛍️", "Shopping"),
            ("Logement",     "🏠", "Logement"),
            ("FraisFixes",   "📄", "Frais fixes"),
        };

        public BudgetsViewModel()
        {
            Title                 = "Budgets";
            LoadCommand           = new RelayCommand(async () => await LoadAsync());
            ModifierBudgetCommand = new RelayCommand<BudgetResume>(async b => await ModifierBudgetAsync(b));
            MoisPrecedentCommand  = new RelayCommand(async () => await ChangerMoisAsync(-1));
            MoisSuivantCommand    = new RelayCommand(async () => await ChangerMoisAsync(+1));
        }

        public async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var userId       = AuthService.Instance.CurrentUser?.Id ?? 0;
                var budgetsMois  = await DatabaseService.Instance.GetBudgetsAsync(userId, Mois, Annee);
                var budgetsAn    = await DatabaseService.Instance.GetBudgetsAnnuelsAsync(userId, Annee);
                var transactions = await DatabaseService.Instance.GetTransactionsAsync(userId, Mois, Annee);

                Budgets.Clear();
                foreach (var (key, icone, nom) in _categories)
                {
                    var mensuel  = budgetsMois.FirstOrDefault(b => b.Categorie == key);
                    var annuel   = budgetsAn.FirstOrDefault(b => b.Categorie == key);
                    var effectif = mensuel ?? annuel;

                    var depenses = transactions
                        .Where(t => t.Type == "Depense" && t.Categorie == key)
                        .Sum(t => t.Montant);

                    Budgets.Add(new BudgetResume
                    {
                        Categorie      = key,
                        CategorieLabel = $"{icone} {nom}",
                        CategorieIcone = icone,
                        CategorieNom   = nom,
                        BudgetPrevu    = effectif?.Montant ?? 0,
                        Depenses       = depenses,
                        EstAnnuel      = mensuel == null && annuel != null
                    });
                }
            }
            finally { IsBusy = false; }
        }

        private async Task ModifierBudgetAsync(BudgetResume? resume)
        {
            if (resume is null) return;

            // Demander : mensuel ou annuel ?
            var choix = await Shell.Current.DisplayActionSheet(
                resume.CategorieLabel,
                "Annuler", null,
                "Ce mois seulement",
                $"Budget annuel par défaut ({Annee})");

            if (choix is null || choix == "Annuler") return;

            var userId     = AuthService.Instance.CurrentUser?.Id ?? 0;
            bool pourAnnee = choix.StartsWith("Budget annuel");

            // Pré-remplir avec la valeur existante
            decimal valeurActuelle;
            if (pourAnnee)
            {
                var budgetAn = await DatabaseService.Instance.GetBudgetAnnuelAsync(userId, resume.Categorie, Annee);
                valeurActuelle = budgetAn?.Montant ?? resume.BudgetPrevu;
            }
            else
            {
                var budgetMois = await DatabaseService.Instance.GetBudgetAsync(userId, resume.Categorie, Mois, Annee);
                // Si pas d'override mensuel, pré-remplir avec le budget effectif (annuel ou 0)
                valeurActuelle = budgetMois?.Montant ?? resume.BudgetPrevu;
            }

            var libelle = pourAnnee
                ? $"Budget annuel par défaut pour {Annee} (€) :"
                : $"Budget pour {new DateTime(Annee, Mois, 1):MMMM yyyy} (€) :";

            var result = await Shell.Current.DisplayPromptAsync(
                resume.CategorieLabel,
                libelle,
                initialValue: valeurActuelle > 0 ? valeurActuelle.ToString("F2") : string.Empty,
                keyboard: Keyboard.Numeric,
                placeholder: "Ex : 300");

            if (result is null) return;

            if (!decimal.TryParse(
                    result.Replace(',', '.'),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var montant) || montant < 0)
            {
                await Shell.Current.DisplayAlert("Erreur", "Montant invalide.", "OK");
                return;
            }

            if (pourAnnee)
            {
                var budget = await DatabaseService.Instance.GetBudgetAnnuelAsync(userId, resume.Categorie, Annee)
                             ?? new Budget { UserId = userId, Categorie = resume.Categorie, Mois = 0, Annee = Annee };
                budget.Montant = montant;
                await DatabaseService.Instance.SaveBudgetAsync(budget);
            }
            else
            {
                var budget = await DatabaseService.Instance.GetBudgetAsync(userId, resume.Categorie, Mois, Annee)
                             ?? new Budget { UserId = userId, Categorie = resume.Categorie, Mois = Mois, Annee = Annee };
                budget.Montant = montant;
                await DatabaseService.Instance.SaveBudgetAsync(budget);
            }

            await LoadAsync();
        }

        private async Task ChangerMoisAsync(int delta)
        {
            var date = new DateTime(Annee, Mois, 1).AddMonths(delta);
            // Mise à jour directe des champs pour éviter double LoadAsync
            _annee = date.Year;
            _mois  = date.Month;
            OnPropertyChanged(nameof(Annee));
            OnPropertyChanged(nameof(Mois));
            OnPropertyChanged(nameof(PeriodeLabel));
            await LoadAsync();
        }
    }
}
