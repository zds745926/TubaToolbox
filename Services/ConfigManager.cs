using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Security.Cryptography;
using TubaToolbox.Models;

namespace TubaToolbox.Services
{
    public static class ConfigManager
    {
        private static string toolsBasePath;
        private static string cachePath;
        private static List<Category> categories;
        private static List<ToolItem> tools;

        public static void Initialize(string basePath)
        {
            toolsBasePath = Path.Combine(basePath, "tools");
            cachePath = Path.Combine(toolsBasePath, ".toolbox_cache.json");
            LoadOrScan();
        }

        private static void LoadOrScan()
        {
            // 检查缓存是否存在且有效
            if (File.Exists(cachePath))
            {
                try
                {
                    string json = File.ReadAllText(cachePath);
                    var cache = JsonSerializer.Deserialize<CacheData>(json);
                    
                    if (cache != null && cache.ToolsHash == GetDirectoryHash())
                    {
                        // 缓存有效，直接使用
                        categories = cache.Categories;
                        tools = cache.Tools;
                        return;
                    }
                }
                catch { }
            }

            // 缓存无效或不存在，重新扫描
            ScanToolsFolder();
            
            // 保存缓存到 tools 目录
            SaveCache();
        }

        private static string GetDirectoryHash()
        {
            if (!Directory.Exists(toolsBasePath))
                return "";

            try
            {
                var files = Directory.GetFiles(toolsBasePath, "*.*", SearchOption.AllDirectories);
                var hashData = "";
                
                foreach (var file in files)
                {
                    // 排除缓存文件自身
                    if (file == cachePath) continue;
                    
                    string ext = Path.GetExtension(file).ToLower();
                    if (ext == ".exe" || ext == ".bat")
                    {
                        var info = new FileInfo(file);
                        hashData += $"{file}|{info.LastWriteTime.Ticks}|{info.Length}|";
                    }
                }

                using (var md5 = MD5.Create())
                {
                    byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(hashData);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);
                    return Convert.ToBase64String(hashBytes);
                }
            }
            catch
            {
                return "";
            }
        }

        private static void ScanToolsFolder()
        {
            categories = new List<Category>();
            tools = new List<ToolItem>();

            if (!Directory.Exists(toolsBasePath))
            {
                return;
            }

            var directories = Directory.GetDirectories(toolsBasePath);
            
            foreach (var dir in directories)
            {
                var dirInfo = new DirectoryInfo(dir);
                string categoryName = dirInfo.Name;
                
                // 跳过点开头的隐藏文件夹
                if (categoryName.StartsWith(".")) continue;

                categories.Add(new Category
                {
                    Name = categoryName,
                    Icon = GetFolderIcon(categoryName)
                });

                ScanDirectoryForTools(dir, categoryName, tools);
            }
        }

        private static void ScanDirectoryForTools(string directory, string categoryName, List<ToolItem> toolsList)
        {
            try
            {
                var files = Directory.GetFiles(directory);
                foreach (var file in files)
                {
                    string ext = Path.GetExtension(file).ToLower();
                    
                    if (ext == ".exe" || ext == ".bat")
                    {
                        var fileInfo = new FileInfo(file);
                        string relativePath = GetRelativePath(file);
                        
                        // 获取显示名称
                        string displayName = GetDisplayName(file, fileInfo.Name);
                        
                        toolsList.Add(new ToolItem
                        {
                            Name = displayName,
                            Description = $"{fileInfo.Length / 1024} KB",
                            Category = categoryName,
                            RelativePath = relativePath,
                            Icon = GetFileIcon(ext),
                            IsImage = false,
                            IsInfoOnly = false
                        });
                    }
                }

                // 递归扫描子文件夹
                var subDirectories = Directory.GetDirectories(directory);
                foreach (var subDir in subDirectories)
                {
                    string subDirName = new DirectoryInfo(subDir).Name;
                    if (subDirName.StartsWith(".")) continue;
                    ScanDirectoryForTools(subDir, categoryName, toolsList);
                }
            }
            catch { }
        }

        private static string GetDisplayName(string filePath, string fileName)
        {
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            
            // 特殊文件名需要显示父文件夹名称
            if (nameWithoutExt.Equals("start", StringComparison.OrdinalIgnoreCase) ||
                nameWithoutExt.Equals("main", StringComparison.OrdinalIgnoreCase) ||
                nameWithoutExt.Equals("run", StringComparison.OrdinalIgnoreCase) ||
                nameWithoutExt.Equals("launch", StringComparison.OrdinalIgnoreCase) ||
                nameWithoutExt.Equals("启动", StringComparison.OrdinalIgnoreCase))
            {
                // 获取父文件夹名称
                var parentDir = Directory.GetParent(filePath);
                if (parentDir != null)
                {
                    return parentDir.Name;
                }
            }
            
            return nameWithoutExt;
        }

        private static string GetRelativePath(string fullPath)
        {
            if (fullPath.StartsWith(toolsBasePath))
            {
                return fullPath.Substring(toolsBasePath.Length + 1);
            }
            return fullPath;
        }

        private static void SaveCache()
        {
            try
            {
                // 确保 tools 目录存在
                if (!Directory.Exists(toolsBasePath))
                {
                    Directory.CreateDirectory(toolsBasePath);
                }

                var cache = new CacheData
                {
                    ToolsHash = GetDirectoryHash(),
                    Categories = categories,
                    Tools = tools,
                    LastUpdate = DateTime.Now
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(cache, options);
                File.WriteAllText(cachePath, json);
                
                // 设置为隐藏文件
                File.SetAttributes(cachePath, FileAttributes.Hidden);
            }
            catch { }
        }

        // 手动刷新（当用户添加新工具时调用）
        public static void Refresh()
        {
            ScanToolsFolder();
            SaveCache();
        }

        private static string GetFolderIcon(string folderName)
        {
            if (folderName.Contains("CPU") || folderName.Contains("处理器")) return "⚡";
            if (folderName.Contains("显卡") || folderName.Contains("GPU")) return "🎮";
            if (folderName.Contains("内存")) return "💾";
            if (folderName.Contains("磁盘") || folderName.Contains("硬盘")) return "💿";
            if (folderName.Contains("屏幕") || folderName.Contains("显示器")) return "🖥️";
            if (folderName.Contains("外设")) return "⌨️";
            if (folderName.Contains("烤鸡") || folderName.Contains("压力")) return "🔥";
            if (folderName.Contains("游戏")) return "🎮";
            if (folderName.Contains("综合") || folderName.Contains("检测")) return "📊";
            if (folderName.Contains("主板")) return "🔌";
            return "🔧";
        }

        private static string GetFileIcon(string extension)
        {
            if (extension == ".exe") return "⚡";
            if (extension == ".bat") return "📜";
            return "🔧";
        }

        public static List<Category> GetCategories()
        {
            return categories ?? new List<Category>();
        }

        public static List<ToolItem> GetToolsByCategory(string categoryName)
        {
            return tools?.FindAll(t => t.Category == categoryName) ?? new List<ToolItem>();
        }
    }

    public class CacheData
    {
        public string ToolsHash { get; set; } = "";
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<ToolItem> Tools { get; set; } = new List<ToolItem>();
        public DateTime LastUpdate { get; set; }
    }
}