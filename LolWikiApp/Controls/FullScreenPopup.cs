using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace LolWikiApp
{
    public class FullScreenPopup
    {
        private bool _wasApplicationBarVisible;
        private Popup _hostPopup;
        private Border _outerBorder;
        private PhoneApplicationPage _hostPage;
        private bool _isCancelled;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public UIElement Child
        {
            get { return _outerBorder.Child; }
            set { _outerBorder.Child = value; }
        }

        public event EventHandler PopupCancelled;

        public event EventHandler PopUpHided = delegate { };

        public FullScreenPopup()
        {
            _outerBorder = new Border
            {
                Background = (SolidColorBrush)Application.Current.Resources["PhoneSemitransparentBrush"]
            };

            _outerBorder.Tap += (s, e) => Hide();

            _hostPopup = new Popup {Child = _outerBorder};
        }

        public void Show()
        {
            //PrepareAppForFullScreen();
            SetOrientation();
            SystemTray.BackgroundColor = Color.FromArgb(255, 14, 13, 16);
            _hostPopup.IsOpen = true;
        }

        public void Hide()
        {
            _hostPopup.IsOpen = false;
            SystemTray.BackgroundColor = Color.FromArgb(255, 41, 40, 46);
            PopUpHided(this, EventArgs.Empty);
        }

        public bool IsOpen
        {
            get { return _hostPopup.IsOpen; }
        }

        private void PrepareAppForFullScreen()
        {
            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            _hostPage = null;

            if (frame != null)
            {
                _hostPage = frame.Content as PhoneApplicationPage;
            }

            //if (_hostPage != null && _hostPage.ApplicationBar != null && _hostPage.ApplicationBar.IsVisible)
            //{
            //    _wasApplicationBarVisible = true;
            //    _hostPage.ApplicationBar.IsVisible = false;
            //}

            if (frame != null)
            {
                //frame.BackKeyPress += OnBackKeyPress;
                frame.Navigating += OnFrameNavigating;
                frame.OrientationChanged += OnOrientationChanged;
            }
        }

        private void RestoreSettings()
        {
            var frame = Application.Current.RootVisual as PhoneApplicationFrame;

            if (_wasApplicationBarVisible && _hostPage != null && _hostPage.ApplicationBar != null)
            {
                _hostPage.ApplicationBar.IsVisible = true;
            }

            if (frame != null)
            {
                //frame.BackKeyPress -= OnBackKeyPress;
                frame.Navigating -= OnFrameNavigating;
                frame.OrientationChanged -= OnOrientationChanged;
            }
        }

        private void OnBackKeyPress(object sender, CancelEventArgs args)
        {
            CancelPopup();
            args.Cancel = true;
        }

        private void OnFrameNavigating(object sender, NavigatingCancelEventArgs args)
        {
            CancelPopup();
        }

        private void OnOrientationChanged(object sender, OrientationChangedEventArgs args)
        {
            SetOrientation();
        }

        private void CancelPopup()
        {
            if (_isCancelled)
            {
                return;
            }

            _isCancelled = true;

            EventHandler handler = PopupCancelled;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
            else
            {
                Hide();
            }
        }

        private void SetOrientation()
        {
            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                PageOrientation orientation = frame.Orientation;
                Transform transform = null;
                double frameWidth = Application.Current.Host.Content.ActualWidth;
                double frameHeight = Application.Current.Host.Content.ActualHeight;
                double width = frameWidth;
                double height = frameHeight;
                switch (orientation)
                {
                    case PageOrientation.Landscape:
                    case PageOrientation.LandscapeLeft:
                        transform = new CompositeTransform { Rotation = 90, TranslateX = frameWidth };
                        width = frameHeight;
                        height = frameWidth;
                        break;
                    case PageOrientation.LandscapeRight:
                        transform = new CompositeTransform { Rotation = -90, TranslateY = frameHeight };
                        width = frameHeight;
                        height = frameWidth;
                        break;
                }
                _outerBorder.RenderTransform = transform;
                _outerBorder.Width = width;
                _outerBorder.Height = height;
            }
        }
    }
}
