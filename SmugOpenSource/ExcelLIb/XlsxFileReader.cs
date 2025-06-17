using System.Data;
using ExcelDataReader;

namespace SmugOpenSource.ExcelLIb
{
    public class XlsxFileReader
    {
        /// <summary>
        /// 지정한 xlsx 파일을 읽어 DataSet으로 반환합니다.
        /// </summary>
        /// <param name="filePath">읽을 xlsx 파일 경로</param>
        /// <returns>엑셀 파일의 모든 시트를 포함하는 DataSet</returns>
        public DataTable ReadXlsxToDataSet(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("파일 경로가 비어 있습니다.", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("지정한 파일을 찾을 수 없습니다.", filePath);

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            var result = reader.AsDataSet();

            // '#'이 포함된 테이블(시트) 삭제
            var tablesToRemove = new List<DataTable>();
            foreach (DataTable table in result.Tables)
            {
                if (table.TableName.Contains('#'))
                {
                    tablesToRemove.Add(table);
                }
            }
            foreach (var table in tablesToRemove)
            {
                result.Tables.Remove(table);
            }

            // 각 테이블에서 첫 번째 칼럼에 '#'이 포함된 Row 삭제
            foreach (DataTable table in result.Tables)
            {
                var rowsToRemove = new List<DataRow>();

                foreach (DataRow row in table.Rows)
                {
                    if (row.ItemArray.Length > 0 && row[0] != null && row[0] is string str && str.Contains('#'))
                    {
                        rowsToRemove.Add(row);
                    }
                }

                foreach (var row in rowsToRemove)
                {
                    table.Rows.Remove(row);
                }

                // Rows[2]에 '#'이 들어간 컬럼 삭제
                if (table.Rows.Count > 2)
                {
                    var colsToRemove = new List<DataColumn>();
                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        var cell = table.Rows[2][col];
                        if (cell != null && cell is string cellStr && cellStr.Contains('#'))
                        {
                            colsToRemove.Add(table.Columns[col]);
                        }
                    }
                    foreach (var col in colsToRemove)
                    {
                        table.Columns.Remove(col);
                    }
                }

                // 칼럼의미, 칼럼타입 row 삭제
                table.Rows.RemoveAt(0);     // 칼럼의미
                table.Rows.RemoveAt(1);     // 칼럼타입
            }

            int nCount = result.Tables[0].Rows.Count;

            System.Console.WriteLine($"{DateTime.Now}\tReading Excel file: {filePath}");
            return result.Tables[0];
        }
    }
}