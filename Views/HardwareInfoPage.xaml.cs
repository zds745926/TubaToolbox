#nullable disable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TubaToolbox.Services;

namespace TubaToolbox.Views
{
    public partial class HardwareInfoPage : UserControl
    {
        private bool _isLoading = false;
        private CancellationTokenSource? _cancellationTokenSource;
        
        private static HardwareInfo? _cachedHardwareInfo;
        private static DateTime _lastCacheTime = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        private static readonly object CacheLock = new object();

        public HardwareInfoPage()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadHardwareInfoAsync();
            Unloaded += (s, e) => CancelLoading();
        }

        private void CancelLoading()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private async Task LoadHardwareInfoAsync(bool forceRefresh = false)
        {
            if (_isLoading) return;
            
            _isLoading = true;
            CancelLoading();
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                HardwareInfo? hardwareInfo = null;
                
                if (!forceRefresh && IsCacheValid())
                {
                    lock (CacheLock)
                    {
                        hardwareInfo = _cachedHardwareInfo;
                    }
                    
                    if (hardwareInfo != null)
                    {
                        await Dispatcher.InvokeAsync(() => BuildUI(hardwareInfo));
                        _isLoading = false;
                        return;
                    }
                }
                
                await Dispatcher.InvokeAsync(ShowLoadingState);
                
                hardwareInfo = await Task.Run(() => HardwareInfoService.GetHardwareInfo(_cancellationTokenSource.Token));
                
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    return;
                
                lock (CacheLock)
                {
                    _cachedHardwareInfo = hardwareInfo;
                    _lastCacheTime = DateTime.Now;
                }
                
                await Dispatcher.InvokeAsync(() => BuildUI(hardwareInfo));
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() => ShowErrorState(ex.Message));
            }
            finally
            {
                _isLoading = false;
            }
        }

        private bool IsCacheValid()
        {
            lock (CacheLock)
            {
                return _cachedHardwareInfo != null && 
                       DateTime.Now - _lastCacheTime < CacheDuration;
            }
        }

        private void ShowLoadingState()
        {
            rootPanel.Children.Clear();
            var loadingText = new TextBlock
            {
                Text = "正在加载硬件信息...",
                FontSize = 14,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            rootPanel.Children.Add(loadingText);
        }

        private void ShowErrorState(string errorMessage)
        {
            rootPanel.Children.Clear();
            var errorText = new TextBlock
            {
                Text = $"加载失败：{errorMessage}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100)),
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            rootPanel.Children.Add(errorText);
        }

        private void BuildUI(HardwareInfo info)
        {
            rootPanel.Children.Clear();

            // 标题
            var title = new TextBlock
            {
                Text = "硬件信息",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                Margin = new Thickness(0, 0, 0, 15)
            };
            rootPanel.Children.Add(title);

            // 信息卡片
            AddInfoCard("型号信息", info.ComputerModel);
            AddInfoCard("系统信息", info.SystemInfo);
            AddInfoCard("运行时间", info.Uptime);

            // 详细信息标题
            var detailTitle = new TextBlock
            {
                Text = "详细信息",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0xDC, 0xFF, 0xFF)),
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                Margin = new Thickness(0, 15, 0, 10)
            };
            rootPanel.Children.Add(detailTitle);

            // 详细信息列表
            var details = new Dictionary<string, string>
            {
                ["处理器"] = info.Processor,
                ["主板"] = info.Motherboard,
                ["内存"] = info.Memory,
                ["显卡"] = info.Graphics,
                ["显示器"] = info.Monitor,
                ["磁盘"] = info.Disk,
                ["声卡"] = info.SoundCard,
                ["网卡"] = info.NetworkAdapter
            };

            foreach (var detail in details)
            {
                var label = new TextBlock
                {
                    Text = $"{detail.Key}：{detail.Value}",
                    FontSize = 13,
                    Foreground = Brushes.White,
                    FontFamily = new FontFamily("Microsoft YaHei UI"),
                    Margin = new Thickness(0, 4, 0, 2),
                    TextWrapping = TextWrapping.Wrap
                };
                rootPanel.Children.Add(label);
            }
        }

        private void AddInfoCard(string labelText, string valueText)
        {
            var cardBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(45, 255, 255, 255)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var label = new TextBlock
            {
                Text = labelText + "：",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(130, 200, 255)),
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            var value = new TextBlock
            {
                Text = valueText,
                FontSize = 13,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5, 0, 0, 0)
            };
            Grid.SetColumn(value, 1);
            grid.Children.Add(value);

            cardBorder.Child = grid;
            rootPanel.Children.Add(cardBorder);
        }
    }
}