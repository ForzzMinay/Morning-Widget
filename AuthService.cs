using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace MorningWidget
{
    public class UserProfile
    {
        public string Name { get; set; } = "";
        public string PasswordHash { get; set; } = "";
    }

    public class AuthService
    {
        private const string APP_NAME = "MorningPremiumWidget";
        private readonly string _authFilePath;

        public AuthService()
        {
            string appFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                APP_NAME);
            
            // Гарантируем создание папки, если её нет
            Directory.CreateDirectory(appFolder);
            _authFilePath = Path.Combine(appFolder, "auth.json");
        }

        /// <summary>
        /// Проверяет, существует ли сохраненный профиль (для авто-входа при запуске).
        /// </summary>
        public bool HasProfile()
        {
            return File.Exists(_authFilePath);
        }

        /// <summary>
        /// Загружает профиль пользователя из файла.
        /// </summary>
        public UserProfile? LoadProfile()
        {
            if (!File.Exists(_authFilePath)) return null;
            
            try
            {
                string json = File.ReadAllText(_authFilePath);
                var profile = JsonSerializer.Deserialize<UserProfile>(json);
                
                // Возвращаем null, если профиль пустой или поврежденный
                return string.IsNullOrWhiteSpace(profile?.Name) ? null : profile;
            }
            catch
            {
                // В случае ошибки чтения/десериализации считаем, что профиля нет
                return null;
            }
        }

        /// <summary>
        /// Сохраняет или обновляет профиль пользователя.
        /// </summary>
        public void SaveProfile(string name, string password)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Имя и пароль не могут быть пустыми.");
            }

            var profile = new UserProfile
            {
                Name = name.Trim(),
                PasswordHash = ComputeSha256(password)
            };
            
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_authFilePath, JsonSerializer.Serialize(profile, options));
        }

        /// <summary>
        /// Проверяет корректность введенных данных при входе.
        /// </summary>
        public bool Authenticate(string name, string password)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
                return false;

            var profile = LoadProfile();
            if (profile == null) return false;

            // Сравниваем имя и хеш введенного пароля с сохраненным хешем
            return profile.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase) && 
                   profile.PasswordHash == ComputeSha256(password);
        }

        /// <summary>
        /// Полностью удаляет данные авторизации (выход из аккаунта).
        /// </summary>
        public void DeleteProfile()
        {
            if (File.Exists(_authFilePath))
            {
                File.Delete(_authFilePath);
            }
        }

        /// <summary>
        /// Добавляет приложение в автозагрузку Windows.
        /// </summary>
        public bool RegisterInStartup()
        {
            try
            {
                // Environment.ProcessPath доступен в .NET 5+. 
                // Если используете старый .NET Framework, замените на: Environment.GetCommandLineArgs()[0]
                string? exePath = Environment.ProcessPath; 
                if (string.IsNullOrEmpty(exePath)) return false;

                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writable: true);
                
                key?.SetValue(APP_NAME, $"\"{exePath}\"");
                return true;
            }
            catch
            {
                // Если нет прав или другая ошибка, просто возвращаем false
                return false;
            }
        }

        /// <summary>
        /// Вычисляет SHA-256 хеш строки.
        /// </summary>
        private static string ComputeSha256(string rawData)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}