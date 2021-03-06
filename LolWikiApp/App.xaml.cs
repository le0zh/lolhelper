﻿using System;
using System.Diagnostics;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using LolWikiApp.Repository;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using LolWikiApp.Resources;
using LolWikiApp.ViewModels;
using Microsoft.Phone.Notification;
using Microsoft.WindowsAzure.Messaging;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace LolWikiApp
{
    public partial class App : Application
    {
        private static MainViewModel _viewModel = null;
        private static NewsViewModel _newsViewModel = null;

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                return _viewModel ?? (_viewModel = new MainViewModel());
            }
        }

        public static NewsViewModel NewsViewModel
        {
            get { return _newsViewModel ?? (_newsViewModel = new NewsViewModel()); }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            UmengSDK.UmengAnalytics.Init("54128299fd98c59d9800a356");
#if DEBUG
            UmengSDK.UmengAnalytics.IsDebug = true;//是否输出调试信息
#endif

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            OverwriteSystemColor();
        }

        private static void OverwriteSystemColor()
        {
            ((SolidColorBrush)Application.Current.Resources["PhoneBackgroundBrush"]).Color = Colors.White;
            ((SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"]).Color = Colors.Black;
            //PhoneSubtleBrush
            //PhoneBorderBrush
            ((SolidColorBrush)Application.Current.Resources["PhoneSubtleBrush"]).Color = Colors.DarkGray;
            ((SolidColorBrush)Application.Current.Resources["PhoneBorderBrush"]).Color = Colors.DarkGray;


            ((SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"]).Color = Colors.Red;

            //PhoneTextLowContrastBrush
            if (Current.GetTheme() == Theme.Light)
            {
                ((SolidColorBrush)Application.Current.Resources["PhoneTextLowContrastBrush"]).Color = Colors.Black;
            }
            else if (Current.GetTheme() == Theme.Dark)
            {
                ((SolidColorBrush)Application.Current.Resources["PhoneTextLowContrastBrush"]).Color = Colors.White;
            }
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            Debug.WriteLine("Application_Launching");
            HelperRepository.CopyContentToIsolatedStorage("Data/html/common.css");
            HelperRepository.CopyContentToIsolatedStorage("Data/html/global_black.css");
            HelperRepository.CopyContentToIsolatedStorage("Data/html/global_light.css");
            HelperRepository.CopyContentToIsolatedStorage("Data/html/reset.css");

            //todo: disable notification for now.
            //var channel = HttpNotificationChannel.Find("MyLolHelperPushChannel");
            //if (channel == null)
            //{
            //    channel = new HttpNotificationChannel("MyLolHelperPushChannel");
            //    channel.Open();
            //    channel.BindToShellToast();
            //}
            //channel.ChannelUriUpdated += async (o, args) =>
            //{
            //    var hub = new NotificationHub("lolhelper", "Endpoint=sb://le0zhhub-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=ljIlC7gsVKyj8Z/HzNj3CHYeFjIdgVNZ20S23i1fDdw=");
            //    //TODO:CHECK THIS
            //    //for test:
            //    //var hub = new NotificationHub("hubtest", "Endpoint=sb://le0zhhub-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=jOb95tZ003QgDpkX3KIdgUA1SlAZOWj9XF9WmBXqkd0=");
            //    await hub.RegisterNativeAsync(args.ChannelUri.ToString());
            //}; 
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            Debug.WriteLine("Application_Activated");
            foreach (var request in ViewModel.VideoDownloadService.Requests)
            {
                Debug.WriteLine(request.TransferStatus);
                Debug.WriteLine(request.IsDownloading);
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
            //var task = Task.Run(() => ViewModel.VideoDownloadService.SaveCacheInfoListToIso());
            //task.Wait();

            var task = Task.Run(() => ViewModel.VideoDownloadService.PauseAll());
            task.Wait();

            Debug.WriteLine("Application_Deactivated");

            foreach (var request in ViewModel.VideoDownloadService.Requests)
            {
                Debug.WriteLine(request.TransferStatus);
                Debug.WriteLine(request.PercentDisplay);
            }
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {

        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            //TODO:handle the unhandled exception
            MessageBox.Show(e.ExceptionObject.Message);

            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            //RootFrame = new PhoneApplicationFrame();
            RootFrame = new TransitionFrame();
            RootFrame.Background = new SolidColorBrush(Colors.White);
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }

        //private void TeamMemberGrid_OnTap(object sender, GestureEventArgs e)
        //{
        //    var grid = sender as Grid;
        //    if (grid != null)
        //    {
        //        var animationHelper = new AnimatonHelper();
                
        //        if (Math.Abs(grid.Height - 190) < 0.1)
        //        {
        //            //hide
        //            animationHelper.RunShowStoryboard(grid, AnimationTypes.TeamMemberDetailInfoHide, TimeSpan.FromSeconds(0), null);
        //        }
        //        else
        //        {
        //            animationHelper.RunShowStoryboard(grid, AnimationTypes.SwivelForwardIn, TimeSpan.FromSeconds(0), null);
        //            //show
        //            animationHelper.RunShowStoryboard(grid, AnimationTypes.TeamMemberDetailInfoShow, TimeSpan.FromSeconds(0), null);
        //        }
        //    }
        //}

        //private void UIElement_OnTap(object sender, GestureEventArgs e)
        //{
        //    var image = sender as Image;
        //    if (image != null)
        //    {
        //        var name = image.Tag.ToString();
        //        Debug.WriteLine("team member name: " + name);
        //        e.Handled = true;
        //        //App.ViewModel.SelectedDetailGameServer;
        //        //NavigationService(new Uri("/GameDetailPage.xaml", UriKind.Relative));
        //    }
        //}
    }
}