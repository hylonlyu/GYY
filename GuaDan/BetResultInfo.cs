using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuaDan
{
    enum BetResultType { FULL, PARTIAL, ACCEPT_NO_MATCH, FAIL };
    public class BetResultInfo
    {
        private string strUrl = "";

        public string StrUrl
        {
            get { return strUrl; }
            set { strUrl = value; }
        }
        private string strRefer = "";

        public string StrRefer
        {
            get { return strRefer; }
            set { strRefer = value; }
        }
        private string strCookie = "";

        public string StrCookie
        {
            get { return strCookie; }
            set { strCookie = value; }
        }
        private string strAnswer = "";

        public string StrAnswer
        {
            get { return strAnswer; }
            set { strAnswer = value; }
        }
        private string strTimeoutExc = "";

        public string StrTimeoutExc
        {
            get { return strTimeoutExc; }
            set { strTimeoutExc = value; }
        }
        private string strExc = "";

        public string StrExc
        {
            get { return strExc; }
            set { strExc = value; }
        }

        private double dBetCount = 0;

        public double DBetCount
        {
            get { return dBetCount; }
            set { dBetCount = value; }
        }

        private string strBetString = "";

        public string StrBetString
        {
            get { return strBetString; }
            set { strBetString = value; }
        }
        //private int intSiteId = -1;

        //public int IntSiteId
        //{
        //    get { return intSiteId; }
        //    set { intSiteId = value; }
        //}

        private BetItem objBetItem = new BetItem();

        internal BetItem ObjBetItem
        {
            get { return objBetItem; }
            set { objBetItem = value; }
        }

        private BetResultType enuBetResultType;

        internal BetResultType EnuBetResultType
        {
            get { return enuBetResultType; }
            set { enuBetResultType = value; }
        }
        public override string ToString()
        {
            string strRet = "";
            strRet += "******************************************************************";
            //strRet += string.Format("SiteId:{0}\r\n", Util.GetSiteById(IntSiteId));
            strRet += string.Format("Url:{0}\r\n", StrUrl);
            strRet += string.Format("Refer:{0}\r\n", StrRefer);
            strRet += string.Format("Cookie:{0}\r\n", strCookie);
            strRet += string.Format("Answer:{0}\r\n", StrAnswer);
            strRet += string.Format("TimeoutExc:{0}\r\n", strTimeoutExc);
            strRet += string.Format("Exc:{0}\r\n", StrExc);
            strRet += string.Format("Count:{0}\r\n", DBetCount);
            strRet += string.Format("BetString:{0}\r\n", StrBetString);
            strRet += string.Format("BetResultType:{0}\r\n", EnuBetResultType);

            strRet += "=================================================================";

            strRet += string.Format("Race:{0}\r\n", objBetItem.StrRace);
            strRet += string.Format("Horse:{0}\r\n", objBetItem.StrHorse);
            strRet += string.Format("Win:{0}\r\n", objBetItem.StrWin);
            strRet += string.Format("Place:{0}\r\n", objBetItem.StrPlace);
            strRet += string.Format("Discount:{0}\r\n", objBetItem.StrDiscount);
            strRet += string.Format("L_win:{0}\r\n", objBetItem.StrL_win);
            strRet += string.Format("L_place:{0}\r\n", objBetItem.StrL_place);
            strRet += string.Format("BetType:{0}\r\n", objBetItem.EnuBetType);
            strRet += "******************************************************************";
            return strRet;

        }
    }
    public class BetItem
    {
        private string strRace;

        public string StrRace
        {
            get { return strRace; }
            set { strRace = value; }
        }
        private string strHorse;

        public string StrHorse
        {
            get { return strHorse; }
            set { strHorse = value; }
        }
        private string strWin;

        public string StrWin
        {
            get { return strWin; }
            set { strWin = value; }
        }
        private string strPlace;

        public string StrPlace
        {
            get { return strPlace; }
            set { strPlace = value; }
        }
        private string strDiscount;

        public string StrDiscount
        {
            get { return strDiscount; }
            set { strDiscount = value; }
        }
        private string strL_win;

        public string StrL_win
        {
            get { return strL_win; }
            set { strL_win = value; }
        }
        private string strL_place;

        public string StrL_place
        {
            get { return strL_place; }
            set { strL_place = value; }
        }
        private BetType enuBetType;

        internal BetType EnuBetType
        {
            get { return enuBetType; }
            set { enuBetType = value; }
        }
        public override string ToString()
        {
            string str = "";
            str = string.Format("{0}_{1}_{2}_{3}_{4}_{5}/{6}", StrRace, StrHorse, StrWin, StrPlace, StrDiscount, StrL_win, StrL_place);
            return str;
        }

    }
}
