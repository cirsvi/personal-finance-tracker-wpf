using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Finance_App.Helpers
{
    public static class PasswordVisibilityHelper
    {
         public static void Toggle(Window owner, PasswordBox passwordBox,
            TextBox textBox, Path eyeIcon, ref bool isPasswordVisible) {
            
            if (isPasswordVisible)
            {
                eyeIcon.Data = (Geometry)owner.FindResource("EyeOpenIcon");
                textBox.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Visible;
                passwordBox.Password = textBox.Text;
            }
            else
            {
                eyeIcon.Data = (Geometry)owner.FindResource("EyeClosedIcon");
                textBox.Visibility = Visibility.Visible;
                passwordBox.Visibility = Visibility.Collapsed;
                textBox.Text = passwordBox.Password;
            }

            isPasswordVisible = !isPasswordVisible;
        }

    }
}
