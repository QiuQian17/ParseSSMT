using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3
{
    public partial class TexturePage
    {


        private void ReadCurrentTextureConfig()
        {
            Debug.WriteLine("Read Current Texture Config::");
            string TextureConfigName = GetCurrentPSValue();
            if (TextureConfigName == "")
            {
                Debug.WriteLine("TextureConfigName is empty, skip reading texture config.");
                return;
            }
            LOG.Info("PS: " + TextureConfigName);

            Debug.WriteLine("TextureConfigName: " + TextureConfigName);
            string TextureConfigSavePath = PathManager.Path_GameTextureConfigFolder + TextureConfigName + ".json";
            Debug.WriteLine("TextureConfigSavePath: " + TextureConfigSavePath);
            if (File.Exists(TextureConfigSavePath))
            {
                Dictionary<string, SlotObject> PixeSlot_MarkName_Dict = TextureConfig.Read_PixelSlot_SlotObject_Dict(TextureConfigSavePath);

                Debug.WriteLine("Count: " + imageCollection.Count.ToString());
                for (int i = 0; i < imageCollection.Count; i++)
                {
                    ImageItem imageItem = imageCollection[i];
                    if (PixeSlot_MarkName_Dict.ContainsKey(imageItem.PixelSlot))
                    {
                        SlotObject slot_obj = PixeSlot_MarkName_Dict[imageItem.PixelSlot];
                        imageItem.MarkName = slot_obj.MarkName;
                        imageItem.MarkStyle = slot_obj.MarkStyle;
                    }

                    imageCollection[i] = imageItem;

                    Debug.WriteLine(imageCollection[i].MarkName);

                }
            }
            else
            {
                Debug.WriteLine("TextureConfigSavePath doesn't exists: " + TextureConfigSavePath);
            }

        }

        public void SaveCurrentTextureConfig()
        {
            //贴图标记的名称，就是当前DrawCall的Index的对应PS的Hash值
            //由于对于一个DrawCall的Index来说，各个槽位上的PS名称都是一样的，所以我们可以直接获取第一个贴图的名称来获取PS值
            string PSHashValue = GetCurrentPSValue();
            if (PSHashValue != "")
            {
                string TextureConfigSavePath = Path.Combine(PathManager.Path_GameTextureConfigFolder, PSHashValue + ".json");
                TextureConfig.SaveTextureConfig(imageCollection, TextureConfigSavePath);
            }




        }


    }
}
