using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CFramework
{
    public static class TextUtilities
    {
        /// <summary>
        /// Example Use case
        /// string original = "Find: 5/15";
        /// var colored = TextUtilities.ColorizeText(original, new List<(int, int, Color)> {
        /// (6, 1, Color.green),     // số 5 màu xanh lá
        /// (8, 2, Color.red)        // số 15 màu đỏ chẳng hạn
        /// });
        /// textMeshProUGUI.text = colored;
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="colorSegments"></param>
        /// <returns></returns>
        public static string ColorizeText(string text, List<(int start, int length, Color color)> colorSegments)
        {
            // Sắp xếp để tránh lỗi khi chèn tag làm lệch vị trí các đoạn sau
            colorSegments.Sort((a, b) => a.start.CompareTo(b.start));
            var sb = new StringBuilder(text);

            int offset = 0;
            foreach (var (start, length, color) in colorSegments)
            {
                string hexColor = ColorUtility.ToHtmlStringRGB(color);
                string openTag = $"<color=#{hexColor}>";
                string closeTag = "</color>";

                sb.Insert(start + offset, openTag);
                offset += openTag.Length;

                sb.Insert(start + length + offset, closeTag);
                offset += closeTag.Length;
            }

            return sb.ToString();
        }

        public static string ColorizeText(string msg, Color? color)
        {
            Color c = color.HasValue ? color.Value : Color.white;

            return $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>" + msg + "</color>";
        }
        
        public static string ColorizeText(int msg, Color? color)
        {
            Color c = color.HasValue ? color.Value : Color.white;

            return $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>" + msg + "</color>";
        }
        
        public static string ColorizeByKeyword(string text, List<(string keyword, Color color)> highlights, bool onlyFirstMatch = true)
        {
            var sb = new StringBuilder(text);
            int offset = 0;

            foreach (var (keyword, color) in highlights)
            {
                string hexColor = ColorUtility.ToHtmlStringRGB(color);
                string openTag = $"<color=#{hexColor}>";
                string closeTag = "</color>";

                int searchStart = 0;
                while (searchStart < sb.Length)
                {
                    int index = sb.ToString().IndexOf(keyword, searchStart, StringComparison.Ordinal);
                    if (index < 0) break;

                    sb.Insert(index + keyword.Length, closeTag);
                    sb.Insert(index, openTag);

                    offset = openTag.Length + closeTag.Length;
                    searchStart = index + keyword.Length + offset;

                    if (onlyFirstMatch) break;
                }
            }

            return sb.ToString();
        }
        
        public static string ToClockFormatMMSS(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        public static string ToClockFormatHHMMSS(float time)
        {
            int hours = Mathf.FloorToInt(time / 3600f);
            int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }
    }
}
