# 客服花花 (Huahua Helper) 🌸

> **专为电商客服打造的轻量级极速快捷回复小助手 · v1.3「蝶翼双飞」**  
> 一键将**文本话术、图片与视频文件**秒级穿透发送至**千牛客户端**聊天窗口，大幅提升客服接待效率！

[![Platform](https://img.shields.io/badge/Platform-Windows-0078D7?logo=windows)](https://github.com/l1011186781-commits/kefu-huahua)
[![Framework](https://img.shields.io/badge/.NET%20Framework-4.0%2B-512BD4?logo=.net)](https://github.com/l1011186781-commits/kefu-huahua)
[![Release](https://img.shields.io/badge/Release-v1.3%20%E8%9D%B6%E7%BF%BC%E5%8F%8C%E9%A3%9E-ff5000)](https://github.com/l1011186781-commits/kefu-huahua/releases)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 🌟 核心特性

- 🦋 **蝶翼双飞 · 文本与视频双模式 (v1.3 新增)**：
  - 新增 **「文本」** / **「视频」** 独立模式切换，各自拥有完全隔离的分类体系与话术列表，互不干扰。
  - 支持直接拖拽视频或手动选择视频创建快捷方式（自动提取文件名，可附加说明文字），一键「填 / 发 / 双击直发」千牛窗口。
- 📂 **零冗余原路径识别 (v1.3 新增)**：
  - 导入图片、视频或自定义背景图时，默认**直接识别电脑原位置**，不再复制大文件到软件目录占用磁盘空间；设置中亦提供「复制图片和视频到软件目录」按需开关。
- 💾 **全量打包备份与换机恢复 (v1.3 升级)**：
  - 导出 `.zip` 备份包时流式直写打包分类、话术、图片、视频及自定义背景图；内置标准 JSON 解析器与路径防损修复引擎，兼容换电脑一键导入恢复。
- 🚀 **千牛极速秒级穿透**：底层 Win32 API 智能检测与激活千牛接待窗口，支持**填入**（复制到输入框）与**直发**（自动模拟回车发送），图文混合/视频+附言无缝发送。
- 🎨 **双主题与深色护眼升级 (v1.3 优化)**：
  - 内置 **「暖橙纸感」**（淘宝橙 `#FF5000` + 暖纸白 `#FAF7F2`）与 **「樱粉花语」**（站点粉 `#FA7298` + 粉纸白 `#FDF8F9`）两套配色，配专属深色护眼模式（暖炭黑 / 夜樱深梅）。
  - 顶栏采用自绘分层玻璃透传 + 圆形裁剪头像 + 透明自绘置顶框，消除深色模式黑框与杂边；按钮、设置弹窗与提示词窗口全面适配深色护眼风格。
- ⚡ **单核 + 多核丝滑性能**：后台多核线程预渲染自定义壁纸缓存、消除无效重绘，大量话术时启用 PLINQ 多核并行搜索，上下滑动零卡顿。
- 🧹 **便捷管理与双重防误触**：支持分别**一键清理所有提示词**与**一键清理所有视频**（均带二次确认弹窗）、一键清理背景图缓存、窗口大小与位置记忆，并附带 GitHub 项目主页直达。
- 🐣 **可爱彩蛋**：点击左上角客服花花头像，可随时查看专属高清立绘（单文件绿色版已内置资源，随处运行皆可触发）。

---

## 💻 快速编译运行

本项目纯原生 C# 开发，**无需安装 Visual Studio 或重型 IDE**，系统自带的 .NET Framework 4.0/4.8 即可完成编译。

### 方式一：双击一键编译
直接运行项目目录下的 `build.bat` 脚本即可在同目录下生成 `客服花花.exe`。

### 方式二：命令行编译
打开 Windows PowerShell 或 CMD，执行以下命令：
```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /win32icon:"app.ico" /win32manifest:"app.manifest" /out:"客服花花.exe" Program.cs
```

---

## 📖 目录结构

```text
kefu-huahua/
├── Program.cs          # 核心源代码（Win32 穿透、双模式逻辑、标准 JSON、液体玻璃渲染与 UI）
├── app.ico             # 应用程序高清图标
├── app.manifest        # Windows 10/11 兼容清单与权限声明
├── avatar.png          # 软件专属头像与彩蛋图片
├── build.bat           # Windows 一键编译脚本
├── start.bat           # 一键后台启动脚本
├── .gitignore          # Git 忽略配置（自动过滤用户词库 phrases.json 与媒体缓存目录）
└── README.md           # 项目介绍与说明文档
```

---

## 🤝 开源与许可

本项目基于 MIT 协议开源。
