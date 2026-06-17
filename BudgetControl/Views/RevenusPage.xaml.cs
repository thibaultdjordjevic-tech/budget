using BudgetControl.ViewModels;

namespace BudgetControl.Views;

public partial class RevenusPage : ContentPage
{
    private readonly RevenusViewModel _vm;

    public RevenusPage()
    {
        InitializeComponent();
        _vm = new RevenusViewModel();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
