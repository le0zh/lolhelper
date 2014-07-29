using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolWikiApp
{
    public class HeroRankWrapper
    {
        public string ServerName { get; set; }
        public string PlayerName { get; set; }
        public string RankInfo { get; set; }
    }

    public class HeroSkin
    {
        public string BigImg { get; set; }

        public int Id { get; set; }

        public string Price { get; set; }

        public string Name { get; set; }

        public string SmallImg { get; set; }
    }


    public class Ability
    {
        public string AbilityFullName { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Cost { get; set; }

        public string CoolDown { get; set; }

        public string Description { get; set; }

        public string Range { get; set; }

        public string Effect { get; set; }

        public string ImageUrl
        {
            get { return string.Format("http://img.lolbox.duowan.com/abilities/{0}_64x64.png", AbilityFullName); }
        }
    }


    public class HeroDetail : INotifyPropertyChanged
    {
        public HeroDetail()
        {
            Abilities = new List<Ability>(5);
        }

        public int Id { get; set; }

        public string ImageUrl
        {
            get { return string.Format("/Data/Images/{0}/{1}.png", this.Name, this.Name); }
        }

        /// <summary>
        /// 英文名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 显示的名称，中文
        /// </summary>
        public string DisplayName { get; set; }

        public string Title { get; set; }

        private string tags;

        //TODO:貌似多玩json里这个属性不是非常完整，其中一个就是缺少“射手”，先这么workaround,考虑使用其他数据源合并
        public string Tags
        {
            get
            {
                if (string.IsNullOrEmpty(tags))
                    tags = "射手";
                return tags;
            }
            set
            {
                tags = value;
            }
        }

        public string Description { get; set; }

        public string DisplayDescription
        {
            get { return "    " + Description.Replace("\n", "\n    "); }
        }

        public string Quote { get; set; }

        public string QuoteAuthor { get; set; }

        /// <summary>
        /// 攻击距离
        /// </summary>
        public float Range { get; set; }

        /// <summary>
        /// 移动速度
        /// </summary>
        public float MoveSpeed { get; set; }

        public float ArmorBase { get; set; }

        private float armor;
        /// <summary>
        /// 基础防御
        /// </summary>
        public float Armor
        {
            get { return armor; }
            set
            {
                if (armor > value || armor < value)
                {
                    armor = value;
                    NotifyPropertyChanged("Armor");
                }
            }
        }

        /// <summary>
        /// 防御，成长
        /// </summary>
        public float ArmorLevel { get; set; }

        public float ManaBase { get; set; }

        private float mana;
        /// <summary>
        /// 基础魔法值
        /// </summary>
        public float Mana
        {
            get { return mana; }
            set
            {
                if (mana > value || mana < value)
                {
                    mana = value;
                    NotifyPropertyChanged("Mana");
                }
            }
        }

        /// <summary>
        /// 魔法值，成长
        /// </summary>
        public float ManaLevel { get; set; }

        public float CriticalChanceBase { get; set; }

        private float criticalChance;
        /// <summary>
        /// 暴击概率
        /// </summary>
        public float CriticalChance
        {
            get { return criticalChance; }
            set
            {
                if (criticalChance > value || criticalChance < value)
                {
                    criticalChance = value;
                    NotifyPropertyChanged("CriticalChance");
                }
            }
        }

        /// <summary>
        /// 暴击概率，成长
        /// </summary>
        public float CriticalChanceLevel { get; set; }

        public float ManaRegenBase { get; set; }

        private float manaRegen;

        /// <summary>
        /// 魔法回复
        /// </summary>
        public float ManaRegen
        {
            get { return manaRegen; }
            set
            {
                if (manaRegen > value || manaRegen < value)
                {
                    manaRegen = value;
                    NotifyPropertyChanged("ManaRegen");
                }
            }
        }

        /// <summary>
        /// 魔法回复，成长
        /// </summary>
        public float ManaRegenLevel { get; set; }

        public float HealthRegenBase { get; set; }

        private float healthRegen;

        /// <summary>
        /// 生命值回复
        /// </summary>
        public float HealthRegen
        {
            get { return healthRegen; }
            set
            {
                if (healthRegen > value || healthRegen < value)
                {
                    healthRegen = value;
                    NotifyPropertyChanged("HealthRegen");
                }
            }
        }

        /// <summary>
        /// 生命值回复，成长
        /// </summary>
        public float HealthRegenLevel { get; set; }

        public float MagicResistBase { get; set; }

        private float magicResist;
        /// <summary>
        /// 魔法抗性
        /// </summary>
        public float MagicResist
        {
            get { return magicResist; }
            set
            {
                if (magicResist > value || magicResist < value)
                {
                    magicResist = value;
                    NotifyPropertyChanged("MagicResist");
                }
            }
        }

        /// <summary>
        /// 魔法抗性，成长
        /// </summary>
        public float MagicResistLevel { get; set; }

        public float HealthBase { get; set; }

        private float health;
        /// <summary>
        /// 基础生命值
        /// </summary>
        public float Health
        {
            get { return health; }
            set
            {
                if (health > value || health < value)
                {
                    health = value;
                    NotifyPropertyChanged("Health");
                }
            }
        }

        /// <summary>
        /// 基础生命值，成长
        /// </summary>
        public float HealthLevel { get; set; }

        public float AttackBase { get; set; }

        private float attack;
        /// <summary>
        /// 基础攻击
        /// </summary>
        public float Attack
        {
            get { return attack; }
            set
            {
                if (attack > value || attack < value)
                {
                    attack = value;
                    NotifyPropertyChanged("Attack");
                }
            }
        }

        /// <summary>
        /// 基础攻击，成长
        /// </summary>
        public float AttackLevel { get; set; }

        /// <summary>
        /// 防御 评价
        /// </summary>
        public float RatingDefense { get; set; }

        /// <summary>
        /// 法术伤害 评价
        /// </summary>
        public float RatingMagic { get; set; }

        /// <summary>
        /// 上手难度 评价
        /// </summary>
        public float RatingDifficulty { get; set; }

        /// <summary>
        /// 物理攻击 评价
        /// </summary>
        public float RatingAttack { get; set; }

        /// <summary>
        /// 盟友小提示
        /// </summary>
        public string Tips { get; set; }

        /// <summary>
        /// 对手小提示
        /// </summary>
        public string OpponentTips { get; set; }

        /// <summary>
        /// 技能列表
        /// </summary>
        public List<Ability> Abilities { get; private set; }

        /// <summary>
        /// 价格
        /// </summary>
        public string Price { get; set; }

        public string DisplayPrice
        {
            get
            {
                if (Price.Contains(','))
                {
                    string[] prices = Price.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    return string.Format("金币：{0}, 点券：{1}", prices[0], prices[1]);
                }
                else
                {
                    return Price;
                }
            }
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

        public void SetLevel(int level)
        {
            this.Armor = this.ArmorBase + level * this.ArmorLevel;
            this.Attack = this.AttackBase + level * this.AttackLevel;
            this.CriticalChance = this.CriticalChanceBase + level * this.CriticalChanceLevel;
            this.Health = this.HealthBase + level * this.HealthLevel;
            this.HealthRegen = this.HealthRegenBase + level * this.HealthRegenLevel;
            this.MagicResist = this.MagicResistBase + level * this.MagicResistLevel;
            this.Mana = this.ManaBase + level * this.ManaLevel;
            this.ManaRegen = this.ManaRegenBase + level * this.ManaRegenLevel;
        }
    }

}