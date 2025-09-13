using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HP_Gaming_Hub
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private int _previousSelectedIndex = 3; // Default to Monitor page (index 3)
        
        public MainWindow()
        {
            this.InitializeComponent();
            // Extend content into title bar and set custom title bar
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }

        private void MainNavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
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
    }
}
