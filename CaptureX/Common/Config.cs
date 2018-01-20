using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptureX
{
    class Config
    {
        public Config() {
            CurrRectRemember = true;
            IsPasteDeleteImage = false;        
        }

        // 截图位置记忆
        public bool CurrRectRemember { get; set; }

        // 粘贴图片时是否删图
        public bool IsPasteDeleteImage { get; set; }
    }
}
