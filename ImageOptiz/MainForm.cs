using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ImageOptiz
{
    public partial class MainForm : Form
    {
        private static readonly string[] ValidExtensions = new[]
        {
            ".png",
            ".jpg",
            ".jpeg",
        };

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

            // TODO do this in background... using the manager.

            if (Directory.Exists(fileName))
            {
                // TODO recurse the directory looking for images, and add them all.
            }
            else
            {
                // TODO queue the file...
                var fileInfo = new FileInfo(fileName);

                if (!ValidExtensions.Contains(fileInfo.Extension.ToLower()))
                    return;

                // TODO filter the file by extension.

                var originalFileLength = fileInfo.Length;

                var listViewItem = new ListViewItem(string.Empty);
                listViewItem.SubItems.Add(fileInfo.Name);
                listViewItem.SubItems.Add(string.Format("{0}", originalFileLength)); // TODO format the number

                listView.Items.Add(listViewItem);

                OptimizeImage(fileInfo);

                fileInfo.Refresh();
                var optimizedFileLength = fileInfo.Length;
                listViewItem.SubItems.Add(string.Format("{0}", optimizedFileLength)); // TODO format the number
                listViewItem.SubItems.Add(string.Format("{0}", optimizedFileLength - originalFileLength)); // TODO format the number
                listViewItem.SubItems.Add(string.Format("{0:#.##}", (optimizedFileLength / (double)originalFileLength) * 100)); // TODO format the number
            }
        }

        private void OptimizeImage(FileInfo fileInfo)
        {
            // TODO catch exceptions, and flag the image path has failed (with the error as tooltip).
            switch (fileInfo.Extension.ToLower())
            {
                case ".png":
                    RunPngOut(fileInfo);
                    break;
                case ".jpg":
                case ".jpeg":
                    RunJpegTran(fileInfo);
                    break;
            }
        }

        private ToolResult RunPngOut(FileInfo fileInfo)
        {
            // NB: do not run pngout with the -q argument (we want to output to the user).
            return RunTool("pngout", "-y", fileInfo.FullName);
        }

        private ToolResult RunJpegTran(FileInfo fileInfo)
        {
            var temporaryFileInfo = CreateTemporaryFile(fileInfo);
            var toolResult = RunTool("jpegtran", "-verbose", "-copy", "none", "-optimize", fileInfo.FullName, temporaryFileInfo.FullName);
            temporaryFileInfo.Refresh();
            if (toolResult.ExitCode == 0 && temporaryFileInfo.Exists && temporaryFileInfo.Length > 0 && fileInfo.Length > temporaryFileInfo.Length)
            {
                File.Delete(fileInfo.FullName);
                File.Move(temporaryFileInfo.FullName, fileInfo.FullName);
            }
            else
            {
                File.Delete(temporaryFileInfo.FullName);
            }
            return toolResult;
        }

        private static FileInfo CreateTemporaryFile(FileInfo fileInfo)
        {
            FileInfo temporaryFileInfo;
            do
            {
                var guid = Guid.NewGuid();
                var temporaryFilePath = Path.Combine(fileInfo.DirectoryName, string.Format("{0}.tmp{1}", guid, fileInfo.Extension));
                temporaryFileInfo = new FileInfo(temporaryFilePath);
            }
            while (temporaryFileInfo.Exists);
            return temporaryFileInfo;
        }

        private class ToolResult
        {
            public int ExitCode { get; set; }
            public string StandardOutput { get; set; }
            public string StandardError { get; set; }
        }

        private ToolResult RunTool(string tool, params string[] arguments)
        {
            var toolPath = Path.Combine(Path.Combine(Application.StartupPath, "tools"), string.Format("{0}.exe", tool));
            using (
                var p = new Process
                {
                    StartInfo =
                    {
                        FileName = EscapeProcessArgument(toolPath),
                        Arguments = EscapeProcessArguments(arguments),
                        RedirectStandardInput = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                }
            )
            {
                var standardOutput = new StringBuilder();
                var standardError = new StringBuilder();

                p.OutputDataReceived += (sendingProcess, e) => standardOutput.AppendLine(e.Data);
                p.ErrorDataReceived += (sendingProcess, e) => standardError.AppendLine(e.Data);

                if (!p.Start())
                {
                    throw new ApplicationException("Ops, failed to launch pngout");
                }

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                p.WaitForExit();

                return new ToolResult {
                    ExitCode = p.ExitCode,
                    StandardOutput = standardOutput.ToString(),
                    StandardError = standardError.ToString()
                };
            }
        }

        private static string EscapeProcessArguments(string[] arguments)
        {
            var sb = new StringBuilder();
            foreach (var argument in arguments)
            {
                if (sb.Length > 0)
                    sb.Append(' ');
                EscapeProcessArgument(argument, sb);
            }
            return sb.ToString();
        }

        private static string EscapeProcessArgument(string argument)
        {
            var sb = new StringBuilder();
            EscapeProcessArgument(argument, sb);
            return sb.ToString();
        }

        private static void EscapeProcessArgument(string argument, StringBuilder sb)
        {
            // Normally, an Windows application (.NET applications too) parses
            // their command line using the CommandLineToArgvW function. Which has
            // some peculiar rules.
            // See http://msdn.microsoft.com/en-us/library/bb776391(VS.85).aspx

            // TODO how about backslashes? there seems to be a weird interaction
            //      between backslahses and double quotes...
            // TODO do test cases of this! even launch a second process that
            //      only dumps its arguments.

            if (argument.Contains('"'))
            {
                sb.Append('"');
                // escape single double quotes with another double quote.
                sb.Append(argument.Replace("\"", "\"\""));
                sb.Append('"');
            }
            else if (argument.Contains(' ')) // AND it does NOT contain double quotes! (those were catched in the previous test)
            {
                sb.Append('"');
                sb.Append(argument);
                sb.Append('"');
            }
            else
            {
                sb.Append(argument);
            }

            // TODO what about null/empty arguments?
        }
    }
}
