using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: AssemblyTitle("客服花花")]
[assembly: AssemblyDescription("千牛工作台快捷回复辅助工具 - macOS × iOS 融合桌面版")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("HuahuaHelper")]
[assembly: AssemblyProduct("客服花花")]
[assembly: AssemblyCopyright("Copyright (C) 2026 HuahuaHelper")]
[assembly: AssemblyVersion("1.4.0.0")]
[assembly: AssemblyFileVersion("1.4.0.0")]

namespace HuahuaHelper
{
    public class PhraseItem
    {
        public string Id { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImagePath { get; set; }
        public string ItemType { get; set; } // "text" 或 "video"
        public string VideoPath { get; set; }

        public PhraseItem()
        {
            Id = Guid.NewGuid().ToString("N");
            ImagePath = "";
            ItemType = "text";
            VideoPath = "";
        }

        public PhraseItem(string cat, string title, string content, string imgPath, string itemType = "text", string videoPath = "")
        {
            Id = Guid.NewGuid().ToString("N");
            Category = cat;
            Title = title;
            Content = content;
            ImagePath = imgPath ?? "";
            ItemType = string.IsNullOrEmpty(itemType) ? "text" : itemType;
            VideoPath = videoPath ?? "";
        }
    }

    public class AppSettings
    {
        public int FontSizeLevel { get; set; } // 0:标准(9pt), 1:中号(11pt), 2:大号(13pt), 3:特大(15pt)
        public string ThemeType { get; set; }   // "white", "black", "custom"
        public string ColorScheme { get; set; } // "orange": 暖橙纸感, "pink": 樱粉花语
        public string CustomBgPath { get; set; }
        public bool CopyMediaToLocal { get; set; } // 是否复制图片/视频/背景到软件目录（默认 false：直接识别原路径）
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public int WindowX { get; set; }
        public int WindowY { get; set; }
        public bool HasWindowPos { get; set; }

        public AppSettings()
        {
            FontSizeLevel = 0;
            ThemeType = "white";
            ColorScheme = "orange";
            CustomBgPath = "";
            CopyMediaToLocal = false;
            WindowWidth = 320;
            WindowHeight = 680;
            WindowX = -1;
            WindowY = -1;
            HasWindowPos = false;
        }
    }

    public class AppData
    {
        public AppSettings Settings { get; set; }
        public List<string> Categories { get; set; }      // 文本话术分类
        public List<string> VideoCategories { get; set; } // 视频快捷方式独立分类
        public List<PhraseItem> Phrases { get; set; }

        public AppData()
        {
            Settings = new AppSettings();
            Categories = new List<string>();
            VideoCategories = new List<string>();
            Phrases = new List<PhraseItem>();
        }
    }

    /// <summary>
    /// 桌面级 Design Tokens 与双主题色板定义（浅色 / 深色分级表面、发丝边框、交互状态）
    /// </summary>
    public class ThemePalette
    {
        public bool IsPink;
        public Color Primary;
        public Color PrimaryHover;
        public Color PrimaryPressed;
        public Color DangerRed;
        public Color SwitchOnGreen;
        public Color FocusRing;

        // 浅色表面层级与文字
        public Color LightPaperBg;
        public Color LightSidebarBg;
        public Color LightCardBg;
        public Color LightTextPrimary;
        public Color LightTextSecondary;
        public Color LightTextMuted;
        public Color LightChipText;
        public Color LightDivider;
        public Color LightBorder;
        public Color LightRowHover;
        public Color LightRowSelected;
        public Color LightSearchBg;
        public Color LightScrollThumb;

        // 深色表面层级与文字
        public Color DarkPaperBg;
        public Color DarkSidebarBg;
        public Color DarkCardBg;
        public Color DarkSecondaryBtnBg;
        public Color DarkElevatedBg;
        public Color DarkTextPrimary;
        public Color DarkTextSecondary;
        public Color DarkTextMuted;
        public Color DarkDivider;
        public Color DarkPrimaryAccent;
        public Color DarkRowHover;
        public Color DarkRowSelected;
        public Color DarkScrollThumb;
    }

    public static class WarmTheme
    {
        // 核心尺寸与圆角 Tokens
        public const int RadiusControl = 7;        // 常规按钮/输入框圆角 (6px-8px)
        public const int RadiusDialogBtn = 9;      // 弹窗主确认按钮圆角 (8px-10px)
        public const int RadiusCard = 11;          // 内容卡片圆角 (10px-12px)
        public const int BtnHeightToolbar = 28;    // 工具栏/次按钮高度 (28px-32px)
        public const int BtnHeightDialog = 34;     // 弹窗主按钮高度 (32px-36px)

        // 发丝分割线 & 半透明交互色 Tokens
        public static readonly Color HairlineLight = Color.FromArgb(20, 0, 0, 0);          // rgba(0,0,0,0.08)
        public static readonly Color HairlineDark = Color.FromArgb(26, 255, 255, 255);     // rgba(255,255,255,0.10)
        public static readonly Color BorderSecondaryLight = Color.FromArgb(26, 0, 0, 0);   // rgba(0,0,0,0.10)
        public static readonly Color TopHighlightDark = Color.FromArgb(30, 255, 255, 255); // rgba(255,255,255,0.12)
        public static readonly Color TopHighlightPrimary = Color.FromArgb(52, 255, 255, 255); // inset 0 1px 0 rgba(255,255,255,0.2)
        public static readonly Color GhostHoverLight = Color.FromArgb(15, 0, 0, 0);        // rgba(0,0,0,0.06)
        public static readonly Color GhostHoverDark = Color.FromArgb(20, 255, 255, 255);   // rgba(255,255,255,0.08)
        public static readonly Color SegmentedTrackLight = Color.FromArgb(14, 0, 0, 0);    // 凹槽底
        public static readonly Color SegmentedTrackDark = Color.FromArgb(18, 255, 255, 255);
        public static readonly Color DestructiveRed = Color.FromArgb(255, 59, 48);         // #FF3B30
        public static readonly Color SwitchGreen = Color.FromArgb(52, 199, 89);            // #34C759

        private static string _cachedFontFamily = null;
        public static string UiFontFamily
        {
            get
            {
                if (_cachedFontFamily != null) return _cachedFontFamily;
                string[] preferred = new string[] { "Segoe UI Variable Text", "Segoe UI", "Microsoft YaHei UI" };
                using (InstalledFontCollection ifc = new InstalledFontCollection())
                {
                    foreach (string name in preferred)
                    {
                        for (int i = 0; i < ifc.Families.Length; i++)
                        {
                            if (string.Equals(ifc.Families[i].Name, name, StringComparison.OrdinalIgnoreCase))
                            {
                                _cachedFontFamily = name;
                                return _cachedFontFamily;
                            }
                        }
                    }
                }
                _cachedFontFamily = "Microsoft YaHei UI";
                return _cachedFontFamily;
            }
        }

        public static Font CreateFont(float sizePt, FontStyle style = FontStyle.Regular)
        {
            return new Font(UiFontFamily, sizePt, style, GraphicsUnit.Point);
        }

        public static Color BlendColor(Color from, Color to, float progress)
        {
            if (progress <= 0f) return from;
            if (progress >= 1f) return to;
            int a = (int)Math.Round(from.A + (to.A - from.A) * progress);
            int r = (int)Math.Round(from.R + (to.R - from.R) * progress);
            int g = (int)Math.Round(from.G + (to.G - from.G) * progress);
            int b = (int)Math.Round(from.B + (to.B - from.B) * progress);
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, a)),
                Math.Max(0, Math.Min(255, r)),
                Math.Max(0, Math.Min(255, g)),
                Math.Max(0, Math.Min(255, b)));
        }

        public static Color Darken(Color c, float amount)
        {
            return Color.FromArgb(
                c.A,
                Math.Max(0, (int)Math.Round(c.R * (1f - amount))),
                Math.Max(0, (int)Math.Round(c.G * (1f - amount))),
                Math.Max(0, (int)Math.Round(c.B * (1f - amount))));
        }

        public static Color ApplyAlpha(Color c, float opacity)
        {
            int a = Math.Max(0, Math.Min(255, (int)Math.Round(c.A * opacity)));
            return Color.FromArgb(a, c.R, c.G, c.B);
        }

        public static readonly ThemePalette OrangePalette = new ThemePalette
        {
            IsPink = false,
            Primary = Color.FromArgb(255, 80, 0),              // 品牌活力橙 #FF5000
            PrimaryHover = Color.FromArgb(255, 102, 26),       // #FF661A
            PrimaryPressed = Color.FromArgb(224, 68, 0),       // #E04400
            DangerRed = Color.FromArgb(255, 59, 48),           // #FF3B30
            SwitchOnGreen = Color.FromArgb(52, 199, 89),       // #34C759
            FocusRing = Color.FromArgb(90, 255, 80, 0),        // 0 0 0 3px rgba(accent, 0.35)

            LightPaperBg = Color.FromArgb(251, 250, 248),      // 温润质感白 #FBFAF8
            LightSidebarBg = Color.FromArgb(245, 245, 247),    // macOS 侧边栏灰 #F5F5F7
            LightCardBg = Color.FromArgb(255, 255, 255),       // 纯白表面 #FFFFFF
            LightTextPrimary = Color.FromArgb(29, 29, 31),     // Apple 炭墨 #1D1D1F
            LightTextSecondary = Color.FromArgb(110, 110, 115),// #6E6E73
            LightTextMuted = Color.FromArgb(142, 142, 147),    // #8E8E93
            LightChipText = Color.FromArgb(66, 66, 69),        // #424245
            LightDivider = Color.FromArgb(232, 229, 224),
            LightBorder = Color.FromArgb(226, 222, 216),
            LightRowHover = Color.FromArgb(255, 243, 236),     // 浅橙微晕
            LightRowSelected = Color.FromArgb(255, 234, 222),
            LightSearchBg = Color.FromArgb(242, 240, 237),     // 凹槽搜索底
            LightScrollThumb = Color.FromArgb(145, 0, 0, 0),

            DarkPaperBg = Color.FromArgb(24, 24, 26),          // 深邃暗黑底 #18181A
            DarkSidebarBg = Color.FromArgb(30, 30, 30),        // macOS 暗黑侧栏 #1E1E1E
            DarkCardBg = Color.FromArgb(36, 36, 39),           // #242427
            DarkSecondaryBtnBg = Color.FromArgb(44, 44, 46),   // Apple #2C2C2E
            DarkElevatedBg = Color.FromArgb(54, 54, 58),       // 浮层提升表面 #36363A
            DarkTextPrimary = Color.FromArgb(245, 245, 247),   // #F5F5F7
            DarkTextSecondary = Color.FromArgb(174, 174, 178), // #AEAEB2
            DarkTextMuted = Color.FromArgb(142, 142, 147),     // #8E8E93
            DarkDivider = Color.FromArgb(56, 56, 58),          // #38383A
            DarkPrimaryAccent = Color.FromArgb(255, 102, 26),
            DarkRowHover = Color.FromArgb(48, 36, 30),
            DarkRowSelected = Color.FromArgb(64, 44, 34),
            DarkScrollThumb = Color.FromArgb(140, 255, 255, 255)
        };

        public static readonly ThemePalette PinkPalette = new ThemePalette
        {
            IsPink = true,
            Primary = Color.FromArgb(250, 114, 152),           // 樱粉强调色 #FA7298
            PrimaryHover = Color.FromArgb(252, 134, 166),      // #FC86A6
            PrimaryPressed = Color.FromArgb(228, 90, 128),     // #E45A80
            DangerRed = Color.FromArgb(255, 59, 48),           // #FF3B30
            SwitchOnGreen = Color.FromArgb(52, 199, 89),       // #34C759
            FocusRing = Color.FromArgb(90, 250, 114, 152),     // 0 0 0 3px rgba(pink, 0.35)

            LightPaperBg = Color.FromArgb(253, 249, 250),      // 樱粉纸白 #FDF9FA
            LightSidebarBg = Color.FromArgb(245, 243, 245),    // 微粉柔灰
            LightCardBg = Color.FromArgb(255, 255, 255),
            LightTextPrimary = Color.FromArgb(34, 30, 32),
            LightTextSecondary = Color.FromArgb(128, 118, 122),
            LightTextMuted = Color.FromArgb(158, 148, 152),
            LightChipText = Color.FromArgb(72, 66, 68),
            LightDivider = Color.FromArgb(240, 228, 232),
            LightBorder = Color.FromArgb(234, 218, 224),
            LightRowHover = Color.FromArgb(253, 239, 244),
            LightRowSelected = Color.FromArgb(251, 226, 234),
            LightSearchBg = Color.FromArgb(246, 240, 242),
            LightScrollThumb = Color.FromArgb(145, 0, 0, 0),

            DarkPaperBg = Color.FromArgb(28, 23, 26),          // 夜樱深梅 #1C171A
            DarkSidebarBg = Color.FromArgb(32, 26, 29),        // 深梅侧边栏
            DarkCardBg = Color.FromArgb(40, 33, 37),
            DarkSecondaryBtnBg = Color.FromArgb(48, 42, 46),
            DarkElevatedBg = Color.FromArgb(60, 50, 56),
            DarkTextPrimary = Color.FromArgb(252, 242, 246),
            DarkTextSecondary = Color.FromArgb(184, 166, 174),
            DarkTextMuted = Color.FromArgb(142, 126, 134),
            DarkDivider = Color.FromArgb(64, 52, 58),
            DarkPrimaryAccent = Color.FromArgb(252, 134, 166),
            DarkRowHover = Color.FromArgb(56, 38, 47),
            DarkRowSelected = Color.FromArgb(72, 45, 58),
            DarkScrollThumb = Color.FromArgb(140, 255, 255, 255)
        };

        public static ThemePalette Current = OrangePalette;

        public static void SetScheme(string scheme)
        {
            Current = (scheme == "pink") ? PinkPalette : OrangePalette;
        }

        // 兼容静态访问属性（动态指向 Current）
        public static Color Orange { get { return Current.Primary; } }
        public static Color OrangeHover { get { return Current.PrimaryHover; } }
        public static Color OrangePressed { get { return Current.PrimaryPressed; } }
        public static Color DangerRed { get { return Current.DangerRed; } }
        public static Color FocusRing { get { return Current.FocusRing; } }

        public static Color LightPaperBg { get { return Current.LightPaperBg; } }
        public static Color LightSidebarBg { get { return Current.LightSidebarBg; } }
        public static Color LightCardBg { get { return Current.LightCardBg; } }
        public static Color LightTextPrimary { get { return Current.LightTextPrimary; } }
        public static Color LightTextSecondary { get { return Current.LightTextSecondary; } }
        public static Color LightTextMuted { get { return Current.LightTextMuted; } }
        public static Color LightChipText { get { return Current.LightChipText; } }
        public static Color LightDivider { get { return Current.LightDivider; } }
        public static Color LightBorder { get { return Current.LightBorder; } }
        public static Color LightRowHover { get { return Current.LightRowHover; } }
        public static Color LightRowSelected { get { return Current.LightRowSelected; } }
        public static Color LightSearchBg { get { return Current.LightSearchBg; } }
        public static Color LightScrollThumb { get { return Current.LightScrollThumb; } }

        public static Color DarkPaperBg { get { return Current.DarkPaperBg; } }
        public static Color DarkSidebarBg { get { return Current.DarkSidebarBg; } }
        public static Color DarkCardBg { get { return Current.DarkCardBg; } }
        public static Color DarkSecondaryBtnBg { get { return Current.DarkSecondaryBtnBg; } }
        public static Color DarkElevatedBg { get { return Current.DarkElevatedBg; } }
        public static Color DarkTextPrimary { get { return Current.DarkTextPrimary; } }
        public static Color DarkTextSecondary { get { return Current.DarkTextSecondary; } }
        public static Color DarkTextMuted { get { return Current.DarkTextMuted; } }
        public static Color DarkDivider { get { return Current.DarkDivider; } }
        public static Color DarkPrimaryAccent { get { return Current.DarkPrimaryAccent; } }
        public static Color DarkRowHover { get { return Current.DarkRowHover; } }
        public static Color DarkRowSelected { get { return Current.DarkRowSelected; } }
        public static Color DarkScrollThumb { get { return Current.DarkScrollThumb; } }
    }

    /// <summary>
    /// Apple 桌面交互曲线与轻量补间计算（cubic-bezier(0.2, 0.8, 0.2, 1)）
    /// </summary>
    public static class MotionHelper
    {
        public static float EaseApple(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            // 对 cubic-bezier(0.2, 0.8, 0.2, 1) 执行快速牛顿迭代求解 x(u)=t -> y(u)
            double u = t;
            for (int i = 0; i < 4; i++)
            {
                double x = 3.0 * u * (1.0 - u) * (1.0 - u) * 0.2 + 3.0 * u * u * (1.0 - u) * 0.2 + u * u * u - t;
                double dx = 3.0 * (1.0 - u) * (1.0 - u) * 0.2 + 6.0 * (1.0 - u) * u * (0.2 - 0.2) + 3.0 * u * u * (1.0 - 0.2);
                if (Math.Abs(dx) < 1e-5) break;
                u -= x / dx;
                if (u < 0.0) u = 0.0;
                if (u > 1.0) u = 1.0;
            }
            double y = 3.0 * u * (1.0 - u) * (1.0 - u) * 0.8 + 3.0 * u * u * (1.0 - u) * 1.0 + u * u * u;
            return (float)Math.Max(0.0, Math.Min(1.0, y));
        }
    }

    /// <summary>
    /// Windows 10/11 DWM 原生圆角、阴影及深浅模式修饰工具
    /// </summary>
    public static class Win32WindowStyler
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWCP_ROUND = 2;

        public static void ApplyModernWindowStyle(Form form, bool isDark)
        {
            if (form == null || !form.IsHandleCreated) return;
            try
            {
                int darkVal = isDark ? 1 : 0;
                DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkVal, sizeof(int));

                // 启用 Windows 11 原生 8px 系统窗口圆角
                int cornerVal = DWMWCP_ROUND;
                DwmSetWindowAttribute(form.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerVal, sizeof(int));

                // 设置 1px 细腻发丝外框色（Win11）
                Color bdr = isDark ? Color.FromArgb(62, 62, 66) : Color.FromArgb(215, 212, 206);
                int colorRef = bdr.R | (bdr.G << 8) | (bdr.B << 16);
                DwmSetWindowAttribute(form.Handle, DWMWA_BORDER_COLOR, ref colorRef, sizeof(int));

                // 触发 DWM 非客户区柔和立体投影
                MARGINS m = new MARGINS { cxLeftWidth = 1, cxRightWidth = 1, cyTopHeight = 1, cyBottomHeight = 1 };
                DwmExtendFrameIntoClientArea(form.Handle, ref m);
            }
            catch { }
        }
    }

    /// <summary>
    /// iOS 26 Liquid Glass 分层参数（浅色 / 深色两套）
    /// </summary>
    public class LiquidGlassParams
    {
        public Color BlurLayer1;     // 分层模糊模拟底色 1
        public Color BlurLayer2;     // 分层模糊模拟底色 2
        public Color GlassFill;      // 玻璃半透明主体 (白色 12%~28%)
        public Color GlassHoverFill; // 悬停增强填充
        public Color InnerRefract;   // 边缘内侧折射加亮
        public Color TopHighlight;   // 顶部 1px 高光描边
        public Color InnerShadow;    // 内阴影
        public Color OuterShadow;    // 外层柔和投影
        public Color OuterBorder;    // 外轮廓边

        public static readonly LiquidGlassParams Light = new LiquidGlassParams
        {
            BlurLayer1 = Color.FromArgb(24, 255, 255, 255),      // ~9% 底层柔光
            BlurLayer2 = Color.FromArgb(46, 255, 255, 255),      // ~18% 中层磨砂
            GlassFill = Color.FromArgb(56, 255, 255, 255),       // ~22% 白色玻璃体 (12%~28%)
            GlassHoverFill = Color.FromArgb(72, 255, 255, 255),  // ~28% 悬停态
            InnerRefract = Color.FromArgb(145, 255, 255, 255),   // 内侧加亮模拟折射
            TopHighlight = Color.FromArgb(230, 255, 255, 255),   // 顶部 1px 高光描边
            InnerShadow = Color.FromArgb(18, 0, 0, 0),           // 底部微弱内阴影
            OuterShadow = Color.FromArgb(22, 0, 0, 0),           // 外层柔和投影
            OuterBorder = Color.FromArgb(55, 0, 0, 0)            // 浅色微边框
        };

        public static readonly LiquidGlassParams Dark = new LiquidGlassParams
        {
            BlurLayer1 = Color.FromArgb(12, 255, 255, 255),      // ~5% 深色柔和底层
            BlurLayer2 = Color.FromArgb(20, 255, 255, 255),      // ~8% 深色玻璃层
            GlassFill = Color.FromArgb(24, 255, 255, 255),       // ~9.5% 深色通透玻璃
            GlassHoverFill = Color.FromArgb(42, 255, 255, 255),  // ~16.5% 悬停态
            InnerRefract = Color.FromArgb(65, 255, 255, 255),    // 柔和内侧折射
            TopHighlight = Color.FromArgb(120, 255, 255, 255),   // 顶部 1px 细腻高光
            InnerShadow = Color.FromArgb(30, 0, 0, 0),           // 柔和内阴影
            OuterShadow = Color.FromArgb(20, 0, 0, 0),           // 极轻外投影，消除图标旁黑色晕圈
            OuterBorder = Color.FromArgb(55, 255, 255, 255)      // 柔和细边框
        };
    }

    /// <summary>
    /// macOS 标志性按钮层级规范（Primary / Secondary / Ghost / Destructive）
    /// </summary>
    public enum ButtonVariant
    {
        Primary,
        Secondary,
        Ghost,
        Destructive
    }

    /// <summary>
    /// Apple Desktop HIG + GDI+ 现代矢量与质感渲染引擎（无第三方依赖）
    /// </summary>
    public static class GlassRenderer
    {
        public static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int d = Math.Min(radius * 2, Math.Min(rect.Width, rect.Height));
            if (d <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            int r = rect.Right;
            int b = rect.Bottom;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(r - d, rect.Y, d, d, 270, 90);
            path.AddArc(r - d, b - d, d, d, 0, 90);
            path.AddArc(rect.X, b - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// 顶栏面板绘制：通透底色 + 顶部 1px 细腻内高光
        /// </summary>
        public static void DrawLiquidGlassPanel(Graphics g, Rectangle rect, bool isDark, int radius = 0)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            LiquidGlassParams p = isDark ? LiquidGlassParams.Dark : LiquidGlassParams.Light;

            using (GraphicsPath path = CreateRoundedPath(rect, radius))
            {
                using (Brush b1 = new SolidBrush(p.BlurLayer1)) g.FillPath(b1, path);
                Rectangle innerBlur = new Rectangle(rect.X + 1, rect.Y + 1, Math.Max(1, rect.Width - 2), Math.Max(1, rect.Height - 2));
                using (GraphicsPath innerPath = CreateRoundedPath(innerBlur, Math.Max(0, radius - 1)))
                using (Brush b2 = new SolidBrush(p.BlurLayer2))
                {
                    g.FillPath(b2, innerPath);
                }
                using (Brush bGlass = new SolidBrush(p.GlassFill)) g.FillPath(bGlass, path);

                // 顶部 1px 细腻高光描边
                using (Pen topPen = new Pen(isDark ? WarmTheme.TopHighlightDark : p.TopHighlight, 1))
                {
                    g.DrawLine(topPen, rect.X + radius, rect.Y, rect.Right - radius, rect.Y);
                }
            }
        }

        /// <summary>
        /// 通用 macOS × iOS 现代按钮表面绘制（支持 4 层级、120ms 悬停渐变、顶部内高光、3px 键盘焦点环、按压下沉与禁用 0.4 透明度）
        /// </summary>
        public static void DrawModernButtonSurface(
            Graphics g,
            Rectangle bounds,
            ButtonVariant variant,
            Color accentColor,
            bool isDark,
            float hoverProgress,
            bool isPressed,
            bool showFocusRing,
            bool isEnabled,
            int radius,
            Color? customHoverBg = null)
        {
            if (bounds.Width <= 4 || bounds.Height <= 4) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float opacity = isEnabled ? 1.0f : 0.4f;
            float hp = isEnabled ? Math.Max(0f, Math.Min(1f, hoverProgress)) : 0f;

            // 按压态克制微下沉（模拟桌面端 transform: scale(0.98) 实体按压手感）
            Rectangle drawRect = isPressed && isEnabled
                ? new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2)
                : bounds;

            // 1. Focus-Visible 外发光焦点环（仅键盘 Tab 导航时显示 0 0 0 3px rgba(accent, 0.35)）
            if (showFocusRing && isEnabled)
            {
                Color ringCol = variant == ButtonVariant.Destructive
                    ? WarmTheme.ApplyAlpha(WarmTheme.DestructiveRed, 0.35f)
                    : WarmTheme.ApplyAlpha(accentColor, 0.35f);
                Rectangle ringRect = new Rectangle(drawRect.X - 2, drawRect.Y - 2, drawRect.Width + 4, drawRect.Height + 4);
                using (GraphicsPath fPath = CreateRoundedPath(ringRect, radius + 2))
                using (Pen fPen = new Pen(ringCol, 2.5f))
                {
                    g.DrawPath(fPen, fPath);
                }
            }

            // 2. 极轻外层投影（仅 Primary 与 Secondary 在未按下态保留）
            if (!isPressed && isEnabled && (variant == ButtonVariant.Primary || variant == ButtonVariant.Secondary))
            {
                int shadowAlpha = variant == ButtonVariant.Primary ? 34 : (isDark ? 24 : 14);
                Rectangle shRect = new Rectangle(drawRect.X, drawRect.Y + 1, drawRect.Width, drawRect.Height);
                using (GraphicsPath sPath = CreateRoundedPath(shRect, radius))
                using (Brush sBrush = new SolidBrush(Color.FromArgb(shadowAlpha, 0, 0, 0)))
                {
                    g.FillPath(sBrush, sPath);
                }
            }

            using (GraphicsPath path = CreateRoundedPath(drawRect, radius))
            {
                if (variant == ButtonVariant.Primary)
                {
                    // Primary：强调色填充 + 顶部 1px 内高光 (inset 0 1px 0 rgba(255,255,255,0.2))
                    Color baseCol = accentColor;
                    Color hoverCol = WarmTheme.BlendColor(accentColor, Color.White, isDark ? 0.14f : 0.10f);
                    Color fill = isPressed ? WarmTheme.Darken(accentColor, 0.10f) : WarmTheme.BlendColor(baseCol, hoverCol, hp);
                    fill = WarmTheme.ApplyAlpha(fill, opacity);

                    using (Brush b = new SolidBrush(fill)) g.FillPath(b, path);

                    // 顶部 1px 内高光弧线
                    g.SetClip(path);
                    using (Pen hi = new Pen(WarmTheme.ApplyAlpha(WarmTheme.TopHighlightPrimary, opacity), 1))
                    {
                        g.DrawLine(hi, drawRect.X + 2, drawRect.Y + 1, drawRect.Right - 2, drawRect.Y + 1);
                    }
                    g.ResetClip();

                    Color bdr = WarmTheme.ApplyAlpha(WarmTheme.Darken(accentColor, 0.14f), opacity * 0.65f);
                    using (Pen pen = new Pen(bdr, 1)) g.DrawPath(pen, path);
                }
                else if (variant == ButtonVariant.Secondary)
                {
                    // Secondary：浅色纯白 + 1px rgba(0,0,0,0.10) 描边；暗黑 #2C2C2E + 顶部 rgba(255,255,255,0.12) 微亮高光
                    Color normalBg = isDark ? WarmTheme.DarkSecondaryBtnBg : WarmTheme.LightCardBg;
                    Color hoverBg = isDark ? Color.FromArgb(58, 58, 62) : Color.FromArgb(248, 248, 250);
                    Color pressBg = isDark ? Color.FromArgb(40, 40, 42) : Color.FromArgb(240, 240, 243);
                    Color fill = isPressed ? pressBg : WarmTheme.BlendColor(normalBg, hoverBg, hp);
                    fill = WarmTheme.ApplyAlpha(fill, opacity);

                    using (Brush b = new SolidBrush(fill)) g.FillPath(b, path);

                    if (isDark)
                    {
                        g.SetClip(path);
                        using (Pen hi = new Pen(WarmTheme.ApplyAlpha(WarmTheme.TopHighlightDark, opacity), 1))
                        {
                            g.DrawLine(hi, drawRect.X + 2, drawRect.Y + 1, drawRect.Right - 2, drawRect.Y + 1);
                        }
                        g.ResetClip();
                    }

                    Color bdrCol = isDark ? WarmTheme.HairlineDark : WarmTheme.BorderSecondaryLight;
                    using (Pen pen = new Pen(WarmTheme.ApplyAlpha(bdrCol, opacity), 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
                else if (variant == ButtonVariant.Ghost)
                {
                    // Ghost：默认透明无底，悬停浮现圆角半透明底色 rgba(0,0,0,0.06) / 暗黑 rgba(255,255,255,0.08)
                    Color hoverBg = customHoverBg.HasValue
                        ? customHoverBg.Value
                        : (isDark ? WarmTheme.GhostHoverDark : WarmTheme.GhostHoverLight);
                    Color pressBg = isDark ? Color.FromArgb(34, 255, 255, 255) : Color.FromArgb(26, 0, 0, 0);
                    Color fill = isPressed ? pressBg : WarmTheme.BlendColor(Color.FromArgb(0, hoverBg), hoverBg, hp);
                    fill = WarmTheme.ApplyAlpha(fill, opacity);
                    if (fill.A > 0)
                    {
                        using (Brush b = new SolidBrush(fill)) g.FillPath(b, path);
                    }
                }
                else // Destructive
                {
                    // Destructive：警示红 #FF3B30，悬停时浅红底色着色（支持定制关闭/删除态）
                    Color hoverBg = customHoverBg.HasValue
                        ? customHoverBg.Value
                        : Color.FromArgb(isDark ? 48 : 26, WarmTheme.DestructiveRed);
                    Color pressBg = Color.FromArgb(isDark ? 72 : 42, WarmTheme.DestructiveRed);
                    Color normalBg = isDark ? WarmTheme.DarkSecondaryBtnBg : WarmTheme.LightCardBg;
                    Color fill = isPressed ? pressBg : WarmTheme.BlendColor(normalBg, hoverBg, hp);
                    fill = WarmTheme.ApplyAlpha(fill, opacity);

                    using (Brush b = new SolidBrush(fill)) g.FillPath(b, path);

                    Color normalBdr = isDark ? WarmTheme.HairlineDark : WarmTheme.BorderSecondaryLight;
                    Color hoverBdr = Color.FromArgb(isDark ? 110 : 85, WarmTheme.DestructiveRed);
                    Color bdrCol = WarmTheme.ApplyAlpha(WarmTheme.BlendColor(normalBdr, hoverBdr, hp), opacity);
                    using (Pen pen = new Pen(bdrCol, 1)) g.DrawPath(pen, path);
                }
            }
        }

        /// <summary>
        /// 兼容行内主按钮 / 液态玻璃快捷绘制
        /// </summary>
        public static void DrawLiquidGlassButton(Graphics g, Rectangle rect, Color accentColor, bool isPrimary, bool isDark, bool isHover, bool isPressed = false, int radius = 7)
        {
            DrawModernButtonSurface(
                g, rect,
                isPrimary ? ButtonVariant.Primary : ButtonVariant.Secondary,
                accentColor, isDark, isHover ? 1.0f : 0.0f, isPressed, false, true, radius);
        }

        /// <summary>
        /// 扁平实色/次级胶囊绘制（供分类芯片等使用）
        /// </summary>
        public static void DrawLiquidGlassButton(Graphics g, Rectangle rect, Color fillNormal, Color fillHover, Color borderCol, bool isHover, int radius = 7, bool isPressed = false, Color? fillPressed = null)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color bg = fillNormal;
            if (isPressed && fillPressed.HasValue) bg = fillPressed.Value;
            else if (isHover) bg = fillHover;

            using (GraphicsPath path = CreateRoundedPath(rect, radius))
            {
                if (bg.A > 0)
                {
                    using (Brush b = new SolidBrush(bg)) g.FillPath(b, path);
                }
                if (borderCol.A > 0)
                {
                    using (Pen pen = new Pen(borderCol, 1)) g.DrawPath(pen, path);
                }
            }
        }

        private static Pen CreateIconPen(Color color, float width = 1.6f)
        {
            Pen p = new Pen(color, width);
            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.Round;
            p.LineJoin = LineJoin.Round;
            return p;
        }

        /// <summary>
        /// 16x16 线性齿轮图标（SF Symbols 风格：1.6px 统一线宽）
        /// </summary>
        public static void DrawVectorGearIcon(Graphics g, Rectangle rect, Color gearColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            float rOuter = 6.2f;
            float rInner = 4.6f;
            float rHole = 2.2f;

            using (Pen p = CreateIconPen(gearColor, 1.55f))
            {
                g.DrawEllipse(p, cx - rHole, cy - rHole, rHole * 2, rHole * 2);
                g.DrawEllipse(p, cx - rInner, cy - rInner, rInner * 2, rInner * 2);
                for (int i = 0; i < 8; i++)
                {
                    double ang = i * (Math.PI / 4.0);
                    float x1 = cx + (float)(Math.Cos(ang) * (rInner - 0.1f));
                    float y1 = cy + (float)(Math.Sin(ang) * (rInner - 0.1f));
                    float x2 = cx + (float)(Math.Cos(ang) * rOuter);
                    float y2 = cy + (float)(Math.Sin(ang) * rOuter);
                    g.DrawLine(p, x1, y1, x2, y2);
                }
            }
        }

        /// <summary>
        /// 16x16 细线放大镜图标
        /// </summary>
        public static void DrawVectorSearchIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = rect.X + rect.Width / 2f - 1.5f;
            float cy = rect.Y + rect.Height / 2f - 1.5f;
            float r = 4.2f;
            using (Pen p = CreateIconPen(color))
            {
                g.DrawEllipse(p, cx - r, cy - r, r * 2, r * 2);
                g.DrawLine(p, cx + r * 0.7f, cy + r * 0.7f, cx + r + 3.5f, cy + r + 3.5f);
            }
        }

        /// <summary>
        /// 16x16 细线图片图标（山 + 太阳）
        /// </summary>
        public static void DrawVectorImageIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float x = rect.X + (rect.Width - 12f) / 2f;
            float y = rect.Y + (rect.Height - 10f) / 2f;
            using (Pen p = CreateIconPen(color))
            {
                RectangleF box = new RectangleF(x, y, 12f, 10f);
                using (GraphicsPath path = CreateRoundedPath(Rectangle.Round(box), 2))
                {
                    g.DrawPath(p, path);
                }
                g.DrawEllipse(p, x + 2.5f, y + 2.2f, 2f, 2f);
                g.DrawLines(p, new PointF[]
                {
                    new PointF(x + 1.5f, y + 8.5f),
                    new PointF(x + 4.8f, y + 5.5f),
                    new PointF(x + 7.2f, y + 7.5f),
                    new PointF(x + 9.2f, y + 5.0f),
                    new PointF(x + 11.0f, y + 8.5f)
                });
            }
        }

        /// <summary>
        /// 16x16 细线剪贴板图标
        /// </summary>
        public static void DrawVectorClipboardIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float x = rect.X + (rect.Width - 10f) / 2f;
            float y = rect.Y + (rect.Height - 12f) / 2f + 1f;
            using (Pen p = CreateIconPen(color))
            {
                g.DrawRectangle(p, x, y + 1.5f, 10f, 10.5f);
                g.DrawRectangle(p, x + 2.5f, y, 5f, 2.5f);
            }
        }

        /// <summary>
        /// 16x16 细线备份/下载盒子图标
        /// </summary>
        public static void DrawVectorBackupIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            using (Pen p = CreateIconPen(color))
            {
                g.DrawLine(p, cx, cy - 5f, cx, cy + 2f);
                g.DrawLines(p, new PointF[]
                {
                    new PointF(cx - 2.8f, cy - 0.5f),
                    new PointF(cx, cy + 2.2f),
                    new PointF(cx + 2.8f, cy - 0.5f)
                });
                g.DrawLines(p, new PointF[]
                {
                    new PointF(cx - 5f, cy + 2.5f),
                    new PointF(cx - 5f, cy + 5f),
                    new PointF(cx + 5f, cy + 5f),
                    new PointF(cx + 5f, cy + 2.5f)
                });
            }
        }

        /// <summary>
        /// 16x16 细线加号图标
        /// </summary>
        public static void DrawVectorPlusIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            using (Pen p = CreateIconPen(color))
            {
                g.DrawLine(p, cx - 4f, cy, cx + 4f, cy);
                g.DrawLine(p, cx, cy - 4f, cx, cy + 4f);
            }
        }

        /// <summary>
        /// 16x16 细线减号图标
        /// </summary>
        public static void DrawVectorMinusIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            using (Pen p = CreateIconPen(color))
            {
                g.DrawLine(p, cx - 4f, cy, cx + 4f, cy);
            }
        }

        /// <summary>
        /// 16x16 细线铅笔编辑图标
        /// </summary>
        public static void DrawVectorEditIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            using (Pen p = CreateIconPen(color))
            {
                g.DrawLine(p, cx - 3.5f, cy + 3.5f, cx + 3.5f, cy - 3.5f);
                g.DrawLine(p, cx - 3.5f, cy + 3.5f, cx - 4.2f, cy + 4.2f);
                g.DrawLine(p, cx + 1.5f, cy - 3.8f, cx + 3.8f, cy - 1.5f);
            }
        }

        /// <summary>
        /// 16x16 细线文本/文档图标
        /// </summary>
        public static void DrawVectorDocIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float x = rect.X + (rect.Width - 10f) / 2f;
            float y = rect.Y + (rect.Height - 12f) / 2f;
            using (Pen p = CreateIconPen(color))
            {
                RectangleF box = new RectangleF(x, y, 10f, 12f);
                using (GraphicsPath path = CreateRoundedPath(Rectangle.Round(box), 2))
                {
                    g.DrawPath(p, path);
                }
                g.DrawLine(p, x + 2.5f, y + 4f, x + 7.5f, y + 4f);
                g.DrawLine(p, x + 2.5f, y + 6.5f, x + 7.5f, y + 6.5f);
                g.DrawLine(p, x + 2.5f, y + 9f, x + 5.8f, y + 9f);
            }
        }

        /// <summary>
        /// 16x16 细线视频播放框图标
        /// </summary>
        public static void DrawVectorVideoIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float x = rect.X + (rect.Width - 12f) / 2f;
            float y = rect.Y + (rect.Height - 10f) / 2f;
            using (Pen p = CreateIconPen(color))
            {
                RectangleF box = new RectangleF(x, y, 12f, 10f);
                using (GraphicsPath path = CreateRoundedPath(Rectangle.Round(box), 2))
                {
                    g.DrawPath(p, path);
                }
                float cx = x + 6f;
                float cy = y + 5f;
                g.DrawPolygon(p, new PointF[]
                {
                    new PointF(cx - 1.5f, cy - 2.3f),
                    new PointF(cx + 2.5f, cy),
                    new PointF(cx - 1.5f, cy + 2.3f)
                });
            }
        }

        /// <summary>
        /// 14x14 细线侧边栏折叠切换图标
        /// </summary>
        public static void DrawVectorSidebarIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float x = rect.X + (rect.Width - 13f) / 2f;
            float y = rect.Y + (rect.Height - 11f) / 2f;
            using (Pen p = CreateIconPen(color, 1.5f))
            {
                Rectangle r = Rectangle.Round(new RectangleF(x, y, 13f, 11f));
                using (GraphicsPath path = CreateRoundedPath(r, 2)) g.DrawPath(p, path);
                g.DrawLine(p, x + 4.5f, y + 0.5f, x + 4.5f, y + 10.5f);
            }
        }

        /// <summary>
        /// 14x14 细线图钉图标（置顶状态）
        /// </summary>
        public static void DrawVectorPinIcon(Graphics g, Rectangle rect, Color color, bool active)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            using (Pen p = CreateIconPen(color, 1.55f))
            {
                // 斜45度图钉头与底座
                PointF[] head = new PointF[]
                {
                    new PointF(cx - 2.2f, cy - 4.2f),
                    new PointF(cx + 3.8f, cy - 4.2f),
                    new PointF(cx + 2.8f, cy - 1.5f),
                    new PointF(cx + 4.5f, cy + 0.8f),
                    new PointF(cx - 3.0f, cy + 0.8f),
                    new PointF(cx - 1.3f, cy - 1.5f)
                };
                if (active)
                {
                    using (Brush b = new SolidBrush(color)) g.FillPolygon(b, head);
                }
                else
                {
                    g.DrawPolygon(p, head);
                }
                g.DrawLine(p, cx + 0.8f, cy + 0.8f, cx + 0.8f, cy + 5.2f);
            }
        }

        /// <summary>
        /// 14x14 下拉小箭头图标
        /// </summary>
        public static void DrawVectorChevronDownIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            using (Pen p = CreateIconPen(color, 1.6f))
            {
                g.DrawLines(p, new PointF[]
                {
                    new PointF(cx - 3.5f, cy - 1.5f),
                    new PointF(cx, cy + 2.0f),
                    new PointF(cx + 3.5f, cy - 1.5f)
                });
            }
        }

        /// <summary>
        /// 14x14 垃圾桶删除图标
        /// </summary>
        public static void DrawVectorTrashIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            using (Pen p = CreateIconPen(color, 1.5f))
            {
                g.DrawLine(p, cx - 5f, cy - 3.2f, cx + 5f, cy - 3.2f);
                g.DrawLine(p, cx - 2f, cy - 5.2f, cx + 2f, cy - 5.2f);
                g.DrawLines(p, new PointF[]
                {
                    new PointF(cx - 3.8f, cy - 3.2f),
                    new PointF(cx - 3.2f, cy + 5.0f),
                    new PointF(cx + 3.2f, cy + 5.0f),
                    new PointF(cx + 3.8f, cy - 3.2f)
                });
            }
        }

        /// <summary>
        /// Windows 右上角窗口控制三键（最小化 / 最大化 / 还原 / 关闭）
        /// </summary>
        public static void DrawVectorWinMinimizeIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            using (Pen p = CreateIconPen(color, 1.5f))
            {
                g.DrawLine(p, cx - 4.5f, cy, cx + 4.5f, cy);
            }
        }

        public static void DrawVectorWinMaximizeIcon(Graphics g, Rectangle rect, Color color, bool isMaximized)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            using (Pen p = CreateIconPen(color, 1.45f))
            {
                if (isMaximized)
                {
                    g.DrawRectangle(p, cx - 4.5f, cy - 2.2f, 6.8f, 6.8f);
                    g.DrawLines(p, new PointF[]
                    {
                        new PointF(cx - 2.2f, cy - 2.2f),
                        new PointF(cx - 2.2f, cy - 4.4f),
                        new PointF(cx + 4.5f, cy - 4.4f),
                        new PointF(cx + 4.5f, cy + 2.3f),
                        new PointF(cx + 2.3f, cy + 2.3f)
                    });
                }
                else
                {
                    g.DrawRectangle(p, cx - 4.2f, cy - 4.2f, 8.4f, 8.4f);
                }
            }
        }

        public static void DrawVectorWinCloseIcon(Graphics g, Rectangle rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            using (Pen p = CreateIconPen(color, 1.55f))
            {
                g.DrawLine(p, cx - 4f, cy - 4f, cx + 4f, cy + 4f);
                g.DrawLine(p, cx + 4f, cy - 4f, cx - 4f, cy + 4f);
            }
        }

        /// <summary>
        /// 紧凑型 14px 加载旋转指示器
        /// </summary>
        public static void DrawVectorSpinner(Graphics g, Rectangle rect, Color color, float startAngle)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float size = 13f;
            float x = rect.X + (rect.Width - size) / 2f;
            float y = rect.Y + (rect.Height - size) / 2f;
            using (Pen trackPen = new Pen(WarmTheme.ApplyAlpha(color, 0.25f), 1.8f))
            {
                g.DrawEllipse(trackPen, x, y, size, size);
            }
            using (Pen arcPen = CreateIconPen(color, 1.8f))
            {
                g.DrawArc(arcPen, x, y, size, size, startAngle, 110f);
            }
        }
    }

    /// <summary>
    /// 文本 / 视频 模式切换栏（内嵌 Apple 分段控制器 SegmentedControl + 右侧计数）
    /// </summary>
    public class ModeSwitchBar : Control
    {
        private SegmentedControl _seg;
        private int _itemCount = 0;
        private bool _isDark = false;

        public string ActiveMode
        {
            get { return _seg != null && _seg.SelectedIndex == 1 ? "video" : "text"; }
            set
            {
                if (_seg != null) _seg.SelectedIndex = (value == "video") ? 1 : 0;
                this.Invalidate();
            }
        }

        public int ItemCount
        {
            get { return _itemCount; }
            set { _itemCount = value; this.Invalidate(); }
        }

        public bool IsDarkTheme
        {
            get { return _isDark; }
            set
            {
                _isDark = value;
                if (_seg != null) { _seg.IsDarkTheme = value; _seg.Invalidate(); }
                this.Invalidate();
            }
        }

        public event Action<string> OnModeChanged;

        public ModeSwitchBar()
        {
            this.Height = 36;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);

            _seg = new SegmentedControl
            {
                Location = new Point(10, 4),
                Size = new Size(146, 28),
                Items = new string[] { "文本", "视频" },
                IconPainters = new Action<Graphics, Rectangle, Color>[]
                {
                    GlassRenderer.DrawVectorDocIcon,
                    GlassRenderer.DrawVectorVideoIcon
                }
            };
            _seg.SelectedIndexChanged += (s, e) =>
            {
                string m = (_seg.SelectedIndex == 1) ? "video" : "text";
                if (OnModeChanged != null) OnModeChanged(m);
            };
            this.Controls.Add(_seg);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color barBg = IsDarkTheme ? WarmTheme.DarkCardBg : WarmTheme.LightCardBg;
            Color lineCol = IsDarkTheme ? WarmTheme.HairlineDark : WarmTheme.HairlineLight;
            g.Clear(barBg);

            using (Pen linePen = new Pen(lineCol, 1))
            {
                g.DrawLine(linePen, 0, this.Height - 1, this.Width, this.Height - 1);
            }

            Color cntCol = IsDarkTheme ? WarmTheme.DarkTextMuted : WarmTheme.LightTextMuted;
            using (Font fCount = WarmTheme.CreateFont(8.5F, FontStyle.Regular))
            {
                int left = _seg.Right + 8;
                Rectangle rCount = new Rectangle(left, 4, Math.Max(40, this.ClientSize.Width - left - 12), 28);
                TextRenderer.DrawText(g, string.Format("共 {0} 条", _itemCount), fCount, rCount, cntCol,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }
    }

    /// <summary>
    /// 窄模式自适应顶部横向分类栏（桌面级 7px 圆角 + 发丝分割 + 下拉全部分类入口 + macOS 右键菜单）
    /// </summary>
    public class CustomCategoryBar : Control
    {
        public List<string> Categories { get; set; }
        public string ActiveCategory { get; set; }
        public bool IsDarkTheme { get; set; }

        public event Action<string> OnCategorySelected;
        public event Action OnAddCategoryRequested;
        public event Action OnDeleteCurrentCategoryRequested;
        public event Action<string> OnRenameCategoryRequested;
        public event Action<string> OnDeleteCategoryRequested;
        public event Action<Point> OnAllCategoriesPopupRequested;

        private int _scrollX = 0;
        private int _hoverIndex = -1;
        private bool _hoverPopBtn = false;
        private bool _hoverAddBtn = false;
        private bool _hoverDelBtn = false;

        public CustomCategoryBar()
        {
            Categories = new List<string>();
            ActiveCategory = "全部";
            IsDarkTheme = false;
            this.Height = 36;
            this.Cursor = Cursors.Hand;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);

            this.MouseWheel += (s, e) =>
            {
                _scrollX += (e.Delta > 0 ? 32 : -32);
                ClampScroll();
                this.Invalidate();
            };

            this.MouseMove += CustomCategoryBar_MouseMove;
            this.MouseLeave += (s, e) => { _hoverIndex = -1; _hoverPopBtn = false; _hoverAddBtn = false; _hoverDelBtn = false; this.Invalidate(); };
            this.MouseDown += CustomCategoryBar_MouseDown;
        }

        public void SetCategories(List<string> cats, string active)
        {
            Categories = new List<string> { "全部" };
            if (cats != null) Categories.AddRange(cats);
            ActiveCategory = active ?? "全部";
            _hoverIndex = -1;
            _hoverPopBtn = false;
            _hoverAddBtn = false;
            _hoverDelBtn = false;
            _scrollX = 0;
            ClampScroll();
            this.Invalidate();
        }

        private int GetTotalWidth(Graphics g, Font font)
        {
            int total = 10;
            for (int i = 0; i < Categories.Count; i++)
            {
                SizeF size = g.MeasureString(Categories[i], font);
                total += (int)size.Width + 20;
            }
            return total;
        }

        private void ClampScroll()
        {
            if (!this.IsHandleCreated) return;
            using (Graphics g = this.CreateGraphics())
            using (Font f = WarmTheme.CreateFont(9F))
            {
                int total = GetTotalWidth(g, f);
                int scrollAreaW = Math.Max(40, this.ClientSize.Width - 100);
                int maxScroll = Math.Max(0, total - scrollAreaW);
                if (_scrollX < -maxScroll) _scrollX = -maxScroll;
                if (_scrollX > 0) _scrollX = 0;
            }
        }

        private void CalculateHitBoxes(Graphics g, Font font, Font activeFont, out List<Rectangle> catRects, out Rectangle popRect, out Rectangle addRect, out Rectangle delRect)
        {
            catRects = new List<Rectangle>();
            int curX = 10 + _scrollX;
            for (int i = 0; i < Categories.Count; i++)
            {
                string cat = Categories[i];
                bool isActive = (cat == ActiveCategory);
                Font drawFont = isActive ? activeFont : font;
                SizeF size = g.MeasureString(cat, drawFont);
                int itemW = (int)size.Width + 16;
                catRects.Add(new Rectangle(curX, 5, itemW, 26));
                curX += itemW + 5;
            }
            // 右侧固定工具按钮组：[∨] [+] [-]
            int rightEdge = Math.Max(120, this.ClientSize.Width);
            delRect = new Rectangle(rightEdge - 32, 5, 24, 26);
            addRect = new Rectangle(rightEdge - 59, 5, 24, 26);
            popRect = new Rectangle(rightEdge - 86, 5, 24, 26);
        }

        private void HitTest(Point pt, out int hitIdx, out bool hitPop, out bool hitAdd, out bool hitDel)
        {
            hitIdx = -1;
            hitPop = false;
            hitAdd = false;
            hitDel = false;
            using (Graphics g = this.CreateGraphics())
            using (Font font = WarmTheme.CreateFont(9F, FontStyle.Regular))
            using (Font activeFont = WarmTheme.CreateFont(9F, FontStyle.Bold))
            {
                List<Rectangle> catRects;
                Rectangle popRect, addRect, delRect;
                CalculateHitBoxes(g, font, activeFont, out catRects, out popRect, out addRect, out delRect);

                if (popRect.Contains(pt)) { hitPop = true; return; }
                if (addRect.Contains(pt)) { hitAdd = true; return; }
                if (delRect.Contains(pt)) { hitDel = true; return; }

                int clipRight = Math.Max(40, this.ClientSize.Width - 94);
                if (pt.X < clipRight)
                {
                    for (int i = 0; i < catRects.Count; i++)
                    {
                        if (catRects[i].Contains(pt))
                        {
                            hitIdx = i;
                            return;
                        }
                    }
                }
            }
        }

        private void CustomCategoryBar_MouseMove(object sender, MouseEventArgs e)
        {
            int prevIdx = _hoverIndex;
            bool prevPop = _hoverPopBtn;
            bool prevAdd = _hoverAddBtn;
            bool prevDel = _hoverDelBtn;

            HitTest(e.Location, out _hoverIndex, out _hoverPopBtn, out _hoverAddBtn, out _hoverDelBtn);

            if (prevIdx != _hoverIndex || prevPop != _hoverPopBtn || prevAdd != _hoverAddBtn || prevDel != _hoverDelBtn)
            {
                this.Invalidate();
            }
        }

        private void CustomCategoryBar_MouseDown(object sender, MouseEventArgs e)
        {
            int hitIdx;
            bool hitPop, hitAdd, hitDel;
            HitTest(e.Location, out hitIdx, out hitPop, out hitAdd, out hitDel);

            if (hitPop && e.Button == MouseButtons.Left)
            {
                if (OnAllCategoriesPopupRequested != null)
                {
                    Point screenPt = this.PointToScreen(new Point(this.ClientSize.Width - 92, this.Height - 2));
                    OnAllCategoriesPopupRequested(screenPt);
                }
                return;
            }

            if (hitAdd && e.Button == MouseButtons.Left)
            {
                if (OnAddCategoryRequested != null) OnAddCategoryRequested();
                return;
            }

            if (hitDel && e.Button == MouseButtons.Left)
            {
                if (OnDeleteCurrentCategoryRequested != null) OnDeleteCurrentCategoryRequested();
                return;
            }

            if (hitIdx >= 0 && hitIdx < Categories.Count)
            {
                string cat = Categories[hitIdx];
                if (e.Button == MouseButtons.Left)
                {
                    ActiveCategory = cat;
                    this.Invalidate();
                    if (OnCategorySelected != null) OnCategorySelected(cat);
                }
                else if (e.Button == MouseButtons.Right && cat != "全部")
                {
                    ContextMenuStrip cms = ModernMenuHelper.CreateMenu(IsDarkTheme);
                    ModernMenuHelper.AddItem(cms, "修改分类名称", "F2", () => { if (OnRenameCategoryRequested != null) OnRenameCategoryRequested(cat); }, false);
                    ModernMenuHelper.AddSeparator(cms);
                    ModernMenuHelper.AddItem(cms, "删除此分类", "Del", () => { if (OnDeleteCategoryRequested != null) OnDeleteCategoryRequested(cat); }, true);
                    cms.Show(this, e.Location);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color barBg = IsDarkTheme ? WarmTheme.DarkPaperBg : WarmTheme.LightPaperBg;
            Color bottomLine = IsDarkTheme ? WarmTheme.HairlineDark : WarmTheme.HairlineLight;

            g.Clear(barBg);
            using (Pen linePen = new Pen(bottomLine, 1))
            {
                g.DrawLine(linePen, 0, this.Height - 1, this.Width, this.Height - 1);
            }

            using (Font font = WarmTheme.CreateFont(9F, FontStyle.Regular))
            using (Font activeFont = WarmTheme.CreateFont(9F, FontStyle.Bold))
            {
                List<Rectangle> catRects;
                Rectangle popRect, addRect, delRect;
                CalculateHitBoxes(g, font, activeFont, out catRects, out popRect, out addRect, out delRect);

                int clipRight = Math.Max(40, this.ClientSize.Width - 94);
                g.SetClip(new Rectangle(0, 0, clipRight, this.Height));

                for (int i = 0; i < Categories.Count; i++)
                {
                    string cat = Categories[i];
                    bool isActive = (cat == ActiveCategory);
                    bool isHover = (i == _hoverIndex);
                    Rectangle rect = catRects[i];

                    if (isActive)
                    {
                        GlassRenderer.DrawModernButtonSurface(g, rect, ButtonVariant.Primary, WarmTheme.Orange, IsDarkTheme, isHover ? 1f : 0f, false, false, true, WarmTheme.RadiusControl);
                        TextRenderer.DrawText(g, cat, activeFont, rect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                    }
                    else
                    {
                        Color bgNormal = isHover ? (IsDarkTheme ? WarmTheme.GhostHoverDark : WarmTheme.GhostHoverLight) : Color.Transparent;
                        Color bdr = isHover ? (IsDarkTheme ? WarmTheme.HairlineDark : WarmTheme.BorderSecondaryLight) : Color.Transparent;
                        Color txt = IsDarkTheme ? WarmTheme.DarkTextSecondary : WarmTheme.LightChipText;
                        GlassRenderer.DrawLiquidGlassButton(g, rect, bgNormal, bgNormal, bdr, isHover, WarmTheme.RadiusControl);
                        TextRenderer.DrawText(g, cat, font, rect, txt, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                    }
                }

                g.ResetClip();

                int dockLeft = Math.Max(40, this.ClientSize.Width - 94);
                using (Brush maskBrush = new SolidBrush(barBg))
                {
                    g.FillRectangle(maskBrush, dockLeft, 0, this.ClientSize.Width - dockLeft, this.Height - 1);
                }
                using (Pen divPen = new Pen(bottomLine, 1))
                {
                    g.DrawLine(divPen, dockLeft, 7, dockLeft, this.Height - 8);
                }

                Color iconMuted = IsDarkTheme ? WarmTheme.DarkTextSecondary : WarmTheme.LightChipText;

                // [∨] 全部分类浮层触发按钮（幽灵风格）
                GlassRenderer.DrawModernButtonSurface(g, popRect, ButtonVariant.Ghost, WarmTheme.Orange, IsDarkTheme, _hoverPopBtn ? 1f : 0f, false, false, true, 6);
                GlassRenderer.DrawVectorChevronDownIcon(g, popRect, _hoverPopBtn ? WarmTheme.Orange : iconMuted);

                // [+] 新建分类（幽灵风格）
                GlassRenderer.DrawModernButtonSurface(g, addRect, ButtonVariant.Ghost, WarmTheme.Orange, IsDarkTheme, _hoverAddBtn ? 1f : 0f, false, false, true, 6);
                GlassRenderer.DrawVectorPlusIcon(g, addRect, _hoverAddBtn ? WarmTheme.Orange : iconMuted);

                // [-] 删除分类（警示红悬停着底）
                Color delHoverBg = Color.FromArgb(IsDarkTheme ? 48 : 26, WarmTheme.DestructiveRed);
                GlassRenderer.DrawModernButtonSurface(g, delRect, ButtonVariant.Ghost, WarmTheme.DestructiveRed, IsDarkTheme, _hoverDelBtn ? 1f : 0f, false, false, true, 6, delHoverBg);
                GlassRenderer.DrawVectorMinusIcon(g, delRect, _hoverDelBtn ? WarmTheme.DestructiveRed : iconMuted);
            }
        }
    }

    /// <summary>
    /// macOS × iOS 融合工作区列表（双行密排、四级行内按钮、6px 悬浮渐隐滚动条、键盘导航与右键菜单）
    /// </summary>
    public class MiniPhraseListView : Control
    {
        public List<PhraseItem> Items { get; set; }
        public int ItemHeight { get; set; }
        public float CustomFontSize { get; set; }
        public string ThemeType { get; set; }
        public string CurrentMode { get; set; }
        public Image CustomBgImage { get; set; }
        public Image EmptyAvatarImage { get; set; }

        private int _hoverIndex = -1;
        private int _selectedIndex = -1;
        private int _hoverBtnType = 0;   // 0: 无, 1: 编辑(Ghost), 2: 填入(Secondary), 3: 发送(Primary)
        private int _pressedBtnType = 0; // 按压微下沉反馈
        private int _scrollOffset = 0;
        private bool _isDraggingScroll = false;
        private bool _isHoveringScroll = false;
        private int _dragStartY = 0;
        private int _dragStartOffset = 0;

        // 悬浮滚动条渐显/淡出状态机
        private float _scrollBarAlpha = 0f;
        private bool _scrollBarTargetVisible = false;
        private int _scrollHideCountdown = 0;
        private System.Windows.Forms.Timer _scrollFadeTimer;

        private Bitmap _cachedBgBitmap = null;
        private int _bgRenderVersion = 0;
        private Font _titleFont;
        private Font _secFont;
        private Font _btnFont;
        private Font _btnBoldFont;

        public event Action<PhraseItem, bool> OnActionTriggered;
        public event Action<PhraseItem> OnEditRequested;
        public event Action<PhraseItem> OnDeleteRequested;
        public event Action<PhraseItem> OnCopyRequested;

        public MiniPhraseListView()
        {
            Items = new List<PhraseItem>();
            ItemHeight = 40;
            CustomFontSize = 9.5F;
            ThemeType = "white";
            CurrentMode = "text";
            RebuildFonts();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.Selectable, true);
            this.TabStop = true;

            _scrollFadeTimer = new System.Windows.Forms.Timer { Interval = 30 };
            _scrollFadeTimer.Tick += (s, e) =>
            {
                if (_scrollHideCountdown > 0 && !_isDraggingScroll && !_isHoveringScroll)
                {
                    _scrollHideCountdown -= 30;
                    if (_scrollHideCountdown <= 0) _scrollBarTargetVisible = false;
                }

                float target = _scrollBarTargetVisible ? 1f : 0f;
                float diff = target - _scrollBarAlpha;
                if (Math.Abs(diff) < 0.04f)
                {
                    _scrollBarAlpha = target;
                    if (!_scrollBarTargetVisible && _scrollHideCountdown <= 0) _scrollFadeTimer.Stop();
                }
                else
                {
                    _scrollBarAlpha += diff * 0.28f;
                }
                this.Invalidate();
            };

            this.MouseWheel += (s, e) =>
            {
                int maxScroll = GetMaxScroll();
                if (maxScroll > 0)
                {
                    _scrollOffset -= (e.Delta / 120) * ItemHeight;
                    if (_scrollOffset < 0) _scrollOffset = 0;
                    if (_scrollOffset > maxScroll) _scrollOffset = maxScroll;
                    ShowScrollBarBriefly();
                    this.Invalidate();
                }
            };

            this.MouseMove += MiniPhraseListView_MouseMove;
            this.MouseLeave += (s, e) =>
            {
                _hoverIndex = -1;
                _hoverBtnType = 0;
                _pressedBtnType = 0;
                _isHoveringScroll = false;
                this.Invalidate();
            };
            this.MouseDown += MiniPhraseListView_MouseDown;
            this.MouseUp += (s, e) =>
            {
                _isDraggingScroll = false;
                if (_pressedBtnType != 0) { _pressedBtnType = 0; this.Invalidate(); }
            };
            this.DoubleClick += MiniPhraseListView_DoubleClick;
            this.GotFocus += (s, e) => this.Invalidate();
            this.LostFocus += (s, e) => this.Invalidate();
        }

        private void ShowScrollBarBriefly()
        {
            if (!NeedsScrollBar()) return;
            _scrollBarTargetVisible = true;
            _scrollHideCountdown = 900; // 停止滚动约 0.9s 后优雅淡出
            if (!_scrollFadeTimer.Enabled) _scrollFadeTimer.Start();
        }

        private void RebuildFonts()
        {
            if (_titleFont != null) _titleFont.Dispose();
            if (_secFont != null) _secFont.Dispose();
            if (_btnFont == null) _btnFont = WarmTheme.CreateFont(8.8F, FontStyle.Regular);
            if (_btnBoldFont == null) _btnBoldFont = WarmTheme.CreateFont(8.8F, FontStyle.Bold);

            _titleFont = WarmTheme.CreateFont(CustomFontSize, FontStyle.Regular);
            _secFont = WarmTheme.CreateFont(GetSecondaryFontSize(CustomFontSize), FontStyle.Regular);
        }

        private void RebuildBgCacheAsync()
        {
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            if (ThemeType != "custom" || CustomBgImage == null || w <= 0 || h <= 0)
            {
                if (_cachedBgBitmap != null) { _cachedBgBitmap.Dispose(); _cachedBgBitmap = null; }
                return;
            }

            int version = ++_bgRenderVersion;
            Image srcClone = null;
            try { srcClone = (Image)CustomBgImage.Clone(); } catch { return; }
            Color maskColor = Color.FromArgb(65, WarmTheme.LightPaperBg);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        DrawImageCover(g, srcClone, new Rectangle(0, 0, w, h), true);
                        using (Brush mask = new SolidBrush(maskColor))
                        {
                            g.FillRectangle(mask, 0, 0, w, h);
                        }
                    }
                    srcClone.Dispose();

                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.BeginInvoke((Action)(() =>
                        {
                            if (version == _bgRenderVersion && !this.IsDisposed)
                            {
                                if (_cachedBgBitmap != null) _cachedBgBitmap.Dispose();
                                _cachedBgBitmap = bmp;
                                this.Invalidate();
                            }
                            else
                            {
                                bmp.Dispose();
                            }
                        }));
                    }
                    else
                    {
                        bmp.Dispose();
                    }
                }
                catch
                {
                    if (srcClone != null) srcClone.Dispose();
                }
            });
        }

        public void SetTheme(string theme, Image bgImg)
        {
            ThemeType = theme ?? "white";
            CustomBgImage = bgImg;
            RebuildBgCacheAsync();
            this.Invalidate();
        }

        public void SetFontConfig(float fontSize, int itemHeight)
        {
            CustomFontSize = fontSize;
            ItemHeight = itemHeight;
            RebuildFonts();
            ClampScroll();
            this.Invalidate();
        }

        public void UpdateData(List<PhraseItem> list)
        {
            Items = list ?? new List<PhraseItem>();
            _selectedIndex = -1;
            _hoverIndex = -1;
            _hoverBtnType = 0;
            _pressedBtnType = 0;
            ClampScroll();
            this.Invalidate();
        }

        public PhraseItem GetSelectedOrFirstItem()
        {
            if (Items == null || Items.Count == 0) return null;
            if (_selectedIndex >= 0 && _selectedIndex < Items.Count) return Items[_selectedIndex];
            return Items[0];
        }

        public void SelectFirstRow()
        {
            if (Items != null && Items.Count > 0)
            {
                _selectedIndex = 0;
                _scrollOffset = 0;
                this.Focus();
                this.Invalidate();
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            if (k == Keys.Up || k == Keys.Down || k == Keys.Enter || k == Keys.Delete) return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (Items == null || Items.Count == 0) return;

            if (e.KeyCode == Keys.Down)
            {
                _selectedIndex = (_selectedIndex < 0) ? 0 : Math.Min(Items.Count - 1, _selectedIndex + 1);
                EnsureSelectedVisible();
                this.Invalidate();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                _selectedIndex = (_selectedIndex < 0) ? 0 : Math.Max(0, _selectedIndex - 1);
                EnsureSelectedVisible();
                this.Invalidate();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && _selectedIndex >= 0 && _selectedIndex < Items.Count)
            {
                bool sendEnter = !e.Shift;
                if (OnActionTriggered != null) OnActionTriggered(Items[_selectedIndex], sendEnter);
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.E && _selectedIndex >= 0 && _selectedIndex < Items.Count)
            {
                if (OnEditRequested != null) OnEditRequested(Items[_selectedIndex]);
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.C && _selectedIndex >= 0 && _selectedIndex < Items.Count)
            {
                if (OnCopyRequested != null) OnCopyRequested(Items[_selectedIndex]);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete && _selectedIndex >= 0 && _selectedIndex < Items.Count)
            {
                if (OnDeleteRequested != null) OnDeleteRequested(Items[_selectedIndex]);
                e.Handled = true;
            }
        }

        private void EnsureSelectedVisible()
        {
            if (_selectedIndex < 0) return;
            int top = _selectedIndex * ItemHeight;
            int bot = top + ItemHeight;
            if (top < _scrollOffset) _scrollOffset = top;
            else if (bot > _scrollOffset + this.ClientSize.Height) _scrollOffset = bot - this.ClientSize.Height;
            ClampScroll();
            ShowScrollBarBriefly();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ClampScroll();
            RebuildBgCacheAsync();
        }

        private int GetMaxScroll()
        {
            return Math.Max(0, Items.Count * ItemHeight - this.ClientSize.Height);
        }

        private void ClampScroll()
        {
            int maxScroll = GetMaxScroll();
            if (_scrollOffset > maxScroll) _scrollOffset = maxScroll;
            if (_scrollOffset < 0) _scrollOffset = 0;
        }

        private bool NeedsScrollBar()
        {
            return Items.Count * ItemHeight > this.ClientSize.Height;
        }

        private Rectangle GetScrollThumbRect()
        {
            int totalH = Items.Count * ItemHeight;
            int viewH = this.ClientSize.Height;
            if (totalH <= viewH || viewH <= 0) return Rectangle.Empty;

            int thumbH = Math.Max(26, (int)((double)viewH * viewH / totalH));
            int maxThumbY = viewH - thumbH;
            int maxScroll = totalH - viewH;
            int thumbY = maxScroll > 0 ? (int)((double)_scrollOffset / maxScroll * maxThumbY) : 0;
            int barW = (_isHoveringScroll || _isDraggingScroll) ? 7 : 6;
            return new Rectangle(this.ClientSize.Width - barW - 3, thumbY + 3, barW, Math.Max(20, thumbH - 6));
        }

        private float GetSecondaryFontSize(float titleSize)
        {
            if (titleSize >= 15.0F) return 11.0F;
            if (titleSize >= 13.0F) return 9.8F;
            if (titleSize >= 11.0F) return 8.8F;
            return 8.0F;
        }

        private void ComputeRowButtonRects(int rowY, int usableW, out Rectangle rEdit, out Rectangle rFill, out Rectangle rSend)
        {
            int sendW = 36;
            int fillW = 32;
            int editW = 26;
            int btnH = Math.Min(26, ItemHeight - 10);
            int btnY = rowY + (ItemHeight - btnH) / 2;

            int sendLeft = usableW - sendW - 6;
            int fillLeft = sendLeft - fillW - 4;
            int editLeft = fillLeft - editW - 4;

            rSend = new Rectangle(sendLeft, btnY, sendW, btnH);
            rFill = new Rectangle(fillLeft, btnY, fillW, btnH);
            rEdit = new Rectangle(editLeft, btnY, editW, btnH);
        }

        private void MiniPhraseListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingScroll)
            {
                int viewH = this.ClientSize.Height;
                Rectangle thumb = GetScrollThumbRect();
                int trackRange = Math.Max(1, viewH - thumb.Height);
                int maxScroll = GetMaxScroll();
                int deltaY = e.Y - _dragStartY;
                _scrollOffset = _dragStartOffset + (int)((double)deltaY / trackRange * maxScroll);
                ClampScroll();
                ShowScrollBarBriefly();
                this.Invalidate();
                return;
            }

            bool overScrollZone = NeedsScrollBar() && e.X >= this.ClientSize.Width - 14;
            if (overScrollZone != _isHoveringScroll)
            {
                _isHoveringScroll = overScrollZone;
                if (overScrollZone) ShowScrollBarBriefly();
                this.Invalidate();
            }

            if (Items.Count == 0)
            {
                bool needInv = (_hoverIndex != -1 || _hoverBtnType != 0);
                _hoverIndex = -1;
                _hoverBtnType = 0;
                if (needInv) this.Invalidate();
                return;
            }

            int offset = _scrollOffset;
            int newIdx = (e.Y + offset) / ItemHeight;
            int prevIdx = _hoverIndex;
            int prevType = _hoverBtnType;

            if (newIdx >= 0 && newIdx < Items.Count)
            {
                _hoverIndex = newIdx;
                int usableW = this.ClientSize.Width - 6;
                int itemY = (newIdx * ItemHeight) - offset;
                Rectangle rEdit, rFill, rSend;
                ComputeRowButtonRects(itemY, usableW, out rEdit, out rFill, out rSend);

                if (rSend.Contains(e.Location)) _hoverBtnType = 3;
                else if (rFill.Contains(e.Location)) _hoverBtnType = 2;
                else if (rEdit.Contains(e.Location)) _hoverBtnType = 1;
                else _hoverBtnType = 0;

                if (prevIdx != _hoverIndex || prevType != _hoverBtnType) this.Invalidate();
            }
            else
            {
                _hoverIndex = -1;
                _hoverBtnType = 0;
                if (prevIdx != -1 || prevType != 0) this.Invalidate();
            }
        }

        private void MiniPhraseListView_MouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();

            if (e.Button == MouseButtons.Left && NeedsScrollBar() && e.X >= this.ClientSize.Width - 14)
            {
                Rectangle thumb = GetScrollThumbRect();
                if (thumb.Contains(e.Location))
                {
                    _isDraggingScroll = true;
                    _dragStartY = e.Y;
                    _dragStartOffset = _scrollOffset;
                    ShowScrollBarBriefly();
                    return;
                }
            }

            if (Items.Count == 0)
            {
                _hoverIndex = -1;
                _hoverBtnType = 0;
                return;
            }

            int clickIdx = (e.Y + _scrollOffset) / ItemHeight;
            if (clickIdx < 0 || clickIdx >= Items.Count)
            {
                _hoverIndex = -1;
                _hoverBtnType = 0;
                _selectedIndex = -1;
                this.Invalidate();
                return;
            }

            _hoverIndex = clickIdx;
            _selectedIndex = clickIdx;
            var item = Items[clickIdx];

            if (e.Button == MouseButtons.Right)
            {
                bool isDark = (ThemeType == "black");
                ContextMenuStrip cms = ModernMenuHelper.CreateMenu(isDark);
                ModernMenuHelper.AddItem(cms, "直接发送", "Enter", () => { if (OnActionTriggered != null) OnActionTriggered(item, true); }, false);
                ModernMenuHelper.AddItem(cms, "填入输入框", "Shift+Enter", () => { if (OnActionTriggered != null) OnActionTriggered(item, false); }, false);
                ModernMenuHelper.AddItem(cms, "复制正文内容", "Ctrl+C", () => { if (OnCopyRequested != null) OnCopyRequested(item); }, false);
                ModernMenuHelper.AddSeparator(cms);
                ModernMenuHelper.AddItem(cms, "编辑条目...", "Ctrl+E", () => { if (OnEditRequested != null) OnEditRequested(item); }, false);
                ModernMenuHelper.AddItem(cms, "删除条目", "Del", () => { if (OnDeleteRequested != null) OnDeleteRequested(item); }, true);
                this.Invalidate();
                cms.Show(this, e.Location);
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (_hoverBtnType > 0) _pressedBtnType = _hoverBtnType;
                if (_hoverBtnType == 3)
                {
                    if (OnActionTriggered != null) OnActionTriggered(item, true);
                }
                else if (_hoverBtnType == 2)
                {
                    if (OnActionTriggered != null) OnActionTriggered(item, false);
                }
                else if (_hoverBtnType == 1)
                {
                    if (OnEditRequested != null) OnEditRequested(item);
                }
                this.Invalidate();
            }
        }

        private void MiniPhraseListView_DoubleClick(object sender, EventArgs e)
        {
            if (Items.Count == 0) return;
            Point pt = this.PointToClient(Cursor.Position);
            int clickIdx = (pt.Y + _scrollOffset) / ItemHeight;
            if (clickIdx >= 0 && clickIdx < Items.Count)
            {
                var item = Items[clickIdx];
                if (OnActionTriggered != null) OnActionTriggered(item, true);
            }
        }

        private void DrawImageCover(Graphics g, Image img, Rectangle target, bool highQuality)
        {
            if (img == null || target.Width <= 0 || target.Height <= 0) return;
            float imgRatio = (float)img.Width / img.Height;
            float targetRatio = (float)target.Width / target.Height;
            RectangleF srcRect;
            if (imgRatio > targetRatio)
            {
                float srcW = img.Height * targetRatio;
                float srcX = (img.Width - srcW) / 2f;
                srcRect = new RectangleF(srcX, 0, srcW, img.Height);
            }
            else
            {
                float srcH = img.Width / targetRatio;
                float srcY = (img.Height - srcH) / 2f;
                srcRect = new RectangleF(0, srcY, img.Width, srcH);
            }
            g.InterpolationMode = highQuality ? InterpolationMode.HighQualityBicubic : InterpolationMode.Low;
            g.DrawImage(img, target, srcRect, GraphicsUnit.Pixel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_scrollFadeTimer != null) { _scrollFadeTimer.Stop(); _scrollFadeTimer.Dispose(); _scrollFadeTimer = null; }
                if (_cachedBgBitmap != null) { _cachedBgBitmap.Dispose(); _cachedBgBitmap = null; }
                if (_titleFont != null) { _titleFont.Dispose(); _titleFont = null; }
                if (_secFont != null) { _secFont.Dispose(); _secFont = null; }
                if (_btnFont != null) { _btnFont.Dispose(); _btnFont = null; }
                if (_btnBoldFont != null) { _btnBoldFont.Dispose(); _btnBoldFont = null; }
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            bool isDark = (ThemeType == "black");

            if (isDark)
            {
                g.Clear(WarmTheme.DarkPaperBg);
            }
            else if (ThemeType == "custom" && CustomBgImage != null)
            {
                if (_cachedBgBitmap != null && _cachedBgBitmap.Width == this.ClientSize.Width && _cachedBgBitmap.Height == this.ClientSize.Height)
                {
                    g.DrawImageUnscaled(_cachedBgBitmap, 0, 0);
                }
                else
                {
                    DrawImageCover(g, CustomBgImage, this.ClientRectangle, false);
                    using (Brush mask = new SolidBrush(Color.FromArgb(65, WarmTheme.LightPaperBg)))
                    {
                        g.FillRectangle(mask, this.ClientRectangle);
                    }
                }
            }
            else
            {
                g.Clear(WarmTheme.LightPaperBg);
            }

            // 空状态绘制（居中显示花花头像小图 + 引导文字）
            if (Items.Count == 0)
            {
                int cy = this.ClientSize.Height / 2;
                int cx = this.ClientSize.Width / 2;
                if (EmptyAvatarImage != null)
                {
                    Rectangle imgRect = new Rectangle(cx - 24, cy - 44, 48, 48);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    using (GraphicsPath clip = new GraphicsPath())
                    {
                        clip.AddEllipse(imgRect);
                        g.SetClip(clip);
                        g.DrawImage(EmptyAvatarImage, imgRect);
                        g.ResetClip();
                    }
                    Color ringCol = WarmTheme.Current.IsPink
                        ? WarmTheme.Orange
                        : (isDark ? WarmTheme.HairlineDark : WarmTheme.BorderSecondaryLight);
                    float ringW = WarmTheme.Current.IsPink ? 1.8f : 1.0f;
                    using (Pen ringPen = new Pen(ringCol, ringW))
                    {
                        g.DrawEllipse(ringPen, imgRect);
                    }
                }
                Color emptyTextCol = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary;
                Rectangle txtRect = new Rectangle(12, cy + 12, this.ClientSize.Width - 24, 24);
                string emptyHint = (CurrentMode == "video") ? "暂无视频快捷方式 · 按 Ctrl+N 新增" : "暂无话术条目 · 按 Ctrl+N 新增";
                using (Font emptyFont = WarmTheme.CreateFont(9F, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, emptyHint, emptyFont, txtRect, emptyTextCol,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                }
                return;
            }

            int offset = _scrollOffset;
            int start = Math.Max(0, offset / ItemHeight);
            int count = (this.ClientSize.Height / ItemHeight) + 2;
            int end = Math.Min(Items.Count - 1, start + count);

            // 悬浮滚动条不常驻挤压列表内容宽度
            int usableW = this.ClientSize.Width - 6;

            Color textPrimary = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
            Color textSecondary = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary;
            Color rowLine = isDark ? WarmTheme.HairlineDark : WarmTheme.HairlineLight;

            Font titleFont = _titleFont;
            Font secFont = _secFont;
            Font btnFont = _btnFont;
            Font btnBoldFont = _btnBoldFont;

            using (Pen linePen = new Pen(rowLine, 1))
            {
                for (int i = start; i <= end; i++)
                {
                    var item = Items[i];
                    int y = (i * ItemHeight) - offset;
                    bool isHover = (i == _hoverIndex);
                    bool isSelected = (i == _selectedIndex);

                    Rectangle rowRect = new Rectangle(6, y + 1, Math.Max(40, usableW - 6), ItemHeight - 2);

                    Color rowBg;
                    if (isDark)
                    {
                        if (isSelected) rowBg = WarmTheme.DarkRowSelected;
                        else if (isHover) rowBg = WarmTheme.DarkRowHover;
                        else rowBg = WarmTheme.DarkPaperBg;
                    }
                    else if (ThemeType == "custom")
                    {
                        if (isSelected) rowBg = Color.FromArgb(230, WarmTheme.LightRowSelected);
                        else if (isHover) rowBg = Color.FromArgb(215, WarmTheme.LightRowHover);
                        else rowBg = Color.FromArgb(165, WarmTheme.LightCardBg);
                    }
                    else
                    {
                        if (isSelected) rowBg = WarmTheme.LightRowSelected;
                        else if (isHover) rowBg = WarmTheme.LightRowHover;
                        else rowBg = WarmTheme.LightCardBg;
                    }

                    if (isHover || isSelected || ThemeType == "custom")
                    {
                        using (GraphicsPath rowPath = GlassRenderer.CreateRoundedPath(rowRect, 8))
                        using (Brush bgBrush = new SolidBrush(rowBg))
                        {
                            g.FillPath(bgBrush, rowPath);
                            if (isSelected && this.Focused && this.ShowFocusCues)
                            {
                                using (Pen fp = new Pen(WarmTheme.ApplyAlpha(WarmTheme.Orange, 0.55f), 1.5f))
                                    g.DrawPath(fp, rowPath);
                            }
                        }
                    }
                    else
                    {
                        using (Brush bgBrush = new SolidBrush(rowBg))
                        {
                            g.FillRectangle(bgBrush, new Rectangle(0, y, this.ClientSize.Width, ItemHeight));
                        }
                    }

                    if (!isHover && !isSelected)
                    {
                        g.DrawLine(linePen, 14, y + ItemHeight - 1, usableW - 6, y + ItemHeight - 1);
                    }

                    Rectangle rEdit, rFill, rSend;
                    ComputeRowButtonRects(y, usableW, out rEdit, out rFill, out rSend);

                    int textLeft = 14;
                    int textW = Math.Max(20, rEdit.Left - textLeft - 8);
                    int line1H = (int)Math.Ceiling(titleFont.GetHeight(g));
                    int line2H = (int)Math.Ceiling(secFont.GetHeight(g));
                    int totalTxtH = line1H + line2H;
                    int txtStartY = y + Math.Max(2, (ItemHeight - totalTxtH) / 2);

                    Rectangle titleRect = new Rectangle(textLeft, txtStartY, textW, line1H);
                    Rectangle secRect = new Rectangle(textLeft, txtStartY + line1H, textW, line2H);

                    TextRenderer.DrawText(g, item.Title, titleFont, titleRect, textPrimary,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);

                    string subInfo = string.IsNullOrEmpty(item.Category) ? "未分类" : item.Category;
                    if (item.ItemType == "video" && !string.IsNullOrEmpty(item.VideoPath))
                        subInfo += " · 视频";
                    else if (!string.IsNullOrEmpty(item.ImagePath))
                        subInfo += " · 带图";

                    TextRenderer.DrawText(g, subInfo, secFont, secRect, textSecondary,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);

                    // 1. 编辑（Ghost 无边框幽灵按钮：平时透明，悬停现柔和圆角底）
                    bool hoverEdit = (isHover && _hoverBtnType == 1);
                    bool pressEdit = (isHover && _pressedBtnType == 1);
                    GlassRenderer.DrawModernButtonSurface(g, rEdit, ButtonVariant.Ghost, WarmTheme.Orange, isDark, hoverEdit ? 1f : 0f, pressEdit, false, true, 6);
                    Color editIconCol = hoverEdit
                        ? (isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary)
                        : (isDark ? WarmTheme.DarkTextMuted : WarmTheme.LightTextMuted);
                    GlassRenderer.DrawVectorEditIcon(g, rEdit, editIconCol);

                    // 2. 填入（Secondary 次级质感按钮）
                    bool hoverFill = (isHover && _hoverBtnType == 2);
                    bool pressFill = (isHover && _pressedBtnType == 2);
                    GlassRenderer.DrawModernButtonSurface(g, rFill, ButtonVariant.Secondary, WarmTheme.Orange, isDark, hoverFill ? 1f : 0f, pressFill, false, true, 6);
                    Color fillTxt = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
                    TextRenderer.DrawText(g, "填", btnFont, rFill, fillTxt, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);

                    // 3. 发送（Primary 强调色主按钮 + inset 高光）
                    bool hoverSend = (isHover && _hoverBtnType == 3);
                    bool pressSend = (isHover && _pressedBtnType == 3);
                    GlassRenderer.DrawModernButtonSurface(g, rSend, ButtonVariant.Primary, WarmTheme.Orange, isDark, hoverSend ? 1f : 0f, pressSend, false, true, 6);
                    TextRenderer.DrawText(g, "发", btnBoldFont, rSend, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                }
            }

            // macOS 风格极细 (6px) 圆角半透明悬浮滚动条，仅悬停或滚动时淡入显现
            if (NeedsScrollBar() && _scrollBarAlpha > 0.02f)
            {
                Rectangle thumb = GetScrollThumbRect();
                if (!thumb.IsEmpty)
                {
                    Color baseThumb = isDark ? WarmTheme.DarkScrollThumb : WarmTheme.LightScrollThumb;
                    Color thumbCol = WarmTheme.ApplyAlpha(baseThumb, _scrollBarAlpha);
                    using (GraphicsPath thumbPath = GlassRenderer.CreateRoundedPath(thumb, 3))
                    using (Brush thumbBrush = new SolidBrush(thumbCol))
                    {
                        g.FillPath(thumbBrush, thumbPath);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 一体化顶部栏背景渲染面板（1px 发丝分割线 + 支持无边框窗口拖拽）
    /// </summary>
    public class GlassTopBarPanel : Panel
    {
        public bool IsDarkTheme { get; set; }

        public GlassTopBarPanel()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Color baseBg = IsDarkTheme ? WarmTheme.DarkCardBg : WarmTheme.LightCardBg;
            e.Graphics.Clear(baseBg);
            GlassRenderer.DrawLiquidGlassPanel(e.Graphics, this.ClientRectangle, IsDarkTheme, 0);
            Color lineCol = IsDarkTheme ? WarmTheme.HairlineDark : WarmTheme.HairlineLight;
            using (Pen p = new Pen(lineCol, 1))
            {
                e.Graphics.DrawLine(p, 0, this.Height - 1, this.Width, this.Height - 1);
            }
        }
    }

    /// <summary>
    /// macOS / iOS 分段控制器 (Segmented Control)：凹槽底 + 滑动白/深灰滑块 + 160ms cubic-bezier 缓动
    /// </summary>
    public class SegmentedControl : Control
    {
        private string[] _items = new string[0];
        private int _selectedIndex = 0;
        private int _hoverIndex = -1;
        private float _animIndex = 0f;
        private float _animStartIndex = 0f;
        private float _animProgress = 1f;
        private System.Windows.Forms.Timer _animTimer;

        public string[] Items
        {
            get { return _items; }
            set
            {
                _items = value ?? new string[0];
                if (_selectedIndex >= _items.Length) _selectedIndex = 0;
                _animIndex = _selectedIndex;
                this.Invalidate();
            }
        }

        public Action<Graphics, Rectangle, Color>[] IconPainters { get; set; }
        public bool IsDarkTheme { get; set; }
        public Color? ActivePillColor { get; set; }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_items == null || _items.Length == 0) return;
                int clamped = Math.Max(0, Math.Min(_items.Length - 1, value));
                if (_selectedIndex != clamped)
                {
                    _animStartIndex = _animIndex;
                    _selectedIndex = clamped;
                    _animProgress = 0f;
                    StartSlideAnim();
                    if (SelectedIndexChanged != null) SelectedIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler SelectedIndexChanged;

        public SegmentedControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.SupportsTransparentBackColor |
                          ControlStyles.Selectable, true);
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
            this.TabStop = true;
            this.Font = WarmTheme.CreateFont(9F, FontStyle.Regular);

            this.MouseMove += (s, e) =>
            {
                int prev = _hoverIndex;
                _hoverIndex = HitItem(e.X);
                if (prev != _hoverIndex) this.Invalidate();
            };
            this.MouseLeave += (s, e) => { _hoverIndex = -1; this.Invalidate(); };
            this.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                int idx = HitItem(e.X);
                if (idx >= 0) SelectedIndex = idx;
            };
            this.GotFocus += (s, e) => this.Invalidate();
            this.LostFocus += (s, e) => this.Invalidate();
        }

        private int HitItem(int x)
        {
            if (_items == null || _items.Length == 0 || this.Width <= 4) return -1;
            int innerW = this.Width - 4;
            float segW = (float)innerW / _items.Length;
            int idx = (int)((x - 2) / segW);
            if (idx < 0 || idx >= _items.Length) return -1;
            return idx;
        }

        private void StartSlideAnim()
        {
            if (_animTimer == null)
            {
                _animTimer = new System.Windows.Forms.Timer { Interval = 16 };
                _animTimer.Tick += (s, e) =>
                {
                    _animProgress += 0.12f; // ~160ms
                    if (_animProgress >= 1f)
                    {
                        _animProgress = 1f;
                        _animIndex = _selectedIndex;
                        _animTimer.Stop();
                    }
                    else
                    {
                        float eased = MotionHelper.EaseApple(_animProgress);
                        _animIndex = _animStartIndex + (_selectedIndex - _animStartIndex) * eased;
                    }
                    this.Invalidate();
                };
            }
            if (!_animTimer.Enabled) _animTimer.Start();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Left || keyData == Keys.Right) return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_items == null || _items.Length == 0) return;
            if (e.KeyCode == Keys.Right) { SelectedIndex = Math.Min(_items.Length - 1, _selectedIndex + 1); e.Handled = true; }
            else if (e.KeyCode == Keys.Left) { SelectedIndex = Math.Max(0, _selectedIndex - 1); e.Handled = true; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _animTimer != null) { _animTimer.Stop(); _animTimer.Dispose(); _animTimer = null; }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle trackRect = new Rectangle(2, 2, Math.Max(4, this.Width - 4), Math.Max(4, this.Height - 4));
            if (this.Focused && this.ShowFocusCues)
            {
                Rectangle ring = new Rectangle(trackRect.X - 2, trackRect.Y - 2, trackRect.Width + 4, trackRect.Height + 4);
                using (GraphicsPath rp = GlassRenderer.CreateRoundedPath(ring, 10))
                using (Pen rpen = new Pen(WarmTheme.ApplyAlpha(WarmTheme.Orange, 0.35f), 2.5f))
                {
                    g.DrawPath(rpen, rp);
                }
            }

            // 1. 凹槽底
            Color trackBg = IsDarkTheme ? WarmTheme.SegmentedTrackDark : WarmTheme.SegmentedTrackLight;
            using (GraphicsPath trackPath = GlassRenderer.CreateRoundedPath(trackRect, 8))
            using (Brush tb = new SolidBrush(trackBg))
            {
                g.FillPath(tb, trackPath);
            }

            if (_items == null || _items.Length == 0) return;
            float segW = (trackRect.Width - 4f) / _items.Length;

            // 2. 平滑滑块 Thumb
            RectangleF thumbF = new RectangleF(
                trackRect.X + 2 + _animIndex * segW,
                trackRect.Y + 2,
                segW,
                trackRect.Height - 4);
            Rectangle thumbRect = Rectangle.Round(thumbF);
            if (thumbRect.Width > 4 && thumbRect.Height > 4)
            {
                // 微投影
                Rectangle sh = new Rectangle(thumbRect.X, thumbRect.Y + 1, thumbRect.Width, thumbRect.Height);
                using (GraphicsPath sp = GlassRenderer.CreateRoundedPath(sh, 6))
                using (Brush sb = new SolidBrush(Color.FromArgb(IsDarkTheme ? 42 : 22, 0, 0, 0)))
                {
                    g.FillPath(sb, sp);
                }

                Color pillCol = ActivePillColor.HasValue
                    ? ActivePillColor.Value
                    : (IsDarkTheme ? Color.FromArgb(58, 58, 62) : Color.White);
                using (GraphicsPath tp = GlassRenderer.CreateRoundedPath(thumbRect, 6))
                {
                    using (Brush pb = new SolidBrush(pillCol)) g.FillPath(pb, tp);
                    if (IsDarkTheme)
                    {
                        g.SetClip(tp);
                        using (Pen hi = new Pen(WarmTheme.TopHighlightDark, 1))
                            g.DrawLine(hi, thumbRect.X + 2, thumbRect.Y + 1, thumbRect.Right - 2, thumbRect.Y + 1);
                        g.ResetClip();
                    }
                    else
                    {
                        using (Pen bdr = new Pen(Color.FromArgb(16, 0, 0, 0), 1)) g.DrawPath(bdr, tp);
                    }
                }
            }

            // 3. 文字与图标
            using (Font fReg = WarmTheme.CreateFont(8.5F, FontStyle.Regular))
            using (Font fBold = WarmTheme.CreateFont(8.5F, FontStyle.Bold))
            {
                for (int i = 0; i < _items.Length; i++)
                {
                    Rectangle itemRect = Rectangle.Round(new RectangleF(
                        trackRect.X + 2 + i * segW,
                        trackRect.Y + 2,
                        segW,
                        trackRect.Height - 4));
                    bool isSel = (i == _selectedIndex);
                    bool isHover = (i == _hoverIndex && !isSel);

                    Color txtCol;
                    if (isSel)
                    {
                        txtCol = ActivePillColor.HasValue
                            ? Color.White
                            : (IsDarkTheme ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary);
                    }
                    else if (isHover)
                    {
                        txtCol = IsDarkTheme ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
                    }
                    else
                    {
                        txtCol = IsDarkTheme ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary;
                    }

                    bool hasIcon = (IconPainters != null && i < IconPainters.Length && IconPainters[i] != null);
                    Font drawFont = isSel ? fBold : fReg;
                    if (hasIcon)
                    {
                        int iconSz = 14;
                        int gap = 4;
                        Size ts = TextRenderer.MeasureText(g, _items[i], drawFont, new Size(int.MaxValue, itemRect.Height), TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                        int totW = iconSz + gap + ts.Width;
                        int sx = itemRect.X + Math.Max(3, (itemRect.Width - totW) / 2);
                        Rectangle ir = new Rectangle(sx, itemRect.Y + (itemRect.Height - iconSz) / 2, iconSz, iconSz);
                        IconPainters[i](g, ir, txtCol);
                        Rectangle tr = new Rectangle(sx + iconSz + gap, itemRect.Y, Math.Max(8, itemRect.Right - (sx + iconSz + gap)), itemRect.Height);
                        TextRenderer.DrawText(g, _items[i], drawFont, tr, txtCol, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                    }
                    else
                    {
                        TextRenderer.DrawText(g, _items[i], drawFont, itemRect, txtCol, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 紧凑型 iOS 胶囊开关（胶囊体 36×20px，开启态 #34C759，160ms 平滑拨动动画 + 焦点环）
    /// </summary>
    public class ToggleSwitch : Control
    {
        private bool _checked = false;
        private bool _hovered = false;
        private float _knobProgress = 0f;
        private System.Windows.Forms.Timer _animTimer;

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    StartAnim();
                    if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public bool IsDarkTheme { get; set; }
        public Color OnColor { get; set; }
        public event EventHandler CheckedChanged;

        public ToggleSwitch()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.SupportsTransparentBackColor |
                          ControlStyles.Selectable, true);
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
            this.TabStop = true;
            this.Size = new Size(38, 22);
            this.Font = WarmTheme.CreateFont(9F, FontStyle.Regular);
            OnColor = WarmTheme.SwitchGreen; // #34C759

            this.MouseEnter += (s, e) => { _hovered = true; this.Invalidate(); };
            this.MouseLeave += (s, e) => { _hovered = false; this.Invalidate(); };
            this.Click += (s, e) => { Checked = !Checked; };
            this.GotFocus += (s, e) => this.Invalidate();
            this.LostFocus += (s, e) => this.Invalidate();
        }

        public void SetCheckedImmediate(bool value)
        {
            _checked = value;
            _knobProgress = value ? 1f : 0f;
            if (_animTimer != null) _animTimer.Stop();
            this.Invalidate();
        }

        private void StartAnim()
        {
            if (_animTimer == null)
            {
                _animTimer = new System.Windows.Forms.Timer { Interval = 16 };
                _animTimer.Tick += (s, e) =>
                {
                    float target = _checked ? 1f : 0f;
                    float diff = target - _knobProgress;
                    if (Math.Abs(diff) < 0.02f)
                    {
                        _knobProgress = target;
                        _animTimer.Stop();
                    }
                    else
                    {
                        _knobProgress += diff * 0.30f; // 160ms 跟手弹性收尾
                    }
                    this.Invalidate();
                };
            }
            if (!_animTimer.Enabled) _animTimer.Start();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                Checked = !Checked;
                e.Handled = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _animTimer != null) { _animTimer.Stop(); _animTimer.Dispose(); _animTimer = null; }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // 胶囊体尺寸 36×20
            int pillW = 36;
            int pillH = 20;
            int pillY = (this.Height - pillH) / 2;
            Rectangle pillRect = new Rectangle(2, pillY, pillW, pillH);

            if (this.Focused && this.ShowFocusCues)
            {
                Rectangle ringRect = new Rectangle(pillRect.X - 2, pillRect.Y - 2, pillRect.Width + 4, pillRect.Height + 4);
                using (GraphicsPath rp = GlassRenderer.CreateRoundedPath(ringRect, 12))
                using (Pen rpen = new Pen(WarmTheme.ApplyAlpha(_checked ? OnColor : WarmTheme.Orange, 0.35f), 2.5f))
                {
                    g.DrawPath(rpen, rp);
                }
            }

            float p = MotionHelper.EaseApple(_knobProgress);
            Color offBg = IsDarkTheme ? Color.FromArgb(66, 66, 70) : Color.FromArgb(212, 212, 216);
            Color trackCol = WarmTheme.BlendColor(offBg, OnColor, p);
            if (_hovered) trackCol = WarmTheme.BlendColor(trackCol, Color.White, IsDarkTheme ? 0.08f : 0.05f);

            using (GraphicsPath pillPath = GlassRenderer.CreateRoundedPath(pillRect, pillH / 2))
            using (Brush b = new SolidBrush(trackCol))
            {
                g.FillPath(b, pillPath);
            }

            // 圆形白色滑钮 16×16 带柔和微投影
            float knobSize = 16f;
            float knobX = pillRect.X + 2f + p * (pillW - knobSize - 4f);
            float knobY = pillRect.Y + (pillH - knobSize) / 2f;

            using (Brush shBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            {
                g.FillEllipse(shBrush, knobX, knobY + 1f, knobSize, knobSize);
            }
            g.FillEllipse(Brushes.White, knobX, knobY, knobSize, knobSize);

            if (!string.IsNullOrEmpty(this.Text))
            {
                Color txtCol = IsDarkTheme ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
                Rectangle textRect = new Rectangle(pillRect.Right + 8, 0, Math.Max(10, this.Width - pillRect.Right - 8), this.Height);
                TextRenderer.DrawText(g, this.Text, this.Font, textRect, txtCol, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }
    }

    /// <summary>
    /// macOS 现代桌面输入框包装器（7px 圆角 + 1px 发丝边框 + 3px 外发光焦点环，消除原生 TextBox 光标切边）
    /// </summary>
    public class ModernTextBox : Control
    {
        private TextBox _tb;
        private bool _focused = false;
        private bool _isDark = false;
        private bool _multiline = false;
        private string _placeholder = "";

        public override string Text
        {
            get { return _tb != null ? _tb.Text : ""; }
            set { if (_tb != null) _tb.Text = value ?? ""; }
        }

        public bool IsDarkTheme
        {
            get { return _isDark; }
            set { _isDark = value; ApplyColors(); }
        }

        public bool Multiline
        {
            get { return _multiline; }
            set
            {
                _multiline = value;
                if (_tb != null)
                {
                    _tb.Multiline = value;
                    _tb.ScrollBars = value ? ScrollBars.Vertical : ScrollBars.None;
                    LayoutInnerBox();
                }
            }
        }

        public string PlaceholderText
        {
            get { return _placeholder; }
            set { _placeholder = value ?? ""; this.Invalidate(); }
        }

        public TextBox InnerTextBox { get { return _tb; } }

        public event EventHandler InnerTextChanged;

        public ModernTextBox()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.Size = new Size(240, 32);
            this.Font = WarmTheme.CreateFont(9.5F, FontStyle.Regular);

            _tb = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = this.Font
            };
            _tb.Enter += (s, e) => { _focused = true; this.Invalidate(); };
            _tb.Leave += (s, e) => { _focused = false; this.Invalidate(); };
            _tb.TextChanged += (s, e) => { this.Invalidate(); if (InnerTextChanged != null) InnerTextChanged(this, e); };
            this.Controls.Add(_tb);

            ApplyColors();
            LayoutInnerBox();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (_tb != null) { _tb.Font = this.Font; LayoutInnerBox(); }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutInnerBox();
        }

        private void LayoutInnerBox()
        {
            if (_tb == null) return;
            int padX = 10;
            if (_multiline)
            {
                _tb.Location = new Point(padX, 7);
                _tb.Size = new Size(Math.Max(20, this.Width - padX * 2), Math.Max(18, this.Height - 14));
            }
            else
            {
                int tbH = Math.Max(18, _tb.PreferredHeight);
                _tb.Location = new Point(padX, Math.Max(4, (this.Height - tbH) / 2));
                _tb.Width = Math.Max(20, this.Width - padX * 2);
            }
        }

        private void ApplyColors()
        {
            if (_tb == null) return;
            _tb.BackColor = _isDark ? WarmTheme.DarkSecondaryBtnBg : WarmTheme.LightCardBg;
            _tb.ForeColor = _isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
            this.Invalidate();
        }

        public void FocusInput()
        {
            if (_tb != null) { _tb.Focus(); _tb.SelectAll(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle boxRect = new Rectangle(2, 2, Math.Max(4, this.Width - 4), Math.Max(4, this.Height - 4));
            Color bgCol = _isDark ? WarmTheme.DarkSecondaryBtnBg : WarmTheme.LightCardBg;
            Color bdrCol = _focused
                ? WarmTheme.Orange
                : (_isDark ? WarmTheme.TopHighlightDark : WarmTheme.BorderSecondaryLight);

            if (_focused)
            {
                Rectangle ringRect = new Rectangle(boxRect.X - 2, boxRect.Y - 2, boxRect.Width + 4, boxRect.Height + 4);
                using (GraphicsPath rp = GlassRenderer.CreateRoundedPath(ringRect, WarmTheme.RadiusControl + 2))
                using (Pen rpen = new Pen(WarmTheme.ApplyAlpha(WarmTheme.Orange, 0.35f), 2.5f))
                {
                    g.DrawPath(rpen, rp);
                }
            }

            using (GraphicsPath bp = GlassRenderer.CreateRoundedPath(boxRect, WarmTheme.RadiusControl))
            {
                using (Brush b = new SolidBrush(bgCol)) g.FillPath(b, bp);
                using (Pen p = new Pen(bdrCol, 1)) g.DrawPath(p, bp);
            }
        }
    }

    /// <summary>
    /// macOS 原生感菜单渲染器（半透质感 + 1px 发丝边框 + 圆角选中项 + 右侧灰字对齐快捷键）
    /// </summary>
    public class ModernMenuItem : ToolStripMenuItem
    {
        public string ShortcutHint { get; set; }
        public bool IsDestructive { get; set; }

        public ModernMenuItem(string text, string shortcutHint, EventHandler onClick, bool isDestructive = false)
            : base(text, null, onClick)
        {
            ShortcutHint = shortcutHint ?? "";
            IsDestructive = isDestructive;
            this.Padding = new Padding(4, 3, 4, 3);
        }
    }

    public class ModernMenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly bool _isDark;
        public ModernMenuRenderer(bool isDark) { _isDark = isDark; }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            Color bg = _isDark ? Color.FromArgb(44, 44, 46) : Color.FromArgb(252, 252, 253);
            e.Graphics.Clear(bg);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            Color bdr = _isDark ? WarmTheme.HairlineDark : Color.FromArgb(30, 0, 0, 0);
            using (Pen p = new Pen(bdr, 1))
            {
                e.Graphics.DrawRectangle(p, 0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1);
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) { }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            Color lineCol = _isDark ? WarmTheme.HairlineDark : WarmTheme.HairlineLight;
            int y = e.Item.Height / 2;
            using (Pen p = new Pen(lineCol, 1))
            {
                e.Graphics.DrawLine(p, 8, y, e.Item.Width - 8, y);
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (!e.Item.Selected || !e.Item.Enabled) return;

            Rectangle r = new Rectangle(4, 1, e.Item.Width - 8, e.Item.Height - 2);
            ModernMenuItem mItem = e.Item as ModernMenuItem;
            Color selBg = (mItem != null && mItem.IsDestructive)
                ? Color.FromArgb(_isDark ? 55 : 28, WarmTheme.DestructiveRed)
                : WarmTheme.Orange;
            using (GraphicsPath path = GlassRenderer.CreateRoundedPath(r, 5))
            using (Brush b = new SolidBrush(selBg))
            {
                e.Graphics.FillPath(b, path);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            ModernMenuItem mItem = e.Item as ModernMenuItem;
            bool selected = e.Item.Selected && e.Item.Enabled;
            bool destructive = (mItem != null && mItem.IsDestructive);

            Color txtCol;
            if (destructive)
                txtCol = WarmTheme.DestructiveRed;
            else if (selected)
                txtCol = Color.White;
            else
                txtCol = _isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;

            Font f = WarmTheme.CreateFont(9F, FontStyle.Regular);
            Rectangle textR = new Rectangle(12, 0, e.Item.Width - 62, e.Item.Height);
            TextRenderer.DrawText(e.Graphics, e.Item.Text, f, textR, txtCol, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);

            if (mItem != null && !string.IsNullOrEmpty(mItem.ShortcutHint))
            {
                Color keyCol = (selected && !destructive)
                    ? Color.FromArgb(220, 255, 255, 255)
                    : (_isDark ? WarmTheme.DarkTextMuted : WarmTheme.LightTextMuted);
                using (Font kf = WarmTheme.CreateFont(8F, FontStyle.Regular))
                {
                    Rectangle keyR = new Rectangle(e.Item.Width - 68, 0, 56, e.Item.Height);
                    TextRenderer.DrawText(e.Graphics, mItem.ShortcutHint, kf, keyR, keyCol, TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                }
            }
            f.Dispose();
        }
    }

    public static class ModernMenuHelper
    {
        public static ContextMenuStrip CreateMenu(bool isDark)
        {
            ContextMenuStrip cms = new ContextMenuStrip();
            cms.ShowImageMargin = false;
            cms.DropShadowEnabled = true;
            cms.Renderer = new ModernMenuRenderer(isDark);
            cms.Font = WarmTheme.CreateFont(9F, FontStyle.Regular);
            return cms;
        }

        public static void AddItem(ContextMenuStrip cms, string text, string shortcutHint, Action onClick, bool isDestructive)
        {
            ModernMenuItem item = new ModernMenuItem(text, shortcutHint, (s, e) => { if (onClick != null) onClick(); }, isDestructive);
            item.AutoSize = false;
            item.Size = new Size(176, 28);
            cms.Items.Add(item);
        }

        public static void AddSeparator(ContextMenuStrip cms)
        {
            cms.Items.Add(new ToolStripSeparator());
        }
    }

    /// <summary>
    /// macOS 左侧导航侧边栏（宽屏 >=480px 自动展示：#F5F5F7 / #1E1E1E 微透灰底 + 圆角胶囊高亮块 + 数量徽章）
    /// </summary>
    public class SidebarCategoryPanel : Panel
    {
        public List<string> Categories { get; private set; }
        public Dictionary<string, int> CategoryCounts { get; private set; }
        public string ActiveCategory { get; private set; }
        public bool IsDarkTheme { get; set; }

        public event Action<string> OnCategorySelected;
        public event Action OnAddCategoryRequested;
        public event Action OnDeleteCurrentCategoryRequested;
        public event Action<string> OnRenameCategoryRequested;
        public event Action<string> OnDeleteCategoryRequested;

        private int _hoverIdx = -1;
        private bool _hoverAdd = false;
        private bool _hoverDel = false;
        private int _scrollY = 0;

        public SidebarCategoryPanel()
        {
            Categories = new List<string>();
            CategoryCounts = new Dictionary<string, int>();
            ActiveCategory = "全部";
            this.Width = 152;
            this.Cursor = Cursors.Hand;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);

            this.MouseMove += (s, e) =>
            {
                int prev = _hoverIdx;
                bool pAdd = _hoverAdd, pDel = _hoverDel;
                HitTest(e.Location, out _hoverIdx, out _hoverAdd, out _hoverDel);
                if (prev != _hoverIdx || pAdd != _hoverAdd || pDel != _hoverDel) this.Invalidate();
            };
            this.MouseLeave += (s, e) => { _hoverIdx = -1; _hoverAdd = false; _hoverDel = false; this.Invalidate(); };
            this.MouseWheel += (s, e) =>
            {
                _scrollY = Math.Max(0, _scrollY - (e.Delta > 0 ? 28 : -28));
                ClampScroll();
                this.Invalidate();
            };
            this.MouseDown += (s, e) =>
            {
                int idx; bool hAdd, hDel;
                HitTest(e.Location, out idx, out hAdd, out hDel);
                if (e.Button == MouseButtons.Left && hAdd) { if (OnAddCategoryRequested != null) OnAddCategoryRequested(); return; }
                if (e.Button == MouseButtons.Left && hDel) { if (OnDeleteCurrentCategoryRequested != null) OnDeleteCurrentCategoryRequested(); return; }
                if (idx >= 0 && idx < Categories.Count)
                {
                    string cat = Categories[idx];
                    if (e.Button == MouseButtons.Left)
                    {
                        ActiveCategory = cat;
                        this.Invalidate();
                        if (OnCategorySelected != null) OnCategorySelected(cat);
                    }
                    else if (e.Button == MouseButtons.Right && cat != "全部")
                    {
                        ContextMenuStrip cms = ModernMenuHelper.CreateMenu(IsDarkTheme);
                        ModernMenuHelper.AddItem(cms, "修改分类名称", "F2", () => { if (OnRenameCategoryRequested != null) OnRenameCategoryRequested(cat); }, false);
                        ModernMenuHelper.AddSeparator(cms);
                        ModernMenuHelper.AddItem(cms, "删除此分类", "Del", () => { if (OnDeleteCategoryRequested != null) OnDeleteCategoryRequested(cat); }, true);
                        cms.Show(this, e.Location);
                    }
                }
            };
        }

        public void SetData(List<string> cats, string active, Dictionary<string, int> counts)
        {
            Categories = new List<string> { "全部" };
            if (cats != null) Categories.AddRange(cats);
            ActiveCategory = active ?? "全部";
            CategoryCounts = counts ?? new Dictionary<string, int>();
            ClampScroll();
            this.Invalidate();
        }

        private void ClampScroll()
        {
            int contentH = 26 + Categories.Count * 30;
            int viewH = Math.Max(40, this.Height - 40);
            int maxS = Math.Max(0, contentH - viewH);
            if (_scrollY > maxS) _scrollY = maxS;
            if (_scrollY < 0) _scrollY = 0;
        }

        private void HitTest(Point pt, out int hitIdx, out bool hitAdd, out bool hitDel)
        {
            hitIdx = -1;
            hitAdd = false;
            hitDel = false;
            Rectangle addR = new Rectangle(8, this.Height - 34, (this.Width - 22) / 2, 26);
            Rectangle delR = new Rectangle(addR.Right + 6, this.Height - 34, (this.Width - 22) / 2, 26);
            if (addR.Contains(pt)) { hitAdd = true; return; }
            if (delR.Contains(pt)) { hitDel = true; return; }

            if (pt.Y >= 24 && pt.Y < this.Height - 40)
            {
                int idx = (pt.Y - 26 + _scrollY) / 30;
                if (idx >= 0 && idx < Categories.Count) hitIdx = idx;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color sbBg = IsDarkTheme ? WarmTheme.DarkSidebarBg : WarmTheme.LightSidebarBg;
            Color lineCol = IsDarkTheme ? WarmTheme.HairlineDark : WarmTheme.HairlineLight;
            g.Clear(sbBg);

            // 右侧 1px 发丝线
            using (Pen p = new Pen(lineCol, 1))
            {
                g.DrawLine(p, this.Width - 1, 0, this.Width - 1, this.Height);
            }

            // 分组微标
            using (Font hf = WarmTheme.CreateFont(7.8F, FontStyle.Bold))
            {
                Color hdrCol = IsDarkTheme ? WarmTheme.DarkTextMuted : WarmTheme.LightTextMuted;
                TextRenderer.DrawText(g, "分类导览", hf, new Rectangle(12, 6, this.Width - 20, 18), hdrCol, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }

            // 列表项区
            int listBottom = this.Height - 40;
            g.SetClip(new Rectangle(0, 24, this.Width - 1, Math.Max(10, listBottom - 24)));

            using (Font fReg = WarmTheme.CreateFont(9F, FontStyle.Regular))
            using (Font fBold = WarmTheme.CreateFont(9F, FontStyle.Bold))
            using (Font fBadge = WarmTheme.CreateFont(8F, FontStyle.Regular))
            {
                for (int i = 0; i < Categories.Count; i++)
                {
                    string cat = Categories[i];
                    int y = 26 + i * 30 - _scrollY;
                    if (y + 28 < 24 || y > listBottom) continue;

                    bool isActive = (cat == ActiveCategory);
                    bool isHover = (i == _hoverIdx);
                    Rectangle itemRect = new Rectangle(8, y, this.Width - 17, 26);

                    if (isActive)
                    {
                        Color pillCol = IsDarkTheme ? WarmTheme.DarkRowSelected : Color.FromArgb(24, WarmTheme.Orange);
                        if (WarmTheme.Current.IsPink && !IsDarkTheme) pillCol = Color.FromArgb(26, WarmTheme.Orange);
                        using (GraphicsPath gp = GlassRenderer.CreateRoundedPath(itemRect, WarmTheme.RadiusControl))
                        using (Brush b = new SolidBrush(pillCol))
                        {
                            g.FillPath(b, gp);
                        }
                    }
                    else if (isHover)
                    {
                        Color hCol = IsDarkTheme ? WarmTheme.GhostHoverDark : WarmTheme.GhostHoverLight;
                        using (GraphicsPath gp = GlassRenderer.CreateRoundedPath(itemRect, WarmTheme.RadiusControl))
                        using (Brush b = new SolidBrush(hCol))
                        {
                            g.FillPath(b, gp);
                        }
                    }

                    Color txtCol = isActive
                        ? WarmTheme.Orange
                        : (IsDarkTheme ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary);
                    Font drawFont = isActive ? fBold : fReg;

                    int cnt = 0;
                    CategoryCounts.TryGetValue(cat, out cnt);
                    Rectangle badgeRect = new Rectangle(itemRect.Right - 34, itemRect.Y, 28, itemRect.Height);
                    Color badgeCol = isActive ? WarmTheme.Orange : (IsDarkTheme ? WarmTheme.DarkTextMuted : WarmTheme.LightTextMuted);
                    TextRenderer.DrawText(g, cnt.ToString(), fBadge, badgeRect, badgeCol, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

                    Rectangle labelRect = new Rectangle(itemRect.X + 8, itemRect.Y, Math.Max(20, itemRect.Width - 40), itemRect.Height);
                    TextRenderer.DrawText(g, cat, drawFont, labelRect, txtCol, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
                }
            }
            g.ResetClip();

            // 底部发丝分隔与新建/删除按钮
            using (Pen p = new Pen(lineCol, 1))
            {
                g.DrawLine(p, 8, this.Height - 40, this.Width - 9, this.Height - 40);
            }

            Rectangle addR = new Rectangle(8, this.Height - 34, (this.Width - 22) / 2, 26);
            Rectangle delR = new Rectangle(addR.Right + 6, this.Height - 34, (this.Width - 22) / 2, 26);
            Color iconCol = IsDarkTheme ? WarmTheme.DarkTextSecondary : WarmTheme.LightChipText;

            GlassRenderer.DrawModernButtonSurface(g, addR, ButtonVariant.Ghost, WarmTheme.Orange, IsDarkTheme, _hoverAdd ? 1f : 0f, false, false, true, 6);
            GlassRenderer.DrawVectorPlusIcon(g, addR, _hoverAdd ? WarmTheme.Orange : iconCol);

            Color delBg = Color.FromArgb(IsDarkTheme ? 48 : 26, WarmTheme.DestructiveRed);
            GlassRenderer.DrawModernButtonSurface(g, delR, ButtonVariant.Ghost, WarmTheme.DestructiveRed, IsDarkTheme, _hoverDel ? 1f : 0f, false, false, true, 6, delBg);
            GlassRenderer.DrawVectorMinusIcon(g, delR, _hoverDel ? WarmTheme.DestructiveRed : iconCol);
        }
    }

    /// <summary>
    /// 自绘制精美置顶复选框（消除深色模式下原生白色方框）
    /// </summary>
    public class WarmCheckBox : Control
    {
        private bool _checked = true;
        private bool _hovered = false;

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    this.Invalidate();
                    if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public bool IsDarkTheme { get; set; }
        public event EventHandler CheckedChanged;

        public WarmCheckBox()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
            this.Size = new Size(48, 22);

            this.MouseEnter += (s, e) => { _hovered = true; this.Invalidate(); };
            this.MouseLeave += (s, e) => { _hovered = false; this.Invalidate(); };
            this.Click += (s, e) => { Checked = !Checked; };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle boxRect = new Rectangle(1, (this.Height - 14) / 2, 14, 14);
            using (GraphicsPath path = GlassRenderer.CreateRoundedPath(boxRect, 3))
            {
                if (_checked)
                {
                    Color fillCol = _hovered ? WarmTheme.OrangeHover : WarmTheme.Orange;
                    using (Brush b = new SolidBrush(fillCol)) g.FillPath(b, path);
                    using (Pen checkPen = new Pen(Color.White, 1.6f))
                    {
                        checkPen.StartCap = LineCap.Round;
                        checkPen.EndCap = LineCap.Round;
                        checkPen.LineJoin = LineJoin.Round;
                        g.DrawLines(checkPen, new PointF[]
                        {
                            new PointF(boxRect.X + 3.2f, boxRect.Y + 7.2f),
                            new PointF(boxRect.X + 6.0f, boxRect.Y + 10.0f),
                            new PointF(boxRect.X + 10.8f, boxRect.Y + 4.2f)
                        });
                    }
                }
                else
                {
                    Color bgCol = IsDarkTheme ? WarmTheme.DarkPaperBg : WarmTheme.LightCardBg;
                    Color bdrCol = _hovered
                        ? WarmTheme.Orange
                        : (IsDarkTheme ? WarmTheme.DarkDivider : WarmTheme.LightBorder);
                    using (Brush b = new SolidBrush(bgCol)) g.FillPath(b, path);
                    using (Pen p = new Pen(bdrCol, 1)) g.DrawPath(p, path);
                }
            }

            Color textCol = IsDarkTheme ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
            Rectangle textRect = new Rectangle(18, 0, this.Width - 18, this.Height);
            TextRenderer.DrawText(g, this.Text, this.Font, textRect, textCol, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }

    /// <summary>
    /// 圆形头像绘制控件（避免浅灰方底在深色模式下产生白/黑边块）
    /// </summary>
    public class CircleAvatarControl : Control
    {
        public Image AvatarImage { get; set; }
        public bool IsDarkTheme { get; set; }

        public CircleAvatarControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
            this.Size = new Size(24, 24);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle r = new Rectangle(1, 1, this.Width - 3, this.Height - 3);
            if (AvatarImage != null)
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                using (GraphicsPath clip = new GraphicsPath())
                {
                    clip.AddEllipse(r);
                    g.SetClip(clip);
                    g.DrawImage(AvatarImage, r);
                    g.ResetClip();
                }
            }

            Color ringCol = WarmTheme.Current.IsPink
                ? WarmTheme.Orange
                : (IsDarkTheme ? WarmTheme.DarkDivider : WarmTheme.LightBorder);
            float ringW = WarmTheme.Current.IsPink ? 1.8f : 1.0f;
            using (Pen ringPen = new Pen(ringCol, ringW))
            {
                g.DrawEllipse(ringPen, r);
            }
        }
    }

    /// <summary>
    /// 通用 macOS × iOS 现代桌面按钮组件
    /// 支持 4 层级 (Primary / Secondary / Ghost / Destructive)、120ms 悬停平滑插值、按压下沉、Tab 焦点外发光环、禁用降透(0.4)与定宽加载指示器
    /// </summary>
    public class ModernButton : Control
    {
        private bool _isHovered = false;
        private bool _isPressed = false;
        private float _hoverProgress = 0f;
        private bool _isLoading = false;
        private float _spinnerAngle = 0f;
        private System.Windows.Forms.Timer _animTimer;

        public ButtonVariant Variant { get; set; }
        public bool IsDarkTheme { get; set; }
        public Color AccentColor { get; set; }
        public Color TextColor { get; set; }
        public Color? CustomHoverBg { get; set; }
        public Color? CustomHoverTextColor { get; set; }
        public int CornerRadius { get; set; }
        public int IconSize { get; set; }
        public int IconTextGap { get; set; }
        public Action<Graphics, Rectangle, Color> IconPainter { get; set; }
        public DialogResult DialogResult { get; set; }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    EnsureAnimRunning();
                    this.Invalidate();
                }
            }
        }

        // 兼容属性别名（便于渐进迁移弹窗代码）
        public bool UseLiquidGlass { get; set; }
        public bool IsPrimaryGlass
        {
            get { return Variant == ButtonVariant.Primary; }
            set { Variant = value ? ButtonVariant.Primary : ButtonVariant.Secondary; }
        }
        public Color BaseNormal
        {
            get { return AccentColor; }
            set { if (Variant == ButtonVariant.Primary) AccentColor = value; }
        }
        public Color BaseHover { get; set; }
        public Color BasePressed { get; set; }
        public Color BorderColor { get; set; }
        public bool IsGearIcon
        {
            get { return IconPainter != null; }
            set { if (value) IconPainter = GlassRenderer.DrawVectorGearIcon; }
        }

        public ModernButton()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.SupportsTransparentBackColor |
                          ControlStyles.Selectable, true);

            this.TabStop = true;
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
            this.Font = WarmTheme.CreateFont(9.5F, FontStyle.Regular);
            Variant = ButtonVariant.Secondary;
            AccentColor = WarmTheme.Orange;
            TextColor = WarmTheme.LightTextPrimary;
            CornerRadius = WarmTheme.RadiusControl; // 默认桌面级 7px 圆角
            IconSize = 15;                          // 统一 14px-16px 图标
            IconTextGap = 6;                        // 规范图文间距 6px
            DialogResult = DialogResult.None;

            this.MouseEnter += (s, e) => { _isHovered = true; EnsureAnimRunning(); };
            this.MouseLeave += (s, e) => { _isHovered = false; _isPressed = false; EnsureAnimRunning(); };
            this.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && this.Enabled && !_isLoading)
                {
                    _isPressed = true;
                    this.Invalidate();
                }
            };
            this.MouseUp += (s, e) => { _isPressed = false; this.Invalidate(); };
            this.GotFocus += (s, e) => this.Invalidate();
            this.LostFocus += (s, e) => { _isPressed = false; this.Invalidate(); };
        }

        private void EnsureAnimRunning()
        {
            if (_animTimer == null)
            {
                _animTimer = new System.Windows.Forms.Timer { Interval = 16 };
                _animTimer.Tick += (s, e) =>
                {
                    bool keepGoing = false;
                    // 120ms ease-out 悬停进度插值 (16ms/frame -> ~0.14 step)
                    float target = _isHovered ? 1f : 0f;
                    float diff = target - _hoverProgress;
                    if (Math.Abs(diff) > 0.02f)
                    {
                        _hoverProgress += diff * 0.35f;
                        keepGoing = true;
                    }
                    else
                    {
                        _hoverProgress = target;
                    }

                    if (_isLoading)
                    {
                        _spinnerAngle = (_spinnerAngle + 18f) % 360f;
                        keepGoing = true;
                    }

                    this.Invalidate();
                    if (!keepGoing) _animTimer.Stop();
                };
            }
            if (!_animTimer.Enabled) _animTimer.Start();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if ((e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) && this.Enabled && !_isLoading)
            {
                _isPressed = true;
                this.Invalidate();
                e.Handled = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if ((e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) && _isPressed)
            {
                _isPressed = false;
                this.Invalidate();
                if (this.Enabled && !_isLoading) this.OnClick(EventArgs.Empty);
                e.Handled = true;
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if (!this.Enabled || _isLoading) return;
            base.OnClick(e);
            if (DialogResult != DialogResult.None)
            {
                Form parent = this.FindForm();
                if (parent != null) parent.DialogResult = DialogResult;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _animTimer != null)
            {
                _animTimer.Stop();
                _animTimer.Dispose();
                _animTimer = null;
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // 预留 2px 外边距以确保 Tab 焦点外发光环完整绘制不被剪裁
            Rectangle bodyRect = new Rectangle(2, 2, Math.Max(4, this.Width - 4), Math.Max(4, this.Height - 4));
            bool showFocus = this.Focused && this.ShowFocusCues;
            float easedHover = MotionHelper.EaseApple(_hoverProgress);

            GlassRenderer.DrawModernButtonSurface(
                g, bodyRect, Variant, AccentColor, IsDarkTheme,
                easedHover, _isPressed, showFocus, this.Enabled, CornerRadius, CustomHoverBg);

            Color baseFg;
            if (Variant == ButtonVariant.Primary) baseFg = Color.White;
            else if (Variant == ButtonVariant.Destructive) baseFg = WarmTheme.DestructiveRed;
            else baseFg = TextColor;

            Color fgColor = (CustomHoverTextColor.HasValue && _hoverProgress > 0.01f)
                ? WarmTheme.BlendColor(baseFg, CustomHoverTextColor.Value, easedHover)
                : baseFg;

            if (!this.Enabled) fgColor = WarmTheme.ApplyAlpha(fgColor, 0.4f);

            Rectangle contentRect = (_isPressed && this.Enabled && !_isLoading)
                ? new Rectangle(bodyRect.X + 1, bodyRect.Y + 1, bodyRect.Width - 2, bodyRect.Height - 2)
                : bodyRect;

            if (_isLoading)
            {
                GlassRenderer.DrawVectorSpinner(g, contentRect, fgColor, _spinnerAngle);
                return;
            }

            bool hasIcon = (IconPainter != null);
            bool hasText = !string.IsNullOrEmpty(this.Text);

            if (hasIcon && !hasText)
            {
                Rectangle ir = new Rectangle(
                    contentRect.X + (contentRect.Width - IconSize) / 2,
                    contentRect.Y + (contentRect.Height - IconSize) / 2,
                    IconSize, IconSize);
                IconPainter(g, ir, fgColor);
            }
            else if (hasIcon && hasText)
            {
                Size txtSize = TextRenderer.MeasureText(g, this.Text, this.Font, new Size(int.MaxValue, contentRect.Height), TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                int totalW = IconSize + IconTextGap + txtSize.Width;
                int startX = contentRect.X + Math.Max(4, (contentRect.Width - totalW) / 2);
                Rectangle ir = new Rectangle(startX, contentRect.Y + (contentRect.Height - IconSize) / 2, IconSize, IconSize);
                IconPainter(g, ir, fgColor);
                Rectangle tr = new Rectangle(startX + IconSize + IconTextGap, contentRect.Y, Math.Max(10, contentRect.Right - (startX + IconSize + IconTextGap) - 2), contentRect.Height);
                TextRenderer.DrawText(g, this.Text, this.Font, tr, fgColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
            else if (hasText)
            {
                TextRenderer.DrawText(g, this.Text, this.Font, contentRect, fgColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }
    }

    /// <summary>
    /// 兼容包装类：沿用新 ModernButton 渲染管线，确保弹窗迁移过渡期零断层
    /// </summary>
    public class LiquidGlassButtonControl : ModernButton
    {
        public LiquidGlassButtonControl()
        {
            Variant = ButtonVariant.Secondary;
            CornerRadius = WarmTheme.RadiusControl;
        }
    }

    public class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const byte VK_CONTROL = 0x11;
        private const byte VK_V = 0x56;
        private const byte VK_RETURN = 0x0D;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 2;
        private const int WM_NCHITTEST = 0x0084;

        private IntPtr _lastTargetHwnd = IntPtr.Zero;
        private string _lastTargetTitle = "";

        // UI 控件
        private System.Windows.Forms.Timer _windowTrackerTimer;
        private GlassTopBarPanel _topBar;
        private CircleAvatarControl _picAvatar;
        private Label _lblLogo;
        private ModernButton _btnSidebar;
        private ModernButton _btnPin;
        private ModernButton _btnSettings;
        private ModernButton _btnWinMin;
        private ModernButton _btnWinMax;
        private ModernButton _btnWinClose;
        private ModernButton _btnAddPhrase;
        private Panel _searchWrap;
        private TextBox _searchBox;
        private bool _searchFocused = false;
        private const string SearchPlaceholder = "搜话术、分类…";
        private ModeSwitchBar _modeBar;
        private SidebarCategoryPanel _sidebarPanel;
        private CustomCategoryBar _categoryBar;
        private MiniPhraseListView _phraseList;
        private Panel _statusBar;
        private Label _lblTarget;
        private int _compactSavedWidth = 336;
        private bool _sidebarManualHidden = false;
        private bool _sidebarExpanded = false;

        // 数据
        private AppData _data = new AppData();
        private string _currentTextCat = "全部";
        private string _currentVideoCat = "全部";
        private string _currentMode = "text"; // "text" 或 "video"

        private string ActiveCat
        {
            get { return _currentMode == "video" ? _currentVideoCat : _currentTextCat; }
            set { if (_currentMode == "video") _currentVideoCat = value; else _currentTextCat = value; }
        }

        private List<string> ActiveCategories()
        {
            return (_currentMode == "video") ? _data.VideoCategories : _data.Categories;
        }
        private readonly string _storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "phrases.json");
        private readonly string _iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
        private readonly string _avatarPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "avatar.png");
        private readonly string _imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
        private readonly string _videosDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "videos");
        private Image _customBgImg = null;

        public MainForm()
        {
            InitializeUi();
            LoadData();
            RestoreWindowBounds();
            ApplySettings();
            RefreshCategoryViews();
            RefreshList();
            StartTracking();

            this.ResizeEnd += (s, e) => SaveWindowBounds();
            this.FormClosing += (s, e) => SaveWindowBounds();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // 保留任务栏最小化/还原原生交互能力
                cp.Style |= 0x00020000; // WS_MINIMIZEBOX
                cp.Style |= 0x00080000; // WS_SYSMENU
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST && this.WindowState == FormWindowState.Normal)
            {
                Point pt = this.PointToClient(new Point(m.LParam.ToInt32()));
                int grip = 6;
                bool left = pt.X <= grip;
                bool right = pt.X >= this.ClientSize.Width - grip;
                bool top = pt.Y <= grip;
                bool bottom = pt.Y >= this.ClientSize.Height - grip;

                if (top && left) m.Result = (IntPtr)13;       // HTTOPLEFT
                else if (top && right) m.Result = (IntPtr)14; // HTTOPRIGHT
                else if (bottom && left) m.Result = (IntPtr)16; // HTBOTTOMLEFT
                else if (bottom && right) m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                else if (left) m.Result = (IntPtr)10;         // HTLEFT
                else if (right) m.Result = (IntPtr)11;        // HTRIGHT
                else if (top) m.Result = (IntPtr)12;          // HTTOP
                else if (bottom) m.Result = (IntPtr)15;       // HTBOTTOM
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.F))
            {
                _searchBox.Focus();
                if (_searchBox.Text == SearchPlaceholder) _searchBox.Text = "";
                _searchBox.SelectAll();
                return true;
            }
            if (keyData == (Keys.Control | Keys.N))
            {
                ShowAddOrEditDialog(null);
                return true;
            }
            if (keyData == (Keys.Control | Keys.Oemcomma))
            {
                OpenSettingsDialog();
                return true;
            }
            if (keyData == (Keys.Control | Keys.T))
            {
                ToggleTopMostState();
                return true;
            }
            if (keyData == (Keys.Control | Keys.B))
            {
                ToggleSidebarLayout();
                return true;
            }
            if (keyData == (Keys.Control | Keys.D1))
            {
                SwitchMode("text");
                return true;
            }
            if (keyData == (Keys.Control | Keys.D2))
            {
                SwitchMode("video");
                return true;
            }
            if (keyData == Keys.F2 && !_searchBox.Focused && ActiveCat != "全部")
            {
                RenameCategoryPrompt(ActiveCat);
                return true;
            }
            if (keyData == Keys.Escape && _searchBox.Focused)
            {
                _searchBox.Text = "";
                _phraseList.Focus();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void BeginDragWindow(Control ctl)
        {
            ctl.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (e.Clicks == 2)
                    {
                        ToggleMaximizeRestore();
                    }
                    else
                    {
                        ReleaseCapture();
                        SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                    }
                }
            };
        }

        private void ToggleMaximizeRestore()
        {
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
            if (this.WindowState == FormWindowState.Maximized)
                this.WindowState = FormWindowState.Normal;
            else
                this.WindowState = FormWindowState.Maximized;
            UpdateWindowChromeButtons();
        }

        private void ToggleTopMostState()
        {
            this.TopMost = !this.TopMost;
            UpdatePinButtonAppearance();
        }

        private void ToggleSidebarLayout()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                _sidebarManualHidden = !_sidebarManualHidden;
                UpdateResponsiveLayout();
                return;
            }

            if (!_sidebarExpanded)
            {
                _sidebarManualHidden = false;
                if (this.Width < 460)
                {
                    _compactSavedWidth = this.Width;
                    Rectangle wa = Screen.FromHandle(this.Handle).WorkingArea;
                    int targetW = Math.Min(500, wa.Width - 20);
                    int rightEdge = this.Right;
                    int newLeft = Math.Max(wa.Left, Math.Min(this.Left, rightEdge - targetW));
                    if (newLeft + targetW > wa.Right) newLeft = Math.Max(wa.Left, wa.Right - targetW);
                    this.Bounds = new Rectangle(newLeft, this.Top, targetW, this.Height);
                }
                else
                {
                    UpdateResponsiveLayout();
                }
            }
            else
            {
                if (this.Width >= 460 && _compactSavedWidth < 460)
                {
                    Rectangle wa = Screen.FromHandle(this.Handle).WorkingArea;
                    int targetW = Math.Max(this.MinimumSize.Width, _compactSavedWidth);
                    int newLeft = Math.Min(wa.Right - targetW, this.Right - targetW);
                    this.Bounds = new Rectangle(Math.Max(wa.Left, newLeft), this.Top, targetW, this.Height);
                }
                else
                {
                    _sidebarManualHidden = true;
                    UpdateResponsiveLayout();
                }
            }
            SaveWindowBounds();
        }

        private void SwitchMode(string mode)
        {
            if (_currentMode == mode) return;
            _modeBar.ActiveMode = mode;
            ApplyModeChange(mode);
        }

        private void ApplyModeChange(string mode)
        {
            _currentMode = mode;
            _phraseList.CurrentMode = mode;
            if (mode == "video")
            {
                _btnAddPhrase.Text = "视频";
                _btnAddPhrase.Width = 64;
            }
            else
            {
                _btnAddPhrase.Text = "词条";
                _btnAddPhrase.Width = 64;
            }
            UpdateResponsiveLayout();
            RefreshCategoryViews();
            RefreshList();
        }

        private void RefreshCategoryViews()
        {
            var cats = ActiveCategories();
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            int total = 0;
            foreach (var p in _data.Phrases)
            {
                string t = string.IsNullOrEmpty(p.ItemType) ? "text" : p.ItemType;
                if (t != _currentMode) continue;
                total++;
                string c = string.IsNullOrEmpty(p.Category) ? "未分类" : p.Category;
                int cur;
                counts.TryGetValue(c, out cur);
                counts[c] = cur + 1;
            }
            counts["全部"] = total;

            _categoryBar.SetCategories(cats, ActiveCat);
            _sidebarPanel.SetData(cats, ActiveCat, counts);
        }

        private bool IsDarkModeActive()
        {
            return (_data != null && _data.Settings != null && _data.Settings.ThemeType == "black");
        }

        /// <summary>
        /// 统一现代无边框弹窗底座（DWM 原生圆角 + 1px 发丝边框 + 可拖拽标题栏 + ESC 退出）
        /// </summary>
        private void PrepareModernDialog(Form dlg, string title, int clientW, int clientH, out Panel titleBar, out ModernButton closeBtn)
        {
            bool isDark = IsDarkModeActive();
            Color bg = isDark ? WarmTheme.DarkPaperBg : WarmTheme.LightPaperBg;
            Color bdr = isDark ? WarmTheme.HairlineDark : WarmTheme.HairlineLight;
            Color txt = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;

            dlg.Text = title;
            dlg.FormBorderStyle = FormBorderStyle.None;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ClientSize = new Size(clientW, clientH);
            dlg.BackColor = bg;
            dlg.ForeColor = txt;
            dlg.Font = WarmTheme.CreateFont(9.5F, FontStyle.Regular);
            dlg.KeyPreview = true;
            if (this.Icon != null) dlg.Icon = this.Icon;

            dlg.Paint += (s, e) =>
            {
                using (Pen p = new Pen(isDark ? Color.FromArgb(62, 62, 66) : Color.FromArgb(208, 205, 200), 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, dlg.ClientSize.Width - 1, dlg.ClientSize.Height - 1);
                }
            };

            Panel hdr = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38,
                BackColor = isDark ? WarmTheme.DarkCardBg : WarmTheme.LightCardBg
            };
            hdr.Paint += (s, e) =>
            {
                using (Pen p = new Pen(bdr, 1))
                {
                    e.Graphics.DrawLine(p, 0, hdr.Height - 1, hdr.Width, hdr.Height - 1);
                }
            };

            Label lblTitle = new Label
            {
                Text = title,
                Location = new Point(14, 9),
                AutoSize = true,
                Font = WarmTheme.CreateFont(10F, FontStyle.Bold),
                ForeColor = txt,
                BackColor = Color.Transparent
            };

            ModernButton btnX = new ModernButton
            {
                Size = new Size(28, 26),
                Location = new Point(clientW - 34, 6),
                Variant = ButtonVariant.Ghost,
                IsDarkTheme = isDark,
                CornerRadius = 6,
                IconSize = 12,
                TextColor = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary,
                CustomHoverBg = WarmTheme.DestructiveRed,
                CustomHoverTextColor = Color.White,
                IconPainter = GlassRenderer.DrawVectorWinCloseIcon,
                TabStop = false
            };
            btnX.Click += (s, e) => { dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };

            MouseEventHandler drag = (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(dlg.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            };
            hdr.MouseDown += drag;
            lblTitle.MouseDown += drag;

            hdr.Controls.AddRange(new Control[] { lblTitle, btnX });
            dlg.Controls.Add(hdr);

            dlg.Load += (s, e) => Win32WindowStyler.ApplyModernWindowStyle(dlg, isDark);
            dlg.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    dlg.DialogResult = DialogResult.Cancel;
                    dlg.Close();
                }
            };

            titleBar = hdr;
            closeBtn = btnX;
        }

        private bool ShowModernConfirm(string title, string message, bool isDestructive, string confirmText = "确认")
        {
            bool isDark = IsDarkModeActive();
            Color txtSec = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary;

            using (Form dlg = new Form())
            {
                Panel hdr; ModernButton xBtn;
                PrepareModernDialog(dlg, title, 368, 194, out hdr, out xBtn);

                Label lblMsg = new Label
                {
                    Text = message,
                    Location = new Point(20, 52),
                    MaximumSize = new Size(328, 0),
                    AutoSize = true,
                    Font = WarmTheme.CreateFont(9.5F, FontStyle.Regular),
                    ForeColor = txtSec
                };

                ModernButton btnConfirm = new ModernButton
                {
                    Text = confirmText,
                    Size = new Size(88, WarmTheme.BtnHeightDialog),
                    Variant = isDestructive ? ButtonVariant.Destructive : ButtonVariant.Primary,
                    IsDarkTheme = isDark,
                    AccentColor = WarmTheme.Orange,
                    CornerRadius = WarmTheme.RadiusDialogBtn,
                    Font = WarmTheme.CreateFont(9.5F, FontStyle.Bold),
                    DialogResult = DialogResult.OK
                };
                if (isDestructive)
                {
                    btnConfirm.CustomHoverBg = Color.FromArgb(isDark ? 65 : 36, WarmTheme.DestructiveRed);
                }

                ModernButton btnCancel = new ModernButton
                {
                    Text = "取消",
                    Size = new Size(76, WarmTheme.BtnHeightDialog),
                    Variant = ButtonVariant.Secondary,
                    IsDarkTheme = isDark,
                    TextColor = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary,
                    CornerRadius = WarmTheme.RadiusDialogBtn,
                    DialogResult = DialogResult.Cancel
                };

                dlg.Controls.AddRange(new Control[] { lblMsg, btnConfirm, btnCancel });
                dlg.Load += (s, e) =>
                {
                    int bodyBottom = Math.Max(110, lblMsg.Bottom + 18);
                    btnCancel.Location = new Point(dlg.ClientSize.Width - btnCancel.Width - 16, bodyBottom);
                    btnConfirm.Location = new Point(btnCancel.Left - btnConfirm.Width - 8, bodyBottom);
                    dlg.ClientSize = new Size(368, btnCancel.Bottom + 14);
                    xBtn.Left = dlg.ClientSize.Width - 34;
                };
                dlg.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        dlg.DialogResult = DialogResult.OK;
                        dlg.Close();
                    }
                };

                return dlg.ShowDialog(this) == DialogResult.OK;
            }
        }

        private void ShowModernAlert(string title, string message)
        {
            bool isDark = IsDarkModeActive();
            Color txtSec = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary;

            using (Form dlg = new Form())
            {
                Panel hdr; ModernButton xBtn;
                PrepareModernDialog(dlg, title, 368, 190, out hdr, out xBtn);

                Label lblMsg = new Label
                {
                    Text = message,
                    Location = new Point(20, 52),
                    MaximumSize = new Size(328, 0),
                    AutoSize = true,
                    Font = WarmTheme.CreateFont(9.5F, FontStyle.Regular),
                    ForeColor = txtSec
                };

                ModernButton btnOk = new ModernButton
                {
                    Text = "知道了",
                    Size = new Size(92, WarmTheme.BtnHeightDialog),
                    Variant = ButtonVariant.Primary,
                    IsDarkTheme = isDark,
                    AccentColor = WarmTheme.Orange,
                    CornerRadius = WarmTheme.RadiusDialogBtn,
                    Font = WarmTheme.CreateFont(9.5F, FontStyle.Bold),
                    DialogResult = DialogResult.OK
                };

                dlg.Controls.AddRange(new Control[] { lblMsg, btnOk });
                dlg.Load += (s, e) =>
                {
                    int bodyBottom = Math.Max(108, lblMsg.Bottom + 18);
                    btnOk.Location = new Point(dlg.ClientSize.Width - btnOk.Width - 16, bodyBottom);
                    dlg.ClientSize = new Size(368, btnOk.Bottom + 14);
                    xBtn.Left = dlg.ClientSize.Width - 34;
                };
                dlg.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        dlg.DialogResult = DialogResult.OK;
                        dlg.Close();
                    }
                };
                dlg.ShowDialog(this);
            }
        }

        /// <summary>
        /// 就近弹出全部分类悬浮浮层 (Popover)
        /// </summary>
        private void ShowCategoryPopover(Point screenPos)
        {
            bool isDark = IsDarkModeActive();
            Color bg = isDark ? WarmTheme.DarkElevatedBg : WarmTheme.LightCardBg;
            Color bdr = isDark ? WarmTheme.HairlineDark : WarmTheme.BorderSecondaryLight;

            Form pop = new Form();
            pop.FormBorderStyle = FormBorderStyle.None;
            pop.ShowInTaskbar = false;
            pop.StartPosition = FormStartPosition.Manual;
            pop.BackColor = bg;
            pop.TopMost = true;
            pop.KeyPreview = true;

            var cats = new List<string> { "全部" };
            cats.AddRange(ActiveCategories());

            int colW = 104;
            int cols = 2;
            int rows = (int)Math.Ceiling(cats.Count / (double)cols);
            int popW = Math.Max(228, cols * colW + 20);
            int popH = Math.Min(320, rows * 32 + 18);

            Rectangle wa = Screen.FromPoint(screenPos).WorkingArea;
            int px = Math.Max(wa.Left + 8, Math.Min(screenPos.X - popW + 24, wa.Right - popW - 8));
            int py = Math.Min(screenPos.Y + 4, wa.Bottom - popH - 8);
            pop.Bounds = new Rectangle(px, py, popW, popH);

            pop.Paint += (s, e) =>
            {
                using (Pen p = new Pen(bdr, 1))
                    e.Graphics.DrawRectangle(p, 0, 0, pop.Width - 1, pop.Height - 1);
            };
            pop.Load += (s, e) => Win32WindowStyler.ApplyModernWindowStyle(pop, isDark);

            for (int i = 0; i < cats.Count; i++)
            {
                string catName = cats[i];
                int r = i / cols;
                int c = i % cols;
                bool active = (catName == ActiveCat);

                ModernButton btn = new ModernButton
                {
                    Text = catName,
                    Location = new Point(10 + c * colW, 10 + r * 32),
                    Size = new Size(colW - 6, 28),
                    Variant = active ? ButtonVariant.Primary : ButtonVariant.Ghost,
                    IsDarkTheme = isDark,
                    AccentColor = WarmTheme.Orange,
                    TextColor = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary,
                    CornerRadius = WarmTheme.RadiusControl,
                    Font = WarmTheme.CreateFont(9F, active ? FontStyle.Bold : FontStyle.Regular)
                };
                btn.Click += (s, e) =>
                {
                    ActiveCat = catName;
                    RefreshCategoryViews();
                    RefreshList();
                    pop.Close();
                };
                pop.Controls.Add(btn);
            }

            pop.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) pop.Close(); };
            pop.Deactivate += (s, e) => pop.Close();
            pop.FormClosed += (s, e) => pop.Dispose();
            pop.Show(this);
        }

        private static bool ContainsInvalidPathChars(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            char[] invalid = Path.GetInvalidPathChars();
            for (int i = 0; i < path.Length; i++)
            {
                char c = path[i];
                if (c < ' ') return true;
                if (c == '\"') return true;
                if (Array.IndexOf(invalid, c) >= 0) return true;
            }
            return false;
        }

        private static bool SafeIsPathRooted(string path)
        {
            if (string.IsNullOrEmpty(path) || ContainsInvalidPathChars(path)) return false;
            try { return Path.IsPathRooted(path); }
            catch { return false; }
        }

        private static string SafeGetExtension(string path)
        {
            if (string.IsNullOrEmpty(path) || ContainsInvalidPathChars(path)) return "";
            try { return Path.GetExtension(path) ?? ""; }
            catch { return ""; }
        }

        private static string SafeGetFileName(string path)
        {
            if (string.IsNullOrEmpty(path) || ContainsInvalidPathChars(path)) return "";
            try { return Path.GetFileName(path) ?? ""; }
            catch { return ""; }
        }

        private static bool SafeFileExists(string path)
        {
            if (string.IsNullOrEmpty(path) || ContainsInvalidPathChars(path)) return false;
            try { return File.Exists(path); }
            catch { return false; }
        }

        private static string MakeSafeZipEntry(string relPath, string prefixDir, string fallbackId, string ext)
        {
            string norm = (relPath ?? "").Replace('\\', '/').Trim().TrimStart('/');
            bool validRel = !string.IsNullOrEmpty(norm)
                && !ContainsInvalidPathChars(norm)
                && norm.IndexOf(':') == -1
                && !norm.Contains("..")
                && norm.StartsWith(prefixDir + "/", StringComparison.OrdinalIgnoreCase);

            if (validRel) return norm;
            string safeExt = string.IsNullOrEmpty(ext) ? "" : ext;
            if (ContainsInvalidPathChars(safeExt) || safeExt.IndexOf('/') >= 0 || safeExt.IndexOf('\\') >= 0) safeExt = "";
            return prefixDir + "/" + fallbackId + safeExt;
        }

        private string ResolveMediaPath(string path)
        {
            string clean = JsonHelper.RepairLegacyPath(path);
            if (string.IsNullOrEmpty(clean) || ContainsInvalidPathChars(clean)) return "";
            try
            {
                return Path.IsPathRooted(clean) ? clean : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, clean);
            }
            catch
            {
                return "";
            }
        }

        private string ImportMediaFile(string sourceFile, bool isVideo)
        {
            string cleanSource = JsonHelper.RepairLegacyPath(sourceFile);
            if (string.IsNullOrEmpty(cleanSource)) return "";
            if (_data != null && _data.Settings != null && _data.Settings.CopyMediaToLocal)
            {
                string subDir = isVideo ? "videos" : "images";
                string targetDir = isVideo ? _videosDir : _imagesDir;
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
                string ext = SafeGetExtension(cleanSource);
                string relPath = subDir + "/" + Guid.NewGuid().ToString("N") + ext;
                string fullTarget = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relPath);
                File.Copy(cleanSource, fullTarget, true);
                return relPath;
            }
            return cleanSource;
        }

        private void RestoreWindowBounds()
        {
            if (_data == null || _data.Settings == null) return;

            int w = _data.Settings.WindowWidth;
            int h = _data.Settings.WindowHeight;
            if (w >= this.MinimumSize.Width && h >= this.MinimumSize.Height)
            {
                this.Size = new Size(w, h);
            }

            if (_data.Settings.HasWindowPos)
            {
                Rectangle savedRect = new Rectangle(_data.Settings.WindowX, _data.Settings.WindowY, this.Width, 40);
                bool onScreen = false;
                foreach (Screen scr in Screen.AllScreens)
                {
                    if (scr.WorkingArea.IntersectsWith(savedRect))
                    {
                        onScreen = true;
                        break;
                    }
                }
                if (onScreen)
                {
                    this.Location = new Point(_data.Settings.WindowX, _data.Settings.WindowY);
                }
            }
        }

        private void SaveWindowBounds()
        {
            if (_data == null || _data.Settings == null) return;

            Rectangle bounds = (this.WindowState == FormWindowState.Normal) ? this.Bounds : this.RestoreBounds;
            if (bounds.Width >= this.MinimumSize.Width && bounds.Height >= this.MinimumSize.Height)
            {
                bool changed = (_data.Settings.WindowWidth != bounds.Width ||
                                _data.Settings.WindowHeight != bounds.Height ||
                                _data.Settings.WindowX != bounds.X ||
                                _data.Settings.WindowY != bounds.Y ||
                                !_data.Settings.HasWindowPos);
                if (changed)
                {
                    _data.Settings.WindowWidth = bounds.Width;
                    _data.Settings.WindowHeight = bounds.Height;
                    _data.Settings.WindowX = bounds.X;
                    _data.Settings.WindowY = bounds.Y;
                    _data.Settings.HasWindowPos = true;
                    SaveData();
                }
            }
        }

        private void UpdatePinButtonAppearance()
        {
            if (_btnPin == null) return;
            bool isDark = IsDarkModeActive();
            _btnPin.IsDarkTheme = isDark;
            _btnPin.TextColor = this.TopMost
                ? WarmTheme.Orange
                : (isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary);
            _btnPin.CustomHoverBg = this.TopMost
                ? WarmTheme.ApplyAlpha(WarmTheme.Orange, isDark ? 0.22f : 0.12f)
                : (Color?)null;
            _btnPin.Invalidate();
        }

        private void UpdateWindowChromeButtons()
        {
            if (_btnWinMax == null) return;
            bool isMax = (this.WindowState == FormWindowState.Maximized);
            _btnWinMax.IconPainter = (g, r, col) => GlassRenderer.DrawVectorWinMaximizeIcon(g, r, col, isMax);
            _btnWinMax.Invalidate();
        }

        private void UpdateResponsiveLayout()
        {
            int w = this.ClientSize.Width;
            if (w <= 0) return;

            bool showSidebar = (w >= 460 && !_sidebarManualHidden);
            _sidebarExpanded = showSidebar;
            _sidebarPanel.Visible = showSidebar;
            _categoryBar.Visible = !showSidebar;

            // 第一行：右上角 Windows 控制三键 [—] [□] [×] + 分割 + [侧栏] [置顶] [设置]
            int btnY = 6;
            _btnWinClose.Location = new Point(w - 32, btnY);
            _btnWinMax.Location = new Point(_btnWinClose.Left - 26, btnY);
            _btnWinMin.Location = new Point(_btnWinMax.Left - 26, btnY);

            int rightToolX = _btnWinMin.Left - 10;
            _btnSettings.Location = new Point(rightToolX - 26, btnY);
            _btnPin.Location = new Point(_btnSettings.Left - 26, btnY);
            _btnSidebar.Location = new Point(_btnPin.Left - 26, btnY);

            // 第二行：搜索框 + 主操作按钮 [+ 词条 / 视频]
            _btnAddPhrase.Location = new Point(w - _btnAddPhrase.Width - 10, 38);
            _searchWrap.Location = new Point(10, 37);
            _searchWrap.Size = new Size(Math.Max(90, _btnAddPhrase.Left - 16), 32);
            _searchBox.Width = Math.Max(40, _searchWrap.Width - 36);

            _topBar.Invalidate();
            _phraseList.Invalidate();
        }

        private void InitializeUi()
        {
            this.Text = "客服花花";
            this.FormBorderStyle = FormBorderStyle.None;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.Size = new Size(336, 680);
            this.MinimumSize = new Size(292, 420);
            this.StartPosition = FormStartPosition.Manual;

            Rectangle workArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workArea.Right - 350, (workArea.Height - 680) / 2);
            this.TopMost = true;
            this.BackColor = WarmTheme.LightPaperBg;
            this.Font = WarmTheme.CreateFont(9.5F, FontStyle.Regular);

            EmbeddedAssets.EnsureIcon(_iconPath);
            if (File.Exists(_iconPath))
            {
                try { this.Icon = new Icon(_iconPath); } catch { }
            }

            // 1. 一体化无边框融合顶栏（上行标题工具栏 + 下行搜索主按钮，总高 74px）
            _topBar = new GlassTopBarPanel
            {
                Dock = DockStyle.Top,
                Height = 74,
                Padding = new Padding(10, 6, 10, 4)
            };
            _topBar.Paint += (s, e) =>
            {
                // 绘制全局工具按钮与 Windows 控制三键之间的 1px 细腻分割线
                if (_btnWinMin == null) return;
                bool isDark = IsDarkModeActive();
                Color lineCol = isDark ? WarmTheme.HairlineDark : WarmTheme.HairlineLight;
                int divX = _btnWinMin.Left - 5;
                using (Pen p = new Pen(lineCol, 1))
                {
                    e.Graphics.DrawLine(p, divX, 12, divX, 26);
                }
            };

            _picAvatar = new CircleAvatarControl
            {
                Size = new Size(22, 22),
                Location = new Point(12, 8),
                AvatarImage = EmbeddedAssets.GetAvatarImage(_avatarPath)
            };
            ToolTip tip = new ToolTip();
            tip.SetToolTip(_picAvatar, "点击查看客服花花原画");
            _picAvatar.Click += (s, e) => ShowEasterEggDialog();

            _lblLogo = new Label
            {
                Text = "客服花花",
                Font = WarmTheme.CreateFont(10F, FontStyle.Bold),
                ForeColor = WarmTheme.LightTextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(38, 9),
                Padding = new Padding(0),
                Margin = new Padding(0),
                AutoSize = true
            };

            BeginDragWindow(_topBar);
            BeginDragWindow(_lblLogo);

            // Windows 窗口控制三键（整齐保留在右上角）
            _btnWinClose = new ModernButton
            {
                Size = new Size(26, 26),
                Variant = ButtonVariant.Ghost,
                CornerRadius = 6,
                IconSize = 12,
                CustomHoverBg = WarmTheme.DestructiveRed,
                CustomHoverTextColor = Color.White,
                IconPainter = GlassRenderer.DrawVectorWinCloseIcon,
                TabStop = false
            };
            tip.SetToolTip(_btnWinClose, "关闭");
            _btnWinClose.Click += (s, e) => this.Close();

            _btnWinMax = new ModernButton
            {
                Size = new Size(26, 26),
                Variant = ButtonVariant.Ghost,
                CornerRadius = 6,
                IconSize = 12,
                IconPainter = (g, r, c) => GlassRenderer.DrawVectorWinMaximizeIcon(g, r, c, false),
                TabStop = false
            };
            tip.SetToolTip(_btnWinMax, "最大化 / 还原");
            _btnWinMax.Click += (s, e) => ToggleMaximizeRestore();

            _btnWinMin = new ModernButton
            {
                Size = new Size(26, 26),
                Variant = ButtonVariant.Ghost,
                CornerRadius = 6,
                IconSize = 12,
                IconPainter = GlassRenderer.DrawVectorWinMinimizeIcon,
                TabStop = false
            };
            tip.SetToolTip(_btnWinMin, "最小化");
            _btnWinMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            // 全局高频工具按钮（带快捷键提示）
            _btnSettings = new ModernButton
            {
                Size = new Size(26, 26),
                Variant = ButtonVariant.Ghost,
                CornerRadius = 6,
                IconSize = 15,
                IconPainter = GlassRenderer.DrawVectorGearIcon
            };
            tip.SetToolTip(_btnSettings, "设置  Ctrl+,");
            _btnSettings.Click += (s, e) => OpenSettingsDialog();

            _btnPin = new ModernButton
            {
                Size = new Size(26, 26),
                Variant = ButtonVariant.Ghost,
                CornerRadius = 6,
                IconSize = 14,
                IconPainter = (g, r, c) => GlassRenderer.DrawVectorPinIcon(g, r, c, this.TopMost)
            };
            tip.SetToolTip(_btnPin, "窗口置顶  Ctrl+T");
            _btnPin.Click += (s, e) => ToggleTopMostState();

            _btnSidebar = new ModernButton
            {
                Size = new Size(26, 26),
                Variant = ButtonVariant.Ghost,
                CornerRadius = 6,
                IconSize = 14,
                IconPainter = GlassRenderer.DrawVectorSidebarIcon
            };
            tip.SetToolTip(_btnSidebar, "展开/收起分类侧边栏  Ctrl+B");
            _btnSidebar.Click += (s, e) => ToggleSidebarLayout();

            // 主操作按钮 [+ 词条] / [+ 视频]
            _btnAddPhrase = new ModernButton
            {
                Text = "词条",
                Size = new Size(64, 30),
                Font = WarmTheme.CreateFont(9F, FontStyle.Bold),
                Variant = ButtonVariant.Primary,
                AccentColor = WarmTheme.Orange,
                CornerRadius = WarmTheme.RadiusControl,
                IconSize = 12,
                IconTextGap = 4,
                IconPainter = GlassRenderer.DrawVectorPlusIcon
            };
            tip.SetToolTip(_btnAddPhrase, "新建条目  Ctrl+N");
            _btnAddPhrase.Click += (s, e) => ShowAddOrEditDialog(null);

            // 第二行：Raycast 风格圆角搜索框（7px 圆角 + 1px 发丝框 + 3px 外发光焦点环）
            _searchWrap = new Panel
            {
                Location = new Point(10, 37),
                Size = new Size(210, 32),
                BackColor = Color.Transparent,
                Cursor = Cursors.IBeam
            };
            _searchWrap.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                bool isDark = IsDarkModeActive();
                Color bgCol = isDark ? WarmTheme.DarkSecondaryBtnBg : WarmTheme.LightCardBg;
                Color iconCol = isDark ? WarmTheme.DarkTextMuted : WarmTheme.LightTextMuted;
                Color bdrCol = _searchFocused
                    ? WarmTheme.Orange
                    : (isDark ? WarmTheme.TopHighlightDark : WarmTheme.BorderSecondaryLight);

                Rectangle boxRect = new Rectangle(2, 2, Math.Max(4, _searchWrap.Width - 4), Math.Max(4, _searchWrap.Height - 4));
                if (_searchFocused)
                {
                    Rectangle ringRect = new Rectangle(boxRect.X - 2, boxRect.Y - 2, boxRect.Width + 4, boxRect.Height + 4);
                    using (GraphicsPath fPath = GlassRenderer.CreateRoundedPath(ringRect, WarmTheme.RadiusControl + 2))
                    using (Pen fPen = new Pen(WarmTheme.ApplyAlpha(WarmTheme.Orange, 0.35f), 2.5f))
                    {
                        g.DrawPath(fPen, fPath);
                    }
                }

                using (GraphicsPath path = GlassRenderer.CreateRoundedPath(boxRect, WarmTheme.RadiusControl))
                {
                    using (Brush b = new SolidBrush(bgCol)) g.FillPath(b, path);
                    using (Pen bPen = new Pen(bdrCol, 1)) g.DrawPath(bPen, path);
                }

                GlassRenderer.DrawVectorSearchIcon(g, new Rectangle(8, (boxRect.Height - 14) / 2 + 2, 14, 14), iconCol);
            };
            _searchWrap.Click += (s, e) => _searchBox.Focus();

            _searchBox = new TextBox
            {
                Location = new Point(28, 7),
                Size = new Size(170, 18),
                BorderStyle = BorderStyle.None,
                BackColor = WarmTheme.LightCardBg,
                Font = WarmTheme.CreateFont(9.5F, FontStyle.Regular),
                ForeColor = WarmTheme.LightTextMuted,
                Text = SearchPlaceholder
            };
            _searchBox.Enter += (s, e) =>
            {
                _searchFocused = true;
                bool isDark = IsDarkModeActive();
                if (_searchBox.Text == SearchPlaceholder)
                {
                    _searchBox.Text = "";
                    _searchBox.ForeColor = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
                }
                _searchWrap.Invalidate();
            };
            _searchBox.Leave += (s, e) =>
            {
                _searchFocused = false;
                bool isDark = IsDarkModeActive();
                if (string.IsNullOrWhiteSpace(_searchBox.Text))
                {
                    _searchBox.Text = SearchPlaceholder;
                    _searchBox.ForeColor = isDark ? WarmTheme.DarkTextMuted : WarmTheme.LightTextMuted;
                }
                _searchWrap.Invalidate();
            };
            _searchBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Down)
                {
                    _phraseList.SelectFirstRow();
                    e.Handled = true;
                }
            };
            _searchBox.TextChanged += (s, e) => RefreshList();
            _searchWrap.Controls.Add(_searchBox);

            _topBar.Controls.AddRange(new Control[]
            {
                _picAvatar, _lblLogo,
                _btnSidebar, _btnPin, _btnSettings, _btnWinMin, _btnWinMax, _btnWinClose,
                _searchWrap, _btnAddPhrase
            });

            // 2. 分段控制切换条（文本 | 视频 + 数量）
            _modeBar = new ModeSwitchBar
            {
                Dock = DockStyle.Top,
                Height = 36
            };

            // 3. 窄屏横排分类导航条
            _categoryBar = new CustomCategoryBar
            {
                Dock = DockStyle.Top,
                Height = 36
            };
            _categoryBar.OnCategorySelected += (cat) => { ActiveCat = cat; RefreshCategoryViews(); RefreshList(); };
            _categoryBar.OnAddCategoryRequested += () => AddCategoryPrompt();
            _categoryBar.OnDeleteCurrentCategoryRequested += () => DeleteCurrentCategoryPrompt();
            _categoryBar.OnRenameCategoryRequested += (cat) => RenameCategoryPrompt(cat);
            _categoryBar.OnDeleteCategoryRequested += (cat) => DeleteCategory(cat);
            _categoryBar.OnAllCategoriesPopupRequested += (screenPt) => ShowCategoryPopover(screenPt);

            // 4. macOS 宽屏左侧导航侧边栏
            _sidebarPanel = new SidebarCategoryPanel
            {
                Dock = DockStyle.Left,
                Width = 154,
                Visible = false
            };
            _sidebarPanel.OnCategorySelected += (cat) => { ActiveCat = cat; RefreshCategoryViews(); RefreshList(); };
            _sidebarPanel.OnAddCategoryRequested += () => AddCategoryPrompt();
            _sidebarPanel.OnDeleteCurrentCategoryRequested += () => DeleteCurrentCategoryPrompt();
            _sidebarPanel.OnRenameCategoryRequested += (cat) => RenameCategoryPrompt(cat);
            _sidebarPanel.OnDeleteCategoryRequested += (cat) => DeleteCategory(cat);

            // 5. 工作区列表
            _phraseList = new MiniPhraseListView
            {
                Dock = DockStyle.Fill,
                BackColor = WarmTheme.LightPaperBg,
                EmptyAvatarImage = _picAvatar.AvatarImage
            };
            _phraseList.OnActionTriggered += (item, isSend) => SendToTarget(item, isSend);
            _phraseList.OnEditRequested += (item) => ShowAddOrEditDialog(item);
            _phraseList.OnCopyRequested += (item) =>
            {
                if (item != null && !string.IsNullOrEmpty(item.Content))
                {
                    try { Clipboard.SetDataObject(item.Content, true); } catch { }
                    _lblTarget.Text = "已复制正文内容到剪贴板";
                }
            };
            _phraseList.OnDeleteRequested += (item) =>
            {
                if (item == null) return;
                if (ShowModernConfirm("删除条目", string.Format("确定要删除【{0}】吗？", item.Title), true, "删除"))
                {
                    _data.Phrases.Remove(item);
                    SaveData();
                    RefreshCategoryViews();
                    RefreshList();
                }
            };

            // 6. 底部精致状态栏
            _statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 24,
                BackColor = WarmTheme.LightSidebarBg,
                Padding = new Padding(12, 3, 12, 2)
            };
            _statusBar.Paint += (s, e) =>
            {
                bool isDark = IsDarkModeActive();
                Color lineCol = isDark ? WarmTheme.HairlineDark : WarmTheme.HairlineLight;
                using (Pen p = new Pen(lineCol, 1))
                {
                    e.Graphics.DrawLine(p, 0, 0, _statusBar.Width, 0);
                }
            };
            _lblTarget = new Label
            {
                Text = "目标: 请先点击千牛聊天框",
                AutoSize = true,
                Dock = DockStyle.Left,
                Font = WarmTheme.CreateFont(8F, FontStyle.Regular),
                ForeColor = WarmTheme.LightTextSecondary
            };
            _statusBar.Controls.Add(_lblTarget);

            this.Controls.Add(_phraseList);
            this.Controls.Add(_categoryBar);
            this.Controls.Add(_sidebarPanel);
            this.Controls.Add(_modeBar);
            this.Controls.Add(_topBar);
            this.Controls.Add(_statusBar);

            _modeBar.OnModeChanged += (mode) => ApplyModeChange(mode);

            this.Load += (s, e) =>
            {
                Win32WindowStyler.ApplyModernWindowStyle(this, IsDarkModeActive());
                UpdateResponsiveLayout();
            };
            this.Resize += (s, e) =>
            {
                UpdateResponsiveLayout();
                UpdateWindowChromeButtons();
            };
            UpdateResponsiveLayout();
        }

        private void OpenSettingsDialog()
        {
            bool isDark = IsDarkModeActive();
            Color dlgBg = isDark ? WarmTheme.DarkPaperBg : WarmTheme.LightPaperBg;
            Color cardBg = isDark ? WarmTheme.DarkCardBg : WarmTheme.LightCardBg;
            Color cardBorder = isDark ? WarmTheme.HairlineDark : WarmTheme.BorderSecondaryLight;
            Color txtPrimary = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
            Color txtSec = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary;

            using (Form setDlg = new Form())
            {
                Rectangle wa = Screen.FromHandle(this.Handle).WorkingArea;
                int idealClientH = 716;
                int maxClientH = Math.Max(520, wa.Height - 60);
                int clientH = Math.Min(idealClientH, maxClientH);

                Panel hdr; ModernButton xBtn;
                PrepareModernDialog(setDlg, "偏好设置", 432, clientH, out hdr, out xBtn);

                // 中间可滚动容器（适配小尺寸屏幕笔记本）
                Panel scrollHost = new Panel
                {
                    Location = new Point(0, hdr.Height),
                    Size = new Size(432, clientH - hdr.Height - 58),
                    AutoScroll = true,
                    BackColor = dlgBg
                };

                Action<Panel> paintCard = (pnl) =>
                {
                    pnl.Paint += (s, e) =>
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        Rectangle cr = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
                        using (GraphicsPath gp = GlassRenderer.CreateRoundedPath(cr, WarmTheme.RadiusCard))
                        {
                            using (Brush b = new SolidBrush(cardBg)) e.Graphics.FillPath(b, gp);
                            using (Pen p = new Pen(cardBorder, 1)) e.Graphics.DrawPath(p, gp);
                        }
                    };
                };

                // 1. 外观与主题卡片 (11px 圆角 + 分段控制器)
                Panel pnlAppear = new Panel { Location = new Point(16, 12), Size = new Size(394, 212), BackColor = dlgBg };
                paintCard(pnlAppear);

                Label lblAppearTitle = new Label
                {
                    Text = "外观与主题",
                    Location = new Point(14, 12),
                    AutoSize = true,
                    Font = WarmTheme.CreateFont(9.5F, FontStyle.Bold),
                    ForeColor = txtPrimary,
                    BackColor = Color.Transparent
                };

                Label lblSchemeSub = new Label
                {
                    Text = "配色风格",
                    Location = new Point(14, 42),
                    AutoSize = true,
                    Font = WarmTheme.CreateFont(8.5F),
                    ForeColor = txtSec,
                    BackColor = Color.Transparent
                };

                SegmentedControl segScheme = new SegmentedControl
                {
                    Location = new Point(84, 36),
                    Size = new Size(294, 32),
                    Items = new string[] { "暖橙纸感", "樱粉花语" },
                    IsDarkTheme = isDark,
                    SelectedIndex = (_data.Settings.ColorScheme == "pink") ? 1 : 0
                };

                Label lblThemeSub = new Label
                {
                    Text = "界面模式",
                    Location = new Point(14, 84),
                    AutoSize = true,
                    Font = WarmTheme.CreateFont(8.5F),
                    ForeColor = txtSec,
                    BackColor = Color.Transparent
                };

                string curTheme = _data.Settings.ThemeType;
                int initBgIdx = (curTheme == "black") ? 1 : (curTheme == "custom" ? 2 : 0);
                SegmentedControl segBg = new SegmentedControl
                {
                    Location = new Point(84, 78),
                    Size = new Size(294, 32),
                    Items = new string[] { "经典白", "深色护眼", "自定义图" },
                    IsDarkTheme = isDark,
                    SelectedIndex = initBgIdx
                };

                string tempCustomBg = _data.Settings.CustomBgPath;

                Label lblBgPathTip = new Label
                {
                    Text = string.IsNullOrEmpty(tempCustomBg) ? "当前未指定自定义壁纸（直接识别电脑原路径，不复制）" : ("当前背景：" + SafeGetFileName(tempCustomBg)),
                    Location = new Point(14, 124),
                    Size = new Size(364, 18),
                    AutoEllipsis = true,
                    Font = WarmTheme.CreateFont(8F),
                    ForeColor = txtSec,
                    BackColor = Color.Transparent
                };

                ModernButton btnChooseBg = new ModernButton
                {
                    Text = "选择壁纸...",
                    Location = new Point(14, 150),
                    Size = new Size(106, 32),
                    Variant = ButtonVariant.Secondary,
                    IsDarkTheme = isDark,
                    TextColor = txtPrimary,
                    CornerRadius = WarmTheme.RadiusControl,
                    IconSize = 14,
                    IconTextGap = 6,
                    IconPainter = GlassRenderer.DrawVectorImageIcon
                };

                ModernButton btnClearBg = new ModernButton
                {
                    Text = "一键删图",
                    Location = new Point(128, 150),
                    Size = new Size(94, 32),
                    Variant = ButtonVariant.Destructive,
                    IsDarkTheme = isDark,
                    CornerRadius = WarmTheme.RadiusControl,
                    IconSize = 13,
                    IconTextGap = 5,
                    IconPainter = GlassRenderer.DrawVectorTrashIcon
                };
                ToolTip clearTip = new ToolTip();
                clearTip.SetToolTip(btnClearBg, "清除背景并删除软件目录下的历史背景图文件");

                btnChooseBg.Click += (s, e) =>
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Title = "选择背景壁纸图片";
                        ofd.Filter = "图片文件 (*.jpg;*.png;*.jpeg;*.bmp)|*.jpg;*.png;*.jpeg;*.bmp";
                        if (ofd.ShowDialog(setDlg) == DialogResult.OK)
                        {
                            // 自定义背景图始终直接识别原文件位置，不复制到软件目录
                            tempCustomBg = ofd.FileName;
                            segBg.SelectedIndex = 2;
                            lblBgPathTip.Text = "已载入：" + SafeGetFileName(tempCustomBg);
                            ShowModernAlert("提示", "背景图片已载入（直接引用原位置），点击「保存设置」后生效。");
                        }
                    }
                };

                btnClearBg.Click += (s, e) =>
                {
                    if (_customBgImg != null) { _customBgImg.Dispose(); _customBgImg = null; }
                    _phraseList.SetTheme("white", null);

                    int deletedCount = 0;
                    try
                    {
                        string[] bgFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "bg_*.*");
                        foreach (string f in bgFiles)
                        {
                            try { File.Delete(f); deletedCount++; } catch { }
                        }
                    }
                    catch { }

                    tempCustomBg = "";
                    _data.Settings.CustomBgPath = "";
                    if (segBg.SelectedIndex == 2) segBg.SelectedIndex = 0;
                    lblBgPathTip.Text = "当前未指定自定义壁纸（直接识别电脑原路径，不复制）";

                    ShowModernAlert(
                        "清理完成",
                        deletedCount > 0 ? string.Format("已一键删除 {0} 张历史背景图片！", deletedCount) : "当前没有已保存的历史背景图片文件。");
                };

                pnlAppear.Controls.AddRange(new Control[]
                {
                    lblAppearTitle, lblSchemeSub, segScheme, lblThemeSub, segBg, lblBgPathTip, btnChooseBg, btnClearBg
                });

                // 2. 话术字号卡片 (4 档滑动分段控制器)
                Panel pnlFont = new Panel { Location = new Point(16, 234), Size = new Size(394, 86), BackColor = dlgBg };
                paintCard(pnlFont);

                Label lblFontTitle = new Label
                {
                    Text = "话术字号",
                    Location = new Point(14, 12),
                    AutoSize = true,
                    Font = WarmTheme.CreateFont(9.5F, FontStyle.Bold),
                    ForeColor = txtPrimary,
                    BackColor = Color.Transparent
                };

                SegmentedControl segFont = new SegmentedControl
                {
                    Location = new Point(14, 40),
                    Size = new Size(364, 34),
                    Items = new string[] { "标准 9pt", "中号 11pt", "大号 13pt", "特大 15pt" },
                    IsDarkTheme = isDark,
                    SelectedIndex = Math.Max(0, Math.Min(3, _data.Settings.FontSizeLevel))
                };
                pnlFont.Controls.AddRange(new Control[] { lblFontTitle, segFont });

                // 3. 备份与换机卡片
                Panel pnlBackup = new Panel { Location = new Point(16, 330), Size = new Size(394, 248), BackColor = dlgBg };
                paintCard(pnlBackup);

                Panel picIconBackup = new Panel { Location = new Point(14, 13), Size = new Size(16, 16), BackColor = Color.Transparent };
                picIconBackup.Paint += (s, e) => GlassRenderer.DrawVectorBackupIcon(e.Graphics, new Rectangle(0, 0, 16, 16), txtPrimary);

                Label lblBackupTitle = new Label
                {
                    Text = "备份与换机",
                    Location = new Point(34, 12),
                    AutoSize = true,
                    Font = WarmTheme.CreateFont(9.5F, FontStyle.Bold),
                    ForeColor = txtPrimary,
                    BackColor = Color.Transparent
                };
                Label lblBackupDesc = new Label
                {
                    Text = "话术、图片和视频打成一个包，换电脑一键恢复",
                    Location = new Point(14, 34),
                    AutoSize = true,
                    Font = WarmTheme.CreateFont(8.2F),
                    ForeColor = txtSec,
                    BackColor = Color.Transparent
                };

                ModernButton btnExport = new ModernButton
                {
                    Text = "导出备份包 (.zip)",
                    Location = new Point(14, 62),
                    Size = new Size(134, 32),
                    Variant = ButtonVariant.Secondary,
                    IsDarkTheme = isDark,
                    TextColor = txtPrimary,
                    CornerRadius = WarmTheme.RadiusControl
                };
                btnExport.Click += (s, e) => ExportBackupData();

                ModernButton btnImport = new ModernButton
                {
                    Text = "导入备份包...",
                    Location = new Point(156, 62),
                    Size = new Size(120, 32),
                    Variant = ButtonVariant.Secondary,
                    IsDarkTheme = isDark,
                    TextColor = txtPrimary,
                    CornerRadius = WarmTheme.RadiusControl
                };
                btnImport.Click += (s, e) => ImportBackupData();

                ModernButton btnOpenDir = new ModernButton
                {
                    Text = "数据目录",
                    Location = new Point(284, 62),
                    Size = new Size(94, 32),
                    Variant = ButtonVariant.Secondary,
                    IsDarkTheme = isDark,
                    TextColor = txtSec,
                    CornerRadius = WarmTheme.RadiusControl
                };
                btnOpenDir.Click += (s, e) =>
                {
                    try { System.Diagnostics.Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory); }
                    catch { }
                };

                ModernButton btnClearPhrases = new ModernButton
                {
                    Text = "一键清理所有提示词",
                    Location = new Point(14, 102),
                    Size = new Size(178, 32),
                    Variant = ButtonVariant.Destructive,
                    IsDarkTheme = isDark,
                    CornerRadius = WarmTheme.RadiusControl
                };
                btnClearPhrases.Click += (s, e) =>
                {
                    int textCount = _data.Phrases.Count(p => string.IsNullOrEmpty(p.ItemType) || p.ItemType != "video");
                    if (textCount == 0)
                    {
                        ShowModernAlert("提示", "当前没有任何文本提示词，无需清理。");
                        return;
                    }

                    if (!ShowModernConfirm(
                        "确认清理所有提示词",
                        string.Format("确定要清理全部 {0} 条文本提示词吗？\n（分类名称与视频快捷方式将会保留，关联的附带图片将一并清理）", textCount),
                        true, "继续清理")) return;

                    if (!ShowModernConfirm(
                        "二次确认",
                        "请再次确认：清空后无法撤销，是否立即删除所有文本提示词？",
                        true, "确定清空")) return;

                    _data.Phrases.RemoveAll(p => string.IsNullOrEmpty(p.ItemType) || p.ItemType != "video");
                    try
                    {
                        if (Directory.Exists(_imagesDir))
                        {
                            foreach (string f in Directory.GetFiles(_imagesDir))
                            {
                                try { File.Delete(f); } catch { }
                            }
                        }
                    }
                    catch { }

                    SaveData();
                    RefreshCategoryViews();
                    RefreshList();
                    ShowModernAlert("清理完成", "已成功清空所有文本提示词！");
                };

                ModernButton btnClearVideos = new ModernButton
                {
                    Text = "一键清理所有视频",
                    Location = new Point(200, 102),
                    Size = new Size(178, 32),
                    Variant = ButtonVariant.Destructive,
                    IsDarkTheme = isDark,
                    CornerRadius = WarmTheme.RadiusControl
                };
                btnClearVideos.Click += (s, e) =>
                {
                    int videoCount = _data.Phrases.Count(p => p.ItemType == "video");
                    if (videoCount == 0)
                    {
                        ShowModernAlert("提示", "当前没有任何视频快捷方式，无需清理。");
                        return;
                    }

                    if (!ShowModernConfirm(
                        "确认清理所有视频",
                        string.Format("确定要清理全部 {0} 条视频快捷方式吗？\n（分类名称与文本提示词将会保留，缓存视频文件将一并删除）", videoCount),
                        true, "继续清理")) return;

                    if (!ShowModernConfirm(
                        "二次确认",
                        "请再次确认：清空后无法撤销，是否立即删除所有视频快捷方式？",
                        true, "确定清空")) return;

                    _data.Phrases.RemoveAll(p => p.ItemType == "video");
                    try
                    {
                        if (Directory.Exists(_videosDir))
                        {
                            foreach (string f in Directory.GetFiles(_videosDir))
                            {
                                try { File.Delete(f); } catch { }
                            }
                        }
                    }
                    catch { }

                    SaveData();
                    RefreshCategoryViews();
                    RefreshList();
                    ShowModernAlert("清理完成", "已成功清空所有视频快捷方式！");
                };

                // 紧凑型 iOS 胶囊开关（36×20px，开启态 #34C759）
                ToggleSwitch swCopyMedia = new ToggleSwitch
                {
                    Text = "复制图片和视频到软件目录",
                    Location = new Point(14, 146),
                    Size = new Size(340, 24),
                    IsDarkTheme = isDark,
                    Font = WarmTheme.CreateFont(9F, FontStyle.Regular)
                };
                swCopyMedia.SetCheckedImmediate(_data.Settings.CopyMediaToLocal);

                Label lblCopyDesc = new Label
                {
                    Text = "不开启则直接读取电脑原位置，不占空间",
                    Location = new Point(58, 172),
                    AutoSize = true,
                    Font = WarmTheme.CreateFont(8F),
                    ForeColor = txtSec,
                    BackColor = Color.Transparent
                };
                Label lblRestoreTip = new Label
                {
                    Text = "提示：导入备份会让备份里面的图片和视频复制到软件目录里，会导致部分空间占用",
                    Location = new Point(14, 194),
                    MaximumSize = new Size(364, 0),
                    AutoSize = true,
                    Font = WarmTheme.CreateFont(8F),
                    ForeColor = txtSec,
                    BackColor = Color.Transparent
                };

                pnlBackup.Controls.AddRange(new Control[]
                {
                    picIconBackup, lblBackupTitle, lblBackupDesc,
                    btnExport, btnImport, btnOpenDir, btnClearPhrases, btnClearVideos,
                    swCopyMedia, lblCopyDesc, lblRestoreTip
                });

                // 4. 开源项目地址卡片
                string repoUrl = "https://github.com/l1011186781-commits/kefu-huahua";
                Panel pnlAbout = new Panel { Location = new Point(16, 588), Size = new Size(394, 58), BackColor = dlgBg };
                paintCard(pnlAbout);

                Label lblGitHubTitle = new Label
                {
                    Text = "开源项目主页 · 客服花花 v1.3 蝶翼双飞",
                    Location = new Point(14, 10),
                    AutoSize = true,
                    Font = WarmTheme.CreateFont(8.5F, FontStyle.Bold),
                    ForeColor = txtPrimary,
                    BackColor = Color.Transparent
                };
                LinkLabel lnkGitHub = new LinkLabel
                {
                    Text = repoUrl,
                    Location = new Point(14, 31),
                    AutoSize = true,
                    Font = WarmTheme.CreateFont(8.5F),
                    LinkColor = isDark ? WarmTheme.DarkPrimaryAccent : WarmTheme.Orange,
                    ActiveLinkColor = WarmTheme.OrangePressed,
                    VisitedLinkColor = isDark ? WarmTheme.DarkPrimaryAccent : WarmTheme.Orange,
                    LinkBehavior = LinkBehavior.HoverUnderline,
                    Cursor = Cursors.Hand,
                    BackColor = Color.Transparent
                };
                ToolTip ghTip = new ToolTip();
                ghTip.SetToolTip(lnkGitHub, "点击在浏览器中访问项目主页");
                lnkGitHub.LinkClicked += (s, e) =>
                {
                    try { System.Diagnostics.Process.Start(repoUrl); }
                    catch (Exception ex) { ShowModernAlert("提示", "无法打开浏览器: " + ex.Message); }
                };
                pnlAbout.Controls.AddRange(new Control[] { lblGitHubTitle, lnkGitHub });

                scrollHost.Controls.AddRange(new Control[] { pnlAppear, pnlFont, pnlBackup, pnlAbout });

                // 底部固定操作栏
                Panel footer = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 56,
                    BackColor = cardBg
                };
                footer.Paint += (s, e) =>
                {
                    using (Pen p = new Pen(cardBorder, 1))
                        e.Graphics.DrawLine(p, 0, 0, footer.Width, 0);
                };

                ModernButton btnSave = new ModernButton
                {
                    Text = "保存设置",
                    Location = new Point(228, 11),
                    Size = new Size(98, WarmTheme.BtnHeightDialog),
                    Variant = ButtonVariant.Primary,
                    IsDarkTheme = isDark,
                    AccentColor = WarmTheme.Orange,
                    CornerRadius = WarmTheme.RadiusDialogBtn,
                    Font = WarmTheme.CreateFont(9.5F, FontStyle.Bold),
                    DialogResult = DialogResult.OK
                };

                ModernButton btnCancel = new ModernButton
                {
                    Text = "取消",
                    Location = new Point(334, 11),
                    Size = new Size(80, WarmTheme.BtnHeightDialog),
                    Variant = ButtonVariant.Secondary,
                    IsDarkTheme = isDark,
                    TextColor = txtPrimary,
                    CornerRadius = WarmTheme.RadiusDialogBtn,
                    DialogResult = DialogResult.Cancel
                };
                footer.Controls.AddRange(new Control[] { btnSave, btnCancel });

                setDlg.Load += (s, e) =>
                {
                    pnlBackup.Height = Math.Max(238, lblRestoreTip.Bottom + 14);
                    pnlAbout.Top = pnlBackup.Bottom + 10;
                    int contentBottom = pnlAbout.Bottom + 14;
                    int wantH = hdr.Height + contentBottom + footer.Height;
                    int finalH = Math.Min(maxClientH, wantH);
                    setDlg.ClientSize = new Size(432, finalH);
                    scrollHost.Size = new Size(432, finalH - hdr.Height - footer.Height);
                    xBtn.Left = setDlg.ClientSize.Width - 34;
                };

                setDlg.Controls.Add(scrollHost);
                setDlg.Controls.Add(footer);
                hdr.BringToFront();

                if (setDlg.ShowDialog(this) == DialogResult.OK)
                {
                    _data.Settings.ColorScheme = (segScheme.SelectedIndex == 1) ? "pink" : "orange";

                    if (segBg.SelectedIndex == 1) _data.Settings.ThemeType = "black";
                    else if (segBg.SelectedIndex == 2) _data.Settings.ThemeType = "custom";
                    else _data.Settings.ThemeType = "white";

                    _data.Settings.CustomBgPath = tempCustomBg;
                    _data.Settings.CopyMediaToLocal = swCopyMedia.Checked;
                    _data.Settings.FontSizeLevel = Math.Max(0, Math.Min(3, segFont.SelectedIndex));

                    SaveData();
                    ApplySettings();
                }
            }
        }

        private void ApplySettings()
        {
            WarmTheme.SetScheme(_data.Settings.ColorScheme);

            int fLevel = _data.Settings.FontSizeLevel;
            if (fLevel == 0) _phraseList.SetFontConfig(9.5F, 40);
            else if (fLevel == 1) _phraseList.SetFontConfig(10.8F, 45);
            else if (fLevel == 2) _phraseList.SetFontConfig(12.5F, 50);
            else _phraseList.SetFontConfig(14.5F, 56);

            string theme = _data.Settings.ThemeType;
            bool isDark = (theme == "black");

            if (_customBgImg != null) { _customBgImg.Dispose(); _customBgImg = null; }
            if (theme == "custom" && !string.IsNullOrEmpty(_data.Settings.CustomBgPath))
            {
                string fullImg = ResolveMediaPath(_data.Settings.CustomBgPath);
                if (SafeFileExists(fullImg))
                {
                    try { _customBgImg = Image.FromFile(fullImg); } catch { }
                }
            }

            this.BackColor = isDark ? WarmTheme.DarkPaperBg : WarmTheme.LightPaperBg;
            Win32WindowStyler.ApplyModernWindowStyle(this, isDark);

            Color textCol = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
            Color secCol = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary;
            Color boxBg = isDark ? WarmTheme.DarkSecondaryBtnBg : WarmTheme.LightCardBg;

            _topBar.IsDarkTheme = isDark;
            _topBar.Invalidate();
            _picAvatar.IsDarkTheme = isDark;
            _picAvatar.Invalidate();
            _lblLogo.ForeColor = textCol;

            _searchBox.BackColor = boxBg;
            if (_searchBox.Text == SearchPlaceholder)
                _searchBox.ForeColor = isDark ? WarmTheme.DarkTextMuted : WarmTheme.LightTextMuted;
            else
                _searchBox.ForeColor = textCol;
            _searchWrap.Invalidate();

            ModernButton[] ghostToolBtns = new ModernButton[] { _btnSidebar, _btnPin, _btnSettings, _btnWinMin, _btnWinMax, _btnWinClose };
            foreach (ModernButton b in ghostToolBtns)
            {
                if (b == null) continue;
                b.IsDarkTheme = isDark;
                b.AccentColor = WarmTheme.Orange;
                b.TextColor = (b == _btnWinClose || b == _btnWinMax || b == _btnWinMin)
                    ? secCol
                    : textCol;
                b.Invalidate();
            }
            UpdatePinButtonAppearance();
            UpdateWindowChromeButtons();

            _btnAddPhrase.IsDarkTheme = isDark;
            _btnAddPhrase.AccentColor = WarmTheme.Orange;
            _btnAddPhrase.Invalidate();

            _statusBar.BackColor = isDark ? WarmTheme.DarkSidebarBg : WarmTheme.LightSidebarBg;
            _statusBar.Invalidate();
            if (string.IsNullOrEmpty(_lastTargetTitle))
            {
                _lblTarget.ForeColor = secCol;
            }

            _modeBar.IsDarkTheme = isDark;
            _modeBar.Invalidate();

            _categoryBar.IsDarkTheme = isDark;
            _categoryBar.Invalidate();

            _sidebarPanel.IsDarkTheme = isDark;
            _sidebarPanel.Invalidate();

            _phraseList.SetTheme(theme, _customBgImg);
        }

        private void RunWithBusyDialog(string message, Action work, Action onDone)
        {
            bool isDark = IsDarkModeActive();
            using (Form busyDlg = new Form())
            {
                busyDlg.Text = "处理中";
                busyDlg.ClientSize = new Size(344, 108);
                busyDlg.StartPosition = FormStartPosition.CenterParent;
                busyDlg.FormBorderStyle = FormBorderStyle.None;
                busyDlg.ShowInTaskbar = false;
                busyDlg.BackColor = isDark ? WarmTheme.DarkElevatedBg : WarmTheme.LightCardBg;
                busyDlg.Font = WarmTheme.CreateFont(9.5F, FontStyle.Regular);

                float angle = 0f;
                System.Windows.Forms.Timer spinTimer = new System.Windows.Forms.Timer { Interval = 24 };
                Panel spinnerBox = new Panel { Location = new Point(22, 36), Size = new Size(28, 28), BackColor = Color.Transparent };
                spinnerBox.Paint += (s, e) =>
                {
                    GlassRenderer.DrawVectorSpinner(e.Graphics, new Rectangle(0, 0, 28, 28), WarmTheme.Orange, angle);
                };
                spinTimer.Tick += (s, e) => { angle = (angle + 18f) % 360f; spinnerBox.Invalidate(); };

                Label lbl = new Label
                {
                    Text = message,
                    Location = new Point(58, 20),
                    Size = new Size(266, 68),
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary
                };

                busyDlg.Paint += (s, e) =>
                {
                    using (Pen p = new Pen(isDark ? WarmTheme.HairlineDark : WarmTheme.BorderSecondaryLight, 1))
                        e.Graphics.DrawRectangle(p, 0, 0, busyDlg.ClientSize.Width - 1, busyDlg.ClientSize.Height - 1);
                };
                busyDlg.Load += (s, e) => Win32WindowStyler.ApplyModernWindowStyle(busyDlg, isDark);
                busyDlg.Controls.AddRange(new Control[] { spinnerBox, lbl });

                Exception err = null;
                busyDlg.Shown += (s, e) =>
                {
                    spinTimer.Start();
                    Task.Factory.StartNew(() =>
                    {
                        try { work(); }
                        catch (Exception ex) { err = ex; }
                        if (busyDlg.IsHandleCreated && !busyDlg.IsDisposed)
                        {
                            busyDlg.BeginInvoke((Action)(() => busyDlg.Close()));
                        }
                    });
                };

                busyDlg.ShowDialog(this);
                spinTimer.Stop();
                spinTimer.Dispose();
                if (err != null) throw err;
                if (onDone != null) onDone();
            }
        }

        private static string EnsureUniqueSafeId(string id, HashSet<string> usedIds)
        {
            bool valid = !string.IsNullOrEmpty(id);
            if (valid)
            {
                for (int i = 0; i < id.Length; i++)
                {
                    char c = id[i];
                    bool ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                              (c >= '0' && c <= '9') || c == '-' || c == '_';
                    if (!ok) { valid = false; break; }
                }
            }
            string candidate = valid ? id : Guid.NewGuid().ToString("N");
            while (!usedIds.Add(candidate))
            {
                candidate = Guid.NewGuid().ToString("N");
            }
            return candidate;
        }

        private string PackPhraseMediaEntry(
            ZipArchive archive,
            HashSet<string> packedEntries,
            string rawPath,
            string prefixDir,
            string safeId,
            CompressionLevel level,
            string itemTitle,
            string mediaLabel,
            List<string> missingMedia)
        {
            if (string.IsNullOrEmpty(rawPath)) return "";
            string fullPath = ResolveMediaPath(rawPath);
            if (!string.IsNullOrEmpty(fullPath) && SafeFileExists(fullPath))
            {
                string ext = SafeGetExtension(fullPath);
                string entryName = MakeSafeZipEntry(rawPath, prefixDir, safeId, ext);
                if (packedEntries.Add(entryName))
                {
                    archive.CreateEntryFromFile(fullPath, entryName, level);
                }
                return entryName;
            }

            if (missingMedia != null)
            {
                string label = string.IsNullOrEmpty(itemTitle) ? mediaLabel : (itemTitle + " (" + mediaLabel + ")");
                missingMedia.Add(label);
            }
            return JsonHelper.RepairLegacyPath(rawPath);
        }

        private void RepairInMemoryPathsBeforeExport()
        {
            bool modified = false;
            if (_data != null && _data.Settings != null)
            {
                bool bgChanged;
                string fixedBg = JsonHelper.RepairLegacyPath(_data.Settings.CustomBgPath, out bgChanged);
                if (bgChanged)
                {
                    _data.Settings.CustomBgPath = fixedBg;
                    modified = true;
                }
            }
            if (_data != null && _data.Phrases != null)
            {
                foreach (var p in _data.Phrases)
                {
                    bool imgChanged;
                    string fixedImg = JsonHelper.RepairLegacyPath(p.ImagePath, out imgChanged);
                    if (imgChanged) { p.ImagePath = fixedImg; modified = true; }

                    bool vidChanged;
                    string fixedVid = JsonHelper.RepairLegacyPath(p.VideoPath, out vidChanged);
                    if (vidChanged) { p.VideoPath = fixedVid; modified = true; }
                }
            }
            if (modified) SaveData();
        }

        private void ExportBackupData()
        {
            if (_data.Phrases.Count == 0 && _data.Categories.Count == 0 && _data.VideoCategories.Count == 0)
            {
                MessageBox.Show("当前没有任何分类、文本提示词或视频，暂无需备份！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "导出客服花花全量备份包（文本 + 图片 + 视频）";
                string timeStr = DateTime.Now.ToString("yyyyMMdd_HHmm");
                sfd.FileName = string.Format("客服花花备份_{0}.zip", timeStr);
                sfd.Filter = "花花备份文件 (*.zip)|*.zip|所有文件 (*.*)|*.*";

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        RepairInMemoryPathsBeforeExport();
                        SaveData();
                        string destZip = sfd.FileName;

                        AppData exportData = new AppData();
                        exportData.Settings = new AppSettings
                        {
                            FontSizeLevel = _data.Settings.FontSizeLevel,
                            ThemeType = _data.Settings.ThemeType,
                            ColorScheme = _data.Settings.ColorScheme,
                            CustomBgPath = JsonHelper.RepairLegacyPath(_data.Settings.CustomBgPath),
                            CopyMediaToLocal = _data.Settings.CopyMediaToLocal,
                            WindowWidth = _data.Settings.WindowWidth,
                            WindowHeight = _data.Settings.WindowHeight,
                            WindowX = _data.Settings.WindowX,
                            WindowY = _data.Settings.WindowY,
                            HasWindowPos = _data.Settings.HasWindowPos
                        };
                        exportData.Categories.AddRange(_data.Categories);
                        exportData.VideoCategories.AddRange(_data.VideoCategories);

                        List<PhraseItem> snapshotPhrases = new List<PhraseItem>(_data.Phrases.Count);
                        var usedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var p in _data.Phrases)
                        {
                            PhraseItem cloned = new PhraseItem(p.Category, p.Title, p.Content, p.ImagePath, p.ItemType, p.VideoPath);
                            cloned.Id = EnsureUniqueSafeId(p.Id, usedIds);
                            snapshotPhrases.Add(cloned);
                        }

                        var missingMedia = new List<string>();

                        RunWithBusyDialog("正在打包备份（包含文本、图片、视频与背景），请稍候...", () =>
                        {
                            if (File.Exists(destZip)) File.Delete(destZip);

                            var packedEntries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                            using (ZipArchive archive = ZipFile.Open(destZip, ZipArchiveMode.Create))
                            {
                                // 打包自定义背景图片（支持原位置或软件目录）
                                if (!string.IsNullOrEmpty(exportData.Settings.CustomBgPath))
                                {
                                    string fullBg = ResolveMediaPath(exportData.Settings.CustomBgPath);
                                    if (!string.IsNullOrEmpty(fullBg) && SafeFileExists(fullBg))
                                    {
                                        string bgExt = SafeGetExtension(fullBg);
                                        string bgEntryName = MakeSafeZipEntry(
                                            exportData.Settings.CustomBgPath,
                                            "images",
                                            "custom_bg_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                                            bgExt);
                                        if (packedEntries.Add(bgEntryName))
                                            archive.CreateEntryFromFile(fullBg, bgEntryName, CompressionLevel.Fastest);
                                        exportData.Settings.CustomBgPath = bgEntryName;
                                    }
                                    else
                                    {
                                        missingMedia.Add("自定义背景图");
                                        exportData.Settings.CustomBgPath = "";
                                    }
                                }

                                foreach (var cloned in snapshotPhrases)
                                {
                                    cloned.ImagePath = PackPhraseMediaEntry(
                                        archive, packedEntries, cloned.ImagePath, "images", cloned.Id,
                                        CompressionLevel.Fastest, cloned.Title, "图片", missingMedia);

                                    cloned.VideoPath = PackPhraseMediaEntry(
                                        archive, packedEntries, cloned.VideoPath, "videos", cloned.Id,
                                        CompressionLevel.NoCompression, cloned.Title, "视频", missingMedia);

                                    exportData.Phrases.Add(cloned);
                                }

                                // 兼容打包本地已有但未在条目标记的文件
                                if (Directory.Exists(_imagesDir))
                                {
                                    foreach (string imgFile in Directory.GetFiles(_imagesDir))
                                    {
                                        string fileName = SafeGetFileName(imgFile);
                                        if (string.IsNullOrEmpty(fileName)) continue;
                                        string entryName = "images/" + fileName;
                                        if (packedEntries.Add(entryName))
                                            archive.CreateEntryFromFile(imgFile, entryName, CompressionLevel.Fastest);
                                    }
                                }
                                if (Directory.Exists(_videosDir))
                                {
                                    foreach (string vidFile in Directory.GetFiles(_videosDir))
                                    {
                                        string fileName = SafeGetFileName(vidFile);
                                        if (string.IsNullOrEmpty(fileName)) continue;
                                        string entryName = "videos/" + fileName;
                                        if (packedEntries.Add(entryName))
                                            archive.CreateEntryFromFile(vidFile, entryName, CompressionLevel.NoCompression);
                                    }
                                }

                                ZipArchiveEntry jsonEntry = archive.CreateEntry("phrases.json", CompressionLevel.Optimal);
                                using (StreamWriter sw = new StreamWriter(jsonEntry.Open(), Encoding.UTF8))
                                {
                                    sw.Write(JsonHelper.Serialize(exportData));
                                }
                            }
                        }, () =>
                        {
                            string baseMsg = string.Format(
                                "备份成功！\n\n备份包已保存至：\n{0}\n\n已包含所有分类、文本话术、附带图片、视频文件与背景设置；换电脑时在「设置 -> 备份与换机 -> 导入备份包」即可恢复。",
                                destZip);
                            if (missingMedia.Count > 0)
                            {
                                int showCount = Math.Min(missingMedia.Count, 5);
                                string detail = string.Join("、", missingMedia.Take(showCount).ToArray());
                                if (missingMedia.Count > showCount) detail += " 等";
                                baseMsg += string.Format("\n\n注意：有 {0} 个引用的媒体文件在原位置已找不到（{1}），已跳过文件打包但保留话术记录。", missingMedia.Count, detail);
                            }
                            MessageBox.Show(baseMsg, "备份成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("备份过程中发生异常: " + ex.Message, "备份失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ImportBackupData()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "选择要恢复的客服花花备份文件";
                ofd.Filter = "花花备份文件 (*.zip;*.huahua)|*.zip;*.huahua|所有文件 (*.*)|*.*";

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    bool confirmed = ShowModernConfirm(
                        "确认导入备份",
                        "导入备份将恢复该备份包中的所有【文本/视频分类】、【话术】、【附带图片】以及【视频文件】。\n注意：导入备份会让备份里面的图片和视频复制到软件目录里，会导致部分空间占用。\n是否确认导入？",
                        false,
                        "确认导入");

                    if (!confirmed) return;

                    string tempExtractDir = Path.Combine(Path.GetTempPath(), "HuahuaRestore_" + Guid.NewGuid().ToString("N"));
                    bool validBackup = true;
                    try
                    {
                        string zipFile = ofd.FileName;
                        RunWithBusyDialog("正在解压并恢复数据（包含视频文件），请稍候...", () =>
                        {
                            Directory.CreateDirectory(tempExtractDir);
                            ZipFile.ExtractToDirectory(zipFile, tempExtractDir);

                            string backupJson = Path.Combine(tempExtractDir, "phrases.json");
                            if (!File.Exists(backupJson))
                            {
                                validBackup = false;
                                return;
                            }

                            File.Copy(backupJson, _storagePath, true);

                            string backupImgDir = Path.Combine(tempExtractDir, "images");
                            if (Directory.Exists(backupImgDir))
                            {
                                if (!Directory.Exists(_imagesDir)) Directory.CreateDirectory(_imagesDir);
                                foreach (string imgFile in Directory.GetFiles(backupImgDir))
                                {
                                    string dest = Path.Combine(_imagesDir, Path.GetFileName(imgFile));
                                    File.Copy(imgFile, dest, true);
                                }
                            }

                            string backupVidDir = Path.Combine(tempExtractDir, "videos");
                            if (Directory.Exists(backupVidDir))
                            {
                                if (!Directory.Exists(_videosDir)) Directory.CreateDirectory(_videosDir);
                                foreach (string vidFile in Directory.GetFiles(backupVidDir))
                                {
                                    string dest = Path.Combine(_videosDir, Path.GetFileName(vidFile));
                                    File.Copy(vidFile, dest, true);
                                }
                            }
                        }, () =>
                        {
                            if (!validBackup)
                            {
                                ShowModernAlert("导入失败", "无效的备份文件：未在该文件中检测到 phrases.json 数据！");
                                return;
                            }

                            LoadData();
                            ApplySettings();
                            _currentTextCat = "全部";
                            _currentVideoCat = "全部";
                            RefreshCategoryViews();
                            RefreshList();

                            int textCnt = _data.Phrases.Count(p => string.IsNullOrEmpty(p.ItemType) || p.ItemType != "video");
                            int vidCnt = _data.Phrases.Count(p => p.ItemType == "video");
                            ShowModernAlert(
                                "恢复成功",
                                string.Format("恢复完成！\n已成功复原 {0} 条文本话术、{1} 条视频快捷方式。", textCnt, vidCnt));
                        });
                    }
                    catch (Exception ex)
                    {
                        ShowModernAlert("恢复失败", "恢复备份时发生异常: " + ex.Message);
                    }
                    finally
                    {
                        try { if (Directory.Exists(tempExtractDir)) Directory.Delete(tempExtractDir, true); } catch { }
                    }
                }
            }
        }

        private void ShowEasterEggDialog()
        {
            Image eggImg = EmbeddedAssets.GetAvatarImage(_avatarPath);
            if (eggImg == null) return;

            bool isDark = IsDarkModeActive();

            using (Form eggForm = new Form())
            {
                Panel hdr; ModernButton xBtn;
                PrepareModernDialog(eggForm, "客服花花 · 原画立绘", 340, 412, out hdr, out xBtn);

                PictureBox pic = new PictureBox
                {
                    Location = new Point(16, 48),
                    Size = new Size(308, 300),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = eggImg,
                    BackColor = Color.Transparent
                };

                Label lblText = new Label
                {
                    Location = new Point(16, 354),
                    Size = new Size(308, 44),
                    Text = "「正在努力帮主人回复客户呢！」\n点击图片或按 Esc 返回",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = WarmTheme.CreateFont(9F, FontStyle.Regular),
                    ForeColor = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary
                };

                eggForm.Controls.AddRange(new Control[] { pic, lblText });

                pic.Click += (s, e) => eggForm.Close();
                lblText.Click += (s, e) => eggForm.Close();

                eggForm.ShowDialog(this);
            }
        }

        private void StartTracking()
        {
            _windowTrackerTimer = new System.Windows.Forms.Timer { Interval = 150 };
            _windowTrackerTimer.Tick += (s, e) =>
            {
                IntPtr fg = GetForegroundWindow();
                if (fg != IntPtr.Zero && fg != this.Handle)
                {
                    StringBuilder sb = new StringBuilder(256);
                    GetWindowText(fg, sb, 256);
                    string title = sb.ToString().Trim();
                    if (!string.IsNullOrEmpty(title) && !title.Contains("客服花花"))
                    {
                        _lastTargetHwnd = fg;
                        _lastTargetTitle = title;
                        string shortTitle = title.Length > 14 ? title.Substring(0, 12) + ".." : title;
                        _lblTarget.Text = "目标: " + shortTitle;
                        bool isQianniu = title.Contains("千牛") || title.Contains("旺旺");
                        bool isDark = IsDarkModeActive();
                        _lblTarget.ForeColor = isQianniu
                            ? (isDark ? WarmTheme.OrangeHover : WarmTheme.Orange)
                            : (isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary);
                    }
                }
            };
            _windowTrackerTimer.Start();
        }

        private void RefreshList()
        {
            string keyword = (_searchBox.Text == SearchPlaceholder) ? "" : _searchBox.Text.Trim();
            string catFilter = ActiveCat;
            string modeFilter = _currentMode;
            bool hasKeyword = !string.IsNullOrEmpty(keyword);

            Func<PhraseItem, bool> matchFunc = (item) =>
            {
                string itemType = string.IsNullOrEmpty(item.ItemType) ? "text" : item.ItemType;
                if (itemType != modeFilter)
                    return false;

                if (catFilter != "全部" && item.Category != catFilter)
                    return false;

                if (hasKeyword)
                {
                    bool matchTitle = item.Title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool matchContent = item.Content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool matchCat = !string.IsNullOrEmpty(item.Category) && item.Category.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!matchTitle && !matchContent && !matchCat) return false;
                }
                return true;
            };

            List<PhraseItem> filtered;
            if (_data.Phrases.Count > 150)
            {
                filtered = _data.Phrases.AsParallel().AsOrdered().Where(matchFunc).ToList();
            }
            else
            {
                filtered = _data.Phrases.Where(matchFunc).ToList();
            }

            _modeBar.ItemCount = filtered.Count;
            _modeBar.Invalidate();
            _phraseList.UpdateData(filtered);
        }

        private void SendToTarget(PhraseItem item, bool sendEnter)
        {
            if (_lastTargetHwnd == IntPtr.Zero)
            {
                ShowModernAlert("提示", "尚未捕获聊天窗口！\n请先鼠标点一下千牛或微信输入框，然后再点发送。");
                return;
            }

            bool isVideo = (item.ItemType == "video");
            string fullVideoPath = isVideo ? ResolveMediaPath(item.VideoPath) : "";
            bool videoFileValid = false;
            if (isVideo && !string.IsNullOrEmpty(fullVideoPath))
            {
                videoFileValid = SafeFileExists(fullVideoPath);
                if (!videoFileValid)
                {
                    ShowModernAlert("找不到视频", "找不到视频文件：\n" + fullVideoPath);
                    return;
                }
            }

            bool hasImage = !isVideo && !string.IsNullOrEmpty(item.ImagePath);
            string fullImgPath = hasImage ? ResolveMediaPath(item.ImagePath) : "";
            bool imgFileValid = hasImage && SafeFileExists(fullImgPath);
            if (hasImage && !imgFileValid)
            {
                ShowModernAlert("找不到图片", "找不到图片文件：\n" + fullImgPath);
                return;
            }
            bool hasText = !string.IsNullOrEmpty(item.Content);

            // 1. 先发送文本内容
            if (hasText)
            {
                try { Clipboard.SetDataObject(item.Content, true); }
                catch { Thread.Sleep(30); Clipboard.SetDataObject(item.Content, true); }

                SetForegroundWindow(_lastTargetHwnd);
                Thread.Sleep(50);

                keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                if (sendEnter)
                {
                    Thread.Sleep(30);
                    keybd_event(VK_RETURN, 0, 0, UIntPtr.Zero);
                    keybd_event(VK_RETURN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                }

                if (imgFileValid || videoFileValid) Thread.Sleep(150);
            }

            // 2. 紧接着发送图片
            if (imgFileValid)
            {
                try
                {
                    DataObject dataObj = new DataObject();
                    using (Image img = Image.FromFile(fullImgPath))
                    {
                        dataObj.SetData(DataFormats.Bitmap, true, new Bitmap(img));
                    }
                    var fileList = new System.Collections.Specialized.StringCollection();
                    fileList.Add(fullImgPath);
                    dataObj.SetFileDropList(fileList);

                    Clipboard.SetDataObject(dataObj, true);
                }
                catch { }

                SetForegroundWindow(_lastTargetHwnd);
                Thread.Sleep(60);

                keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                if (sendEnter)
                {
                    Thread.Sleep(80);
                    keybd_event(VK_RETURN, 0, 0, UIntPtr.Zero);
                    keybd_event(VK_RETURN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                }
            }

            // 3. 发送视频快捷方式（FileDrop 剪贴板粘贴）
            if (videoFileValid)
            {
                try
                {
                    var fileList = new System.Collections.Specialized.StringCollection();
                    fileList.Add(fullVideoPath);
                    Clipboard.SetFileDropList(fileList);
                }
                catch
                {
                    try
                    {
                        Thread.Sleep(40);
                        var fileList = new System.Collections.Specialized.StringCollection();
                        fileList.Add(fullVideoPath);
                        Clipboard.SetFileDropList(fileList);
                    }
                    catch { }
                }

                SetForegroundWindow(_lastTargetHwnd);
                Thread.Sleep(80);

                keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                if (sendEnter)
                {
                    Thread.Sleep(260);
                    keybd_event(VK_RETURN, 0, 0, UIntPtr.Zero);
                    keybd_event(VK_RETURN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                }
            }
        }

        private void AddCategoryPrompt()
        {
            string title = (_currentMode == "video") ? "新建视频分类" : "新建文本分类";
            string name = PromptInput(title, "请输入新分类名称：", "");
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                var list = ActiveCategories();
                if (list.Contains(name))
                {
                    ShowModernAlert("提示", "该分类已存在！");
                    return;
                }
                list.Add(name);
                ActiveCat = name;
                SaveData();
                RefreshCategoryViews();
                RefreshList();
            }
        }

        private void DeleteCurrentCategoryPrompt()
        {
            if (ActiveCat == "全部")
            {
                ShowModernAlert("提示", "【全部】为系统默认聚合分类，不可删除。\n请先选择想要删除的具体分类，然后再点击删除。");
                return;
            }

            DeleteCategory(ActiveCat);
        }

        private void RenameCategoryPrompt(string oldName)
        {
            string newName = PromptInput("修改分类名称", "请输入新的分类名称：", oldName);
            if (!string.IsNullOrWhiteSpace(newName) && newName != oldName)
            {
                newName = newName.Trim();
                var list = ActiveCategories();
                int idx = list.IndexOf(oldName);
                if (idx >= 0) list[idx] = newName;

                foreach (var p in _data.Phrases)
                {
                    string itemType = string.IsNullOrEmpty(p.ItemType) ? "text" : p.ItemType;
                    if (itemType == _currentMode && p.Category == oldName) p.Category = newName;
                }

                if (ActiveCat == oldName) ActiveCat = newName;
                SaveData();
                RefreshCategoryViews();
                RefreshList();
            }
        }

        private void DeleteCategory(string cat)
        {
            if (ShowModernConfirm("确认删除分类", "确定删除分类【" + cat + "】吗？\n该分类下的内容将自动归入【未分类】。", true, "删除分类"))
            {
                var list = ActiveCategories();
                list.Remove(cat);
                foreach (var p in _data.Phrases)
                {
                    string itemType = string.IsNullOrEmpty(p.ItemType) ? "text" : p.ItemType;
                    if (itemType == _currentMode && p.Category == cat) p.Category = "未分类";
                }
                ActiveCat = "全部";
                SaveData();
                RefreshCategoryViews();
                RefreshList();
            }
        }

        private void ShowAddOrEditDialog(PhraseItem editItem)
        {
            bool isEditing = (editItem != null);
            bool isVideoMode = isEditing ? (editItem.ItemType == "video") : (_currentMode == "video");
            bool isDark = IsDarkModeActive();
            Color cardBg = isDark ? WarmTheme.DarkSecondaryBtnBg : WarmTheme.LightCardBg;
            Color bdrCol = isDark ? WarmTheme.HairlineDark : WarmTheme.BorderSecondaryLight;
            Color txtCol = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
            Color secCol = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary;

            using (Form dlg = new Form())
            {
                string titleText = isVideoMode
                    ? (isEditing ? "修改视频快捷方式" : "添加视频快捷方式")
                    : (isEditing ? "修改话术" : "添加话术");

                Panel hdr; ModernButton xBtn;
                PrepareModernDialog(dlg, titleText, 430, 444, out hdr, out xBtn);
                dlg.AllowDrop = true;

                Label lblCat = new Label { Text = "分类", Location = new Point(20, 56), ForeColor = secCol, AutoSize = true, Font = WarmTheme.CreateFont(9F) };
                ComboBox cmbCat = new ComboBox
                {
                    Location = new Point(106, 52),
                    Width = 300,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = cardBg,
                    ForeColor = txtCol,
                    Font = WarmTheme.CreateFont(9.5F)
                };

                var dialogCats = isVideoMode ? _data.VideoCategories : _data.Categories;
                if (dialogCats.Count == 0) cmbCat.Items.Add("未分类");
                else foreach (var c in dialogCats) cmbCat.Items.Add(c);

                if (isEditing)
                {
                    if (!cmbCat.Items.Contains(editItem.Category)) cmbCat.Items.Add(editItem.Category);
                    cmbCat.SelectedItem = editItem.Category;
                }
                else if (ActiveCat != "全部" && cmbCat.Items.Contains(ActiveCat))
                {
                    cmbCat.SelectedItem = ActiveCat;
                }
                else
                {
                    cmbCat.SelectedIndex = 0;
                }

                Label lblTitle = new Label { Text = isVideoMode ? "视频名称" : "提示词", Location = new Point(20, 96), ForeColor = secCol, AutoSize = true, Font = WarmTheme.CreateFont(9F) };
                ModernTextBox txtTitle = new ModernTextBox
                {
                    Location = new Point(104, 88),
                    Size = new Size(304, 34),
                    IsDarkTheme = isDark,
                    Text = isEditing ? editItem.Title : ""
                };

                Label lblContent = new Label { Text = isVideoMode ? "附带文字" : "话术正文", Location = new Point(20, 138), ForeColor = secCol, AutoSize = true, Font = WarmTheme.CreateFont(9F) };
                ModernTextBox txtContent = new ModernTextBox
                {
                    Location = new Point(104, 130),
                    Size = new Size(304, 102),
                    Multiline = true,
                    IsDarkTheme = isDark,
                    Text = isEditing ? editItem.Content : ""
                };

                Label lblImg = new Label { Text = isVideoMode ? "视频文件" : "附带图片", Location = new Point(20, 248), ForeColor = secCol, AutoSize = true, Font = WarmTheme.CreateFont(9F) };

                Panel pnlDropBox = new Panel
                {
                    Location = new Point(106, 244),
                    Size = new Size(92, 92),
                    BackColor = cardBg,
                    AllowDrop = true
                };
                pnlDropBox.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    Rectangle r = new Rectangle(1, 1, pnlDropBox.Width - 2, pnlDropBox.Height - 2);
                    using (GraphicsPath gp = GlassRenderer.CreateRoundedPath(r, WarmTheme.RadiusControl))
                    using (Pen pen = new Pen(bdrCol, 1))
                    {
                        pen.DashStyle = DashStyle.Dash;
                        e.Graphics.DrawPath(pen, gp);
                    }
                };

                PictureBox picPreview = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent,
                    AllowDrop = true,
                    Visible = false,
                    Padding = new Padding(4)
                };
                pnlDropBox.Controls.Add(picPreview);

                Label lblDropHint = new Label
                {
                    Text = isVideoMode ? "将视频\n拖入此处" : "将图片\n拖入此处",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = WarmTheme.CreateFont(8.5F),
                    ForeColor = secCol,
                    Visible = true
                };
                pnlDropBox.Controls.Add(lblDropHint);
                lblDropHint.BringToFront();

                string currentSelectedImgPath = isEditing ? editItem.ImagePath : "";
                string currentSelectedVideoPath = isEditing ? editItem.VideoPath : "";
                Action updatePreview = () =>
                {
                    if (isVideoMode)
                    {
                        picPreview.Image = null;
                        picPreview.Visible = false;
                        lblDropHint.Visible = true;
                        lblDropHint.BringToFront();
                        if (!string.IsNullOrEmpty(currentSelectedVideoPath))
                        {
                            string fullV = ResolveMediaPath(currentSelectedVideoPath);
                            if (SafeFileExists(fullV))
                            {
                                long lenKb = new FileInfo(fullV).Length / 1024;
                                string sizeText = lenKb >= 1024 ? string.Format("{0:F1}MB", lenKb / 1024.0) : (lenKb + "KB");
                                string extTag = SafeGetExtension(fullV).TrimStart('.').ToUpper();
                                lblDropHint.Text = extTag + " 视频\n(" + sizeText + ")";
                                lblDropHint.ForeColor = WarmTheme.Orange;
                                return;
                            }
                        }
                        lblDropHint.Text = "将视频\n拖入此处";
                        lblDropHint.ForeColor = secCol;
                        return;
                    }

                    if (!string.IsNullOrEmpty(currentSelectedImgPath))
                    {
                        string fullP = ResolveMediaPath(currentSelectedImgPath);
                        if (SafeFileExists(fullP))
                        {
                            try
                            {
                                using (var temp = Image.FromFile(fullP))
                                {
                                    picPreview.Image = new Bitmap(temp);
                                }
                                picPreview.Visible = true;
                                picPreview.BringToFront();
                                lblDropHint.Visible = false;
                                return;
                            }
                            catch { }
                        }
                    }
                    picPreview.Image = null;
                    picPreview.Visible = false;
                    lblDropHint.Visible = true;
                    lblDropHint.BringToFront();
                };

                updatePreview();

                DragEventHandler handleDragEnter = (s, e) =>
                {
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                        e.Effect = DragDropEffects.Copy;
                    else
                        e.Effect = DragDropEffects.None;
                };

                DragEventHandler handleDragDrop = (s, e) =>
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files != null && files.Length > 0)
                    {
                        string file = files[0];
                        string ext = SafeGetExtension(file).ToLower();
                        if (isVideoMode)
                        {
                            if (ext == ".mp4" || ext == ".mov" || ext == ".avi" || ext == ".mkv" || ext == ".wmv" || ext == ".flv" || ext == ".webm")
                            {
                                try
                                {
                                    currentSelectedVideoPath = ImportMediaFile(file, true);
                                    updatePreview();
                                }
                                catch (Exception ex)
                                {
                                    ShowModernAlert("导入视频失败", ex.Message);
                                }
                            }
                            else
                            {
                                ShowModernAlert("格式提示", "请拖入视频文件（支持 mp4 / mov / avi / mkv / wmv / flv / webm）。");
                            }
                        }
                        else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp" || ext == ".webp")
                        {
                            try
                            {
                                currentSelectedImgPath = ImportMediaFile(file, false);
                                updatePreview();
                            }
                            catch (Exception ex)
                            {
                                ShowModernAlert("导入图片失败", ex.Message);
                            }
                        }
                    }
                };

                dlg.DragEnter += handleDragEnter;
                dlg.DragDrop += handleDragDrop;
                pnlDropBox.DragEnter += handleDragEnter;
                pnlDropBox.DragDrop += handleDragDrop;
                picPreview.DragEnter += handleDragEnter;
                picPreview.DragDrop += handleDragDrop;
                lblDropHint.AllowDrop = true;
                lblDropHint.DragEnter += handleDragEnter;
                lblDropHint.DragDrop += handleDragDrop;

                ModernButton btnUploadImg = new ModernButton
                {
                    Text = isVideoMode ? "选择视频" : "本地选择",
                    Location = new Point(210, 244),
                    Size = new Size(92, 32),
                    Variant = ButtonVariant.Secondary,
                    IsDarkTheme = isDark,
                    TextColor = txtCol,
                    CornerRadius = WarmTheme.RadiusControl,
                    IconSize = 14,
                    IconTextGap = 6,
                    IconPainter = isVideoMode ? (Action<Graphics, Rectangle, Color>)GlassRenderer.DrawVectorVideoIcon : GlassRenderer.DrawVectorImageIcon
                };

                ModernButton btnPasteImg = new ModernButton
                {
                    Text = "粘贴图片",
                    Location = new Point(308, 244),
                    Size = new Size(98, 32),
                    Variant = ButtonVariant.Secondary,
                    IsDarkTheme = isDark,
                    TextColor = txtCol,
                    CornerRadius = WarmTheme.RadiusControl,
                    IconSize = 14,
                    IconTextGap = 6,
                    IconPainter = GlassRenderer.DrawVectorClipboardIcon,
                    Visible = !isVideoMode
                };

                ModernButton btnClearImg = new ModernButton
                {
                    Text = isVideoMode ? "清除视频" : "清除图片",
                    Location = new Point(210, 282),
                    Size = new Size(92, 32),
                    Variant = ButtonVariant.Destructive,
                    IsDarkTheme = isDark,
                    CornerRadius = WarmTheme.RadiusControl
                };

                btnUploadImg.Click += (s, e) =>
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        if (isVideoMode)
                        {
                            ofd.Title = "选择视频文件";
                            ofd.Filter = "视频文件 (*.mp4;*.mov;*.avi;*.mkv;*.wmv;*.flv;*.webm)|*.mp4;*.mov;*.avi;*.mkv;*.wmv;*.flv;*.webm|所有文件 (*.*)|*.*";
                            if (ofd.ShowDialog(dlg) == DialogResult.OK)
                            {
                                try
                                {
                                    currentSelectedVideoPath = ImportMediaFile(ofd.FileName, true);
                                    updatePreview();
                                }
                                catch (Exception ex) { ShowModernAlert("加载视频失败", ex.Message); }
                            }
                        }
                        else
                        {
                            ofd.Title = "选择话术图片";
                            ofd.Filter = "图片文件 (*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp";
                            if (ofd.ShowDialog(dlg) == DialogResult.OK)
                            {
                                try
                                {
                                    currentSelectedImgPath = ImportMediaFile(ofd.FileName, false);
                                    updatePreview();
                                }
                                catch (Exception ex) { ShowModernAlert("加载图片失败", ex.Message); }
                            }
                        }
                    }
                };

                btnPasteImg.Click += (s, e) =>
                {
                    bool recognized = false;
                    try
                    {
                        IDataObject iData = Clipboard.GetDataObject();
                        if (iData != null)
                        {
                            if (iData.GetDataPresent(DataFormats.Bitmap))
                            {
                                Bitmap bmp = (Bitmap)iData.GetData(DataFormats.Bitmap);
                                if (bmp != null)
                                {
                                    if (!Directory.Exists(_imagesDir)) Directory.CreateDirectory(_imagesDir);
                                    string newName = "images/" + Guid.NewGuid().ToString("N") + ".png";
                                    string targetFull = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, newName);
                                    bmp.Save(targetFull, System.Drawing.Imaging.ImageFormat.Png);

                                    currentSelectedImgPath = newName;
                                    updatePreview();
                                    recognized = true;
                                }
                            }

                            if (!recognized && iData.GetDataPresent(DataFormats.FileDrop))
                            {
                                string[] files = (string[])iData.GetData(DataFormats.FileDrop);
                                if (files != null && files.Length > 0)
                                {
                                    string ext = SafeGetExtension(files[0]).ToLower();
                                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp" || ext == ".webp")
                                    {
                                        currentSelectedImgPath = ImportMediaFile(files[0], false);
                                        updatePreview();
                                        recognized = true;
                                    }
                                }
                            }

                            if (!recognized && Clipboard.ContainsImage())
                            {
                                Image img = Clipboard.GetImage();
                                if (img != null)
                                {
                                    if (!Directory.Exists(_imagesDir)) Directory.CreateDirectory(_imagesDir);
                                    string newName = "images/" + Guid.NewGuid().ToString("N") + ".png";
                                    string targetFull = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, newName);
                                    img.Save(targetFull, System.Drawing.Imaging.ImageFormat.Png);

                                    currentSelectedImgPath = newName;
                                    updatePreview();
                                    recognized = true;
                                }
                            }
                        }
                    }
                    catch { }

                    if (!recognized)
                    {
                        ShowModernAlert("提示", "未能在剪贴板中检测到图片！\n您也可以直接将图片拖进左侧方框中。");
                    }
                };

                btnClearImg.Click += (s, e) =>
                {
                    if (isVideoMode) currentSelectedVideoPath = "";
                    else currentSelectedImgPath = "";
                    updatePreview();
                };

                // 底部发丝分割 + 主次确认按钮（高度 34px，圆角 9px）
                Panel footDiv = new Panel { Location = new Point(20, 356), Size = new Size(390, 1), BackColor = bdrCol };

                ModernButton btnSave = new ModernButton
                {
                    Text = isVideoMode ? "保存视频" : "保存话术",
                    Location = new Point(228, 372),
                    Size = new Size(94, WarmTheme.BtnHeightDialog),
                    Variant = ButtonVariant.Primary,
                    IsDarkTheme = isDark,
                    AccentColor = WarmTheme.Orange,
                    CornerRadius = WarmTheme.RadiusDialogBtn,
                    Font = WarmTheme.CreateFont(9.5F, FontStyle.Bold),
                    DialogResult = DialogResult.OK
                };

                ModernButton btnCancel = new ModernButton
                {
                    Text = "取消",
                    Location = new Point(330, 372),
                    Size = new Size(76, WarmTheme.BtnHeightDialog),
                    Variant = ButtonVariant.Secondary,
                    IsDarkTheme = isDark,
                    TextColor = txtCol,
                    CornerRadius = WarmTheme.RadiusDialogBtn,
                    DialogResult = DialogResult.Cancel
                };

                dlg.Load += (s, e) =>
                {
                    int maxR = Math.Max(Math.Max(lblCat.Right, lblTitle.Right), Math.Max(lblContent.Right, lblImg.Right));
                    int inputX = Math.Max(94, maxR + 12);
                    int rightEdge = dlg.ClientSize.Width - 20;
                    int fieldW = Math.Max(190, rightEdge - inputX);

                    cmbCat.Left = inputX + 2; cmbCat.Width = fieldW - 4;
                    txtTitle.Left = inputX; txtTitle.Width = fieldW;
                    txtContent.Left = inputX; txtContent.Width = fieldW;
                    pnlDropBox.Left = inputX + 2;
                    btnUploadImg.Left = pnlDropBox.Right + 10;
                    btnPasteImg.Left = btnUploadImg.Right + 6;
                    btnClearImg.Left = pnlDropBox.Right + 10;
                    footDiv.Location = new Point(20, 356);
                    footDiv.Width = dlg.ClientSize.Width - 40;
                    btnCancel.Left = rightEdge - btnCancel.Width;
                    btnSave.Left = btnCancel.Left - btnSave.Width - 8;
                };

                if (isEditing)
                {
                    ModernButton btnDel = new ModernButton
                    {
                        Text = "删除",
                        Location = new Point(20, 372),
                        Size = new Size(76, WarmTheme.BtnHeightDialog),
                        Variant = ButtonVariant.Destructive,
                        IsDarkTheme = isDark,
                        CornerRadius = WarmTheme.RadiusDialogBtn,
                        IconSize = 13,
                        IconTextGap = 5,
                        IconPainter = GlassRenderer.DrawVectorTrashIcon
                    };
                    btnDel.Click += (s, e) =>
                    {
                        if (ShowModernConfirm("删除条目", string.Format("确定永久删除【{0}】吗？", editItem.Title), true, "删除"))
                        {
                            _data.Phrases.Remove(editItem);
                            SaveData();
                            dlg.DialogResult = DialogResult.Abort;
                            dlg.Close();
                        }
                    };
                    dlg.Controls.Add(btnDel);
                }

                dlg.Controls.AddRange(new Control[] {
                    lblCat, cmbCat, lblTitle, txtTitle, lblContent, txtContent,
                    lblImg, pnlDropBox, btnUploadImg, btnPasteImg, btnClearImg,
                    footDiv, btnSave, btnCancel
                });

                var res = dlg.ShowDialog(this);
                if (res == DialogResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(txtTitle.Text))
                    {
                        ShowModernAlert("提示", isVideoMode ? "视频名称不能为空！" : "提示词不能为空！");
                        return;
                    }
                    if (isVideoMode)
                    {
                        if (string.IsNullOrEmpty(currentSelectedVideoPath))
                        {
                            ShowModernAlert("提示", "请先选择或拖入一个视频文件！");
                            return;
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(txtContent.Text) && string.IsNullOrEmpty(currentSelectedImgPath))
                    {
                        ShowModernAlert("提示", "话术正文和附带图片至少需要填写一项！");
                        return;
                    }

                    string selectedCategory = cmbCat.SelectedItem != null ? cmbCat.SelectedItem.ToString() : "未分类";

                    if (isEditing)
                    {
                        editItem.Category = selectedCategory;
                        editItem.Title = txtTitle.Text.Trim();
                        editItem.Content = txtContent.Text.Trim();
                        editItem.ImagePath = isVideoMode ? "" : currentSelectedImgPath;
                        editItem.ItemType = isVideoMode ? "video" : "text";
                        editItem.VideoPath = isVideoMode ? currentSelectedVideoPath : "";
                    }
                    else
                    {
                        _data.Phrases.Insert(0, new PhraseItem(
                            selectedCategory,
                            txtTitle.Text.Trim(),
                            txtContent.Text.Trim(),
                            isVideoMode ? "" : currentSelectedImgPath,
                            isVideoMode ? "video" : "text",
                            isVideoMode ? currentSelectedVideoPath : ""));
                    }

                    SaveData();
                    RefreshCategoryViews();
                    RefreshList();
                }
                else if (res == DialogResult.Abort)
                {
                    RefreshCategoryViews();
                    RefreshList();
                }
            }
        }

        private string PromptInput(string title, string promptText, string defaultValue)
        {
            bool isDark = IsDarkModeActive();
            Color txtCol = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;

            using (Form form = new Form())
            {
                Panel hdr; ModernButton xBtn;
                PrepareModernDialog(form, title, 336, 168, out hdr, out xBtn);

                Label label = new Label { Left = 20, Top = 52, Text = promptText, ForeColor = txtCol, AutoSize = true, Font = WarmTheme.CreateFont(9F) };
                ModernTextBox textBox = new ModernTextBox
                {
                    Location = new Point(18, 74),
                    Size = new Size(300, 34),
                    Text = defaultValue,
                    IsDarkTheme = isDark
                };
                ModernButton btnOk = new ModernButton
                {
                    Text = "确定",
                    Location = new Point(144, 118),
                    Size = new Size(88, WarmTheme.BtnHeightDialog),
                    Variant = ButtonVariant.Primary,
                    IsDarkTheme = isDark,
                    AccentColor = WarmTheme.Orange,
                    CornerRadius = WarmTheme.RadiusDialogBtn,
                    Font = WarmTheme.CreateFont(9.5F, FontStyle.Bold),
                    DialogResult = DialogResult.OK
                };
                ModernButton btnCancel = new ModernButton
                {
                    Text = "取消",
                    Location = new Point(240, 118),
                    Size = new Size(76, WarmTheme.BtnHeightDialog),
                    Variant = ButtonVariant.Secondary,
                    IsDarkTheme = isDark,
                    TextColor = txtCol,
                    CornerRadius = WarmTheme.RadiusDialogBtn,
                    DialogResult = DialogResult.Cancel
                };

                textBox.InnerTextBox.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        form.DialogResult = DialogResult.OK;
                        form.Close();
                        e.Handled = true;
                    }
                };
                form.Shown += (s, e) => textBox.FocusInput();
                form.Controls.AddRange(new Control[] { label, textBox, btnOk, btnCancel });

                if (form.ShowDialog(this) == DialogResult.OK) return textBox.Text;
            }
            return null;
        }

        private void LoadData()
        {
            if (File.Exists(_storagePath))
            {
                try
                {
                    string json = File.ReadAllText(_storagePath, Encoding.UTF8);
                    bool repairedPaths = false;
                    var data = JsonHelper.Deserialize(json, out repairedPaths);
                    if (data != null)
                    {
                        _data = data;
                        if (repairedPaths) SaveData();
                        return;
                    }
                }
                catch { }
            }

            _data.Settings = new AppSettings();
            _data.Categories = new List<string>();
            _data.VideoCategories = new List<string>();
            _data.Phrases = new List<PhraseItem>();

            SaveData();
        }

        private void SaveData()
        {
            try
            {
                string json = JsonHelper.Serialize(_data);
                File.WriteAllText(_storagePath, json, Encoding.UTF8);
            }
            catch { }
        }
    }

    public static class JsonHelper
    {
        public static string Serialize(AppData data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n");

            // Settings
            sb.Append("  \"Settings\": {\n");
            sb.AppendFormat("    \"FontSizeLevel\": {0},\n", data.Settings.FontSizeLevel);
            sb.AppendFormat("    \"ThemeType\": \"{0}\",\n", Escape(data.Settings.ThemeType));
            sb.AppendFormat("    \"ColorScheme\": \"{0}\",\n", Escape(data.Settings.ColorScheme));
            sb.AppendFormat("    \"CustomBgPath\": \"{0}\",\n", Escape(data.Settings.CustomBgPath));
            sb.AppendFormat("    \"CopyMediaToLocal\": {0},\n", data.Settings.CopyMediaToLocal ? "true" : "false");
            sb.AppendFormat("    \"WindowWidth\": {0},\n", data.Settings.WindowWidth);
            sb.AppendFormat("    \"WindowHeight\": {0},\n", data.Settings.WindowHeight);
            sb.AppendFormat("    \"WindowX\": {0},\n", data.Settings.WindowX);
            sb.AppendFormat("    \"WindowY\": {0},\n", data.Settings.WindowY);
            sb.AppendFormat("    \"HasWindowPos\": {0}\n", data.Settings.HasWindowPos ? "true" : "false");
            sb.Append("  },\n");

            // Categories
            sb.Append("  \"Categories\": [");
            for (int i = 0; i < data.Categories.Count; i++)
            {
                sb.AppendFormat("\"{0}\"", Escape(data.Categories[i]));
                if (i < data.Categories.Count - 1) sb.Append(", ");
            }
            sb.Append("],\n");

            // VideoCategories
            sb.Append("  \"VideoCategories\": [");
            for (int i = 0; i < data.VideoCategories.Count; i++)
            {
                sb.AppendFormat("\"{0}\"", Escape(data.VideoCategories[i]));
                if (i < data.VideoCategories.Count - 1) sb.Append(", ");
            }
            sb.Append("],\n");

            // Phrases
            sb.Append("  \"Phrases\": [\n");
            for (int i = 0; i < data.Phrases.Count; i++)
            {
                var p = data.Phrases[i];
                sb.Append("    {\n");
                sb.AppendFormat("      \"Id\": \"{0}\",\n", Escape(p.Id));
                sb.AppendFormat("      \"Category\": \"{0}\",\n", Escape(p.Category));
                sb.AppendFormat("      \"Title\": \"{0}\",\n", Escape(p.Title));
                sb.AppendFormat("      \"Content\": \"{0}\",\n", Escape(p.Content));
                sb.AppendFormat("      \"ImagePath\": \"{0}\",\n", Escape(p.ImagePath));
                sb.AppendFormat("      \"ItemType\": \"{0}\",\n", Escape(string.IsNullOrEmpty(p.ItemType) ? "text" : p.ItemType));
                sb.AppendFormat("      \"VideoPath\": \"{0}\"\n", Escape(p.VideoPath));
                sb.Append("    }");
                if (i < data.Phrases.Count - 1) sb.Append(",");
                sb.Append("\n");
            }
            sb.Append("  ]\n");
            sb.Append("}");
            return sb.ToString();
        }

        public static AppData Deserialize(string json)
        {
            bool repaired = false;
            return Deserialize(json, out repaired);
        }

        public static AppData Deserialize(string json, out bool repairedPaths)
        {
            repairedPaths = false;
            if (string.IsNullOrWhiteSpace(json)) return null;

            int pos = 0;
            object rootObj = ParseValue(json, ref pos);
            Dictionary<string, object> root = rootObj as Dictionary<string, object>;
            if (root == null) return null;

            AppData appData = new AppData();

            Dictionary<string, object> settingsObj = GetDict(root, "Settings");
            if (settingsObj != null)
            {
                int iVal;
                if (TryGetInt(settingsObj, "FontSizeLevel", out iVal)) appData.Settings.FontSizeLevel = iVal;

                string theme = GetString(settingsObj, "ThemeType");
                if (!string.IsNullOrEmpty(theme)) appData.Settings.ThemeType = theme;

                string scheme = GetString(settingsObj, "ColorScheme");
                if (!string.IsNullOrEmpty(scheme)) appData.Settings.ColorScheme = scheme;

                string rawCustomBg = GetString(settingsObj, "CustomBgPath");
                bool bgChanged;
                string fixedCustomBg = RepairLegacyPath(rawCustomBg, out bgChanged);
                if (bgChanged) repairedPaths = true;
                if (!string.IsNullOrEmpty(fixedCustomBg)) appData.Settings.CustomBgPath = fixedCustomBg;

                bool bVal;
                if (TryGetBool(settingsObj, "CopyMediaToLocal", out bVal)) appData.Settings.CopyMediaToLocal = bVal;
                if (TryGetInt(settingsObj, "WindowWidth", out iVal)) appData.Settings.WindowWidth = iVal;
                if (TryGetInt(settingsObj, "WindowHeight", out iVal)) appData.Settings.WindowHeight = iVal;
                if (TryGetInt(settingsObj, "WindowX", out iVal)) appData.Settings.WindowX = iVal;
                if (TryGetInt(settingsObj, "WindowY", out iVal)) appData.Settings.WindowY = iVal;
                if (TryGetBool(settingsObj, "HasWindowPos", out bVal)) appData.Settings.HasWindowPos = bVal;
            }

            ReadStringList(root, "Categories", appData.Categories);
            ReadStringList(root, "VideoCategories", appData.VideoCategories);

            List<object> phrasesArr = GetArray(root, "Phrases");
            if (phrasesArr != null)
            {
                foreach (object entry in phrasesArr)
                {
                    Dictionary<string, object> itemDict = entry as Dictionary<string, object>;
                    if (itemDict == null) continue;

                    string id = GetString(itemDict, "Id");
                    string cat = GetString(itemDict, "Category");
                    string title = GetString(itemDict, "Title");
                    string content = GetString(itemDict, "Content");
                    string rawImgPath = GetString(itemDict, "ImagePath");
                    string itemType = GetString(itemDict, "ItemType");
                    string rawVideoPath = GetString(itemDict, "VideoPath");

                    bool imgChanged;
                    string imgPath = RepairLegacyPath(rawImgPath, out imgChanged);
                    if (imgChanged) repairedPaths = true;

                    bool vidChanged;
                    string videoPath = RepairLegacyPath(rawVideoPath, out vidChanged);
                    if (vidChanged) repairedPaths = true;

                    if (!string.IsNullOrEmpty(title))
                    {
                        PhraseItem item = new PhraseItem(cat, title, content, imgPath, itemType, videoPath);
                        if (IsSafeIdentifier(id)) item.Id = id;
                        appData.Phrases.Add(item);
                    }
                }
            }

            return appData;
        }

        /// <summary>
        /// 修复因旧版 Unescape 将 "\\n" / "\\r" 误替换为真实换行符而受损的文件路径
        /// </summary>
        public static string RepairLegacyPath(string rawPath)
        {
            bool changed;
            return RepairLegacyPath(rawPath, out changed);
        }

        public static string RepairLegacyPath(string rawPath, out bool changed)
        {
            changed = false;
            if (string.IsNullOrEmpty(rawPath)) return "";

            string trimmed = rawPath.Trim();
            if (trimmed.Length >= 2 && trimmed[0] == '\"' && trimmed[trimmed.Length - 1] == '\"')
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2).Trim();
            }
            if (trimmed != rawPath) changed = true;

            if (trimmed.IndexOf('\n') == -1 && trimmed.IndexOf('\r') == -1 && !HasInvalidFileSystemChars(trimmed))
            {
                return trimmed;
            }

            changed = true;
            // 构建主候选：把 "[1个以上反斜杠]+\n" 还原成 "\n"（盘符/普通路径分隔符）
            string primaryCandidate = RebuildBrokenControlChars(trimmed, false);
            if (SafeDiskFileExists(primaryCandidate)) return primaryCandidate;

            // 如果是 UNC 开头被破坏（如 \\newserver -> \<LF>ewserver），尝试起始保留双反斜杠候选
            string uncCandidate = RebuildBrokenControlChars(trimmed, true);
            if (uncCandidate != primaryCandidate && SafeDiskFileExists(uncCandidate)) return uncCandidate;

            return StripInvalidPathChars(primaryCandidate);
        }

        private static string RebuildBrokenControlChars(string input, bool doubleLeadingSlash)
        {
            StringBuilder sb = new StringBuilder(input.Length);
            int i = 0;
            while (i < input.Length)
            {
                char ch = input[i];
                if (ch == '\\')
                {
                    int slashStart = i;
                    while (i < input.Length && input[i] == '\\') i++;
                    int slashCount = i - slashStart;

                    if (i < input.Length && (input[i] == '\n' || input[i] == '\r'))
                    {
                        char mapped = (input[i] == '\n') ? 'n' : 'r';
                        if (doubleLeadingSlash && slashStart == 0)
                            sb.Append("\\\\");
                        else
                            sb.Append('\\');
                        sb.Append(mapped);
                        i++;
                        continue;
                    }

                    for (int k = 0; k < slashCount; k++) sb.Append('\\');
                    continue;
                }

                if (ch == '\n')
                {
                    sb.Append("\\n");
                    i++;
                    continue;
                }
                if (ch == '\r')
                {
                    sb.Append("\\r");
                    i++;
                    continue;
                }

                sb.Append(ch);
                i++;
            }
            return StripInvalidPathChars(sb.ToString());
        }

        private static string StripInvalidPathChars(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            char[] invalid = Path.GetInvalidPathChars();
            StringBuilder sb = new StringBuilder(path.Length);
            for (int i = 0; i < path.Length; i++)
            {
                char c = path[i];
                if (c < ' ') continue;
                if (c == '\"') continue;
                if (Array.IndexOf(invalid, c) >= 0) continue;
                sb.Append(c);
            }
            return sb.ToString().Trim();
        }

        private static bool HasInvalidFileSystemChars(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            char[] invalid = Path.GetInvalidPathChars();
            for (int i = 0; i < path.Length; i++)
            {
                char c = path[i];
                if (c < ' ') return true;
                if (c == '\"') return true;
                if (Array.IndexOf(invalid, c) >= 0) return true;
            }
            return false;
        }

        private static bool SafeDiskFileExists(string path)
        {
            if (string.IsNullOrEmpty(path) || HasInvalidFileSystemChars(path)) return false;
            try
            {
                string full = Path.IsPathRooted(path)
                    ? path
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                return File.Exists(full);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsSafeIdentifier(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];
                bool ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                          (c >= '0' && c <= '9') || c == '-' || c == '_';
                if (!ok) return false;
            }
            return true;
        }

        private static Dictionary<string, object> GetDict(Dictionary<string, object> obj, string key)
        {
            object v;
            if (obj != null && obj.TryGetValue(key, out v)) return v as Dictionary<string, object>;
            return null;
        }

        private static List<object> GetArray(Dictionary<string, object> obj, string key)
        {
            object v;
            if (obj != null && obj.TryGetValue(key, out v)) return v as List<object>;
            return null;
        }

        private static string GetString(Dictionary<string, object> obj, string key)
        {
            object v;
            if (obj == null || !obj.TryGetValue(key, out v) || v == null) return "";
            return v as string ?? v.ToString();
        }

        private static bool TryGetInt(Dictionary<string, object> obj, string key, out int result)
        {
            result = 0;
            object v;
            if (obj == null || !obj.TryGetValue(key, out v) || v == null) return false;
            if (v is int) { result = (int)v; return true; }
            if (v is long) { result = (int)(long)v; return true; }
            if (v is double) { result = (int)(double)v; return true; }
            return int.TryParse(v.ToString(), out result);
        }

        private static bool TryGetBool(Dictionary<string, object> obj, string key, out bool result)
        {
            result = false;
            object v;
            if (obj == null || !obj.TryGetValue(key, out v) || v == null) return false;
            if (v is bool) { result = (bool)v; return true; }
            string s = v.ToString().Trim().ToLower();
            if (s == "true") { result = true; return true; }
            if (s == "false") { result = false; return true; }
            return false;
        }

        private static void ReadStringList(Dictionary<string, object> obj, string key, List<string> target)
        {
            List<object> arr = GetArray(obj, key);
            if (arr == null) return;
            foreach (object item in arr)
            {
                if (item == null) continue;
                string s = item as string ?? item.ToString();
                if (!string.IsNullOrEmpty(s)) target.Add(s);
            }
        }

        private static void SkipWhitespace(string s, ref int pos)
        {
            while (pos < s.Length)
            {
                char c = s[pos];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\uFEFF')
                    pos++;
                else
                    break;
            }
        }

        private static object ParseValue(string s, ref int pos)
        {
            SkipWhitespace(s, ref pos);
            if (pos >= s.Length) return null;

            char c = s[pos];
            if (c == '{') return ParseObject(s, ref pos);
            if (c == '[') return ParseArray(s, ref pos);
            if (c == '\"') return ParseString(s, ref pos);
            if (c == 't' && pos + 4 <= s.Length && s.Substring(pos, 4) == "true")
            {
                pos += 4;
                return true;
            }
            if (c == 'f' && pos + 5 <= s.Length && s.Substring(pos, 5) == "false")
            {
                pos += 5;
                return false;
            }
            if (c == 'n' && pos + 4 <= s.Length && s.Substring(pos, 4) == "null")
            {
                pos += 4;
                return null;
            }
            if (c == '-' || (c >= '0' && c <= '9')) return ParseNumber(s, ref pos);

            // 容错跳过未识别字符
            pos++;
            return null;
        }

        private static Dictionary<string, object> ParseObject(string s, ref int pos)
        {
            var dict = new Dictionary<string, object>(StringComparer.Ordinal);
            pos++; // skip '{'
            while (pos < s.Length)
            {
                SkipWhitespace(s, ref pos);
                if (pos >= s.Length) break;
                if (s[pos] == '}') { pos++; break; }
                if (s[pos] == ',') { pos++; continue; }

                if (s[pos] != '\"')
                {
                    pos++;
                    continue;
                }

                string key = ParseString(s, ref pos);
                SkipWhitespace(s, ref pos);
                if (pos < s.Length && s[pos] == ':') pos++;
                object val = ParseValue(s, ref pos);
                dict[key] = val;

                SkipWhitespace(s, ref pos);
                if (pos < s.Length && s[pos] == ',') pos++;
            }
            return dict;
        }

        private static List<object> ParseArray(string s, ref int pos)
        {
            var list = new List<object>();
            pos++; // skip '['
            while (pos < s.Length)
            {
                SkipWhitespace(s, ref pos);
                if (pos >= s.Length) break;
                if (s[pos] == ']') { pos++; break; }
                if (s[pos] == ',') { pos++; continue; }

                object val = ParseValue(s, ref pos);
                list.Add(val);

                SkipWhitespace(s, ref pos);
                if (pos < s.Length && s[pos] == ',') pos++;
            }
            return list;
        }

        private static string ParseString(string s, ref int pos)
        {
            pos++; // skip opening '"'
            StringBuilder sb = new StringBuilder();
            while (pos < s.Length)
            {
                char c = s[pos++];
                if (c == '\"') return sb.ToString();
                if (c == '\\')
                {
                    if (pos >= s.Length) break;
                    char esc = s[pos++];
                    switch (esc)
                    {
                        case '\"': sb.Append('\"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (pos + 4 <= s.Length)
                            {
                                string hex = s.Substring(pos, 4);
                                int code;
                                if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out code))
                                {
                                    sb.Append((char)code);
                                    pos += 4;
                                }
                                else
                                {
                                    sb.Append("\\u");
                                }
                            }
                            else
                            {
                                sb.Append("\\u");
                            }
                            break;
                        default:
                            // 容错保留未知转义，避免解析中断
                            sb.Append('\\');
                            sb.Append(esc);
                            break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static object ParseNumber(string s, ref int pos)
        {
            int start = pos;
            if (s[pos] == '-') pos++;
            while (pos < s.Length)
            {
                char c = s[pos];
                if ((c >= '0' && c <= '9') || c == '.' || c == 'e' || c == 'E' || c == '+' || c == '-')
                    pos++;
                else
                    break;
            }
            string numStr = s.Substring(start, pos - start);
            long lVal;
            if (long.TryParse(numStr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out lVal))
                return lVal;
            double dVal;
            if (double.TryParse(numStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out dVal))
                return dVal;
            return numStr;
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            StringBuilder sb = new StringBuilder(s.Length + 8);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '\"': sb.Append("\\\""); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ')
                            sb.AppendFormat("\\u{0:X4}", (int)c);
                        else
                            sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log"), ex.ToString());
            }
        }
    }


    public static class EmbeddedAssets
    {
        public static readonly string AvatarBase64 = "iVBORw0KGgoAAAANSUhEUgAAAIAAAACACAYAAADDPmHLAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEE0AABBNAWeMAeAAAE+FSURBVHhe7b3nV1xZniU6f8O8+fJWr/dhemZW+37zpqunqmu603tlpTfKVMqmvBfyyHuQhDDCChAIkAGE9957731A4D1EEIGH/fbvRAQKkShLyspKqVZKuXbe4MZ1cfb+uXPPufc/jY6OaiYmJkZHRkZe4lcE4Vy4FwEYZmZmwBUv8SuCcC7c/yezGsDlS/yKYOZ89KUAfqV4KYBfOV4K4FeOlwL4leOlAH7leCmAXzleCuBXjpcC+JXjzyaA0dHRx7DSNi/x/PGzCWBYlqMjGBsbw/jYOHTjOuj1eoyNj2Ncr4NOp4eO68eeVQw85iiPOTZOcDk68lJMPyd+NgEoknTjGBoZRptGg/LyMmRnZSEtJRVZGZkoLSlFh7YDI2OjShTDJHal4zwGikVI7+nqRl1tLVpaWjA0NPTSo/yM+FkEIBY/OjyCuqpqhD8Iwc3rN3Dx+CmcPnAYJ/cfwkmbw7hw4jQ8Xd2QmpwMrVarxCIEr3Q8C0QoHW0ahAbexY3LdvBwvokciml48KUIfi78aQIgCeM6nSI0LiwS10+dx4kd+2C7bQ+xFye278VJQpbHiWPb9yhR+HreQm1tHcOC7olEyvrh4WElKNvdB3Bsx14c2bEHjpfs0FRbj3GKY6X9XuLZ8CcJYIJxva2lFbe9vHFs5z4c27obtiTqxI79OLlzP04JKIhT2/fhJAVxipClCMHR7hoqKyqUCNTxSLb1sWV9W1sb7M5dxIHNO3B81z4lgGO79yMzKVXlGH/Mg7zEH8dPEoDEbz0J6G7RwNfVA4d37sFxWr2yfitYLN+WEE8g5J/eTlFweWTbbjheuYrGhgbomCyKtVufY8IwofIGW4aPg9t24QgFcJjiOrhjN+IjoimAPx5CXuKP49kFwEaX+D3c249Q3wBlzUL+SSHcTPwx/n1kyy61lHWytN2yG6e3iTegJ9hq8gYiAl93L3R3d2OUWb7lHOL+9RN6lUAeFcvnOY7s3EsB7FFiiAp+qATwMg/40/HMApBGF/ecTTd8areNcvsW4sXaj/Dvk3tscPPyVTz0C0JMSBju+QXA7sRZlRsI8acoBCUGfj7J+B4fHUNhmY5tOYckgGEhoUokx2n5RwUUwsGtLwXwc+KpBKBqb2b54qYl9kpm7nrlGo5s3mly94z/Fsu/evIcMhOS0cXwMDYg2foYBoYGVbz3dLqpRCI5wWniDHOF4/QMNy5cQUtT81I+IMQOs5z09/LBoe93MIRQXDy+EgA9QExo+EsB/Ex4Og9A8gXS4NKZk56UgpN7D5pcPBM9If8oLd/l3BXUFpfBIJ0+JGhc+gYIydgn9BOKZPfrTor00+IJJFnkvif3HUQG3b0ls5fz9Pf3w/OGs1kApjxCvMFRhoLUuAToRSwvBfAn46kEYEnQpOQb6h/AHW9fHCYZQrxk+xIGzh88jpLMHBjH9RgdIzFml26BCMFgMKCqrAJXbE/jxGaKgJYt3uAw3fpdvzsYHBxU24n779R2qjAiXkaSSYGc5+S+QyjIzDYL4IfX+hLPhqfzAITF/Xe1d8DlqoOyRin3lOtnwhd6OwDjA8MqXEhvoFQK0jlkEYD0AIqA5Fj3AwIVmZILCI5yfy+Gh56eHrXNOJNMKS8dz16it9i11J9wnPuctTmKqqISkwCWXeNLPDueSgBC4ODwkHLrrY1NuHb2oin5E/cv1kkR5Kakw2DeXwnAsr+QbxaALHV6HVISk1Rid1KqAXoAEYATS8KOjg6VBwgaa+tx1faMEpeljJTP5w8fR0NljbrPYH2NL/HT8NQCUOTRA7Q1t8Dx4hVljWL9J0ighIEMJn5y00eIFut/EsZZ30t38OMC2K0EoKV3kTxAyK0pr8TFQ8eVOCwCkM+Xjp5Ea13DSwH8THg6AQhEALTMLq0WbtcdTcRQABYPEPUgVFm+xP+ViBeM6sYwxO8fBASpRFCVhBSAeBMPJnwqBJgFUF5YjDOM93Kek5Ismj3FtVPnoW1uXaoY1PXx2pRIzX+/xNPjqQSgumkZzyVBGxocQkjgXZZze5aSwLN7DyH87gP0Dw480QOMMq7rpHevuASXjp00dQyJ96AIpCPpgV8AhodEQGNKAIU5eTixiwmm2QOc4HaSEEr52d/R9VjFIKJTeCmCZ8ZTCoAkCjkkUhq+vKhY1ftioReZ/V8/eR5p8YmK6OUCkL9Vz+HAIArz8uFw4bIiXCxfcHLrHpzebYOspFSV2AmJExRAXnqmIl6FGi5tud0RloTeN1ww1N1nqhYI8RqxEZGIj4xGX3ePEtCKv+ElVsTTCcAK0ugDff1IiYmD51VHeNjfQNidu2hjcijduYp0bmchf4Trmpqa1DbnmMAd3mayaCUAVgDiCZzPX1HJpSSIsq8kk1nJqaY7iiTeIoDDFECQhzdGB4YwNjqmcpKkxETV+3hq1wGU5RYsVRov8XR4ZgEIhOjB3j7UFJehIrcQ7Q3NGGFokLrc2vrF8uvq6+HhclP16aubOjtMhC5ZP4lLiIhWSaYllsuNpsSoGFr/HuX6VT8AvYaEAOlenhgxdS5JZ5G7ozMOrN+CUzsPID89i15Er3KWla77JX6InyQAgcTcMdb8umHW9+ZhWsvJ13Z2wt/TW+UK0oOn7ulzKQIQ8iUJ9Hf1RGeHdulmkAhAho2F3w9WSZ8IwOIBjlIA0XeDMTmuV3mCiOviIVsc2bgdV4+fQWNNnepEeimAp8dPFoCQbLFa63VqvTnuZ2dm4dyBIzhBNy9l33EZMyBCoMuW+wC3HW6iWZHG0GE+hoQY6XQK8L5tqgBEALR+EYDsFx8SgckxHfS81szMTFWBHKUAbl60R3dnlwo5LwXw9PjJAlgJFgFIItY/PKTuAkoSp1y4+Z6B3AyS+j4q8AG0jSznhHARjfkYyrUPDcDL2RWHtzzqBhYByP4ZsYmY1huU8IL87yiRHPt+JwLdbmFgYECd++XA0afHn0cA4v57uuHh5KIEYBkrICWd4+mLKMrOwyCrAlXLm6sGyzGkt7FvoB+uDo44tHmHlQBMPY95KemYMU6q7N/p6nVa/w4c37wLCQ8j1fFUOfhSAE+Nn10AQ2YBdPZ24xatWNy3ECg9ebabdyPQxROD/QNqFNBKN3OUB6AliwcQAUj8l/1FPBJOqotKMWUwqhHCl8+cw6H1W1UpWlde+cRjvsST8WcRgMThvqFB3Lt9xxS/SeBpxvwzzPxv2TtC09KqMv0xuVlkhuUYkgCKG49+GE4B7DTdDKL1H2Kcdzx3GT1t7TDqJ5QHuOXqjuO79qteyLGhYZWTvBTAs+FnFYAF0t07ypo+PzcP5w8eU+7/7E4bXNp7BLdvMPFraFSu/knJmrjy5qYm3LS/rkrBY3TxF5g3qMGgUnVInCeaGptQWFCA7q6ux5LRPwWqCuGxxRMJfuy41tvKNctS/l5p2xcVP7sAhFQV0xkGunt7EHjbHxdtjsH+wHG4nLqIqKAH6GzvUFXCSvsLLA3bwDIvMuQhQgLuoig3X/VGStJoEYCl4f/URpfzybGktJQxC/K3eBjpZ5A2EVgLQT7Ltpb1fX19amh8J8te+V7lNuZtX3T87AKwYJheQEJBc0szokPCEODmhXDW8HWV1ep76wZdERSR3F0ck0kn/Ft5DB5vjJ9l35+LfDmGkC5laFl5OQICWLkcP46NmzZhx44d8PX1RVNz0xLZUrLK55qaGvjdvq223bZtGzZs2KC2P3PmDEpKSv5iRPCzC0Cs3xoSl7voCZooBJkaJuMKhFzJ1H+sXpeu3iFWCpIv9Hf3QM+/ZZyg3JgSD9Db1Y3ayir00lKXxh48JSwCmmQ10c1qJSoqCnv37sUHH3yAVcRGknns6FHs3bMH77/7Hj75+GOEhYWpoepyzX5+fli1ahX+8NEfsGvXLly8eBFu7m7YvHkz/uqv/gp2dnZLnmSl879I+NkFYI0lIUhOINZLt6+bMPXiCdREUfM21smgkN/f3YuI4FDYn7sIv5seaK2uN/U4ch8RRnjQfZw/dAwJkdEY57qnbWzxGMZJo3LvYQ/DlOW+8cYb+H7jJni6uSM7NR2N1bXQNDarcQc5SWmwPXgYr7/2GlJTU/Hg3n288h//gXPnz6OKXkCONzc/j6SUZHxMoRw+fBilpaVq/UsBWARASIOMk9halmvJ0XHqbl93u1atl+8t+0ijifvMTsvA4T37sf/7bTi4cRvue93GaN+AulPYxCTy8rFT2LNmI3xd3KEbMh3f+tzLIceV3yfzDTKzsrB9+3aS+jr27dqNqNAw1SPZ2dwGbQM9VX0T2usaoalpQE99Czprm2B74BA+eO89vPvuu7hy+TIGevtUf8Ti3Dyys7Px5ttvK08gk1fF+i3ntIh9+fW8KPhFBCAJm9w3yE5OhZ3tGdN4wj02CHT3RjfLOplcatlHGk3IDLsfjINbd6oS8tAmUwnYS8HINba1tsLR/hqOk5T4uHg1UvnHrE2OZzQa0d7eDnt7e2XN69asRXhIKNrqG9FF4ttJuniZ1hoz5HNVHdrLa9Fb34qyzFy8+tvf47X/eAXltPAJViNzhim0NjXj008/w5GjR9S55PqkK1vOKUKQ6W0VFRXqux+7xueFX0QAcou2vroGl21Pw4ZkSmknXbjSR5AZHc/4bkUgl7J9Snyimm8odwAPbt4BHxc3DPT0qoaVXsTSomJk0l13dj8aHLISxJuI1ScnJ2P16tV45513cNPJGXUVVehp0aC9tlERbSG+pbruEbi+rZLhoJJCqG5AyO0ANXi1o6UNs+MGjDNk2djY4GseV8YzisjknGLxct74+HiK41N89dVX0Gg0L6Qn+LMLQJYS+9OSU0z3Aiz3BraZevce0LWPD5pGAln2E0LbaTl+7l6qH+HmpauoyCuCnpYuCZ9pcAo9BfEkq5L1BhLSx1jv5OyM1159Dbu370BOaga0JLCjrgmaCpJLC2+XJUkWstuEdBGEFVq4vrmiBlUFxSjPK8RwVy8wPYvwsHC8+tqrSEtPx+TkpDqviEDuSTg4OOBthoUPP/xQoZ4l7c/dvj8H/vwegEux6IyMDNjuNs0EMglgjxoMEuF/V3XuWAtAIJYuQ9BlzEFHYwutjUSTVBVS+L2MTvox8qdISFNjI3Yzxr/+yqvqmQVCZmd9syJbyO8g+R1lNWgnWkur0VJSiaayKjSWV6OhtFKhiZ+FfBFBaVYeagpLYaTHGuzrx5o1a3Du3DlFvlyvtJ/0Cdja2iryw8PDERgYiHXr1qk+ghexNPyzC0CW8sMbmxrVcLDDdOni1g8zFFw6eByVYtnSMOZtrSGNKt9JjiBCGpKxidxO1gvJ1rDsI5/FCgvy87H68y/wxR8+Rnx4FLoaW6FlUqcpo7WT7NbiSjTml6I2qxAV6bkoT8tBWVo2itKzUSjIyEFRRjaK+bmYn0uYA+SyIhAhLE7OIC42Du8xKZSMf3Z2VsV7mdgi5EuimMVEU/5dv34de1hOynX9WKh6XvhFBKAIIkryCuB+zREXj56Ay/kryElIUSOJllv/ipAS0YJl31kEIMIwkoikpCS8+/Y72L7xe5Rl56tp7JLRt9HqmwrLUZ1TiPKMXJSQ8IrMPNTnl6CZgmgtpyegl2irbVCjnFRV0NSCFlYIdaUVqnKRZFEE4OHujt/85jeK3KCgINVz6OLigjfefBPpDAlzc3NK+NJP4OTkhHmWii9i38CfVQDWkB8u0GraUVNWseTWpTS09AFY3xR6WkjGbTm21PeSeElcPrh/PxrpwiWDb6uoQ1NpFapIfAktujSbpPO7DpI8oO3CCMtLmdUk5aQkdnJNUlmoBHJcBx3/1vNapX+glVXDOLeLj4nBjWvXsXvbDrzB/OLTzz7F7373O4QEh2B2ZgZTLBGbWSF8+cWXuHzxEjw9PZGQkLB0rSv9lueBX0wAFqguXEISOtWBY0X+k0Two7U0txfLlzgsE0z/8P4q2F+4hK6GFnRXN6IprwS1QnxqFopSMlje1aG3s0s9Z0j2k/xklNcjt7AlWRVvpDySdGVLBxavUUpYI7eTGc91ZRROcyuqS8uVYKRvIjMxBZvXb8BRm4Oqs0qenDI9OYW4mFj8t//613idAvntb3+rOp0kQXyRcoFfXAAKZgtYiXwLrLevrmYi1tysRCCkWdZbrF9ifm15FXau2QCvqzfQ39qBdrry+sIyJnWM93TpYr3trW2KHOOEqaOmlaVZX3+f+XrMlmkmfrkAxAsM9fajqrhUlZBSCopQdGwz6VKWUDbExFB5NG4vSWgovcHnn36GxPgEXDx/Afv27lvqI7D8hueN5yMAM1Q2b0W6uF/rv8VSJLZKH/tnn32GtLQ0Rba1NxDLl7uOO7dtx7XjZ1Rmr6lrRA8rCB2zdePUpPqRqu+f287NzmGKAmiqq0dlSRn6eHwhRIheAgkU0gUWAQjEunu0neihB5HrE4HIrW9ZypB2geUY0p0tj7frlfsYDCM2JN/D1Y1iMfUVvCh4vgKwIns5+QIZ9y8NuGHderz5xpuqnvbw8ED/QL8iU5IqufO2fccOvM+MvJhJppRos9MzKgOXf1Mz04pgEc301DQFMEtxaFFbVIZBbTfJMcV6ucEkBCuyeW61FGu2iMJCLLdTMP9t7S3Ekyxtx/0lj1iYm0dyYhL+33/6Z0RFRmJufs603wrt8awQb7LS+mfBCyMA69FBFog7FRw7chRnz5zBHX9/VV9LZl1WVobQ0FCs+nAVtm/bhpKiYiwuLJhpN/2T8kxKSLH+qakp9YIkKStrSsrRyzBhpMDkNxvoESQ0mPISU7e1BRZCnwRrAVivt+xv0E+gqqISn33yKTYwT2hublLiXak9nhXq/CusfxY8VwFYsJx4C6QBJw1GeLi5Y9OGjYqc4oIi9fntt97Gu++8C3d3d9Wgln+L5iUWFlXjyz6Skc/NzGKGHqCno9M0fpDHl0kkMgtpcsLIvGBCiUCsVonAch1WpD4JPyYAgeQcjQw5675bizXffIvGhsalbuOlNpB9zN5FBGtJfC1QXorrBbKN9b5/Cl4IAQiWk2+BkCbW/clHH6Mwv0BxK9PIA/zvKNeq3D1Zn2dsl/g+T5e7ML+g9hMihVTZRgQgIUCI0LA8E2uXRpV8YEZvwBTFMklYBCDnlrEH1qQuh5C70nqBtQAktMywKpBH5Hy7+husZ0iTm0SWfEaWEs6EXOlMkl5DGfTa0NCgupBlap3cyOrt7VVtJfsIVzI3Qn7DnyKIF0YAguXkC4RsIf4Pqz7E9avXlDWLRclSoOr1ZVCu3IwRGSzaPwg9SzYp26qlD4IZvBIHCZc5BrM6Hk9HARCyv4U4Ob9arkDws0JEMEsB1tXU4pOPP4HNQRvVbSzVTRKFLF7u1ImT2Ll9BzYyVHxDoXz91df45uvVWP3118p7bN28BTYHbGB35QruBgYhJzsHra2t6vgiop8yL/KFFYAQ0dfTi7279+AtJoD/+zf/qjpUxKKFPOWmzUQpkiyQ/bmUpK6/tw+ahmZ1c6dT7vpJf35OPtoampa8w5SV9VtChvVxl46/jNBnhRxDji2eKT8vH++9+y6+/OILkvwVE9j3lWcQ8kUETjcc4e11CwF+dxTRt3184ebiCvsrdjhkc1AJRMrLD7if3Gk8dvQYwsPCVMUkHmGltn0SXmgByPwBiZnyY1OSktHeplHrn0SSAr+TBu9muSZduHIjp72yDtqKOjQWlqE0MxfdGpaIPI5ARGCBuNY/pwAEFhHIyKIvP/8CN51dkJOVreZHSueUlIyS94h3k+2kQ0lg8XxyDDGMxvoG1b/g7OiEbVu24q3X31TJsjx0U0LJSu27El5YAUhDSdwOvv8AH36wCvW1dSqWy/qVSNIRE2w8Sd46Ne1oqqpVN2401fVKAB2V9agvKEVVXpHq+rUI6XFw3cijY4vbXioNiZWIfVpYjiGQc8tvlGn2QrZACZDX/ySBy3XJetlGtpXE1RIG5fuk+ETlVSKjon6QYP4YXkgBSCOI2qV8khAgN3YkD5CGsm4US4eNIn+YjdY7oO4xNFXVoLmyRrl80z19CoHLmtwidQ9AzS2wNLKc03wcBXMmvkScFYk/ADN/620fA4+lfhM/L21jdR45vxD5JMKtoY5hOa4F5vWyr4hAvMenzC2cnZwwPT2tvlvevivhhROA/CCJxbHRMSrxW792nZplLA2xvKEkS5cGFAHIY2Mk1su9fLH85WijF2jIK1VikO3lPoQ6zh8RwJMg26hr5j4rfS/3KGRpOZZ0EgmJS+d5BlgfdznkexGSzIwWAXh6eDwSwAq32JfjhRKAxSqCAgLxxmuv4+zpM+hiLLeUdPJjl5ZmTyF98JLVN1ZUo5nkqxE8AivyZV0Hk8CylCyUZ+crAVg8jfTx/1QByAQXeWyuyjuW9wPIOqu/TevM53hGLD+ONeR78ZYdDHtSMYQEBz9TR9MLIwDJ2uWH3L97D6/+xytwdbmpumnFvQnp1pAfreYJkPw21tb1JL+JLt8ylMuafBkHIGP9suOT4Xz6khrXJ/tKgilWIx7AckwFadinFEBDJQVnnua2vCPIso0ca0jNhZBjW53nGbD8uNaQ76XdpLz8hmVjSnLyoxzgL8UDiAXKj4iOisTrr76qyDcyyTHoGfOl0QhFEmO3WKxYsKCjtQ31JKFZEj5z0idooiAs5NeXVCAiMBju127A9fJVhAXcVc8ylps6clevvaUVAz19JiFY4jGhXLu6NpMrXwLXq6lpQ8NquFoXKwrJuh/bxgwRgEWkIralsCPHXrbtT4UcS4wkMz1DVRUyAvlZeHwhBCDnzc3OwftM9q7b2aswIF20imw2oiwtDSeQzFzmFDQy2ZOYLsmehXyBjPSV8i+LVu914yZuObnSQ1QhPy0N9277KbKlFGwqr1LVQiMhvYN98t4CEibfm85tKhUtlqwanefWj+nQSwHVlFaop5+px9ssI0aRw30Ge/vRVE1R1tZhmF5HBKbEtcL2PwVyPCkTA+8E4NtvvlFzFP+iykA5Zy/r2g1r1+PokWOY0BtUEqPX6+gBWBOrTNnU+BZPIB08anQOiVMjeglx/WrYF8s+GdcX5OWLm3bXEBvyEP2dHcDiHKqLChF4y1s98LqHAtCUM2xQMCKgZkLEILOBpJKQ+Qq92i4MdPeq7cXiRRiCCYpCBNMs7y7ita10L0CRw+/6u3qUh5Jj97DWVwJYtt2fAgln4gEuXbioBsDKHcK/KAHIHbqHoaGqM6Ojox1jg8PorG9FS0k1SlNzUVdQBv2gWKTU6BQBiRCXKmVeR4Wpg0db04g2ki8DN0NuB8LV7jpC/QPQWluDWcM45qYmgBkDBVKJAHcPuv8u5QFk0ocaGcxjSF+ByiFEDPQezZJXyGhgrpNjaxqaoG1qVXcRu5vbkBAWhdLcAuUN1AiiFcgRsns7OpUARGBdre3qNyzf7ieD7SdilOcjbtq4EdeuXVOdWfKdtO3T3C5+rgKQARRaWsVWkn/m5EncvHoNPhcd8fDaHdyzC0SgvR/uXfVAR0klpnU66MV1t2gUOa3M+MX6tbTi8vQc9fg4NxIf6OWNGlr6rG6UpBsxP0kBGEbAA2C4vRV+zi7qKSPt9c1oJcEdcgwRgGV+gBBu9gqt4lnMyxYRA9FOMRSmZKhnFgd5+GCIHkI6oKQKsIjAEuPFA4inaTKLSh5xqzzACt7iWSDnEY8jyaf0aZRQiDKBNTIy8rEK4IUXgNFgRHxUDP7nP/8z/u5//A0+euUD3L4agqi75UiIqkOsTxLu23uiq5qWrBtHf3unIq2VsVdTVoP63BJE+ATC44oD7nv5oDIvD4bhAZB1Ej6OeRK/MDGC+YkhwMgqo78Hdz3ckZ+cqiaGCLntQqqQb8YS+SuB4pD8IjrwPnyvO8GL561gWTlJ7yS5gkoOLSSRIEkqJZSopJTn0dQ3me4wMmewJvRZoSoK8TpDQxjQdiMs8J7qA6iqqrIQ+tR4rgKQBmpuaMAGeoC/+Yff4IZDMGIydYjMnUJkcgv8r/ijPD4TxnE2Wl8vmmXWLl11S3E5mqn6iugEJATdQ11xEUkg8bMGzM/oMGck8fohYhiLxMIElxTD/PgQYu4FIvFhmGlKGEkVq39aAWi4T21xKXwdnSm+PCQ/CMU9N09oaxvQ09aB0f5BJQRLTiCJpLh96aOQEcqSB8isZHnGwErEPjVEXPQy3Ty2tqEFdmcu4Pvvv1fHfpb4L3iuApDxcdnM/l9571PsO+GBmJwxhOXNIypjFP4e0YjyDoahb4Bxdhia5iY0SNlGd91B968tyEMWEzpNWT4tnjF+Six+GHO09gUuF4V0swCUCAhMjiEvKRYPbnmRUCaAcp/gGQQgnUkpkdEIcHXFzNAAhtua4G1vh7SwcIaGeuVVBjq7mayaElZxzzLxVARQmVuI2rIK9Pex5JTRx8tJfQaI9Q8wuZRw1MIQuHXdRlyxu6JGNinPsEJbPwnPQQC0Di4ly+/s0OCbr7/Ft5uOIjK9Hw+zJhGRNYWosFr42/mgtagKUxP8Qb3daKypUZ09LSSpp64R9fFx8Dt0GN01FYr8BR0tXGI9PQCDPmE0LWf1pu8pCBFAW1UxbjveQHV+ERuwkclfPTormflXUAzlpiljK4lAU1Wv3LjfTTdkxkarpBKTIyhOjIHnlUuoys5FB48nSWN3q0aNQRjrH1LPQpRksiwrD9XFZRgcMD/L8DFSpa/Bur/B8r3VenNeofIMtl9HUwuT5RbkJqTg4/dXIS4+/icNOH0OApCyaRgGgw4O1x3x6hufICi8mm7fiHsZesSkj+C+ayziaf36gSEmWMNM/JrQKOQzF5AG7qquQzxJjLK/iPmxASxOCfEyqmYaLa29SEktQWhoGsLDMlBUWEt3SfJFDNzOONBFC3ZGakS0asCG4grkxqUwrFQpISz3BNKvIEshNy8hFbccHDHQ0YJFEdbkMOZH+xEX4I8AJ2c0SnIps4wZptrpDYR8yRskAazILlCzi6QL+ocVg7UAWG6S7DG202PbsO3EugUyKlnmN/Q0tcGDbfjVZ59D0675SxkQMoYJkl9RWYG33vkMJy/dQVy+EcGZU3iQaUBUdBv8L/ujKa+UitbRYnpIeg3dKMmgAKSB2/JycdvmABriI+n+GffnZ9HcOgxP71QcOuaFQ0e9cOJ0APYfdMO6TRdx+KgTysrque2UEkFa5EMmgx7oamhWTx69cPQEYgOD0VUlnuBRRSBTyUQIUhmIEILcvJEaFsFj6DAzOYjZiUGKYByG7g7cvemMu+5uaCktR0c1PQH3lY4mWcq+cg+inn/LM45+KABrUADEY5bPfeRlG2N6hg6WwRrmHHKbW8Ql7v/82XOPlX/Pgl9cAOLelPU7OeGdVd8hJKkNYTmTJN+IsCwDHtzOQZjLXYx09UHHDF6raVWut4XESEN2MpMuDr0P/8P7MaltwuL8DLJzm3Dq7D1cuxGD7LwmdPUZMKKfR//YNEoqO3DuciC+WWuLgrxKMCbwGBXwvXEN+UkpCPT0wR23W/C66oTUh9FKAEsiICQkdNL65V6C17Ub6G9pVGFmzjCEeSabkndIiBlsqYe/43UEe3qhuaTCFA64r+qpJFESAhqYC4h1rywAkxfQ62SsnxE6nUyCEUJN4xLlgVnSlSw5hXgkmfmUFBGD9955V82XmJycUtut1OY/hl9cADomQD3dWny9Zj0OHruJhMIJBDP2h2RNIyZ1CHeuPUBpLH8Q3dngYB+a6sTy5S6fuH8hox4R164gzd2Bpd4E0unuT53xR2xCFVhVLvtnGiM8OQfYOdzFzt12GB7UYYG5QGzQHTicOQfPaw4YYqlWm1/IctIeSSHhaC2rZpihC6cQOukVWstq4OvkxurhIc/JUEKvo3KAGZabs8QMw8GsDkMtDbjnchN33dzQUFiEzjruK9dN4sUDSIeQDOhQTz4zEy9ikJs2o6NDinRt5ygKi9pR1zDA9jJy+yGGTLp/7jdA199mFlQnw8vp4yewdu06NbZQR+8wojwHQ4dq66ebM/CLC2DCMI6y0hK8/f5qePjlIK7UiNACPaIK6AHCGnDnsi+0JN1A19rTo0WDxH2zu9PUNqElNx8Bhw+gKzNe3f49e9YHmXltIMf8N088PjdgwSyC7gE9Nm2+hLi4Qv61iJ7Gatwi4XG3fQGGGQz0oi4hDrcvXUIwPUJVej66SX5/fSsyIuLgyVg7RlcvD4ZorGlD+MM03LkVhsj7yXTtjdQEBbE4jbH2ZoR6euC2ww2UpKWycqBHYbyW/gIRsGFMRv2YprwLWUslI8u3zq4RuLNNjl1MwMmraQiOYnnZayBJBgwN9KvnFrVUVaO9vhEFWTlYxeTPz89fdf4o4nlMNUOJxvO03uA5CECP0uJivPraKny9/hD2nffCXtb7B07cgtu5ACT5hmGUZIzqhtXdPnF37dXNaJaJnnSrcfcewOv0cbSVFcLuogMi43IU7czy1P8np4EOWlFdbTc5JSn8JxKQby/ZBcDR8QFVwb8GOlEeGIiGB3eBGuYb2elAYT564+IQePIMz3ERiUEhKGTi53XdgeVjCoxDOvi6hWLT+jP4futF7NlzFd9vOstK5ggunWVJWk+BYBrG/k4k3A2C5+VLSA4JYS5RifqiEtQUlqBXo1U3haQPX0YvC1GS8E0YDYhOKcehS4nwCu+Gy30NjtplwiOgHF29RvRrOxgCxRDo/pn8Xb10RY2VbO/oUJNepP+hv7cXHe0talygzKp6mj6BX1gA/LG8sOGhfvjc9sa6DWux+ru1+JdX3sE//vVvcX3HWTSkFVIkYxgcHmSDalBXXI+0zEo8TKzEnchyXPZIhKNfOu7Tki9cv4MBHRM787/qxkFccY7Etj1u2LTFATt3eSA7q0F9JyJw8YiAvZ0/QC80X5wHY2QUkJuFhcwkTMbSvedlYjErFZ2hwSgKCMBDJxe4njmDe17uGO8egKdzMHZuu4aEuGIMDNDaDDPQ9o4hPrEMW7dewZqvjqIwg2Up5TY/3o+KtBR4X7XDPSacWdGxqKL3koRWnj8gnTij8sR0CmFinHU9Q8ANn2Sc96hARPYUorMnERg7Atvr2XC/U4yaqnaV/LVLSKJH3LNjl3qI5dTUDDIyM3Hu3Hls+n4rVq/5HmvWbcNNF0+GWnkR54+L4BcVgDzoUVyU0TCpuoG7OjoRfD8Er7+3CttWb0HoZU/00tINRj0beBBldK33I4tx434lnGK18EwbxO1sHW5ljCIgZwi5jUZMmTw8Sut6cPiML667xSAjrxlF5Z3YuOkaDux1xqI5Kly+HgSna35AlwbTsRFYzEhRpE/GhlEECUB+OqbiwyiKNKqpDMaqSrTm5mCYZV9faxcu2joxJPWqY82a3M7Sv5GxGdhffYDPPrRBWW61CgeSHPY11SM59AETRJaeYZGqipGkVh49I0Loa+/EBA1D2zOMc05pcLzbgXDmRGFpY4jIMuJ2bB8OXU6Hp386WhgCu+s68PDOPRy22Y9aXmOgnxfefecjrPpkMzbsvIgdx1yxdvcl/K/fvw8nZ7c/Wh38YgIwJT3D6tEpDjdccPa8HbbttMErb3+GjbtOItDjIRK9QjDRO6DGBDa39OB2VBEcIhoRVDCBSCbfMc1ALBHNz/FN5E4DDM+y8Znx2zkl415k0RIxEzMLcHAIQFgIXbv8zSx5y/bzCL/D0rGyHFNR4UBRNqYSI0g6PUFhLuaTYzGbwM9FuWCiArS1gL5VEaln+aWtZ55Q1QPHmxGwOXQLZ87eRTo9zJxZhHOMLPb297CFIWKoc4jRgPsyUV0cH0GEjy9iAu6rEnHpriNF0EJ0N7egpr4LZ51p7WF9CMs2ICR9XEEqI6+IHhyxS4f/nUy4XWG7ffw6Dq99E45Hv8baN/8Op/ceQkxiE2Ly9YQOCSUG7L/oi/c+/gqNjU0UgYXXHyaGv5gA1KgZXsCWLd/jr//mn/CHr3di9ZYzuOASj6jUTgR5RCE/jK5YYteIHum0YofIJjyonkdyG5BAxLUS5CS2eRFxFEFC3QI6JshVLV3/zXQMGk1MLC4uUgjzrJsfzRmMjszAV5/shbaaB2ipw1xuBt1GPmYyklQYWCzIw3wKP2dRMKVFWKwox6JGBDCARSPr/bl5lpEa7Nl7EyfP3oeXTyZOnAnER5+dhl9QJubNIhgencIOhoP7t+ldSL70FrKuQ/Rtf8TfDVElpZDfynjeJu6clYampg5ZOdU4cT0Dt6IG8DBrYkkAIRkUAb2e28M+bD9wBRve+q8It/0Ajb5bkG63Cj47/wVO2z+Ct4M3YtOGEMl94/Mn4RFchrf+8C3ycnNZdlt6CJ+jAMQDyFM+fX298dvfv4YbnjFIKpxEYuEiwhLa4XT+pur8mTJMMOMdR2xSFdzi2xFMAURUzSGUuFc6hYjaecTS+mMZ2uMogOaReZRWaXHBKRaaAbrdZf+mZ2eRmJiOj1eth4/rfcBIUYz3YbGlFovlpSS6BAtlApJeXACUF2C+qhjzzVVYHO7Aor6fHmAU9fVaHD7ljwfR5TCYTyPOJi6pBl9+cw5l1VrTSv4L8E9kieaJWWb70vE0NzyEcG9fpIZGKgGo/oEKCqCyAZqqRlY9jcxzqlW894kZpAD0jwQgXiCdVVKqHhcPHECQzf/GbMoRivYMFnIuQZd2BkXua2G3lWJwv4eInH5E5Rnge7cc73/4LXJycmDkbzbx8BwFIDDdqBjC8eOn8NrbGxAQ1oiM0hmcuXwPX7z9paq5Z5gNDzAeFuSzJIytwA36/quhTTh/vxYXQ+pwt9iAeBpmXMMiEhoWoB2VcYMjcPWOg71LDDJz21Bd24us7BYE3UvGgf2XseqDtXC87gnjKAkx0iqNIyS3F4sdbVhoa8BCYx0Wm+spCi41/LuzBYtDnVig5S8aBrHApPROYDK8GIdnlKUv8j9TrBnVzWP9piuIiitXf8u/5MQKHD/sDn2/hAEdpvt7EOLhieyoeHogWr1UNvytGhEAPYHcTczIqoPttVz4RA/R7eseE0AIw0BoeCk8dn6AjttfAZlHMJd+gnkKE9rGCIrhGvIdvob97g0IjatEZMEsS+wivLdqNQoKCl4cAQgMEzpo2trwzdodeOP99Th83BV/+0+v4cN3P4audxgzugn0tnehvqYZKWVtcIyqw8XgejjFdcI7exjhtXNIogCiahYRkN0HrTTyLPdhzPULSMKxU544eMgZO3dcIE7g4vkbyMnKxaxBrJGQO4NGuT08Sk9AguniMU7IPQVZ6rhOT9Inhljb0/UbhzA+NIDrN6NRVNZppngBE1OzyCluxJkLD7BzrwtaO+nqzf/iYotwnuFhcpjrpvTQaTW45+KGkqR0dFY30fLF/ZvyAIGWiWFuZg1s7TLgE0FvQQ8QTNcfTPJD0/QIz5mALwX0wOZ3mEvYCaQewHzacSxUe2OxPphh6yqMUTbw2vcWvL0fIKZoEW5+eXh31TcoZGkrCbep/V8AAUhvldGgp0ttxAGbw3jzrffxP/7+H3HN/irmJmdYHw+rOYBF5fQACeVMApto9VPK7ccx6YslIuoB16QB+CQ0Yoj1r9yUwaSBIdeATo0GDTW1aGusx3Cv1nSjaE7uCI6q28Ryx3B2yrSETm4VU0AkXLBokFvJJsxPiOWbBKDndV93i0VqNk9u/tfbN4pLdndw9lwwKmu4r1pr6nbycIuGjyerCl6TCKC3sRYBjjdRnZVPDyACkB7CxwVQnFuFE1fi4RHaSwEYlADE+kMzmD9kDcHr3D7kXPgtkG5jEkAGQU8wn3ESC0XuTIQ8kHh5FZzP2CIqh4ljUDneff8bZGZmWU0VexEEQEhXpWFCksJBJik5+OLrLxEeFqXm/kmpWFTTDq/ICtyMbkVIxTTimQAmEjG0/ICKKdjFdjM/aEGjdhyLk3TpOiGRGbeRYpgmVDetuHp+FqJljICMDJL63ziKuQnmABO0eK4XASxKf74MIlECGTRBBEBICFicYmKVWIiLDhHQ9Jg6l8Akc3REBrBaHkdjqjU1mjGcOH4LxXl1vBYRng7NZYVKAE2FZaoKkATQQr7cbNLW1qOquBInLoXimn8Tkz7jkgAeZugRl9aDW7bfodLhdxTAISym7MdC2h4spu7AfPpBLHYzeZ2oQknAbjjarEd4ag8Cotrw/gcbEBISbhIAw+QLIADTBQzLLWEup6amUVJcjI8/+5SuqgSz9AAd2n7cjq7GjZgePKxbRHwHrZ7Zf0jtAlyTe3EjtAZhue3oloRP4rka/CFDv8bUcnFilKAY5P6/gK4dY0zk5DNd/ExzHQz52ZhtYpJHISyoW7q9mO3rwMJonxKCiEZGEJmOK3f82FCDA7jlm4Qr18IYCrqh+8F9B2BwaAKON6LgSQ8wpSP5FJvkAMVpiQhycUcbSz6NZP6PCaAOHVXVaK6uxln7+zjhlM8k0KiyfxFAWIYO8and8LRdh7Lrr1AAx5hk0AukHADS9mIu9TBmtBQAxtCc4Iyre1YjlO7yYfoQPv1iLxyuu1oJ4Id4Lh7AAuPkJO49uI/Va9ZA29Gl5uin59TAJaYdYXTz4bT6e+UT8EnvgX14DW6GlqK0ppshhMRP6kxWrTD6GBbpXcCYvzjSBz2z+vGSHGCgBXMNpTCmJmImLQXGhHgsNFUA/c0wFmVCnxiLmQJu18dsXjwKa3cRkwwkWaC3kNu+o/1jCAvPw/lLwXDxSEBEQgXyS9pRXNmNhOQaXLoaCgd6if5Oio3Ei+cRMaU8DEaYrx+05sRPxgtYBKBEUFHFSqAG/veScOB8HO4mjSIsUxJBni99AnEZEgIOIeXk7ymAoxTAYXoBEcAezKUcxFRDOAUwg8aUO7i2Zw1CEhoQkT+BdVvO49DB06o30HRn8YccPDcBSFkoj0Wxv3YVO/fsgY4Zutz7vhtXhKsM9F45I7gRxyQwmNmvfyqi4wvQx4RvbopuXxI0/Q+Jt2DBqKObH8ZUVQl0GYkwErNpCZhOiMa89PmXF2OOIphOiMF0Siym4yOxmJaEmbgYTGWkwFCYi5lWJh3iAXieOSaKMsZQRDBL8TWwJAwNy4YLLf2GYxwcXZPhfDMeYREl6OmVjiOGCRmCxkRzbrQfD31vISU0TJWAIgCxemsByG3ujppqlBZU46RdPJzvtpk7g8ZoyXrEZBoQ4Hkft7b8L+hCNwAZx5QAFlP3YT51PyalGtC3It7jDByO7EN4WjsiCo3YddgDWzbvx+AgE1nmSivx8NwEYBndst/mAE6fP09vPomJMT2Kq7vgF10Jn7BShMZXIDe7FlEe/si7KzX8MKan+jHF8mwl4hVYsknsn+1pwURKHFBMq67Mx2wSyc9MJPmFTJiKgZJ8zJHsqRQma0X5XFeEhYI8TMbHYCYiEpOpqZjrZdInHoYeYV4SRX6W0cVqPMCUEaNjE+jqmUJbzzQ6+yfRb5yGftbAvILuf1yS03GMd7XhjrMjCpNSTJ1ArP+XewCFKnoBfh8QVITj1/MQmDZB98/yjyIIZziISGiDw6GNCDvyG0wn72Xmf4Zh4DQ9whGg8BI6Yk7jxo4P4ecdQvEMsRScwKFzd7H2u13o7pInh6z8pNXnJgDpGZRHmqzftBE+fn4kdhZjI+MY7htBS0Uj6stoKVVN6KlpQPjlS8j2dKFF9XO7IcwYB0i2ZOqM3wpW3kAEQLImS7IxnRoLVJDccqJEunfzsUjrXyTZ0tuHEhFDAVHIdfxcbl5XkItphoTJIopnvJ/5wBgTRMkHJK+Q40tiKW5+AnUtvUisHka+dgGxtaNIquyky6UHYMIpCWALj+nv5Ii6gkJFcEu5DBF7nHwNQ4Dc5u3ib81KacD+86m4FqxFeOYkc4ExVgJjiGBiGBKSQxf/IcLP/Dt6grdhMno/pqJ3odf/S9xlieh6/ggTQA3uZ48jKm8cx66E4uvV29GhkUfVvyACkMkKll7BxsZGfLH6a8TFJbACmMMYw8BQVy9jpcz4oVukpWjZOGHnz6HE/xYbdFgNxZpTWboQL6SPqeWCnp/143T/RH8HptMTsZiXoqwfpXkkmOTLCGISLmQvgR7hsb8r6B0qKZKcLEwlJGCho5HHFGHJ+cyik5HGkheQ4OTCZjgkalSFElw1hxtRzShr6uJ3FMnkKLKjY3DP7ZZpdFGVeABaf4X54RVmSAhoraxCV20T4mMrsOVoBA5cK8H9NB1CVZ/AKB7yc3TmGB6EpsL+wFo47HwNfjb/Br/d/wjntX+PUxs/ZlgqRkSuDvczBhFRoIPtlWCs/mYTtO1aivIF8gCW+F8sj3/74nMU0Dqm6T7Hx3Toa+tEW1mNGv7dXtGA3uoGJFy/hhQne1peH+bpAeaUB1hBAGKdk2OYballbKe7pxdYFA+giLfASgDLyVfrihTEE8wlJ2OqKJcufQCzDD+PPI6UlSIAPTJp8TeStYimAASuKf0Iz27iNTIJZe7w0McPcUEhtO5GU/3P39X2AwHI3yKAFqQkVmL7sYc4YFeI+yl0/yQ9RAlAj8gMIwLi+mHvGA0v10Bs/+ZLfPz7v8e+Ldtge9oHbnfqTLlD5jiimQTus/XGhg3b0d83YH5R5wvQDyDky1JKk/S0NHz65ReoovubZA6gH5VOEw3aSqpoMTIurx69tIqioCAEHbXBlLaBbpdJmb7fTMQyAUjZRe8wSYuflVu9dP2LZXlPFsAKkHAA8QKC3CxMJtML9LQx5osXkFBjJQDW+QV1PXBI0ICGrzqqfAqM8E9pYaViwIimCb7XnVGSkvUo/qsc4BH5JlQTNaqTqCS3Bheux+OcW7XKAcLTR5gIjnKpR1TWFOxvNeCkQzFiWSJu23YFf/e3/451O+xxO7wH55zL4RykQXDyGGJzR/DluuOwPX6G4dY8f/FFEIDpfoBpUoi88evzr75EQ0OjehbA+NAYraAZGiWAGjVxs5MeoD0nF4E2+1ArgzampTR7ggdgvT4/3ImJnFQmdFkkkfG91Jr8pxOA8gwV4gUKGAbiMVNTxjAgnueRB1iU/oHpcdR2jMAhrhlhdQuIb6cAiibhFV/HEKdDdU4m/G7cVEPOO6qE+HrTqJ4VBNBGAXTwc01xBTy8k3HiegEepOhJPL0AkZDPiskjG99t84bLHQ3i8xZw8FQQ/vFfP8HHX+9HeNIgPEP6cdqxHBcc87H/hA/+z+ufITY2juX2FNv8BRCA9WRFGccWHRONz+kBmpqaYaAHGBsaRScF0F5araZuSxjQ0GL6auqQ4emO+yeOYLqT5dkMkzKxQrPbl06gef2YImi6uwX6zGSSR8sXd66SvKcXgDVk/4X0NBiyKKixXswxppsEIH0DUhaOoXfEiFuJDbhbYkBiJ+CeP45b8ZXQjQwh9m4QIv3umup/q67fH0NNfhGiQrJw8GICPKOGEZ09hZhcA9yDcvBPv/kD/vXfv8e92EHE8WfZ2kXgd++swSvvfAPvwFJE5c4iIF7P9en4f/77a/jqy3VMADsYbimA4ResH0AEEBsXi08//5R1dYMSgIFlVR9DQDs9QIfyADUUgEwEqUd7fgECDtmgJMhbhYFF6cGzyv7nmJlLV66xqQoTrPmFbBORP10AC5IL5OfAmBiPua5WephHAhBIb+Hs1BQSS9rhSP9/r2oGV8LrEJ1Rh4F2DW47OqFYbgDVmCaiPgmmJLBWvbCqKjsftXnVcPBMxEGHbPjFjMHepxi/f+9r/F//93/Dmi2nEZk5iNj8WZy+GoN3P9uM9z5egwPHPBFHoYTnTSEgqgvrNp7Hq698iJ279jHE1kOvm2a7/1AEz00AkgMkp6bgMyaBtTW1DAnT0La0q/fzdUoCWFatBGDKBWrRQ89Q/CAYfvt2oY/JHWYkFAgRZgEQMAzBQIKnJAFUJIoHELf+0wSgksHSAswkxGG+plzdRbQWgEAy/f7hCdxJqsEl/0y430tDb/cwytLT4efkglYRs4xqtiJ8OWSImCzri8tRkZXH313PSrQBV26m4cjFFOylO1+3dTv+7bW3cd0rhR5BvMIMHG5l4w9fbcWh4yfVkLDQeA2ic4yqZHxIkVz3iMc//8tr8LzlTYMzqt5AmXtgCgUmb/zcBCDnKi4tUSGgkFXAhM6I44eP4/yxk2gprUI3rUa9008qAi47mEF3szSMvGyP0FPHMdvbSk8g8V+IoCcwUBDjfTDkZWIhjSFAkfinCUCFEJaEC9J9nJ2psnpLIjhHbyPPHZB7BQtTE+gfHEMFXX1/Vxdm6I3ue91SI4Ak+zcR/Xjmbw2LAGoKStQbStsofm2VBlVFbUhILEdKaiH27tqNNRv2MxEcRBTDQlTmNPzDmvE+ife47YNVn6/BkXMBiC+YRHiOAQmls3AJyMf/eetTJNHQZJylzD2Q8RiP8BwFIGPXZUjzt9+tQXBwCFqbNerx7//2r/8bblcd0M9QIIlTG/MB6xk6TelZ8Ny+HeUP/FUSJpM85mUqOD3AwlAXjJkpWMxMfYIAnsH6CdUxJH0CueksK5Ox0K+lF5BQ83goECzKnUcZbzCvRxP38eJvqM4uoPWbBoBY3/9fDvlORFCRU6BmEauZy9IPwn3liSSpcSn4u//+Dzhy/CZSC0YRl6NDAl19LCuET1cfwA03D7h6+eA3r3wGx9vZrAD0uB/bjrc+3YWdB4+p3Mt0L+Bx8gXPTQAyhVpEcPL0SRw+cgRhoeHqpUryirX333oHCQ8jMSD5gHgAQpJCmcGrZUPlBgTh/tFDqE8IU51DkoxJR81iVxsmxf3LeD9zj58i8icKQIUAqQaKcjCZlIg5bQuJptBWEIBJBBKKhhARdAeh3n4s6yzW/8cFIPFfnmPcUFJhtW0NuhrakBKXjL+hAP7nv7yJb9Yfw+bd13HolD8cPVOwZuMJrN+yE03trdhhcwS/+Y+Pse+YGz5hZfD+R2tQxhLboLd0AlnIf5SMPzcBCGSkSiqz7I8++QTffvMdjtvaqidsykOPv/joE5Rn5qGvrsX0dk8JA8wHGkvkhY/FyPG/A+/tW1AZdle5YZYEmGtnOZkcBxQwR1gSgHUS+FMEUAwU52EqKR6zTXXqFq94AVPP4CNIV7FMEeuor4bXNQcUJ2eo5wmIZcs9/5WmnFujrsj0IGsZKWwtFg29Xj1zom9Wf8uE+Wt6gTPYsHEnPv5sLd5873Os37wTl69dRUdPDzRd3XDz8IKjszuuXXdGXl4+CbZMFRMMEo+Xgs9VAOoBi1zu3bcP//k//xe43XRVr2NvbWnB2jXfYSNFUUW32EVLknxA5uo1s06uzi6ElnlCnl8APHZsRYrrDUx2M0tvb4AxIUYJwNTn/9Pjv4L0B/A40is4nZyAqapydYdPdQ1bkW8RgAgj8l4Qgr1uqxJWRv3I1PDlPX/LIYSL6ze5/+Xf1aOzRYvDzI9OnTrHNjOir3cQTY31yMhKQSmvs7evmznIgJpzMaGXR9WMQD8uj5Bl7T8qcwzF/T9K/Eww9cc8VwEI5JwycPHAARsUFRWpOQEz00xw/Pzx//3DP+Ghf5CKhwKtdKRQAJXpuWgvrWGi2IjS8DD47NmJgBNHUBtyB6NRIarzRwlALNhaAKUWUi2hwUy0NcTlC2Qby3ZcTmUkYpKeAFJ1qDAg9wVM9wbk1rMMO2uoKIS7/TX10Cq58WMZ9qWSPF7/cnItEPcvs4drC0sfWy/CEAGM943i5k1PbNy4VU1uNUxMq/Ap1q0n4abONSF3SPW0mp5QwoxftfHjFv8IppLwuQtAYJnDpp6cIRMl+ePOnD6NLWvXoyTV9HrXytwCNMqzgYoq1MTNlpIaaOub0SOvds3PQ7jDZYaETUg9YwuD9ANU01ory8zEkzjpEWRJpwiV0EAxKEFYky/fkXwRgYISkAlzTASNeRlYHB9U/QEyjExZvtwinjVgtKcd3jccEB1wX8X+lopqldUrUhWRP8wBLOuqGdJEAOpZx+b1lu8kjMzQ6uNiE/H556vRUN/Cmn5SJXbDw4Pm5ZNI/uN4IQQgqlUi4FKmMrW0tuLLzz6Hn6snq4E2NBSUojyDQqDlV2UWoTqjBM2l8jbvAsRFJ+He3Wh4utzG+R074fzt54jYtxUNvm6Yy8+iEEooBCGW4HKx0kSsAuO7xcqXBGD9N7dFJfevKsUcrV+fzQpjuJuVhwjAVA1g1ghdfy+CfX0R4OyG1mKSyHBVQXdekpGj4rrKA54gACFdthMRWK8XyH7d/P3ztPgafv70ky+QnpaN6Sl5L7G8SGpcYVieOrJCuz4NXggBLGHU1EMYEhKCD9//AMUkvbu2Sb0FVMbTt5SR9NxyJCdkw8MrCHuPX8XW4w7Ye8kbRxz8cOqILXy2rYP31+/j+qpXEX1wJzqCbmMyPQGLMjBExFAloGcw3/BZTj4qRDClZg8in7kdE7tF7jucGoP5vnYsTo1hcVKeCWBEv6YVD3x8cNvFTb2bWN5KIrW8qaQrUpb9JAEIhHgRiYQB6/WmsFGDgTYtpnVGDHT1Yd2adbjtwzA3SiNpaUV9PbdracbQ0KOy7lnxQglAYpn8GHkR5EnbE0x+2tBezcaraUZ6agH8AiLhfDsMV26F4oJ3GJxjShFc1oewqm44B4XD6dAxRB87iAqHC4g/sB3un72Pu9+vRdDmDYg/cRj5DBN51y6j3NMV40kJmMlIVwmecvVVJLqGwijKxVhsJJp9b6HUwR4F9hfQ4OeFibxUNEbdw3RvM4mXQSFjrEgK4efigkAXdxQkpJD4HJSkZSlSZfKnLEUIikySarFsy2chXURenVe0tM7yoErVPVxdj1GZKzkqj8w14tzpC3j9tbexfftufMck+Y03XlfPBxaPKZ5zpTb9Y3ihBCBjBOTV6/Je/uysLPVuv6qySnjcfogdJ1xgcy0QrjEliKoeQoZ2EiVDQHq1FvZ2znA9dBy1/n6YzmD8p7uWoV1R+3Yi1mYX7mxegwynC0h2uYDwC8eQ5HQFrZEh6EmOxWJzLdBajwXmB61BfkiyPYiw7RsRw3wi7eAepDK5DN27HTFnjqLygS/qY0NRds8Xae4ucDx4EFcOHIT3BTs8lAdV5kgcr2TMr1GTPi2J3ZOsX8Qh7r+ZQlClIrdTj6wlRADyZFF5a7lgxjCFhJhEvPXGu7h8yQ7Xrzvgb//2bymG7eqF1E/zLICV8MIIQKxfcgEbGxvs3r2bP2gMXZouOLgFYPMZV7jGViKxdRJZvfPELIr7F5CSV41LNkcQcOQwxmNjab1044W5QE42l0WocnHEvc1rEXlkDyZq+Z2hDwvDWqILc4NazA91AyNdGEyPQ/Lxgwjesh65p0+ix/82puKiAOlSzs6ELiocQdu+h++eTQg6Z4ugi1dw380TEQ+Ze3j449Sh8zhucwoxobFM2loYthpUwlpMcsXK1QOdzKQrK+ffkiAK+dL/L+ssEAGoioGfZXTUBMs1EYBhZByDPf1oa25jqTeBCxcuYM1336l3Cv5U6xe8MAKQ2J+RkaFeDZuamqJe8liQVYZth6/CO6sRhYNAVs8c0jonkN03idSyNtjbnECU7UlMp6WS8AISzzifS/KVAAowFhWB0G0bELB1LTrz0qAeISsTRuZ0BN14fyfq/W8xTHyLLNsDGAu/T8JlX4qFlYXpeKZjJtoeRfatm2ht60BB2wDyOseR18trbp9AbIkWjoHxOHLRBR637qEkp1TFf6lcLORLGLB8tnT6WLyDxfoVyk2xXx46qR8YJvEmAQiMYzrMTbJEvu2H999/HxlZmVazfn4aXggBSNknsX/Llq30AAdM49ip+NLCOhxmgueZUoOcfiCjcw6p3UZkaEZgf9ERAQePYS4lBSgQ8rOxSLJkLJ9AEZeVgbTD++Gx+hM0JoWrcXrjdPlT/W3AWCeqPJ1w7zuWVs4O3FYeEUPSSbbaX4QgyKVHyc1CxrkTyLgfjNqhOSR3zyK9axqZgp5Z5NArZXMZXtbO3CQUNrb2iLgXg7YKsXZx/5IEmp4HYEn6rLN+a0h/hwigp7UdetbqOgpAHgsrL8iQdxy2Njbh3373b1i9ejUGhwZV263Upk+L5y4Acfti/UFBQXj7nXdQWlKEmZlpDPUOMcvtwnnXYGy+4o/YVgMbmobZO4U7gRFw3G2DkTjp9mV9T6IWl0OsN5/Z+JXz8Pr0A9T53ESVrzvu7tyBtBtXUOPvgeCNa9Dt42kSUF4+98skKCCr4yBXPEEa0k4dRWTAXRT1TdELzSFLO0VB0gNYQDHkDy4gvUOPvQxZdudd0cySsLG0DPVFpSrRE+IF4v4tXb6C5SKQ6eN9HZ3quT/qjSkkX6DedNLVrcj/+OOP1StmpfdvpXZ9Wjx3AYgLq6mpwQcffABnF2fMTE9hwjiDivJmOLjfw8bjN/Dt2VsILO1C3jCTvvI2OO0/hAZvXyZ7rNmFpKzcx0izQKzXGBuFsku2SN6zBdkH9qHRzg7B69fAd/Wn6LjlTpGQ+BwhX47xOPnqGHl5mGP5F2azF7FRccjtJ/FaWn3HDIm3EkHXFHK6Z+ARX4pj9gHw9gpBTnwKCc9CcVomSs21vuQETyLeRL6pYhjs7lXPD5LHy1sEYKQAOjs6sGrVKtjb20M/wdj/hClfT4vnKgB5lbvMWpGkb936dRiS/uwJI6LTi3DwrDv2XboFr+QK3C5qx628VqR3T+HegyhEnD1ntsxcLIjrX0aaCQwHBPK4TUIURu/dwVwCK4TMbLTccECTyw0VIuQ4lm1/cAzJJehheoJ84X3IBukVDcjqn0GmdhrZHdOPyCdy+uYQVtGNr4844+tddgiPLVA3cRqKy5gQlqmqQEi3TghXgqXPYKSv32T9yzxAb3c3Pv/8c1y6dAkTMvD0L1UAkvXLD3B3dcU7rGerysoxNKyD252H+P7EDVx7kIHk1nHk0K0mdhrhW9iOkJI23LruhJYHTNaKSxSZJgH80HIlBCxSIIspTBAjI5nRM8azTFvMyMRctGT4XM/tkLVsP+tj5VFAOZnIOHkMdxxdkNelR2b3NN3/JEUwiXTlAaa4bhZJHZNwy6jBlfACnPaJx4FzbnQeFdDWNau+DA1zAGX1yvIf9QssF4CEBnkK+Ji8u4htZC0A6d+X9wN53/LGmm+/Ve8Hkp7Aldr3afFcBGCJ+/EJrGvffB2BAQFo1wzi/I3b2HnBAwHZjcz0Z5FFq5JkK53uNbxpBC4hqfA7eQaz6SRPXPdjxJlgEoSQLyGAiIvHIgHxBpLQpSRgLjaCxGc+wXtkYcH8GQV56PP3gd+OHUjPKkUuryeThIsAxOozKUwRRCLJ9y3Swqe4A2msVFJYGRx2vgvby+6oKTVN/26retTPv5x0a8jdw86GZvVexJXeKzBFARSzxP3oo4/UA7ek72SlNn5a/KICsNy0kLhfUlKMj/7wEc6eP4+CikacuOQHW6dgxFUPIr9vXjXwkoV1zSC1axLeIcmIt7tmdt3LXLZYvHkpoQH5BVhgOTkcGKAmeCwW5rJiiIMx9C5molkRCPniIZTrF6u3wHQcEc8Utwvfvg3hHt4oY/mZxesQyxcoAXRPIqHDSOI74UXEMjHM7plhOJhBeE0ftp13g5NbIJorG+gFpNyrhrzwYuW5AY8EIM8DVq+WoQCWP1dYZlTJS7PWrVsHLy8v1ZayfnlbPy1+cQ8gnRbyapPvvluDQ4cPI7ugEicu++CiawSym/Qo7p5ngjWJLDZsppYlX4dBLaVhkwrrkeLkiOnYGJN1W8o1unEFsXJaxXxqOnRR0Siys4fLF5+j1NsVbF3UBt1CibM9kmwPo5/CQDZDgoQA7mPBQhYFQIiH6fTxgse69UwDqlHA82fzmrJ5LVlyXbT+RI0BfkWd8MnXIq6NguhheOhkmKBYcgbm4MuQ8P0RezwIjkNzjcwMpgeQOl/mBkiytwKkC1keIqk6xsQDsM2sBSCzfCeNkzh79ix27dql2lS2Xd7OT4tfVADSXdnS0oKDBw9i79696GAM878bCTuXB6ho06O6dxol2gkUafUo7DSgoGsCeVod8oliln/ZdVrcOXcBuZcvYzAqAlMMBXMZ2QwJWTAmpWAoIgrNvv7Is7+G6CPHEGJzCAlX7dFTzNAw0YvxrkYYNfVIc72B5NOnMCeiKS5l/c98woKCYoLrSiowFheLe2fPIK+0CaW09iJeR2GnCfldOkRXduBBQRMyWkZR0mXk+jHkd44Q4yjo1qOo1wjnB4k4dcUNxRR6R0MT2mspgroG9Yq75WiTR+EzSexnqacEIGC7WQtAptBLGIiJicGmTZtUe/7F9ATKj5Ln2MrAD/WCQyYwjfwBmp4B9I5NoXOYZY5gRI8uhQl0j06YliNGdI4a0dOoQV9ZCYabq2DQtmCyox2G9g6MtWsw2NyErhq62YoydFZVYqi1GYaRAUzr5Lm8w2wo5h6GMQz3dqK7ugpG7jfV24vJHsKy7OkhumHk0tjF7Zoa0dk3ip5RA69Fh65RXpdgRIfWvmG0D47yek3rZWmNrrEJaAZHUN2sQU83s/qBAWb38h7CPgz292OImb4s1WdZyt/ESm1nDWnHXl5vdXW1elK4CGOl7Z4Gv6gABHKxch7xBtL7J494GxtjfjA6RHULTC9MkGFNsrRA3voxzH3FBep0tISxQUJGwJhmGw+xUYblhghLy1G9DuNyDlrGMEUm340M8fxMrAZpQXKMCfMDKUYYax+HrOO2Al7DuG6C59czC5fzWM5nwvg4r3Gcec3oIIYptEeDLh9hlN/rJkahk2MN8/fKCB2eY8miV8BK7bYcsp2049Nu/yT84gJ4HLz4Yf5oEiPEmyCf5TsZ6TK0hFFCNaBkx9xHlqbt6CaHxVVa9mcDq2TTBMuxxrhOIN8tfZZthEzztmr7JYJNYlSQc4mA+FkeYmWBZZ8R8zXKsZZg/m5UzkOBjI1QsCIIWcel5S2kK2HltvrzYEkAVJJB3uIpK35Z6E1LljNS0khWK8vlkGfmG3X8zG1l3KCRkKXsa9Bzvflvy3eW72U5KfvJNjyGLC3fq224To69BINciwmW7/Q8h5HexHK+Jaj9TZDPj31nXqf242+U1+CqpQI90PJtnxOEc+FeBKDhChoMnd9zAEtEXsLoY+se+yfrhh8HLZPgfgqWv3+I5fs92lc+D6tz/xCma6Klm65H/jZfl6y3vs4fg+zzrFjpOH8uCOejo6Oa/x+S5W4VyCpEvQAAAABJRU5ErkJggg==";
        public static readonly string IconBase64 = "AAABAAEAgIAQKgAAAABoKAAAFgAAACgAAACAAAAAAAEAAAEABAAAAAAAACgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACAAACAAAAAgIAAgAAAAIAAgACAgAAAgICAAMDAwAAAAP8AAP8AAAD//wD/AAAA/wD/AP//AAD///8Aj4iIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiI+P////////////////////////////////////////////////////////////////////////////////////+P///////////////////////////////////////////////////////////////////////////////////4j///////////////////////////////////////////////////////////////////////////////////+I/////////////////4/4///////4iP//iIj////4//////j//////////4//////////////j///////////iP////////////////h3+Hj/iIiPh3iIiIh3d4//d4iIiIiHj4iIiI+IiPh4iIiIiIj/////+Hj//////////4j/////////////////h3h3j4iIj4eIiIiIiHePiHiIiIiIh4+IiIiIiI+HiIiIiIiI//////d///////////+I//////////////////h3h3j4iIiHiIiIiIiIiIiIiIiIiId/iIiIiI/4d4iIiIiIiI/////3f///////////iP//////////////////h493j/iIh4iIiIiIiIiIiIiIiIiIePiIiIiIh3iIiIiIiIiP////h4///////////4j///////////////////d4+HiPiIeIiIiIiIiIiIiIiIiIiHeIiIiIiHeIiIiIiIiIj////3f///////////+I////////////////////d/+HeIiIeIiIiIiIiIiIiIiIiIiHj4iIj4d4iIiIiIiIiI////93////////////iP///////////////////4d/+Hd4+HiIiIiIiHd4d4iIiIiIh4+IiIj4d4iIiIiIiIiP////d////////////4j////////////////////4eP+HeP+HiIiIiId4h3h4iIiIiIePiIiI+HeIiIiIiIiIj////3f///////////+I/////////////////////3f/h4+Ih3iIiIiHePj/h4iIiIiHj4iP/4d4iIiIiIiIiI////gH////////////iP////////////////////+Hj4eP+Ph3iIiIh3j///h3iIiIh4iI+Ih3iIiIiIiIiIiI////d4///////////4j/////////////////////93/3eI//h3iIiId4////h3eIiId3d4d3iIiIiIiIiIiIiP//+Pd4//////////+I//////////////////////h4+Id4iPh3iIh3d3d4j/iHd3d3iIh3eIiIiIiIiIiIiIiP/4d3B///////////iP//////////////////////d4iIh3eIiHd3d3d3iIiI+IiHiI//h3iIiIiIiIiIiIiIj/91d3eP/////////4j//////////////////////4eIiIiHd3d3c3iIiIiIiIiPiPiP//93iIiIiIiIiIiIiI//d/iI//////////+I//////////////////////+HeIiIiIiHd3iIiIiIiIiIiIiIj///+HiIiIiIiIiIiIiI+Hj/////////////iP//////////////////////93iIiIiIh3ePiIiIiIiIiIiIiI////94iIiIiIiIiIiIiPh4/////////////4j///////////////////+Hj/cIiIiIiHeIiIiIiIiIiIiIiIiP////h4iIiIiIiIiIiIj3f/////////////+I////////////////////cH+HeIiIiId/+IiIiIiIiIiIiIiIj/////h4iIiIiIiIiIiIh3/4iP//////////iP//////////////////+Hd/d4iIiIh4//iIiIiIiIiIiIiIiP/////4eIiIiIiIiIiIiId3d3d/93eP/////4j///////////////////d4d3j4iIiHj//4iIiIiIiIiIiIiIj//////4iIiIiIiIiIiIiAR3eHeHR3eP////+I///////////////////3iHB4iIiIeP//+IiIiIiIiIiIiIiI//////+HiIiIiIiIiIiPgHiIh3dIiHf/////iP//////////////////h493iIiIh3////iIiIiIiIiIiIiIiP////+P94iIiIiIiIiIiId4iId3eIiGj////4j//////////////////4ePiIiIiIeP///4iIiIiIiIiIiIiIj////4iPh4iIiIiIiIiHd3R4iHd4iIcH////+I//////////////////+Hj/iIiIh4/////4iIiIiIiIiIiIiP////iIiPeIiIiIiIiId4hgeId3eIh0eP////iP//////////////////h4/4iIiIf/////+IiIiIiIiIiIiIj////4iIj4iIiIiIiIiHiIhEiHZoiHB4/////4j//////////////////4eP+IiIh4//////iIiIiIiIiIiIiI////+IiI+HiIiIiIiIh4iIh4h0eId3aP////+I//////////////////+Hj/iIiIf//////4iIiIiIiIiIiIj////4iIiP94iIiIiIiHiIiIh4cHh4iHf/////iP//////////////////h4//iIh3///////4iIiIiIiIiIiI////+IiIj/iIiIiIiIh3iIiId3F3iIh3/////4j//////////////////4eP/4iIeP//////+IiIiIiIiIiIj/////iIiI/4eIiIiIiId4iIiHMzN4iId/////+I///////////////////3f/+IiHj///////iIiIiIiIiIiI/////4j4j/+HiIiIiIiHiIiIc4iHN4iHf/////iP//////////////////93/4iIiI////////iIiIiIiIiIj///////////94iIiIiIh4iIhziIiIeIh3/////4j///////////////////d/+IiIiP///////4iIiIiIiIiP////iHd3j///iIiIiIiHiIiIOIiIj3eId/////+I///////////////////4f/iIiIiI///////4j4iI/4iP////+HeIiHf//4iIiIiIh4iIhziIiI+HiHf/////iP//////////////////+Hj/iIiIiI//////////j////////4eP///3f/+HiIiIiHeIiIc4iIiPh4h3/////4j///////////////////h4/4iIiIiI//////////////////h//////3j/h4iIiIh4iIiHOIiIj3eIeP////+I//////////////////h3B/+IiIiIiI//+Ij////////////3j/93j/94/4eIiIiIeIiIhziIiIh3iHj/////iP////////////////+Hdwf/+IiIiIiP/4d3d4//////////94//h3//h//3iIiIiHiIiIc4iIiId4h4/////4j/////////////////doh3j/iIiIiIj/h4iId3//////////iP//94/4f/94iIiIh4iIiId4iIh3iIeP////+I////////////////9weIh4/4iIiIiI+H////h3//iIiIj//4eP/4eP+H//iIiIiIeIiIiHOIiHd4iHj/////iP////////////////cHd4iP+IiIiIiPh/////h4+IiIiIj//4d4d4//eP/4iIiIiHiIiIhzd3N3iIh4/////4j////////////////4d3Z4j/iIiIiI//eP/3j/eIiIiIiI//+Id3j/93//+IiIiIh4iIiHh3dzeIiHeP////+I///////////////////4d3j/iIiIiI/4d4h3/3f4iIiIiIj/eP///3eP+IeIiI//eIiIeIiHh3d3dH//////iP/////////////////3d0B4/4iIh4h//4d3eP93+IiIiIh3f3eP/4d4/4d4iI///4iIh4iIh3+Hd3f//////4j/////////////////hHd3eP+IiIh4eP//+P//d/+IiIh3iHj3d3d3j/h4iI////+HiHj/iId///j///////+I/////////////////4eIiIeP+IiIeIeP/////3j4eP+HeIiH/4iIj/93iIiP////+HeP/4iId///////////iP/////////////////3eIiHj/iIiHf4eP////d/+Hf4eIiIh4////+HeIiIj/////+I///4iHf//////////4j/////////////////93iIiI/4iIiH/4d4//h3j4eHd4iIiIh4///4d4iIiI//////////+IiHj/////////+I//////////////////h3iIh4/4iIh4//h3d3eP+HiHiIiIiIeP//93iIiIiP//////////+Ih3//////////iP//////////////////doiIiP+IiId///iIiP//eIiIiIiIiIf//4eIiIiIj///////////+Id4/////////4j///////////////////d4iIeP+IiIeP//////+HiIiIiIiIiHj/h4iI/4iI////////////iIeP////////+I///////////////////4Z4iIj/iIiIj///////h4iIiIiIiIiH/3iI//+IiP////////////iHf/////////iP//////////////////+HB4iHj/iIiIj//////4iIiIiIiIiIh4h4////iIj/////////////h3/////////4j///////////////////gAV4iHj/iIiHeP////94iIiIiIiIiIh3j/////iI//////////////h4////////+I////////////////////d3d3d4/4iIiId4////eIiIiIiIiIiId//////4iP/////////////4eP////////iP///////////////////4f/iIj//4iIiIh4j/+H/4iIiIiIiI+Ij/////+Ij//////////////3f////////4j////////////////////3f//////4iIiIh3j/h/+IiIiP////+I//////iP//////////////93////////+I////////////////////+Hf//////4iIiIiHeIj/iIj///////+P/////4j///////////////h/////////iP////////////////////+Bf/////+IiIiIiId4/4j/////////j/////+P///////////////4eP///////4j/////////////////////+Af/////+IiIiIiIeP///////////4j/////////////////////+Hj///////+I//////////////////////+Ff/////+IiIiIiIj////////////4//////////////////////h4////////iP//////////////////////+HeP/////4iIiIiI////////////////////////////////////d////////4j///////////////////////+HeP/////4iIiIiP///////////////////////////////////3f///////+I////////////////////////+Hj///////iIiIj///////////////////////////////////h4////////iP////////////////////////d4j///////iIiI//////////////////////h3j///////+Id3f////////4j////////////////////////3cH////////iIiI////////////////////+Hd3d3d4h3d3d3eP////////+I/////////////////////////4d///////////iP///////////////////4d/iHd3d3d3d4iP//////////iP////////////////////////+Hj/iP///////4j///////////////////h3j////4iIj//////////////4j/////////////////////////d/h3d////////4//////////////////+HeP//////////////////////+I/////////////////////////3eHeId4//////+P/////////////////4d4////////////////////////iP////////////////////////93d//4d4//////+P////////////////+Aj////////////////////////4j/////////////////////////d4////d4j/////iP////////////////93////////////////////////+I/////////////////////////4j////4d/iIiI//j///////+Ij/////h4d/////////////////////////iP///////////////////////////////4d4iIiI/////////4iI/4d//4AQf////////////////////////4j////////////////4iP/////////////4B4iIiIiP//////+I+HcXCP+Hd4////////////////////////+I////////////////h3eP/////////////4d4iIiIiIiIiIiIiPcHj3eP93//////////////////////////iP//////////////+Hd3eP/////////////4d4iIiIiIiIiIiIj4eP/3ePd//////////////////////////4j///////////////h3d3j////4j////////4cXiIiIiIiIiIiI/3f//3eIf/////////////////////////+I///////////////4d3d3///4d3j/////////cHiIiIiIiIiIiP+Aj//3d3j/////////////////////////iP//////////////+Hd3eP//+Hd3//iP//////h3eIiIiIiIiIeP93///4d4/////////////////////////4j///////////////+Hd3////h3d/+HeP//////hxeIiIiIiIgAj/h3///4j/////////////////////////+I////////////////+Ij////4d3f/h3f///////+HV4iIiIiHd3//h3//////////////////////////////iP///////////////////////4d4//h4/////////4d3eIiIcYgHj/h3j////////////////////////////4j///////////////j////////4////j///////////h3d3d3j/d3d3cH////////////////////////////+I//////////////iHiP////////////////////////+Id3eP//h3d3eP////////////////////////////iP/////////////4d3j//////4iI/////////////////////////////////////////////////////////4j/////////////h3d3j/////h3d4//iP////////////////////////////////////////////////////+I/////////////4d3d4/////4d3d//3eP////////////////////////////////////////////////////iP////////////93d3f/////93d3ePh3eP///////////////////////////////////////////////////4j/////////////d3d3j/////d3d3j/d3j///////////////////////////////////////////////////+I/////////////4d3d3iP///3d3d4/4d3////////////////////////////////////////////////////iP////////////+Hd3d3eP//93d3eP+Hd////////////////////////////////////////////////////4j/////////////93d3d3f///h3d3j/h3eP//////////////////////////////////////////////////+I////////iI////h3d3d3j//4d3d4//d3j///////////////////////////////////////////////////iP//////+HeI////iHd3d3j/+Hd3d//4d4///////////////////////////////////////////////////4j//////4d3eP////+Hd3d4//h3d3f//4j///////////////////////////////////////////////////+I//////+Hd3j/////+Hd3eP/4d3d3////////////////////////////////////////////////////////iP//////d3d4//////+Hd3f/+Hd3d4///////////////////////////////////////////////////////4j/////+Hd3eP//////h3d3//h3d3eP//////////////////////////////////////////////////////+I//////h3d3j//////4d3d//4d3d3j///////////////////////////////////////////////////////iP//////d3d4//////+Hd3f/+Hd3d4///////////////////////////////////////////////////////4j//////3d3d4/////4d3d4//93d3eP//////////////////////////////////////////////////////+I//////+Hd3d4////h3d3eP//d3d3j///////////////////////////////////////////////////////iP//////h3d3d4j/+Hd3d3j//3d3d4///////////////////////////////////////////////////////4j///////h3d3d3iId3d3eP//+Hd3eP//////////////////////////////////////////////////////+I////////h3d3d3d3d3d3////h3d3j///////////////////////////////////////////////////////iP///////4d3d3d3d3d3eP///4d3d4///////////////////////////////////////////////////////4j////////4d3d3d3d3d4////+Hd3eP//////////////////////////////////////////////////////+I//////////h3d3d3d3j/////+Hd3j///////////////////////////////////////////////////////iP//////////iHd3d3j///////iHeP///////////////////////////////////////////////////////4j///////////+IiIj/////////+I////////////////////////////////////////////////////////+I////////////////////////////////////////////////////////////////////////////////////iP///////////////////////////////////////////////////////////////////////////////////4j///////////////////////////////////////////////////////////////////////////////////+I////////////////////////////////////////////////////////////////////////////////////iP///////////////////////////////////////////////////////////////////////////////////4j///////////////////////////////////////////////////////////////////////////////////+I////////////////////////////////////////////////////////////////////////////////////iP///////////////////////////////////////////////////////////////////////////////////4j///////////////////////////////////////////////////////////////////////////////////+I////////////////////////////////////////////////////////////////////////////////////iP///////////////////////////////////////////////////////////////////////////////////4/////////////////////////////////////////////////////////////////////////////////////4+IiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiPj/////////////////////gAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAAAAAAB/////////////////////w==";

        public static Image GetAvatarImage(string fallbackPath)
        {
            if (File.Exists(fallbackPath))
            {
                try { return Image.FromFile(fallbackPath); } catch {}
            }
            try
            {
                byte[] bytes = Convert.FromBase64String(AvatarBase64);
                if (!string.IsNullOrEmpty(fallbackPath))
                {
                    try { File.WriteAllBytes(fallbackPath, bytes); } catch {}
                }
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (Image temp = Image.FromStream(ms))
                    {
                        return new Bitmap(temp);
                    }
                }
            }
            catch { return null; }
        }

        public static void EnsureIcon(string iconPath)
        {
            if (!File.Exists(iconPath))
            {
                try
                {
                    byte[] bytes = Convert.FromBase64String(IconBase64);
                    File.WriteAllBytes(iconPath, bytes);
                }
                catch {}
            }
        }
    }

}