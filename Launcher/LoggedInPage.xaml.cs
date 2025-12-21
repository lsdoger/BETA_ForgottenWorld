using Launcher.Models;
using System.Windows;
using System.Windows.Controls; // Потрібно для роботи з ListBoxItem
using System.Windows.Input;

namespace Launcher
{
    public partial class LoggedInPage : Window
    {
        private readonly UserSession _currentUser;

        // Конструктор приймає сесію
        public LoggedInPage(UserSession session)
        {
            InitializeComponent();

            _currentUser = session;

            // ✅ ГОЛОВНА ЗМІНА: Робимо нікнейм ВЕЛИКИМИ ЛІТЕРАМИ
            if (!string.IsNullOrEmpty(_currentUser.Username))
            {
                UsernameText.Text = _currentUser.Username.ToUpper();
            }
            else
            {
                UsernameText.Text = "PLAYER";
            }
        }

        // Перетягування вікна за верхню панель
        private void TopMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        // Закриття програми
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Натискання кнопки Play
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            // Отримуємо вибраний сервер зі списку
            string serverName = "Unknown Server";

            if (ServersList.SelectedItem is ListBoxItem selectedItem)
            {
                serverName = selectedItem.Content.ToString();
            }

            // Виводимо повідомлення (тут пізніше буде запуск гри)
            MessageBox.Show(
                $"Server: {serverName}\nUser: {_currentUser.Username}\nToken: {_currentUser.Token}",
                "Starting Game...",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}