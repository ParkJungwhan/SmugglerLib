using System.Data;
using System.Text;
using ExcelDataReader;

namespace TestCommonConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            //System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string Path = @"D:\Repository\Work\PristontaleS\trunk\Resources\Assets\DB\RuneSocket.xlsx";
            var result = GetExcelFile(Path);
            var rowdata = result.Tables[1].Rows[4];
        }

        public static DataSet GetExcelFile(string Path)
        {
            DataSet result = new DataSet();
            using FileStream fileStream = File.Open(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (IExcelDataReader excelDataReader = ExcelReaderFactory.CreateReader(fileStream))
            {
                result = excelDataReader.AsDataSet();
                excelDataReader.Close();
            }

            fileStream.Close();
            return result;
        }
    }
}