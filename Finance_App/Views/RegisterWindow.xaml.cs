using Finance_App.Helpers;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Text.RegularExpressions;

namespace Finance_App
{
    public partial class RegisterWindow : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ConnectionString;
        private bool IsPasswordVisible = false;
        private bool IsRepeatPasswordVisible = false;

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            WindowResizeHelper.HandleResize(this, MiddleForm, LeftRectangle, RightRectangle);
        }

        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            if(sender == PasswordToggleButton)
            {
                PasswordVisibilityHelper.Toggle(this, PasswordInvisible,
                    PasswordVisible, EyeIcon, ref IsPasswordVisible);
            } else if(sender == PasswordRepeatToggleButton)
            {
                PasswordVisibilityHelper.Toggle(this, PasswordInvisibleRepeat,
                    PasswordVisibleRepeat, EyeIconRepeat, ref IsRepeatPasswordVisible);
            }

        }

        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text.Trim();
            string email = EmailInput.Text.Trim();
            string password = IsPasswordVisible ? PasswordVisible.Text.Trim() : PasswordInvisible.Password.Trim();
            string repeatPassword = IsRepeatPasswordVisible ? PasswordVisibleRepeat.Text.Trim() : PasswordInvisibleRepeat.Password.Trim();

            GeneralErrorBorder.Visibility = Visibility.Collapsed;
            GeneralErrorText.Visibility = Visibility.Collapsed;
            UsernameError.Visibility = Visibility.Collapsed;
            EmailError.Visibility = Visibility.Collapsed;
            PasswordError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(username)) 
            {
                UsernameError.Text = "Username cannot be empty";
                UsernameError.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                EmailError.Text = "Email cannot be empty.";
                EmailError.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(repeatPassword)) 
            {
                PasswordError.Text = "Password(s) cannot be empty.";
                PasswordError.Visibility = Visibility.Visible;
                return;
            }

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]{3,30}$"))
            {
                UsernameError.Text = "Username must be 3–30 characters and can only contain letters, numbers, and underscores.";
                UsernameError.Visibility = Visibility.Visible;
                return;
            }

            if (!Regex.IsMatch(email, @"\b[A-z0-9._%+-]+@[A-z0-9.-]+\.[A-z]{2,}\b"))
            {
                EmailError.Text = "Invalid email format.";
                EmailError.Visibility = Visibility.Visible;
                return;
            }

            if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"))
            {
                PasswordError.Text = "Password must be at least 8 characters and include uppercase, lowercase, and a number.";
                PasswordError.Visibility = Visibility.Visible;
                return;
            }


            if (password != repeatPassword)
            {
                GeneralErrorBorder.Visibility = Visibility.Visible;
                GeneralErrorText.Text = "Passwords do not match. Please, try again.";
                GeneralErrorText.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username=@username OR Email=@email";

                    using (SqlCommand cmd = new SqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@email", email);

                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            GeneralErrorBorder.Visibility = Visibility.Visible;
                            GeneralErrorText.Text = "User with this username and/or email already exists.";
                            GeneralErrorText.Visibility = Visibility.Visible;
                            return;
                        }
                    }

                    string salt = PasswordHelper.GenerateSalt();
                    string passwordHash = PasswordHelper.ComputeHash(password, salt);

                    string insertQuery = "INSERT INTO Users (Username, Email, PasswordHash, Salt) " +
                        "VALUES (@username, @email, @passwordHash, @salt) " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int newUserId;

                    using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
                        cmd.Parameters.AddWithValue("@salt", salt);

                        newUserId = (int)cmd.ExecuteScalar();
                    }

                    string insertDefaultsQuery = "INSERT INTO ExpenseCategories (CategoryName, UserId) " +
                        "VALUES ('Uncategorized', @userId); " +
                        "INSERT INTO IncomeSources (SourceName, UserId) " +
                        "VALUES ('Uncategorized', @userId);";

                    using (SqlCommand cmd = new SqlCommand(insertDefaultsQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", newUserId);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Registration sucessful! You can now login.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            } catch (Exception ex) 
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}