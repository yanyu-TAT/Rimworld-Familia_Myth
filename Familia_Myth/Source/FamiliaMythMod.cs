using HugsLib;
using Verse;

namespace FamiliaMyth.Core
{
    /// <summary>
    /// 模组主类
    /// </summary>
    public class FamiliaMythMod : ModBase
    {
        public static string godName = "你";
        public override string ModIdentifier => "FamiliaMyth";
    }

    /// <summary>
    /// 文本翻译工具类
    /// </summary>
    public static class TranslateHelper
    {
        /// <summary>
        /// 文本翻译工具方法
        /// </summary>
        /// <param name="key">对应的翻译键</param>
        /// <param name="args">替换键值序列</param>
        /// <returns></returns>
        public static TaggedString Translate(string key, params (string, object)[] args)
        {
            String str = key.Translate();
            if (args == null || args.Length == 0)
            {
                return str;
            }

            NamedArgument[] arguments = args.Select(t => new NamedArgument(t.Item2, t.Item1)).ToArray();
            return str.Formatted(arguments);
        }
    }
}

/* Todo List:
 * - [x] 创建恩惠使用的健康状态
 * - [x] 添加Hediff_Pharna代码及def
 * - [x] 添加StatPart_Pharna代码
 * - [x] 添加补丁注入属性修正
 * - [x] 添加神之恩惠仪式
 * - [x] 创建RitualOutComeWorker_GrantPharna代码及def
 * - [x] 创建RitualBehavior_GrantPharna.xml
 * - [x] 创建Precepts_Ritual_Pharna.xml添加PreceptDef和RitualPatternDef
 * - [x] 为两个仪式创建自定义的RitualRole类
 * - [x] 为升华仪式创建同样的代码和xml文件
 * - [x] 添加GameComponent注入仪式
 * - [x] 添加本地化
 * - [x] 测试
 * - [x] 添加经验值获取方式
 * - [x] 添加各属性的监听patch
 * - [x] 添加开发者指令（提高属性，赋予恩惠）
 * - [x] 添加伟业值获取指令
 * - [x] 实现魔力属性注入和成长
 * - [x] 添加升级奖励
 * - [x] 扩展DevelopableAbilityDef
 * - [x] 创建StatPart_DevelopableAbility
 * - [x] 创建基础的发展能力xml（魔导，异常抗性）
 * - [x] 实现基础的技能框架SkillDef
 * - [x] 实现基础的魔法框架SpellDef
 * - [x] 扩展Hediff_Pharna的技能/魔法管理
 * - [x] 完整实现AscendHelper
 * - [x] 实现各个IAscendReward接口
 * - [ ] 添加更多升级奖励
 * - [-] 添加伟业值获取方式
 * - [ ] 添加仪式质量相关建筑
 * - [ ] 添加更多提示和消息
 * - [ ] 添加本地化
 * - [ ] 试玩，调整数值
 */

/* Develop Log:
 * 02/14 15:31 初步搭建代码框架，StatPart部分开发未完成，Def未创建
 * 02/15 02:54 基本完成恩惠状态的开发，未进行测试；初步设定了恩惠的属性加成，经过表格测试并调整，待实际测试；仪式部分框架完成大半，待继续
 * 02/15 17:11 基本完成0.0.1版本开发，众多bug待修复
 * 02/15 23:06 基本修复了发现的bug，初步测试通过
 * 02/19 02:57 基本完成了经验值的各种获取方式的实现，并添加了开发者指令；初步测试未发现问题，待更多测试，数值待调整
 * 02/25 23:41 完成了基础的发展能力框架，添加了两个基础的发展能力，未测试
 * 02/26 04:08 添加了技能def的基础框架，技能基类和两个基础的技能待实现
 * 02/26 16:40 完成了基本的技能示例框架，余烬技能实现中
 * 02/26 22:32 余烬技能实现中，进行了patch
 * 02/27 03:16 基本完成了余烬的技能，待测试
 * 03/02 01:51 狂化招乱技能已实现，待测试；魔法的基本框架搭建中，咏唱使用的jobdriver待实现
 * 07/26 16:58 基本完成魔法基本框架，升级奖励框架搭建中
 * 07/26 23:06 基本完成升级奖励，添加了一个魔法（诺亚治愈），待更多测试，升级弹窗待润色
 * 07/28 03:06 完成了诺亚治愈和余烬的测试和改进，咏唱词的展示形式待改进，升级奖励的弹窗待改进
 * 07/28 16:39 完成了升级弹窗的改进，修复了咏唱词的bug并调整了颜色
 * 07/28 23:08 任务系统框架基本完成，待测试，细节待完善；修复了Patch存在的bug
 * 07/29 04:22 完成了任务发布及接取的完整逻辑，待测试
 * 07/29 22:57 测试并完成了任务发布及接取，反馈部分待完善
 * 07/30 03:30 继续完善了反馈部分
 * 07/31 03:38 搭建了魔法弹的基类以及多种运动控制器，在魔法基类中预留了接口，实际子类待落实，期待加入更多基于魔法弹的魔法
 * 07/31 17:07 完成了光明之风和风弹子类的实现，材质待添加，效果待测试
 * 08/01 03:27 基本完成了光明之风及其风弹子类的测试和修复，以及材质绘制。特效和机制待调整
 * 08/02 01:23 优化了风弹的材质，修复了一系列风弹相关的bug；期待加入风弹产生时的特效，音效等；风弹状态转变时的不明位置偏移待排查
 * 08/03 03:12 添加并修复了光明之风的一系列问题；着手制作齐射火标枪，实现并测试了预热阶段的特效，效果待实现
 * 08/03 16:50 完成了齐射火标枪的锁定和射击效果，锁定特效未添加
 * 08/03 21:53 添加并测试了锁定特效
 * 08/04 03:04 添加了齐射火标枪的技能图标，修改了部分机制和特效
 * 08/05 03:35 大幅修改了任务系统，提高了可扩展性，加入了眷族评级等内容，UI部分待添加，待更多测试
 * 08/06 00:22 添加了眷族评级的UI系统，测试并优化完毕
 */