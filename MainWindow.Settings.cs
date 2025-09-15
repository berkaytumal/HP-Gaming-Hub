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
using Serilog;

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
            else if (SettingsPage.Visibility == Visibility.Visible) currentPage = SettingsPage;

            // Determine target page and index
            FrameworkElement targetPage = null;
            int currentSelectedIndex = -1;
            if (args.SelectedItem == FanNavItem) { targetPage = FanPage; currentSelectedIndex = 0; }
            else if (args.SelectedItem == GraphicsNavItem) { targetPage = GraphicsPage; currentSelectedIndex = 1; }
            else if (args.SelectedItem == KeyboardNavItem) { targetPage = KeyboardPage; currentSelectedIndex = 2; }
            else if (args.SelectedItem == MonitorNavItem) { targetPage = MonitorPage; currentSelectedIndex = 3; }
            else if (args.SelectedItem == SettingsNavItem) { targetPage = SettingsPage; currentSelectedIndex = 4; }

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
                Log.Debug("MainWindow - LoadSettingsPagePreferences called");
                var appSettings = AppSettings.Instance;
                
                Log.Debug("MainWindow - Retrieved settings - BackdropSelectedIndex: {BackdropSelectedIndex}, SelectedWallpaperIndex: {SelectedWallpaperIndex}", appSettings.BackdropSelectedIndex, appSettings.SelectedWallpaperIndex);
                
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
                    // AutoRefresh is runtime-only, enable it when AutoStartMonitoring is on
                    AutoRefreshToggle.IsOn = appSettings.AutoStartMonitoring;
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
                    Log.Debug("MainWindow - Setting BackdropComboBox.SelectedIndex to {BackdropSelectedIndex}", appSettings.BackdropSelectedIndex);
                    BackdropComboBox.SelectionChanged -= BackdropComboBox_SelectionChanged;
                    BackdropComboBox.SelectedIndex = appSettings.BackdropSelectedIndex;
                    BackdropComboBox.SelectionChanged += BackdropComboBox_SelectionChanged;
                    
                    // Apply the saved backdrop setting
                    if (BackdropComboBox.SelectedItem is ComboBoxItem selectedItem)
                    {
                        var backdrop = selectedItem.Tag?.ToString();
                        Log.Debug("MainWindow - Applying backdrop setting: {Backdrop}", backdrop);
                        ApplyBackdropSettings(backdrop);
                        
                        // Show wallpaper panel and apply wallpaper if Image backdrop is selected
                        if (backdrop == "Image" && WallpaperPanel != null)
                        {
                            Log.Debug("MainWindow - Image backdrop selected, showing wallpaper panel and applying wallpaper index {WallpaperIndex}", appSettings.SelectedWallpaperIndex);
                            ShowWallpaperGalleryWithAnimation();
                            UpdateWallpaperSelection(appSettings.SelectedWallpaperIndex);
                            // Apply the saved wallpaper
                            await ApplyWallpaperSettings(appSettings.SelectedWallpaperIndex.ToString());
                        }
                        else if (WallpaperPanel != null)
                        {
                            Log.Debug("MainWindow - Non-image backdrop selected, hiding wallpaper panel");
                            WallpaperPanel.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        Log.Warning("MainWindow - BackdropComboBox.SelectedItem is not a ComboBoxItem");
                    }
                }
                else
                {
                    Log.Warning("MainWindow - BackdropComboBox is null");
                }
                
                Log.Information("Settings page preferences loaded");
                Log.Debug("MainWindow - LoadSettingsPagePreferences completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MainWindow - Error in LoadSettingsPagePreferences");
                Log.Error(ex, "Error loading settings page preferences: {Message}", ex.Message);
            }
        }
        
        private async void BackdropComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Log.Debug("MainWindow - BackdropComboBox_SelectionChanged triggered, SelectedIndex: {SelectedIndex}", BackdropComboBox?.SelectedIndex);
                
                if (BackdropComboBox?.SelectedItem is ComboBoxItem selectedItem)
                {
                    var backdrop = selectedItem.Tag?.ToString();
                    Log.Debug("MainWindow - User selected backdrop: {Backdrop}", backdrop);
                    
                    // Save preference
                    Log.Debug("MainWindow - Saving BackdropSelectedIndex: {SelectedIndex}", BackdropComboBox.SelectedIndex);
                    AppSettings.Instance.BackdropSelectedIndex = BackdropComboBox.SelectedIndex;
                    
                    // Apply the selected backdrop
                    Log.Debug("MainWindow - Applying backdrop: {Backdrop}", backdrop);
                    ApplyBackdropSettings(backdrop);
                    
                    // Show/hide wallpaper selection based on backdrop choice
                    if (backdrop == "Image")
                    {
                        if (WallpaperPanel != null)
                        {
                            Log.Debug("MainWindow - Showing wallpaper panel for Image backdrop");
                            ShowWallpaperGalleryWithAnimation();
                            
                            // Apply the default wallpaper immediately
                            var defaultWallpaperIndex = AppSettings.Instance.SelectedWallpaperIndex;
                            Log.Debug("MainWindow - Applying default wallpaper index: {WallpaperIndex}", defaultWallpaperIndex);
                            UpdateWallpaperSelection(defaultWallpaperIndex);
                            await ApplyWallpaperSettings(defaultWallpaperIndex.ToString());
                        }
                        else
                        {
                            Log.Error("WallpaperPanel is null when trying to show it");
                        }
                    }
                    else
                    {
                        if (WallpaperPanel != null)
                        {
                            Log.Debug("MainWindow - Hiding wallpaper panel for non-Image backdrop");
                            HideWallpaperGalleryWithAnimation();
                        }
                        else
                        {
                            Log.Error("WallpaperPanel is null when trying to hide it");
                        }
                    }
                    
                    // Backdrop changed notification removed
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MainWindow - Error in BackdropComboBox_SelectionChanged");
                Log.Error(ex, "Error in BackdropComboBox_SelectionChanged: {Message}", ex.Message);
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
                Log.Debug("MainWindow - WallpaperBorder_Tapped - wallpaper index: {WallpaperIndex}", wallpaperIndex);
                
                // Save preference
                if (int.TryParse(wallpaperIndex, out int index))
                {
                    Log.Debug("MainWindow - Saving SelectedWallpaperIndex: {Index}", index);
                    AppSettings.Instance.SelectedWallpaperIndex = index;
                    UpdateWallpaperSelection(index);
                }
                
                // Apply selected wallpaper
                Log.Debug("MainWindow - Applying wallpaper settings for index: {WallpaperIndex}", wallpaperIndex);
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
                Log.Error(ex, "Error updating wallpaper selection: {Message}", ex.Message);
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
                Log.Error(ex, "Error showing wallpaper gallery: {Message}", ex.Message);
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
                Log.Error(ex, "Error hiding wallpaper gallery: {Message}", ex.Message);
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
                Log.Error(ex, "Error animating wallpaper thumbnails: {Message}", ex.Message);
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
                 
                 Log.Information("Wallpaper applied with acrylic backdrop: {WallpaperPath}", wallpaperPath);
                 
                 // Wallpaper changed notification removed
             }
             catch (Exception ex)
             {
                 Log.Error(ex, "Failed to apply wallpaper {WallpaperIndex}: {Message}", wallpaperIndex, ex.Message);
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
                Log.Error(ex, "Error cleaning up wallpaper composition visual: {Message}", ex.Message);
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
                Log.Error(ex, "Error cleaning up wallpaper composition visual: {Message}", ex.Message);
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
                  
                  Log.Information("Backdrop changed to: {BackdropType}", backdropType);
              }
              catch (Exception ex)
              {
                  Log.Error(ex, "Failed to apply backdrop {BackdropType}: {Message}", backdropType, ex.Message);
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
                    Log.Information("Auto start monitoring {Status}", toggle.IsOn ? "enabled" : "disabled");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating auto start monitoring setting: {Message}", ex.Message);
            }
        }
        
        private void AutoRefreshToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is ToggleSwitch toggle)
                {
                    // AutoRefresh is now runtime-only, not saved to preferences
                    Log.Information("Auto refresh {Status} (runtime only)", toggle.IsOn ? "enabled" : "disabled");
                    
                    // Update the hardware monitor view model
                    if (_hardwareMonitorViewModel != null)
                    {
                        _hardwareMonitorViewModel.OnAutoRefreshChanged(toggle.IsOn);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating auto refresh setting: {Message}", ex.Message);
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
                    
                    Log.Information("Focused refresh interval set to {Value} seconds", value);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating focused refresh interval: {Message}", ex.Message);
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
                    
                    Log.Information("Focused refresh interval set to {Value} seconds", value);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating focused refresh interval: {Message}", ex.Message);
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
                    
                    Log.Information("Blurred refresh interval set to {Value} seconds", value);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating blurred refresh interval: {Message}", ex.Message);
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
                    
                    Log.Information("Blurred refresh interval set to {Value} seconds", value);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating blurred refresh interval: {Message}", ex.Message);
            }
        }

        // Update Check Functionality
        private async void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button)
                {
                    button.IsEnabled = false;
                    button.Content = "Checking...";
                }

                var updateInfo = await CheckForUpdates();
                
                if (updateInfo.HasUpdate)
                {
                    ShowUpdateAvailableDialog(updateInfo);
                }
                else
                {
                    ShowNoUpdateDialog();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking for updates: {Message}", ex.Message);
                ShowUpdateErrorDialog(ex.Message);
            }
            finally
            {
                if (sender is Button button)
                {
                    button.IsEnabled = true;
                    button.Content = "Check for Updates";
                }
            }
        }

        private async Task<UpdateInfo> CheckForUpdates()
        {
            try
            {
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "HP-Gaming-Hub");
                
                var response = await httpClient.GetStringAsync("https://api.github.com/repos/berkaytumal/HP-Gaming-Hub/releases/latest");
                var releaseInfo = System.Text.Json.JsonSerializer.Deserialize<GitHubRelease>(response);
                
                var currentVersion = GetCurrentVersion();
                var latestVersion = releaseInfo.tag_name.TrimStart('v');
                
                var hasUpdate = IsNewerVersion(latestVersion, currentVersion);
                
                return new UpdateInfo
                {
                    HasUpdate = hasUpdate,
                    LatestVersion = latestVersion,
                    CurrentVersion = currentVersion,
                    DownloadUrl = releaseInfo.assets?.FirstOrDefault(a => a.name.EndsWith(".msix"))?.browser_download_url,
                    ReleaseNotes = releaseInfo.body
                };
            }
            catch (System.Net.Http.HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                // No releases published yet, current version is the latest
                var currentVersion = GetCurrentVersion();
                return new UpdateInfo
                {
                    HasUpdate = false,
                    LatestVersion = currentVersion,
                    CurrentVersion = currentVersion,
                    DownloadUrl = null,
                    ReleaseNotes = "No releases published yet. You are running the development version."
                };
            }
        }

        private string GetCurrentVersion()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        private bool IsNewerVersion(string latestVersion, string currentVersion)
        {
            try
            {
                var latest = new Version(latestVersion);
                var current = new Version(currentVersion);
                return latest > current;
            }
            catch
            {
                return false;
            }
        }

        private async void ShowUpdateAvailableDialog(UpdateInfo updateInfo)
        {
            var dialog = new ContentDialog
            {
                Title = "Update Available",
                Content = $"A new version ({updateInfo.LatestVersion}) is available!\n\nCurrent version: {updateInfo.CurrentVersion}\n\nRelease Notes:\n{updateInfo.ReleaseNotes}",
                PrimaryButtonText = "Download",
                SecondaryButtonText = "Later",
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && !string.IsNullOrEmpty(updateInfo.DownloadUrl))
            {
                try
                {
                    var uri = new Uri(updateInfo.DownloadUrl);
                    await Windows.System.Launcher.LaunchUriAsync(uri);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error opening download URL: {Message}", ex.Message);
                }
            }
        }

        private async void ShowNoUpdateDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "No Updates Available",
                Content = "You are running the latest version of HP Gaming Hub.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async void ShowUpdateErrorDialog(string errorMessage)
        {
            var dialog = new ContentDialog
            {
                Title = "Update Check Failed",
                Content = $"Failed to check for updates:\n{errorMessage}",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        // Data classes for GitHub API response
        private class GitHubRelease
        {
            public string tag_name { get; set; }
            public string body { get; set; }
            public GitHubAsset[] assets { get; set; }
        }

        private class GitHubAsset
        {
            public string name { get; set; }
            public string browser_download_url { get; set; }
        }

        private class UpdateInfo
        {
            public bool HasUpdate { get; set; }
            public string LatestVersion { get; set; }
            public string CurrentVersion { get; set; }
            public string DownloadUrl { get; set; }
            public string ReleaseNotes { get; set; }
        }
    }
}