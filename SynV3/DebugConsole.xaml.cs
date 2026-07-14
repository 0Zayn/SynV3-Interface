using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SynV3
{
    /// <summary>
    /// Interaction logic for DebugConsole.xaml
    /// </summary>
    public partial class DebugConsole : Window
    {
        public DebugConsole()
        {
            InitializeComponent();

            for (int i = 0; i < 20; i++)
                Log(0, "Hello, world!");
        }

        private void Titlebar_MLBD(object sender, MouseButtonEventArgs e) => DragMove();

        private void WindowControl(object sender, RoutedEventArgs e)
        {
            var Actions = new Dictionary<string, Action>
            {
                { "Minimize", () => WindowState = WindowState.Minimized },
                { "Maximize", () => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized },
                { "Close", () => Close() }
            };

            var Name = ((Button)sender).Name;

            foreach (var Action in Actions)
            {
                if (Name.Contains(Action.Key))
                {
                    Action.Value.Invoke();
                    break;
                }
            }
        }

        public void Log(int type, string message)
        {
            Color color;
            switch (type)
            {
                case 0:
                    color = (Color)ColorConverter.ConvertFromString("#5a5a5a"); // Default
                    break;
                case 1:
                    color = (Color)ColorConverter.ConvertFromString("#579146"); // Success
                    break;
                case 2:
                    color = (Color)ColorConverter.ConvertFromString("#5b80c4"); // Info
                    break;
                case 3:
                    color = (Color)ColorConverter.ConvertFromString("#c14e43"); // Error
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Type must be 1, 2, or 3.");
            }

            StackPanel LogEntry = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 1, 5, 1)
            };

            Border Indicator = new Border
            {
                Width = 3,
                Height = 25,
                Background = new SolidColorBrush(color),
                Margin = new Thickness(0, 0, 4, 0)
            };

            Label Timestamp = new Label
            {
                Content = DateTime.Now.ToString("hh:mm:ss tt"),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5a5a5a")),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)Application.Current.Resources["Poppins Regular"]
            };

            Label Message = new Label
            {
                Content = message,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5a5a5a")),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)Application.Current.Resources["Poppins Regular"]
            };

            LogEntry.Children.Add(Indicator);
            LogEntry.Children.Add(Timestamp);
            LogEntry.Children.Add(Message);

            Console.Children.Add(LogEntry);
        }
    }
}
