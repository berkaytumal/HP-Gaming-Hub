using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Storage;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace HP_Gaming_Hub
{
    public sealed partial class WelcomeWindow : Window
    {
        private int currentPage = 1;
        private const int totalPages = 4;
        private bool isOmenMonDownloaded = false;
        private readonly HttpClient httpClient;
        private readonly AppSettings appSettings = AppSettings.Instance;

        public WelcomeWindow()
        {
            this.InitializeComponent();
            
            // Configure window properties
            this.Title = "HP Gaming Hub Setup";
            
            // Set window size (1:1 aspect ratio, square)
            var appWindow = this.AppWindow;
            appWindow.Resize(new Windows.Graphics.SizeInt32(600, 600));
            
            // Make window non-resizable and configure presenter
            if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
            }
            
            // Center window on screen
            var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(appWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
            var centerX = (displayArea.WorkArea.Width - 600) / 2;
            var centerY = (displayArea.WorkArea.Height - 600) / 2;
            appWindow.Move(new Windows.Graphics.PointInt32(centerX, centerY));
            
            // Set Mica material backdrop
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            
            // Make title bar button background transparent
            appWindow.TitleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
            
            // Extend Mica into client area
            appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            appWindow.TitleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
            appWindow.TitleBar.ButtonInactiveBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
            
            // Set drag rectangles to make entire window draggable
            appWindow.TitleBar.SetDragRectangles([new(0, 0, 600, 600)]);
            
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "HP-Gaming-Hub/1.0");
            
            // Check if OmenMon is already downloaded
            CheckOmenMonStatus();
        }

        private async void UpdateNavigationState()
        {
            // Update Previous button
            PreviousButton.IsEnabled = currentPage > 1;

            // Update Next button
            if (currentPage == 3)
            {
                NextButton.IsEnabled = isOmenMonDownloaded;
                NextButton.Content = "Next";
                NextButton.Visibility = Visibility.Visible;
            }
            else if (currentPage == 4)
            {
                NextButton.Content = "Launch";
                NextButton.IsEnabled = true;
                NextButton.Visibility = Visibility.Visible;
            }
            else
            {
                NextButton.Content = "Next";
                NextButton.IsEnabled = true;
                NextButton.Visibility = Visibility.Visible;
            }

            // Update page visibility with animations
            await AnimatePageTransition(); // Default forward direction

            // Update page indicators
            UpdatePageIndicators();
        }

        private void UpdatePageIndicators()
        {
            var dots = new[] { Dot1, Dot2, Dot3, Dot4 };
            var activeBrush = (SolidColorBrush)Application.Current.Resources["AccentFillColorDefaultBrush"];
            var inactiveBrush = (SolidColorBrush)Application.Current.Resources["ControlFillColorDisabledBrush"];

            for (int i = 0; i < dots.Length; i++)
            {
                dots[i].Fill = (i + 1) == currentPage ? activeBrush : inactiveBrush;
            }
        }

        private async Task AnimatePageTransition(bool isForward = true)
        {
            var pages = new[] { Page1, Page2, Page3, Page4 };
            
            // First, fade out all visible pages
            var fadeOutTasks = new List<Task>();
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i].Visibility == Visibility.Visible && (i + 1) != currentPage)
                {
                    var fadeOutStoryboard = CreateFadeOutStoryboard(isForward);
                    Storyboard.SetTarget(fadeOutStoryboard, pages[i]);
                    fadeOutTasks.Add(AnimateAsync(fadeOutStoryboard));
                }
            }
            
            if (fadeOutTasks.Count > 0)
            {
                await Task.WhenAll(fadeOutTasks);
            }
            
            // Hide all pages
            foreach (var page in pages)
            {
                page.Visibility = Visibility.Collapsed;
            }
            
            // Show and animate in the current page
            var currentPageElement = pages[currentPage - 1];
            currentPageElement.Visibility = Visibility.Visible;
            
            var fadeInStoryboard = CreateFadeInStoryboard(isForward);
            Storyboard.SetTarget(fadeInStoryboard, currentPageElement);
            await AnimateAsync(fadeInStoryboard);
        }
        
        private Storyboard CreateFadeInStoryboard(bool isForward = true)
        {
            var storyboard = new Storyboard();
            
            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
            
            var translateAnimation = new DoubleAnimation
            {
                From = isForward ? 30 : -30,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(translateAnimation, "(UIElement.RenderTransform).(TranslateTransform.X)");
            
            storyboard.Children.Add(opacityAnimation);
            storyboard.Children.Add(translateAnimation);
            
            return storyboard;
        }
        
        private Storyboard CreateFadeOutStoryboard(bool isForward = true)
        {
            var storyboard = new Storyboard();
            
            var opacityAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
            
            var translateAnimation = new DoubleAnimation
            {
                From = 0,
                To = isForward ? -30 : 30,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTargetProperty(translateAnimation, "(UIElement.RenderTransform).(TranslateTransform.X)");
            
            storyboard.Children.Add(opacityAnimation);
            storyboard.Children.Add(translateAnimation);
            
            return storyboard;
        }
        
        private Task AnimateAsync(Storyboard storyboard)
        {
            var tcs = new TaskCompletionSource<bool>();
            
            void OnCompleted(object sender, object e)
            {
                storyboard.Completed -= OnCompleted;
                tcs.SetResult(true);
            }
            
            storyboard.Completed += OnCompleted;
            storyboard.Begin();
            
            return tcs.Task;
        }

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                
                // Recheck OmenMon status when navigating to page 3
                if (currentPage == 3)
                {
                    CheckOmenMonStatus();
                }
                else
                {
                    UpdateNavigationState();
                }
                
                await AnimatePageTransition(false); // Reverse animation for Previous
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage == 4)
            {
                // Launch the main application
                LaunchButton_Click(sender, e);
                return;
            }
            
            if (currentPage < totalPages)
            {
                currentPage++;
                
                // Recheck OmenMon status when navigating to page 3
                if (currentPage == 3)
                {
                    CheckOmenMonStatus();
                }
                else
                {
                    UpdateNavigationState();
                }
                
                await AnimatePageTransition(true); // Forward animation for Next
            }
        }

        private void CheckOmenMonStatus()
        {
            // Always check current state from AppSettings which verifies file existence
            isOmenMonDownloaded = appSettings.IsOmenMonDownloaded;
            
            if (isOmenMonDownloaded)
            {
                DownloadButton.Visibility = Visibility.Collapsed;
                RedownloadButton.Visibility = Visibility.Visible;
                RedownloadButton.IsEnabled = true;
                DownloadStatus.Text = "OmenMon is already downloaded and ready to use.";
                DownloadStatus.Foreground = (SolidColorBrush)Application.Current.Resources["AccentFillColorDefaultBrush"];
            }
            else
            {
                DownloadButton.Visibility = Visibility.Visible;
                DownloadButton.IsEnabled = true;
                RedownloadButton.Visibility = Visibility.Collapsed;
                DownloadStatus.Text = "";
                DownloadStatus.Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorSecondaryBrush"];
            }
            
            // Update navigation state after checking status
            UpdateNavigationState();
        }
        
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadButton.IsEnabled = false;
                RedownloadButton.IsEnabled = false;
                DownloadProgress.Visibility = Visibility.Visible;
                DownloadStatus.Text = "Fetching latest release information...";

                // Get latest release info from GitHub API
                var releaseInfo = await GetLatestReleaseInfo();
                if (releaseInfo == null)
                {
                    throw new Exception("Could not fetch release information");
                }

                DownloadStatus.Text = $"Downloading {releaseInfo.Name}...";
                
                // Download the zip file
                var zipData = await DownloadFile(releaseInfo.DownloadUrl);
                
                DownloadStatus.Text = "Extracting files...";
                
                // Extract and save OmenMon files
                await ExtractOmenMonFiles(zipData);
                
                DownloadStatus.Text = "Download completed successfully!";
                DownloadProgress.Visibility = Visibility.Collapsed;
                
                isOmenMonDownloaded = true;
                
                // Update UI without page animation
                DownloadButton.Visibility = Visibility.Collapsed;
                RedownloadButton.Visibility = Visibility.Collapsed;
                
                // Show installed status
                DownloadStatus.Text = "✓ OmenMon installed successfully!";
                DownloadStatus.Foreground = (SolidColorBrush)Application.Current.Resources["AccentFillColorDefaultBrush"];
                
                // Enable Next button without triggering page animation
                NextButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                DownloadStatus.Text = $"Error: {ex.Message}";
                DownloadButton.IsEnabled = true;
                RedownloadButton.IsEnabled = true;
                DownloadProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<ReleaseInfo> GetLatestReleaseInfo()
        {
            try
            {
                var response = await httpClient.GetStringAsync("https://api.github.com/repos/OmenMon/OmenMon/releases/latest");
                var jsonDoc = JsonDocument.Parse(response);
                var root = jsonDoc.RootElement;

                var assets = root.GetProperty("assets").EnumerateArray();
                var zipAsset = assets.FirstOrDefault(asset => 
                    asset.GetProperty("name").GetString().EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

                if (zipAsset.ValueKind == JsonValueKind.Undefined)
                {
                    throw new Exception("No zip file found in the latest release");
                }

                return new ReleaseInfo
                {
                    Name = zipAsset.GetProperty("name").GetString(),
                    DownloadUrl = zipAsset.GetProperty("browser_download_url").GetString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get release info: {ex.Message}");
            }
        }

        private async Task<byte[]> DownloadFile(string url)
        {
            try
            {
                return await httpClient.GetByteArrayAsync(url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download file: {ex.Message}");
            }
        }

        private async Task ExtractOmenMonFiles(byte[] zipData)
        {
            try
            {
                // Get the local app data folder
                var localFolder = ApplicationData.Current.LocalFolder;
                var dependenciesFolder = await localFolder.CreateFolderAsync("Dependencies", CreationCollisionOption.OpenIfExists);
                var omenMonFolder = await dependenciesFolder.CreateFolderAsync("OmenMon", CreationCollisionOption.ReplaceExisting);

                // Extract zip file
                using (var zipStream = new MemoryStream(zipData))
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    bool foundExe = false;
                    bool foundXml = false;

                    foreach (var entry in archive.Entries)
                    {
                        if (entry.Name.Equals("OmenMon.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            var exeFile = await omenMonFolder.CreateFileAsync("OmenMon.exe", CreationCollisionOption.ReplaceExisting);
                            using (var entryStream = entry.Open())
                            using (var fileStream = await exeFile.OpenStreamForWriteAsync())
                            {
                                await entryStream.CopyToAsync(fileStream);
                            }
                            foundExe = true;
                        }
                        else if (entry.Name.Equals("OmenMon.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            var xmlFile = await omenMonFolder.CreateFileAsync("OmenMon.xml", CreationCollisionOption.ReplaceExisting);
                            using (var entryStream = entry.Open())
                            using (var fileStream = await xmlFile.OpenStreamForWriteAsync())
                            {
                                await entryStream.CopyToAsync(fileStream);
                            }
                            foundXml = true;
                        }

                        if (foundExe && foundXml)
                            break;
                    }

                    if (!foundExe || !foundXml)
                    {
                        throw new Exception("Required files (OmenMon.exe or OmenMon.xml) not found in the archive");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to extract files: {ex.Message}");
            }
        }

        private void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Drag rectangles are set during window initialization
        }

        private void Grid_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Not needed for system drag move
        }

        private void Grid_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Not needed for system drag move
        }

        private void Grid_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            // Prevent default double-click maximization behavior
            e.Handled = true;
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            // Mark welcome as completed and launch main window
            appSettings.IsWelcomeCompleted = true;
            
            var mainWindow = new MainWindow();
            mainWindow.Activate();
            this.Close();
        }

        private class ReleaseInfo
        {
            public string Name { get; set; }
            public string DownloadUrl { get; set; }
        }
    }
}