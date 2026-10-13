using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Finance_App.Controls
{
    public partial class Header : UserControl
    {
        private int CurrentUserId;
        public Header()
        {
            InitializeComponent();
        }
        public void SetUserId(int userId)
        {
            CurrentUserId = userId;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            Window.GetWindow(this)?.Close();
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            DashboardView dashboard = new DashboardView(CurrentUserId);
            dashboard.Show();
            Window.GetWindow(this)?.Close();
        }

        private void Expenses_Click(object sender, RoutedEventArgs e)
        {
            ExpensesView expenses = new ExpensesView(CurrentUserId);
            expenses.Show();
            Window.GetWindow(this)?.Close();
        }

        private void Incomes_Click(object sender, RoutedEventArgs e)
        {
            IncomesView incomes = new IncomesView(CurrentUserId);
            incomes.Show();
            Window.GetWindow(this)?.Close();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            CategoriesSourcesView categoriesSources = new CategoriesSourcesView(CurrentUserId);
            categoriesSources.Show();
            Window.GetWindow(this)?.Close();
        }
    }
}
