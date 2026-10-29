using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MetasequoiaTuner
{
    // ---------------- 主题 ----------------
    public static class T
    {
        public static bool IsDark = true;
        public static Color Bg, Card, Card2, Line, Text, Muted, Accent;
        public static Font H1 = new Font("Microsoft YaHei UI", 15F, FontStyle.Bold);
        public static Font H2 = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
        public static Font Body = new Font("Microsoft YaHei UI", 9.5F);
        public static Font Mono = new Font("Consolas", 10F, FontStyle.Bold);

        public static void Apply(bool dark)
        {
            IsDark = dark;
            if (dark)
            {
                Bg = Color.FromArgb(22, 23, 27);
                Card = Color.FromArgb(32, 33, 39);
                Card2 = Color.FromArgb(41, 43, 50);
                Line = Color.FromArgb(56, 58, 66);
                Text = Color.FromArgb(233, 234, 238);
                Muted = Color.FromArgb(148, 153, 162);
                Accent = Color.FromArgb(255, 165, 0);
            }
            else
            {
                Bg = Color.FromArgb(244, 245, 248);
                Card = Color.FromArgb(255, 255, 255);
                Card2 = Color.FromArgb(236, 238, 243);
                Line = Color.FromArgb(214, 218, 226);
                Text = Color.FromArgb(28, 30, 35);
                Muted = Color.FromArgb(108, 114, 125);
                Accent = Color.FromArgb(240, 140, 0);
            }
        }
    }

    // ---------------- TOML ----------------
    public class TomlDoc
    {
        public List<string> Lines = new List<string>();
        public string Path;

        public static TomlDoc Load(string path)
        {
            var d = new TomlDoc { Path = path };
            d.Lines = File.ReadAllLines(path, new UTF8Encoding(false)).ToList();
            return d;
        }

        int SectionStart(string section)
        {
            for (int i = 0; i < Lines.Count; i++)
                if (Lines[i].Trim() == "[" + section + "]") return i;
            return -1;
        }

        public string Get(string section, string key, string def)
        {
            int s = SectionStart(section);
            if (s < 0) return def;
            for (int i = s + 1; i < Lines.Count; i++)
            {
                string t = Lines[i].Trim();
                if (t.StartsWith("[")) break;
                if (t.StartsWith("#")) continue;
                int eq = t.IndexOf('=');
                if (eq <= 0) continue;
                if (t.Substring(0, eq).Trim() == key)
                {
                    string v = t.Substring(eq + 1).Trim();
                    bool inQ = false;
                    for (int k = 0; k < v.Length; k++)
                    {
                        if (v[k] == '"') inQ = !inQ;
                        else if (v[k] == '#' && !inQ) { v = v.Substring(0, k).Trim(); break; }
                    }
                    return v.Trim('"');
                }
            }
            return def;
        }

        public void Set(string section, string key, string value)
        {
            int s = SectionStart(section);
            if (s < 0)
            {
                if (Lines.Count > 0 && Lines[Lines.Count - 1].Trim() != "") Lines.Add("");
                Lines.Add("[" + section + "]");
                Lines.Add(key + " = " + value);
                return;
            }
            int end = Lines.Count;
            for (int i = s + 1; i < Lines.Count; i++)
                if (Lines[i].Trim().StartsWith("[")) { end = i; break; }

            for (int i = s + 1; i < end; i++)
            {
                string t = Lines[i].Trim();
                if (t.StartsWith("#")) continue;
                int eq = t.IndexOf('=');
                if (eq <= 0) continue;
                if (t.Substring(0, eq).Trim() == key)
                {
                    Lines[i] = key + " = " + value;
                    return;
                }
            }
            Lines.Insert(end, key + " = " + value);
        }

        public void Save() { File.WriteAllText(Path, string.Join("\r\n", Lines) + "\r\n", new UTF8Encoding(false)); }
    }

    public static class ColorUtil
    {
        public static Color Parse(string s, Color fallback)
        {
            s = (s ?? "").Trim();
            try
            {
                if (s.StartsWith("rgba", StringComparison.OrdinalIgnoreCase) || s.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
                {
                    var inner = s.Substring(s.IndexOf('(') + 1).TrimEnd(')');
                    var parts = inner.Split(',');
                    if (parts.Length >= 3)
                    {
                        int r = (int)Math.Round(double.Parse(parts[0].Trim()));
                        int g = (int)Math.Round(double.Parse(parts[1].Trim()));
                        int b = (int)Math.Round(double.Parse(parts[2].Trim()));
                        double a = parts.Length > 3 ? double.Parse(parts[3].Trim()) : 1.0;
                        return Color.FromArgb((int)Math.Round(a * 255), r, g, b);
                    }
                }
                if (s.StartsWith("#"))
                {
                    string h = s.Substring(1);
                    if (h.Length == 3) h = "" + h[0] + h[0] + h[1] + h[1] + h[2] + h[2];
                    if (h.Length == 6) h = "ff" + h;
                    if (h.Length == 8)
                        return Color.FromArgb(
                            Convert.ToInt32(h.Substring(0, 2), 16),
                            Convert.ToInt32(h.Substring(2, 2), 16),
                            Convert.ToInt32(h.Substring(4, 2), 16),
                            Convert.ToInt32(h.Substring(6, 2), 16));
                }
            }
            catch { }
            return fallback;
        }

        public static string Format(Color c)
        {
            if (c.A == 255) return "#" + c.R.ToString("x2") + c.G.ToString("x2") + c.B.ToString("x2");
            return string.Format("rgba({0},{1},{2},{3})", c.R, c.G, c.B, Math.Round(c.A / 255.0, 2));
        }
    }

    public static class R
    {
        public static GraphicsPath Round(RectangleF r, float radius)
        {
            var p = new GraphicsPath();
            float d = Math.Max(0, Math.Min(radius, Math.Min(r.Width, r.Height) / 2)) * 2;
            if (d <= 0.5f) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    // ---------------- 可折叠卡片 ----------------
    public class Card : Panel
    {
        public string Title = "";
        public bool Expanded = true;
        public int ContentHeight = 100;
        public Action OnToggle;
        public const int HeaderH = 46;

        public Card() { DoubleBuffered = true; BackColor = T.Bg; }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new RectangleF(0.5f, 0.5f, Width - 1, Height - 1);
            using (var path = R.Round(rect, 12))
            {
                using (var b = new SolidBrush(T.Card)) g.FillPath(b, path);
                using (var p = new Pen(T.Line)) g.DrawPath(p, path);
            }
            using (var b = new SolidBrush(T.Text))
                g.DrawString(Title ?? "", T.H2, b, 18, 14);

            float cx = Width - 26, cy = 24;
            using (var pen = new Pen(T.Muted, 1.8f))
            {
                if (Expanded)
                {
                    g.DrawLine(pen, cx - 5, cy - 2, cx, cy + 3);
                    g.DrawLine(pen, cx, cy + 3, cx + 5, cy - 2);
                }
                else
                {
                    g.DrawLine(pen, cx - 5, cy + 2, cx, cy - 3);
                    g.DrawLine(pen, cx, cy - 3, cx + 5, cy + 2);
                }
            }
            base.OnPaint(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Y <= HeaderH && OnToggle != null) OnToggle();
        }

        public void SetExpanded(bool expanded)
        {
            Expanded = expanded;
            Height = expanded ? ContentHeight : HeaderH;
            foreach (Control c in Controls) c.Visible = expanded;
            Invalidate();
        }
    }

    // ---------------- 滑块 ----------------
    public class FlatSlider : Control
    {
        int _min, _max = 64, _value;
        public event EventHandler ValueChanged;
        public int Minimum { get { return _min; } set { _min = value; if (_value < _min) Value = _min; Invalidate(); } }
        public int Maximum { get { return _max; } set { _max = value; if (_value > _max) Value = _max; Invalidate(); } }
        public int Value
        {
            get { return _value; }
            set
            {
                int v = Math.Max(_min, Math.Min(_max, value));
                if (v != _value)
                {
                    _value = v;
                    Invalidate();
                    if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
                }
            }
        }
        public FlatSlider()
        {
            DoubleBuffered = true;
            Height = 26;
            BackColor = T.Card;
            TabStop = false;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int th = 5;
            int y = Height / 2 - th / 2;
            using (var path = R.Round(new RectangleF(0, y, Width, th), th / 2f))
            using (var b = new SolidBrush(T.Card2)) g.FillPath(b, path);

            float pct = (_max == _min) ? 0 : (_value - _min) / (float)(_max - _min);
            float w = Math.Max(th, Width * pct);
            using (var path = R.Round(new RectangleF(0, y, w, th), th / 2f))
            using (var b = new SolidBrush(T.Accent)) g.FillPath(b, path);

            float cx = Math.Min(Width - 9, Math.Max(9, w));
            using (var b = new SolidBrush(T.Card)) g.FillEllipse(b, cx - 9, Height / 2 - 9, 18, 18);
            using (var p = new Pen(T.Accent, 3f)) g.DrawEllipse(p, cx - 7, Height / 2 - 7, 14, 14);
        }
        void SetFromX(int x)
        {
            float pct = Math.Max(0f, Math.Min(1f, (x - 9f) / Math.Max(1f, Width - 18f)));
            Value = _min + (int)Math.Round(pct * (_max - _min));
        }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); SetFromX(e.X); }
        protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e); if (e.Button == MouseButtons.Left) SetFromX(e.X); }
    }

    // ---------------- 开关 ----------------
    public class Toggle : Control
    {
        bool _checked;
        public event EventHandler CheckedChanged;
        public bool Checked
        {
            get { return _checked; }
            set { if (_checked != value) { _checked = value; Invalidate(); if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty); } }
        }
        public Toggle() { DoubleBuffered = true; BackColor = T.Card; Height = 24; Width = 46; Cursor = Cursors.Hand; }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = R.Round(new RectangleF(0, 1, 44, 22), 11))
            using (var b = new SolidBrush(_checked ? T.Accent : T.Card2)) g.FillPath(b, path);
            float kx = _checked ? 24 : 4;
            using (var b = new SolidBrush(_checked ? Color.FromArgb(30, 30, 34) : T.Muted))
                g.FillEllipse(b, kx, 3, 18, 18);
        }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); Checked = !Checked; }
    }

    // ---------------- 颜色块 ----------------
    public class Swatch : Control
    {
        public Color Color = Color.Orange;
        public event EventHandler Picked;
        public Swatch() { DoubleBuffered = true; BackColor = T.Card; Height = 28; Width = 56; Cursor = Cursors.Hand; }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = R.Round(new RectangleF(0.5f, 0.5f, Width - 1, Height - 1), 8))
            {
                using (var b = new SolidBrush(Color)) g.FillPath(b, path);
                using (var p = new Pen(T.Line)) g.DrawPath(p, path);
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            using (var dlg = new ColorDialog { FullOpen = true, Color = Color })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Color = System.Drawing.Color.FromArgb(Color.A, dlg.Color.R, dlg.Color.G, dlg.Color.B);
                    Invalidate();
                    if (Picked != null) Picked(this, EventArgs.Empty);
                }
            }
        }
    }

    // ---------------- 预览 ----------------
    public class PreviewPanel : Panel
    {
        public Color Selected = Color.FromArgb(255, 165, 0);
        public Color SelectedText = Color.White;
        public Color Surface = Color.White;
        public Color Border = Color.FromArgb(228, 228, 228);
        public Color NormalText = Color.FromArgb(40, 40, 40);
        public Color NumberText = Color.FromArgb(130, 130, 130);
        public Color BarColor = Color.DarkOrange;
        public int ItemGap = 14, InnerPadding = 6, ItemPadRight = 8, NumberGap = 4, NumberInset = 6, Radius = 10, BarWidth = 3;
        public bool ShowBar = true, BarInside = true;
        public string FontName = "Microsoft YaHei UI";
        public int FontSizePx = 17;
        public string[] Candidates = { "你们", "你", "尼", "妮", "泥", "逆", "拟", "腻", "匿" };

        public PreviewPanel() { DoubleBuffered = true; BackColor = Color.FromArgb(18, 19, 23); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var bg = new SolidBrush(BackColor))
            using (var path = R.Round(new RectangleF(0, 0, Width, Height), 10))
                g.FillPath(bg, path);

            int n = Candidates.Length;
            float bodyPt = Math.Max(7f, FontSizePx * 0.75f);
            using (var f0 = new Font(FontName, bodyPt))
            using (var nf0 = new Font(FontName, Math.Max(6f, bodyPt * 0.72f)))
            {
                var numW = new int[n];
                var txtW = new int[n];
                int totalLogical = InnerPadding * 2;
                for (int i = 0; i < n; i++)
                {
                    numW[i] = TextRenderer.MeasureText(g, (i + 1).ToString(), nf0, new Size(1000, 1000), TextFormatFlags.NoPadding).Width;
                    txtW[i] = TextRenderer.MeasureText(g, Candidates[i], f0, new Size(1000, 1000), TextFormatFlags.NoPadding).Width;
                    totalLogical += NumberInset + numW[i] + NumberGap + txtW[i] + ItemPadRight;
                    if (i < n - 1) totalLogical += ItemGap;
                }

                float avail = Math.Max(60, ClientSize.Width - 32);
                float sc = totalLogical > avail ? avail / totalLogical : 1f;

                int baseItemH = (int)Math.Round(FontSizePx * 1.9f);
                int itemH = Math.Max(18, (int)(baseItemH * sc));
                int pad = Math.Max(2, (int)(InnerPadding * sc));
                int gap = Math.Max(0, (int)(ItemGap * sc));
                int numGap = Math.Max(0, (int)(NumberGap * sc));
                int rightPad = Math.Max(1, (int)(ItemPadRight * sc));
                int radius = Math.Max(1, (int)(Radius * sc));
                int innerLeft = Math.Max(2, (int)(NumberInset * sc));

                var widths = new int[n];
                int cardW = pad * 2;
                for (int i = 0; i < n; i++)
                {
                    widths[i] = (int)((NumberInset + numW[i] + NumberGap + txtW[i] + ItemPadRight) * sc) + 2;
                    cardW += widths[i];
                    if (i < n - 1) cardW += gap;
                }
                int cardH = itemH + pad * 2;
                int x0 = Math.Max(16, (ClientSize.Width - cardW) / 2);
                int y0 = Math.Max(6, (ClientSize.Height - cardH) / 2);

                using (var path = R.Round(new RectangleF(x0, y0, cardW, cardH), Math.Min(radius, 16)))
                {
                    using (var b = new SolidBrush(Surface)) g.FillPath(b, path);
                    using (var p = new Pen(Border)) g.DrawPath(p, path);
                }

                using (var f = new Font(FontName, Math.Max(6f, bodyPt * sc)))
                using (var nf = new Font(FontName, Math.Max(5f, bodyPt * 0.72f * sc)))
                using (var sf = new StringFormat(StringFormat.GenericTypographic) { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near, FormatFlags = StringFormatFlags.NoWrap })
                using (var bNormal = new SolidBrush(NormalText))
                using (var bNumber = new SolidBrush(NumberText))
                using (var bSel = new SolidBrush(SelectedText))
                {
                    int x = x0 + pad;
                    for (int i = 0; i < n; i++)
                    {
                        var item = new Rectangle(x, y0 + pad, widths[i], itemH);
                        if (i == 0)
                        {
                            using (var ip = R.Round(item, Math.Max(2, radius / 2)))
                            using (var b = new SolidBrush(Selected)) g.FillPath(b, ip);
                            if (ShowBar && BarWidth > 0)
                            {
                                int bw = Math.Max(1, (int)(BarWidth * sc));
                                int bh = Math.Max(4, (int)(itemH * 0.6));
                                int barX = BarInside ? item.Left + 3 : item.Left - bw / 2;
                                using (var bb = new SolidBrush(BarColor))
                                    g.FillRectangle(bb, new Rectangle(barX, item.Top + (itemH - bh) / 2, bw, bh));
                            }
                        }
                        var brNum = i == 0 ? bSel : bNumber;
                        var brTxt = i == 0 ? bSel : bNormal;
                        g.DrawString((i + 1).ToString(), nf, brNum,
                            new RectangleF(item.Left + innerLeft, item.Top, numW[i] * sc + 2, itemH), sf);
                        float textX = item.Left + innerLeft + numW[i] * sc + numGap;
                        g.DrawString(Candidates[i], f, brTxt,
                            new RectangleF(textX, item.Top, Math.Max(4, item.Right - textX - rightPad + 6), itemH), sf);
                        x += widths[i] + gap;
                    }
                }
            }
        }
    }

    // ---------------- 设置 ----------------
    public static class Settings
    {
        public static string Dir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "MetasequoiaTuner"); }
        }
        public static string FilePath { get { return Path.Combine(Dir, "settings.ini"); } }

        public static Dictionary<string, string> Load()
        {
            var d = new Dictionary<string, string>();
            try
            {
                if (!File.Exists(FilePath)) return d;
                foreach (var line in File.ReadAllLines(FilePath, Encoding.UTF8))
                {
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    d[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
                }
            }
            catch { }
            return d;
        }

        public static void Save(Dictionary<string, string> d)
        {
            try
            {
                Directory.CreateDirectory(Dir);
                var sb = new StringBuilder();
                foreach (var kv in d) sb.AppendLine(kv.Key + "=" + kv.Value);
                File.WriteAllText(FilePath, sb.ToString(), new UTF8Encoding(false));
            }
            catch { }
        }
    }

    // ---------------- 主窗口 ----------------
    public class MainForm : Form
    {
        const string SkinsDir = @"%LOCALAPPDATA%\metasequoiaime\skins";
        const string ServerExe = @"C:\Program Files\metasequoiaime\server\MetasequoiaImeServer.exe";

        ListBox skinList = new ListBox();
        ComboBox cbFont, cbEnglishFont;
        FlatSlider sGap, sPad, sPadRight, sNumberGap, sItemMinWidth, sRadius, sMinWidth, sBarWidth, sInset, sFontSize;
        Label vGap, vPad, vPadRight, vNumberGap, vItemMinWidth, vRadius, vMinWidth, vBarWidth, vInset, vFontSize;
        Swatch swLightSel, swLightSelText, swLightAccent, swLightHover, swLightBar;
        Swatch swDarkSel, swDarkSelText, swDarkAccent, swDarkHover, swDarkBar;
        Label lbLightSel, lbLightSelText, lbLightAccent, lbLightHover, lbLightBar;
        Label lbDarkSel, lbDarkSelText, lbDarkAccent, lbDarkHover, lbDarkBar;
        Toggle tgBar, tgBarInside, tgTrim, tgDarkPreview, tgCandPreedit, tgTsfPreedit, tgTheme;
        Label status = new Label();
        PreviewPanel preview = new PreviewPanel();
        TomlDoc doc;
        Panel side, scroll, right, bottom;
        List<Card> cards = new List<Card>();
        string currentSkin = "";

        public MainForm()
        {
            var st = Settings.Load();
            bool dark = !st.ContainsKey("theme") || st["theme"] != "light";
            T.Apply(dark);

            Text = "水杉调校台 · Metasequoia Tuner";
            Width = 1120; Height = 1180;
            if (st.ContainsKey("window"))
            {
                try
                {
                    var wh = st["window"].Split('x');
                    if (wh.Length == 2) { Width = int.Parse(wh[0]); Height = int.Parse(wh[1]); }
                }
                catch { }
            }
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = T.Bg;
            ForeColor = T.Text;
            Font = T.Body;
            DoubleBuffered = true;

            side = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = T.Bg, Padding = new Padding(16, 18, 12, 12) };
            var logo = new Label { Text = "水杉调校台", Font = T.H1, ForeColor = T.Text, Dock = DockStyle.Top, Height = 40, BackColor = T.Bg };
            var sub = new Label { Text = "Metasequoia Tuner", Font = T.Body, ForeColor = T.Muted, Dock = DockStyle.Top, Height = 26, BackColor = T.Bg };
            var skinTitle = new Label { Text = "皮肤", Font = T.H2, ForeColor = T.Muted, Dock = DockStyle.Top, Height = 30, Padding = new Padding(0, 8, 0, 0), BackColor = T.Bg };


            var rescan = new Button
            {
                Text = "重新扫描",
                Dock = DockStyle.Bottom,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = T.Card2,
                ForeColor = T.Text,
                Font = T.Body
            };
            rescan.FlatAppearance.BorderColor = T.Line;
            rescan.Click += (s, e) => LoadSkins(null);

            skinList.Dock = DockStyle.Fill;
            skinList.BackColor = T.Card;
            skinList.ForeColor = T.Text;
            skinList.BorderStyle = BorderStyle.None;
            skinList.Font = new Font("Microsoft YaHei UI", 10F);
            skinList.IntegralHeight = false;
            skinList.ItemHeight = 28;
            skinList.DrawMode = DrawMode.OwnerDrawFixed;
            skinList.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                using (var b = new SolidBrush(sel ? T.Accent : T.Card))
                    e.Graphics.FillRectangle(b, e.Bounds);
                using (var b = new SolidBrush(sel ? Color.FromArgb(28, 28, 30) : T.Text))
                    e.Graphics.DrawString(skinList.Items[e.Index].ToString(), skinList.Font, b, e.Bounds.X + 10, e.Bounds.Y + 6);
            };
            skinList.SelectedIndexChanged += (s, e) => LoadSkin();

            side.Controls.Add(skinList);
            side.Controls.Add(rescan);
            side.Controls.Add(skinTitle);
            side.Controls.Add(sub);
            side.Controls.Add(logo);

            scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = T.Bg, Padding = new Padding(6, 10, 6, 6) };

            var cardPrev = new Card { Title = "实时预览", Left = 0, Top = 0, Width = 820, ContentHeight = 210 };
            cardPrev.Height = cardPrev.ContentHeight;
            preview.Left = 18; preview.Top = 48; preview.Width = 784; preview.Height = 144;
            preview.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cardPrev.Controls.Add(preview);
            tgDarkPreview = new Toggle { Left = 736, Top = 18, Width = 46 };
            var lblDark = new Label { Text = "深色预览", Left = 650, Top = 22, Width = 80, ForeColor = T.Muted, Font = T.Body, BackColor = T.Card };
            tgDarkPreview.Checked = st.ContainsKey("preview_dark") && st["preview_dark"] == "1";
            tgDarkPreview.CheckedChanged += (s, e) => UpdatePreview();
            cardPrev.Controls.Add(tgDarkPreview);
            cardPrev.Controls.Add(lblDark);

            var cardGeom = new Card { Title = "间距与布局", Left = 0, Top = 0, Width = 820, ContentHeight = 448 };
            cardGeom.Height = cardGeom.ContentHeight;
            int y = 46;
            sGap = AddSlider(cardGeom, "候选间距 item_gap", ref y, 0, 64, out vGap);
            sPad = AddSlider(cardGeom, "内边距 padding", ref y, 0, 40, out vPad);
            sPadRight = AddSlider(cardGeom, "候选右留白 item_padding_right", ref y, 0, 40, out vPadRight);
            sNumberGap = AddSlider(cardGeom, "序号间距 number_gap", ref y, 0, 24, out vNumberGap);
            sItemMinWidth = AddSlider(cardGeom, "候选最小宽度 item_min_width", ref y, 20, 200, out vItemMinWidth);
            sRadius = AddSlider(cardGeom, "圆角 corner_radius_dip", ref y, 0, 32, out vRadius);
            sMinWidth = AddSlider(cardGeom, "卡片最小宽度 min_width_dip", ref y, 60, 400, out vMinWidth);
            sBarWidth = AddSlider(cardGeom, "竖条宽度 selected_bar_width", ref y, 0, 12, out vBarWidth);
            sInset = AddSlider(cardGeom, "内容左内缩 number_inset", ref y, 0, 24, out vInset);

            tgBar = new Toggle { Left = 106, Top = 404, Width = 46, BackColor = T.Card };
            var lblBar = new Label { Text = "显示竖条", Left = 18, Top = 406, Width = 86, ForeColor = T.Muted, BackColor = T.Card };
            tgBar.CheckedChanged += (s, e) => UpdatePreview();
            cardGeom.Controls.Add(tgBar);
            cardGeom.Controls.Add(lblBar);
            tgBarInside = new Toggle { Left = 330, Top = 404, Width = 46, BackColor = T.Card };
            var lblBarIn = new Label { Text = "竖条在块内", Left = 242, Top = 406, Width = 86, ForeColor = T.Muted, BackColor = T.Card };
            tgBarInside.CheckedChanged += (s, e) => UpdatePreview();
            cardGeom.Controls.Add(tgBarInside);
            cardGeom.Controls.Add(lblBarIn);
            tgTrim = new Toggle { Left = 560, Top = 404, Width = 46, BackColor = T.Card };
            var lblTrim = new Label { Text = "隐藏拼音时裁剪空白", Left = 380, Top = 406, Width = 174, ForeColor = T.Muted, BackColor = T.Card };
            cardGeom.Controls.Add(tgTrim);
            cardGeom.Controls.Add(lblTrim);

            var cardColor = new Card { Title = "颜色", Left = 0, Top = 0, Width = 820, ContentHeight = 390 };
            cardColor.Height = cardColor.ContentHeight;
            cardColor.Controls.Add(new Label { Text = "浅色", Left = 18, Top = 46, Width = 60, Font = T.H2, ForeColor = T.Text, BackColor = T.Card });
            cardColor.Controls.Add(new Label { Text = "深色", Left = 420, Top = 46, Width = 60, Font = T.H2, ForeColor = T.Text, BackColor = T.Card });
            int cy = 82;
            AddColor(cardColor, 18, ref cy, "选中背景", out swLightSel, out lbLightSel);
            AddColor(cardColor, 18, ref cy, "选中文字", out swLightSelText, out lbLightSelText);
            AddColor(cardColor, 18, ref cy, "强调色", out swLightAccent, out lbLightAccent);
            AddColor(cardColor, 18, ref cy, "悬停色", out swLightHover, out lbLightHover);
            AddColor(cardColor, 18, ref cy, "竖条色", out swLightBar, out lbLightBar);
            cy = 82;
            AddColor(cardColor, 420, ref cy, "选中背景", out swDarkSel, out lbDarkSel);
            AddColor(cardColor, 420, ref cy, "选中文字", out swDarkSelText, out lbDarkSelText);
            AddColor(cardColor, 420, ref cy, "强调色", out swDarkAccent, out lbDarkAccent);
            AddColor(cardColor, 420, ref cy, "悬停色", out swDarkHover, out lbDarkHover);
            AddColor(cardColor, 420, ref cy, "竖条色", out swDarkBar, out lbDarkBar);

            tgCandPreedit = new Toggle { Left = 470, Top = 340, Width = 46, BackColor = T.Card };
            var lblCandP = new Label { Text = "候选窗显示拼音", Left = 330, Top = 342, Width = 132, ForeColor = T.Muted, BackColor = T.Card };
            cardColor.Controls.Add(tgCandPreedit);
            cardColor.Controls.Add(lblCandP);
            tgTsfPreedit = new Toggle { Left = 160, Top = 340, Width = 46, BackColor = T.Card };
            var lblTsfP = new Label { Text = "输入框显示拼音", Left = 18, Top = 342, Width = 132, ForeColor = T.Muted, BackColor = T.Card };
            cardColor.Controls.Add(tgTsfPreedit);
            cardColor.Controls.Add(lblTsfP);

            var cardFont = new Card { Title = "字体", Left = 0, Top = 0, Width = 820, ContentHeight = 210 };
            cardFont.Height = cardFont.ContentHeight;
            cardFont.Controls.Add(new Label { Text = "候选窗主字体 english_font", Left = 18, Top = 52, Width = 150, ForeColor = T.Muted, BackColor = T.Card });
            cardFont.Controls.Add(new Label { Text = "中文回退字体 font", Left = 18, Top = 92, Width = 150, ForeColor = T.Muted, BackColor = T.Card });
            cbFont = new ComboBox { Left = 200, Top = 48, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = T.Card2, ForeColor = T.Text, Font = T.Body };
            cbEnglishFont = new ComboBox { Left = 200, Top = 88, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = T.Card2, ForeColor = T.Text, Font = T.Body };
            cbFont.SelectedIndexChanged += (s, e) => UpdatePreview();
            cbEnglishFont.SelectedIndexChanged += (s, e) => UpdatePreview();
            cardFont.Controls.Add(cbFont);
            cardFont.Controls.Add(cbEnglishFont);
            int fy = 128;
            sFontSize = AddSlider(cardFont, "字号 font_size", ref fy, 12, 32, out vFontSize);
            cardFont.Controls.Add(new Label { Text = "预览使用所选字体与字号", Left = 500, Top = 52, Width = 300, ForeColor = T.Muted, BackColor = T.Card });

            cards.Add(cardPrev);
            cards.Add(cardGeom);
            cards.Add(cardColor);
            cards.Add(cardFont);
            foreach (var c in cards) scroll.Controls.Add(c);

            bottom = new Panel { Dock = DockStyle.Bottom, Height = 78, BackColor = T.Bg, Padding = new Padding(6, 8, 6, 6) };
            var btnRow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, BackColor = T.Bg };
            var bSave = MakeButton("保存", false);
            var bRestart = MakeButton("重启水杉", false);
            var bBoth = MakeButton("保存并重启", true);
            bBoth.Tag = "primary";
            bSave.Click += (s, e) => SaveDoc(false);
            bRestart.Click += (s, e) => RestartIme();
            bBoth.Click += (s, e) => SaveDoc(true);
            btnRow.Controls.Add(bSave);
            btnRow.Controls.Add(bRestart);
            btnRow.Controls.Add(bBoth);
            var lblTheme = new Label { Text = "浅色主题", Left = 460, Top = 10, Width = 80, ForeColor = T.Muted, Font = T.Body, BackColor = T.Bg };
            tgTheme = new Toggle { Left = 540, Top = 8, Width = 46, BackColor = T.Bg };
            tgTheme.Checked = !dark;
            tgTheme.CheckedChanged += (s, e) => { if (tgTheme.Checked == T.IsDark) SwitchTheme(tgTheme.Checked); };
            btnRow.Controls.Add(lblTheme);
            btnRow.Controls.Add(tgTheme);
            status.Dock = DockStyle.Bottom;
            status.Height = 22;
            status.ForeColor = T.Muted;
            status.Font = T.Body;
            status.Text = "就绪";
            bottom.Controls.Add(btnRow);
            bottom.Controls.Add(status);

            right = new Panel { Dock = DockStyle.Fill, BackColor = T.Bg };
            right.Controls.Add(scroll);
            right.Controls.Add(bottom);

            Controls.Add(right);
            Controls.Add(side);

            foreach (var sld in new[] { sGap, sPad, sPadRight, sNumberGap, sItemMinWidth, sRadius, sMinWidth, sBarWidth, sInset })
                sld.ValueChanged += (s, e) => UpdatePreview();
            sFontSize.ValueChanged += (s, e) => UpdatePreview();
            foreach (var sw in new[] { swLightSel, swLightSelText, swLightAccent, swLightHover, swLightBar, swDarkSel, swDarkSelText, swDarkAccent, swDarkHover, swDarkBar })
                sw.Picked += (s, e) => { UpdateColorLabels(); UpdatePreview(); };

            foreach (var c in cards) c.OnToggle = () => ToggleCard(c);
            if (st.ContainsKey("collapsed"))
            {
                var set = st["collapsed"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < cards.Count; i++)
                    if (set.Contains(i.ToString())) cards[i].SetExpanded(false);
            }
            Relayout();
            Shown += (s, e) => { scroll.AutoScrollPosition = new Point(0, 0); };
            FormClosing += (s, e) => SaveSettings();

            LoadSkins(st.ContainsKey("skin") ? st["skin"] : null);
        }

        void ToggleCard(Card c)
        {
            c.SetExpanded(!c.Expanded);
            Relayout();
            SaveSettings();
        }

        void Relayout()
        {
            int top = 0;
            foreach (var c in cards)
            {
                c.Top = top;
                top += c.Height + 12;
            }
        }

        void SaveSettings()
        {
            var d = new Dictionary<string, string>();
            d["theme"] = T.IsDark ? "dark" : "light";
            d["skin"] = currentSkin;
            d["preview_dark"] = tgDarkPreview.Checked ? "1" : "0";
            var collapsed = new List<string>();
            for (int i = 0; i < cards.Count; i++) if (!cards[i].Expanded) collapsed.Add(i.ToString());
            d["collapsed"] = string.Join(",", collapsed);
            d["window"] = Width + "x" + Height;
            Settings.Save(d);
        }

        void SwitchTheme(bool light)
        {
            T.Apply(!light);
            Recolor(this);
            Invalidate(true);
            SaveSettings();
        }

        void Recolor(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Card) c.BackColor = T.Bg;
                else if (c is PreviewPanel) { }
                else if (c is FlatSlider || c is Toggle || c is Swatch) c.BackColor = T.Card;
                else if (c is ListBox) { c.BackColor = T.Card; c.ForeColor = T.Text; }
                else if (c is ComboBox) { c.BackColor = T.Card2; c.ForeColor = T.Text; }
                else if (c is Button)
                {
                    var b = (Button)c;
                    bool primary = (b.Tag as string) == "primary";
                    b.BackColor = primary ? T.Accent : T.Card2;
                    b.ForeColor = primary ? Color.FromArgb(28, 28, 30) : T.Text;
                    b.FlatAppearance.BorderColor = primary ? T.Accent : T.Line;
                }
                else if (c is Label)
                {
                    c.BackColor = (c.Parent is Card) ? T.Card : T.Bg;
                    c.ForeColor = T.Muted;
                }
                else if (c is Panel) c.BackColor = T.Bg;
                if (c.HasChildren) Recolor(c);
                c.Invalidate();
            }
        }

        Button MakeButton(string text, bool primary)
        {
            var b = new Button
            {
                Text = text,
                Width = 132,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 10F, primary ? FontStyle.Bold : FontStyle.Regular),
                BackColor = primary ? T.Accent : T.Card2,
                ForeColor = primary ? Color.FromArgb(28, 28, 30) : T.Text,
                Margin = new Padding(0, 0, 12, 0)
            };
            b.FlatAppearance.BorderColor = primary ? T.Accent : T.Line;
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        FlatSlider AddSlider(Control parent, string label, ref int y, int min, int max, out Label valueLabel)
        {
            var l = new Label { Text = label, Left = 18, Top = y + 3, Width = 240, ForeColor = T.Muted, Font = T.Body, BackColor = T.Card };
            var s = new FlatSlider { Left = 262, Top = y - 3, Width = 380, Minimum = min, Maximum = max, Value = min, BackColor = T.Card };
            var v = new Label
            {
                Text = min.ToString(),
                Left = 656,
                Top = y + 2,
                Width = 60,
                ForeColor = T.Text,
                Font = new Font("Consolas", 11F, FontStyle.Bold),
                BackColor = T.Card
            };
            s.ValueChanged += (a, b) => v.Text = s.Value.ToString();
            parent.Controls.Add(l);
            parent.Controls.Add(s);
            parent.Controls.Add(v);
            valueLabel = v;
            y += 38;
            return s;
        }

        void AddColor(Control parent, int x, ref int cy, string label, out Swatch sw, out Label lb)
        {
            parent.Controls.Add(new Label { Text = label, Left = x, Top = cy + 6, Width = 86, ForeColor = T.Muted, Font = T.Body, BackColor = T.Card });
            sw = new Swatch { Left = x + 92, Top = cy, Width = 54, BackColor = T.Card };
            lb = new Label { Left = x + 154, Top = cy + 6, Width = 150, ForeColor = T.Muted, Font = new Font("Consolas", 9F), BackColor = T.Card };
            parent.Controls.Add(sw);
            parent.Controls.Add(lb);
            cy += 44;
        }

        string SkinRoot() { return Environment.ExpandEnvironmentVariables(SkinsDir); }

        void LoadSkins(string prefer)
        {
            skinList.Items.Clear();
            string root = SkinRoot();
            if (!Directory.Exists(root)) { status.Text = "找不到皮肤目录：" + root; return; }
            foreach (var dir in Directory.GetDirectories(root).OrderBy(d => d))
                if (File.Exists(Path.Combine(dir, "skin.toml")))
                    skinList.Items.Add(Path.GetFileName(dir));
            status.Text = "共 " + skinList.Items.Count + " 个皮肤";
            int idx = prefer != null ? skinList.Items.IndexOf(prefer) : -1;
            if (idx < 0) idx = skinList.Items.IndexOf("rimetune-orange");
            skinList.SelectedIndex = idx >= 0 ? idx : (skinList.Items.Count > 0 ? 0 : -1);
        }

        int Clamp(string s, int def, int min, int max)
        {
            int v;
            if (!int.TryParse(s, out v)) v = def;
            return Math.Max(min, Math.Min(max, v));
        }

        string HGet(string key, string def)
        {
            string v = doc.Get("candidate_window.horizontal", key, null);
            if (!string.IsNullOrEmpty(v)) return v;
            return doc.Get("candidate_window", key, def);
        }

        void LoadSkin()
        {
            if (skinList.SelectedItem == null) return;
            currentSkin = skinList.SelectedItem.ToString();
            string path = Path.Combine(SkinRoot(), currentSkin, "skin.toml");
            doc = TomlDoc.Load(path);

            sGap.Value = Clamp(HGet("item_gap", "4"), 4, sGap.Minimum, sGap.Maximum);
            sPad.Value = Clamp(HGet("padding", "4"), 4, sPad.Minimum, sPad.Maximum);
            sPadRight.Value = Clamp(doc.Get("candidate_window.horizontal", "item_padding_right", "4"), 4, sPadRight.Minimum, sPadRight.Maximum);
            sNumberGap.Value = Clamp(HGet("number_gap", "4"), 4, sNumberGap.Minimum, sNumberGap.Maximum);
            sItemMinWidth.Value = Clamp(HGet("item_min_width", "40"), 40, sItemMinWidth.Minimum, sItemMinWidth.Maximum);
            sRadius.Value = Clamp(doc.Get("candidate_window", "corner_radius_dip", "10"), 10, sRadius.Minimum, sRadius.Maximum);
            sMinWidth.Value = Clamp(doc.Get("candidate_window", "min_width_dip", "120"), 120, sMinWidth.Minimum, sMinWidth.Maximum);
            sBarWidth.Value = Clamp(doc.Get("candidate_window", "selected_bar_width", "1"), 1, sBarWidth.Minimum, sBarWidth.Maximum);
            sInset.Value = Clamp(HGet("number_inset", "4"), 4, sInset.Minimum, sInset.Maximum);

            SetSwatch(swLightSel, doc.Get("candidate.light", "selected", "#ffa500"), Color.Orange);
            SetSwatch(swLightSelText, doc.Get("candidate.light", "selected_text", "#ffffff"), Color.White);
            SetSwatch(swLightAccent, doc.Get("candidate.light", "accent", "#ffa500"), Color.Orange);
            SetSwatch(swLightHover, doc.Get("candidate.light", "hover", "rgba(255,165,0,0.16)"), Color.Orange);
            SetSwatch(swLightBar, doc.Get("candidate.light", "selected_bar", "#d97706"), Color.DarkOrange);
            SetSwatch(swDarkSel, doc.Get("candidate.dark", "selected", "#ffa500"), Color.Orange);
            SetSwatch(swDarkSelText, doc.Get("candidate.dark", "selected_text", "#ffffff"), Color.White);
            SetSwatch(swDarkAccent, doc.Get("candidate.dark", "accent", "#ffa500"), Color.Orange);
            SetSwatch(swDarkHover, doc.Get("candidate.dark", "hover", "rgba(255,165,0,0.25)"), Color.Orange);
            SetSwatch(swDarkBar, doc.Get("candidate.dark", "selected_bar", "#d97706"), Color.DarkOrange);

            tgBar.Checked = !string.Equals(doc.Get("candidate.light", "show_selected_bar", "true"), "false", StringComparison.OrdinalIgnoreCase);
            tgBarInside.Checked = !string.Equals(doc.Get("candidate_window", "selected_bar_inside", "true"), "false", StringComparison.OrdinalIgnoreCase);
            tgTrim.Checked = string.Equals(doc.Get("candidate_window", "trim_hidden_preedit_gap", "false"), "true", StringComparison.OrdinalIgnoreCase);

            LoadConfigOptions();
            UpdateColorLabels();
            UpdatePreview();
            status.Text = "已加载：" + path;
        }

        void SetSwatch(Swatch s, string text, Color fallback)
        {
            s.Color = ColorUtil.Parse(text, fallback);
            s.Invalidate();
        }

        void UpdateColorLabels()
        {
            lbLightSel.Text = ColorUtil.Format(swLightSel.Color);
            lbLightSelText.Text = ColorUtil.Format(swLightSelText.Color);
            lbLightAccent.Text = ColorUtil.Format(swLightAccent.Color);
            lbLightHover.Text = ColorUtil.Format(swLightHover.Color);
            lbLightBar.Text = ColorUtil.Format(swLightBar.Color);
            lbDarkSel.Text = ColorUtil.Format(swDarkSel.Color);
            lbDarkSelText.Text = ColorUtil.Format(swDarkSelText.Color);
            lbDarkAccent.Text = ColorUtil.Format(swDarkAccent.Color);
            lbDarkHover.Text = ColorUtil.Format(swDarkHover.Color);
            lbDarkBar.Text = ColorUtil.Format(swDarkBar.Color);
        }

        void UpdatePreview()
        {
            if (preview == null) return;
            preview.ItemGap = sGap.Value;
            preview.InnerPadding = sPad.Value;
            preview.ItemPadRight = sPadRight.Value;
            preview.NumberGap = sNumberGap.Value;
            preview.Radius = sRadius.Value;
            preview.NumberInset = sInset.Value;
            preview.BarWidth = sBarWidth.Value;
            preview.ShowBar = tgBar.Checked;
            preview.BarInside = tgBarInside.Checked;
            preview.Selected = tgDarkPreview.Checked ? swDarkSel.Color : swLightSel.Color;
            preview.SelectedText = tgDarkPreview.Checked ? swDarkSelText.Color : swLightSelText.Color;
            preview.BarColor = tgDarkPreview.Checked ? swDarkBar.Color : swLightBar.Color;
            preview.Surface = tgDarkPreview.Checked ? Color.FromArgb(34, 34, 39) : Color.White;
            preview.NormalText = tgDarkPreview.Checked ? Color.FromArgb(232, 232, 236) : Color.FromArgb(40, 40, 40);
            preview.NumberText = tgDarkPreview.Checked ? Color.FromArgb(150, 150, 156) : Color.FromArgb(130, 130, 130);
            preview.Border = tgDarkPreview.Checked ? Color.FromArgb(70, 72, 82) : Color.FromArgb(228, 228, 228);
            preview.FontName = (cbFont != null && cbFont.SelectedItem != null) ? cbFont.SelectedItem.ToString() : "Microsoft YaHei UI";
            preview.FontSizePx = sFontSize != null ? sFontSize.Value : 17;
            preview.Invalidate();
        }

        static string ConfigPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "metasequoiaime", "config.toml");
        }

        void FillFonts(ComboBox cb, string current)
        {
            cb.Items.Clear();
            var names = new List<string>();
            try
            {
                using (var fc = new System.Drawing.Text.InstalledFontCollection())
                    foreach (var f in fc.Families) names.Add(f.Name);
            }
            catch { }
            names.Sort(StringComparer.CurrentCulture);
            if (!string.IsNullOrEmpty(current) && !names.Contains(current)) names.Insert(0, current);
            foreach (var n in names) cb.Items.Add(n);
            int idx = -1;
            for (int i = 0; i < cb.Items.Count; i++)
                if (string.Equals(cb.Items[i].ToString(), current, StringComparison.OrdinalIgnoreCase)) { idx = i; break; }
            cb.SelectedIndex = idx >= 0 ? idx : (cb.Items.Count > 0 ? 0 : -1);
        }

        void LoadConfigOptions()
        {
            try
            {
                string cfg = ConfigPath();
                if (!File.Exists(cfg)) return;
                var cd = TomlDoc.Load(cfg);
                tgCandPreedit.Checked = cd.Get("appearance", "candidate_window_preedit_style", "pinyin") != "empty";
                tgTsfPreedit.Checked = cd.Get("appearance", "tsf_preedit_style", "raw") != "empty";
                FillFonts(cbFont, cd.Get("appearance", "english_font", "Segoe UI"));
                FillFonts(cbEnglishFont, cd.Get("appearance", "font", "Noto Sans SC"));
                sFontSize.Value = Clamp(cd.Get("appearance", "font_size", "17"), 17, sFontSize.Minimum, sFontSize.Maximum);
            }
            catch { }
        }

        void SaveConfigOptions()
        {
            try
            {
                string cfg = ConfigPath();
                if (!File.Exists(cfg)) return;
                var cd = TomlDoc.Load(cfg);
                cd.Set("appearance", "candidate_window_preedit_style", tgCandPreedit.Checked ? "\"pinyin\"" : "\"empty\"");
                cd.Set("appearance", "tsf_preedit_style", tgTsfPreedit.Checked ? "\"raw\"" : "\"empty\"");
                if (cbFont.SelectedItem != null) cd.Set("appearance", "english_font", "\"" + cbFont.SelectedItem.ToString() + "\"");
                if (cbEnglishFont.SelectedItem != null) cd.Set("appearance", "font", "\"" + cbEnglishFont.SelectedItem.ToString() + "\"");
                cd.Set("appearance", "font_size", sFontSize.Value.ToString());
                cd.Save();
            }
            catch (Exception ex) { status.Text = "写 config.toml 失败：" + ex.Message; }
        }

        void SaveDoc(bool restart)
        {
            if (doc == null) { status.Text = "没有选中皮肤"; return; }
            doc.Set("candidate_window", "item_gap", sGap.Value.ToString());
            doc.Set("candidate_window", "padding", sPad.Value.ToString());
            doc.Set("candidate_window", "number_gap", sNumberGap.Value.ToString());
            doc.Set("candidate_window", "item_min_width", sItemMinWidth.Value.ToString());
            doc.Set("candidate_window", "corner_radius_dip", sRadius.Value.ToString());
            doc.Set("candidate_window", "min_width_dip", sMinWidth.Value.ToString());
            doc.Set("candidate_window", "selected_bar_width", sBarWidth.Value.ToString());
            doc.Set("candidate_window", "number_inset", sInset.Value.ToString());
            doc.Set("candidate_window", "selected_bar_inside", tgBarInside.Checked ? "true" : "false");
            doc.Set("candidate_window", "trim_hidden_preedit_gap", tgTrim.Checked ? "true" : "false");

            doc.Set("candidate_window.horizontal", "item_gap", sGap.Value.ToString());
            doc.Set("candidate_window.horizontal", "padding", sPad.Value.ToString());
            doc.Set("candidate_window.horizontal", "item_padding_right", sPadRight.Value.ToString());
            doc.Set("candidate_window.horizontal", "number_gap", sNumberGap.Value.ToString());
            doc.Set("candidate_window.horizontal", "item_min_width", sItemMinWidth.Value.ToString());
            doc.Set("candidate_window.horizontal", "number_inset", sInset.Value.ToString());

            doc.Set("candidate.light", "show_selected_bar", tgBar.Checked ? "true" : "false");
            doc.Set("candidate.dark", "show_selected_bar", tgBar.Checked ? "true" : "false");
            doc.Set("candidate.light", "selected", Q(ColorUtil.Format(swLightSel.Color)));
            doc.Set("candidate.light", "selected_text", Q(ColorUtil.Format(swLightSelText.Color)));
            doc.Set("candidate.light", "accent", Q(ColorUtil.Format(swLightAccent.Color)));
            doc.Set("candidate.light", "hover", Q(ColorUtil.Format(swLightHover.Color)));
            doc.Set("candidate.light", "selected_bar", Q(ColorUtil.Format(swLightBar.Color)));
            doc.Set("candidate.dark", "selected", Q(ColorUtil.Format(swDarkSel.Color)));
            doc.Set("candidate.dark", "selected_text", Q(ColorUtil.Format(swDarkSelText.Color)));
            doc.Set("candidate.dark", "accent", Q(ColorUtil.Format(swDarkAccent.Color)));
            doc.Set("candidate.dark", "hover", Q(ColorUtil.Format(swDarkHover.Color)));
            doc.Set("candidate.dark", "selected_bar", Q(ColorUtil.Format(swDarkBar.Color)));

            try
            {
                doc.Save();
                status.Text = "已保存：" + doc.Path;
            }
            catch (Exception ex)
            {
                status.Text = "保存失败：" + ex.Message;
                return;
            }
            SaveConfigOptions();
            SaveSettings();
            if (restart) RestartIme();
        }

        static string Q(string v) { return "\"" + (v ?? "").Replace("\"", "") + "\""; }

        void RestartIme()
        {
            string ps = "Stop-Process -Name MetasequoiaImeServer -Force -ErrorAction SilentlyContinue; " +
                        "Start-Sleep -Milliseconds 1500; " +
                        "if (-not (Get-Process MetasequoiaImeServer -ErrorAction SilentlyContinue)) { " +
                        "Start-Process '" + ServerExe + "' }";
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("powershell.exe",
                    "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -Command \"" + ps + "\"");
                psi.UseShellExecute = true;
                psi.Verb = "runas";
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                System.Diagnostics.Process.Start(psi);
                status.Text = "已请求重启（请在 UAC 点“是”），约 2 秒后生效";
            }
            catch (Exception ex)
            {
                status.Text = "重启失败（可能取消了 UAC）：" + ex.Message;
            }
        }

        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try
            {
                int dark = T.IsDark ? 1 : 0;
                DwmSetWindowAttribute(Handle, 20, ref dark, sizeof(int));
            }
            catch { }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
