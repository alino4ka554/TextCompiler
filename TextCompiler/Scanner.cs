using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextCompiler
{
    public class Scanner
    {
        public string Text;
        public Dictionary<string, int> KeyWords = new Dictionary<string, int> 
        { 
            { "const", 1 }, 
            { "real", 3 } 
        };
        public List<string> Errors = new List<string>();
        public List<(int, string, string, string)> values = new List<(int, string, string, string)>();
        public string Error;
        public DataTable dataTable { get; set; } = new DataTable();
        public Scanner(string text)
        {
            Text = text;
            InitializeTable();
        }

        private void InitializeTable()
        {
            dataTable.Columns.Add("Статус", typeof(int));
            dataTable.Columns.Add("Тип", typeof(string));
            dataTable.Columns.Add("Значение", typeof(string));
            dataTable.Columns.Add("Позиция", typeof(string));
        }
        public void Analyze()
        {
            string str = Text.Replace("\t", "").Replace("\n", ""); 
            int status = 0;
            int position = 0;
            while (position < str.Length)
            {
                switch(status)
                {
                    case 0:
                        if (char.IsLetter(str[position]) && str[position] >= 65 && str[position] <= 122)
                            status = 1;
                        else if (char.IsWhiteSpace(str[position]))
                        {
                            if (position >= 5 && str.Substring(position - 5, 5).Contains("const"))
                                status = 2;
                            else
                                position++;
                        }
                        else if (str[position] == ':')
                            status = 3;
                        else if (str[position] == '=')
                            status = 4;
                        else if (char.IsDigit(str[position]))
                            status = 7;
                        else if (str[position] == '-')
                            status = 5;
                        else if (str[position] == '+')
                            status = 6;
                        else if (str[position] == ';')
                            status = 8;
                        else
                        {
                            Error = str[position].ToString();
                            status = 9;
                        }
                        break;
                    case 1:
                        string word = "";
                        int beginIndex = position;
                        while (position < str.Length && 
                            ((char.IsLetterOrDigit(str[position]) && str[position] >= 48 && str[position] <= 122) 
                            || str[position] == '_'))
                        {
                            word += str[position];
                            position++;
                        }
                        if (KeyWords.ContainsKey(word))
                        {
                            dataTable.Rows.Add(KeyWords[word], "ключевое слово", word, $"с {beginIndex + 1} по {position} символ");
                        }
                        else
                        {
                            dataTable.Rows.Add(2, "идентификатор", word, $"с {beginIndex + 1} по {position} символ");
                        }
                        status = 0;
                        break;
                    case 2:
                        dataTable.Rows.Add(4, "разделитель", str[position], $"с {position + 1} по {position + 1} символ");
                        position++;
                        status = 0;
                        break;
                    case 3:
                        dataTable.Rows.Add(5, "символ", str[position], $"с {position + 1} по {position + 1} символ");
                        position++;
                        status = 0;
                        break;
                    case 4:
                        dataTable.Rows.Add(6, "оператор присваивания", str[position], $"с {position + 1} по {position + 1} символ");
                        position++;
                        status = 0;
                        break;
                    case 5:
                        dataTable.Rows.Add(7, "знак -", str[position], $"с {position + 1} по {position + 1} символ");
                        position++;
                        status = 0;
                        break;
                    case 6:
                        dataTable.Rows.Add(8, "знак +", str[position], $"с {position + 1} по {position + 1} символ");
                        position++;
                        status = 0;
                        break;
                    case 7:
                        string number = "";
                        beginIndex = position;
                        while (position < str.Length && char.IsDigit(str[position]))
                        {
                            number += str[position];
                            position++;
                        }
                        if (position + 1 < str.Length && str[position] == '.')
                        {
                            if (char.IsDigit(str[position + 1]))
                            {
                                number += str[position];
                                position++;
                                while (position < str.Length && char.IsDigit(str[position]))
                                {
                                    number += str[position];
                                    position++;
                                }
                            }
                        }
                        if (number.IndexOf(".") == -1)
                            dataTable.Rows.Add(9, "целое без знака", number, $"с {beginIndex + 1} по {position} символ");
                        else 
                            dataTable.Rows.Add(10, "вещественное число", number, $"с {beginIndex + 1} по {position} символ");
                        status = 0;
                        break;
                    case 8:
                        dataTable.Rows.Add(11, "конец оператора", str[position], $"с {position + 1} по {position + 1} символ");
                        position++;
                        status = 0;
                        break;
                    default:
                        dataTable.Rows.Add(12, "ERROR: недопустимый символ", Error, $"с {position + 1} по {position + 1} символ");
                        position++;
                        status = 0;
                        break;
                }
            }

        }

    }
}
