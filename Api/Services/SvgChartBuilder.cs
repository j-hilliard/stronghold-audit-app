namespace Stronghold.AppDashboard.Api.Services;

/// <summary>
/// Generates lightweight, self-contained SVG charts as HTML strings.
/// No JavaScript runtime required — renders perfectly in headless Chromium / PDF.
/// </summary>
public static class SvgChartBuilder
{
    // ── Bar Chart ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Renders a vertical bar chart.
    /// </summary>
    /// <param name="data">Series of (label, value) pairs.</param>
    /// <param name="maxValue">Y-axis maximum. 0 = auto-scale from data.</param>
    /// <param name="colorHex">Bar fill color in #RRGGBB.</param>
    public static string BarChart(
        IReadOnlyList<(string Label, double Value)> data,
        int width  = 520,
        int height = 220,
        double maxValue = 0,
        string colorHex = "#2563eb")
    {
        if (data.Count == 0)
            return EmptyChart(width, height, "No data");

        var max = maxValue > 0 ? maxValue : (data.Max(d => d.Value) * 1.15);
        if (max == 0) max = 1;

        int padLeft = 36, padRight = 12, padTop = 10, padBottom = 40;
        int chartW = width - padLeft - padRight;
        int chartH = height - padTop - padBottom;

        var barW = Math.Max(6, (int)(chartW / (data.Count * 1.6)));
        var gap  = (chartW - data.Count * barW) / (data.Count + 1);

        var bars = new System.Text.StringBuilder();
        var labels = new System.Text.StringBuilder();

        for (int i = 0; i < data.Count; i++)
        {
            var (label, value) = data[i];
            double pct  = value / max;
            int    bh   = (int)(chartH * pct);
            int    x    = padLeft + gap + i * (barW + gap);
            int    y    = padTop + chartH - bh;

            bars.Append($@"<rect x=""{x}"" y=""{y}"" width=""{barW}"" height=""{bh}"" fill=""{colorHex}"" rx=""2""/>");

            if (value > 0)
                bars.Append($@"<text x=""{x + barW / 2}"" y=""{y - 4}"" text-anchor=""middle"" font-size=""9"" fill=""#94a3b8"">{value:F1}</text>");

            var shortLabel = label.Length > 7 ? label[..7] + "…" : label;
            labels.Append($@"<text x=""{x + barW / 2}"" y=""{padTop + chartH + 14}"" text-anchor=""middle"" font-size=""9"" fill=""#94a3b8"">{Encode(shortLabel)}</text>");
        }

        return Wrap(width, height, colorHex, $@"
            {GridLines(padLeft, padTop, chartW, chartH, max)}
            {bars}
            {labels}
            {XAxis(padLeft, padTop + chartH, chartW)}
            {YAxis(padLeft, padTop, chartH, max)}");
    }

    // ── Grouped Bar Chart ──────────────────────────────────────────────────────

    /// <summary>
    /// Renders a grouped bar chart comparing multiple series side-by-side.
    /// </summary>
    /// <param name="groups">Each group is a category label + series values.</param>
    /// <param name="seriesLabels">Name for each series.</param>
    /// <param name="colors">Fill color per series.</param>
    public static string GroupedBarChart(
        IReadOnlyList<(string GroupLabel, IReadOnlyList<double> Values)> groups,
        IReadOnlyList<string> seriesLabels,
        IReadOnlyList<string>? colors = null,
        int width  = 620,
        int height = 260,
        double maxValue = 0)
    {
        if (groups.Count == 0 || seriesLabels.Count == 0)
            return EmptyChart(width, height, "No data");

        var palette = colors ?? DefaultPalette;
        int seriesCount = seriesLabels.Count;

        var allValues = groups.SelectMany(g => g.Values).Where(v => v > 0).ToList();
        var max = maxValue > 0 ? maxValue : (allValues.Any() ? allValues.Max() * 1.15 : 1);

        int padLeft = 40, padRight = 120, padTop = 10, padBottom = 44;
        int chartW = width - padLeft - padRight;
        int chartH = height - padTop - padBottom;

        int groupBarW  = Math.Max(seriesCount * 6, (int)(chartW / (groups.Count * 1.4)));
        int singleBarW = Math.Max(4, (groupBarW - 2 * (seriesCount - 1)) / seriesCount);
        int groupGap   = (chartW - groups.Count * groupBarW) / (groups.Count + 1);

        var bars   = new System.Text.StringBuilder();
        var labels = new System.Text.StringBuilder();

        for (int gi = 0; gi < groups.Count; gi++)
        {
            var (groupLabel, values) = groups[gi];
            int groupX = padLeft + groupGap + gi * (groupBarW + groupGap);

            for (int si = 0; si < Math.Min(seriesCount, values.Count); si++)
            {
                double pct = values[si] / max;
                int bh  = Math.Max(1, (int)(chartH * pct));
                int bx  = groupX + si * (singleBarW + 2);
                int by  = padTop + chartH - bh;
                var col = si < palette.Count ? palette[si] : "#6366f1";

                bars.Append($@"<rect x=""{bx}"" y=""{by}"" width=""{singleBarW}"" height=""{bh}"" fill=""{col}"" rx=""2""/>");
            }

            var gl = groupLabel.Length > 8 ? groupLabel[..8] + "…" : groupLabel;
            labels.Append($@"<text x=""{groupX + groupBarW / 2}"" y=""{padTop + chartH + 14}"" text-anchor=""middle"" font-size=""9"" fill=""#94a3b8"">{Encode(gl)}</text>");
        }

        // Legend (right side)
        var legend = new System.Text.StringBuilder();
        for (int si = 0; si < seriesLabels.Count; si++)
        {
            var col = si < palette.Count ? palette[si] : "#6366f1";
            int ly  = padTop + si * 16;
            legend.Append($@"<rect x=""{width - padRight + 8}"" y=""{ly + 1}"" width=""10"" height=""10"" fill=""{col}"" rx=""2""/>");
            legend.Append($@"<text x=""{width - padRight + 22}"" y=""{ly + 10}"" font-size=""9"" fill=""#94a3b8"">{Encode(seriesLabels[si])}</text>");
        }

        return Wrap(width, height, palette[0], $@"
            {GridLines(padLeft, padTop, chartW, chartH, max)}
            {bars}
            {labels}
            {legend}
            {XAxis(padLeft, padTop + chartH, chartW)}
            {YAxis(padLeft, padTop, chartH, max)}");
    }

    // ── Line Chart ────────────────────────────────────────────────────────────

    public static string LineChart(
        IReadOnlyList<(string Label, double Value)> data,
        int width  = 520,
        int height = 180,
        double maxValue = 0,
        string colorHex = "#22c55e")
    {
        if (data.Count < 2)
            return EmptyChart(width, height, data.Count == 1 ? BarChart(data, width, height, maxValue, colorHex) : "No data");

        var max = maxValue > 0 ? maxValue : (data.Max(d => d.Value) * 1.15);
        if (max == 0) max = 1;

        int padLeft = 36, padRight = 12, padTop = 10, padBottom = 36;
        int chartW = width - padLeft - padRight;
        int chartH = height - padTop - padBottom;

        double stepX = (double)chartW / (data.Count - 1);

        var points = data.Select((d, i) =>
        {
            double px = padLeft + i * stepX;
            double py = padTop + chartH - (chartH * (d.Value / max));
            return (px, py, d.Label, d.Value);
        }).ToList();

        var polyline = string.Join(" ", points.Select(p => $"{p.px:F1},{p.py:F1}"));
        var areaBottom = padTop + chartH;
        var area = $"M {points[0].px:F1},{areaBottom} " +
                   string.Join(" ", points.Select(p => $"L {p.px:F1},{p.py:F1}")) +
                   $" L {points[^1].px:F1},{areaBottom} Z";

        var dots = new System.Text.StringBuilder();
        var lbls = new System.Text.StringBuilder();

        foreach (var (px, py, lbl, val) in points)
        {
            dots.Append($@"<circle cx=""{px:F1}"" cy=""{py:F1}"" r=""3"" fill=""{colorHex}""/>");
            dots.Append($@"<text x=""{px:F1}"" y=""{py - 6:F1}"" text-anchor=""middle"" font-size=""8"" fill=""#94a3b8"">{val:F1}</text>");
        }

        // x-axis labels (skip if too many)
        int step = Math.Max(1, data.Count / 6);
        for (int i = 0; i < points.Count; i += step)
        {
            var (px, _, lbl, _) = points[i];
            var sl = lbl.Length > 6 ? lbl[..6] : lbl;
            lbls.Append($@"<text x=""{px:F1}"" y=""{padTop + chartH + 14}"" text-anchor=""middle"" font-size=""9"" fill=""#94a3b8"">{Encode(sl)}</text>");
        }

        var areaId = $"grad_{Guid.NewGuid():N[..6]}";
        return Wrap(width, height, colorHex, $@"
            <defs>
                <linearGradient id=""{areaId}"" x1=""0"" y1=""0"" x2=""0"" y2=""1"">
                    <stop offset=""0%"" stop-color=""{colorHex}"" stop-opacity=""0.25""/>
                    <stop offset=""100%"" stop-color=""{colorHex}"" stop-opacity=""0.02""/>
                </linearGradient>
            </defs>
            {GridLines(padLeft, padTop, chartW, chartH, max)}
            <path d=""{area}"" fill=""url(#{areaId})""/>
            <polyline points=""{polyline}"" fill=""none"" stroke=""{colorHex}"" stroke-width=""2""/>
            {dots}
            {lbls}
            {XAxis(padLeft, padTop + chartH, chartW)}
            {YAxis(padLeft, padTop, chartH, max)}");
    }

    // ── Donut / Pie Chart ─────────────────────────────────────────────────────

    public static string DonutChart(
        IReadOnlyList<(string Label, double Value, string Color)> segments,
        int size = 180)
    {
        if (segments.Count == 0) return EmptyChart(size, size, "No data");

        double total = segments.Sum(s => s.Value);
        if (total == 0) return EmptyChart(size, size, "No data");

        int cx = size / 2, cy = size / 2;
        int outerR = size / 2 - 8;
        int innerR = outerR * 6 / 10;

        var paths = new System.Text.StringBuilder();
        double angle = -Math.PI / 2;

        foreach (var (lbl, val, col) in segments)
        {
            double sweep = (val / total) * 2 * Math.PI;
            var (x1, y1) = PolarToCart(cx, cy, outerR, angle);
            var (x2, y2) = PolarToCart(cx, cy, outerR, angle + sweep);
            var (ix1, iy1) = PolarToCart(cx, cy, innerR, angle + sweep);
            var (ix2, iy2) = PolarToCart(cx, cy, innerR, angle);

            int largeArc = sweep > Math.PI ? 1 : 0;
            paths.Append($@"<path d=""M {x1:F1} {y1:F1} A {outerR} {outerR} 0 {largeArc} 1 {x2:F1} {y2:F1} L {ix1:F1} {iy1:F1} A {innerR} {innerR} 0 {largeArc} 0 {ix2:F1} {iy2:F1} Z"" fill=""{col}""/>");
            angle += sweep;
        }

        var totalText = $"{total:F0}";
        return $@"<svg xmlns=""http://www.w3.org/2000/svg"" width=""{size}"" height=""{size}"" viewBox=""0 0 {size} {size}"">
            {paths}
            <text x=""{cx}"" y=""{cy + 4}"" text-anchor=""middle"" font-size=""14"" font-weight=""bold"" fill=""#e2e8f0"">{totalText}</text>
        </svg>";
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private static readonly IReadOnlyList<string> DefaultPalette = new[]
    {
        "#2563eb", "#22c55e", "#f59e0b", "#ef4444", "#8b5cf6", "#06b6d4",
    };

    private static string Wrap(int w, int h, string accentColor, string inner)
    {
        return $@"<svg xmlns=""http://www.w3.org/2000/svg"" width=""{w}"" height=""{h}"" viewBox=""0 0 {w} {h}"" style=""background:transparent;"">{inner}</svg>";
    }

    private static string EmptyChart(int w, int h, string message)
    {
        if (message.StartsWith("<")) return message; // already an SVG fragment
        return $@"<svg xmlns=""http://www.w3.org/2000/svg"" width=""{w}"" height=""{h}"">
            <text x=""{w / 2}"" y=""{h / 2}"" text-anchor=""middle"" fill=""#475569"" font-size=""12"">{Encode(message)}</text></svg>";
    }

    private static string GridLines(int ox, int oy, int w, int h, double max, int steps = 4)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i <= steps; i++)
        {
            double pct = (double)i / steps;
            int y = oy + h - (int)(h * pct);
            double val = max * pct;
            sb.Append($@"<line x1=""{ox}"" y1=""{y}"" x2=""{ox + w}"" y2=""{y}"" stroke=""#334155"" stroke-width=""1""/>");
            sb.Append($@"<text x=""{ox - 4}"" y=""{y + 4}"" text-anchor=""end"" font-size=""8"" fill=""#64748b"">{val:F0}</text>");
        }
        return sb.ToString();
    }

    private static string XAxis(int ox, int y, int w) =>
        $@"<line x1=""{ox}"" y1=""{y}"" x2=""{ox + w}"" y2=""{y}"" stroke=""#475569"" stroke-width=""1""/>";

    private static string YAxis(int ox, int oy, int h, double max) =>
        $@"<line x1=""{ox}"" y1=""{oy}"" x2=""{ox}"" y2=""{oy + h}"" stroke=""#475569"" stroke-width=""1""/>";

    private static (double x, double y) PolarToCart(int cx, int cy, int r, double angle) =>
        (cx + r * Math.Cos(angle), cy + r * Math.Sin(angle));

    private static string Encode(string s) =>
        System.Security.SecurityElement.Escape(s) ?? s;
}
