namespace RansauSysteme.Avalonia.WebView
{
    /// <summary>
    /// Event arguments for WebView message received events.
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the web message as a string.
        /// </summary>
        public string Message { get; }

        public MessageReceivedEventArgs(string message)
        {
            Message = message;
        }
    }
}