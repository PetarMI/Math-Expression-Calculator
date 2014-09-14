using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

class ExpressionCalculator
{
    private static List<char> arithmeticOperators = new List<char> { '+', '-', '*', '/'};
    private static HashSet<String> arithOperators = new HashSet<String> { "+", "-", "*", "/" };
    private static List<String> functions = new List<String> { "ln", "pow", "sqrt" }; 

    static void Main(string[] args)
    {
        string input = TrimInput("powp(2, 3.14) * (3 - (3 * sqrt(2) - 3.2) + 1.5*0.3) ");
        Console.WriteLine(input);

        try
        {
            var rpn = SeparateTokens(input);
            var reversed = ConvertToReversePolishNotation(rpn);
            Console.WriteLine(CalcuateRPN(reversed));
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
        }
    }

    static double CalcuateRPN(Queue<String> input)
    {
        Stack<double> stack = new Stack<double>();
        double firstValue;
        double secondValue;
        double result;
        String currentToken;

        while (input.Count > 0)
        {
            currentToken = input.Dequeue();

            if (double.TryParse(currentToken, out result))
            {
                stack.Push(result);
            }
            else
            {
                if (arithOperators.Contains(currentToken))
                {
                    if (stack.Count < 2)
                    {
                        throw new ArgumentException("Invalid expression in reverse polish notation");        
                    }
                    if (currentToken == "+")
                    {
                        firstValue = stack.Pop();
                        secondValue = stack.Pop();
                        stack.Push(firstValue + secondValue);
                    }
                    else if (currentToken == "-")
                    {
                        firstValue = stack.Pop();
                        secondValue = stack.Pop();
                        stack.Push(secondValue - firstValue);
                    }
                    else if (currentToken == "*")
                    {
                        firstValue = stack.Pop();
                        secondValue = stack.Pop();
                        stack.Push(secondValue * firstValue);
                    }
                    else
                    {
                        firstValue = stack.Pop();
                        secondValue = stack.Pop();
                        stack.Push(secondValue / firstValue);
                    }
                }
                else if (functions.Contains(currentToken))
                {
                    if (currentToken == "ln")
                    {
                        if (stack.Count < 1)
                        {
                            throw new ArgumentException("Invalid expression in reverse polish notation"); 
                        }

                        firstValue = stack.Pop();
                        stack.Push(Math.Log(firstValue));
                    }
                    else if (currentToken == "pow")
                    {
                        if (stack.Count < 2)
                        {
                            throw new ArgumentException("Invalid expression in reverse polish notation");
                        }

                        firstValue = stack.Pop();
                        secondValue = stack.Pop();
                        stack.Push(Math.Pow(secondValue, firstValue));
                    }
                    else if (currentToken == "sqrt")
                    {
                        if (stack.Count < 1)
                        {
                            throw new ArgumentException("Invalid expression in reverse polish notation");
                        }

                        firstValue = stack.Pop();
                        stack.Push(Math.Sqrt(firstValue));
                    }
                }
                else
                {
                    throw new ArgumentException("Unrecognised symbol in reverse polish notation");
                }
            }
        }

        if (stack.Count == 1)
        {
            return stack.Pop();
        }
        else
        {
            throw new ArgumentException("Input has too many values.");
        }
    }

    public static Queue<String> ConvertToReversePolishNotation(List<String> expression)
    {
        Queue<String> output = new Queue<String>();
        Stack <String> stack = new Stack<String>();

        for (int i = 0; i < expression.Count; i++)
        {
            String currentToken = expression[i];

            double number;

            if (double.TryParse(currentToken, out number))
            {
                output.Enqueue(currentToken);
            }
            else if (functions.Contains(currentToken))
            {
                stack.Push(currentToken);
            }
            else if (currentToken == ",")
            {
                if (!stack.Contains("("))
                {
                    throw new ArgumentException("Invalid bracket or misplaced function separator");
                }

                while (stack.Peek() != "(")
                {
                    output.Enqueue(stack.Pop());
                }
            }
            else if (arithOperators.Contains(currentToken))
            {
                while (stack.Count > 0 && arithOperators.Contains(stack.Peek()) && (OperatorPrecedence(currentToken) <= OperatorPrecedence(stack.Peek())))
                {
                    output.Enqueue(stack.Pop());
                }

                stack.Push(currentToken);
            }
            else if (currentToken == "(")
            {
                stack.Push(currentToken);
            }
            else if (currentToken == ")")
            {
                if (!stack.Contains("("))
                {
                    throw new ArgumentException("Invalid brackets");
                }

                while (stack.Peek() != "(")
                {
                    output.Enqueue(stack.Pop());
                }

                stack.Pop();

                if (stack.Count > 0 && functions.Contains(stack.Peek()))
                {
                    output.Enqueue(stack.Pop());
                }
            }
            else
            {
                throw new ArgumentException("Invalid expression");
            }
        }

        while (stack.Count > 0)
        {
            if (stack.Peek() == "(" || stack.Peek() == ")")
            {
                throw new ArgumentException("Invalid brackets");
            }

            output.Enqueue(stack.Pop());
        }

        return output;
    }

    public static List<String> SeparateTokens(String input)
    {
        List<String> tokens = new List<string>();
        StringBuilder number = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '-' && (i == 0 || input[i - 1] == '(' || input[i - 1] == ',' || arithmeticOperators.Contains(input[i - 1])))
            {
                number.Append('-');
            }
            else if (Char.IsDigit(input[i]))
            {
                while (i < input.Length && (Char.IsDigit(input[i]) || input[i] == '.'))
                {
                   number.Append(input[i]);
                   i++;
                }
                tokens.Add(number.ToString());
                number.Clear();
                i--;
            }
            else if (input[i] == '(')
            {
                tokens.Add("(");
            }
            else if (input[i] == ')')
            {
                tokens.Add(")");
            }
            else if (arithmeticOperators.Contains(input[i]))
            {
                tokens.Add(input[i].ToString());
            }
            else if (input[i] == ',')
            {
                tokens.Add(",");
            }
            else if (input[i] == 'l' && i < (input.Length - 1) && input[i + 1] == 'n')
            {
                tokens.Add("ln");
                i++;
            }
            else if (input[i] == 'p' && (i + 2 < input.Length - 1) && input.Substring(i, 3) == "pow")
            {
                tokens.Add("pow");
                i += 2;
            }
            else if (input[i] == 's' && (i + 3 < input.Length - 1) && input.Substring(i, 4) == "sqrt")
            {
                tokens.Add("sqrt");
                i += 3;
            }
        }

        if (number.Length > 0)
        {
            tokens.Add(number.ToString());
        }

        return tokens;
    }

    private static String TrimInput(String expression)
    {
        StringBuilder validExpression = new StringBuilder();

        for (int i = 0; i < expression.Length; i++)
        {
            if (expression[i] != ' ')
            {
                validExpression.Append(expression[i]);
            }
        }

        return validExpression.ToString();
    }

    private static int OperatorPrecedence(String arithmeticOperator)
    {
        if (arithmeticOperator == "+" || arithmeticOperator == "-")
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }

    private static void SetInvariantCulture()
    {
        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    }
}
