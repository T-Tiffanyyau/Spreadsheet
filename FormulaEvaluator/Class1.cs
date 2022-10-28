using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    /// <summary>
    /// This is an in-fix calculator.
    /// The legal tokens are the four operator symbols (+ - * /), left parentheses, right parentheses, 
    /// non-negative integers, whitespace, and variables consisting of one or more letters followed by one or more digits. 
    /// </summary>
    public static class Evaluator
    {
        /// <summary>
        /// Given a variable name as its parameter, the delegate will either return an integer
        /// or throw an ArgumentException (if the variable has no value).
        /// </summary>
        /// <param name="v"> The variable that needs to be lookup </param>
        /// <returns> The value of the variable </returns>
        public delegate int Lookup(String v);



        /// <summary>
        /// The method should evaluate the expression. It should return the value of the expression if it has a value, 
        /// or throw an ArgumentException if any of the possible errors from the algorithm occurs.
        /// </summary>
        /// <param name="exp"> The input expression that needs to be calculated </param>
        /// <param name="variableEvaluator"> The function that looks up the value of a variable</param>
        /// <returns> The result of the expression </returns>
        /// <exception cref="ArgumentException"></exception>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            // Splits string in tokens
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            Stack<int> values = new Stack<int>();
            Stack<String> operators = new Stack<String>();

            //openParen is for the parentheses when open the parentheses will be ture for the program know when someone use parentheses
            //forgot the open.
            bool openParen = false;

            for (int i = 0; i < substrings.Length; i++)
            {
                string T = substrings[i].Trim();
                int number;
                bool isNumber = int.TryParse(T, out number);

                if (T == "")
                    continue;

                /* If token is number also have more than one values in the stack, else if operators stack has * or /
                   act on it with the token and pop value, else, add token to the stack.*/
                if (isNumber)
                {
                    values.Push(number);
                    if (IsOnTop(operators, "*") || IsOnTop(operators, "/"))
                    {
                        PushResultForMutAndDiv(operators, values);
                    }

                }
                else if (T == "+" || T == "-")
                {
                    if (IsOnTop(operators, "+") || IsOnTop(operators, "-"))
                    {
                        PushResultForAddandSub(operators, values);
                    }
                    operators.Push(T);
                }
                else if (T == "*" || T == "/")
                {
                    operators.Push(T);
                }
                else if (T == "(")
                {
                    operators.Push(T);
                    openParen = true;
                }
                else if (T == ")")
                {
                    if (openParen == false)
                        throw new ArgumentException("not open Paren");
                    if (IsOnTop(operators, "+") || IsOnTop(operators, "-"))
                    {
                        PushResultForAddandSub(operators, values);
                        if (IsOnTop(operators, "("))
                        {
                            operators.Pop();
                        }
                    }

                    if (IsOnTop(operators, "*") || IsOnTop(operators, "/"))
                    {
                        PushResultForMutAndDiv(operators, values);
                    }

                    if (IsOnTop(operators, "("))
                    {
                        operators.Pop();
                    }


                }
                else
                {
                    string variablePattern = "^[a-zA-Z]+[0-9]+$";
                    if (!Regex.IsMatch(T,variablePattern))
                        throw new ArgumentException("Invalid variable");
                    number = variableEvaluator(T);
                    values.Push(number);

                    if (IsOnTop(operators, "*") || IsOnTop(operators, "/"))
                    {
                        PushResultForMutAndDiv(operators, values);
                    }
                }

            }
            while (values.Count > 1 && operators.Count != 0)
            {
                if (IsOnTop(operators, "+") || IsOnTop(operators, "-"))
                {
                    PushResultForAddandSub(operators, values);
                }
                if (IsOnTop(operators, "*") || IsOnTop(operators, "/"))
                {
                    PushResultForMutAndDiv(operators, values);
                }

            }
            ErrorControl(operators, values);

            return (int)values.Pop();
        }

        /// <summary>
        /// Check if the top operation in stack is same as expected operation
        /// </summary>
        /// <param name="stack"> The operation stack </param>
        /// <param name="c"> The expected operation </param>
        /// <returns> True if the top operation in stack is same as expected operation, else false </returns>
        private static bool IsOnTop(this Stack<String> stack, string c)
        {
            return stack.Count > 0 && stack.Peek() == c;
        }

        /// <summary>
        /// A function that performs addition and subtraction
        /// </summary>
        /// <param name="opt"> The operation stack </param>
        /// <param name="intstack"> The integer stack </param>
        /// <returns> The value after calculation </returns>
        /// <exception cref="ArgumentException"></exception>
        private static int PushResultForAddandSub(this Stack<String> opt, Stack<int> intstack)
        {
            if (opt.Count < 0 || intstack.Count < 2)
            {
                throw new ArgumentException("opt stack is empty or only have one number in the stack");
            }
            int value1;
            int value2;
            String top;
            int ans;
            top = opt.Pop();
            value1 = intstack.Pop();
            value2 = intstack.Pop();

            if (top == "+")
            {
                ans = value1 + value2;
                intstack.Push(ans);
                return ans;
            }
            else if (top == "-")
            {
                ans = value2 - value1;
                intstack.Push(ans);
                return ans;
            }

            else
                throw new ArgumentException("That's opt is not add or sub");
        }

        /// <summary>
        /// A function that performs multiplication and division
        /// </summary>
        /// <param name="opt"> The operation stack </param>
        /// <param name="intstack"> The integer stack </param>
        /// <returns> The value after calculation </returns>
        /// <exception cref="ArgumentException"></exception>
        private static int PushResultForMutAndDiv(this Stack<String> opt, Stack<int> intstack)
        {
            if (opt.Count < 0 || intstack.Count < 2)
            {
                throw new ArgumentException("opt stack is empty or only have one number in the stack");
            }
            int value1;
            int value2;
            String top;
            int ans;
            top = opt.Pop();
            value1 = intstack.Pop();
            value2 = intstack.Pop();

            if (top == "*")
            {
                ans = value1 * value2;
                intstack.Push(ans);
                return ans;
            }
            else if (top == "/")
            {
                if (value1 == 0)
                {
                    throw new ArgumentException("cannot div by 0");
                }
                ans = value2 / value1;
                intstack.Push(ans);
                return ans;
            }
            else throw new ArgumentException("the opt is not mult or div");
        }

        /// <summary>
        /// Catches possible errors - 
        /// When there is not enough number or operator to perform a calculation
        /// </summary>
        /// <param name="opt"> The operation stack </param>
        /// <param name="values"> The integer stack </param>
        /// <exception cref="ArgumentException"></exception>
        private static void ErrorControl(this Stack<String> opt, Stack<int> values) // function for the catch error 
        {
            if (opt.Count == 1 && values.Count == 1 || opt.Count == 1 && values.Count == 0)
            {
                throw new ArgumentException("There are not enough values left to compute");
            }
            if (opt.Count == 0 && values.Count != 1)
            {
                throw new ArgumentException("There must only one value" + values.Count + "still left.");
            }



        }
    }
}