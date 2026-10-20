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
- 现行实现: 升华奖励抽取已改为 `TryRandomElementByWeight`（权重和 ≤ 0 时返回 false）并在候选池为空时报错；零权重奖励只登记不入池（见 C-02 / C-03）。
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

## BUG-020 | 类名重命名未同步 XML 引用 → 加载期 `Could not find a type named ...`
- 现象: 游戏加载时报红字 `Could not find a type named FamiliaMyth.Abilities.DevelopableAbiility_AbnormalResistance`，导致「异常抗性」发展能力 Def 加载失败（该能力无法获得）。
- 根因: 提交 `cf9545d`（"修复了部分命名错误"）把源码类名由错拼的 `DevelopableAbiility_AbnormalResistance` 修正为 `DevelopableAbility_AbnormalResistance`，但**漏改** `Defs/DevelopableAbilityDefs/DevelopableAbilities.xml` 里的 `<abilityClass>` 引用；且源文件名一直带着同一处错拼，掩盖了不一致。
- 修复方案: XML 引用改为正确类名；源文件重命名为 `DevelopableAbility_AbnormalResistance.cs`；并全库扫描 `Defs`/`Patches` 内全部 `FamiliaMyth.*` 类型引用与源码类型定义比对（当时为 104 处引用 / 222 个类型，其余均可解析；后续实现已增至 105 处 / 223 个类型）。
- 是否规范缺失: 是（重命名类型后必须**全库同步 XML 类型引用**，且该同步不能只靠人工记忆）
- 规范反哺: `12-本地化检查.ps1` 新增第 7 项「XML 类型引用 → 源码类型可解析性」自动兜底（自动跳过 XML 注释；`Type.Method` 形式的方法引用视为合法）；`docs/C/C-03-项目长期记忆.md` 记录脚本 7 项。
- 关联: 次数 1

## BUG-021 | Def 字段填写 Keyed 键不会触发翻译 → 英文环境显示原始键名
- 现象: 英文环境下美德系统的 Hediff 名称、精神状态信件标题/正文显示为 `FamiliaMyth.VirtueBuff.Stalwart.Label` 之类的原始键名；英文 Keyed 里对应的 44 个键从未生效。
- 根因: 误以为「Def 字段里填 Keyed 键会被自动翻译」。实测反编译：`Verse/Def.cs` 的 `LabelCap => label.CapitalizeFirst()`、`TaggedString(string)` 仅赋值、`Verse.AI/MentalState.cs` 的 `GetBeginLetterText()` 均**不 Translate**；`LoadedLanguage` 的 `Keyed` 与 `DefInjected` 是两套独立加载。中文之所以显示正常，纯粹是有 `DefInjected` 覆盖兜住（英文缺 `MentalStateDef`/`ThoughtDef` 两个目录，于是露键）。
- 修复方案: 58 处 Def 字段的键占位改回**实际文本**（Def 内保留为默认/兜底），玩家可见文本统一下沉到 `DefInjected`；补齐英文；Keyed 里 45 个死键删除。
- 是否规范缺失: 是（四条文本路径混用）
- 规范反哺: AGENTS.md「五、基础代码规范 · 本地化」重写为四条路径说明 + 两条 ❌；`docs/C/C-03-项目长期记忆.md` 记录四条路径与取证；新增 `docs/C/C-06-本地化术语表.md`。
- 关联: 次数 58（同一写法）

## BUG-022 | 美德文本两则：beginLetter 漏字 + 振奋 thought 自身/同伴共用导致叙事错位
- 现象: ①「振奋」美德开始信件显示为 `{0}的精神不再：[PAWN_pronoun]现在振奋昂扬`（"精神不再"后缺半句）；②触发者本人读到的心情记忆是「**同伴的**振奋让我也感到精神振奋」——而他自己就是振奋的源头。
- 根因: ①纯笔误漏字；②`Hediff_VirtueBuff_Vigorous`（自身主 Buff）与 `Hediff_VirtueAura_Vigorous`（范围光环）**共用同一条 `ThoughtDef`** `Thought_Virtue_Vigorous_Morale`，而该 thought 文案是站在"被同伴感染"的视角写的。
- 修复方案: ①重写该句（顺带统一五种美德的 beginLetter 口吻为"精神连结/自我振作"）；②新增 `Thought_Virtue_Vigorous_Self` 专供触发者本人，Morale 保留给受光环影响的同伴；主 Buff 的 `HediffCompProperties_ThoughtSetter.thought` 改指 Self，中英双语补键。
- 是否规范缺失: 否（一次性笔误 + 复用组件时未核对叙事视角）
- 规范反哺: `docs/C/C-06` 增补「美德心声」小节，并注明该类键名由代码动态拼接、`12-本地化检查.ps1` **抓不到**，改动后须人工核对中英对称。
- 关联: 次数 1

## BUG-023 | 美德启动失败残留 `currentVirtue` → 静默吞掉一次精神崩溃
- 现象: 偶发「该崩溃却没崩溃、也没有美德触发」——小人情绪崩溃当拍没有进入任何精神状态，且此后一段时间内也不再触发美德。
- 根因: `Hediff_Pharna.TryTriggerVirtue` 原写法 `currentVirtue = type; return pawn.mindState.mentalStateHandler.TryStartMentalState(def);`。当 `TryStartMentalState` 返回 false（未能启动，如已在其他精神状态中）时，`currentVirtue` 已赋值却不存在对应美德；此后 `Patch_MentalBreaker` 开头的 `if (pharna.currentVirtue.HasValue) { __result = false; return false; }` 会认定"已在美德中"而**直接放行 return false**，把这次崩溃凭空消除。又因状态从未开始，`PostEnd` 不会执行，`currentVirtue` 迟迟得不到清理。
- 修复方案: `TryStartMentalState` 返回 false 时回退 `currentVirtue = null` 并 return false。**注意不能把赋值挪到启动之后**——`MentalState_Virtue.PostStart` 依赖 `pharna.currentVirtue` 取美德类型。
- 是否规范缺失: 是（"先写共享状态、再执行可能失败的动作"必须在失败路径回滚，否则残留值会被其他系统误读）
- 规范反哺: AGENTS.md「四、强制规范 · 核心开发原则」新增一条 ✅；`docs/C/C-02` 意志/美德段与 `C-03` 约定同步记录。
- 关联: 次数 1

## BUG-024 | Keyed 文本用 `{0:P0}` 等格式说明符 → 参数位置静默显示为空
- 现象: 新增的信息卡展示项 `ManaConversionSpeed`（魔力转化速度），其解释文本中三个参数位置**全部为空**（显示成 `魔力转化效率  × 当前意识 `）。而键本身中英对称、`12-本地化检查.ps1` 七项全绿、游戏日志也没有任何红字，极易误判为「键没生效」或「参数没传进去」。
- 根因: `"Key".Translate(args)` **不走 `string.Format`**，而是 `TranslatorFormattedStringExtensions.Translate(key, params NamedArgument[])` → `TaggedString.Formatted` → `Verse/GrammarResolverSimple.cs`。该解析器（`TryResolveInner`，line 155-168）**一遇到 `:` 就切进「函数调用」模式**并停止读占位符名，于是 `{0:P0}` 被拆成「函数名 `0` + 参数 `P0`」；再经 `Verse/LanguageWorker.cs` 的 `ResolveFunction`（line 254-264）判定——它只认 `lookup` / `replace`，其余分支**直接 `return ""`**，既不 Log 也不 Warning。三个占位符因此同时变成空串。（`Verse/Translator.cs` 里那个 `params object[]` 的旧重载已标 `[Obsolete]`，根本没被调用。）
- 修复方案: 语言文件只写裸 `{0}` `{1}` `{2}`；格式化前移到参数侧，如 `efficiency.ToStringPercent()`、`speed.ToString("F1")`。项目既有键（`FamiliaMyth.Stat.PharnaStatus` 等）一直传的就是已格式化好的 `int`，正好印证此惯例。
- 是否规范缺失: 是（AGENTS.md / B-01 只约束了「四条文本路径」，未约束占位符语法；坑点静默无日志，本地化检查脚本也抓不到）
- 规范反哺: AGENTS.md「五、基础代码规范 · 本地化」新增一条 ❌；`docs/B/B-01-开发规范.md`「七 · 本地化文本」新建小节；`docs/C/C-03-项目长期记忆.md` 该条补齐「未知函数静默返回空串、无日志」与反编译行号。
- 关联: 次数 1（同一处文本内 3 个占位符同时失效）

## BUG-025 | 删除枚举成员未同步 XML 字面值 → Def 加载抛 `'X' is not a valid value for ...`
- 现象: 伟业系统落地时 `PharnaDisplayMode` 的 `Feats` 成员被删除（信息卡不再展示「伟业值」），但 `Defs/StatDefs/PharnaStats.xml` 的 `StatDef "PharnaFeats"` 仍写着 `<mode>Feats</mode>`。若照原样保留，加载期该 Def 会直接抛枚举解析错误、**整个 Def 加载失败**（不是静默降级）。本次在清理展示项时同步发现，连同该 StatDef 一并删除，未流入实机。
- 根因: `Verse/ParseHelper.cs` 的 `FromString<T>`（line 418-436）对枚举类型走 `Enum.Parse`，失败时抛 `ArgumentException`，消息形如 `'Feats' is not a valid value for FamiliaMyth.Abilities.PharnaDisplayMode. Valid values are: ...`。XML 里的枚举字面值与 C# 枚举定义**没有编译期关联**——删成员不会让 `dotnet build` 出任何错。
- 修复方案: 删除或重命名枚举成员前，全库检索 XML 中该成员的**裸字面值**（如 `<mode>X</mode>`），确认无引用后再动代码，或同步清理引用处。
- 是否规范缺失: 是（BUG-020 覆盖了「类名/类型引用」，未覆盖「枚举与数值这类无命名空间前缀的 XML 字面值」）
- 规范反哺: 与 BUG-020 合并理解为同一条自查项——「改动 C# 侧标识符（类型名、枚举成员名）后必须全库检索 XML 同步」；`12-本地化检查.ps1` 第 7 项只能兜住带命名空间的类型引用，裸字面值仍须人工检索。
- 关联: 次数 1（`PharnaFeats` 一处）

## BUG-026 | 升华奖励弹窗符文取随机字符未校验空池 → 索引越界
- 现象: 战局中玩家派系无殖民者（或在打开弹窗前全部离场 / 死亡）时，`Dialog_AscendReward` 的符文层每次重绘都抛 `IndexOutOfRangeException`；异常被原版 `Window` 的 try/catch 兜住并 `Log.Error`，表现为边框符文缺失 + 日志刷红字。
- 根因: 符文候选字符池由"遍历当前地图上玩家殖民者的名字逐字符收集"动态构建，原实现初始化为空数组。池为空时 `hieroglyphSymbols[Rand.Range(0, 0)]` 取索引 0 越界——`Rand.Range(0, 0)` 本身不报错、只返回 0，异常在**索引取值处**才炸。
- 修复方案: 收集完仍为空时填入兜底字符集（`FallbackSymbols`）再构建符文列表。
- 是否规范缺失: 是（AGENTS.md「八、AI 高频错误」#5 只覆盖"权重耗尽死循环"，未覆盖"空集合取随机索引"）
- 规范反哺: AGENTS.md「八、AI 高频错误」新增 #15、「七、代码审查清单」补一条自查项、`docs/B/B-01-开发规范.md` 补随机取样小节；`docs/C/C-03-项目长期记忆.md` 新增一条约定。
- 关联: 次数 1

## BUG-027 | 历史窗口页签依赖"非法枚举哨兵 + 反射"，原版字段或枚举变动即静默失效（预防性加固）
- 现象: 尚未实机发生。`HistoryTabHelper` 以 `Sentinel = 99` 写入原版 `MainTabWindow_History` 的 private static `curTab` 字段（该枚举并无 99 号成员），并反射读写 `tabs` 列表。
- 根因: 该接入依赖原版私有实现细节（字段名、字段类型、枚举取值空间），且缺省路径无任何前置校验——"反射拿不到字段"与"哨兵值被原版占用"两种情况都只在绘制期暴露：前者 `curTabField.GetValue` 直接 `NullReferenceException`，后者会把原版页签误判为眷族页。
- 修复方案: 新增静态 `Usable` 校验——字段缺失 / `Enum.IsDefined` 判定哨兵被占用 / 类型校验抛异常时 `Log.Error` 并停用自建页签（`GetTabs` 返回空、绘制 Prefix 直接放行原版）；`SelectFamilia` 与 `IsFamiliaTabActive` 也先查 `Usable`。
- 是否规范缺失: 否（属既有原则"复用原版私有实现细节必须先做可用性预检"的延伸）
- 规范反哺: `docs/C/C-02-架构详解.md` 与 `docs/C/C-04-已实现内容清单.md` 记录兜底语义。
- 关联: 次数 1

## BUG-028 | 自管 Spell 列表读档后 `Ability.pawn` 为 null → 读档首拍 NRE 并清掉整个恩惠
- 现象: 读档完成后下一拍，`Player.log` 报 `Exception ticking hediff (Hediff_Pharna 躯干 ticksSinceCreation=…) for pawn 赵璧. Removing hediff...`，栈为 `Ability.get_Casting` ← `Ability.CastingTick` ← `Ability.AbilityTick` ← `Spell.AbilityTick` ← `Hediff_Pharna.PostTick`（异常偏移 `[0x0000d]`）；原版 `Pawn_HealthTracker.HealthTick` 的兜底 `RemoveHediff` 把该 Hediff 直接删除，恩惠等级 / 六维能力值 / 经验 / 伟业 / 魔力池 / 技能 / 发展能力一并丢失。
- 根因: `Hediff_Pharna` 用 `Scribe_Collections.Look(ref spells, "spells", LookMode.Deep)` 自管 `Spell`（继承原版 `Ability`）列表，而 `Ability.pawn` 与 `verb.caster` 是继承来的普通字段、**不参与 Scribe**。原版对同类自管列表在 `Verse.Hediff.ExposeData` 的 `LoadSaveMode.PostLoadInit` 阶段显式补 `ability.pawn = pawn; ability.verb.caster = pawn;`，本模组的 `PostLoadInit` 只补了 `feats`，漏掉这一步 → 读档后每个 `Spell` 的 `pawn` 均为 null。随后 `Ability.Casting` 的首个条件 `!verb.WarmingUp` 因 `Verb.WarmupStance` 在 `CasterPawn == null` 时直接 `return null` 而安全成立，代码继续求值 `pawn.jobs` → NRE（异常 IL 偏移正落在 `ldfld Ability::pawn` / `ldfld Pawn::jobs` 所在基本块）。`Skill` 与 `DevelopableAbility` 各自 `Scribe_References.Look(ref pharna)` 并经 `pharna.pawn` 间接取 Pawn，故不受影响。
- 修复方案: 在 `Hediff_Pharna.ExposeData` 的 `PostLoadInit` 分支内遍历 `spells` 重建 `spell.pawn = pawn; spell.verb.caster = pawn;`（与原版 `Hediff` 的处理保持一致）。
- 是否规范缺失: 是
- 规范反哺: B-01「七、技术栈特定规范 · RimWorld 对象模型」新增"自管原版类型集合须连原版 `PostLoadInit` 重建一并照搬"一条；AGENTS.md「核心开发原则」新增对应 ✅ 条目、「八、AI 高频错误防犯清单」新增第 16 项。
- 关联: 次数 1

## BUG-029 | 批量烘焙咏唱词纹理时逐句叠加（后一句叠在前一句上）
- 现象: 用调试指令批量导出 76 张咏唱词 PNG 时，只有第一张干净，第二张起都是「当前句叠在之前所有句之上」；改为施法时逐句触发却完全正常。
- 根因: `UnityEngine.Object.Destroy()` 是**帧末延迟销毁**。导出循环在同一帧内连续调用 `BuildTexture`，每次 `bakeCam.Render()` 时上一句的 `bakeGo`（网格 + 材质，且都摆在世界原点、正落在正交相机视野内）尚未真正销毁，于是被一并渲染进当前纹理。运行时施法两次之间隔着大量帧，`Destroy` 早已生效，故该缺陷只在批量调用下暴露。
- 修复方案: 烘焙的临时对象（`bakeGo` / `camGo` / `rt` / `mat` / `bakeMesh`）与导出用临时纹理一律改用 `DestroyImmediate`，并在 `finally` 处留注释说明原因（避免以后被改回 `Destroy`）。
- 是否规范缺失: 是
- 规范反哺: B-01「七、技术栈特定规范」新增"同帧批量创建/销毁临时对象须用 DestroyImmediate"；AGENTS.md「资源与性能规范」与「八、AI 高频错误防犯清单」新增对应条目。
- 关联: 次数 1

## BUG-030 | 字形请求尺寸与查询尺寸不一致 → 动态字体字形数据错乱、全叠在起点
- 现象: 批量导出的咏唱词纹理中，单句内所有字符叠在同一位置（纹理内容错乱），第一句却正常。
- 根因: 动态字体要求"按同一尺寸请求、按同一尺寸查询"。当时 `RequestCharactersInTexture(s, 96, Bold)` 与 `GetCharacterInfo(c, out ci, 0, Bold)` 的尺寸不一致（`0` 表示字体默认尺寸），图集内不同尺寸的字形副本与图集重建时机相互纠缠，取回的 `CharacterInfo` 位置与前进量随之错乱。
- 修复方案: 引入 `bakePixelSize` 常量，请求与查询统一使用它；同时把字形光栅化尺寸由 96 提到 512（原 96px 字形要填满 512px 高的纹理，等于放大 5 倍以上），并用 `worldPerPixel = 9.6f / bakePixelSize` 保持字形世界高不变。
- 是否规范缺失: 否（属对 Unity 动态字体语义的理解偏差，非流程遗漏）
- 规范反哺: 无需新增规范；结论记入 C-03「运行时与构建运维约定」。
- 关联: 次数 1

## BUG-031 | 滚动视窗高度漏算分区卡外壳 → 最后一个分区的末尾条目滚不出来
- 现象: 恩惠页签内容变长后，伟业分区最后一张卡片始终卡在滚动视窗外，滚到底也看不到完整内容（卡片更多时，最后两张都有部分被裁掉）。
- 根因: `ITab_Pawn_Pharna.MeasureHeight()` 用各分区的**内区高度**（`AbilityInnerHeight()` / `DevelopableInnerHeight()` / `SkillSpellInnerHeight()` / `FeatInnerHeight()`）直接相加，而绘制侧每个分区实际占用的是 `SectionHeight(inner) = UI_GroupBox.TitleHeight(30) + BasicMargin×2(8) + inner`——**测量漏掉 4 份外壳共 152px**，比一张伟业卡（`FeatCardH + FeatCardGap = 78`）还高，于是最后一行必然落在 `Widgets.BeginScrollView` 的裁剪区之外。
- 修复方案: `MeasureHeight()` 改为累加 `SectionHeight(...)`（与绘制侧调用同一个函数），并追加 `ScrollBottomPad(8)` 作为底部留白。`SkillSpellInnerHeight` / `FeatInnerHeight` 末尾已含一个条目间隔，属安全方向的富余，未再收敛。
- 是否规范缺失: 是（`UI_Cursor` 的既有设计意图正是「使高度测算与绘制共用同一套步进」，本次违反的正是它）
- 规范反哺: `docs/C/C-03-项目长期记忆.md` 的 UI 约定追加一条——凡「测量—绘制」两处都要计算的分区高度，必须调用同一个高度函数（`SectionHeight`），不得在测量侧自行用内区高度相加。
- 关联: 次数 1

## BUG-032 | Keyed 值以 `x` 开头 → 属性说明栏显示成大写 `X`（机制未查明，已规避）
- 现象: 恩惠身价倍率的说明栏两行（`FamiliaMyth.Stat.PharnaWealthLevel` / `...WealthExperience`）在游戏内显示为 `X` 开头，而语言文件里写的是小写 `x`。
- 根因: **未查明**。以下环节已逐行核对且均不改大小写：`Verse/Translator.cs:53-65` `Translate` 原样返回（`PseudoTranslated` 在 `Translator.cs:109-118` 非 DevMode 直接 return original）、`Verse/TranslatorFormattedStringExtensions.cs:5-47` 仅转调、`Verse/ColoredText.cs:155` `Resolve` 只处理颜色标签、`RimWorld/StatWorker.cs:847-851` 与 `:889-901` 原文拼装、`RimWorld/StatDrawEntry.cs:158-169` 原文透传。全工程唯一能把小写变大写的只有 `Verse/TaggedString.cs:27-47` `CapitalizeFirst()`（只翻首字母；整串 `ToUpper()` 仅在串长为 1 时触发），而属性说明链路上无人调用它（`RimWorld/StatsReportUtility.cs` 中两处调用只在 `:220` / `:343` 的标签/标题）。两文件该行 `x` 均为 ASCII 码点 120，`Player.log` 无 `Exception translating`。
- 修复方案: 键值只保留标签（`恩惠等级 (Lv.{0}) 乘数` / `Pharna level (Lv.{0}) multiplier`），**把倍数符号 `": x{0}"` 移到代码侧与 `ToStringPercent()` 结果拼接**（`StatPart_PharnaWealth.cs` 的 `ExplanationPart`）——与全库原版 101 处调用点同一写法（`RimWorld/StatPart_Age.cs:39`、`RimWorld/StatPart_Quality.cs:57` 等），改动后显示正常。
- 是否规范缺失: 否（规避写法已与原版惯例一致）
- 规范反哺: `docs/C/C-03-项目长期记忆.md` 的「四条文本路径」追加一条——纯前缀/符号类文本（如倍数符号 `x`）应拼接在代码侧，不写进 Keyed 值。
- 关联: 次数 1

## BUG-033 | 用 `Thing.HostileTo(Faction)` 判敌对 → 致死一击的伤害被判「非敌对」而漏记
- 现象: 伟业结算的「合账」行里 `敌方承受` 与 `我方输出` 差出一大截（实测 1066.9 对 401.5，Σ占比仅 37.6%）；单人近战清场的个人占比也只有 44%~49%。原版记录页签显示的「造成总伤害」却是完整的。
- 根因: `GameComponent_FeatTracker.RecordDamage` 用 `victim.HostileTo(Faction.OfPlayer)` 判「打到的是敌人」。该方法解析到 `GenHostility.HostileTo(this Thing t, Faction fac)`，首行即 `if (t.Destroyed) return false;`（`kb/L0-证据/decompiled/1.6/RimWorld/GenHostility.cs:118-123`）。1.7 的死亡路径在伤害结算阶段就销毁了原 Pawn，而本模组的归因回调挂在 `Pawn.PostApplyDamage` 的 Postfix 上（`Verse/Thing.cs:1540` 才调用 `PostApplyDamage`）——**致死一击到达回调时受害者已 `Destroyed`**，`dealt` 判 false；同一击的 `enemyDamage` 走 `victim.Faction.HostileTo(player)`（不受销毁影响）照记，于是形成单边丢失。诊断日志三次命中 `destroyed=True hostile=False factionHostile=True`。**注意 `kb/L0-证据/decompiled/1.6/` 是 1.6 反编译，本次在 1.7.4981 触发；1.6 的 `Pawn.Kill` 不销毁原 Pawn，1.7 会。**
- 修复方案: 三条路径的敌对判据统一到派系层：`enemyHit = victim.Faction != null && victim.Faction.HostileTo(player)`；`dealt` 直接复用 `enemyHit`（不再查 `victim.HostileTo`）；`taken` 改用 `attacker.Faction != null && attacker.Faction.HostileTo(player)`。
- 是否规范缺失: 是（复用原版工具方法未核对其隐式前置条件）
- 规范反哺: AGENTS.md 已有「复用原版部件/工具前先反编译核对其隐式前置条件」，本条目补实例——`GenHostility.HostileTo` 对 `Destroyed` 早退，凡「事后归因 / 统计」场景都不该用它，直接判 `Faction.HostileTo` 更稳。
- 关联: 次数 1

## BUG-034 | HediffComp 内部的跨存档累积量未 Scribe → 读档后效果强度归零
- 现象: 「魔力装填」读档后 Hediff 与其剩余时间都还在，但已积累的魔力消耗量变成 0，属性补正回落。
- 根因: `HediffComp_ManaLoadInEffect.manaSpent` 是普通字段，没有在 `CompExposeData` 里 `Scribe_Values.Look` 注册；Hediff 自身的持久化只覆盖 Hediff / comp 基类字段，不覆盖 comp 自定义字段。
- 修复方案: 补 `CompExposeData`（`Scribe_Values.Look(ref manaSpent, "manaSpent")`）。
- 是否规范缺失: 否（已有「需要跨存读档保持的运行时状态随所属对象 ExposeData 持久化」规范，本次为实现遗漏）
- 规范反哺: 无新增；实现「按累计量成长」的 comp 时，把累积字段与效果 Hediff 同期持久化。
- 关联: 次数 1

## BUG-035 | 新增 Def 的 defName 以数字开头 → DefInjected 无法表达（语言文件解析失败）
- 现象: 为 `2FightIs2BeInEffect` 写 `<2FightIs2BeInEffect.label>` 时整个语言文件解析失败，该 Hediff 的中/英文本都无法覆盖。
- 根因: DefInjected 的键就是 **XML 元素名** `<defName>.<字段>`，而 XML 规定元素名不得以数字开头；这类 def 的 label/description 只能留在 Def 里（中英共用中文），违背本地化规范。
- 修复方案: 改名为 `FightIs2BeInEffect`，同步改代码侧 `DefDatabase<HediffDef>.GetNamed("FightIs2BeInEffect")` 字符串。
- 是否规范缺失: 是（Def 命名规范缺少「必须是合法标识符」一条）
- 规范反哺: AGENTS.md「5. XML 定义」与 B-01「XML（Defs / Patches）」各补一条 defName 命名约束。
- 关联: 次数 1

## BUG-036 | HediffStage 字段只在一次性 Init 里设置 → 读档后数值修正与提示行静默消失
- 现象: 「正义继巡」读档后，受影响者的崩溃阈值压制失效、Hediff 提示框里「共鸣来自：XXX」一行消失；无任何报错。
- 根因: `Hediff_SuccessionOfJusticeInEffect.Init()` 里设置 `stage.statFactors` 与 `stage.extraTooltip`，而 `Init` 只在 Tick 新建 Hediff 时调用；读档走 `ExposeData` 恢复，不会重跑 `Init`，`stage` 是实例字段（不参与 Scribe），两个字段保持 null。`StatUtility.GetStatValueFromList` 对 null 列表返回默认值（1）、`extraTooltip` 有 `NullOrEmpty` 判空 → 全程静默，只在功能上悄悄失效。
- 修复方案: `ExposeData` 的 `PostLoadInit` 反查到施予者技能后重跑 `Init(skill)`，把 `statFactors` / `extraTooltip` 一并重建；`Init` 内部对参数判空（`from = skill?.Owner`），否则「查不到施予者」的兜底路径会先 NRE。
- 是否规范缺失: 是（缺少「`CurStage` 依赖的字段必须可重建」的约定）
- 规范反哺: C-03 补长期约定；C-05 补 `HediffStage.extraTooltip` 走 `Formatted` 而非 `Translate` 的语义。
- 关联: 次数 1

## BUG-037 | Fleck 粒子速度量级过小 → 视觉上「静止不动」
- 现象: 索尔之光的治愈粒子按「从上方落下」实现，实机看起来只是原地淡入淡出，没有向下位移。
- 根因: `FleckThrown` 的 `velocity` 单位是**格/秒**（`NextPosition` 里 `worldPos += velocity * deltaTime`），而 `HealSpark` 寿命只有 1.5s（fadeIn 0.5 + solid 0.5 + fadeOut 0.5）；原实现给 0.3~0.9 格/秒，整段只落 0.45~1.35 格，肉眼几乎不可辨。参数本身合法，错的是**量级与寿命不匹配**。
- 修复方案: 向下速度改为 2~3.5 格/秒（1.5s 落 3~5 格），生成高度 +3，并给 ±0.15 横向扰动避免笔直。
- 是否规范缺失: 是（缺少「位移类特效的速度要按寿命量级估算」的经验条目）
- 规范反哺: C-05 新增「十五、特效与粒子」补 `FleckThrown` 速度语义与「下落距离 = 速度 × 寿命」的估算方法。
- 关联: 次数 1
