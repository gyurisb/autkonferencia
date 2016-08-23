using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Events.Helpers
{
    public class XlsxHelper
    {
        public static XlsxHelperDocument CreateDocument()
        {
            MemoryStream dataStream = new MemoryStream();
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(dataStream, SpreadsheetDocumentType.Workbook);
            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "mySheet"
            };
            sheets.Append(sheet);

            return new XlsxHelperDocument
            {
                SpreadsheetDocument = spreadsheetDocument,
                WorksheetPart = worksheetPart,
                DataStream = dataStream,
                WorkbookPart = workbookpart
            };
        }

        public static string ToCol(int columnIndex)
        {
            return ((char)('A' + columnIndex)).ToString();
        }

        public static Cell SetCellText(XlsxHelperDocument document, string column, uint row, string text, bool bold = false)
        {
            // Insert the text into the SharedStringTablePart.
            var tablePart = GetSharedStringTablePart(document.SpreadsheetDocument);
            int index = InsertSharedStringItem(tablePart, text, bold);
            Cell cell = InsertCellInWorksheet(column, row, document.WorksheetPart);
            // Set the value of cell.
            cell.CellValue = new CellValue(index.ToString());
            cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
            return cell;
        }

        private static RunProperties GenerateSharedStringItemBold()
        {
            RunProperties runProperties = new RunProperties();
            Bold bold1 = new Bold();
            FontSize fontSize1 = new FontSize() { Val = 11D };
            Color color1 = new Color() { Theme = (UInt32Value)1U };
            RunFont runFont1 = new RunFont() { Val = "Calibri" };
            FontFamily fontFamily1 = new FontFamily() { Val = 2 };
            FontScheme fontScheme1 = new FontScheme() { Val = FontSchemeValues.Minor };

            runProperties.Append(bold1);
            runProperties.Append(fontSize1);
            runProperties.Append(color1);
            runProperties.Append(runFont1);
            runProperties.Append(fontFamily1);
            runProperties.Append(fontScheme1);

            return runProperties;
        }
        private static Text GenerateText(string str)
        {
            Text text = new Text();
            text.Text = str;
            return text;
        }

        public static SharedStringTablePart GetSharedStringTablePart(SpreadsheetDocument spreadSheet)
        {
            // Get the SharedStringTablePart. If it does not exist, create a new one.
            if (spreadSheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
            {
                return spreadSheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
            }
            else
            {
                return spreadSheet.WorkbookPart.AddNewPart<SharedStringTablePart>();
            }
        }
        // Given text and a SharedStringTablePart, creates a SharedStringItem with the specified text 
        // and inserts it into the SharedStringTablePart. If the item already exists, returns its index.
        public static int InsertSharedStringItem(SharedStringTablePart shareStringPart, string text, bool bold = false)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }

            var sharedStringItem = new SharedStringItem();
            Run run = new Run();
            if (bold) run.Append(GenerateSharedStringItemBold());
            run.Append(GenerateText(text));
            sharedStringItem.Append(run);
            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(sharedStringItem);
            shareStringPart.SharedStringTable.Save();

            return i;
        }
        // Given a column name, a row index, and a WorksheetPart, inserts a cell into the worksheet. 
        // If the cell already exists, returns it. 
        public static Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            string cellReference = columnName + rowIndex;

            // If the worksheet does not contain a row with the specified row index, insert one.
            Row row;
            if (sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).Count() != 0)
            {
                row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
            }
            else
            {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            // If there is not a cell with the specified column name, insert one.  
            if (row.Elements<Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0)
            {
                return row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).First();
            }
            else
            {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                Cell refCell = null;
                foreach (Cell cell in row.Elements<Cell>())
                {
                    if (string.Compare(cell.CellReference.Value, cellReference, true) > 0)
                    {
                        refCell = cell;
                        break;
                    }
                }

                Cell newCell = new Cell() { CellReference = cellReference };
                row.InsertBefore(newCell, refCell);

                worksheet.Save();
                return newCell;
            }
        }
    }

    public class XlsxHelperDocument : IDisposable
    {
        public SpreadsheetDocument SpreadsheetDocument;
        public WorksheetPart WorksheetPart;
        public MemoryStream DataStream;
        public WorkbookPart WorkbookPart;

        private bool documentClosed = false;

        public byte[] ToByteArray()
        {
            WorkbookPart.Workbook.Save();
            SpreadsheetDocument.Close();
            documentClosed = true;
            return DataStream.ToArray();
        }

        public void Dispose()
        {
            if (!documentClosed)
                SpreadsheetDocument.Close();
            DataStream.Dispose();
        }
    }
}