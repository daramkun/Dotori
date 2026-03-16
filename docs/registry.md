# 레지스트리 서버

dotori 레지스트리 서버는 C++ 패키지를 배포하고 검색할 수 있는 REST API 서버입니다.
ASP.NET Core 기반으로 구현되어 있으며 다양한 데이터베이스와 스토리지 백엔드를 지원합니다.

---

## 빠른 시작 (Docker Compose)

```bash
# 기본 구성 (SQLite + 로컬 파일 시스템)
docker compose up registry

# S3 호환 스토리지 (MinIO) 사용
docker compose --profile s3 up registry-s3 minio
```

기본 포트: `8080`

---

## 환경 변수 구성

### 기본 설정

| 변수 | 기본값 | 설명 |
|------|--------|------|
| `Registry__Mode` | `standalone` | 레지스트리 운영 모드 |
| `Registry__StorageRoot` | `/data/packages` | 패키지 파일 저장 경로 (파일시스템 모드) |
| `ASPNETCORE_URLS` | `http://+:8080` | 서버 수신 주소 |

### 데이터베이스

| 변수 | 기본값 | 설명 |
|------|--------|------|
| `ConnectionStrings__Default` | `Data Source=/data/registry.db` | DB 연결 문자열 |
| `Database__Provider` | `sqlite` | DB 엔진: `sqlite`, `postgres`, `mysql`, `oracle` |

### 인증 (JWT)

| 변수 | 설명 |
|------|------|
| `OAuth__Jwt__Secret` | JWT 서명 시크릿 (최소 32자, **운영 환경에서 반드시 변경**) |

### GitHub OAuth

| 변수 | 기본값 | 설명 |
|------|--------|------|
| `OAuth__Providers__github__Enabled` | `false` | GitHub OAuth 활성화 |
| `OAuth__Providers__github__ClientId` | — | GitHub OAuth App Client ID |
| `OAuth__Providers__github__ClientSecret` | — | GitHub OAuth App Client Secret |

### S3 스토리지

| 변수 | 설명 |
|------|------|
| `AWS_ACCESS_KEY_ID` | S3 액세스 키 |
| `AWS_SECRET_ACCESS_KEY` | S3 시크릿 키 |
| `AWS_REGION` | S3 리전 (예: `us-east-1`) |
| `S3_BUCKET` | S3 버킷 이름 |
| `S3_ENDPOINT` | S3 엔드포인트 (MinIO 등 호환 서비스용) |

---

## 데이터베이스 백엔드

### SQLite (기본)

별도 설치 없이 단일 파일로 운영됩니다. 소규모 팀 또는 테스트 환경에 적합합니다.

```yaml
environment:
  - ConnectionStrings__Default=Data Source=/data/registry.db
```

### PostgreSQL

```bash
docker compose --profile postgres up registry-postgres postgres
```

```yaml
environment:
  - Database__Provider=postgres
  - ConnectionStrings__Default=Host=postgres;Database=dotori;Username=dotori;Password=dotori
```

### MySQL / MariaDB

```bash
docker compose --profile mysql up registry-mysql mysql
```

```yaml
environment:
  - Database__Provider=mysql
  - ConnectionStrings__Default=Server=mysql;Database=dotori;User=dotori;Password=dotori;
```

### Oracle XE

```bash
docker compose --profile oracle up registry-oracle oracle
```

```yaml
environment:
  - Database__Provider=oracle
  - ConnectionStrings__Default=User Id=dotori;Password=dotori;Data Source=oracle/XEPDB1
```

---

## 스토리지 백엔드

### 로컬 파일 시스템 (기본)

```yaml
environment:
  - Registry__StorageRoot=/data/packages
volumes:
  - registry-data:/data
```

### S3 호환 스토리지 (MinIO / AWS S3)

MinIO와 함께 실행하는 예시:

```bash
# .env 파일 생성
cat > .env << 'EOF'
MINIO_ROOT_USER=minioadmin
MINIO_ROOT_PASSWORD=minioadmin
DOTORI_JWT_SECRET=your-secret-key-minimum-32-characters
EOF

docker compose --profile s3 up
```

AWS S3를 사용하는 경우:

```yaml
environment:
  - AWS_ACCESS_KEY_ID=AKIAIOSFODNN7EXAMPLE
  - AWS_SECRET_ACCESS_KEY=wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
  - AWS_REGION=ap-northeast-2
  - S3_BUCKET=dotori-packages
  # S3_ENDPOINT는 AWS S3 사용 시 생략
```

---

## REST API

### 패키지 조회

```
GET /packages/{name}
GET /packages/{name}/{version}
```

응답 예시 (`GET /packages/fmt`):

```json
{
  "name": "fmt",
  "description": "A modern formatting library",
  "license": "MIT",
  "homepage": "https://fmt.dev",
  "versions": [
    { "version": "10.2.0", "publishedAt": "2024-01-15T00:00:00Z" },
    { "version": "10.1.1", "publishedAt": "2023-10-20T00:00:00Z" }
  ]
}
```

### 패키지 검색

```
GET /packages/search?q=<키워드>&page=1&per_page=20
```

### 패키지 다운로드

```
GET /packages/{name}/{version}/download
```

### 패키지 배포

인증이 필요합니다.

```
POST /packages/publish
Content-Type: multipart/form-data
Authorization: Bearer <api-token>

metadata: { "name": "my-lib", "version": "1.0.0", ... }
source:   <소스 아카이브>
```

`dotori publish` 명령을 사용하면 이 과정이 자동화됩니다.

### 버전 비활성화 (Yank)

```
POST /packages/{name}/{version}/yank
DELETE /packages/{name}/{version}/yank   (unyank)
Authorization: Bearer <api-token>
```

### 소유자 관리

```
GET    /packages/{name}/owners
POST   /packages/{name}/owners         { "username": "alice" }
DELETE /packages/{name}/owners/{user}
Authorization: Bearer <api-token>
```

---

## CLI에서 레지스트리 사용

### 레지스트리 로그인

```bash
# 기본 공개 레지스트리
dotori login

# 사내 레지스트리
dotori login --registry https://registry.internal.example.com
```

로그인 정보는 `~/.dotori/config.toml`에 저장됩니다.

### 패키지 검색

```bash
dotori search fmt
dotori search "json parser"
```

### 패키지 배포

`.dotori` 파일에 `package { }` 블록이 있어야 합니다.

```bash
# 현재 디렉토리 패키지 배포
dotori publish

# 특정 레지스트리에 배포
dotori publish --registry https://registry.internal.example.com
```

### 패키지 의존성 추가

```bash
# 레지스트리에서 최신 버전 추가
dotori add fmt

# 특정 버전 추가
dotori add fmt@10.2.0
```

`.dotori`에 다음이 추가됩니다:

```
dependencies {
    fmt = "10.2.0"
}
```

---

## 사내 레지스트리 운영

### 기본 설정 파일 예시

```yaml
# docker-compose.prod.yml
version: "3.9"
services:
  registry:
    image: dotori-registry:latest
    ports:
      - "8080:8080"
    environment:
      - Registry__Mode=standalone
      - Registry__StorageRoot=/data/packages
      - Database__Provider=postgres
      - ConnectionStrings__Default=Host=db;Database=dotori;Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      - OAuth__Jwt__Secret=${DOTORI_JWT_SECRET}
      - OAuth__Providers__github__Enabled=true
      - OAuth__Providers__github__ClientId=${GITHUB_CLIENT_ID}
      - OAuth__Providers__github__ClientSecret=${GITHUB_CLIENT_SECRET}
    volumes:
      - registry-data:/data
    depends_on:
      - db
    restart: unless-stopped

  db:
    image: postgres:17-alpine
    environment:
      - POSTGRES_DB=dotori
      - POSTGRES_USER=${POSTGRES_USER}
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
    volumes:
      - db-data:/var/lib/postgresql/data
    restart: unless-stopped

volumes:
  registry-data:
  db-data:
```

### .env 파일

```env
POSTGRES_USER=dotori
POSTGRES_PASSWORD=secure-password-here
DOTORI_JWT_SECRET=at-least-32-character-secret-key-here
GITHUB_CLIENT_ID=your-github-oauth-app-client-id
GITHUB_CLIENT_SECRET=your-github-oauth-app-client-secret
```

### 팀 설정 — 사내 레지스트리 사용

팀원들의 `~/.dotori/config.toml`에 사내 레지스트리를 등록합니다.

```bash
dotori login --registry https://registry.internal.example.com
```

또는 `~/.dotori/config.toml`을 직접 편집:

```toml
[registries.internal]
url = "https://registry.internal.example.com"
token = "your-api-token"
```

---

## 보안 고려사항

- `OAuth__Jwt__Secret`은 최소 32자 이상의 무작위 문자열을 사용하세요.
  ```bash
  openssl rand -base64 32
  ```
- 운영 환경에서는 HTTPS를 사용하세요. nginx 등의 리버스 프록시로 TLS를 종료하는 것을 권장합니다.
- API 토큰은 패키지 배포·소유자 관리에 필요합니다. 토큰 유출 시 즉시 재발급하세요.
- 취약 버전은 `dotori yank`로 즉시 비활성화할 수 있습니다.
