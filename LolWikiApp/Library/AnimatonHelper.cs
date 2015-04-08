using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LolWikiApp
{
    public enum AnimationTypes
    {
        Flash,
        FadeIn,
        FadeOut,
        SlideUp,
        SlideDown,
        SlideLeftFadeIn,
        SwivelForwardIn,
        TurnstileForwardIn,
        SlideLeftOutFade,
        SlideRightOutFade,
        TeamMemberDetailInfoShow,
        TeamMemberDetailInfoHide,
    }

    public class AnimatonHelper
    {
        private const string SlideUpStoryboard = @"
        <Storyboard  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(TranslateTransform.Y)"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.5"" Value=""-90"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""8""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>           
        </Storyboard>";

        private const string SlideDownStoryboard = @"
        <Storyboard  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(TranslateTransform.Y)"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""-90""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.3"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""8""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>            
        </Storyboard>";

        private const string SlideLeftFadeInStoryBoard = @"
<Storyboard
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(TranslateTransform.X)"">
        <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
        <EasingDoubleKeyFrame KeyTime=""0:0:0.5"" Value=""0"">            
        </EasingDoubleKeyFrame>
    </DoubleAnimationUsingKeyFrames>
    <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"">
        <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
        <EasingDoubleKeyFrame KeyTime=""0:0:0.5"" Value=""1""/>
    </DoubleAnimationUsingKeyFrames>
</Storyboard>
    ";

        private const string SwivelForwardInStoryBoard = @"<Storyboard
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                    <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.RotationX)"">
                        <EasingDoubleKeyFrame KeyTime=""0"" Value=""60""/>
                        <EasingDoubleKeyFrame KeyTime=""0:0:0.6"" Value=""0"">            
                        </EasingDoubleKeyFrame>
                    </DoubleAnimationUsingKeyFrames>     
                </Storyboard>";

        private const string TurnstileForwardInStoryBoard = @"<Storyboard
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.RotationY)"">
        <EasingDoubleKeyFrame KeyTime=""0"" Value=""-80""/>
        <EasingDoubleKeyFrame KeyTime=""0:0:0.35"" Value=""0"">
            <EasingDoubleKeyFrame.EasingFunction>
                <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
            </EasingDoubleKeyFrame.EasingFunction>
        </EasingDoubleKeyFrame>
    </DoubleAnimationUsingKeyFrames>
    <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"">
        <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
        <EasingDoubleKeyFrame KeyTime=""0:0:0.01"" Value=""1""/>
    </DoubleAnimationUsingKeyFrames>
    <ObjectAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Visibility)"">
        <DiscreteObjectKeyFrame KeyTime=""0"">
            <DiscreteObjectKeyFrame.Value>
                <Visibility>Visible</Visibility>
            </DiscreteObjectKeyFrame.Value>
        </DiscreteObjectKeyFrame>
    </ObjectAnimationUsingKeyFrames>
</Storyboard>";

        private const string SlideLeftOutFadeStoryBoard = @"<Storyboard
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                    <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(TranslateTransform.X)"">
                        <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                        <EasingDoubleKeyFrame KeyTime=""0:0:0.3"" Value=""-200"">
                            <EasingDoubleKeyFrame.EasingFunction>
                                <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                            </EasingDoubleKeyFrame.EasingFunction>
                        </EasingDoubleKeyFrame>
                    </DoubleAnimationUsingKeyFrames>
                    <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"">
                        <EasingDoubleKeyFrame KeyTime=""0"" Value=""1""/>
                        <EasingDoubleKeyFrame KeyTime=""0:0:0.29"" Value=""1""/>
                        <EasingDoubleKeyFrame KeyTime=""0:0:0.3"" Value=""0""/>
                    </DoubleAnimationUsingKeyFrames>
                </Storyboard>";

        private const string SlideRightOutFadeStoryBoard = @"<Storyboard
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                    <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(TranslateTransform.X)"">
                        <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                        <EasingDoubleKeyFrame KeyTime=""0:0:0.4"" Value=""400"">
                            <EasingDoubleKeyFrame.EasingFunction>
                                <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                            </EasingDoubleKeyFrame.EasingFunction>
                        </EasingDoubleKeyFrame>
                    </DoubleAnimationUsingKeyFrames>
                    <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"">
                        <EasingDoubleKeyFrame KeyTime=""0"" Value=""1""/>
                        <EasingDoubleKeyFrame KeyTime=""0:0:0.3"" Value=""1""/>
                        <EasingDoubleKeyFrame KeyTime=""0:0:0.4"" Value=""0""/>
                    </DoubleAnimationUsingKeyFrames>
                </Storyboard>";

        private const string FadeInStoryBoard = @"<Storyboard
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                     <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"">
                        <EasingDoubleKeyFrame KeyTime=""0"" Value=""&VAL&""/>
                        <EasingDoubleKeyFrame KeyTime=""0:0:0.5"" Value=""1""/>
                    </DoubleAnimationUsingKeyFrames>
                </Storyboard>";

        private const string FadeOutStoryBoard = @"<Storyboard
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                     <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"">
                        <EasingDoubleKeyFrame KeyTime=""0"" Value=""1""/>
                        <EasingDoubleKeyFrame KeyTime=""0:0:0.5"" Value=""&VAL&""/>
                    </DoubleAnimationUsingKeyFrames>
                </Storyboard>";

        private const string FlashStoryBoard = @"<Storyboard
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                     <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"">
                        <EasingDoubleKeyFrame KeyTime=""0"" Value=""1""/>
                        <EasingDoubleKeyFrame KeyTime=""0:0:0.35"" Value=""0""/>
                        <EasingDoubleKeyFrame KeyTime=""0:0:0.5"" Value=""1""/>
                    </DoubleAnimationUsingKeyFrames>
                </Storyboard>";

        private const string TeamMemberDetailInfoShowStoryBoard = @"<Storyboard
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                     <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Height)"">
                        <EasingDoubleKeyFrame KeyTime=""90"" Value=""1""/>
                        <EasingDoubleKeyFrame KeyTime=""0:0:0.25"" Value=""190""/>
                    </DoubleAnimationUsingKeyFrames>
                </Storyboard>";

        private const string TeamMemberDetailInfoHideStoryBoard = @"<Storyboard
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                     <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Height)"">
                        <EasingDoubleKeyFrame KeyTime=""0"" Value=""190""/>
                        <EasingDoubleKeyFrame KeyTime=""0:0:0.25"" Value=""90""/>                        
                    </DoubleAnimationUsingKeyFrames>
                </Storyboard>";

        public void RunShowStoryboard(UIElement grid, AnimationTypes animation, TimeSpan delay, EventHandler sbCompleted = null, string origianVal = "0")
        {
            if (grid == null)
                return;

            Storyboard storyboard;
           

            switch (animation)
            {
                case AnimationTypes.Flash:
                    storyboard = XamlReader.Load(FlashStoryBoard) as Storyboard;
                    break;
                case AnimationTypes.SlideUp:
                    storyboard = XamlReader.Load(SlideUpStoryboard) as Storyboard;
                    grid.RenderTransform = new TranslateTransform() { Y = 0 };
                    break;
                case AnimationTypes.SlideDown:
                    storyboard = XamlReader.Load(SlideDownStoryboard) as Storyboard;
                    grid.RenderTransform = new TranslateTransform() { Y = -90 };
                    break;
                case AnimationTypes.SlideLeftFadeIn:
                    storyboard = XamlReader.Load(SlideLeftFadeInStoryBoard) as Storyboard;
                    break;
                case AnimationTypes.TurnstileForwardIn:
                    storyboard = XamlReader.Load(TurnstileForwardInStoryBoard) as Storyboard;
                    grid.Projection = new PlaneProjection();
                    break;
                case AnimationTypes.SwivelForwardIn:
                    storyboard = XamlReader.Load(SwivelForwardInStoryBoard) as Storyboard;
                    grid.Projection = new PlaneProjection() { RotationX = 90 };
                    break;
                case AnimationTypes.SlideLeftOutFade:
                    storyboard = XamlReader.Load(SlideLeftOutFadeStoryBoard) as Storyboard;
                    break;
                case AnimationTypes.SlideRightOutFade:
                    storyboard = XamlReader.Load(SlideRightOutFadeStoryBoard) as Storyboard;
                    break;
                case AnimationTypes.FadeIn:
                    storyboard = XamlReader.Load(FadeInStoryBoard.Replace("&VAL&", origianVal)) as Storyboard;
                    break;
                case AnimationTypes.FadeOut:
                    storyboard = XamlReader.Load(FadeOutStoryBoard.Replace("&VAL&", origianVal)) as Storyboard;
                    grid.Opacity = 1;
                    break;
                case AnimationTypes.TeamMemberDetailInfoShow:
                    storyboard = XamlReader.Load(TeamMemberDetailInfoShowStoryBoard.Replace("&VAL&", origianVal)) as Storyboard;
                    break;
                case AnimationTypes.TeamMemberDetailInfoHide:
                    storyboard = XamlReader.Load(TeamMemberDetailInfoHideStoryBoard.Replace("&VAL&", origianVal)) as Storyboard;
                    break;
                default:
                    storyboard = XamlReader.Load(SlideLeftOutFadeStoryBoard) as Storyboard;
                    break;
            }

            if (storyboard != null)
            {
                foreach (var storyboardAnimation in storyboard.Children)
                {
                    if (!(storyboardAnimation is DoubleAnimationUsingKeyFrames))
                        continue;

                    var doubleKey = storyboardAnimation as DoubleAnimationUsingKeyFrames;

                    foreach (var frame in doubleKey.KeyFrames)
                    {
                        frame.KeyTime = KeyTime.FromTimeSpan(frame.KeyTime.TimeSpan.Add(delay));
                    }
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    foreach (var t in storyboard.Children)
                        Storyboard.SetTarget(t, grid);

                    storyboard.Begin();
                    if (sbCompleted != null)
                    {
                        storyboard.Completed += sbCompleted;
                    }
                });
            }
        }
    }
}
