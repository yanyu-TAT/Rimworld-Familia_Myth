# 构建 Release 并把可上传的模组目录输出到 <仓库根>\package\Familia_Myth。
# 用法：
#   pwsh -File .\14-打包.ps1
#   pwsh -File .\14-打包.ps1 -SkipBuild    # 用现有 DLL，只打包
#   pwsh -File .\14-打包.ps1 -NoSource     # 不打包 Source/（仅游戏运行所需）
[CmdletBinding()]
param(
    [string]$OutputDir = 'package',
    [switch]$SkipBuild,
    [switch]$NoSource
)

$ErrorActionPreference = 'Stop'

$repoRoot = $PSScriptRoot
$modRoot  = Join-Path $repoRoot 'Familia_Myth'
$csproj   = Join-Path $modRoot 'Familia_Myth.csproj'
$destRoot = Join-Path $repoRoot $OutputDir
$destMod  = Join-Path $destRoot 'Familia_Myth'

# 1. 构建（必须 Release：Debug 产物无 JIT 优化，tick 密集场景会掉帧）
if ($SkipBuild) {
    Write-Host '== 跳过构建 ==' -ForegroundColor Yellow
}
else {
    Write-Host '== 构建 Release ==' -ForegroundColor Cyan
    & dotnet build $csproj -c Release
    if ($LASTEXITCODE -ne 0) { throw "dotnet build 失败（exit $LASTEXITCODE）" }
}

# 2. 确认 1.6 / 1.7 的输出 DLL 就是 Release 产物
Write-Host '== 校验 DLL 构建配置 ==' -ForegroundColor Cyan
$targets = @(
    @{ Tfm = 'net472';  Ver = '1.6' },
    @{ Tfm = 'net10.0'; Ver = '1.7' }
)
foreach ($t in $targets) {
    $release = Join-Path $modRoot "obj\Release\$($t.Tfm)\Familia_Myth.dll"
    $output  = Join-Path $modRoot "$($t.Ver)\Assemblies\Familia_Myth.dll"
    if (-not (Test-Path -LiteralPath $release)) { throw "找不到 Release 产物：$release" }
    if (-not (Test-Path -LiteralPath $output))  { throw "找不到输出 DLL：$output" }
    $hRelease = (Get-FileHash -LiteralPath $release -Algorithm SHA256).Hash
    $hOutput  = (Get-FileHash -LiteralPath $output  -Algorithm SHA256).Hash
    if ($hRelease -ne $hOutput) {
        throw "$($t.Ver) 的 Assemblies DLL 与 obj\Release 产物不一致——可能仍是 Debug 构建，请检查构建命令"
    }
    Write-Host ("  {0}  {1}" -f $t.Ver, $hOutput.Substring(0, 16)) -ForegroundColor DarkGray
}

# 3. 已上传过的条目会在 About 下留 PublishedFileId.txt，重建目录前先记住它
$publishedPath = Join-Path $destMod 'About\PublishedFileId.txt'
$publishedId = $null
if (Test-Path -LiteralPath $publishedPath) {
    $publishedId = (Get-Content -LiteralPath $publishedPath -Raw).Trim()
}

# 4. 清空并重建输出目录
if (Test-Path -LiteralPath $destMod) { Remove-Item -LiteralPath $destMod -Recurse -Force }
New-Item -ItemType Directory -Path $destMod -Force | Out-Null

$includeDirs = @('About', '1.6', '1.7', 'AssetBundles', 'Defs', 'Languages', 'Materials', 'Patches', 'Sounds', 'Textures')
if (-not $NoSource) { $includeDirs += 'Source' }

foreach ($dir in $includeDirs) {
    $src = Join-Path $modRoot $dir
    if (-not (Test-Path -LiteralPath $src)) { throw "缺少目录：$src" }
    Copy-Item -LiteralPath $src -Destination $destMod -Recurse -Force
}
Copy-Item -LiteralPath (Join-Path $modRoot 'LoadFolders.xml') -Destination $destMod -Force

# 5. 剔除不该上传的产物：美术源文件、调试符号
Get-ChildItem -Recurse -File -LiteralPath $destMod -Filter *.aseprite | Remove-Item -Force
Get-ChildItem -Recurse -File -LiteralPath $destMod -Filter *.pdb | Remove-Item -Force

# 6. 还原 PublishedFileId
if ($publishedId) {
    $aboutDir = Join-Path $destMod 'About'
    New-Item -ItemType Directory -Path $aboutDir -Force | Out-Null
    [System.IO.File]::WriteAllText($publishedPath, $publishedId)
    Write-Host "  已保留 PublishedFileId: $publishedId" -ForegroundColor DarkGray
}

# 7. 完整性校验
$required = @(
    'LoadFolders.xml',
    'About\About.xml',
    'About\Preview.png',
    '1.6\Assemblies\Familia_Myth.dll',
    '1.7\Assemblies\Familia_Myth.dll'
)
$missing = $required | Where-Object { -not (Test-Path -LiteralPath (Join-Path $destMod $_)) }
if ($missing) { throw "缺少必需文件：`n$($missing -join "`n")" }

foreach ($dir in @('About', 'Defs', 'Languages', 'Patches', 'Sounds', 'Textures', 'AssetBundles')) {
    $path = Join-Path $destMod $dir
    if (-not (Test-Path -LiteralPath $path)) { throw "缺少目录：$dir" }
    if (-not (Get-ChildItem -LiteralPath $path -File -Recurse)) { throw "目录为空：$dir" }
}

$badXml = @()
Get-ChildItem -Recurse -File -LiteralPath (Join-Path $destMod 'Defs'), (Join-Path $destMod 'Patches') -Filter *.xml | ForEach-Object {
    try { [xml](Get-Content -Raw -LiteralPath $_.FullName) | Out-Null }
    catch { $badXml += $_.FullName }
}
if ($badXml) { throw "XML 解析失败：`n$($badXml -join "`n")" }

# 8. 汇总
$about  = [xml](Get-Content -Raw -LiteralPath (Join-Path $destMod 'About\About.xml'))
$files  = Get-ChildItem -Recurse -File -LiteralPath $destMod
$sizeMb = [math]::Round((($files | Measure-Object Length -Sum).Sum / 1MB), 1)

Write-Host ''
Write-Host '== 打包完成 ==' -ForegroundColor Green
Write-Host ("  输出目录 : {0}" -f $destMod)
Write-Host ("  模组名   : {0}" -f $about.ModMetaData.name)
Write-Host ("  packageId: {0}" -f $about.ModMetaData.packageId)
Write-Host ("  版本     : {0}" -f $about.ModMetaData.modVersion)
Write-Host ("  文件数   : {0}" -f $files.Count)
Write-Host ("  总大小   : {0} MB" -f $sizeMb)
if ($NoSource) { Write-Host '  已排除 Source/' -ForegroundColor DarkGray }
else { Write-Host '  含 Source/（如需精简，加 -NoSource）' -ForegroundColor DarkGray }
