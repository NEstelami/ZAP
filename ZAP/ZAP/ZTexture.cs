using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml;
using System.Runtime.InteropServices;

namespace ZAP
{
    public enum TextureType
    {
        RGBA32bpp,
        RGBA16bpp,
        Palette4bpp,
        Palette8bpp,
        Grayscale4bpp,
        Grayscale8bpp,
        GrayscaleAlpha4bpp,
        GrayscaleAlpha8bpp,
        GrayscaleAlpha16bpp
    };

    public class ZTexture
    {
        TextureType type;
        string name;
        int width, height;
        byte[] rawData;
        Bitmap bmpRgb, bmpAlpha, bmpPalette;

        public ZTexture(ref XmlReader reader, byte[] nRawData, int rawDataIndex)
        {
            ParseXML(ref reader);
            rawData = new byte[GetRawDataSize()];

            Array.Copy(nRawData, rawDataIndex, rawData, 0, rawData.Length);
            FixRawData();
            PrepareBitmap();
        }

        public ZTexture(TextureType nType, byte[] nRawData, string nName, int nWidth, int nHeight)
        {
            FixRawData();
        }

        void ParseXML(ref XmlReader reader)
        {
            name = reader.GetAttribute("Name");
            width = Convert.ToInt32(reader.GetAttribute("Width"));
            height = Convert.ToInt32(reader.GetAttribute("Height"));

            string formatStr = reader.GetAttribute("Format");
            
            switch (formatStr)
            {
                case "rgba32": type = TextureType.RGBA32bpp; break;
                case "i4": type = TextureType.Palette4bpp; break;
                case "i8": type = TextureType.Palette8bpp; break;
                case "ia4": type = TextureType.GrayscaleAlpha4bpp; break;
                case "ia8": type = TextureType.GrayscaleAlpha8bpp; break;
                default: throw new Exception(String.Format("Format {0} is not supported!", formatStr));
            }
        }

        void FixRawData()
        {
            if (type == TextureType.RGBA32bpp)
            {
                for (int i = 0; i < rawData.Length; i += 4)
                {
                    byte tmp = rawData[i];
                    rawData[i] = rawData[i + 2];
                    rawData[i + 2] = tmp;
                }
            }
        }

        private void PrepareBitmap()
        {
            bmpRgb = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            bmpAlpha = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            if (type == TextureType.RGBA32bpp)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int pos = ((y * width) + x) * 4;
                        Color c = Color.FromArgb(255, rawData[pos + 2], rawData[pos + 1], rawData[pos + 0]);
                        Color a = Color.FromArgb(255, rawData[pos + 3], rawData[pos + 3], rawData[pos + 3]);
                        bmpRgb.SetPixel(x, y, c);
                        bmpAlpha.SetPixel(x, y, a);
                    }
                }
            }
            else if (type == TextureType.Grayscale8bpp)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int pos = ((y * width) + x) * 1;
                        Color c = Color.FromArgb(255, rawData[pos], rawData[pos], rawData[pos]);
                        bmpRgb.SetPixel(x, y, c);
                    }
                }
            }
            else if (type == TextureType.GrayscaleAlpha8bpp)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int pos = ((y * width) + x) * 1;
                        byte grayscale = (byte)(rawData[pos] & 0xF0);
                        byte alpha = (byte)( (rawData[pos] & 0x0F) << 4);
                        Color c = Color.FromArgb(255, grayscale, grayscale, grayscale);
                        Color a = Color.FromArgb(255, alpha, alpha, alpha);
                        bmpRgb.SetPixel(x, y, c);
                        bmpAlpha.SetPixel(x, y, a);
                    }
                }
            }
            else if (type == TextureType.Grayscale4bpp)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x += 2)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            int pos = ((y * width) + x) / 2;
                            byte grayscale = 0;

                            if (i == 0)
                                grayscale = (byte)(rawData[pos] & 0xF0);
                            else
                                grayscale = (byte)((rawData[pos] & 0x0F) << 4);

                            Color c = Color.FromArgb(255, grayscale, grayscale, grayscale);
                            bmpRgb.SetPixel(x+i, y, c);
                        }
                    }
                }
            }
            else if (type == TextureType.GrayscaleAlpha4bpp)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x += 2)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            int pos = ((y * width) + x) / 2;
                            byte data = 0;

                            if (i == 0)
                                data = (byte)((rawData[pos] & 0xF0) >> 4);
                            else
                                data = (byte)(rawData[pos] & 0x0F);

                            byte grayscale = (byte)((data & 0x0E) >> 1);
                            byte alpha = (byte)((data & 0x01) * 255);

                            Color c = Color.FromArgb(255, grayscale, grayscale, grayscale);
                            Color a = Color.FromArgb(255, alpha, alpha, alpha);
                            bmpRgb.SetPixel(x + i, y, c);
                            bmpAlpha.SetPixel(x + i, y, a);
                        }
                    }
                }
            }
        }

        public int GetRawDataSize()
        {
            return (int)(width * height * GetPixelMultiplyer());
        }

        float GetPixelMultiplyer()
        {
            switch (type)
            {
                case TextureType.Grayscale4bpp: case TextureType.GrayscaleAlpha4bpp: case TextureType.Palette4bpp: return 0.5f;
                case TextureType.Grayscale8bpp: case TextureType.GrayscaleAlpha8bpp: case TextureType.Palette8bpp: return 1;
                case TextureType.GrayscaleAlpha16bpp: case TextureType.RGBA16bpp: return 2;
                case TextureType.RGBA32bpp: return 4;
            }

            return -1;
        }

        public void Save()
        {
            if (type == TextureType.RGBA32bpp)
            {
                bmpRgb.Save("out\\" + name + ".rgb.png");
                bmpAlpha.Save("out\\" + name + ".a.png");
            }
            else if (type == TextureType.Grayscale8bpp || type == TextureType.Grayscale4bpp)
            {
                bmpRgb.Save("out\\" + name + ".gray.png");
            }
            else if (type == TextureType.GrayscaleAlpha8bpp || type == TextureType.GrayscaleAlpha4bpp)
            {
                bmpRgb.Save("out\\" + name + ".gray.png");
                bmpAlpha.Save("out\\" + name + ".a.png");
            }
        }
    }
}
