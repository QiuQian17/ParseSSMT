using Microsoft.UI.Xaml;
using Sword.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3.Pages.ManuallyReversePage
{
    public partial class ManuallyReversePage
    {


        private void Menu_ReversedFolder_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(PathManager.Path_ReversedFolder))
            {
                SSMTCommandHelper.ShellOpenFolder(PathManager.Path_ReversedFolder);
            }
            else
            {
                _ = SSMTMessageHelper.Show("当前Reversed文件夹不存在，请先进行手动逆向生成此文件夹再来打开此文件夹。");
            }
        }


        private async void Menu_Textures_ConvertJpg_Click(object sender, RoutedEventArgs e)
        {
            string selected_folder_path = await SSMTCommandHelper.ChooseFolderAndGetPath();
            if (selected_folder_path == "")
            {
                return;
            }

            try
            {
                SSMTTextureHelper.ConvertAllTexturesIntoConvertedTexturesReverse(selected_folder_path, "jpg");

                SSMTCommandHelper.ShellOpenFolder(selected_folder_path + "\\ConvertedTextures\\");

            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

        private async void Menu_Textures_ConvertPng_Click(object sender, RoutedEventArgs e)
        {
            string selected_folder_path = await SSMTCommandHelper.ChooseFolderAndGetPath();
            if (selected_folder_path == "")
            {
                return;
            }

            try
            {
                SSMTTextureHelper.ConvertAllTexturesIntoConvertedTexturesReverse(selected_folder_path, "png");

                SSMTCommandHelper.ShellOpenFolder(selected_folder_path + "\\ConvertedTextures\\");

            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

        private async void Menu_Textures_ConvertTga_Click(object sender, RoutedEventArgs e)
        {
            string selected_folder_path = await SSMTCommandHelper.ChooseFolderAndGetPath();
            if (selected_folder_path == "")
            {
                return;
            }

            try
            {
                SSMTTextureHelper.ConvertAllTexturesIntoConvertedTexturesReverse(selected_folder_path, "tga");

                SSMTCommandHelper.ShellOpenFolder(selected_folder_path + "\\ConvertedTextures\\");

            }
            catch (Exception ex)
            {
                await SSMTMessageHelper.Show(ex.ToString());
            }
        }


        private void Menu_OpenLogsFolder_Click(object sender, RoutedEventArgs e)
        {
            SSMTCommandHelper.ShellOpenFolder(PathManager.Path_LogsFolder);
        }

        private async void Menu_OpenLatestLogFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await SSMTCommandHelper.ShellOpenFile(PathManager.Path_LatestSSMTLogFile);
            }
            catch (Exception ex)
            {
                await SSMTMessageHelper.Show("Error: " + ex.ToString());
            }
        }

        private void Menu_OpenConfigsFolder_Click(object sender, RoutedEventArgs e)
        {
            SSMTCommandHelper.ShellOpenFolder(PathManager.Path_ConfigsFolder);
        }

        private void Menu_GameTypeFolder_Click(object sender, RoutedEventArgs e)
        {
            SSMTCommandHelper.ShellOpenFolder(PathManager.Path_GameTypeConfigsFolder);
        }





    }
}
