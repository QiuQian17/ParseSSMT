using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3
{
    public partial class ProtectPage
    {

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // ✅ 每次进入页面都会执行，适合刷新 UI
            // 因为开启了缓存模式之后，是无法刷新页面语言的，只能在这里执行来刷新
            TranslatePage();
        }

        public void TranslatePage()
        {
            if (GlobalConfig.Chinese)
            {
                Menu_ModEncryption.Title = "Mod加密";
                Menu_ObfuscateAndEncryptBufferAndIni.Text = "一键混淆名称并加密Buffer和ini文件";
                Menu_EncryptBufferAndIni.Text = "一键加密Buffer和ini文件";

                Menu_Obfuscate.Text = "一键混淆Mod中的资源名称(Play版)";
                Menu_EncryptBuffer.Text = "一键加密Buffer文件";
                Menu_EncryptIni.Text = "一键加密ini文件";

                Menu_Obfuscation.Title = "Mod混淆";
                Menu_ObfuscateIni.Text = "混淆Mod的ini文件";
            }
            else
            {
                Menu_ModEncryption.Title = "Mod Encryption";
                Menu_ObfuscateAndEncryptBufferAndIni.Text = "Obfuscate And Encrypt Mod's Buffer And Ini File";
                Menu_EncryptBufferAndIni.Text = "Encrypt Mod's Buffer And Ini File";

                Menu_Obfuscate.Text = "Obfuscate Resource Name In Mod's .ini File(Used Only In Play Version d3d11.dll)";
                Menu_EncryptBuffer.Text = "Encrypt Mod's Buffer File";
                Menu_EncryptIni.Text = "Encrypt Mod's .ini File";

                Menu_Obfuscation.Title = "Mod Obfuscation";
                Menu_ObfuscateIni.Text = "Obfuscate Mod's ini file";
            }
        }

    }
}
