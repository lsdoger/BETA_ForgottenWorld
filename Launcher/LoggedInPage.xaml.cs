using Launcher.Models;
using System; // Потрібно для TimeSpan
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation; // ✅ ВАЖЛИВО: Додайте це для анімацій

namespace Launcher
{
    public partial class LoggedInPage : Window
    {
        private readonly UserSession _currentUser;

        public LoggedInPage(UserSession session)
        {
            InitializeComponent();
            _currentUser = session;

            if (!string.IsNullOrEmpty(_currentUser.Username))
            {
                UsernameText.Text = _currentUser.Username.ToUpper();
            }
            else
            {
                UsernameText.Text = "PLAYER";
            }

            // Додаємо подію, щоб при розгортанні вікна з панелі задач воно плавно з'являлося назад
            this.StateChanged += MainWindow_StateChanged;
        }

        // --- ЛОГІКА АНІМОВАНОГО ЗГОРТАННЯ ---
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            // 1. Створюємо анімацію зникнення (Opacity від 1 до 0)
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.3)) // Швидкість анімації
            };

            // 2. Що робити, коли анімація закінчилась
            fadeOut.Completed += (s, _) =>
            {
                this.WindowState = WindowState.Minimized;

                // ВАЖЛИВО: Повертаємо прозорість назад, щоб коли ми розгорнемо вікно, воно не було невидимим
                this.BeginAnimation(UIElement.OpacityProperty, null);
                this.Opacity = 1.0;
            };

            // 3. Запускаємо анімацію
            this.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        // Цей метод робить так, що коли ти натискаєш на іконку в панелі задач, 
        // вікно не просто "стрибає", а теж може мати логіку (тут просто скидання)
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                // Тут можна додати анімацію появи, якщо хочеться, 
                // але Windows сам непогано справляється з розгортанням.
                // Головне переконатися, що вікно видиме:
                this.Opacity = 1.0;
            }
        }

        // --- ІНШІ МЕТОДИ БЕЗ ЗМІН ---

        private void TopMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            string serverName = "Unknown Server";
            if (ServersList.SelectedItem is ListBoxItem selectedItem && selectedItem.Content != null)
            {
                serverName = selectedItem.Content.ToString() ?? "Unknown";
            }

            MessageBox.Show(
                $"Server: {serverName}\nUser: {_currentUser.Username}\nToken: {_currentUser.Token}",
                "Starting Game...",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}