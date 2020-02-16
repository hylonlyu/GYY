using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuaDan
{
    public class RegResult
    {
        private bool bResult = false;
        private string cMachineCode = null;
        private string cRegCode = null;
        private DateTime dExpiredTime = DateTime.Now;
        private double dMaxSubmitRMB = 0.0;
        private string cMsg = "";
        public bool IsTry
        {
            get; set;
        }

        public DateTime GetExpiredTime()
        {
            return this.dExpiredTime;
        }

        public string GetMachineCode()
        {
            return this.cMachineCode;
        }

        public double GetMaxSubmitRMB()
        {
            return this.dMaxSubmitRMB;
        }

        public string GetRegCode()
        {
            return this.cRegCode;
        }

        public bool GetResult()
        {
            return this.bResult;
        }

        public void SetExpiredTime(DateTime dExpiredTime)
        {
            this.dExpiredTime = dExpiredTime;
        }

        public void SetMachineCode(string cMachineCode)
        {
            this.cMachineCode = cMachineCode;
        }

        public void SetMaxSubmitRMB(double dMaxSubmitRMB)
        {
            this.dMaxSubmitRMB = dMaxSubmitRMB;
        }

        public void SetRegCode(string cRegCode)
        {
            this.cRegCode = cRegCode;
        }

        public void SetResult(bool bResult)
        {
            this.bResult = bResult;
        }

        public string GetMsg()
        {
            return this.cMsg;
        }

        public void SetMsg(string val)
        {
            this.cMsg = val;
        }
    }
}
