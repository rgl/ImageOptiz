using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ImageOptiz
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        #region AboutForm handling

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, int wPosition, int wFlags, int wIDNewItem, string lpNewItem);

        [DllImport("user32.dll")]
        private static extern int GetMenuItemCount(IntPtr hMenu);

        public const int WM_SYSCOMMAND = 0x112;
        public const int MF_SEPARATOR = 0x800;
        public const int MF_BYPOSITION = 0x400;
        public const int MF_STRING = 0x0;
        public const int IDM_ABOUT = 1000;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var systemMenu = GetSystemMenu(Handle, false);
            var baseMenuItemPosition = GetMenuItemCount(systemMenu) - 2;

            InsertMenu(systemMenu, baseMenuItemPosition + 0, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
            InsertMenu(systemMenu, baseMenuItemPosition + 1, MF_BYPOSITION | MF_STRING, IDM_ABOUT, "About");
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND)
            {
                switch (m.WParam.ToInt32())
                {
                    case IDM_ABOUT:
                        ShowAboutForm();
                        return;
                }
            }

            base.WndProc(ref m);
        }

        private void ShowAboutForm()
        {
            using (var aboutForm = new AboutForm())
            {
                aboutForm.ShowDialog(this);
            }
        }

        #endregion

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            var data = e.Data;

            if (data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effect = DragDropEffects.Copy;
            }

            if (data.GetDataPresent(DataFormats.Text, true))
            {
                e.Effect = DragDropEffects.Copy;
            }

#if DEBUG
            Trace.WriteLine("MainForm_DragEnter Native formats:");
            var nativeFormats = data.GetFormats(false);
            foreach (var format in nativeFormats)
                Trace.WriteLine(format);

            Trace.WriteLine("MainForm_DragEnter Converted formats:");
            var convertedFormats = data.GetFormats(true);
            foreach (var format in convertedFormats)
                Trace.WriteLine(format);
#endif
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files == null)
                {
                    var text = (string)e.Data.GetData(DataFormats.Text);
                    if (string.IsNullOrEmpty(text))
                        return;
                    files = text.Split('\n');
                }

                if (files.Length == 0)
                    return;

                // bring this form to the top.
                Activate();

                listView.Items.Clear();

                QueueFiles(files);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Error in DragDrop method: {0}", ex.Message));
                // NB: Do not show a MessageBox here; explorer is waiting.
            }
        }

        private void QueueFiles(string[] fileNames)
        {
            foreach (var fileName in fileNames)
                QueueFile(fileName);
        }

        private void QueueFile(string fileName)
        {
            Trace.WriteLine(string.Format("QueueFile {0}", fileName));

            // TODO do this in background... using some kind of manager.

            if (Directory.Exists(fileName))
            {
                // TODO recurse the directory looking for images, and add them all.
            }
            else
            {
                // TODO queue the file...
                var fileInfo = new FileInfo(fileName);

                if (!ImageOptimizer.ValidExtensions.Contains(fileInfo.Extension.ToLower()))
                    return;

                var originalFileLength = fileInfo.Length;

                var listViewItem = new ListViewItem(string.Empty);
                listViewItem.SubItems.Add(fileInfo.Name);
                listViewItem.SubItems.Add(string.Format("{0}", originalFileLength)); // TODO format the number, eg. 2345 into 2.345

                listView.Items.Add(listViewItem);

                OptimizeImage(fileInfo);

                fileInfo.Refresh();
                var optimizedFileLength = fileInfo.Length;
                listViewItem.SubItems.Add(string.Format("{0}", optimizedFileLength)); // TODO format the number, eg. 2345 into 2.345
                listViewItem.SubItems.Add(string.Format("{0}", optimizedFileLength - originalFileLength)); // TODO format the number, eg. 2345 into 2.345
                listViewItem.SubItems.Add(string.Format("{0:#.##}", 100 - (optimizedFileLength / (double)originalFileLength) * 100));
            }
        }

        private void OptimizeImage(FileInfo fileInfo)
        {
            ImageOptimizer.Optimize(fileInfo);
        }
    }
}
