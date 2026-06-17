using BudgetControl.Views;

namespace BudgetControl
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AddTransactionPage), typeof(AddTransactionPage));
        }
    }
}
