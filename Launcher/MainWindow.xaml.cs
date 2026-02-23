using Launcher.Models;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using Application = System.Windows.Application;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        // =========================
        // === FIELDS ==============
        // =========================
        private readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri(AppConfig.BaseApiUrl)
        };

        private NotifyIcon? _trayIcon;
        private bool _realExit = false;
        private Process? _minecraftProcess;

        // =========================
        // === CONSTRUCTOR =========
        // =========================
        public MainWindow()
        {
            InitializeComponent();
            InitTray();
        }

        // =========================
        // === TRAY ================
        // =========================
        private void InitTray()
        {
            try
            {
                string iconPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "launcher.ico"
                );

                _trayIcon = new NotifyIcon
                {
                    Icon = System.IO.File.Exists(iconPath)
                        ? new System.Drawing.Icon(iconPath)
                        : System.Drawing.SystemIcons.Application,
                    Text = "ForgottenWorld",
                    Visible = true
                };

                _trayIcon.DoubleClick += (s, e) => RestoreWindow();

                var menu = new ContextMenuStrip();
                menu.Items.Add("Open Launcher", null, (s, e) => RestoreWindow());
                menu.Items.Add("Exit", null, (s, e) =>
                {
                    _realExit = true;
                    KillMinecraft();
                    _trayIcon!.Visible = false;
                    Application.Current.Shutdown();
                });

                _trayIcon.ContextMenuStrip = menu;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "Tray init failed:\n" + ex.Message,
                    "Launcher error"
                );
            }
        }

        private void RestoreWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        // =========================
        // === CLOSE BEHAVIOR ======
        // =========================
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_realExit)
            {
                e.Cancel = true;
                Hide();

                _trayIcon?.ShowBalloonTip(
                    1000,
                    "Launcher minimized",
                    "Launcher is running in system tray",
                    ToolTipIcon.Info
                );
            }
            else
            {
                base.OnClosing(e);
            }
        }

        // =========================
        // === LOGIN & PLAY ========
        // =========================
        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            string? username = UsernameBox.Text?.Trim();

            if (string.IsNullOrEmpty(username))
            {
                System.Windows.MessageBox.Show("Enter username");
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
                System.Windows.MessageBox.Show("Backend not available:\n" + ex.Message);
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                System.Windows.MessageBox.Show("Login failed");
                return;
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var auth = JsonSerializer.Deserialize<AuthResponse>(responseJson, options);

            if (auth == null)
            {
                System.Windows.MessageBox.Show("Invalid server response");
                return;
            }

            var session = new UserSession
            {
                UserId = auth.UserId,
                Username = auth.Username,
                Token = auth.Token
            };

            var page = new LoggedInPage(session.Username);
            page.Show();

            Hide();
        }

        // =========================
        // === WINDOW CONTROLS =====
        // =========================
        private void TopMenu_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Close(); // в трей
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.25)
            };

            fadeOut.Completed += (s, _) =>
            {
                WindowState = WindowState.Minimized;
                BeginAnimation(UIElement.OpacityProperty, null);
                Opacity = 1.0;
            };

            BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWin = new SettingsWindow
            {
                Owner = this
            };
            settingsWin.ShowDialog();
        }

        // =========================
        // === MINECRAFT PROCESS ===
        // =========================
        public void SetMinecraftProcess(Process process)
        {
            _minecraftProcess = process;
        }

        private void KillMinecraft()
        {
            try
            {
                if (_minecraftProcess != null && !_minecraftProcess.HasExited)
                {
                    _minecraftProcess.Kill(true);
                }
            }
            catch { }
        }
    }

    // =========================
    // === DTO =================
    // =========================
    public class AuthResponse
    {
        public string UserId { get; set; } = "";
        public string Username { get; set; } = "";
        public string Token { get; set; } = "";
    }
}
