# 刷新 lib\<版本> 下的引用副本，供 Familia_Myth.csproj 编译对应版本使用。
# 用法（需先把 Steam 切到目标版本，脚本会校验游戏 Version.txt）：
#   pwsh -File .\13-同步引用库.ps1 -Version 1.7
#   pwsh -File .\13-同步引用库.ps1 -Version 1.6
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('1.6', '1.7')]
    [string]$Version,

    [string]$GameDir = 'D:\steam\steamapps\common\RimWorld',

    [string]$WorkshopDir = 'D:\steam\steamapps\workshop\content\294100'
)

$ErrorActionPreference = 'Stop'

$repoRoot = $PSScriptRoot
$libDir = Join-Path $repoRoot "lib\$Version"

$harmonyDir = Join-Path $WorkshopDir "2009463077\$Version\Assemblies"

# 1.6 的 mscorlib 取自游戏的 Mono 运行时：Unity 的 Mono 支持 C# 8 默认接口实现，
# 编译器据此放行；换成 .NET Framework targeting pack 的同名程序集会报 CS8701。
$sourceMap = if ($Version -eq '1.6') {
    $managed = Join-Path $GameDir 'RimWorldWin64_Data\Managed'
    [ordered]@{
        "$managed\Assembly-CSharp.dll"                     = 'Assembly-CSharp.dll'
        "$managed\mscorlib.dll"                            = 'mscorlib.dll'
        "$managed\netstandard.dll"                         = 'netstandard.dll'
        "$managed\UnityEngine.dll"                         = 'UnityEngine.dll'
        "$managed\UnityEngine.CoreModule.dll"              = 'UnityEngine.CoreModule.dll'
        "$managed\UnityEngine.IMGUIModule.dll"             = 'UnityEngine.IMGUIModule.dll'
        "$managed\UnityEngine.InputLegacyModule.dll"       = 'UnityEngine.InputLegacyModule.dll'
        "$managed\UnityEngine.TextRenderingModule.dll"     = 'UnityEngine.TextRenderingModule.dll'
        "$managed\UnityEngine.UIElementsModule.dll"        = 'UnityEngine.UIElementsModule.dll'
        "$managed\UnityEngine.ImageConversionModule.dll"   = 'UnityEngine.ImageConversionModule.dll'
        "$harmonyDir\0Harmony.dll"                         = '0Harmony.dll'
    }
}
else {
    # 1.7 的 Unity 类型由 UnityStubs 提供，实际类型定义转发到 Waylith 运行时程序集。
    [ordered]@{
        "$GameDir\Assembly-CSharp.dll"                     = 'Assembly-CSharp.dll'
        "$GameDir\UnityStubs.dll"                          = 'UnityStubs.dll'
        "$GameDir\Waylith.dll"                             = 'Waylith.dll'
        "$GameDir\Waylith.UnityCompat.dll"                 = 'Waylith.UnityCompat.dll'
        "$harmonyDir\0Harmony.dll"                         = '0Harmony.dll'
    }
}

$versionFile = Join-Path $GameDir 'Version.txt'
if (-not (Test-Path $versionFile)) {
    Write-Error "找不到游戏版本文件：$versionFile"
    exit 1
}

$gameVersion = (Get-Content $versionFile -Raw).Trim()
if (-not $gameVersion.StartsWith($Version)) {
    Write-Error "游戏当前版本为 $gameVersion，与请求的 $Version 不匹配。请先在 Steam 切换游戏版本。"
    exit 1
}

New-Item -ItemType Directory -Path $libDir -Force | Out-Null

$missing = @()
foreach ($source in $sourceMap.Keys) {
    if (Test-Path $source) {
        Copy-Item $source (Join-Path $libDir $sourceMap[$source]) -Force
    }
    else {
        $missing += $source
    }
}

$lines = @("# 游戏版本 $gameVersion", "# 生成时间 $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')", '')
$lines += Get-ChildItem $libDir -Filter *.dll | Sort-Object Name | ForEach-Object {
    "{0,-36} {1,10}  {2}" -f $_.Name, $_.Length, (Get-FileHash $_.FullName -Algorithm SHA256).Hash.Substring(0, 16)
}
$lines | Set-Content (Join-Path $libDir '_refs.txt') -Encoding UTF8

$lines | ForEach-Object { Write-Host $_ }

if ($missing.Count -gt 0) {
    Write-Warning "以下来源缺失，未拷贝：`n$($missing -join "`n")"
    exit 1
}

Write-Host "`nlib\$Version 已同步（游戏版本 $gameVersion）。"
