using BudgetControl.ViewModels;

namespace BudgetControl.Views;

public partial class SynthesePage : ContentPage
{
    private readonly SyntheseViewModel _vm;

    public SynthesePage()
    {
        InitializeComponent();
        _vm = new SyntheseViewModel();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
