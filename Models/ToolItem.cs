namespace TubaToolbox.Models
{
    public class ToolItem
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public string RelativePath { get; set; } = "";
        public string Icon { get; set; } = "🔧";
        public bool IsImage { get; set; } = false;
        public bool IsInfoOnly { get; set; } = false;
    }

    public class Category
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "📁";
    }
}