# Tuba Toolbox

> 基于图吧工具箱二次开发的硬件检测工具合集启动器，采用 WPF + .NET 8 重写，支持配置文件化工具管理。

## 项目简介

Tuba Toolbox 是在经典「图吧工具箱」基础上进行二次开发的版本。原版工具箱集成了大量硬件检测工具，但存在界面陈旧、编码问题等不足。

本项目主要改进：

- **配置文件化**：所有工具分类和路径通过 `config.json` 管理，无需修改代码即可增删工具
- **现代界面**：采用液态玻璃设计风格，支持高 DPI 缩放
- **双击启动**：单击工具卡片显示简介，双击启动程序，避免误触
- **自动图标**：自动从 exe 文件中提取图标显示

原版图吧工具箱：https://www.tbtool.cn

## 功能特性

### 硬件信息检测

通过 WMI 自动获取电脑硬件配置信息，包括：
- 处理器型号、核心数、线程数
- 主板厂商、型号
- 内存容量、插槽数
- 显卡型号
- 磁盘型号、容量
- 声卡、网卡信息
- 系统版本、运行时间

### 工具分类管理

支持 12 个工具分类，所有工具通过 `config.json` 配置：

| 分类 | 说明 |
|------|------|
| CPU工具 | CPU-Z、Core Temp、Prime95、LinX 等 |
| 主板工具 | AIDA64、HWiNFO、RWEverything |
| 内存工具 | MemTest、Thaiphoon、TM5 |
| 显卡工具 | GPU-Z、FurMark、DDU、nvidiaInspector |
| 磁盘工具 | CrystalDiskInfo、AS SSD、DiskGenius |
| 屏幕工具 | 坏点测试、色域检测、UFO测试 |
| 综合工具 | AIDA64、HWiNFO、Speccy、HWMonitor |
| 外设工具 | 键盘测试、鼠标测试 |
| 烤鸡工具 | FurMark、Prime95、一键烤机 |
| 游戏工具 | Steam/Epic/战网下载、加速器 |
| 其他工具 | Dism++、Everything、Ventoy、rufus |

> 具体工具列表可通过编辑 `config.json` 自由增删。

## 技术栈

- 框架：WPF (.NET 8)
- 语言：C# / XAML
- 硬件检测：System.Management (WMI)
- 配置管理：System.Text.Json
- 图标提取：System.Drawing.Common

## 📁 项目结构
    TubaToolbox/
        ├── .gitignore # Git 忽略文件配置
        ├── LICENSE # MIT 开源协议
        ├── README.md # 项目说明文档
        ├── TubaToolbox.csproj # 项目配置文件
        │
        ├── App.xaml # 应用程序入口 XAML（全局样式定义）
        ├── App.xaml.cs # 应用程序逻辑（单例模式、全局异常处理）
        │
        ├── MainWindow.xaml # 主窗口界面（左侧分类栏 + 右侧内容区）
        ├── MainWindow.xaml.cs # 主窗口逻辑（分类切换、工具加载）
        │
        ├── Data/ # 数据配置层
        │ └── ToolConfig.cs # 工具配置管理（从 config.json 读取）
        │
        ├── Models/ # 数据模型层
        │ └── ToolItem.cs # 工具项模型（名称、路径、图标、描述等）
        │ └── Category.cs # 分类模型（分类名称、图标）
        │
        ├── Services/ # 服务层
        │ ├── ConfigManager.cs # 配置文件管理器（读取 config.json）
        │ ├── HardwareInfoService.cs # 硬件信息采集服务（WMI 查询）
        │ └── ToolLauncher.cs # 工具启动器（双击启动外部程序）
        │
        ├── Views/ # 视图层（UI 页面）
        │ ├── HardwareInfoPage.xaml # 硬件信息页面
        │ ├── HardwareInfoPage.xaml.cs # 硬件信息逻辑（缓存、WMI 调用）
        │ ├── ToolsPage.xaml # 工具列表页面
        │ └── ToolsPage.xaml.cs # 工具列表逻辑（卡片生成、双击启动）
        │
        ├── Resources/ # 资源文件
        │ └── Styles.xaml # 全局样式（液态玻璃卡片、按钮样式）
        │
        ├── bin/ # 编译输出目录（已通过 .gitignore 忽略）
         └── Release/ # Release 版本编译结果
        │
        └── obj/ # 临时编译文件目录（已通过 .gitignore 忽略）