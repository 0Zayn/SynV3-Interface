using Microsoft.Web.WebView2.Wpf;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SynV3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<CollapsibleItem> CollapsibleItems { get; } = new();

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            Loader(); // Comment out when u tryna work n shit its annoying to wait through

            CTask.Initialize(TaskList, TasksContainer);
            Messages.Initialize(Message, M_Container);
            Toast.Initialize(Notifications);

            SetupTV();

            MD.Display(Changes, "## Update a8dce8\r\n### Fixes\r\n- Updated.\r\n");

            Settings.Visibility = Visibility.Collapsed;
            Themes.Visibility = Visibility.Collapsed;
            Plugins.Visibility = Visibility.Collapsed;

            Settings.Margin = new Thickness(0, 50, 0, 0);
            Themes.Margin = new Thickness(0, 50, 0, 0);
            Plugins.Margin = new Thickness(0, 50, 0, 0);
        }


        private async void AddTab_Click(object Sender, RoutedEventArgs EventArgs) {
            try {
                var NewTabItem = new TabItem {
                    Header = $"Untitled {TabContainer.Items.Count + 1}",
                    Style = FindResource("Tab") as Style                
                };

                TabContainer.Items.Add(NewTabItem);
                TabContainer.SelectedItem = NewTabItem;
            } catch (Exception Ex) {
                MessageBox.Show($"Error adding tab: {Ex.Message}\n\nStack trace: {Ex.StackTrace}");
            }
        }

        private void CloseTab_Click(object Sender, RoutedEventArgs EventArgs) {
            if (Sender is Button ButtonSender && FindParent<TabItem>(ButtonSender) is TabItem TabItem && TabContainer.Items.Count > 1) {
                if (TabItem.Content is Grid GridContent && GridContent.Children.Count > 0 && GridContent.Children[0] is WebView2 WebView)
                    Editor.ActiveEditors.RemoveAll(EditorItem => EditorItem.WebView == WebView);

                TabContainer.Items.Remove(TabItem);
            }
        }

        private T FindParent<T>(DependencyObject Child) where T : DependencyObject {
            while (Child != null && Child is not T)
                Child = VisualTreeHelper.GetParent(Child);

            return Child as T;
        }

        private async void Execute_Click(object Sender, RoutedEventArgs EventArgs) {
            if (TabContainer.SelectedItem is TabItem { Content: Grid GridContent } &&
                GridContent.Children.Count > 0 &&
                GridContent.Children[0] is WebView2 WebView) {

                var SelectedEditor = Editor.ActiveEditors.FirstOrDefault(EditorItem => EditorItem.WebView == WebView);
                if (SelectedEditor != null) {
                    var Script = await SelectedEditor.GetValue();
                    if (!string.IsNullOrEmpty(Script)) {
                        Script = Script.Replace("\r", "");
                        MessageBox.Show($"Executing: {Script.Substring(0, Math.Min(50, Script.Length))}...");
                    }
                }
            }
        }

        private async void Duplicate_Click(object Sender, RoutedEventArgs EventArgs) {
            if (GetCurrentEditor() is Editor CurrentEditor) {
                var Content = await CurrentEditor.GetValue();
                await AddNewTabWithContent(Content, "Copy of " + GetCurrentTabHeader());
            }
        }

        private async void Format_Click(object Sender, RoutedEventArgs EventArgs) {
            if (GetCurrentEditor() is Editor SelectedEditor && SelectedEditor.WebView != null)
                await SelectedEditor.WebView.ExecuteScriptAsync("editor.getAction('editor.action.formatDocument').run();");
        }

        private void Rename_Click(object Sender, RoutedEventArgs EventArgs) {
            if (TabContainer.SelectedItem is TabItem TabItem)
                TabItem.Header = Microsoft.VisualBasic.Interaction.InputBox("Enter new name:", "Rename Tab", TabItem.Header?.ToString() ?? "");
        }

        private void CloseAllButThis_Click(object Sender, RoutedEventArgs EventArgs) {
            var CurrentTab = TabContainer.SelectedItem as TabItem;
            var TabsToRemove = TabContainer.Items.Cast<TabItem>().Where(TabItem => TabItem != CurrentTab).ToList();

            foreach (var Tab in TabsToRemove) {
                if (Tab.Content is Grid GridContent && GridContent.Children.Count > 0 && GridContent.Children[0] is WebView2 WebView)
                    Editor.ActiveEditors.RemoveAll(EditorItem => EditorItem.WebView == WebView);

                TabContainer.Items.Remove(Tab);
            }
        }

        private Editor GetCurrentEditor() {
            if (TabContainer.SelectedItem is TabItem TabItem && TabItem.Content is Grid GridContent && GridContent.Children.Count > 0 && GridContent.Children[0] is WebView2 WebView)
                return Editor.ActiveEditors.FirstOrDefault(EditorItem => EditorItem.WebView == WebView);

            return null;
        }

        private string GetCurrentTabHeader() => TabContainer.SelectedItem is TabItem TabItem ? TabItem.Header?.ToString() ?? "Untitled" : "Untitled";

        private async Task AddNewTabWithContent(string Content, string Header) {
            var NewEditor = new Editor();
            await NewEditor.Initialize();

            if (NewEditor.WebView == null)
                return;

            await Task.Delay(1000);
            await NewEditor.SetValue(Content);

            var NewTabItem = new TabItem {
                Header = Header,
                Style = FindResource("Tab") as Style,
                Content = new Grid { Children = { NewEditor.WebView } }
            };

            TabContainer.Items.Add(NewTabItem);
            TabContainer.SelectedItem = NewTabItem;
        }

        private void SetupTV()
        {
            CollapsibleItems.Add(new CollapsibleItem
            {
                Header = "Local Filesystem",
                IconData = (Geometry)FindResource("Drive"),
                SubItems = { new SubItem { Name = "Script 1", Script = "print('hello')" }, new SubItem { Name = "Script 2", Script = "print('hello')" } }
            });

            CollapsibleItems.Add(new CollapsibleItem
            {
                Header = "Bookmarks",
                IconData = (Geometry)FindResource("Bookmark")
            });

            CollapsibleItems.Add(new CollapsibleItem
            {
                Header = "GitHub Gists",
                IconData = (Geometry)FindResource("Github")
            });

            DataContext = this;
        }

        private async void Loader()
        {
            KeyPage.Margin = new Thickness(0, 0, 0, 0);

            var Transform = new RotateTransform();
            Spinner.RenderTransform = Transform;

            var Spin = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(0.8),
                RepeatBehavior = RepeatBehavior.Forever
            };
            Transform.BeginAnimation(RotateTransform.AngleProperty, Spin);

            await Task.Delay(6000);

            Transform.BeginAnimation(RotateTransform.AngleProperty, null);

            var Fade = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromSeconds(0.2) };
            Fade.Completed += (s, e) => Load.Visibility = Visibility.Hidden;
            Load.BeginAnimation(OpacityProperty, Fade);

            Panel.Visibility = Visibility.Visible;
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

        private void NavigationHandler(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton { IsChecked: true } Navigator)
            {
                if (Homepage == null) return;

                Homepage.Visibility = Visibility.Collapsed;
                Settings.Visibility = Visibility.Collapsed;
                Themes.Visibility = Visibility.Collapsed;
                Plugins.Visibility = Visibility.Collapsed;

                switch (Navigator.Name)
                {
                    case "EditorN":
                        Homepage.Visibility = Visibility.Visible;
                        break;
                    case "SettingsN":
                        Settings.Visibility = Visibility.Visible;
                        break;
                    case "ThemesN":
                        Themes.Visibility = Visibility.Visible;
                        break;
                    case "PluginsN":
                        Plugins.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            var Fade = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromSeconds(0.2) };
            Fade.Completed += (s, e) => KeyPage.Visibility = Visibility.Hidden;
            KeyPage.BeginAnimation(OpacityProperty, Fade);
        }

        private void ExitCL_Click(object sender, RoutedEventArgs e) => Changelog.Visibility = Visibility.Hidden;

        private void ShowCL_Click(object sender, RoutedEventArgs e) => Changelog.Visibility = Visibility.Visible;

        private void ListBox_PMD(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ListBox ListBox || VisualTree.FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource) is not ListBoxItem ClickedItem || ClickedItem.DataContext is not CollapsibleItem Item) return;
            if (e.OriginalSource is not FrameworkElement ClickedElement || ClickedElement.DataContext is SubItem) return;

            Item.IsExpanded = !Item.IsExpanded;

            if (ListBox.ItemContainerGenerator.ContainerFromItem(Item) is ListBoxItem BoxItem && BoxItem.Template.FindName("SubItems", BoxItem) is ItemsControl SubItems)
                SubItems.Visibility = Item.IsExpanded ? Visibility.Visible : Visibility.Collapsed;

            ListBox.SelectedItem = Item;
        }

        private void AI_Click(object sender, RoutedEventArgs e)
        {
            AiPanel.Visibility = AiPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            double Width = AiPanel.Visibility == Visibility.Visible ? 200 : 0;
            AiSpace.Width = new GridLength(Width);

            E_Control.Margin = AiPanel.Visibility == Visibility.Visible ? new Thickness(-120, 8, 8, 8) : new Thickness(8);
        }
    }
}