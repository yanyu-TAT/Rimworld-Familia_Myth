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
