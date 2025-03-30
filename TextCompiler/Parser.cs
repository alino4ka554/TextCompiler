﻿using System;
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
        private string text;

        public Parser(string _text)
        {
            text = _text.Replace("\t", " ").Replace("\n", " ");
        }

        public List<Error> GetErrors()
        {
            return errors;
        }
        
        public void AddError(string text, string symbol, int position, ref int index)
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
                            AddError("Ожидалось ключевое слово 'const'", text[token.position].ToString(), token.position, ref position);
                            state = State.Id;
                        }
                        else
                            state = State.Space;
                        break;

                    case State.Space:
                        if (token.type != type.SPACE)
                            AddError("Ожидалось ключевое слово 'const'", text[token.position].ToString(), token.position, ref position);
                        state = State.Id;
                        break;

                    case State.Id:
                        if (token.type != type.ID)
                            AddError("Ожидался идентификатор", text[token.position].ToString(), token.position, ref position);
                        state = State.Real;
                        break;

                    case State.Real:
                        if (token.type == type.SYMBOL)
                        {
                            state = State.Real;
                            break;
                        }
                        else if (token.type != type.REAL)
                        {
                            state = GetState(token);
                            while (position < tokens.Count - 1 && state != State.Real && state != State.Equal && state != State.Num && state != State.End)
                            {
                                position++;
                                token = tokens[position];
                                state = GetState(token);
                            }
                            if (state != State.Real)
                                AddError("Ожидалось ключевое слово 'real'", text[token.position].ToString(), token.position, ref position);
                            continue;
                        }
                        else state = State.Equal;
                        
                        break;

                    case State.Equal:
                        if (token.type != type.EQUAL)
                        {
                            state = GetState(token);
                            while (position < tokens.Count - 1 && state != State.Equal && state != State.Num && state != State.End)
                            {
                                position++;
                                token = tokens[position];
                                state = GetState(token);
                            }
                            if (state != State.Equal)
                                AddError("Ожидался оператор присваивания", text[token.position].ToString(), token.position, ref position);
                            continue;
                        }
                        else state = State.Num;
                        break;

                    case State.Num:
                        if (token.type == type.SIGN)
                        {
                            state = State.Num;
                            break;
                        }
                        else if (token.type != type.INT && token.type != type.DECIMAL)
                        {
                            
                            state = GetState(token);
                            while (position < tokens.Count - 1 && state != State.Num && state != State.End)
                            {
                                position++;
                                token = tokens[position];
                                state = GetState(token);
                            }
                            if (state != State.Num)
                                AddError("Ожидалось число", text[token.position].ToString(), token.position, ref position);
                            continue;

                        }
                        else state = State.End;
                        break;

                    case State.End:
                        if (token.type != type.END)
                            AddError("Ожидалась ';'", text[token.position].ToString(), token.position, ref position);
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
                case type.INT:
                    return State.Num;
                case type.DECIMAL: 
                    return State.Num;
                case type.SIGN:
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

    }

}

