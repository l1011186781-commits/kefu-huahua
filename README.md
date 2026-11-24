# 客服花花 (Huahua Helper) 🌸

> **专为电商客服打造的轻量级极速快捷回复小助手**  
> 一键将话术与图片秒级穿透发送至**千牛客户端**聊天窗口，大幅提升客服接待效率！

[![Platform](https://img.shields.io/badge/Platform-Windows-0078D7?logo=windows)](https://github.com/l1011186781-commits/kefu-huahua)
[![Framework](https://img.shields.io/badge/.NET%20Framework-4.0%2B-512BD4?logo=.net)](https://github.com/l1011186781-commits/kefu-huahua)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 🌟 核心特性

- 🚀 **千牛极速秒级穿透**：底层 Win32 API 智能检测与激活千牛接待窗口，支持**填入**（复制到输入框）与**直发**（自动模拟回车发送）。
- 🖼️ **图文混合穿透**：支持话术附带图片。发送时先贴入文字再无缝发送图片，告别繁琐的手工找图。
- 📋 **极简图片导入**：支持剪贴板图片自动识别，或从本地文件管理器**一键拖拽**图片到编辑窗口直接导入。
- 🎨 **iOS 液体玻璃（Liquid Glass）拟态 UI**：
  - 双层半透明光感反射胶囊按键。
  - 纯矢量高精自绘齿轮图标，自适应 DPI 缩放，永不遮挡。
- 📂 **完全自定义分类体系**：零预设冗余分类，完全由客服自由增删、重命名与个性化归纳。
- 🔍 **全分类实时极速搜索**：支持拼音与关键词模糊匹配，海量话术毫秒级直达。
- 💾 **全量备份与跨机无缝迁移**：设置内支持一键导出全量数据包（`.zip` 打包话术与所有图片），换电脑只需一键导入即可 100% 恢复。
- 🔠 **4 档字体与主题背景**：
  - 支持标准 (9pt)、中号 (11pt)、大号 (13pt)、特大 (15pt) 随意切换。
  - 支持经典白、极夜黑，以及用户**自定义本地壁纸图片**。
- 🐣 **可爱彩蛋**：点击左上角客服花花头像，可随时查看专属高清立绘。

---

## 💻 快速编译运行

本项目纯原生 C# 开发，**无需安装 Visual Studio 或重型 IDE**，系统自带的 .NET Framework 4.0/4.8 即可完成编译。

### 方式一：双击一键编译
直接运行项目目录下的 `build.bat` 脚本即可在同目录下生成 `客服花花.exe`。

### 方式二：命令行编译
打开 Windows PowerShell 或 CMD，执行以下命令：
```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /r:System.IO.Compression.FileSystem.dll /win32icon:"app.ico" /out:"客服花花.exe" Program.cs
```

---

## 📖 目录结构

```text
kefu-huahua/
├── Program.cs          # 核心源代码（包含 Win32 穿透、液体玻璃渲染、UI 与业务逻辑）
├── app.ico             # 应用程序高清图标
├── avatar.png          # 软件专属头像与彩蛋图片
├── build.bat           # Windows 一键编译脚本
├── .gitignore          # Git 忽略配置
└── README.md           # 项目介绍与说明文档
```

---

## 🤝 开源与许可

本项目基于 MIT 协议开源。制作：Gemini antigravity。模型：3.8flash。
