using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TubaToolbox.Models;
using TubaToolbox.Services;

namespace TubaToolbox.Views
{
    public partial class ToolsPage : UserControl
    {
        private ToolLauncher launcher;
        private Border lastSelectedCard;
        private DateTime lastClickTime;
        private Border lastClickCard;
        private string toolsBasePath;

        public ToolsPage(List<ToolItem> tools, string basePath)
        {
            InitializeComponent();
            toolsBasePath = basePath;
            launcher = new ToolLauncher(basePath);
            
            Loaded += (s, e) => BuildTools(tools);
        }

        private void BuildTools(List<ToolItem> tools)
        {
            toolsPanel.Children.Clear();

            foreach (var tool in tools)
            {
                var card = CreateToolCard(tool);
                toolsPanel.Children.Add(card);
            }
        }

        private Border CreateToolCard(ToolItem tool)
        {
            var card = new Border
            {
                Style = FindResource("ToolCard") as Style,
                Tag = tool,
                Cursor = Cursors.Hand
            };

            var stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12)
            };

            // 图标 - 从 exe 文件提取
            var iconElement = CreateIcon(tool);
            iconElement.Margin = new Thickness(0, 0, 0, 8);
            stackPanel.Children.Add(iconElement);

            // 名称
            var nameText = new TextBlock
            {
                Text = tool.Name,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 100,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stackPanel.Children.Add(nameText);

            card.Child = stackPanel;

            if (tool.IsInfoOnly)
            {
                card.Opacity = 0.5;
                card.Cursor = Cursors.Arrow;
            }
            else
            {
                card.MouseLeftButtonDown += (s, e) => OnCardMouseDown(card, tool);
            }

            return card;
        }

        private FrameworkElement CreateIcon(ToolItem tool)
        {
            // 尝试从 exe/bat 文件提取图标
            if (!string.IsNullOrEmpty(tool.RelativePath))
            {
                string fullPath = Path.Combine(toolsBasePath, tool.RelativePath);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        // 对于 exe 文件，提取图标
                        if (!tool.IsImage && (fullPath.EndsWith(".exe") || fullPath.EndsWith(".bat")))
                        {
                            var icon = ExtractIconFromFile(fullPath);
                            if (icon != null)
                            {
                                return new Image
                                {
                                    Source = icon,
                                    Width = 32,
                                    Height = 32
                                };
                            }
                        }
                        // 对于图片文件，直接显示图片
                        else if (tool.IsImage || fullPath.EndsWith(".jpg") || fullPath.EndsWith(".png"))
                        {
                            try
                            {
                                var bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(fullPath);
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.DecodePixelWidth = 32;
                                bitmap.DecodePixelHeight = 32;
                                bitmap.EndInit();
                                bitmap.Freeze();
                                return new Image
                                {
                                    Source = bitmap,
                                    Width = 32,
                                    Height = 32
                                };
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }

            // 使用配置中的 Emoji 图标作为后备
            return new TextBlock
            {
                Text = string.IsNullOrEmpty(tool.Icon) ? "🔧" : tool.Icon,
                FontSize = 32,
                FontFamily = new FontFamily("Segoe UI Emoji"),
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }

        private BitmapSource ExtractIconFromFile(string filePath)
        {
            try
            {
                // 使用 System.Drawing.Icon 提取图标
                using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath))
                {
                    if (icon != null)
                    {
                        // 转换为 BitmapSource
                        using (var bitmap = icon.ToBitmap())
                        using (var ms = new MemoryStream())
                        {
                            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            ms.Position = 0;
                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = ms;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.DecodePixelWidth = 32;
                            bitmapImage.DecodePixelHeight = 32;
                            bitmapImage.EndInit();
                            bitmapImage.Freeze();
                            return bitmapImage;
                        }
                    }
                }
            }
            catch { }

            // 对于 .bat 文件，尝试使用 shell32 的默认图标
            if (filePath.EndsWith(".bat"))
            {
                try
                {
                    var shellIcon = ExtractShellIcon(filePath);
                    if (shellIcon != null) return shellIcon;
                }
                catch { }
            }

            return null;
        }

        private BitmapSource ExtractShellIcon(string filePath)
        {
            try
            {
                // 使用 Windows API 提取图标（备用方案）
                using (var icon = System.Drawing.Icon.ExtractAssociatedIcon("cmd.exe"))
                {
                    if (icon != null)
                    {
                        using (var bitmap = icon.ToBitmap())
                        using (var ms = new MemoryStream())
                        {
                            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            ms.Position = 0;
                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = ms;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.DecodePixelWidth = 32;
                            bitmapImage.DecodePixelHeight = 32;
                            bitmapImage.EndInit();
                            bitmapImage.Freeze();
                            return bitmapImage;
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private void OnCardMouseDown(Border card, ToolItem tool)
        {
            if (lastClickCard == card && (DateTime.Now - lastClickTime).TotalMilliseconds < 300)
            {
                launcher.Launch(tool);
                lastClickCard = null;
            }
            else
            {
                ShowDescription(tool);
                HighlightCard(card);
                lastClickCard = card;
                lastClickTime = DateTime.Now;
            }
        }

        private void HighlightCard(Border selectedCard)
        {
            if (lastSelectedCard != null)
            {
                lastSelectedCard.BorderBrush = null;
                lastSelectedCard.BorderThickness = new Thickness(1);
            }

            selectedCard.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 215, 0));
            selectedCard.BorderThickness = new Thickness(2);
            lastSelectedCard = selectedCard;
        }

        private void ShowDescription(ToolItem tool)
        {
            if (descriptionText == null) return;
            
            string description = string.IsNullOrEmpty(tool.Description) ? "暂无简介" : tool.Description;
            descriptionText.Text = $"📌 {tool.Name}：{description}";
        }
    }
}