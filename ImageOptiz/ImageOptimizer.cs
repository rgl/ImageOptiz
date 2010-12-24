using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ImageOptiz
{
    public class ImageOptimizer
    {
        public static readonly string[] ValidExtensions = new[]
        {
            ".png",
            ".jpg",
            ".jpeg",
        };

        // TODO report progress and errors
        public static void Optimize(FileInfo fileInfo)
        {
            // TODO catch exceptions, and flag the image path has failed (with the error as tooltip).

            var extension = fileInfo.Extension.ToLower();

            switch (extension)
            {
                case ".png":
                    OptimizePng(fileInfo);
                    break;

                case ".jpg":
                case ".jpeg":
                    OptimizeJpeg(fileInfo);
                    break;

                default:
                    throw new ApplicationException(string.Format("Unsuported image file format extension {0}", extension));
            }
        }

        private static void OptimizePng(FileInfo fileInfo)
        {
            var optiPngResult = RunOptiPng(fileInfo);
            var advPngResult = RunAdvPng(fileInfo);
            var pngCrushResult = RunPngCrush(fileInfo);

            if (ToolExists("pngout"))
            {
                var pngOutResult = RunPngOut(fileInfo);
            }
        }

        private static void OptimizeJpeg(FileInfo fileInfo)
        {
            var jpegOptimResult = RunJpegOptim(fileInfo);
            var jpegTranResult = RunJpegTran(fileInfo);
        }

        private static OptimizeToolResult RunOptiPng(FileInfo fileInfo)
        {
            // TODO make sure the tool exited with success.
            return RunOptimizeTool(
                fileInfo,
                "optipng",
                    "-v",
                    "-o7",
                    fileInfo.FullName
                );
        }

        private static OptimizeToolResult RunAdvPng(FileInfo fileInfo)
        {
            // TODO make sure the tool exited with success.
            return RunOptimizeTool(
                fileInfo,
                "advpng",
                    "-z4",
                    fileInfo.FullName
                );
        }

        private static OptimizeToolResult RunPngCrush(FileInfo fileInfo)
        {
            var temporaryFileInfo = CreateTemporaryFile(fileInfo);

            // TODO make sure the tool exited with success.
            var result = RunOptimizeTool(
                fileInfo,
                temporaryFileInfo,
                "pngcrush",
                    "-rem", "gAMA",
                    "-rem", "alla",
                    "-rem", "cHRM",
                    "-rem", "iCCP",
                    "-rem", "sRGB",
                    "-rem", "time",
                    fileInfo.FullName,
                    temporaryFileInfo.FullName
                );

            var toolResult = result.ToolResult;

            if (toolResult.ExitCode == 0 && temporaryFileInfo.Exists && temporaryFileInfo.Length > 0 && fileInfo.Length > temporaryFileInfo.Length)
            {
                File.Delete(fileInfo.FullName);
                File.Move(temporaryFileInfo.FullName, fileInfo.FullName);
            }
            else
            {
                File.Delete(temporaryFileInfo.FullName);
            }

            return result;
        }

        private static OptimizeToolResult RunPngOut(FileInfo fileInfo)
        {
            return RunOptimizeTool(fileInfo, "pngout", "-y", fileInfo.FullName);
        }

        private static OptimizeToolResult RunJpegTran(FileInfo fileInfo)
        {
            var temporaryFileInfo = CreateTemporaryFile(fileInfo);

            // TODO make sure the tool exited with success.
            var result = RunOptimizeTool(
                    fileInfo,
                    temporaryFileInfo,
                    "jpegtran",
                        "-verbose",
                        "-copy", "none",
                        "-optimize",
                        fileInfo.FullName,
                        temporaryFileInfo.FullName
                );

            var toolResult = result.ToolResult;

            if (toolResult.ExitCode == 0 && temporaryFileInfo.Exists && temporaryFileInfo.Length > 0 && fileInfo.Length > temporaryFileInfo.Length)
            {
                File.Delete(fileInfo.FullName);
                File.Move(temporaryFileInfo.FullName, fileInfo.FullName);
            }
            else
            {
                File.Delete(temporaryFileInfo.FullName);
            }

            return result;
        }

        private static OptimizeToolResult RunJpegOptim(FileInfo fileInfo)
        {
            // TODO make sure the tool exited with success.
            return RunOptimizeTool(
                    fileInfo,
                    "jpegoptim",
                        "-f",
                        "--strip-all",
                        fileInfo.FullName
                );
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

        private static bool ToolExists(string tool)
        {
            var toolFileInfo = new FileInfo(GetToolPath(tool));
            return toolFileInfo.Exists;
        }

        private static string GetToolPath(string tool)
        {
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(Path.Combine(assemblyLocation, "tools"), string.Format("{0}.exe", tool));
        }

        private class OptimizeToolResult
        {
            public long UnoptimizedSize { get; set; }
            public long OptimizedSize { get; set; }
            public ToolResult ToolResult { get; set; }
        }

        private class ToolResult
        {
            public int ExitCode { get; set; }
            public string StandardOutput { get; set; }
            public string StandardError { get; set; }
        }

        private static OptimizeToolResult RunOptimizeTool(FileInfo sourceFileInfo, string tool, params string[] arguments)
        {
            return RunOptimizeTool(sourceFileInfo, sourceFileInfo, tool, arguments);
        }

        private static OptimizeToolResult RunOptimizeTool(FileInfo sourceFileInfo, FileInfo destinyFileInfo, string tool, params string[] arguments)
        {
            sourceFileInfo.Refresh();

            var result = new OptimizeToolResult { UnoptimizedSize = sourceFileInfo.Length };

            result.ToolResult = RunTool(tool, arguments);

            destinyFileInfo.Refresh();

            result.OptimizedSize = destinyFileInfo.Length;

            return result;
        }

        private static ToolResult RunTool(string tool, params string[] arguments)
        {
            var toolPath = GetToolPath(tool);
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
                    throw new ApplicationException(string.Format("Failed to launch tool {0}", tool));
                }

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                p.WaitForExit();

                return new ToolResult
                {
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