using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

// ZAP - Zelda Asset Processor

namespace ZAP
{
    class Program
    {
        static void Main(string[] args)
        {
            Test();
        }

        static void Test()
        {
            XmlReader reader = XmlReader.Create("icon_item_static.xml");

            while (!reader.EOF)
            {
                reader.Read();

                if (reader.Name == "File")
                {
                    ZFile file = new ZFile(ref reader);

                    file.ExtractResources();
                }

                Console.WriteLine(reader.Name);
            }
        }

        static void Extract(string xmlPath)
        {

        }

        static void Build(string xmlPath)
        {

        }
    }
}
