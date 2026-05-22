using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TubaToolbox.Models;

namespace TubaToolbox.Services
{
    public static class ConfigManager
    {
        private static string configPath;
        private static ConfigData configData;

        public static void Initialize(string basePath)
        {
            configPath = Path.Combine(basePath, "config.json");
            LoadConfig();
        }

        private static void LoadConfig()
        {
            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    configData = JsonSerializer.Deserialize<ConfigData>(json);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"加载配置文件失败：{ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    configData = new ConfigData();
                }
            }
            else
            {
                configData = new ConfigData();
                SaveDefaultConfig();
            }
        }

        private static void SaveDefaultConfig()
        {
            configData = new ConfigData
            {
                Categories = new List<Category>(),
                Tools = new List<ToolItem>()
            };
            SaveConfig();
        }

        public static void SaveConfig()
        {
            try
            {
                string json = JsonSerializer.Serialize(configData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);
            }
            catch { }
        }

        public static List<Category> GetCategories()
        {
            return configData?.Categories ?? new List<Category>();
        }

        public static List<ToolItem> GetToolsByCategory(string categoryName)
        {
            return configData?.Tools?.FindAll(t => t.Category == categoryName) ?? new List<ToolItem>();
        }
    }

    public class ConfigData
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<ToolItem> Tools { get; set; } = new List<ToolItem>();
    }
}