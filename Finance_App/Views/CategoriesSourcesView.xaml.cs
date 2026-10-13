using Finance_App.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;

namespace Finance_App
{
    public partial class CategoriesSourcesView : Window
    {
        private int CurrentUserId;
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ConnectionString;
        public CategoriesSourcesView(int userId)
        {
            InitializeComponent();
            CurrentUserId = userId;
            HeaderControl.SetUserId(CurrentUserId);

            LoadIncomeSources();
            LoadExpensesCategories();
        }

        private void LoadIncomeSources()
        {
            try
            {
                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT SourceId, SourceName " +
                        "FROM IncomeSources " +
                        "WHERE UserId = @userId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<IncomeSourcesItem> sources = new List<IncomeSourcesItem>();
                            while (reader.Read()) 
                            {
                                sources.Add(new IncomeSourcesItem
                                {
                                    SourceId = reader.GetInt32(0),
                                    SourceName = reader.GetString(1)
                                });
                            }
                            SourcesGrid.ItemsSource = sources;
                        }
                    }
                }

            } catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadExpensesCategories()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT CategoryId, CategoryName " +
                        "FROM ExpenseCategories " +
                        "WHERE UserId = @userId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<ExpenseCategoriesItem> categories = new List<ExpenseCategoriesItem>();
                            while (reader.Read())
                            {
                                categories.Add(new ExpenseCategoriesItem
                                {
                                    ExpensesCategoriesId = reader.GetInt32(0),
                                    CategoryName = reader.GetString(1)
                                });
                            }
                            CategoriesGrid.ItemsSource = categories;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            CategoryAddEditView window = new CategoryAddEditView(CurrentUserId);

            if (window.ShowDialog() == true)
                LoadExpensesCategories();
        }

        private void AddSource_Click(object sender, RoutedEventArgs e)
        {
            SourceAddEditView window = new SourceAddEditView(CurrentUserId);

            if (window.ShowDialog() == true)
                LoadIncomeSources();
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            ExpenseCategoriesItem selected = CategoriesGrid.SelectedItem as ExpenseCategoriesItem;

            if (selected == null)
            {
                MessageBox.Show("Select a category first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selected.CategoryName == "Uncategorized")
            {
                MessageBox.Show("The 'Uncategorized' category cannot be edited.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CategoryAddEditView window = new CategoryAddEditView(CurrentUserId, selected);

            if (window.ShowDialog() == true)
                LoadExpensesCategories();
        }

        private void EditSource_Click(object sender, RoutedEventArgs e)
        {
            IncomeSourcesItem selected = SourcesGrid.SelectedItem as IncomeSourcesItem;

            if (selected == null)
            {
                MessageBox.Show("Select a source first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selected.SourceName == "Uncategorized")
            {
                MessageBox.Show("The 'Uncategorized' source cannot be edited.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SourceAddEditView window = new SourceAddEditView(CurrentUserId, selected);

            if (window.ShowDialog() == true)
                LoadIncomeSources();
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            ExpenseCategoriesItem selected = CategoriesGrid.SelectedItem as ExpenseCategoriesItem;

            if(selected == null)
            {
                MessageBox.Show("Select a category first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(selected.CategoryName == "Uncategorized")
            {
                MessageBox.Show("The 'Uncategorized' category cannot be deleted.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(MessageBox.Show("All expenses in this category will be moved to 'Uncategorized'.\n\nContinue?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    int uncategorizedId;

                    string query = "SELECT CategoryId FROM ExpenseCategories " +
                        "WHERE CategoryName='Uncategorized' AND UserId=@userId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                        uncategorizedId = (int)cmd.ExecuteScalar();  
                    }

                    string updateExpenses = "UPDATE Expenses SET CategoryId=@newCategoryId " +
                        "WHERE CategoryId=@oldCategoryId AND UserId=@userId";

                    using (SqlCommand cmd = new SqlCommand(updateExpenses, connection))
                    {
                        cmd.Parameters.AddWithValue("@newCategoryId", uncategorizedId);
                        cmd.Parameters.AddWithValue("@oldCategoryId", selected.ExpensesCategoriesId);
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                        cmd.ExecuteNonQuery();
                    }

                    string deleteCategory = "DELETE FROM ExpenseCategories WHERE CategoryId=@categoryId AND UserId=@userId";
                    using (SqlCommand cmd = new SqlCommand(deleteCategory, connection))
                    {
                        cmd.Parameters.AddWithValue("@categoryId", selected.ExpensesCategoriesId);
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                        cmd.ExecuteNonQuery();
                    }

                }
                LoadExpensesCategories();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSource_Click(object sender, RoutedEventArgs e)
        {
            IncomeSourcesItem selected = SourcesGrid.SelectedItem as IncomeSourcesItem;

            if (selected == null)
            {
                MessageBox.Show("Select a source first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selected.SourceName == "Uncategorized")
            {
                MessageBox.Show("The 'Uncategorized' source cannot be deleted.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("All incomes with this source will be moved to 'Uncategorized'.\n\nContinue?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    int uncategorizedId;

                    string query = "SELECT SourceId FROM IncomeSources " +
                        "WHERE SourceName='Uncategorized' AND UserId=@userId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                        uncategorizedId = (int)cmd.ExecuteScalar();
                    }

                    string updateIncomes = "UPDATE Incomes SET SourceId=@newSourceId " +
                        "WHERE SourceId=@oldSourceId AND UserId=@userId";

                    using (SqlCommand cmd = new SqlCommand(updateIncomes, connection))
                    {
                        cmd.Parameters.AddWithValue("@newSourceId", uncategorizedId);
                        cmd.Parameters.AddWithValue("@oldSourceId", selected.SourceId);
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                        cmd.ExecuteNonQuery();
                    }

                    string deleteSource = "DELETE FROM IncomeSources WHERE SourceId=@sourceId AND UserId=@userId";
                    using (SqlCommand cmd = new SqlCommand(deleteSource, connection))
                    {
                        cmd.Parameters.AddWithValue("@sourceId", selected.SourceId);
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                        cmd.ExecuteNonQuery();
                    }

                }
                LoadIncomeSources();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
