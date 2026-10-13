using Finance_App.Models;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;

namespace Finance_App
{
    public partial class SourceAddEditView : Window
    {
        private int CurrentUserId;
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ConnectionString;
        private IncomeSourcesItem EditingSource = null;
        public SourceAddEditView(int userId, IncomeSourcesItem source = null)
        {
            InitializeComponent();
            CurrentUserId = userId;
            EditingSource = source;

            if (EditingSource != null)
            {
                TitleText.Text = "Edit Source:";
                SourceNameBox.Text = EditingSource.SourceName;
            }
            else
            {
                TitleText.Text = "Add Source:";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SourceNameBox.Text))
            {
                SourceNameError.Text = "Name cannot be empty.";
                SourceNameError.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string name = SourceNameBox.Text.Trim();

                    string checkQuery = "SELECT COUNT(*) FROM IncomeSources " +
                                        "WHERE SourceName=@name AND UserId=@userId";

                    if (EditingSource != null)
                    {
                        checkQuery += " AND SourceId<>@id";
                    }

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@name", name);
                        checkCmd.Parameters.AddWithValue("@userId", CurrentUserId);
                        if (EditingSource != null)
                            checkCmd.Parameters.AddWithValue("@id", EditingSource.SourceId);

                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            SourceNameError.Text = "A source with this title already exists. Try again!";
                            SourceNameError.Visibility = Visibility.Visible;
                            return;
                        }
                    }

                    SqlCommand cmd;

                    if (EditingSource == null)
                    {
                        string insertQuery = "INSERT INTO IncomeSources (SourceName, UserId) VALUES (@name, @userId)";
                        cmd = new SqlCommand(insertQuery, connection);
                        cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                    }
                    else
                    {
                        string editQuery = "UPDATE IncomeSources SET SourceName=@name WHERE SourceId=@id";
                        cmd = new SqlCommand(editQuery, connection);
                        cmd.Parameters.AddWithValue("@id", EditingSource.SourceId);
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
