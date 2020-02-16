using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuaDan
{
    public class BettedItem
    {
        public DateTime BetTime;
        public string Race;
        public string Horse
        {
            get
            {
                return $"{Horse1}-{Horse2}";
            }
        }
        public string  Horse1;
        public string Horse2;
        public int DBetCount;
        public int TotalCount;
        public double Zhe;
        public int Lim;
        public PlayType PlayType;
        public BetType BetType;
        public double Odds;
        public bool Result;
        public string Reason;

        public BettedItem Clone()
        {
            return this.MemberwiseClone() as BettedItem;
        }

        public override string ToString()
        {

            return $"场:{Race}|马:{Horse}|票:{DBetCount}|折:{Zhe}|赔:{Odds}|{PlayType}|{BetType}";
        }
    }
}
