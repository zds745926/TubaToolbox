# TubaToolbox
这是一个基于图吧工具箱二次开发的工具箱
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
│ └── Release/ # Release 版本编译结果
│
└── obj/ # 临时编译文件目录（已通过 .gitignore 忽略）
