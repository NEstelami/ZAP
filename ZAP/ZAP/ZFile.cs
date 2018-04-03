using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace ZAP
{
    public enum ZFileMode
    {
        Build,
        Extract,
    };

    public class ZFile
    {
        string name;
        List<ZTexture> resources;
        
        public ZFile(ZFileMode mode, ref XmlReader reader)
        {
            resources = new List<ZTexture>();
            ParseXML(mode, ref reader);
        }

        void ParseXML(ZFileMode mode, ref XmlReader reader)
        {
            int startDepth = reader.Depth;
            name = reader.GetAttribute("Name");

            byte[] rawData = null;

            if (mode == ZFileMode.Extract)
                rawData = File.ReadAllBytes(name);

            int rawDataIndex = 0;

            reader.Read();

            while (reader.Depth > startDepth)
            {
                if (reader.Name == "Texture")
                {
                    ZTexture tex = null;

                    if (mode == ZFileMode.Extract)
                        tex = new ZTexture(ref reader, rawData, rawDataIndex);
                    else
                        tex = new ZTexture(ref reader);

                    resources.Add(tex);
                    rawDataIndex += tex.GetRawDataSize();
                }

                reader.Read();
            }
        }

        public void ExtractResources()
        {
            foreach (ZTexture res in resources)
            {
                Console.WriteLine("Saving resource " + res.GetName());
                res.Save();
            }
        }
    }
}
