using SS;
using SpreadsheetUtilities;
using System;

namespace SpreadsheetTest
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestDefaultConstructor()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("X64", "X64");

            Assert.AreEqual("X64", s.GetCellContents("X64"));

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestInvalidName1()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("X_", "0");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestInvalidName2()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("hello", "0");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test3ArgumentsConstructorAlwaysFalse()
        {
            Spreadsheet s = new Spreadsheet(s => false, s => s, "default");
            s.SetContentsOfCell("", "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test3ArgumentsConstructorVersionIsNull()
        {
            Spreadsheet s = new Spreadsheet(s => false, s => s, null);
        }

        [TestMethod]
        public void Test3ArgumentsConstructorUpperCase()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s.ToUpper(), "default");
            s.SetContentsOfCell("abcde0", "7.77");

            Assert.AreEqual("7.77", s.GetCellContents("ABCDE0") + "");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void Test4ArgumentsConstructorVersionUnmatch()
        {
            string tempFile = Path.GetTempFileName();
            try
            {

                using (var fileWriter = new StreamWriter(tempFile))
                {
                    string json = "{\"cells\":{},\"Version\":\"default\"}";
                    fileWriter.Write(json);
                }
                Console.WriteLine(tempFile);
                Spreadsheet s = new Spreadsheet(tempFile, s => true, s => s, "tluafed");

            }
            catch
            {
                throw;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }

        }

        [TestMethod]
        public void TestSetContentsOfCell()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("A0", "XD");
            s.SetContentsOfCell("A1", "=6.78");
            s.SetContentsOfCell("A2", "3.333");

            Assert.AreEqual(s.GetCellValue("A0"), "XD");
            Assert.AreEqual(s.GetCellValue("A1"), 6.78);
            Assert.AreEqual(s.GetCellValue("A2"), 3.333);

        }

        [TestMethod]
        public void TestChanged()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            Assert.IsFalse(s.Changed);
            s.SetContentsOfCell("xxx0", "zzz");
            Assert.IsTrue(s.Changed);
            s.Save(Path.GetTempFileName());
            Assert.IsFalse(s.Changed);
        }

        [TestMethod]
        public void TestSave()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            var filePath = Path.GetTempFileName();
            s.Save(filePath);
            using (var streamReader = new StreamReader(filePath))
            {
                Assert.AreEqual("{\"cells\":{},\"Version\":\"default\"}", streamReader.ReadToEnd());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test_GetCellContents_Invalid_Name()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents("");
            s.GetCellContents("0z");
        }

        [TestMethod]
        public void Test_GetCellContents_Valid_Name()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.AreEqual("", s.GetCellContents("s0"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test_GetCellContents_Null()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents(null);
        }

        [TestMethod]
        public void Test_GetCellContents_Valid_Return()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("z0", 0 + "");
            Assert.AreEqual(0.0, s.GetCellContents("z0"));
            Assert.AreEqual("", s.GetCellContents("z1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test_SetContentsOfCell_Double_NullName()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, 2.0 + "");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test_SetContentsOfCell_Double_InvalidName()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("0z", 2.0 + "");
        }

        [TestMethod]
        public void Test_SetContentsOfCell_Double_ValidName()
        {
            Spreadsheet s = new Spreadsheet();
            List<string> expected = new List<string> { "" };
            Assert.AreEqual(s.SetContentsOfCell("z0", 2.0 + "").ToString(), expected.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test_SetContentsOfCell_String_NullName()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, "0");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_SetContentsOfCell_String_NullText()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("z0", (string)null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test_SetContentsOfCell_String_InvalidName()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("0z", "0");
        }

        [TestMethod]
        public void Test_SetContentsOfCell_String_ValidName()
        {
            Spreadsheet s = new Spreadsheet();
            List<string> expected = new List<string> { "" };
            Assert.AreEqual(s.SetContentsOfCell("z0", "0").ToString(), expected.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test_SetContentsOfCell_Formula_NullName()
        {
            Spreadsheet s = new Spreadsheet();
            Formula f = new Formula("a1 + 1");
            s.SetContentsOfCell(null, f + "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_SetContentsOfCell_String_NullFormula()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("z0", null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test_SetContentsOfCell_Formula_InvalidName()
        {
            Spreadsheet s = new Spreadsheet();
            Formula f = new Formula("a1 + 1");
            s.SetContentsOfCell("0z", f + "");
        }

        [TestMethod]
        public void Test_SetContentsOfCell_Formula_ValidName()
        {
            Spreadsheet s = new Spreadsheet();
            Formula f1 = new Formula("1");
            Formula f2 = new Formula("f1 + 1");
            List<string> expected = new List<string> { "z0" };
            s.SetContentsOfCell("z0", f1 + "");
            Assert.AreEqual(s.SetContentsOfCell("z1", f2 + "").ToString(), expected.ToString());
            Assert.AreEqual(s.SetContentsOfCell("z1", f1 + "").ToString(), new List<string> { "" }.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void Test_SetContentsOfCell_Formula_CircularException()
        {
            Spreadsheet s = new Spreadsheet();
            Formula f = new Formula("a1 + 1");
            s.SetContentsOfCell("a1", "=" + f);
        }

        [TestMethod]
        public void Test_GetNamesOfAllNonemptyCells_Empty()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.AreEqual(0, s.GetNamesOfAllNonemptyCells().Count());
            List<string> expected = new List<string> { "" };
            Assert.AreEqual(expected.ToString(), s.GetNamesOfAllNonemptyCells().ToList().ToString());
        }

        [TestMethod]
        public void Test_GetNamesOfAllNonemptyCells()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("z0", 0 + "");
            s.SetContentsOfCell("z1", 0 + "");
            Assert.AreEqual(2, s.GetNamesOfAllNonemptyCells().Count());
            List<string> expected = new List<string> { "z0", "z1" };
            Assert.AreEqual(expected.ToString(), s.GetNamesOfAllNonemptyCells().ToList().ToString());
        }



        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestSpreadsheetFileNotExists()
        {
            Spreadsheet spreadsheet = new Spreadsheet("??||\\--\\////--//", s => true, s => s, "default");
        }

        [TestMethod]
        public void TestSpreadsheetNormal()
        {
            string tempFile = Path.GetTempFileName();
            try
            {

                using (var fileWriter = new StreamWriter(tempFile))
                {
                    string json = "{\"cells\":{},\"Version\":\"default\"}";
                    fileWriter.Write(json);
                }
                Console.WriteLine(tempFile);
                Spreadsheet s = new Spreadsheet(tempFile, s => true, s => s, "default");

            }
            catch
            {
                throw;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }

            Assert.IsTrue(true);

        }

        [TestMethod(), Timeout(1000)]
        public void SimpleAddNumberTest()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            IList<string> cell_list = ss.SetContentsOfCell("A03", ".1e+9");

            Assert.AreEqual(1, cell_list.Count);
            Assert.AreEqual("A03", cell_list[0]);
            Assert.AreEqual(.1e+9, ss.GetCellContents("A03"));
        }




        [TestMethod(), Timeout(1000)]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellValueWithNullNameTest()
        {
            Spreadsheet ss = new Spreadsheet();
            string name = null;

            ss.GetCellValue(name);
        }

        [TestMethod(), Timeout(1000)]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidNameTestGetCellValue()
        {
            Spreadsheet ss = new Spreadsheet(str => false, str => str, "1000/2932/4910");
            string name = "a0";

            ss.GetCellValue(name);
        }

        [TestMethod(), Timeout(1000)]
        public void GetCellValueTest()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("B0", "0.5");
            ss.SetContentsOfCell("E0", "Hello");
            ss.SetContentsOfCell("P0", "=(0.3 + 0.2) / B0");

            Assert.AreEqual(0.5, (double)ss.GetCellValue("B0"));
            Assert.AreEqual("Hello", (string)ss.GetCellValue("E0"));
            Assert.AreEqual(1.0, (double)ss.GetCellValue("P0"), 1e-6);
        }

    }
}