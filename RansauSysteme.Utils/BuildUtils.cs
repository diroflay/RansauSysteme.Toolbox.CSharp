using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RansauSysteme
{
    /// <summary>
    /// Provides build configuration information about the current assembly.
    /// </summary>
    public static class BuildUtils
    {
        /// <summary>
        /// Gets a value indicating whether the assembly was built in debug mode.
        /// </summary>
        /// <returns>True if the assembly was compiled in debug mode; otherwise, false.</returns>
        public static bool IsDebug
        {
            get
            {
                bool isDebug = false;
#if DEBUG
                isDebug = true;
#endif
                return isDebug;
            }
        }

        /// <summary>
        /// Gets the version of the current assembly.
        /// </summary>
        public static Version? Version => Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        /// Gets the version of the current assembly as a string.
        /// </summary>
        public static string VersionString => Version?.ToString() ?? string.Empty;

        /// <summary>
        /// Gets a value indicating whether the application is running on Windows.
        /// </summary>
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Gets a value indicating whether the application is running on Linux.
        /// </summary>
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        /// <summary>
        /// Gets a value indicating whether the application is running on macOS.
        /// </summary>
        public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        /// <summary>
        /// Gets the operating system platform the application is running on.
        /// </summary>
        public static string OsPlatform
        {
            get
            {
                if (IsWindows) return "Windows";
                if (IsLinux) return "Linux";
                if (IsMacOS) return "macOS";
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets a value indicating whether the application is running in a 64-bit process.
        /// </summary>
        public static bool Is64BitProcess => Environment.Is64BitProcess;

        /// <summary>
        /// Gets the build date of the assembly, or null if it cannot be determined.
        /// </summary>
        public static DateTime? BuildDate
        {
            get
            {
                try
                {
                    string filePath = Assembly.GetExecutingAssembly().Location;
                    return File.GetLastWriteTime(filePath);
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a formatted summary of the build configuration.
        /// </summary>
        /// <returns>A string containing the build configuration summary.</returns>
        public static string GetSummary()
        {
            return $"Version: {VersionString}\n" +
                   $"OS: {OsPlatform}\n" +
                   $"Process: {(Is64BitProcess ? "64-bit" : "32-bit")}\n" +
                   $"Build Date: {BuildDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown"}";
        }

        /// <summary>
        /// Gets the .NET runtime version the application is running on.
        /// </summary>
        public static string RuntimeVersion => RuntimeInformation.FrameworkDescription;

        /// <summary>
        /// Gets the name of the current machine.
        /// </summary>
        public static string MachineName => Environment.MachineName;
    }
}