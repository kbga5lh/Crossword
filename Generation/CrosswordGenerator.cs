using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;

namespace CrosswordApp
{
    class CrosswordGenerator
    {
        static readonly Random rng = new Random();

        public List<(string word, string definition)> Words;

        public Crossword Generate()
        {
            var result = new Crossword
            {
                name = "Crossword"
            };

            Shuffle(Words);

            var wordPlacements = new List<Crossword.Placement>
            {
                new Crossword.Placement
                    {x = 0, y = 0, isVertical = rng.Next(0, 2) > 0, wordIndex = 0, wordLength = Words[0].word.Length}
            };

            var a = new List<(string, bool)>(Words.ConvertAll(v => (v.word, false)));
            a[0] = (a[0].Item1, true);
            Backtrack(wordPlacements, a);

            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minY = int.MaxValue;
            var maxY = int.MinValue;
            foreach (var placement in wordPlacements)
            {
                minX = Math.Min(placement.x, minX);
                maxX = Math.Max(placement.x + placement.Width, maxX);
                minY = Math.Min(placement.y, minY);
                maxY = Math.Max(placement.y + placement.Height, maxY);
            }

            result.words = new List<(string word, string definition)>();
            result.placements = new List<Crossword.Placement>();
            int i = 1;
            foreach (var placement in wordPlacements)
            {
                var samePlacementPos = result.placements.FindIndex(v => v.x == placement.x - minX && v.y == placement.y - minY);
                var p = new Crossword.Placement
                {
                    x = placement.x - minX,
                    y = placement.y - minY,
                    isVertical = placement.isVertical,
                    index = samePlacementPos >= 0 ? (result.placements[samePlacementPos].index) : i++,
                };

                result.words.Add(Words[placement.wordIndex]);
                p.wordIndex = result.words.Count - 1;
                p.wordLength = result.words[p.wordIndex].word.Length;

                result.placements.Add(p);
            }

            return result;
        }

        bool Backtrack(List<Crossword.Placement> result, List<(string word, bool isPlaced)> words)
        {
            if (words.FindIndex(v => v.isPlaced == false) < 0)
                return true;

            for (var wordIndex = 0; wordIndex < Words.Count; ++wordIndex)
            {
                if (words[wordIndex].isPlaced)
                    continue;

                for (var i = 0; i < result.Count; i++)
                {
                    Crossword.Placement placement = result[i];
                    for (var charInPlacementIndex = 0; charInPlacementIndex < placement.wordLength; charInPlacementIndex++)
                    {
                        var charInPlacement = words[placement.wordIndex].word[charInPlacementIndex];
                        for (var charInWordIndex = 0; charInWordIndex < words[wordIndex].word.Length; charInWordIndex++)
                        {
                            var charInWord = words[wordIndex].word[charInWordIndex];
                            if (charInPlacement != charInWord) continue;

                            var newPlacement = new Crossword.Placement
                            {
                                wordIndex = wordIndex,
                                isVertical = !placement.isVertical,
                                wordLength = words[wordIndex].word.Length,
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

        bool IsPlacementCorrect(Crossword.Placement placement, List<Crossword.Placement> placements)
        {
            foreach (var p in placements)
            {
                if (IntersectsWith(p.Rect, placement.Rect))
                {
                    if (p.isVertical == placement.isVertical)
                        return false;
                }
                else if (IsCloseTo(p, placement))
                    return false;
                if (!IsIntersectionRight(p, placement))
                    return false;
            }

            return true;
        }

        bool IsIntersectionRight(Crossword.Placement placement, Crossword.Placement other)
        {
            for (var i = 0; i < placement.wordLength; ++i)
            {
                for (var j = 0; j < other.wordLength; ++j)
                {
                    var p1 = new Point(placement.x + (placement.isVertical ? 0 : i), placement.y + (placement.isVertical ? i : 0));
                    var p2 = new Point(other.x + (other.isVertical ? 0 : j), other.y + (other.isVertical ? j : 0));
                    if (p1 == p2 && Words[placement.wordIndex].word[i] != Words[other.wordIndex].word[j])
                        return false;
                }
            }

            return true;
        }

        public bool IsCloseTo(Crossword.Placement placement, Crossword.Placement other)
        {
            return IntersectsWith(new Rect(placement.x - 1, placement.y - 1, placement.Width + 2, placement.Height + 2), other.Rect);
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
    }
}
