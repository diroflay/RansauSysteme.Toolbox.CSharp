using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Web.WebView2.Core;

namespace RansauSysteme.Avalonia.WebView.Windows
{
    internal class WindowsWebViewHost : NativeWebViewHost
    {
        private CoreWebView2Environment? _webView2Environment;
        private CoreWebView2Controller? _webView2Controller;
        private CoreWebView2? _coreWebView2;
        private IntPtr _hwnd;
        private bool _isInitialized;

        public WindowsWebViewHost()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new InvalidOperationException("Cannot create Windows WebView control on another platform than Windows");

            bool isWebview2Available = false;
            try
            {
                string? version = CoreWebView2Environment.GetAvailableBrowserVersionString();
                isWebview2Available = !string.IsNullOrEmpty(version);
            }
            catch
            {
                isWebview2Available = false;
            }

            if (!isWebview2Available)
            {
                throw new InvalidOperationException("Microsoft WebView2 have to be installed to run Windows WebView control");
            }
        }

        public override event EventHandler<NavigationCompletedEventArgs>? NavigationCompleted;

        public override event EventHandler<NavigationStartingEventArgs>? NavigationStarting;

        public override event EventHandler<MessageReceivedEventArgs>? WebMessageReceived;

        public override event EventHandler<string>? SourceChanged;

        public override void OnSourceChanged(string newSource)
        {
            if (_isInitialized && _coreWebView2 != null)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _coreWebView2.Navigate(newSource);
                });
            }
        }

        public override void OnIsDevToolsEnabledChanged(bool enabled)
        {
            if (_isInitialized && _coreWebView2 != null)
            {
                _coreWebView2.Settings.AreDevToolsEnabled = enabled;
            }
        }

        public override bool SupportsCookies => _isInitialized && _coreWebView2 != null;

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("WebView2 is only supported on Windows.");

            _hwnd = CreateWindowEx(
                0,
                "Static",
                "",
                WS_CHILD | WS_VISIBLE,
                0, 0,
                (int)Bounds.Width, (int)Bounds.Height,
                parent.Handle,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

            Task.Run(async () => await InitializeWebView2());

            return new PlatformHandle(_hwnd, "HWND");
        }

        private async Task InitializeWebView2()
        {
            try
            {
                // Create environment options
                var environmentOptions = new CoreWebView2EnvironmentOptions();

                // Create the WebView2 environment
                _webView2Environment = await CoreWebView2Environment.CreateAsync(null, null, environmentOptions);

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    // Create the CoreWebView2Controller
                    _webView2Controller = await _webView2Environment.CreateCoreWebView2ControllerAsync(_hwnd);

                    // Get the CoreWebView2 from the controller
                    _coreWebView2 = _webView2Controller.CoreWebView2;

                    // Set the bounds to match the Avalonia control
                    _webView2Controller.Bounds = new System.Drawing.Rectangle(
                        0, 0, (int)Bounds.Width, (int)Bounds.Height);

                    // Wire up events
                    _coreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
                    _coreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                    _coreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                    _coreWebView2.SourceChanged += CoreWebView2_SourceChanged;

                    // Configure WebView2 settings
                    _coreWebView2.Settings.AreDevToolsEnabled = IsDevToolsEnabled;
                    _coreWebView2.Settings.IsScriptEnabled = true;
                    _coreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;

                    // Set the initial source
                    _coreWebView2.Navigate(Source);

                    // Make the controller visible
                    _webView2Controller.IsVisible = true;

                    _isInitialized = true;
                });
            }
            catch (Exception ex)
            {
                // Handle initialization errors
                Console.WriteLine($"WebView2 initialization error: {ex.Message}");
            }
        }

        private void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            NavigationStarting?.Invoke(this, new NavigationStartingEventArgs(e.Uri));
        }

        private void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            NavigationCompleted?.Invoke(this, new NavigationCompletedEventArgs(e.IsSuccess, e.WebErrorStatus));
        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            WebMessageReceived?.Invoke(this, new MessageReceivedEventArgs(e.TryGetWebMessageAsString()));
        }

        private void CoreWebView2_SourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
        {
            if (_coreWebView2 != null)
            {
                string newSource = _coreWebView2.Source;
                if (newSource != Source)
                {
                    Source = newSource;
                    SourceChanged?.Invoke(this, newSource);
                }
            }
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            if (_hwnd != IntPtr.Zero)
            {
                SetWindowPos(
                    _hwnd,
                    IntPtr.Zero,
                    0, 0,
                    (int)e.NewSize.Width, (int)e.NewSize.Height,
                    SWP_NOMOVE | SWP_NOZORDER);

                if (_webView2Controller != null)
                {
                    _webView2Controller.Bounds = new System.Drawing.Rectangle(
                        0, 0, (int)e.NewSize.Width, (int)e.NewSize.Height);
                }
            }
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            if (_webView2Controller != null)
            {
                _webView2Controller.Close();
                _webView2Controller = null;
            }

            _coreWebView2 = null;

            if (_hwnd != IntPtr.Zero)
            {
                DestroyWindow(_hwnd);
                _hwnd = IntPtr.Zero;
            }

            _isInitialized = false;
            base.DestroyNativeControlCore(control);
        }

        #region Native Methods

        // Constants for window styles
        private const int WS_CHILD = 0x40000000;

        private const int WS_VISIBLE = 0x10000000;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateWindowEx(
            int dwExStyle,
            string lpClassName,
            string lpWindowName,
            int dwStyle,
            int x, int y,
            int nWidth, int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X, int Y,
            int cx, int cy,
            uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        #endregion Native Methods

        #region Public Methods

        /// <summary>
        /// Executes JavaScript in the WebView asynchronously.
        /// </summary>
        /// <param name="script">JavaScript code to execute</param>
        /// <returns>A task that completes with the result of the script execution</returns>
        public override async Task<string> ExecuteScriptAsync(string script)
        {
            if (!_isInitialized || _coreWebView2 == null)
                throw new InvalidOperationException("WebView is not initialized.");

            return await _coreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Registers a JavaScript callback function that can be called from JavaScript.
        /// </summary>
        /// <param name="functionName">Name of the JavaScript function</param>
        /// <param name="callback">The callback to invoke when the JavaScript function is called</param>
        public override void AddWebMessageReceived(string functionName, Action<string> callback)
        {
            if (!_isInitialized || _coreWebView2 == null)
                throw new InvalidOperationException("WebView is not initialized.");

            _coreWebView2.WebMessageReceived += (sender, args) =>
            {
                // Parse the message to determine if it's for our function
                string message = args.TryGetWebMessageAsString();

                try
                {
                    // Simple message format: "functionName:message"
                    if (message.StartsWith($"{functionName}:"))
                    {
                        string payload = message.Substring(functionName.Length + 1);
                        callback(payload);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing web message: {ex.Message}");
                }
            };
        }

        /// <summary>
        /// Injects JavaScript code into the WebView that exposes a function in the global scope.
        /// </summary>
        /// <param name="functionName">Name of the JavaScript function</param>
        /// <returns>A task that completes when the JavaScript is injected</returns>
        public override async Task ExposeJavaScriptFunction(string functionName)
        {
            if (!_isInitialized || _coreWebView2 == null)
                throw new InvalidOperationException("WebView is not initialized.");

            // Create a JavaScript function that will post a message to the WebView
            string script = $@"
                window.{functionName} = function(message) {{
                    window.chrome.webview.postMessage('{functionName}:' + message);
                }};
            ";

            await _coreWebView2.ExecuteScriptAsync(script);
        }

        #endregion Public Methods

        #region Cookie Management

        public override async Task AddCookieAsync(WebViewCookie cookie)
        {
            if (!_isInitialized || _coreWebView2 == null)
                throw new InvalidOperationException("WebView is not initialized.");

            var cookieManager = _coreWebView2.CookieManager;
            var coreWebView2Cookie = cookieManager.CreateCookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path);
            await Task.Run(() => cookieManager.AddOrUpdateCookie(coreWebView2Cookie));
        }

        public override async Task<List<WebViewCookie>> GetCookiesAsync(string url)
        {
            if (!_isInitialized || _coreWebView2 == null)
                throw new InvalidOperationException("WebView is not initialized.");

            var cookieManager = _coreWebView2.CookieManager;
            var cookies = await cookieManager.GetCookiesAsync(url);

            var result = new List<WebViewCookie>();
            foreach (var cookie in cookies)
            {
                result.Add(new WebViewCookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path));
            }

            return result;
        }

        public override async Task RemoveCookieAsync(WebViewCookie cookie)
        {
            if (!_isInitialized || _coreWebView2 == null)
                throw new InvalidOperationException("WebView is not initialized.");

            var cookieManager = _coreWebView2.CookieManager;
            var cookies = await cookieManager.GetCookiesAsync(cookie.Domain);

            foreach (var existingCookie in cookies)
            {
                if (existingCookie.Name == cookie.Name && existingCookie.Path == cookie.Path)
                {
                    cookieManager.DeleteCookie(existingCookie);
                    break;
                }
            }
        }

        public override Task ClearCookiesAsync()
        {
            if (!_isInitialized || _coreWebView2 == null)
                throw new InvalidOperationException("WebView is not initialized.");

            _coreWebView2.CookieManager.DeleteAllCookies();
            return Task.CompletedTask;
        }

        #endregion Cookie Management

        #region Navigation

        public override bool CanGoBack => _isInitialized && _coreWebView2?.CanGoBack == true;

        public override bool CanGoForward => _isInitialized && _coreWebView2?.CanGoForward == true;

        public override bool GoBack()
        {
            if (!_isInitialized || _coreWebView2 == null)
                return false;

            if (!_coreWebView2.CanGoBack)
                return false;

            _coreWebView2.GoBack();
            return true;
        }

        public override bool GoForward()
        {
            if (!_isInitialized || _coreWebView2 == null)
                return false;

            if (!_coreWebView2.CanGoForward)
                return false;

            _coreWebView2.GoForward();
            return true;
        }

        public override bool Stop()
        {
            if (!_isInitialized || _coreWebView2 == null)
                return false;

            _coreWebView2.Stop();
            return true;
        }

        public override bool Reload()
        {
            if (!_isInitialized || _coreWebView2 == null)
                return false;

            _coreWebView2.Reload();
            return true;
        }

        #endregion Navigation
    }
}