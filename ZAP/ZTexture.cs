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

    public class ZTexture : ZResource
    {
        TextureType type;
        int width, height;
        Bitmap bmpRgb, bmpAlpha;

        // EXTRACT MODE
        public ZTexture(ref XmlReader reader, byte[] nRawData, int rawDataIndex)
        {
            ParseXML(ref reader);
            rawData = new byte[GetRawDataSize()];

            Array.Copy(nRawData, rawDataIndex, rawData, 0, rawData.Length);
            FixRawData();
            PrepareBitmap();
        }

        // BUILD MODE
        public ZTexture(ref XmlReader reader, string inFolder)
        {
            ParseXML(ref reader);

            // Get Raw Data
            PrepareRawData(inFolder);
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
                case "rgb5a1": type = TextureType.RGBA16bpp; break;
                case "i4": type = TextureType.Grayscale4bpp; break;
                case "i8": type = TextureType.Grayscale8bpp; break;
                case "ia4": type = TextureType.GrayscaleAlpha4bpp; break;
                case "ia8": type = TextureType.GrayscaleAlpha8bpp; break;
                case "ia16": type = TextureType.GrayscaleAlpha16bpp; break;
                case "ci4": type = TextureType.Palette4bpp; break;
                case "ci8": type = TextureType.Palette8bpp; break;
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
            else if (type == TextureType.RGBA16bpp)
            {
                for (int i = 0; i < rawData.Length; i += 2)
                {
                    byte tmp = rawData[i];
                    rawData[i] = rawData[i + 1];
                    rawData[i+1] = tmp;
                }
            }
        }

        private void PrepareBitmap()
        {
            bmpRgb = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            bmpAlpha = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            switch (type)
            {
                case TextureType.RGBA16bpp: PrepareBitmapRGBA16(); break;
                case TextureType.RGBA32bpp: PrepareBitmapRGBA32(); break;
                case TextureType.Grayscale4bpp: PrepareBitmapGrayscale4(); break;
                case TextureType.Grayscale8bpp: PrepareBitmapGrayscale8(); break;
                case TextureType.GrayscaleAlpha4bpp: PrepareBitmapGrayscaleAlpha4(); break;
                case TextureType.GrayscaleAlpha8bpp: PrepareBitmapGrayscaleAlpha8(); break;
                case TextureType.GrayscaleAlpha16bpp: PrepareBitmapGrayscaleAlpha16(); break;
                case TextureType.Palette4bpp: PrepareBitmapPalette4(); break;
                case TextureType.Palette8bpp: PrepareBitmapPalette8(); break;
            }
        }

        private void PrepareBitmapRGBA16()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = ((y * width) + x) * 2;
                    short data = (short)((rawData[pos + 1] << 8) + rawData[pos]);
                    byte r = (byte)((data & 0xF800) >> 11);
                    byte g = (byte)((data & 0x07C0) >> 6);
                    byte b = (byte)((data & 0x003E) >> 1);
                    byte alpha = (byte)(data & 0x01);
                    Color c = Color.FromArgb(255, r * 8, g * 8, b * 8);
                    Color a = Color.FromArgb(255, alpha * 255, alpha * 255, alpha * 255);
                    bmpRgb.SetPixel(x, y, c);
                    bmpAlpha.SetPixel(x, y, a);
                }
            }
        }

        private void PrepareBitmapRGBA32()
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

        private void PrepareBitmapGrayscale8()
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

        private void PrepareBitmapGrayscaleAlpha8()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = ((y * width) + x) * 1;
                    byte grayscale = (byte)(rawData[pos] & 0xF0);
                    byte alpha = (byte)((rawData[pos] & 0x0F) << 4);
                    Color c = Color.FromArgb(255, grayscale, grayscale, grayscale);
                    Color a = Color.FromArgb(255, alpha, alpha, alpha);
                    bmpRgb.SetPixel(x, y, c);
                    bmpAlpha.SetPixel(x, y, a);
                }
            }
        }

        private void PrepareBitmapGrayscale4()
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
                        bmpRgb.SetPixel(x + i, y, c);
                    }
                }
            }
        }

        private void PrepareBitmapGrayscaleAlpha4()
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

                        byte grayscale = (byte)(((data & 0x0E) >> 1) * 32);
                        byte alpha = (byte)((data & 0x01) * 255);

                        Color c = Color.FromArgb(255, grayscale, grayscale, grayscale);
                        Color a = Color.FromArgb(255, alpha, alpha, alpha);
                        bmpRgb.SetPixel(x + i, y, c);
                        bmpAlpha.SetPixel(x + i, y, a);
                    }
                }
            }
        }

        private void PrepareBitmapGrayscaleAlpha16()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = ((y * width) + x) * 2;
                    byte grayscale = rawData[pos];
                    byte alpha = rawData[pos + 1];
                    Color c = Color.FromArgb(255, grayscale, grayscale, grayscale);
                    Color a = Color.FromArgb(255, alpha, alpha, alpha);
                    bmpRgb.SetPixel(x, y, c);
                    bmpAlpha.SetPixel(x, y, a);
                }
            }
        }

        private void PrepareBitmapPalette4()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x += 2)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int pos = ((y * width) + x) / 2;
                        byte paletteIndex = 0;

                        if (i == 0)
                            paletteIndex = (byte)((rawData[pos] & 0xF0) >> 4);
                        else
                            paletteIndex = (byte)((rawData[pos] & 0x0F));

                        Color c = Color.FromArgb(255, paletteIndex * 16, paletteIndex * 16, paletteIndex * 16);
                        bmpRgb.SetPixel(x + i, y, c);
                    }
                }
            }
        }

        private void PrepareBitmapPalette8()
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

        private void PrepareRawData(string inFolder)
        {
            rawData = new byte[GetRawDataSize()];

            switch (type)
            {
                case TextureType.RGBA16bpp: PrepareRawDataRGBA16(inFolder); break;
                case TextureType.RGBA32bpp: PrepareRawDataRGBA32(inFolder); break;
                case TextureType.Grayscale8bpp: PrepareRawDataGrayscale8(inFolder); break;
                case TextureType.GrayscaleAlpha8bpp: PrepareRawDataGrayscaleAlpha8(inFolder); break;
                case TextureType.Grayscale4bpp: PrepareRawDataGrayscale4(inFolder); break;
                case TextureType.GrayscaleAlpha4bpp: PrepareRawDataGrayscaleAlpha4(inFolder); break;
                case TextureType.GrayscaleAlpha16bpp: PrepareRawDataGrayscaleAlpha16(inFolder); break;
                case TextureType.Palette4bpp: PrepareRawDataPalette4(inFolder); break;
                case TextureType.Palette8bpp: PrepareRawDataPalette8(inFolder); break;
                default: throw new Exception(String.Format("Build Mode: Format {0} is not supported!", type.ToString()));
            }
        }

        private void PrepareRawDataRGBA32(string inFolder)
        {
            bmpRgb = new Bitmap(inFolder + "/" + name + ".rgb.png");
            bmpAlpha = new Bitmap(inFolder + "/" + name + ".a.png");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = ((y * width) + x) * 4;
                    Color c = bmpRgb.GetPixel(x, y);
                    Color a = bmpAlpha.GetPixel(x, y);

                    rawData[pos + 0] = c.R;
                    rawData[pos + 1] = c.G;
                    rawData[pos + 2] = c.B;
                    rawData[pos + 3] = a.R;
                }
            }
        }

        private void PrepareRawDataRGBA16(string inFolder)
        {
            bmpRgb = new Bitmap(inFolder + "/" + name + ".rgb.png");
            bmpAlpha = new Bitmap(inFolder + "/" + name + ".a.png");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = ((y * width) + x) * 2;
                    Color c = bmpRgb.GetPixel(x, y);
                    Color a = bmpAlpha.GetPixel(x, y);

                    byte r = (byte)(c.R / 8);
                    byte g = (byte)(c.G / 8);
                    byte b = (byte)(c.B / 8);

                    byte alphaBit = Convert.ToByte(a.R != 0);

                    short data = (short)((r << 11) + (g << 6) + (b << 1) + alphaBit);

                    rawData[pos + 0] = (byte)((data & 0xFF00) >> 8);
                    rawData[pos + 1] = (byte)((data & 0x00FF));
                }
            }
        }

        private void PrepareRawDataGrayscale8(string inFolder)
        {
            bmpRgb = new Bitmap(inFolder + "/" + name + ".gray.png");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = ((y * width) + x);
                    Color c = bmpRgb.GetPixel(x, y);
                    
                    rawData[pos] = c.R;
                }
            }
        }

        private void PrepareRawDataGrayscaleAlpha8(string inFolder)
        {
            bmpRgb = new Bitmap(inFolder + "/" + name + ".gray.png");
            bmpAlpha = new Bitmap(inFolder + "/" + name + ".a.png");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = (y * width) + x;
                    Color c = bmpRgb.GetPixel(x, y);
                    Color a = bmpAlpha.GetPixel(x, y);

                    rawData[pos] = (byte)(((c.R / 16) << 4) + (a.R / 16));
                }
            }
        }

        private void PrepareRawDataGrayscale4(string inFolder)
        {
            bmpRgb = new Bitmap(inFolder + "/" + name + ".gray.png");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x += 2)
                {
                    int pos = ((y * width) + x) / 2;
                    Color c1 = bmpRgb.GetPixel(x, y);
                    Color c2 = bmpRgb.GetPixel(x + 1, y);

                    rawData[pos] = (byte)(((c1.R / 16) << 4) + (c2.R / 16));
                }
            }
        }

        private void PrepareRawDataGrayscaleAlpha4(string inFolder)
        {
            bmpRgb = new Bitmap(inFolder + "/" + name + ".gray.png");
            bmpAlpha = new Bitmap(inFolder + "/" + name + ".a.png");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x += 2)
                {
                    int pos = ((y * width) + x) / 2;
                    byte data = 0;
                    
                    for (int i = 0; i < 2; i++)
                    {
                        Color c = bmpRgb.GetPixel(x + i, y);
                        Color a = bmpAlpha.GetPixel(x + i, y);

                        byte alphaBit = Convert.ToByte(a.R != 0);
                        
                        if (i == 0)
                            data += (byte)((((c.R / 32) << 1) + alphaBit) << 4);
                        else
                            data += (byte)(((c.R / 32) << 1) + alphaBit);
                    }

                    rawData[pos] = data;
                }
            }
        }

        private void PrepareRawDataGrayscaleAlpha16(string inFolder)
        {
            bmpRgb = new Bitmap(inFolder + "/" + name + ".gray.png");
            bmpAlpha = new Bitmap(inFolder + "/" + name + ".a.png");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = ((y * width) + x) * 2;
                    Color c = bmpRgb.GetPixel(x, y);
                    Color a = bmpAlpha.GetPixel(x, y);

                    rawData[pos] = (byte)(c.R);
                    rawData[pos + 1] = (byte)(a.R);
                }
            }
        }

        private void PrepareRawDataPalette4(string inFolder)
        {
            bmpRgb = new Bitmap(inFolder + "/" + name + ".ci4.png");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x += 2)
                {
                    int pos = ((y * width) + x) / 2;
                    Color c1 = bmpRgb.GetPixel(x, y);
                    Color c2 = bmpRgb.GetPixel(x + 1, y);

                    rawData[pos] = (byte)(((c1.R / 16) << 4) + (c2.R / 16));
                }
            }
        }

        private void PrepareRawDataPalette8(string inFolder)
        {
            bmpRgb = new Bitmap(inFolder + "/" + name + ".ci8.png");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = ((y * width) + x);
                    Color c = bmpRgb.GetPixel(x, y);

                    rawData[pos] = c.R;
                }
            }
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

        public override byte[] GetRawData()
        {
            return rawData;
        }

        public override int GetRawDataSize()
        {
            return (int)(width * height * GetPixelMultiplyer());
        }

        public override void Save(string outFolder)
        {
            if (type == TextureType.RGBA32bpp || type == TextureType.RGBA16bpp)
            {
                bmpRgb.Save(outFolder + "/" + name + ".rgb.png");
                bmpAlpha.Save(outFolder + "/" + name + ".a.png");
            }
            else if (type == TextureType.Grayscale8bpp || type == TextureType.Grayscale4bpp)
            {
                bmpRgb.Save(outFolder + "/" + name + ".gray.png");
            }
            else if (type == TextureType.GrayscaleAlpha8bpp || type == TextureType.GrayscaleAlpha4bpp)
            {
                bmpRgb.Save(outFolder + "/" + name + ".gray.png");
                bmpAlpha.Save(outFolder + "/" + name + ".a.png");
            }
            else if (type == TextureType.Palette4bpp)
            {
                bmpRgb.Save(outFolder + "/" + name + ".ci4.png");
            }
            else if (type == TextureType.Palette8bpp)
            {
                bmpRgb.Save(outFolder + "/" + name + ".ci8.png");
            }
        }
    }
}
