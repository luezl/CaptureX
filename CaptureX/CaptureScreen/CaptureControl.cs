namespace CaptureX
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class CaptureControl
    {
        private static Bitmap capturedPic;

        private static CaptureForm cf;

        public static bool Capture()
        {
            if (cf == null)
            {
                cf = new CaptureForm();
            } 
            else
            {
                return false;
            }

            if (cf.ShowDialog() == DialogResult.OK)
            {
                IDataObject iData = Clipboard.GetDataObject();
                if (iData.GetDataPresent(DataFormats.Bitmap))
                {
                    capturedPic = (Bitmap) iData.GetData(DataFormats.Bitmap);
                }

                cf = null;
                return true;
            }
            cf = null;
            return false;
        }

        public static Bitmap CapturedPic
        {
            get
            {
                return capturedPic;
            }
        }
    }
}

