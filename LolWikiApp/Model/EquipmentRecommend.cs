using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using LolWikiApp.Repository;

namespace LolWikiApp
{
    public class SkillImageWrapper
    {
        /// <summary>
        /// 技能图标的地址
        /// </summary>
        public string SkillImageUrl { get; set; }

        /// <summary>
        /// 技能快捷键的图标地址
        /// </summary>
        public string SkillShortImageUrl { get; set; }
    }

    /// <summary>
    /// 英雄推荐出装信息类
    /// </summary>
    public class EquipmentRecommend
    {
        public int Record_id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Skill { get; set; }

        public string Pre_cz { get; set; }

        public List<SkillImageWrapper> PreSkillImageUrls
        {
            get
            {
                return ParseSkillImageUrls().Take(6).ToList();
            }
        }

        public List<string> PreEquipmentImageUrls
        {
            get { return ParseEquipmentImageUrls(Pre_cz); }
        }

        public string Pre_explain { get; set; }

        public string Mid_cz { get; set; }

        public List<string> MidEquipmentImageUrls
        {
            get { return ParseEquipmentImageUrls(Mid_cz); }
        }

        public List<SkillImageWrapper> MidSkillImageUrls
        {
            get { return ParseSkillImageUrls().Skip(6).Take(6).ToList(); }
        }

        public string Mid_explain { get; set; }

        public string End_cz { get; set; }

        public List<string> EndEquipmentImageUrls
        {
            get { return ParseEquipmentImageUrls(End_cz); }
        }

        public List<SkillImageWrapper> EndSkillImageUrls
        {
            get { return ParseSkillImageUrls().Skip(12).ToList(); }
        }

        public string End_explain { get; set; }

        public string Nf_cz { get; set; }

        public List<string> NfEquipmentImageUrls
        {
            get { return ParseEquipmentImageUrls(Nf_cz); }
        }

        public string Nf_explain { get; set; }

        public int Cost { get; set; }

        public string Game_type { get; set; }

        public string User_name { get; set; }

        public string ServerName
        {
            get { return ServerRepository.Instance.GetServerDisplayName(Server); }
        }

        public string Server { get; set; }

        public int Combat { get; set; }

        public int Good { get; set; }

        public int Bad { get; set; }

        public string Time { get; set; }

        public string En_name { get; set; }

        public string ImageUrl
        {
            get { return string.Format("/Data/Images/{0}/{1}.png", this.En_name, this.En_name); }
        }

        public string Ch_name { get; set; }

        public int Cost_nf { get; set; }

        public string Ni_name { get; set; }

        public string Tags { get; set; }

        public string Sc { get; set; }

        private List<SkillImageWrapper> skillWrapperImgUrlList;

        private List<SkillImageWrapper> ParseSkillImageUrls()
        {
            if (skillWrapperImgUrlList == null || skillWrapperImgUrlList.Count == 0)
            {
                skillWrapperImgUrlList = new List<SkillImageWrapper>();

                if (string.IsNullOrEmpty(Skill))
                    return skillWrapperImgUrlList;

                const string skillImgUrl = "http://img.lolbox.duowan.com/abilities/{0}_{1}_64x64.png";
                string[] skillShortcuts = Skill.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);

                foreach (string shortcut in skillShortcuts)
                {
                    SkillImageWrapper wrapper = new SkillImageWrapper()
                    {
                        SkillShortImageUrl = string.Format("/Assets/{0}.png", shortcut),
                        SkillImageUrl = string.Format(skillImgUrl, En_name, shortcut)
                    };
                    skillWrapperImgUrlList.Add(wrapper);
                }
            }

            return skillWrapperImgUrlList;
        }


        private List<string> ParseEquipmentImageUrls(string cz)
        {
            List<string> equpimentImgUrlList = new List<string>();

            if (string.IsNullOrEmpty(cz))
                return equpimentImgUrlList;


            const string equpimentImgUrl = "http://img.lolbox.duowan.com/zb/{0}_64x64.png";
            string[] equipmentIds = cz.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            equpimentImgUrlList.AddRange(equipmentIds.Select(id => string.Format(equpimentImgUrl, id)));

            return equpimentImgUrlList;
        }
    }
}
