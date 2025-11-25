using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuaDan
{
    [Serializable]
    public class GdConfig: AppConfig
    {
        public string Accout2;
        public string Pwd2;
        public string Pin2;

        public double Qgqzk;
        public double Qgdzk;
        public int Qzkps;
        public int Qgdps;

        public double WPgqzk;
        public double WPgdzk;
        public int WPgdps;

        public double Wgqzk;
        public double Wgdzk;
        public int Wzkps;
        public int Wgdps;

        public double Pgqzk;
        public double Pgdzk;
        public int Pzkps;
        public int Pgdps;

        public double Qgqzk3;
        public double Qgdzk3;
        public int Qzkps3;


        public double Wgqzk3;
        public double Wgdzk3;
        public int Wzkps3;

        public double Pgqzk3;
        public double Pgdzk3;
        public int Pzkps3;

        public double Qgqzk2;
        public double Qgdzk2;

        public double WPgqzk2;
        public double WPgdzk2;

        public double Wgqzk2;
        public double Wgdzk2;

        public double Pgqzk2;
        public double Pgdzk2;

        public bool bZk;
        public bool bGp;
    }
    [Serializable]
    public class GtConfig: AppConfig
    {
        public string Accout2;
        public string Pwd2;
        public string Pin2;

        public double ZheQ;
        public int LlimQ;
        public int RLimQ;

        public double ZheQp;
        public int LlimQp;
        public int RLimQp;

        public int Gdz;

        // TCP服务器配置
        public int TcpServerPort = 8888;
    }
}
