using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TextCompiler
{
    enum State
    {
        Start, Space, Id, IdRem, Real, Equal, Num, Int, IntRem, Decimal, DecRem, End
    }
    public class Parser
    {
        private State state = State.Start;
        private List<Error> errors = new List<Error>();
        private Error currentError = null;
        private bool isError = false;
        private string text;
        private bool isConst = false;
        private string id;
        private string errorText;

        public Parser(string _text)
        {
            text = _text.Replace("\t", " ").Replace("\n", " ");
        }

        public List<Error> GetErrors()
        {
            return errors;
        }
        public void Analyze()
        {
            int position = 0;
            while(position < text.Length)
            {
                if (text[position] == ' ' && isConst == false)
                    position++;
                /*if (isError == false && currentError != null)
                {
                    errors.Add(currentError);
                    currentError = null;
                }*/
                switch (state)
                {
                    case State.Start:
                        if (text.Substring(position).StartsWith("const"))
                        {
                            position += 4;
                            isConst = true;
                        }
                        state = State.Space;
                        break;
                    case State.Space:
                        if (text[position] != ' ' || isConst == false)
                            errors.Add(new Error("Ожидалось ключевое слово const"));
                        isConst = false;
                        state = State.Id;
                        break;
                    case State.Id:
                        if (Regex.IsMatch(text.Substring(position), "^[A-Za-z]"))
                        {
                            id += text[position];
                            state = State.IdRem;
                            isError = false;
                        }
                        else AddError("Неожиданный символ", text[position].ToString(), position);
                        break;
                    case State.IdRem:
                        if (Regex.IsMatch(text.Substring(position), "^[A-Za-z0-9]"))
                        {
                            id += text[position];
                            isError = false;
                        }
                        else if (text[position] == ':')
                        {
                            state = State.Real;
                            isError = false;
                        }
                        else AddError("Неожиданный символ", text[position].ToString(), position);
                        break;
                    case State.Real:
                        if (text.Substring(position).StartsWith("real"))
                            position += 3;
                        else AddError("Ожидалось ключевое слово real", text[position].ToString(), position);
                        state = State.Equal;
                        break;
                    case State.Equal:
                        if (text[position] == '=')
                        {
                            
                            isError = false;
                        }
                        else AddError("Ожидался оператор присваивания", text[position].ToString(), position);
                        state = State.Num;
                        break;
                    case State.Num:
                        if (text[position] == '+' || text[position] == '-')
                        {
                            state = State.Int;
                            isError = false;
                        }
                        else if (char.IsDigit(text[position]))
                        {
                            state = State.IntRem;
                            isError = false;
                        }
                        else AddError("Неожиданный символ", text[position].ToString(), position);
                        break;
                    case State.Int:
                        if (char.IsDigit(text[position]))
                            state = State.IntRem;
                        else AddError("Неожиданный символ", text[position].ToString(), position);
                        break;
                    case State.IntRem:
                        if (char.IsDigit(text[position]))
                        {
                            isError = false;
                            state = State.IntRem;
                        }
                        else if (text[position] == '.')
                        {
                            isError = false;
                            state = State.Decimal;
                        }
                        else if (text[position] == ';')
                        {
                            if (isError && currentError != null)
                            {
                                errors.Add(currentError);
                                currentError = null;
                            }
                            isError = false;
                            state = State.End;
                        }
                        else AddError("Неожиданный символ", text[position].ToString(), position);
                        break;
                    case State.Decimal:
                        if (char.IsDigit(text[position]))
                        {
                            state = State.DecRem;
                            isError = false;
                        }
                        else AddError("Неожиданный символ", text[position].ToString(), position);
                        break;
                    case State.DecRem:
                        if ((char.IsDigit(text[position])))
                        {
                            isError = false;
                            state = State.DecRem;
                        }
                        else if (text[position] == ';')
                        {
                            if (isError && currentError != null) 
                            {
                                errors.Add(currentError);
                                currentError = null;
                            }
                            isError = false;
                            state = State.End;
                        }
                        else AddError("Неожиданный символ", text[position].ToString(), position);
                        break;
                    case State.End:
                        state = State.Start;
                        break;
                }
                position++;
            }
        }
        public void AddError(string text, string symbol, int position)
        {
            if (isError)
                currentError.BeginOfError += symbol;
            else
            {
                if(currentError != null)
                    errors.Add(currentError);
                currentError = new Error(text, symbol, position);
                isError = true;
            }
        }
    }
        
    }

