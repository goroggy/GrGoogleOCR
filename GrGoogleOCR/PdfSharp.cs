using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace Gr;

public static class PdfSharpServices {

    /// <summary>
    /// Extracts a single page from a PDF document into a byte array.
    /// </summary>
    public static byte[] PageToByteArray(string originalPdfPath, int pageIndex) {
        using MemoryStream ms = new();
        using PdfDocument originalDoc = PdfReader.Open(originalPdfPath, PdfDocumentOpenMode.Import);

        if (pageIndex < 0 || pageIndex >= originalDoc.PageCount) {
            throw new ArgumentOutOfRangeException(nameof(pageIndex), "Page index is out of bounds.");
        }

        using PdfDocument singlePageDoc = new();
        singlePageDoc.AddPage(originalDoc.Pages[pageIndex]);
        singlePageDoc.Save(ms, false); // Save without closing stream

        return ms.ToArray();
    }

    /// <summary>
    /// Extracts a single page from a PDF document into a byte array.
    /// </summary>
    public static byte[] PageToByteArray(byte[] originalPdfBytes, int pageIndex) {
        using MemoryStream inputMs = new(originalPdfBytes);
        using MemoryStream outputMs = new();
        using PdfDocument originalDoc = PdfReader.Open(inputMs, PdfDocumentOpenMode.Import);

        if (pageIndex < 0 || pageIndex >= originalDoc.PageCount) {
            throw new ArgumentOutOfRangeException(nameof(pageIndex), "Page index is out of bounds.");
        }

        using PdfDocument singlePageDoc = new();
        singlePageDoc.AddPage(originalDoc.Pages[pageIndex]);
        singlePageDoc.Save(outputMs, false); // Save without closing stream

        return outputMs.ToArray();
    }

    /// <summary>
    /// Merges multiple single-page PDF files into one document.
    /// </summary>
    public static PdfDocument PdfPagesJoin(List<string> pagePaths) {
        PdfDocument outputDoc = new();

        foreach (string pagePath in pagePaths) {
            try {
                using PdfDocument inputDoc = PdfReader.Open(pagePath, PdfDocumentOpenMode.Import);

                foreach (PdfPage page in inputDoc.Pages) {
                    outputDoc.AddPage(page);
                }
            }
            catch (Exception ex) {
                // Handle potential errors opening or importing pages
                Console.WriteLine($"Error processing {pagePath}: {ex.Message}");
                // Consider adding more robust error handling or logging.
            }
        }

        return outputDoc;
    }

    /// <summary>
    /// Saves a PdfDocument with some basic options. PdfSharpCore's compression
    /// is less configurable than Syncfusion's.
    /// </summary>
    public static void SavePdf(this PdfDocument document, string pdfPath) {
        // PdfSharpCore applies compression by default (FlateDecode).
        // More advanced options (like image re-compression) aren't
        // as straightforward as in Syncfusion.
        document.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;
        document.Save(pdfPath);
    }
}
