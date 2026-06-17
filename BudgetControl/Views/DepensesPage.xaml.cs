using BudgetControl.ViewModels;

namespace BudgetControl.Views;

public partial class DepensesPage : ContentPage
{
    private readonly DepensesViewModel _vm;

    public DepensesPage()
    {
        InitializeComponent();
        _vm = new DepensesViewModel();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
