using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3.Pages.AutoReversePage
{
    public partial class AutoReversePage
    {

        private void Menu_OpenPluginsFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SSMTCommandHelper.ShellOpenFolder(PathManager.Path_PluginsFolder);

            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show("Error: " + ex.ToString());
            }
        }

        private async void Menu_OpenLatestLogFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await SSMTCommandHelper.ShellOpenFile(PathManager.Path_LatestSSMTLogFile);
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show("Error: " + ex.ToString());
            }
        }

    }
}
