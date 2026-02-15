using Verse;

namespace FamiliaMyth.Core
{
    /// <summary>
    /// 模组主类
    /// </summary>
    public class FamiliaMythMod
    {
        public static string godName = "你";
        
    }

    /// <summary>
    /// 文本翻译工具类
    /// </summary>
    public class TranslateHelper
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
 * - [ ] 添加本地化
 * - [ ] 测试
 */

/* Develop Log:
 * 02/14 15:31 初步搭建代码框架，StatPart部分开发未完成，Def未创建
 * 02/15 02:54 基本完成恩惠状态的开发，未进行测试；初步设定了恩惠的属性加成，经过表格测试并调整，待实际测试；仪式部分框架完成大半，待继续
 * 02/15 17:11 基本完成0.0.1版本开发，众多bug待修复
 */