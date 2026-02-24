using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Input;
using Launcher.Models;

namespace Launcher
{
    // 1. Успадковуємо від Window, щоб працював метод .Show()
    public partial class LoggedInPage : BaseWindow
    {
        private const string ApiUrl = AppConfig.BaseApiUrl + "/api/packs";
        private string _currentUsername;

        // 2. Додаємо аргумент username у конструктор
        public LoggedInPage(string username)
        {
            InitializeComponent();

            _currentUsername = username;

            // Встановлюємо нікнейм у інтерфейс
            UsernameText.Text = _currentUsername.ToUpper();

            // Завантажуємо сервери
            LoadServers();
        }

        private async void LoadServers()
        {
            StatusText.Text = "Loading servers...";
            try
            {
                using (var client = new HttpClient())
                {
                    var packs = await client.GetFromJsonAsync<List<ServerPack>>(ApiUrl);

                    ServersList.ItemsSource = packs;

                    if (packs != null && packs.Count > 0)
                        ServersList.SelectedIndex = 0;

                    StatusText.Text = "";
                }
            }
            catch (Exception)
            {
                StatusText.Text = "Failed to connect to update server.";
                // Фейковий список на випадок помилки
                ServersList.ItemsSource = new List<ServerPack>
                {
                    new ServerPack { Name = "Offline Mode / Error" }
                };
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedServer = ServersList.SelectedItem as ServerPack;

            // Перевірка на null з урахуванням нових налаштувань класу
            if (selectedServer == null || string.IsNullOrEmpty(selectedServer.ServerIp))
            {
                MessageBox.Show("Please select a valid server from the list.");
                return;
            }

            PlayButton.IsEnabled = false;
            PlayButton.Content = "STARTING...";
            StatusText.Text = "Checking files & installing...";

            try
            {
                var controller = new LauncherController();

                // Викликаємо метод запуску
                await controller.LaunchGameAsync(
                    selectedServer.MinecraftVersion ?? "1.21.1",
                    _currentUsername,
                    selectedServer.Ram > 0 ? selectedServer.Ram : 4096,
                    selectedServer.ServerIp,
                    selectedServer.ServerPort
                );

                StatusText.Text = "Game Launched!";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error launching game.";
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                PlayButton.IsEnabled = true;
                PlayButton.Content = "PLAY SELECTED";
            }
        }
    }
}