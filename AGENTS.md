# AGENTS.md — AI 编程助手指南

> **共享知识库 `kb/` 的位置**：`D:\CODE\rim\kb\`。本项目根目录下的 `kb/` 是指向该目录的**目录联接**（junction），因此 `kb/...` 与 `..\kb\...` 两种写法等价，**不是**项目内的独立副本。

> 本文件为本项目 AI 编程规范的**权威源**。详细规则见 `docs/B/B-01-开发规范.md`，Bug 反哺见 `docs/B/B-04-BUG知识库.md`。

***

## 一、项目身份

**Familia Myth** — 基于《在地下城期待邂逅是否搞错了什么》设定的 RimWorld 1.6 / 1.7 双版本模组。玩家扮演神明，通过仪式给予殖民者「神之恩惠」，让殖民者积累能力值/经验/伟业（Feat）并升华升级，成长为拥有技能、魔法、发展能力的眷族冒险者。

| 项    | 值                                                                            |
| ---- | ---------------------------------------------------------------------------- |
| 包 ID | `yanyu.familiamyth`                                                          |
| 语言   | C# 10 双目标：`net472`（1.6）/ `net10.0`（1.7）；单 csproj，**`ImplicitUsings` 开启**              |
| 游戏   | RimWorld 1.6（Mono）+ 1.7（CoreCLR / .NET 10，Unity 引用面由 `UnityStubs` + `Waylith` 提供） |
| 依赖库  | Harmony 2.x（`[HarmonyPatch]` + 模组构造函数内 `PatchAll` 注册）、原版 `Verse.Mod` 设置数据（`ModSettings` + `GetSettings<T>`，见 `FamiliaMythSettingsData.cs`）、Royalty / Ideology DLC      |
| 可选依赖 | Odyssey（海之眷顾等部分能力需要）                                                         |
| 输出   | 单 csproj 双目标，直出 `Familia_Myth/1.6/Assemblies` 与 `Familia_Myth/1.7/Assemblies`，由 `LoadFolders.xml` 按游戏版本装载（见 `docs/B/B-05`） |
| 远程仓库 | GitHub `yanyu-TAT/Rimworld-Familia_Myth`（main）                               |

### 设计文档（只读参考，改动需先对齐）

- `Familia Myth rim.txt` — 主设计文档（恩惠/魔法/技能/意志/公会/版本计划）
- `能力值效果.txt` / `能力值增长.txt` / `技能.txt` / `发展能力.txt` / `魔法.txt` / `魔法.xlsx` / `pointPreview.xlsx`
- `任务.txt` — 公会任务细节（主设计文档第 7 节指向的配套文档）
- 已实现清单 / 版本进度 / 美术本地化 — `docs/C/C-04-已实现内容清单.md`

***

## 二、常用命令

| 操作     | 命令                                                               |
| ------ | ---------------------------------------------------------------- |
| 并行调用工具 | 同一批次内并发发起多个互相独立的工具调用（如多个 `pwsh` / `read`） |
| 编译     | `dotnet build Familia_Myth/Familia_Myth.csproj`（一次出 1.6 + 1.7 两版；`-f net472` / `-f net10.0` 单编） |
| 刷新引用副本 | `pwsh -File .\13-同步引用库.ps1 -Version 1.6`（或 `1.7`，需先把 Steam 切到该版本） |
| 清理构建产物 | 运行 `11.bat`（清除 1.6 / 1.7 目录中非本模组 DLL 与 obj/）                      |
| 反编译原版  | 通过 ilspycmd 输出到 `kb/L0-证据/decompiled/<版本>/`，详见 `kb/L0-证据/decompiled/README.md`（1.6 程序集在 `RimWorldWin64_Data\Managed\`，1.7 在游戏根目录）             |
| 查看游戏日志 | 项目根目录 `Player.log`                                               |
| 检查 XML | PowerShell `[xml](Get-Content <file>)` 解析验证                      |
| 本地化体检 | `pwsh -File .\12-本地化检查.ps1`（7 项：Def 文本覆盖 / TKey / rulesStrings / key 占位 / Keyed 中英差集 / Translate 键 / **XML 类型引用可解析性**） |

> 编译引用取自 `lib\<版本>\` 副本（由 `13-同步引用库.ps1` 从 Steam 目录刷新），CI 上不可编译；构建与测试均在本机完成。双版本结构详见 `docs/B/B-05-双版本构建与发布.md`。

> **构建与实机测试由用户执行**：受运行环境沙箱限制（MSBuild 的 `Csc` 任务需用匿名管道读取编译器输出，受限沙箱下报 `MSB3883 拒绝访问`），Agent 不执行 `dotnet build`，也不进游戏实测；代码落地后由用户本地构建并验证，把问题反馈给 Agent。Agent 侧负责静态核对：XML 解析、`12-本地化检查.ps1`、反编译 API 核对与代码审阅。

***

## 三、Boundaries

**Allowed**：`Source/`、`Defs/`、`Patches/`、`Languages/`、`Textures/`、`Materials/`、`docs/`、XML 文件、设计文档。
**Ask First**：csproj 程序集引用变更、`About.xml` 依赖/版本变更、破坏性重构、改动主设计文档。
**Never Touch**：`Data/`（原版/DLC XML）、`lib/` 中的 DLL（只由 `13-同步引用库.ps1` 刷新，禁止手工替换）、`Player.log`（仅读取分析）。
**受控整理**：`kb/L0-证据/decompiled/` 仅允许按「十二 · 反编译文件管理规范」查阅，添加以及管理反编译文件，不可修改其中内容。
**工作守则**：必须遵循第十二节的协作规范进行工作


***

## 四、强制规范

### 核心开发原则

```
✅ 高内聚低耦合、模块职责单一、组合优于继承
✅ 原版/第三方程序集一律经 `lib\<版本>\` 引用并标 `<Private>false</Private>`；切游戏版本后先跑 `13-同步引用库.ps1` 刷新副本
✅ 需要按游戏版本分叉的代码用 `#if RW16` / `#if RW17`，两版产物由 `LoadFolders.xml` 装载
❌ `<HintPath>` 指向 Steam 游戏目录（切版本即失效）、在 mod 根目录保留 `Assemblies/`（`<li>/</li>` 会让两版 DLL 同时被装载）
✅ 复用已有代码 — 先查 AscendHelper / HealingHelper / 扩展方法 / 现有基类，不重复造轮子
✅ 数值系数抽为 private const 常量（顶部集中，便于调参），避免魔法数字
✅ 避免过度注释 — 仅在逻辑不自明处写注释
✅ 配置尽量硬编码在代码/XML 中，不引入多余数据结构
✅ 动手前先 ilspycmd 反编译确认原版 API 签名/字段（不改则问，不凭旧记忆猜）
✅ 涉及未知原版 API / DLC 特性时先反编译验证，必要时联网核对当前版本用法
✅ 需要跨存读档保持的运行时状态，随所属对象 ExposeData 持久化，或在 Load 后从可持久化数据重建（如嘲讽 Provokers 在 PostLoadInit 重建）
✅ 自管原版类型的集合时（如仿 `Hediff.abilities` 用 `LookMode.Deep` 自存 `Ability` 列表），必须连原版在该集合上的 `PostLoadInit` 重建一并照搬——`Ability.pawn` / `verb.caster` 这类**继承字段不参与 Scribe**，漏搬即读档首拍 NRE 并被原版兜底 `RemoveHediff` 清掉整个 Hediff（如 B-04 BUG-028）
✅ AcceptanceReport 语义：返回字符串（含空串）即「不接受/禁用」；表示接受用 return true（bool→WasAccepted）
✅ 根据文档内容使用 RimWorld / Harmony API
❌ 凭旧记忆写 RimWorld / Harmony API（版本差异大，易踩坑）
❌ 把需跨存读档保持的运行时状态放进不参与 Scribe 序列化的 static / 非 ExposeData 字段（存读档后丢失）
❌ 引入可由现有依赖覆盖的新依赖
❌ 只走 happy path、吞掉异常、硬编码不再问
❌ 绕过统一工厂自行构造「Def 派生型生成器 / 工厂」（漏赋 def → 派生属性静默为 null、轻则丢奖励重则 NRE）
✅ 对外入口（Debug 指令 / 事件回调 / 跨系统调用）先校验关键依赖（null → Log.Error + return），且校验必须排在**任何副作用之前**（如 B-04 BUG-017）
✅ Bug 溯源/修复/调试中勿臆测实际现象——先确认对象与机制身份、原版交互路径，收集现场状态（版本/位置/日志/报错原文）；有疑问直接向用户提出，再动手
✅ 复用原版部件/工具前先反编译核对其**隐式前置条件**——缺数据不会在部件入口报错，往往在更晚的文本解析/结算期才炸；必要时自建预检或兜底（如 B-04 BUG-018 延迟回报部件缺 rewardGiver）
✅ 跨系统「归位/移交」步骤必须与**信号窗口**对齐：`SentSatisfied/SentUnsatisfied` 当拍成员仍在穿梭机容器内，归位须在此当拍完成（如 B-04 BUG-018）
✅ 「先写共享状态、再执行可能失败的动作」时，失败路径必须回滚该状态——残留值会被其他系统误读（如 `currentVirtue` 残留致崩溃被静默吞掉，见 B-04 BUG-023）
❌ 从公共名单派生「待处理子集」时漏列应排除类别（离场名单漏排「已登机/已转正」者 → 把该留下的人赶走，见 B-04 BUG-019）
```

### Harmony 补丁规范（mod 稳定性关键）

```
✅ 补丁类统一 [HarmonyPatch(typeof(X), nameof(X.Y))]，方法必须 static
✅ Prefix 返回 bool：false 跳过原方法；Postfix 用 ref __result 改返回值
✅ 补丁方法体包裹 try-catch 并 Log.Error，防止一处报错拖垮游戏
✅ 重载方法必须写明参数类型数组
✅ 访问 Find.* 单例前判空（游戏阶段未初始化时易 NRE）
✅ 补丁**注册**在 `FamiliaMythMod` 构造函数内一次性完成：`new Harmony(Content.PackageIdPlayerFacing).PatchAll(typeof(FamiliaMythMod).Assembly)`，整体包 try-catch + `Log.Error`。新增补丁只需「静态类 + `[HarmonyPatch]`」即可生效，除该构造函数外勿写 `PatchAll` / `new Harmony`（细则见 C-03）
❌ 移除或绕过构造函数里的 `PatchAll` —— 会整体丢失本程序集的补丁注册
```

### 资源与性能规范（RimWorld 特有）

```
✅ Mote/投影/纹理/相机等一次性资源用 try/finally 销毁（Material、RenderTexture、Camera、GameObject）
✅ 同帧批量创建/销毁临时对象时用 `DestroyImmediate`——`Destroy` 延迟到帧末，上一批对象会残留在相机视野里被渲染进当前结果（见 B-04 BUG-029）
✅ 帧循环（DrawAt/Tick）中避免 GC 分配与 LINQ 高频调用
✅ 咏唱词纹理取自离线预渲染 PNG（`Textures/Familia_Myth/Incantation/`，调试指令导出），运行时只读资源、不再烘焙
```

### 代码规范体系（三层引用）

- 核心规则 → 本文件「强制规范」+「五、基础代码规范」
- 详细规则 → `docs/B/B-01-开发规范.md`（8 章实写，权威源为本文件）
- 反哺闭环 → `docs/B/B-04-BUG知识库.md`（修复 Bug 根因是规范缺失时，反哺 B-01 与本文件）

***

## 五、基础代码规范（C# 核心规则）

### 1. 命名

```
✅ 类/接口/方法/属性/常量/枚举 → PascalCase；接口前缀 I；泛型参数前缀 T
✅ 私有/内部字段 → _camelCase；参数与局部变量 → camelCase
✅ 用原语别名 int/float/string/bool（声明处）；.NET 名用于静态成员（int.TryParse 除外用原语）
❌ 匈牙利命名、缩写、两字符以上缩写全大写混用
```

### 2. 声明与表达式

```
✅ 类型明显时用 var；只读字段用 readonly
✅ 常量数字集中为 const（见强制规范）；字符串用 $"" 插值
✅ 空值判断用 is null / is not null
✅ 隐式 using 已开启（csproj `ImplicitUsings`），**实际只生成 6 项**：`System` / `System.Collections.Generic` / `System.IO` / `System.Linq` / `System.Threading` / `System.Threading.Tasks`；除这 6 项外的命名空间（`System.Reflection` / `System.Text` / `System.Net.Http` 等）**均须自行 using**
❌ 字符串 + 拼接循环、var 滥用（类型不明显时显式声明）
```

### 3. 集合与 LINQ

```
✅ 过滤/转换/聚合用 LINQ（Where/Select/Any）；简单遍历可用 foreach
❌ foreach 中增删集合（需用反向 for 或 ToList 快照）
```

### 4. 错误处理

```
✅ 只在系统边界（玩家输入/外部 API/原版回调）校验，内部代码信任框架保证
✅ catch 至少 Log.Error + 恢复路径；不空吞异常
❌ 为不可能发生的情况加防御性代码
```

### 5. XML 定义

```
✅ 引用类名带命名空间前缀（如 <li Class="HarmonyLib..."> / Verse.HediffDef）——xml 中类名注意命名空间前缀
✅ 新 Def 的 defName 全局唯一；ParentName 继承时核对父子关系
✅ defName 必须是字母/下划线开头的合法标识符——DefInjected 键是 XML 元素名 `<DefName>.<字段>`，数字开头无法表达（见 B-04 BUG-035）
✅ 逻辑优先做进 Defs/Patches（PatchOperationConditional 按 DLC/Mod 条件注入），减少代码分支
```

### 6. 本地化

```
✅ 玩家可见文本进 Languages/（ChineseSimplified + English），用 DefInjected/Keyed
✅ 四条路径别混用：Def 字段→DefInjected（`<DefName>.<字段>`）；rulesStrings→DefInjected 整表替换；
   任务节点 SlateRef 文本→Def 元素加 `TKey="引用名"` + 语言文件 `<DefName>.<引用名>.slateRef`；
   代码内文本→Keyed + `"Key".Translate()`（键名 `FamiliaMyth.<域>.<语义>`）
✅ Def 内文本一律保留作默认/兜底，由语言文件覆盖（原版模式 `<label TKey="Xxx">Default</label>`）
✅ `description` 只写**客观定性**描述（大量 / 数次 / 大范围 / 片刻），精确数值与叙事留给图鉴——运行时精确值已由 UI 承担（充能数 / 状态标签 / 剩余次数）
❌ 把 Keyed 键填进 Def 字段——Def 字段不会自动 Translate，只会把键原样显示（细则见 C-03）
❌ 在语言文件里写 `{0:P0}` / `{0:F1}` 等 .NET 格式说明符——`Translate(...)` 走 RimWorld 的 `GrammarResolverSimple`，`:` 会被当作 `{func:args}` 函数语法，未知函数**静默返回空串**（无红字、检查脚本也不报），表现为该参数位置显示为空；格式化请在参数侧做完再传（`ToStringPercent()` / `ToString("F1")`，见 B-04 BUG-024）
❌ 清空 Def 内文本（语言键缺失时会退化成 defName）
❌ 将纯展示文本硬编码散落在逻辑代码里（例外：运行时动态拼接）
✅ 本地化完整性用根目录 `12-本地化检查.ps1` 校验
```

### 7. 注释

```
✅ 注释只描述 **当前状态** —— 这个类型 / 成员现在是什么、做什么
✅ 力求简短，一两句话概括即可
✅ 仅在逻辑不自明处、或需说明接口契约与约束时才写注释
✅ 实现发生变化时同步更新注释 —— 与实现不符的注释比冗长的注释更糟
❌ 不写历史状态（"此前…"、"原本…"、"已由…改为…"）
❌ 不写设计理由与推导（"为了…"、"因此…"、"之所以…"、"为什么不直接用…"）
❌ 不写方案对比与前后变化（"而不再需要…"、"比…更…"）
❌ 不写改动过程与决策记录 —— 改动缘由属于 `docs/`、`plans/` 与提交信息
```

***

## 六、Git 规范

- 分支：`main` 唯一常驻，正式版本用 tag 发布（版本号见 `About.xml` 与主设计文档「版本计划」）；双版本共存期可在 tag 名标注对应游戏版本（如 `v0.0.5-1.6-final`）
- 提交：所有变更由用户进行提交  
- 回滚：必要时征求用户意见后进行回滚
- ❌ `git push --force` / `git reset --hard`
- ❌ 提交 `Data/`、`lib/`（引用副本）、非本模组的 DLL、`obj/`、`Player.log`、临时文件（.gitignore 已覆盖）

***

## 七、代码审查清单

```
通用:
✅ 新文件是必需的吗？能否复用已有基类/工具（AscendHelper/HealingHelper/扩展方法）？
✅ 数值系数是否已抽为 const？来源是否对应设计文档？
✅ Harmony 补丁 static + try-catch？原版签名已反编译核对？
✅ 一次性资源（Material/RenderTexture/Camera/GameObject）是否已释放？
✅ 随机选择是否用 `TryRandomElementByWeight`（权重和 ≤ 0 时返回 false）并已判空候选集合（空集合取随机索引会越界）？零权重项不入池？
✅ Hediff 初始 severity > 0（避免被立即移除）？
✅ XML 类名带命名空间前缀？defName 唯一？DLC 依赖是否用 PatchOperationConditional 保护？
✅ 无吞异常 / 无只走 happy path？
✅ 已对照「八、AI 高频错误」自查？
语言特定（C#）:
✅ dotnet build 通过？无新增警告/错误？
✅ 本地进游戏加载无红字（检查 Player.log）？
```

***

## 八、AI 高频错误防犯清单（本技术栈）

| #  | 易错点                                | 正确做法                                                          |
| -- | ---------------------------------- | ------------------------------------------------------------- |
| 1  | 凭记忆写 RimWorld/Harmony API，方法/字段不存在 | 先 ilspycmd 反编译核对 |
| 2  | Harmony 补丁方法非 static / 签名不匹配       | 保持 static，重载写明参数类型                                            |
| 3  | 补丁无 try-catch，一处异常拖垮游戏             | 包裹 try-catch + Log.Error                                      |
| 4  | Mote/特效资源泄漏（纹理/相机/材质）              | try/finally 销毁；DrawAt 中重查贴图占位                                 |
| 5  | 随机抽取死循环 / 空池抽样                     | 用 `TryRandomElementByWeight`（权重和 ≤ 0 时返回 false）并在抽样前判空候选集；零权重项不入池 |
| 6  | 迭代中修改集合                            | 快照 / 反向遍历                                                     |
| 7  | Hediff severity=0 立即被移除            | `initialSeverity` 置正                                          |
| 8  | XML 类名缺命名空间前缀 / defName 冲突         | 带前缀、全局唯一                                                      |
| 9  | 魔法数字无出处                            | 抽 const 并对应设计文档数值                                             |
| 10 | 在 Patch 中访问未初始化 Find 单例            | 判空 + 阶段判断                                                     |
| 11 | 复用原版部件未核对其隐式前置（缺数据在解析/结算期才炸）      | 先反编译确认前置；必要时自建预检/兜底                                         |
| 12 | 归位/移交与信号窗口错位（`Sent*` 当拍成员仍在容器内）     | 在信号当拍完成归位                                                    |
| 13 | 派生名单漏列应排除类别（把该留下的对象赶走）            | 逐项列出排除集合并核对成功/失败两线口径                                       |
| 14 | 补丁 catch 里 `return false` 静默屏蔽原版方法 / 成对 `__state` 未设哨兵值 | Prefix 的 catch 一律 `return true` 放行原版；Prefix 出错时给 `__state` 写哨兵值令 Postfix 短路（见 C-03） |
| 15 | 随机取样前未校验候选集合非空（空集合取随机索引越界） | 取样前判空，必要时填兜底集合（见 B-04 BUG-026） |
| 16 | 自管原版类型集合时漏搬原版 `PostLoadInit` 重建（`Ability.pawn` / `verb.caster` 等继承字段不参与 Scribe，读档为 null） | 照搬原版 `Hediff.ExposeData` 的 PostLoadInit 补回；或改持 `Scribe_References` 引用后间接取 Pawn（见 B-04 BUG-028） |
| 17 | 同帧批量创建/销毁临时对象用 `Destroy`，上一批对象残留在相机视野里被渲染进当前结果 | 用 `DestroyImmediate`；批量导出/生成类代码尤其要注意（见 B-04 BUG-029） |
| 18 | 反编译的版本与实际运行的版本不一致（`kb/L0-证据/decompiled/1.6` 是 1.6，实机跑 1.7；Clap 项目则相反：库为 1.7），据此判断运行时行为 | 排查运行时行为前先确认游戏版本，反编译对应版本（1.6 在 `RimWorldWin64_Data\Managed\`，1.7 在游戏根目录）；`Pawn` / `Thing` 的死亡与销毁路径两版不同（见 B-04 BUG-033） |
| 19 | net472 目标下误用 `System.dll` 独有的泛型集合（如 `Queue<T>`）→ `CS0433` 类型同时存在于 mscorlib 与 System | 统一用 mscorlib 的 `List<T>` / `Dictionary<K,V>`；需要队列/栈语义时用 `List<T>` 手动维护（`Add` / `[0]` / `RemoveAt(0)`，见 B-04 BUG-038） |
| 20 | 含富文本标签的文本走了 `TaggedString → string` 的隐式/显式转型 | `TaggedString` 的 `implicit operator string` 是 `RawText.StripTags()`，会把 `<color>` 与 `(*...)` 一起剥掉；要保留标签必须取 `.RawText`（见 B-04 BUG-039） |
| 21 | 自研 `SkillDef` 与原版 `RimWorld.SkillDef` 同名，在 `FamiliaMyth.Patch` 等外部命名空间裸写 → `CS0104` 歧义 | 写完全限定名 `FamiliaMyth.Abilities.SkillDef`（`Patch_SkillGrowth` / `Patch_Railgun` 为先例；`FamiliaMyth.Abilities` 内裸写安全，本命名空间优先） |

***

## 九、模块速查表

| 系统 | 文件 / 前缀 | 职责 |
| --- | --- | --- |
| **恩惠与成长** | `Hediff_Pharna.cs` | 恩惠状态：六维能力值 / 经验 / 伟业记录、成长与升华、铭刻、魔力池 |
| | `PharnaUtility`（同文件内） | 恩惠与生效中技能的查询扩展 |
| | `StatPart_*.cs` + `Patches/StatParts_*.xml` | StatDef 注入修正（恩惠系数 / 信息卡展示项 / 魔力操控）；`StatPart_PharnaWealth` 把恩惠等级与经验折入原版 `MarketValue`（顶点 ×25），经原版财富链路抬升袭击点数 |
| | `PharnaCurve.cs` | 六维加成曲线：把原始能力值折算为系数计算用的有效值（优先取设置页配置，未配置时用内置默认：轻微前缓后急，魔力维恒等） |
| | `DevelopableAbility*.cs` | 发展能力（熟练度 + StatPart 注入）；武器同调含按武器 def 的熟练度、冷却重置 / 暴击补丁与当前装备展示 Hediff；连攻含 10 秒近战攻速窗口与 `IStatusModifier` 补正；精愈含精神力持续恢复与按精神力消耗喂熟练度 |
| | `Patch_<能力值>Gain_<来源>.cs` | 各能力值的经验获取补丁 |
| | `Hediff_Pharna.PainFactor`（override） | 耐久 → 疼痛系数：覆盖 `Hediff.PainFactor` 返回 `0.9995^s`（`s` 为 `PharnaCurve` 折算值），`GainStatus` 改耐久后刷 `DirtyPainCache`（疼痛是 Hediff 累乘值、不是 StatDef，无法经 `StatPart_Pharna` 注入） |
| | `AscendHelper.cs`（含 `AscendRewardSession`） | 升华奖励：Def 注册表与抽取（权重 / 保底 / 确定性种子）、选取会话（刷新 / 锁定） |
| | `PharnaRecord.cs` | 恩惠丢失快照（六维 / 潜在值 / 铭刻 / 等级 / 经验 / 伟业 / 魔力池 + 技能魔法发展能力 defName 与熟练度）与重建（统一工厂 + `Apply`，事后校验） |
| | `ChoiceLetter_PharnaLost.cs` + `Defs/Misc/LetterDefs/LetterDef_PharnaLost.xml` | 恩惠意外消失信（重建 / 放弃两个按钮）；`Patch_PharnaDeathLetter.cs` 在持有恩惠者死亡信正文前置神明感知段 |
| | `GrowthFeedback.cs` | 成长反馈统一出口：能力值 / 发展能力跨档时发一条 Message（一次跨多档只报最终档），音效由原版 `MessageTypeDef` 自带 |
| **魔法** | `Spell*.cs` + `SpellDef.cs` | 魔法框架与具体魔法（继承原版 Ability） |
| | `Command_Spell.cs` | 魔法图标命令（继承原版 `Command_Psycast`）：供原版心灵熵条识别并预览施法熵消耗，且角标与描述中显示魔力消耗 |
| | `Hediff_SpellState.cs` + `Verb_CastSpell*.cs` | 魔法效果状态与咏唱 verb |
| | `Projectile_*` + `MagicMissileStates.cs` | 魔法弹与运动状态机 |
| | `Gizmo_ManaPool.cs` | 魔力池状态条、自动转化开关与施法魔力消耗预览 |
| **技能** | `Skill*.cs` + `SkillDef.cs` | 技能框架与具体技能（主动 / 被动 / 触发） |
| | `HediffComp_SkillState.cs` / `HediffComp_SkillInEffect.cs` | 技能状态标签与效果生命周期 |
| | `HediffComp_DisappearsWithCounter.cs` | 按剩余次数结束效果的计数 comp（`TryConsume` 消耗，`ResetCnt` 重设计数） |
| | `Hediff_*.cs`（效果类） | 技能 / 魔法的常驻效果 Hediff（配套 `Defs/HediffDefs/`） |
| **战斗日志** | `CombatLogHelper.cs` + `BattleLogEntry_AbilityCast.cs` | `ICombatLogSource` 日志配置接口（图标 / 施法行与伤害行规则包 / 显示名）与统一构造入口；施法行为自建条目、仅记施法者，伤害行复用原版 `MeleeCombat` + 可配规则包 |
| **意志与美德** | `MentalState_Virtue.cs` | 美德精神状态与触发当拍的心声飘字 |
| | `Patch_Virtue*.cs` | 崩溃前拦截与美德可控性补丁 |
| | `Defs/HediffDefs/Hediff_Virtues.xml` | 五种美德 Buff 与范围光环 |
| **公会与任务** | `GuildQuestDef.cs` / `GuildQuestGenerator*.cs` | 公会任务定义与生成（统一工厂 `Create(def)` + `Available()` 预筛）；任务侧只发评级升级点数，**不发放伟业** |
| | `IncidentWorker_GuildEnvoy.cs` / `JobDriver_TalkGuildEnvoy.cs` / `GameComponent_GuildEnvoyTracker.cs` | 使者到访、对话接取与全局单槽登记 |
| | `QuestNode_Root_{Daily/RankUp}_{任务名}` / `QuestNode_*Setup` / `QuestPart_*` | 任务三件套：根节点、过程 part、奖励与评级 part |
| | `QuestUtilities.cs` | 任务点数计算（升级点由各任务 part 发放） |
| | `WorldObjectComp_*.cs` | 世界对象交互 comp（定居点领取 / 交付按钮） |
| | `Defs/GuildQuestDefs/` + `Defs/QuestScriptDefs/` | 任务定义与任务脚本 |
| **伟业（Feat）** | `FeatTraitDef.cs`（内含 `FeatTraitScope` / `FeatContext` / `FeatTraitJudger` / `FeatRecord`） | 伟业数据层：性质定义（`judgerClass` + 系数 + `scope` 作用域）、战斗数据包门面（持贡献账本）、判定器基类（战斗层 / 个人层双入口）与统一工厂、记录 |
| | `FeatTraitJudgers.cs` | 各性质判定器（阈值集中在类内；判定只看战斗事实，不看来源；`Battle` 实现 `Judge(ctx)`、`Pawn` 实现 `Judge(ctx, subject)`） |
| | `FeatUtility.cs` | 结算流水线：取战斗事实 → 战斗层逐 Def 判定并累乘 → 个人层逐人判定 → 按贡献分摊并乘个人系数 → 阈值分割档位 → 整理成 `FeatBattleReport`（日志与信件由它自己的两个视图输出） |
| | `FeatBattleReport.cs` | 结算快照与它自己的两个视图：`WriteLog()` 输出开发者日志（保留合账 / Σ占比核对字段），`SendLetter()` 输出伟业达成信（战斗解析 + 可升华判断，一场战斗一封，`LetterDef` 见 `Defs/Misc/LetterDefs/LetterDef_Feat.xml`） |
| | `GameComponent_FeatTracker.cs` | 战斗扫描器与贡献账本（`BattleAccount` / `PawnContribution`）：按战斗聚合、归属解析、吸收归并、结算去重与清理 |
| | `Patch_FeatContribution.cs` | 伤害归因补丁（`Pawn.PostApplyDamage` 记输出 / 承受） |
| | `HealHelper.cs`（治疗工具） + `HediffComp_FeatSupport.cs` | 支援贡献上报：治疗三工具按真实量记（末位 `healer` 参数归因），持续型效果由 Hediff 组件按实际生效 tick 记（XML 配 `<supportPerSecond>`） |
| | `Defs/FeatTraitDefs/` | 伟业性质定义（`<judgerClass>` 指向判定器） |
| **仪式与文化** | `Ritual*.cs`（含 `RitualRoleColonist_*` / `RitualCandidateUtility`） | 给予 / 升华恩惠仪式及其接受者校验 |
| | `Defs/PreceptDefs/` + `Defs/RitualBehaviorDefs/` + `Defs/RitualOutcomeEffectDefs/` + `Defs/RitualPatternDefs/` | 仪式的 Precept、行为、结果与图案定义 |
| | `Defs/MemeDefs/` + `Defs/RulePackDefs/` + `Patch_DeityGender.cs` | 文化结构、文化命名规则包与神灵性别绑定 |
| **存档级与场地** | `GameComponent_*.cs` | 存档级追踪与引擎（评级 / 使者 / 位移击退 / 定时动作） |
| | `MapComponent_*.cs` | 地图级状态（恩惠特效管理） |
| **表现层** | `Mote_*.cs` | 地图特效（咏唱词 / 魔法阵 / 光环 / 斩击等） |
| | `UI_*.cs` | UI 基础设施与页面：`UI_Palette` 全局色源 / `UI_Scope` GUI 状态作用域 / `UI_Text` 度量与截断 / `UI_Bar` 进度条与门槛刻度 / `UI_Layout` 纵向游标 / `UI_GroupBox` 卡片 / `UI_RadarChart` 雷达图 / `UI_FamiliaPage` 眷族页 / `UI_SettingsPage` 模组设置页 / `UI_CurveEditor` 曲线编辑器 |
| | `Window_FamiliaBase.cs` + `Dialog_AscendReward.cs` | 窗口基类（内容区模板 + 初始尺寸换算）与升华奖励弹窗（符文动画） |
| | `ITab_Pawn_Pharna.cs` | Pawn 级「恩惠」页签：能力 / 发展能力 / 技能·魔法 / 伟业 四分区（仅受恩惠者可见），顶栏含铭刻选取；六维条与发展能力条按显示值追赶真实值（亮度随剩余差距衰减） |
| | `HistoryTabHelper.cs` + `Gizmo_ManaPool.cs` | 历史窗口眷族页签接入（含失效兜底）与魔力池 Gizmo |
| **工具与基础设施** | `WeaponHelper.cs` | 武器查询与直线索敌 / 射线工具 |
| | `HealHelper.cs` | 伤口与断肢治疗工具 |
| | `FamiliaMythMod.cs` | 模组主类（`Verse.Mod` 子类）：构造函数内注册全部 Harmony 补丁与设置数据，包含开发日志和计划（文件内日志 / 计划表仅用户维护） |
| | `FamiliaMythSettings.cs` | 模组设置项门面：实验性内容开关（`GuildContentEnabled`）与六维曲线读写（`Curve` / `ResetCurve` / `ResetAllCurves`） |
| | `FamiliaMythSettingsData.cs` | 模组设置数据（`ModSettings` 子类，`GetSettings<T>` 读写 Config 目录）：公会开关 + 六条 `Scribe_Deep` 曲线 |
| | `Patch_*.cs` | Harmony 补丁（命名与异常防护细则见 C-03） |
| | `DebugAction_*.cs` | 开发者模式调试指令（恩惠 / 任务 / 杂项：导出咏唱词纹理） |
| | `Mote_Incantation.cs` + `Textures/Familia_Myth/Incantation/` | 咏唱词纹理：只读离线预渲染 PNG（1.6 用调试指令导出），缺失时报错；`MeshHeight` / `TextHeightRatio` / `marginRatio` 分层控制字号与飞溅留白 |
| | `Defs/` + `Patches/` + `Languages/` | XML 定义、条件补丁与中英本地化 |
| | `LoadFolders.xml` + `1.6/` + `1.7/` | 双版本装载：`<v1.6>` / `<v1.7>` 各装载共享根目录与对应版本目录（细则见 B-05） |
| | `13-同步引用库.ps1` | 从 Steam 游戏与创意工坊刷新 `lib\<版本>\` 引用副本（含哈希记录） |

> 本表只做**定位索引**：每行一句话说明该文件 / 前缀负责什么。机制细节、常量与信号约定见 `docs/C/C-02-架构详解.md`，长期约定见 `docs/C/C-03-项目长期记忆.md`，任务与内容明细见 `docs/C/C-04-已实现内容清单.md`。新增模块/文件时同步更新本表。

***

## 十、文档维护

- **维护范围**：文档维护只涉及 `docs/` 目录下的文件与根目录 `AGENTS.md`；`CLAUDE.md`、`.cursor/rules/project.mdc` 等规则引用文件以及其他根目录文档均不做主动维护。
- **不加时间戳**：维护上述文档时，不在文件头部添加或更新"更新: 日期"之类的时间戳。
- **主设定文档仅用户修改**：`Familia Myth rim.txt` 及其配套文档（`能力值效果.txt`、`能力值增长.txt`、`技能.txt`、`发展能力.txt`、`魔法.txt`、`魔法.xlsx`、`pointPreview.xlsx` 等）为权威设定来源，**仅由用户修改**；Agent 只能读取参考，不得改动或代写。
- **计划文件统一存放 `plans/`**：用户要求生成的开发计划（如技能/特效/补充 XML 计划）一律放入根目录 `plans/`，不散落在 docs/ 或工具目录。**计划仅为阶段性思路参考，其中的代码/数值/接口可能与最终实际实现存在极大差异，落地一律以 Source/Defs 实际代码与用户确认为准，勿把计划当作已实现内容引用。**
- **模组主类内的日志与计划表仅用户维护**：`Familia_Myth/Source/FamiliaMythMod.cs` 中的 Todo List、Develop Log、开发日志、计划表等记录均**只由用户维护**；Agent 不得新增、修改、删除、整理或"顺手补一条"。需要记录进度时写 `docs/C/C-04-已实现内容清单.md` 或 `plans/`。

***

## 十一、上下文管理

- **Agent 主动维护本文件** — 在代码/XML被修改并经审阅后，依据修改后的实现进行文档维护。发现有 文档与项目实际实现/内容有不符 时，向用户提问确认后进行修改。每次完成以下操作时同步更新：
  | 操作                     | 更新内容                                    |
  | ---------------------- | --------------------------------------- |
  | 新增模块/文件                | 更新「模块速查表」                               |
  | 新增/迁移玩家可见文本            | 按 C-03「四条文本路径」处理；新术语先补 `docs/C/C-06-本地化术语表.md`；跑 `12-本地化检查.ps1` 确认对称 |
  | 重命名类名 / 文件；增删枚举成员 | 同步 `Defs`/`Patches` 内相关引用——枚举/数值这类**裸字面值**（如 `<mode>X</mode>`）脚本抓不到，须人工全库检索；类型引用跑 `12-本地化检查.ps1` 第 [7] 项（见 B-04 BUG-020 / BUG-025） |
  | 新增依赖/工具                | 更新「项目身份」技术栈行                            |
  | 构建/发布结构变更（目标框架 / 输出目录 / LoadFolders / 引用来源） | 更新「项目身份」输出行、「常用命令」与本条；细则写入 `docs/B/B-05-双版本构建与发布.md` |
  | 发现代码与现有规范明显偏离（或新的代码规范） | 先提出偏离点并询问是否为新的规范，确认后追加「强制规范 / 基础代码规范」   |
  | 做出架构决策                 | 追加到关键决策记录（可写 docs/C/C-02）               |
  | 修复典型 Bug               | 写入 `docs/B/B-04-BUG知识库.md`              |
  | 修复 Bug 且根因是规范缺失        | 补充 B-01 对应章节 + 本文件新增一条 ✅/❌              |
  | 文档膨胀                   | 按职责域拆分到 docs/ 引用，保持本文件聚焦核心              |
  | 项目进度检查（功能完成/重构/发版）     | 检查 AGENTS.md / B-01 / B-04 是否滞后，滞后则提醒用户 |

***

## 十二、全局协作规则（用户级，全项目适用）

- **疑问即问**：对用户的要求或说明有疑惑时，直接通过工具提问；不猜测、不质疑、不否定。
- **结论需证据**：给出任何结论前，基于反编译结果查证；若没有证据，则在给出结论后明确指明"无证据/未经验证"。
- **XML 命名空间前缀**：编写 XML 文件时，引用类名注意命名空间前缀（详见「五、基础代码规范 · XML 定义」）。
- **反编译工具**：使用 `ilspycmd` 进行反编译，结果统一输出到全项目共享库 `kb/L0-证据/decompiled/<版本>/`（命令见「二、常用命令」，布局与增补规范见 `kb/L0-证据/decompiled/README.md`）。
- **反编译文件管理规范**（`kb/L0-证据/decompiled/<版本>/` 布局与维护；重新反编译或补充反编译后按此执行）：
  - 布局：文件按「完整命名空间目录 + 类型名文件」存放，目录即命名空间（如 `Verse/`、`Verse.AI/`、`RimWorld.Planet/`），无命名空间类型归 `_global/`
  - 命名：文件以其中主类型（class / struct / interface / enum / delegate）名命名，纯委托文件以委托名命名（文件名 = 类型名）
  - 去重：内容（哈希）完全相同的文件只保留一份，不留整目录/整批次的重复副本
  - 同名异内容：同命名空间同类型名但内容不同时，保留 ilspycmd 常规批量版；`.decompiled.cs` 手工版在有同名常规版时删除，仅当其是该类型唯一来源时保留
  - 清理：0 字节空壳、无类型的 `AssemblyInfo.cs`、`*.csproj` 等非源码杂项一律删除；`ilspy_help.txt` 保留在目录根
  - 边界：该目录不提交 git、不参与构建、不得当作作品代码改动；整理属受控维护，动作须按本规范执行
- **不改代码文件**：除非被直接要求，不修改代码文件（XML 文件除外）。
- **中文思考链**：始终使用中文输出思考链（reasoning / chain-of-thought），便于与用户协作审阅。
