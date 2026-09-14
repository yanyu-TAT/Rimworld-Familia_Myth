# AGENTS.md — AI 编程助手指南

> 本文件为本项目 AI 编程规范的**权威源**。详细规则见 `docs/B/B-01-开发规范.md`，Bug 反哺见 `docs/B/B-04-BUG知识库.md`。

***

## 一、项目身份

**Familia Myth** — 基于《在地下城期待邂逅是否搞错了什么》设定的 RimWorld 1.6 模组。玩家扮演神明，通过仪式给予殖民者「神之恩惠」，让殖民者积累能力值/经验/伟业值并升华升级，成长为拥有技能、魔法、发展能力的眷族冒险者。

| 项    | 值                                                                            |
| ---- | ---------------------------------------------------------------------------- |
| 包 ID | `yanyu.familiamyth`                                                          |
| 语言   | C# 10 / .NET Framework 4.7.2（`net472`，`LangVersion 10.0`）                    |
| 游戏   | RimWorld 1.6（`Assembly-CSharp`）+ Unity（IMGUI / TextMeshPro / CoreModule）     |
| 依赖库  | HugsLib（`ModBase` + `ModSettings` 模组设置）、Harmony 2.x（`[HarmonyPatch]`）、Royalty / Ideology DLC      |
| 可选依赖 | Odyssey（海之眷顾等部分能力需要）                                                         |
| 输出   | SDK 风格 csproj，`BaseOutputPath` 直出 `Familia_Myth/Assemblies/Familia_Myth.dll` |
| 远程仓库 | GitHub `yanyu-TAT/Rimworld-Familia_Myth`（main）                               |

### 设计文档（只读参考，改动需先对齐）

- `Familia Myth rim.txt` — 主设计文档（恩惠/魔法/技能/意志/公会/版本计划）
- `能力值效果.txt` / `能力值增长.txt` / `技能.txt` / `发展能力.txt` / `魔法.txt` / `魔法.xlsx`
- 已实现清单 / 版本进度 / 美术本地化 — `docs/C/C-04-已实现内容清单.md`

***

## 二、常用命令

| 操作     | 命令                                                               |
| ------ | ---------------------------------------------------------------- |
| 并行调用工具 | run_code                                                      |
| 编译     | `dotnet build Familia_Myth/Familia_Myth.csproj`（输出直写 Assemblies） |
| 清理构建产物 | 运行 `11.bat`（清除 Assemblies 中非本模组 DLL 与 obj/）                      |
| 反编译原版  | 通过ilspycmd输出到decompiledFiles\，详见decompiledFiles\ilspy_help.txt             |
| 查看游戏日志 | 项目根目录 `Player.log`                                               |
| 检查 XML | PowerShell `[xml](Get-Content <file>)` 解析验证                      |
| 本地化体检 | `pwsh -File .\12-本地化检查.ps1`（7 项：Def 文本覆盖 / TKey / rulesStrings / key 占位 / Keyed 中英差集 / Translate 键 / **XML 类型引用可解析性**） |

> 游戏程序集引用路径为 Steam 本地目录（见 csproj），CI 上不可编译；构建与测试均在本机完成。

***

## 三、Boundaries

**Allowed**：`Source/`、`Defs/`、`Patches/`、`Languages/`、`Textures/`、`Materials/`、`docs/`、XML 文件、设计文档。
**Ask First**：csproj 程序集引用变更、`About.xml` 依赖/版本变更、破坏性重构、改动主设计文档。
**Never Touch**：`Data/`（原版/DLC XML）、`lib/`、`Assemblies/` 中的第三方 DLL（0Harmony/HugsLib 等）、`Player.log`（仅读取分析）。
**受控整理**：`decompiledFiles/` 仅允许按「十二 · 反编译文件管理规范」查阅，添加以及管理反编译文件，不可修改其中内容。

***

## 四、强制规范

### 核心开发原则

```
✅ 高内聚低耦合、模块职责单一、组合优于继承
✅ 复用已有代码 — 先查 AscendHelper / HealingHelper / 扩展方法 / 现有基类，不重复造轮子
✅ 数值系数抽为 private const 常量（顶部集中，便于调参），避免魔法数字
✅ 避免过度注释 — 仅在逻辑不自明处写注释
✅ 配置尽量硬编码在代码/XML 中，不引入多余数据结构
✅ 动手前先 ilspycmd 反编译确认原版 API 签名/字段（不改则问，不凭旧记忆猜）
✅ 涉及未知原版 API / DLC 特性时先反编译验证，必要时联网核对当前版本用法
✅ 需要跨存读档保持的运行时状态，随所属对象 ExposeData 持久化，或在 Load 后从可持久化数据重建（如嘲讽 Provokers 在 PostLoadInit 重建）
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
```

### 资源与性能规范（RimWorld 特有）

```
✅ Mote/投影/纹理/相机等一次性资源用 try/finally 销毁（Material、RenderTexture、Camera、GameObject）
✅ 帧循环（DrawAt/Tick）中避免 GC 分配与 LINQ 高频调用
✅ Mote_Incantation 等文本渲染用相机烘焙贴图，不读字体图集 CPU 数据
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
❌ 清空 Def 内文本（语言键缺失时会退化成 defName）
❌ 将纯展示文本硬编码散落在逻辑代码里（例外：运行时动态拼接）
✅ 本地化完整性用根目录 `12-本地化检查.ps1` 校验
```

### 7. 注释

```
✅ 注释只描述**当前状态** —— 这个类型 / 成员现在是什么、做什么
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

- 分支：`main` 唯一常驻，正式版本用 tag 发布（版本号见 `About.xml` 与主设计文档「版本计划」）
- 提交：沿用项目现有中文描述风格（如 `添加了魔法X` / `修复了X` / `重构了X`），一句话说清变更
- 部署/发版前必须 commit；线上问题用 `git revert` 回滚
- ❌ `git push --force` / `git reset --hard`
- ❌ 提交 `Data/`、`lib/`、`Assemblies/` 第三方 DLL、`obj/`、`Player.log`、临时文件（.gitignore 已覆盖）

***

## 七、代码审查清单

```
通用:
✅ 新文件是必需的吗？能否复用已有基类/工具（AscendHelper/HealingHelper/扩展方法）？
✅ 数值系数是否已抽为 const？来源是否对应设计文档？
✅ Harmony 补丁 static + try-catch？原版签名已反编译核对？
✅ 一次性资源（Material/RenderTexture/Camera/GameObject）是否已释放？
✅ 随机选择是否有权重耗尽死循环保护（如 tt>0）？
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
| 5  | 随机抽取死循环                            | 权重耗尽判断（`tt > 0`），可抽项为空即返回                                     |
| 6  | 迭代中修改集合                            | 快照 / 反向遍历                                                     |
| 7  | Hediff severity=0 立即被移除            | `initialSeverity` 置正                                          |
| 8  | XML 类名缺命名空间前缀 / defName 冲突         | 带前缀、全局唯一                                                      |
| 9  | 魔法数字无出处                            | 抽 const 并对应设计文档数值                                             |
| 10 | 在 Patch 中访问未初始化 Find 单例            | 判空 + 阶段判断                                                     |
| 11 | 复用原版部件未核对其隐式前置（缺数据在解析/结算期才炸）      | 先反编译确认前置；必要时自建预检/兜底                                         |
| 12 | 归位/移交与信号窗口错位（`Sent*` 当拍成员仍在容器内）     | 在信号当拍完成归位                                                    |
| 13 | 派生名单漏列应排除类别（把该留下的对象赶走）            | 逐项列出排除集合并核对成功/失败两线口径                                       |
| 14 | 补丁 catch 里 `return false` 静默屏蔽原版方法 / 成对 `__state` 未设哨兵值 | Prefix 的 catch 一律 `return true` 放行原版；Prefix 出错时给 `__state` 写哨兵值令 Postfix 短路（见 C-03） |

***

## 九、模块速查表

| 目录/前缀                                       | 职责                      |
| ------------------------------------------- | ----------------------- |
| `Patch_*.cs`                                | Harmony 补丁（**以相关功能命名**：技能/魔法名、`<能力值>Gain_来源`、系统名；类名与文件名对应、命名空间统一 `FamiliaMyth.Patch`、方法体必包 try-catch，细则见 C-03） |
| `Hediff_Pharna.cs`（恩惠状态与成长）              | 成长入口 `GainStatus`（含**阈值式双衰减**：单项 400 / 总量 2000 起衰减，参数为文件顶部 4 个 const）；`CycleExp` = 六项当前值之和；`Ascend()` 把当前值累入潜在值并清零、消耗伟业值升级；等级上限由 `HediffDef.maxSeverity` 决定；**魔力池**（`manaPool` / `MaxMana` / `SpendMana` / `RefundMana` / 常态转化 `ConvertPsyfocusToMana`；注意 `mana` 是六维属性「魔力」，勿与 `manaPool` 混用）（细则见 C-03） |
| `Spell*.cs` + `SpellDef.cs`                 | 魔法框架与具体魔法（继承原版 Ability） |
| `Skill*.cs` + `SkillDef.cs`                 | 技能（主动/被动/触发；charge 充能 + active 效果生命周期） |
| `HediffComp_SkillState`（override CompLabelInBracketsExtra）/ `HediffComp_SkillInEffect` | 技能状态指示（Def 用 HediffWithComps，comp 反查 skillDef 的 StateText 展示括号标签；effect 生命周期间回调） |
| `HediffComp_DisappearsWithCounter`           | 效果按剩余次数结束的计数 comp（归零 CompShouldRemove 移除，配合 SkillInEffect 驱动 active；超电磁炮等按次数强化技能用） |
| `DevelopableAbility*.cs`                    | 发展能力（熟练度 + StatPart 注入） |
| `WeaponHelper.cs`（扩展 GetMeleeSharpWeapon/GetUnarmedMeleeVerb/GetRangedEquippedVerb + `RangeUtilities` 直线路径索敌/射线工具） | 武器/路径查询工具（直线沿线地格采样、敌对索敌、带 validator 的射线终点 GetDest；居合/夹枪/超电磁炮共用） |
| `Hediff_Pharna.cs` 内 `PharnaUtility`（`GetPharna` / `GetActiveSkill<T>`） | 恩惠获取；GetActiveSkill 取 pawn 自身 active 的指定技能实例（区别于嘲讽 Provokers 静态集合语义） |
| `Mote_*.cs`                                 | 地图特效（咏唱词/魔法阵/光环/斩击等）    |
| `Gizmo_ManaPool.cs`（`FamiliaMyth.UI`）      | 魔力池状态条：显示 `manaPool / ManaMax`，右上角角标开关自动转化；由 `Hediff_Pharna.GetGizmos` 挂出 |
| `Projectile_*` + `MagicMissileStates.cs`    | 魔法弹与运动状态机               |
| `Ritual*`                                   | 仪式（给予/升华恩惠）及结果处理        |
| `GuildQuestDef` / `GuildQuestGenerator*`        | 公会任务定义与生成（日常 = 使者对话接取指定接取者；强制 = 使者出场自动接取、不可拒绝；Slate 传参 → GenerateQuestAndMakeAvailable）。**实例一律经静态工厂 `GuildQuestGenerator.Create(def)` 构造**（绑定 def；漏绑会导致 Category/Type 静默为 null，见 B-04 BUG-017），发布前由 `Available()` 预筛 |
| `QuestUtilities` | 任务静态工具（点数统一计算 + 伟业值名单化发放，签名与规则见 C-02） |
| `QuestNode_ExploitsReward` / `QuestPart_ExploitsReward` + `QuestNode_RankUpComplete/Failed` / `QuestPart_RankUp*` | 统一奖励结算（伟业值/升级点）与评级晋升、失败重发部件（节点只生成 QuestPart、信号触发，机制见 C-02） |
| `QuestNode_Root_{Daily/RankUp}_{任务名}`（如 `QuestNode_Root_Daily_SiteClearance` / `QuestNode_Root_RankUp_SiteClearance` / `QuestNode_Root_Daily_Rescue`） | 任务三件套之「根节点」：生成期统一点数/参数、准备任务专属对象、装配过程 part（每任务自实现，规格相似，见 C-02） |
| `QuestNode_RescueSetup` + `QuestPart_Rescue` / `QuestPart_RescueSpawn` + `QuestPart_ExtraFaction` | 「紧急救援」过程 part（QuestPartActivable 状态机）：威胁清除当拍幸存者难民式临时加入玩家派系（`SetFaction` 玩家 + HomeFaction「原派系」标记 → 上头像栏可指挥、lodger 限制自动生效、守点 lord 当拍释放不再发放），结算一律归还原派系（就地接送流程见 C-02） |
| `QuestNode_Root_Daily_CriminalHunt` / `QuestNode_CriminalHuntSetup` + `QuestPart_CriminalHunt` / `QuestPart_CriminalHuntSpawn` | 「罪犯追捕」过程 part：Outpost 守卫 + 1 名同派系通缉犯同场（守点迎战）；威胁清除分流（击毙 → 普通结算；存活 → 打包带离窝点、离图计时 1~3 天 → 公会押解穿梭机 2 天窗口）；活犯上船起飞 → ×2 结算（囚犯 despawn discard、不改派系）；死亡任意原因 → 普通结算；招募/奴役或活着失踪 → 失败（流程见 C-02） |
| `QuestNode_Root_Daily_AncientRelic` / `QuestNode_AncientRelicSetup` + `QuestPart_AncientRelic`（圣物匣 `Thing_AncientRelic` + 命名 `AncientRelicNamer`） | 「遗迹探索」过程 part：生成原版 AncientComplex 遗迹（2~10 格、12 天时限）；图内唯一任务机关箱（HermeticCrate）室内落位装圣物匣（随机异教文化现造名）→ 开箱 = 发现（唤醒房内沉睡守卫 + 双计时：1~3 天 0.5× 袭击打据点 / 3~5 天公会穿梭机 2 天窗口）；圣物装船起飞 → 结算；窗口未装 / 圣物被毁或失踪 → 失败（详见 `plans/遗迹探索任务开发计划.md`） |
| `QuestNode_Root_RankUp_SupplyEscort` / `QuestNode_SupplyEscortSetup` + `QuestPart_SupplyEscort` / `QuestPart_SupplyEscortAmbush` + `WorldObjectComp_SupplyEscort`（props 经 Patch 注入 `SettlementDef.comps`） | 「物资押运」过程 part：世界中既有定居点选 A/B（中立/友好 + 与据点三边 ≤8 格，`GuildQuestGenerator.Available()` 预筛、根节点兜底红字）→ 远行队停在 A/B 格经 comp 按钮领取/交付（清单由 A 的 `TraderKind` 现生成：单价 ≤100、不腐烂、无需材质且无品质，总重 300~600kg 随机）→ 12 天限时 → 途中以 A/B 为圆心、AB 距离为半径的双圆内伏击（≤2 次，原版 `IncidentDef Ambush`，`forced` 下须自行校验 tile 可生成伏击图）→ 交付按**领取清单逐项相等**（def + 数量）核验，**支持分批交付**（每次只收现有部分、未交齐发信提示缺口），只扣清单内物品、不追踪实例来源（详见 `plans/物资押运任务开发计划.md`） |
| `QuestNode_Root_RankUp_RefugeeResettle` / `QuestNode_RefugeeResettleSetup` + `QuestPart_RefugeeResettle` + `QuestPart_AddQuestRefugeeDelayedRewardSafe` | 「难民安置」过程 part：强制任务；难民**地图边缘到访并临时加入玩家派系**（`quest.PawnsArrive(joinPlayer:true)` 边缘走入 + `MiniFaction` 标记 = lodger，临时隐藏难民派系由 `FactionGenerator` 现生成、与原版难民事件一致）→ 到场当拍随机受伤（部分倒地）→ 停留 15 天：**点数×1.2 的袭击 2 次**（首发 3~5 天、间隔 3~5 天，关「大威胁」难度则本次作废）+ 心情驱动劝加入（`PawnJoinOffer`，加入即转正并从 `CompShuttle.requiredPawns` 移除、全员加入则直接结算不召船）→ 第 15 天公会穿梭机（`Util_TransportShip_Pickup`，2 天窗口，仅落地发信）；失败线 = 囚禁/奴役、二次有害手术（`victims.SurgeryViolation`）、全灭、无人生还 → 存活难民转中立后自行离图（被囚禁/奴役的受害者本人留下）；伟业值按存活率（登机 + 加入）结算（见 C-02） |
| `GameComponent_*.cs`                            | 存档级追踪（`FamiliaRankTracker`：多分类评级/升级点数/强制任务与重发冷却、统一发布轮询、公会派系补建；`GuildEnvoyTracker`：全局单槽待对话使者）+ 位移/击退引擎（LanceCharge/RoundhouseKick） |
| `JobDriver_*`                               | 工作驱动（对话使者等）             |
| `MentalState_*` / `Hediff_*` / `Verb_*`     | 精神状态（`MentalState_Virtue`：美德状态 + 触发当拍随机飘出独白/祷告心声）、词缀、咏唱 verb         |
| `FamiliaMythMod.cs`（模组主类） | HugsLib `ModBase` 子类：`Initialize()` 里经 `Settings.GetHandle` 注册模组设置（神名）；持有神名三件套 `DefaultGodName1`（玩家视角）/ `DefaultGodName2` + `Honorific`（眷族视角）与 `GodAddress`（接口签名与陷阱见 C-03） |
| `UI_*.cs`                                   | 界面（眷族页、升级奖励弹窗、雷达图）      |
| `StatPart_*`                                | StatDef 注入修正（`StatPart_ManaControl`：魔力操控按 SpellType 过滤技能 / 发展能力 / hediff comp 的 `IManaControlModifier` 加成）            |
| `Defs/` + `Patches/`                        | XML 定义与条件补丁             |
| `Languages/`（`Keyed/` + `DefInjected/`）    | 本地化文本（中英逐键对称，`12-本地化检查.ps1` 7 项全绿）；`Keyed/` 放代码内文本、`DefInjected/` 放 Def 文本且文件名与 `Defs/` 一一对应；四条文本路径与命名规范见 C-03，术语基准见 C-06 |

> 新增模块/文件时同步更新本表。

***

## 十、文档维护

- **维护范围**：文档维护只涉及 `docs/` 目录下的文件与根目录 `AGENTS.md`；`CLAUDE.md`、`.cursor/rules/project.mdc` 等规则引用文件以及其他根目录文档均不做主动维护。
- **不加时间戳**：维护上述文档时，不在文件头部添加或更新"更新: 日期"之类的时间戳。
- **主设定文档仅用户修改**：`Familia Myth rim.txt` 及其配套文档（`能力值效果.txt`、`能力值增长.txt`、`技能.txt`、`发展能力.txt`、`魔法.txt`、`魔法.xlsx`、`pointPreview.xlsx` 等）为权威设定来源，**仅由用户修改**；Agent 只能读取参考，不得改动或代写。
- **计划文件统一存放 `plans/`**：用户要求生成的开发计划（如技能/特效/补充 XML 计划）一律放入根目录 `plans/`，不散落在 docs/ 或工具目录。**计划仅为阶段性思路参考，其中的代码/数值/接口可能与最终实际实现存在极大差异，落地一律以 Source/Defs 实际代码与用户确认为准，勿把计划当作已实现内容引用。**

***

## 十一、上下文管理

- **Agent 主动维护本文件** — 在代码/XML被修改并经审阅后，依据修改后的实现进行文档维护。发现有 文档与项目实际实现/内容有不符 时，向用户提问确认后进行修改。每次完成以下操作时同步更新：
  | 操作                     | 更新内容                                    |
  | ---------------------- | --------------------------------------- |
  | 新增模块/文件                | 更新「模块速查表」                               |
  | 新增/迁移玩家可见文本            | 按 C-03「四条文本路径」处理；新术语先补 `docs/C/C-06-本地化术语表.md`；跑 `12-本地化检查.ps1` 确认对称 |
  | 重命名类名 / 文件              | 同步 `Defs`/`Patches` 内相关类型引用；跑 `12-本地化检查.ps1` 第 [7] 项（见 B-04 BUG-020） |
  | 新增依赖/工具                | 更新「项目身份」技术栈行                            |
  | 发现代码与现有规范明显偏离（或新的代码规范） | 先提出偏离点并询问是否为新的规范，确认后追加「强制规范 / 基础代码规范」   |
  | 做出架构决策                 | 追加到关键决策记录（可写 docs/C/C-02）               |
  | 修复典型 Bug               | 写入 `docs/B/B-04-BUG知识库.md`              |
  | 修复 Bug 且根因是规范缺失        | 补充 B-01 对应章节 + 本文件新增一条 ✅/❌              |
  | 文档膨胀                   | 按职责域拆分到 docs/ 引用，保持本文件聚焦核心              |
  | 项目进度检查（功能完成/重构/发版）     | 检查 AGENTS.md / B-01 / B-04 是否滞后，滞后则提醒用户 |

***

## 十二、全局协作规则（用户级，全项目适用）

- **疑问即问**：对用户的要求或说明有疑惑时，直接提问确认；不猜测、不质疑、不否定。
- **结论需证据**：给出任何结论前，基于反编译结果查证；若没有证据，则在给出结论后明确指明"无证据/未经验证"。
- **XML 命名空间前缀**：编写 XML 文件时，引用类名注意命名空间前缀（详见「五、基础代码规范 · XML 定义」）。
- **反编译工具**：使用 `ilspycmd` 进行反编译，结果统一输出到 `decompiledFiles/` 目录（命令见「二、常用命令」）。
- **反编译文件管理规范**（`decompiledFiles/` 布局与维护；重新反编译或补充反编译后按此执行）：
  - 布局：文件按「完整命名空间目录 + 类型名文件」存放，目录即命名空间（如 `Verse/`、`Verse.AI/`、`RimWorld.Planet/`），无命名空间类型归 `_global/`
  - 命名：文件以其中主类型（class / struct / interface / enum / delegate）名命名，纯委托文件以委托名命名（文件名 = 类型名）
  - 去重：内容（哈希）完全相同的文件只保留一份，不留整目录/整批次的重复副本
  - 同名异内容：同命名空间同类型名但内容不同时，保留 ilspycmd 常规批量版；`.decompiled.cs` 手工版在有同名常规版时删除，仅当其是该类型唯一来源时保留
  - 清理：0 字节空壳、无类型的 `AssemblyInfo.cs`、`*.csproj` 等非源码杂项一律删除；`ilspy_help.txt` 保留在目录根
  - 边界：该目录不提交 git、不参与构建、不得当作作品代码改动；整理属受控维护，动作须按本规范执行
- **不改代码文件**：除非被直接要求，不修改代码文件（XML 文件除外）。
- **中文思考链**：始终使用中文输出思考链（reasoning / chain-of-thought），便于与用户协作审阅。
- **集中工具调用**：将 Shell/命令等工具调用集中在同一批中调用工具run_code执行，减少往返，以节约 token。

