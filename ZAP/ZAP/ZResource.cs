using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAP
{
    public class ZResource
    {
        protected string name;
        protected byte[] rawData;

        public virtual void Save(string outFolder)
        {

        }

        public string GetName()
        {
            return name;
        }

        public virtual byte[] GetRawData()
        {
            return rawData;
        }

        public virtual int GetRawDataSize()
        {
            if (rawData != null)
                return rawData.Length;
            else
                return -1;
        }
    }
}
