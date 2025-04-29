using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RansauSysteme.Avalonia.WebView
{
    /// <summary>
    /// Defines methods for managing cookies in a WebView control.
    /// </summary>
    public interface IWebViewCookieManager
    {
        /// <summary>
        /// Gets a value indicating whether the WebView implementation supports cookie management.
        /// </summary>
        bool SupportsCookies { get; }

        /// <summary>
        /// Adds a cookie to the WebView.
        /// </summary>
        /// <param name="cookie">The cookie to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddCookieAsync(WebViewCookie cookie);

        /// <summary>
        /// Gets all cookies for the specified URL.
        /// </summary>
        /// <param name="url">The URL to get cookies for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the cookies.</returns>
        Task<List<WebViewCookie>> GetCookiesAsync(string url);

        /// <summary>
        /// Removes a cookie from the WebView.
        /// </summary>
        /// <param name="cookie">The cookie to remove.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task RemoveCookieAsync(WebViewCookie cookie);

        /// <summary>
        /// Clears all cookies from the WebView.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ClearCookiesAsync();
    }
}