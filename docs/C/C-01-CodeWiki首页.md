# C-01 CodeWiki 首页

> 代码 Wiki 入口。维护核心类索引与阅读路线，帮助新 Agent/开发者快速定位。

## 阅读路线（按子系统）

| 想了解 | 入口 |
|---|---|
| 模组入口与初始化 | `FamiliaMythMod.cs`（HugsLib ModBase） |
| 恩惠/能力值 | `Hediff_Pharna.cs` + `StatPart_Pharna.cs` + `Patches/StatParts_Pharna.xml` |
| 成长/经验 | `Patch_Pawn.cs`、`Patch_WillGain.cs`、`Patch_Pawn_SkillTracker.cs` |
| 升华/升级奖励 | `AscendHelper.cs` + `Dialog_AscendReward.cs` + 仪式类 |
| 魔法框架 | `Spell.cs`（继承原版 Ability）+ `SpellDef.cs` |
| 咏唱 | `Verb_CastSpell.cs` / `Mote_Incantation.cs` / `Mote_MagicCircle.cs` |
| 魔法弹 | `Projectile_MagicMissileBase.cs` + `MagicMissileStates.cs` |
| 技能框架 | `Skill.cs` + `SkillDef.cs` |
| 发展能力 | `DevelopableAbility.cs` + `DevelopableAbilityDef.cs` + `StatPart_DevelopableAbility.cs` |
| 意志/美德 | `MentalState_Virtue.cs` + `Hediff_VirtueBuff_*.cs` |
| 公会 | `GameComponent_FamiliaRankTracker.cs` + `GameComponent_GuildEnvoyTracker.cs`（单槽使者）+ `GuildQuestDef/Generator*`（统一工厂 `GuildQuestGenerator.Create(def)` + `Available()` 发布预筛）+ `QuestUtilities.cs` + 任务三件套（`QuestNode_Root_*` / `QuestPart_*` / `ExploitsReward`，细节见 C-02）+ 世界对象交互 comp（`WorldObjectComp_*`，如物资押运的定居点按钮）+ 任务脚本（`Defs/QuestScriptDefs/`） |
| 眷族 UI | `UI_FamiliaPage.cs` + `UI_RadarChart.cs` |
| 特效 | `Mote_*.cs` + `Textures/` + `Materials/` |

## 内容索引

- `C-02-架构详解.md` — 系统设计细节
- `C-03-项目长期记忆.md` — 长期事实与约定
- `C-04-已实现内容清单.md` — 已实现魔法/技能/发展能力、版本进度、美术本地化
- `C-05-原版API简述.md` — 按系统分类收录的原版 API 方法速查（反编译证据，持续扩充）
- 数值/设计: 主设计文档（`Familia Myth rim.txt` 及配套 txt/xlsx）
