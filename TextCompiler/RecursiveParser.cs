using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TextCompiler
{
    public class RecursiveParser
    {
        public List<Token> Tokens = new List<Token>();
        public List<string> Out = new List<string>();
        private List<char> keyWords = new List<char>
        {
            '+', '-', '*', '/', '_', '=', '>', '&', '|', '~', '$', '%', '\\', '@',
        };
        public List<Error> Errors = new List<Error>();
        int i = -1;
        public List<bool> flags = new List<bool>();
        public string Text;
        private int position = 0;

        public RecursiveParser(string text)
        {
            Text = text.Replace("\t", "").Replace("\n", "").Replace(" ", "");
            Lexer lexer = new Lexer(Text);
            lexer.Analyze();
            Tokens = lexer.Tokens;
            Errors = lexer.Errors;
        }
        public void AddError(string text, string symbol, int position)
        {
            Errors.Add(new Error(text, symbol, position));
            //this.position++;
        }
        public void Parse()
        {
            Program();
        }
        public void Program()
        {
            Out.Add("Program");
            if (position >= Tokens.Count)
            {
                Out.Add("ε");
                return;
            }
            else if (Tokens[position].name == ']')
            {
                Out.Add("ε");
            }
            else
            {
                Instr();
                Program();
            }
        }
        public void Instr()
        {
            if (position >= Tokens.Count)
                return;
            Out.Add("Instr");
            if (keyWords.Contains(Tokens[position].name))
            {
                Out.Add(Tokens[position].name.ToString());
                position++;
            }
            else if (position < Tokens.Count && Tokens[position].name == '[')
            {
                Out.Add(Tokens[position].name.ToString());
                position++;
                Program();
                if (position >= Tokens.Count)
                    AddError("Ожидалась закрывающаяся скобка ']'", Tokens[Tokens.Count - 1].name.ToString(), Tokens[Tokens.Count - 1].position);
                else if (Tokens[position].name.ToString() != "]")
                    AddError("Ожидалась закрывающаяся скобка ']'", Tokens[position].name.ToString(), Tokens[position].position);
                else
                    Out.Add(Tokens[position].name.ToString());
                position++;
            }
        }
    }
}
