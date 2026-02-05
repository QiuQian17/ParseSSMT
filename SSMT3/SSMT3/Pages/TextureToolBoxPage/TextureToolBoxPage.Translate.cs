using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3
{
    public partial class TextureToolBoxPage
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
                SettingsCard_VerticalFlip.Header = "垂直翻转";
                SettingsCard_VerticalFlip.Description = "将选中的视频内容进行垂直翻转，因为部分游戏Dump出来的图片是垂直翻转的";

                ToggleSwitch_VerticalFlip.OnContent = "开启";
                ToggleSwitch_VerticalFlip.OffContent = "关闭";

                SettingsCard_HorizontalFlip.Header = "水平翻转";
                SettingsCard_HorizontalFlip.Description = "将选中的视频内容进行水平翻转，因为部分游戏Dump出来的图片是水平翻转的";

                ToggleSwitch_HorizontalFlip.OnContent = "开启";
                ToggleSwitch_HorizontalFlip.OffContent = "关闭";
            }
            else
            {
                SettingsCard_VerticalFlip.Header = "Vertical Flip";
                SettingsCard_VerticalFlip.Description = "Vertically flip the selected video content, as some games dump images that are vertically flipped.";

                ToggleSwitch_VerticalFlip.OnContent = "On";
                ToggleSwitch_VerticalFlip.OffContent = "Off";

                SettingsCard_HorizontalFlip.Header = "Horizontal Flip";
                SettingsCard_HorizontalFlip.Description = "Horizontally flip the selected video content, as some games dump images that are horizontally flipped.";

                ToggleSwitch_HorizontalFlip.OnContent = "On";
                ToggleSwitch_HorizontalFlip.OffContent = "Off";


            }
        }




    }


}
