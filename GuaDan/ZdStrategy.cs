using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GuaDan
{
    public class ZdStrategy:CCMember
    {

        private Thread WorkThread;
        private Dictionary<string, int> dicBetWinPiao = new Dictionary<string, int>();
        private HashSet<string> hsEatWin = new HashSet<string>();
        private HashSet<string> hsBetFail = new HashSet<string>();
        //RaceInfoEnity oldEnity = null;
        Dictionary<string, RaceInfoEnity> dicRaceinfo = new Dictionary<string, RaceInfoEnity>();
        public override void Start()
        {
            if (WorkThread != null && WorkThread.IsAlive == true)
            {
                WorkThread.Abort();
            }
            WorkThread = new Thread(DoWork);
            WorkThread.IsBackground = true;
            WorkThread.Start();
            base.Start();
        }

        public override void Stop()
        {
            if (WorkThread != null && WorkThread.IsAlive == true)
            {
                WorkThread.Abort();
            }
            base.Stop();
        }

        private void DoWork()
        {
            while (true)
            {
                MatchTimeInfo tInfo = GetRaceLastTime(Config.MatchUrl, Config.Race);
                System.Diagnostics.Debug.WriteLine($"GetRaceLastTime-{Config.MatchUrl}-{Config.Race}");
                if(tInfo.Stage == MatchStage.BreadFast && tInfo.LastTime<= Config.KPSJ)
                {
                    DoBreadFast();
                   
                }
                if(tInfo.Stage == MatchStage.Running)
                {
                    //临近开场，删除没有成交的挂单
                    if (tInfo.LastTime < Config.SCSJ)
                    {
                        DelNotBetTickets();
                    }
                }
                DoRunning();
                System.Threading.Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 删除没有成交的走地挂单
        /// </summary>
        private void DelNotBetTickets()
        {
            GetBetInfo(out string betinfo);
            bool bRet = DeleteAllLiveEatGuaDan(betinfo);
            if(bRet)
            {
                SendShowMsgEvent("删除吃挂单成功");
            }
        }
        private void DoBreadFast()
        {
            //设置自动挂w赌条件：当赛事距离开跑时间小于等于设置时间t秒，w挂单赔率小于等于设置的w赔率wr，
            //设置亏损金额m（实际下单票数(此数值要是5的倍数) = m / 此马的实际赔率，设置读取赔率时间间隔ts，
            //当赔率赔率下降了，需要按m的实际金额补足，如果赔率上升就不需要变动），
            //设置极限的增加比例p %（w实际下单极限 = 此马的实际赔率 *（1 + p）*10，此值要取偶数）
            Dictionary<string, WPOdds> dicData = GetWPPeiData(new RaceInfoItem
            { Date = GetNow(), Url = Config.MatchUrl, Race = Config.Race });
            foreach (var item in dicData)
            {
                string match = Config.MatchUrl;
                string race = Config.Race;
                string horse = item.Key;
                string betKey = $"{Config.MatchUrl}-{Config.Race}-{horse}";
                int jine = (int)(Config.KSJE / item.Value.Win);
                jine = Util.Closeto5(jine);
                if(!hsBetFail.Contains(betKey))
                {
                    double win = item.Value.Win;
                    if (win != 0 && win <= Config.GDPL)
                    {
                        //判断这只马是否还可以打单.如果已经打过，并且需要打的金额没有增加(赔率没有变小)，则不需要再打这只马
                        if (dicBetWinPiao.ContainsKey(betKey))
                        {
                            if (jine <= dicBetWinPiao[betKey])
                            {
                                continue;
                            }
                            else
                            {
                                //赔率变小了，需要补票
                                jine = jine - dicBetWinPiao[betKey];
                                jine = Util.Closeto5(jine);
                                //如果金额过小，就不需要打单据了
                                if (jine <= 5)
                                {
                                    continue;
                                }
                            }
                        }

                        int lim = (int)(item.Value.Win * (1 + Config.JXBL * 0.01) * 10);
                        lim = Util.ClosetoEven(lim);
                        //不能高于最高极限
                        lim = lim > Config.ZGJX ? Config.ZGJX : lim;

                        RaceInfoItem raceitem = new RaceInfoItem();
                        raceitem.Url = match;
                        raceitem.Horse = horse;
                        raceitem.Race = race;
                        raceitem.Win = jine;
                        raceitem.Place = 0;
                        raceitem.Zhe = Config.GDJK;
                        raceitem.LWin = lim;
                        raceitem.LPlace = 0;
                        raceitem.Date = GetNow(match);
                        bool bRet = XiaZhuGua(raceitem, out BetResultInfo info);

                        //打单成功，记录下来，并根据赔率变化来跟进
                        BettedItem bitem = new BettedItem();
                        bitem.BetTime = DateTime.Now;
                        bitem.Race = raceitem.Race;
                        bitem.Horse1 = horse;
                        bitem.Horse2 = horse;
                        bitem.DBetCount = (int)raceitem.Win;
                        bitem.Zhe = raceitem.Zhe;
                        bitem.Lim = raceitem.LWin;
                        bitem.PlayType = raceitem.Playtype;
                        bitem.BetType = raceitem.Bettype;
                        bitem.Odds = item.Value.Win;
                        bitem.TotalCount = (int)raceitem.Win;
                        bitem.Result = bRet;
                        bitem.Reason = info.StrAnswer;
                        SendBetOkEvent(bitem);
                        if (bRet)
                        {

                            if (!dicBetWinPiao.ContainsKey(betKey))
                            {
                                dicBetWinPiao.Add(betKey, (int)raceitem.Win);
                            }
                            else
                            {
                                dicBetWinPiao[betKey] = dicBetWinPiao[betKey] + (int)raceitem.Win;
                            }
                        }
                        else
                        {
                            if (!hsBetFail.Contains(betKey))
                            {
                                hsBetFail.Add(betKey);
                            }
                        }
                    }
                }
           
            }
        }

        private void DoRunning()
        {
            //走地反跟挂赌成交的单，这里可以设置反跟票数比例fp，走地折扣固定120，
            //极限是300 / 0，设置删除挂单时间dt（这个时间是根据走地还剩余多少秒来设置）
            bool isempty;
            RaceInfoEnity htRace = GetRaceInfo(AccountInfo.CUserName, out isempty);
            ProcessQueue_ProcessItemEvent(htRace);
            /*
            Hashtable htBetInfo = GetNewBetInfo();
            foreach(DictionaryEntry entry in htBetInfo)
            {
                RaceItem item = entry.Value as RaceItem;
                if(item !=null && !hsEatWin.Contains(entry.Key as string))
                {
                    RaceInfoItem raceitem = new RaceInfoItem();
                    raceitem.Url = item.Url;
                    raceitem.Horse = item.Horse;
                    raceitem.Race = item.Race;
                    raceitem.Win =Util.Closeto5((int)(item.Piao * Config.PSBL)) ;
                    raceitem.Place = 0;
                    raceitem.Zhe = 120;
                    raceitem.LWin = 300;
                    raceitem.LPlace = 0;
                    raceitem.Date = GetNow(item.Url);
                    bool bRet = QiPiaoGuaRunning(raceitem, out BetResultInfo info);

                   
                    BettedItem bitem = new BettedItem();
                    bitem.BetTime = DateTime.Now;
                    bitem.Race = raceitem.Race;
                    bitem.Horse1 = raceitem.Horse;
                    bitem.Horse2 = raceitem.Horse;
                    bitem.DBetCount = (int)raceitem.Win;
                    bitem.Zhe = raceitem.Zhe;
                    bitem.Lim = raceitem.LWin;
                    bitem.PlayType = raceitem.Playtype;
                    bitem.BetType = raceitem.Bettype;
                    bitem.Odds = 0;
                    bitem.TotalCount = (int)raceitem.Win;
                    bitem.Result = bRet;
                    bitem.Reason = info.StrAnswer;
                    SendBetOkEvent(bitem);
                    if (!hsEatWin.Contains(entry.Key))
                    {
                        hsEatWin.Add(entry.Key as string);
                    }
                }
            }
            */
        }

        private void ProcessQueue_ProcessItemEvent(RaceInfoEnity info)
        {
            if (!dicRaceinfo.ContainsKey(info.SnakeHead))
            {

                dicRaceinfo.Add(info.SnakeHead, new RaceInfoEnity());

            }
            RaceInfoEnity oldEnity = dicRaceinfo[info.SnakeHead] as RaceInfoEnity;
            if (oldEnity != null)
            {
                if (Monitor.TryEnter(oldEnity))
                {
                    RaceInfoEnity htDiff = CompareRaceInfo(oldEnity, info);

                    if (!IsBackFirst(htDiff))
                    {

                        SendNewTicktEvent(Util.CloneObject(htDiff) as RaceInfoEnity);
                        //防止由于网络原因，导致下单数据变为空.表现为原来有数据，现在没有数据了
                        if (!((oldEnity != null && oldEnity.DicRaceInfo.Count != 0) && info.DicRaceInfo.Count == 0))
                        {

                            dicRaceinfo[info.SnakeHead] = info;
                            DoBetRunning(htDiff);
                        }
                        //DisplayNewBetInfo(htDiff);
                    }
                    Monitor.Exit(oldEnity);
                }
            }


        }

        private void DoBetRunning(RaceInfoEnity info)
        {
            foreach (var enity in info.DicRaceInfo)
            {
                RaceInfoItem raceitem = enity.Value as RaceInfoItem;
                //只下wp的赌
                if (raceitem.Playtype == PlayType.WP && raceitem.Bettype == BetType.BET)
                {
                    int piao = Util.Closeto5((int)(raceitem.Win * Config.PSBL));
                    raceitem.Win = piao;
                    raceitem.Place = 0;
                    raceitem.Zhe = 120;
                    raceitem.LWin = 300;
                    bool bRet = QiPiaoGuaRunning(raceitem, out BetResultInfo resultinfo);
                    BettedItem bitem = new BettedItem();
                    bitem.BetTime = DateTime.Now;
                    bitem.Race = raceitem.Race;
                    bitem.Horse1 = raceitem.Horse;
                    bitem.Horse2 = raceitem.Horse;
                    bitem.DBetCount = (int)raceitem.Win;
                    bitem.Zhe = raceitem.Zhe;
                    bitem.Lim = raceitem.LWin;
                    bitem.PlayType = raceitem.Playtype;
                    bitem.BetType = raceitem.Bettype;
                    bitem.Odds = 0;
                    bitem.TotalCount = (int)raceitem.Win;
                    bitem.Result = bRet;
                    bitem.Reason = resultinfo.StrAnswer;
                    SendBetOkEvent(bitem);
                }
            }
        }
        private RaceInfoEnity CompareRaceInfo(RaceInfoEnity oldEnity, RaceInfoEnity newEnity)
        {
            string sh = newEnity.SnakeHead;
            RaceInfoEnity raceInfo = new RaceInfoEnity();
            raceInfo.SnakeHead = sh;
            Dictionary<string, RaceInfoItem> htReturn = new Dictionary<string, RaceInfoItem>();
            if (oldEnity != null)
            {
                foreach (KeyValuePair<string, RaceInfoItem> de in newEnity.DicRaceInfo)
                {
                    //如果下单情况没有变化，hashtable是一样的
                    //如果有新增加或者修改，在旧的hashtable中，将不存在新的单据信息
                    if (!oldEnity.DicRaceInfo.ContainsKey(de.Key))
                    {
                        RaceInfoItem newRaceInfo = de.Value as RaceInfoItem;
                        bool bAddFlag = true;
                        foreach (KeyValuePair<string, RaceInfoItem> deOld in oldEnity.DicRaceInfo)
                        {
                            RaceInfoItem oldRaceInfo = deOld.Value as RaceInfoItem;
                            if ((newRaceInfo.Bettype == oldRaceInfo.Bettype)
                                && (newRaceInfo.Playtype == oldRaceInfo.Playtype)
                                && (newRaceInfo.Country.Equals(oldRaceInfo.Country))
                                && (newRaceInfo.Location.Equals(oldRaceInfo.Location))
                                && (newRaceInfo.Date.Equals(oldRaceInfo.Date))
                                && (newRaceInfo.Url.Equals(oldRaceInfo.Url))
                                && (newRaceInfo.Race.Equals(oldRaceInfo.Race))
                                && (newRaceInfo.Horse.Equals(oldRaceInfo.Horse))
                                && (newRaceInfo.Zhe == oldRaceInfo.Zhe)
                                && (newRaceInfo.LWin == oldRaceInfo.LWin)
                                && (newRaceInfo.LPlace == oldRaceInfo.LPlace)
                                )
                            {
                                //计算差异的票数
                                double diffWin = newRaceInfo.Win - oldRaceInfo.Win;
                                double diffPlace = newRaceInfo.Place - oldRaceInfo.Place;
                                //在原来单的基础上增加了打单的票数
                                RaceInfoItem tmp = newRaceInfo.Clone();
                                tmp.Win = diffWin;
                                tmp.Place = diffPlace;
                                if (!htReturn.ContainsKey(tmp.ToString()))
                                {
                                    htReturn.Add(tmp.ToString(), tmp);
                                }

                                bAddFlag = false;
                                break;
                            }

                        }
                        if (bAddFlag)
                        {
                            //如果查完所有的旧单，都没有找到这条单，所以这是一条新打的单
                            if (!htReturn.ContainsKey(newRaceInfo.ToString()))
                            {
                                htReturn.Add(newRaceInfo.ToString(), newRaceInfo.Clone());
                            }
                        }
                    }
                    raceInfo.DicRaceInfo = htReturn;
                }
            }


            return raceInfo;
        }

        /// <summary>
        /// 判断打单的差异数据是不是由后发先至造成的
        /// </summary>
        /// <param name="info"></param>
        /// <returns>如果是后发先至，返回为true;否则为false</returns>
        private bool IsBackFirst(RaceInfoEnity info)
        {
            bool bRet = false;
            if (info != null)
            {
                foreach (var item in info.DicRaceInfo)
                {
                    RaceInfoItem temp = item.Value as RaceInfoItem;
                    if (temp != null)
                    {
                        if (temp.Win < 0 || temp.Place < 0)
                        {
                            bRet = true;
                            break;
                        }
                    }
                }
            }
            return bRet;
        }
    }
}
