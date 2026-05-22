using System.Collections.Generic;
using TubaToolbox.Models;
using TubaToolbox.Services;

namespace TubaToolbox.Data
{
    public static class ToolConfig
    {
        public static List<Category> GetCategories()
        {
            return ConfigManager.GetCategories();
        }

        public static List<ToolItem> GetToolsByCategory(string categoryName)
        {
            return ConfigManager.GetToolsByCategory(categoryName);
        }
    }
}