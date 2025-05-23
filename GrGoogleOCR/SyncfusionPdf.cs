using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Graphics;
using System.Text.Json;

namespace Gr;

public static class SyncfusionPdfServices {

    private static PdfFontFamily GetFontFamily(JsonElement el) {

        if (!el.TryGetProperty("styleInfo", out JsonElement styleInfo))
            return PdfFontFamily.TimesRoman; // Default fallback

        if (!styleInfo.TryGetProperty("fontType", out JsonElement ft))
            return PdfFontFamily.TimesRoman; // Default fallback

        string fontType = (ft.GetString() ?? "").ToUpperInvariant();

        return fontType switch {
            "SANS_SERIF" => PdfFontFamily.TimesRoman,
            "SERIF" => PdfFontFamily.TimesRoman,
            "MONOSPACE" => PdfFontFamily.TimesRoman,
            _ => PdfFontFamily.TimesRoman // Default fallback
        };
    }

    public static byte[] ToByteArray(this PdfLoadedDocument origDocument, int pageIndex) {
        using MemoryStream ms = new();

        // Create a new PDF document for the single page.
        PdfDocument singlePageDoc = new();

        // Import the desired page (pageIndex is zero-based)
        singlePageDoc.ImportPage(origDocument, pageIndex);

        // Save the single-page document into the memory stream.
        singlePageDoc.Save(ms);
        ms.Position = 0;

        // Get the byte array.
        byte[] pdfPageBytes = ms.ToArray();

        // Close the document.
        singlePageDoc.Close(true);

        // Now you can convert to ByteString if needed:
        // ByteString pdfPageByteString = ByteString.CopyFrom(pdfPageBytes);

        return pdfPageBytes;
    }

    public static PdfDocument PdfPagesJoin(List<string> pagePaths) {
        PdfDocument doc = new() {
            Compression = PdfCompressionLevel.AboveNormal
        };

        foreach (PdfLoadedDocument singlePageDocument in pagePaths.Select(pagePath => new PdfLoadedDocument(pagePath)))
            doc.ImportPage(singlePageDocument, 0);

        return doc;
    }

    public static void CompressSavePdf(this PdfDocument document, string pdfPath) {

        using MemoryStream ms = new();
        document.Save(ms);
        // Load the existing PDF document with text layer
        using PdfLoadedDocument loadedDocument = new(ms);

        PdfCompressionOptions options = new() {
            // Set compression level to maximum
            CompressImages = true,
            ImageQuality = 10,
            OptimizeFont = true,
            OptimizePageContents = true,
            RemoveMetadata = true
        };

        loadedDocument.CompressionOptions = options;
        loadedDocument.Compression = PdfCompressionLevel.AboveNormal;
        loadedDocument.FileStructure.IncrementalUpdate = false;

        // 6. Save with compression settings
        loadedDocument.Save(pdfPath);

        // Close the document
        loadedDocument.Close(true);

    }
}
