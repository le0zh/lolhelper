using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace LolWikiApp
{
    public partial class NewsDetailPage : PhoneApplicationPage
    {
        private NewsDetail newsDetail;
        public NewsDetailPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DataContext == null)
            {
                string artId;
                if (NavigationContext.QueryString.TryGetValue("newsId", out artId))
                {
                    newsDetail = await App.NewsViewModel.GetNewsDetailAsync(artId);
                    DataContext = newsDetail;

                    //this.ContentWebBrowser.NavigateToString(newsDetail.Content);
                    //RenderNews
                    RenderNewsContent(newsDetail.Content);
                    this.LoadingBar.Visibility = Visibility.Collapsed;
                    this.NewsContentPanel.Visibility = Visibility.Visible;
                }
            }

            base.OnNavigatedTo(e);
        }

        private void RenderNewsContent(string content)
        {
            Regex videoDivRegex = new Regex("(?<=<div class=\"u-videoimg\">)[\\s\\S]+?(?=</div>)");
            Regex paragraphRegex = new Regex("(?<=<p)[\\s\\S]+?(?=</p>)");
            //Regex paragraphRegex = new Regex("<p[\\s\\S]+(</p>)");
            Regex h1Regex = new Regex("(?<=<h1>)[\\s\\S]+?(?=</h1>)");
            Regex headerRegex = new Regex("(?<=<header>)[\\s\\S]+?(?=</header>)");
            Regex articleRegex = new Regex("(?<=<article>)[\\s\\S]+?(?=</article>)");

            Match headerMatch = headerRegex.Match(content);

            //Title and Publish time
            this.TitleTextBlock.Text = h1Regex.Match(headerMatch.Value).ToString();
            this.PubtimeTextBlock.Text = paragraphRegex.Match(headerMatch.Value).ToString().Replace(">", string.Empty);
            
            string articleContent = articleRegex.Match(content).Value;

            //VideoDiv if here is any
            Match videoDivMatch = videoDivRegex.Match(articleContent);
            if (videoDivMatch.Success)
            {
                RenderVideoNews(articleContent, videoDivMatch);
                return; //no need to render for now.             
            }

            MatchCollection mc = paragraphRegex.Matches(articleContent);

            List<string> paragraphList = (from Match m in mc select m.Value).ToList();

            foreach (string pargraph in paragraphList)
            {
                this.MainContentPanel.Children.Add(RenderParagraph(pargraph));
            }
        }

        private void RenderVideoNews(string content, Match videoDivMatch)
        {
            Regex imageRegex = new Regex("(?<=src=\")[\\s\\S]+?(?=\")");
            Match videoImageMatch = imageRegex.Match(videoDivMatch.Value);
            if (videoImageMatch.Success)
            {
                //TODO: This is a Video News, add handle to watch video
                string imageUrl = videoImageMatch.Value;
                Image image = new Image();
                image.MouseLeftButtonUp += image_MouseLeftButtonUp;
                image.Source = new BitmapImage(new Uri(imageUrl));

                this.MainContentPanel.Children.Add(image);
            }

            Regex paragraphWithoutStyleRegex = new Regex("(?<=<p>)[\\s\\S]+?(?=</p>)");
            MatchCollection paragraphWithoutStyleMatchSMatchCollection = paragraphWithoutStyleRegex.Matches(content);

            foreach (Match match in paragraphWithoutStyleMatchSMatchCollection)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = "    " + HtmlDecode(match.Value.Trim());
                textBlock.TextWrapping = TextWrapping.Wrap;
                textBlock.Margin = new Thickness(0, 8, 0, 8);
                this.MainContentPanel.Children.Add(textBlock);
            }
        }

        async void image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //handle the image click to launch video player
            if (newsDetail.Video.Count > 0)
            {
                string vu = newsDetail.Video[0]["vu"];
                if (!string.IsNullOrEmpty(vu))
                {
                    string videoUrl = await App.NewsViewModel.GetVideoUrlInNewsAsync(vu);
                    
                    MediaPlayerLauncher player = new MediaPlayerLauncher();
                    player.Media = new Uri(videoUrl);
                    player.Show();
                }
            }
        }

        private UIElement RenderParagraph(string paragraph)
        {
            UIElement uiElement = new TextBlock();

            Regex imageRegex = new Regex("(?<=src=\")[\\s\\S]+?(?=\")");

            Regex boldRegex = new Regex("<b>[\\s\\S]+</b>");

            //Only text in this paragraph, just return a textblock.
            if (paragraph.StartsWith(">"))
            {
                TextBlock textBlock = new TextBlock();
                //string text = paragraph.Remove(0, 1).Replace("<span>", string.Empty).Replace("</span>", string.Empty).Trim();
                string text = HtmlDecode(paragraph.Remove(0, 1)).Trim();

                //Strong style
                if (boldRegex.Match(paragraph).Success)
                {
                    text = text.Replace("<b>", string.Empty).Replace("</b>", string.Empty);
                    textBlock.FontWeight = FontWeights.Bold;
                    textBlock.Margin = new Thickness(0, 8, 0, 8);
                }

                textBlock.Text = "    " + text;
                textBlock.TextWrapping = TextWrapping.Wrap;
                uiElement = textBlock;
            }

            //Paragraph contains IMAGE, need to create Image control
            if (paragraph.StartsWith(" style") && !paragraph.Contains("font-family"))
            {
                Match imageMatch = imageRegex.Match(paragraph);
                if (imageMatch.Success)
                {
                    //Got image
                    string imageUrl = imageMatch.Value; 
                    Image image = new Image();
                    image.Source = new BitmapImage(new Uri(imageUrl));
                    image.Margin = new Thickness(12, 12, 12, 12);

                    if (paragraph.Contains("text-align:left"))
                    {
                        image.Stretch = Stretch.None;
                        image.HorizontalAlignment = HorizontalAlignment.Left;
                        image.Margin = new Thickness(24, 8, 12, 8);
                    }
                    else
                    {
                        image.Stretch = Stretch.Uniform;
                        image.HorizontalAlignment = HorizontalAlignment.Center;
                    }

                    image.MaxHeight = 640;
                    uiElement = image;
                }
                else
                {
                    if (paragraph.IndexOf('>') != -1)
                    {
                        paragraph = paragraph.Remove(0, paragraph.IndexOf('>') + 1);
                    }
                    string description = HtmlDecode(paragraph);
                    TextBlock textBlock = new TextBlock();
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.Text = description;
                    uiElement = textBlock;
                }
            }

            return uiElement;
        }

        private string HtmlDecode(string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            Regex htmlTagRegx = new Regex("(?<=<[^b][\\s\\S]+)[\\s\\S]+?(?=</[\\s\\S]+>)");
            MatchCollection mc = htmlTagRegx.Matches(content);
            foreach (Match m in mc)
            {

                content = Regex.Replace(content, "<[^b][\\s\\S]+[\\s\\S]+</[\\s\\S]+>", m.Value);
            }

            content = content.Replace("&ldquo;", "“");
            content = content.Replace("&rdquo;", "”");

            content = content.Replace("&lsquo;", "‘");
            content = content.Replace("&rsquo;", "’");

            content = content.Replace("&bull;", "•");

            content = content.Replace("&hellip;", "…");

            content = content.Replace("&lsaquo;", "<");
            content = content.Replace("&rsaquo;", ">");

            content = content.Replace("&tilde;", "˜");

            content = content.Replace("&nbsp;", " ");
            content = content.Replace("&mdash;", "——");
            content = content.Replace("&ndash;", "-");

            return content;
        }

        private void ShowVidwoButton_OnClick(object sender, RoutedEventArgs e)
        {
            //test
            LetvSourceConverter converter = new LetvSourceConverter();
            const string decodedString = "aHR0cDovL2czLmxldHYuY24vdm9kL3YyL05qVXZNVFV2TmpFdlltTnNiM1ZrTHpFd01UY3hNUzkyWlhKZk1EQmZNVFF0TWpFNU5EZzRPRFl0WVhaakxURXlPVGc1TkMxaFlXTXRNekl3TURFdE1qVTVNak16TXkwMU5USTNNVE0xTlMxbU9UZG1NamMxTnpFMFpXVXdNbU13WTJWaU9ERTBOakpsTVdFNVlUa3hOUzB4TkRBeU1qYzRNVE01TXpreUxtMXdOQT09P2I9MTcwJm1tc2lkPTIxMDE0MDMxJnRtPTE0MDIzNzMyMjMma2V5PWMxOTJiYmI5YWFiNmI3OGNkZjZiNmRiNzBiYmQ1ZjljJnBsYXRpZD0yJnNwbGF0aWQ9MjAzJnBsYXlpZD0wJnRzcz1ubyZ2dHlwZT05JmN2aWQ9OTcyOTI3NzgwNDU0JnRhZz1tb2JpbGUmYmNsb3VkPVM3JnNpZ249YmNsb3VkXzEwMTcxMSZ0ZXJtaWQ9MiZwYXk9MCZvc3R5cGU9YW5kcm9pZCZod3R5cGU9dW4=";
            string url = converter.Decode(decodedString);

            MediaPlayerLauncher player = new MediaPlayerLauncher();
            player.Media = new Uri(url);
            player.Location = MediaLocationType.Data;
            player.Show();
        }
    }
}