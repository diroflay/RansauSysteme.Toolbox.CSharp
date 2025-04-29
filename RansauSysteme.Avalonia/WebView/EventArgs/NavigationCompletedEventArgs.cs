using Microsoft.Web.WebView2.Core;

namespace RansauSysteme.Avalonia.WebView
{
    /// <summary>
    /// Event arguments for WebView navigation completed events.
    /// </summary>
    public class NavigationCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether the navigation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the WebView2 error status if navigation failed.
        /// </summary>
        public CoreWebView2WebErrorStatus WebErrorStatus { get; }

        public NavigationCompletedEventArgs(bool isSuccess, CoreWebView2WebErrorStatus webErrorStatus)
        {
            IsSuccess = isSuccess;
            WebErrorStatus = webErrorStatus;
        }
    }
}