using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace MorningWidget
{
    public static class UiFactory
    {
        public static Border WrapInput(UIElement input, double bottomMargin) => new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(51, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Margin = new Thickness(0, 0, 0, bottomMargin),
            Child = input
        };

        public static Border CreateGlassCard(UIElement child)
        {
            var bgBrush = new SolidColorBrush(Color.FromArgb(8, 255, 255, 255));
            var borderBrush = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
            var scale = new ScaleTransform(1, 1);

            var card = new Border
            {
                Background = bgBrush,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(18, 16, 18, 16),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = scale,
                Child = child
            };

            card.MouseEnter += (s, e) =>
            {
                bgBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(Color.FromArgb(20, 255, 255, 255), TimeSpan.FromMilliseconds(220)));
                borderBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(Color.FromArgb(45, 255, 255, 255), TimeSpan.FromMilliseconds(220)));
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.015, TimeSpan.FromMilliseconds(220)));
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.015, TimeSpan.FromMilliseconds(220)));
            };
            card.MouseLeave += (s, e) =>
            {
                bgBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(Color.FromArgb(8, 255, 255, 255), TimeSpan.FromMilliseconds(220)));
                borderBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(Color.FromArgb(20, 255, 255, 255), TimeSpan.FromMilliseconds(220)));
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(220)));
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(220)));
            };

            return card;
        }

        public static Border CreateAccentButton(string text)
        {
            var shift = new TranslateTransform(0, 0);
            var glow = new DropShadowEffect { BlurRadius = 10, ShadowDepth = 0, Opacity = 0.25, Color = Color.FromRgb(99, 102, 241) };

            var btn = new Border
            {
                Height = 46,
                Background = new SolidColorBrush(Color.FromRgb(99, 102, 241)),
                CornerRadius = new CornerRadius(14),
                Cursor = Cursors.Hand,
                RenderTransform = shift,
                Effect = glow,
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            btn.MouseEnter += (s, e) =>
            {
                shift.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(-2, TimeSpan.FromMilliseconds(200)));
                glow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.65, TimeSpan.FromMilliseconds(200)));
                glow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(20, TimeSpan.FromMilliseconds(200)));
            };
            btn.MouseLeave += (s, e) =>
            {
                shift.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(200)));
                glow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.25, TimeSpan.FromMilliseconds(200)));
                glow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(10, TimeSpan.FromMilliseconds(200)));
            };

            return btn;
        }

        public static Border CreateIconButton(string icon, string tooltip)
        {
            var bg = new SolidColorBrush(Color.FromArgb(13, 255, 255, 255));
            var btn = new Border
            {
                Width = 32,
                Height = 32,
                Background = bg,
                BorderBrush = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Cursor = Cursors.Hand,
                ToolTip = tooltip,
                Child = new TextBlock
                {
                    Text = icon,
                    Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                    FontSize = 13,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            btn.MouseEnter += (s, e) => bg.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(Color.FromArgb(35, 255, 255, 255), TimeSpan.FromMilliseconds(180)));
            btn.MouseLeave += (s, e) => bg.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(Color.FromArgb(13, 255, 255, 255), TimeSpan.FromMilliseconds(180)));

            return btn;
        }

        public static TextBlock CreateRateColumn(Grid grid, int col, string label)
        {
            var sp = new StackPanel();
            sp.Children.Add(new TextBlock
            {
                Text = label,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                FontSize = 12,
                FontWeight = FontWeights.Medium
            });
            var val = new TextBlock
            {
                Text = "-- ₽",
                Foreground = Brushes.White,
                FontSize = 17,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 4, 0, 0)
            };
            sp.Children.Add(val);
            Grid.SetColumn(sp, col);
            grid.Children.Add(sp);
            return val;
        }
    }
}
