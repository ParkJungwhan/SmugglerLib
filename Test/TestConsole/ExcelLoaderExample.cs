using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Smuggler.Common.ExcelLib;

namespace TestConsole;

/// <summary>
/// C0505: ExcelSheetLoader 사용 예제
/// </summary>
internal static class ExcelLoaderExample
{
    internal static void Run()
    {
        Console.WriteLine("=== Excel Loader 예제 시작 ===");
        Console.WriteLine();

        string tempDir = Path.Combine(Path.GetTempPath(), $"smug_excel_example_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            SetupSampleFiles(tempDir);

            var loader = new ExcelSheetLoader();

            RunSingleLoad(loader, tempDir);
            RunIgnoreRules(loader, tempDir);
            RunMapTo(loader, tempDir);
            RunMergedLoad(loader, tempDir);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }

        Console.WriteLine("=== Excel Loader 예제 종료 ===");
    }

    // -------------------------------------------------------------------------

    private static void RunSingleLoad(ExcelSheetLoader loader, string dir)
    {
        Console.WriteLine("--- 단일 시트 로드 ---");

        ExcelSheet sheet = loader.Load(Path.Combine(dir, "item.xlsx"));
        Console.WriteLine($"  시트명: {sheet.Name}");
        Console.WriteLine($"  헤더: {string.Join(", ", sheet.Headers)}");
        Console.WriteLine($"  행 수: {sheet.Rows.Count}");
        foreach (IReadOnlyDictionary<string, string?> row in sheet.Rows)
            Console.WriteLine($"    Id={row["Id"]}, Name={row["Name"]}, Price={row["Price"]}");

        Console.WriteLine();
    }

    private static void RunIgnoreRules(ExcelSheetLoader loader, string dir)
    {
        Console.WriteLine("--- '#' 행/컬럼 무시 규칙 ---");

        ExcelSheet sheet = loader.Load(Path.Combine(dir, "item_with_ignore.xlsx"), "item_ignore");
        Console.WriteLine($"  헤더 (Internal 제외): {string.Join(", ", sheet.Headers)}");
        Console.WriteLine($"  행 수 ('#' 행 제외): {sheet.Rows.Count}");

        Console.WriteLine();
    }

    private static void RunMapTo(ExcelSheetLoader loader, string dir)
    {
        Console.WriteLine("--- MapTo<T> 리플렉션 매핑 ---");

        ExcelSheet sheet = loader.Load(Path.Combine(dir, "item.xlsx"));
        List<ItemModel> items = sheet.MapTo<ItemModel>().ToList();
        foreach (ItemModel item in items)
            Console.WriteLine($"  [{item.Id}] {item.Name,-10} {item.Price,6:N0}원");

        Console.WriteLine();
    }

    private static void RunMergedLoad(ExcelSheetLoader loader, string dir)
    {
        Console.WriteLine("--- LoadMergedAuto: item.xlsx + item@market.xlsx 병합 ---");

        ExcelSheet merged = loader.LoadMergedAuto(
            Path.Combine(dir, "item.xlsx"), keyColumn: "Id");

        Console.WriteLine($"  병합 헤더: {string.Join(", ", merged.Headers)}");

        List<MergedItemModel> items = merged.MapTo<MergedItemModel>().ToList();
        foreach (MergedItemModel item in items)
            Console.WriteLine($"  [{item.Id}] {item.Name,-10} 가격: {item.Price,6:N0}원  재고: {item.Stock,4}개");

        Console.WriteLine();
    }

    // -------------------------------------------------------------------------
    // 샘플 Excel 파일 생성
    // -------------------------------------------------------------------------

    private static void SetupSampleFiles(string dir)
    {
        // item.xlsx: 기본 아이템 데이터
        using (XLWorkbook wb = new XLWorkbook())
        {
            IXLWorksheet ws = wb.AddWorksheet("item");
            ws.Cell(1, 1).Value = "Id";   ws.Cell(1, 2).Value = "Name";   ws.Cell(1, 3).Value = "Price";
            ws.Cell(2, 1).Value = 1;      ws.Cell(2, 2).Value = "검";      ws.Cell(2, 3).Value = 1500;
            ws.Cell(3, 1).Value = 2;      ws.Cell(3, 2).Value = "방패";    ws.Cell(3, 3).Value = 1200;
            ws.Cell(4, 1).Value = 3;      ws.Cell(4, 2).Value = "물약";    ws.Cell(4, 3).Value = 100;
            wb.SaveAs(Path.Combine(dir, "item.xlsx"));
        }

        // item@market.xlsx: 파티션 - 재고 정보
        using (XLWorkbook wb = new XLWorkbook())
        {
            IXLWorksheet ws = wb.AddWorksheet("item");
            ws.Cell(1, 1).Value = "Id"; ws.Cell(1, 2).Value = "Stock";
            ws.Cell(2, 1).Value = 1;    ws.Cell(2, 2).Value = 30;
            ws.Cell(3, 1).Value = 2;    ws.Cell(3, 2).Value = 15;
            ws.Cell(4, 1).Value = 3;    ws.Cell(4, 2).Value = 200;
            wb.SaveAs(Path.Combine(dir, "item@market.xlsx"));
        }

        // item_with_ignore.xlsx: '#' 규칙 예제
        using (XLWorkbook wb = new XLWorkbook())
        {
            IXLWorksheet ws = wb.AddWorksheet("item_ignore");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "#Internal"; // '#' 컬럼 무시
            ws.Cell(1, 3).Value = "Name";
            ws.Cell(2, 1).Value = 1;  ws.Cell(2, 2).Value = "secret"; ws.Cell(2, 3).Value = "검";
            ws.Cell(3, 1).Value = "#2"; // '#' 행 무시
            ws.Cell(3, 2).Value = "hidden"; ws.Cell(3, 3).Value = "방패";
            ws.Cell(4, 1).Value = 3;  ws.Cell(4, 2).Value = "ok";     ws.Cell(4, 3).Value = "물약";
            wb.SaveAs(Path.Combine(dir, "item_with_ignore.xlsx"));
        }
    }

    // -------------------------------------------------------------------------

    private class ItemModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Price { get; set; }
    }

    private class MergedItemModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Price { get; set; }
        public int Stock { get; set; }
    }
}
