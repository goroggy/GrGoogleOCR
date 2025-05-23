using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Text.Json;
using static GrGoogleOCR.GrExtensions;

namespace GrGoogleOCR;

public sealed class PdfOcrPageRebuilder {

    private GrOcrSettings _settings = new();
    private string _fullText = "";
    private float _dpiX = 150f;
    private float _dpiY = 150f;

    public bool RebuildPageFromOcr(JsonDocument pageData, PdfPage page, GrOcrSettings settings) {

        _settings = settings;
        _dpiX = settings.DpiX;
        _dpiY = settings.DpiY;

        // Get XGraphics object for drawing on the page
        // Draw in the background first, then content, then foreground.
        // We draw in Foreground here.
        using XGraphics graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

        _fullText = pageData.ExtractText();
        if (string.IsNullOrWhiteSpace(_fullText)) return false;

        if (!pageData.RootElement.TryGetProperty("pages", out JsonElement pages) || pages.GetArrayLength() == 0)
            return false;

        JsonElement firstPage = pages[0];

        JsonElement elements = GetOcrElements(firstPage, settings.OcrMode);
        if (elements.ValueKind == JsonValueKind.Undefined) return false;

        List<JsonElement> textElements = SortElements(elements);

        foreach (JsonElement element in textElements.Where(CheckOrientation))
            DrawTextElement(element, page, graphics);

        return true;
    }

    private static JsonElement GetOcrElements(JsonElement firstPage, OcrMode mode) {
        return mode switch {
            OcrMode.Lines => firstPage.TryGetProperty("lines", out JsonElement linesEl) ? linesEl : default,
            OcrMode.Tokens => firstPage.TryGetProperty("tokens", out JsonElement tokensEl) ? tokensEl : default,
            OcrMode.Symbols => firstPage.TryGetProperty("symbols", out JsonElement symbolsEl) ? symbolsEl : default,
            _ => default
        };
    }

    private static List<JsonElement> SortElements(JsonElement elements) {
        List<JsonElement> textElements = [.. elements.EnumerateArray()];

        textElements.Sort((a, b) => {
                (float ax, float ay) = GetTopLeft(a);
                (float bx, float by) = GetTopLeft(b);
                int yCompare = ay.CompareTo(by);
                return yCompare == 0 ? ax.CompareTo(bx) : yCompare;
            }
        );

        return textElements;
    }

    private void DrawTextElement(JsonElement element, PdfPage page, XGraphics graphics) {
        string text = ExtractOcrSubstring(element, _fullText)
            .RemoveChars(LineEndingChars); // Remove newlines for drawing

        if (string.IsNullOrWhiteSpace(text)) return;

        JsonElement boundingPoly = element.GetProperty("layout").GetProperty("boundingPoly");
        bool isNormalized = boundingPoly.TryGetProperty("normalizedVertices", out JsonElement vertices);
        if (!isNormalized) boundingPoly.TryGetProperty("vertices", out vertices);

        (float pdfX, float pdfY, float boxWidth, float boxHeight) =
            CalculateBoxDimensions(vertices, isNormalized, page);

        if (boxWidth <= 0 || boxHeight <= 0) return; // Skip if dimensions are invalid

        float fontSize = GetFontSize(element, boxHeight);
        XFont font = GetPdfFont(element, fontSize);

        // --- Character Spacing & Drawing (Simplified for PdfSharpCore) ---
        // PdfSharpCore doesn't support character spacing directly.
        // We will draw the string at the location. We can try to fit it,
        // but it won't stretch like with character spacing.
        // We'll draw at (pdfX, pdfY + boxHeight) because PDF uses bottom-left for text usually,
        // but XGraphics uses top-left, so we draw at (pdfX, pdfY). Let's draw at top-left.

        XRect rect = new(
            pdfX, pdfY, boxWidth,
            boxHeight
        );

        // Measure string width
        XSize measuredSize = graphics.MeasureString(text, font);

        // --- Transparency ---
        // XGraphics doesn't handle transparency easily. We'll draw in Red or not.
        // If IsTextVisible is false, we won't draw text. This means NO SEARCHABLE LAYER if hidden.
        // TODO: For a true invisible searchable layer, a different technique is needed.
        if (_settings.IsTextVisible) {
            // Try to scale font or use DrawString in rect to fit, but it's tricky.
            // Simplest: Draw at start, let it flow.
            // We use a Red brush.
            graphics.DrawString(
                text, font, XBrushes.Red,
                rect, XStringFormats.TopLeft
            );
        }

        // Draw bounding box if needed
        if (_settings.IsTextBoxVisible) {
            XPen redPen = new(XColors.Red, 0.05);
            graphics.DrawRectangle(redPen, rect);
        }
    }

    private (float pdfX, float pdfY, float width, float height) CalculateBoxDimensions(
        JsonElement vertices, bool isNormalized, PdfPage page) {
        float pdfX = 0, pdfY = 0, width = 0, height = 0;

        if (vertices.ValueKind == JsonValueKind.Undefined || vertices.GetArrayLength() < 4)
            return (pdfX, pdfY, width, height);

        float x0 = vertices[0].GetProperty("x").GetSingle();
        float y0 = vertices[0].GetProperty("y").GetSingle();
        float x1 = vertices[1].GetProperty("x").GetSingle();
        float y1 = vertices[1].GetProperty("y").GetSingle();
        float x2 = vertices[2].GetProperty("x").GetSingle();
        float y2 = vertices[2].GetProperty("y").GetSingle();
        float x3 = vertices[3].GetProperty("x").GetSingle();
        float y3 = vertices[3].GetProperty("y").GetSingle();

        // Use min/max to handle potentially non-rectangular or rotated boxes simply
        float minX = Math.Min(Math.Min(x0, x1), Math.Min(x2, x3));
        float minY = Math.Min(Math.Min(y0, y1), Math.Min(y2, y3));
        float maxX = Math.Max(Math.Max(x0, x1), Math.Max(x2, x3));
        float maxY = Math.Max(Math.Max(y0, y1), Math.Max(y2, y3));

        if (isNormalized) {
            // Assuming page.Width/Height are in points (XUnit)
            pdfX = minX * (float)page.Width.Point;
            pdfY = minY * (float)page.Height.Point;
            width = (maxX - minX) * (float)page.Width.Point;
            height = (maxY - minY) * (float)page.Height.Point;
        }
        else {
            // Convert from pixels to points (1 point = 1/72 inch)
            pdfX = minX * 72f / _dpiX;
            pdfY = minY * 72f / _dpiY;
            width = (maxX - minX) * 72f / _dpiX;
            height = (maxY - minY) * 72f / _dpiY;
        }

        return (pdfX, pdfY, width, height);
    }

    private static (float X, float Y) GetTopLeft(JsonElement el) {
        // This method seems okay, just need to ensure it finds either vertices
        // or normalizedVertices and returns the top-left (usually 0th index).
        // (Implementation remains mostly the same)
        if (!el.TryGetProperty("layout", out JsonElement layout) ||
            !layout.TryGetProperty("boundingPoly", out JsonElement bpoly)) {
            return (0, 0);
        }

        if (bpoly.TryGetProperty("normalizedVertices", out JsonElement nVertices) && nVertices.GetArrayLength() > 0) {
            float nx = nVertices[0].GetProperty("x").GetSingle();
            float ny = nVertices[0].GetProperty("y").GetSingle();
            return (nx, ny);
        }

        if (bpoly.TryGetProperty("vertices", out JsonElement vertices) && vertices.GetArrayLength() > 0) {
            float x = vertices[0].GetProperty("x").GetSingle();
            float y = vertices[0].GetProperty("y").GetSingle();
            return (x, y);
        }

        return (0, 0);
    }

    // Character spacing is omitted due to PdfSharpCore limitations.

    private static string ExtractOcrSubstring(JsonElement el, string fullText) {
        // This method should work as-is.
        if (!el.TryGetProperty("layout", out JsonElement layout) ||
            !layout.TryGetProperty("textAnchor", out JsonElement anchor) ||
            !anchor.TryGetProperty("textSegments", out JsonElement segments) ||
            segments.GetArrayLength() == 0)
            return "";

        JsonElement segment = segments[0];
        int start = segment.TryGetProperty("startIndex", out JsonElement s) ? int.Parse(s.GetString() ?? "0") : 0;
        int end = segment.TryGetProperty("endIndex", out JsonElement e) ? int.Parse(e.GetString() ?? "0") : 0;

        return (start < end && end <= fullText.Length) ? fullText.Substring(start, end - start) : "";
    }

    private static XFont GetPdfFont(JsonElement el, float fontSize) {

        // PdfSharpCore uses XFont.
        bool isBold = false;
        bool isItalic = false;

        // Note: Google's styleInfo might not be available/enabled.
        // Relying on it might be fragile. Using Times as a fallback.
        if (el.TryGetProperty("textStyle", out JsonElement textStyle)) {
            isBold = textStyle.TryGetProperty("bold", out JsonElement bold) && bold.GetBoolean();
            isItalic = textStyle.TryGetProperty("italic", out JsonElement italic) && italic.GetBoolean();
        }

        XFontStyle fontStyle = XFontStyle.Regular;

        if (isBold && isItalic) fontStyle = XFontStyle.BoldItalic;
        else if (isBold) fontStyle = XFontStyle.Bold;
        else if (isItalic) fontStyle = XFontStyle.Italic;

        // Using Times New Roman as a base. Ensure font handling/embedding is set up
        // if you need specific or Unicode fonts. PdfSharpCore needs a font resolver
        // for fonts not built-in, especially on non-Windows systems.
        try {
            return new XFont(
                "Times New Roman", fontSize, fontStyle,
                XPdfFontOptions.UnicodeDefault
            );
        }
        catch {
            // Fallback
            return new XFont(
                "Arial", fontSize, fontStyle,
                XPdfFontOptions.UnicodeDefault
            );
        }
    }

    // GetBoundingBoxWidth/Height are effectively replaced by CalculateBoxDimensions

    private static bool CheckOrientation(JsonElement element) {
        // This method should work as-is.
        if (!element.TryGetProperty("layout", out JsonElement layout)) return true;
        if (!layout.TryGetProperty("orientation", out JsonElement orientationEl)) return true;
        return orientationEl.GetString() == "PAGE_UP";
    }

    private static float GetFontSize(JsonElement el, float boxHeightPt) {
        // This logic needs adaptation for XFont and XGraphics.MeasureString.
        // It's an approximation. A simpler approach is often just `boxHeightPt * 0.8f` or similar.
        // Let's try to keep it similar but use XFont.
        if (boxHeightPt <= 0) return 1; // Avoid zero/negative size

        float initialFontSize = boxHeightPt * 0.9f;
        if (initialFontSize < 1) initialFontSize = 1; // Min font size

        try {
            XFont testFont = GetPdfFont(el, initialFontSize);
            // Measuring height isn't as direct as Syncfusion. We can use GetCellAscent/Descent.
            // A simpler, often good enough, approximation:
            return initialFontSize; // Use 90% and live with it, or implement complex measurement.
        }
        catch {
            return boxHeightPt * 0.8f; // Fallback
        }
    }
}