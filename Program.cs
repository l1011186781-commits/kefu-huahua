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

        public AppSettings()
        {
            FontSizeLevel = 0;
            ThemeType = "white";
            CustomBgPath = "";
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
            ApplySettings();
            _categoryBar.SetCategories(_data.Categories, _currentCat);
            RefreshList();
            StartTracking();
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
            if (File.Exists(_avatarPath))
            {
                try { _picAvatar.Image = Image.FromFile(_avatarPath); } catch { }
            }
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
            sb.AppendFormat("    \"CustomBgPath\": \"{0}\"\n", Escape(data.Settings.CustomBgPath));
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
}
