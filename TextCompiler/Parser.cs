using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TextCompiler
{
    public enum State
    {
        Start, Space, Id, IdRem, Real, Equal, Num, Int, IntRem, Decimal, DecRem, End
    }
    public class Parser
    {
        private State state = State.Start;
        private List<Error> errors = new List<Error>();
        private string textError = null;
        public bool isSymbol = false;
        private string text;

        public Parser(string _text)
        {
            text = _text.Replace("\t", " ").Replace("\n", " ");
        }

        public List<Error> GetErrors()
        {
            return errors;
        }
        
        public void AddError(string text, string symbol, int position)
        {
            errors.Add(new Error(text, symbol, position));
            
        }
        public void Analyze()
        {
            Scanner scanner = new Scanner(text);
            scanner.Analyze();
            List<Token> tokens = scanner.GetTokens();
            int position = 0;

            state = State.Start;
            while (position < tokens.Count)
            {
                Token token = tokens[position];

                switch (state)
                {
                    case State.Start:
                        if (token.type != type.CONST)
                        {
                            AddError("Ожидалось ключевое слово 'const'", text[token.position].ToString(), token.position);
                            state = State.Id;
                        }
                        else
                            state = State.Space;
                        break;

                    case State.Space:
                        if (token.type != type.SPACE)
                            AddError("Ожидалось ключевое слово 'const'", text[token.position].ToString(), token.position);
                        state = State.Id;
                        break;

                    case State.Id:
                        if (token.type != type.ID)
                        {
                            var currentState = state;
                            state = GetState(token);
                            textError = token.code;
                            while (position < tokens.Count - 1 && state != State.Id && state != State.IdRem && state != State.Real && state != State.Equal && state != State.Num && state != State.End)
                            {
                                position++;
                                token = tokens[position];
                                textError += token.code;
                                state = GetState(token);
                            }
                            if (state != State.Id)
                            {
                                if (token.type != type.ID)
                                {
                                    var tokenReal = IsValueString(tokens, type.ID);
                                    if (tokenReal != null)
                                    {
                                        position = tokens.IndexOf(tokenReal);
                                        token = tokens[position];
                                        state = GetState(token);
                                        AddError("Неожиданный символ", textError, token.position);
                                    }
                                    else
                                    {
                                        AddErrorForMissingTokens(token, currentState);
                                    }
                                }

                            }
                            continue;
                        }
                        else 
                            state = State.IdRem;
                        break;

                    case State.IdRem:
                        if (token.type != type.SYMBOL)
                        {
                            var currentState = state;
                            state = GetState(token);
                            textError = token.code;
                            while (position < tokens.Count - 1 && state != State.IdRem && state != State.Real && state != State.Equal && state != State.Num && state != State.End)
                            {
                                position++;
                                token = tokens[position];
                                textError += token.code;
                                state = GetState(token);
                            }
                            
                            if (state != State.IdRem)
                            {
                                if (token.type != type.SYMBOL)
                                {
                                    var tokenReal = IsValueString(tokens, type.SYMBOL);
                                    if (tokenReal != null)
                                    {
                                        position = tokens.IndexOf(tokenReal);
                                        token = tokens[position];
                                        state = GetState(token);
                                        AddError("Неожиданный символ", textError, token.position);
                                    }
                                    else
                                    {
                                        AddErrorForMissingTokens(token, currentState);
                                    }
                                }
                                
                            }
                            continue;
                        }
                        else state = State.Real;
                        break;
                    case State.Real:
                        if (token.type != type.REAL)
                        {
                            var currentState = state;
                            state = GetState(token);
                            textError = token.code;
                            while (position < tokens.Count - 1 && state != State.Real && state != State.Equal && state != State.Num && state != State.End)
                            {
                                position++;
                                token = tokens[position];
                                textError += token.code;
                                state = GetState(token);
                            }
                            if (state != State.Real)
                            {
                                if (token.type != type.REAL)
                                {
                                    var tokenReal = IsValueString(tokens, type.REAL);
                                    if (tokenReal != null)
                                    {
                                        position = tokens.IndexOf(tokenReal);
                                        token = tokens[position];
                                        state = GetState(token);
                                        AddError("Неожиданный символ", textError, token.position);
                                    }
                                    else
                                        AddErrorForMissingTokens(token, currentState);
                                }
                                
                            }
                            continue;
                        }
                        else state = State.Equal;
                        
                        break;

                    case State.Equal:
                        if (token.type != type.EQUAL)
                        {
                            var currentState = state;
                            state = GetState(token);
                            textError = token.code;
                            while (position < tokens.Count - 1 && state != State.Equal && state != State.Num && state != State.End)
                            {
                                textError += token.code;
                                position++;
                                token = tokens[position];
                                state = GetState(token);
                            }
                            if (state != State.Equal)
                            {
                                if (token.type != type.EQUAL)
                                {
                                    var tokenReal = IsValueString(tokens, type.EQUAL);
                                    if (tokenReal != null)
                                    {
                                        position = tokens.IndexOf(tokenReal);
                                        token = tokens[position];
                                        state = GetState(token);
                                        AddError("Неожиданный символ", textError, token.position);
                                    }
                                    else
                                        AddErrorForMissingTokens(token, currentState);
                                }
                            }
                            continue;
                        }
                        else state = State.Num;
                        break;

                    case State.Num:
                        if (token.type == type.SIGN && isSymbol == false)
                        {
                            state = State.Num;
                            isSymbol = true;
                            break;
                        }
                        else if (token.type != type.INT && token.type != type.DECIMAL)
                        {
                            textError = null;
                            int positionError = token.position;
                            state = GetState(token);
                            while (position < tokens.Count - 1 && token.type != type.DECIMAL && token.type != type.INT && token.type != type.END)
                            {
                                textError += token.code;
                                position++;
                                token = tokens[position];
                                state = GetState(token);
                            }
                            isSymbol = false;
                            AddError("Неожиданный символ", textError, positionError);
                            if (state != State.Num)
                                AddError("Ожидалось число", text[token.position].ToString(), token.position);
                            continue;
                        }
                        else state = State.End;
                        break;

                    case State.End:
                        if (token.type != type.END)
                            AddError("Ожидалась ';'", text[token.position].ToString(), token.position);
                        state = State.Start;
                        break;
                }
                position++;
            }
            foreach(var error in scanner.errorList)
                errors.Add(error);
        }

        private State GetState(Token token)
        {
            switch(token.type)
            {
                case type.CONST:
                    return State.Id;
                case type.ID:
                    return State.Id;
                case type.SYMBOL:
                    return State.IdRem;
                case type.INT:
                    isSymbol = false;
                    return State.Num;
                case type.DECIMAL:
                    isSymbol = false;
                    return State.Num;
                case type.SIGN:
                    isSymbol = true;
                    return State.Num;
                case type.REAL: 
                    return State.Real;
                case type.EQUAL: 
                    return State.Equal;
                case type.END:
                    return State.End;
                default:
                    return State.End;
            }
        }

        private Token IsValueString(List<Token> tokens, type type)
        {
            foreach(var token in tokens)
            {
                if (token.type == type)
                    return token;
            }
            return null;
        }
        private void AddErrorForMissingTokens(Token token, State state)
        {
            switch(state)
            {
                case State.Id:
                    switch (token.type)
                    {
                        case type.SYMBOL:
                            AddError("Ожидался идентификатор", text[token.position].ToString(), token.position);
                            break;
                        case type.REAL:
                            AddError("Ожидался идентификатор", text[token.position].ToString(), token.position);
                            AddError("Ожидалось :", text[token.position].ToString(), token.position);
                            break;
                        case type.EQUAL:
                            AddError("Ожидался идентификатор", text[token.position].ToString(), token.position);
                            AddError("Ожидалось :", text[token.position].ToString(), token.position);
                            AddError("Ожидалось ключевое слово 'real'", text[token.position].ToString(), token.position);
                            break;
                        case type.INT:
                        case type.DECIMAL:
                            AddError("Ожидался идентификатор", text[token.position].ToString(), token.position);
                            AddError("Ожидалось :", text[token.position].ToString(), token.position);
                            AddError("Ожидалось ключевое слово 'real'", text[token.position].ToString(), token.position);
                            AddError("Ожидался оператор присваивания", text[token.position].ToString(), token.position);
                            break;
                        case type.END:
                            AddError("Ожидался идентификатор", text[token.position].ToString(), token.position);
                            AddError("Ожидалось :", text[token.position].ToString(), token.position);
                            AddError("Ожидалось ключевое слово 'real'", text[token.position].ToString(), token.position);
                            AddError("Ожидался оператор присваивания", text[token.position].ToString(), token.position);
                            AddError("Ожидалось число", text[token.position].ToString(), token.position);
                            break;
                    }
                    break;
                case State.IdRem:
                    switch (token.type)
                    {
                        case type.REAL: 
                            AddError("Ожидалось :", text[token.position].ToString(), token.position);
                            break;
                        case type.EQUAL:
                            AddError("Ожидалось :", text[token.position].ToString(), token.position);
                            AddError("Ожидалось ключевое слово 'real'", text[token.position].ToString(), token.position);
                            break;
                        case type.INT:
                        case type.DECIMAL:
                            AddError("Ожидалось :", text[token.position].ToString(), token.position);
                            AddError("Ожидалось ключевое слово 'real'", text[token.position].ToString(), token.position);
                            AddError("Ожидался оператор присваивания", text[token.position].ToString(), token.position);
                            break;
                        case type.END:
                            AddError("Ожидалось :", text[token.position].ToString(), token.position);
                            AddError("Ожидалось ключевое слово 'real'", text[token.position].ToString(), token.position);
                            AddError("Ожидался оператор присваивания", text[token.position].ToString(), token.position);
                            AddError("Ожидалось число", text[token.position].ToString(), token.position);
                            break;
                        default:
                            token.type = type.END;
                            AddErrorForMissingTokens(token, state);
                            break;
                    }
                    break;
                case State.Real:
                    switch (token.type)
                    {
                        case type.EQUAL:
                            AddError("Ожидалось ключевое слово 'real'", text[token.position].ToString(), token.position);
                            break;
                        case type.INT:
                        case type.DECIMAL:
                            AddError("Ожидалось ключевое слово 'real'", text[token.position].ToString(), token.position);
                            AddError("Ожидался оператор присваивания", text[token.position].ToString(), token.position);
                            break;
                        case type.END:
                            AddError("Ожидалось ключевое слово 'real'", text[token.position].ToString(), token.position);
                            AddError("Ожидался оператор присваивания", text[token.position].ToString(), token.position);
                            AddError("Ожидалось число", text[token.position].ToString(), token.position);
                            break;
                    }
                    break;
                case State.Equal:
                    switch (token.type)
                    {
                        case type.INT:
                        case type.DECIMAL:
                            AddError("Ожидалось ключевое слово 'real'", text[token.position].ToString(), token.position);
                            AddError("Ожидался оператор присваивания", text[token.position].ToString(), token.position);
                            break;
                        case type.END:
                            AddError("Ожидалось ключевое слово 'real'", text[token.position].ToString(), token.position);
                            AddError("Ожидался оператор присваивания", text[token.position].ToString(), token.position);
                            AddError("Ожидалось число", text[token.position].ToString(), token.position);
                            break;
                    }
                    break;
                case State.Num:
                    switch (token.type)
                    {
                        case type.END:
                            AddError("Ожидалось число", text[token.position].ToString(), token.position);
                            break;
                    }
                    break;
            }
        }
    }

}

