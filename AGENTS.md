# AGENTS.md
LLM 에이전트에 가이드를 제공하는 용도로 사용하는 파일.

## 프로젝트 스펙
@SPEC.md 파일을 참고하여 작업

## 작업 가이드
1. 각 작업에 대해서 각각 git 커밋 생성 
   - 제목에는 작업 이름, 내용에는 해당 커밋의 작업 내역에 대한 설명 요약
   - 자잘한 수정에 대해서는 모아서 커밋 생성
2. 각 기능에 대해서 테스트 추가
   - 테스트에 실패하는 경우 스펙이 바뀐게 아닌 이상 테스트를 변경사항에 맞추는 게 아니라 변경사항을 테스트에 성공하도록 수정할 것
   - 너무 당연한 기능에 대해서는 굳이 테스트를 구현할 필요 없음
   - 검증이 꼭 필요하거나 다른 수정에 의해 동작이 달라질 수 있는 부분에 대해서 테스트 추가
3. SPEC.md 구현을 순서대로 해달라고 요청했을 때 구현 완료된 요청사항에 대해서 체크 표시

## 사용 가능한 명령어
```bash
# 빌드
$ dotnet build <프로젝트 파일>

# 빌드 결과물 실행
$ dotnet run <프로젝트 파일>

# 빌드 결과물 테스트
$ dotnet test

# 코드 포매팅
# (최초 프로젝트 세팅 시 `dotnet tool install csharpier` 명령 먼저 실행 필요)
$ dotnet csharpier format .
```

@.claude/claude-code-agent-workflow-principles-boris-cherny-claude.md
