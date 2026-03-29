# SmugglerLib
본인이 프로젝트에 공용으로 참조하여 언제나 붙여서 사용할 수 있는 라이브러리들의 모음.

## NuGet Packages

현재 프로젝트에서 사용하는 주요 NuGet 패키지를 용도별로 정리하면 아래와 같습니다.

| Package | Version | Usage |
| --- | --- | --- |
| HandyControls | 3.6.0 | WPF UI 컨트롤 및 공통 스타일 구성 |
| Dapper | 2.1.72 | DB 커넥터 |
| ExcelDataReader | 3.7.0 | Excel Controller |
| Log4net | 3.3.0 | Logger |
| MemoryPack | 1.21.4 | 패킷/바이너리 Formatter |

Preview 버전의 패키지는 참조하지 않습니다.

## Test Projects
테스트용 프로젝트들을 구현하여 사용예 와 참조하는 패키지들의 버전이 변경될때마다 Test를 진행하여 이슈를 확인합니다.
1. TestAPI
 - API 서버에서 사용할 수 있는 라이브러리 Test 모음
2. TestConSole
 - Console/공용환경에서 사용할 수 있는 라이브러리 Test 모음
3. TestWindow
 - Window/WPF 등에서 사용할 수 있는 라이브러리 Test 모음

