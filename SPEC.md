# dotori — C++ 빌드 시스템 + 패키지 매니저

C# (.NET 10, NativeAOT). 설계 문서: `docs/` 참조

---

## 구현 현황

**완료**: Phase 1-A~1-H, 1-J~1-N, Phase 2, Phase 4 (Docker/K8s 제외)

---

## 미구현 항목

### Phase 1-I: 플랫폼 검증

- [ ] `windows-x64` (MSVC)
- [ ] `windows-x64` (Clang + lld-link)
- [ ] `linux-x64` (glibc + libstdc++ + dynamic)
- [ ] `linux-x64` (musl + static)
- [ ] `ios-arm64` (macOS에서 크로스)
- [ ] `android-arm64`
- [ ] `wasm32-emscripten`
- [ ] `wasm32-bare`

---

### Phase 1-N: 옵션(Options) 기능

`option <name>` 블록 선언 (`default`, `defines`, `dependencies`). CLI `--<name>`/`--no-<name>`. 환경변수 `DOTORI_OPTION_<NAME>`. `[<name>]` 조건 블록과 자동 연동.

- [x] AST `OptionBlock` 추가 (`Ast.cs`)
- [x] 파서 `option` 블록 지원 (`Parser.Blocks.cs`)
- [x] Formatter `OptionBlock` 포매팅 (`DotoriFormatter.cs`)
- [x] `TargetContext.EnabledOptions` + `ActiveAtoms()` 연동
- [x] `ProjectFlattener` 옵션 적용 (defines / dependencies)
- [x] `BuildContext.ScanOptions()` + `MakeTargetContext()` 환경변수 주입
- [x] `BuildCommand` 동적 `--X` / `--no-X` 처리
- [x] 단위 테스트 (파서, 플래트너, 환경변수)
- [x] `docs/dotori-file.md`, `docs/cli.md` 업데이트

---

### Phase 4: 레지스트리 서버

- [ ] Docker / Kubernetes 배포 구성

---

### Phase 5: Tree-sitter 문법 (`grammar/tree-sitter-dotori/`)

현재 `dotori export grammar --format zed`는 `highlights.scm`만 생성하며, 실제 Tree-sitter 파서가 없다. `grammar.js`를 작성하면 Zed, Helix, Neovim(nvim-treesitter) 등 모든 Tree-sitter 기반 에디터에서 정확한 구문 분석이 가능해진다.

- [ ] `grammar.js` 작성 (`docs/dotori-file.md` EBNF 기반)
- [ ] `tree-sitter generate`로 `src/parser.c` 생성 및 커밋
- [ ] `highlights.scm` 개선 (실제 node type에 맞게 정밀화)
- [ ] `build.sh`에 플랫폼별 파서 컴파일 단계 추가 (`tree-sitter build`)
