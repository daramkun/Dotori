#Requires -Version 5.1
<#
.SYNOPSIS
    dotori CLI를 시스템에 설치합니다.

.DESCRIPTION
    build/ 폴더에 빌드된 dotori CLI 바이너리를 설치하고 PATH에 등록합니다.
    관리자 권한이 있으면 C:\Program Files\Dotori 설치를 제안하고,
    그렇지 않으면 %LOCALAPPDATA%\Dotori 에 사용자 수준으로 설치합니다.

.PARAMETER BuildFirst
    설치 전 CLI를 먼저 빌드합니다 (dotnet SDK 필요).

.PARAMETER Rid
    BuildFirst 사용 시 지정할 .NET Runtime Identifier.
    예: win-x64, win-arm64. 생략 시 현재 아키텍처 자동 선택.

.PARAMETER Version
    BuildFirst 사용 시 주입할 어셈블리 버전. 예: v1.2.3

.PARAMETER Uninstall
    설치된 dotori를 제거합니다.

.EXAMPLE
    .\install.ps1
    빌드된 바이너리를 설치합니다.

.EXAMPLE
    .\install.ps1 -BuildFirst
    CLI를 빌드한 후 설치합니다.

.EXAMPLE
    .\install.ps1 -BuildFirst -Rid win-x64 -Version v1.0.0
    win-x64용으로 빌드 후 설치합니다.

.EXAMPLE
    .\install.ps1 -Uninstall
    dotori를 시스템에서 제거합니다.
#>
[CmdletBinding()]
param(
    [switch]$BuildFirst,
    [string]$Rid      = "",
    [string]$Version  = "",
    [switch]$Uninstall
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ──────────────────────────────────────────────────────────────────────────────
# 유틸리티
# ──────────────────────────────────────────────────────────────────────────────
function Write-Info    ($msg) { Write-Host "[install] $msg" -ForegroundColor Cyan    }
function Write-Ok      ($msg) { Write-Host "[ok]      $msg" -ForegroundColor Green   }
function Write-Warn    ($msg) { Write-Host "[warn]    $msg" -ForegroundColor Yellow  }
function Write-Err     ($msg) { Write-Host "[error]   $msg" -ForegroundColor Red     }

function Test-Admin {
    $identity  = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]$identity
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# PATH에 디렉토리가 등록되어 있는지 확인 (대소문자 무시)
function Test-InPath ([string]$dir) {
    $scope = if (Test-Admin) { "Machine" } else { "User" }
    $current = [Environment]::GetEnvironmentVariable("PATH", $scope) ?? ""
    foreach ($p in $current -split ";") {
        if ($p.TrimEnd("\") -ieq $dir.TrimEnd("\")) { return $true }
    }
    return $false
}

# PATH에 디렉토리 추가
function Add-ToPath ([string]$dir, [string]$scope) {
    $current = [Environment]::GetEnvironmentVariable("PATH", $scope) ?? ""
    $entries = ($current -split ";" | Where-Object { $_ -ne "" })
    if ($entries -inotcontains $dir) {
        $newPath = ($entries + $dir) -join ";"
        [Environment]::SetEnvironmentVariable("PATH", $newPath, $scope)
        # 현재 프로세스 PATH에도 즉시 반영
        $env:PATH = "$env:PATH;$dir"
        Write-Ok "PATH 에 등록했습니다: $dir ($scope)"
    }
}

# PATH에서 디렉토리 제거
function Remove-FromPath ([string]$dir, [string]$scope) {
    $current = [Environment]::GetEnvironmentVariable("PATH", $scope) ?? ""
    $entries = $current -split ";" | Where-Object { $_.TrimEnd("\") -ine $dir.TrimEnd("\") -and $_ -ne "" }
    [Environment]::SetEnvironmentVariable("PATH", ($entries -join ";"), $scope)
    Write-Ok "PATH 에서 제거했습니다: $dir ($scope)"
}

# ──────────────────────────────────────────────────────────────────────────────
# 경로 상수
# ──────────────────────────────────────────────────────────────────────────────
$ScriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$BuildDir   = Join-Path $ScriptDir "build\cli"
$Binary     = Join-Path $BuildDir "dotori.exe"
$ProgramDir = "C:\Program Files\Dotori"
$UserDir    = Join-Path $env:LOCALAPPDATA "Dotori"
$IsAdmin    = Test-Admin

# ──────────────────────────────────────────────────────────────────────────────
# 제거 모드
# ──────────────────────────────────────────────────────────────────────────────
if ($Uninstall) {
    Write-Host ""
    Write-Info "dotori 제거를 시작합니다..."

    $removed = $false

    # Program Files 설치 확인 (관리자 전용)
    $pfExe = Join-Path $ProgramDir "dotori.exe"
    if (Test-Path $pfExe) {
        if (-not $IsAdmin) {
            Write-Err "C:\Program Files\Dotori 제거에는 관리자 권한이 필요합니다."
            Write-Err "PowerShell을 관리자 권한으로 실행 후 다시 시도하세요."
            exit 1
        }
        Remove-Item $pfExe -Force
        Remove-FromPath $ProgramDir "Machine"
        # 빈 디렉토리 정리
        if ((Get-ChildItem $ProgramDir -ErrorAction SilentlyContinue).Count -eq 0) {
            Remove-Item $ProgramDir -Force -Recurse
        }
        Write-Ok "제거 완료: $pfExe"
        $removed = $true
    }

    # 사용자 설치 확인
    $userExe = Join-Path $UserDir "dotori.exe"
    if (Test-Path $userExe) {
        Remove-Item $userExe -Force
        Remove-FromPath $UserDir "User"
        if ((Get-ChildItem $UserDir -ErrorAction SilentlyContinue).Count -eq 0) {
            Remove-Item $UserDir -Force -Recurse
        }
        Write-Ok "제거 완료: $userExe"
        $removed = $true
    }

    if (-not $removed) {
        Write-Warn "설치된 dotori를 찾을 수 없습니다."
    }
    exit 0
}

# ──────────────────────────────────────────────────────────────────────────────
# 빌드 (--BuildFirst)
# ──────────────────────────────────────────────────────────────────────────────
if ($BuildFirst) {
    Write-Host ""
    Write-Info "CLI 빌드를 시작합니다..."

    $buildArgs = @("--only", "cli")
    if ($Rid)     { $buildArgs += "--rid";     $buildArgs += $Rid     }
    if ($Version) { $buildArgs += "--version"; $buildArgs += $Version }

    $buildScript = Join-Path $ScriptDir "build.sh"
    if (Test-Path $buildScript) {
        # WSL 또는 Git Bash 환경
        & bash $buildScript @buildArgs
    } else {
        # dotnet 직접 호출
        $proj    = Join-Path $ScriptDir "src\Dotori.Cli\Dotori.Cli.csproj"
        $outDir  = $BuildDir
        $dotArgs = @("publish", $proj, "-c", "Release", "-o", $outDir, "--nologo", "-v", "quiet")
        if ($Rid)     { $dotArgs += @("-r", $Rid, "--self-contained") }
        if ($Version) { $dotArgs += "-p:Version=$($Version.TrimStart('v'))" }
        & dotnet @dotArgs
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Err "CLI 빌드에 실패했습니다."
        exit 1
    }
    Write-Ok "CLI 빌드 완료"
}

# ──────────────────────────────────────────────────────────────────────────────
# 바이너리 존재 확인
# ──────────────────────────────────────────────────────────────────────────────
if (-not (Test-Path $Binary)) {
    Write-Err "CLI 바이너리를 찾을 수 없습니다: $Binary"
    Write-Err "먼저 build.sh 또는 .\install.ps1 -BuildFirst 로 빌드하세요."
    exit 1
}

# ──────────────────────────────────────────────────────────────────────────────
# 설치 경로 결정
# ──────────────────────────────────────────────────────────────────────────────
Write-Host ""

$installDir = $null
$pathScope  = "User"

if ($IsAdmin) {
    # 관리자: Program Files 설치 여부 확인
    $answer = Read-Host "[install] 관리자 권한이 있습니다. '$ProgramDir' 에 설치하시겠습니까? [Y/n]"
    if ($answer -eq "" -or $answer -imatch "^y") {
        $installDir = $ProgramDir
        $pathScope  = "Machine"
    } else {
        $installDir = $UserDir
        $pathScope  = "User"
    }
} else {
    # 일반 사용자: LOCALAPPDATA 에 자동 설치
    Write-Info "관리자 권한이 없습니다. '$UserDir' 에 설치합니다."
    $installDir = $UserDir
    $pathScope  = "User"
}

# ──────────────────────────────────────────────────────────────────────────────
# 설치 실행
# ──────────────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Info "설치 경로: $installDir"

if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

Copy-Item $Binary (Join-Path $installDir "dotori.exe") -Force
Write-Ok "복사 완료: $(Join-Path $installDir 'dotori.exe')"

# PATH 등록
if (-not (Test-InPath $installDir)) {
    Add-ToPath $installDir $pathScope
} else {
    Write-Info "PATH 에 이미 등록되어 있습니다: $installDir"
}

# ──────────────────────────────────────────────────────────────────────────────
# 완료
# ──────────────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Ok "dotori 설치가 완료되었습니다."
Write-Info "새 터미널을 열거나 다음 명령으로 PATH를 즉시 적용하세요:"
Write-Host "    `$env:PATH = [Environment]::GetEnvironmentVariable('PATH','$pathScope') + ';' + `$env:PATH" -ForegroundColor Gray
Write-Host ""
