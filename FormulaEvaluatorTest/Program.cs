using FormulaEvaluator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester
{
    /// <summary>
    /// A tester class for FormulaEvaluator.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (FormulaEvaluator.Evaluator.Evaluate("1 * 0", varlible) != 0)
                Console.WriteLine("Fail 1*0");

            if (FormulaEvaluator.Evaluator.Evaluate("1 * 50", varlible) != 50)
                Console.WriteLine("Fail 1 * 50");

            if (FormulaEvaluator.Evaluator.Evaluate("1 + 0", varlible) != 1)
                Console.WriteLine("Fail 1 + 0");

            if (FormulaEvaluator.Evaluator.Evaluate("500 / 2", varlible) != 250)
                Console.WriteLine("Fail 500 / 2");

            if (FormulaEvaluator.Evaluator.Evaluate("100 - 50", varlible) != 50)
                Console.WriteLine("Fail");

            if (FormulaEvaluator.Evaluator.Evaluate("2*(6-3*(3-2*(2-5)))", varlible) != -42)
                Console.WriteLine("2*(6-3*(3-2*(2-5)))\", varlible) != -42");

            if (FormulaEvaluator.Evaluator.Evaluate("a1+a2", varlible) != 3)
                Console.WriteLine("fail a1+a2");

            if (FormulaEvaluator.Evaluator.Evaluate("a5+10 - 2 - 4 / 4", varlible) != 107)
                Console.WriteLine("fail a5+10 - 2 - 4 / 4");

            if (FormulaEvaluator.Evaluator.Evaluate("(10 / 2) * 4 + (9 / 3)+150", varlible) != 173)
                Console.WriteLine("fail (10 / 2) * 4 + (9 / 3)+150");

            if (FormulaEvaluator.Evaluator.Evaluate("1 + a2 - (a1 + 3)", varlible) != -1)
                Console.WriteLine("fail 1 + a2 - (a1 + 3)");
            try
            {
                FormulaEvaluator.Evaluator.Evaluate("FaKe", varlible);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message + " catched");
            }
            try
            {
                FormulaEvaluator.Evaluator.Evaluate("0/0", varlible);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message + " catched");
            }
            try
            {
                FormulaEvaluator.Evaluator.Evaluate("1 + ", varlible);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message + " catched");
            }
        }

        /// <summary>
        /// Given a variable name as its parameter, the function will either return an integer
        /// or throw an ArgumentException (if the variable has no value).
        /// </summary>
        /// <param name="s"> The variable that needs to be lookup </param>
        /// <returns> The value of the variable </returns>
        /// <exception cref="ArgumentException"></exception>
        public static int varlible(String s)
        {
            String t = s;
            if (t.Equals("a1"))
            {
                return 1;
            }
            else if (t.Equals("a2"))
            {
                return 2;
            }
            else if (t.Equals("a3"))
            {
                return -1;
            }
            else if (t.Equals("a4"))
            {
                return 30;
            }
            else if (t.Equals("a5"))
            {
                return 100;
            }
            else
            {
                throw new ArgumentException("The variable has no value");
            }
        }
    }
}