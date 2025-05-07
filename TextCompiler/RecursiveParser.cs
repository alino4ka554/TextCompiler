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
            E();
            if (position < Tokens.Count)
            {
                    AddError("Лишние символы после конца выражения", Tokens[position].code, Tokens[position].position);
            }
        }
        public void E()
        {
            T();
            while (position < Tokens.Count)
            {
                if (Tokens[position].code == "+" || Tokens[position].code == "-")
                {
                    position++;
                    T();
                }
                else if (Tokens[position].code == ")" && i == -1)
                {
                    AddError("Закрывающая скобка без соответствующей открывающей", Tokens[position].code, Tokens[position].position+1);
                    position++; 
                }
                else break;
            }
        }
        public void T()
        {
            O();
            while (position < Tokens.Count)
            {
                if (Tokens[position].code == "*" || Tokens[position].code == "/")
                {
                    position++;
                    O();
                }
                else if (Tokens[position].code == ")" && i == -1)
                {
                    AddError("Закрывающая скобка без соответствующей открывающей", Tokens[position].code, Tokens[position].position+1);
                    position++; 
                }
                else break;
            }
        }
        public void O()
        {
            if(position >= Tokens.Count)
                AddError("Ожидался операнд", Tokens[Tokens.Count - 1].code, Tokens[Tokens.Count - 1].position);
            else if (Int32.TryParse(Tokens[position].code, out int result) == true)
                position++;
            else if (Tokens[position].code == "(")
            {
                position++;
                i++;
                flags.Add(true);
                E();
                if (position >= Tokens.Count)
                    AddError("Ожидалась закрывающаяся скобка ')'", Tokens[Tokens.Count - 1].code, Tokens[Tokens.Count - 1].position);
                else if (Tokens[position].code != ")")
                    AddError("Ожидалась закрывающаяся скобка ')'", Tokens[position].code, Tokens[position].position);
                else 
                    position++;
                flags.RemoveAt(i);
                i--;
            }
            else
            {
                AddError("Ожидался операнд", Tokens[position].code, Tokens[position].position);

            }
        }
    }
}
