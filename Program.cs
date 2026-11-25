using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace HuahuaHelper
{
    public class PhraseItem
    {
        public string Id { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImagePath { get; set; }

        public PhraseItem()
        {
            Id = Guid.NewGuid().ToString("N");
            ImagePath = "";
        }

        public PhraseItem(string cat, string title, string content, string imgPath)
        {
            Id = Guid.NewGuid().ToString("N");
            Category = cat;
            Title = title;
            Content = content;
            ImagePath = imgPath ?? "";
        }
    }

    public class AppSettings
    {
        public int FontSizeLevel { get; set; } // 0:标准(9pt), 1:中号(11pt), 2:大号(13pt), 3:特大(15pt)
        public string ThemeType { get; set; }   // "white", "black", "custom"
        public string CustomBgPath { get; set; }
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public int WindowX { get; set; }
        public int WindowY { get; set; }
        public bool HasWindowPos { get; set; }

        public AppSettings()
        {
            FontSizeLevel = 0;
            ThemeType = "white";
            CustomBgPath = "";
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
        public List<string> Categories { get; set; }
        public List<PhraseItem> Phrases { get; set; }

        public AppData()
        {
            Settings = new AppSettings();
            Categories = new List<string>();
            Phrases = new List<PhraseItem>();
        }
    }

    /// <summary>
    /// iOS 液体玻璃质感绘制核心工具类 (Liquid Glassmorphism Engine)
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

            int d = radius * 2;
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
        /// 绘制具有光泽折射与通透高光的 iOS 液体玻璃按钮
        /// </summary>
        public static void DrawLiquidGlassButton(Graphics g, Rectangle rect, Color baseTop, Color baseBottom, Color borderCol, bool isHover, int radius = 6)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. 微弱柔和阴影
            Rectangle shadowRect = new Rectangle(rect.X, rect.Y + 1, rect.Width, rect.Height);
            using (GraphicsPath sPath = CreateRoundedPath(shadowRect, radius))
            using (Brush sBrush = new SolidBrush(Color.FromArgb(isHover ? 35 : 20, 0, 0, 0)))
            {
                g.FillPath(sBrush, sPath);
            }

            using (GraphicsPath path = CreateRoundedPath(rect, radius))
            {
                // 2. 液体玻璃基础半透明渐变
                Color top = isHover ? Color.FromArgb(Math.Min(255, baseTop.A + 25), baseTop) : baseTop;
                Color bottom = isHover ? Color.FromArgb(Math.Min(255, baseBottom.A + 25), baseBottom) : baseBottom;

                using (LinearGradientBrush lgb = new LinearGradientBrush(rect, top, bottom, LinearGradientMode.Vertical))
                {
                    g.FillPath(lgb, path);
                }

                // 3. 顶部微弧高光反射层 (Liquid Gloss Highlight)
                Rectangle glossRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 2);
                if (glossRect.Height > 0)
                {
                    using (GraphicsPath gPath = CreateRoundedPath(glossRect, radius))
                    using (LinearGradientBrush glossLgb = new LinearGradientBrush(glossRect,
                        Color.FromArgb(isHover ? 110 : 80, 255, 255, 255),
                        Color.FromArgb(10, 255, 255, 255), LinearGradientMode.Vertical))
                    {
                        g.SetClip(path);
                        g.FillRectangle(glossLgb, glossRect);
                        g.ResetClip();
                    }
                }

                // 4. 1px 晶莹外边缘与内高光折射
                using (Pen pen = new Pen(borderCol, 1))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        /// <summary>
        /// 纯矢量自绘的高清设置齿轮图标（永不模糊、永不错位、抗锯齿锐利）
        /// </summary>
        public static void DrawVectorGearIcon(Graphics g, Rectangle rect, Color gearColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            float outerR = Math.Min(rect.Width, rect.Height) * 0.38f;
            float innerR = outerR * 0.72f;
            float holeR = outerR * 0.35f;

            using (GraphicsPath gearPath = new GraphicsPath())
            {
                int teeth = 6;
                double step = Math.PI * 2 / teeth;
                double toothHalf = step * 0.22;

                List<PointF> pts = new List<PointF>();
                for (int i = 0; i < teeth; i++)
                {
                    double angle = i * step;

                    // 齿槽起点
                    double a1 = angle - toothHalf * 1.5;
                    pts.Add(new PointF(cx + (float)(Math.Cos(a1) * innerR), cy + (float)(Math.Sin(a1) * innerR)));

                    // 齿顶起点
                    double a2 = angle - toothHalf * 0.7;
                    pts.Add(new PointF(cx + (float)(Math.Cos(a2) * outerR), cy + (float)(Math.Sin(a2) * outerR)));

                    // 齿顶终点
                    double a3 = angle + toothHalf * 0.7;
                    pts.Add(new PointF(cx + (float)(Math.Cos(a3) * outerR), cy + (float)(Math.Sin(a3) * outerR)));

                    // 齿槽终点
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
    }

    /// <summary>
    /// 自定义纯自绘分类导航栏（iOS 液体玻璃胶囊标签）
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
        private Color _primaryOrange = Color.FromArgb(255, 80, 0);

        public CustomCategoryBar()
        {
            Categories = new List<string>();
            ActiveCategory = "全部";
            IsDarkTheme = false;
            this.Height = 36;
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
            ClampScroll();
            this.Invalidate();
        }

        private int GetTotalWidth(Graphics g, Font font)
        {
            int total = 10;
            for (int i = 0; i < Categories.Count; i++)
            {
                SizeF size = g.MeasureString(Categories[i], font);
                total += (int)size.Width + 18;
            }
            total += 60;
            return total;
        }

        private void ClampScroll()
        {
            using (Graphics g = this.CreateGraphics())
            using (Font f = new Font("Microsoft YaHei UI", 8.5F))
            {
                int total = GetTotalWidth(g, f);
                int maxScroll = Math.Max(0, total - this.ClientSize.Width);
                if (_scrollX < -maxScroll) _scrollX = -maxScroll;
                if (_scrollX > 0) _scrollX = 0;
            }
        }

        private void CustomCategoryBar_MouseMove(object sender, MouseEventArgs e)
        {
            int prevIdx = _hoverIndex;
            bool prevAdd = _hoverAddBtn;
            bool prevDel = _hoverDelBtn;
            _hoverIndex = -1;
            _hoverAddBtn = false;
            _hoverDelBtn = false;

            using (Graphics g = this.CreateGraphics())
            using (Font font = new Font("Microsoft YaHei UI", 8.5F))
            {
                int curX = 8 + _scrollX;
                for (int i = 0; i < Categories.Count; i++)
                {
                    SizeF size = g.MeasureString(Categories[i], font);
                    int itemW = (int)size.Width + 16;
                    Rectangle rect = new Rectangle(curX, 5, itemW, 26);
                    if (rect.Contains(e.Location))
                    {
                        _hoverIndex = i;
                        break;
                    }
                    curX += itemW + 5;
                }

                Rectangle addRect = new Rectangle(curX, 5, 24, 25);
                Rectangle delRect = new Rectangle(curX + 28, 5, 24, 25);

                if (addRect.Contains(e.Location)) _hoverAddBtn = true;
                else if (delRect.Contains(e.Location)) _hoverDelBtn = true;
            }

            if (prevIdx != _hoverIndex || prevAdd != _hoverAddBtn || prevDel != _hoverDelBtn)
            {
                this.Invalidate();
            }
        }

        private void CustomCategoryBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (_hoverAddBtn && e.Button == MouseButtons.Left)
            {
                if (OnAddCategoryRequested != null) OnAddCategoryRequested();
                return;
            }

            if (_hoverDelBtn && e.Button == MouseButtons.Left)
            {
                if (OnDeleteCurrentCategoryRequested != null) OnDeleteCurrentCategoryRequested();
                return;
            }

            if (_hoverIndex >= 0 && _hoverIndex < Categories.Count)
            {
                string cat = Categories[_hoverIndex];
                if (e.Button == MouseButtons.Left)
                {
                    ActiveCategory = cat;
                    this.Invalidate();
                    if (OnCategorySelected != null) OnCategorySelected(cat);
                }
                else if (e.Button == MouseButtons.Right && cat != "全部")
                {
                    ContextMenuStrip cms = new ContextMenuStrip();
                    ToolStripMenuItem itemRename = new ToolStripMenuItem("✏️ 修改分类名称");
                    itemRename.Click += (s, ev) => { if (OnRenameCategoryRequested != null) OnRenameCategoryRequested(cat); };
                    ToolStripMenuItem itemDel = new ToolStripMenuItem("🗑️ 删除此分类");
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

            Color barBg = IsDarkTheme ? Color.FromArgb(32, 34, 38) : Color.White;
            Color bottomLine = IsDarkTheme ? Color.FromArgb(48, 52, 60) : Color.FromArgb(235, 238, 243);

            g.Clear(barBg);
            using (Pen linePen = new Pen(bottomLine, 1))
            {
                g.DrawLine(linePen, 0, this.Height - 1, this.Width, this.Height - 1);
            }

            using (Font font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Regular))
            using (Font activeFont = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold))
            {
                int curX = 8 + _scrollX;
                for (int i = 0; i < Categories.Count; i++)
                {
                    string cat = Categories[i];
                    bool isActive = (cat == ActiveCategory);
                    bool isHover = (i == _hoverIndex);

                    Font drawFont = isActive ? activeFont : font;
                    SizeF size = g.MeasureString(cat, drawFont);
                    int itemW = (int)size.Width + 16;
                    Rectangle rect = new Rectangle(curX, 5, itemW, 25);

                    if (isActive)
                    {
                        // 选中的活力橙液体玻璃胶囊
                        GlassRenderer.DrawLiquidGlassButton(g, rect,
                            Color.FromArgb(255, 120, 30), Color.FromArgb(255, 75, 0),
                            Color.FromArgb(255, 145, 60), isHover, 12);
                        TextRenderer.DrawText(g, cat, activeFont, rect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    else
                    {
                        // 通透冰晶磨砂胶囊
                        Color top = IsDarkTheme ? Color.FromArgb(50, 54, 62) : Color.FromArgb(252, 253, 255);
                        Color btm = IsDarkTheme ? Color.FromArgb(40, 44, 52) : Color.FromArgb(242, 245, 250);
                        Color bdr = IsDarkTheme ? Color.FromArgb(65, 70, 80) : Color.FromArgb(220, 225, 235);
                        Color txt = IsDarkTheme ? Color.FromArgb(205, 210, 220) : Color.FromArgb(70, 75, 85);

                        GlassRenderer.DrawLiquidGlassButton(g, rect, top, btm, bdr, isHover, 12);
                        TextRenderer.DrawText(g, cat, font, rect, txt, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }

                    curX += itemW + 5;
                }

                // [+] 增加分类液体玻璃按钮
                Rectangle addRect = new Rectangle(curX, 5, 24, 25);
                GlassRenderer.DrawLiquidGlassButton(g, addRect,
                    Color.FromArgb(245, 248, 255), Color.FromArgb(235, 242, 252),
                    Color.FromArgb(200, 220, 245), _hoverAddBtn, 6);
                TextRenderer.DrawText(g, "+", activeFont, addRect, Color.FromArgb(50, 130, 245), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                // [-] 删除分类液体玻璃按钮
                Rectangle delRect = new Rectangle(curX + 28, 5, 24, 25);
                GlassRenderer.DrawLiquidGlassButton(g, delRect,
                    Color.FromArgb(255, 245, 245), Color.FromArgb(252, 235, 235),
                    Color.FromArgb(245, 205, 205), _hoverDelBtn, 6);
                TextRenderer.DrawText(g, "-", activeFont, delRect, Color.FromArgb(230, 50, 50), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }

    /// <summary>
    /// 支持 iOS 液体玻璃操作按键的工作区列表
    /// </summary>
    public class MiniPhraseListView : Control
    {
        public List<PhraseItem> Items { get; set; }
        public int ItemHeight { get; set; }
        public float CustomFontSize { get; set; }
        public string ThemeType { get; set; }
        public Image CustomBgImage { get; set; }

        private int _hoverIndex = -1;
        private int _hoverBtnType = 0; // 0: 无, 1: 编辑, 2: 填入, 3: 发送
        private VScrollBar _vScrollBar;
        private ToolTip _tip = new ToolTip();
        private int _lastTipIndex = -1;

        public event Action<PhraseItem, bool> OnActionTriggered;
        public event Action<PhraseItem> OnEditRequested;

        public MiniPhraseListView()
        {
            Items = new List<PhraseItem>();
            ItemHeight = 36;
            CustomFontSize = 9.0F;
            ThemeType = "white";

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);

            _vScrollBar = new VScrollBar { Dock = DockStyle.Right, Visible = false, Width = 12 };
            _vScrollBar.Scroll += (s, e) => { this.Invalidate(); };
            this.Controls.Add(_vScrollBar);

            this.MouseWheel += (s, e) =>
            {
                if (_vScrollBar.Visible)
                {
                    int newVal = _vScrollBar.Value - (e.Delta / 120) * (ItemHeight);
                    if (newVal < _vScrollBar.Minimum) newVal = _vScrollBar.Minimum;
                    if (newVal > _vScrollBar.Maximum - _vScrollBar.LargeChange + 1)
                        newVal = Math.Max(0, _vScrollBar.Maximum - _vScrollBar.LargeChange + 1);
                    _vScrollBar.Value = newVal;
                    this.Invalidate();
                }
            };

            this.MouseMove += MiniPhraseListView_MouseMove;
            this.MouseLeave += (s, e) => { _hoverIndex = -1; _hoverBtnType = 0; _lastTipIndex = -1; _tip.Hide(this); this.Invalidate(); };
            this.MouseDown += MiniPhraseListView_MouseDown;
            this.DoubleClick += MiniPhraseListView_DoubleClick;
        }

        public void SetTheme(string theme, Image bgImg)
        {
            ThemeType = theme ?? "white";
            CustomBgImage = bgImg;
            this.Invalidate();
        }

        public void SetFontConfig(float fontSize, int itemHeight)
        {
            CustomFontSize = fontSize;
            ItemHeight = itemHeight;
            UpdateScroll();
            this.Invalidate();
        }

        public void UpdateData(List<PhraseItem> list)
        {
            Items = list ?? new List<PhraseItem>();
            UpdateScroll();
            this.Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScroll();
        }

        private void UpdateScroll()
        {
            int total = Items.Count * ItemHeight;
            if (total > this.ClientSize.Height)
            {
                _vScrollBar.Visible = true;
                _vScrollBar.Minimum = 0;
                _vScrollBar.Maximum = total;
                _vScrollBar.LargeChange = this.ClientSize.Height;
                _vScrollBar.SmallChange = ItemHeight;
            }
            else
            {
                _vScrollBar.Visible = false;
                _vScrollBar.Value = 0;
            }
        }

        private int GetScrollOffset()
        {
            return _vScrollBar.Visible ? _vScrollBar.Value : 0;
        }

        private void MiniPhraseListView_MouseMove(object sender, MouseEventArgs e)
        {
            int offset = GetScrollOffset();
            int newIdx = (e.Y + offset) / ItemHeight;

            if (newIdx >= 0 && newIdx < Items.Count)
            {
                _hoverIndex = newIdx;
                int rightMargin = _vScrollBar.Visible ? 14 : 2;
                int usableW = this.ClientSize.Width - rightMargin;
                int itemY = (newIdx * ItemHeight) - offset;

                int sendW = 34;
                int fillW = 28;
                int editW = 24;
                int btnH = Math.Min(24, ItemHeight - 10);
                int btnY = itemY + (ItemHeight - btnH) / 2;

                int sendLeft = usableW - sendW - 4;
                int fillLeft = sendLeft - fillW - 4;
                int editLeft = fillLeft - editW - 4;

                Rectangle rSend = new Rectangle(sendLeft, btnY, sendW, btnH);
                Rectangle rFill = new Rectangle(fillLeft, btnY, fillW, btnH);
                Rectangle rEdit = new Rectangle(editLeft, btnY, editW, btnH);

                int prevType = _hoverBtnType;
                if (rSend.Contains(e.Location)) _hoverBtnType = 3;
                else if (rFill.Contains(e.Location)) _hoverBtnType = 2;
                else if (rEdit.Contains(e.Location)) _hoverBtnType = 1;
                else _hoverBtnType = 0;

                if (_lastTipIndex != newIdx)
                {
                    _lastTipIndex = newIdx;
                    var item = Items[newIdx];
                    string imgTip = string.IsNullOrEmpty(item.ImagePath) ? "" : "\n🖼️ 附带图片: 已包含图片（先发字后发图）";
                    _tip.SetToolTip(this, string.Format("【{0}】{1}\n{2}\n\n💡 提示：双击该行或点击【发】直接发出", item.Title, imgTip, item.Content));
                }

                if (prevType != _hoverBtnType) this.Invalidate();
            }
            else
            {
                _hoverIndex = -1;
                _hoverBtnType = 0;
                _lastTipIndex = -1;
                _tip.Hide(this);
                this.Invalidate();
            }
        }

        private void MiniPhraseListView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (_hoverIndex >= 0 && _hoverIndex < Items.Count)
            {
                var item = Items[_hoverIndex];
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
            }
        }

        private void MiniPhraseListView_DoubleClick(object sender, EventArgs e)
        {
            if (_hoverIndex >= 0 && _hoverIndex < Items.Count)
            {
                var item = Items[_hoverIndex];
                if (OnActionTriggered != null) OnActionTriggered(item, true);
            }
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
                g.Clear(Color.FromArgb(24, 25, 28));
            }
            else if (ThemeType == "custom" && CustomBgImage != null)
            {
                g.DrawImage(CustomBgImage, new Rectangle(0, 0, this.Width, this.Height));
                using (Brush mask = new SolidBrush(Color.FromArgb(160, 255, 255, 255)))
                {
                    g.FillRectangle(mask, this.ClientRectangle);
                }
            }
            else
            {
                g.Clear(Color.White);
            }

            int offset = GetScrollOffset();
            int start = Math.Max(0, offset / ItemHeight);
            int count = (this.ClientSize.Height / ItemHeight) + 2;
            int end = Math.Min(Items.Count - 1, start + count);

            int rightMargin = _vScrollBar.Visible ? 14 : 2;
            int usableW = this.ClientSize.Width - rightMargin;

            Color textPrimary = isDark ? Color.FromArgb(235, 238, 245) : Color.FromArgb(42, 45, 52);
            Color rowLine = isDark ? Color.FromArgb(42, 45, 52) : Color.FromArgb(244, 246, 250);

            using (Font titleFont = new Font("Microsoft YaHei UI", CustomFontSize, FontStyle.Regular))
            using (Font btnFont = new Font("Microsoft YaHei UI", 8F, FontStyle.Regular))
            using (Font btnBoldFont = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold))
            using (Pen linePen = new Pen(rowLine, 1))
            {
                for (int i = start; i <= end; i++)
                {
                    var item = Items[i];
                    int y = (i * ItemHeight) - offset;
                    bool isHover = (i == _hoverIndex);

                    Rectangle rowRect = new Rectangle(2, y + 1, usableW - 2, ItemHeight - 2);

                    // 行背景
                    Color rowBg;
                    if (isDark)
                    {
                        rowBg = isHover ? Color.FromArgb(40, 44, 52) : Color.FromArgb(28, 30, 34);
                    }
                    else if (ThemeType == "custom")
                    {
                        rowBg = isHover ? Color.FromArgb(230, 255, 255, 255) : Color.FromArgb(180, 255, 255, 255);
                    }
                    else
                    {
                        rowBg = isHover ? Color.FromArgb(254, 248, 244) : Color.White;
                    }

                    using (Brush bgBrush = new SolidBrush(rowBg))
                    {
                        g.FillRectangle(bgBrush, rowRect);
                    }
                    g.DrawLine(linePen, 4, y + ItemHeight - 1, usableW, y + ItemHeight - 1);

                    int sendW = 34;
                    int fillW = 28;
                    int editW = 24;
                    int btnH = Math.Min(24, ItemHeight - 10);
                    int btnY = y + (ItemHeight - btnH) / 2;

                    int sendLeft = usableW - sendW - 4;
                    int fillLeft = sendLeft - fillW - 4;
                    int editLeft = fillLeft - editW - 4;

                    Rectangle rSend = new Rectangle(sendLeft, btnY, sendW, btnH);
                    Rectangle rFill = new Rectangle(fillLeft, btnY, fillW, btnH);
                    Rectangle rEdit = new Rectangle(editLeft, btnY, editW, btnH);

                    int textLeft = 10;
                    int textW = editLeft - textLeft - 4;
                    Rectangle textRect = new Rectangle(textLeft, y, textW, ItemHeight);

                    TextRenderer.DrawText(g, item.Title, titleFont, textRect, textPrimary,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);

                    // 1. 【✏️】iOS 液体玻璃按钮
                    bool hoverEdit = (isHover && _hoverBtnType == 1);
                    Color ebTop = isDark ? Color.FromArgb(55, 60, 70) : Color.FromArgb(252, 253, 255);
                    Color ebBtm = isDark ? Color.FromArgb(42, 46, 54) : Color.FromArgb(238, 242, 248);
                    Color ebBdr = isDark ? Color.FromArgb(70, 75, 88) : Color.FromArgb(215, 222, 234);
                    Color ebTxt = isDark ? Color.FromArgb(190, 195, 205) : Color.FromArgb(85, 90, 100);

                    GlassRenderer.DrawLiquidGlassButton(g, rEdit, ebTop, ebBtm, ebBdr, hoverEdit, 5);
                    TextRenderer.DrawText(g, "✏️", btnFont, rEdit, ebTxt, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    // 2. 【填】iOS 液体玻璃胶囊按钮
                    bool hoverFill = (isHover && _hoverBtnType == 2);
                    GlassRenderer.DrawLiquidGlassButton(g, rFill, ebTop, ebBtm, ebBdr, hoverFill, 5);
                    TextRenderer.DrawText(g, "填", btnFont, rFill, ebTxt, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    // 3. 【发】阿里活力橙·琥珀水晶液体玻璃宝石按钮
                    bool hoverSend = (isHover && _hoverBtnType == 3);
                    GlassRenderer.DrawLiquidGlassButton(g, rSend,
                        Color.FromArgb(255, 125, 35), Color.FromArgb(255, 75, 0),
                        Color.FromArgb(255, 150, 70), hoverSend, 6);
                    TextRenderer.DrawText(g, "发", btnBoldFont, rSend, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        }
    }

    /// <summary>
    /// 自绘制的 iOS 液体玻璃独立按钮控件（用于顶栏）
    /// </summary>
    public class LiquidGlassButtonControl : Control
    {
        public bool IsHovered { get; private set; }
        public Color BaseTop { get; set; }
        public Color BaseBottom { get; set; }
        public Color BorderColor { get; set; }
        public Color TextColor { get; set; }
        public bool IsGearIcon { get; set; }
        public int CornerRadius { get; set; }

        public LiquidGlassButtonControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);

            BaseTop = Color.FromArgb(252, 253, 255);
            BaseBottom = Color.FromArgb(238, 242, 248);
            BorderColor = Color.FromArgb(215, 222, 234);
            TextColor = Color.FromArgb(70, 75, 85);
            CornerRadius = 6;
            this.Cursor = Cursors.Hand;

            this.MouseEnter += (s, e) => { IsHovered = true; this.Invalidate(); };
            this.MouseLeave += (s, e) => { IsHovered = false; this.Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle rect = new Rectangle(1, 1, this.Width - 2, this.Height - 2);

            GlassRenderer.DrawLiquidGlassButton(g, rect, BaseTop, BaseBottom, BorderColor, IsHovered, CornerRadius);

            if (IsGearIcon)
            {
                // 绘制纯矢量抗锯齿高清齿轮图标
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
        private Panel _topBar;
        private PictureBox _picAvatar;
        private Label _lblLogo;
        private LiquidGlassButtonControl _btnSettings; // iOS 液体玻璃矢量齿轮按钮
        private CheckBox _chkTopMost;
        private LiquidGlassButtonControl _btnAddPhrase; // iOS 液体玻璃橙色宝石按钮
        private TextBox _searchBox;
        private CustomCategoryBar _categoryBar;
        private MiniPhraseListView _phraseList;
        private Panel _statusBar;
        private Label _lblTarget;

        // 数据
        private AppData _data = new AppData();
        private string _currentCat = "全部";
        private readonly string _storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "phrases.json");
        private readonly string _iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
        private readonly string _avatarPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "avatar.png");
        private readonly string _imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
        private Image _customBgImg = null;
        private Color _primaryOrange = Color.FromArgb(255, 80, 0);

        public MainForm()
        {
            if (!Directory.Exists(_imagesDir)) Directory.CreateDirectory(_imagesDir);

            InitializeUi();
            LoadData();
            RestoreWindowBounds();
            ApplySettings();
            _categoryBar.SetCategories(_data.Categories, _currentCat);
            RefreshList();
            StartTracking();

            this.ResizeEnd += (s, e) => SaveWindowBounds();
            this.FormClosing += (s, e) => SaveWindowBounds();
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
            // 默认宽度优化为舒适的 320px
            this.Size = new Size(320, 680);
            this.MinimumSize = new Size(270, 400);
            this.StartPosition = FormStartPosition.Manual;

            Rectangle workArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workArea.Right - 335, (workArea.Height - 680) / 2);
            this.TopMost = true;
            this.BackColor = Color.White;
            this.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            if (File.Exists(_iconPath))
            {
                try { this.Icon = new Icon(_iconPath); } catch { }
            }

            // 1. 顶部 Header
            _topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.White,
                Padding = new Padding(6, 6, 6, 4)
            };

            // 头像彩蛋
            _picAvatar = new PictureBox
            {
                Size = new Size(24, 24),
                Location = new Point(8, 7),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };
            ToolTip tip = new ToolTip();
            tip.SetToolTip(_picAvatar, "✨ 点击查看软件原图 (彩蛋)");
            _picAvatar.Image = EmbeddedAssets.GetAvatarImage(_avatarPath);
            _picAvatar.Click += (s, e) => ShowEasterEggDialog();

            _lblLogo = new Label
            {
                Text = "客服花花",
                Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold),
                ForeColor = _primaryOrange,
                Location = new Point(35, 7),
                AutoSize = true
            };

            // 【设置】iOS 液体玻璃矢量高清齿轮按钮（无 emoji 跨系统排版 Bug）
            _btnSettings = new LiquidGlassButtonControl
            {
                Size = new Size(30, 24),
                IsGearIcon = true,
                CornerRadius = 6,
                TextColor = Color.FromArgb(80, 85, 95)
            };
            tip.SetToolTip(_btnSettings, "系统设置（背景、字体、备份恢复）");
            _btnSettings.Click += (s, e) => OpenSettingsDialog();

            _chkTopMost = new CheckBox
            {
                Text = "置顶",
                Checked = true,
                AutoSize = true,
                Cursor = Cursors.Hand,
                Font = new Font("Microsoft YaHei UI", 8.5F)
            };
            _chkTopMost.CheckedChanged += (s, e) => this.TopMost = _chkTopMost.Checked;

            // 【+词】iOS 液体玻璃宝石按钮
            _btnAddPhrase = new LiquidGlassButtonControl
            {
                Text = "+词",
                Size = new Size(44, 24),
                Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold),
                BaseTop = Color.FromArgb(255, 125, 35),
                BaseBottom = Color.FromArgb(255, 75, 0),
                BorderColor = Color.FromArgb(255, 150, 70),
                TextColor = Color.White,
                CornerRadius = 6
            };
            _btnAddPhrase.Click += (s, e) => ShowAddOrEditDialog(null);

            // 第二行：搜索框
            _searchBox = new TextBox
            {
                Location = new Point(8, 36),
                Size = new Size(285, 24),
                Font = new Font("Microsoft YaHei UI", 9F),
                ForeColor = Color.Gray,
                Text = "🔍 搜索提示词..."
            };
            _searchBox.Enter += (s, e) => {
                if (_searchBox.Text.StartsWith("🔍")) { _searchBox.Text = ""; _searchBox.ForeColor = Color.Black; }
            };
            _searchBox.Leave += (s, e) => {
                if (string.IsNullOrWhiteSpace(_searchBox.Text)) { _searchBox.Text = "🔍 搜索提示词..."; _searchBox.ForeColor = Color.Gray; }
            };
            _searchBox.TextChanged += (s, e) => RefreshList();

            _topBar.Controls.AddRange(new Control[] { _picAvatar, _lblLogo, _btnSettings, _chkTopMost, _btnAddPhrase, _searchBox });

            // 2. 分类导航条
            _categoryBar = new CustomCategoryBar
            {
                Dock = DockStyle.Top,
                Height = 36
            };
            _categoryBar.OnCategorySelected += (cat) => { _currentCat = cat; RefreshList(); };
            _categoryBar.OnAddCategoryRequested += () => AddCategoryPrompt();
            _categoryBar.OnDeleteCurrentCategoryRequested += () => DeleteCurrentCategoryPrompt();
            _categoryBar.OnRenameCategoryRequested += (cat) => RenameCategoryPrompt(cat);
            _categoryBar.OnDeleteCategoryRequested += (cat) => DeleteCategory(cat);

            // 3. 工作区列表
            _phraseList = new MiniPhraseListView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            _phraseList.OnActionTriggered += (item, isSend) => SendToTarget(item, isSend);
            _phraseList.OnEditRequested += (item) => ShowAddOrEditDialog(item);

            // 4. 底部状态栏
            _statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                BackColor = Color.FromArgb(242, 244, 248),
                Padding = new Padding(6, 3, 6, 2)
            };
            _lblTarget = new Label
            {
                Text = "🎯 目标: 请先点击千牛聊天框",
                AutoSize = true,
                Dock = DockStyle.Left,
                Font = new Font("Microsoft YaHei UI", 7.5F),
                ForeColor = Color.FromArgb(120, 125, 135)
            };
            _statusBar.Controls.Add(_lblTarget);

            this.Controls.Add(_phraseList);
            this.Controls.Add(_categoryBar);
            this.Controls.Add(_topBar);
            this.Controls.Add(_statusBar);

            // 绝对排版：保证一打开立刻完整显示 [⚙️设置] [☑置顶] [+词]，且绝无遮挡！
            Action updateLayout = () =>
            {
                int w = this.ClientSize.Width;
                _btnAddPhrase.Location = new Point(w - 52, 6);
                _chkTopMost.Location = new Point(_btnAddPhrase.Left - 54, 9);
                _btnSettings.Location = new Point(_chkTopMost.Left - 36, 6);
                _searchBox.Width = w - 16;
                _phraseList.Invalidate();
            };

            this.Load += (s, e) => updateLayout();
            this.Resize += (s, e) => updateLayout();
            updateLayout();
        }

        private void OpenSettingsDialog()
        {
            using (Form setDlg = new Form())
            {
                setDlg.Text = "系统设置 - 客服花花";
                setDlg.Size = new Size(420, 480);
                setDlg.StartPosition = FormStartPosition.CenterParent;
                setDlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                setDlg.MaximizeBox = false;
                setDlg.MinimizeBox = false;
                setDlg.BackColor = Color.White;
                setDlg.Font = new Font("Microsoft YaHei UI", 9F);
                if (this.Icon != null) setDlg.Icon = this.Icon;

                // 1. 自定义背景分组
                GroupBox grpBg = new GroupBox
                {
                    Text = "🎨 界面背景主题",
                    Location = new Point(18, 12),
                    Size = new Size(368, 130),
                    ForeColor = Color.FromArgb(40, 44, 52)
                };

                RadioButton rbWhite = new RadioButton { Text = "⚪ 经典白 (清爽明亮)", Location = new Point(20, 26), AutoSize = true };
                RadioButton rbBlack = new RadioButton { Text = "⚫ 极夜黑 (暗黑护眼)", Location = new Point(190, 26), AutoSize = true };
                RadioButton rbCustom = new RadioButton { Text = "🖼️ 自定义图片背景", Location = new Point(20, 56), AutoSize = true };

                string currentTheme = _data.Settings.ThemeType;
                if (currentTheme == "black") rbBlack.Checked = true;
                else if (currentTheme == "custom") rbCustom.Checked = true;
                else rbWhite.Checked = true;

                Button btnChooseBg = new Button
                {
                    Text = "📁 选择背景图片...",
                    Location = new Point(20, 86),
                    Size = new Size(130, 30),
                    FlatStyle = FlatStyle.Flat
                };
                Button btnClearBg = new Button
                {
                    Text = "清除图片",
                    Location = new Point(160, 86),
                    Size = new Size(90, 30),
                    FlatStyle = FlatStyle.Flat
                };

                string tempCustomBg = _data.Settings.CustomBgPath;

                btnChooseBg.Click += (s, e) =>
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Title = "选择背景壁纸图片";
                        ofd.Filter = "图片文件 (*.jpg;*.png;*.jpeg;*.bmp)|*.jpg;*.png;*.jpeg;*.bmp";
                        if (ofd.ShowDialog(setDlg) == DialogResult.OK)
                        {
                            try
                            {
                                string ext = Path.GetExtension(ofd.FileName);
                                string newBgName = "bg_" + Guid.NewGuid().ToString("N") + ext;
                                string fullTarget = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, newBgName);
                                File.Copy(ofd.FileName, fullTarget, true);
                                tempCustomBg = newBgName;
                                rbCustom.Checked = true;
                                MessageBox.Show("背景图片已载入！保存设置后生效。", "提示");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("加载背景图片失败: " + ex.Message, "提示");
                            }
                        }
                    }
                };

                btnClearBg.Click += (s, e) =>
                {
                    tempCustomBg = "";
                    rbWhite.Checked = true;
                };

                grpBg.Controls.AddRange(new Control[] { rbWhite, rbBlack, rbCustom, btnChooseBg, btnClearBg });

                // 2. 字体大小分组（加大最大等级，提供4档舒适字号）
                GroupBox grpFont = new GroupBox
                {
                    Text = "🔠 提示词字体大小",
                    Location = new Point(18, 152),
                    Size = new Size(368, 75),
                    ForeColor = Color.FromArgb(40, 44, 52)
                };

                RadioButton rbF0 = new RadioButton { Text = "标准(9pt)", Location = new Point(15, 30), AutoSize = true };
                RadioButton rbF1 = new RadioButton { Text = "中号(11pt)", Location = new Point(98, 30), AutoSize = true };
                RadioButton rbF2 = new RadioButton { Text = "大号(13pt)", Location = new Point(188, 30), AutoSize = true };
                RadioButton rbF3 = new RadioButton { Text = "特大(15pt)🔥", Location = new Point(275, 30), AutoSize = true, Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold), ForeColor = _primaryOrange };

                int curLevel = _data.Settings.FontSizeLevel;
                if (curLevel == 1) rbF1.Checked = true;
                else if (curLevel == 2) rbF2.Checked = true;
                else if (curLevel == 3) rbF3.Checked = true;
                else rbF0.Checked = true;

                grpFont.Controls.AddRange(new Control[] { rbF0, rbF1, rbF2, rbF3 });

                // 3. 数据备份与换机迁移分组
                GroupBox grpBackup = new GroupBox
                {
                    Text = "💾 数据备份与换机迁移",
                    Location = new Point(18, 237),
                    Size = new Size(368, 125),
                    ForeColor = Color.FromArgb(40, 44, 52)
                };

                Button btnExport = new Button
                {
                    Text = "📤 导出全量备份包 (.zip)",
                    Location = new Point(18, 28),
                    Size = new Size(200, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(246, 248, 252)
                };
                btnExport.Click += (s, e) => ExportBackupData();

                Button btnImport = new Button
                {
                    Text = "📥 导入备份包恢复...",
                    Location = new Point(18, 72),
                    Size = new Size(200, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(246, 248, 252)
                };
                btnImport.Click += (s, e) => ImportBackupData();

                Button btnOpenDir = new Button
                {
                    Text = "📂 数据目录",
                    Location = new Point(235, 48),
                    Size = new Size(115, 36),
                    FlatStyle = FlatStyle.Flat
                };
                btnOpenDir.Click += (s, e) =>
                {
                    try { System.Diagnostics.Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory); }
                    catch { }
                };

                grpBackup.Controls.AddRange(new Control[] { btnExport, btnImport, btnOpenDir });

                Button btnSave = new Button
                {
                    Text = "保存设置",
                    Location = new Point(205, 385),
                    Size = new Size(95, 34),
                    BackColor = _primaryOrange,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.OK
                };
                btnSave.FlatAppearance.BorderSize = 0;

                Button btnCancel = new Button
                {
                    Text = "取消",
                    Location = new Point(310, 385),
                    Size = new Size(76, 34),
                    DialogResult = DialogResult.Cancel
                };

                setDlg.Controls.AddRange(new Control[] { grpBg, grpFont, grpBackup, btnSave, btnCancel });

                if (setDlg.ShowDialog(this) == DialogResult.OK)
                {
                    if (rbBlack.Checked) _data.Settings.ThemeType = "black";
                    else if (rbCustom.Checked) _data.Settings.ThemeType = "custom";
                    else _data.Settings.ThemeType = "white";

                    _data.Settings.CustomBgPath = tempCustomBg;

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
                string fullImg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _data.Settings.CustomBgPath);
                if (File.Exists(fullImg))
                {
                    try { _customBgImg = Image.FromFile(fullImg); } catch { }
                }
            }

            Color topBg = isDark ? Color.FromArgb(32, 34, 38) : Color.White;
            Color textCol = isDark ? Color.FromArgb(235, 238, 245) : Color.FromArgb(40, 44, 52);
            Color boxBg = isDark ? Color.FromArgb(45, 48, 56) : Color.White;

            _topBar.BackColor = topBg;
            _chkTopMost.ForeColor = textCol;
            _searchBox.BackColor = boxBg;
            _searchBox.ForeColor = isDark ? Color.FromArgb(200, 205, 215) : Color.Black;

            _btnSettings.BaseTop = isDark ? Color.FromArgb(55, 60, 70) : Color.FromArgb(252, 253, 255);
            _btnSettings.BaseBottom = isDark ? Color.FromArgb(42, 46, 54) : Color.FromArgb(238, 242, 248);
            _btnSettings.BorderColor = isDark ? Color.FromArgb(70, 75, 88) : Color.FromArgb(215, 222, 234);
            _btnSettings.TextColor = isDark ? Color.FromArgb(200, 205, 215) : Color.FromArgb(80, 85, 95);
            _btnSettings.Invalidate();

            _categoryBar.IsDarkTheme = isDark;
            _categoryBar.Invalidate();

            _phraseList.SetTheme(theme, _customBgImg);
        }

        private void ExportBackupData()
        {
            if (_data.Phrases.Count == 0 && _data.Categories.Count == 0)
            {
                MessageBox.Show("当前没有任何分类或提示词，暂无需备份！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "导出客服花花全量备份包";
                string timeStr = DateTime.Now.ToString("yyyyMMdd_HHmm");
                sfd.FileName = string.Format("客服花花备份_{0}.zip", timeStr);
                sfd.Filter = "花花备份文件 (*.zip)|*.zip|所有文件 (*.*)|*.*";

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    string tempDir = Path.Combine(Path.GetTempPath(), "HuahuaBackup_" + Guid.NewGuid().ToString("N"));
                    try
                    {
                        Directory.CreateDirectory(tempDir);
                        SaveData();
                        File.Copy(_storagePath, Path.Combine(tempDir, "phrases.json"), true);

                        if (Directory.Exists(_imagesDir))
                        {
                            string targetImgDir = Path.Combine(tempDir, "images");
                            Directory.CreateDirectory(targetImgDir);
                            foreach (string imgFile in Directory.GetFiles(_imagesDir))
                            {
                                string dest = Path.Combine(targetImgDir, Path.GetFileName(imgFile));
                                File.Copy(imgFile, dest, true);
                            }
                        }

                        if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
                        ZipFile.CreateFromDirectory(tempDir, sfd.FileName, CompressionLevel.Optimal, false);

                        MessageBox.Show(
                            string.Format("🎉 备份成功！\n\n备份包已保存至：\n{0}\n\n💡 换电脑时，只需将此备份文件发到新电脑，\n点击【设置 ➔ 导入备份包】即可一秒复原所有提示词与图片！", sfd.FileName),
                            "备份成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("备份过程中发生异常: " + ex.Message, "备份失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); } catch { }
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
                        "导入备份将恢复该备份包中的所有【分类】、【提示词】以及【附带图片】。\n是否确认导入？",
                        "确认导入备份", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirm != DialogResult.Yes) return;

                    string tempExtractDir = Path.Combine(Path.GetTempPath(), "HuahuaRestore_" + Guid.NewGuid().ToString("N"));
                    try
                    {
                        Directory.CreateDirectory(tempExtractDir);
                        ZipFile.ExtractToDirectory(ofd.FileName, tempExtractDir);

                        string backupJson = Path.Combine(tempExtractDir, "phrases.json");
                        if (!File.Exists(backupJson))
                        {
                            MessageBox.Show("无效的备份文件：未在该文件中检测到 phrases.json 数据！", "导入失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                        LoadData();
                        ApplySettings();
                        _currentCat = "全部";
                        _categoryBar.SetCategories(_data.Categories, _currentCat);
                        RefreshList();

                        MessageBox.Show(
                            string.Format("🎉 恢复完成！\n\n已成功复原 {0} 个分类、{1} 条提示词（附带图片均已恢复就绪）！", _data.Categories.Count, _data.Phrases.Count),
                            "恢复成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (!File.Exists(_avatarPath)) return;

            using (Form eggForm = new Form())
            {
                eggForm.Text = "🌸 客服花花 - 看板娘原图";
                eggForm.Size = new Size(360, 420);
                eggForm.StartPosition = FormStartPosition.CenterParent;
                eggForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                eggForm.MaximizeBox = false;
                eggForm.MinimizeBox = false;
                eggForm.BackColor = Color.White;
                if (this.Icon != null) eggForm.Icon = this.Icon;

                PictureBox pic = new PictureBox
                {
                    Dock = DockStyle.Top,
                    Height = 310,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(_avatarPath),
                    Padding = new Padding(10)
                };

                Label lblText = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = "🐱「？？！正在努力帮主人回复客户呢！」\n点击任意处或按 ESC 即可返回",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(100, 105, 115)
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
                        _lblTarget.Text = "🎯 目标: " + shortTitle;
                        _lblTarget.ForeColor = title.Contains("千牛") || title.Contains("旺旺") ? _primaryOrange : Color.FromArgb(40, 160, 70);
                    }
                }
            };
            _windowTrackerTimer.Start();
        }

        private void RefreshList()
        {
            string keyword = _searchBox.Text.StartsWith("🔍") ? "" : _searchBox.Text.Trim();

            List<PhraseItem> filtered = new List<PhraseItem>();
            foreach (var item in _data.Phrases)
            {
                if (_currentCat != "全部" && item.Category != _currentCat)
                    continue;

                if (!string.IsNullOrEmpty(keyword))
                {
                    bool matchTitle = item.Title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool matchContent = item.Content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!matchTitle && !matchContent) continue;
                }

                filtered.Add(item);
            }

            _phraseList.UpdateData(filtered);
        }

        private void SendToTarget(PhraseItem item, bool sendEnter)
        {
            if (_lastTargetHwnd == IntPtr.Zero)
            {
                MessageBox.Show("尚未捕获聊天窗口！\n请先鼠标点一下千牛或微信输入框，然后再点发送。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool hasImage = !string.IsNullOrEmpty(item.ImagePath);
            string fullImgPath = hasImage ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, item.ImagePath) : "";
            bool imgFileValid = hasImage && File.Exists(fullImgPath);
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

                if (imgFileValid) Thread.Sleep(120);
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
        }

        private void AddCategoryPrompt()
        {
            string name = PromptInput("新建分类", "请输入新分类名称：", "");
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                if (_data.Categories.Contains(name))
                {
                    MessageBox.Show("该分类已存在！", "提示");
                    return;
                }
                _data.Categories.Add(name);
                _currentCat = name;
                SaveData();
                _categoryBar.SetCategories(_data.Categories, _currentCat);
                RefreshList();
            }
        }

        private void DeleteCurrentCategoryPrompt()
        {
            if (_currentCat == "全部")
            {
                MessageBox.Show("【全部】为系统默认聚合分类，不可删除哦！\n请先点击想要删除的具体分类，然后再点击 [-] 删除。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DeleteCategory(_currentCat);
        }

        private void RenameCategoryPrompt(string oldName)
        {
            string newName = PromptInput("修改分类名称", "请输入新的分类名称：", oldName);
            if (!string.IsNullOrWhiteSpace(newName) && newName != oldName)
            {
                newName = newName.Trim();
                int idx = _data.Categories.IndexOf(oldName);
                if (idx >= 0) _data.Categories[idx] = newName;

                foreach (var p in _data.Phrases)
                {
                    if (p.Category == oldName) p.Category = newName;
                }

                if (_currentCat == oldName) _currentCat = newName;
                SaveData();
                _categoryBar.SetCategories(_data.Categories, _currentCat);
                RefreshList();
            }
        }

        private void DeleteCategory(string cat)
        {
            if (MessageBox.Show("确定删除分类【" + cat + "】吗？\n该分类下的话术将自动归入【未分类】。", "确认删除分类", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _data.Categories.Remove(cat);
                foreach (var p in _data.Phrases)
                {
                    if (p.Category == cat) p.Category = "未分类";
                }
                _currentCat = "全部";
                SaveData();
                _categoryBar.SetCategories(_data.Categories, _currentCat);
                RefreshList();
            }
        }

        private void ShowAddOrEditDialog(PhraseItem editItem)
        {
            bool isEditing = (editItem != null);
            using (Form dlg = new Form())
            {
                dlg.Text = isEditing ? "修改话术" : "添加话术";
                dlg.Size = new Size(390, 440);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.Font = new Font("Microsoft YaHei UI", 9F);
                dlg.AllowDrop = true;
                if (this.Icon != null) dlg.Icon = this.Icon;

                Label lblCat = new Label { Text = "分类：", Location = new Point(16, 18), AutoSize = true };
                ComboBox cmbCat = new ComboBox { Location = new Point(85, 15), Width = 265, DropDownStyle = ComboBoxStyle.DropDownList };
                
                if (_data.Categories.Count == 0)
                {
                    cmbCat.Items.Add("未分类");
                }
                else
                {
                    foreach (var c in _data.Categories) cmbCat.Items.Add(c);
                }

                if (isEditing)
                {
                    if (!cmbCat.Items.Contains(editItem.Category)) cmbCat.Items.Add(editItem.Category);
                    cmbCat.SelectedItem = editItem.Category;
                }
                else if (_currentCat != "全部" && cmbCat.Items.Contains(_currentCat))
                {
                    cmbCat.SelectedItem = _currentCat;
                }
                else
                {
                    cmbCat.SelectedIndex = 0;
                }

                Label lblTitle = new Label { Text = "提示词：", Location = new Point(16, 55), AutoSize = true };
                TextBox txtTitle = new TextBox { Location = new Point(85, 52), Width = 265, Text = isEditing ? editItem.Title : "" };

                Label lblContent = new Label { Text = "正文：", Location = new Point(16, 92), AutoSize = true };
                TextBox txtContent = new TextBox
                {
                    Location = new Point(85, 89),
                    Width = 265,
                    Height = 95,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    Text = isEditing ? editItem.Content : ""
                };

                Label lblImg = new Label { Text = "图片：", Location = new Point(16, 198), AutoSize = true };
                
                Panel pnlDropBox = new Panel
                {
                    Location = new Point(85, 195),
                    Size = new Size(80, 80),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(246, 248, 252),
                    AllowDrop = true
                };

                PictureBox picPreview = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent,
                    AllowDrop = true
                };
                pnlDropBox.Controls.Add(picPreview);

                Label lblDropHint = new Label
                {
                    Text = "直接将图片\n拖入此处",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft YaHei UI", 8F),
                    ForeColor = Color.FromArgb(140, 145, 155),
                    Visible = true
                };
                pnlDropBox.Controls.Add(lblDropHint);

                string currentSelectedImgPath = isEditing ? editItem.ImagePath : "";
                Action updatePreview = () =>
                {
                    if (!string.IsNullOrEmpty(currentSelectedImgPath))
                    {
                        string fullP = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, currentSelectedImgPath);
                        if (File.Exists(fullP))
                        {
                            try
                            {
                                using (var temp = Image.FromFile(fullP))
                                {
                                    picPreview.Image = new Bitmap(temp);
                                }
                                lblDropHint.Visible = false;
                                return;
                            }
                            catch { }
                        }
                    }
                    picPreview.Image = null;
                    lblDropHint.Visible = true;
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
                        if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp" || ext == ".webp")
                        {
                            try
                            {
                                string newName = "images/" + Guid.NewGuid().ToString("N") + ext;
                                string targetFull = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, newName);
                                File.Copy(file, targetFull, true);
                                currentSelectedImgPath = newName;
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

                Button btnUploadImg = new Button
                {
                    Text = "📁 本地选择",
                    Location = new Point(175, 195),
                    Size = new Size(80, 26),
                    FlatStyle = FlatStyle.Flat
                };

                Button btnPasteImg = new Button
                {
                    Text = "📋 粘贴图片",
                    Location = new Point(265, 195),
                    Size = new Size(85, 26),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(240, 246, 255)
                };

                Button btnClearImg = new Button
                {
                    Text = "🗑️ 清除图片",
                    Location = new Point(175, 230),
                    Size = new Size(80, 26),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(190, 50, 50)
                };

                btnUploadImg.Click += (s, e) =>
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Title = "选择话术图片";
                        ofd.Filter = "图片文件 (*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp";
                        if (ofd.ShowDialog(dlg) == DialogResult.OK)
                        {
                            try
                            {
                                string ext = Path.GetExtension(ofd.FileName);
                                string newName = "images/" + Guid.NewGuid().ToString("N") + ext;
                                string targetFull = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, newName);
                                File.Copy(ofd.FileName, targetFull, true);

                                currentSelectedImgPath = newName;
                                updatePreview();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("加载图片失败: " + ex.Message, "提示");
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
                                        string newName = "images/" + Guid.NewGuid().ToString("N") + ext;
                                        string targetFull = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, newName);
                                        File.Copy(files[0], targetFull, true);

                                        currentSelectedImgPath = newName;
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
                    currentSelectedImgPath = "";
                    updatePreview();
                };

                Button btnSave = new Button
                {
                    Text = "保存话术",
                    Location = new Point(185, 345),
                    Size = new Size(80, 32),
                    BackColor = _primaryOrange,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.OK
                };
                btnSave.FlatAppearance.BorderSize = 0;

                Button btnCancel = new Button
                {
                    Text = "取消",
                    Location = new Point(275, 345),
                    Size = new Size(75, 32),
                    DialogResult = DialogResult.Cancel
                };

                if (isEditing)
                {
                    Button btnDel = new Button
                    {
                        Text = "删除",
                        Location = new Point(16, 345),
                        Size = new Size(60, 32),
                        ForeColor = Color.Red
                    };
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
                        MessageBox.Show("提示词不能为空！", "提示");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtContent.Text) && string.IsNullOrEmpty(currentSelectedImgPath))
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
                        editItem.ImagePath = currentSelectedImgPath;
                    }
                    else
                    {
                        _data.Phrases.Insert(0, new PhraseItem(selectedCategory, txtTitle.Text.Trim(), txtContent.Text.Trim(), currentSelectedImgPath));
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
            using (Form form = new Form())
            {
                form.Text = title;
                form.Size = new Size(320, 150);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                if (this.Icon != null) form.Icon = this.Icon;

                Label label = new Label { Left = 20, Top = 16, Text = promptText, AutoSize = true };
                TextBox textBox = new TextBox { Left = 20, Top = 40, Width = 260, Text = defaultValue };
                Button btnOk = new Button { Text = "确定", Left = 130, Width = 70, Top = 72, DialogResult = DialogResult.OK, BackColor = _primaryOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                Button btnCancel = new Button { Text = "取消", Left = 210, Width = 70, Top = 72, DialogResult = DialogResult.Cancel };

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
                    var data = JsonHelper.Deserialize(json);
                    if (data != null)
                    {
                        _data = data;
                        return;
                    }
                }
                catch { }
            }

            _data.Settings = new AppSettings();
            _data.Categories = new List<string>();
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
            sb.AppendFormat("    \"CustomBgPath\": \"{0}\",\n", Escape(data.Settings.CustomBgPath));
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
                sb.AppendFormat("      \"ImagePath\": \"{0}\"\n", Escape(p.ImagePath));
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
            AppData appData = new AppData();
            
            string fLevelStr = ExtractValue(json, "FontSizeLevel");
            int fLevel;
            if (int.TryParse(fLevelStr, out fLevel)) appData.Settings.FontSizeLevel = fLevel;

            string theme = ExtractValue(json, "ThemeType");
            if (!string.IsNullOrEmpty(theme)) appData.Settings.ThemeType = theme;

            string customBg = ExtractValue(json, "CustomBgPath");
            if (!string.IsNullOrEmpty(customBg)) appData.Settings.CustomBgPath = customBg;

            string wStr = ExtractValue(json, "WindowWidth");
            int wVal;
            if (int.TryParse(wStr, out wVal)) appData.Settings.WindowWidth = wVal;

            string hStr = ExtractValue(json, "WindowHeight");
            int hVal;
            if (int.TryParse(hStr, out hVal)) appData.Settings.WindowHeight = hVal;

            string xStr = ExtractValue(json, "WindowX");
            int xVal;
            if (int.TryParse(xStr, out xVal)) appData.Settings.WindowX = xVal;

            string yStr = ExtractValue(json, "WindowY");
            int yVal;
            if (int.TryParse(yStr, out yVal)) appData.Settings.WindowY = yVal;

            string hasPosStr = ExtractValue(json, "HasWindowPos");
            if (hasPosStr == "true") appData.Settings.HasWindowPos = true;

            int catIdx = json.IndexOf("\"Categories\":");
            if (catIdx != -1)
            {
                int bStart = json.IndexOf("[", catIdx);
                int bEnd = json.IndexOf("]", bStart);
                if (bStart != -1 && bEnd != -1)
                {
                    string rawCats = json.Substring(bStart + 1, bEnd - bStart - 1);
                    string[] tokens = rawCats.Split(',');
                    foreach (var token in tokens)
                    {
                        string clean = token.Trim().Trim('\"', ' ', '\r', '\n');
                        if (!string.IsNullOrEmpty(clean)) appData.Categories.Add(Unescape(clean));
                    }
                }
            }

            string[] items = json.Split(new string[] { "}," }, StringSplitOptions.None);
            foreach (var itemStr in items)
            {
                string cat = ExtractValue(itemStr, "Category");
                string title = ExtractValue(itemStr, "Title");
                string content = ExtractValue(itemStr, "Content");
                string imgPath = ExtractValue(itemStr, "ImagePath");

                if (!string.IsNullOrEmpty(title))
                {
                    appData.Phrases.Add(new PhraseItem(cat, title, content, imgPath));
                }
            }
            return appData;
        }

        private static string ExtractValue(string source, string key)
        {
            string pattern = "\"" + key + "\": ";
            int idx = source.IndexOf(pattern);
            if (idx == -1) return "";
            int start = idx + pattern.Length;
            if (start < source.Length && source[start] == '\"')
            {
                start++;
                int end = source.IndexOf("\"", start);
                if (end == -1) return "";
                return Unescape(source.Substring(start, end - start));
            }
            else
            {
                int end = source.IndexOfAny(new char[] { ',', '\n', '\r', '}' }, start);
                if (end == -1) end = source.Length;
                return source.Substring(start, end - start).Trim();
            }
        }

        private static string Escape(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        private static string Unescape(string s)
        {
            if (s == null) return "";
            return s.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\\"", "\"").Replace("\\\\", "\\");
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
        public static readonly string AvatarBase64 = "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";
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