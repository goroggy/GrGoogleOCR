using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System.Text.Json;
using static GrGoogleOCR.GrExtensions;

namespace GrGoogleOCR;

public sealed class PdfOcrPageRebuilder {

    private GrOcrSettings _settings = new();
    private string _fullText = "";
    private float _dpiX = 150f; // Initialize with default DPI
    private float _dpiY = 150f; // Initialize with default DPI

    public bool RebuildPageFromOcr(JsonDocument pageData, PdfPageBase page, GrOcrSettings settings) {

        _settings = settings;
        _dpiX = settings.DpiX;
        _dpiY = settings.DpiY;

        PdfGraphics graphics = page.Graphics;

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

    /// <summary>
    /// Returns OCR elements based on the specified mode.
    /// </summary>
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

    private void DrawTextElement(JsonElement element, PdfPageBase newPage, PdfGraphics graphics) {
        string text = ExtractOcrSubstring(element, _fullText);
        if (string.IsNullOrWhiteSpace(text)) return;

        // Get bounding box coordinates and dimensions
        JsonElement boundingPoly = element.GetProperty("layout").GetProperty("boundingPoly");

        // Get vertices
        bool isNormalized = boundingPoly.TryGetProperty("normalizedVertices", out JsonElement vertices);

        if (!isNormalized)
            boundingPoly.TryGetProperty("vertices", out vertices);

        // Calculate box dimensions with proper scaling
        (float pdfX, float pdfY, float boxWidth, float boxHeight) =
            CalculateBoxDimensions(vertices, isNormalized, newPage);

        // For simplicity, scale font size to about 90% of the box height.
        float fontSize = GetFontSize(element, boxHeight);
        PdfFont font = GetPdfFont(element, fontSize);

        PdfStringFormat format = new() {
            CharacterSpacing = CalculateCharacterSpacing(text, font, boxWidth),
            LineAlignment = PdfVerticalAlignment.Top,
            NoClip = false, // ✅ Let default behavior handle text visibility naturally
            LineLimit = false, // ✅ We don't need extra line-breaking logic
            MeasureTrailingSpaces = false
        };

        graphics.SetTransparency(_settings.IsTextVisible ? 255 : 0);

        graphics.DrawString(
            text, font, PdfBrushes.Red,
            new PointF(pdfX, pdfY), format
        );

        if (!_settings.IsTextBoxVisible) return;

        graphics.SetTransparency(255);
        PdfPen redPen = new(PdfBrushes.Red, 0.05f);

        graphics.DrawRectangle(
            redPen, new RectangleF(
                pdfX, pdfY, boxWidth,
                boxHeight
            )
        );
    }

    private (float pdfX, float pdfY, float width, float height) CalculateBoxDimensions(
        JsonElement vertices, bool isNormalized, PdfPageBase newPage) {

        float pdfX = 0, pdfY = 0, width = 0, height = 0;

        if (vertices.ValueKind == JsonValueKind.Undefined) return (pdfX, pdfY, width, height);
        float x0 = vertices[0].GetProperty("x").GetSingle();
        float y0 = vertices[0].GetProperty("y").GetSingle();
        float x1 = vertices[1].GetProperty("x").GetSingle();
        float y3 = vertices[3].GetProperty("y").GetSingle();

        if (isNormalized) {
            pdfX = x0 * newPage.Size.Width;
            pdfY = y0 * newPage.Size.Height;
            width = (x1 - x0) * newPage.Size.Width;
            height = (y3 - y0) * newPage.Size.Height;
        }
        else {
            pdfX = x0 * 72f / _dpiX;
            pdfY = y0 * 72f / _dpiY;
            width = (x1 - x0) * 72f / _dpiX;
            height = (y3 - y0) * 72f / _dpiY;
        }

        return (pdfX, pdfY, width, height);
    }

    // --- Methods ---

    /// <summary>
    /// Returns the top-left (normalized) coordinates from OCR boundingPoly.
    /// </summary>
    private static (float X, float Y) GetTopLeft(JsonElement el) {

        if (!el.TryGetProperty("layout", out JsonElement layout) ||
            !layout.TryGetProperty("boundingPoly", out JsonElement bpoly) ||
            !bpoly.TryGetProperty("normalizedVertices", out JsonElement vertices)) {

            // Try non-normalized vertices if normalized are not available
            if (!layout.TryGetProperty("boundingPoly", out bpoly) ||
                !bpoly.TryGetProperty("vertices", out vertices)) return (0, 0);

            float x = vertices[0].GetProperty("x").GetSingle();
            float y = vertices[0].GetProperty("y").GetSingle();
            return (x, y);
        }

        // Normalized vertices
        float nx = vertices[0].GetProperty("x").GetSingle();
        float ny = vertices[0].GetProperty("y").GetSingle();
        return (nx, ny);
    }

    /// <summary>
    /// Calculates extra character spacing to fill the OCR box width.
    /// </summary>
    /// <summary>
    /// Iteratively adjusts the character spacing so that the rendered text fits the target box width.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="font">The PdfFont used for drawing.</param>
    /// <param name="boxWidth">The target width in points.</param>
    /// <returns>The final character spacing (in points) that makes the text fit.</returns>
    private static float CalculateCharacterSpacing(string text, PdfFont font, float boxWidth) {
        // Guard against empty or single character text
        if (string.IsNullOrEmpty(text) || text.Length <= 1) return 0;

        // Determine the number of gaps between characters (ignore newlines if necessary)
        int gaps = text.RemoveChars(LineEndingChars).Length - 1;
        if (gaps <= 0) return 0;

        // Create a PdfStringFormat with an initial spacing of 0.
        PdfStringFormat format = new();
        float spacing = 0f;
        format.CharacterSpacing = spacing;

        // Measure the rendered width of the text with the current spacing.
        SizeF measured = font.MeasureString(text, format);
        int iterations = 0;
        const float tolerance = 0.5f; // points tolerance

        // Only adjust if measured width is significantly different
        if (Math.Abs(measured.Width - boxWidth) < tolerance || boxWidth <= 0)
            return 0;

        // Iteratively adjust until the measured width is within tolerance of the boxWidth.
        while (Math.Abs(measured.Width - boxWidth) > tolerance && iterations < 20) {
            // Compute the difference between target and measured width.
            float diff = boxWidth - measured.Width;
            // Adjust spacing per gap.
            spacing += diff / gaps;
            format.CharacterSpacing = spacing;
            measured = font.MeasureString(text, format);
            iterations++;
        }

        return spacing;
    }

    /// <summary>
    /// Extracts the OCR text substring from the global full text using textAnchor.
    /// </summary>
    private static string ExtractOcrSubstring(JsonElement el, string fullText) {

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

    private static PdfFont GetPdfFont(JsonElement el, float fontSize) {

        // Check if the element has any specific font attributes we might want to use
        bool isBold = false;
        bool isItalic = false;

        if (el.TryGetProperty("textStyle", out JsonElement textStyle)) {
            isBold = textStyle.TryGetProperty("bold", out JsonElement bold) && bold.GetBoolean();
            isItalic = textStyle.TryGetProperty("italic", out JsonElement italic) && italic.GetBoolean();
        }

        FontStyle fontStyle = FontStyle.Regular;

        // Apply style variants if needed
        if (isBold && isItalic)
            fontStyle = FontStyle.Italic | FontStyle.Bold;
        else if (isBold)
            fontStyle = FontStyle.Bold;
        else if (isItalic)
            fontStyle = FontStyle.Italic;

        try {
            return new PdfTrueTypeFont(new("Times", fontSize, fontStyle), true);
        }
        catch {
            // Fallback to standard font if no TrueType font is found
            // (this won't handle Unicode properly)
            return new PdfStandardFont(
                PdfFontFamily.TimesRoman, fontSize,
                (isBold ? PdfFontStyle.Bold : PdfFontStyle.Regular) |
                (isItalic ? PdfFontStyle.Italic : PdfFontStyle.Regular)
            );
        }
    }

    private float GetBoundingBoxWidth(JsonElement el) {

        try {
            if (el.TryGetProperty("layout", out JsonElement layout) &&
                layout.TryGetProperty("boundingPoly", out JsonElement bpoly)) {
                // Try normalized vertices first
                if (bpoly.TryGetProperty("normalizedVertices", out JsonElement vertices)) {
                    float x0 = vertices[0].GetProperty("x").GetSingle();
                    float x1 = vertices[1].GetProperty("x").GetSingle();
                    return Math.Abs(x1 - x0) * 72f; // Normalized coordinates, so multiply by page width
                }
                // Fall back to regular vertices
                else if (bpoly.TryGetProperty("vertices", out vertices)) {
                    float x0 = vertices[0].GetProperty("x").GetSingle();
                    float x1 = vertices[1].GetProperty("x").GetSingle();
                    return Math.Abs(x1 - x0) * (72f / _dpiX); // Convert from pixels to points
                }
            }

            return 10f; // Fallback width
        }
        catch {
            return 10f; // Fallback width
        }
    }

    private float GetBoundingBoxHeight(JsonElement el) {

        try {
            if (el.TryGetProperty("layout", out JsonElement layout) &&
                layout.TryGetProperty("boundingPoly", out JsonElement bpoly)) {
                // Try normalized vertices first
                if (bpoly.TryGetProperty("normalizedVertices", out JsonElement vertices)) {
                    float y0 = vertices[0].GetProperty("y").GetSingle();
                    float y3 = vertices[3].GetProperty("y").GetSingle();
                    return Math.Abs(y3 - y0) * 72f; // Normalized coordinates, so multiply by page height
                }
                // Fall back to regular vertices
                else if (bpoly.TryGetProperty("vertices", out vertices)) {
                    float y0 = vertices[0].GetProperty("y").GetSingle();
                    float y3 = vertices[3].GetProperty("y").GetSingle();
                    return Math.Abs(y3 - y0) * (72f / _dpiY); // Convert from pixels to points
                }
            }

            return 10f; // Fallback height
        }
        catch {
            return 10f; // Fallback height
        }
    }

    private static bool CheckOrientation(JsonElement element) {
        if (!element.TryGetProperty("layout", out JsonElement layout))
            return true;

        if (!layout.TryGetProperty("orientation", out JsonElement orientationEl))
            return true;

        return orientationEl.GetString() == "PAGE_UP";
    }

    private static float GetFontSize(JsonElement el, float boxHeightPt) {

        // Start with an initial guess—say, 90% of the OCR bounding box height.
        float initialFontSize = boxHeightPt * 0.9f;

        // Create a test font with the initial guess.
        PdfFont testFont = GetPdfFont(el, initialFontSize);

        // Measure a sample string that includes accents and descenders.
        SizeF testSize = testFont.MeasureString("ÁÉŐÚÜŰgjpqy");
        float measuredHeight = testSize.Height;

        // Compute the ratio of the measured visible height to the initial font size.
        // This ratio tells us, for this font, how tall the rendered glyphs are per point of font size.
        float ratio = measuredHeight / initialFontSize;

        // To have the visible text (with diacritics and descenders) fill the OCR box height,
        // we want: finalFontSize * ratio = boxHeightPt.
        // Solve for finalFontSize:
        float finalFontSize = boxHeightPt / ratio;

        return finalFontSize;
    }
}
