using System;
using System.IO;

namespace RansauSysteme.Utils
{
    /// <summary>
    /// Provides utility methods for input/output operations such as file and directory manipulation.
    /// </summary>
    public static class IOUtils
    {
        /// <summary>
        /// Creates a new file at the specified path.
        /// </summary>
        /// <param name="fileName">The path where the file should be created.</param>
        /// <remarks>This method will create any necessary directories in the path.</remarks>
        public static void CreateFile(string fileName)
        {
            // Create the directory if it doesn't exist
            string? directory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create the file and immediately close it
            using (File.Create(fileName))
            {
                // File is automatically closed when the using block exits
            }
        }

        /// <summary>
        /// Recursively copies a directory and all its contents to a new location.
        /// </summary>
        /// <param name="sourceDir">The source directory to copy from.</param>
        /// <param name="destinationDir">The destination directory to copy to.</param>
        public static void CopyDirectory(string sourceDir, string destinationDir)
        {
            // Validate parameters
            if (!Directory.Exists(sourceDir))
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

            // Create the destination directory if it doesn't exist
            Directory.CreateDirectory(destinationDir);

            // Get all files from the source directory
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destinationDir, fileName);
                File.Copy(file, destFile, true); // true to overwrite if file exists
            }

            // Get all subdirectories
            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(dir);
                string destDir = Path.Combine(destinationDir, dirName);

                // Recursively copy subdirectories
                CopyDirectory(dir, destDir);
            }
        }

        /// <summary>
        /// Converts a DateTime value to a filename-friendly string format.
        /// </summary>
        /// <param name="dateTime">The DateTime to convert, or null.</param>
        /// <returns>A string in the format "yyyy-MM-dd_HH-mm-ss" if dateTime is not null; otherwise, an empty string.</returns>
        public static string DatetimeFilename(DateTime? dateTime) => dateTime?.ToString("yyyy-MM-dd_HH-mm-ss") ?? string.Empty;
    }
}