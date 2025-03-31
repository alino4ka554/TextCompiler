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
        private Dictionary<char, (int, type)> symbols = new Dictionary<char, (int, type)>()
                    {
                        { ':', (5, type.SYMBOL) },
                        { '=', (6, type.EQUAL) },
                        { '-', (7, type.SIGN) },
                        { '+', (8, type.SIGN) },
                        { ';', (11, type.END) }
                    };
        public List<string> codes = new List<string> ();
        private List<Token> tokens = new List<Token> ();
        public List<Error> errorList = new List<Error> ();
        private bool afterConst = false;
        public DataTable dataTable { get; set; } = new DataTable();
        public Scanner(string text)
        {
            Text = text.Replace("\t", " ").Replace("\n", " ");
            InitializeTable();
        }
        public List<Token> GetTokens()
        {
            return tokens;
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

        private void AddToken(type type, string code, int position)
        {
            tokens.Add(new Token(type, code, position));
        }

        public void ErrorReading(ref int position)
        {
            int startIndex = position;
            string errorText = null;
            while (position < Text.Length && GetStatus(Text[position]) == 5)
            {
                errorText += Text[position].ToString();
                position++;
            }
            errorList.Add(new Error("Неожиданный символ", errorText, startIndex));
        }

        public void Analyze()
        {
            int position = 0;
            int status = 0;

            while (position < Text.Length)
            {
                switch(status)
                {
                    case 0:
                        status = GetStatus(Text[position]);
                        if (status == 2 && !afterConst)
                            position++;
                        break;
                    case 1:
                        int start = position;
                        string word = "";

                        while (position < Text.Length && char.IsLetterOrDigit(Text[position]) && Text[position] >= 48 && Text[position] <= 122)
                            word += Text[position++];
                        int code = KeyWords.TryGetValue(word, out int kwCode) ? kwCode : 2;
                        type type = KeyWords.ContainsKey(word) ? word == "const" ? type.CONST : type.REAL : type.ID;
                        AddToken(type, word, start);
                        afterConst = (word == "const");
                        status = 0;
                        break;
                    case 2:
                        if (afterConst)
                        {
                            AddToken(type.SPACE, Text[position].ToString(), position);
                            afterConst = false;
                            position++;
                            status = 0;
                        }
                        else status = 0;
                        
                        break;
                    case 3:
                        AddToken(symbols[Text[position]].Item2, Text[position].ToString(), position);
                        position++;
                        status = 0;
                        break;
                    case 4:
                        start = position;
                        string number = "";

                        while (position < Text.Length && char.IsDigit(Text[position]))
                            number += Text[position++];

                        if (position + 1 < Text.Length && Text[position] == '.' && char.IsDigit(Text[position + 1]))
                        {
                            number += Text[position++];
                            while (position < Text.Length && char.IsDigit(Text[position]))
                                number += Text[position++];
                        }

                        code = number.Contains(".") ? 10 : 9;
                        AddToken(code == 10 ? type.DECIMAL : type.INT, number, start);
                        status = 0;
                        break;
                    case 5:
                        ErrorReading(ref position);
                        status = 0;
                        break;
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
