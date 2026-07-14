using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SynV3
{
    public enum NType { Info, Success, Warning, Error }

    public class Toast
    {
        private static StackPanel? NotificationsPanel;
        private static readonly Queue<Border> ActiveToasts = new();
        private static readonly Queue<(Border Toast, string Title, string Message)> PendingToasts = new();
        private static readonly object QueueLock = new();
        private static bool IsShowingToast;

        private const int MaxToasts = 5;
        private const int DisplayTimeMs = 5000;

        private const int AnimationDurationMs = 400;
        private const int ToastSpacingMs = 200;

        private static readonly Dictionary<NType, (string Color, string Icon)> ToastStyles = new()
        {
            [NType.Info] = ("#0252a0", "Info"),
            [NType.Success] = ("#1ea049", "Success"),
            [NType.Warning] = ("#d09726", "Warning"),
            [NType.Error] = ("#d52a33", "Error")
        };

        public static void Initialize(StackPanel Stack) => NotificationsPanel = Stack;

        public static void Show(string Title, string Message, NType Type)
        {
            ArgumentNullException.ThrowIfNull(NotificationsPanel, "Toast system not initialized. Call Initialize first.");

            Application.Current.Dispatcher.Invoke(async () =>
            {
                var ToastBorder = CreateToast(Title, Message, Type);

                lock (QueueLock)
                {
                    PendingToasts.Enqueue((ToastBorder, Title, Message));
                }

                if (!IsShowingToast)
                {
                    await ShowNextToast();
                }
            });
        }

        private static async Task ShowNextToast()
        {
            if (PendingToasts.Count == 0)
            {
                IsShowingToast = false;
                return;
            }

            IsShowingToast = true;

            Border? ToastToShow = null;
            lock (QueueLock)
            {
                if (PendingToasts.Count > 0)
                {
                    var (Toast, _, _) = PendingToasts.Dequeue();
                    ToastToShow = Toast;
                }
            }

            if (ToastToShow != null)
            {
                await AddToast(ToastToShow);
                await Task.Delay(ToastSpacingMs);
                await ShowNextToast();
            }
        }

        private static Border CreateToast(string Title, string Message, NType Type)
        {
            var (Background, Icon) = ToastStyles[Type];

            var ToastBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Background)),
                CornerRadius = new CornerRadius(2),
                Width = 240,
                Height = 48,
                Margin = new Thickness(0, 8, 0, 0),
                RenderTransform = new TransformGroup
                {
                    Children = new TransformCollection
                    {
                        new TranslateTransform(320, 0),
                        new ScaleTransform(1, 1)
                    }
                },
                Opacity = 0,
                Child = new Grid
                {
                    Children =
                    {
                        new StackPanel
                        {
                            Margin = new Thickness(8),
                            Children =
                            {
                                new StackPanel
                                {
                                    Orientation = Orientation.Horizontal,
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = Title,
                                            FontSize = 11.2,
                                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d8ddde")),
                                            FontFamily = Application.Current.Resources["Poppins Medium"] as FontFamily
                                        },
                                        new Path
                                        {
                                            Data = Application.Current.Resources[Icon] as Geometry,
                                            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d8ddde")),
                                            Stretch = Stretch.Uniform,
                                            Width = 9.6,
                                            Margin = new Thickness(4.8, 0, 0, 0)
                                        }
                                    }
                                },
                                new TextBlock
                                {
                                    Text = Message,
                                    FontSize = 9.6,
                                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d3d3d3")),
                                    FontFamily = Application.Current.Resources["Poppins Regular"] as FontFamily
                                }
                            }
                        }
                    }
                }
            };

            return ToastBorder;
        }

        private static async Task AddToast(Border ToastBorder)
        {
            var RemoveOldToast = false;
            Border? OldestToast = null;

            lock (QueueLock)
            {
                if (ActiveToasts.Count >= MaxToasts)
                {
                    OldestToast = ActiveToasts.Dequeue();
                    RemoveOldToast = true;
                }

                NotificationsPanel!.Children.Add(ToastBorder);
                ActiveToasts.Enqueue(ToastBorder);
            }

            if (RemoveOldToast && OldestToast != null)
            {
                await ToastOut(OldestToast);
            }

            ToastIn(ToastBorder);

            var Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(DisplayTimeMs)
            };

            Timer.Tick += async (_, _) =>
            {
                Timer.Stop();
                await ToastOut(ToastBorder);

                lock (QueueLock)
                {
                    if (ActiveToasts.Count > 0 && ActiveToasts.Peek() == ToastBorder)
                    {
                        ActiveToasts.Dequeue();
                    }
                }
            };

            Timer.Start();
        }

        private static void ToastIn(Border ToastBorder)
        {
            var TransformGroup = (TransformGroup)ToastBorder.RenderTransform;
            var TranslateTransform = (TranslateTransform)TransformGroup.Children[0];
            var ScaleTransform = (ScaleTransform)TransformGroup.Children[1];

            var SlideAnimation = new DoubleAnimation
            {
                From = 320,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(AnimationDurationMs),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5 }
            };

            var FadeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(AnimationDurationMs),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5 }
            };

            var ScaleAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(AnimationDurationMs),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5 }
            };

            TranslateTransform.BeginAnimation(TranslateTransform.XProperty, SlideAnimation);
            ToastBorder.BeginAnimation(UIElement.OpacityProperty, FadeAnimation);
            ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, ScaleAnimation);
            ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, ScaleAnimation);
        }

        private static async Task ToastOut(Border ToastBorder)
        {
            var TransformGroup = (TransformGroup)ToastBorder.RenderTransform;
            var TranslateTransform = (TranslateTransform)TransformGroup.Children[0];
            var ScaleTransform = (ScaleTransform)TransformGroup.Children[1];

            var SlideAnimation = new DoubleAnimation
            {
                From = 0,
                To = 320,
                Duration = TimeSpan.FromMilliseconds(AnimationDurationMs),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5 }
            };

            var FadeAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(AnimationDurationMs),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5 }
            };

            var ScaleAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0.8,
                Duration = TimeSpan.FromMilliseconds(AnimationDurationMs),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5 }
            };

            var TaskCompletionSource = new TaskCompletionSource();
            SlideAnimation.Completed += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (NotificationsPanel!.Children.Contains(ToastBorder))
                    {
                        NotificationsPanel.Children.Remove(ToastBorder);
                    }
                    TaskCompletionSource.SetResult();
                });
            };

            TranslateTransform.BeginAnimation(TranslateTransform.XProperty, SlideAnimation);
            ToastBorder.BeginAnimation(UIElement.OpacityProperty, FadeAnimation);
            ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, ScaleAnimation);
            ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, ScaleAnimation);

            await TaskCompletionSource.Task;
        }
    }
}