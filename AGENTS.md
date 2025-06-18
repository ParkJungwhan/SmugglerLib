# AGENTS TODOs

This file lists tasks for SmugglerLib repository.

## 1. 개요

## 2. 종류 및 역할

## 3. 아키텍쳐


## 4. 프로젝트 구조
- 이 솔루션은 공용 라이브러리용으로 쓰기 위해 각종 라이브러리들을 래핑해서 모아두기 위해 만들어 놓은것.
- SmugControl은 각종 WPF 컨트롤을 만들어 놓고 이를 MVVM으로 바로 사용하기 위해 모아놓은것. 
- SmugCommon은 공용으로 필수적으로 쓰인다고 생각하는것들을 미리 다 모아놓은것.
- ThirdParty
  - SmugAWS는 AWS의 각종 기능을 연동하기 위해 만들어 놓은 프로젝트. 주로 EC2, EBS 등에 관련 기능을 하기위해 만들어졌다.
  - SmugMessenger는 카카오웤, 텔레그램, 슬랙 등 각종 메신저에 연동할 수 있는 API를 만들기 위해 만들어졌다.
  - SmugOpenSource는 자주 사용하는 오픈소스 라이브러리들을 미리 세팅된채로 만들어 놓기 위해 만들어졌다.


## TODOs
- [ ] CCU 출력을 위한 ServerView 위젯 컨트롤 생성
- [ ] CCU 출력하는 서버의 그룹 위젯 생성
- [ ] 그룹 위젯과 속한 서버위젯을 화면에 출력한 대쉬보드 생성 및 연동
- [ ] 작성중...


## Priority Items

1. Configure `log4net.config` so that file logs do not exceed **5MB**. When the file exceeds this size, logs should roll over using a pattern `fileName_N.log` (e.g., `fileName_1.log`, `fileName_2.log`).