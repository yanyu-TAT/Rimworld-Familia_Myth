# C-05 原版 API 简述

> 按系统分类收录原版 API 方法速查（反编译证据）。本文档将持续扩充：新分析的系统按「X、系统名」追加章节即可（如当前已有 Quest / Pawn / 远行队 / 穿梭机等章节），后续可再加入魔法、技能、UI 等其他系统的分析。
> 出处统一为 `decompiledFiles/`（ilspycmd 反编译）；**方法签名与行为以文档编写时反编译版本为准，动手前仍须按规范实时 ilspycmd 核对**，本文件不替代核对。

## 一、Quest 任务系统

### QuestPart / QuestPartActivable 生命周期
- `QuestPartActivable`：`inSignalEnable`/`inSignalDisable` 信号开关部件；**仅 Enabled 时**收到 `Notify_QuestSignalReceived` 与 `QuestPartTick()` 调用（每 tick 由 Quest 驱动，项目内按 ~250 tick 节流），`Enable/Disable/Complete/ProcessQuestSignal` 为虚方法可覆写。
- 信号 tag 规则：Quest 只接收 `Quest{id}.` 前缀或全局 tag；生成期用 `QuestGenUtility.HardcodedSignalWithQuestID(tag)` 固化同串，运行时 `Find.SignalManager.SendSignal(new Signal(同串))` 即可送达（含在 `site.AllEnemiesDefeated` 等原版生成信号上拼接）。
- `QuestManager.Notify_PawnKilled(Pawn, DamageInfo?)`：任意 pawn 被杀广播给所有 Ongoing 任务的全部部件，部件自行过滤关注对象（全灭结算等）。
- 出处：`decompiledFiles/RimWorld/QuestPart.cs`、`RimWorld/QuestPartActivable.cs`、`RimWorld/QuestUtility.cs`。

### QuestPart_ExtraFaction（「原派系」标记，声明式）
- 字段：`extraFaction`（`ExtraFaction { Faction faction; ExtraFactionType factionType; }`）、`affectedPawns`、`areHelpers`、`inSignalRemovePawn`/`inSignalsRemovePawn`。
- **声明式语义**：extra faction 状态不存于 pawn，运行时由 `QuestUtility.GetExtraFactionsFromQuestParts` 从 **Ongoing 任务**的部件反查——部件加进任务即生效（与部件是否 Enabled 无关），`quest.End` 清理后失效。`Notify_FactionRemoved` 清空 affectedPawns。
- `Notify_PawnKilled`：若成员 `HomeFaction != null && Faction != HomeFaction` → 自动 `SetFaction(HomeFaction)`（成员在玩家派系期间死亡，尸体自动归还，防「白送人口」）。
- `inSignalRemovePawn`：收到信号（`SUBJECT` 参数为该 pawn）时将其移出 affectedPawns 并 `Notify_MemberLeftExtraFaction`。
- `Cleanup()/Disable()`：对 affectedPawns 设 30 天 `SetNoAidRelationsGainUntilTick`。
- 出处：`decompiledFiles/RimWorld/QuestPart_ExtraFaction.cs`、`RimWorld/ExtraFaction.cs`。

### QuestUtility 查询扩展（lodger / helper 判定）
- `IsQuestLodger(this Pawn p)`：`HasExtraHomeFaction() || HasExtraMiniFaction()` → true（带 Home/Mini 标记即 lodger）。
- `IsQuestHelper(this Pawn p)`：先 lodger，再查存在 Ongoing 任务中 `QuestPart_ExtraFaction { areHelpers: true }` 且 affectedPawns 含该 pawn（王室帮手）。
- `GetExtraHomeFaction(p, Quest forQuest = null)` / `GetExtraHostFaction` / `GetExtraMiniFaction`、`HasExtraHomeFaction(p, Faction)` 等：按类型/派系反查 extra faction。
- 出处：`decompiledFiles/RimWorld/QuestUtility.cs`（IsQuestLodger ≈L524、IsQuestHelper ≈L533、Get/Has 系列 ≈L616-671）。

### QuestPart_JoinPlayer（难民「加入玩家」部件）
- 字段：`pawns`、`joinPlayer`、`makePrisoners`、`inSignal`/`outSignalResult`。
- `joinPlayer=true` 时：已是殖民者/囚犯/奴隶走 `RecruitUtility.Recruit(pawn, Faction.OfPlayer)`；否则 `pawn.SetFaction(Faction.OfPlayer)`（与手动 SetFaction 等价）。
- 出处：`decompiledFiles/RimWorld/QuestPart_JoinPlayer.cs`。

### 离场归还：QuestPart_Leave / LeaveQuestPartUtility
- `MakePawnLeave(Pawn pawn, Quest quest)`：若在 caravan → 先 `CaravanInventoryUtility.MoveAllInventoryToSomeoneElse` + `caravan.RemovePawn(pawn)`；再选归还派系（extra home → extra mini → SlaveFaction → 随机非敌对人类派系兜底）并 `SetFaction`；清 guest / 手中物 / 通知 lord（`Notify_PawnLost(ForcedByQuest)`）。
- `MakePawnsLeave(IEnumerable<Pawn>, bool sendLetter, Quest quest, bool wakeUp=false)`：批量——发「离开」信 → 逐人 MakePawnLeave → 对存活站立者统一发 `LordJob_ExitMapBest` 走出地图。
- 出处：`decompiledFiles/RimWorld/LeaveQuestPartUtility.cs`、`RimWorld/QuestPart_Leave.cs`。

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
- 出处：`decompiledFiles/Verse/Pawn.cs`（≈L1838）。

### ExtraFactionType 与「原派系」渲染
- `ExtraFactionType`：`HomeFaction`（原属派系，王室帮手/本项目救援用）/ `HostFaction`（寄主派系）/ `MiniFaction`（难民本体用）。
- 渲染：角色卡经 `Pawn.FactionDesc` + `CharacterCardUtility` 显示 extra faction 项，标签由 `ExtraFactionTypeExt.GetLabel`，tooltip `ExtraFactionDesc`（「{PAWN_labelShort} 原本所属的派系」）。玩家侧只有玩家成员才看得到此栏。
- 出处：`decompiledFiles/Verse/Pawn.cs`、`RimWorld/CharacterCardUtility.cs`。

### 殖民者栏成员口径
- 头像栏 `ColonistBar` 只列 `mapPawns.FreeColonists`（玩家派系自由成员）+ 可控 subhuman + 殖民者尸体。**非玩家派系临时成员永远不上栏、不可命令**——「像殖民者一样出现」必须真实加入玩家派系。
- 出处：`decompiledFiles/RimWorld/ColonistBar.cs`、`RimWorld/MapPawns.cs`。

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
- 出处：`decompiledFiles/RimWorld.Planet/CaravanFormingUtility.cs`（≈L264）、`RimWorld/Dialog_FormCaravan.cs`（≈L1532）、`RimWorld/TransporterUtility.cs`（≈L63）、`RimWorld/Dialog_EnterPortal.cs`。

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
- 出处：`decompiledFiles/RimWorld/CompShuttle.cs`（IsAllowed ≈L722、IsAllowedNow ≈L759、字段 ≈L33/L581）。

### 信号与归还时机窗口
- `ShipJob_Arrive.TryStart` 广播 `Arrived`；`ShipJob_FlyAway.TryStart` 先做未满载处理（dropMode 卸载），随后 `SendLaunchedSignals` 广播 **`SentSatisfied`/`SentUnsatisfied`**，contents 才移入离场 skyfaller。
- **窗口**：收到 `SentSatisfied/SentUnsatisfied` 信号当拍，成员仍在穿梭机内容器（未转移）——在此处 `SetFaction(原派系)` 归还，成员即以原派系身份随船离场（本项目结算归还点）。
- **信号收发方不对称（易踩坑）**：穿梭机相关信号按广播方分两类——
  - `SentSatisfied/SentUnsatisfied/SentWithExtraColonists`（`CompShuttle.SendLaunchedSignals`）与 `Spawned/Despawned/Destroyed/Killed/LeftBehind`（Thing 通用）由**穿梭机 thing**（`parent.questTags`）广播；thing 的 questTags 会被生成期「引用过该变量信号的 `HardcodedSignalWithQuestID`」自动登记（slate 变量名即 tag 名），可靠可达；
  - `Arrived/FlewAway`（`ShipJob_Arrive/FlyAway`）由 **TransportShip.questTags** 广播；`QuestNode_GenerateTransportShip` 与 `Util_TransportShip_Pickup` **不会给 transportShip 打 tag**（vanilla 的 Mission/Bestowing 是 root 手动对 thing 与 transportShip 各 `AddQuestTag` 一次）→ util 生成场景下 **`Arrived` 静默无接收者**，vanilla 所有 util 使用者都不监听 `Arrived`，「穿梭机已抵达」改用 thing 的 `Spawned`（Skyfaller 落地放出穿梭机时广播）作为到达信号。
- 出处：`decompiledFiles/RimWorld/ShipJob_Arrive.cs`（L65）、`RimWorld/ShipJob_FlyAway.cs`（L47）、`RimWorld/CompShuttle.cs`（SendLaunchedSignals ≈L543）、`RimWorld/Thing.cs`（Spawned ≈L914）、`RimWorld.QuestGen/QuestGenUtility.cs`（自动打 tag ≈L50）、`RimWorld.QuestGen/QuestNode_GenerateTransportShip.cs`。

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
- 出处：`decompiledFiles/RimWorld.QuestGen/QuestGen_Shuttle.cs`、`RimWorld.QuestGen/QuestNode_GenerateShuttle.cs`。

## 五、Lord 与离图安排

- **lord 释放路径**：`Pawn.SetFaction` 内部即 `GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ChangedFaction)`；也可手动 `pawn.Map.lordManager.RemoveLord(pawn.GetLord())`（单成员 defend lord）。
- `LordJob_DefendPoint(cell, …, isCaravanSendable: false, addFleeToil: false)`：营地小范围踱步/自卫、禁逃跑离图，用于把「无人接管的原派系 NPC」钉在站点（本项目进图 → 加入玩家派系当拍释放，之后不再发放）。
- `LordJob_ExitMapBest(LocomotionUrgency, canDig, canDefendSelf)`：安排批量 pawn 走出地图（滞留者离场用）。
- 出处：`decompiledFiles/Verse/Pawn.cs`（≈L2695）、`RimWorld.Planet` / `Verse.AI.Group` 下 LordJob 系列。

## 六、站点威胁生成（狂暴兽群）

- `AggressiveAnimalIncidentUtility.TryFindAggressiveAnimalKind(float points, PlanetTile tile, out PawnKindDef animalKind)`：给定威胁点能否选出狂暴动物（含 `PlanetTile.Invalid` 全局兜底预检）。
- **点数下限**：威胁点低于最弱候选 `combatPower × 2` 时返回空 → 站点 `SitePartWorker_Manhunters` 生成拿不到 animalKind（`GetAnimalsCount`）会 NRE。本项目以 `MinManhunterThreat` 抬升点数兜底（不用 Outpost 备选）。
- 出处：`decompiledFiles/RimWorld/AggressiveAnimalIncidentUtility.cs`、`RimWorld/SitePartWorker_Manhunters.cs`、`RimWorld/ManhunterPackGenStepUtility.cs`。
