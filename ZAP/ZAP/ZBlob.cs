using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;

namespace ZAP
{
    public class ZBlob : ZResource
    {

        // EXTRACT MODE
        public ZBlob(ref XmlReader reader, byte[] nRawData, int rawDataIndex)
        {            
            name = reader.GetAttribute("Name");
            int size = Convert.ToInt32(reader.GetAttribute("Size"), 16);
            rawData = new byte[size];

            Array.Copy(nRawData, rawDataIndex, rawData, 0, rawData.Length);
        }

        // BUILD MODE
        public ZBlob(ref XmlReader reader)
        {
            name = reader.GetAttribute("Name");

            rawData = File.ReadAllBytes("out\\" + name + ".bin");
        }
        
        public override void Save()
        {
            File.WriteAllBytes("out\\" + name + ".bin", rawData);
        }
    }
}
