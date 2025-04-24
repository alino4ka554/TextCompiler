using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TextCompiler
{
    public class PolishNotation
    {
        public Stack<string> Operations = new Stack<string>();
        public List<string> PolishVersion = new List<string>();
        public Stack<string> Operands = new Stack<string>();
        public List<Token> Tokens = new List<Token>();
        public PolishNotation(List<Token> tokens)
        {
            this.Tokens = tokens;
        }
        public void WritePolishNotation()
        {
            foreach (var token in Tokens)
            {
                if(GetPriority(token.code) == 10)
                    PolishVersion.Add(token.code);
                else
                {
                    if (Operations.Count == 0)
                        Operations.Push(token.code);
                    else
                    {
                        if (GetPriority(token.code) != 1)
                        {
                            if (GetPriority(Operations.Peek()) < GetPriority(token.code) || GetPriority(token.code) == 0)
                                Operations.Push(token.code);
                            else
                            {
                                string operation = "";
                                while (Operations.Count > 0 && GetPriority(operation) >= GetPriority(token.code))
                                {
                                    operation = Operations.Pop();
                                    PolishVersion.Add(operation);
                                }
                                Operations.Push(token.code);
                            }
                        }
                        else
                        {
                            string operation = Operations.Pop(); 
                            while (Operations.Count > 0 && GetPriority(operation) != 0)
                            {
                                PolishVersion.Add(operation);
                                operation = Operations.Pop();
                            }
                        }
                    }
                }
            }
            while (Operations.Count > 0)
                PolishVersion.Add(Operations.Pop());
                
        }
        public void CalculationPolishNotation()
        {
            foreach(var str in PolishVersion)
            {
                if(GetPriority(str) == 10)
                    Operands.Push(str);
                else
                {
                    double number = CalculationOperands(str, Operands.Pop(), Operands.Pop());
                    Operands.Push(number.ToString());
                }
            }
        }
        public double CalculationOperands(string operation, string number1, string number2)
        {
            double a = double.Parse(number1);
            double b = double.Parse(number2);
            switch(operation)
            {
                case "+":
                    return a + b;
                case "-":
                    return b - a;
                case "*": 
                    return a * b;
                case "/":
                    return b / a;
                default: 
                    return a;
            }
        }
        public int GetPriority(string token)
        {
            switch(token)
            {
                case "+":
                case "-":
                    return 7;
                case "*": 
                case "/":
                    return 8;
                case "(":
                    return 0;
                case ")":
                    return 1;
                default:
                    return 10;
            }
        }
    }
}
