using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuaDan
{
    public class UserInfo
    {
        private string cPassword = null;

        public string CPassword
        {
            get { return cPassword; }
            set { cPassword = value; }
        }
        private string cUserName = null;

        public string CUserName
        {
            get { return cUserName; }
            set { cUserName = value; }
        }

        private string cPin = null;

        public string CPin
        {
            get { return cPin; }
            set { cPin = value; }
        }

        private string cUrl = null;

        public string CUrl
        {
            get { return cUrl; }
            set { cUrl = value; }
        }

        private int iReadSpan = 100;

        public int IReadSpan
        {
            get { return iReadSpan; }
            set { iReadSpan = value; }
        }

    }
}
