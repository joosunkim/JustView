# JustView 프로젝트 지침

## 프로젝트 개요
Windows 전용 PDF 뷰어. WinUI 3 + C# 기반.
성능과 단순함이 핵심. 기능은 최소한으로 유지.
실사용 목적의 완성도 높은 앱을 목표로 한다.

## 앱 스펙
- 빠른 파일 열기
- 페이지 확대/축소
- 페이지 맞춤 확대 (너비 맞춤 / 페이지 맞춤)
- 2페이지 보기 모드 (표지 단독 처리 여부 토글)
- 텍스트 검색
- 편집 기능 없음
- UI는 최대한 단순하고 직관적으로

## 기술 스택
- 언어: C#
- 프레임워크: WinUI 3
- PDF 렌더링: PdfiumViewer.Updated (Apache 2.0) — Google PDFium 기반
- 개발 도구: Visual Studio 2026, Claude Code

## 오픈소스 원칙
- MIT 라이선스 적용
- 사용 라이브러리: MIT / Apache 2.0 / BSD 라이선스만 허용
- API 키, 개인정보, 민감한 설정값은 저장소에 포함하지 않음

## 작업 원칙
- 작업 단위를 작게 나눠서 진행, 각 단계마다 동작 확인
- 기능과 완성도를 함께 고려 (실사용 가능한 수준)
- 추후 Mac / Linux 확장 가능성 염두에 두되 지금은 Windows만 집중

## 커밋 컨벤션
- Conventional Commits 형식 사용
- prefix는 영어, 내용은 한국어
- 예: `feat: PDF 파일 열기 기능 추가`