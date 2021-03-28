using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Imports.Tests
{
    [TestClass]
    public class FremoImport
    {
        [TestMethod]
        public void ReadsExcelSheed()
        {
            var result = new List<Data.Module>(200);
            var app = new Application();
            var workbooks = app.Workbooks;
            Workbook workbook = workbooks.Open(@"C:\Users\Stefan\Documents\FREMO Import data\H0-RE Schweden.xlsx");
            Worksheet worksheet = workbook.Worksheets[1];

            var colA = "X";
            var row = 0;
            while (!string.IsNullOrWhiteSpace(colA))
            {
                row++;
                var rowValues = (Array)(worksheet.get_Range(Cell("A", row), Cell("R", row)).Cells.Value);


            }
           
            
        }

        private static string Cell(string col, int row)=> col + row.ToString(CultureInfo.InvariantCulture);
    }

    public static class UsefulExtensions
    {
       public static string AsString(this Array? row, int col)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            return row.GetValue(1, col)?.ToString() ?? string.Empty;
        }

        public static int? AsInt(this Array? row, int col)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            return (int?)(row.GetValue(1, col));

        }
        public static short? AsShort(this Array? row, int col)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            return (short?)(row.GetValue(1, col));

        }
        public static double? AsDouble(this Array? row, int col)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            return (double?)(row.GetValue(1, col));

        }
        public static Data.Module AsModule(this Array? row) {
            var module = new Data.Module
            {
                FremoNumber = row.AsInt(2),
                NumberOfSections = row.AsShort(3),
                FullName = row.AsString(4),
                Radius = row.AsDouble(6),
                Angle = row.AsDouble(7),
                Length = row.AsDouble(8)!.Value
            };
            var gable1Id = EndPlateId(row.AsString(9));
            if (gable1Id.HasValue) module.ModuleExits.Add(new Data.ModuleExit { GablePropertyId = gable1Id.Value, Label = "Sida 1" });
            var gable2Id = EndPlateId(row.AsString(10));
            if (gable2Id.HasValue) module.ModuleExits.Add(new Data.ModuleExit { GablePropertyId = gable2Id.Value, Label = "Sida 2" });

            return module;
        }

        private static int? EndPlateId(string value) =>
            value switch
            {
                "B02" => 6,
                "B96" => 7,
                "E96" => 8,
                "F02" => 13,
                "F96" => 9,
                "IH05" => 14,
                _ => (int?)null
            };
    }
}
