using Finance_App.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Finance_App
{
    public partial class IncomeAddEditView : Window
    {
        private int CurrentUserId;
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ConnectionString;
        private IncomeItem EditingIncome = null;
        public IncomeAddEditView(int userId, IncomeItem income = null)
        {
            InitializeComponent();
            CurrentUserId = userId;
            EditingIncome = income;

            LoadSources();

            if (EditingIncome != null)
            {
                LoadIncomeData();
            }
            else
            {
                TitleText.Text = "Add Income:";
                DatePicker.SelectedDate = DateTime.Now;
                AmountBox.Text = "0.00";
            }
        }

        private void LoadIncomeData()
        {
            TitleText.Text = "Edit Income:";

            DatePicker.SelectedDate = EditingIncome.Date;
            AmountBox.Text = EditingIncome.Amount.ToString(CultureInfo.InvariantCulture);
            CategoryBox.SelectedValue = EditingIncome.SourceId;
            NoteBox.Text = EditingIncome.Note;
        }

        private void LoadSources()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    List<IncomeSourcesItem> sources = new List<IncomeSourcesItem>();
                    string query = "Select SourceId, SourceName FROM " +
                        "IncomeSources WHERE UserId=@userId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                sources.Add(new IncomeSourcesItem
                                {
                                    SourceId = reader.GetInt32(0),
                                    SourceName = reader.GetString(1)
                                });
                            }
                        }
 
                        CategoryBox.ItemsSource = sources;
                        CategoryBox.DisplayMemberPath = "SourceName";
                        CategoryBox.SelectedValuePath = "SourceId";

                        if (EditingIncome == null)
                        {
                            foreach (IncomeSourcesItem item in sources)
                            {
                                if (item.SourceName == "Uncategorized")
                                {
                                    CategoryBox.SelectedValue = item.SourceId;
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
            int sourceId = (int)CategoryBox.SelectedValue;
            string note = NoteBox.Text;


            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand cmd;

                    if (EditingIncome != null)
                    {
                        string updateQuery = "UPDATE Incomes SET " +
                            "IncomeDate=@date,IncomeAmount=@amount, " +
                            "SourceId=@sourceId, IncomeNote=@note " +
                            "Where IncomeId=@id";

                        cmd = new SqlCommand(updateQuery, connection);
                        cmd.Parameters.AddWithValue("@id",EditingIncome.IncomeId);
                    }
                    else
                    {
                        string insertQuery = "INSERT INTO Incomes (IncomeDate, IncomeAmount, " +
                            "SourceId, UserId, IncomeNote) " +
                            "VALUES (@date, @amount, @sourceId, @userId, @note)";

                        cmd = new SqlCommand(insertQuery, connection);
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                    }

                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@sourceId", sourceId);
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
            if (DatePicker.SelectedDate == null)
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
                CategoryError.Text = "Please select a source.";
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

