using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace WpfGourmetDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string filePathCognex;
        string filePathImages;
        private FileSystemWatcher fileSystemWatcher;

        public MainWindow()
        {
            InitializeComponent();
            filePathCognex = WpfGateGourmetDemo.Properties.Settings.Default.filePathCognex;
            filePathImages = WpfGateGourmetDemo.Properties.Settings.Default.filePathImages;

            if (!Directory.Exists(filePathCognex))
            {
                // Log a message to txbstatus.txt
                Dispatcher.Invoke(() =>
                {
                    txbStatus.Text = $"filePathCognex in App.config does not exist: {filePathCognex}";
                    txbStatus.Background = Brushes.Pink; // Set the background color to red
                });
                return;
            }

            if (!Directory.Exists(filePathImages))
            {
                // Log a message to txbStatus
                Dispatcher.Invoke(() =>
                {
                    txbStatus.Text = $"filePathImages in App.config does not exist: {filePathImages}";
                    txbStatus.Background = Brushes.Pink; // Set the background color to red
                });
                return;
            }

            SetupFileSystemWatcher();
        }

        private void SetupFileSystemWatcher()
        {
            // Get the directory path from settings
            string directoryPath = filePathCognex;

            // Initialize the FileSystemWatcher
            fileSystemWatcher = new FileSystemWatcher
            {
                Path = directoryPath,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = "*.*" // Watch all files; adjust if you need specific types
            };

            // Add event handlers
            fileSystemWatcher.Created += OnFileCreated;
            fileSystemWatcher.Changed += OnFileChanged;
            fileSystemWatcher.Deleted += OnFileDeleted;
            fileSystemWatcher.Renamed += OnFileRenamed;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void HandleFileEvent(string fullPath)
        {
            Dispatcher.Invoke(() =>
            {
                // Update the status TextBox
                txbStatus.Text = $"File event occurred: {fullPath}";

                // Get the file name from the full path
                string fileName = System.IO.Path.GetFileName(fullPath);

                // Ensure the file name is not empty
                if (!string.IsNullOrEmpty(fileName))
                {
                    // Get the first letter of the file name
                    char firstLetter = fileName[0];

                    // Find the first image file starting with the firstLetter
                    string[] imageFiles = Directory.GetFiles(filePathImages, $"{firstLetter}*.jpg");

                    // Display the first matching image file
                    if (imageFiles.Length > 0)
                    {
                        string imagePath = imageFiles[0];
                        imgControl.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                    }
                    else
                    {
                        // Handle case where no matching image is found
                        imgControl.Source = null; // or set to a default image
                        txbStatus.Text = "";
                    }
                }
                else
                {
                    // Handle case where file name might be empty
                    imgControl.Source = null; // or set to a default image
                    txbStatus.Text = "";
                }
            });
        }

        private void OnFileCreated(object source, FileSystemEventArgs e)
        {
            // Handle file creation
            HandleFileEvent(e.FullPath);
        }
        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            // Handle file changes
            HandleFileEvent(e.FullPath);
        }

        private void OnFileDeleted(object source, FileSystemEventArgs e)
        {
            // Handle file deletions
            Dispatcher.Invoke(() =>
            {
                imgControl.Source = null;
                txbStatus.Text = "";
            });
        }

        private void OnFileRenamed(object source, RenamedEventArgs e)
        {
            // Handle file renames
            HandleFileEvent(e.FullPath);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Dispose of the FileSystemWatcher
            if (fileSystemWatcher != null)
            {
                fileSystemWatcher.EnableRaisingEvents = false;
                fileSystemWatcher.Dispose();
            }
        }
    }
}
