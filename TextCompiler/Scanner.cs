using System;
using System.Collections.Generic;
using System.Data;

namespace TextCompiler
{
    public class Scanner
    {
        public string Text;
        private readonly Dictionary<string, int> KeyWords = new Dictionary<string, int>()
        {
            { "const", 1 },
            { "real", 3 }
        };
        private Dictionary<char, (int, string)> symbols = new Dictionary<char, (int, string)>()
                    {
                        { ':', (5, "символ") },
                        { '=', (6, "оператор присваивания") },
                        { '-', (7, "знак -") },
                        { '+', (8, "знак +") },
                        { ';', (11, "конец оператора") }
                    };
        public List<string> codes = new List<string> ();
        private bool afterConst = false;
        public DataTable dataTable { get; set; } = new DataTable();
        public Scanner(string text)
        {
            Text = text;
            InitializeTable();
        }

        private void InitializeTable()
        {
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn("Статус", typeof(int)),
                new DataColumn("Тип", typeof(string)),
                new DataColumn("Значение", typeof(string)),
                new DataColumn("Позиция", typeof(string))
            });
        }

        private void AddToken(int code, string type, string value, int start, int end)
        {
            dataTable.Rows.Add(code, type, value, $"с {start + 1} по {end} символ");
            for(int i = start; i < end; i++)
                if (code != 12)
                    codes.Add(code.ToString());
        }

        public void Analyze()
        {
            string str = Text.Replace("\t", " ").Replace("\n", " ");
            int position = 0;
            int status = 0;

            while (position < str.Length)
            {
                switch(status)
                {
                    case 0:
                        status = GetStatus(str[position]);
                        if (codes.Count == 0)
                            codes.Add("0");
                        else if (status != 5)
                        {
                            if (status == 2 && !afterConst)
                                position++;
                            else
                                codes.Add("START");
                        }
                        break;
                    case 1:
                        int start = position;
                        string word = "";

                        while (position < str.Length && char.IsLetterOrDigit(str[position]) && str[position] >= 48 && str[position] <= 122)
                            word += str[position++];
                        int code = KeyWords.TryGetValue(word, out int kwCode) ? kwCode : 2;
                        string type = KeyWords.ContainsKey(word) ? "ключевое слово" : "идентификатор";
                        AddToken(code, type, word, start, position);
                        afterConst = (word == "const");
                        status = 6;
                        break;
                    case 2:
                        if (afterConst)
                        {
                            AddToken(4, "разделитель", " ", position, position + 1);
                            afterConst = false;
                            position++;
                            status = 6;
                        }
                        else status = 0;
                        
                        break;
                    case 3:
                        AddToken(symbols[str[position]].Item1, symbols[str[position]].Item2, str[position].ToString(), position, position + 1);
                        position++;
                        status = 6;
                        break;
                    case 4:
                        start = position;
                        string number = "";

                        while (position < str.Length && char.IsDigit(str[position]))
                            number += str[position++];

                        if (position + 1 < str.Length && str[position] == '.' && char.IsDigit(str[position + 1]))
                        {
                            number += str[position++];
                            while (position < str.Length && char.IsDigit(str[position]))
                                number += str[position++];
                        }

                        code = number.Contains(".") ? 10 : 9;
                        AddToken(code, code == 10 ? "вещественное число" : "целое без знака", number, start, position);
                        status = 6;
                        break;
                    case 5:
                        AddToken(12, "ERROR: недопустимый символ", str[position].ToString(), position, position + 1);
                        return;
                    case 6:
                        codes.Add("OUT");
                        status = 0;
                        break;

                }
            }
            
        }

        private int GetStatus(char c)
        {
            if (char.IsLetter(c) && c >= 65 && c <= 122)
                return 1;
            else if (char.IsWhiteSpace(c))
                return 2;
            else if (symbols.ContainsKey(c))
                return 3;
            else if (char.IsDigit(c))
                return 4;
            else
                return 5;
        }
    }
}
