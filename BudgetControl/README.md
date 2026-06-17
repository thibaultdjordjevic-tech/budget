# Budget Control – Application Mobile .NET MAUI

Application de suivi des dépenses personnelles et budgets, développée en .NET MAUI
avec architecture MVVM et stockage local SQLite.

---

## 🏗 Structure du projet

```
BudgetControl/
├── Models/
│   ├── User.cs               → Compte utilisateur (email + hash SHA-256)
│   ├── Transaction.cs        → Dépense ou revenu (Type = "Depense"|"Revenu")
│   └── Budget.cs             → Budget mensuel par catégorie
├── ViewModels/
│   ├── BaseViewModel.cs      → INotifyPropertyChanged + RelayCommand / RelayCommand<T>
│   ├── LoginViewModel.cs     → Connexion & inscription
│   ├── MainViewModel.cs      → Tableau de bord (solde + dernières transactions)
│   ├── DepensesViewModel.cs  → CRUD dépenses + filtre période/catégorie
│   ├── RevenusViewModel.cs   → CRUD revenus
│   ├── BudgetsViewModel.cs   → Budgets par catégorie + barre de progression
│   ├── SyntheseViewModel.cs  → Graphiques revenus/dépenses + répartition catégories
│   └── AddTransactionViewModel.cs → Formulaire ajout/modification partagé
├── Views/
│   ├── LoginPage.xaml        → Connexion / Inscription
│   ├── MainPage.xaml         → Tableau de bord
│   ├── DepensesPage.xaml     → Liste et CRUD dépenses
│   ├── RevenusPage.xaml      → Liste et CRUD revenus
│   ├── BudgetsPage.xaml      → Budgets par catégorie
│   ├── SynthesePage.xaml     → Synthèse graphique
│   └── AddTransactionPage.xaml → Formulaire partagé
├── Services/
│   ├── AuthService.cs        → Authentification + session (SHA-256 + sel)
│   └── DatabaseService.cs    → Accès SQLite (singleton)
├── Converters/
│   └── Converters.cs         → StringToBool, InverseBool, IdToTitle, etc.
├── App.xaml / App.xaml.cs    → Ressources globales + restauration session
├── AppShell.xaml             → Navigation Shell + menu latéral
└── BudgetControl.csproj      → sqlite-net-pcl 1.9.172
```

---

## ▶️ Lancer le projet

1. Ouvrir `BudgetControl.sln` dans **Visual Studio 2022** (avec workload MAUI installé)
2. Sélectionner la cible : Android Emulator ou iOS Simulator
3. `Ctrl+F5` pour lancer

---

## 🔐 Sécurité

Les mots de passe sont hachés avec **SHA-256 + sel aléatoire** (16 octets) avant stockage.
Format en base : `"<sel_base64>:<hash_base64>"`. Aucun mot de passe n'est jamais stocké en clair.

---

## 📦 Dépendances NuGet

| Package | Version |
|---|---|
| sqlite-net-pcl | 1.9.172 |
| SQLitePCLRaw.bundle_green | 2.1.8 |
