using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using TubaToolbox.Models;

namespace TubaToolbox.Services
{
    public class ToolLauncher
    {
        private string basePath;

        public ToolLauncher(string basePath)
        {
            this.basePath = basePath;
        }

        public void Launch(ToolItem tool)
        {
            try
            {
                if (string.IsNullOrEmpty(tool.RelativePath))
                {
                    MessageBox.Show($"未配置路径", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string fullPath = Path.Combine(basePath, tool.RelativePath);

                if (!File.Exists(fullPath))
                {
                    MessageBox.Show($"文件不存在：{fullPath}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = fullPath,
                    WorkingDirectory = Path.GetDirectoryName(fullPath),
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}