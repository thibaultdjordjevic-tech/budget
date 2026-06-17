using System.Collections.ObjectModel;
using BudgetControl.Models;
using BudgetControl.Services;

namespace BudgetControl.ViewModels
{
    /// <summary>
    /// ViewModel de la page Revenus — CRUD complet.
    /// </summary>
    public class RevenusViewModel : BaseViewModel
    {
        private Transaction? _revenuSelectionne;
        private Transaction  _formulaire = new() { Type = "Revenu", Date = DateTime.Now };
        private bool         _isFormVisible;
        private int          _mois  = DateTime.Now.Month;
        private int          _annee = DateTime.Now.Year;

        public ObservableCollection<Transaction> Revenus { get; } = new();

        public Transaction? RevenuSelectionne
        {
            get => _revenuSelectionne;
            set { SetProperty(ref _revenuSelectionne, value); if (value != null) OuvrirEdition(value); }
        }

        public Transaction Formulaire
        {
            get => _formulaire;
            set => SetProperty(ref _formulaire, value);
        }

        public bool IsFormVisible
        {
            get => _isFormVisible;
            set => SetProperty(ref _isFormVisible, value);
        }

        public int Mois
        {
            get => _mois;
            set => SetProperty(ref _mois, value);
        }

        public int Annee
        {
            get => _annee;
            set => SetProperty(ref _annee, value);
        }

        public string  PeriodeLabel  => new DateTime(Annee, Mois, 1).ToString("MMMM yyyy");
        public decimal TotalRevenus  => Revenus.Sum(t => t.Montant);
        public string  TotalFormate  => $"{TotalRevenus:C2}";

        // Commandes

        public RelayCommand LoadCommand           { get; }
        public RelayCommand NouveauRevenuCommand  { get; }
        public RelayCommand SauvegarderCommand    { get; }
        public RelayCommand SupprimerCommand      { get; }
        public RelayCommand AnnulerCommand        { get; }
        public RelayCommand MoisPrecedentCommand  { get; }
        public RelayCommand MoisSuivantCommand    { get; }

        public RevenusViewModel()
        {
            Title                 = "Mes revenus";
            LoadCommand           = new RelayCommand(async () => await LoadAsync());
            NouveauRevenuCommand  = new RelayCommand(NouveauRevenu);
            SauvegarderCommand    = new RelayCommand(async () => await SauvegarderAsync());
            SupprimerCommand      = new RelayCommand(async () => await SupprimerAsync());
            AnnulerCommand        = new RelayCommand(() => { IsFormVisible = false; ResetFormulaire(); _revenuSelectionne = null; OnPropertyChanged(nameof(RevenuSelectionne)); });
            MoisPrecedentCommand  = new RelayCommand(async () => await ChangerMoisAsync(-1));
            MoisSuivantCommand    = new RelayCommand(async () => await ChangerMoisAsync(+1));
        }

        public async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var userId = AuthService.Instance.CurrentUser?.Id ?? 0;
                var all    = await DatabaseService.Instance.GetTransactionsAsync(userId, Mois, Annee);

                Revenus.Clear();
                foreach (var r in all.Where(t => t.Type == "Revenu"))
                    Revenus.Add(r);

                OnPropertyChanged(nameof(TotalRevenus));
                OnPropertyChanged(nameof(TotalFormate));
                OnPropertyChanged(nameof(PeriodeLabel));
            }
            finally { IsBusy = false; }
        }

        private void NouveauRevenu()
        {
            ResetFormulaire();
            _revenuSelectionne = null;
            IsFormVisible = true;
        }

        private void OuvrirEdition(Transaction r)
        {
            Formulaire = new Transaction
            {
                Id          = r.Id,
                Montant     = r.Montant,
                Date        = r.Date,
                Description = r.Description,
                Type        = "Revenu",
                UserId      = r.UserId
            };
            IsFormVisible = true;
        }

        private async Task SauvegarderAsync()
        {
            if (Formulaire.Montant <= 0)
            {
                await Shell.Current.DisplayAlert("Erreur", "Le montant doit être supérieur à 0.", "OK");
                return;
            }

            Formulaire.UserId = AuthService.Instance.CurrentUser?.Id ?? 0;
            Formulaire.Type   = "Revenu";
            await DatabaseService.Instance.SaveTransactionAsync(Formulaire);

            IsFormVisible = false;
            ResetFormulaire();
            _revenuSelectionne = null;
            OnPropertyChanged(nameof(RevenuSelectionne));
            await LoadAsync();
        }

        private async Task SupprimerAsync()
        {
            if (_revenuSelectionne is null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirmation", $"Supprimer le revenu de {_revenuSelectionne.Montant:C2} ?", "Oui", "Annuler");
            if (!confirm) return;

            await DatabaseService.Instance.DeleteTransactionAsync(_revenuSelectionne);
            IsFormVisible = false;
            ResetFormulaire();
            _revenuSelectionne = null;
            OnPropertyChanged(nameof(RevenuSelectionne));
            await LoadAsync();
        }

        private async Task ChangerMoisAsync(int delta)
        {
            var date = new DateTime(Annee, Mois, 1).AddMonths(delta);

            Annee = date.Year;
            Mois = date.Month;

            await LoadAsync();
        }

        private void ResetFormulaire() =>
            Formulaire = new Transaction
            {
                Type   = "Revenu",
                Date   = DateTime.Now,
                UserId = AuthService.Instance.CurrentUser?.Id ?? 0
            };
    }
}
