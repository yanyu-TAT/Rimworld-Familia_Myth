# C-05 原版 API 简述

> 按系统分类收录原版 API 方法速查（反编译证据）。本文档将持续扩充：新分析的系统按「X、系统名」追加章节即可（如当前已有 Quest / Pawn / 远行队 / 穿梭机等章节），后续可再加入魔法、技能、UI 等其他系统的分析。
> 出处统一为 `kb/L0-证据/decompiled/1.6/`（项目内原目录已迁入 kb；ilspycmd 反编译）；**方法签名与行为以文档编写时反编译版本为准，动手前仍须按规范实时 ilspycmd 核对**，本文件不替代核对。

## 一、Quest 任务系统

### QuestPart / QuestPartActivable 生命周期
- `QuestPartActivable`：`inSignalEnable`/`inSignalDisable` 信号开关部件；**仅 Enabled 时**收到 `Notify_QuestSignalReceived` 与 `QuestPartTick()` 调用（每 tick 由 Quest 驱动，项目内按 ~250 tick 节流），`Enable/Disable/Complete/ProcessQuestSignal` 为虚方法可覆写。
- 信号 tag 规则：Quest 只接收 `Quest{id}.` 前缀或全局 tag；生成期用 `QuestGenUtility.HardcodedSignalWithQuestID(tag)` 固化同串，运行时 `Find.SignalManager.SendSignal(new Signal(同串))` 即可送达（含在 `site.AllEnemiesDefeated` 等原版生成信号上拼接）。
- `QuestManager.Notify_PawnKilled(Pawn, DamageInfo?)`：任意 pawn 被杀广播给所有 Ongoing 任务的全部部件，部件自行过滤关注对象（全灭结算等）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/QuestPart.cs`、`RimWorld/QuestPartActivable.cs`、`RimWorld/QuestUtility.cs`。

### QuestPart_ExtraFaction（「原派系」标记，声明式）
- 字段：`extraFaction`（`ExtraFaction { Faction faction; ExtraFactionType factionType; }`）、`affectedPawns`、`areHelpers`、`inSignalRemovePawn`/`inSignalsRemovePawn`。
- **声明式语义**：extra faction 状态不存于 pawn，运行时由 `QuestUtility.GetExtraFactionsFromQuestParts` 从 **Ongoing 任务**的部件反查——部件加进任务即生效（与部件是否 Enabled 无关），`quest.End` 清理后失效。`Notify_FactionRemoved` 清空 affectedPawns。
- `Notify_PawnKilled`：若成员 `HomeFaction != null && Faction != HomeFaction` → 自动 `SetFaction(HomeFaction)`（成员在玩家派系期间死亡，尸体自动归还，防「白送人口」）。
- `inSignalRemovePawn`：收到信号（`SUBJECT` 参数为该 pawn）时将其移出 affectedPawns 并 `Notify_MemberLeftExtraFaction`。
- `Cleanup()/Disable()`：对 affectedPawns 设 30 天 `SetNoAidRelationsGainUntilTick`。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/QuestPart_ExtraFaction.cs`、`RimWorld/ExtraFaction.cs`。

### QuestUtility 查询扩展（lodger / helper 判定）
- `IsQuestLodger(this Pawn p)`：`HasExtraHomeFaction() || HasExtraMiniFaction()` → true（带 Home/Mini 标记即 lodger）。
- `IsQuestHelper(this Pawn p)`：先 lodger，再查存在 Ongoing 任务中 `QuestPart_ExtraFaction { areHelpers: true }` 且 affectedPawns 含该 pawn（王室帮手）。
- `GetExtraHomeFaction(p, Quest forQuest = null)` / `GetExtraHostFaction` / `GetExtraMiniFaction`、`HasExtraHomeFaction(p, Faction)` 等：按类型/派系反查 extra faction。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/QuestUtility.cs`（IsQuestLodger ≈L524、IsQuestHelper ≈L533、Get/Has 系列 ≈L616-671）。

### QuestPart_JoinPlayer（难民「加入玩家」部件）
- 字段：`pawns`、`joinPlayer`、`makePrisoners`、`inSignal`/`outSignalResult`。
- `joinPlayer=true` 时：已是殖民者/囚犯/奴隶走 `RecruitUtility.Recruit(pawn, Faction.OfPlayer)`；否则 `pawn.SetFaction(Faction.OfPlayer)`（与手动 SetFaction 等价）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/QuestPart_JoinPlayer.cs`。

### 离场归还：QuestPart_Leave / LeaveQuestPartUtility
- `MakePawnLeave(Pawn pawn, Quest quest)`：若在 caravan → 先 `CaravanInventoryUtility.MoveAllInventoryToSomeoneElse` + `caravan.RemovePawn(pawn)`；再选归还派系（extra home → extra mini → SlaveFaction → 随机非敌对人类派系兜底）并 `SetFaction`；清 guest / 手中物 / 通知 lord（`Notify_PawnLost(ForcedByQuest)`）。
- `MakePawnsLeave(IEnumerable<Pawn>, bool sendLetter, Quest quest, bool wakeUp=false)`：批量——发「离开」信 → 逐人 MakePawnLeave → 对存活站立者统一发 `LordJob_ExitMapBest` 走出地图。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/LeaveQuestPartUtility.cs`、`RimWorld/QuestPart_Leave.cs`。

### QuestPart_ExtraFaction 的移除信号实为死代码（必须显式 Enable）
- `QuestPart_ExtraFaction : QuestPartActivable`，而 `QuestPartActivable.Notify_QuestSignalReceived` **只在 `state == Enabled` 时才调 `ProcessQuestSignal`**（`QuestPartActivable.cs:219-236`）。
- 原版**所有**调用点（`QuestGen_Factions.ExtraFaction` L22-33、`QuestNode_ExtraFaction` L40-47、原版 XML 共 7 处）**均未设 `inSignalEnable`** → 部件恒为 `NeverEnabled` → `inSignalRemovePawn / inSignalsRemovePawn` **永不生效**。
- 旁证：`QuestUtility.GetExtraFactionsFromQuestParts` 对 `Hospitality_Refugee` 有硬编码特判（任务已结束但 `!pawn.IsColonist` 时仍保留标记，L715）。
- **用法结论**：要让「成员脱离 lodger 标记」真的生效，必须显式设 `inSignalEnable`（本项目用任务接取信号 `quest.InitiateSignal`）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/QuestPartActivable.cs`、`RimWorld/QuestPart_ExtraFaction.cs`、`RimWorld.QuestGen/QuestGen_Factions.cs`、`RimWorld.QuestGen/QuestNode_ExtraFaction.cs`、`RimWorld/QuestUtility.cs`（L706-732）。

### 劝加入（PawnJoinOffer + ChoiceLetter_AcceptVisitors）
- `quest.PawnJoinOffer(pawn, letterLabel, letterTitle, letterText, accepted, rejected, inSignal, outSignalPawnAccepted, outSignalPawnRejected, charity, sendLetterOnEnable)`（`QuestGen_Misc.cs:335`）→ `QuestPart_PawnJoinOffer`：Enable 后每 2500 tick 判「心情 ≥0.5」且 `Rand.MTBEventOccurs(曲线(0.5→60天, 1.0→15天), 60000, 2500)` → 发 `LetterDefOf.AcceptVisitors` 抉择信，并置 `pawn.guest.Recruitable`。
- `outSignalPawnAccepted` 缺省自动生成，可从返回的部件读取后作为自己的入信号使用。
- `ChoiceLetter_AcceptVisitors`：点接受时对每个仍可接受者发 `SendQuestTargetSignals(pawn.questTags, "Recruited")`，再广播 `acceptedSignal`（`SUBJECT` = 单个 pawn 或 List）；`CanStillAccept` 排除已销毁/未生成/正在离图/敌对/处于 ExitMap toil 者。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld.QuestGen/QuestGen_Misc.cs`（≈L335）、`RimWorld/QuestPart_PawnJoinOffer.cs`、`RimWorld/ChoiceLetter_AcceptVisitors.cs`。

### QuestPart_AddQuestRefugeeDelayedReward 的隐式前置（缺 rewardGiver 会炸文本解析）
- `GetSlate()` 从 `lodgers` 里挑第一名「`!Dead && Faction != Faction.OfPlayer && !IsPrisoner`」者作 `rewardGiver`；**一个都没有时它照样生成子任务**，随后在 `QuestGen.Generate → QuestNode_ResolveTextRequests.Resolve` 抛 `Grammar unresolvable`（轨迹中 `rewardGiver_nameDef/objective/pronoun/possessive` 全 UNRESOLVABLE，之后附带「Failed to resolve text. Trying again with English.」）。
- 易触发场景：结算当拍所有候选仍在**玩家派系**（例：登机者未归还原派系、或全员被玩家收留）。
- 规避：结算当拍先完成归位；或自建子类覆写 `CanAdd` 做同条件预检（`QuestPart_AddQuest.Notify_QuestSignalReceived` 的 `&& CanAdd` 会拦住生成）。详见 B-04 BUG-018。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/QuestPart_AddQuestRefugeeDelayedReward.cs`（L19-33）、`RimWorld/QuestPart_AddQuest.cs`（L21-40）。

### 生成期 inSignal 缺省取 `slate["inSignal"]`（根层为空 → 部件永不触发）
- QuestGen 扩展（`PawnsArrive` / `SetAllApparelLocked` / `Letter` / `AddMemoryThought` / `PawnJoinOffer` …）普遍写法为 `inSignal = HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal")`；而**根层 slate 没有 `inSignal` 变量**（`Slate.Get` 找不到 key 时静默返回 default，不报错）→ 在根节点/装配节点里未显式传信号时，部件将**永不触发**。
- **用法结论**：一律显式传 `QuestGen.quest.InitiateSignal`（形如 `Quest{id}.Initiate`，`HardcodedSignalWithQuestID` 对该形式原样返回）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld.QuestGen/Slate.cs`（L36-61）、`RimWorld.QuestGen/QuestGen_Misc.cs`、`RimWorld/Quest.cs`（L123）。

### QuestNode_Signal + RunInnerNode（util「生成期就绪、信号到达才生效」的原理）
- `QuestNode_Signal.RunInt`：建 `QuestPart_Pass{ inSignal = 外层信号, outSignal = OuterNodeCompleted }`，并 `QuestGenUtility.RunInnerNode(node, outSignal)` → **内层节点在生成期执行**，但其创建的 part 全部挂在 `OuterNodeCompleted`（内层信号）上，外层信号到达时才真正生效。
- 由此可解释两件事：① `Util_TransportShip_Pickup` 之后的节点能在生成期读到 `slate["pickupShipThing"]`；② 该 util 的船设置/降落 job 均由内层信号驱动 → **不发外层信号，穿梭机永不降落**。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld.QuestGen/QuestNode_Signal.cs`（L34-75）、`RimWorld.QuestGen/QuestNode_GenerateTransportShip.cs`、`RimWorld.QuestGen/QuestGenUtility.cs`。

## 二、Pawn 与派系成员

### Pawn.SetFaction（加入/离开玩家的核心入口）
`public override void SetFaction(Faction newFaction, Pawn recruiter = null)`（`Verse/Pawn.cs`），自动附带：
1. 目标派系相同 → Log.Warning 并直接返回；
2. `guest?.SetGuestStatus(null)`；
3. Spawned：地图 MapPawns 注销/注册、清目的地预约、清 designation、autoSlaughter 通知；
4. 进出玩家派系（Playing 态）→ `Find.ColonistBar.MarkColonistsDirty()`（头像栏刷新）；
5. `this.GetLord()?.Notify_PawnLost(this, PawnLostCondition.ChangedFaction)`（**当前 lord 自动释放**）；
6. `newFaction == 玩家 && Humanlike && !IsQuestLodger()` → `ChangeKind(newFaction.def.basicMemberKind)`（**已是 lodger 时跳过 kind 重设**——普通成员无实质影响）；
7. 入玩家派系 → `workSettings?.EnableAndInitialize()`、PopAdaptation `GainedColonist`、人口上限/记录更新；离玩家对称处理；
8. `Drafted` 复位、`ClearMind_NewTemp(ifLayingKeepLaying: true)`、手术单清空、名字补全、`playerSettings?.ResetMedicalCare/Notify_FactionChanged`、关系/思潮通知等。
- 结论：**只要 pawn 真进玩家派系，头像栏/指挥/工作组件由本方法链路补齐**（难民即走此路）。

### Pawn.Notify_PassedToWorld（离图进世界的自动归位兜底）
- 条件：`(Faction == null && 人类) || Faction.IsPlayer || Ancients` 且非 Dead、世界处境为 Free。
- 行为：带**非玩家 Home extra faction** → `SetFaction(该 HomeFaction)`；否则随机人类非殖民地派系（按科技等级）；再不行 `SetFaction(null)`。
- 意义：带「原派系」标记的临时成员即使没被手动归还，离开地图进世界时也会自动归还原派系。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/Pawn.cs`（≈L1838）。

### ExtraFactionType 与「原派系」渲染
- `ExtraFactionType`：`HomeFaction`（原属派系，王室帮手/本项目救援用）/ `HostFaction`（寄主派系）/ `MiniFaction`（难民本体用）。
- 渲染：角色卡经 `Pawn.FactionDesc` + `CharacterCardUtility` 显示 extra faction 项，标签由 `ExtraFactionTypeExt.GetLabel`，tooltip `ExtraFactionDesc`（「{PAWN_labelShort} 原本所属的派系」）。玩家侧只有玩家成员才看得到此栏。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/Pawn.cs`、`RimWorld/CharacterCardUtility.cs`。

### 殖民者栏成员口径
- 头像栏 `ColonistBar` 只列 `mapPawns.FreeColonists`（玩家派系自由成员）+ 可控 subhuman + 殖民者尸体。**非玩家派系临时成员永远不上栏、不可命令**——「像殖民者一样出现」必须真实加入玩家派系。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/ColonistBar.cs`、`RimWorld/MapPawns.cs`。

### 囚禁 / 奴役判定与「逮捕自己的 lodger」
- `Pawn.IsPrisoner`（= `guest.IsPrisoner`）、`IsPrisonerOfColony`（+ `guest.HostFaction.IsPlayer`）、`IsSlave`、`IsSlaveOfColony`（+ `Faction.IsPlayer`）（`Verse/Pawn.cs:612-658`）。
- 玩家**可以**逮捕自己派系的 lodger：`FloatMenuOptionProvider_Arrest.cs:55` 特意为 `IsQuestLodger()` 放行 → 「临时加入玩家派系」的成员仍可被逮捕/奴役（任务可据此做失败判据）。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/Pawn.cs`、`RimWorld/FloatMenuOptionProvider_Arrest.cs`。

### 临时派系（难民）生成
- `FactionGenerator.NewGeneratedFactionWithRelations(new FactionGeneratorParms(FactionDefOf.OutlanderRefugee, ideoParms, hidden: true), relations)`；`FactionGeneratorParms` 是主构造函数 struct（`factionDef / ideoGenerationParms / hidden`）。随后 `faction.temporary = true` + `Find.FactionManager.Add(faction)` + `quest.SetFactionHidden(faction)`（→ `QuestPart_SetFactionHidden`）＝ 原版难民事件的全套做法。
- `hidden: true` 时不建定居点、不生成领袖以外内容；`FactionDefOf.OutlanderRefugee` 属 Royalty，未激活时为 null，需兜底（`Beggars` / `OutlanderCivil`）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/FactionGenerator.cs`（L125-216）、`RimWorld/FactionGeneratorParms.cs`、`RimWorld/Faction.cs`（L56）、`RimWorld.QuestGen/QuestGen_Factions.cs`（L70）。

### Pawn.HomeFaction 与「随机受伤」
- `Pawn.HomeFaction => GetExtraHomeFaction() ?? Faction`（`Verse/Pawn.cs:1064-1078`）→ 只带 `MiniFaction` 的 lodger，其 `HomeFaction` 即**当前派系**（恒非 null；`RecipeWorker.ReportViolation` 的 `factionToInform != null` 因此成立，手术侵害信号才会发）。
- 随机受伤：`HealthUtility.RandomViolenceDamageType()` 取伤害类型 → `new DamageInfo(def, amount)`（不传 `hitPart` 时由引擎随机选部位，`Verse/DamageInfo.cs:144`）→ `Thing.TakeDamage(dinfo)`（`Verse/Thing.cs:1477`）；致倒地用 `HealthUtility.DamageUntilDowned(pawn, allowBleedingWounds: false)`（`Verse/HealthUtility.cs:245`）。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/Pawn.cs`、`Verse/HealthUtility.cs`、`Verse/DamageInfo.cs`、`Verse/Thing.cs`。

## 三、远行队与运输装载的候选过滤

### CaravanFormingUtility.AllSendablePawns
```
public static List<Pawn> AllSendablePawns(Map map,
    bool allowEvenIfDowned = false, bool allowEvenIfInMentalState = false,
    bool allowEvenIfPrisonerNotSecure = false, bool allowCapturableDownedPawns = false,
    bool allowLodgers = false, int allowLoadAndEnterTransportersLordForGroupID = -1)
```
- 候选条件含：`Faction == 玩家 | 囚犯 | (可自动俘获的倒地者)`、`RaceProps.allowedOnCaravan`、**`!IsQuestHelper()`（无条件排除）**、**`(!IsQuestLodger() || allowLodgers)`（lodger 默认排除）**、lord 可 caravan 发送等。
- 调用方 allowLodgers 传值：
  - `Dialog_FormCaravan.AllSendablePawns(map, reform)` → `(map, allowEvenIfDowned: true, reform, reform, reform)`：**allowLodgers 恒 false**（玩家手动组远行队，lodger/helper 全不可入）；
  - `TransporterUtility.AllSendablePawns(transporters, map)` → `allowLodgers = shuttle != null`：装**穿梭机**放行 lodger，装普通运输舱不放行；
  - `Dialog_EnterPortal`：`allowLodgers: true`（传送门）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld.Planet/CaravanFormingUtility.cs`（≈L264）、`RimWorld/Dialog_FormCaravan.cs`（≈L1532）、`RimWorld/TransporterUtility.cs`（≈L63）、`RimWorld/Dialog_EnterPortal.cs`。

## 四、穿梭机（CompShuttle）登机

### IsAllowed 过滤链
```
public virtual bool IsAllowed(Thing t)
```
1. 该船 `shipDef.playerShuttle`（玩家专属穿梭机 def）→ 全放行；
2. `IsRequired(t)`（在 `requiredPawns`/`requiredItems` 名单）→ **无条件放行**（任务名单通道）；
3. `acceptColonists` 通道：`IsColonist | (囚犯 && acceptColonyPrisoners) | (!onlyAcceptColonists && 动物 && 玩家派系)` 且 `(!奴隶 || allowSlaves)` 且 `(!onlyAcceptColonists || !IsQuestLodger())` 且健康/年龄门槛；
4. `permitShuttle` 通道：`Faction == 玩家 && !IsQuestLodger() | 囚犯`；
5. 非 pawn 物品：非玩家家图且无活跃敌对威胁 → 可装。
- **用法结论**：要「只有任务名单者可登机」= `requiredPawns` 精确名单 + `acceptColonists=false`（关闭搭便车通道）；required 成员（含倒地、lodger）不受任何附加过滤。
- `IsAllowedNow`：叠加 `maxColonistCount` 限制。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/CompShuttle.cs`（IsAllowed ≈L722、IsAllowedNow ≈L759、字段 ≈L33/L581）。

### 信号与归还时机窗口
- `ShipJob_Arrive.TryStart` 广播 `Arrived`；`ShipJob_FlyAway.TryStart` 先做未满载处理（dropMode 卸载），随后 `SendLaunchedSignals` 广播 **`SentSatisfied`/`SentUnsatisfied`**，contents 才移入离场 skyfaller。
- **窗口**：收到 `SentSatisfied/SentUnsatisfied` 信号当拍，成员仍在穿梭机内容器（未转移）——在此处 `SetFaction(原派系)` 归还，成员即以原派系身份随船离场（本项目结算归还点）。
- **信号收发方不对称（易踩坑）**：穿梭机相关信号按广播方分两类——
  - `SentSatisfied/SentUnsatisfied/SentWithExtraColonists`（`CompShuttle.SendLaunchedSignals`）与 `Spawned/Despawned/Destroyed/Killed/LeftBehind`（Thing 通用）由**穿梭机 thing**（`parent.questTags`）广播；thing 的 questTags 会被生成期「引用过该变量信号的 `HardcodedSignalWithQuestID`」自动登记（slate 变量名即 tag 名），可靠可达；
  - `Arrived/FlewAway`（`ShipJob_Arrive/FlyAway`）由 **TransportShip.questTags** 广播；`QuestNode_GenerateTransportShip` 与 `Util_TransportShip_Pickup` **不会给 transportShip 打 tag**（vanilla 的 Mission/Bestowing 是 root 手动对 thing 与 transportShip 各 `AddQuestTag` 一次）→ util 生成场景下 **`Arrived` 静默无接收者**，vanilla 所有 util 使用者都不监听 `Arrived`，「穿梭机已抵达」改用 thing 的 `Spawned`（Skyfaller 落地放出穿梭机时广播）作为到达信号。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/ShipJob_Arrive.cs`（L65）、`RimWorld/ShipJob_FlyAway.cs`（L47）、`RimWorld/CompShuttle.cs`（SendLaunchedSignals ≈L543）、`RimWorld/Thing.cs`（Spawned ≈L914）、`RimWorld.QuestGen/QuestGenUtility.cs`（自动打 tag ≈L50）、`RimWorld.QuestGen/QuestNode_GenerateTransportShip.cs`。

### 生成参数（QuestGen_Shuttle.GenerateShuttle）
```
GenerateShuttle(Faction owningFaction = null, IEnumerable<Pawn> requiredPawns = null,
    IEnumerable<ThingDefCount> requiredItems = null, bool acceptColonists = false,
    bool onlyAcceptColonists = false, bool onlyAcceptHealthy = false, int requireColonistCount = -1,
    bool dropEverythingIfUnsatisfied = false, bool leaveImmediatelyWhenSatisfied = false,
    bool dropEverythingOnArrival = false, bool stayAfterDroppedEverythingOnArrival = false,
    WorldObject missionShuttleTarget = null, WorldObject missionShuttleHome = null,
    int maxColonistCount = -1, ThingDef shuttleDef = null, bool permitShuttle = false,
    bool hideControls = true, bool allowSlaves = false,
    bool requireAllColonistsOnMap = false, bool acceptColonyPrisoners = false)
```
- 默认 `acceptColonists=false`；SubScript util（如 `Util_TransportShip_Pickup`）经 parms → slate → 节点字段传入。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld.QuestGen/QuestGen_Shuttle.cs`、`RimWorld.QuestGen/QuestNode_GenerateShuttle.cs`。

### 装载机制（物品/被抬者共用；任务 util 船需自行 InitiateLoading）——B-04 BUG-013/015
- **双层 comp**：穿梭机 thing = `CompShuttle : ThingComp`（登机/起飞/autoload/playerShuttle 判定）+ 独立 `CompTransporter`（货舱 innerContainer/leftToLoad/装载状态）；`CompShuttle.Transporter => parent.GetComp<CompTransporter>()`（CompShuttle.cs ≈L115）。
- **requiredItems/requiredPawns 只用于两件事**：`IsRequired/IsAllowed` 放行校验 + 起飞 `satisfied` 判定（innerContainer 计数）；**不驱动装载**。
- **装载前置 = `TransporterUtility.InitiateLoading`**：`LoadingInProgressOrReadyToLaunch` 在 `groupID<0` 时仅 `parent.IsInCaravan()`；InitiateLoading 分配 groupID 后才可装载。谁触发：`playerShuttle` 且舱内有殖民者（CompShuttle.CompTick ≈L495）、autoload 开关（≈L390）、玩家 gizmo/载人浮标（≈L418）。**util 任务船（hideControls、非 playerShuttle、autoload 关）三者皆不触发 → 默认不可装载**。
- **`JobDriver_HaulToContainer`（物品 haul 与「抱囚犯」`FloatMenuOptionProvider_CarryToShuttle` 共用）**：目标 A=货/被抬者、B=穿梭机；全局 `FailOn(TransporterUtility.WasLoadingCanceled(Container))`（= 船未可装载）→ 未 InitiateLoading 时任何 HaulToTransporter 首帧秒败 → 若 toLoad 恒有货则死循环刷 "started N jobs in one tick"。样板 `CarryToShuttle` 派 job 前先 `InitiateLoading`。
- 项目落地：部件内穿梭机落地后幂等 `InitiateLoading` + 幂等把 required 物加入 toLoad（不清空），即与抱囚犯同机制。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/CompShuttle.cs`、`RimWorld/CompTransporter.cs`、`RimWorld/TransporterUtility.cs`（WasLoadingCanceled ≈L43）、`RimWorld/FloatMenuOptionProvider_CarryToShuttle.cs`（L48-55）、`kb/L0-证据/decompiled/1.6/Verse.AI/JobDriver_HaulToContainer.cs`（FailOn ≈L134）。

### 自建物品 def 可交互/可搬运/可入库的前置字段（B-04 BUG-012）
- `selectable` **默认 false**（不开则可见但鼠标点不中）；`EverHaulable = alwaysHaulable || designateHaulable` 默认 false（不开则不可 haul）；`drawGUIOverlay`、`Mass`（haulable 需 mass）。
- **`ThingDef.EverStorable` 要求 `thingCategories` 非空**（category=Item 不够）→ 否则存储区 filter 的 `disallowNotEverStorable` 恒拒 → 即便"全部允许"也提示无存储点、无法入库（EverStorable 也影响装仓 haul）。
- 原版物品基类 `ResourceBase`（Core Items_Resource_Base.xml）显式开上述字段；自建 def 不继承时须逐项补齐（或挂既有 thingCategory，如 Artifacts）。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/ThingDef.cs`（EverStorable ≈L1394）、`RimWorld/StorageSettings.cs`（AllowedToAccept ≈L97）、`RimWorld/Tradeability` 枚举（无 Never，禁卖用 None）。

### requiredPawns 可运行时增删（动态登机名单）
- `CompShuttle.requiredPawns` 是 **public `List<Pawn>`**（`CompShuttle.cs:17`）→ 任务部件可在运行时 `Remove`。
- 起飞达标判定 `AllRequiredThingsLoaded`（L179）：逐个 `RequiredPawns` 检查是否在 `innerContainer`；而 `RequiredPawns`（L245）会剔除「已死」与「倒地且不在本图且在 `pawnsToIgnoreIfDownedOfNotOnTheMap` 名单」者（**单纯倒地不会自动放行**）。
- `SendLaunchedSignals`（L543）：`AllRequiredThingsLoaded ? "SentSatisfied" : "SentUnsatisfied"`，另对「非 required 的殖民者」广播 `SentWithExtraColonists`。
- **用法结论**：把「已被玩家收留 / 不再需要登机」的对象从 `requiredPawns` 移除，即可不再计入起飞达标；名单为空时落地即满足 → 若配 `leaveImmediatelyWhenSatisfied` 会立刻起飞（因此「全员已被收留」应直接结算、不再召船）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/CompShuttle.cs`、`RimWorld/ShipJob_FlyAway.cs`。

### Util_TransportShip_Pickup 的内部结构（无 Letter、全部挂内层信号）
- 节点序列：`QuestNode_GenerateShuttle(storeAs=pickupShipThing)` → `QuestNode_GenerateTransportShip(storeAs=pickUpShip)` → `AddShipJob_Arrive` → `AddShipJob_Wait(ticks=$leaveDelayTicks, leaveImmediatelyWhenSatisfied, sendAwayIfAllDespawned)` → `AddShipJob_FlyAway` → `SendTransportShipAwayOnCleanup`。
- **整个 util 不含任何 Letter 节点** → "穿梭机已抵达"一类信件必须由使用方自己监听 `pickupShipThing.Spawned` 发出。
- 出处：`Data/Royalty/Defs/QuestScriptDefs/Scripts_Utility_TransportShip.xml`（L59-110）。

## 五、Lord 与离图安排

- **lord 释放路径**：`Pawn.SetFaction` 内部即 `GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ChangedFaction)`；也可手动 `pawn.Map.lordManager.RemoveLord(pawn.GetLord())`（单成员 defend lord）。
- `LordJob_DefendPoint(cell, …, isCaravanSendable: false, addFleeToil: false)`：营地小范围踱步/自卫、禁逃跑离图，用于把「无人接管的原派系 NPC」钉在站点（本项目进图 → 加入玩家派系当拍释放，之后不再发放）。
- `LordJob_ExitMapBest(LocomotionUrgency, canDig, canDefendSelf)`：安排批量 pawn 走出地图（滞留者离场用）。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/Pawn.cs`（≈L2695）、`RimWorld.Planet` / `Verse.AI.Group` 下 LordJob 系列。

## 六、站点威胁生成（狂暴兽群）

- `AggressiveAnimalIncidentUtility.TryFindAggressiveAnimalKind(float points, PlanetTile tile, out PawnKindDef animalKind)`：给定威胁点能否选出狂暴动物（含 `PlanetTile.Invalid` 全局兜底预检）。
- **点数下限**：威胁点低于最弱候选 `combatPower × 2` 时返回空 → 站点 `SitePartWorker_Manhunters` 生成拿不到 animalKind（`GetAnimalsCount`）会 NRE。本项目以 `MinManhunterThreat` 抬升点数兜底（不用 Outpost 备选）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/AggressiveAnimalIncidentUtility.cs`、`RimWorld/SitePartWorker_Manhunters.cs`、`RimWorld/ManhunterPackGenStepUtility.cs`。

## 七、世界对象交互 / 远行队伏击 / 任务信号

### WorldObjectComp：世界对象的交互能力容器
- `WorldObject.comps` 在 `PostMake()` 与 `ExposeData` 的 `LoadingVars` 分支经 `InitializeComps()` **从 `def.comps` 逐个 `Activator.CreateInstance(compClass)` 重建**（读档不反序列化列表本身）→ 后期往 `WorldObjectDef.comps` 注入的自定义 comp **对已有存档同样出现**；各自状态由 `PostExposeData()` 存取。
- 交互入口：`GetGizmos()`（选中世界对象）、`GetCaravanGizmos(Caravan)`（**选中远行队**：`Caravan.GetGizmos` 遍历 `Find.WorldObjects.ObjectsAt(caravan.Tile)` 逐个调用）、`GetFloatMenuOptions(Caravan)`、`CompInspectStringExtra()`。
- 无实例级 AddComp API → 自定义 comp 必须声明在 def 级 `comps`；样板 = `TradeRequestComp` + `WorldObjectCompProperties_TradeRequest`（props 构造函数里 `compClass = typeof(...)`）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld.Planet/WorldObject.cs`（L263-306、L448-451、L627-636）、`RimWorld.Planet/WorldObjectComp.cs`、`RimWorld.Planet/Caravan.cs` L793-799、`RimWorld/WorldObjectCompProperties_TradeRequest.cs`。

### 远行队伏击（原版 Ambush）
- `IncidentDef "Ambush"`（targetTags = `Caravan` + `Map_TempIncident`），worker `IncidentWorker_Ambush_EnemyFaction`；`parms.target = Caravan` 时经 `CaravanIncidentUtility.SetupCaravanAttackMap` 现场生成临时地图（`WorldObjectDefOf.Ambush`，`blockExitGridUntilBattleIsWon=true` → 打赢才能离图）。
- **`parms.forced = true` 会让 `IncidentWorker.CanFireNow` 跳过全部检查**（`IncidentWorker.cs` L37 起）→ 想要"打之前确认可行"必须自己调 `CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(tile)`：该 tile 已有地图 / 生物群系未实现 / 被未开 `allowCaravanIncidentsWhichGenerateMap` 的世界对象占用时返回 false（原版仅 `Caravan` / `TravellingTransporters` / `RoutePlannerWaypoint` / `Ambush` 四个 def 开了该字段）。
- `WorldObjectsHolder.AllWorldObjects => worldObjects`（**不含 caravans**）→ 远行队不会挡住自己；已进图的队伍不在世界层，天然不是目标。
- 袭击者派系不可指定：`IncidentWorker_Ambush_EnemyFaction.GeneratePawns` 内 `TryGetRandomFactionForCombatPawnGroup(parms.points, out parms.faction)` 会覆盖外部值。
- 出处：`Data/Core/Defs/Storyteller/Incidents_Caravan_All.xml`、`kb/L0-证据/decompiled/1.6/RimWorld/IncidentWorker_Ambush.cs`、`IncidentWorker_Ambush_EnemyFaction.cs`、`RimWorld.Planet/CaravanIncidentUtility.cs`、`RimWorld/IncidentWorker.cs`、`RimWorld.Planet/WorldObjectsHolder.cs`。

### 任务 part 启用与信号固化
- `QuestPartActivable`：`Quest.QuestTick` **只 tick `State == Enabled` 的 part**（`Quest.cs` L272-289）；`inSignalEnable` 缺省则永不启用 → "任务一开始就要跑"的 part 填 `QuestGen.quest.InitiateSignal`（`QuestManager.Add` 中 part 全部添加完才 `PostAdded()` → `if (initiallyAccepted) quest.Initiate()`）。
- `QuestGenUtility.HardcodedSignalWithQuestID(signal)` **幂等**：已形如 `Quest*.xxx` 的串原样返回（L26-39）；`QuestNode_Signal` 用它转换 `<inSignal>`（L44），内层节点再由 `RunInnerNode` 把 `slate["inSignal"]` 设为 `OuterNodeCompleted` 串。
- `Dialog_MessageBox.CreateConfirmation(text, confirmedAct[, cancelAct])` → 按钮为「Confirm」/「GoBack」（`Verse/Dialog_MessageBox.cs` L50-60）；`Gizmo.Disabled` 是 **public 虚属性**（`disabled` 字段为 protected，不可直接赋值）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/Quest.cs`、`RimWorld/QuestManager.cs`、`RimWorld.QuestGen/QuestGenUtility.cs`、`RimWorld.QuestGen/QuestNode_Signal.cs`、`Verse/Dialog_MessageBox.cs`、`Verse/Gizmo.cs`。

### 定居点交易清单与清单载体
- `Settlement` 构造函数即 `trader = new Settlement_TraderTracker(this)`（`trader` 恒非 null）→ `Settlement.TraderKind` 可直接取 `TraderKindDef`；`traderKind.stockGenerators[i].GenerateThings(PlanetTile, Faction)` 为公开方法，可自由生成"该派系此地会卖的商品"（`ThingSetMaker_TraderStock` 即其官方封装，含 `TraderCanSell` 校验）。
- `ThingDefCountClass`（`Verse/`：`thingDef + count + stuff + quality`，`IExposable`）适合做任务清单载体（`Scribe_Collections.Look(..., LookMode.Deep)`）。
- `ThingDef.tradeability` 默认 `Tradeability.All`（`ThingDef.cs` L324），过滤入口 `TradeabilityUtility.TraderCanSell()`；块堆为 `None` 且 `category=Item` 无 `building` → 只靠可卖性即可排除。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld.Planet/Settlement.cs` L96/L141-144、`RimWorld/StockGenerator.cs` L36、`RimWorld/ThingSetMaker_TraderStock.cs`、`Verse/ThingDefCountClass.cs`、`Verse/ThingDef.cs` L324、`RimWorld/TradeabilityUtility.cs`。

## 八、地图内事件触发与交互信号

### 任务内发动地图袭击（IncidentDefOf.RaidEnemy）
- 调用：`IncidentDefOf.RaidEnemy.Worker.TryExecute(new IncidentParms { target = Map, points, faction, forced = true, quest, customLetterLabel/Text })`（`IncidentDefOf.cs:6`；def 见 `Data/Core/Defs/Storyteller/Incidents_Map_Threats.xml`，`targetTags = Map_PlayerHome`、`category = ThreatBig`）。
- `IncidentWorker_Raid.TryExecuteWorker`（`IncidentWorker_Raid.cs:133`）→ `TryGenerateRaidInfo`（L70）四步：
  1. `ResolveRaidPoints` **要求 `points > 0`**，否则 `Log.Error` 并回退默认点数（`IncidentWorker_RaidEnemy.cs:83-90`）；
  2. `TryResolveRaidFaction`：`parms.faction` 已设且**敌对**（且未 deactivated 或在 forced 下）→ 直接采用，否则随机重选（L58-73）；
  3. `ResolveRaidStrategy` 为空则按权重随机（全失败兜底 `ImmediateAttack`），`ResolveRaidArriveMode` 同理；
  4. `AdjustedRaidPoints`（L191-211）会在传入点数上**再乘** `raidArrivalMode / raidStrategy.pointsFactorCurve` 与图层系数并设策略下限 → **实机强度 ≠ 传入值**。
- **`TryExecute` 不经过 `CanFireNow`**（后者只在 Storyteller 触发路径被调用，`IncidentWorker.cs:35-154`）→ 「允许大威胁」等难度开关**必须自行判断**（`Find.Storyteller.difficulty.allowBigThreats`）；`parms.forced` 只作用于 `CanFireNow`，与本路径无关。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/IncidentWorker.cs`、`IncidentWorker_Raid.cs`、`IncidentWorker_RaidEnemy.cs`、`RimWorld/IncidentParms.cs`。

### 手术侵害信号（SurgeryViolation）
- 发出点 `RecipeWorker.ReportViolation`（`Verse/RecipeWorker.cs:67-74`）：条件 = `billDoer.Faction == Faction.OfPlayer` 且 `factionToInform != null`（= 被手术者的 `HomeFaction`，对 lodger 恒非 null）→ 发 `SendQuestTargetSignals(pawn.questTags, "SurgeryViolation", SUBJECT)`，并记 `HistoryEventDefOf.PerformedHarmfulSurgery`。
- 是否算违规：`IsViolationOnPawn` 基类在 `pawn.Faction == billDoerFaction && !pawn.IsQuestLodger()` 时返回 false，否则看 `recipe.isViolation`（`RecipeWorker.cs:30-37`）→ **对 lodger 一律进入违规判定**；摘除类覆写只把 `BodyPartRemovalIntent.Harvest`（摘器官）算违规，**截肢不算**（`Recipe_RemoveBodyPart.cs:35-46`）。调用点覆盖摘器官 / 拆植入物 / 装假肢 / 植异种胚 / 抽血原 / 灌药 / 终止妊娠 / 斩首处决等 12 处。
- 前置：被手术者的 `questTags` 上要有本任务 tag —— 生成期调用一次 `HardcodedSignalWithQuestID("victims.SurgeryViolation")` 就会自动登记名单 tag（`QuestGen.cs:329-374`）。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/RecipeWorker.cs`、`RimWorld/Recipe_RemoveBodyPart.cs`、`RimWorld/RecipeWorker.cs`、`RimWorld/ThoughtDefOf.cs`（`OtherTravelerSurgicallyViolated`）。

### 灵能仪式目标信号（PsychicRitualTarget）对 lodger 恒不可达
- 发出点 `PsychicRitualToil_TargetCleanup.Start`（L50，无条件广播；另在目标有非玩家 HomeFaction 时对该派系 goodwill -25 并记 `WasPsychicRitualTarget`）；来源仪式为 Anomaly 的 `Psychophagy / Philophagy / Chronophagy / Brainwipe / ImbueDeathRefusal`。
- 但 5 个 Target 角色的 `allowedConditions` 均为 `Default, Downed, Prisoner, Sleeping, IdeoUnwilling`（`Data/Anomaly/Defs/PsychicRitualDefs/PsychicRitualRoles.xml:41/50/59/68/77`），**无一声明 `TemporaryFactionMember`**；而 `PsychicRitualRoleDef.PawnCanDo` 规定 `!ConditionAllowed(Condition.TemporaryFactionMember) && pawn.IsQuestLodger()` → 拒绝（`ConditionAllowed` 是位掩码「豁免清单」，L209-212 / L439）→ **lodger 不可能被选为这些仪式的目标**（即便已被逮捕，lodger 标记仍在）。
- 出处：`kb/L0-证据/decompiled/1.6/Verse.AI.Group/PsychicRitualToil_TargetCleanup.cs`、`RimWorld/PsychicRitualRoleDef.cs`、`Data/Anomaly/Defs/PsychicRitualDefs/PsychicRitualRoles.xml`。

## 九、成长与数值系统

### Hediff_Level（等级型 Hediff 基类）
- `ChangeLevel(int levelOffset)`：`level = (int)Mathf.Clamp(level + levelOffset, def.minSeverity, def.maxSeverity)` → **等级上下限由 HediffDef 的 min/maxSeverity 决定**；本项目「恩惠」即以此把等级封在 Lv1~Lv10（`Hediff_Pharna.xml` 的 `<maxSeverity>10</maxSeverity>`）。
- `TickInterval(int delta)`：同步 `Severity = level`（故 severity 即等级）；`SetLevelTo(int)` 转调 `ChangeLevel`；`ShouldRemove => level == 0`。
- `PostAdd` 与读档 `PostLoadInit` 均要求 `Part != null`（否则 Log.Error，后者还会移除自身）→ 自定义 HediffDef 必须指定 `part`，本项目用躯干。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/Hediff_Level.cs`。
- `ExposeData`：`Scribe_Values.Look(ref level, "level", 0)`；`LoadSaveMode.PostLoadInit` 且 `Part == null` 时先 `Log.Error(GetType().Name + " has null part after loading.")` 再 `pawn.health.hediffSet.hediffs.Remove(this)` —— **直接改列表、不触发 `PostRemoved`**，故依赖 `PostRemoved` 的清理 / 通知逻辑在读档期收不到该次移除。

### Pawn_HealthTracker.AddHediff / HediffSet.AddDirect（失败是静默的）
- `AddHediff(HediffDef def, BodyPartRecord part = null, ...)` 内部 `HediffMaker.MakeHediff` → `AddHediff(hediff, part, ...)`，**无论内部是否真的加入都 `return hediff`**；`part == null` 时只按 `def.defaultInstallPart` 补，补不到则 `hediff.Part` 保持 null。
- `HediffSet.AddDirect` 的两种拒绝都是 `Log.Error` + `return`（无异常、无返回值）：`hediff.def == null`、`hediff.Part != null && !GetNotMissingParts().Contains(hediff.Part)`。
- 结论：判定「是否真的加上了」只能用 `hediffSet.hediffs.Contains(hediff)` 或重新 `GetFirstHediffOfDef`。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/Pawn_HealthTracker.cs`、`Verse/HediffSet.cs`。

### StorytellerUtility 威胁点数（原版难度基准）
- `DefaultSiteThreatPointsNow(IIncidentTarget)` = `PointsPerWealthCurve.Evaluate(财富)` + Σ`PointsPerColonistByWealthCurve.Evaluate(财富)`（每名自由殖民者；动物按 `0.08 × combatPower`、机械体另计）→ 由**全殖民地财富与人口**共同决定。
- 曲线值：`PointsPerWealthCurve`（0→0、14000→0、400000→2400、700000→3600、1000000→4200）；`PointsPerColonistByWealthCurve`（0→15、10000→15、400000→140、1000000→200）。
- 本项目任务难度即以此为基准（`QuestUtilities.CalculateTaskParams` 的 `points = DefaultSiteThreatPointsNow() × PharnaGrowthFactor()`）；**任务侧奖励（升级点数）与该点数解耦，伟业不参与任务结算**——点数只标定难度（见 C-03 数值平衡框架）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/StorytellerUtility.cs`。

### StatDef.forInformationOnly（「仅信息」标记）
- 定义：`Verse/StatDef.cs:18` 的 `public bool forInformationOnly;`。
- **全库唯一使用点**：`RimWorld/ScenPart_StatFactor.cs:31` —— `if (!allDef.forInformationOnly && allDef.CanShowWithLoadedMods())`，即**剧本编辑器**在列出「可设系数的 Stat」时将其过滤掉。
- **不影响**运行时取值、`StatPart` 的应用、信息卡显示。Stat 取值流程为：`StatWorker.GetValue` → `GetValueUnfinalized`（**子类可 override**，如 `StatWorker_MeleeArmorPenetration` 在此反算武器穿甲）→ `FinalizeValue`（**在此统一应用 `stat.parts`**）→ `PostProcessValue`/`postProcessCurve`。
- 原版惯例：用于「值由其他数据派生、玩家与剧本无法直接设定」的展示型 Stat —— `MeleeWeapon_AverageDPS`、`MeleeWeapon_AverageArmorPenetration`（`Stats_Weapons_Melee.xml`）、`MaxInstallCount`（`Stats_Misc.xml`）。
- 本项目用法：`Defs/StatDefs/PharnaStats.xml` 的 **19 个恩惠展示 Stat**（六项能力值、`ManaControl` 系列 9 个、`ManaMax` / `ManaConversion` / `ManaConversionSpeed`、经验值）全部标注 —— 它们的值由 `StatPart_PharnaDisplay`（展示项）/ `StatPart_Pharna`（系数注入）/ `StatPart_ManaControl`（魔力操控）从恩惠 Hediff 派生，玩家与剧本都不应直接设定。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/StatDef.cs`、`RimWorld/ScenPart_StatFactor.cs`、`RimWorld/StatWorker.cs`、`RimWorld/StatWorker_MeleeArmorPenetration.cs`。

### HediffStage 的数值修正（statOffsets / statFactors / 饥饿与休息速率）

- `HediffStage` 是 hediff 阶段的**声明式修正表**，一个 stage 可同时挂：`statOffsets`（对任意 StatDef 加算）、`statFactors`（乘算）、`hungerRateFactor` / `hungerRateFactorOffset`、`restFallFactor` / `restFallFactorOffset`、`statOffsetsBySeverity` / `statFactorsBySeverity`（按 severity 曲线）、`damageFactors`、`capMods` 等。
- **StatDef 修正的生效点**：`StatWorker.GetValueInternal` 在 offsets 累加阶段调 `HediffStatsUtility.GetStatOffsetForSeverity(stat, stage, pawn, severity)`，在 factors 累乘阶段调 `GetStatFactorForSeverity(...)` —— 与特性 / 基因 / 戒律 / 装备同层参与，最终 `applyPostProcess` 时统一 `Mathf.Clamp(def.minValue, def.maxValue)`（`StatWorker.cs:823-826`）。
- **优先级**：`statOffsets` 命中（非 0）即 return，`statOffsetsBySeverity` 只是 fallback；`statFactors` 同理（值 `1` 视为未命中）。同一 stage 两处都配同一 stat 时，普通列表会盖掉曲线。
- **额外修饰**：`multiplyStatChangesBySeverity`（offset 再 ×severity）、`statOffsetEffectMultiplier` / `statFactorEffectMultiplier`（再乘一个 pawn 的 StatDef 值）。
- **显示**：`statOffsets` / `statFactors` 会进 StatDef 提示框的「相关健康状态」区（`StatWorker.GetOffsetsAndFactorsExplanation` 按 hediff 逐条列出，格式 `LabelBaseCap: +x`），hediff 卡片另经 `HediffStatsUtility.SpecialDisplayStats` 列出（饥饿/休息速率条目也在其中）——**不需要自写 ExplanationPart**。
- **共享实例陷阱**：`def.stages` 的元素是**全 Def 共享**，动态值必须在 `CurStage` 里 `new HediffStage` 返回（本项目 `Hediff_EfficientCirculation` / `Hediff_2FightIs2Be` 即此写法），就地改字段会污染所有同 Def 的 pawn 与存档。
- **饥饿速率**：`Need_Food.FoodFallPerTickAssumingCategory` = `BaseHungerRate(生命阶段, 种族) × 饥饿档位倍率 × hediffSet.GetHungerRateFactor() × 特性 HungerRateFactor × 床 BedHungerRateFactor`。**没有** `HungerRateMultiplier` 这类 StatDef（`StatDefOf` 里只有仅对床生效的 `BedHungerRateFactor`），故营养下降只能走 `stage.hungerRateFactor`（或 patch / 特性 / 床）。
- **休息速率**：`Need_Rest.RestFallPerTick` = 常量 × `hediffSet.RestFallFactor` × 档位系数；`NeedInterval` 里**非休息分支**才再 `× GetStatValue(StatDefOf.RestFallRateFactor)`（睡眠恢复走 `RestRateMultiplier`）。两者都只作用于清醒时的下降。
- **聚合语义**：`HediffSet.RestFallFactor` / `GetHungerRateFactor` 遍历所有 hediff 的 `CurStage`，先把各值**相乘**、再累加各 Offset，最后 `Mathf.Max(..., 0)`。
- **崩溃阈值**：HediffStage **没有**对应字段；它是 `StatDefOf.MentalBreakThreshold`（默认 0.35、`minValue 0.01`、`maxValue 0.5`），`MentalBreaker` 三个阈值都由它派生：`Minor = GetStatValue(MentalBreakThreshold)`、`Major = ×0.5714286`、`Extreme = ×1/7`。降低崩溃阈值 = 给该 StatDef 注入负值（`HediffStage.statOffsets` 或 `StatPart` 皆可），最终受 `minValue 0.01` 截断。
- **战斗状态窗口**：`Pawn_RecordsTracker.BattleActive` 不是即时失效——`EnterBattle` 每次把 `battleExitTick = now + 5000`（`Battle.TicksForBattleExit = 5000`），getter 只有在 `battleExitTick < TicksGame` 时才返回 null → 最后一次战斗事件后仍有 **2 小时**「在战斗中」窗口。判断「脱离战斗」要么接受这个尾巴，要么改用自有计时。
- **`extraTooltip`**：`Hediff.GetTooltip` 对 `def.extraTooltip` 与 `CurStage.extraTooltip` 都走 `tipSb.AppendLineTagged(...Formatted(pawn.Named("PAWN")))`（`Hediff.cs:799-806`）——只做 `Formatted`（替换 `{PAWN}` 等命名参数），**不会 `Translate`**，所以赋给 `stage.extraTooltip` 的必须是**已翻译好的最终文本**（`"Key".Translate(args)` 的结果），否则显示的是键名。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/HediffStage.cs`、`Verse/HediffSet.cs`、`Verse/HediffStatsUtility.cs`、`Verse/Hediff.cs`、`RimWorld/StatWorker.cs`、`RimWorld/Need_Food.cs`、`RimWorld/Need_Rest.cs`、`Verse.AI/MentalBreaker.cs`、`RimWorld/Pawn_RecordsTracker.cs`、`Verse/Battle.cs`、`Data/Core/Defs/Stats/Stats_Pawns_General.xml`（1.6 与 `lib/1.7/Assembly-CSharp.dll` 两版均核对）。

### Hediff 的动态文本与展示（`Label` / `TipStringExtra` / `Description`）
- `Hediff.Label`、`LabelBase`、`LabelInBrackets`、`TipStringExtra`、`Description` 均为 virtual，可覆写成按数据实时计算的文本；健康面板按 `LabelCap` 绘制，tooltip 由 `GetTooltip` 拼 `LabelCap` + `SeverityLabel` + `TipStringExtra` + `Description`。
- 展示型 Hediff 只需 `initialSeverity > 0`（`ShouldRemove => Severity <= 0`）；不设 `maxSeverity` 时健康面板**不画 severity 条**（`HealthCardUtility` 只在 `maxSeverity < float.MaxValue` 或 `lethalSeverity > 0` 时绘制）。
- 避免被当作伤病：`isBad=false`、`tendable=false`、`everCurableByItem=false`、`makesAlert=false`。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/Hediff.cs`、`RimWorld/HealthCardUtility.cs`、`Verse/HediffDef.cs`。

## 十、文本与本地化

### `"Key".Translate(args)` 的占位符格式化器（GrammarResolverSimple）
- 入口链：`TranslatorFormattedStringExtensions.Translate(key, params NamedArgument[])`（`Verse/TranslatorFormattedStringExtensions.cs:45`）→ `TaggedString.Formatted` → `GrammarResolverSimple.Formatted`（`Verse/GrammarResolverSimple.cs:42`）。注意 `Verse/Translator.cs:68` 那个 `params object[]` 旧重载已标 `[Obsolete]`，**实际不会被调用**，所以不能按 `string.Format` 的心智模型理解参数语法。
- 占位符解析（`GrammarResolverSimple.TryResolveInner`，line 91-305）：读 `{` 之后按字符分流 —— 遇 `_` 记子符号、遇 `?` 记条件分支、**遇 `:` 则整段切为函数调用** `{func:args}`；闭合后按索引（`int.TryParse`）或具名（`argsLabels`）取参。
- `{0}` / `{name}`：走 `TryResolveSymbol`（line 306），最终落到 `resolvedStr = obj.ToString()`（line 1061-1073）；`obj` 为 null 则输出空串。传值前先格式化好，是本项目的既有惯例。
- `{func:args}`：只支持 `lookup` 与 `replace`（`Verse/LanguageWorker.cs:254-264`），**其余函数名一律 `return ""`** —— 不 Log、不 Warning。
- 因此 **.NET 的 `{0:P0}` / `{0:F1}` 格式说明符不可用**：`:` 会让解析器把 `0` 当作函数名，最终静默替换为空串（现象与排查见 B-04 BUG-024）。需要格式化时在参数侧先 `ToStringPercent()` / `ToString("F1")`，再作为裸 `{0}` 传入。
- 无参 `.Translate()`（`Verse/Translator.cs:53`）直接查表返回原文、**不做符号解析**，故文本中含 `{BASETEXT}` 等二次占位符是安全的。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/TranslatorFormattedStringExtensions.cs`、`Verse/GrammarResolverSimple.cs`、`Verse/LanguageWorker.cs`、`Verse/Translator.cs`。

### 正文着色（ColoredText.Colorize）与死亡信挂载点
- `ColoredText.Colorize(this TaggedString ts, Color color)`（`Verse/ColoredText.cs:240`）= `ts.Resolve().Colorize(color)`；`Colorize(this string, Color)`（`:245`）= `<color=#RRGGBBAA>…</color>` 包裹 → **TaggedString 可着色，但产出是 string**（`TaggedString` 本身只有 `rawText` + `Resolve()`）。
- 信件正文经 `Dialog_NodeTree.DrawNode` 的 `Widgets.Label(rect, curNode.text.Resolve())` 绘制 → `Widgets.Label(Rect, TaggedString)` → `GUI.Label(position, label, Text.CurFontStyle)`，颜色标签随原版富文本渲染生效；尺寸测量走 `RawText.StripTags()`，不撑坏布局；`CapitalizeFirst()` 经 `FirstLetterBetweenTags()` 跳过 `<` 标签，以 `<color=…>` 开头不会被误大写。
- 死亡信正文由 `HealthUtility.GetDiedLetterText(pawn, dinfo, hediff)` 生成（三选一：`dinfo.Value.Def.deathMessage` / `PawnDiedBecauseOf` / `PawnDied`），在 `Pawn_HealthTracker.NotifyPlayerOfKilled` 里被调用后继续追加借用殖民者 / precept / 动物等段落 → 对 `GetDiedLetterText` 做 Postfix 追加的内容**不在正文最末**；要位于最末须改挂 `LetterStack.ReceiveLetter(Letter let, string debugInfo = null, int delayTicks = 0, bool playSound = true)`（`Verse/LetterStack.cs:56`）。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/ColoredText.cs`、`TaggedString.cs`、`Dialog_NodeTree.cs`、`Widgets.cs`、`HealthUtility.cs`、`Pawn_HealthTracker.cs`、`LetterStack.cs`。

## 十一、理想文化（Ideo）系统

### 结构 / 模因 / 戒律三层，而结构与模因**共用** `MemeDef`
- `MemeCategory { Normal, Structure }`（`Verse/MemeCategory.cs`）：`category=Structure` 是「结构」——文化创建时必选且**恰好 1 个**（`Dialog_ChooseMemes` 点选结构时移除其余结构），决定神灵数量与命名、崇拜厅名称、文化描述生成；`Normal` 是「模因」——选 1~4 个（`IdeoFoundation.MemeCountRangeAbsolute`），主要作用是把戒律放进可选池。
- 戒律是 `PreceptDef` → `Precept` 实例，**仪式 = `issue=Ritual` 的 `Precept_Ritual`**（该 issue 的 `allowMultiplePrecepts=true`，定义在 `Data/Core/Defs/PreceptDefs/Precepts_Ritual.xml`）。
- 不存在独立的 `StructureDef`；`IdeoFoundationDef` 全库仅 `Deity` 一条，只是「文化基石实现类」的注册表（`foundationClass`）。

### 仪式随 meme 自动带出：两条通道，都不占 impact 配额
- `MemeDef.requiredRituals`（`RequiredRitualAndBuilding`：`precept` / `pattern` / `building`）→ `IdeoFoundation.AddRequiredPreceptsForMemes`（仅在 `!parms.classicExtra` 时执行）。
- `PreceptDef.requiredMemes` → `IdeoFoundation.AddRitualsForMemes`；**附带移除语义**：文化不再含任一 `requiredMemes` 时该 precept 会被 `GetPreceptsToRemoveFromMemeChanges` 移除。
- 两者都在文化生成、改模因（`EnsurePreceptsCompatibleWithMemes`）、随机化戒律时自动执行——这正是「不靠代码注入」的基础。

### 戒律生成期的两个开关
- `countsTowardsPreceptLimit`：只有 `true` 才进入按 impact 的随机填充池（High 0~2 / Medium 5 / Low 5）。
- `canGenerateAsSpecialPrecept`：`countsTowardsPreceptLimit=false` **且** 此值为 `true` 时，`AddSpecialPrecepts` 会把它**无条件加进每个文化**（原版 Odyssey `GravshipLaunch` 即此用法）。
- `PreceptDef.maxCount` / `ignoreLimitsInEditMode` 只约束文化编辑界面（`IdeoUIUtility.AddPrecept`）。
- 全库 `IdeoFoundation.MaxRituals = 6` **无任何引用**，原版并没有「仪式数量上限」这类机制。

### 文化命名链路（`IdeoFoundation_Deity.GenerateTextSymbols`）
- 先二选一取源：`Pack`（meme 的 `symbolPacks`，权重 1）或 `Deity`（文化神灵，权重 0.5）；随后用**同一个 request**依次生成 `ideo.name`（`r_ideoName`）、`ideo.adjective`（`r_ideoAdjective`）、`ideo.memberName`（`r_memberName`）。
- `symbolPacks` 的 `ideoName`（结构 meme 走 `packIdeoNameStructure`）/ `theme` / `adjective` / `member` 被注入为 `pack*` token；`prefix=true` 时统一加 `[hyphenPrefix]-` 前缀并置常量 `forcePrefix`（规则可写 `forcePrefix==False` 做条件）。
- Deity 侧：`deityNameMakerOverride` / `deityTypeMakerOverride` 可**整体替换**命名包（只读结构 meme 的该字段）；`fixedDeityNameTypes` 把「名字 + 称号」**成对固定**（此时两条 `GenerateName` 全部跳过，只对 `name` 做跨文化重名校验）。
- 角色名走 `Precept.AddIdeoRulesTo`（会把 meme 的 `generalRules` 与 `chosenIdeoName` / `chosenTheme` 注入请求），`chosen*` 由 `TryGuessChosenSymbolPack` **反查**文化名/成员名命中了哪个 pack 才提供。

### `GrammarResolver` 的抽取语义（写规则包必知）
- 只从 **最高 `priority` 层**按 `SelectionWeight`（`p=`，`Rule_String` 的 `p` 参数）加权随机抽取（`GrammarResolver.RandomPossiblyResolvableEntry`）。
- 抽中的规则若解析失败（子符号缺失等）会被 `MarkKnownUnresolvable` 排除，然后**重新抽取**；整层耗尽后 `maxPriority` 重算，自动降级到下一层。
- 因此「`priority=1` 主规则 + `priority=0` 兜底」是原版既有的降级写法（如 `r_ideoAdjective(priority=1)->[packAdjective]` + `->[keyDeity]的`），**不改原版数据即可压制同名规则**。
- 规则右侧 `->` 之后是 anything-goes：可写 token、可写死文本，也**可以留空**（生成空串；原版 `episode(priority=-1)->` 即此用法）。缺 `->` 会 `Log.Error("Bad string pass when reading rule …")`。

### 玩家文化的唯一性与编辑入口
- 正常玩法只有世界创建（`Page_ConfigureIdeo`）与文化改革（`Dialog_ReformIdeo`）两条路；会新增文化的 `Dialog_ConfigureIdeo`（→ `IdeoManager.Add`）仅由 Anomaly「新殖民地 / Archonexus 重启」链路调用（`QuestPart_NewColony`）。
- 神灵的**名字、称号、性别**都可在文化界面右键 → `Dialog_EditDeity` 手动修改（写回 `deity.name` / `deity.type` / `deity.gender`）。
- `AllExistingDeities` 会跨**所有文化**做重名校验；`NameGenerator.GenerateName` 重试 150 次仍失败则 `Log.Error("Could not get new name …")` 并返回最后结果（不返回 null）。`FillDeity` 传的 `appendNumberIfNameUsed=false`，所以不会自动加编号避让。

### 角色候选的文化门会被短路
`Precept_Ritual.GetRitualBeginWindow` 的候选 filter 为
`!ritualOnlyForIdeoMembers || def.allowSpectatorsFromOtherIdeos || pawn.Ideo == ideo || !voluntary || allowOtherIdeos || pawn.IsPrisonerOfColony || …`，
其中 `PreceptDef.allowSpectatorsFromOtherIdeos` **默认 true** → 整个表达式恒真，`RitualRole.allowOtherIdeos` 形同虚设。要限制候选文化，只能在 `RitualRole.AppliesToPawn` 里自行校验。

### 语言包与 `.tar`
- `Data/<DLC>/Languages/` 下除 `English/` 是目录外，其余语言均为 `<英文名> (<本地名>).tar`（Core 28 种、Ideology 20 种、Anomaly/Biotech/Odyssey 各 18 种），内部即标准布局：`DefInjected/<DefType>/<与 Defs 源文件同名>.xml`。
- `RulePack.rulesStrings` 带 `[MustTranslate] [TranslationCanChangeCount]`，故**整表替换（`<li>` 列表）与逐条索引（`.0` / `.1`）都合法，但同一列表不可混用**（`DefInjectionPackage.CheckErrors` 会判定冲突）；`List<对象>`（如 `fixedDeityNameTypes`）没有该特性，只能逐条索引。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/MemeDef.cs`、`PreceptDef.cs`、`IdeoFoundation.cs`、`IdeoFoundation_Deity.cs`、`Ideo.cs`、`Dialog_ChooseMemes.cs`、`Precept.cs`、`Precept_Ritual.cs`、`NameGenerator.cs`、`Verse.Grammar/RulePack.cs`、`Rule_String.cs`、`GrammarResolver.cs`、`Verse/DefInjectionPackage.cs`。

## 十二、战斗日志（Battle 系统）

### 组件与数据流

| 组件 | 位置 | 职责 |
| --- | --- | --- |
| `LogEntry`（抽象基类） | `Verse/LogEntry.cs` | 一条战斗日志。公开成员：`def`、`Age` / `Tick` / `Timestamp`、`Concerns(Thing)` / `GetConcerns()`、`ToGameStringFromPOV`、`IconFromPOV`、`ShowInCompactView` |
| `LogEntry_DamageResult` | `Verse/LogEntry_DamageResult.cs` | 伤害类条目的中间层。成员只有 `damagedParts` / `damagedPartsDestroyed` / `deflected`（**protected**）与 `FillTargets(...)` |
| `BattleLogEntry_*` | `Verse/` | 具体条目：`MeleeCombat` / `RangedFire` / `RangedImpact` / `DamageTaken` / `ExplosionImpact` / `AbilityUsed` / `ItemUsed` / `StateTransition` / `Event` |
| `Battle` | `Verse/Battle.cs` | 一场战斗：聚合条目与参与者；`GetUniqueLoadID()` = `"Battle_" + loadID`（实现 `ILoadReferenceable`） |
| `BattleLog` | `Verse/BattleLog.cs` | 战斗集合与归并；`Find.BattleLog => Current.Game.battleLog`（`Verse/Find.cs:146`），随存档持久化 |
| `Pawn_RecordsTracker` | `RimWorld/Pawn_RecordsTracker.cs` | 每个 Pawn 的"当前战斗"引用与退出计时 |

```
战斗行为（近战/远程/爆炸/能力/状态转换） → 生成 BattleLogEntry_*
      ↓ BattleLog.Add(entry)  归并 + 参与者登记
Battle{ entries[], concerns{Pawn} }
      ↓ records.EnterBattle(battle)
battleActive / battleExitTick（超时未刷新即视为战斗结束）
```

`BattleLog.Add` 的归并逻辑：遍历条目涉及的每个 Pawn，取各自 `records.BattleActive`（已归一化到 `AbsorbedBy` 链末端），以 `Importance`（= `Entries.Count`）最大者作为归属战斗；若该 Pawn 尚属另一场战斗，把那一场 `Absorb` 合并进来并从列表移除；最后对每个参与者 `EnterBattle(battle)` 并 `battle.Add(entry)`。

### 参与者集合（concerns）

- `Battle.concerns` 是 **private** `HashSet<Pawn>`，公开查询只有 `Concerns(Pawn)`；**枚举参与者只能从公开的 `Battle.Entries` 逐条 `GetConcerns()` 重建**（原版 `ExposeData` 在 PostLoadInit 阶段也是这么做）。
- 参与者 = 所有出现在日志条目 `GetConcerns()` 中的 Pawn。**在场却没有任何战斗日志的 Pawn 不算参与者**。
- `BattleLog.Add` 只收集 `concern is Pawn`，**非 Pawn（炮塔、机械建筑）永远不会进入 concerns**。

### 生命周期与时序

| 机制 | 数值 | 出处 |
| --- | --- | --- |
| 单场战斗退出窗口 | 5000 tick | `Battle.TicksForBattleExit`；`Pawn_RecordsTracker.EnterBattle` 内 `battleExitTick = TicksGame + 5000` |
| 记录刷新 | 每 80 tick | `Pawn_RecordsTracker.RecordsTickInterval` |
| 战斗列表容量 | 最多 20 场（`BattleHistoryLength`） | `BattleLog.cs` |
| 陈旧战斗清理 | 非吸收态战斗数 > 20 **且** 最末一场 `LastEntryTimestamp + max(420000, 5000) < now` | `BattleLog.ReduceToCapacity` |
| 战斗合并 | 同一 Pawn 同时关联两场时保留 `Importance` 大者，另一场置 `absorbedBy` 并清空自身 entries | `Battle.Absorb` |

### 原版在何时自动写战斗日志（关键）

**只有两个时机是原版自动写的**：

| 时机 | 位置 | initiator 取值 |
| --- | --- | --- |
| 倒地 | `Verse/Pawn_HealthTracker.cs:869` | `dinfo?.Instigator as Pawn` |
| 死亡 | `Verse/Pawn.cs:2028` | `dinfo?.Instigator as Pawn` |

`DamageWorker_AddInjury.Apply`（`Verse/DamageWorker_AddInjury.cs:9`）**不创建任何日志** → **战斗过程日志必须由攻击方显式创建**（原版近战/远程由 Verb / Projectile 自己建，自建投射物要照 `Projectile_ArcsRay` 的写法补 `Find.BattleLog.Add` + `AssociateWithLog`）。两处自动日志在 `Instigator` 非 Pawn（炮塔 / 陷阱 / 召唤物）时归因会断。

### 三个陷阱

1. **`Battle.LastEntryTimestamp` 返回的是最旧条目**：`Battle.Add` 用 `entries.Insert(0, entry)`（`Absorb` 后 `OrderBy(e => e.Age)` 重排，同样最新在前），而该属性取 `entries[entries.Count - 1]` → 名字与行为相反。判"战斗是否结束"应自行求 `Entries` 的最大 `Timestamp`。
2. **时间基准**：`LogEntry.Timestamp` 是 `TicksAbs`，比较须用 `Find.TickManager.TicksAbs`；`battleExitTick` 用的却是 `TicksGame`，两者不可混用。
3. **列表顺序是隐式契约**：`Entries[0]` 才是最新条目——依赖它等于依赖插入顺序，宜用 `Max` 之类显式表达。

### 覆盖面与限制

- 战斗日志与"是任务 / 袭击 / 随机遭遇"无关 → 任何来源的战斗都进同一套 Battle；**远行队遭遇伏击会生成临时地图打常规战斗**（`RimWorld.Planet/CaravanIncidentUtility.cs:41` `SetupCaravanAttackMap` → `GetOrGenerateMapForIncident(..., WorldObjectDefOf.Ambush)`）。
- 日志条目内部字段多为 private（`initiatorPawn` / `recipientPawn` 等）→ **无法从日志区分攻守双方**；例外：`BattleLogEntry_MeleeCombat.RuleDef`（public，可判命中/闪避）、`BattleLogEntry_AbilityUsed.abilityUsed`（public，知所用能力）。
- **日志里没有伤害数值**（`RangedImpact` 只有 initiator / recipient / originalTarget / weaponDef / projectileDef / coverDef）→ 伤害量须另取，本项目走 `Pawn.PostApplyDamage(dinfo, totalDamageDealt)` 补丁。
- 原版 `Ability` 是否写战斗日志由 `AbilityDef.writeCombatLog` 决定（默认 false），开启后原版会在能力使用时 `Add(new BattleLogEntry_AbilityUsed(...))`。

- 出处：`kb/L0-证据/decompiled/1.6/Verse/Battle.cs`、`BattleLog.cs`、`LogEntry.cs`、`LogEntry_DamageResult.cs`、`BattleLogEntry_*.cs`、`Pawn.cs`、`Pawn_HealthTracker.cs`、`PawnKindDef.cs`、`DamageWorker.cs`、`DamageWorker_AddInjury.cs`、`Find.cs`、`RimWorld/Pawn_RecordsTracker.cs`、`RimWorld/Ability.cs`、`RimWorld/AbilityDef.cs`、`RimWorld/GenDate.cs`、`RimWorld.Planet/WorldGrid.cs`、`RimWorld.Planet/Tile.cs`、`RimWorld.Planet/PlanetTile.cs`、`RimWorld.Planet/CaravanIncidentUtility.cs`。

## 十三、Gizmo 与 Ability 命令

### MapGizmoUtility.LastMouseOverGizmo（「鼠标下的 gizmo」）
- `MapUIOnGUI()` 用 `GizmoGridDrawer.DrawGizmoGridFor(..., out mouseoverGizmo)` 记录本帧，`MapUIUpdate()` 才把它转正为 `lastMouseOverGizmo` → **读到的是上一帧的值**；且仅在 `Find.MainTabsRoot.OpenTab == null || OpenTab == MainButtonDefOf.Inspect` 时刷新网格。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/MapGizmoUtility.cs`。

### 资源条 gizmo 的「施法消耗预览」通道（两处，判据不同）

| 消费者 | 判据 | 预览内容 |
|---|---|---|
| `PsychicEntropyGizmo.GizmoOnGUI` | `LastMouseOverGizmo as Command_Psycast`（硬编码） | 熵条叠加 `def.EntropyGain` 增量段；精神力条叠加 `def.PsyfocusCostRange.min` 消耗段 |
| `GeneGizmo_ResourceHemogen.GizmoOnGUI` | `LastMouseOverGizmo is Command_Ability`（宽松） | 血原质条叠加 `CompAbilityEffect_HemogenCost.Props.hemogenCost` 消耗段 |

- 两者节奏一致：`Mathf.Repeat(Time.time, 0.85f)`，`0.1s` 淡入、`0.25s` 起按 `0.6s` 淡出，绘制时 `GUI.color` 的 alpha 再乘 `0.7`。
- 熵条只在 `Find.Selector.SingleSelectedThing == pawn && psychicEntropy != null && psychicEntropy.NeedToShowGizmo()` 时出现（`Verse/Pawn.cs:3362`）；`NeedToShowGizmo()` = Royalty 开启 + `PsychicSensitivity > 0` + （熵 > 0 或带 `Hediff_PsychicAmplifier`）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/PsychicEntropyGizmo.cs`、`GeneGizmo_ResourceHemogen.cs`、`Pawn_PsychicEntropyTracker.cs`、`Verse/Pawn.cs`、`Verse/Gizmo.cs`。

### Command_Ability / Command_Psycast 的构造与覆盖面
- `Ability.GetGizmos()` 用 `Activator.CreateInstance(def.gizmoClass, this, pawn)` 建命令（**无 try-catch**），默认 `gizmoClass = typeof(Command_Ability)`；而 `Psycast.GetGizmos()` 直接 `new Command_Psycast(this, pawn)`，**不读 `gizmoClass`**。
- `Command_Psycast` 只覆盖三处：构造 `(Psycast ability, Pawn pawn)`、`TopRightLabel`（输出 `NeuralHeatLetter` / `PsyfocusLetter`，即 `H` / `P`，值见 `Data/Royalty/Languages/*/Keyed/GameplayCommands.xml`）、`DisabledCheck`（新增 `def.level` 灵能等级与 `WouldOverflowEntropy(def.EntropyGain + 队列熵)` 两条禁用）。
- `Command_Ability` 构造体内即解引用 `ability.def`（`category` / `LabelCap` / `hotKey` / `uiIcon` / `level` / `displayOrder`）并**虚调用 `Tooltip`** → 传入的 ability 不能为 null，且派生类 override 的 `Tooltip` 会在派生类改写 `ability` 之前先跑一次。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/Ability.cs`、`Command_Ability.cs`、`Command_Psycast.cs`、`Psycast.cs`、`AbilityDef.cs`、`Verse/Gizmo.cs`。

### Ability.Initialize() 的副作用（`new Ability(pawn, def)` 会做什么）
- 为 `def.comps` 每项 `Activator.CreateInstance(compClass)` + `Initialize(props)`；`Id == -1` 时取 `Find.UniqueIDsManager.GetNextAbilityID()`；访问 `VerbTracker.PrimaryVerb`（懒建 `new VerbTracker(this)` → 按 `def.verbProperties` 建 verb）；`def.charges > 0` 时填 `maxCharges` / `charges`。
- 故「只想借一个 def 载体」时不要走 `new Ability(pawn, def)`（会消耗唯一 ID 并建 verb / comps），可改用 `FormatterServices.GetUninitializedObject` 造未初始化对象。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/Ability.cs:1175-1210`。

### AbilityDef 的文本出口
- `Ability.Tooltip` → `AbilityDef.GetTooltip(pawn)`，其正文为 `StatSummary(pawn).ToLineList()` 等 → **description 里的消耗行由 `StatSummary` 生成**：`AbilityPsyfocusCost`（`def.PsyfocusCost`）、`AbilityEntropyGain`（`def.EntropyGain`）、`AbilityCastingTime`、冷却、持续时间等。
- `StatSummary` **非 virtual**；官方扩展点是 `AbilityCompProperties.ExtraStatSummary()`（virtual，被 `StatSummary` 的 comps 循环调用），但 props 侧拿不到所属 Def 的字段值。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/AbilityDef.cs`、`Verse/AbilityCompProperties.cs`。

## 十四、战斗控制与施法中断

### 减速（StaggerHandler）
- `Pawn_StanceTracker.stagger`（`RimWorld.StaggerHandler`）的 `StaggerFor(ticks, moveSpeedFactor = 0.17f)`：时长先乘 `StatDefOf.StaggerDurationFactor`；取 `staggerTicksLeft = Max(旧, 新)`、`staggerMoveSpeedFactor = Min(旧, 新)`（周期性重复施加只会续时长/取更小系数，**不会叠加**）。
- 生效点只有 `Pawn_PathFollower.CostToPayThisTick`：stagger 期间每格耗时 × `StaggerMoveSpeedFactor`（默认 0.17 → 移速 17%）；**不打断动作、不改 stance**，检查字符串显示「SlowedByDamage」。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/StaggerHandler.cs:31-45`、`Verse.AI/Pawn_PathFollower.cs:812-826`。

### 定身（StunHandler）
- `Pawn_StanceTracker.stunner`（`RimWorld.StunHandler`）的 `StunFor(ticks, instigator, addBattleLog = true, showMote = true, disableRotation = false)`：只写 `stunTicksLeft = Max(...)`，**不设置 stance**；`StopStun()` 清零。
- 行动阻断来自 `Pawn_StanceTracker.FullBodyBusy`（`stunner.Stunned || curStance.StanceBusy`）→ 移动、攻击、施法均被挡；`showMote:false` 关闭原版眩晕星星，`addBattleLog:false` 不写战斗日志。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/StunHandler.cs:178-192`、`Verse/Pawn_StanceTracker.cs:28-38`。

### 打断预热（CancelBusyStanceSoft）
- `Pawn_StanceTracker.CancelBusyStanceSoft()`：仅当 `curStance is Stance_Warmup` 时 `SetStance(new Stance_Mobile())` —— 覆盖射击瞄准、本模组施法（`Verb_CastSpell` 的 `Stance_Warmup`）与一切 `Stance_Warmup` 预热；**不覆盖 `Stance_Busy`**（装填 / 手术 / 部分 job 动作，要打断只能 `EndCurrentJob` 或短 `StunFor`）。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/Pawn_StanceTracker.cs:93-99`。

### 无需选择目标直接施放（AbilityDef.targetRequired）
- `Command_Ability.ProcessInput`：`if (ability.def.targetRequired) Find.Targeter.BeginTargeting(ability.verb); else ability.QueueCastingJob(ability.pawn, LocalTargetInfo.Invalid);` → 把 `AbilityDef.targetRequired` 设为 `false`，点图标即直接进入咏唱/施法，不再要求玩家选目标（布都御魂即此用法）。
- 1.6 与 1.7 的 `AbilityDef.targetRequired` 均存在且默认 `true`（1.7 已用 `lib/1.7/Assembly-CSharp.dll` 核对）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/Command_Ability.cs`、`RimWorld/AbilityDef.cs`。

### 近战攻击冷却（`Stance_Cooldown` 的创建点）
- 近战攻击没有 verb 冷却字段：`Verb.TryCastNextBurstShot` 在 `TryCastShot` 结算后 `CasterPawn.stances.SetStance(new Stance_Cooldown(verbProps.AdjustedCooldownTicks(...), ...))`；`Pawn_MeleeVerbs.TryMeleeAttack` 由 `pawn.stances.FullBodyBusy` 拦截，冷却结束即可再攻。
- `Stance_Busy.ticksLeft` 是 public，`Stance_Cooldown` 构造 Postfix 里置 0 即让下一拍 `StanceTick` 递减后过期——这是「按每次挥击概率重置冷却」的稳当时机（此时冷却刚建立）。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/Verb.cs:519-532`、`Verse/Stance_Busy.cs`、`RimWorld/Pawn_MeleeVerbs.cs`。

## 十五、特效与粒子（Mote / Fleck）

### 带颜色的飘字（`MoteMaker.ThrowText`）
- 两个重载：`ThrowText(Vector3 loc, Map map, string text, float timeBeforeStartFadeout = -1f)` 与 `ThrowText(Vector3 loc, Map map, string text, Color color, float timeBeforeStartFadeout = -1f)`；颜色是**文字颜色**，不传则用默认白。
- 本项目样本：闪避飘字 `CompRangeAttackDodge`（默认色）、武器同调暴击飘字 `Patch_WeaponAttunement_Crit`（淡红 `new Color(1f, 0.55f, 0.55f)`、`1.9f` 时长）。
- 出处：`kb/L0-证据/decompiled/1.6/RimWorld/MoteMaker.cs:43-48`。

### Mote 的绘制状态（`linearScale` / `instanceColor` / `exactRotation`）
- `Mote` 是 `Thing`，绘制由 `Graphic_Mote` + 三个实例字段决定：`linearScale`（**世界尺寸倍数**，配 `drawSize=1` 时等价于「直径」）、`instanceColor`（乘到材质颜色上，含 alpha）、`exactRotation`（需自己按 `deltaTime` 累加）。
- 驱动位置：`TimeInterval(float deltaTime)` 每帧调用 —— 把动画写成「elapsed 的纯函数」最省心：外部只 `SetElapsed(tick)`，Mote 里按 elapsed 推导 scale/alpha；`TickInterval(int delta)` 每 tick 调用，适合计数器。
- `needsMaintenance` + `fadeOutUnmaintained` 需要每拍 `Mote.Maintain()` 续命；由代码 `Destroy()` 收尾时把 `fadeOutUnmaintained` 设 false，避免与代码里的 alpha 淡出叠成两段。
- 贴图是白底时 `instanceColor` 可直接染色（`ZeoGullveigCircle` 即白底图，金色/灰绿都靠染色）；`Mote` 的 `def` 是 `ThingDef`，**没有** `SpellDef.themeColor`，颜色需由 Spell 侧传入。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/Mote.cs`、`RimWorld/Graphic_Mote.cs`；本项目样本 `Mote_ZeoGullveig` / `Mote_SolLight`。

### Fleck 粒子的速度与生命周期（`FleckSystemThrown` / `FleckThrown`）
- `FleckSystemThrown` 只是 `FleckSystemBase<FleckThrown>` 的壳，运动在 `FleckThrown`：
  - `Setup(FleckCreationData)`：先 `SetVelocity(velocityAngle, velocitySpeed)`（水平方向），再把 `creationData.velocity`（`Vector3?`）**叠加**上去：`velocity += creationData.velocity.Value`；`airTimeLeft = creationData.airTimeLeft ?? 999999f`。
  - `TimeInterval`：`velocity += def.acceleration * deltaTime`；`NextPosition` 用 `worldPos += velocity * deltaTime` —— **velocity 单位是「格/秒」**，垂直位移直接用 y 分量（`Vector3.down * 速度`）。
  - `Skidding`（`!Flying && Speed > 0.01`）会按 `skidSpeedMultiplierPerTick` 衰减速度；默认 `airTimeLeft` 极大 → `Flying` 恒真，不进入 skidding。
- `FleckDef` 的 `fadeInTime` / `solidTime` / `fadeOutTime` 决定 alpha 时间线（`FleckStatic.TimeInterval`），总寿命 = 三者之和。**下落距离 = 速度 × 总寿命**：想让粒子明显位移，速度必须按寿命量级给（如寿命 1.5s、想落 3 格 → 2 格/秒）。
- 结论：**速度量级太小是「粒子看起来不动」的最常见原因**（本项目 BUG-037）。
- 出处：`kb/L0-证据/decompiled/1.6/Verse/FleckThrown.cs`（`Setup` / `TimeInterval` / `NextPosition`）、`Verse/FleckCreationData.cs`、`Verse/FleckStatic.cs`、`Verse/FleckSystemThrown.cs`；`Setup` 的 velocity 叠加语义已用 `lib/1.7/Assembly-CSharp.dll` 核对一致。

### 原版火焰特效的复用件（Fleck / Effecter）
- `DamageDef Flame` 的 `explosionCellFleck` 为 `BlastFlame`（`Data/Core/Defs/DamageDefs/Damages_Environmental.xml`）：原版火焰爆炸按**每个爆炸格**生成 `BlastFlame`，不是单个中心爆点。自制方向性范围可对每个受影响格 `FleckMaker.GetDataStatic(cell.ToVector3Shifted(), map, BlastFlameDef, scale)`。
- `Fleck_RadialSparks`（`MoteCircularSparks`）是**原地环形扩张**的 shader 特效，不适合方向性扇形；需要火星时用 `MicroSparksFast` / `HeatGlow_Intense`。
- 其他可复用件：`ExplosionFlash`（爆炸闪光）、`HeatGlow` / `HeatGlow_Intense`（热光）、`Smoke` / `SmokeGrowing`（烟）、`Fleck_HeatDiffusion` / `Fleck_HeatWaveDistortion`（热浪/扭曲）；巨型爆炸的 `Mote_GiantExplosion` / `Mote_GiantExplosionInner` 画幅过大，不适合小范围。
