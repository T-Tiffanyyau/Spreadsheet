using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace FormulaTester
{
    [TestClass]
    public class FormulaTests
    {

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void testInvalidToken()
        {
            Formula f = new Formula("5 + !");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void testInvalidBaseVariables()
        {
            Formula f = new Formula("343?? + 2X");
        }

        [TestMethod()]
        public void NoDefineVariableTest()
        {
            Formula f = new Formula("2+X1");
            Assert.IsInstanceOfType(f.Evaluate(s => { throw new ArgumentException("no this variable"); }), typeof(FormulaError));
        }

        [TestMethod()]
        public void ToStringTEST()
        {

            Formula f = new Formula("A2  +  200 * B5 + X7", s => s.ToUpper(), s => true);
            Assert.AreEqual("A2+200*B5+X7", f.ToString());
        }


        [TestMethod()]
        public void testEquals()
        {
            Formula f1 = new Formula("3+2*3");
            Formula f2 = new Formula("3 +2  * 3");
            Assert.IsTrue(f1 == f2);

            Formula v1 = new Formula("a7+70", normalize, s => true);
            Formula v2 = new Formula("A7 +  70");
            Assert.IsTrue(v1 == v2);

            Assert.IsTrue(v1 != f2);
        }

        [TestMethod()]
        public void TestEqualGetHashCode()
        {
            Formula f1 = new Formula("1*2+3/4-5+(1+2.0)");
            Formula f2 = new Formula("1*2+3/4-5+(1+2.0)");
            Assert.IsTrue(f1.GetHashCode() == f2.GetHashCode());
        }

        [TestMethod()]
        public void TestNotEqualGetHashCode()
        {
            Formula f1 = new Formula("1*2+3/4+(3+1.0)");
            Formula f2 = new Formula("1*2+3/4+(5+3)");
            Assert.IsTrue(f1.GetHashCode() != f2.GetHashCode());
        }

        [TestMethod()]
        public void SingleNumberTest()
        {
            Formula f = new Formula("8");
            Assert.AreEqual(8d, f.Evaluate(s => 0));
        }

        [TestMethod()]
        public void TestAdd()
        {
            Formula f1 = new Formula("5.0+8");
            Assert.AreEqual(13d, f1.Evaluate(s => 0));

            Formula f2 = new Formula("5.00+15.0");
            Assert.AreEqual(20d, f2.Evaluate(s => 0));
        }

        [TestMethod()]
        public void TestSub()
        {
            Formula f1 = new Formula("200.00-100.00");
            Assert.AreEqual(100d, f1.Evaluate(s => 0));

            Formula f2 = new Formula("2.00  -  1.00");
            Assert.AreEqual(1d, f2.Evaluate(s => 0));

            Formula f3 = new Formula("5  -  2.0");
            Assert.AreEqual(3d, f3.Evaluate(s => 0));
        }

        [TestMethod()]
        public void TestMuti()
        {
            Formula f1 = new Formula("3.0*4");
            Assert.AreEqual(12d, f1.Evaluate(s => 0));

            Formula f2 = new Formula("3.0 * 3.0");
            Assert.AreEqual(9d, f2.Evaluate(s => 0));

            Formula f3 = new Formula("3*10");
            Assert.AreEqual(30d, f3.Evaluate(s => 0));
        }

        [TestMethod()]
        public void TestDiv()
        {
            Formula f1 = new Formula("16.0/8.0");
            Assert.AreEqual(2d, f1.Evaluate(s => 0));

            Formula f2 = new Formula("20/2.0");
            Assert.AreEqual(10d, f2.Evaluate(s => 0));

            Formula f3 = new Formula("27/3");
            Assert.AreEqual(9d, f3.Evaluate(s => 0));
        }

        [TestMethod()]
        public void TestComplexFormula()
        {
            Formula f1 = new Formula("5+(3+5.0*9/3.0)+2-(3+4)");
            Assert.AreEqual(18d, f1.Evaluate(s => 0));

            Formula f2 = new Formula("(3 +4*5)+(8 /4 )-10");
            Assert.AreEqual(15d, f2.Evaluate(s => 0));

            Formula f3 = new Formula("(3 +x4*5/2)+(8 /4 )-10");
            Assert.AreEqual(-2.5d, f3.Evaluate(s => 1));

        }
        [TestMethod()]
        public void TestDivideByZero()
        {
            Formula f1 = new Formula("1/0");
            Assert.IsInstanceOfType(f1.Evaluate(s => 0), typeof(FormulaError));

            Formula f2 = new Formula("1/0.000");
            Assert.IsInstanceOfType(f2.Evaluate(s => 0), typeof(FormulaError));


            Formula f3 = new Formula("(3 +4*5)+(8 /4 )-10/0");
            Assert.IsInstanceOfType(f3.Evaluate(s => 0), typeof(FormulaError));


            Formula f4 = new Formula("(3 +x4*5/0)+(8 /4 )-10");
            Assert.IsInstanceOfType(f4.Evaluate(s => 0), typeof(FormulaError));

            Formula f5 = new Formula("(3 +x4*5/2)/0");
            Assert.IsInstanceOfType(f5.Evaluate(s => 0), typeof(FormulaError));
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void checkEmpty()
        {
            Formula f = new Formula("");
            f.Evaluate(s => 0);
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestEndWithOperator()
        {
            Formula f = new Formula("5 +3 *");
            f.Evaluate(s => 0);
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestOperatorRule()
        {
            Formula f1 = new Formula("3*/4");
            f1.Evaluate(s => 0);

            Formula f2 = new Formula("3+-4");
            f2.Evaluate(s => 0);
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestParelNotEqual()
        {
            Formula f1 = new Formula("(3*4))");
            f1.Evaluate(s => 0);

            Formula f2 = new Formula("((3+4)");
            f2.Evaluate(s => 0);
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestInvaildFirst()
        {
            Formula f1 = new Formula("+3*4");
            f1.Evaluate(s => 0);

            Formula f2 = new Formula("*3-4");
            f2.Evaluate(s => 0);
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestCloseParenthesesFollowingVar()
        {
            Formula f1 = new Formula("(5+5)X");
            f1.Evaluate(s => 0);

            Formula f2 = new Formula("(5+5)8");
            f2.Evaluate(s => 0);
        }

        [TestMethod()]
        public void TestComplexWithVar()
        {
            Formula f = new Formula("y1*3-8/2+4*(2-9.0*2)/4*a2");
            Assert.AreEqual(-102d, f.Evaluate(s => (s == "a2") ? 8 : 10));
        }


        [TestMethod()]
        public void TestParensAndVailblecComplex()
        {
            Formula f = new Formula("a1+(a2+(a3+(a4+(a5+a6))))");
            Assert.AreEqual(12d, f.Evaluate(s => 2));
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestParensNoOperator()
        {
            Formula f = new Formula("(2)3");
        }

        [TestMethod()]
        public void TestEqualFunction()
        {
            Formula f1 = new Formula("1");
            Formula f2 = new Formula("1");
            Formula? f3 = null;
            Assert.IsFalse(f1 == null);
            Assert.IsTrue(f1 == f2);
            Assert.IsFalse(f3 == f3);
        }

        [TestMethod()]
        public void CheckNormalizeTest()
        {
            Formula f = new Formula("2+X1", s => "" + s + "_1", s => true);
            Assert.AreEqual("X1_1", f.GetVariables().First());
        }

        public static string normalize(string s)
        {
            return s.ToUpper();
        }

        public bool checkVaiable(string s)
        {
            return Regex.IsMatch(s, @"^[A-Z]+[0-9]+$");
        }

    }


}