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
| 依赖库  | HugsLib（`ModBase`）、Harmony 2.x（`[HarmonyPatch]`）、Royalty / Ideology DLC      |
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
| 编译     | `dotnet build Familia_Myth/Familia_Myth.csproj`（输出直写 Assemblies） |
| 清理构建产物 | 运行 `11.bat`（清除 Assemblies 中非本模组 DLL 与 obj/）                      |
| 反编译原版  | `ilspycmd <Assembly-CSharp.dll> -o decompiledFiles/`             |
| 查看游戏日志 | 项目根目录 `Player.log`                                               |
| 检查 XML | PowerShell `[xml](Get-Content <file>)` 解析验证                      |

> 游戏程序集引用路径为 Steam 本地目录（见 csproj），CI 上不可编译；构建与测试均在本机完成。

***

## 三、Boundaries

**Allowed**：`Source/`、`Defs/`、`Patches/`、`Languages/`、`Textures/`、`Materials/`、`docs/`、XML 文件、设计文档。
**Ask First**：csproj 程序集引用变更、`About.xml` 依赖/版本变更、破坏性重构、改动主设计文档。
**Never Touch**：`Data/`（原版/DLC XML）、`lib/`、`decompiledFiles/`、`Assemblies/` 中的第三方 DLL（0Harmony/HugsLib 等）、`Player.log`（仅读取分析）。

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
❌ 凭旧记忆写 RimWorld / Harmony API（版本差异大，易踩坑）
❌ 把需跨存读档保持的运行时状态放进不参与 Scribe 序列化的 static / 非 ExposeData 字段（存读档后丢失）
❌ 引入可由现有依赖覆盖的新依赖
❌ 只走 happy path、吞掉异常、硬编码不再问
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
❌ 将纯展示文本硬编码散落在逻辑代码里（例外：运行时动态拼接）
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
| 1  | 凭记忆写 RimWorld/Harmony API，方法/字段不存在 | 先 ilspycmd 反编译核对（如 `SetDuration` 不存在，应直接设 `ticksToDisappear`） |
| 2  | Harmony 补丁方法非 static / 签名不匹配       | 保持 static，重载写明参数类型                                            |
| 3  | 补丁无 try-catch，一处异常拖垮游戏             | 包裹 try-catch + Log.Error                                      |
| 4  | Mote/特效资源泄漏（纹理/相机/材质）              | try/finally 销毁；DrawAt 中重查贴图占位                                 |
| 5  | 随机抽取死循环                            | 权重耗尽判断（`tt > 0`），可抽项为空即返回                                     |
| 6  | 迭代中修改集合                            | 快照 / 反向遍历                                                     |
| 7  | Hediff severity=0 立即被移除            | `initialSeverity` 置正                                          |
| 8  | XML 类名缺命名空间前缀 / defName 冲突         | 带前缀、全局唯一                                                      |
| 9  | 魔法数字无出处                            | 抽 const 并对应设计文档数值                                             |
| 10 | 在 Patch 中访问未初始化 Find 单例            | 判空 + 阶段判断                                                     |

***

## 九、模块速查表

| 目录/前缀                                       | 职责                      |
| ------------------------------------------- | ----------------------- |
| `Patch_*.cs`                                | Harmony 补丁（成长、意志、触发类效果） |
| `Spell*.cs` + `SpellDef.cs`                 | 魔法框架与具体魔法（继承原版 Ability） |
| `Skill*.cs` + `SkillDef.cs`                 | 技能（主动/被动/触发；charge 充能 + active 效果生命周期） |
| `Hediff_SkillState` / `HediffComp_SkillState` / `HediffComp_SkillInEffect` | 技能状态指示（skillDef 反查 StateText；effect 生命周期间回调） |
| `DevelopableAbility*.cs`                    | 发展能力（熟练度 + StatPart 注入） |
| `Mote_*.cs`                                 | 地图特效（咏唱词/魔法阵/光环/斩击等）    |
| `Projectile_*` + `MagicMissileStates.cs`    | 魔法弹与运动状态机               |
| `Ritual*`                                   | 仪式（给予/升华恩惠）及结果处理        |
| `GuildQuest*` / `QuestNode*` / `QuestPart*` | 公会任务生成与奖励               |
| `GameComponent_*.cs`                        | 存档级追踪（眷族评级、使者）          |
| `JobDriver_*`                               | 工作驱动（对话使者等）             |
| `MentalState_*` / `Hediff_*` / `Verb_*`     | 精神状态、词缀、咏唱 verb         |
| `UI_*.cs`                                   | 界面（眷族页、升级奖励弹窗、雷达图）      |
| `StatPart_*`                                | StatDef 注入修正            |
| `Defs/` + `Patches/`                        | XML 定义与条件补丁             |

> 新增模块/文件时同步更新本表。

***

## 十、文档维护

- **维护范围**：文档维护只涉及 `docs/` 目录下的文件与根目录 `AGENTS.md`；`CLAUDE.md`、`.cursor/rules/project.mdc` 等规则引用文件以及其他根目录文档均不做主动维护。
- **不加时间戳**：维护上述文档时，不在文件头部添加或更新"更新: 日期"之类的时间戳。
- **主设定文档仅用户修改**：`Familia Myth rim.txt` 及其配套文档（`能力值效果.txt`、`能力值增长.txt`、`技能.txt`、`发展能力.txt`、`魔法.txt`、`魔法.xlsx`、`pointPreview.xlsx` 等）为权威设定来源，**仅由用户修改**；Agent 只能读取参考，不得改动或代写。

***

## 十一、上下文管理

- **Agent 主动维护本文件** — 在代码/XML被修改并经审阅后，依据修改后的实现进行文档维护。发现有 文档与项目实际实现/内容有不符 时，向用户提问确认后进行修改。每次完成以下操作时同步更新：
  | 操作                     | 更新内容                                    |
  | ---------------------- | --------------------------------------- |
  | 新增模块/文件                | 更新「模块速查表」                               |
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
- **不改代码文件**：除非被直接要求，不修改代码文件（XML 文件除外）。
- **中文思考链**：始终使用中文输出思考链（reasoning / chain-of-thought），便于与用户协作审阅。

