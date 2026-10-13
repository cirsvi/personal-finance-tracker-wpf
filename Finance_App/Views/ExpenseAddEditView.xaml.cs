using Finance_App.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace Finance_App
{
    public partial class ExpenseAddEditView : Window
    {
        private int CurrentUserId;
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ConnectionString;
        private ExpenseItem EditingExpense = null;
        public ExpenseAddEditView(int userId, ExpenseItem expense = null)
        {
            InitializeComponent();
            CurrentUserId = userId;
            EditingExpense = expense;

            LoadCategories();

            if (EditingExpense != null) 
            {
                LoadExpenseData();
            }
            else
            {
                TitleText.Text = "Add Expense:";
                DatePicker.SelectedDate = DateTime.Now;
                AmountBox.Text = "0.00";
            }
        }

        private void LoadExpenseData()
        {
            TitleText.Text = "Edit Expense:";

            DatePicker.SelectedDate = EditingExpense.Date;
            AmountBox.Text = EditingExpense.Amount.ToString(CultureInfo.InvariantCulture);
            CategoryBox.SelectedValue = EditingExpense.CategoryId;
            NoteBox.Text = EditingExpense.Note;
        }

        private void LoadCategories()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    List<ExpenseCategoriesItem> categories = new List<ExpenseCategoriesItem>();
                    string query = "Select CategoryId, CategoryName FROM " +
                        "ExpenseCategories WHERE UserId=@userId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                        using (SqlDataReader reader = cmd.ExecuteReader()) 
                        {
                           
                            while (reader.Read()) 
                            {
                                categories.Add(new ExpenseCategoriesItem
                                {
                                    ExpensesCategoriesId = reader.GetInt32(0),
                                    CategoryName = reader.GetString(1)
                                });
                            } 
                        }

                        CategoryBox.ItemsSource = categories;
                        CategoryBox.DisplayMemberPath = "CategoryName";
                        CategoryBox.SelectedValuePath = "ExpensesCategoriesId";

                        if (EditingExpense == null)
                        {
                            foreach (ExpenseCategoriesItem category in categories)
                            {
                                if(category.CategoryName == "Uncategorized")
                                {
                                    CategoryBox.SelectedValue = category.ExpensesCategoriesId;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateData())
            {
                return;
            }

            DateTime date = DatePicker.SelectedDate ?? DateTime.Now;
            decimal amount = decimal.Parse(AmountBox.Text, CultureInfo.InvariantCulture);
            int categoryId = (int)CategoryBox.SelectedValue;
            string note = NoteBox.Text;


            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand cmd;

                    if (EditingExpense != null) 
                    {
                        string updateQuery = "UPDATE Expenses SET " +
                            "ExpenseDate=@date, ExpenseAmount=@amount, " +
                            "CategoryId=@categoryId, ExpenseNote=@note " +
                            "Where ExpenseId=@id";

                        cmd = new SqlCommand(updateQuery, connection);
                        cmd.Parameters.AddWithValue("@id", EditingExpense.ExpenseId);
                    }
                    else
                    {
                        string insertQuery = "INSERT INTO Expenses (ExpenseDate, ExpenseAmount, " +
                            "CategoryId, UserId, ExpenseNote) " +
                            "VALUES (@date, @amount, @categoryId, @userId, @note)";

                        cmd = new SqlCommand(insertQuery, connection);
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                    }

                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@categoryId", categoryId);
                    cmd.Parameters.AddWithValue("@note", note);

                    cmd.ExecuteNonQuery();
                }

                this.DialogResult = true;
                this.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateData()
        {
            if(DatePicker.SelectedDate == null)
            {
                DateError.Text = "Please select a date.";
                DateError.Visibility = Visibility.Visible;
                return false;
            }

            decimal amount;
            if (!decimal.TryParse(AmountBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
            {
                AmountError.Text = "Please enter a valid amount (e.g. 2.00).";
                AmountError.Visibility = Visibility.Visible;
                return false;
            }

            if (amount <= 0)
            {
                AmountError.Text = "Amount must be greater than 0.";
                AmountError.Visibility = Visibility.Visible;
                return false;
            }

            if (CategoryBox.SelectedItem == null) 
            {
                CategoryError.Text = "Please select a category.";
                CategoryError.Visibility = Visibility.Visible;
                return false;
            }
            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
