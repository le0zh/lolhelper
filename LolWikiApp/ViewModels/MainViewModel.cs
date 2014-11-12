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
        public object CachedObject { get; set; }

        private readonly HeroRepository _heroRepository;
        private readonly PlayerRepository _playerRepository;
        private readonly LocalFileRepository _localFileRepository;
        private readonly Dictionary<string, HeroDetail> _heroDetailsCache;

     
        public VideoDownloadService VideoDownloadService { get; private set; }

        public Player BindedPlayer { get; set; }
        public Player SelectedPlayer { get; set; }

        public readonly VideoRepository VideoRepository;

        public LocalFileRepository LocalFileHelper
        {
            get { return _localFileRepository; }
        }

        public string SelectedDetailGameInfoUrl { get; set; }

        public EquipmentRecommend EquipmentRecommendSelected { get; set; }
        

        public MainViewModel()
        {
            HeroBasicInfoCollection = new ObservableCollection<Hero>();

            _heroDetailsCache = new Dictionary<string, HeroDetail>();
            _heroRepository = new HeroRepository();
            _playerRepository = new PlayerRepository();
            _localFileRepository =new LocalFileRepository();

            BindedPlayer = GetPlayerInfoFromSettings();

            VideoRepository = new VideoRepository();
            VideoDownloadService =new VideoDownloadService();
        }

        #region 读取图片html模板

        private string _imageHtmlTemplateCache;
        public async Task<string> GetImageHtmlTemplate()
        {
            if (string.IsNullOrEmpty(_imageHtmlTemplateCache))
            {
                _imageHtmlTemplateCache = await HelperRepository.ReadImageHtmlTemplage();
            }

            if (Application.Current.GetTheme() == Theme.Light)
            {
                _imageHtmlTemplateCache = _imageHtmlTemplateCache.Replace("$theme$", "fff");
            }
            else
            {
                _imageHtmlTemplateCache = _imageHtmlTemplateCache.Replace("$theme$", "000");
            }

            return _imageHtmlTemplateCache;
        }
        #endregion

        #region 召唤师相关

        public async Task<CurrentGameInfo> GetCurentGameInfo()
        {
            if (BindedPlayer == null)
                return null;

            CurrentGameInfo currentGameInfo =
                await _playerRepository.GetCurrentGameInfoAsync(BindedPlayer.ServerInfo.Value, BindedPlayer.Name);

            return currentGameInfo;
        }

        public async Task<HttpActionResult> GetPlayerDetailInfo(string sn,string pn)
        {
            HttpActionResult result = await _playerRepository.PharsePlayerInfo(sn,pn);
            return result;
        }

        public Player GetPlayerInfoFromSettings()
        {
            return _playerRepository.ReadPlayerInfoSettings();
        }

        public void SavePlayerInfoToAppSettings(Player player)
        {
            if (player != null)
            {
                _playerRepository.SavePlayerInfo(player.Name, player.ServerInfo);
            }
        }

        public void RemovePlayerBind()
        {
            _playerRepository.RemovePlayerInfoFromSettings();
        }
        #endregion

        private string _dataVersion;
        /// <summary>
        /// Version for current Data source
        /// </summary>
        public string DataVersion
        {
            get { return String.IsNullOrEmpty(_dataVersion) ? "N/A" : _dataVersion; }
            private set
            {
                if (_dataVersion != value)
                {
                    _dataVersion = value;
                    NotifyPropertyChanged("DataVersion");
                }
            }
        }

        private string _dataLastUpdated;
        /// <summary>
        /// Display information for last updated time
        /// </summary>
        public string DataLastUpdated
        {
            get { return String.IsNullOrEmpty(_dataLastUpdated) ? "N/A" : _dataLastUpdated; ; }
            private set
            {
                if (_dataLastUpdated != value)
                {
                    _dataLastUpdated = value;
                    NotifyPropertyChanged("DataLastUpdated");
                }
            }
        }

        public ObservableCollection<Hero> HeroBasicInfoCollection { get; private set; }

        public async Task<HeroDetail> GetHeroDetailByKeyAsync(string id)
        {
            if (_heroDetailsCache.ContainsKey(id))
            {
                return _heroDetailsCache[id];
            }
            else
            {
                HeroDetail heroDetail = await _heroRepository.GetHeroDetailByKeyAshync(id);
                _heroDetailsCache.Add(id, heroDetail);
                return heroDetail;
            }
        }

        public async Task<List<HeroSkin>> LoadHeroSkinListAsync(string heroEnName)
        {
            return await _heroRepository.GetHeroSkinListAsync(heroEnName);
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public async Task<List<Hero>> LoadFreeHeroInfoListAsync(bool isForced)
        {
            List<Hero> heros = await _heroRepository.GetFreeHeroListlAsync(isForced);
            return heros;
        }

        public async Task<List<EquipmentRecommend>> LoadEquipmentRecommendList(string heroEnName)
        {
            List<EquipmentRecommend> list = await _heroRepository.GetEquipmentRecommendListAsync(heroEnName);
            return list;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public async void LoadHeroBaiscInfoDataAsync()
        {
            // Sample data; replace with real data
            if (HeroBasicInfoCollection.Count > 0)
                return;

            IList<Hero> herosList = await _heroRepository.GetAllHeroBasicInfosAsync();
            HeroBasicInfoCollection.Clear();

            foreach (var hero in herosList)
            {
                this.HeroBasicInfoCollection.Add(hero);
            }

            DataVersion = _heroRepository.DataVersion;
            _dataLastUpdated = _heroRepository.DataLastUpdated;

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