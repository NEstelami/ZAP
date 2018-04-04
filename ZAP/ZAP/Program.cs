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
        // Usage: ZAP.exe [input xml] [mode (b/e)]
        static void Main(string[] args)
        {
            //Test(ZFileMode.Extract);
            //Test(ZFileMode.Build);

            if (args.Length == 2)
            {
                string inputXml = args[0];
                string mode = args[1];

                if (mode == "b")
                    Test(inputXml, ZFileMode.Build);
                else if (mode == "e")
                    Test(inputXml, ZFileMode.Extract);
                else
                    PrintUsage();
            }
            else
            {
                PrintUsage();
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("ZAP: Zelda Asset Processor");
            Console.WriteLine("Usage: ZAP [input xml file] [mode (b/e)]");
        }

        static void Test(string xmlFilePath, ZFileMode fileMode)
        {
            XmlReader reader = XmlReader.Create(xmlFilePath);

            while (!reader.EOF)
            {
                reader.Read();

                if (reader.Name == "File")
                {
                    ZFile file = new ZFile(fileMode, ref reader, Path.GetDirectoryName(xmlFilePath).Replace("\\", "/"));

                    if (fileMode == ZFileMode.Extract)
                        file.ExtractResources();
                    else
                        file.BuildResources();
                }

                //Console.WriteLine(reader.Name);
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
