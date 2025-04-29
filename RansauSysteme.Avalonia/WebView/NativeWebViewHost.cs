using Avalonia.Controls;

namespace RansauSysteme.Avalonia.WebView
{
    internal abstract class NativeWebViewHost : NativeControlHost, IWebViewCookieManager
    {
        #region Properties

        public string Source
        {
            get => _source;
            set
            {
                _source = value;
                OnSourceChanged(value);
            }
        }

        protected string _source = "about:blank";

        public bool IsDevToolsEnabled
        {
            get => _isDevToolsEnabled;
            set
            {
                _isDevToolsEnabled = value;
                OnIsDevToolsEnabledChanged(value);
            }
        }

        protected bool _isDevToolsEnabled;

        #endregion Properties

        #region Events

        public abstract event EventHandler<NavigationCompletedEventArgs>? NavigationCompleted;

        public abstract event EventHandler<NavigationStartingEventArgs>? NavigationStarting;

        public abstract event EventHandler<MessageReceivedEventArgs>? WebMessageReceived;

        public abstract event EventHandler<string>? SourceChanged;

        #endregion Events

        #region Public Methods

        public abstract void OnSourceChanged(string newSource);

        public abstract void OnIsDevToolsEnabledChanged(bool enabled);

        /// <summary>
        /// Executes JavaScript in the WebView asynchronously.
        /// </summary>
        /// <param name="script">JavaScript code to execute</param>
        /// <returns>A task that completes with the result of the script execution</returns>
        public abstract Task<string> ExecuteScriptAsync(string script);

        /// <summary>
        /// Registers a JavaScript callback function that can be called from JavaScript.
        /// </summary>
        /// <param name="functionName">Name of the JavaScript function</param>
        /// <param name="callback">The callback to invoke when the JavaScript function is called</param>
        public abstract void AddWebMessageReceived(string functionName, Action<string> callback);

        /// <summary>
        /// Injects JavaScript code into the WebView that exposes a function in the global scope.
        /// </summary>
        /// <param name="functionName">Name of the JavaScript function</param>
        /// <returns>A task that completes when the JavaScript is injected</returns>
        public abstract Task ExposeJavaScriptFunction(string functionName);

        #endregion Public Methods

        #region IWebViewCookieManager implementation

        /// <summary>
        /// Gets a value indicating whether the WebView implementation supports cookie management.
        /// </summary>
        public abstract bool SupportsCookies { get; }

        /// <summary>
        /// Adds a cookie to the WebView.
        /// </summary>
        /// <param name="cookie">The cookie to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task AddCookieAsync(WebViewCookie cookie);

        /// <summary>
        /// Gets all cookies for the specified URL.
        /// </summary>
        /// <param name="url">The URL to get cookies for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the cookies.</returns>
        public abstract Task<List<WebViewCookie>> GetCookiesAsync(string url);

        /// <summary>
        /// Removes a cookie from the WebView.
        /// </summary>
        /// <param name="cookie">The cookie to remove.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task RemoveCookieAsync(WebViewCookie cookie);

        /// <summary>
        /// Clears all cookies from the WebView.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task ClearCookiesAsync();

        #endregion IWebViewCookieManager implementation

        #region Navigation

        // Add to NativeWebViewHost.cs in the #region Public Methods section
        /// <summary>
        /// Navigates back to the previous page if possible.
        /// </summary>
        /// <returns>True if successfully navigated back, false otherwise.</returns>
        public abstract bool GoBack();

        /// <summary>
        /// Navigates forward to the next page if possible.
        /// </summary>
        /// <returns>True if successfully navigated forward, false otherwise.</returns>
        public abstract bool GoForward();

        /// <summary>
        /// Stops the current navigation.
        /// </summary>
        /// <returns>True if successfully stopped, false otherwise.</returns>
        public abstract bool Stop();

        /// <summary>
        /// Reloads the current page.
        /// </summary>
        /// <returns>True if successfully started reloading, false otherwise.</returns>
        public abstract bool Reload();

        /// <summary>
        /// Gets a value indicating whether navigation to a previous page is possible.
        /// </summary>
        public abstract bool CanGoBack { get; }

        /// <summary>
        /// Gets a value indicating whether navigation to a next page is possible.
        /// </summary>
        public abstract bool CanGoForward { get; }

        #endregion Navigation
    }
}