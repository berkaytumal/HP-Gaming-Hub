using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;

namespace HP_Gaming_Hub
{
    public sealed partial class MainWindow : Window
    {
        // Navigation and Settings Event Handlers
        
        private void MainNavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            // Find currently visible page
            FrameworkElement currentPage = null;
            if (FanPage.Visibility == Visibility.Visible) currentPage = FanPage;
            else if (GraphicsPage.Visibility == Visibility.Visible) currentPage = GraphicsPage;
            else if (KeyboardPage.Visibility == Visibility.Visible) currentPage = KeyboardPage;
            else if (MonitorPage.Visibility == Visibility.Visible) currentPage = MonitorPage;
            else if (ConsolePage.Visibility == Visibility.Visible) currentPage = ConsolePage;
            else if (SettingsPage.Visibility == Visibility.Visible) currentPage = SettingsPage;

            // Determine target page and index
            FrameworkElement targetPage = null;
            int currentSelectedIndex = -1;
            if (args.SelectedItem == FanNavItem) { targetPage = FanPage; currentSelectedIndex = 0; }
            else if (args.SelectedItem == GraphicsNavItem) { targetPage = GraphicsPage; currentSelectedIndex = 1; }
            else if (args.SelectedItem == KeyboardNavItem) { targetPage = KeyboardPage; currentSelectedIndex = 2; }
            else if (args.SelectedItem == MonitorNavItem) { targetPage = MonitorPage; currentSelectedIndex = 3; }
            else if (args.SelectedItem == ConsoleNavItem) { targetPage = ConsolePage; currentSelectedIndex = 4; }
            else if (args.SelectedItem == SettingsNavItem) { targetPage = SettingsPage; currentSelectedIndex = 5; }

            // If same page is selected, do nothing
            if (currentPage == targetPage) return;
            
            // Load preferences when navigating to settings page
            if (targetPage == SettingsPage)
            {
                LoadSettingsPagePreferences();
            }
            
            // Determine animation direction based on index comparison
            bool isMovingDown = currentSelectedIndex < _previousSelectedIndex;
            double slideOutDistance = isMovingDown ? 30.0 : -30.0;
            double slideInStartPosition = isMovingDown ? -30.0 : 30.0;
            
            // Update previous selected index
            _previousSelectedIndex = currentSelectedIndex;

            if (currentPage != null && targetPage != null)
             {
                 // Create slide out and fade out storyboard for current page
                 var slideOutStoryboard = new Storyboard();
                 
                 var fadeOut = new DoubleAnimation
                  {
                      From = 1.0,
                      To = 0.0,
                      Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                      EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                  };
                 Storyboard.SetTarget(fadeOut, currentPage);
                 Storyboard.SetTargetProperty(fadeOut, "Opacity");
                 slideOutStoryboard.Children.Add(fadeOut);

                 var slideOut = new DoubleAnimation
                  {
                      From = 0.0,
                      To = slideOutDistance,
                      Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                      EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                  };
                 Storyboard.SetTarget(slideOut, currentPage);
                 Storyboard.SetTargetProperty(slideOut, "(UIElement.RenderTransform).(TranslateTransform.Y)");
                 slideOutStoryboard.Children.Add(slideOut);

                 // Ensure current page has a TranslateTransform
                 if (currentPage.RenderTransform == null || !(currentPage.RenderTransform is TranslateTransform))
                     currentPage.RenderTransform = new TranslateTransform();

                 // When slide out completes, start slide in
                 slideOutStoryboard.Completed += (s, e) =>
                 {
                     // Hide current page and show target page
                     currentPage.Visibility = Visibility.Collapsed;
                     currentPage.Opacity = 1.0; // Reset opacity
                     ((TranslateTransform)currentPage.RenderTransform).Y = 0; // Reset transform
                     
                     targetPage.Visibility = Visibility.Visible;
                     targetPage.Opacity = 0.0;
                     
                     // Ensure target page has a TranslateTransform
                     if (targetPage.RenderTransform == null || !(targetPage.RenderTransform is TranslateTransform))
                         targetPage.RenderTransform = new TranslateTransform();
                     
                     ((TranslateTransform)targetPage.RenderTransform).Y = slideInStartPosition; // Start from calculated position

                     // Create slide in and fade in storyboard for target page
                     var slideInStoryboard = new Storyboard();
                     
                     var fadeIn = new DoubleAnimation
                      {
                          From = 0.0,
                          To = 1.0,
                          Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                          EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                      };
                     Storyboard.SetTarget(fadeIn, targetPage);
                     Storyboard.SetTargetProperty(fadeIn, "Opacity");
                     slideInStoryboard.Children.Add(fadeIn);

                     var slideIn = new DoubleAnimation
                      {
                          From = slideInStartPosition,
                          To = 0.0,
                          Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                          EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                      };
                     Storyboard.SetTarget(slideIn, targetPage);
                     Storyboard.SetTargetProperty(slideIn, "(UIElement.RenderTransform).(TranslateTransform.Y)");
                     slideInStoryboard.Children.Add(slideIn);
                     
                     slideInStoryboard.Begin();
                 };

                 slideOutStoryboard.Begin();
             }
            else if (targetPage != null)
             {
                 // No current page, just show target page with slide up and fade in
                 FanPage.Visibility = Visibility.Collapsed;
                 GraphicsPage.Visibility = Visibility.Collapsed;
                 KeyboardPage.Visibility = Visibility.Collapsed;
                 MonitorPage.Visibility = Visibility.Collapsed;
                 SettingsPage.Visibility = Visibility.Collapsed;

                 targetPage.Visibility = Visibility.Visible;
                 targetPage.Opacity = 0.0;
                 
                 // Ensure target page has a TranslateTransform
                 if (targetPage.RenderTransform == null || !(targetPage.RenderTransform is TranslateTransform))
                     targetPage.RenderTransform = new TranslateTransform();
                 
                 ((TranslateTransform)targetPage.RenderTransform).Y = 30.0; // Start from below

                 var slideInStoryboard = new Storyboard();
                 
                 var fadeIn = new DoubleAnimation
                  {
                      From = 0.0,
                      To = 1.0,
                      Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                      EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                  };
                 Storyboard.SetTarget(fadeIn, targetPage);
                 Storyboard.SetTargetProperty(fadeIn, "Opacity");
                 slideInStoryboard.Children.Add(fadeIn);

                 var slideIn = new DoubleAnimation
                  {
                      From = 30.0,
                      To = 0.0,
                      Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                      EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                  };
                 Storyboard.SetTarget(slideIn, targetPage);
                 Storyboard.SetTargetProperty(slideIn, "(UIElement.RenderTransform).(TranslateTransform.Y)");
                 slideInStoryboard.Children.Add(slideIn);
                 
                 slideInStoryboard.Begin();
             }
        }

        // Settings Page Event Handlers
        private void LoadSettingsPagePreferences()
        {
            try
            {
                Debug.WriteLine("[MainWindow] LoadSettingsPagePreferences called");
                var appSettings = AppSettings.Instance;
                
                Debug.WriteLine($"[MainWindow] Retrieved settings - BackdropSelectedIndex: {appSettings.BackdropSelectedIndex}, SelectedWallpaperIndex: {appSettings.SelectedWallpaperIndex}");
                
                // Load backdrop selection without triggering events
                if (BackdropComboBox != null)
                {
                    Debug.WriteLine($"[MainWindow] Setting BackdropComboBox.SelectedIndex to {appSettings.BackdropSelectedIndex}");
                    BackdropComboBox.SelectionChanged -= BackdropComboBox_SelectionChanged;
                    BackdropComboBox.SelectedIndex = appSettings.BackdropSelectedIndex;
                    BackdropComboBox.SelectionChanged += BackdropComboBox_SelectionChanged;
                    
                    // Apply the saved backdrop setting
                    if (BackdropComboBox.SelectedItem is ComboBoxItem selectedItem)
                    {
                        var backdrop = selectedItem.Tag?.ToString();
                        Debug.WriteLine($"[MainWindow] Applying backdrop setting: {backdrop}");
                        ApplyBackdropSettings(backdrop);
                        
                        // Show wallpaper panel and apply wallpaper if Image backdrop is selected
                        if (backdrop == "Image" && WallpaperPanel != null)
                        {
                            Debug.WriteLine($"[MainWindow] Image backdrop selected, showing wallpaper panel and applying wallpaper index {appSettings.SelectedWallpaperIndex}");
                            WallpaperPanel.Visibility = Visibility.Visible;
                            UpdateWallpaperSelection(appSettings.SelectedWallpaperIndex);
                            // Apply the saved wallpaper
                            ApplyWallpaperSettings(appSettings.SelectedWallpaperIndex.ToString());
                        }
                        else if (WallpaperPanel != null)
                        {
                            Debug.WriteLine("[MainWindow] Non-image backdrop selected, hiding wallpaper panel");
                            WallpaperPanel.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("[MainWindow] Warning: BackdropComboBox.SelectedItem is not a ComboBoxItem");
                    }
                }
                else
                {
                    Debug.WriteLine("[MainWindow] Warning: BackdropComboBox is null");
                }
                
                LogInfo("Settings page preferences loaded");
                Debug.WriteLine("[MainWindow] LoadSettingsPagePreferences completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error in LoadSettingsPagePreferences: {ex.Message}");
                LogError($"Error loading settings page preferences: {ex.Message}");
            }
        }
        
        private void BackdropComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Debug.WriteLine($"[MainWindow] BackdropComboBox_SelectionChanged triggered, SelectedIndex: {BackdropComboBox?.SelectedIndex}");
                
                if (BackdropComboBox?.SelectedItem is ComboBoxItem selectedItem)
                {
                    var backdrop = selectedItem.Tag?.ToString();
                    Debug.WriteLine($"[MainWindow] User selected backdrop: {backdrop}");
                    
                    // Save preference
                    Debug.WriteLine($"[MainWindow] Saving BackdropSelectedIndex: {BackdropComboBox.SelectedIndex}");
                    AppSettings.Instance.BackdropSelectedIndex = BackdropComboBox.SelectedIndex;
                    
                    // Apply the selected backdrop
                    Debug.WriteLine($"[MainWindow] Applying backdrop: {backdrop}");
                    ApplyBackdropSettings(backdrop);
                    
                    // Show/hide wallpaper selection based on backdrop choice
                    if (backdrop == "Image")
                    {
                        if (WallpaperPanel != null)
                        {
                            Debug.WriteLine("[MainWindow] Showing wallpaper panel for Image backdrop");
                            WallpaperPanel.Visibility = Visibility.Visible;
                            
                            // Apply the default wallpaper immediately
                            var defaultWallpaperIndex = AppSettings.Instance.SelectedWallpaperIndex;
                            Debug.WriteLine($"[MainWindow] Applying default wallpaper index: {defaultWallpaperIndex}");
                            UpdateWallpaperSelection(defaultWallpaperIndex);
                            ApplyWallpaperSettings(defaultWallpaperIndex.ToString());
                        }
                        else
                        {
                            LogError("WallpaperPanel is null when trying to show it");
                        }
                    }
                    else
                    {
                        if (WallpaperPanel != null)
                        {
                            Debug.WriteLine("[MainWindow] Hiding wallpaper panel for non-Image backdrop");
                            WallpaperPanel.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            LogError("WallpaperPanel is null when trying to hide it");
                        }
                    }
                    
                    // Backdrop changed notification removed
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error in BackdropComboBox_SelectionChanged: {ex.Message}");
                LogError($"Error in BackdropComboBox_SelectionChanged: {ex.Message}");
                if (SettingsInfoBar != null)
                {
                    SettingsInfoBar.IsOpen = true;
                    SettingsInfoBar.Message = "An error occurred while changing backdrop settings.";
                    SettingsInfoBar.Severity = InfoBarSeverity.Error;
                }
            }
        }
        
        private void WallpaperBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Handle wallpaper selection
            if (sender is Border border && border.Tag is string wallpaperIndex)
            {
                Debug.WriteLine($"[MainWindow] WallpaperBorder_Tapped - wallpaper index: {wallpaperIndex}");
                
                // Save preference
                if (int.TryParse(wallpaperIndex, out int index))
                {
                    Debug.WriteLine($"[MainWindow] Saving SelectedWallpaperIndex: {index}");
                    AppSettings.Instance.SelectedWallpaperIndex = index;
                    UpdateWallpaperSelection(index);
                }
                
                // Apply selected wallpaper
                Debug.WriteLine($"[MainWindow] Applying wallpaper settings for index: {wallpaperIndex}");
                ApplyWallpaperSettings(wallpaperIndex);
            }
        }
        
        private void UpdateWallpaperSelection(int selectedIndex)
        {
            try
            {
                // Reset all wallpaper borders
                var wallpaperBorders = new[] { Wallpaper0, Wallpaper1, Wallpaper2, Wallpaper3, Wallpaper4, Wallpaper6, Wallpaper7 };
                
                foreach (var border in wallpaperBorders)
                {
                    if (border != null)
                    {
                        border.BorderThickness = new Thickness(0);
                        border.BorderBrush = null;
                    }
                }
                
                // Highlight selected wallpaper
                Border selectedBorder = selectedIndex switch
                {
                    0 => Wallpaper0,
                    1 => Wallpaper1,
                    2 => Wallpaper2,
                    3 => Wallpaper3,
                    4 => Wallpaper4,
                    6 => Wallpaper6,
                    7 => Wallpaper7,
                    _ => null
                };
                
                if (selectedBorder != null)
                {
                    selectedBorder.BorderThickness = new Thickness(3);
                    selectedBorder.BorderBrush = (SolidColorBrush)Application.Current.Resources["AccentFillColorDefaultBrush"];
                }
            }
            catch (Exception ex)
            {
                LogError($"Error updating wallpaper selection: {ex.Message}");
            }
        }

        private void ApplyWallpaperSettings(string wallpaperIndex)
         {
             try
             {
                 // Apply acrylic backdrop first
                 this.SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
                 
                 // Apply the selected wallpaper using Composition API
                 var wallpaperPath = $"/Assets/wallpapers/{wallpaperIndex}.png";
                 
                 if (this.Content is Grid mainGrid)
                 {
                     // Remove any existing wallpaper borders
                     var existingBorders = mainGrid.Children.OfType<Border>().Where(b => b.Name == "WallpaperBorder").ToList();
                     foreach (var border in existingBorders)
                     {
                         mainGrid.Children.Remove(border);
                     }
                     
                     // Create a container for the composition visual
                     var wallpaperContainer = new Border
                     {
                         Name = "WallpaperBorder"
                     };
                     
                     mainGrid.Background = null;
                     mainGrid.Children.Insert(0, wallpaperContainer);
                     
                     // Set up composition visual with gradient mask
                     wallpaperContainer.Loaded += (s, e) =>
                     {
                         var compositor = ElementCompositionPreview.GetElementVisual(wallpaperContainer).Compositor;
                         
                         // Create brush for the image
                          var imageBrush = compositor.CreateSurfaceBrush(
                               LoadedImageSurface.StartLoadFromUri(new Uri($"ms-appx://{wallpaperPath}"))
                           );
                           imageBrush.Stretch = Microsoft.UI.Composition.CompositionStretch.UniformToFill;
                         
                         // Create gradient mask (transparent -> opaque)
                         var gradient = compositor.CreateLinearGradientBrush();
                         gradient.StartPoint = new Vector2(0, 0);
                         gradient.EndPoint = new Vector2(0, 1);
                         
                         var stop1 = compositor.CreateColorGradientStop();
                         stop1.Offset = 0.5f;
                         stop1.Color = Windows.UI.Color.FromArgb(8, 0, 0, 0); // fully transparent
                         
                         var stop2 = compositor.CreateColorGradientStop();
                         stop2.Offset = 1f;
                         stop2.Color = Colors.White; // solid
                         
                         gradient.ColorStops.Add(stop1);
                         gradient.ColorStops.Add(stop2);
                         
                         // Mask brush
                         var mask = compositor.CreateMaskBrush();
                         mask.Source = imageBrush;
                         mask.Mask = gradient;
                         
                         // SpriteVisual to display
                         var sprite = compositor.CreateSpriteVisual();
                         sprite.Brush = mask;
                         sprite.Size = new Vector2(
                             (float)mainGrid.ActualWidth,
                             (float)mainGrid.ActualHeight
                         );
                         
                         ElementCompositionPreview.SetElementChildVisual(wallpaperContainer, sprite);
                         
                         // Update size when grid size changes
                         mainGrid.SizeChanged += (sender, args) =>
                         {
                             sprite.Size = new Vector2(
                                 (float)args.NewSize.Width,
                                 (float)args.NewSize.Height
                             );
                         };
                     };
                 }
                 
                 LogInfo($"Wallpaper applied with acrylic backdrop: {wallpaperPath}");
                 
                 // Wallpaper changed notification removed
             }
             catch (Exception ex)
             {
                 LogError($"Failed to apply wallpaper {wallpaperIndex}: {ex.Message}");
                 if (SettingsInfoBar != null)
                 {
                     SettingsInfoBar.IsOpen = true;
                     SettingsInfoBar.Message = $"Failed to apply wallpaper {wallpaperIndex}.";
                     SettingsInfoBar.Severity = InfoBarSeverity.Error;
                 }
             }
          }

         private void ApplyBackdropSettings(string backdropType)
          {
              try
              {
                  // Get the main grid once
                  Grid? mainGrid = this.Content as Grid;
                  
                  switch (backdropType)
                  {
                      case "Mica":
                          this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
                          // Clear any custom background and wallpaper borders
                          if (mainGrid != null)
                          {
                              mainGrid.Background = null;
                              var wallpaperBorders = mainGrid.Children.OfType<Border>().Where(b => b.Name == "WallpaperBorder").ToList();
                              foreach (var border in wallpaperBorders)
                              {
                                  mainGrid.Children.Remove(border);
                              }
                          }
                          break;
                      case "Acrylic":
                          this.SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
                          // Clear any custom background and wallpaper borders
                          if (mainGrid != null)
                          {
                              mainGrid.Background = null;
                              var wallpaperBorders = mainGrid.Children.OfType<Border>().Where(b => b.Name == "WallpaperBorder").ToList();
                              foreach (var border in wallpaperBorders)
                              {
                                  mainGrid.Children.Remove(border);
                              }
                          }
                          break;
                      case "Image":
                          // Remove system backdrop for image background
                          this.SystemBackdrop = null;
                          break;
                      default:
                          this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
                          // Clear any custom background
                          if (mainGrid != null)
                          {
                              mainGrid.Background = null;
                          }
                          break;
                  }
                  
                  LogInfo($"Backdrop changed to: {backdropType}");
              }
              catch (Exception ex)
              {
                  LogError($"Failed to apply backdrop {backdropType}: {ex.Message}");
              }
           }

        // Console functionality
        private void ClearConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleOutput.Text = "HP Gaming Hub Console - Ready\n";
        }

        private void CopyConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                dataPackage.SetText(ConsoleOutput.Text);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                
                // Optional: Show a brief confirmation
                LogInfo("Console content copied to clipboard");
            }
            catch (Exception ex)
            {
                LogError($"Failed to copy to clipboard: {ex.Message}");
            }
        }
    }
}