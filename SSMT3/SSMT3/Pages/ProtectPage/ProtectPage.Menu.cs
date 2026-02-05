using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3
{
    public partial class ProtectPage
    {


        private void Menu_OpenACLFolder_Click(object sender, RoutedEventArgs e)
        {
            string ACLFolderPath = TextBox_ACLFolderPath.Text;
            if (Directory.Exists(ACLFolderPath))
            {
                SSMTCommandHelper.ShellOpenFolder(ACLFolderPath);
            }
            else
            {
                _ = SSMTMessageHelper.Show("无法找到当前填写的ACL文件夹的路径", "Can't find current ACL folder path.");
            }
        }

        private void Menu_OpenTargetFolder_Click(object sender, RoutedEventArgs e)
        {
            string TargetModFolderPath = TextBox_TargetFolderPath.Text;
            if (Directory.Exists(TargetModFolderPath))
            {
                SSMTCommandHelper.ShellOpenFolder(TargetModFolderPath);
            }
            else
            {
                _ = SSMTMessageHelper.Show("无法找到当前填写的目标Mod文件夹的路径", "Can't find current target Mod folder path.");
            }
        }





    }
}
