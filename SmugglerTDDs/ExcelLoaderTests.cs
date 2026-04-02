using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Smuggler.Common.ExcelLib;

namespace SmugglerTDDs;

/// <summary>
/// C0504: ExcelSheetLoader 샘플 Excel 파일 기반 테스트
/// </summary>
public class ExcelLoaderTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ExcelSheetLoader _loader = new ExcelSheetLoader();

    public ExcelLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"smug_excel_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // =========================================================================
    // C0501: Load - 헤더 / 행 수 검증
    // =========================================================================

    [Fact]
    public void Load_SimpleSheet_ReturnsCorrectHeaders()
    {
        string path = CreateItemFile();

        ExcelSheet sheet = _loader.Load(path);

        Assert.Equal(new[] { "Id", "Name", "Price" }, sheet.Headers);
    }

    [Fact]
    public void Load_SimpleSheet_ReturnsCorrectRowCount()
    {
        string path = CreateItemFile();

        ExcelSheet sheet = _loader.Load(path);

        Assert.Equal(3, sheet.Rows.Count);
    }

    [Fact]
    public void Load_SheetNameFromFileName_WhenSheetNameOmitted()
    {
        // 파일명 = "item" → 시트명도 "item"
        string path = CreateItemFile();

        ExcelSheet sheet = _loader.Load(path);

        Assert.Equal("item", sheet.Name);
    }

    [Fact]
    public void Load_NonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() =>
            _loader.Load(Path.Combine(_tempDir, "ghost.xlsx")));
    }

    [Fact]
    public void Load_NonExistentSheet_ThrowsArgumentException()
    {
        string path = CreateItemFile();

        Assert.Throws<ArgumentException>(() =>
            _loader.Load(path, "NoSuchSheet"));
    }

    // =========================================================================
    // '#' 무시 정책
    // =========================================================================

    [Fact]
    public void Load_IgnoresRowsWithHashPrefix()
    {
        string path = CreateFileWithIgnoredRows();

        ExcelSheet sheet = _loader.Load(path, "data");

        // '#' 접두사 행은 제외, 정상 행 2개만 남아야 함
        Assert.Equal(2, sheet.Rows.Count);
    }

    [Fact]
    public void Load_IgnoresColumnsWithHashPrefix()
    {
        string path = CreateFileWithIgnoredColumns();

        ExcelSheet sheet = _loader.Load(path, "data");

        // '#Internal' 컬럼은 제외되어야 함
        Assert.DoesNotContain("Internal", sheet.Headers);
        Assert.Contains("Id", sheet.Headers);
        Assert.Contains("Name", sheet.Headers);
    }

    // =========================================================================
    // C0503: null / 빈 셀 변환 정책
    // =========================================================================

    [Fact]
    public void Load_EmptyCell_StoredAsNull()
    {
        string path = CreateFileWithNullCells();

        ExcelSheet sheet = _loader.Load(path, "data");

        // 두 번째 행의 Name 셀이 null이어야 함
        Assert.Null(sheet.Rows[1]["Name"]);
    }

    // =========================================================================
    // C0502: MapTo<T> - 리플렉션 매핑
    // =========================================================================

    [Fact]
    public void MapTo_MapsHeadersToProperties()
    {
        string path = CreateItemFile();

        ExcelSheet sheet = _loader.Load(path);
        List<ItemRow> items = sheet.MapTo<ItemRow>().ToList();

        Assert.Equal(3, items.Count);
        Assert.Equal(1, items[0].Id);
        Assert.Equal("Sword", items[0].Name);
        Assert.Equal(1000, items[0].Price);
    }

    [Fact]
    public void MapTo_NullCell_UsesDefaultForValueType()
    {
        string path = CreateFileWithNullCells();

        ExcelSheet sheet = _loader.Load(path, "data");
        List<NullableRow> rows = sheet.MapTo<NullableRow>().ToList();

        Assert.Equal(2, rows.Count);
        Assert.Null(rows[1].Name);    // null 셀 → null string
        Assert.Equal(0, rows[1].Score); // null 셀 → int default (0)
    }

    // =========================================================================
    // C0502: LoadMerged - 파티션 파일 수평 병합
    // =========================================================================

    [Fact]
    public void LoadMerged_CombinesColumnsFromPartitionFiles()
    {
        (string baseFile, string partFile) = CreatePartitionedFiles();

        ExcelSheet merged = _loader.LoadMerged(baseFile, new[] { partFile }, keyColumn: "Id");

        // 병합 후 헤더: Id, Name, Price + Stock (파티션 추가 컬럼)
        Assert.Contains("Id",    merged.Headers);
        Assert.Contains("Name",  merged.Headers);
        Assert.Contains("Price", merged.Headers);
        Assert.Contains("Stock", merged.Headers);
    }

    [Fact]
    public void LoadMerged_MatchesRowsByKeyColumn()
    {
        (string baseFile, string partFile) = CreatePartitionedFiles();

        ExcelSheet merged = _loader.LoadMerged(baseFile, new[] { partFile }, keyColumn: "Id");
        List<MergedItemRow> items = merged.MapTo<MergedItemRow>().ToList();

        Assert.Equal(3, items.Count);
        Assert.Equal("50", merged.Rows.First(r => r["Id"] == "1")["Stock"]);
        Assert.Equal("30", merged.Rows.First(r => r["Id"] == "2")["Stock"]);
    }

    [Fact]
    public void LoadMergedAuto_AutoDiscoversPartitionFiles()
    {
        (string baseFile, string partFile) = CreatePartitionedFiles();

        // baseFile과 같은 디렉터리의 item@*.xlsx 자동 탐색
        ExcelSheet merged = _loader.LoadMergedAuto(baseFile, keyColumn: "Id");

        Assert.Contains("Stock", merged.Headers);
    }

    // =========================================================================
    // ExcelCellConverter 직접 검증 (C0503)
    // =========================================================================

    [Theory]
    [InlineData("bigint",   "12345",      typeof(long))]
    [InlineData("int",      "42",         typeof(int))]
    [InlineData("float",    "3.14",       typeof(double))]
    [InlineData("nvarchar", "hello",      typeof(string))]
    [InlineData("date",     "2024-01-15", typeof(DateTime))]
    public void CellConverter_Convert_ReturnsExpectedType(string typeHint, string value, Type expectedType)
    {
        object? result = ExcelCellConverter.Convert(typeHint, value);

        Assert.NotNull(result);
        Assert.IsType(expectedType, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("-")]
    [InlineData("null")]
    [InlineData("NULL")]
    public void CellConverter_Convert_NullSentinel_ReturnsNull(string? value)
    {
        object? result = ExcelCellConverter.Convert("int", value);

        Assert.Null(result);
    }

    // =========================================================================
    // 테스트용 Excel 파일 생성 헬퍼
    // =========================================================================

    private string CreateItemFile()
    {
        string path = Path.Combine(_tempDir, "item.xlsx");
        using XLWorkbook wb = new XLWorkbook();
        IXLWorksheet ws = wb.AddWorksheet("item");

        ws.Cell(1, 1).Value = "Id";
        ws.Cell(1, 2).Value = "Name";
        ws.Cell(1, 3).Value = "Price";

        ws.Cell(2, 1).Value = 1;  ws.Cell(2, 2).Value = "Sword";  ws.Cell(2, 3).Value = 1000;
        ws.Cell(3, 1).Value = 2;  ws.Cell(3, 2).Value = "Shield"; ws.Cell(3, 3).Value = 800;
        ws.Cell(4, 1).Value = 3;  ws.Cell(4, 2).Value = "Potion"; ws.Cell(4, 3).Value = 50;

        wb.SaveAs(path);
        return path;
    }

    private string CreateFileWithIgnoredRows()
    {
        string path = Path.Combine(_tempDir, "ignored_rows.xlsx");
        using XLWorkbook wb = new XLWorkbook();
        IXLWorksheet ws = wb.AddWorksheet("data");

        ws.Cell(1, 1).Value = "Id";
        ws.Cell(1, 2).Value = "Name";

        ws.Cell(2, 1).Value = 1;  ws.Cell(2, 2).Value = "Alice";
        ws.Cell(3, 1).Value = "#2"; ws.Cell(3, 2).Value = "Ignored"; // '#' 접두사 → 무시
        ws.Cell(4, 1).Value = 3;  ws.Cell(4, 2).Value = "Carol";

        wb.SaveAs(path);
        return path;
    }

    private string CreateFileWithIgnoredColumns()
    {
        string path = Path.Combine(_tempDir, "ignored_cols.xlsx");
        using XLWorkbook wb = new XLWorkbook();
        IXLWorksheet ws = wb.AddWorksheet("data");

        ws.Cell(1, 1).Value = "Id";
        ws.Cell(1, 2).Value = "#Internal"; // '#' 접두사 → 무시
        ws.Cell(1, 3).Value = "Name";

        ws.Cell(2, 1).Value = 1; ws.Cell(2, 2).Value = "secret"; ws.Cell(2, 3).Value = "Alice";

        wb.SaveAs(path);
        return path;
    }

    private string CreateFileWithNullCells()
    {
        string path = Path.Combine(_tempDir, "null_cells.xlsx");
        using XLWorkbook wb = new XLWorkbook();
        IXLWorksheet ws = wb.AddWorksheet("data");

        ws.Cell(1, 1).Value = "Id";
        ws.Cell(1, 2).Value = "Name";
        ws.Cell(1, 3).Value = "Score";

        ws.Cell(2, 1).Value = 1; ws.Cell(2, 2).Value = "Alice"; ws.Cell(2, 3).Value = 90;
        ws.Cell(3, 1).Value = 2; // Name, Score 비워둠

        wb.SaveAs(path);
        return path;
    }

    private (string BaseFile, string PartFile) CreatePartitionedFiles()
    {
        string basePath = Path.Combine(_tempDir, "item.xlsx");
        string partPath = Path.Combine(_tempDir, "item@market.xlsx");

        using (XLWorkbook wb = new XLWorkbook())
        {
            IXLWorksheet ws = wb.AddWorksheet("item");
            ws.Cell(1, 1).Value = "Id";  ws.Cell(1, 2).Value = "Name"; ws.Cell(1, 3).Value = "Price";
            ws.Cell(2, 1).Value = 1;     ws.Cell(2, 2).Value = "Sword"; ws.Cell(2, 3).Value = 1000;
            ws.Cell(3, 1).Value = 2;     ws.Cell(3, 2).Value = "Shield"; ws.Cell(3, 3).Value = 800;
            ws.Cell(4, 1).Value = 3;     ws.Cell(4, 2).Value = "Potion"; ws.Cell(4, 3).Value = 50;
            wb.SaveAs(basePath);
        }

        using (XLWorkbook wb = new XLWorkbook())
        {
            IXLWorksheet ws = wb.AddWorksheet("item");
            ws.Cell(1, 1).Value = "Id"; ws.Cell(1, 2).Value = "Stock";
            ws.Cell(2, 1).Value = 1;    ws.Cell(2, 2).Value = 50;
            ws.Cell(3, 1).Value = 2;    ws.Cell(3, 2).Value = 30;
            ws.Cell(4, 1).Value = 3;    ws.Cell(4, 2).Value = 100;
            wb.SaveAs(partPath);
        }

        return (basePath, partPath);
    }

    // =========================================================================
    // 매핑용 레코드
    // =========================================================================

    private class ItemRow
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Price { get; set; }
    }

    private class NullableRow
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Score { get; set; }
    }

    private class MergedItemRow
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Price { get; set; }
        public int Stock { get; set; }
    }
}
