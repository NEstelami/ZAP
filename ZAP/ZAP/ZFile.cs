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
        List<ZResource> resources;
        
        public ZFile(ZFileMode mode, ref XmlReader reader)
        {
            resources = new List<ZResource>();
            ParseXML(mode, ref reader);
        }

        void ParseXML(ZFileMode mode, ref XmlReader reader)
        {
            int startDepth = reader.Depth;
            name = reader.GetAttribute("Name");

            string folderName = Path.GetFileNameWithoutExtension(name);

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
                        tex = new ZTexture(ref reader, folderName);

                    resources.Add(tex);
                    rawDataIndex += tex.GetRawDataSize();
                }
                else if (reader.Name == "Blob")
                {
                    ZBlob blob = null;

                    if (mode == ZFileMode.Extract)
                        blob = new ZBlob(ref reader, rawData, rawDataIndex);
                    else
                        blob = new ZBlob(ref reader, folderName);

                    resources.Add(blob);

                    rawDataIndex += blob.GetRawDataSize();
                }

                reader.Read();
            }
        }

        public void ExtractResources()
        {
            string folderName = Path.GetFileNameWithoutExtension(name);

            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);

            foreach (ZResource res in resources)
            {
                Console.WriteLine("Saving resource " + res.GetName());
                res.Save(folderName);
            }
        }

        public void BuildResources()
        {
            int size = 0;

            foreach (ZResource res in resources)
                size += res.GetRawDataSize();

            // Make sure size is 16 byte aligned
            if (size % 16 != 0)
                size = ((size / 16) + 1) * 16;

            byte[] file = new byte[size];
            int fileIndex = 0;

            foreach (ZResource res in resources)
            {
                Console.WriteLine("Building resource " + res.GetName());
                Array.Copy(res.GetRawData(), 0, file, fileIndex, res.GetRawData().Length);
                fileIndex += res.GetRawData().Length;
            }

            File.WriteAllBytes(name + ".test", file);
        }
    }
}
