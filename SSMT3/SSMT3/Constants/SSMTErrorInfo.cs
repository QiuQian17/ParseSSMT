using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMT3;

namespace SSMT3
{
    public class SSMTErrorInfo
    {
        public static string CantFindDataType { get; set; } = "";

        public static string CantFind3DmigotoSwordLv5CN = "您当前Plugins目录下不存在此插件，请联系NicoMico赞助获取此插件，将其安装到SSMT缓存文件夹下的Plugins目录下再重试此功能。\n您是否需要打开【SSMT长期技术支持LTS】赞助页面来赞助并获取插件?";

        public static string CantFind3DmigotoSwordLv5EN = "Your current Plugins directory does not contain this plugin. Please contact NicoMico for sponsorship to obtain this tool and install it in the Plugins directory before retrying this feature.\nDo you want to open the SSMT Long Time Support sponsorship page to sponsor and get the plugin?";

        public static void ShowCantFindDataTypeError(string DrawIB)
        {
            LOG.Error("无法找到当前提取IndexBuffer Hash值:   " + DrawIB + "   对应Buffer数据的数据类型\n" + 
                "可能的解决方案如下，请先自己排查一下:" + 
                "1.结合运行日志信息，到数据类型管理页面添加此数据类型\n" + 
                "2.联系NicoMico，发送给他Dump下来的FrameAnalysis文件夹和提取使用的DrawIB来添加此数据类型支持并更新SSMT版本\n" + 
                "3.可能游戏中未关闭[角色动态高精度]图形设置项，关闭后重新F8 Dump并提取测试\n" + 
                "4.可能当前提取逻辑与当前游戏渲染逻辑并未适配，检查提取逻辑是否设置正确\n" + 
                "5.可能提取的是打了Mod的模型，SSMT并不支持提取Mod的模型，请使用SSMT的一键逆向插件来提取Mod中的模型。\n" + 
                "6.可能是提取用的Hash是VB(Vertex Buffer Hash)而不是IB(Index Buffer Hash)，IB是通过小键盘7和8切换，小键盘9复制得到的，IB是SSMT的通用用法，输入VB一般是使用过类似GIMI这种古董脚本按照经验在SSMT中使用VB导致的，SSMT只使用IB。\n" + 
                "如果实在无法解决，请联系开发者NicoMico获取技术支持。");
        }

        public static string NotSupportedGamePreset()
        {
            GameConfig gameConfig = new GameConfig();
            return "您当前选择的游戏预设为: " 
                + gameConfig.GamePreset 
                + " 已停止提供支持.\n"
                + "具体原因如下:\n"
                + "1.米系列游戏严打Mod，已有倒卖Mod被抓案例:BiliBili雨过天晴小胡桃\n"
                + "2.米系列游戏严打Mod，对Mod作者狩野樱等多个作者进行了上门取证，后续情况可关注BiliBili狩野樱的动态\n"
                + "3.SSMT不再提供任何关于米的技术支持，使用SSMT可能有风险，请自行斟酌";
        }

        

    }
}
