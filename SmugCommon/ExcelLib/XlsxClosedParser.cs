using ClosedXML.Excel;

namespace SmugCommon.ExcelLib;

public class XlsxClosedParser
{
    private static string[] FixedColumnsType = new[] { "bigint", "int", "float", "nvarchar", "varchar", "ntext", "time", "datetime", "date" };

    public List<TableHeader> MDBDatas = new List<TableHeader>();

    public async Task<string> XlsxFileRead(string sheetname, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("파일 경로가 비어 있습니다.", nameof(filePath));
        if (false == File.Exists(filePath)) throw new FileNotFoundException("지정한 파일을 찾을 수 없습니다.", filePath);

        // 파일 읽기
        using var workbook = new XLWorkbook(filePath);
        if (false == workbook.Worksheets.TryGetWorksheet(sheetname, out var selectSheet))
            throw new ArgumentException($"해당 시트가 존재하지 않습니다. 문자열을 확인해주세요: {sheetname}", nameof(filePath));

        // 시작되는 col index, row index 받아오기
        var nStartindex = GetStartRowIndex(selectSheet);

        // 해당 시트
        var foundSheet = MDBDatas.Find(x => x.DBTableName == sheetname);
        if (foundSheet is null) return string.Empty;

        // 칼럼 헤더 모아오기
        var headers = GetSheetColumnHeader(selectSheet, nStartindex);

        //var startCell = selectSheet.Cell(nStartindex.Item1, nStartindex.Item2);
        //string callString = startCell.Value.ToString();

        // 단일 엑셀 파일에 대한 설정
        TableHeader header = new TableHeader();
        header.DBTableName = sheetname;

        MDBDatas.Add(header);

        return sheetname;
    }

    private List<ColumnHeaders> GetSheetColumnHeader(IXLWorksheet selectSheet, (int, int) nStartindex, string annotation = "#")
    {
        var headers = new List<ColumnHeaders>();

        var namerow = selectSheet.Row(nStartindex.Item1 + 1);
        var typerow = selectSheet.Row(nStartindex.Item1);

        int lastCol = selectSheet.LastCellUsed()?.Address.ColumnNumber ?? 0;

        for (int col = 1; col <= lastCol; col++)
        {
            var cell = namerow.Cell(col);

            // 여기서 원하는 다른 처리 가능
            // 해당 셀이 빈칸이면 칼럼은 무시
            if (cell.IsEmpty()) continue;

            var value = cell.GetString();

            // 예: 값 검증 / 로깅 / 변환
            //# 으로 시작하면 해당 칼럼은 무시
            if (value.StartsWith(annotation)) continue;

            var typecell = typerow.Cell(col);
            var typevalue = typecell.GetString();

            ColumnHeaders header = new ColumnHeaders();
            header.ColumnName = value;
            header.TypeName = typevalue;
            header.ColIndex = col;
            headers.Add(header);
        }

        return headers;
    }

    private (int, int) GetStartRowIndex(IXLWorksheet selectSheet)
    {
        // TODO : 시트의 cell을 0,0 부터 시작해서 FixedColumnsType에 포함된 문자열이 검출될때까지의 col, row index 값 찾기 (최대 10,10 까지)
        // selectSheet가 null이면 기본(0,0) 반환
        if (selectSheet == null)
            return (0, 0);

        const int maxRow = 10;
        const int maxCol = 10;

        // ClosedXML은 1-based 인덱스 사용
        for (int row = 1; row <= maxRow; row++)
        {
            for (int col = 1; col <= maxCol; col++)
            {
                var cell = selectSheet.Cell(row, col);
                if (cell == null)
                    continue;

                string text;
                try
                {
                    text = cell.GetString();
                }
                catch
                {
                    var v = cell.Value;
                    text = v.ToString() ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(text))
                    continue;

                var normalized = text.Trim().ToLowerInvariant();

                foreach (var t in FixedColumnsType)
                {
                    if (normalized.Equals(t, StringComparison.OrdinalIgnoreCase) || normalized.Contains(t))
                    {
                        // 요청대로 1-based (col, row) 반환
                        return (col, row);
                        //return (col - 1, row - 1);
                    }
                }
            }
        }

        // 찾지 못하면 기본 (0,0)
        return (0, 0);
    }
}

public class TableHeader
{
    public string DBTableName { get; set; }
    public List<ColumnHeaders> ColumnHeaders { get; set; }
}

public class ColumnHeaders
{
    public string TypeName { get; set; } = "-";
    public string ColumnName { get; set; } = "-";
    public int ColIndex { get; set; } = -1;   // 1 base
}