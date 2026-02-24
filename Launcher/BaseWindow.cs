using System.Windows;
using System.Windows.Input;

namespace Launcher
{
    public class BaseWindow : Window
    {
        // Спільний метод для перетягування вікна
        protected void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        protected void TopMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        // Спільний метод для згортання
        protected void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Спільний метод для закриття
        protected void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}