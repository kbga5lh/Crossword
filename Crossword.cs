using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CrosswordApp
{
    public struct Crossword
    {
        public struct Placement
        {
            public int x;
            public int y;
            public int wordIndex;
            public int index;
            public int wordLength;
            public bool isVertical;

            [Newtonsoft.Json.JsonIgnore]
            public int Width => isVertical ? 1 : wordLength;
            [Newtonsoft.Json.JsonIgnore]
            public int Height => isVertical ? wordLength : 1;
            [Newtonsoft.Json.JsonIgnore]
            public Rect Rect => new Rect(x, y, Width, Height);
        }

        public string name;
        public List<(string word, string definition)> words;
        public List<Placement> placements;
        public (int x, int y) Size
        {
            get
            {
                var maxX = int.MinValue;
                var maxY = int.MinValue;
                foreach (var placement in placements)
                {
                    maxX = Math.Max(placement.x + placement.Width, maxX);
                    maxY = Math.Max(placement.y + placement.Height, maxY);
                }
                return (maxX, maxY);
            }
        }

        public string GetDefinitionsString()
        {
            var result = string.Empty;

            result += "По горизонтали:\n\n";

            for (var i = 0; i < placements.Count; ++i)
            {
                if (!placements[i].isVertical)
                {
                    result += $"{placements[i].index} - {words[placements[i].wordIndex].definition}\n\n";
                }
            }

            result += "По вертикали:\n\n";

            for (var i = 0; i < placements.Count; ++i)
            {
                if (placements[i].isVertical)
                {
                    result += $"{placements[i].index} - {words[placements[i].wordIndex].definition}\n\n";
                }
            }

            return result;
        }

        public Bitmap ToImage(bool fillWithWords = true, int cellSize = 48)
        {
            var (sizeX, sizeY) = Size;

            var result = new Bitmap(sizeX * cellSize, sizeY * cellSize);

            for (var x = 0; x < sizeX * cellSize; ++x)
                for (var y = 0; y < sizeY * cellSize; ++y)
                    result.SetPixel(x, y, Color.FromArgb(10, 10, 10));

            foreach (var placement in placements)
            {
                for (var x = placement.x; x < placement.x + placement.Width; ++x)
                {
                    for (var y = placement.y; y < placement.y + placement.Height; ++y)
                    {
                        DrawCell(cellSize, result, x, y);
                        if (fillWithWords)
                        {
                            DrawChar(cellSize, result, x, y,
                                words[placement.wordIndex].word[placement.isVertical ? y - placement.y : x - placement.x]);
                        }
                    }
                }
            }

            foreach (var placement in placements)
            {
                DrawIndex(cellSize, result, placement.x, placement.y, placement.index);
            }

            return result;
        }

        static void DrawCell(int cellSize, Bitmap result, int x, int y)
        {
            for (var i = 1; i < cellSize - 1; ++i)
            {
                for (var j = 1; j < cellSize - 1; ++j)
                {
                    result.SetPixel(x * cellSize + i, y * cellSize + j,
                        Color.FromArgb(255, 255, 255));
                }
            }
        }

        static void DrawIndex(int cellSize, Bitmap result, int x, int y, int index)
        {
            var g = Graphics.FromImage(result);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            var rectf = new RectangleF(2 + x * cellSize,
                2 + y * cellSize, cellSize, cellSize);
            g.DrawString(index.ToString(), new Font("Tahoma", 8), Brushes.Black, rectf);

            g.Flush();

        }

        static void DrawChar(int cellSize, Bitmap result, int x, int y, char ch)
        {
            var g = Graphics.FromImage(result);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            var sizeF = g.MeasureString(ch.ToString(), new Font("Tahoma", 16));
            var rectf = new RectangleF(x * cellSize + (cellSize - sizeF.Width) / 2,
                y * cellSize + (cellSize - sizeF.Height) / 2,
                sizeF.Width,
                sizeF.Height);
            g.DrawString(ch.ToString(), new Font("Tahoma", 16), Brushes.Black, rectf);

            g.Flush();
        }

        public static BitmapImage ConvertToBitmapImage(Bitmap src)
        {
            var ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            var image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
