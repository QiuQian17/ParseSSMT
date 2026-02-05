using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMT3;

namespace SSMT3
{
    public partial class GameTypePage
    {
      

        private void InitializeComboBoxGameTypeNameList()
        {
            IsLoading = true;

            ComboBox_GameTypeNameList.Items.Clear();

            if (Directory.Exists(PathManager.Path_CurrentSelected_GameTypeFolder))
            {
                string[] GameTypeJsonFilePathList = Directory.GetFiles(PathManager.Path_CurrentSelected_GameTypeFolder);

                foreach (string GameTypeJsonFilePath in GameTypeJsonFilePathList)
                {
                    string FileName = Path.GetFileNameWithoutExtension(GameTypeJsonFilePath);
                    ComboBox_GameTypeNameList.Items.Add(FileName);
                }
            }

            IsLoading = false;
        }




    }
}
