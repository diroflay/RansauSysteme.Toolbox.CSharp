namespace RansauSysteme.Avalonia.WebView
{
    /// <summary>
    /// Event arguments for WebView navigation starting events.
    /// </summary>
    public class NavigationStartingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the URI of the navigation.
        /// </summary>
        public string Uri { get; }

        public NavigationStartingEventArgs(string uri)
        {
            Uri = uri;
        }
    }
}