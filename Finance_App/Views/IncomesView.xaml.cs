using Finance_App.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;

namespace Finance_App
{
    public partial class IncomesView : Window
    {
        private int CurrentUserId;
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ConnectionString;
        public IncomesView(int UserId)
        {
            InitializeComponent();
            CurrentUserId = UserId;
            HeaderControl.SetUserId(CurrentUserId);
            LoadIncomes();
        }


        private void LoadIncomes()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT i.IncomeId, " +
                        "i.IncomeDate, i.IncomeAmount, " +
                        "s.SourceName, i.IncomeNote, " +
                        "i.SourceId " +
                        "FROM Incomes i " +
                        "INNER JOIN IncomeSources s ON i.SourceId = s.SourceId " +
                        "WHERE i.UserId = @userId " +
                        "ORDER BY i.IncomeDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<IncomeItem> incomes = new List<IncomeItem>();
                            while (reader.Read())
                            {
                                incomes.Add(new IncomeItem
                                {
                                    IncomeId = reader.GetInt32(0),
                                    Date = reader.GetDateTime(1),
                                    Amount = reader.GetDecimal(2),
                                    Source = reader.GetString(3),
                                    Note = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    SourceId = reader.GetInt32(5)
                                });
                            }
                            IncomesGrid.ItemsSource = incomes;
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddIncome_Click(object sender, RoutedEventArgs e)
        {
            IncomeAddEditView addWindow = new IncomeAddEditView(CurrentUserId);

            if (addWindow.ShowDialog() == true)
            {
                LoadIncomes();
            }
        }

        private void EditIncome_Click(object sender, RoutedEventArgs e)
        {
            IncomeItem selected = IncomesGrid.SelectedItem as IncomeItem;
            if (selected == null)
            {
                MessageBox.Show("Select an income first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IncomeAddEditView editWindow = new IncomeAddEditView(CurrentUserId, selected);

            if (editWindow.ShowDialog() == true)
            {
                LoadIncomes();
            }

        }

        private void DeleteIncome_Click(object sender, RoutedEventArgs e)
        {
            IncomeItem selected = IncomesGrid.SelectedItem as IncomeItem;

            if (selected == null)
            {
                MessageBox.Show("Select an income first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Delete this income record?",
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

                    string query = "DELETE FROM Incomes WHERE IncomeId = @id";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", selected.IncomeId);
                    cmd.ExecuteNonQuery();
                }
                LoadIncomes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
