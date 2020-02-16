using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuaDan
{
    [Serializable]
    public class AppConfig
    {
        public string MatchCombol;
        public string MatchUrl;
        public string SiteUrl;
        public string Race;
        public string Accout;
        public string Pwd;
        public string Pin;


    }

    [Serializable]
    public class ZdConfig:AppConfig
    {
#region 早餐

        /// <summary>
        /// 开跑时间
        /// </summary>
        public int KPSJ;
        /// <summary>
        /// 挂单赔率
        /// </summary>
        public double GDPL;

        /// <summary>
        /// 读单间隔
        /// </summary>
        public int DDJG;

        /// <summary>
        ///亏损金额
        /// </summary>
        public int KSJE;

        /// <summary>
        /// 挂单折扣
        /// </summary>
        public double GDJK;

        /// <summary>
        /// 极限比例
        /// </summary>
        public double JXBL;
        #endregion
        #region 走地
        /// <summary>
        /// 票数比例
        /// </summary>
        public double PSBL;
        /// <summary>
        /// 删除时间
        /// </summary>
        public int SCSJ;

        /// <summary>
        /// 最高极限
        /// </summary>
        public int ZGJX;
        #endregion

    }

    public class RCConfig
    {
        public static double QEatZhe = 80;
        public static int QEatLim = 700;
        public static double QBetZhe = 100;
        public static int QBetLim = 700;

        public static double QPEatZhe = 80;
        public static int QPEatLim = 400;
        public static double QPBetZhe = 100;
        public static int QPBetLim = 400;
    }
}
