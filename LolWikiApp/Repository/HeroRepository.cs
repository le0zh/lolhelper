﻿using System;
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
        private const string EquipmentRecommendRequestUrl = "http://db.duowan.com/lolcz/img/ku11/api/lolcz.php?limit=7&championName={0}"; //读取英雄出装列表，参数为英雄的英文名称
        private const string FreeHeroListRequestUrl = "http://lolbox.duowan.com/phone/apiHeroes.php?v=25&type=free"; //每周免费英雄列表请求地址

        private const string SkinListRequestUrl = "http://box.dwstatic.com/apiHeroSkin.php?hero={0}"; //英雄皮肤列表请求地址

        /// <summary>
        /// 获取推荐出装列表
        /// </summary>
        /// <param name="heroEnName"></param>
        /// <returns></returns>
        public async Task<List<EquipmentRecommend>> GetEquipmentRecommendListAsync(string heroEnName)
        {
            var url = string.Format(EquipmentRecommendRequestUrl, heroEnName);
            var json = await GetJsonStringViaHttpAsync(url);

            var equipmentRecommendList = JsonConvert.DeserializeObject<List<EquipmentRecommend>>(json);
            return equipmentRecommendList;
        }

        /// <summary>
        /// 获取英雄皮肤列表
        /// </summary>
        /// <param name="heroEnName"></param>
        /// <returns></returns>
        public async Task<List<HeroSkin>> GetHeroSkinListAsync(string heroEnName)
        {
            var url = string.Format(SkinListRequestUrl, heroEnName);
            var json = await GetJsonStringViaHttpAsync(url);

            var equipmentRecommendList = JsonConvert.DeserializeObject<List<HeroSkin>>(json);
            return equipmentRecommendList;
        }

        private const string FreeHeroCacheKey = "_free_hero_cache";

        public FreeHeroCache GetFreeHeroCache()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(FreeHeroCacheKey))
            {
                return null;
            }

            return IsolatedStorageSettings.ApplicationSettings[FreeHeroCacheKey] as FreeHeroCache;
        }

        public void SaveFreeHeroCache(FreeHeroCache cache)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains(FreeHeroCacheKey))
            {
                settings[FreeHeroCacheKey] = cache;
            }
            else
            {
                settings.Add(FreeHeroCacheKey, cache);
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
                    if ((DateTime.Now - cache.LastUpdated).Days > 7
                            || (DateTime.Now.DayOfWeek == DayOfWeek.Friday
                                    && DateTime.Now.Hour >= 10
                                    && cache.LastUpdated.DayOfWeek != DayOfWeek.Friday))
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

            var json = await GetJsonStringViaHttpAsync(FreeHeroListRequestUrl);
            Debug.WriteLine("free hero HTTP requested.");

            var jObject = JObject.Parse(json);
            var freeHeroInfos = JsonConvert.DeserializeObject<List<FreeHeroInfo>>(jObject["free"].ToString());

            var heroes = freeHeroInfos.Select(freeHeroInfo => new Hero()
            {
                Id = freeHeroInfo.EnName,
                Title = freeHeroInfo.Title,
                Name = freeHeroInfo.CnName
            }).ToList();

            //update cache
            var neweCache = new FreeHeroCache { LastUpdated = DateTime.Now, Cache = heroes };

            SaveFreeHeroCache(neweCache);

            return heroes;
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
            var heroJsonStorageFile = await GetStorageFileFromInstalledDataFolderAsync("heros", id + ".json");
            var json = await ReadJsonFileAsync(heroJsonStorageFile);

            var heroDetail = JsonConvert.DeserializeObject<HeroDetail>(json);
            var heroAbilitiesKeys = new List<string>
            {
                id + "_B", 
                id + "_Q", 
                id + "_W", 
                id + "_E", 
                id + "_R"
            };

            var i = 0;
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
            var championJsonStorageFile = await GetStorageFileFromInstalledDataFolderAsync("champion.json");
            var json = await ReadJsonFileAsync(championJsonStorageFile);

            var heros = new List<Hero>();

            //Read the key/id dictionary to get the full list of Hero, then use these ids to get HeroInfo from data[id]
            var jobj = JObject.Parse(json);

            //read data version
            DataVersion = jobj["version"].ToString();

            //read last updated time
            DataLastUpdated = jobj["updated"].ToString();

            //read hero key-id dictionary
            Dictionary<string, string> heroDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jobj["keys"].ToString());

            foreach (var id in heroDictionary.Values)
            {
                var hero = JsonConvert.DeserializeObject<Hero>(jobj["data"][id].ToString());
                heros.Add(hero);
            }

            return heros;
        }
    }
}
