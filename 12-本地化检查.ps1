# ============================================================
#  Familia Myth 本地化完整性检查
#  用法： pwsh -File .\12-本地化检查.ps1
#  说明： 只读扫描，不修改任何文件。
#         用于在迁移/翻译前后确认玩家可见文本与语言文件的对应关系。
# ============================================================

$ErrorActionPreference = 'Stop'
$root = if ($PSScriptRoot) { $PSScriptRoot } else { (Get-Location).Path }
$langRoot = Join-Path $root 'Familia_Myth\Languages'
$defsRoot = Join-Path $root 'Familia_Myth\Defs'
$srcRoot  = Join-Path $root 'Familia_Myth\Source'

# Def 一层的玩家可见文本字段（会经 DefInjected 的 <DefName.字段名> 覆盖）
$textFields = @(
    'label', 'description', 'letterText', 'letterLabel', 'beginLetter', 'beginLetterLabel',
    'recoveryMessage', 'baseInspectLine', 'text', 'jobString', 'reportString', 'verb',
    'title', 'labelPlural', 'customLabel', 'deathMessage', 'fixedName', 'inspectString',
    'letterTextNews', 'letterLabelNews'
)

function Get-LangKeys {
    param([string]$Lang)
    $keys = @{}
    $dir = Join-Path $langRoot "$Lang\DefInjected"
    if (-not (Test-Path $dir)) { return $keys }
    foreach ($f in (Get-ChildItem $dir -Recurse -Filter *.xml)) {
        try { $x = [xml](Get-Content $f.FullName -Raw) } catch { continue }
        foreach ($n in $x.DocumentElement.ChildNodes) {
            if ($n.NodeType -eq 'Element') { $keys[$n.Name] = $true }
        }
    }
    return $keys
}

function Get-KeyedMap {
    param([string]$Lang)
    $map = @{}
    $dir = Join-Path $langRoot "$Lang\Keyed"
    if (-not (Test-Path $dir)) { return $map }
    foreach ($f in (Get-ChildItem $dir -Filter *.xml)) {
        try { $x = [xml](Get-Content $f.FullName -Raw) } catch { continue }
        foreach ($n in $x.DocumentElement.ChildNodes) {
            if ($n.NodeType -eq 'Element') {
                if ($map.ContainsKey($n.Name)) {
                    Write-Host ("    [警告] Keyed 重复键: {0}  ({1})" -f $n.Name, $f.Name) -ForegroundColor Red
                } else {
                    $map[$n.Name] = $n.InnerText
                }
            }
        }
    }
    return $map
}

Write-Host ''
Write-Host '==================== Familia Myth 本地化检查 ====================' -ForegroundColor Cyan

# ---------- 1. Def 顶层文本字段的语言覆盖 ----------
Write-Host ''
Write-Host '[1] Def 文本字段 -> DefInjected 覆盖' -ForegroundColor Yellow
$langs = @('ChineseSimplified', 'English')
$langKeys = @{}
$defTextWanted = @()
foreach ($f in (Get-ChildItem $defsRoot -Recurse -Filter *.xml)) {
    try { $x = [xml](Get-Content $f.FullName -Raw) } catch { continue }
    foreach ($def in $x.DocumentElement.ChildNodes) {
        if ($def.NodeType -ne 'Element') { continue }
        $dn = $def.defName
        if (-not $dn) { continue }
        foreach ($child in $def.ChildNodes) {
            if ($child.NodeType -ne 'Element') { continue }
            if ($textFields -notcontains $child.Name) { continue }
            $defTextWanted += [pscustomobject]@{ Key = "$dn.$($child.Name)"; Def = $dn; Field = $child.Name }
        }
    }
}
"    待覆盖键位总计: $($defTextWanted.Count)"
foreach ($lang in $langs) {
    $keys = Get-LangKeys -Lang $lang
    $langKeys[$lang] = $keys
    $missing = @($defTextWanted | Where-Object { -not $keys.ContainsKey($_.Key) })
    $pct = if ($defTextWanted.Count -gt 0) { [math]::Round(100.0 * ($defTextWanted.Count - $missing.Count) / $defTextWanted.Count, 1) } else { 100 }
    Write-Host ("    {0,-18} 已覆盖 {1}/{2} ({3}%)" -f $lang, ($defTextWanted.Count - $missing.Count), $defTextWanted.Count, $pct)
    if ($missing.Count -gt 0 -and $missing.Count -le 25) {
        $missing | ForEach-Object { Write-Host "        - $($_.Key)" -ForegroundColor DarkGray }
    } elseif ($missing.Count -gt 25) {
        Write-Host "        （缺口 $($missing.Count) 项，明细已省略）" -ForegroundColor DarkGray
    }
}

# ---------- 2. TKey -> slateRef 键 ----------
Write-Host ''
Write-Host '[2] 任务脚本 TKey -> <DefName>.<TKey>.slateRef' -ForegroundColor Yellow
$tkeyWanted = @()
foreach ($f in (Get-ChildItem (Join-Path $defsRoot 'QuestScriptDefs') -Filter *.xml -ErrorAction SilentlyContinue) +
                (Get-ChildItem (Join-Path $defsRoot 'GuildQuestDefs') -Filter *.xml -ErrorAction SilentlyContinue)) {
    $lines = Get-Content $f.FullName
    $dn = ''
    foreach ($l in $lines) {
        if ($l -match '<defName>([\w_]+)</defName>') { $dn = $Matches[1] }
        foreach ($m in [regex]::Matches($l, 'TKey="([\w_]+)"')) {
            if ($dn) { $tkeyWanted += [pscustomobject]@{ Key = "$dn.$($m.Groups[1].Value).slateRef"; Def = $dn; File = $f.Name } }
        }
    }
}
"    Def 中 TKey 总数: $($tkeyWanted.Count)"
foreach ($lang in $langs) {
    $keys = $langKeys[$lang]
    if (-not $keys) { $keys = Get-LangKeys -Lang $lang; $langKeys[$lang] = $keys }
    $missing = @($tkeyWanted | Where-Object { -not $keys.ContainsKey($_.Key) })
    Write-Host ("    {0,-18} 已覆盖 {1}/{2}" -f $lang, ($tkeyWanted.Count - $missing.Count), $tkeyWanted.Count)
    $missing | Select-Object -First 12 | ForEach-Object { Write-Host "        - $($_.Key)  [$($_.File)]" -ForegroundColor DarkGray }
}

# ---------- 3. 规则列表（rulesStrings）覆盖 ----------
Write-Host ''
Write-Host '[3] rulesStrings 规则列表覆盖' -ForegroundColor Yellow
$rulesWanted = @()
foreach ($f in (Get-ChildItem (Join-Path $defsRoot 'QuestScriptDefs') -Filter *.xml -ErrorAction SilentlyContinue)) {
    $raw = Get-Content $f.FullName -Raw
    $dn = ([regex]::Match($raw, '<defName>([\w_]+)</defName>')).Groups[1].Value
    foreach ($t in @('questNameRules', 'questDescriptionRules')) {
        if ($raw -match "<$t>") { $rulesWanted += [pscustomobject]@{ Key = "$dn.$t.rulesStrings"; Def = $dn; Field = $t } }
    }
}
"    待覆盖规则列表: $($rulesWanted.Count)"
foreach ($lang in $langs) {
    $keys = $langKeys[$lang]
    if (-not $keys) { $keys = Get-LangKeys -Lang $lang; $langKeys[$lang] = $keys }
    $missing = @($rulesWanted | Where-Object { -not $keys.ContainsKey($_.Key) })
    Write-Host ("    {0,-18} 已覆盖 {1}/{2}" -f $lang, ($rulesWanted.Count - $missing.Count), $rulesWanted.Count)
    $missing | Select-Object -First 12 | ForEach-Object { Write-Host "        - $($_.Key)" -ForegroundColor DarkGray }
}

# ---------- 4. 遗留的「Def 字段填 Keyed key」写法 ----------
Write-Host ''
Write-Host '[4] 遗留 key 占位检测（Def 字段填写了 Keyed key —— 该写法不会触发翻译）' -ForegroundColor Yellow
$badPlaceholders = @()
foreach ($f in (Get-ChildItem $defsRoot -Recurse -Filter *.xml)) {
    $lines = Get-Content $f.FullName
    $dn = ''
    foreach ($l in $lines) {
        if ($l -match '<defName>([\w_]+)</defName>') { $dn = $Matches[1] }
        foreach ($m in [regex]::Matches($l, "<($($textFields -join '|'))>((?:FamiliaMyth|FM_)[\w\.]*)</\1>")) {
            $badPlaceholders += [pscustomobject]@{ Def = $dn; Field = $m.Groups[1].Value; Value = $m.Groups[2].Value; File = $f.Name }
        }
    }
}
if ($badPlaceholders.Count -eq 0) {
    Write-Host '    未发现（OK）' -ForegroundColor Green
} else {
    Write-Host "    发现 $($badPlaceholders.Count) 处：" -ForegroundColor Red
    $badPlaceholders | Select-Object -First 15 | ForEach-Object { Write-Host "        [$($_.Def)] <$($_.Field)> = $($_.Value)  ($($_.File))" -ForegroundColor DarkGray }
}

# ---------- 5. Keyed 中英差集 ----------
Write-Host ''
Write-Host '[5] Keyed 键位中英对照' -ForegroundColor Yellow
$cnKeyed = Get-KeyedMap -Lang 'ChineseSimplified'
$enKeyed = Get-KeyedMap -Lang 'English'
"    中文 Keyed: $($cnKeyed.Count) / 英文 Keyed: $($enKeyed.Count)"
$onlyEn = @($enKeyed.Keys | Where-Object { -not $cnKeyed.ContainsKey($_) })
$onlyCn = @($cnKeyed.Keys | Where-Object { -not $enKeyed.ContainsKey($_) })
Write-Host "    仅英文有（中文环境会显示原始 key）: $($onlyEn.Count)" -ForegroundColor $(if ($onlyEn.Count) { 'Red' } else { 'Green' })
$onlyEn | Select-Object -First 10 | ForEach-Object { Write-Host "        - $_" -ForegroundColor DarkGray }
Write-Host "    仅中文有: $($onlyCn.Count)"
$onlyCn | Select-Object -First 10 | ForEach-Object { Write-Host "        - $_" -ForegroundColor DarkGray }

# ---------- 6. 代码内 Translate 键覆盖 ----------
Write-Host ''
Write-Host '[6] Source 内 .Translate() 键 -> Keyed 覆盖' -ForegroundColor Yellow
$usedKeys = @()
if (Test-Path $srcRoot) {
    # 两种引用形式都要抓：① "Key".Translate(...)   ② TranslateHelper.Translate("Key") / Translate("Key")
    $keyPatterns = @(
        '"([\w\.]+)"\s*\.Translate',
        'Translate\w*\s*\(\s*"([\w\.]+)"'
    )
    foreach ($p in $keyPatterns) {
        $hits = Select-String -Path (Join-Path $srcRoot '*.cs') -Pattern $p -AllMatches -ErrorAction SilentlyContinue
        foreach ($h in $hits) { foreach ($m in $h.Matches) { $usedKeys += $m.Groups[1].Value } }
    }
}
$usedKeys = @($usedKeys | Sort-Object -Unique)
"    代码引用 key 数: $($usedKeys.Count)"
foreach ($lang in $langs) {
    $map = if ($lang -eq 'ChineseSimplified') { $cnKeyed } else { $enKeyed }
    $missing = @($usedKeys | Where-Object { -not $map.ContainsKey($_) })
    Write-Host ("    {0,-18} 缺失 {1} 个" -f $lang, $missing.Count) -ForegroundColor $(if ($missing.Count) { 'Red' } else { 'Green' })
    $missing | ForEach-Object { Write-Host "        - $_" -ForegroundColor DarkGray }
}

# ---------- 7. XML 中的类型引用能否解析到源码类型 ----------
# 说明：Defs/Patches 里的 <xxxClass> / Class="..." / Def 元素标签都是 C# 类型引用，
#       一旦类名被重命名而 XML 未同步（或反之），游戏加载期会报
#       "Could not find a type named ..." 红字。本项用于自动兜住这类笔误。
Write-Host ''
Write-Host '[7] XML 类型引用 -> 源码类型可解析性' -ForegroundColor Yellow

$srcTypes = @{}
if (Test-Path $srcRoot) {
    foreach ($f in (Get-ChildItem $srcRoot -Filter *.cs -Recurse)) {
        $ns = ''
        foreach ($line in (Get-Content $f.FullName)) {
            if ($line -match '^namespace\s+([\w\.]+)') { $ns = $Matches[1] }
            foreach ($m in [regex]::Matches($line, '(?:public|internal)\s+(?:static\s+|sealed\s+|abstract\s+|partial\s+)*(?:class|struct|enum|interface)\s+(\w+)')) {
                $srcTypes["$ns.$($m.Groups[1].Value)"] = $f.Name
            }
        }
    }
}
Write-Host "    源码类型总数: $($srcTypes.Count)"

$modRoot = Split-Path $defsRoot -Parent
$refFiles = @()
foreach ($dir in @('Defs', 'Patches')) {
    $p = Join-Path $modRoot $dir
    if (Test-Path $p) { $refFiles += Get-ChildItem $p -Recurse -Filter *.xml }
}

$refs = @{}
foreach ($f in $refFiles) {
    $raw = [regex]::Replace((Get-Content $f.FullName -Raw), '<!--.*?-->', '',
        [System.Text.RegularExpressions.RegexOptions]::Singleline)
    foreach ($m in [regex]::Matches($raw, '(FamiliaMyth(?:\.\w+)+)')) {
        $t = $m.Groups[1].Value
        if (-not $refs.ContainsKey($t)) { $refs[$t] = @() }
        $refs[$t] += $f.Name
    }
}
Write-Host "    XML 引用总数: $($refs.Count)"

$badRefs = @()
foreach ($t in $refs.Keys) {
    if ($srcTypes.ContainsKey($t)) { continue }
    # 方法引用（validator 等）：形如 Type.Method —— 去掉最后一段后若能命中类型即视为合法
    $idx = $t.LastIndexOf('.')
    if ($idx -gt 0 -and $srcTypes.ContainsKey($t.Substring(0, $idx))) { continue }
    $badRefs += $t
}
if ($badRefs.Count -eq 0) {
    Write-Host '    ✓ 全部类型引用均可解析' -ForegroundColor Green
} else {
    Write-Host "    ✗ 发现 $($badRefs.Count) 处无法解析的引用（游戏加载会报红字）：" -ForegroundColor Red
    foreach ($b in ($badRefs | Sort-Object)) {
        Write-Host "        - $b   <- $(($refs[$b] | Select-Object -Unique) -join ', ')" -ForegroundColor DarkGray
    }
}

Write-Host ''
Write-Host '==================== 检查结束 ====================' -ForegroundColor Cyan
