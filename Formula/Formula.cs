// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!

// Change log:
// Last updated: 9/8, updated for non-nullable types

using System.Text.RegularExpressions;
namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        private List<string> Tokens; //hold the token for fomula
        private HashSet<string> normalizeT; //track and hold the normailize varlible


        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            if (formula.Length == 0)
                throw new FormulaFormatException("formula cannot be empty");
            double num;
            int openparel = 0;
            int closerparel = 0;
            Tokens = new List<string>(GetTokens(formula)); //initialize token
            normalizeT = new HashSet<string>();
            string? prevT = ""; //keep the previous Token for check

            for (int i = 0; i < Tokens.Count; i++)
            {
                string T = Tokens[i];
                if (T == "(")
                    openparel++;   // count the openparel
                else if (T == ")")
                    closerparel++; //count the closerparel
                if (i == 0)
                {
                    if (T != "(" && !Regex.IsMatch(T, @"[a-zA-Z_](?: [a-zA-Z_]|\d)*") && !Double.TryParse(T, out num))  //check the first token is vaild for the formula 
                    {
                        throw new FormulaFormatException("The formula should start with a open parenthesis, number or a cell name");
                    }
                }

                else
                {
                    if (prevT == "(" || Regex.IsMatch(prevT, @"^[\+\-*/]")) // Check the token after opt or open parel must be a number or open parel
                    {
                        if (!(Regex.IsMatch(T, @"^[a-zA-Z][0-9a-zA-Z]*") || (T == "(") || Double.TryParse(T, out num)))
                        {
                            throw new FormulaFormatException("after opertor must need open parel number or varlible");
                        }

                    }

                    if (Regex.IsMatch(prevT, @"^[a-zA-Z][0-9a-zA-Z]*") || prevT == ")" || Double.TryParse(prevT, out num)) //check the token after closer parel or varlible and number must be a opt or closer parel 
                    {
                        if (!(Regex.IsMatch(T, @"^[\+\-*/]$") || T == ")"))
                        {
                            throw new FormulaFormatException("invaild token after the closer parel, number and variable ");
                        }
                    }
                }
                prevT = T;
            }

            if (openparel != closerparel)
                throw new FormulaFormatException("open preal and close parel is not equal");  // if the parel is not Equal which mean that's Fomula format problem.
            if (!Regex.IsMatch(prevT, @"^[a-zA-Z][0-9a-zA-Z]*$") && prevT != ")" && !Double.TryParse(prevT, out num))
                throw new FormulaFormatException("The last token must is number,variable or close parel");

            for (int i = 0; i < Tokens.Count; i++) // check normalize avoid floating point error
            {

                string T = Tokens[i];
                if (Regex.IsMatch(T, @"[a-zA-Z_](?: [a-zA-Z_]|\d)*"))
                {

                    if (!isValid(normalize(T)))
                        throw new FormatException("is not vaild normalized variable");
                    else
                    {
                        Tokens[i] = normalize(T);
                        normalizeT.Add(Tokens[i]);
                    }

                }
                if (Double.TryParse(T, out num))
                    Tokens[i] = num.ToString();
            }
        }



        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            string[] substrings = Tokens.Cast<string>().ToArray();
            Stack<double> values = new Stack<double>();
            Stack<String> operators = new Stack<String>();
            bool openParen = false;

            for (int i = 0; i < substrings.Length; i++)
            {
                string T = substrings[i].Trim();
                double number;
                bool isNumber = double.TryParse(T, out number);

                if (T == "")
                    continue;

                /* If token is number also have more than one values in the stack, else if operators stack has * or /
                   act on it with the token and pop value, else, add token to the stack.*/
                if (isNumber)
                {
                    values.Push(number);
                    if (IsOnTop(operators, "*") || IsOnTop(operators, "/"))
                    {
                        if (IsOnTop(operators, "/"))
                        {
                            double check;
                            check = values.Peek();
                            if (check == 0)
                                return new FormulaError("cannot div by 0");
                        }
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
                        if (IsOnTop(operators, "/"))
                        {
                            double check;
                            check = values.Peek();
                            if (check == 0)
                                return new FormulaError("cannot div by 0");
                        }
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
                    if (!Regex.IsMatch(T, variablePattern))
                    { 

                        return new FormulaError("Invalid variable");
                    }

                    try
                    {
                        number = (double)lookup(T);
                    }
                    catch (ArgumentException)
                    {
                        return new FormulaError("not this varlible in program");
                    }

                    number = (double)lookup(T);
                    values.Push(number);

                    if (IsOnTop(operators, "*") || IsOnTop(operators, "/"))
                    {
                        if (IsOnTop(operators, "/"))
                        {
                            double check;
                            check = values.Peek();
                            if (check == 0)
                                return new FormulaError("cannot div by 0");
                        }
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
                    if(IsOnTop(operators,"/"))
                    {
                        double check;
                        check=values.Peek();
                        if (check == 0)
                            return new FormulaError("cannot div by 0");
                    }

                    PushResultForMutAndDiv(operators, values);
                }

            }
            ErrorControl(operators, values);

            return (double)values.Pop();


        }
        /// <summary>
        /// Check if the top operation in stack is same as expected operation
        /// </summary>
        /// <param name="stack"> The operation stack </param>
        /// <param name="c"> The expected operation </param>
        /// <returns> True if the top operation in stack is same as expected operation, else false </returns>
        private static bool IsOnTop(Stack<String> stack, string c)
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
        private static double PushResultForAddandSub(Stack<String> opt, Stack<double> intstack)
        {
            if (opt.Count < 0 || intstack.Count < 2)
            {
                throw new ArgumentException("opt stack is empty or only have one number in the stack");
            }
            double value1;
            double value2;
            String top;
            double ans;
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
        private static object PushResultForMutAndDiv(Stack<String> opt, Stack<double> intstack)
        {
            if (opt.Count < 0 || intstack.Count < 2)
            {
                throw new ArgumentException("opt stack is empty or only have one number in the stack");
            }
            double value1;
            double value2;
            String top;
            double ans;
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
        private static object ErrorControl(Stack<String> opt, Stack<double> values) // function for the catch error 
        {
            if (opt.Count == 1 && values.Count == 1 || opt.Count == 1 && values.Count == 0)
            {
                return new FormulaError("There are not enough values left to compute");
            }
            if (opt.Count == 0 && values.Count != 1)
            {
                return new FormulaError("There must only one value" + values.Count + "still left.");
            }

            return 0;

        }



        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            HashSet<string> temp = new HashSet<string>(normalizeT);
            return temp;
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            string? formula = "";
            for (int i = 0; i < Tokens.Count; i++)
            {
                formula += Tokens[i];
            }
            return formula;
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is Formula))
                return false;
            return ToString().Equals(obj.ToString());

        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            if (f1 is null || f2 is null)
                return false;
            return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            if (f1 is null || f2 is null)
                return true;
            return !(f1 == f2);

        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            int hash = ToString().GetHashCode();
            return hash;
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

        }
    }



    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}
