using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextCompiler
{
    public enum type
    {
        CONST, SPACE, ID, REAL, SYMBOL, EQUAL, INT, DECIMAL, END, SIGN
    }
    public class Token
    {
        public type type;
        public char name;
        public int code;
        public int position;
        public Token(type _type, char _name, int _position, int _code)
        {
            type = _type;
            name = _name;
            position = _position;
            code = _code;
        }
    }
    
}
