# 用 .NET Framework 自带的 csc.exe 编译，无需安装 VS
$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) {
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not (Test-Path $csc)) {
    throw '找不到 csc.exe，请确认已安装 .NET Framework 4.x'
}

$dist = Join-Path $root 'dist'
New-Item -ItemType Directory -Path $dist -Force | Out-Null

& $csc /nologo /target:winexe /codepage:65001 `
    /win32manifest:"$root\src\app.manifest" `
    /out:"$dist\MetasequoiaTuner.exe" `
    /r:System.Windows.Forms.dll /r:System.Drawing.dll `
    "$root\src\Program.cs"

if ($LASTEXITCODE -ne 0) { throw "编译失败，退出码 $LASTEXITCODE" }
Write-Host "已生成：$dist\MetasequoiaTuner.exe"
