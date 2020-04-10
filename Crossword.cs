using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
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
        
        List<string> words;

        public void ReadWords(string rawWords)
        {
            words = rawWords.Split('\n', ' ').ToList();
            words.RemoveAll(string.IsNullOrWhiteSpace);
        }

        public List<WordPlacement> Generate()
        {
            Shuffle(words);

            var wordPlacements = new List<WordPlacement>
            {
                new WordPlacement
                    {x = 0, y = 0, isVertical = rng.Next(0, 2) > 0, wordIndex = 0, word = words[0]}
            };

            var a = new List<(string, bool)>(words.ConvertAll(v => (v, false)));
            a[0] = (a[0].Item1, true);
            Backtrack(wordPlacements, a);

            return wordPlacements;
        }

        bool Backtrack(List<WordPlacement> result, List<(string word, bool isPlaced)> words)
        {
            if (words.FindIndex(v => v.isPlaced == false) < 0)
                return true;
            
            for (var wordIndex = 0; wordIndex < this.words.Count; ++wordIndex)
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

        public static Bitmap ToImage(List<WordPlacement> source)
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
            
            var result = new Bitmap(sizeX * cellSize, sizeY * cellSize);

            for (var x = 0; x < sizeX * cellSize; ++x)
                for (var y = 0; y < sizeY * cellSize; ++y)
                    result.SetPixel(x, y, Color.FromArgb(40, 40, 40));

            foreach (var placement in source)
            {
                for (var x = placement.x; x < placement.x + placement.Width; ++x)
                {
                    for (var y = placement.y; y < placement.y + placement.Height; ++y)
                    {
                        DrawCell(cellSize, result, x, minX, y, minY);
                        if (false)
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
                    if (i == 0 || i == cellSize - 1 || j == 0 || j == cellSize - 1)
                        result.SetPixel((x - minX) * cellSize + i, (y - minY) * cellSize + j,
                            Color.FromArgb(180, 180, 180));
                    else
                        result.SetPixel((x - minX) * cellSize + i, (y - minY) * cellSize + j,
                            Color.FromArgb(255, 255, 255));
                }
            }
        }

        static void DrawIndex(int cellSize, Bitmap result, int x, int minX, int y, int minY, int index)
        {
            var g = Graphics.FromImage(result);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            var rectf = new RectangleF((x - minX) * cellSize,
                (y - minY) * cellSize, cellSize, cellSize);
            g.DrawString(index.ToString(), new Font("Tahoma",9), Brushes.Black, rectf);

            g.Flush();

        }
        
        static void DrawChar(int cellSize, Bitmap result, int x, int minX, int y, int minY, char ch)
        {
            var g = Graphics.FromImage(result);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            var sizeF = g.MeasureString(ch.ToString(), new Font("Tahoma", 16));
            var rectf = new RectangleF((x - minX) * cellSize + (cellSize - sizeF.Width) / 2,
                (y - minY) * cellSize + (cellSize - sizeF.Height) / 2, sizeF.Width, sizeF.Height);
            g.DrawString(ch.ToString(), new Font("Tahoma",16), Brushes.Black, rectf);

            g.Flush();

        }
    }
}
