using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace SynV3
{
    public static class Messages
    {
        private static Border? MessageContainer;
        private static TaskCompletionSource<bool>? UserResponse;
        private static Grid? MainContainer;

        public static void Initialize(Border Border, Grid Grid)
        {
            MessageContainer = Border;
            MainContainer = Grid;

            MessageContainer.Visibility = Visibility.Visible;
        }

        private static void AnimateScale(FrameworkElement Element, bool IsAppearing, Action? CompletionCallback = null)
        {
            var ScaleTransform = new ScaleTransform(IsAppearing ? 0.3 : 1, IsAppearing ? 0.3 : 1);
            Element.RenderTransform = ScaleTransform;
            Element.RenderTransformOrigin = new Point(0.5, 0.5);

            var ScaleAnimation = new DoubleAnimation
            {
                From = IsAppearing ? 0.3 : 1,
                To = IsAppearing ? 1 : 0.3,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            var OpacityAnimation = new DoubleAnimation
            {
                From = IsAppearing ? 0 : 1,
                To = IsAppearing ? 1 : 0,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            if (CompletionCallback != null)
                OpacityAnimation.Completed += (s, e) => CompletionCallback();

            ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, ScaleAnimation);
            ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, ScaleAnimation);
            Element.BeginAnimation(UIElement.OpacityProperty, OpacityAnimation);
        }

        public static Task<bool> Show(string Title, string Message, bool ShowYesNo = false)
        {
            UserResponse = new TaskCompletionSource<bool>();
            ShowMessage(Title, Message, ShowYesNo);
            return UserResponse.Task;
        }

        public static void ShowMessage(string Title, string Message, bool ShowYesNo = false, Action<bool>? Callback = null)
        {
            var MainGrid = new Grid();

            var OuterBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#101010")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#313131")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Width = 250,
                Height = 150,
                Opacity = 0
            };

            var InnerGrid = new Grid();
            var MainStackPanel = new StackPanel();

            var HeaderStackPanel = new StackPanel
            {
                Margin = new Thickness(10, 10, 10, 0),
                Orientation = Orientation.Horizontal
            };

            var InfoPath = new Path
            {
                Data = Application.Current.Resources["Info"] as Geometry,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0")),
                Width = 15,
                Margin = new Thickness(0, 0, 8, 0),
                Stretch = Stretch.Uniform
            };

            var TitleBlock = new TextBlock
            {
                Text = Title,
                FontSize = 15,
                FontFamily = Application.Current.Resources["Poppins Medium"] as FontFamily,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0"))
            };

            HeaderStackPanel.Children.Add(InfoPath);
            HeaderStackPanel.Children.Add(TitleBlock);

            var MessageBlock = new TextBlock
            {
                Text = Message,
                Margin = new Thickness(10, 8, 10, 0),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                FontFamily = Application.Current.Resources["Poppins Medium"] as FontFamily,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0"))
            };

            MainStackPanel.Children.Add(HeaderStackPanel);
            MainStackPanel.Children.Add(MessageBlock);

            var ButtonBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#030303")),
                Height = 45,
                VerticalAlignment = VerticalAlignment.Bottom,
                CornerRadius = new CornerRadius(0, 0, 8, 8)
            };

            var ButtonStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            void HandleResult(bool result)
            {
                AnimateScale(OuterBorder, false, () =>
                {
                    MainContainer!.Children.Remove(MainGrid);
                    UserResponse?.SetResult(result);
                    Callback?.Invoke(result);
                });
            }

            if (ShowYesNo)
            {
                var YesButton = new Button
                {
                    Content = "Yes",
                    Margin = new Thickness(8),
                    Width = 45,
                    Height = 25,
                    Style = Application.Current.Resources["MiscN"] as Style
                };
                YesButton.Click += (s, e) => HandleResult(true);

                var NoButton = new Button
                {
                    Content = "No",
                    Margin = new Thickness(0, 8, 8, 8),
                    Width = 45,
                    Height = 25,
                    Style = Application.Current.Resources["MiscN"] as Style
                };
                NoButton.Click += (s, e) => HandleResult(false);

                ButtonStackPanel.Children.Add(YesButton);
                ButtonStackPanel.Children.Add(NoButton);
            }
            else
            {
                var OkButton = new Button
                {
                    Content = "OK",
                    Margin = new Thickness(8),
                    Width = 45,
                    Height = 25,
                    Style = Application.Current.Resources["MiscN"] as Style
                };
                OkButton.Click += (s, e) => HandleResult(true);

                ButtonStackPanel.Children.Add(OkButton);
            }

            ButtonBorder.Child = ButtonStackPanel;

            InnerGrid.Children.Add(MainStackPanel);
            InnerGrid.Children.Add(ButtonBorder);

            OuterBorder.Child = InnerGrid;
            MainGrid.Children.Add(OuterBorder);

            MainContainer!.Children.Add(MainGrid);
            AnimateScale(OuterBorder, true);
        }
    }
}