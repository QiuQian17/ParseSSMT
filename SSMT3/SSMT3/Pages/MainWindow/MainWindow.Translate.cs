using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3
{
    public partial class MainWindow
    {

        public void TranslatePage()
        {
            if (GlobalConfig.Chinese)
            {
                NavigationViewItem_StarterPage.Content = "主页";
                NavigationViewItem_WorkPage.Content = "工作台";
                NavigationViewItem_TexturePage.Content = "贴图标记";
                NavigationViewItem_TextureToolBoxPage.Content = "贴图工具箱";
                NavigationViewItem_GameTypePage.Content = "数据类型管理";
                NavigationViewItem_ModManagePage.Content = "Mod管理";
                NavigationViewItem_DocumentPage.Content = "SSMT文档";
            }
            else
            {
                NavigationViewItem_StarterPage.Content = "Starter";
                NavigationViewItem_WorkPage.Content = "Work";
                NavigationViewItem_TexturePage.Content = "Mark Texture";
                NavigationViewItem_TextureToolBoxPage.Content = "Texture ToolBox";
                NavigationViewItem_GameTypePage.Content = "GameType Management";
                NavigationViewItem_ModManagePage.Content = "Mod Management";
                NavigationViewItem_DocumentPage.Content = "SSMT Documents";

            }
        }


    }
}
