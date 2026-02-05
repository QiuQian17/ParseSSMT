using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMT3;

namespace SSMT3
{
    public class GameIconItem
    {
        public string GameIconImage { get; set; } = "";
        public string GameName { get; set; } = "";
        public string MigotoFolder { get; set; } = "";

        //类型，比如UnityVS,WWMI等等
        public string Type { get; set; } = "";

        /// <summary>
        /// 缓存的背景图
        /// </summary>
        public BitmapImage GameBackGroundImage { get; set; } = new BitmapImage();

        public BitmapImage GameIconImageSource
        {
            get
            {
                var bmp = new BitmapImage
                {
                    CreateOptions = BitmapCreateOptions.IgnoreImageCache
                };
                bmp.UriSource = new Uri(GameIconImage);
                return bmp;
            }
        }

        public void SaveToJson(string JsonFilePath)
        {
            JObject jObject = DBMTJsonUtils.CreateJObject();

            jObject["GameIconImage"] = GameIconImage;
            jObject["GameName"] = GameName;
            jObject["MigotoFolder"] = MigotoFolder;

            DBMTJsonUtils.SaveJObjectToFile(jObject, JsonFilePath);
        }

    }
}
