using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Finance_App.Helpers
{
    public static class WindowResizeHelper
    {
        public static void HandleResize(Window window, Grid middleForm,
            FrameworkElement leftRectangle, FrameworkElement rightRectangle)
        {
            if (window.ActualHeight >= 600)
                middleForm.VerticalAlignment = VerticalAlignment.Center;
            else
                middleForm.VerticalAlignment = VerticalAlignment.Top;

            if (window.ActualWidth < 600)
            {
                leftRectangle.Visibility = Visibility.Collapsed;
                rightRectangle.Visibility = Visibility.Collapsed;

                Grid.SetColumn(middleForm, 0);
                Grid.SetColumnSpan(middleForm, 3);
            }
            else if (window.ActualWidth < 850)
            {
                leftRectangle.Visibility = Visibility.Visible;
                rightRectangle.Visibility = Visibility.Visible;
                leftRectangle.Width = 100;
                rightRectangle.Width = 100;

                Grid.SetColumn(middleForm, 1);
                Grid.SetColumnSpan(middleForm, 1);
            }
            else
            {
                leftRectangle.Visibility = Visibility.Visible;
                rightRectangle.Visibility = Visibility.Visible;
                leftRectangle.Width = 150;
                rightRectangle.Width = 150;

                Grid.SetColumn(middleForm, 1);
                Grid.SetColumnSpan(middleForm, 1);
            }
        }
    }
}
