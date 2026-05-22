using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TubaToolbox.Data;
using TubaToolbox.Views;
using TubaToolbox.Services;  // 添加这行

namespace TubaToolbox
{
    public partial class MainWindow : Window
    {
        private string toolsBasePath;
        private Button lastClickedButton;

        public MainWindow()
        {
            toolsBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools");
            InitializeComponent();
            ConfigManager.Initialize(AppDomain.CurrentDomain.BaseDirectory);  // 添加这行
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
            CheckToolsPath();
            ShowCategory("硬件信息");
        }

        private void LoadCategories()
        {
            categoriesPanel.Children.Clear();
            var categories = ToolConfig.GetCategories();  // 改为 GetCategories()

            foreach (var category in categories)
            {
                var btn = new Button
                {
                    Content = $"{category.Icon}  {category.Name}",
                    Tag = category.Name,
                    Style = FindResource("CategoryButton") as Style,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 2, 0, 2)
                };
                btn.Click += Category_Click;
                categoriesPanel.Children.Add(btn);
            }
        }

        private void CheckToolsPath()
        {
            if (!Directory.Exists(toolsBasePath))
            {
                var warningBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                    Margin = new Thickness(20),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(15)
                };

                var warningPanel = new StackPanel();
                
                warningPanel.Children.Add(new TextBlock
                {
                    Text = "⚠️ 提示",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Yellow,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                warningPanel.Children.Add(new TextBlock
                {
                    Text = $"未找到工具目录：{toolsBasePath}\n\n请将 TubaToolbox.exe 放到工具箱目录下运行（与 tools 文件夹同级），否则工具列表中的程序将无法启动。",
                    FontSize = 12,
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap
                });

                warningBorder.Child = warningPanel;
                contentArea.Content = warningBorder;
            }
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string categoryName)
            {
                if (lastClickedButton != null)
                {
                    lastClickedButton.Background = Brushes.Transparent;
                    lastClickedButton.Foreground = new SolidColorBrush(Color.FromRgb(0xDC, 0xDC, 0xDC));
                }

                btn.Background = new SolidColorBrush(Color.FromRgb(0x29, 0x80, 0xB9));
                btn.Foreground = Brushes.White;
                lastClickedButton = btn;

                ShowCategory(categoryName);
            }
        }

        private void ShowCategory(string categoryName)
        {
            try
            {
                if (categoryName == "硬件信息")
                {
                    contentArea.Content = new HardwareInfoPage();
                }
                else
                {
                    var tools = ToolConfig.GetToolsByCategory(categoryName);
                    contentArea.Content = new ToolsPage(tools, toolsBasePath);
                }
            }
            catch (Exception ex)
            {
                contentArea.Content = new TextBlock
                {
                    Text = $"加载失败：{ex.Message}",
                    Foreground = Brushes.Red,
                    FontSize = 14,
                    Margin = new Thickness(20)
                };
            }
        }
    }
}