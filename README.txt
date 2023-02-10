```
Author:     Tiffany Chung-Lam Yau
Date:       20-Oct-2020
GitHub ID:  T-Tiffanyyau
Repo:       https://github.com/uofu-cs3500-spring23/spreadsheet-T-Tiffanyyau
```

# Overview of the Spreadsheet functionality

This is a simple spreadsheet GUI.
It includes: entering data (i.e., strings, numbers, and formulas) into the spreadsheet and updating/displaying the results in a clear manner, and saving/loading files.
The input could only be typed in the "Enter Value" entry and be set to the spreadsheet by pressing the "Set value" button.

When the application starts, it will display a single window, containing an empty spreadsheet.
The User Interface (UI) will consist of a window displaying a grid of 26 columns and 99 rows (scroll bars are of course acceptable). 
Cell addresses should be as in Excel: A1 in the upper left and Z99 in the lower right.

When entering a formula, the formula should start with "="
Cell names in a formula is case insensitive. For example, A2 and a2 should be treated as the same.
When the selected cell is unsuccessfully edited (e.g. because of an invalid formula), an error message will be displayed but nothing else will be changed.
Formula error will be display in the cell if the cell needed to calculate the formular is not inputed

The spreadsheet contain the following saving/loading features which are done by pressing different selection under "File" in the top left corner.
	"new" - creates a new empty spreadsheet
	"open" - open a spreadsheet that was saved in a file
If the spreadsheet is being modified, a warning dialog will be displayed asking to save the data when "new" or "open" is pressed.

##Additional function

A additional search function is added
	"Search by Value" searches for a specific value in the spreadsheet and will display all the cells the content such value in a pop up window.
		if the value is not found in the spreadsheet, a message will display in the pop up window indicating it is not found.
	"Search by Cell" searches for the specific cell in the spreadsheet and will display the cell content in a pop up window.
		if the cell name entered is invalid, a message will display in the pop up window indicating the cell name entered is invalid.
		if the associate cell does not have a value, a message will display in the pop up window indicating the cell do not have a value.

# Time Expenditures:

    Hours Estimated/Worked         Assignment                            Note
           10   /   10    -        Assignment 1 - Formula Evaluator     Finished on time.
           10   /   8     -        Assignment 2 - Dependency Graph      Finished before expected.
           10   /   12    -        Assignment 3 - Formula               Finished longer than expected.
           10   /   10    -        Assignment 4 - Spreadsheet           Finished on time.

