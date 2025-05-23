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
            '+', '-', '*', '/', '_', '=', '>', '&', '|', '~', '$', '%', '\\', '@', '[', ']'
        };
        public List<Token> Tokens = new List<Token>();
        public List<Error> Errors = new List<Error>();

        public Lexer(string text)
        {
            Text = text;
        }
        public void AddToken(char text, int position, int code)
        {
            Tokens.Add(new Token(type.ID, text, position, code + 1));    
        }
        public void ErrorReading(ref int position)
        {
            int startIndex = position;
            string errorText = null;
            while (position < Text.Length && !keyWords.Contains(Text[position]))
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
                if(keyWords.Contains(Text[position]))
                {
                    AddToken(Text[position], position, keyWords.IndexOf(Text[position]));
                    position++;
                    continue;
                }
                else ErrorReading(ref position);
            }
        }

    }
}
