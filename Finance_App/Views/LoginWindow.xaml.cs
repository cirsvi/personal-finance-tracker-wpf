using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data.SqlClient;
using Finance_App.Helpers;

namespace Finance_App
{
    public partial class LoginWindow : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ConnectionString;
        private bool IsPasswordVisible = false;

        public LoginWindow()
        {
            InitializeComponent();
            AnimateRectangleColor(LeftRectangle, (Color)ColorConverter.ConvertFromString("#567C8D"), (Color)ColorConverter.ConvertFromString("#2F4156"), 5);
            AnimateRectangleColor(RightRectangle, (Color)ColorConverter.ConvertFromString("#567C8D"), (Color)ColorConverter.ConvertFromString("#2F4156"), 5);
        }

        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            PasswordVisibilityHelper.Toggle(this, PasswordInvisible, PasswordVisible, EyeIcon, ref IsPasswordVisible);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            WindowResizeHelper.HandleResize(this, MiddleForm, LeftRectangle, RightRectangle);
        }

        private void AnimateRectangleColor(Rectangle rectangle, Color fromColor, Color toColor, double durationSeconds)
        {
            var brush = new SolidColorBrush(fromColor);
            rectangle.Fill = brush;

            var animation = new ColorAnimation()
            {
                From = fromColor,
                To = toColor,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }


        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text.Trim();
            string password = IsPasswordVisible ? PasswordVisible.Text.Trim() : PasswordInvisible.Password.Trim();

            LoginError.Visibility = Visibility.Hidden;

            if (string.IsNullOrEmpty(username))
            {
                LoginError.Text = "Username cannot be empty.";
                LoginError.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                LoginError.Text = "Password cannot be empty.";
                LoginError.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT UserId, PasswordHash, Salt FROM Users WHERE Username=@username";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                LoginError.Text = "Incorrect username or password. Try again!";
                                LoginError.Visibility = Visibility.Visible;
                                return;
                            }

                            int userId = reader.GetInt32(0);
                            string storedHash = reader.GetString(1);
                            string storedSalt = reader.GetString(2);

                            string enteredHash = PasswordHelper.ComputeHash(password, storedSalt);

                            if (enteredHash != storedHash)
                            {
                                LoginError.Text = "Incorrect username or password. Try again!";
                                LoginError.Visibility = Visibility.Visible;
                                return;
                            }

                            DashboardView dashboard = new DashboardView(userId);
                            dashboard.Show();
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
