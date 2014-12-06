using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        public bool IsBindedPlayerInfoDataLoaded
        {
            get { return BindedPlayerInfoWrappers.All(info => info.IsDataLoaded); }
        }
        public List<PlayerInfoSettingWrapper> BindedPlayerInfoWrappers { get; private set; } 
        public ObservableCollection<Player> BindedPlayers { get; set; }

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

            BindedPlayerInfoWrappers = GetPlayerInfoFromSettings();
            BindedPlayers = new ObservableCollection<Player>();

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

        //todo: change here
        public async Task<CurrentGameInfo> GetCurentGameInfo()
        {
            if (BindedPlayers == null)
                return null;

            CurrentGameInfo currentGameInfo =
                await _playerRepository.GetCurrentGameInfoAsync(BindedPlayers[0].ServerInfo.Value, BindedPlayers[0].Name);

            return currentGameInfo;
        }

        public async Task<HttpActionResult> GetPlayerDetailInfo(string sn,string pn)
        {
            var result = await _playerRepository.PharsePlayerInfo(sn,pn);//sn: server name, pn: player name
            return result;
        }

        public List<PlayerInfoSettingWrapper> GetPlayerInfoFromSettings()
        {
            return _playerRepository.ReadPlayerInfoSettings();
        }

        public void AddBindedPlayer(Player player)
        {
            if (player != null)
            {
                if (App.ViewModel.BindedPlayerInfoWrappers == null)
                {
                    App.ViewModel.BindedPlayerInfoWrappers = new List<PlayerInfoSettingWrapper>();
                }

                if (!App.ViewModel.BindedPlayerInfoWrappers.Any(
                        p => p.Name == player.Name && p.ServerInfo.DisplayName == player.ServerInfo.DisplayName))
                {
                    App.ViewModel.BindedPlayerInfoWrappers.Add(new PlayerInfoSettingWrapper() { IsDataLoaded = true, Name = player.Name, ServerInfo = player.ServerInfo });
                    App.ViewModel.BindedPlayers.Add(player);
                    SavePlayerListToSettings();
                }
            }
        }

        public void DeleteBindedPlayer(Player player)
        {
            App.ViewModel.BindedPlayers.Remove(player);

            PlayerInfoSettingWrapper foundWrapper = null;

            foreach (var playerInfoSettingWrapper in App.ViewModel.BindedPlayerInfoWrappers)
            {
                if (playerInfoSettingWrapper.Name == player.Name &&
                    playerInfoSettingWrapper.ServerInfo.DisplayName == player.ServerInfo.DisplayName)
                {
                    foundWrapper = playerInfoSettingWrapper;
                }
            }

            if (foundWrapper != null)
            {
                App.ViewModel.BindedPlayerInfoWrappers.Remove(foundWrapper);
                App.ViewModel.SavePlayerListToSettings();
            }
        }

        public void SavePlayerListToSettings()
        {
            _playerRepository.SavePlayerInfo(App.ViewModel.BindedPlayerInfoWrappers);
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
        public async Task LoadHeroBaiscInfoDataAsync()
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