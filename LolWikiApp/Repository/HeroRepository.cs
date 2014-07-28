using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.Phone.Controls.Primitives;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LolWikiApp.Repository
{
    public class HeroRepository : Repository
    {
        private const string equipmentRecommendRequestUrl = "http://db.duowan.com/lolcz/img/ku11/api/lolcz.php?limit=7&championName={0}"; //读取英雄出装列表，参数为英雄的英文名称
        private const string freeHeroListRequestUrl = "http://lolbox.duowan.com/phone/apiHeroes.php?v=25&type=free"; //每周免费英雄列表请求地址

        private const string skinListRequestUrl =
            "http://box.dwstatic.com/apiHeroSkin.php?hero={0}"; //英雄皮肤列表请求地址

        /// <summary>
        /// 获取推荐出装列表
        /// </summary>
        /// <param name="heroEnName"></param>
        /// <returns></returns>
        public async Task<List<EquipmentRecommend>> GetEquipmentRecommendListAsync(string heroEnName)
        {
            string url = string.Format(equipmentRecommendRequestUrl, heroEnName);
            string json = await GetJsonStringViaHTTPAsync(url);

            List<EquipmentRecommend> equipmentRecommendList = JsonConvert.DeserializeObject<List<EquipmentRecommend>>(json);
            return equipmentRecommendList;
        }

        /// <summary>
        /// 获取英雄皮肤列表
        /// </summary>
        /// <param name="heroEnName"></param>
        /// <returns></returns>
        public async Task<List<HeroSkin>> GetHeroSkinListAsync(string heroEnName)
        {
            string url = string.Format(skinListRequestUrl, heroEnName);
            string json = await GetJsonStringViaHTTPAsync(url);

            List<HeroSkin> equipmentRecommendList = JsonConvert.DeserializeObject<List<HeroSkin>>(json);
            return equipmentRecommendList;
        }

        private const string freeHeroCacheKey = "_free_hero_cache";

        public FreeHeroCache GetFreeHeroCache()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(freeHeroCacheKey))
            {
                return null;
            }

            return IsolatedStorageSettings.ApplicationSettings[freeHeroCacheKey] as FreeHeroCache;

        }

        public void SaveFreeHeroCache(FreeHeroCache cache)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            
            if (settings.Contains(freeHeroCacheKey))
            {
                settings[freeHeroCacheKey] = cache;
            }
            else
            {
                settings.Add(freeHeroCacheKey, cache);
            }

            settings.Save();
        }

        /// <summary>
        /// 获取免费英雄列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<Hero>> GetFreeHeroListlAsync(bool forceRefresh = false)
        {
            #region 增加缓存判断
            if (!forceRefresh)
            {
                FreeHeroCache cache = GetFreeHeroCache();

                if (cache != null)
                {
                    if ( (DateTime.Now - cache.LastUpdated).Days > 7 
                            || ( DateTime.Now.DayOfWeek == DayOfWeek.Friday
                                    && DateTime.Now.Hour >= 10
                                    && cache.LastUpdated.DayOfWeek!=DayOfWeek.Friday) )
                    {
                        //need update
                    }
                    else
                    {
                        return cache.Cache;
                    }
                }
            }
            #endregion


            string json = await GetJsonStringViaHTTPAsync(freeHeroListRequestUrl);
            Debug.WriteLine("free hero HTTP requested.");

            JObject jObject = JObject.Parse(json);
            List<FreeHeroInfo> freeHeroInfos = JsonConvert.DeserializeObject<List<FreeHeroInfo>>(jObject["free"].ToString());

            List<Hero> heroes = freeHeroInfos.Select(freeHeroInfo => new Hero()
            {
                Id = freeHeroInfo.EnName,
                Title = freeHeroInfo.Title,
                Name = freeHeroInfo.CnName
            }).ToList();

            //update cache
            FreeHeroCache neweCache = new FreeHeroCache();
            neweCache.LastUpdated = DateTime.Now;
            neweCache.Cache = heroes;
            
            SaveFreeHeroCache(neweCache);

            return heroes;
        }

        /// <summary>
        /// Read a specified StorageFile in async way
        /// </summary>
        /// <param name="jsonFile">StorageFile</param>
        /// <returns></returns>
        private async Task<string> ReadJsonFileAsync(IRandomAccessStreamReference jsonFile)
        {
            string json;
            using (IRandomAccessStreamWithContentType readStream = await jsonFile.OpenReadAsync())
            using (StreamReader sr = new StreamReader(readStream.AsStream()))
            {
                json = await sr.ReadToEndAsync();
            }

            return json;
        }

        /// <summary>
        /// GetStorageFileFromInstalledDataFolderAsync
        /// </summary>
        /// <param name="parts">Path parts to be combined</param>
        /// <returns></returns>
        private async Task<StorageFile> GetStorageFileFromInstalledDataFolderAsync(params string[] parts)
        {
            string path = parts.Aggregate("ms-appx:///Data/", (current, part) => current + (part + "/"));
            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            StorageFile storageFile;
            try
            {
                storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(path));
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message + "\n" + path);
                throw;
            }

            return storageFile;
        }

        public string DataVersion { get; private set; }
        public string DataLastUpdated { get; private set; }

        /// <summary>
        /// 通过英雄的英文名称，获取英雄详细信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<HeroDetail> GetHeroDetailByKeyAshync(string id)
        {
            StorageFile heroJsonStorageFile = await GetStorageFileFromInstalledDataFolderAsync("heros", id + ".json");
            string json = await ReadJsonFileAsync(heroJsonStorageFile);

            HeroDetail heroDetail = JsonConvert.DeserializeObject<HeroDetail>(json);
            List<string> heroAbilitiesKeys = new List<string>();
            heroAbilitiesKeys.Add(id + "_B");
            heroAbilitiesKeys.Add(id + "_Q");
            heroAbilitiesKeys.Add(id + "_W");
            heroAbilitiesKeys.Add(id + "_E");
            heroAbilitiesKeys.Add(id + "_R");
            int i = 0;
            string[] abilitiesSuffix = { "（被动）", "（Q）", "（W）", "（E）", "（R）" };

            JObject jobj = JObject.Parse(json);
            foreach (string abilitiesKey in heroAbilitiesKeys)
            {
                Ability ability = JsonConvert.DeserializeObject<Ability>(jobj[abilitiesKey].ToString());
                ability.AbilityFullName = abilitiesKey;
                ability.Name += abilitiesSuffix[i++];

                if (string.IsNullOrEmpty(ability.Cost))
                    ability.Cost = "无";
                if (string.IsNullOrEmpty(ability.CoolDown))
                    ability.CoolDown = "无";
                if (string.IsNullOrEmpty(ability.Range))
                    ability.Range = "无";
                if (string.IsNullOrEmpty(ability.Cost))
                    ability.Cost = "无";
                if (string.IsNullOrEmpty(ability.Effect))
                    ability.Effect = "无";

                ability.Effect = ability.Effect.Replace("\n\n", "\n        ");

                heroDetail.Abilities.Add(ability);
            }

            return heroDetail;
        }

        public async Task<IList<Hero>> GetAllHeroBasicInfosAsync()
        {
            StorageFile championJsonStorageFile = await GetStorageFileFromInstalledDataFolderAsync("champion.json");
            string json = await ReadJsonFileAsync(championJsonStorageFile);

            List<Hero> heros = new List<Hero>();

            //Read the key/id dictionary to get the full list of Hero, then use these ids to get HeroInfo from data[id]
            JObject jobj = JObject.Parse(json);

            //read data version
            DataVersion = jobj["version"].ToString();

            //read last updated time
            DataLastUpdated = jobj["updated"].ToString();

            //read hero key-id dictionary
            Dictionary<string, string> heroDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jobj["keys"].ToString());

            foreach (var id in heroDictionary.Values)
            {
                Hero hero = JsonConvert.DeserializeObject<Hero>(jobj["data"][id].ToString());
                heros.Add(hero);
            }

            return heros;
        }


    }
}
