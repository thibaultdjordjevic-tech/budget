using BudgetControl.ViewModels;

namespace BudgetControl.Views;

public partial class AddTransactionPage : ContentPage
{
    public AddTransactionPage()
    {
        InitializeComponent();
        BindingContext = new AddTransactionViewModel();
    }
}
