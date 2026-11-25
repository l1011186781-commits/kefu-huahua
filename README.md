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
- 🎨 **双主题与 iOS 26 Liquid Glass 质感 (v1.2 新增)**：
  - 内置 **「暖橙纸感」**（淘宝橙 `#FF5000` + 暖纸白 `#FAF7F2`）与 **「樱粉花语」**（站点粉 `#FA7298` + 粉纸白 `#FDF8F9`，头像带 `2px` 粉圈）两套配色，均配专属深色护眼模式（暖炭黑 / 夜樱深梅）。
  - 顶栏与主按钮采用 WinForms + GDI+ 自绘 iOS 26 液态玻璃（分层模糊、内侧折射、`1px` 顶部高光、内外阴影），话术行保持实色清晰易读。
- ⚡ **单核 + 多核丝滑性能 (v1.2 新增)**：后台多核线程预渲染自定义壁纸缓存、消除无效重绘，大量话术时启用 PLINQ 多核并行搜索，上下滑动零卡顿。
- 🧹 **便捷管理与清理 (v1.2 新增)**：设置内置「一键删图」（清理历史背景缓存）与「一键清理所有提示词」（带二次确认），并附带 GitHub 开源地址直达。
- 📐 **智能窗口记忆**：自动记忆您上次关闭或调整后的窗口大小与屏幕位置，下次启动完美还原，无需每次重新拖拽。
- 🐣 **可爱彩蛋**：点击左上角客服花花头像，可随时查看专属高清立绘（单文件绿色版已内置资源，随处运行皆可触发）。

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

本项目基于 MIT 协议开源。
