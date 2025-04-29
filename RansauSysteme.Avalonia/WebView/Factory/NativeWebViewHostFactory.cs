using System.Runtime.InteropServices;
using RansauSysteme.Avalonia.WebView.Windows;

namespace RansauSysteme.Avalonia.WebView
{
    /// <summary>
    /// Factory class for creating WebView instances.
    /// </summary>
    internal static class NativeWebViewHostFactory
    {
        /// <summary>
        /// Creates a new WebView instance if the current platform is supported.
        /// </summary>
        /// <returns>A new WebView instance or null if the platform is not supported.</returns>
        public static NativeWebViewHost Create()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsWebViewHost();
            }

            throw new InvalidOperationException("Current OS is not supported yet");
        }

        /// <summary>
        /// Creates a new WebView instance if the current platform is supported,
        /// with the specified initial source URL.
        /// </summary>
        /// <param name="source">The initial source URL.</param>
        /// <returns>A new WebView instance or null if the platform is not supported.</returns>
        public static NativeWebViewHost Create(string source)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsWebViewHost { Source = source };
            }

            throw new InvalidOperationException("Current OS is not supported yet");
        }

        /// <summary>
        /// Checks if WebView2 is supported on the current platform.
        /// </summary>
        /// <returns>True if WebView2 is supported, otherwise false.</returns>
        public static bool IsSupported()
        {
            var supportedPlatforms = new List<OSPlatform> { OSPlatform.Windows };

            return supportedPlatforms.Any(RuntimeInformation.IsOSPlatform);
        }
    }
}