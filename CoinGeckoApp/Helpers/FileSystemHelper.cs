using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Storage;

namespace CoinGeckoApp.Helpers
{
    public class FileSystemHelper
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
        public FileSystemHelper() { }

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
            // TODO: Do Unit Test or Manual Test.
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
            // TODO: Do Unit Test or Manual Test.
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

        public static async Task CopyOverwriteFileAsync(string sourceFilePath, string targetDir, string? newFileName = null)
        {
            // Check if source file exists
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException($"Source file '{sourceFilePath}' not found.");
            }

            // Create the target directory if it doesn't exist
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // Determine the new file name
            string fileName = newFileName ?? Path.GetFileName(sourceFilePath);

            // Combine the target directory with the new file name to get the full target path
            string targetFilePath = Path.Combine(targetDir, fileName);

            // Overwrite to Target File
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                using (FileStream targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await sourceStream.CopyToAsync(targetStream);
                }
            }
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
                    //await Task.Run(() => Directory.Delete(dir, true));
                    Directory.Delete(dir, true);
                    Trace.WriteLine($"Directory '{dir}' and its contents removed successfully.");
                }
            }
        }

        public async Task<List<string>> GetAllFilePathsAsync(string directoryPath)
        {
            List<string> filePaths = new List<string>();

            // Get all files in the current directory
            string[] files = Directory.GetFiles(directoryPath);
            filePaths.AddRange(files);

            // Get all subdirectories
            string[] subDirectories = Directory.GetDirectories(directoryPath);

            // For each subdirectory, recursively call the method
            foreach (string subDirectory in subDirectories)
            {
                List<string> subDirectoryFiles = await GetAllFilePathsAsync(subDirectory);
                filePaths.AddRange(subDirectoryFiles);
            }

            return filePaths;
        }

        public async Task<List<string>> GetAllSubdirectoriesAsync(string directoryPath)
        {
            var subdirectories = new List<string>();

            // Get all subdirectories in the current directory
            string[] directories = Directory.GetDirectories(directoryPath);
            subdirectories.AddRange(directories);

            // For each subdirectory, recursively call the method
            foreach (string subDirectory in directories)
            {
                List<string> subSubdirectories = await GetAllSubdirectoriesAsync(subDirectory);
                subdirectories.AddRange(subSubdirectories);
            }

            return subdirectories;
        }
    }
}
