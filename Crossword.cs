using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;

namespace CrosswordGenerator
{
    class Crossword
    {
        public struct WordPlacement
        {
            public int x;
            public int y;
            public int wordIndex;
            public string word;
            public bool isVertical;

            public int Width => isVertical ? 1 : word.Length;
            public int Height => isVertical ? word.Length : 1;
            public Rect Rect => new Rect(x, y, Width, Height);

            public bool IsCloseTo(WordPlacement other)
            {
                return IntersectsWith(new Rect(x - 1, y - 1, Width + 2, Height + 2), other.Rect);
            }

            public bool IsIntersectionRight(WordPlacement other)
            {
                for (var i = 0; i < word.Length; ++i)
                {
                    for (var j = 0; j < other.word.Length; ++j)
                    {
                        var p1 = new Point(x + (isVertical ? 0 : i), y + (isVertical ? i : 0));
                        var p2 = new Point(other.x + (other.isVertical ? 0 : j), other.y + (other.isVertical ? j : 0));
                        if (p1 == p2 && word[i] != other.word[j])
                            return false;
                    }
                }

                return true;
            }
        }

        static readonly Random rng = new Random();

        public List<string> Words;

        public void ReadWords(string rawWords)
        {
            Words = rawWords.Split('\n', ' ').ToList();
            Words.RemoveAll(string.IsNullOrWhiteSpace);
            Words.ForEach(v => v.ToLower());
        }

        public List<WordPlacement> Generate()
        {
            Shuffle(Words);

            var wordPlacements = new List<WordPlacement>
            {
                new WordPlacement
                    {x = 0, y = 0, isVertical = rng.Next(0, 2) > 0, wordIndex = 0, word = Words[0]}
            };

            var a = new List<(string, bool)>(Words.ConvertAll(v => (v, false)));
            a[0] = (a[0].Item1, true);
            Backtrack(wordPlacements, a);

            return wordPlacements;
        }

        bool Backtrack(List<WordPlacement> result, List<(string word, bool isPlaced)> words)
        {
            if (words.FindIndex(v => v.isPlaced == false) < 0)
                return true;

            for (var wordIndex = 0; wordIndex < this.Words.Count; ++wordIndex)
            {
                if (words[wordIndex].isPlaced)
                    continue;

                for (var i = 0; i < result.Count; i++)
                {
                    WordPlacement placement = result[i];
                    for (var charInPlacementIndex = 0; charInPlacementIndex < placement.word.Length; charInPlacementIndex++)
                    {
                        var charInPlacement = words[placement.wordIndex].word[charInPlacementIndex];
                        for (var charInWordIndex = 0; charInWordIndex < words[wordIndex].word.Length; charInWordIndex++)
                        {
                            var charInWord = words[wordIndex].word[charInWordIndex];
                            if (charInPlacement != charInWord) continue;

                            var newPlacement = new WordPlacement
                            {
                                wordIndex = wordIndex,
                                isVertical = !placement.isVertical,
                                word = words[wordIndex].word,
                            };
                            if (placement.isVertical)
                            {
                                newPlacement.x = placement.x - charInWordIndex;
                                newPlacement.y = placement.y + charInPlacementIndex;
                            }
                            else
                            {
                                newPlacement.x = placement.x + charInPlacementIndex;
                                newPlacement.y = placement.y - charInWordIndex;
                            }

                            if (IsPlacementCorrect(newPlacement, result))
                            {
                                result.Add(newPlacement);
                                words[wordIndex] = (words[wordIndex].word, true);

                                if (Backtrack(result, words))
                                    return true;

                                result.Remove(newPlacement);
                                words[wordIndex] = (words[wordIndex].word, false);
                            }
                        }
                    }
                }
            }

            return false;
        }

        bool IsPlacementCorrect(WordPlacement placement, List<WordPlacement> placements)
        {
            foreach (var p in placements)
            {
                if (IntersectsWith(p.Rect, placement.Rect))
                {
                    if (p.isVertical == placement.isVertical)
                        return false;
                }
                else if (p.IsCloseTo(placement))
                    return false;
                if (!p.IsIntersectionRight(placement))
                    return false;
            }

            return true;
        }

        static bool IntersectsWith(Rect r1, Rect r2)
        {
            return r2.Left < r1.Right && r2.Right > r1.Left && r2.Top < r1.Bottom && r2.Bottom > r1.Top;
        }

        public static void Shuffle<T>(IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static Bitmap ToImage(List<WordPlacement> source, bool fillWithWords = true)
        {
            int cellSize = 48;

            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minY = int.MaxValue;
            var maxY = int.MinValue;
            foreach (var placement in source)
            {
                minX = Math.Min(placement.x, minX);
                maxX = Math.Max(placement.x + placement.Width, maxX);
                minY = Math.Min(placement.y, minY);
                maxY = Math.Max(placement.y + placement.Height, maxY);
            }
            var (sizeX, sizeY) = (maxX - minX, maxY - minY);

            var result = new Bitmap(sizeX * cellSize + 2, sizeY * cellSize + 2);

            for (var x = 0; x < sizeX * cellSize + 2; ++x)
                for (var y = 0; y < sizeY * cellSize + 2; ++y)
                    result.SetPixel(x, y, Color.FromArgb(245, 245, 245));

            foreach (var placement in source)
            {
                for (var x = placement.x; x < placement.x + placement.Width; ++x)
                {
                    for (var y = placement.y; y < placement.y + placement.Height; ++y)
                    {
                        DrawCell(cellSize, result, x, minX, y, minY);
                        if (fillWithWords)
                        {
                            DrawChar(cellSize, result, x, minX, y, minY,
                                placement.word[placement.isVertical ? y - placement.y : x - placement.x]);
                        }
                    }
                }
            }

            foreach (var placement in source)
            {
                DrawIndex(cellSize, result, placement.x, minX, placement.y, minY, placement.wordIndex + 1);
            }

            return result;
        }

        static void DrawCell(int cellSize, Bitmap result, int x, int minX, int y, int minY)
        {
            for (var i = 0; i < cellSize; ++i)
            {
                for (var j = 0; j < cellSize; ++j)
                {
                    //if (i == 0 || j == 0)
                    //    result.SetPixel((x - minX) * cellSize + i, (y - minY) * cellSize + j,
                    //        Color.FromArgb(180, 180, 180));
                    //else
                    result.SetPixel(1 + (x - minX) * cellSize + i, 1 + (y - minY) * cellSize + j,
                        Color.FromArgb(255, 255, 255));
                }
            }
            for (var i = -1; i <= cellSize; ++i)
            {
                for (var j = -1; j <= cellSize; ++j)
                {
                    if (i != -1 && j != -1 && i != cellSize && j != cellSize)
                        continue;

                    int pixelX = 1 + (x - minX) * cellSize + i;
                    int pixelY = 1 + (y - minY) * cellSize + j;

                    if (pixelX >= 0 && pixelX < result.Width
                        && pixelY >= 0 && pixelY < result.Height)
                    {
                        result.SetPixel(pixelX, pixelY, Color.FromArgb(180, 180, 180));
                    }
                }
            }
        }

        static void DrawIndex(int cellSize, Bitmap result, int x, int minX, int y, int minY, int index)
        {
            var g = Graphics.FromImage(result);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            var rectf = new RectangleF(1 + (x - minX) * cellSize,
                1 + (y - minY) * cellSize, cellSize, cellSize);
            g.DrawString(index.ToString(), new Font("Tahoma", 9), Brushes.Black, rectf);

            g.Flush();

        }

        static void DrawChar(int cellSize, Bitmap result, int x, int minX, int y, int minY, char ch)
        {
            var g = Graphics.FromImage(result);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            var sizeF = g.MeasureString(ch.ToString(), new Font("Tahoma", 16));
            var rectf = new RectangleF(1 + (x - minX) * cellSize + (cellSize - sizeF.Width) / 2,
                1 + (y - minY) * cellSize + (cellSize - sizeF.Height) / 2,
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
