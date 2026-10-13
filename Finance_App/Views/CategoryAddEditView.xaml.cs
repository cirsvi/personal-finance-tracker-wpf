using Finance_App.Models;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;


namespace Finance_App
{
    public partial class CategoryAddEditView : Window
    {
        private int CurrentUserId;
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ConnectionString;
        private ExpenseCategoriesItem EditingCategory = null;
        public CategoryAddEditView(int userId, ExpenseCategoriesItem category = null)
        {
            InitializeComponent();
            CurrentUserId = userId;

            EditingCategory = category;

            if (EditingCategory != null)
            {
                TitleText.Text = "Edit Category:";
                CategoryNameBox.Text = EditingCategory.CategoryName;
            }
            else
            {
                TitleText.Text = "Add Category:";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryNameBox.Text))
            {
                CategoryNameError.Text = "Name cannot be empty.";
                CategoryNameError.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string name = CategoryNameBox.Text.Trim();

                    string checkQuery = "SELECT COUNT(*) FROM ExpenseCategories " +
                                        "WHERE CategoryName=@name AND UserId=@userId";

                    if (EditingCategory != null)
                    {
                        checkQuery += " AND CategoryId<>@id";
                    }

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@name", name);
                        checkCmd.Parameters.AddWithValue("@userId", CurrentUserId);
                        if (EditingCategory != null)
                            checkCmd.Parameters.AddWithValue("@id", EditingCategory.ExpensesCategoriesId);

                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            CategoryNameError.Text = "A category with this name already exists. Try again!";
                            CategoryNameError.Visibility = Visibility.Visible;
                            return;
                        }
                    }

                    SqlCommand cmd;
                    if (EditingCategory == null)
                    {
                        string insertQuery = "INSERT INTO ExpenseCategories (CategoryName, UserId) VALUES (@name, @userId)";
                        cmd = new SqlCommand(insertQuery, connection);
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                    }
                    else
                    {
                        string editQuery = "UPDATE ExpenseCategories SET CategoryName=@name WHERE CategoryId=@id";
                        cmd = new SqlCommand(editQuery, connection);
                        cmd.Parameters.AddWithValue("@id", EditingCategory.ExpensesCategoriesId);
                    }

                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.ExecuteNonQuery();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
