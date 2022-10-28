

using Newtonsoft.Json;
using SpreadsheetUtilities;
using System.Text;
using System.Text.RegularExpressions;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
    {
        Dictionary<string, object> getCellValueCache = new Dictionary<string, object>();
        // Dictionary<string, object> valueCache = new Dictionary<string, object>();
        private Dictionary<string, object> content = new Dictionary<string, object>(); // Storage for spreadsheet content

        //private DependencyGraph dependencyGraph = new DependencyGraph(); // dependency graph
        public DependencyGraph dependencyGraph = new DependencyGraph(); // dependency graph

        [JsonProperty]
        private Dictionary<string, SequenceCell> cells = new Dictionary<string, SequenceCell>(); // var for json serialize deserialize

        class SequenceCell // the class for json serialize deserialize 
        {
            [JsonProperty]
            public string stringForm { get; set; } // Match json field stringForm

            public SequenceCell(string stringForm)
            {
                this.stringForm = stringForm;
            }

        }

        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) :
            base(isValid, normalize, version)
        {
            if (version == null) // Check if version is null
            {
                throw new ArgumentNullException();
            }
        }


        public Spreadsheet() :
            this(s => true, (s => s), "default")
        { }

        public Spreadsheet(string path, Func<string, bool> isValid, Func<string, string> normalize, string version) :
            this(isValid, normalize, version)
        {
            Dictionary<string, SequenceCell> _cells;
            try
            {
                _cells = loadSpreadsheetFromJson(path, version); // try to load json content into [cells]
            }
            catch
            {
                throw new SpreadsheetReadWriteException("IO Exception"); // if loadSpreadsheetFromJson throw some error, throw a SpreadsheetReadWriteException
            }

            foreach (var cell in _cells) // After load json content to cells, put data into [content]
            {
                try
                {
                    SetContentsOfCell(cell.Key, cell.Value.stringForm); // Set cell content
                }
                catch
                {
                    throw new SpreadsheetReadWriteException("Cell name is invalid.");
                }
            }

        }

        Dictionary<string, SequenceCell> getCells()
        {
            return cells; // Return a Dictionary contains all cells
        }

        string getVersion()
        {
            return Version; // Return current version
        }

        Dictionary<string, SequenceCell> loadSpreadsheetFromJson(string path, string version)
        {
            String content = "";
            if (!File.Exists(path)) // Check if given file path exists
            {
                throw new SpreadsheetReadWriteException("File not exists");
            }

            using (var fileReader = new StreamReader(path)) // Read file content
            {
                content = fileReader.ReadToEnd();
            }

            Spreadsheet? spreadsheet = JsonConvert.DeserializeObject<Spreadsheet>(content); // Deserialize file content into object

            if (spreadsheet == null)
            {
                throw new SpreadsheetReadWriteException("Json is invalid.");
            }

            if (spreadsheet.getVersion() != version) // Check version
            {
                throw new SpreadsheetReadWriteException("Version not match.");
            }

            return spreadsheet.getCells();

        }

        bool flagChanged = false;
        public override bool Changed { get => flagChanged; protected set => flagChanged = value; }

        /// <summary>
        ///  using bool check is empty or not function
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool isEmpty(string key)
        {
            if (content.ContainsKey(key))
            {
                if (content[key] is string)
                {
                    return ((string)content[key]) == "";
                }
            }
            return false;
        }
        /// <summary>
        /// check the character is vaild or not function
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        protected bool checkCharacter(char ch) //check vaild charcter (Only contains A-Za-z_)
        {
            if (ch >= 'A' && ch <= 'Z')
            {
                return true;
            }
            if (ch == '_')
            {
                return true;
            }
            if (ch >= 'a' && ch <= 'z')
            {
                return true;
            }

            return false;

        }

        /// <summary>
        /// check the input is digital or not 
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        protected bool isDigital(char ch) //check the ch is a digital
        {
            return (ch >= '0' && ch <= '9');
        }


        /// <summary>
        /// check the cell name is vaild or not
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected bool testNameCore(string name)
        {
            if (name == null)
            {
                return false;
            }

            if (name == "")
            {
                return false;
            }

            if (isDigital(name[0])) // Check if first char is digital
            {
                return false;
            }

            foreach (var ch in name)
            {
                if (isDigital(ch) || checkCharacter(ch))
                {

                }
                else
                {
                    return false;
                }
            }

            return true;

        }

        Dictionary<string, bool> testNameCache = new Dictionary<string, bool>();

        private void testName(string name)
        {
            if (testNameCache.ContainsKey(name))
            {
                if (!testNameCache[name])
                {
                    throw new InvalidNameException();
                }
            }
            else
            {
                testNameCache[name] = testNameOld(name);

                if (!testNameCache[name])
                {
                    throw new InvalidNameException();
                }
            }

        }

        /// <summary>
        /// if cannot pass the cell name thown the error
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="InvalidNameException"></exception>
        bool testNameOld(string name) // Check if name is valid, if not throw InvalidNameException
        {

            if (name == null)
            {
                return false;
            }
            if (name.Length == 1)
            {
                return false;
            }
            if (!testNameCore(name))
            {
                return false;
            }
            if (!IsValid(name))
            {
                return false;
            }
            if (name == "")
            {
                return false;
            }
            if (!Regex.IsMatch(name, "[A-Za-z]+[0-9]*"))
            {
                return false;
            }

            return true;

        }
        /// <returns></returns>
        public override object GetCellContents(string name)
        {
            name = Normalize(name);
            testName(name);
            return content.ContainsKey(name) ? content[name] : new string(""); // Check if cell is a exist Cell, if not return empty string
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            List<string> result = new List<string>();
            foreach (var key in content.Keys)
            {
                if (!isEmpty(key))
                {
                    result.Add(key);
                }
            }

            return result.Distinct();
        }

        /// <summary>
        /// remove the depandecy reference of the cell 
        /// </summary>
        /// <param name="name"></param>
        private void removeReferenceOf(String name)
        {
            name = Normalize(name);

            Formula formula = (Formula)content[name];

            foreach (var variable in formula.GetVariables())
            {
                dependencyGraph.RemoveDependency(variable, name);
            }

        }


        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return dependencyGraph.GetDependents(name).Distinct(); //Get one step dependents
        }

        public override void Save(string filename)
        {
            cells = new Dictionary<string, SequenceCell>(); // Create cell object 
            foreach (var pair in content)
            {
                if (pair.Value is Formula)
                {
                    cells.Add(pair.Key, new SequenceCell("=" + pair.Value)); // Put contents into cell Object if cell is formula, put a = 
                }
                else
                {
                    cells.Add(pair.Key, new SequenceCell("" + pair.Value));
                }
            }

            string json = JsonConvert.SerializeObject(this);

            Changed = false;


            try
            {
                using (var stream = File.OpenWrite(filename)) // Write Serialized json into file
                {
                    byte[] byteArray = new UTF8Encoding(true).GetBytes(json);
                    stream.Write(byteArray, 0, byteArray.Length);
                }
            }
            catch
            {
                throw new SpreadsheetReadWriteException("IO Exception");
            }

        }

        double GetCellValueDouble(string name)
        {
            object result = GetCellValue(name);

            if (result is double) // Check if result is Double
            {
                return (double)result;
            }
            else
            {
                return double.NaN; // If not, return NaN
            }


        }

        public override object GetCellValue(string name)
        {
            /*
            if (valueCache.ContainsKey(name))
            {
                return valueCache[name];
            }

            Console.WriteLine("Will add: " + name);
            */

            if (getCellValueCache.ContainsKey(name))
            {
                return getCellValueCache[name];
            }

            object value = GetCellContents(name);

            if (value is Formula)
            {
                object result = ((Formula)value).Evaluate(s => GetCellValueDouble(s));

                if (result is double)
                {
                    if (double.IsNaN((double)result))
                    {
                        var finalResult = new FormulaError();
                        getCellValueCache[name] = finalResult;
                        return finalResult;
                    }
                    else
                    {
                        getCellValueCache[name] = result;
                        return result;
                    }
                }
                else
                {
                    var finalResult = new FormulaError();
                    getCellValueCache[name] = finalResult;
                    return finalResult;
                }

            }
            else
            {
                getCellValueCache[name] = value;
                return value;
            }

        }

        private bool isDouble(string content) // Check if string is double
        {
            if (content.Count() <= 0)
            {
                return false;
            }
            bool flagAllNumber = content.Aggregate(true, (final, ch) => final &= isDigital(ch)); // Check if all char is number
            if (flagAllNumber)
            {
                return true;
            }
            else
            {
                int dotCount = 0;
                int digitalCount = 0;
                int eCount = 0;
                int plusCount = 0;
                foreach (var ch in content)
                {
                    if (ch == '.')
                    {
                        dotCount++;
                    }
                    else if (isDigital(ch))
                    {
                        digitalCount++;
                    }
                    else if (ch == 'e')
                    {
                        eCount++;
                    }
                    else if (ch == '+')
                    {
                        plusCount++;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (dotCount > 1) // if content contains one more dot, it is not double
                {
                    return false;
                }
                else if (eCount == 1 && plusCount == 1 && dotCount == 1 && eCount + dotCount + plusCount + digitalCount == content.Length)
                {
                    return true; // if content contains one dot one e one +, it is double
                }
                else if (digitalCount + dotCount != content.Length)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }

        }

        /// <summary>
        /// Check the setcellcontents function if is null throw the error
        /// </summary>
        /// <param name="k"></param>
        /// <param name="v"></param>
        /// <exception cref="InvalidNameException"></exception>
        /// <exception cref="ArgumentNullException"></exception>

        private void checkNull(object k, object v) // Check if k, v is null
        {
            if (k == null)
            {
                throw new InvalidNameException();
            }
            if (v == null)
            {
                throw new ArgumentNullException();
            }
        }

        List<string> setCellValue(string name, object value)
        {
            if (content.ContainsKey(name)) // Check if cell is exists
            {
                object cellContent = content[name];

                if (cellContent is Formula) // if exists cell is formula, call removeReferenceOf on it
                {
                    removeReferenceOf(name);
                }

            }

            content[name] = value;

            return GetCellsToRecalculate(name).ToList();
        }

        protected override IList<string> SetCellContents(string name, double number) // Set cell contents
        {
            return setCellValue(name, number);
        }

        protected override IList<string> SetCellContents(string name, string text)
        {
            return setCellValue(name, text);
        }

        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            foreach (var variable in formula.GetVariables())
            {
                try
                {
                    testName(variable);
                }
                catch
                {
                    throw new FormulaFormatException("Formula variable name is not corrent.");
                }
                dependencyGraph.AddDependency(variable, name);
            }

            //GetCellsToRecalculate(name);

            IList<string> result = setCellValue(name, formula);

            return new List<string>(result.Distinct());
        }

        void removeAllInTree(string name)
        {
            // Console.WriteLine("Will remove: " + name);
            // valueCache.Remove(name);
            foreach (var dependeeName in dependencyGraph.GetDependees(name))
            {
                removeAllInTree(name);
            }
        }

        public override IList<string> SetContentsOfCell(string name, string content)
        {
            name = Normalize(name);
            testName(name);
            var lastChanged = Changed;
            Changed = true;
            getCellValueCache.Clear();

            // removeAllInTree(name);

            if (isDouble(content)) // is content is double or string, put it into content direct
            {
                return SetCellContents(name, double.Parse(content));
            }
            else if ((content.Length > 0) && content[0] == '=') // if content if fomula, put a = before content
            {
                try
                {
                    return SetCellContents(name, new Formula(content.Substring(1)));
                }
                catch
                {
                    Changed = lastChanged;
                    throw;
                }
            }
            else
            {
                return SetCellContents(name, content);
            }
        }
    }
}
