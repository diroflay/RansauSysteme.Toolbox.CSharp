using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Reactive;

namespace RansauSysteme.Avalonia.WebView;

public class WebView : TemplatedControl, IWebViewCookieManager
{
    private readonly NativeWebViewHost _nativeHost;

    public WebView()
    {
        _nativeHost = NativeWebViewHostFactory.Create();

        this.GetObservable(SourceProperty).Subscribe(new AnonymousObserver<string>(_nativeHost.OnSourceChanged));
        this.GetObservable(IsDevToolsEnabledProperty).Subscribe(new AnonymousObserver<bool>(_nativeHost.OnIsDevToolsEnabledChanged));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Create and add the native host
        _nativeHost.Source = Source;
        _nativeHost.IsDevToolsEnabled = IsDevToolsEnabled;

        // Wire up events
        _nativeHost.NavigationStarting += (s, args) =>
        {
            if (NavigationStartingCommand?.CanExecute(args) == true)
                NavigationStartingCommand.Execute(args);
        };

        _nativeHost.NavigationCompleted += (s, args) =>
        {
            if (NavigationCompletedCommand?.CanExecute(args) == true)
                NavigationCompletedCommand.Execute(args);
        };

        _nativeHost.WebMessageReceived += (s, args) =>
        {
            if (WebMessageReceivedCommand?.CanExecute(args) == true)
                WebMessageReceivedCommand.Execute(args);
        };

        _nativeHost.SourceChanged += (s, newSource) =>
        {
            if (Source != newSource)
            {
                SetCurrentValue(SourceProperty, newSource);
            }
        };

        // Add to visual tree
        var contentPresenter = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
        if (contentPresenter != null)
        {
            contentPresenter.Content = _nativeHost;
        }
    }

    #region Dependency Properties

    public static readonly StyledProperty<string> SourceProperty =
            AvaloniaProperty.Register<WebView, string>(nameof(Source), "about:blank", defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsDevToolsEnabledProperty =
            AvaloniaProperty.Register<WebView, bool>(nameof(IsDevToolsEnabled), false);

    public static readonly StyledProperty<ICommand> NavigationStartingCommandProperty =
            AvaloniaProperty.Register<WebView, ICommand>(nameof(NavigationStartingCommand));

    public static readonly StyledProperty<ICommand> NavigationCompletedCommandProperty =
            AvaloniaProperty.Register<WebView, ICommand>(nameof(NavigationCompletedCommand));

    public static readonly StyledProperty<ICommand> WebMessageReceivedCommandProperty =
            AvaloniaProperty.Register<WebView, ICommand>(nameof(WebMessageReceivedCommand));

    public string Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public bool IsDevToolsEnabled
    {
        get => GetValue(IsDevToolsEnabledProperty);
        set => SetValue(IsDevToolsEnabledProperty, value);
    }

    public ICommand NavigationStartingCommand
    {
        get => GetValue(NavigationStartingCommandProperty);
        set => SetValue(NavigationStartingCommandProperty, value);
    }

    public ICommand NavigationCompletedCommand
    {
        get => GetValue(NavigationCompletedCommandProperty);
        set => SetValue(NavigationCompletedCommandProperty, value);
    }

    public ICommand WebMessageReceivedCommand
    {
        get => GetValue(WebMessageReceivedCommandProperty);
        set => SetValue(WebMessageReceivedCommandProperty, value);
    }

    #endregion Dependency Properties

    #region Public Methods

    /// <summary>
    /// Executes JavaScript in the WebView asynchronously.
    /// </summary>
    /// <param name="script">JavaScript code to execute</param>
    /// <returns>A task that completes with the result of the script execution</returns>
    public async Task<string> ExecuteScriptAsync(string script)
    {
        if (_nativeHost == null)
            throw new InvalidOperationException("WebView is not initialized.");

        return await _nativeHost.ExecuteScriptAsync(script);
    }

    /// <summary>
    /// Registers a JavaScript callback function that can be called from JavaScript.
    /// </summary>
    /// <param name="functionName">Name of the JavaScript function</param>
    /// <param name="callback">The callback to invoke when the JavaScript function is called</param>
    public void AddWebMessageReceived(string functionName, Action<string> callback)
    {
        if (_nativeHost == null)
            throw new InvalidOperationException("WebView is not initialized.");

        _nativeHost.AddWebMessageReceived(functionName, callback);
    }

    /// <summary>
    /// Injects JavaScript code into the WebView that exposes a function in the global scope.
    /// </summary>
    /// <param name="functionName">Name of the JavaScript function</param>
    /// <returns>A task that completes when the JavaScript is injected</returns>
    public async Task ExposeJavaScriptFunction(string functionName)
    {
        if (_nativeHost == null)
            throw new InvalidOperationException("WebView is not initialized.");

        await _nativeHost.ExposeJavaScriptFunction(functionName);
    }

    /// <summary>
    /// Injects a CSS stylesheet into the current page.
    /// </summary>
    /// <param name="cssContent">The CSS content to inject</param>
    /// <returns>A task that completes when the CSS is injected</returns>
    public async Task InjectCssAsync(string cssContent)
    {
        if (_nativeHost == null)
            throw new InvalidOperationException("WebView is not initialized.");

        // Escape single quotes in CSS content
        cssContent = cssContent.Replace("'", "\\'");

        // Create a script to inject the CSS
        string script = @"
                (function() {
                    const style = document.createElement('style');
                    style.textContent = '" + cssContent + @"';
                    document.head.appendChild(style);
                })();
            ";

        await _nativeHost.ExecuteScriptAsync(script);
    }

    #endregion Public Methods

    #region Navigation Methods

    /// <summary>
    /// Sets the WebView to navigate to the specified URL.
    /// </summary>
    /// <param name="url">The URL to navigate to</param>
    public void NavigateTo(string url)
    {
        if (_nativeHost == null)
            throw new InvalidOperationException("WebView is not initialized.");

        Source = url;
    }

    /// <summary>
    /// Gets a value indicating whether navigation to a previous page is possible.
    /// </summary>
    public bool CanGoBack => _nativeHost?.CanGoBack ?? false;

    /// <summary>
    /// Gets a value indicating whether navigation to a next page is possible.
    /// </summary>
    public bool CanGoForward => _nativeHost?.CanGoForward ?? false;

    /// <summary>
    /// Navigates back to the previous page if possible.
    /// </summary>
    /// <returns>True if successfully navigated back, false otherwise.</returns>
    public bool GoBack()
    {
        if (_nativeHost == null)
            throw new InvalidOperationException("WebView is not initialized.");

        return _nativeHost.GoBack();
    }

    /// <summary>
    /// Navigates forward to the next page if possible.
    /// </summary>
    /// <returns>True if successfully navigated forward, false otherwise.</returns>
    public bool GoForward()
    {
        if (_nativeHost == null)
            throw new InvalidOperationException("WebView is not initialized.");

        return _nativeHost.GoForward();
    }

    /// <summary>
    /// Stops the current navigation.
    /// </summary>
    /// <returns>True if successfully stopped, false otherwise.</returns>
    public bool Stop()
    {
        if (_nativeHost == null)
            throw new InvalidOperationException("WebView is not initialized.");

        return _nativeHost.Stop();
    }

    /// <summary>
    /// Reloads the current page.
    /// </summary>
    /// <returns>True if successfully started reloading, false otherwise.</returns>
    public bool Reload()
    {
        if (_nativeHost == null)
            throw new InvalidOperationException("WebView is not initialized.");

        return _nativeHost.Reload();
    }

    #endregion Navigation Methods

    #region IWebViewCookieManager implementation

    /// <summary>
    /// Gets a value indicating whether the WebView implementation supports cookie management.
    /// </summary>
    public bool SupportsCookies => _nativeHost?.SupportsCookies ?? false;

    /// <summary>
    /// Adds a cookie to the WebView.
    /// </summary>
    /// <param name="cookie">The cookie to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the WebView is not initialized.</exception>
    public Task AddCookieAsync(WebViewCookie cookie)
    {
        if (_nativeHost == null)
            throw new InvalidOperationException("WebView is not initialized.");

        return _nativeHost.AddCookieAsync(cookie);
    }

    /// <summary>
    /// Gets all cookies for the specified URL.
    /// </summary>
    /// <param name="url">The URL to get cookies for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the cookies.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the WebView is not initialized.</exception>
    public Task<List<WebViewCookie>> GetCookiesAsync(string url)
    {
        if (_nativeHost == null)
            throw new InvalidOperationException("WebView is not initialized.");

        return _nativeHost.GetCookiesAsync(url);
    }

    /// <summary>
    /// Removes a cookie from the WebView.
    /// </summary>
    /// <param name="cookie">The cookie to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the WebView is not initialized.</exception>
    public Task RemoveCookieAsync(WebViewCookie cookie)
    {
        if (_nativeHost == null)
            throw new InvalidOperationException("WebView is not initialized.");

        return _nativeHost.RemoveCookieAsync(cookie);
    }

    /// <summary>
    /// Clears all cookies from the WebView.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the WebView is not initialized.</exception>
    public Task ClearCookiesAsync()
    {
        if (_nativeHost == null)
            throw new InvalidOperationException("WebView is not initialized.");

        return _nativeHost.ClearCookiesAsync();
    }

    #endregion IWebViewCookieManager implementation
}