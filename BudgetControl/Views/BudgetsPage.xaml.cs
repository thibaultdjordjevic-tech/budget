using BudgetControl.ViewModels;

namespace BudgetControl.Views;

public partial class BudgetsPage : ContentPage
{
    private readonly BudgetsViewModel _vm;

    public BudgetsPage()
    {
        InitializeComponent();
        _vm = new BudgetsViewModel();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
