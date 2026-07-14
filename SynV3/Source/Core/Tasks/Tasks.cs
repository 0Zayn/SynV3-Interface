using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace SynV3
{
    public static class CTask
    {
        private static StackPanel? TaskList;
        private static Border? TasksContainer;

        private static readonly Duration FadeInDuration = new Duration(TimeSpan.FromMilliseconds(200));

        public static void Initialize(StackPanel Tasks, Border Container)
        {
            TaskList = Tasks;
            TasksContainer = Container;

            TasksContainer.Visibility = Visibility.Collapsed;
        }

        public static void Add(string Title, string Status)
        {
            var TaskBorder = new Border
            {
                Background = Brushes.Transparent,
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(5, 5, 5, 0),
                Opacity = 0
            };

            var MainGrid = new Grid();

            var ArrowGrid = new Grid
            {
                Width = 11,
                Height = 11,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };

            var ArrowPath = new Path
            {
                Data = Application.Current.Resources["CCArrow"] as Geometry,
                Fill = Brushes.Black,
                Stretch = Stretch.Uniform
            };
            ArrowGrid.Children.Add(ArrowPath);

            var HorizontalStack = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var TaskIconPath = new Path
            {
                Data = Application.Current.Resources["Task"] as Geometry,
                Margin = new Thickness(5, 10, 10, 10),
                Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#89a6b0"),
                Stretch = Stretch.Uniform,
                Height = 12
            };

            var TextStack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            var TitleBlock = new TextBlock
            {
                Text = Title,
                HorizontalAlignment = HorizontalAlignment.Left,
                FontSize = 12,
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#c5c5c5"),
                FontFamily = Application.Current.Resources["Poppins Medium"] as FontFamily
            };

            var StatusBlock = new TextBlock
            {
                Text = Status,
                HorizontalAlignment = HorizontalAlignment.Left,
                FontSize = 11,
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#676767"),
                FontFamily = Application.Current.Resources["Poppins Regular"] as FontFamily
            };

            TextStack.Children.Add(TitleBlock);
            TextStack.Children.Add(StatusBlock);

            HorizontalStack.Children.Add(TaskIconPath);
            HorizontalStack.Children.Add(TextStack);

            MainGrid.Children.Add(HorizontalStack);
            MainGrid.Children.Add(ArrowGrid);

            TaskBorder.Child = MainGrid;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (TaskList?.Children.Count == 0)
                {
                    TasksContainer!.Visibility = Visibility.Visible;

                    var CFade = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = FadeInDuration
                    };

                    TasksContainer.BeginAnimation(UIElement.OpacityProperty, CFade);
                }

                TaskList?.Children.Add(TaskBorder);

                var FadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = FadeInDuration
                };

                TaskBorder.BeginAnimation(UIElement.OpacityProperty, FadeIn);
            });
        }

        public static void Remove(int Index)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (TaskList != null && TaskList.Children.Count > 0)
                {
                    int I;
                    if (Index == -1)
                        I = TaskList.Children.Count - 1;
                    else
                        I = Index;

                    if (I >= 0 && I < TaskList.Children.Count)
                    {
                        TaskList.Children.RemoveAt(I);

                        if (TaskList.Children.Count == 0)
                        {
                            var FadeOut = new DoubleAnimation
                            {
                                From = 1,
                                To = 0,
                                Duration = FadeInDuration
                            };

                            FadeOut.Completed += (s, e) => TasksContainer!.Visibility = Visibility.Collapsed;
                            TasksContainer!.BeginAnimation(UIElement.OpacityProperty, FadeOut);
                        }
                    }
                }
            });
        }
    }
}