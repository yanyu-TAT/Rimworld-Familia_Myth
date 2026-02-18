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
 * - [ ] 添加伟业值获取指令，实现恩惠升华
 * - [ ] 添加伟业值获取方式
 * - [ ] 添加数值调整设置入口
 * - [ ] 添加公会任务
 * - [ ] 添加眷族评级
 */

/* Develop Log:
 * 02/14 15:31 初步搭建代码框架，StatPart部分开发未完成，Def未创建
 * 02/15 02:54 基本完成恩惠状态的开发，未进行测试；初步设定了恩惠的属性加成，经过表格测试并调整，待实际测试；仪式部分框架完成大半，待继续
 * 02/15 17:11 基本完成0.0.1版本开发，众多bug待修复
 * 02/15 23:06 基本修复了发现的bug，初步测试通过
 * 02/19 02:57 基本完成了经验值的各种获取方式的实现，并添加了开发者指令；初步测试未发现问题，待更多测试，数值待调整
 */