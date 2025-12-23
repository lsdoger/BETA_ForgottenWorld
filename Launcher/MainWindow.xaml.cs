using Launcher.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Animation;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("http://134.249.146.36:5296")
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text?.Trim();

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Enter username");
                return;
            }

            var body = new { username };
            string json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            try
            {
                response = await _http.PostAsync("/api/auth/login", content);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Backend not available:\n" + ex.Message);
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Login failed");
                return;
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            // 👇 ДОДАЙТЕ ЦІ НАЛАШТУВАННЯ 👇
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Це дозволяє читати "username" як "Username"
            };

            var auth = JsonSerializer.Deserialize<AuthResponse>(responseJson, options); // Передаємо options сюди

            // Тепер auth.Username не буде пустим
            if (auth == null)
            {
                MessageBox.Show("Помилка обробки даних від сервера");
                return;
            }

            // Створення сесії
            var session = new UserSession
            {
                UserId = auth.UserId,
                Username = auth.Username, // Тепер тут буде "admin"
                Token = auth.Token
            };

            // ✅ ВІДКРИВАЄМО LoggedInPage І ПЕРЕДАЄМО СЕСІЮ
            var page = new LoggedInPage(session.Username);
            page.Show();

            this.Close(); // закриваємо login
        }


        private void TopMenu_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Метод для згортання вікна
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

        // Метод для відкриття налаштувань
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // Створюємо нове вікно налаштувань
            SettingsWindow settingsWin = new SettingsWindow();
            // Встановлюємо головне вікно як власника, щоб налаштування відкривались поверх
            settingsWin.Owner = this;
            // Відкриваємо як діалогове вікно (блокує головне вікно, поки не закриєш налаштування)
            settingsWin.ShowDialog();
        }

    }

    public class AuthResponse
    {
        public string UserId { get; set; } = "";
        public string Username { get; set; } = "";
        public string Token { get; set; } = "";
    }
}
