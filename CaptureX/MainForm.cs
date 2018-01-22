using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CaptureX
{
    /// <summary>
    /// luezl 2017.08.08
    /// </summary>
    public partial class MainForm : Form
    {
        #region 固定值

        public const int WM_PASTE = 0x302;
        public const int WM_CUT = 0x300;
        public const int WM_COPY = 0x301;

        public const int WM_DRAWCLIPBOARD = 0x308;
        public const int WM_CHANGECBCHAIN = 0x30D;

        #endregion

        #region 窗体移动

        Point CPoint;//获取控件中鼠标的坐标

        /// <summary>
        /// 利用控件移动窗体
        /// </summary>
        /// <param Frm="Form">窗体</param>
        /// <param e="MouseEventArgs">控件的移动事件</param>
        public void FrmMove(Form Frm, MouseEventArgs e)  //Form或MouseEventArgs添加命名空间using System.Windows.Forms;
        {
            if (e.Button == MouseButtons.Left)
            {
                Point myPosittion = Control.MousePosition;//获取当前鼠标的屏幕坐标
                myPosittion.Offset(CPoint.X, CPoint.Y);//重载当前鼠标的位置
                Frm.DesktopLocation = myPosittion;//设置当前窗体在屏幕上的位置
            }
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            CPoint = new Point(-e.X, -e.Y);
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            FrmMove(this, e);
        }

        #endregion

        public MainForm()
        {
            InitializeComponent();

            // 锁定窗体截图快捷键
            HotKey.RegisterHotKey(Handle, 100, HotKey.KeyModifiers.None, Keys.F1);
            // 窗体截图快捷键
            HotKey.RegisterHotKey(Handle, 101, HotKey.KeyModifiers.None, Keys.F2);
            // 选择区域截图快捷键
            HotKey.RegisterHotKey(Handle, 102, HotKey.KeyModifiers.None, Keys.F3);
            // 图片粘贴
            HotKey.RegisterHotKey(Handle, 103, HotKey.KeyModifiers.None, Keys.F4);            
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.TopMost = true;

            this.ClientSize = new Size(155, 30);

            //启动位置
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.ClientSize.Width, Screen.PrimaryScreen.Bounds.Height - this.ClientSize.Height-60);

            // 初始化配置
            StaticClass.Init();
        }

        /// <summary>
        /// 手动截图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XCapture(object sender, EventArgs e)
        {
            if (CaptureControl.Capture())
            {
                if (StaticClass.Images.Add((Image)CaptureControl.CapturedPic)>0)
                {
                    this.notifyIcon1.BalloonTipText = string.Format("已截图{0}张。", StaticClass.Images.Count());
                    this.notifyIcon1.ShowBalloonTip(3000);
                }
            }
        }

        /// <summary>
        /// 活动窗体截图
        /// </summary>
        private void ActiveWinCapture()
        {

            IntPtr tmpHandle = Win32API.GetForegroundWindow();
            if (tmpHandle != this.Handle)
            {
                StaticClass.Handle = tmpHandle;           
            }
            WinCapture();        
        }

        /// <summary>
        /// 窗体截图
        /// </summary>
        private void WinCapture()
        {
            if (StaticClass.Handle == IntPtr.Zero) return;
            Image img = MyCapture.GetScreenSnapshot(StaticClass.Handle);
            Clipboard.SetImage(img);
            if (StaticClass.Images.Add(img)>0)
            {
                this.notifyIcon1.BalloonTipText = string.Format("已截图{0}张。", StaticClass.Images.Count());
                this.notifyIcon1.ShowBalloonTip(3000);
            }
        }
        /// <summary>
        /// 清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbClear_Click(object sender, EventArgs e)
        {
            StaticClass.Images.Clear();
            this.notifyIcon1.BalloonTipText = "缓存已清空。";
            this.notifyIcon1.ShowBalloonTip(3000);            
        }

        /// <summary>
        /// 图片粘贴
        /// </summary>
        private void Paste()
        {
            Image img = StaticClass.Images.GetFristImage();
            if (img != null)
            {
                Clipboard.SetImage(img);

                System.Threading.Thread.Sleep(100);
                Win32API.keybd_event((byte)Keys.ControlKey, 0, 0, 0);
                Win32API.keybd_event((byte)Keys.V, 0, 0, 0);
                Win32API.keybd_event((byte)Keys.V, 0, 2, 0);
                Win32API.keybd_event((byte)Keys.ControlKey, 0, 2, 0);
            }            
        }

        /// <summary>
        /// 导出文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbExport_Click(object sender, EventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            if(folderDialog.ShowDialog() == DialogResult.OK)
            {
                string folderPath = folderDialog.SelectedPath;
                tsbExport.Enabled = false;
                this.notifyIcon1.BalloonTipText = "正在导出图片，请稍后。。";
                this.notifyIcon1.ShowBalloonTip(3000);

                Bitmap bitmap = null;
                int index = 0;
                Asyn.Task(() =>
                {
                    while ((bitmap = StaticClass.Images.GetExportImage()) != null)
                    {
                        index++;
                        bitmap.Save(System.IO.Path.Combine(folderPath, string.Format("{0}.png", index)), bitmap.RawFormat);
                    }
                }).OnSuccess(() =>
                {
                    tsbExport.Enabled = true;
                    this.notifyIcon1.BalloonTipText = string.Format("已成功导出{0}张", index);
                    this.notifyIcon1.ShowBalloonTip(3000);
                }).OnError((ex) => 
                {
                    this.notifyIcon1.BalloonTipText = "导出处理失败。。";
                    this.notifyIcon1.ShowBalloonTip(3000);
                }).Start();                
            }
        }

        /// <summary>
        /// 重新导入缓存图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reset(object sender, EventArgs e)
        {
            this.notifyIcon1.BalloonTipText = string.Format("共{0}张。", StaticClass.Images.AllCount());
            this.notifyIcon1.ShowBalloonTip(3000);
        }

        #region 快捷键

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            //按快捷键 
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    switch (m.WParam.ToInt32())
                    {
                        case 100:
                            ActiveWinCapture();
                            break;
                        case 101:
                            WinCapture();
                            break;
                        case 102:
                            XCapture(null, null);
                            break;
                        case 103:
                            Paste();
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        #endregion


        //显示/隐藏
        private void ShowOrHideForm(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                this.Visible = false;
                this.TopMost = false;
                this.tsmShowOrHideForm.Text = "显示";                
            }
            else
            {
                this.Visible = true;
                this.TopMost = true;
                this.tsmShowOrHideForm.Text = "隐藏"; 
            }
        }

        //关闭
        private void tsmClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolStrip1_MouseHover(object sender, EventArgs e)
        {
            this.Activate();
            tsbClear.ToolTipText = string.Format("清空{0}张", StaticClass.Images.AllCount());
        }       
    }
}
