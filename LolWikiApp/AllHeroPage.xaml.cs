using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.Foundation.Metadata;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace LolWikiApp
{
    public partial class AllHeroPage : PhoneApplicationPage
    {
        private readonly string[] heroTags = { "All", "Fighter", "Mage", "Assassin", "Tank", "Marksman", "Support" };
        private WrapPanel[] heroPanels;

        public AllHeroPage()
        {
            InitializeComponent();

            SystemTray.ProgressIndicator = indicator;

            heroPanels = new WrapPanel[] { 
                this.allHeroWrapPanel, 
                this.fighterHeroWrapPanel, this.mageHeroWrapPanel, 
                this.assassinHeroWrapPanel, this.tankHeroWrapPanel, 
                this.marksmanHeroWrapPanel, this.supportHeroWrapPanel };
        }

        private void LoadHeroList(int index)
        {
            if (index < 0 || index > 6)
                return;

            if (heroPanels[index].Children.Count > 0)
                return;

            //SystemTray.ProgressIndicator.IsVisible = true;

            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadHeroBaiscInfoDataAsync();
            }

            Task t = new Task(() => this.Dispatcher.BeginInvoke(() =>
            {
                foreach (Hero hero in (
                                    index == 0 ? App.ViewModel.HeroBasicInfoCollection
                                               : App.ViewModel.HeroBasicInfoCollection.Where(h => h.Tags.Contains(heroTags[index]))))
                {
                    AddFreeHeroItem(hero, heroPanels[index]);
                }

                //SystemTray.ProgressIndicator.IsVisible = false;
            }));
            t.Start();

            //Debug.WriteLine("index:" + index.ToString());

            //foreach (Hero hero in
            //    (index == 0 ? App.ViewModel.HeroBasicInfoCollection
            //                : App.ViewModel.HeroBasicInfoCollection.Where(h => h.Tags.Contains(heroTags[index]))))
            //{
            //    AddFreeHeroItem(hero, heroPanels[index]);
            //}


        }


        private void AddFreeHeroItem(Hero hero, WrapPanel wrapPanel)
        {
            if (hero == null)
                return;

            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = System.Windows.Controls.Orientation.Vertical;
            stackPanel.Margin = new Thickness(0, 8, 20, 8);
            //stackPanel.Height = 76;
            //stackPanel.Width = 76;

            Image img = new Image();
            img.Source = new BitmapImage(new Uri(hero.ImageUrl, UriKind.Relative));
            img.Height = 90;
            img.Width = 90;
            img.Stretch = Stretch.UniformToFill;
            img.Tap += (s, e) => NavigationService.Navigate(new Uri("/HeroDetailsPage.xaml?selectedId=" + hero.Id, UriKind.Relative));

            TextBlock textBlock = new TextBlock();

            string tmpTitle = hero.Title.Length > 4 ? hero.Title.Substring(0, 4) + ".." : hero.Title;
            textBlock.Text = tmpTitle;

            stackPanel.Children.Add(img);
            stackPanel.Children.Add(textBlock);

            wrapPanel.Children.Add(stackPanel);            
        }

        private void HeroPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadHeroList(this.HeroPivot.SelectedIndex);
        }
    }
}