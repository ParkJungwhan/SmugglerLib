# AGENTS TODOs

- 여기는 SmugglerLib에서 작성해야 할 라이브러리들의 모음을 정리한 파일임
- 기능 추가의 완성시 빌드 후 (성공시)커밋을 진행하는것이 ㅎ
- 라이브러리의 기능이 추가시 TDD 프로젝트에 Test용 모듈이 추가되어야 한다.

## 개발방향
- 필요한 기능이 개발에 대한 내용을 Tasks.md에 추가 하고 체크 여부를 확인하여 진행
- 구현 후에는 SmugglerTDDs 프로젝트에 xUnit의 TDD 클래스를 추가한다.
- TDD가 정상 진행 후에 알맞는 'Test[Project]'에 예제 기능을 추가한다.
- 전체 빌드를 진행한다.
- 빌드 완료 및 성공시 커밋을 진행한다.

## Priority Items

1. Configure `log4net.config` so that file logs do not exceed **5MB**. When the file exceeds this size, logs should roll over using a pattern `fileName_N.log` (e.g., `fileName_1.log`, `fileName_2.log`).

## Additional Notes

- The repository currently contains no source code. Future tasks may involve establishing basic project structure and adding initial implementation.
