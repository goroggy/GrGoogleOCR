using System.Text.Json;
using Gr; // Using our new PdfSharpServices
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace GrGoogleOCR;

public partial class MainForm {
    private async Task OcrPdf() {
        try {
            string pdfFileName = _grOcrSettings.FilePath.Trim();
            if (string.IsNullOrEmpty(pdfFileName) || !File.Exists(pdfFileName)) return;

            string fileNameStem = Path.GetFileNameWithoutExtension(pdfFileName);
            string dir = Path.GetDirectoryName(pdfFileName) ?? "";
            string ocrPdfFileName = Path.Combine(dir, fileNameStem + "_ocr.pdf");

            // Load the original PDF to get page count and process pages
            using PdfDocument originalPdf = PdfReader.Open(pdfFileName, PdfDocumentOpenMode.Import);
            List<string> pdfPagePaths = [];
            string txt = "";

            PdfOcrPageRebuilder pageBuilder = new();

            for (int i = 0; i < originalPdf.Pages.Count; i++) {
                JsonDocument? ocrJson = null;
                string jsonFileName = Path.Combine(dir, $"{fileNameStem}_page_{i:D3}_{_grOcrSettings.OcrMode}.json");
                string pdfPagePath = Path.ChangeExtension(jsonFileName, ".pdf");

                // Get bytes for the single page
                byte[] pageBytes = PdfSharpServices.PageToByteArray(pdfFileName, i);

                // Perform OCR
                if (File.Exists(jsonFileName)) {
                    ocrJson = JsonDocument.Parse(await File.ReadAllTextAsync(jsonFileName));
                }
                else {
                    ocrJson = await GrOcr(pageBytes);
                    if (ocrJson is null) continue;
                    await ocrJson.ToJsonString().WriteToDiskAsync(jsonFileName, false);
                }

                // Create a new single-page document to draw on
                using PdfDocument pageToRebuild = new();
                pageToRebuild.AddPage(originalPdf.Pages[i]); // Add original page
                PdfPage pdfPage = pageToRebuild.Pages[0];

                // Rebuild the page with OCR text layer
                bool rebuilt = pageBuilder.RebuildPageFromOcr(ocrJson, pdfPage, _grOcrSettings);

                if (rebuilt) {
                    // Save the single page with the new text layer
                    pageToRebuild.Save(pdfPagePath);
                    pdfPagePaths.Add(pdfPagePath);

                    // Extract and save text
                    string pageTxt = ocrJson.ExtractText();
                    txt += pageTxt + Environment.NewLine + Environment.NewLine;
                    await File.WriteAllTextAsync(Path.ChangeExtension(jsonFileName, ".txt"), pageTxt);
                }
                else {
                    // If rebuild failed, maybe just save the original page?
                    // Or log an error. For now, we skip adding it.
                    TbError.Text += $"Failed to rebuild page {i}.\n";
                }
            }

            // Join all processed pages and save
            if (pdfPagePaths.Count > 0) {
                using PdfDocument finalDoc = PdfSharpServices.PdfPagesJoin(pdfPagePaths);
                finalDoc.SavePdf(ocrPdfFileName);
                await File.WriteAllTextAsync(Path.ChangeExtension(ocrPdfFileName, ".txt"), txt);
                TbError.Text += $"Successfully saved {ocrPdfFileName}\n";
            }
            else {
                TbError.Text += "No pages were processed successfully.\n";
            }

        }
        catch (Exception ex) {
            TbError.Text = ex.ToString(); // More detail
        }
    }

}