// Written by Tiffany Yau and Matthew Lam for CS 3500 PS6, 20th Oct, 2020
using SS;
using System.Diagnostics;

namespace SpreadsheetGUI;

/// <summary>
/// Example of using a SpreadsheetGUI object
/// </summary>
public partial class MainPage : ContentPage
{

    int col = 0;
    int row = 0;
    string content;
    char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private Spreadsheet spreadsheet = new Spreadsheet(s => true, (s => s), "ps6");
    string _cellName;

    /// <summary>
    /// Constructor for the demo
    /// </summary>
    public MainPage()
    {
        InitializeComponent();
        // This an example of registering a method so that it is notified when
        // an event happens.  The SelectionChanged event is declared with a
        // delegate that specifies that all methods that register with it must
        // take a SpreadsheetGrid as its parameter and return nothing.  So we
        // register the displaySelection method below.
        spreadsheetGrid.SelectionChanged += displaySelection;
        spreadsheetGrid.SetSelection(col, row);
        updateCellName();
        updateCellContent();
        updateCellValue();
        
    }

    /// <summary>
    /// Updates both the cellValue and the instant display.
    /// </summary>
    private void updateCellValue()
    {
        spreadsheetGrid.SetValue(col, row, spreadsheet.GetCellValue(_cellName).ToString());
        spreadsheetGrid.GetValue(col, row, out content);
        CellValue.Text = content;
    }

    /// <summary>
    /// Updates the cell content in instant display.
    /// </summary>
    private void updateCellContent()
    {
        CellContent.Text = spreadsheet.GetCellContents(_cellName).ToString();
    }

    /// <summary>
    /// Updates the cell name
    /// </summary>
    private void updateCellName()
    {
        _cellName = alpha[col] + (row + 1).ToString();
        CellName.Text = "Cell Name: " + _cellName;
    }

    /// <summary>
    /// Updates the spreadsheet grid by the cell name.
    /// </summary>
    /// <param name="cellName"></param>
    private void cellNameto_Col_and_Row(string cellName)
    {
        int tempCol = -1;
        for (int i = 0; i < 26; i++)
        {
            if (cellName.StartsWith(alpha[i]))
            {
                tempCol = i;
            }
        }
        int.TryParse(cellName.Substring(1), out int tempRow);
        tempRow = tempRow - 1;
        spreadsheetGrid.SetValue(tempCol, tempRow, spreadsheet.GetCellValue(cellName).ToString());
        spreadsheetGrid.GetValue(tempCol, tempRow, out content);
    }

    /// <summary>
    /// The event for the "set value" button which sets the input value to the specfic cell
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnButtonClicked(Object sender,EventArgs e)
    {
        string text = editable.Text;
        // Cell names should be case insensitive. For example, A2 and a2 should be treated as the same.
        if (text.StartsWith("="))
            text = text.ToUpper();
        
        try
        {
            spreadsheet.SetContentsOfCell(_cellName, text);
            spreadsheetGrid.SetValue(col, row, text);
            updateCellContent();
            updateCellValue();
            // updates the dependent cells of current cell
            foreach (string s in spreadsheet.dependencyGraph.GetDependents(_cellName))
            {
                cellNameto_Col_and_Row(s);
            }
        }
        catch (Exception excp)
        {
            // When the selected cell is unsuccessfully edited, an error message should be displayed
            await DisplayAlert("Alert", excp.Message, "OK");
        }
        editable.Text = string.Empty;

    }

    /// <summary>
    /// Updates the current view in spreadsheet grid
    /// </summary>
    /// <param name="grid"></param>
    private void displaySelection(SpreadsheetGrid grid)
    {
        spreadsheetGrid.GetSelection(out col, out row);
        updateCellName();
        updateCellContent();
        updateCellValue();
    }

    /// <summary>
    /// The event for "new" which creates an empty new spreadsheet
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void NewClicked(Object sender, EventArgs e)
    {
        if (spreadsheet.Changed == true)
        {
            bool answer = await DisplayAlert("Save now spreedsheet", "Would you like to Save this spreadsheet first?", "Yes", "No");
            Debug.WriteLine("Answer: " + answer);
            // If the spreadsheet is modified, save before opening another one
            if (answer == true)
            {
                string Filename = await DisplayPromptAsync("Save File", "What's your file name");
                string FullPath = await DisplayPromptAsync("Save File", "Enter your want to save path!", initialValue: "C:/" + Filename);
                try
                {
                    spreadsheet.Save(FullPath + ".sprd");
                }
                catch
                {
                    await DisplayAlert("Alert", "Invalid File Path", "OK");
                }
            }
        }
        spreadsheetGrid.Clear();
    }

    /// <summary>
    /// Opens any file as text and prints its contents.
    /// Note the use of async and await, concepts we will learn more about
    /// later this semester.
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e)
    {
        if (spreadsheet.Changed==true)
        {
            bool answer = await DisplayAlert("Save now spreedsheet", "Would you like to Save this spreadsheet first?", "Yes", "No");
            Debug.WriteLine("Answer: " + answer);
            // If the spreadsheet is modified, save before opening another one
            if(answer== true)
            {
                string Filename = await DisplayPromptAsync("Save File", "What's your file name");
                string FullPath = await DisplayPromptAsync("Save File", "Enter your want to save path!", initialValue: "C:/" + Filename);
                try
                {
                    spreadsheet.Save(FullPath + ".sprd");
                }
                catch
                {
                    await DisplayAlert("Alert", "Invalid File Path", "OK");
                }
            }
        }
        try
        {
            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
                System.Diagnostics.Debug.WriteLine("Successfully chose file: " + fileResult.FileName);
                spreadsheet = new Spreadsheet(fileResult.FullPath, s => true, (s => s), "ps6");
                spreadsheetGrid.Clear();
                CellValue.Text = string.Empty;
                foreach (string s in spreadsheet.GetNamesOfAllNonemptyCells())
                {
                    cellNameto_Col_and_Row(s);
                }

                // Will not ask user to save the spreadsheet if the spreadsheet in file is unmodified
                spreadsheet.Save(fileResult.ToString());
            }
      
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error opening file:");
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    /// <summary>
    /// Save any file as text and prints its contents.
    /// Note the use of async and await, concepts we will learn more about
    /// later this semester.
    /// </summary>
    private async void SaveClicked(object sender, EventArgs e)
    {
        string Filename = await DisplayPromptAsync("Save File", "What's your file name");
        string FullPath = await DisplayPromptAsync("Save File", "Confrim your path", initialValue: "C:\\" + Filename);
        try
        {
            spreadsheet.Save(FullPath + ".sprd");
        }
        catch 
        {
            await DisplayAlert("Alert", "Invalid File Path", "OK");
        }

    }

    /// <summary>
    /// private function to search for a value in the spreadsheet
    /// Will display a window to show which cell the value is in
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnSearchValueClicked(object sender, EventArgs e)
    {
        String temp = seach.Text;
        string resultCell = null;
        string resultContant = null;

        List<string> list = new List<string>();
        foreach(string s in spreadsheet.GetNamesOfAllNonemptyCells())
        {
            if(spreadsheet.GetCellValue(s).ToString()==temp)
            {
                list.Add(s);
            }
        }
        if (list.Count > 0)
        {
            // shows a window if a value is in the speadsheet
            foreach (string i in list)
            {
                string y = i;
                cellNameto_Col_and_Row(i);
                resultCell += i+"; ";
                resultContant += i + " = " + spreadsheet.GetCellContents(y).ToString();
                resultContant += "\n";
            }
            
            await DisplayAlert("Search for value", "Value in " + resultCell +"\n"+ resultContant, "OK");

        }
        else
        {
            await DisplayAlert("Search for value", "Cannot Find the Value in Spreadsheet", "OK");
        }
        resultCell = "";

    }

    /// <summary>
    /// private function to search for the value in the cell
    /// Will display a window to show the value and content of the cell
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnSearchCellClicked(object sender, EventArgs e)
    {
        string cell = seach.Text.ToUpper();
        string temp = seach.Text.ToUpper();
        try
        {
            temp = spreadsheet.GetCellValue(temp).ToString();
            if (temp.Length < 1)
                await DisplayAlert("Search for cell", cell + " do not have a value", "OK");
            else
                await DisplayAlert("Search for cell", cell + " have value " + temp + " with content: " + spreadsheet.GetCellContents(cell) ,"OK");
        }
        catch (Exception) 
        { 
            await DisplayAlert("Search for cell", "Invalid cell name" , "OK"); 
        }
    }

    /// <summary>
    /// private method to help display mesaage to the user when help is pressed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void HelpInput(object sender, EventArgs e)
    {
        await DisplayAlert("How to input to the spreadsheet",
            "1. Strings, numbers, and formulas can be input into the spreadsheet" +
            "\n2. The input could only be typed in the \"Enter Value\" entry and be set to the spreadsheet by pressing the \"Set value\" button " +
            "\n3. When entering a formula, the formula should start with \"=\"" +
            "\n4. Cell names in a formula is case insensitive.", "OK");
    }
    
    /// <summary>
    /// private method to help display mesaage to the user when help is pressed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void HelpFormulaError(object sender, EventArgs e)
    {
        await DisplayAlert("What is Formula Error",
            "Formula error will be display in the cell if the cell needed to calculate the formular is not inputed", "OK");
    }

    /// <summary>
    /// private method to help display mesaage to the user when help is pressed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void HelpSavingLoading(object sender, EventArgs e)
    {
        await DisplayAlert("How to save/load a spreadsheet",
            "\"new\" - creates a new empty spreadsheet" +
            "\n\"open\" - open a spreadsheet that was saved in a file" +
            "\nIf the spreadsheet is being modified, a warning dialog will be displayed asking to save the data when \"new\" or \"open\" is pressed.", "OK");
    }

    /// <summary>
    /// private method to help display mesaage to the user when help is pressed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void HelpSearchbyValue(object sender, EventArgs e)
    {
        await DisplayAlert("How to use \"Search by Value\"",
            "\"Search by Value\" searches for a specific value in the spreadsheet and will display all the cells the content such value in a pop up window." +
            "\n\nif the value is not found in the spreadsheet, a message will display in the pop up window indicating it is not found.", "OK");
    }

    /// <summary>
    /// private method to help display mesaage to the user when help is pressed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void HelpSearchbyCell(object sender, EventArgs e)
    {
        await DisplayAlert("How to use \"Search by Cell\"",
            "\"Search by Cell\" searches for the specific cell in the spreadsheet and will display the cell content in a pop up window." +
            "\n\nif the cell name entered is invalid, a message will display in the pop up window indicating the cell name entered is invalid." +
            "\n\nif the associate cell does not have a value, a message will display in the pop up window indicating the cell do not have a value."
            , "OK");
    }
}
