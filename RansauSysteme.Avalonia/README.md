# RansauSysteme.Avalonia.WebView

A modern WebView control for Avalonia UI applications targeting Windows platforms. This library integrates Microsoft's WebView2 component with Avalonia, providing a modern web rendering experience in your desktop applications.

## Features

- Modern web rendering using Microsoft Edge WebView2
- MVVM-friendly with two-way binding and command support
- JavaScript execution and interop capabilities
- Navigation events and commands for full control
- CSS injection support
- DevTools access for debugging
- Proper XAML control template integration

## Requirements

- Windows OS (The control is Windows-specific for now)
- Microsoft Edge WebView2 Runtime must be installed on target machines
- .NET 9.0 or higher
- Avalonia 11.0 or higher

## Installation

### 1. Install via NuGet:

```bash
dotnet add package RansauSysteme.Avalonia.WebView
```

### 2. Make sure the WebView2 Runtime is installed:

The Microsoft Edge WebView2 Runtime must be installed on the target machine. You can either:

- Let your users install it manually: [WebView2 Runtime Download](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)
- Include it in your installer
- Use the Evergreen Bootstrapper to install it at runtime

## Usage

### Basic Usage in XAML

Add the namespace to your XAML file:

```xml
xmlns:webview="clr-namespace:RansauSysteme.Avalonia.WebView;assembly=RansauSysteme.Avalonia.WebView"
```
or 
```xml
xmlns:webview="https://ransausysteme.com/webview"
```

Then, add the WebView control:

```xml
<webview:WebView Source="{Binding CurrentUrl, Mode=TwoWay}" 
                 IsDevToolsEnabled="{Binding IsDevMode}"
                 NavigationStartingCommand="{Binding NavigationStartingCommand}"
                 NavigationCompletedCommand="{Binding NavigationCompletedCommand}"
                 WebMessageReceivedCommand="{Binding WebMessageReceivedCommand}" />
```

### Basic Usage in Code-Behind

```csharp
var webView = new WebView
{
    Source = "https://www.example.com",
    IsDevToolsEnabled = true
};

// Handle events via commands
webView.NavigationStartingCommand = ReactiveCommand.Create<NavigationStartingEventArgs>(args => 
{
    Console.WriteLine($"Navigation starting to: {args.Uri}");
});
```

### JavaScript Execution

```csharp
// Execute JavaScript code
string result = await webView.ExecuteScriptAsync("document.title");
```

### JavaScript to .NET Communication

```csharp
// Expose a function to JavaScript
await webView.ExposeJavaScriptFunction("sendToApp");

// Add a handler for the function
webView.AddWebMessageReceived("sendToApp", message =>
{
    Console.WriteLine($"Message from JS: {message}");
});
```

In JavaScript, call the exposed function:

```javascript
window.sendToApp("Hello from JavaScript!");
```

### CSS Injection

```csharp
// Inject CSS into the current page
await webView.InjectCssAsync("body { background-color: #f0f0f0; }");
```

### Navigation

```csharp
// Navigate to a URL
webView.NavigateTo("https://www.google.com");

// Reload the current page
webView.Reload();
```

## MVVM Example

Here's a simple example of using the WebView in an MVVM pattern:

```csharp
// ViewModel
public class BrowserViewModel : ReactiveObject
{
    private string _currentUrl = "https://www.example.com";
    public string CurrentUrl 
    {
        get => _currentUrl;
        set => this.RaiseAndSetIfChanged(ref _currentUrl, value);
    }
    
    public ReactiveCommand<NavigationStartingEventArgs, Unit> NavigationStartingCommand { get; }
    public ReactiveCommand<NavigationCompletedEventArgs, Unit> NavigationCompletedCommand { get; }
    
    private bool _isLoading;
    public bool IsLoading 
    {
        get => _isLoading;
        private set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }
    
    public BrowserViewModel()
    {
        NavigationStartingCommand = ReactiveCommand.Create<NavigationStartingEventArgs>(OnNavigationStarting);
        NavigationCompletedCommand = ReactiveCommand.Create<NavigationCompletedEventArgs>(OnNavigationCompleted);
    }
    
    private void OnNavigationStarting(NavigationStartingEventArgs e)
    {
        IsLoading = true;
    }
    
    private void OnNavigationCompleted(NavigationCompletedEventArgs e)
    {
        IsLoading = false;
    }
}
```

```xml
<!-- View -->
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:webview="clr-namespace:RansauSysteme.Avalonia.WebView;assembly=RansauSysteme.Avalonia.WebView">
    
    <Grid RowDefinitions="Auto,*">
        <Grid Grid.Row="0" ColumnDefinitions="*,Auto">
            <TextBox Text="{Binding CurrentUrl, Mode=TwoWay}" />
            <Button Content="Go" Command="{Binding GoCommand}" />
        </Grid>
        
        <webview:WebView Grid.Row="1"
                         Source="{Binding CurrentUrl, Mode=TwoWay}"
                         NavigationStartingCommand="{Binding NavigationStartingCommand}"
                         NavigationCompletedCommand="{Binding NavigationCompletedCommand}" />
    </Grid>
</Window>
```

## Known Limitations

- Windows-only: This control only works on Windows due to its dependency on WebView2
- The WebView2 Runtime must be installed on the target machine

## License

MIT License

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Roadmap

- [ ] macOS support via WebKit
- [ ] Linux support via WebKitGTK
- [ ] File download handling
- [ ] Cookie and web storage management
- [ ] Context menu customization
- [ ] Print support