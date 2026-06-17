using BudgetControl.Services;
using BudgetControl.Views;

namespace BudgetControl;

public partial class App : Application
{
    public static double RevenusMax  { get; set; } = 0;
    public static double DepensesMax { get; set; } = 0;

    public App()
    {
        InitializeComponent();

        // Restaurer le thème choisi par l'utilisateur
        var savedTheme = Preferences.Get("app_theme", "system");
        UserAppTheme = savedTheme switch
        {
            "dark"  => AppTheme.Dark,
            "light" => AppTheme.Light,
            _       => AppTheme.Unspecified
        };

        MainPage = new AppShell();
    }

    protected override async void OnStart()
    {
        base.OnStart();
        await AuthService.Instance.RestoreSessionAsync();

        if (AuthService.Instance.IsLoggedIn)
            await Shell.Current.GoToAsync("//MainPage");
        else
            await Shell.Current.GoToAsync("//LoginPage");
    }
}
