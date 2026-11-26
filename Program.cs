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
[assembly: AssemblyDescription("千牛工作台快捷回复辅助工具")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("HuahuaHelper")]
[assembly: AssemblyProduct("客服花花")]
[assembly: AssemblyCopyright("Copyright (C) 2026 HuahuaHelper")]
[assembly: AssemblyVersion("1.3.0.0")]
[assembly: AssemblyFileVersion("1.3.0.0")]

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
    /// 主题色板定义（支持 主题一「暖橙纸感」 与 主题二「樱粉花语」）
    /// </summary>
    public class ThemePalette
    {
        public bool IsPink;
        public Color Primary;
        public Color PrimaryHover;
        public Color PrimaryPressed;
        public Color DangerRed;
        public Color FocusRing;

        public Color LightPaperBg;
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

        public Color DarkPaperBg;
        public Color DarkCardBg;
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
        public static readonly ThemePalette OrangePalette = new ThemePalette
        {
            IsPink = false,
            Primary = Color.FromArgb(255, 80, 0),            // #FF5000
            PrimaryHover = Color.FromArgb(255, 106, 31),     // #FF6A1F
            PrimaryPressed = Color.FromArgb(230, 72, 0),     // #E64800
            DangerRed = Color.FromArgb(220, 38, 38),         // #DC2626
            FocusRing = Color.FromArgb(255, 178, 135),       // #FFB287

            LightPaperBg = Color.FromArgb(250, 247, 242),    // #FAF7F2
            LightCardBg = Color.FromArgb(255, 255, 255),     // #FFFFFF
            LightTextPrimary = Color.FromArgb(41, 37, 36),   // #292524
            LightTextSecondary = Color.FromArgb(120, 113, 108), // #78716C
            LightTextMuted = Color.FromArgb(168, 162, 158),  // #A8A29E
            LightChipText = Color.FromArgb(87, 83, 78),      // #57534E
            LightDivider = Color.FromArgb(234, 227, 216),    // #EAE3D8
            LightBorder = Color.FromArgb(231, 224, 214),     // #E7E0D6
            LightRowHover = Color.FromArgb(255, 241, 232),   // #FFF1E8
            LightRowSelected = Color.FromArgb(255, 231, 216),
            LightSearchBg = Color.FromArgb(245, 241, 234),   // #F5F1EA
            LightScrollThumb = Color.FromArgb(214, 208, 198),// #D6D0C6

            DarkPaperBg = Color.FromArgb(28, 25, 23),        // #1C1917
            DarkCardBg = Color.FromArgb(41, 37, 36),         // #292524
            DarkTextPrimary = Color.FromArgb(245, 240, 234), // #F5F0EA
            DarkTextSecondary = Color.FromArgb(168, 162, 158), // #A8A29E
            DarkTextMuted = Color.FromArgb(120, 113, 108),   // #78716C
            DarkDivider = Color.FromArgb(68, 64, 60),        // #44403C
            DarkPrimaryAccent = Color.FromArgb(255, 106, 31),// #FF6A1F
            DarkRowHover = Color.FromArgb(59, 36, 26),       // #3B241A
            DarkRowSelected = Color.FromArgb(74, 43, 30),
            DarkScrollThumb = Color.FromArgb(87, 83, 78)     // #57534E
        };

        public static readonly ThemePalette PinkPalette = new ThemePalette
        {
            IsPink = true,
            Primary = Color.FromArgb(250, 114, 152),         // #FA7298
            PrimaryHover = Color.FromArgb(251, 140, 169),    // #FB8CA9
            PrimaryPressed = Color.FromArgb(232, 93, 126),   // #E85D7E
            DangerRed = Color.FromArgb(220, 38, 38),         // #DC2626
            FocusRing = Color.FromArgb(253, 185, 200),       // #FDB9C8

            LightPaperBg = Color.FromArgb(253, 248, 249),    // #FDF8F9
            LightCardBg = Color.FromArgb(255, 255, 255),     // #FFFFFF
            LightTextPrimary = Color.FromArgb(51, 46, 47),   // #332E2F
            LightTextSecondary = Color.FromArgb(151, 138, 142), // #978A8E
            LightTextMuted = Color.FromArgb(196, 184, 187),  // #C4B8BB
            LightChipText = Color.FromArgb(87, 83, 78),      // #57534E
            LightDivider = Color.FromArgb(243, 226, 230),    // #F3E2E6
            LightBorder = Color.FromArgb(237, 217, 222),     // #EDD9DE
            LightRowHover = Color.FromArgb(253, 238, 242),   // #FDEEF2
            LightRowSelected = Color.FromArgb(251, 224, 232),
            LightSearchBg = Color.FromArgb(249, 239, 241),   // #F9EFF1
            LightScrollThumb = Color.FromArgb(227, 203, 210),// #E3CBD2

            DarkPaperBg = Color.FromArgb(34, 26, 30),        // #221A1E
            DarkCardBg = Color.FromArgb(46, 36, 41),         // #2E2429
            DarkTextPrimary = Color.FromArgb(253, 242, 245), // #FDF2F5
            DarkTextSecondary = Color.FromArgb(184, 166, 171), // #B8A6AB
            DarkTextMuted = Color.FromArgb(138, 118, 124),   // #8A767C
            DarkDivider = Color.FromArgb(74, 54, 62),        // #4A363E
            DarkPrimaryAccent = Color.FromArgb(251, 140, 169), // #FB8CA9
            DarkRowHover = Color.FromArgb(61, 37, 48),       // #3D2530
            DarkRowSelected = Color.FromArgb(78, 45, 60),
            DarkScrollThumb = Color.FromArgb(90, 66, 75)     // #5A424B
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
        public static Color DarkCardBg { get { return Current.DarkCardBg; } }
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
    /// iOS 26 Liquid Glass GDI+ 渲染引擎（无第三方依赖）
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
        /// 顶栏 / 玻璃卡片面板绘制：分层半透明色块模拟背景模糊 + 顶部高光 + 内折射
        /// </summary>
        public static void DrawLiquidGlassPanel(Graphics g, Rectangle rect, bool isDark, int radius = 0)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            LiquidGlassParams p = isDark ? LiquidGlassParams.Dark : LiquidGlassParams.Light;

            using (GraphicsPath path = CreateRoundedPath(rect, radius))
            {
                // 分层半透明色块模拟毛玻璃模糊散射
                using (Brush b1 = new SolidBrush(p.BlurLayer1)) g.FillPath(b1, path);
                Rectangle innerBlur = new Rectangle(rect.X + 1, rect.Y + 1, Math.Max(1, rect.Width - 2), Math.Max(1, rect.Height - 2));
                using (GraphicsPath innerPath = CreateRoundedPath(innerBlur, Math.Max(0, radius - 1)))
                using (Brush b2 = new SolidBrush(p.BlurLayer2))
                {
                    g.FillPath(b2, innerPath);
                }
                using (Brush bGlass = new SolidBrush(p.GlassFill)) g.FillPath(bGlass, path);

                // 顶部 1px 高光描边
                using (Pen topPen = new Pen(p.TopHighlight, 1))
                {
                    g.DrawLine(topPen, rect.X + radius, rect.Y, rect.Right - radius, rect.Y);
                }
            }
        }

        /// <summary>
        /// iOS 26 Liquid Glass 按钮完整实现（半透明玻璃 / 主色液态玻璃、内侧折射、顶部 1px 高光、内外阴影）
        /// </summary>
        public static void DrawLiquidGlassButton(Graphics g, Rectangle rect, Color accentColor, bool isPrimary, bool isDark, bool isHover, bool isPressed = false, int radius = 6)
        {
            if (rect.Width <= 2 || rect.Height <= 2) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            LiquidGlassParams p = isDark ? LiquidGlassParams.Dark : LiquidGlassParams.Light;

            // 1. 外层柔和投影（两层扩散，模拟悬浮感）
            if (!isPressed)
            {
                Rectangle s2 = new Rectangle(rect.X, rect.Y + 2, rect.Width, rect.Height);
                Rectangle s1 = new Rectangle(rect.X, rect.Y + 1, rect.Width, rect.Height);
                using (GraphicsPath p2 = CreateRoundedPath(s2, radius))
                using (Brush b2 = new SolidBrush(Color.FromArgb(p.OuterShadow.A / 2, p.OuterShadow)))
                    g.FillPath(b2, p2);
                using (GraphicsPath p1 = CreateRoundedPath(s1, radius))
                using (Brush b1 = new SolidBrush(p.OuterShadow))
                    g.FillPath(b1, p1);
            }

            using (GraphicsPath path = CreateRoundedPath(rect, radius))
            {
                // 2. 分层半透明底色（模拟背景模糊）
                if (isPrimary)
                {
                    int baseAlpha = isPressed ? 245 : (isHover ? 235 : 215);
                    using (Brush baseBrush = new SolidBrush(Color.FromArgb(baseAlpha, accentColor)))
                        g.FillPath(baseBrush, path);

                    // 叠加白色玻璃半透层 (12%~28%) 产生通透液态感
                    Color overlayCol = isHover ? p.GlassHoverFill : p.GlassFill;
                    using (Brush glassOverlay = new SolidBrush(Color.FromArgb(isPressed ? 24 : overlayCol.A, 255, 255, 255)))
                        g.FillPath(glassOverlay, path);
                }
                else
                {
                    using (Brush b1 = new SolidBrush(p.BlurLayer1)) g.FillPath(b1, path);
                    using (Brush b2 = new SolidBrush(p.BlurLayer2)) g.FillPath(b2, path);
                    Color glassFill = isPressed ? p.BlurLayer2 : (isHover ? p.GlassHoverFill : p.GlassFill);
                    using (Brush bGlass = new SolidBrush(glassFill)) g.FillPath(bGlass, path);
                }

                // 3. 边缘内侧加亮模拟折射（内缩 1px 描边）
                Rectangle refractRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
                if (refractRect.Width > 2 && refractRect.Height > 2)
                {
                    using (GraphicsPath rPath = CreateRoundedPath(refractRect, Math.Max(1, radius - 1)))
                    using (Pen refractPen = new Pen(isPrimary ? Color.FromArgb(115, 255, 255, 255) : p.InnerRefract, 1))
                    {
                        g.DrawPath(refractPen, rPath);
                    }
                }

                // 4. 顶部 1px 高光描边 + 底部内阴影
                g.SetClip(path);
                using (Pen topPen = new Pen(p.TopHighlight, 1))
                {
                    g.DrawLine(topPen, rect.X + 2, rect.Y + 1, rect.Right - 2, rect.Y + 1);
                }
                using (Pen innerShadowPen = new Pen(p.InnerShadow, 1))
                {
                    g.DrawLine(innerShadowPen, rect.X + 2, rect.Bottom - 2, rect.Right - 2, rect.Bottom - 2);
                }
                g.ResetClip();

                // 5. 外轮廓细边框
                Color outerCol = isPrimary ? Color.FromArgb(180, accentColor) : p.OuterBorder;
                using (Pen outerPen = new Pen(outerCol, 1))
                {
                    g.DrawPath(outerPen, path);
                }
            }
        }

        /// <summary>
        /// 扁平实色按钮绘制（保留给次按钮/分类胶囊使用）
        /// </summary>
        public static void DrawLiquidGlassButton(Graphics g, Rectangle rect, Color fillNormal, Color fillHover, Color borderCol, bool isHover, int radius = 6, bool isPressed = false, Color? fillPressed = null)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. 克制投影
            Rectangle shadowRect = new Rectangle(rect.X, rect.Y + 1, rect.Width, rect.Height);
            using (GraphicsPath sPath = CreateRoundedPath(shadowRect, radius))
            using (Brush sBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
            {
                g.FillPath(sBrush, sPath);
            }

            // 2. 实色填充
            Color bg = fillNormal;
            if (isPressed && fillPressed.HasValue) bg = fillPressed.Value;
            else if (isHover) bg = fillHover;

            using (GraphicsPath path = CreateRoundedPath(rect, radius))
            {
                if (bg.A > 0)
                {
                    using (Brush b = new SolidBrush(bg))
                    {
                        g.FillPath(b, path);
                    }
                }

                // 3. 1px 边框
                if (borderCol.A > 0)
                {
                    using (Pen pen = new Pen(borderCol, 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }
        }

        private static Pen CreateIconPen(Color color)
        {
            Pen p = new Pen(color, 1.5f);
            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.Round;
            p.LineJoin = LineJoin.Round;
            return p;
        }

        /// <summary>
        /// 16x16 矢量齿轮图标
        /// </summary>
        public static void DrawVectorGearIcon(Graphics g, Rectangle rect, Color gearColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            float outerR = Math.Min(rect.Width, rect.Height) * 0.36f;
            float innerR = outerR * 0.72f;
            float holeR = outerR * 0.34f;

            using (GraphicsPath gearPath = new GraphicsPath())
            {
                int teeth = 6;
                double step = Math.PI * 2 / teeth;
                double toothHalf = step * 0.22;

                List<PointF> pts = new List<PointF>();
                for (int i = 0; i < teeth; i++)
                {
                    double angle = i * step;

                    double a1 = angle - toothHalf * 1.5;
                    pts.Add(new PointF(cx + (float)(Math.Cos(a1) * innerR), cy + (float)(Math.Sin(a1) * innerR)));

                    double a2 = angle - toothHalf * 0.7;
                    pts.Add(new PointF(cx + (float)(Math.Cos(a2) * outerR), cy + (float)(Math.Sin(a2) * outerR)));

                    double a3 = angle + toothHalf * 0.7;
                    pts.Add(new PointF(cx + (float)(Math.Cos(a3) * outerR), cy + (float)(Math.Sin(a3) * outerR)));

                    double a4 = angle + toothHalf * 1.5;
                    pts.Add(new PointF(cx + (float)(Math.Cos(a4) * innerR), cy + (float)(Math.Sin(a4) * innerR)));
                }

                gearPath.AddPolygon(pts.ToArray());
                gearPath.AddEllipse(cx - holeR, cy - holeR, holeR * 2, holeR * 2);

                using (Brush b = new SolidBrush(gearColor))
                {
                    g.FillPath(b, gearPath);
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
    }

    /// <summary>
    /// 文本 / 视频 模式切换栏（位于分类栏上方）
    /// </summary>
    public class ModeSwitchBar : Control
    {
        public string ActiveMode { get; set; } // "text" or "video"
        public int ItemCount { get; set; }
        public bool IsDarkTheme { get; set; }
        public event Action<string> OnModeChanged;

        private int _hoverTab = -1; // 0: text, 1: video

        public ModeSwitchBar()
        {
            ActiveMode = "text";
            ItemCount = 0;
            IsDarkTheme = false;
            this.Height = 34;
            this.Cursor = Cursors.Hand;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);

            this.MouseMove += (s, e) =>
            {
                int prev = _hoverTab;
                Rectangle rText = new Rectangle(12, 4, 64, 25);
                Rectangle rVideo = new Rectangle(82, 4, 64, 25);
                if (rText.Contains(e.Location)) _hoverTab = 0;
                else if (rVideo.Contains(e.Location)) _hoverTab = 1;
                else _hoverTab = -1;
                if (prev != _hoverTab) this.Invalidate();
            };
            this.MouseLeave += (s, e) => { _hoverTab = -1; this.Invalidate(); };
            this.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                if (_hoverTab == 0 && ActiveMode != "text")
                {
                    ActiveMode = "text";
                    this.Invalidate();
                    if (OnModeChanged != null) OnModeChanged("text");
                }
                else if (_hoverTab == 1 && ActiveMode != "video")
                {
                    ActiveMode = "video";
                    this.Invalidate();
                    if (OnModeChanged != null) OnModeChanged("video");
                }
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color barBg = IsDarkTheme ? WarmTheme.DarkCardBg : WarmTheme.LightCardBg;
            Color lineCol = IsDarkTheme ? WarmTheme.DarkDivider : WarmTheme.LightDivider;
            g.Clear(barBg);

            using (Pen linePen = new Pen(lineCol, 1))
            {
                g.DrawLine(linePen, 0, this.Height - 1, this.Width, this.Height - 1);
            }

            Rectangle rText = new Rectangle(12, 4, 64, 25);
            Rectangle rVideo = new Rectangle(82, 4, 64, 25);

            bool textActive = (ActiveMode != "video");
            bool videoActive = (ActiveMode == "video");

            using (Font fReg = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Regular))
            using (Font fBold = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold))
            using (Font fCount = new Font("Microsoft YaHei UI", 8F, FontStyle.Regular))
            {
                // 文本按钮
                DrawTab(g, rText, "文本", textActive, _hoverTab == 0, true, fReg, fBold);
                // 视频按钮
                DrawTab(g, rVideo, "视频", videoActive, _hoverTab == 1, false, fReg, fBold);

                // 右侧数量统计
                Color cntCol = IsDarkTheme ? WarmTheme.DarkTextMuted : WarmTheme.LightTextMuted;
                Rectangle rCount = new Rectangle(152, 4, Math.Max(40, this.ClientSize.Width - 164), 25);
                TextRenderer.DrawText(g, string.Format("共 {0} 条", ItemCount), fCount, rCount, cntCol,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }
        }

        private void DrawTab(Graphics g, Rectangle rect, string label, bool isActive, bool isHover, bool isDoc, Font fReg, Font fBold)
        {
            Color txtCol;
            if (isActive)
            {
                GlassRenderer.DrawLiquidGlassButton(g, rect, WarmTheme.Orange, true, IsDarkTheme, isHover, false, 12);
                txtCol = Color.White;
            }
            else
            {
                Color bgNormal = IsDarkTheme ? Color.Transparent : WarmTheme.LightSearchBg;
                Color bgHover = IsDarkTheme ? WarmTheme.DarkRowHover : WarmTheme.LightRowHover;
                Color bdr = IsDarkTheme ? WarmTheme.DarkDivider : WarmTheme.LightBorder;
                txtCol = IsDarkTheme ? WarmTheme.DarkTextSecondary : WarmTheme.LightChipText;
                GlassRenderer.DrawLiquidGlassButton(g, rect, bgNormal, bgHover, bdr, isHover, 12);
            }

            Rectangle iconRect = new Rectangle(rect.X + 7, rect.Y + (rect.Height - 16) / 2, 16, 16);
            if (isDoc) GlassRenderer.DrawVectorDocIcon(g, iconRect, txtCol);
            else GlassRenderer.DrawVectorVideoIcon(g, iconRect, txtCol);

            Rectangle txtRect = new Rectangle(rect.X + 23, rect.Y, rect.Width - 27, rect.Height);
            TextRenderer.DrawText(g, label, isActive ? fBold : fReg, txtRect, txtCol,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }

    /// <summary>
    /// 自定义纯自绘分类导航栏（暖橙纸感胶囊 chip）
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

        private int _scrollX = 0;
        private int _hoverIndex = -1;
        private bool _hoverAddBtn = false;
        private bool _hoverDelBtn = false;

        public CustomCategoryBar()
        {
            Categories = new List<string>();
            ActiveCategory = "全部";
            IsDarkTheme = false;
            this.Height = 40;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);

            this.MouseWheel += (s, e) =>
            {
                _scrollX += (e.Delta > 0 ? 30 : -30);
                ClampScroll();
                this.Invalidate();
            };

            this.MouseMove += CustomCategoryBar_MouseMove;
            this.MouseLeave += (s, e) => { _hoverIndex = -1; _hoverAddBtn = false; _hoverDelBtn = false; this.Invalidate(); };
            this.MouseDown += CustomCategoryBar_MouseDown;
        }

        public void SetCategories(List<string> cats, string active)
        {
            Categories = new List<string> { "全部" };
            if (cats != null) Categories.AddRange(cats);
            ActiveCategory = active ?? "全部";
            _hoverIndex = -1;
            _hoverAddBtn = false;
            _hoverDelBtn = false;
            _scrollX = 0;
            ClampScroll();
            this.Invalidate();
        }

        private int GetTotalWidth(Graphics g, Font font)
        {
            int total = 12;
            for (int i = 0; i < Categories.Count; i++)
            {
                SizeF size = g.MeasureString(Categories[i], font);
                total += (int)size.Width + 24;
            }
            return total;
        }

        private void ClampScroll()
        {
            using (Graphics g = this.CreateGraphics())
            using (Font f = new Font("Microsoft YaHei UI", 9F))
            {
                int total = GetTotalWidth(g, f);
                int scrollAreaW = Math.Max(40, this.ClientSize.Width - 76);
                int maxScroll = Math.Max(0, total - scrollAreaW);
                if (_scrollX < -maxScroll) _scrollX = -maxScroll;
                if (_scrollX > 0) _scrollX = 0;
            }
        }

        private void CalculateHitBoxes(Graphics g, Font font, Font activeFont, out List<Rectangle> catRects, out Rectangle addRect, out Rectangle delRect)
        {
            catRects = new List<Rectangle>();
            int curX = 12 + _scrollX;
            for (int i = 0; i < Categories.Count; i++)
            {
                string cat = Categories[i];
                bool isActive = (cat == ActiveCategory);
                Font drawFont = isActive ? activeFont : font;
                SizeF size = g.MeasureString(cat, drawFont);
                int itemW = (int)size.Width + 18;
                catRects.Add(new Rectangle(curX, 6, itemW, 28));
                curX += itemW + 6;
            }
            // 固定在右侧，最窄窗口下也 100% 可见
            int rightEdge = Math.Max(100, this.ClientSize.Width);
            delRect = new Rectangle(rightEdge - 36, 6, 28, 28);
            addRect = new Rectangle(rightEdge - 68, 6, 28, 28);
        }

        private void HitTest(Point pt, out int hitIdx, out bool hitAdd, out bool hitDel)
        {
            hitIdx = -1;
            hitAdd = false;
            hitDel = false;
            using (Graphics g = this.CreateGraphics())
            using (Font font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular))
            using (Font activeFont = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold))
            {
                List<Rectangle> catRects;
                Rectangle addRect, delRect;
                CalculateHitBoxes(g, font, activeFont, out catRects, out addRect, out delRect);

                // 优先命中右侧固定的 [+] [-] 按钮
                if (addRect.Contains(pt)) { hitAdd = true; return; }
                if (delRect.Contains(pt)) { hitDel = true; return; }

                int clipRight = Math.Max(40, this.ClientSize.Width - 74);
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
            bool prevAdd = _hoverAddBtn;
            bool prevDel = _hoverDelBtn;

            HitTest(e.Location, out _hoverIndex, out _hoverAddBtn, out _hoverDelBtn);

            if (prevIdx != _hoverIndex || prevAdd != _hoverAddBtn || prevDel != _hoverDelBtn)
            {
                this.Invalidate();
            }
        }

        private void CustomCategoryBar_MouseDown(object sender, MouseEventArgs e)
        {
            int hitIdx;
            bool hitAdd, hitDel;
            HitTest(e.Location, out hitIdx, out hitAdd, out hitDel);

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
                    ContextMenuStrip cms = new ContextMenuStrip();
                    ToolStripMenuItem itemRename = new ToolStripMenuItem("修改分类名称");
                    itemRename.Click += (s, ev) => { if (OnRenameCategoryRequested != null) OnRenameCategoryRequested(cat); };
                    ToolStripMenuItem itemDel = new ToolStripMenuItem("删除此分类");
                    itemDel.Click += (s, ev) => { if (OnDeleteCategoryRequested != null) OnDeleteCategoryRequested(cat); };

                    cms.Items.Add(itemRename);
                    cms.Items.Add(itemDel);
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

            Color barBg = IsDarkTheme ? WarmTheme.DarkCardBg : WarmTheme.LightCardBg;
            Color bottomLine = IsDarkTheme ? WarmTheme.DarkDivider : WarmTheme.LightDivider;

            g.Clear(barBg);
            using (Pen linePen = new Pen(bottomLine, 1))
            {
                g.DrawLine(linePen, 0, this.Height - 1, this.Width, this.Height - 1);
            }

            using (Font font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular))
            using (Font activeFont = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold))
            {
                List<Rectangle> catRects;
                Rectangle addRect, delRect;
                CalculateHitBoxes(g, font, activeFont, out catRects, out addRect, out delRect);

                int clipRight = Math.Max(40, this.ClientSize.Width - 74);
                g.SetClip(new Rectangle(0, 0, clipRight, this.Height));

                for (int i = 0; i < Categories.Count; i++)
                {
                    string cat = Categories[i];
                    bool isActive = (cat == ActiveCategory);
                    bool isHover = (i == _hoverIndex);
                    Rectangle rect = catRects[i];

                    if (isActive)
                    {
                        // 选中态：主色液态玻璃胶囊 chip
                        GlassRenderer.DrawLiquidGlassButton(g, rect, WarmTheme.Orange, true, IsDarkTheme, isHover, false, 14);
                        TextRenderer.DrawText(g, cat, activeFont, rect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    else
                    {
                        // 未选中态：白底（深色下透明）+ 1px 边框
                        Color bgNormal = IsDarkTheme ? Color.Transparent : WarmTheme.LightCardBg;
                        Color bgHover = IsDarkTheme ? WarmTheme.DarkRowHover : WarmTheme.LightSearchBg;
                        Color bdr = IsDarkTheme ? WarmTheme.DarkDivider : WarmTheme.LightBorder;
                        Color txt = IsDarkTheme ? WarmTheme.DarkTextSecondary : WarmTheme.LightChipText;

                        GlassRenderer.DrawLiquidGlassButton(g, rect, bgNormal, bgHover, bdr, isHover, 14);
                        TextRenderer.DrawText(g, cat, font, rect, txt, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }

                g.ResetClip();

                // 用实底遮罩盖住右侧固定区，彻底避免左侧滚动文字/胶囊渗入 [+] [-] 下方
                int dockLeft = Math.Max(40, this.ClientSize.Width - 74);
                using (Brush maskBrush = new SolidBrush(barBg))
                {
                    g.FillRectangle(maskBrush, dockLeft, 0, this.ClientSize.Width - dockLeft, this.Height - 1);
                }
                using (Pen divPen = new Pen(bottomLine, 1))
                {
                    g.DrawLine(divPen, dockLeft, 6, dockLeft, this.Height - 7);
                }

                // [+] 增加分类：幽灵按钮 28x28，矢量加号
                Color ghostBdr = IsDarkTheme ? WarmTheme.DarkDivider : WarmTheme.LightBorder;
                Color addHoverBg = IsDarkTheme ? WarmTheme.DarkRowHover : WarmTheme.LightRowHover;
                Color addIconCol = _hoverAddBtn
                    ? (IsDarkTheme ? WarmTheme.OrangeHover : WarmTheme.Orange)
                    : (IsDarkTheme ? WarmTheme.DarkTextSecondary : WarmTheme.LightChipText);

                GlassRenderer.DrawLiquidGlassButton(g, addRect, Color.Transparent, addHoverBg, ghostBdr, _hoverAddBtn, 6);
                GlassRenderer.DrawVectorPlusIcon(g, addRect, addIconCol);

                // [-] 删除分类：幽灵按钮 28x28，矢量减号，悬停变砖红
                Color delHoverBg = IsDarkTheme ? Color.FromArgb(60, 28, 28) : Color.FromArgb(254, 226, 226);
                Color delIconCol = _hoverDelBtn
                    ? WarmTheme.DangerRed
                    : (IsDarkTheme ? WarmTheme.DarkTextSecondary : WarmTheme.LightChipText);

                GlassRenderer.DrawLiquidGlassButton(g, delRect, Color.Transparent, delHoverBg, ghostBdr, _hoverDelBtn, 6);
                GlassRenderer.DrawVectorMinusIcon(g, delRect, delIconCol);
            }
        }
    }

    /// <summary>
    /// 暖橙纸感工作区列表（双行密排、扁平实色按键、自绘细滚动条、空状态）
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
        private int _hoverBtnType = 0; // 0: 无, 1: 编辑, 2: 填入, 3: 发送
        private int _scrollOffset = 0;
        private bool _isDraggingScroll = false;
        private int _dragStartY = 0;
        private int _dragStartOffset = 0;
        private Bitmap _cachedBgBitmap = null;
        private int _bgRenderVersion = 0;
        private Font _titleFont;
        private Font _secFont;
        private Font _btnFont;
        private Font _btnBoldFont;

        public event Action<PhraseItem, bool> OnActionTriggered;
        public event Action<PhraseItem> OnEditRequested;

        public MiniPhraseListView()
        {
            Items = new List<PhraseItem>();
            ItemHeight = 36;
            CustomFontSize = 9.0F;
            ThemeType = "white";
            CurrentMode = "text";
            RebuildFonts();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);

            this.MouseWheel += (s, e) =>
            {
                int maxScroll = GetMaxScroll();
                if (maxScroll > 0)
                {
                    _scrollOffset -= (e.Delta / 120) * ItemHeight;
                    if (_scrollOffset < 0) _scrollOffset = 0;
                    if (_scrollOffset > maxScroll) _scrollOffset = maxScroll;
                    this.Invalidate();
                }
            };

            this.MouseMove += MiniPhraseListView_MouseMove;
            this.MouseLeave += (s, e) => { _hoverIndex = -1; _hoverBtnType = 0; this.Invalidate(); };
            this.MouseDown += MiniPhraseListView_MouseDown;
            this.MouseUp += (s, e) => { _isDraggingScroll = false; };
            this.DoubleClick += MiniPhraseListView_DoubleClick;
        }

        private void RebuildFonts()
        {
            if (_titleFont != null) _titleFont.Dispose();
            if (_secFont != null) _secFont.Dispose();
            if (_btnFont == null) _btnFont = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
            if (_btnBoldFont == null) _btnBoldFont = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);

            _titleFont = new Font("Microsoft YaHei UI", CustomFontSize, FontStyle.Regular);
            _secFont = new Font("Microsoft YaHei UI", GetSecondaryFontSize(CustomFontSize), FontStyle.Regular);
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
            ClampScroll();
            this.Invalidate();
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

            int thumbH = Math.Max(24, (int)((double)viewH * viewH / totalH));
            int maxThumbY = viewH - thumbH;
            int maxScroll = totalH - viewH;
            int thumbY = maxScroll > 0 ? (int)((double)_scrollOffset / maxScroll * maxThumbY) : 0;
            return new Rectangle(this.ClientSize.Width - 8, thumbY + 2, 5, Math.Max(18, thumbH - 4));
        }

        private float GetSecondaryFontSize(float titleSize)
        {
            if (titleSize >= 15.0F) return 11.0F;
            if (titleSize >= 13.0F) return 10.0F;
            if (titleSize >= 11.0F) return 9.0F;
            return 8.0F;
        }

        private void MiniPhraseListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingScroll)
            {
                int totalH = Items.Count * ItemHeight;
                int viewH = this.ClientSize.Height;
                Rectangle thumb = GetScrollThumbRect();
                int trackRange = Math.Max(1, viewH - thumb.Height);
                int maxScroll = GetMaxScroll();
                int deltaY = e.Y - _dragStartY;
                _scrollOffset = _dragStartOffset + (int)((double)deltaY / trackRange * maxScroll);
                ClampScroll();
                this.Invalidate();
                return;
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
                int rightMargin = NeedsScrollBar() ? 12 : 4;
                int usableW = this.ClientSize.Width - rightMargin;
                int itemY = (newIdx * ItemHeight) - offset;

                int sendW = 36;
                int fillW = 32;
                int editW = 24;
                int btnH = Math.Min(24, ItemHeight - 8);
                int btnY = itemY + (ItemHeight - btnH) / 2;

                int sendLeft = usableW - sendW - 8;
                int fillLeft = sendLeft - fillW - 4;
                int editLeft = fillLeft - editW - 4;

                Rectangle rSend = new Rectangle(sendLeft, btnY, sendW, btnH);
                Rectangle rFill = new Rectangle(fillLeft, btnY, fillW, btnH);
                Rectangle rEdit = new Rectangle(editLeft, btnY, editW, btnH);

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
            if (e.Button != MouseButtons.Left) return;

            if (NeedsScrollBar() && e.X >= this.ClientSize.Width - 12)
            {
                Rectangle thumb = GetScrollThumbRect();
                if (thumb.Contains(e.Location))
                {
                    _isDraggingScroll = true;
                    _dragStartY = e.Y;
                    _dragStartOffset = _scrollOffset;
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

            // 空状态绘制（居中显示花花头像小图 + 提示语）
            if (Items.Count == 0)
            {
                int cy = this.ClientSize.Height / 2;
                int cx = this.ClientSize.Width / 2;
                if (EmptyAvatarImage != null)
                {
                    Rectangle imgRect = new Rectangle(cx - 24, cy - 42, 48, 48);
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
                        : (isDark ? WarmTheme.DarkDivider : WarmTheme.LightBorder);
                    float ringW = WarmTheme.Current.IsPink ? 2.0f : 1.0f;
                    using (Pen ringPen = new Pen(ringCol, ringW))
                    {
                        g.DrawEllipse(ringPen, imgRect);
                    }
                }
                Color emptyTextCol = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary;
                Rectangle txtRect = new Rectangle(12, cy + 14, this.ClientSize.Width - 24, 24);
                string emptyHint = (CurrentMode == "video") ? "还没有视频快捷方式，点右上角加一个吧" : "还没有话术，点右上角加一个吧";
                using (Font emptyFont = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, emptyHint, emptyFont, txtRect, emptyTextCol,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                return;
            }

            int offset = _scrollOffset;
            int start = Math.Max(0, offset / ItemHeight);
            int count = (this.ClientSize.Height / ItemHeight) + 2;
            int end = Math.Min(Items.Count - 1, start + count);

            int rightMargin = NeedsScrollBar() ? 12 : 4;
            int usableW = this.ClientSize.Width - rightMargin;

            Color textPrimary = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
            Color textSecondary = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary;
            Color rowLine = isDark ? WarmTheme.DarkDivider : WarmTheme.LightDivider;

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

                    Rectangle rowRect = new Rectangle(4, y + 1, usableW - 6, ItemHeight - 2);

                    // 行背景（白底 / 深暖灰，悬停暖橙 tint）
                    Color rowBg;
                    if (isDark)
                    {
                        if (isSelected) rowBg = WarmTheme.DarkRowSelected;
                        else if (isHover) rowBg = WarmTheme.DarkRowHover;
                        else rowBg = WarmTheme.DarkCardBg;
                    }
                    else if (ThemeType == "custom")
                    {
                        if (isSelected) rowBg = Color.FromArgb(230, WarmTheme.LightRowSelected);
                        else if (isHover) rowBg = Color.FromArgb(215, WarmTheme.LightRowHover);
                        else rowBg = Color.FromArgb(155, WarmTheme.LightCardBg);
                    }
                    else
                    {
                        if (isSelected) rowBg = WarmTheme.LightRowSelected;
                        else if (isHover) rowBg = WarmTheme.LightRowHover;
                        else rowBg = WarmTheme.LightCardBg;
                    }

                    if (isHover || isSelected)
                    {
                        using (GraphicsPath rowPath = GlassRenderer.CreateRoundedPath(rowRect, 4))
                        using (Brush bgBrush = new SolidBrush(rowBg))
                        {
                            g.FillPath(bgBrush, rowPath);
                        }
                    }
                    else
                    {
                        using (Brush bgBrush = new SolidBrush(rowBg))
                        {
                            g.FillRectangle(bgBrush, rowRect);
                        }
                    }
                    g.DrawLine(linePen, 12, y + ItemHeight - 1, usableW - 4, y + ItemHeight - 1);

                    int sendW = 36;
                    int fillW = 32;
                    int editW = 24;
                    int btnH = Math.Min(24, ItemHeight - 8);
                    int btnY = y + (ItemHeight - btnH) / 2;

                    int sendLeft = usableW - sendW - 8;
                    int fillLeft = sendLeft - fillW - 4;
                    int editLeft = fillLeft - editW - 4;

                    Rectangle rSend = new Rectangle(sendLeft, btnY, sendW, btnH);
                    Rectangle rFill = new Rectangle(fillLeft, btnY, fillW, btnH);
                    Rectangle rEdit = new Rectangle(editLeft, btnY, editW, btnH);

                    int textLeft = 12;
                    int textW = editLeft - textLeft - 6;
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
                    {
                        subInfo += " · 视频";
                    }
                    else if (!string.IsNullOrEmpty(item.ImagePath))
                    {
                        subInfo += " · 带图";
                    }
                    TextRenderer.DrawText(g, subInfo, secFont, secRect, textSecondary,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);

                    // 次按钮通用色（白底 / 深暖灰，1px 暖边框）
                    Color secNormal = isDark ? WarmTheme.DarkCardBg : WarmTheme.LightCardBg;
                    Color secHover = isDark ? WarmTheme.DarkRowHover : WarmTheme.LightSearchBg;
                    Color secBdr = isDark ? WarmTheme.DarkDivider : WarmTheme.LightBorder;
                    Color secTxt = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;

                    // 1. 编辑按钮（矢量铅笔图标）
                    bool hoverEdit = (isHover && _hoverBtnType == 1);
                    GlassRenderer.DrawLiquidGlassButton(g, rEdit, secNormal, secHover, secBdr, hoverEdit, 6);
                    GlassRenderer.DrawVectorEditIcon(g, rEdit, isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightChipText);

                    // 2. 填入按钮
                    bool hoverFill = (isHover && _hoverBtnType == 2);
                    GlassRenderer.DrawLiquidGlassButton(g, rFill, secNormal, secHover, secBdr, hoverFill, 6);
                    TextRenderer.DrawText(g, "填", btnFont, rFill, secTxt, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    // 3. 发送按钮（iOS 26 Liquid Glass 主按钮）
                    bool hoverSend = (isHover && _hoverBtnType == 3);
                    GlassRenderer.DrawLiquidGlassButton(g, rSend,
                        WarmTheme.Orange, true, isDark, hoverSend, false, 6);
                    TextRenderer.DrawText(g, "发", btnBoldFont, rSend, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }

            // 细滚动条滑块
            if (NeedsScrollBar())
            {
                Rectangle thumb = GetScrollThumbRect();
                if (!thumb.IsEmpty)
                {
                    Color thumbCol = isDark ? WarmTheme.DarkScrollThumb : WarmTheme.LightScrollThumb;
                    using (GraphicsPath thumbPath = GlassRenderer.CreateRoundedPath(thumb, 2))
                    using (Brush thumbBrush = new SolidBrush(thumbCol))
                    {
                        g.FillPath(thumbBrush, thumbPath);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 解决 WinForms 子控件透明背景继承父面板绘制色块的问题（消除文字/图标旁黑框）
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
            Color lineCol = IsDarkTheme ? WarmTheme.DarkDivider : WarmTheme.LightDivider;
            using (Pen p = new Pen(lineCol, 1))
            {
                e.Graphics.DrawLine(p, 0, this.Height - 1, this.Width, this.Height - 1);
            }
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
    /// 扁平实色/幽灵独立按钮控件（用于顶栏与弹窗）
    /// </summary>
    public class LiquidGlassButtonControl : Control
    {
        public bool IsHovered { get; private set; }
        public bool IsPressed { get; private set; }
        public bool UseLiquidGlass { get; set; }
        public bool IsPrimaryGlass { get; set; }
        public bool IsDarkTheme { get; set; }
        public Color BaseNormal { get; set; }
        public Color BaseHover { get; set; }
        public Color BasePressed { get; set; }
        public Color BorderColor { get; set; }
        public Color TextColor { get; set; }
        public bool IsGearIcon { get; set; }
        public int CornerRadius { get; set; }

        public LiquidGlassButtonControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.SupportsTransparentBackColor, true);

            BaseNormal = WarmTheme.LightCardBg;
            BaseHover = WarmTheme.LightSearchBg;
            BasePressed = WarmTheme.LightDivider;
            BorderColor = WarmTheme.LightBorder;
            TextColor = WarmTheme.LightChipText;
            CornerRadius = 6;
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;

            this.MouseEnter += (s, e) => { IsHovered = true; this.Invalidate(); };
            this.MouseLeave += (s, e) => { IsHovered = false; IsPressed = false; this.Invalidate(); };
            this.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { IsPressed = true; this.Invalidate(); } };
            this.MouseUp += (s, e) => { IsPressed = false; this.Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle rect = new Rectangle(1, 1, this.Width - 2, this.Height - 2);

            if (UseLiquidGlass)
            {
                Color accent = IsPrimaryGlass ? BaseNormal : BorderColor;
                GlassRenderer.DrawLiquidGlassButton(g, rect, accent, IsPrimaryGlass, IsDarkTheme, IsHovered, IsPressed, CornerRadius);
            }
            else
            {
                GlassRenderer.DrawLiquidGlassButton(g, rect, BaseNormal, BaseHover, BorderColor, IsHovered, CornerRadius, IsPressed, BasePressed);
            }

            if (IsGearIcon)
            {
                GlassRenderer.DrawVectorGearIcon(g, rect, TextColor);
            }
            else
            {
                using (Font font = new Font(this.Font.FontFamily, this.Font.Size, this.Font.Style))
                {
                    TextRenderer.DrawText(g, this.Text, font, rect, TextColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
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

        private const byte VK_CONTROL = 0x11;
        private const byte VK_V = 0x56;
        private const byte VK_RETURN = 0x0D;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private IntPtr _lastTargetHwnd = IntPtr.Zero;
        private string _lastTargetTitle = "";

        // UI 控件
        private System.Windows.Forms.Timer _windowTrackerTimer;
        private GlassTopBarPanel _topBar;
        private CircleAvatarControl _picAvatar;
        private Label _lblLogo;
        private LiquidGlassButtonControl _btnSettings; // 幽灵矢量齿轮按钮
        private WarmCheckBox _chkTopMost;
        private LiquidGlassButtonControl _btnAddPhrase; // 实色淘宝橙按钮
        private Panel _searchWrap;
        private TextBox _searchBox;
        private bool _searchFocused = false;
        private const string SearchPlaceholder = "搜话术、分类…";
        private ModeSwitchBar _modeBar;
        private CustomCategoryBar _categoryBar;
        private MiniPhraseListView _phraseList;
        private Panel _statusBar;
        private Label _lblTarget;

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
        private Color _primaryOrange = Color.FromArgb(255, 80, 0);

        public MainForm()
        {
            InitializeUi();
            LoadData();
            RestoreWindowBounds();
            ApplySettings();
            _categoryBar.SetCategories(ActiveCategories(), ActiveCat);
            RefreshList();
            StartTracking();

            this.ResizeEnd += (s, e) => SaveWindowBounds();
            this.FormClosing += (s, e) => SaveWindowBounds();
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

        private void InitializeUi()
        {
            this.Text = "客服花花";
            this.Size = new Size(320, 680);
            this.MinimumSize = new Size(292, 400);
            this.StartPosition = FormStartPosition.Manual;

            Rectangle workArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workArea.Right - 335, (workArea.Height - 680) / 2);
            this.TopMost = true;
            this.BackColor = WarmTheme.LightPaperBg;
            this.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            EmbeddedAssets.EnsureIcon(_iconPath);
            if (File.Exists(_iconPath))
            {
                try { this.Icon = new Icon(_iconPath); } catch { }
            }

            // 1. 顶部 Header（OnPaintBackground 绘制分层液态玻璃，子控件透明继承零黑框）
            _topBar = new GlassTopBarPanel
            {
                Dock = DockStyle.Top,
                Height = 72,
                Padding = new Padding(12, 6, 12, 6)
            };

            // 头像彩蛋（圆形裁剪，彻底告别灰底方框）
            _picAvatar = new CircleAvatarControl
            {
                Size = new Size(24, 24),
                Location = new Point(12, 8),
                AvatarImage = EmbeddedAssets.GetAvatarImage(_avatarPath)
            };
            ToolTip tip = new ToolTip();
            tip.SetToolTip(_picAvatar, "点一点，看花花的原画");
            _picAvatar.Click += (s, e) => ShowEasterEggDialog();

            _lblLogo = new Label
            {
                Text = "客服花花",
                Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold),
                ForeColor = WarmTheme.LightTextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(40, 9),
                Padding = new Padding(0),
                Margin = new Padding(0),
                AutoSize = true
            };

            // 【设置】iOS 26 Liquid Glass 矢量齿轮按钮
            _btnSettings = new LiquidGlassButtonControl
            {
                Size = new Size(28, 24),
                IsGearIcon = true,
                UseLiquidGlass = true,
                IsPrimaryGlass = false,
                CornerRadius = 6,
                BaseNormal = Color.Transparent,
                BaseHover = WarmTheme.LightSearchBg,
                BasePressed = WarmTheme.LightDivider,
                BorderColor = WarmTheme.LightBorder,
                TextColor = WarmTheme.LightChipText
            };
            tip.SetToolTip(_btnSettings, "设置");
            _btnSettings.Click += (s, e) => OpenSettingsDialog();

            _chkTopMost = new WarmCheckBox
            {
                Text = "置顶",
                Checked = true,
                Size = new Size(48, 22),
                Font = new Font("Microsoft YaHei UI", 8.5F)
            };
            _chkTopMost.CheckedChanged += (s, e) => this.TopMost = _chkTopMost.Checked;

            // 【+词】iOS 26 Liquid Glass 主按钮
            _btnAddPhrase = new LiquidGlassButtonControl
            {
                Text = "+词",
                Size = new Size(44, 24),
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                UseLiquidGlass = true,
                IsPrimaryGlass = true,
                BaseNormal = WarmTheme.Orange,
                BaseHover = WarmTheme.OrangeHover,
                BasePressed = WarmTheme.OrangePressed,
                BorderColor = WarmTheme.Orange,
                TextColor = Color.White,
                CornerRadius = 6
            };
            _btnAddPhrase.Click += (s, e) => ShowAddOrEditDialog(null);

            // 第二行：自绘圆角搜索框（底色 #F5F1EA，16x16 矢量放大镜，2px 焦点环）
            _searchWrap = new Panel
            {
                Location = new Point(12, 38),
                Size = new Size(280, 26),
                BackColor = Color.Transparent
            };
            _searchWrap.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                bool isDark = (_data != null && _data.Settings != null && _data.Settings.ThemeType == "black");
                Color bgCol = isDark ? WarmTheme.DarkPaperBg : WarmTheme.LightSearchBg;
                Color iconCol = isDark ? WarmTheme.DarkTextMuted : WarmTheme.LightTextMuted;

                Rectangle boxRect = new Rectangle(1, 1, _searchWrap.Width - 3, _searchWrap.Height - 3);
                using (GraphicsPath path = GlassRenderer.CreateRoundedPath(boxRect, 6))
                using (Brush b = new SolidBrush(bgCol))
                {
                    g.FillPath(b, path);
                }

                if (_searchFocused)
                {
                    using (GraphicsPath fPath = GlassRenderer.CreateRoundedPath(boxRect, 6))
                    using (Pen fPen = new Pen(WarmTheme.FocusRing, 2))
                    {
                        g.DrawPath(fPen, fPath);
                    }
                }
                else if (isDark)
                {
                    using (GraphicsPath bPath = GlassRenderer.CreateRoundedPath(boxRect, 6))
                    using (Pen bPen = new Pen(WarmTheme.DarkDivider, 1))
                    {
                        g.DrawPath(bPen, bPath);
                    }
                }

                GlassRenderer.DrawVectorSearchIcon(g, new Rectangle(6, 5, 16, 16), iconCol);
            };

            _searchBox = new TextBox
            {
                Location = new Point(26, 5),
                Size = new Size(246, 18),
                BorderStyle = BorderStyle.None,
                BackColor = WarmTheme.LightSearchBg,
                Font = new Font("Microsoft YaHei UI", 9F),
                ForeColor = WarmTheme.LightTextMuted,
                Text = SearchPlaceholder
            };
            _searchBox.Enter += (s, e) => {
                _searchFocused = true;
                bool isDark = (_data != null && _data.Settings != null && _data.Settings.ThemeType == "black");
                if (_searchBox.Text == SearchPlaceholder)
                {
                    _searchBox.Text = "";
                    _searchBox.ForeColor = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
                }
                _searchWrap.Invalidate();
            };
            _searchBox.Leave += (s, e) => {
                _searchFocused = false;
                bool isDark = (_data != null && _data.Settings != null && _data.Settings.ThemeType == "black");
                if (string.IsNullOrWhiteSpace(_searchBox.Text))
                {
                    _searchBox.Text = SearchPlaceholder;
                    _searchBox.ForeColor = isDark ? WarmTheme.DarkTextMuted : WarmTheme.LightTextMuted;
                }
                _searchWrap.Invalidate();
            };
            _searchBox.TextChanged += (s, e) => RefreshList();
            _searchWrap.Controls.Add(_searchBox);

            _topBar.Controls.AddRange(new Control[] { _picAvatar, _lblLogo, _btnSettings, _chkTopMost, _btnAddPhrase, _searchWrap });

            // 2. 模式切换栏（文本 / 视频）
            _modeBar = new ModeSwitchBar
            {
                Dock = DockStyle.Top,
                Height = 34
            };

            // 3. 分类导航条
            _categoryBar = new CustomCategoryBar
            {
                Dock = DockStyle.Top,
                Height = 40
            };
            _categoryBar.OnCategorySelected += (cat) => { ActiveCat = cat; RefreshList(); };
            _categoryBar.OnAddCategoryRequested += () => AddCategoryPrompt();
            _categoryBar.OnDeleteCurrentCategoryRequested += () => DeleteCurrentCategoryPrompt();
            _categoryBar.OnRenameCategoryRequested += (cat) => RenameCategoryPrompt(cat);
            _categoryBar.OnDeleteCategoryRequested += (cat) => DeleteCategory(cat);

            // 4. 工作区列表
            _phraseList = new MiniPhraseListView
            {
                Dock = DockStyle.Fill,
                BackColor = WarmTheme.LightPaperBg,
                EmptyAvatarImage = _picAvatar.AvatarImage
            };
            _phraseList.OnActionTriggered += (item, isSend) => SendToTarget(item, isSend);
            _phraseList.OnEditRequested += (item) => ShowAddOrEditDialog(item);

            // 5. 底部状态栏
            _statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                BackColor = WarmTheme.LightPaperBg,
                Padding = new Padding(12, 3, 12, 2)
            };
            _statusBar.Paint += (s, e) =>
            {
                bool isDark = (_data != null && _data.Settings != null && _data.Settings.ThemeType == "black");
                Color lineCol = isDark ? WarmTheme.DarkDivider : WarmTheme.LightDivider;
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
                Font = new Font("Microsoft YaHei UI", 8F),
                ForeColor = WarmTheme.LightTextSecondary
            };
            _statusBar.Controls.Add(_lblTarget);

            this.Controls.Add(_phraseList);
            this.Controls.Add(_categoryBar);
            this.Controls.Add(_modeBar);
            this.Controls.Add(_topBar);
            this.Controls.Add(_statusBar);

            // 排版：左右内边距 12px，动态定位确保设置图标永不被遮挡
            Action updateLayout = () =>
            {
                int w = this.ClientSize.Width;
                _btnAddPhrase.Location = new Point(w - _btnAddPhrase.Width - 10, 8);
                _chkTopMost.Location = new Point(_btnAddPhrase.Left - _chkTopMost.Width - 6, 11);
                _btnSettings.Location = new Point(_chkTopMost.Left - _btnSettings.Width - 6, 8);
                _btnSettings.BringToFront();
                _chkTopMost.BringToFront();
                _btnAddPhrase.BringToFront();
                _searchWrap.Width = w - 24;
                _searchBox.Width = Math.Max(40, _searchWrap.Width - 34);
                _phraseList.Invalidate();
            };

            _modeBar.OnModeChanged += (mode) =>
            {
                _currentMode = mode;
                _phraseList.CurrentMode = mode;
                if (mode == "video")
                {
                    _btnAddPhrase.Text = "+视频";
                    _btnAddPhrase.Width = 52;
                }
                else
                {
                    _btnAddPhrase.Text = "+词";
                    _btnAddPhrase.Width = 44;
                }
                updateLayout();
                _categoryBar.SetCategories(ActiveCategories(), ActiveCat);
                RefreshList();
            };

            this.Load += (s, e) => updateLayout();
            this.Resize += (s, e) => updateLayout();
            updateLayout();
        }

        private void OpenSettingsDialog()
        {
            bool isDark = (_data != null && _data.Settings != null && _data.Settings.ThemeType == "black");
            Color dlgBg = isDark ? WarmTheme.DarkPaperBg : WarmTheme.LightPaperBg;
            Color cardBg = isDark ? WarmTheme.DarkCardBg : WarmTheme.LightCardBg;
            Color cardBorder = isDark ? WarmTheme.DarkDivider : WarmTheme.LightBorder;
            Color txtPrimary = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
            Color txtSec = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary;

            using (Form setDlg = new Form())
            {
                setDlg.Text = "设置";
                setDlg.Size = new Size(420, 760);
                setDlg.StartPosition = FormStartPosition.CenterParent;
                setDlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                setDlg.MaximizeBox = false;
                setDlg.MinimizeBox = false;
                setDlg.BackColor = dlgBg;
                setDlg.ForeColor = txtPrimary;
                setDlg.Font = new Font("Microsoft YaHei UI", 9F);
                if (this.Icon != null) setDlg.Icon = this.Icon;

                Action<Panel> paintCard = (pnl) =>
                {
                    pnl.Paint += (s, e) =>
                    {
                        using (Pen p = new Pen(cardBorder, 1))
                        {
                            e.Graphics.DrawRectangle(p, 0, 0, pnl.Width - 1, pnl.Height - 1);
                        }
                    };
                };

                // 0. 主题配色风格卡片（暖橙纸感 / 樱粉花语）
                Panel pnlScheme = new Panel
                {
                    Location = new Point(16, 12),
                    Size = new Size(372, 82),
                    BackColor = cardBg
                };
                paintCard(pnlScheme);

                Label lblSchemeTitle = new Label
                {
                    Text = "主题配色",
                    Location = new Point(14, 10),
                    AutoSize = true,
                    Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                    ForeColor = txtPrimary
                };

                RadioButton rbSchemeOrange = new RadioButton { Text = "暖橙纸感", Location = new Point(16, 34), AutoSize = true, ForeColor = txtPrimary };
                Label lblOrangeDesc = new Label { Text = "淘宝橙，干练", Location = new Point(34, 56), AutoSize = true, Font = new Font("Microsoft YaHei UI", 8F), ForeColor = txtSec };

                RadioButton rbSchemePink = new RadioButton { Text = "樱粉花语", Location = new Point(188, 34), AutoSize = true, ForeColor = txtPrimary };
                Label lblPinkDesc = new Label { Text = "站点粉，柔和", Location = new Point(206, 56), AutoSize = true, Font = new Font("Microsoft YaHei UI", 8F), ForeColor = txtSec };

                if (_data.Settings.ColorScheme == "pink") rbSchemePink.Checked = true;
                else rbSchemeOrange.Checked = true;

                pnlScheme.Controls.AddRange(new Control[] { lblSchemeTitle, rbSchemeOrange, lblOrangeDesc, rbSchemePink, lblPinkDesc });

                // 1. 界面背景主题卡片
                Panel pnlBg = new Panel
                {
                    Location = new Point(16, 104),
                    Size = new Size(372, 158),
                    BackColor = cardBg
                };
                paintCard(pnlBg);

                Label lblBgTitle = new Label
                {
                    Text = "界面背景",
                    Location = new Point(14, 10),
                    AutoSize = true,
                    Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                    ForeColor = txtPrimary
                };

                RadioButton rbWhite = new RadioButton { Text = "经典白", Location = new Point(16, 36), AutoSize = true, ForeColor = txtPrimary };
                Label lblWhiteDesc = new Label { Text = "清爽明亮", Location = new Point(34, 59), AutoSize = true, Font = new Font("Microsoft YaHei UI", 8F), ForeColor = txtSec };

                RadioButton rbBlack = new RadioButton { Text = "深色护眼", Location = new Point(188, 36), AutoSize = true, ForeColor = txtPrimary };
                Label lblBlackDesc = new Label { Text = "晚上看不刺眼", Location = new Point(206, 59), AutoSize = true, Font = new Font("Microsoft YaHei UI", 8F), ForeColor = txtSec };

                RadioButton rbCustom = new RadioButton { Text = "自定义图片", Location = new Point(16, 88), AutoSize = true, ForeColor = txtPrimary };
                Label lblCustomDesc = new Label { Text = "用自己的图做背景", Location = new Point(34, 111), AutoSize = true, Font = new Font("Microsoft YaHei UI", 8F), ForeColor = txtSec };

                Panel picIconImg = new Panel { Location = new Point(120, 89), Size = new Size(16, 16), BackColor = Color.Transparent };
                picIconImg.Paint += (s, e) => GlassRenderer.DrawVectorImageIcon(e.Graphics, new Rectangle(0, 0, 16, 16), txtSec);

                string currentTheme = _data.Settings.ThemeType;
                if (currentTheme == "black") rbBlack.Checked = true;
                else if (currentTheme == "custom") rbCustom.Checked = true;
                else rbWhite.Checked = true;

                LiquidGlassButtonControl btnChooseBg = new LiquidGlassButtonControl
                {
                    Text = "选择图片...",
                    Location = new Point(188, 94),
                    Size = new Size(86, 28),
                    BaseNormal = cardBg,
                    BaseHover = isDark ? WarmTheme.DarkRowHover : WarmTheme.LightSearchBg,
                    BasePressed = cardBorder,
                    BorderColor = cardBorder,
                    TextColor = txtPrimary
                };
                LiquidGlassButtonControl btnClearBg = new LiquidGlassButtonControl
                {
                    Text = "一键删图",
                    Location = new Point(280, 94),
                    Size = new Size(76, 28),
                    BaseNormal = cardBg,
                    BaseHover = isDark ? Color.FromArgb(60, 28, 28) : Color.FromArgb(254, 226, 226),
                    BasePressed = cardBorder,
                    BorderColor = cardBorder,
                    TextColor = WarmTheme.DangerRed
                };
                ToolTip clearTip = new ToolTip();
                clearTip.SetToolTip(btnClearBg, "清除背景并一键删除本地保存的所有背景图片文件");

                string tempCustomBg = _data.Settings.CustomBgPath;

                WarmCheckBox chkCopyMedia = new WarmCheckBox
                {
                    Text = "复制图片和视频到软件目录",
                    Location = new Point(14, 142),
                    Size = new Size(260, 22),
                    Checked = _data.Settings.CopyMediaToLocal,
                    IsDarkTheme = isDark,
                    Font = new Font("Microsoft YaHei UI", 8.5F)
                };
                Label lblCopyDesc = new Label
                {
                    Text = "不勾选则直接读取电脑原位置，不占空间",
                    Location = new Point(32, 166),
                    AutoSize = true,
                    Font = new Font("Microsoft YaHei UI", 8F),
                    ForeColor = txtSec
                };
                Label lblRestoreTip = new Label
                {
                    Text = "提示：导入备份会让备份里面的图片和视频复制到软件目录里，会导致部分空间占用",
                    Location = new Point(14, 186),
                    MaximumSize = new Size(344, 0),
                    AutoSize = true,
                    Font = new Font("Microsoft YaHei UI", 8F),
                    ForeColor = txtSec
                };

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
                            rbCustom.Checked = true;
                            MessageBox.Show("背景图片已载入，点保存后生效。", "提示");
                        }
                    }
                };

                btnClearBg.Click += (s, e) =>
                {
                    if (_customBgImg != null)
                    {
                        _customBgImg.Dispose();
                        _customBgImg = null;
                    }
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
                    if (rbCustom.Checked) rbWhite.Checked = true;

                    MessageBox.Show(
                        deletedCount > 0 ? string.Format("已一键删除 {0} 张历史背景图片！", deletedCount) : "当前没有已保存的背景图片文件。",
                        "清理完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                pnlBg.Controls.AddRange(new Control[] { lblBgTitle, rbWhite, lblWhiteDesc, rbBlack, lblBlackDesc, rbCustom, lblCustomDesc, picIconImg, btnChooseBg, btnClearBg });

                // 2. 字体大小卡片（2x2 网格防止单选框被遮挡）
                Panel pnlFont = new Panel
                {
                    Location = new Point(16, 272),
                    Size = new Size(372, 92),
                    BackColor = cardBg
                };
                paintCard(pnlFont);

                Label lblFontTitle = new Label
                {
                    Text = "话术字号",
                    Location = new Point(14, 10),
                    AutoSize = true,
                    Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                    ForeColor = txtPrimary
                };

                RadioButton rbF0 = new RadioButton { Text = "标准 (9pt)", Location = new Point(16, 36), AutoSize = true, ForeColor = txtPrimary };
                RadioButton rbF1 = new RadioButton { Text = "中号 (11pt)", Location = new Point(185, 36), AutoSize = true, ForeColor = txtPrimary };
                RadioButton rbF2 = new RadioButton { Text = "大号 (13pt)", Location = new Point(16, 62), AutoSize = true, ForeColor = txtPrimary };
                RadioButton rbF3 = new RadioButton { Text = "特大 (15pt)", Location = new Point(185, 62), AutoSize = true, ForeColor = txtPrimary };

                int curLevel = _data.Settings.FontSizeLevel;
                if (curLevel == 1) rbF1.Checked = true;
                else if (curLevel == 2) rbF2.Checked = true;
                else if (curLevel == 3) rbF3.Checked = true;
                else rbF0.Checked = true;

                pnlFont.Controls.AddRange(new Control[] { lblFontTitle, rbF0, rbF1, rbF2, rbF3 });

                // 3. 备份与换机卡片
                Panel pnlBackup = new Panel
                {
                    Location = new Point(16, 374),
                    Size = new Size(372, 228),
                    BackColor = cardBg
                };
                paintCard(pnlBackup);

                Panel picIconBackup = new Panel { Location = new Point(14, 10), Size = new Size(16, 16), BackColor = Color.Transparent };
                picIconBackup.Paint += (s, e) => GlassRenderer.DrawVectorBackupIcon(e.Graphics, new Rectangle(0, 0, 16, 16), txtPrimary);

                Label lblBackupTitle = new Label
                {
                    Text = "备份与换机",
                    Location = new Point(34, 10),
                    AutoSize = true,
                    Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                    ForeColor = txtPrimary
                };
                Label lblBackupDesc = new Label
                {
                    Text = "话术、图片和视频打成一个包，换电脑一键恢复",
                    Location = new Point(14, 32),
                    AutoSize = true,
                    Font = new Font("Microsoft YaHei UI", 8F),
                    ForeColor = txtSec
                };

                LiquidGlassButtonControl btnExport = new LiquidGlassButtonControl
                {
                    Text = "导出备份包 (.zip)",
                    Location = new Point(14, 62),
                    Size = new Size(124, 32),
                    BaseNormal = cardBg,
                    BaseHover = isDark ? WarmTheme.DarkRowHover : WarmTheme.LightSearchBg,
                    BasePressed = cardBorder,
                    BorderColor = cardBorder,
                    TextColor = txtPrimary
                };
                btnExport.Click += (s, e) => ExportBackupData();

                LiquidGlassButtonControl btnImport = new LiquidGlassButtonControl
                {
                    Text = "导入备份包...",
                    Location = new Point(146, 62),
                    Size = new Size(110, 32),
                    BaseNormal = cardBg,
                    BaseHover = isDark ? WarmTheme.DarkRowHover : WarmTheme.LightSearchBg,
                    BasePressed = cardBorder,
                    BorderColor = cardBorder,
                    TextColor = txtPrimary
                };
                btnImport.Click += (s, e) => ImportBackupData();

                LiquidGlassButtonControl btnOpenDir = new LiquidGlassButtonControl
                {
                    Text = "数据目录",
                    Location = new Point(264, 62),
                    Size = new Size(92, 32),
                    BaseNormal = cardBg,
                    BaseHover = isDark ? WarmTheme.DarkRowHover : WarmTheme.LightSearchBg,
                    BasePressed = cardBorder,
                    BorderColor = cardBorder,
                    TextColor = txtSec
                };
                btnOpenDir.Click += (s, e) =>
                {
                    try { System.Diagnostics.Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory); }
                    catch { }
                };

                LiquidGlassButtonControl btnClearPhrases = new LiquidGlassButtonControl
                {
                    Text = "一键清理所有提示词",
                    Location = new Point(14, 102),
                    Size = new Size(166, 32),
                    BaseNormal = cardBg,
                    BaseHover = isDark ? Color.FromArgb(60, 28, 28) : Color.FromArgb(254, 226, 226),
                    BasePressed = cardBorder,
                    BorderColor = cardBorder,
                    TextColor = WarmTheme.DangerRed
                };
                btnClearPhrases.Click += (s, e) =>
                {
                    int textCount = _data.Phrases.Count(p => string.IsNullOrEmpty(p.ItemType) || p.ItemType != "video");
                    if (textCount == 0)
                    {
                        MessageBox.Show("当前没有任何文本提示词，无需清理。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DialogResult r1 = MessageBox.Show(
                        string.Format("确定要清理全部 {0} 条文本提示词吗？\n（分类名称与视频快捷方式将会保留，关联的附带图片将一并清理）", textCount),
                        "确认清理所有提示词", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (r1 != DialogResult.Yes) return;

                    DialogResult r2 = MessageBox.Show(
                        "请再次确认：清空后无法撤销，是否立即删除所有文本提示词？",
                        "二次确认", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (r2 != DialogResult.Yes) return;

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
                    RefreshList();
                    MessageBox.Show("已成功清空所有文本提示词！", "清理完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                LiquidGlassButtonControl btnClearVideos = new LiquidGlassButtonControl
                {
                    Text = "一键清理所有视频",
                    Location = new Point(190, 102),
                    Size = new Size(166, 32),
                    BaseNormal = cardBg,
                    BaseHover = isDark ? Color.FromArgb(60, 28, 28) : Color.FromArgb(254, 226, 226),
                    BasePressed = cardBorder,
                    BorderColor = cardBorder,
                    TextColor = WarmTheme.DangerRed
                };
                btnClearVideos.Click += (s, e) =>
                {
                    int videoCount = _data.Phrases.Count(p => p.ItemType == "video");
                    if (videoCount == 0)
                    {
                        MessageBox.Show("当前没有任何视频快捷方式，无需清理。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DialogResult r1 = MessageBox.Show(
                        string.Format("确定要清理全部 {0} 条视频快捷方式吗？\n（分类名称与文本提示词将会保留，缓存视频文件将一并删除）", videoCount),
                        "确认清理所有视频", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (r1 != DialogResult.Yes) return;

                    DialogResult r2 = MessageBox.Show(
                        "请再次确认：清空后无法撤销，是否立即删除所有视频快捷方式？",
                        "二次确认", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (r2 != DialogResult.Yes) return;

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
                    RefreshList();
                    MessageBox.Show("已成功清空所有视频快捷方式！", "清理完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                pnlBackup.Controls.AddRange(new Control[] { picIconBackup, lblBackupTitle, lblBackupDesc, btnExport, btnImport, btnOpenDir, btnClearPhrases, btnClearVideos, chkCopyMedia, lblCopyDesc, lblRestoreTip });

                // 4. 开源项目地址卡片
                string repoUrl = "https://github.com/l1011186781-commits/kefu-huahua";
                Panel pnlAbout = new Panel
                {
                    Location = new Point(16, 612),
                    Size = new Size(372, 54),
                    BackColor = cardBg
                };
                paintCard(pnlAbout);

                Label lblGitHubTitle = new Label
                {
                    Text = "开源项目地址（点击打开）",
                    Location = new Point(14, 8),
                    AutoSize = true,
                    Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold),
                    ForeColor = txtPrimary
                };
                LinkLabel lnkGitHub = new LinkLabel
                {
                    Text = repoUrl,
                    Location = new Point(14, 28),
                    AutoSize = true,
                    Font = new Font("Microsoft YaHei UI", 8.5F),
                    LinkColor = isDark ? WarmTheme.DarkPrimaryAccent : WarmTheme.Orange,
                    ActiveLinkColor = WarmTheme.OrangePressed,
                    VisitedLinkColor = isDark ? WarmTheme.DarkPrimaryAccent : WarmTheme.Orange,
                    LinkBehavior = LinkBehavior.HoverUnderline,
                    Cursor = Cursors.Hand
                };
                ToolTip ghTip = new ToolTip();
                ghTip.SetToolTip(lnkGitHub, "点击在浏览器中访问项目主页");
                lnkGitHub.LinkClicked += (s, e) =>
                {
                    try { System.Diagnostics.Process.Start(repoUrl); }
                    catch (Exception ex) { MessageBox.Show("无法打开浏览器: " + ex.Message, "提示"); }
                };
                pnlAbout.Controls.AddRange(new Control[] { lblGitHubTitle, lnkGitHub });

                LiquidGlassButtonControl btnSave = new LiquidGlassButtonControl
                {
                    Text = "保存",
                    Location = new Point(216, 678),
                    Size = new Size(88, 32),
                    Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                    BaseNormal = WarmTheme.Orange,
                    BaseHover = WarmTheme.OrangeHover,
                    BasePressed = WarmTheme.OrangePressed,
                    BorderColor = WarmTheme.Orange,
                    TextColor = Color.White
                };
                btnSave.Click += (s, e) => { setDlg.DialogResult = DialogResult.OK; setDlg.Close(); };

                LiquidGlassButtonControl btnCancel = new LiquidGlassButtonControl
                {
                    Text = "取消",
                    Location = new Point(312, 678),
                    Size = new Size(76, 32),
                    BaseNormal = cardBg,
                    BaseHover = isDark ? WarmTheme.DarkRowHover : WarmTheme.LightSearchBg,
                    BasePressed = cardBorder,
                    BorderColor = cardBorder,
                    TextColor = txtPrimary
                };
                btnCancel.Click += (s, e) => { setDlg.DialogResult = DialogResult.Cancel; setDlg.Close(); };

                setDlg.Load += (s, e) =>
                {
                    lblOrangeDesc.Top = rbSchemeOrange.Bottom + 2;
                    lblPinkDesc.Top = rbSchemePink.Bottom + 2;
                    lblWhiteDesc.Top = rbWhite.Bottom + 2;
                    lblBlackDesc.Top = rbBlack.Bottom + 2;
                    lblCustomDesc.Top = rbCustom.Bottom + 2;
                    lblCopyDesc.Top = chkCopyMedia.Bottom + 2;
                    lblRestoreTip.Top = lblCopyDesc.Bottom + 5;
                    pnlBackup.Height = Math.Max(228, lblRestoreTip.Bottom + 12);
                    pnlAbout.Top = pnlBackup.Bottom + 10;
                    btnSave.Top = pnlAbout.Bottom + 12;
                    btnCancel.Top = btnSave.Top;
                    setDlg.ClientSize = new Size(404, btnSave.Bottom + 14);
                    picIconImg.Left = rbCustom.Right + 4;
                    picIconImg.Top = rbCustom.Top + (rbCustom.Height - picIconImg.Height) / 2;
                };

                setDlg.Controls.AddRange(new Control[] { pnlScheme, pnlBg, pnlFont, pnlBackup, pnlAbout, btnSave, btnCancel });

                if (setDlg.ShowDialog(this) == DialogResult.OK)
                {
                    _data.Settings.ColorScheme = rbSchemePink.Checked ? "pink" : "orange";

                    if (rbBlack.Checked) _data.Settings.ThemeType = "black";
                    else if (rbCustom.Checked) _data.Settings.ThemeType = "custom";
                    else _data.Settings.ThemeType = "white";

                    _data.Settings.CustomBgPath = tempCustomBg;
                    _data.Settings.CopyMediaToLocal = chkCopyMedia.Checked;

                    if (rbF1.Checked) _data.Settings.FontSizeLevel = 1;
                    else if (rbF2.Checked) _data.Settings.FontSizeLevel = 2;
                    else if (rbF3.Checked) _data.Settings.FontSizeLevel = 3;
                    else _data.Settings.FontSizeLevel = 0;

                    SaveData();
                    ApplySettings();
                }
            }
        }

        private void ApplySettings()
        {
            WarmTheme.SetScheme(_data.Settings.ColorScheme);

            int fLevel = _data.Settings.FontSizeLevel;
            if (fLevel == 0) _phraseList.SetFontConfig(9.0F, 36);
            else if (fLevel == 1) _phraseList.SetFontConfig(11.0F, 42);
            else if (fLevel == 2) _phraseList.SetFontConfig(13.0F, 48);
            else _phraseList.SetFontConfig(15.0F, 56);

            string theme = _data.Settings.ThemeType;
            bool isDark = (theme == "black");

            if (_customBgImg != null) { _customBgImg.Dispose(); _customBgImg = null; }
            if (theme == "custom" && !string.IsNullOrEmpty(_data.Settings.CustomBgPath))
            {
                string fullImg = ResolveMediaPath(_data.Settings.CustomBgPath);
                if (File.Exists(fullImg))
                {
                    try { _customBgImg = Image.FromFile(fullImg); } catch { }
                }
            }

            this.BackColor = isDark ? WarmTheme.DarkPaperBg : WarmTheme.LightPaperBg;

            Color textCol = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
            Color secCol = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary;
            Color boxBg = isDark ? WarmTheme.DarkPaperBg : WarmTheme.LightSearchBg;

            _topBar.IsDarkTheme = isDark;
            _topBar.Invalidate();
            _picAvatar.IsDarkTheme = isDark;
            _picAvatar.Invalidate();
            _lblLogo.ForeColor = textCol;
            _chkTopMost.IsDarkTheme = isDark;
            _chkTopMost.Invalidate();
            _searchBox.BackColor = boxBg;
            if (_searchBox.Text == SearchPlaceholder)
                _searchBox.ForeColor = isDark ? WarmTheme.DarkTextMuted : WarmTheme.LightTextMuted;
            else
                _searchBox.ForeColor = textCol;
            _searchWrap.Invalidate();

            _btnSettings.IsDarkTheme = isDark;
            _btnSettings.BaseNormal = Color.Transparent;
            _btnSettings.BaseHover = isDark ? WarmTheme.DarkRowHover : WarmTheme.LightSearchBg;
            _btnSettings.BasePressed = isDark ? WarmTheme.DarkDivider : WarmTheme.LightDivider;
            _btnSettings.BorderColor = isDark ? WarmTheme.DarkDivider : WarmTheme.LightBorder;
            _btnSettings.TextColor = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightChipText;
            _btnSettings.Invalidate();

            _btnAddPhrase.IsDarkTheme = isDark;
            _btnAddPhrase.BaseNormal = WarmTheme.Orange;
            _btnAddPhrase.BaseHover = WarmTheme.OrangeHover;
            _btnAddPhrase.BasePressed = WarmTheme.OrangePressed;
            _btnAddPhrase.BorderColor = WarmTheme.Orange;
            _btnAddPhrase.Invalidate();

            _statusBar.BackColor = isDark ? WarmTheme.DarkPaperBg : WarmTheme.LightPaperBg;
            _statusBar.Invalidate();
            if (string.IsNullOrEmpty(_lastTargetTitle))
            {
                _lblTarget.ForeColor = secCol;
            }

            _modeBar.IsDarkTheme = isDark;
            _modeBar.Invalidate();

            _categoryBar.IsDarkTheme = isDark;
            _categoryBar.Invalidate();

            _phraseList.SetTheme(theme, _customBgImg);
        }

        private void RunWithBusyDialog(string message, Action work, Action onDone)
        {
            bool isDark = (_data != null && _data.Settings != null && _data.Settings.ThemeType == "black");
            using (Form busyDlg = new Form())
            {
                busyDlg.Text = "处理中";
                busyDlg.Size = new Size(340, 110);
                busyDlg.StartPosition = FormStartPosition.CenterParent;
                busyDlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                busyDlg.ControlBox = false;
                busyDlg.ShowInTaskbar = false;
                busyDlg.BackColor = isDark ? WarmTheme.DarkPaperBg : WarmTheme.LightPaperBg;
                busyDlg.Font = new Font("Microsoft YaHei UI", 9F);

                Label lbl = new Label
                {
                    Text = message,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary
                };
                busyDlg.Controls.Add(lbl);

                Exception err = null;
                busyDlg.Shown += (s, e) =>
                {
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
                    var confirm = MessageBox.Show(
                        "导入备份将恢复该备份包中的所有【文本/视频分类】、【话术】、【附带图片】以及【视频文件】。\n注意：导入备份会让备份里面的图片和视频复制到软件目录里，会导致部分空间占用。\n是否确认导入？",
                        "确认导入备份", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirm != DialogResult.Yes) return;

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
                                MessageBox.Show("无效的备份文件：未在该文件中检测到 phrases.json 数据！", "导入失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            LoadData();
                            ApplySettings();
                            _currentTextCat = "全部";
                            _currentVideoCat = "全部";
                            _categoryBar.SetCategories(ActiveCategories(), ActiveCat);
                            RefreshList();

                            int textCnt = _data.Phrases.Count(p => string.IsNullOrEmpty(p.ItemType) || p.ItemType != "video");
                            int vidCnt = _data.Phrases.Count(p => p.ItemType == "video");
                            MessageBox.Show(
                                string.Format("恢复完成！\n已成功复原 {0} 条文本话术、{1} 条视频快捷方式。", textCnt, vidCnt),
                                "恢复成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("恢复备份时发生异常: " + ex.Message, "恢复失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            bool isDark = (_data != null && _data.Settings != null && _data.Settings.ThemeType == "black");

            using (Form eggForm = new Form())
            {
                eggForm.Text = "客服花花 - 原画";
                eggForm.Size = new Size(360, 420);
                eggForm.StartPosition = FormStartPosition.CenterParent;
                eggForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                eggForm.MaximizeBox = false;
                eggForm.MinimizeBox = false;
                eggForm.BackColor = isDark ? WarmTheme.DarkPaperBg : WarmTheme.LightPaperBg;
                if (this.Icon != null) eggForm.Icon = this.Icon;

                PictureBox pic = new PictureBox
                {
                    Dock = DockStyle.Top,
                    Height = 310,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = eggImg,
                    Padding = new Padding(10),
                    BackColor = Color.Transparent
                };

                Label lblText = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = "「正在努力帮主人回复客户呢！」\n点击任意处或按 ESC 返回",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular),
                    ForeColor = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary
                };

                eggForm.Controls.Add(lblText);
                eggForm.Controls.Add(pic);

                pic.Click += (s, e) => eggForm.Close();
                lblText.Click += (s, e) => eggForm.Close();
                eggForm.Click += (s, e) => eggForm.Close();

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
                        bool isDark = (_data != null && _data.Settings != null && _data.Settings.ThemeType == "black");
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
                // 大数据量时启用多核并行筛选（保持原有顺序）
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
                MessageBox.Show("尚未捕获聊天窗口！\n请先鼠标点一下千牛或微信输入框，然后再点发送。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool isVideo = (item.ItemType == "video");
            string fullVideoPath = isVideo ? ResolveMediaPath(item.VideoPath) : "";
            bool videoFileValid = false;
            if (isVideo && !string.IsNullOrEmpty(fullVideoPath))
            {
                videoFileValid = File.Exists(fullVideoPath);
                if (!videoFileValid)
                {
                    MessageBox.Show("找不到视频文件：\n" + fullVideoPath, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            bool hasImage = !isVideo && !string.IsNullOrEmpty(item.ImagePath);
            string fullImgPath = hasImage ? ResolveMediaPath(item.ImagePath) : "";
            bool imgFileValid = hasImage && File.Exists(fullImgPath);
            if (hasImage && !imgFileValid)
            {
                MessageBox.Show("找不到图片文件：\n" + fullImgPath, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("该分类已存在！", "提示");
                    return;
                }
                list.Add(name);
                ActiveCat = name;
                SaveData();
                _categoryBar.SetCategories(list, ActiveCat);
                RefreshList();
            }
        }

        private void DeleteCurrentCategoryPrompt()
        {
            if (ActiveCat == "全部")
            {
                MessageBox.Show("【全部】为系统默认聚合分类，不可删除哦！\n请先点击想要删除的具体分类，然后再点击 [-] 删除。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                _categoryBar.SetCategories(list, ActiveCat);
                RefreshList();
            }
        }

        private void DeleteCategory(string cat)
        {
            if (MessageBox.Show("确定删除分类【" + cat + "】吗？\n该分类下的内容将自动归入【未分类】。", "确认删除分类", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
                _categoryBar.SetCategories(list, ActiveCat);
                RefreshList();
            }
        }

        private void ShowAddOrEditDialog(PhraseItem editItem)
        {
            bool isEditing = (editItem != null);
            bool isVideoMode = isEditing ? (editItem.ItemType == "video") : (_currentMode == "video");
            bool isDark = (_data != null && _data.Settings != null && _data.Settings.ThemeType == "black");
            Color dlgBg = isDark ? WarmTheme.DarkPaperBg : WarmTheme.LightPaperBg;
            Color cardBg = isDark ? WarmTheme.DarkCardBg : WarmTheme.LightCardBg;
            Color bdrCol = isDark ? WarmTheme.DarkDivider : WarmTheme.LightBorder;
            Color txtCol = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;
            Color secCol = isDark ? WarmTheme.DarkTextSecondary : WarmTheme.LightTextSecondary;

            using (Form dlg = new Form())
            {
                if (isVideoMode)
                    dlg.Text = isEditing ? "修改视频快捷方式" : "添加视频快捷方式";
                else
                    dlg.Text = isEditing ? "修改话术" : "添加话术";

                dlg.Size = new Size(424, 440);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = dlgBg;
                dlg.ForeColor = txtCol;
                dlg.Font = new Font("Microsoft YaHei UI", 9F);
                dlg.AllowDrop = true;
                if (this.Icon != null) dlg.Icon = this.Icon;

                Label lblCat = new Label { Text = "分类：", Location = new Point(16, 18), Padding = new Padding(0), ForeColor = txtCol, AutoSize = true };
                ComboBox cmbCat = new ComboBox { Location = new Point(108, 15), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = cardBg, ForeColor = txtCol };
                
                var dialogCats = isVideoMode ? _data.VideoCategories : _data.Categories;
                if (dialogCats.Count == 0)
                {
                    cmbCat.Items.Add("未分类");
                }
                else
                {
                    foreach (var c in dialogCats) cmbCat.Items.Add(c);
                }

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

                Label lblTitle = new Label { Text = isVideoMode ? "视频名称：" : "提示词：", Location = new Point(16, 55), Padding = new Padding(0), ForeColor = txtCol, AutoSize = true };
                TextBox txtTitle = new TextBox { Location = new Point(108, 52), Width = 280, BackColor = cardBg, ForeColor = txtCol, Text = isEditing ? editItem.Title : "" };

                Label lblContent = new Label { Text = isVideoMode ? "附带文字：" : "正文：", Location = new Point(16, 92), Padding = new Padding(0), ForeColor = txtCol, AutoSize = true };
                TextBox txtContent = new TextBox
                {
                    Location = new Point(108, 89),
                    Width = 280,
                    Height = 95,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    BackColor = cardBg,
                    ForeColor = txtCol,
                    Text = isEditing ? editItem.Content : ""
                };

                Label lblImg = new Label { Text = isVideoMode ? "视频文件：" : "图片：", Location = new Point(16, 198), Padding = new Padding(0), ForeColor = txtCol, AutoSize = true };
                
                Panel pnlDropBox = new Panel
                {
                    Location = new Point(108, 195),
                    Size = new Size(84, 84),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = cardBg,
                    AllowDrop = true
                };

                PictureBox picPreview = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent,
                    AllowDrop = true,
                    Visible = false
                };
                pnlDropBox.Controls.Add(picPreview);

                Label lblDropHint = new Label
                {
                    Text = isVideoMode ? "将视频文件\n拖入此处" : "直接将图片\n拖入此处",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft YaHei UI", 8F),
                    ForeColor = Color.FromArgb(140, 145, 155),
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
                        lblDropHint.Text = "将视频文件\n拖入此处";
                        lblDropHint.ForeColor = Color.FromArgb(140, 145, 155);
                        return;
                    }

                    if (!string.IsNullOrEmpty(currentSelectedImgPath))
                    {
                        string fullP = ResolveMediaPath(currentSelectedImgPath);
                        if (File.Exists(fullP))
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
                        string ext = Path.GetExtension(file).ToLower();
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
                                    MessageBox.Show("导入视频失败: " + ex.Message, "提示");
                                }
                            }
                            else
                            {
                                MessageBox.Show("请拖入视频文件（支持 mp4 / mov / avi / mkv / wmv / flv / webm）。", "提示");
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
                                MessageBox.Show("导入拖入图片失败: " + ex.Message, "提示");
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

                Button btnUploadImg = new Button
                {
                    Text = isVideoMode ? "选择视频" : "本地选择",
                    Location = new Point(202, 195),
                    Size = new Size(80, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = cardBg,
                    ForeColor = txtCol
                };
                btnUploadImg.FlatAppearance.BorderColor = bdrCol;

                Button btnPasteImg = new Button
                {
                    Text = "  粘贴图片",
                    Location = new Point(290, 195),
                    Size = new Size(92, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = cardBg,
                    ForeColor = txtCol,
                    TextAlign = ContentAlignment.MiddleRight,
                    Padding = new Padding(0, 0, 6, 0),
                    Visible = !isVideoMode
                };
                btnPasteImg.FlatAppearance.BorderColor = bdrCol;
                btnPasteImg.Paint += (s, e) =>
                {
                    GlassRenderer.DrawVectorClipboardIcon(e.Graphics, new Rectangle(6, 6, 16, 16), secCol);
                };

                Button btnClearImg = new Button
                {
                    Text = isVideoMode ? "清除视频" : "清除图片",
                    Location = new Point(202, 232),
                    Size = new Size(80, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = cardBg,
                    ForeColor = WarmTheme.DangerRed
                };
                btnClearImg.FlatAppearance.BorderColor = bdrCol;

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
                                catch (Exception ex)
                                {
                                    MessageBox.Show("加载视频失败: " + ex.Message, "提示");
                                }
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
                                catch (Exception ex)
                                {
                                    MessageBox.Show("加载图片失败: " + ex.Message, "提示");
                                }
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
                                    string ext = Path.GetExtension(files[0]).ToLower();
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
                        MessageBox.Show("未能在剪贴板中检测到图片！\n您也可以直接将图片拖进左侧方框中。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };

                btnClearImg.Click += (s, e) =>
                {
                    if (isVideoMode) currentSelectedVideoPath = "";
                    else currentSelectedImgPath = "";
                    updatePreview();
                };

                Button btnSave = new Button
                {
                    Text = isVideoMode ? "保存视频" : "保存话术",
                    Location = new Point(226, 345),
                    Size = new Size(82, 32),
                    BackColor = WarmTheme.Orange,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.OK
                };
                btnSave.FlatAppearance.BorderSize = 0;

                Button btnCancel = new Button
                {
                    Text = "取消",
                    Location = new Point(316, 345),
                    Size = new Size(72, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = cardBg,
                    ForeColor = txtCol,
                    DialogResult = DialogResult.Cancel
                };
                btnCancel.FlatAppearance.BorderColor = bdrCol;

                dlg.Load += (s, e) =>
                {
                    int maxR = Math.Max(Math.Max(lblCat.Right, lblTitle.Right), Math.Max(lblContent.Right, lblImg.Right));
                    int inputX = Math.Max(108, maxR + 8);
                    int rightEdge = dlg.ClientSize.Width - 18;
                    int fieldW = Math.Max(180, rightEdge - inputX);

                    cmbCat.Left = inputX; cmbCat.Width = fieldW;
                    txtTitle.Left = inputX; txtTitle.Width = fieldW;
                    txtContent.Left = inputX; txtContent.Width = fieldW;
                    pnlDropBox.Left = inputX;
                    btnUploadImg.Left = pnlDropBox.Right + 10;
                    btnPasteImg.Left = btnUploadImg.Right + 8;
                    btnClearImg.Left = pnlDropBox.Right + 10;
                    btnCancel.Left = rightEdge - btnCancel.Width;
                    btnSave.Left = btnCancel.Left - btnSave.Width - 8;
                };

                if (isEditing)
                {
                    Button btnDel = new Button
                    {
                        Text = "删除",
                        Location = new Point(16, 345),
                        Size = new Size(60, 32),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = cardBg,
                        ForeColor = WarmTheme.DangerRed
                    };
                    btnDel.FlatAppearance.BorderColor = bdrCol;
                    btnDel.Click += (s, e) =>
                    {
                        if (MessageBox.Show("确定删除此话术？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
                    btnSave, btnCancel
                });

                var res = dlg.ShowDialog(this);
                if (res == DialogResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(txtTitle.Text))
                    {
                        MessageBox.Show(isVideoMode ? "视频名称不能为空！" : "提示词不能为空！", "提示");
                        return;
                    }
                    if (isVideoMode)
                    {
                        if (string.IsNullOrEmpty(currentSelectedVideoPath))
                        {
                            MessageBox.Show("请先选择或拖入一个视频文件！", "提示");
                            return;
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(txtContent.Text) && string.IsNullOrEmpty(currentSelectedImgPath))
                    {
                        MessageBox.Show("话术正文和附带图片至少需要填写一项！", "提示");
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
                    RefreshList();
                }
                else if (res == DialogResult.Abort)
                {
                    RefreshList();
                }
            }
        }

        private string PromptInput(string title, string promptText, string defaultValue)
        {
            bool isDark = (_data != null && _data.Settings != null && _data.Settings.ThemeType == "black");
            Color dlgBg = isDark ? WarmTheme.DarkPaperBg : WarmTheme.LightPaperBg;
            Color cardBg = isDark ? WarmTheme.DarkCardBg : WarmTheme.LightCardBg;
            Color bdrCol = isDark ? WarmTheme.DarkDivider : WarmTheme.LightBorder;
            Color txtCol = isDark ? WarmTheme.DarkTextPrimary : WarmTheme.LightTextPrimary;

            using (Form form = new Form())
            {
                form.Text = title;
                form.Size = new Size(320, 150);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.BackColor = dlgBg;
                form.ForeColor = txtCol;
                if (this.Icon != null) form.Icon = this.Icon;

                Label label = new Label { Left = 20, Top = 16, Text = promptText, ForeColor = txtCol, AutoSize = true };
                TextBox textBox = new TextBox { Left = 20, Top = 40, Width = 260, Text = defaultValue, BackColor = cardBg, ForeColor = txtCol };
                Button btnOk = new Button { Text = "确定", Left = 130, Width = 70, Top = 72, DialogResult = DialogResult.OK, BackColor = WarmTheme.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                btnOk.FlatAppearance.BorderSize = 0;
                Button btnCancel = new Button { Text = "取消", Left = 210, Width = 70, Top = 72, DialogResult = DialogResult.Cancel, BackColor = cardBg, ForeColor = txtCol, FlatStyle = FlatStyle.Flat };
                btnCancel.FlatAppearance.BorderColor = bdrCol;

                form.Controls.AddRange(new Control[] { label, textBox, btnOk, btnCancel });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

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