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
        public int code;
        public int position;
        public Token(type _type, int _code, int _position)
        {
            type = _type;
            code = _code;
            position = _position;
        }
    }
    
}
