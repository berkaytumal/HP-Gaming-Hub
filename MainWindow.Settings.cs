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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;

namespace HP_Gaming_Hub
{
    public sealed partial class MainWindow : Window
    {
        // Navigation and Settings Event Handlers
        
        private async void MainNavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            // Find currently visible page
            FrameworkElement currentPage = null;
            if (FanPage.Visibility == Visibility.Visible) currentPage = FanPage;
            else if (GraphicsPage.Visibility == Visibility.Visible) currentPage = GraphicsPage;
            else if (KeyboardPage.Visibility == Visibility.Visible) currentPage = KeyboardPage;
            else if (MonitorPage.Visibility == Visibility.Visible) currentPage = MonitorPage;
            else if (LogsPage.Visibility == Visibility.Visible) currentPage = LogsPage;
            else if (SettingsPage.Visibility == Visibility.Visible) currentPage = SettingsPage;

            // Determine target page and index
            FrameworkElement targetPage = null;
            int currentSelectedIndex = -1;
            if (args.SelectedItem == FanNavItem) { targetPage = FanPage; currentSelectedIndex = 0; }
            else if (args.SelectedItem == GraphicsNavItem) { targetPage = GraphicsPage; currentSelectedIndex = 1; }
            else if (args.SelectedItem == KeyboardNavItem) { targetPage = KeyboardPage; currentSelectedIndex = 2; }
            else if (args.SelectedItem == MonitorNavItem) { targetPage = MonitorPage; currentSelectedIndex = 3; }
            else if (args.SelectedItem == LogsNavItem) { targetPage = LogsPage; currentSelectedIndex = 4; }
            else if (args.SelectedItem == SettingsNavItem) { targetPage = SettingsPage; currentSelectedIndex = 5; }

            // If same page is selected, do nothing
            if (currentPage == targetPage) return;
            
            // Load preferences when navigating to settings page
            if (targetPage == SettingsPage)
            {
                await LoadSettingsPagePreferences();
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
        private async Task LoadSettingsPagePreferences()
        {
            try
            {
                Debug.WriteLine("[MainWindow] LoadSettingsPagePreferences called");
                var appSettings = AppSettings.Instance;
                
                Debug.WriteLine($"[MainWindow] Retrieved settings - BackdropSelectedIndex: {appSettings.BackdropSelectedIndex}, SelectedWallpaperIndex: {appSettings.SelectedWallpaperIndex}");
                
                // Load monitor settings without triggering events
                if (AutoStartMonitoringToggle != null)
                {
                    AutoStartMonitoringToggle.Toggled -= AutoStartMonitoringToggle_Toggled;
                    AutoStartMonitoringToggle.IsOn = appSettings.AutoStartMonitoring;
                    AutoStartMonitoringToggle.Toggled += AutoStartMonitoringToggle_Toggled;
                }
                
                if (AutoRefreshToggle != null)
                {
                    AutoRefreshToggle.Toggled -= AutoRefreshToggle_Toggled;
                    AutoRefreshToggle.IsOn = appSettings.AutoRefresh;
                    AutoRefreshToggle.Toggled += AutoRefreshToggle_Toggled;
                }
                
                if (FocusedIntervalSlider != null && FocusedIntervalNumberBox != null)
                {
                    FocusedIntervalSlider.ValueChanged -= FocusedIntervalSlider_ValueChanged;
                    FocusedIntervalNumberBox.ValueChanged -= FocusedIntervalNumberBox_ValueChanged;
                    FocusedIntervalSlider.Value = appSettings.FocusedRefreshInterval;
                    FocusedIntervalNumberBox.Value = appSettings.FocusedRefreshInterval;
                    FocusedIntervalSlider.ValueChanged += FocusedIntervalSlider_ValueChanged;
                    FocusedIntervalNumberBox.ValueChanged += FocusedIntervalNumberBox_ValueChanged;
                }
                
                if (BlurredIntervalSlider != null && BlurredIntervalNumberBox != null)
                {
                    BlurredIntervalSlider.ValueChanged -= BlurredIntervalSlider_ValueChanged;
                    BlurredIntervalNumberBox.ValueChanged -= BlurredIntervalNumberBox_ValueChanged;
                    BlurredIntervalSlider.Value = appSettings.BlurredRefreshInterval;
                    BlurredIntervalNumberBox.Value = appSettings.BlurredRefreshInterval;
                    BlurredIntervalSlider.ValueChanged += BlurredIntervalSlider_ValueChanged;
                    BlurredIntervalNumberBox.ValueChanged += BlurredIntervalNumberBox_ValueChanged;
                }
                
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
                            ShowWallpaperGalleryWithAnimation();
                            UpdateWallpaperSelection(appSettings.SelectedWallpaperIndex);
                            // Apply the saved wallpaper
                            await ApplyWallpaperSettings(appSettings.SelectedWallpaperIndex.ToString());
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
        
        private async void BackdropComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                            ShowWallpaperGalleryWithAnimation();
                            
                            // Apply the default wallpaper immediately
                            var defaultWallpaperIndex = AppSettings.Instance.SelectedWallpaperIndex;
                            Debug.WriteLine($"[MainWindow] Applying default wallpaper index: {defaultWallpaperIndex}");
                            UpdateWallpaperSelection(defaultWallpaperIndex);
                            await ApplyWallpaperSettings(defaultWallpaperIndex.ToString());
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
                            HideWallpaperGalleryWithAnimation();
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
        
        private async void WallpaperBorder_Tapped(object sender, TappedRoutedEventArgs e)
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
                await ApplyWallpaperSettings(wallpaperIndex);
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
                        // Reset scale transform
                        border.RenderTransform = new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 };
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

        private void ShowWallpaperGalleryWithAnimation()
        {
            try
            {
                if (WallpaperPanel == null) return;

                // Make panel visible but keep it transparent
                WallpaperPanel.Visibility = Visibility.Visible;
                WallpaperPanel.Opacity = 0;

                // Animate panel fade-in
                var panelFadeIn = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                var panelStoryboard = new Storyboard();
                Storyboard.SetTarget(panelFadeIn, WallpaperPanel);
                Storyboard.SetTargetProperty(panelFadeIn, "Opacity");
                panelStoryboard.Children.Add(panelFadeIn);

                // Start panel animation and then animate individual wallpapers
                panelStoryboard.Completed += (s, e) => AnimateWallpaperThumbnails(true);
                panelStoryboard.Begin();
            }
            catch (Exception ex)
            {
                LogError($"Error showing wallpaper gallery: {ex.Message}");
            }
        }

        private void HideWallpaperGalleryWithAnimation()
        {
            try
            {
                if (WallpaperPanel == null) return;

                // Animate wallpapers out first, then panel
                AnimateWallpaperThumbnails(false, () =>
                {
                    // Animate panel fade-out
                    var panelFadeOut = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = TimeSpan.FromMilliseconds(200),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                    };

                    var panelStoryboard = new Storyboard();
                    Storyboard.SetTarget(panelFadeOut, WallpaperPanel);
                    Storyboard.SetTargetProperty(panelFadeOut, "Opacity");
                    panelStoryboard.Children.Add(panelFadeOut);

                    panelStoryboard.Completed += (s, e) =>
                    {
                        WallpaperPanel.Visibility = Visibility.Collapsed;
                    };
                    panelStoryboard.Begin();
                });
            }
            catch (Exception ex)
            {
                LogError($"Error hiding wallpaper gallery: {ex.Message}");
            }
        }

        private void AnimateWallpaperThumbnails(bool fadeIn, Action onComplete = null)
        {
            try
            {
                var wallpaperBorders = new[] { Wallpaper0, Wallpaper1, Wallpaper2, Wallpaper3, Wallpaper4, Wallpaper6, Wallpaper7 };
                var masterStoryboard = new Storyboard();
                int delay = 0;

                foreach (var border in wallpaperBorders)
                {
                    if (border == null) continue;

                    // Opacity animation only
                    var opacityAnimation = new DoubleAnimation
                    {
                        From = fadeIn ? 0.0 : 1.0,
                        To = fadeIn ? 1.0 : 0.0,
                        Duration = TimeSpan.FromMilliseconds(400),
                        BeginTime = TimeSpan.FromMilliseconds(delay),
                        EasingFunction = new QuadraticEase { EasingMode = fadeIn ? EasingMode.EaseOut : EasingMode.EaseIn }
                    };

                    Storyboard.SetTarget(opacityAnimation, border);
                    Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
                    masterStoryboard.Children.Add(opacityAnimation);

                    delay += 50; // Stagger each thumbnail by 50ms
                }

                if (onComplete != null)
                {
                    masterStoryboard.Completed += (s, e) => onComplete();
                }

                masterStoryboard.Begin();
            }
            catch (Exception ex)
            {
                LogError($"Error animating wallpaper thumbnails: {ex.Message}");
            }
        }

        private async Task ApplyWallpaperSettings(string wallpaperIndex)
         {
             try
             {
                 // Apply acrylic backdrop first
                 this.SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
                 
                 // Apply the selected wallpaper using Composition API
                 var wallpaperPath = $"/Assets/wallpapers/{wallpaperIndex}.png";
                 
                 if (this.Content is Grid mainGrid)
                 {
                     // Get existing wallpaper borders for animation
                     var existingBorders = mainGrid.Children.OfType<Border>().Where(b => b.Name == "WallpaperBorder").ToList();
                     
                     // Create a container for the new composition visual
                     var wallpaperContainer = new Border
                     {
                         Name = "WallpaperBorder",
                         Opacity = 0 // Start invisible for fade-in animation
                     };
                     
                     mainGrid.Background = null;
                     mainGrid.Children.Insert(0, wallpaperContainer);
                     
                     // Animate out existing wallpapers if any
                     if (existingBorders.Count > 0)
                     {
                         var fadeOutStoryboard = new Storyboard();
                         
                         foreach (var existingBorder in existingBorders)
                         {
                             var fadeOutAnimation = new DoubleAnimation
                             {
                                 From = 1.0,
                                 To = 0.0,
                                 Duration = new Duration(TimeSpan.FromMilliseconds(300))
                             };
                             
                             Storyboard.SetTarget(fadeOutAnimation, existingBorder);
                             Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");
                             fadeOutStoryboard.Children.Add(fadeOutAnimation);
                         }
                         
                         fadeOutStoryboard.Completed += (s, e) =>
                         {
                             // Remove old borders after fade out
                             foreach (var border in existingBorders)
                             {
                                 mainGrid.Children.Remove(border);
                             }
                         };
                         
                         fadeOutStoryboard.Begin();
                     }
                     
                     // Set up composition visual with gradient mask immediately
                     await SetupWallpaperCompositionVisual(wallpaperContainer, wallpaperPath, mainGrid);
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

         private SizeChangedEventHandler _wallpaperSizeChangedHandler;
        private Microsoft.UI.Composition.SpriteVisual _currentWallpaperSprite;

         private async Task SetupWallpaperCompositionVisual(Border wallpaperContainer, string wallpaperPath, Grid mainGrid)
         {
             // Force layout update and wait a bit to ensure proper sizing
             wallpaperContainer.UpdateLayout();
             mainGrid.UpdateLayout();
             await Task.Delay(50);
             
             // Clean up previous handler and sprite if they exist
             CleanupWallpaperCompositionVisual(mainGrid);
             
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
             _currentWallpaperSprite = compositor.CreateSpriteVisual();
             _currentWallpaperSprite.Brush = mask;
             
             // Ensure we have valid dimensions before setting size
             var width = (float)mainGrid.ActualWidth;
             var height = (float)mainGrid.ActualHeight;
             
             if (width > 0 && height > 0)
             {
                 _currentWallpaperSprite.Size = new Vector2(width, height);
             }
             else
             {
                 // Fallback to window bounds if ActualWidth/Height are still 0
                 var bounds = this.AppWindow.Size;
                 _currentWallpaperSprite.Size = new Vector2((float)bounds.Width, (float)bounds.Height);
             }
             
             ElementCompositionPreview.SetElementChildVisual(wallpaperContainer, _currentWallpaperSprite);
             
             // Create and store the event handler
             _wallpaperSizeChangedHandler = (sender, args) =>
             {
                 try
                 {
                     if (args.NewSize.Width > 0 && args.NewSize.Height > 0 && _currentWallpaperSprite != null)
                     {
                         _currentWallpaperSprite.Size = new Vector2(
                             (float)args.NewSize.Width,
                             (float)args.NewSize.Height
                         );
                     }
                 }
                 catch (System.ObjectDisposedException)
                 {
                     // Visual has been disposed, unsubscribe from the event
                     CleanupWallpaperCompositionVisual(mainGrid);
                 }
             };
             
             // Subscribe to size changes
             mainGrid.SizeChanged += _wallpaperSizeChangedHandler;
             
             // Fade in the new wallpaper
             var fadeInAnimation = new DoubleAnimation
             {
                 From = 0.0,
                 To = 1.0,
                 Duration = new Duration(TimeSpan.FromMilliseconds(400))
             };
             
             var fadeInStoryboard = new Storyboard();
             Storyboard.SetTarget(fadeInAnimation, wallpaperContainer);
             Storyboard.SetTargetProperty(fadeInAnimation, "Opacity");
             fadeInStoryboard.Children.Add(fadeInAnimation);
             fadeInStoryboard.Begin();
         }

         private void CleanupWallpaperCompositionVisual(Grid mainGrid)
        {
            try
            {
                // Unsubscribe from size changed event
                if (mainGrid != null && _wallpaperSizeChangedHandler != null)
                {
                    mainGrid.SizeChanged -= _wallpaperSizeChangedHandler;
                    _wallpaperSizeChangedHandler = null;
                }
                
                // Dispose the sprite visual
                if (_currentWallpaperSprite != null)
                {
                    _currentWallpaperSprite.Dispose();
                    _currentWallpaperSprite = null;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error cleaning up wallpaper composition visual: {ex.Message}");
            }
        }

        public void CleanupWallpaperCompositionVisual()
        {
            try
            {
                // Get the main grid from content
                Grid? mainGrid = this.Content as Grid;
                CleanupWallpaperCompositionVisual(mainGrid);
            }
            catch (Exception ex)
            {
                LogError($"Error cleaning up wallpaper composition visual: {ex.Message}");
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
        private void ClearLogsButton_Click(object sender, RoutedEventArgs e)
        {
            LogsOutput.Text = "HP Gaming Hub Console - Ready\n";
        }

        private void CopyLogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                dataPackage.SetText(LogsOutput.Text);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                
                // Optional: Show a brief confirmation
                LogInfo("Console content copied to clipboard");
            }
            catch (Exception ex)
            {
                LogError($"Failed to copy to clipboard: {ex.Message}");
            }
        }
        
        // Monitor Settings Event Handlers
        private void AutoStartMonitoringToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is ToggleSwitch toggle)
                {
                    AppSettings.Instance.AutoStartMonitoring = toggle.IsOn;
                    LogInfo($"Auto start monitoring {(toggle.IsOn ? "enabled" : "disabled")}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error updating auto start monitoring setting: {ex.Message}");
            }
        }
        
        private void AutoRefreshToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is ToggleSwitch toggle)
                {
                    AppSettings.Instance.AutoRefresh = toggle.IsOn;
                    LogInfo($"Auto refresh {(toggle.IsOn ? "enabled" : "disabled")}");
                    
                    // Update the hardware monitor view model
                    if (_hardwareMonitorViewModel != null)
                    {
                        _hardwareMonitorViewModel.OnAutoRefreshChanged(toggle.IsOn);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error updating auto refresh setting: {ex.Message}");
            }
        }
        
        private void FocusedIntervalSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            try
            {
                if (sender is Slider slider && FocusedIntervalNumberBox != null)
                {
                    var value = (int)slider.Value;
                    FocusedIntervalNumberBox.ValueChanged -= FocusedIntervalNumberBox_ValueChanged;
                    FocusedIntervalNumberBox.Value = value;
                    FocusedIntervalNumberBox.ValueChanged += FocusedIntervalNumberBox_ValueChanged;
                    
                    AppSettings.Instance.FocusedRefreshInterval = value;
                    
                    // Update the hardware monitor view model
                    if (_hardwareMonitorViewModel != null)
                    {
                        _hardwareMonitorViewModel.OnRefreshIntervalChanged(value, AppSettings.Instance.BlurredRefreshInterval);
                    }
                    
                    LogInfo($"Focused refresh interval set to {value} seconds");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error updating focused refresh interval: {ex.Message}");
            }
        }
        
        private void FocusedIntervalNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            try
            {
                if (FocusedIntervalSlider != null && !double.IsNaN(sender.Value))
                {
                    var value = (int)sender.Value;
                    FocusedIntervalSlider.ValueChanged -= FocusedIntervalSlider_ValueChanged;
                    FocusedIntervalSlider.Value = value;
                    FocusedIntervalSlider.ValueChanged += FocusedIntervalSlider_ValueChanged;
                    
                    AppSettings.Instance.FocusedRefreshInterval = value;
                    
                    // Update the hardware monitor view model
                    if (_hardwareMonitorViewModel != null)
                    {
                        _hardwareMonitorViewModel.OnRefreshIntervalChanged(value, AppSettings.Instance.BlurredRefreshInterval);
                    }
                    
                    LogInfo($"Focused refresh interval set to {value} seconds");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error updating focused refresh interval: {ex.Message}");
            }
        }
        
        private void BlurredIntervalSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            try
            {
                if (sender is Slider slider && BlurredIntervalNumberBox != null)
                {
                    var value = (int)slider.Value;
                    BlurredIntervalNumberBox.ValueChanged -= BlurredIntervalNumberBox_ValueChanged;
                    BlurredIntervalNumberBox.Value = value;
                    BlurredIntervalNumberBox.ValueChanged += BlurredIntervalNumberBox_ValueChanged;
                    
                    AppSettings.Instance.BlurredRefreshInterval = value;
                    
                    // Update the hardware monitor view model
                    if (_hardwareMonitorViewModel != null)
                    {
                        _hardwareMonitorViewModel.OnRefreshIntervalChanged(AppSettings.Instance.FocusedRefreshInterval, value);
                    }
                    
                    LogInfo($"Blurred refresh interval set to {value} seconds");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error updating blurred refresh interval: {ex.Message}");
            }
        }
        
        private void BlurredIntervalNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            try
            {
                if (BlurredIntervalSlider != null && !double.IsNaN(sender.Value))
                {
                    var value = (int)sender.Value;
                    BlurredIntervalSlider.ValueChanged -= BlurredIntervalSlider_ValueChanged;
                    BlurredIntervalSlider.Value = value;
                    BlurredIntervalSlider.ValueChanged += BlurredIntervalSlider_ValueChanged;
                    
                    AppSettings.Instance.BlurredRefreshInterval = value;
                    
                    // Update the hardware monitor view model
                    if (_hardwareMonitorViewModel != null)
                    {
                        _hardwareMonitorViewModel.OnRefreshIntervalChanged(AppSettings.Instance.FocusedRefreshInterval, value);
                    }
                    
                    LogInfo($"Blurred refresh interval set to {value} seconds");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error updating blurred refresh interval: {ex.Message}");
            }
        }
    }
}