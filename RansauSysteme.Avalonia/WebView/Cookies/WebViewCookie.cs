namespace RansauSysteme.Avalonia.WebView
{
    /// <summary>
    /// Represents a cookie used by the WebView control.
    /// </summary>
    public class WebViewCookie
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebViewCookie"/> class.
        /// </summary>
        /// <param name="name">The name of the cookie.</param>
        /// <param name="value">The value of the cookie.</param>
        /// <param name="domain">The domain of the cookie.</param>
        /// <param name="path">The path of the cookie.</param>
        public WebViewCookie(string name, string value, string domain, string path)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrEmpty(domain))
                throw new ArgumentNullException(nameof(domain));

            Name = name;
            Value = value ?? string.Empty;
            Domain = domain;
            Path = path ?? "/";
        }

        /// <summary>
        /// Gets the name of the cookie.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the cookie.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the domain of the cookie.
        /// </summary>
        public string Domain { get; }

        /// <summary>
        /// Gets the path of the cookie.
        /// </summary>
        public string Path { get; }
    }
}