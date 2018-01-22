using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaptureX
{
    class StaticClass
    {
        //活动窗体句柄
        public static IntPtr Handle { get; set; }

        public static Rectangle CurrRect { get; set; }

        //配置
        public static Config Config { get; set; }

        public static ImageData Images { get; set; }

        //初始化
        public static void Init()
        {
            Config = new Config();           


            Images = new ImageData();
            
        }
    }
}
