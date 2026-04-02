using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace Smuggler.Common.ExcelLib;

/// <summary>
/// C0501: ClosedXML 기반 엑셀 시트 로딩 래퍼 클래스.
/// </summary>
/// <remarks>
/// <b>시트 포맷 규칙:</b><br/>
/// - 헤더 행: 컬럼명. '#' 접두사 컬럼은 무시된다.<br/>
/// - 데이터 행: 첫 셀이 '#'으로 시작하면 해당 행은 무시된다.<br/>
/// - hasTypeRow=true: 첫 행이 타입 힌트(int, bigint 등), 두 번째 행이 헤더 행으로 처리된다.<br/>
/// <br/>
/// <b>파일명 규칙:</b><br/>
/// sheetName을 지정하지 않으면 파일명(확장자 제외)을 시트명으로 사용한다.<br/>
/// 파티션 파일명 규칙: <c>BaseName@PartName.xlsx</c>
/// </remarks>
public class ExcelSheetLoader
{
    private const char IgnorePrefix = '#';

    // -------------------------------------------------------------------------
    // C0501: 단일 시트 로딩
    // -------------------------------------------------------------------------

    /// <summary>
    /// 지정한 xlsx 파일에서 시트를 읽어 <see cref="ExcelSheet"/>를 반환한다.
    /// </summary>
    /// <param name="filePath">읽을 xlsx 파일 경로.</param>
    /// <param name="sheetName">읽을 시트 이름. null이면 파일명(확장자 제외)을 사용한다.</param>
    /// <param name="hasTypeRow">true이면 첫 행을 타입 힌트 행으로 간주하고 두 번째 행을 헤더로 읽는다.</param>
    /// <exception cref="ArgumentException">filePath가 비어 있거나 시트가 존재하지 않을 때.</exception>
    /// <exception cref="FileNotFoundException">파일이 존재하지 않을 때.</exception>
    public ExcelSheet Load(string filePath, string? sheetName = null, bool hasTypeRow = false)
    {
        ValidateFilePath(filePath);

        string resolvedName = sheetName ?? Path.GetFileNameWithoutExtension(filePath);

        using XLWorkbook workbook = new XLWorkbook(filePath);
        if (!workbook.Worksheets.TryGetWorksheet(resolvedName, out IXLWorksheet? sheet))
            throw new ArgumentException($"시트를 찾을 수 없습니다: '{resolvedName}'", nameof(sheetName));

        return ReadSheet(sheet, resolvedName, hasTypeRow);
    }

    // -------------------------------------------------------------------------
    // C0502: 파티션 파일 통합 로딩
    // -------------------------------------------------------------------------

    /// <summary>
    /// 기준 파일과 파티션 파일들을 keyColumn 기준으로 수평 병합하여 반환한다.
    /// </summary>
    /// <param name="baseFilePath">기준 xlsx 파일 경로.</param>
    /// <param name="partitionFilePaths">병합할 파티션 파일 경로 목록.</param>
    /// <param name="keyColumn">행을 매칭할 키 컬럼명 (예: "Id").</param>
    /// <param name="hasTypeRow">타입 행 포함 여부.</param>
    public ExcelSheet LoadMerged(
        string baseFilePath,
        IEnumerable<string> partitionFilePaths,
        string keyColumn,
        bool hasTypeRow = false)
    {
        ExcelSheet merged = Load(baseFilePath, hasTypeRow: hasTypeRow);

        foreach (string partPath in partitionFilePaths)
        {
            ExcelSheet partSheet = Load(partPath, merged.Name, hasTypeRow);
            merged = MergeSheets(merged, partSheet, keyColumn);
        }

        return merged;
    }

    /// <summary>
    /// 기준 파일과 같은 디렉터리에서 <c>BaseName@*.xlsx</c> 패턴의 파티션 파일을 자동 탐색하여 병합한다.
    /// </summary>
    /// <param name="baseFilePath">기준 xlsx 파일 경로.</param>
    /// <param name="keyColumn">행을 매칭할 키 컬럼명.</param>
    /// <param name="hasTypeRow">타입 행 포함 여부.</param>
    public ExcelSheet LoadMergedAuto(string baseFilePath, string keyColumn, bool hasTypeRow = false)
    {
        ValidateFilePath(baseFilePath);

        string dir      = Path.GetDirectoryName(Path.GetFullPath(baseFilePath)) ?? ".";
        string baseName = Path.GetFileNameWithoutExtension(baseFilePath);
        string ext      = Path.GetExtension(baseFilePath);

        IEnumerable<string> partitions = Directory
            .GetFiles(dir, $"{baseName}@*{ext}")
            .OrderBy(f => f);

        return LoadMerged(baseFilePath, partitions, keyColumn, hasTypeRow);
    }

    // -------------------------------------------------------------------------
    // 내부 구현
    // -------------------------------------------------------------------------

    private static ExcelSheet ReadSheet(IXLWorksheet sheet, string name, bool hasTypeRow)
    {
        int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
        int lastCol = sheet.LastColumnUsed()?.ColumnNumber() ?? 0;

        if (lastRow == 0 || lastCol == 0)
            return new ExcelSheet(name, new List<string>(), new List<IReadOnlyDictionary<string, string?>>());

        int headerRowNum = hasTypeRow ? 2 : 1;

        // 헤더 수집: '#' 접두사 컬럼 제외
        List<(int ColIndex, string ColName)> headers = new();
        for (int col = 1; col <= lastCol; col++)
        {
            string val = sheet.Cell(headerRowNum, col).GetString().Trim();
            if (string.IsNullOrEmpty(val) || val.StartsWith(IgnorePrefix))
                continue;
            headers.Add((col, val));
        }

        // 데이터 행 수집
        List<IReadOnlyDictionary<string, string?>> rows = new();
        for (int row = headerRowNum + 1; row <= lastRow; row++)
        {
            // '#' 접두사 행 제외
            string firstCell = sheet.Cell(row, 1).GetString();
            if (firstCell.StartsWith(IgnorePrefix))
                continue;

            // 빈 행 제외
            bool isEmpty = headers.All(h => sheet.Cell(row, h.ColIndex).IsEmpty());
            if (isEmpty)
                continue;

            Dictionary<string, string?> rowData = new();
            foreach ((int colIdx, string colName) in headers)
                rowData[colName] = GetCellValue(sheet.Cell(row, colIdx));

            rows.Add(rowData);
        }

        return new ExcelSheet(name, headers.Select(h => h.ColName).ToList(), rows);
    }

    /// <summary>
    /// baseSheet와 partSheet를 keyColumn 기준으로 수평 병합한다.<br/>
    /// partSheet에만 있는 컬럼을 baseSheet 행에 추가한다.
    /// </summary>
    private static ExcelSheet MergeSheets(ExcelSheet baseSheet, ExcelSheet partSheet, string keyColumn)
    {
        // 기존 헤더 + 파티션의 새 헤더 (키 컬럼 중복 제외)
        List<string> mergedHeaders = baseSheet.Headers.ToList();
        foreach (string h in partSheet.Headers)
        {
            if (!h.Equals(keyColumn, StringComparison.OrdinalIgnoreCase)
                && !mergedHeaders.Contains(h, StringComparer.OrdinalIgnoreCase))
            {
                mergedHeaders.Add(h);
            }
        }

        // 파티션 행을 키 기준으로 인덱싱
        Dictionary<string, IReadOnlyDictionary<string, string?>> partLookup = partSheet.Rows
            .Where(r => r.TryGetValue(keyColumn, out string? k) && k is not null)
            .GroupBy(r => r[keyColumn]!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // 기준 행에 파티션 컬럼을 병합
        List<IReadOnlyDictionary<string, string?>> mergedRows = new();
        foreach (IReadOnlyDictionary<string, string?> baseRow in baseSheet.Rows)
        {
            Dictionary<string, string?> merged = new(baseRow, StringComparer.OrdinalIgnoreCase);

            if (baseRow.TryGetValue(keyColumn, out string? key)
                && key is not null
                && partLookup.TryGetValue(key, out IReadOnlyDictionary<string, string?>? partRow))
            {
                foreach (KeyValuePair<string, string?> cell in partRow)
                {
                    if (!cell.Key.Equals(keyColumn, StringComparison.OrdinalIgnoreCase))
                        merged[cell.Key] = cell.Value;
                }
            }

            mergedRows.Add(merged);
        }

        return new ExcelSheet(baseSheet.Name, mergedHeaders, mergedRows);
    }

    private static string? GetCellValue(IXLCell cell)
    {
        if (cell.IsEmpty())
            return null;

        string s = cell.GetString().Trim();
        return s.Length == 0 ? null : s;
    }

    private static void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("파일 경로가 비어 있습니다.", nameof(filePath));
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Excel 파일을 찾을 수 없습니다: {filePath}", filePath);
    }
}
