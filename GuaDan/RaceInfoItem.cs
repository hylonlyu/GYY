using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuaDan
{
    public enum PlayType { WP = 1, FORECAST, QP, Q };
    //public enum PlayType { Q=1, QP };
    public enum BetType { EAT = 1, BET };
    [Serializable]
    public class RaceInfoItem
    {
        private string country;
        private string location;
        private string race;
        private string date;
        private string url;
        private string uid;
        private string oddstype;
        private string odds;
        private string horse;
        private double win;
        private double place;
        private double zhe;
        private int lWin;
        private int lPlace;
        private string live;

        //玩法
        private PlayType playtype;
        private BetType bettype;
        private string classType;
        public string Country
        {
            get
            {
                return country;
            }

            set
            {
                country = value;
            }
        }

        public string Location
        {
            get
            {
                return location;
            }

            set
            {
                location = value;
            }
        }

        public string Race
        {
            get
            {
                return race;
            }

            set
            {
                race = value;
            }
        }

        public string Date
        {
            get
            {
                return date;
            }

            set
            {
                date = value;
            }
        }

        public string Url
        {
            get
            {
                return url;
            }

            set
            {
                url = value;
            }
        }

        public string Uid
        {
            get
            {
                return uid;
            }

            set
            {
                uid = value;
            }
        }

        /// <summary>
        /// 赔率
        /// </summary>
        public string Odds
        {
            get
            {
                return odds;
            }

            set
            {
                odds = value;
            }
        }

        /// <summary>
        /// 赔率类型
        /// </summary>
        public string OddsType
        {
            get
            {
                return oddstype;
            }

            set
            {
                oddstype = value;
            }
        }

        public string Horse
        {
            get
            {
                return horse;
            }

            set
            {
                horse = value;
            }
        }

        public double Win
        {
            get
            {
                return win - (int)win == 0 ? win : Math.Round(win, 2);
            }

            set
            {
                win = value;
            }
        }

        public double Place
        {
            get
            {
                return place - (int)place == 0 ? place : Math.Round(place, 2);
            }

            set
            {
                place = value;
            }
        }

        public double Zhe
        {
            get
            {
                return zhe;
            }

            set
            {
                zhe = value;
            }
        }
        /// <summary>
        /// win极限
        /// </summary>
        public int LWin
        {
            get
            {
                return lWin;
            }

            set
            {
                lWin = value;
            }
        }
        /// <summary>
        /// P极限
        /// </summary>
        public int LPlace
        {
            get
            {
                return lPlace;
            }

            set
            {
                lPlace = value;
            }
        }

        public PlayType Playtype
        {
            get
            {
                return playtype;
            }

            set
            {
                playtype = value;
            }
        }

        public BetType Bettype
        {
            get
            {
                return bettype;
            }

            set
            {
                bettype = value;
            }
        }

        /// <summary>
        /// 类式
        /// </summary>
        public string ClassType
        {
            get
            {
                return classType;
            }

            set
            {
                classType = value;
            }
        }

        public string Live
        {
            get
            {
                return live;
            }

            set
            {
                live = value;
            }
        }

        public override string ToString()
        {
            return $"{Country}-{Location}-{OddsType}-{Url}#{Date}#{Race}-{Horse}-{Win}-{Place}-{Zhe}-{lWin}-{LPlace}-{Bettype}-{Playtype}-{ClassType}-{Live}";

        }

        public RaceInfoItem Clone()
        {
            return this.MemberwiseClone() as RaceInfoItem;
        }

        /// <summary>
        ///  string key = $"{item.Url}|{item.Race}|{item.Horse}|{item.Playtype}|{item.Bettype}";
        /// </summary>
        /// <returns></returns>
        public string GetKey()
        {
            string key = $"{Url}|{Race}|{Horse}|{Playtype}|{Bettype}";
            return key;
        }
    }

    [Serializable]
    public class RaceInfoEnity
    {
        private string snakeHead;
        private Dictionary<string, RaceInfoItem> dicRaceInfo;
        public RaceInfoEnity()
        {
            dicRaceInfo = new Dictionary<string, RaceInfoItem>();
        }
        /// <summary>
        /// 蛇头账号名
        /// </summary>
        public string SnakeHead
        {
            get
            {
                return snakeHead;
            }

            set
            {
                snakeHead = value;
            }
        }

        public Dictionary<string, RaceInfoItem> DicRaceInfo
        {
            get
            {
                return dicRaceInfo;
            }

            set
            {
                dicRaceInfo = value;
            }
        }
    }
}
