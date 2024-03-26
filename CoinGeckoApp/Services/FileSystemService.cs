using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Storage;

namespace CoinGeckoApp.Services
{
    public class FileSystemService
    {
        private const string applicationName = "CoinGeckoMAUIApp";  // Root directory name for Cache and AppData

        /// <summary>
        /// Gets the application's directory to store cache data.
        /// </summary>
        public string CacheDir
        {
            get => Path.Combine(FileSystem.Current.CacheDirectory, applicationName);
        }

        /// <summary>
        /// Gets the app's top-level directory for any files that aren't user data files.
        /// </summary>
        public string AppDataDir
        {
            get => Path.Combine(FileSystem.Current.AppDataDirectory, applicationName);
        }

        /* Constructors */
        public FileSystemService() { }

        /// <summary>
        /// <para> Read contents from text file asynchronously.</para>
        /// <para>This method uses OpenAppPackageFileAsync.</para>
        /// <para>Files that were added to the project with the Build Action of MauiAsset can be opened with this method. 
        /// .NET MAUI projects will process any file in the Resources\Raw folder as a MauiAsset.</para>
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task<string> ReadTextFile(string filePath)
        {
            using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(filePath);
            using StreamReader reader = new StreamReader(fileStream);
            return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// <para>Copies a bundled file to the AppData directory.</para>
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async Task CopyFileToAppDataDirectory(string filename, string? subDirectoryPath = null)
        {
            // Open the source file
            using Stream inputStream = await FileSystem.Current.OpenAppPackageFileAsync(filename);

            // Create an output filename
            string targetFile;
            if (subDirectoryPath == null)
            {
                targetFile = Path.Combine(AppDataDir, filename);
            }
            else
            {
                targetFile = Path.Combine(AppDataDir, subDirectoryPath, filename);
            }

            // Copy the file to the AppDataDirectory
            using FileStream outputStream = File.Create(targetFile);
            await inputStream.CopyToAsync(outputStream);
        }

        /// <summary>
        /// <para>Copies a bundled file to the Cache directory.</para>
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async Task CopyFileToCacheDirDirectory(string filename, string? subDirectoryPath = null)
        {
            // Open the source file
            using Stream inputStream = await FileSystem.Current.OpenAppPackageFileAsync(filename);

            // Create an output filename
            string targetFile;
            if (subDirectoryPath == null)
            {
                targetFile = Path.Combine(CacheDir, filename);
            }
            else
            {
                targetFile = Path.Combine(CacheDir, subDirectoryPath, filename);
            }

            // Copy the file to the AppDataDirectory
            using FileStream outputStream = File.Create(targetFile);
            await inputStream.CopyToAsync(outputStream);
        }


        public async Task CreateDirectoryInCacheDirAsync(string subDirectoryPath)
        {
            await Task.Delay(100);

            string newDirectory = Path.Combine(CacheDir, subDirectoryPath);  // Append the Cache Directory and the Subdirectory Path
            if (!Directory.Exists(newDirectory))
            {
                Directory.CreateDirectory(newDirectory);
            }
        }
        public async Task CreateDirectoryInAppDataDirAsync(string subDirectoryPath)
        {
            await Task.Delay(100);

            string newDirectory = Path.Combine(AppDataDir, subDirectoryPath);  // Append the Cache Directory and the Subdirectory Path
            if (!Directory.Exists(newDirectory))
            {
                Directory.CreateDirectory(newDirectory);
            }
        }

        public async Task CreateFileInCacheDirAsync(string subDirectory, string filename)
        {
            string fileParentDir = Path.Combine(CacheDir, subDirectory);
            string newFile = Path.Combine(fileParentDir, filename);  // Full Path to the File

            if (!File.Exists(newFile))
            {
                // Try to create the parent directory for the new file
                await CreateDirectoryInCacheDirAsync(subDirectory);

                using FileStream fs = File.Create(newFile);
            }
        }
        public async Task CreateFileInAppDataDirAsync(string subDirectory, string filename)
        {
            string fileParentDir = Path.Combine(AppDataDir, subDirectory);
            string newFile = Path.Combine(fileParentDir, filename);  // Full Path to the File

            if (!File.Exists(newFile))
            {
                // Try to create the parent directory for the new file
                await CreateDirectoryInAppDataDirAsync(subDirectory);

                using FileStream fs = File.Create(newFile);
            }
        }


        public async Task RemoveFileFromCacheDirAsync(string filename, string? subDirectory = null)
        {

            string filePath;
            if (subDirectory == null)
            {
                filePath = Path.Combine(CacheDir, filename);
            }
            else
            {
                filePath = Path.Combine(CacheDir, subDirectory, filename);
            }

            await RemoveFileAsync(filePath);
        }
        public async Task RemoveFileFromAppDataDirAsync(string filename, string? subDirectory = null)
        {
            string filePath;
            if (subDirectory == null)
            {
                filePath = Path.Combine(AppDataDir, filename);
            }
            else
            {
                filePath = Path.Combine(AppDataDir, subDirectory, filename);
            }

            await RemoveFileAsync(filePath);
        }

        public async Task RemoveDirectoryFromCacheDir(string subDirectory)
        {
            string dirPath = Path.Combine(CacheDir, subDirectory);
            await RemoveDirectoryAsync(dirPath);
        }
        public async Task RemoveDirectoryFromAppDataDir(string subDirectory)
        {
            string dirPath = Path.Combine(AppDataDir, subDirectory);
            await RemoveDirectoryAsync(dirPath);
        }

        public async Task RemoveDirectoryContentsFromCacheDir(string subDirectory)
        {
            string dirPath = Path.Combine(CacheDir, subDirectory);
            await RemoveDirectoryContentsAsync(dirPath);
        }
        public async Task RemoveDirectoryContentsFromAppDataDir(string subDirectory)
        {
            string dirPath = Path.Combine(AppDataDir, subDirectory);
            await RemoveDirectoryContentsAsync(dirPath);
        }

        private async Task RemoveFileAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }
        private async Task RemoveDirectoryAsync(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                await Task.Run(() => Directory.Delete(directoryPath, true));
            }
        }
        private async Task RemoveDirectoryContentsAsync(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                string[] files = Directory.GetFiles(directoryPath);
                string[] directories = Directory.GetDirectories(directoryPath);
                
                // Delete Files Asynchronously
                foreach (string file in files)
                {
                    await Task.Run(() => File.Delete(file));
                    Trace.WriteLine($"File '{file}' removed successfully.");
                }

                // Delete Subdirectories Asynchronously
                foreach (string dir in directories)
                {
                    await Task.Run(() => Directory.Delete(dir, true));
                    Trace.WriteLine($"Directory '{dir}' and its contents removed successfully.");
                }
            }
        }

        public static string[] SearchForFile(string directoryPath, string fileName)
        {
            try
            {
                // Search for the file in the current directory
                string[] files = Directory.GetFiles(directoryPath, fileName);

                if (files.Length > 0)
                {
                    // File found in the current directory
                    return files;
                }

                // Search for the file in subdirectories (Recursive)
                string[] subdirectories = Directory.GetDirectories(directoryPath);
                foreach (string subdirectory in subdirectories)
                {
                    string[] filePaths = SearchForFile(subdirectory, fileName);
                    if (filePaths.Length > 0)
                    {
                        // File found in one of the subdirectories
                        return filePaths;
                    }
                }

                // File not found in the directory or its subdirectories
                return new string[] {};
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore unauthorized access to directories
                return new string[] { };
            }
            catch (DirectoryNotFoundException)
            {
                // Ignore directories not found
                return new string[] { };
            }
        }

        public async Task<string[]> SearchForFileInCacheDir(string searchPattern)
        {
            return await SearchFilesAsync(CacheDir, searchPattern);
        }
        public async Task<string[]> SearchForFileInAppDataDir(string searchPattern)
        {
            return await SearchFilesAsync(AppDataDir, searchPattern);
        }

        public static async Task<string[]> SearchFilesAsync(string directoryPath, string searchPattern)
        {
            return await Task.Run(() =>
            {
                return Directory.GetFiles(directoryPath, searchPattern);
            });
        }
    }
}
