using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using static System.Windows.Forms.AxHost;

namespace TextCompiler
{
    public class RegularExpression
    {
        public Dictionary<int, string> Substrings = new Dictionary<int, string>();
        Dictionary<int, string> patterns = new Dictionary<int, string>
        {
            {0, @"[\\$a-zA-Z_][a-zA-Z]*" },
            //{0, @"(?=.*[А-Я])(?=.*[а-я])(?=.*[0-9])(?=.*[#?!|@/$%^&*\-_])[А-Яа-я0-9#?!|@$%^&*\-_]{8,}"},
            {1, @"[А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+" },
            {2, @"([0-9A-Fa-f]{2}-){5}[0-9A-Fa-f]{2}" }
        };

        public void SearchRegularExpression(string text, int number)
        {
            foreach (Match match in Regex.Matches(text, patterns[number]))
            {
                Substrings.Add(match.Index, match.Value);
            }
        }
        private bool IsHexChar(char c)
        {
            return char.IsDigit(c) || (char.ToLower(c) >= 'a' && char.ToLower(c) <= 'f');
        }

        public void SearchMAC(string _text)
        {
            string text = _text.Replace("\t", " ").Replace("\n", " ");
            Substrings.Clear();
            for (int start = 0; start <= text.Length - 17; start++)  
            {
                int state = 0;
                int pos = start;
                while (pos < text.Length && state != -1)
                {
                    char c = text[pos];
                    switch (state)
                    {
                        case 0:
                        case 3:
                        case 6:
                        case 9:
                        case 12:
                        case 15:
                            if (IsHexChar(c)) state++;
                            else state = -1;
                            break;
                        case 1:
                        case 4:
                        case 7:
                        case 10:
                        case 13:
                        case 16:
                            if (IsHexChar(c)) state++;
                            else state = -1;
                            break;
                        case 2:
                        case 5:
                        case 8:
                        case 11:
                        case 14:
                            if (c == '-') state++;
                            else state = -1;
                            break;
                        default:
                            state = -1;
                            break;
                    }

                    if (state == 17)
                    {
                        Substrings[start] = text.Substring(start, 17);
                        break; 
                    }
                    pos++;
                }
            }
        }

    }
}

