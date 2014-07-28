using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HtmlAgilityPack;
using LolWikiApp.Repository;
using LolWikiApp.Resources;
using Microsoft.Phone.Controls.Primitives;

namespace LolWikiApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {

        private readonly HeroRepository heroRepository;
        private readonly PlayerRepository playerRepository;
        private readonly LocalFileRepository localFileRepository;
        private readonly Dictionary<string, HeroDetail> HeroDetailsCache;

        public Player BindedPlayer { get; set; }
        public Player SelectedPlayer { get; set; }

        public LocalFileRepository LocalFileHelper
        {
            get { return localFileRepository; }
        }

        public string SelectedDetailGameInfoUrl { get; set; }

        public EquipmentRecommend EquipmentRecommendSelected { get; set; }

        public MainViewModel()
        {
            this.HeroBasicInfoCollection = new ObservableCollection<Hero>();

            HeroDetailsCache = new Dictionary<string, HeroDetail>();
            heroRepository = new HeroRepository();
            playerRepository = new PlayerRepository();
            localFileRepository =new LocalFileRepository();

            BindedPlayer = GetPlayerInfoFromSettings();
        }

        #region 读取图片html模板

        private string imageHtmlTemplateCache;
        public async Task<string> GetImageHtmlTemplate()
        {
            if (string.IsNullOrEmpty(imageHtmlTemplateCache))
            {
                imageHtmlTemplateCache = await HelperRepository.ReadImageHtmlTemplage();
            }

            if (Application.Current.GetTheme() == Theme.Light)
            {
                imageHtmlTemplateCache = imageHtmlTemplateCache.Replace("$theme$", "fff");
            }
            else
            {
                imageHtmlTemplateCache = imageHtmlTemplateCache.Replace("$theme$", "000");
            }

            return imageHtmlTemplateCache;
        }
        #endregion

        #region 召唤师相关

        public async Task<CurrentGameInfo> GetCurentGameInfo()
        {
            if (BindedPlayer == null)
                return null;

            CurrentGameInfo currentGameInfo =
                await playerRepository.GetCurrentGameInfoAsync(BindedPlayer.ServerInfo.Value, BindedPlayer.Name);

            return currentGameInfo;
        }

        public async Task<HttpActionResult> GetPlayerDetailInfo(string sn,string pn)
        {
            HttpActionResult result = await playerRepository.PharsePlayerInfo(sn,pn);
            return result;
        }

        public Player GetPlayerInfoFromSettings()
        {
            return playerRepository.ReadPlayerInfoSettings();
        }

        public void SavePlayerInfoToAppSettings(Player player)
        {
            if (player != null)
            {
                playerRepository.SavePlayerInfo(player.Name, player.ServerInfo);
            }
        }

        public void RemovePlayerBind()
        {
            playerRepository.RemovePlayerInfoFromSettings();
        }
        #endregion

        private string dataVersion;
        /// <summary>
        /// Version for current Data source
        /// </summary>
        public string DataVersion
        {
            get { return String.IsNullOrEmpty(dataVersion) ? "N/A" : dataVersion; }
            private set
            {
                if (dataVersion != value)
                {
                    dataVersion = value;
                    NotifyPropertyChanged("DataVersion");
                }
            }
        }

        private string dataLastUpdated;
        /// <summary>
        /// Display information for last updated time
        /// </summary>
        public string DataLastUpdated
        {
            get { return String.IsNullOrEmpty(dataLastUpdated) ? "N/A" : dataLastUpdated; ; }
            private set
            {
                if (dataLastUpdated != value)
                {
                    dataLastUpdated = value;
                    NotifyPropertyChanged("DataLastUpdated");
                }
            }
        }

        public ObservableCollection<Hero> HeroBasicInfoCollection { get; private set; }

        public async Task<HeroDetail> GetHeroDetailByKeyAsync(string id)
        {
            if (HeroDetailsCache.ContainsKey(id))
            {
                return HeroDetailsCache[id];
            }
            else
            {
                HeroDetail heroDetail = await heroRepository.GetHeroDetailByKeyAshync(id);
                HeroDetailsCache.Add(id, heroDetail);
                return heroDetail;
            }
        }

        public async Task<List<HeroSkin>> LoadHeroSkinListAsync(string heroEnName)
        {
            return await heroRepository.GetHeroSkinListAsync(heroEnName);
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public async Task<List<Hero>> LoadFreeHeroInfoListAsync(bool isForced)
        {
            List<Hero> heros = await heroRepository.GetFreeHeroListlAsync(isForced);
            return heros;
        }

        public async Task<List<EquipmentRecommend>> LoadEquipmentRecommendList(string heroEnName)
        {
            List<EquipmentRecommend> list = await heroRepository.GetEquipmentRecommendListAsync(heroEnName);
            return list;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public async Task LoadHeroBaiscInfoDataAsync()
        {
            // Sample data; replace with real data
            if (HeroBasicInfoCollection.Count > 0)
                return;

            IList<Hero> herosList = await heroRepository.GetAllHeroBasicInfosAsync();
            HeroBasicInfoCollection.Clear();

            foreach (var hero in herosList)
            {
                this.HeroBasicInfoCollection.Add(hero);
            }

            DataVersion = heroRepository.DataVersion;
            dataLastUpdated = heroRepository.DataLastUpdated;

            this.IsDataLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}