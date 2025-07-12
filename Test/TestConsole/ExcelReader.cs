using System.Data;
using Amazon.Runtime.Internal.Util;
using SmugOpenSource.ExcelLIb;

namespace TestConsole
{
    public static class ExcelReader
    {
        public static void ReadExcelFile()
        {
            string filePath = @"D:\Repository\Work\PristontaleS\trunk\Resources\Assets\DB\NPC@Monster.xlsx";

            // This is a placeholder for the actual implementation of reading an Excel file. You
            // would typically use a library like EPPlus, ClosedXML, or NPOI to read Excel files in C#.

            DataTable dt = new DataTable();

            XlsxFileReader excel = new XlsxFileReader();
            var ds1 = excel.ReadXlsxToDataSet(filePath);
            dt.Merge(ds1);

            filePath = @"D:\Repository\Work\PristontaleS\trunk\Resources\Assets\DB\NPC@ChoiceTower.xlsx";
            var ds2 = excel.ReadXlsxToDataSet(filePath);
            dt.Merge(ds2);

            filePath = @"D:\Repository\Work\PristontaleS\trunk\Resources\Assets\DB\NPC@Bellatra.xlsx";
            var ds3 = excel.ReadXlsxToDataSet(filePath);
            dt.Merge(ds3);
        }
    }
}