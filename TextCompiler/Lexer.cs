using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextCompiler
{
    public class Lexer
    {
        public string Text;
        private List<char> keyWords = new List<char>
        {
            '*', '/', '+', '-', '(', ')'
        };
        public List<Token> Tokens = new List<Token>();
        public List<Error> Errors = new List<Error>();

        public Lexer(string text)
        {
            Text = text;
        }
        public void AddToken(string text, int position)
        {
            Tokens.Add(new Token(type.ID, text, position));    
        }
        public void ErrorReading(ref int position)
        {
            int startIndex = position;
            string errorText = null;
            while (position < Text.Length && !Char.IsDigit(Text[position]) && !keyWords.Contains(Text[position]))
            {
                errorText += Text[position].ToString();
                position++;
            }
            Errors.Add(new Error("Неожиданный символ", errorText, startIndex));
        }
        public void Analyze()
        {
            int position = 0;
            while(position < Text.Length)
            {
                if (Char.IsDigit(Text[position]))
                {
                    int startPosition = position;
                    string number = "";
                    while (position < Text.Length)
                    {
                        if (Char.IsDigit(Text[position]))
                            number += Text[position++];
                        else if (!keyWords.Contains(Text[position]))
                            ErrorReading(ref position);
                        else break;
                    }
                    AddToken(number, startPosition);
                    continue;
                }
                else if(keyWords.Contains(Text[position]))
                {
                    AddToken(Text[position].ToString(), position);
                    position++;
                    continue;
                }
                else ErrorReading(ref position);
            }
        }

    }
}
