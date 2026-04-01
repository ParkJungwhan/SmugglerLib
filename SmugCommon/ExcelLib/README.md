# SmugOpenSource

## Excel Templete
- 파일명과 시트명을 맞춘다. 
  - Item.xlsx, Item(SheetName)
- 지원되는 칼럼타입
  - 숫자형: "bigint"(double), "int", "float"
  - 문자형(string): "nvarchar", "varchar", "ntext"
  - 시간/날짜형(DateTime): "time(HH:mm:ss.fff)", "datetime(yyyy-MM-dd HH:mm:ss, nullable)", "date(yyyy-MM-dd, nullable)"
- DataRow의 첫번째 Column값에 '#'이 있을시 해당 Row는 무시(Non Parsing)한다.
- 파티셔닝된 엑셀파일을 하나로 머지하여 DB에 반영
  - Item.xlsx
  - Item@PakcgeBox.xlsx
  - Item@Equip.xlsx
  - ...
  
## Excel Reader/Parser
- 엑셀 파일로 만든 MDB 데이터를 DB에 Update하게 하는 전단계 처리절차
- SQLServer(MSSQL) : Bulk Insert
```sql
BULK INSERT dbo.{Table} FROM '{BulkDataFileFullPath}' WITH (FORMAT = 'CSV', FIELDTERMINATOR = '0x09', ROWTERMINATOR = '\n', CODEPAGE = 65001) 
```

## 참조 라이브러리
[ClosedXML](https://github.com/ClosedXML/ClosedXML) (MIT License)
