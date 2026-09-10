# BUG 知识库

> 典型 Bug 记录与规范反哺闭环。每条 Bug 记录后检查是否暴露规范缺失。

## 记录模板

```markdown
## BUG-XXX | 标题
- 现象 / 根因 / 修复方案
- 是否规范缺失: 是 / 否
- 规范反哺: {若是 → 补充 B-01-开发规范.md 第X章规则 + AGENTS.md 强制规范新增一条 ✅/❌ 防复发}
- 关联: {出现次数}
```

---

## BUG-001 | Mote_Incantation 咏唱词渲染异常/截断
- 现象: 咏唱词纹理渲染为空/被截断/压缩变形。
- 根因: 早期用 GUI→RenderTexture 及字体图集 CPU 读取方式渲染文本，可靠性差；世界尺寸计算不当导致文字截断。
- 修复方案: 改用 Camera.Render + TextMesh + 正交相机烘焙为贴图；DrawAt 中若 tex 为占位或 null 则重查；displayH 设为 2.5f（世界单位），linearScale 复位 (1,1,1)，世界尺寸用 MeshRenderer.bounds 计算。
- 是否规范缺失: 是
- 规范反哺: B-01「七、技术栈特定规范」+ AGENTS.md「资源与性能规范」已补相机烘焙与尺寸计算约定。
- 关联: 次数 1

## BUG-002 | Vanish() 强制结束时进度回滚
- 现象: forcedVanish 时特效进度倒退。
- 根因: 未记录 forcedProgress 状态。
- 修复方案: Vanish() 记录 forcedProgress，防止强制结束时进度被重置。
- 是否规范缺失: 是
- 规范反哺: AGENTS.md「资源与性能规范」补充"强制中断路径需保持状态一致性"。
- 关联: 次数 1

## BUG-003 | HediffComp_Disappears 调用不存在的 SetDuration
- 现象: 编译/运行错误，方法不存在。
- 根因: 凭记忆使用不存在的 `SetDuration` 方法。
- 修复方案: 直接给 `ticksToDisappear` 字段赋值。
- 是否规范缺失: 是
- 规范反哺: AGENTS.md / B-01「AI 高频错误 #1」——先反编译核对 API，不存在的成员直接用字段。
- 关联: 次数 1

## BUG-004 | 升华奖励随机选择死循环
- 现象: 非零权重奖励耗尽后随机抽取死循环。
- 根因: 未判断可抽项权重总和（tt）为 0 的情况。
- 修复方案: Ascend 随机选择加入 `tt > 0` 条件，耗尽即返回。
- 是否规范缺失: 是
- 规范反哺: AGENTS.md / B-01「AI 高频错误 #5」——权重耗尽判断。
- 关联: 次数 1

## BUG-005 | Hediff 初始 severity=0 被立即移除
- 现象: 技能状态类 Hediff 刚添加即消失。
- 根因: `HediffWithComps` 默认 ShouldRemove 逻辑把 severity=0 的 Hediff 移除。
- 修复方案: `initialSeverity` 置正（>0）。
- 是否规范缺失: 是
- 规范反哺: AGENTS.md / B-01「XML 规范」——Hediff initialSeverity > 0。
- 关联: 次数 1

## BUG-006 | StatParts_Pharna.xml 早期 XML 闭合错误
- 现象: 加载时报 XmlException。
- 根因: XML 标签闭合不匹配。
- 修复方案: 修正闭合标签。
- 是否规范缺失: 是
- 规范反哺: B-03 测试指南「XML 静态校验」——提交前用 `[xml]` 解析全部 Defs/Patches；AGENTS.md 审查清单含 XML 校验。
- 关联: 次数 1

## BUG-007 | 嘲讽(Provoke)效果存读档后失效
- 现象: 存读档后嘲讽不拉仇恨，施放后敌单位不再回头攻击嘲讽者。
- 根因: `Provokers` 为 `static HashSet<Pawn>`，不随 Scribe 序列化；读档后(尤其进程重启)集合为空，`Patch_Provoke` 见 `Count==0` 直接 return，效果失效。
- 修复方案: 放弃运行时手动维护集合，改由效果 Hediff 的 `HediffComp_SkillInEffect` 生命周期回调增删 `Provokers`，并在 `Skill.ExposeData` 的 `PostLoadInit` 阶段按 `active` 重建集合。
- 是否规范缺失: 是
- 规范反哺: AGENTS.md「核心开发原则」补充——需跨存读档的运行时状态须 ExposeData 持久化或 Load 后重建；勿放不参与序列化的 static/非 ExposeData 字段。
- 关联: 次数 1

## BUG-008 | 回旋踢击退目标下次移动时闪回原点
- 现象: 被击退的目标在飞行结束后，下一次寻路移动时画面瞬间弹回击退起点。
- 根因: 击退飞行中直接改 `pawn.Position`，但未同步 pather 内部状态（`nextCell`/`cachedMovePercentage` 仍为击退开始时的旧值）；下次 `PatherTick` 时 `PawnTweener.TweenedPosRoot` 用旧 `nextCell` 计算渲染根位置，画面被拉回原点。仅 `StopDead()` 在 kick 时清一次不够，飞行中每格位移后又产生不一致。
- 修复方案: 每 tick 位移后调用 `pawn.Notify_Teleported(endCurrentJob:false, resetTweenedPos:false)`（内部 `pather.Notify_Teleported_Int()` = StopDead+ResetToCurrentPosition，把 nextCell 同步到当前位置；`resetTweenedPos:false` 保留平滑动画）。飞行期间可用 `pawn.stances.stunner.StunFor(…)` 使 `FullBodyBusy=true`，让 `PatherTick` 短路、彻底禁用寻路。
- 是否规范缺失: 是
- 规范反哺: AGENTS.md「资源与性能规范（RimWorld 特有）」新增"手动改变 Pawn/Thing 位置后必须同步 pather"与"原版无位移击退 API，需自写逐 tick 位移并配合 stun 禁用寻路"两条。
- 关联: 次数 1

## BUG-009 | 技能冷却时间单位填错（误填现实秒）
- 现象: 回旋踢 3 小时冷却配成 `cooldown=10800`，实际回充远慢于预期。
- 根因: 把"3 小时"按现实秒换算填进 XML，未核对基类 `CooldownTicks = def.cooldown * 60f`（单位是秒）与游戏内时间换算（1 游戏内小时=2500 ticks）。
- 修复方案: `cooldown = 3 游戏内小时 / 60 = 7500/60 = 125`。
- 是否规范缺失: 是
- 规范反哺: AGENTS.md「资源与性能规范（RimWorld 特有）」新增"游戏内时长换算先确认基类单位（cooldown 单位秒，1 游戏内小时=2500 ticks）"。
- 关联: 次数 1

## BUG-010 | 原版 util SlateRef 参数类型误用（sendAwayIfAllDespawned 传布尔）
- 现象: 罪犯追捕任务接取瞬间生成报错 `Could not convert SlateRef "$sendAwayIfAllDespawned" (String) to List<Thing>`（StackTrace 指向 `QuestNode_AddShipJob_Wait.AddJobVars`），任务无法生成。
- 根因: 向原版 util `Util_TransportShip_Pickup` 的 parms 传 `<sendAwayIfAllDespawned>true</sendAwayIfAllDespawned>`，按注释"send shuttle away if all things are despawned"误当作布尔开关；反编译确认 `QuestNode_AddShipJob_Wait.sendAwayIfAllDespawned` 实为 `SlateRef<List<Thing>>`——语义是"这些物品全部 despawn 即提前送走"的**物品名单**。字面量/字符串无法转换为 `List<Thing>`。
- 修复方案: 删除该 parms 项（Rescue 亦未传）；死亡/失踪已由 `quest.End` → util 自带 `SendTransportShipAwayOnCleanup` 即时送船，无需该功能。
- 是否规范缺失: 是
- 规范反哺: 属「AI 高频错误 #1」变体——XML/SubScript parms 传参同样须反编译核对字段类型与语义，勿按参数名/注释猜类型（存在 SlateRef<bool> 与 SlateRef<List<Thing>> 的歧义字段）。现有 #1 规则已覆盖，不新增条目。
- 关联: 次数 1

## BUG-011 | 罪犯追捕：通缉犯被扛起/装入运输容器被误判「失踪」
- 现象: 玩家把倒地通缉犯抱起搬运、装入空投仓或（Odyssey）自家穿梭机时，任务数拍内误判「通缉犯失踪」失败。
- 根因: 押送合法性判定以 `Spawned` 为主代理：pawn 被扛起（`ParentHolder` 变为搬运者 `Pawn_CarryTracker`）或装入任意运输容器（空投仓/穿梭机 CompTransporter、gravship 等）时会 `DeSpawn` 且不世界化，落入「未 spawn 且非玩家远行队 → 失踪」的错误分支；被扛着时还误判为"已离窝点"提前启动押解计时。
- 修复方案: 押送合法性改为四档判定——显式位置（窝点图/家图+玩家囚犯/玩家远行队）+ 被玩家成员扛起（`Pawn.CarriedBy`，搬运者须在场且玩家派系）+ **任意运输容器 = 押送待定（不判死，落定后按地图归属判定：落玩家据点继续、落他派系/无人地失败）** + 无主世界 pawn（失踪）；离图/计时判定改用「pawn 或搬运者所在地图」(`WantedLocationMap`)。
- 是否规范缺失: 是（建议）
- 规范反哺: 建议 AGENTS.md「核心开发原则」新增"pawn 存在性/位置判定勿以 `Spawned` 单断言，须组合 `Spawned/IsWorldPawn/CarriedBy/ParentHolder`；容器/搬运/载具态需单列"——是否纳入规则由用户确认，暂未写入。
- 关联: 次数 1

## BUG-012 | 自建 ThingDef 物品「能看见但无法选中/无法搬运」
- 现象: 遗迹探索任务物品（自定义 `FM_AncientRelicArtifact`，直接手写 `ThingDef`）从机关箱弹出后照常渲染、名字正常，但鼠标完全点不中（无信息卡）；右键无"搬到储存区"、小人也无法 haul；穿梭机也无法装载该物。
- 根因: 自建 `ThingDef` 没继承物品基类 `ResourceBase`（Core `Items_Resource_Base.xml`），漏了物品可交互的必要字段——`ThingDef.selectable` **默认 false**（未开 → 无法鼠标点选）、`EverHaulable = alwaysHaulable || designateHaulable` **默认 false**（未开 → 无法 haul/入储存/装仓）；且漏 `drawGUIOverlay/alwaysHaulable/Mass 等`。原版资源都经基类显式 `<selectable>true</selectable>`、`<alwaysHaulable>true</alwaysHaulable>`、`<drawGUIOverlay>true</drawGUIOverlay>`、`<Mass>` 开启。
- 修复方案: 自建物品 def 补齐与 `ResourceBase` 等价的字段：`<category>Item</category>` + `<selectable>true>` + `<alwaysHaulable>true>` + `<drawGUIOverlay>true>` + `<Mass>`(+`MaxHitPoints`)+ `CompProperties_Forbiddable`；**且必须给一个 `thingCategories`（`ThingDef.EverStorable` 要求 `thingCategories` 非空才返回 true，否则任何存储区 filter 的 `disallowNotEverStorable` 都不接受 → 即便"全部允许"也"无可用存储点"、无法搬运入库）**；或直接 `ParentName="ResourceBase"` 继承后覆盖。
- 是否规范缺失: 是
- 规范反哺: AGENTS.md「八、AI 高频错误」新增防犯——"自建 ThingDef 若非直接继承原版物品基类（`ResourceBase`），须显式补 `selectable/alwaysHaulable/drawGUIOverlay/Mass` 等字段，否则物品'可见却不可点选/不可搬运'；枚举与默认值(如 `Tradeability.None`、`ThingDef.selectable`、`EverHaulable`)须反编译核对默认值，勿臆想（此前还把 Tradeability 误写不存在的 `Never`）。"
- 关联: 次数 1

## BUG-013 | 任务穿梭机（util）装载: `requiredItems` 只判起飞、不触发装载
- 现象: 任务把目标物品放入 `requiredItems`、配 `leaveImmediatelyWhenSatisfied=true`，但穿梭机降落玩家据点后目标物**不会自动被搬上船**，玩家也"无法指定小人搬运至穿梭机"。
- 根因: 原版任务穿梭机（`Util_TransportShip_Pickup` 生成、`hideControls=true`）**选中时没有装载 gizmo**；`CompShuttle.requiredItems` 仅用于两件事——`IsRequired` 放行校验 + 起飞时判 `satisfied`（`innerContainer.TotalStackCountOfDef < required.Count`）。真正把物品加入待装清单、让殖民者自动搬运的是 **`CompShuttle.autoload`（`CheckAutoload` 每 120 tick → `AddToTheToLoadList`），该字段默认 `false`、无公开 setter，toggle gizmo 又被隐藏** → 无人触发装载，物品悬空等 2 天窗口结束失败。
- 修复方案: 在任务部件内「照抄」原版 `CheckAutoload` 的物品装载段（不改原版）：穿梭机落地后每节拍——若舱内目标物计数已满足则跳过；否则用 `TransporterUtility.AllSendableItems(group, map)` 找船所在图可送达的目标物，`清空 leftToLoad` 后 `comp.AddToTheToLoadList(new TransferableOneWay{things=匹配物}, count)` → 殖民者自动 haul 入舱 → 装齐 → `leaveImmediatelyWhenSatisfied` 起飞。
- 是否规范缺失: 是
- 规范反哺: AGENTS.md「核心开发原则」新增"复用原版 util/穿梭机交付时，`requiredItems` 只作装载校验与起飞判定，**不驱动装载**；任务船无 gizmo、`autoload` 默认关且无 setter——需在部件内自行把 required 物加入待装清单（`AddToTheToLoadList`）实现自动装载"。另提示协作：**原版任务穿梭机 ≠ 玩家自有穿梭机（Odyssey `Ship_PassengerShuttle`）**，交互能力不同，勿混用（载人走"选中 pawn 右击穿梭机 floatmenu 登机"）。
- 关联: 次数 1

## BUG-014 | 协作经验：勿臆测机制/现象，先以原版事实与现场证据对齐
- 背景: 遗迹探索任务联调中，本 agent 曾：① 凭印象把任务穿梭机当"有装载 gizmo 的载具"设计装载交互；② 对"物品点不中/无法 haul""穿梭机不装货"等现场现象靠猜根因而非求证，反复返工。用户提示「原版的 thing 并不是直接就是物品」「任务穿梭机与玩家穿梭机是两个东西」「对测试中的实际现象有疑问直接提出，不要臆测」。
- 经验/纠正:
  - 原版中"一个能交互的物品"需具备成套 def 字段（category/selectable/alwaysHaulable/EverHaulable/drawGUIOverlay/mass…），"能看到"≠"可选中/haul"；任务用特殊工具与 DLC 载具是不同对象，能力面不同。
  - 交付/装载类任务先确认玩家可用的原版交互路径（gizmo? floatmenu? 自动?），再定设计；不确定就先读源码/问用户，不自作假设。
  - 现场 Bug（点不中/装不上/存储区报错）先向用户收集：def 版本是否最新、物品所在状态（地面/背包/容器）、报错原文，再定位；不臆测单一根因。
- 是否规范缺失: 是
- 规范反哺: 已纳入 `AGENTS.md`「四、强制规范 · 核心开发原则」——"Bug 溯源/修复/调试中勿臆测实际现象：先确认对象与机制身份、原版交互路径，收集现场状态（版本/位置/日志/报错原文）；有疑问直接向用户提出，再动手"。
- 关联: 次数 1

## BUG-015 | 任务 util 穿梭机装物品：把物塞进 toLoad 但不 InitiateLoading → HaulToContainer 秒败死循环
- 现象: 遗迹探索交付阶段给穿梭机 `AddToTheToLoadList` 塞 required 物后，出现连续刷屏 `Yang started 10 jobs in one tick ... HaulToTransporter`（小人反复 StartJob→立即 EndJob(Fail)→再派，无限循环）。
- 根因（经源码深挖 + 用户提问校准）: 穿梭机是**双层 comp**——`CompShuttle : ThingComp`（逻辑/起飞/autoload/playerShuttle），货舱与装载在另一独立 comp `CompTransporter`（`CompShuttle.Transporter => parent.GetComp<CompTransporter>()`）。`JobDriver_HaulToContainer`（HaulToTransporter 的基类）有 `FailOn(TransporterUtility.WasLoadingCanceled(Container))`，而 `WasLoadingCanceled = comp != null && !comp.LoadingInProgressOrReadyToLaunch`；`LoadingInProgressOrReadyToLaunch` 在 `groupID < 0` 时仅 `parent.IsInCaravan()`——**只有 `TransporterUtility.InitiateLoading` 分配 groupID 后船才处于可装载态**。util 任务船（hideControls、非 playerShuttle、autoload 关、无 gizmo）默认从不 InitiateLoading → 任何物品 `HaulToTransporter`（自动或手动 WorkGiver 强制）首帧即被该 FailOn 判失败 → job 失败后 toLoad 仍有货 → 无限重派。
- 关键参照: 「抱囚犯上船」`FloatMenuOptionProvider_CarryToShuttle` 也用同一 `HaulToTransporter`（A=囚犯,B=船），但其在派 job 前**先 `TransporterUtility.InitiateLoading`**（L48-50），故不失败——**物品与被抬的囚犯装载是同一驱动 API，前置同为"船须先 InitiateLoading"**；区别只是装物品的 `WorkGiver_LoadTransporters.JobOnTransporter` 不负责 InitiateLoading。
- 修复方案: 部件内穿梭机落地后（幂等）——若 `!Transporter.LoadingInProgressOrReadyToLaunch` → `TransporterUtility.InitiateLoading(单船)`；再把 required 物**幂等**加入 toLoad（已在则跳过、绝不清空重建）→ WorkGiver 自动/玩家强制 `HaulToTransporter` 均能完成（有搬运演出）→ 装齐起飞。
- 是否规范缺失: 是
- 规范反哺: AGENTS.md「AI 高频错误」防犯补一条——"给 transporter/shuttle 手动排待装(toLoad)前，必须先确认 `LoadingInProgressOrReadyToLaunch`（必要时 `InitiateLoading`），否则 `JobDriver_HaulToContainer` 被 `WasLoadingCanceled` 秒判失败并死循环；装载 job 与载人(CarryToShuttle)同机制，勿自造驱动"。
- 关联: 次数 1

## BUG-016 | 机关箱落位校验误用 Fogged → 进图当拍生成失败、延迟到玩家揭示
- 现象: `TrySpawnCrateAndRelic` 在进图时落位失败（日志可见生成尝试自动失败），仅当玩家深入遗迹揭示迷雾后才轮询成功 → 机关箱"延迟出现"。
- 根因: `CanPlaceAt` 落位校验要求 `!Fogged`；地图刚生成（MapGenerated/首个节拍）时复杂体整体处于迷雾 → 全部候选格被 Fogged 判否 → 当拍生成失败。
- 修复方案: 落位校验去掉 `Fogged` 检查（root 与占格）；雾中生成本身安全（原版 genstep 全部在雾中落物，玩家靠近自然揭示）。
- 是否规范缺失: 是
- 规范反哺: AGENTS.md「资源与性能规范（RimWorld 特有）」补一条——"地图生成期/首节拍的自建落位校验不要用 `Fogged` 当排除条件（生成时全图在迷雾，会误杀所有候选）；迷雾对 spawn 无副作用"。
- 关联: 次数 1

## BUG-017 | 外部构造 GuildQuestGenerator 漏绑 def → 强制任务调试指令必现 NRE / 日常任务静默丢奖励
- 现象: 新增调试指令 `DebugAction_Quest.TriggerGuildEnvoyRankUp` 一触发就抛 `NullReferenceException`（控制台红字）；同期的日常调试指令不崩，但任务完成后**升级点静默不发放**。
- 根因: 调试菜单自行 `Activator.CreateInstance(questDef.generatorClass)` 构造生成器，**漏了 `gen.def = questDef`**（重构前入口是 `SpawnEnvoyWithQuest(GuildQuestDef, Map)`，由其内部赋值；改成传 generator 后这一步丢失）。而 `GuildQuestGenerator` 的 `Category / Type / LetterText` 全部由 `def` 派生：`Category => def?.category`（null → `slate.Set("category", null)`）、`Type => def?.type ?? QuestType.Daily`（null → 恒判为日常）。
  - 强制侧：`GameComponent_FamiliaRankTracker.StartRankUpQuest` 内 `GetOrAddState(gen.def.category)` 直接 NRE；且该行位于**使者已 GenSpawn、lord 已建、`GuildEnvoyTracker.Register` 已注册、`SummonQuest` 已生成任务之后** → 产生一个 tracker 不认识的**孤儿强制任务**（未记 `rankUpQuestID`），下轮 1000 tick 轮询会再发一个，可叠加。
  - 日常侧：`def?.label / LetterText` 有兜底所以不崩，但 `slate.Set("category", null)` → `QuestNode_GainUpgradePoints` 的 `if (cat == null || amt <= 0) return;` → **升级点静默丢失**。
- 修复方案:
  1. `GuildQuestGenerator.Create(GuildQuestDef def)` 统一静态工厂（实例化 + 绑定 def + `generatorClass` 缺失时 `Log.Error`），全部创建点改走它：`FamiliaCategoryDef.GetAvailable{Daily,RankUp}Quests`、`DebugAction_Quest` 两处、`GuildQuestDef.Generator` 缓存属性。
  2. `StartRankUpQuest` / `SpawnEnvoyWithQuest` 增加入口校验（`gen?.def == null || map == null` → `Log.Error` + false），且**校验提到任何副作用之前**；`StartRankUpQuest` 内 `GetOrAddState(...)` 返回值判空（category 无效时报错而非 NRE）；`gen.SetupParams(map)` 收进这两个入口，调用方不再需要各自记忆。
  3. 调试菜单的 `Available()` 预筛改为「标注【不可用】但仍允许强制触发」（调试期正需要跨过预筛验证）。
  4. 顺带清掉 `GameComponent_FamiliaRankTracker` 里 IDE 自动加入的无用 `using System.Drawing;` / `using Verse.Noise;`。
- 是否规范缺失: 是
- 规范反哺: B-01「RimWorld 对象模型」补两条（Def 派生型生成器必须绑 def + 统一工厂；对外入口的依赖校验须前置于副作用）；AGENTS.md「四、强制规范 · 核心开发原则」新增对应 ❌/✅。
- 关联: 次数 1

## BUG-018 | 难民安置：复用原版延迟回报部件时缺 rewardGiver → 满载结算必现 `Grammar unresolvable`
- 现象: 强制任务「难民安置」在**满载起飞线**（`CompShuttle.SendLaunchedSignals` 发 `SentSatisfied`）结算时控制台红字 `Grammar unresolvable. Root 'root'`，解析轨迹中 `rewardGiver_nameDef / rewardGiver_objective / rewardGiver_pronoun / rewardGiver_possessive → UNRESOLVABLE`，随后附带 `Failed to resolve text. Trying again with English.`；栈为 `QuestPart_RefugeeResettle.Settle` → 发 `rewardsReady` → `QuestPart_AddQuestRefugeeDelayedReward.AddQuest` → `QuestGen.Generate` → `QuestNode_ResolveTextRequests.Resolve` → `GrammarResolver`。
- 根因（两层）:
  1. **结算当拍漏了给登机者归还原派系**（项目「紧急救援」在 `SentSatisfied` 当拍有 `RestoreOriginalFactions()`，抄漏）。`SentSatisfied` 发出时登机者**仍在穿梭机内容器里且 Faction 仍是玩家**（见 C-05 第四节窗口），加上被劝加入者也在玩家派系 → 原版 `QuestPart_AddQuestRefugeeDelayedReward.GetSlate()` 的候选条件（`!Dead && Faction != Faction.OfPlayer && !IsPrisoner`，`QuestPart_AddQuestRefugeeDelayedReward.cs:24-31`）**一个都不满足** → 子任务 slate 里没有 `rewardGiver` → 其文本变量无法解析。旁证：原版难民任务在同一部件上无此问题，因为它的难民在绑定信号（`outSignalLast_LeftMapAllHealthy`）之前已被 `LeaveQuestPartUtility.MakePawnLeave` 归还原派系。
  2. **原版部件不校验隐式前置**：候选缺失时它照样生成子任务（`QuestPart_AddQuest.AddQuest` → `QuestUtility.GenerateQuestAndMakeAvailable`），直到文本解析期才炸——错误位置离根因很远，排查成本高。
- 修复方案:
  1. `QuestPart_RefugeeResettle` 增加 `RestoreBoardedFaction()`：结算当拍对仍在机舱内的登机者 `SetFaction(refugeeFaction)`，**成功线与失败线都调用**（失败线若已有人登机同样需要归位，否则会带着玩家派系飞走）；
  2. 新建 `QuestPart_AddQuestRefugeeDelayedRewardSafe : QuestPart_AddQuestRefugeeDelayedReward`，覆写 `CanAdd` 做与原版 `GetSlate` **完全一致**的候选预检，无候选时静默跳过（`QuestPart_AddQuest.Notify_QuestSignalReceived` 的 `&& CanAdd` 会拦住生成）；
  3. `QuestNode_RefugeeResettleSetup` 改用 Safe 版。
- 是否规范缺失: 是
- 规范反哺: 已补 —— AGENTS.md「四、强制规范 · 核心开发原则」新增两条 ✅（复用原版部件前先反编译核对隐式前置条件；跨系统归位/移交与信号窗口对齐）+「八、AI 高频错误防犯清单」第 11/12 条；`docs/B/B-01-开发规范.md`「七 · RimWorld 对象模型」补两条 ✅ + 「八」第 11/12 行；`docs/C/C-05-原版API简述.md` 收录该部件的隐式前置与规避方式（§一）。
- 关联: 次数 1

## BUG-019 | 难民安置：成功线离场名单未排除「已转正者」→ 加入殖民地的难民在结算当拍被赶走
- 现象: 满载/部分登机结算时，已被玩家劝加入（已转正）的难民会被一并转出玩家派系、并安排走向地图边缘离场——与「加入视为存活、留在殖民地」的设计直接矛盾（真跑起来就会看到刚加入的人立刻走人）。
- 根因: `QuestPart_RefugeeResettle.Settle()` 成功线调用 `Dismiss(OnMapAlive().Where(p => !IsBoarded(p)))`，过滤条件**只排除了「已在穿梭机机舱内的登机者」**；而 `OnMapAlive()` 返回的是 victims 中「全部存活且在图上者」，天然包含 `adopted`（已转正者）→ 他们被当成「未登机滞留者」处理。
- 修复方案: 成功线离场名单补 `&& !adopted.Contains(p)`；失败线**保持包含 adopted**（用户确认：失败时含已转正者一并转中立离场，唯一例外是被囚禁/奴役的受害者本人）。该修正同时是「全员被劝加入 → 直接结算完成」路径的前置（否则全员加入 = 全员被赶走）。
- 是否规范缺失: 是（从公共名单派生「待处理子集」时，应排除的对象类别必须逐项显式列出并与设计口径对齐）
- 规范反哺: 已补 —— AGENTS.md「四、强制规范」新增 ❌（从公共名单派生「待处理子集」时漏列应排除类别）+「八」第 13 条；`docs/B/B-01-开发规范.md`「RimWorld 对象模型」补一条 ✅ + 「八」第 13 行。
- 关联: 次数 1
