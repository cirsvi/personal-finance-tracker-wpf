using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using Finance_App.Models;

namespace Finance_App
{
    public partial class DashboardView : Window
    {
        private int CurrentUserId;
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ConnectionString;
        public DashboardView(int UserId)
        {
            InitializeComponent();
            CurrentUserId = UserId;
            HeaderControl.SetUserId(CurrentUserId);
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    decimal totalIncome;
                    decimal totalExpenses;

                    string queryIncome = "SELECT ISNULL(SUM(IncomeAmount),0) FROM Incomes WHERE UserId=@userId";
                    using (SqlCommand cmd = new SqlCommand(queryIncome, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                        totalIncome = (decimal)cmd.ExecuteScalar();
                        TotalIncomeText.Text = totalIncome.ToString("C");
                    }

                    string queryExpenses = "SELECT ISNULL(SUM(ExpenseAmount),0) FROM Expenses WHERE UserId=@userId";
                    using (SqlCommand cmd = new SqlCommand(queryExpenses, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                        totalExpenses = (decimal)cmd.ExecuteScalar();
                        TotalExpensesText.Text = totalExpenses.ToString("C");
                    }

                    decimal balance = totalIncome - totalExpenses;
                    BalanceText.Text = balance.ToString("C");

                    string recentExpenses = "SELECT TOP 10 " +
                        "e.ExpenseId, " +
                        "e.ExpenseDate, " +
                        "e.ExpenseAmount, " +
                        "e.ExpenseNote, " +
                        "c.CategoryName, " +
                        "e.CategoryId " +
                        "FROM Expenses e " +
                        "INNER JOIN ExpenseCategories c " +
                        "ON e.CategoryId = c.CategoryId " +
                        "WHERE e.UserId = @userId " +
                        "ORDER BY e.ExpenseDate DESC";

                    using (SqlCommand cmd = new SqlCommand(recentExpenses, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<ExpenseItem> recentEspenses = new List<ExpenseItem>();
                            while (reader.Read())
                            {
                                recentEspenses.Add(new ExpenseItem
                                {
                                    ExpenseId = reader.GetInt32(0),
                                    Date = reader.GetDateTime(1),
                                    Amount = reader.GetDecimal(2),
                                    Note = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                    Category = reader.GetString(4),
                                    CategoryId = reader.GetInt32(5)
                                });
                            }
                            RecentExpensesGrid.ItemsSource = recentEspenses;
                        }

                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
