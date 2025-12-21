using Launcher.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7085")
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
            var page = new LoggedInPage(session);
            MessageBox.Show("Username = " + session.Username);
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
    }

    public class AuthResponse
    {
        public string UserId { get; set; } = "";
        public string Username { get; set; } = "";
        public string Token { get; set; } = "";
    }
}
