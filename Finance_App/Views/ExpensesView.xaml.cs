using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using Finance_App.Models;

namespace Finance_App
{
    public partial class ExpensesView : Window
    {
        private int CurrentUserId;
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ConnectionString;
        public ExpensesView(int UserId)
        {
            InitializeComponent();
            CurrentUserId = UserId;
            HeaderControl.SetUserId(CurrentUserId);
            LoadExpenses();
        }

        private void LoadExpenses()
        {
            try
            {
                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT e.ExpenseId, " +
                        "e.ExpenseDate, e.ExpenseAmount, " +
                        "c.CategoryName, e.ExpenseNote, " +
                        "e.CategoryId " +
                        "FROM Expenses e " +
                        "INNER JOIN ExpenseCategories c ON e.CategoryId = c.CategoryId " +
                        "WHERE e.UserId = @userId " +
                        "ORDER BY e.ExpenseDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<ExpenseItem> expenses = new List<ExpenseItem>();
                            while (reader.Read())
                            {
                                expenses.Add(new ExpenseItem
                                {
                                    ExpenseId = reader.GetInt32(0),
                                    Date = reader.GetDateTime(1),
                                    Amount = reader.GetDecimal(2),
                                    Category = reader.GetString(3),
                                    Note = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    CategoryId = reader.GetInt32(5)
                                });
                            }
                            ExpensesGrid.ItemsSource = expenses;
                        }

                    }

                }
            } catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            ExpenseAddEditView addWindow = new ExpenseAddEditView(CurrentUserId);

            if (addWindow.ShowDialog() == true)
            {
                LoadExpenses();
            }
        }

        private void EditExpense_Click(object sender, RoutedEventArgs e)
        {
            ExpenseItem selected = ExpensesGrid.SelectedItem as ExpenseItem;
            if (selected == null)
            {
                MessageBox.Show("Select an expense first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ExpenseAddEditView editWindow = new ExpenseAddEditView(CurrentUserId, selected);

            if (editWindow.ShowDialog() == true)
            {
                LoadExpenses();
            }

        }

        private void DeleteExpense_Click(object sender, RoutedEventArgs e)
        {
            ExpenseItem selected = ExpensesGrid.SelectedItem as ExpenseItem;

            if (selected == null) 
            {
                MessageBox.Show("Select an expense first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(MessageBox.Show("Delete this expense record?",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "DELETE FROM Expenses WHERE ExpenseId = @id";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", selected.ExpenseId);
                    cmd.ExecuteNonQuery();
                }
                LoadExpenses();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
