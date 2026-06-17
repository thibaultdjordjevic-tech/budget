using BudgetControl.ViewModels;

namespace BudgetControl.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _vm;

    public MainPage()
    {
        InitializeComponent();
        _vm = new MainViewModel();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadDataAsync();
    }

    private void OnThemeToggleClicked(object? sender, EventArgs e)
    {
        var current = Application.Current;
        if (current == null) return;

        if (current.RequestedTheme == AppTheme.Dark || current.UserAppTheme == AppTheme.Dark)
        {
            current.UserAppTheme = AppTheme.Light;
            Preferences.Set("app_theme", "light");
        }
        else
        {
            current.UserAppTheme = AppTheme.Dark;
            Preferences.Set("app_theme", "dark");
        }
    }
}
