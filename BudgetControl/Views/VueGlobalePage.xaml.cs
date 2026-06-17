using BudgetControl.ViewModels;

namespace BudgetControl.Views;

public partial class VueGlobalePage : ContentPage
{
    private readonly VueGlobaleViewModel _vm;

    public VueGlobalePage()
    {
        InitializeComponent();
        _vm = new VueGlobaleViewModel();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.ChargerAsync();
    }
}
