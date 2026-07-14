using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.Windows;

namespace SynV3 {
    public class Editor {
        public static List<Editor> ActiveEditors = new();
        public WebView2? WebView { get; private set; }
        public bool IsInitialized { get; private set; }
        private string LatestContent = "";

        public Editor() => ActiveEditors.Add(this);

        public async Task Initialize() {
            if (IsInitialized)
                return;

            WebView = new WebView2 {
                Margin = new Thickness(5, 15, 5, 5),
                DefaultBackgroundColor = System.Drawing.Color.Transparent
            };

            await WebView.EnsureCoreWebView2Async();

            WebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            WebView.CoreWebView2.Settings.AreHostObjectsAllowed = false;
            WebView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

            WebView.CoreWebView2.Navigate("https://public-monaco.vercel.app/");
            IsInitialized = true;
        }

        public async Task<string> GetValue() {
            if (IsInitialized && WebView?.CoreWebView2 != null)
                await WebView.CoreWebView2.ExecuteScriptAsync("window.chrome.webview.postMessage(editor.getValue())");

            return LatestContent;
        }

        public async Task SetValue(string Code) {
            if (IsInitialized && WebView != null)
                await WebView.ExecuteScriptAsync($"editor.setValue(`{EscapeString(Code)}`);");
        }

        private void CoreWebView2_WebMessageReceived(object Sender, CoreWebView2WebMessageReceivedEventArgs EventArgs) {
            LatestContent = EventArgs.TryGetWebMessageAsString();
        }

        private void CoreWebView2_NavigationCompleted(object Sender, CoreWebView2NavigationCompletedEventArgs EventArgs) { }

        private static string EscapeString(string Input) => Input.Replace("\\", "\\\\").Replace("`", "``").Replace("'", "\\'");
    }
}