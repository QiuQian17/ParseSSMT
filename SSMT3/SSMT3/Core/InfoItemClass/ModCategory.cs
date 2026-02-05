using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3
{
    public class ModCategory
    {
       

        public string Name { get; set; } = "";
        public string BackgroundImage { get; set; } = "";
        public string ModNumber { get; set; } = "";

        public bool NotEnable { get; set; } = false;

        public BitmapImage BackgroundImageSource
        {
            get
            {
                var bmp = new BitmapImage
                {
                    CreateOptions = BitmapCreateOptions.IgnoreImageCache
                };
                try
                {
                    bmp.UriSource = new Uri(BackgroundImage);
                }
                catch
                {
                    // ignore invalid path
                }
                return bmp;
            }
        }

    }


}
