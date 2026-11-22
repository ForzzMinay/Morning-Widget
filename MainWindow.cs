using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MorningWidget
{
    public class MainWindow : Window
    {
        private readonly AuthService authService = new AuthService();
        private readonly ApiService apiService = new ApiService(); // Убедитесь, что этот класс у вас есть
        private readonly DispatcherTimer checkTimer = new DispatcherTimer();

        private string currentUserName = "Пользователь";
        private bool? wasMorning = null;

        private StackPanel authPanel = null!;
        private StackPanel dashboardPanel = null!;
        private TextBox nameInput = null!;
        private PasswordBox passwordInput = null!;
        private TextBlock authErrorText = null!;

        private TextBlock greetingText = null!;
        private TextBlock subDateText = null!;
        private Border weatherCard = null!;
        private TextBlock weatherDescText = null!;
        private TextBlock weatherDetailsText = null!;
        private TextBlock tempText = null!;

        private TextBlock usdText = null!;
        private TextBlock eurText = null!;
        private TextBlock cnyText = null!;
        private TextBlock startupStatusText = null!;

        public MainWindow()
        {
            Title = "Premium Dashboard";
            Width = 460;
            SizeToContent = SizeToContent.Height;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Topmost = true;

            MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };

            checkTimer.Interval = TimeSpan.FromSeconds(10);
            checkTimer.Tick += CheckTimer_Tick;

            BuildUi();

            bool isRegistered = authService.RegisterInStartup();
            startupStatusText.Text = isRegistered
                ? "В автозагрузке Windows"
                : "Ошибка записи в автозагрузку";

            var profile = authService.LoadProfile();
            if (profile != null)
            {
                currentUserName = profile.Name;
                ShowDashboard();
            }
            else
            {
                authPanel.Visibility = Visibility.Visible;
                dashboardPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void CheckTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                await UpdateStateByTimeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления состояния: {ex.Message}");
            }
        }

        private void BuildUi()
        {
            var slideTransform = new TranslateTransform(0, 14);
            var mainContainer = new Border();
            mainContainer.CornerRadius = new CornerRadius(24);
            mainContainer.BorderBrush = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
            mainContainer.BorderThickness = new Thickness(1);
            mainContainer.Margin = new Thickness(20);
            mainContainer.Opacity = 0;
            mainContainer.RenderTransform = slideTransform;
            mainContainer.Background = new LinearGradientBrush(
                Color.FromArgb(250, 15, 23, 42),
                Color.FromArgb(250, 30, 27, 75),
                new Point(0, 0),
                new Point(1, 1));
            mainContainer.Effect = new DropShadowEffect
            {
                BlurRadius = 30,
                ShadowDepth = 10,
                Opacity = 0.5,
                Color = Colors.Black
            };

            Loaded += (s, e) =>
            {
                mainContainer.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400)));
                var slideAnim = new DoubleAnimation(14, 0, TimeSpan.FromMilliseconds(400));
                slideAnim.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                slideTransform.BeginAnimation(TranslateTransform.YProperty, slideAnim);
            };

            var rootGrid = new Grid();

            var bgEllipse = new Ellipse();
            bgEllipse.Width = 300;
            bgEllipse.Height = 300;
            bgEllipse.Fill = new SolidColorBrush(Color.FromRgb(99, 102, 241));
            bgEllipse.Opacity = 0.16;
            bgEllipse.HorizontalAlignment = HorizontalAlignment.Left;
            bgEllipse.VerticalAlignment = VerticalAlignment.Top;
            bgEllipse.Margin = new Thickness(-80, -80, 0, 0);
            bgEllipse.IsHitTestVisible = false;
            bgEllipse.Effect = new BlurEffect { Radius = 90 };
            rootGrid.Children.Add(bgEllipse);

            var glassLayer = new Border();
            glassLayer.Background = new SolidColorBrush(Color.FromArgb(8, 255, 255, 255));
            glassLayer.CornerRadius = new CornerRadius(24);
            glassLayer.Padding = new Thickness(32);

            var contentGrid = new Grid();

            // === ЭКРАН 1: АВТОРИЗАЦИЯ ===
            authPanel = new StackPanel();
            authPanel.Visibility = Visibility.Collapsed;

            var authTitle = new TextBlock();
            authTitle.Text = "Настройка профиля";
            authTitle.Foreground = Brushes.White;
            authTitle.FontSize = 26;
            authTitle.FontWeight = FontWeights.SemiBold;
            authTitle.Margin = new Thickness(0, 0, 0, 6);
            authPanel.Children.Add(authTitle);

            var authSub = new TextBlock();
            authSub.Text = "Данные сохранятся в локальный файл";
            authSub.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            authSub.FontSize = 14;
            authSub.Margin = new Thickness(0, 0, 0, 24);
            authPanel.Children.Add(authSub);

            var nameLabel = new TextBlock();
            nameLabel.Text = "Ваше имя";
            nameLabel.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            nameLabel.FontSize = 12;
            nameLabel.Margin = new Thickness(0, 0, 0, 6);
            authPanel.Children.Add(nameLabel);

            nameInput = new TextBox();
            nameInput.Height = 42;
            nameInput.Background = Brushes.Transparent;
            nameInput.Foreground = Brushes.White;
            nameInput.BorderThickness = new Thickness(0);
            nameInput.Padding = new Thickness(14, 10, 14, 10);
            nameInput.FontSize = 15;
            nameInput.CaretBrush = Brushes.White;
            authPanel.Children.Add(UiFactory.WrapInput(nameInput, 16));

            var passLabel = new TextBlock();
            passLabel.Text = "Пароль для входа";
            passLabel.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            passLabel.FontSize = 12;
            passLabel.Margin = new Thickness(0, 0, 0, 6);
            authPanel.Children.Add(passLabel);

            passwordInput = new PasswordBox();
            passwordInput.Height = 42;
            passwordInput.Background = Brushes.Transparent;
            passwordInput.Foreground = Brushes.White;
            passwordInput.BorderThickness = new Thickness(0);
            passwordInput.Padding = new Thickness(14, 10, 14, 10);
            passwordInput.FontSize = 15;
            passwordInput.CaretBrush = Brushes.White;
            authPanel.Children.Add(UiFactory.WrapInput(passwordInput, 10));

            authErrorText = new TextBlock();
            authErrorText.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            authErrorText.FontSize = 13;
            authErrorText.Margin = new Thickness(0, 0, 0, 12);
            authPanel.Children.Add(authErrorText);

            // ИСПРАВЛЕНИЕ 1: Используем MouseLeftButtonDown + e.Handled = true для гарантии срабатывания
            var saveBtn = UiFactory.CreateAccentButton("Сохранить и войти");
            saveBtn.MouseLeftButtonDown += (s, e) => 
            { 
                OnSaveAuth(); 
                e.Handled = true; 
            };
            authPanel.Children.Add(saveBtn);

            // === ЭКРАН 2: ДАШБОРД ===
            dashboardPanel = new StackPanel();
            dashboardPanel.Visibility = Visibility.Visible;

            var headerGrid = new Grid();
            headerGrid.Margin = new Thickness(0, 0, 0, 24);

            var titleStack = new StackPanel();
            titleStack.Margin = new Thickness(0, 0, 75, 0);

            greetingText = new TextBlock();
            greetingText.Text = "Добрый день, Пользователь";
            greetingText.Foreground = Brushes.White;
            greetingText.FontSize = 23;
            greetingText.FontWeight = FontWeights.SemiBold;
            greetingText.TextTrimming = TextTrimming.CharacterEllipsis;

            subDateText = new TextBlock();
            subDateText.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            subDateText.FontSize = 13;
            subDateText.Margin = new Thickness(0, 4, 0, 0);

            titleStack.Children.Add(greetingText);
            titleStack.Children.Add(subDateText);
            headerGrid.Children.Add(titleStack);

            var buttonsStack = new StackPanel();
            buttonsStack.Orientation = Orientation.Horizontal;
            buttonsStack.HorizontalAlignment = HorizontalAlignment.Right;
            buttonsStack.VerticalAlignment = VerticalAlignment.Top;

            // ИСПРАВЛЕНИЕ 1 (продолжение): То же самое для остальных кнопок
            var resetBtn = UiFactory.CreateIconButton("\u2699", "Сменить профиль");
            resetBtn.Margin = new Thickness(0, 0, 8, 0);
            resetBtn.MouseLeftButtonDown += (s, e) => 
            { 
                OnResetAuth(); 
                e.Handled = true; 
            };

            var closeBtn = UiFactory.CreateIconButton("\u2715", "Закрыть");
            closeBtn.MouseLeftButtonDown += (s, e) => 
            { 
                Application.Current.Shutdown(); 
                e.Handled = true; 
            };

            buttonsStack.Children.Add(resetBtn);
            buttonsStack.Children.Add(closeBtn);
            headerGrid.Children.Add(buttonsStack);
            dashboardPanel.Children.Add(headerGrid);

            // Карточка погоды
            var weatherGrid = new Grid();
            var wInfoStack = new StackPanel();
            wInfoStack.VerticalAlignment = VerticalAlignment.Center;

            var wCaption = new TextBlock();
            wCaption.Text = "ПОГОДА НА УЛИЦЕ";
            wCaption.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            wCaption.FontSize = 11;
            wCaption.FontWeight = FontWeights.SemiBold;
            
            weatherDescText = new TextBlock();
            weatherDescText.Text = "Загрузка...";
            weatherDescText.Foreground = Brushes.White;
            weatherDescText.FontSize = 16;
            weatherDescText.FontWeight = FontWeights.Medium;
            weatherDescText.Margin = new Thickness(0, 4, 0, 0);
            
            weatherDetailsText = new TextBlock();
            weatherDetailsText.Text = "Влажность: --%  |  Ветер: -- м/с";
            weatherDetailsText.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            weatherDetailsText.FontSize = 12;
            weatherDetailsText.Margin = new Thickness(0, 4, 0, 0);
            
            wInfoStack.Children.Add(wCaption);
            wInfoStack.Children.Add(weatherDescText);
            wInfoStack.Children.Add(weatherDetailsText);
            
            tempText = new TextBlock();
            tempText.Text = "--\u00B0";
            tempText.HorizontalAlignment = HorizontalAlignment.Right;
            tempText.VerticalAlignment = VerticalAlignment.Center;
            tempText.Foreground = new SolidColorBrush(Color.FromRgb(129, 140, 248));
            tempText.FontSize = 38;
            tempText.FontWeight = FontWeights.Bold;
            
            weatherGrid.Children.Add(wInfoStack);
            weatherGrid.Children.Add(tempText);
            
            weatherCard = UiFactory.CreateGlassCard(weatherGrid);
            weatherCard.Visibility = Visibility.Collapsed;
            weatherCard.Margin = new Thickness(0, 0, 0, 12);
            dashboardPanel.Children.Add(weatherCard);
            
            // Карточка курсов
            var currStack = new StackPanel();
            var currCaption = new TextBlock();
            currCaption.Text = "КУРСЫ ВАЛЮТ ЦБ РФ";
            currCaption.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            currCaption.FontSize = 11;
            currCaption.FontWeight = FontWeights.SemiBold;
            currCaption.Margin = new Thickness(0, 0, 0, 12);
            currStack.Children.Add(currCaption);
            
            var currGrid = new Grid();
            currGrid.ColumnDefinitions.Add(new ColumnDefinition());
            currGrid.ColumnDefinitions.Add(new ColumnDefinition());
            currGrid.ColumnDefinitions.Add(new ColumnDefinition());
            
            usdText = UiFactory.CreateRateColumn(currGrid, 0, "USD $");
            eurText = UiFactory.CreateRateColumn(currGrid, 1, "EUR \u20AC");
            cnyText = UiFactory.CreateRateColumn(currGrid, 2, "CNY \u00A5");
            
            currStack.Children.Add(currGrid);
            dashboardPanel.Children.Add(UiFactory.CreateGlassCard(currStack));
            
            startupStatusText = new TextBlock();
            startupStatusText.Text = "В автозагрузке Windows";
            startupStatusText.Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105));
            startupStatusText.FontSize = 11;
            startupStatusText.HorizontalAlignment = HorizontalAlignment.Center;
            startupStatusText.Margin = new Thickness(0, 18, 0, 0);
            dashboardPanel.Children.Add(startupStatusText);
            
            contentGrid.Children.Add(authPanel);
            contentGrid.Children.Add(dashboardPanel);
            glassLayer.Child = contentGrid;
            rootGrid.Children.Add(glassLayer);
            mainContainer.Child = rootGrid;
            Content = mainContainer;
        }

        // ИСПРАВЛЕНИЕ 2: Добавлен try-catch для перехвата скрытых ошибок
        private void OnSaveAuth()
        {
            try
            {
                string name = nameInput.Text.Trim();
                string password = passwordInput.Password;
                
                if (string.IsNullOrEmpty(name))
                {
                    authErrorText.Text = "Введите имя для приветствия";
                    return;
                }
                if (password.Length < 4)
                {
                    authErrorText.Text = "Пароль должен быть минимум 4 символа";
                    return;
                }
                
                authService.SaveProfile(name, password);
                currentUserName = name;
                authErrorText.Text = "";
                
                ShowDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnResetAuth()
        {
            authService.DeleteProfile();
            nameInput.Text = "";
            passwordInput.Password = "";
            authErrorText.Text = ""; // Очищаем ошибки при сбросе
            wasMorning = null;
            checkTimer.Stop();
            dashboardPanel.Visibility = Visibility.Collapsed;
            authPanel.Visibility = Visibility.Visible;
        }

        private async void ShowDashboard()
        {
            authPanel.Visibility = Visibility.Collapsed;
            dashboardPanel.Visibility = Visibility.Visible;
            
            await UpdateStateByTimeAsync();
            
            try
            {
                var rates = await apiService.GetCurrencyRatesAsync();
                usdText.Text = rates.Usd;
                eurText.Text = rates.Eur;
                cnyText.Text = rates.Cny;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки курсов: {ex.Message}");
                usdText.Text = "Ошибка";
                eurText.Text = "Ошибка";
                cnyText.Text = "Ошибка";
            }
            
            checkTimer.Start();
        }

        private async Task UpdateStateByTimeAsync()
        {
            DateTime now = DateTime.Now;
            int h = now.Hour;
            
            string greeting = h switch
            {
                >= 5 and < 12 => "Доброе утро",
                >= 12 and < 18 => "Добрый день",
                >= 18 and < 23 => "Добрый вечер",
                _ => "Доброй ночи"
            };
            
            greetingText.Text = $"{greeting}, {currentUserName}";
            string dateStr = now.ToString("dddd, d MMMM", new CultureInfo("ru-RU"));
            
            subDateText.Text = char.ToUpper(dateStr[0]) + dateStr.Substring(1);
            
            bool isMorning = h >= 6 && h < 10;
            if (wasMorning != isMorning)
            {
                wasMorning = isMorning;
                if (isMorning)
                {
                    weatherCard.Visibility = Visibility.Visible;
                    try
                    {
                        var weather = await apiService.GetWeatherAsync();
                        tempText.Text = weather.Temp;
                        weatherDescText.Text = weather.Description;
                        weatherDetailsText.Text = weather.Details;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки погоды: {ex.Message}");
                        weatherDescText.Text = "Ошибка загрузки";
                    }
                }
                else
                {
                    weatherCard.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}