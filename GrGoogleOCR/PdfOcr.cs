using System.Text.Json;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using static Gr.SyncfusionPdfServices;

namespace GrGoogleOCR;

public partial class MainForm {

    private async Task OcrPdf() {
        try {

            string pdfFileName = _grOcrSettings.FilePath.Trim();
            if (string.IsNullOrEmpty(pdfFileName) || !File.Exists(pdfFileName)) return;

            string fileNameStem = Path.GetFileNameWithoutExtension(pdfFileName);
            string dir = Path.GetDirectoryName(pdfFileName) ?? "";
            string ocrPdfFileName = Path.Combine(dir, fileNameStem + "_ocr.pdf");

            using PdfLoadedDocument pdf = new(pdfFileName);
            List<string> pdfPagePaths = [];

            PdfOcrPageRebuilder pageBuilder = new();
            string txt = "";

            for (int i = 0; i < pdf.Pages.Count; i++) {

                JsonDocument? ocrJson = null;
                string jsonFileName = Path.Combine(dir, $"{fileNameStem}_page_{i:D3}_{_grOcrSettings.OcrMode}.json");

                if (File.Exists(jsonFileName)) {
                    ocrJson = JsonDocument.Parse(await File.ReadAllTextAsync(jsonFileName));
                }
                else {
                    byte[] pageBytes = pdf.PageToByteArray(i);
                    ocrJson = await GrOcr(pageBytes);
                    if (ocrJson is null) continue;
                    await ocrJson.ToJsonString().WriteToDiskAsync(jsonFileName, false);
                }

                pageBuilder.RebuildPageFromOcr(ocrJson, pdf.Pages[i], _grOcrSettings);

                string pdfPagePath = Path.ChangeExtension(jsonFileName, ".pdf");
                pdfPagePaths.Add(pdfPagePath);
                SavePdfPage(pdf, i, pdfPagePath);
                await ShowPdfPage(pdfPagePath);

                string pageTxt = ocrJson.ExtractText();
                txt += pageTxt + Environment.NewLine + Environment.NewLine;
                await File.WriteAllTextAsync(Path.ChangeExtension(jsonFileName, ".txt"), pageTxt);

                PdfPagesJoin(pdfPagePaths).CompressSavePdf(ocrPdfFileName);
                // pdf.Save(outputPdfPath ); destroyed text layer 

                await File.WriteAllTextAsync(Path.ChangeExtension(ocrPdfFileName, ".txt"), txt);
            }
        }
        catch (Exception ex) {
            TbError.Text = ex.Message;
        }
    }

    private static void SavePdfPage(PdfLoadedDocument? pdf, int pageNo, string pagePath) {
        if (pdf is null) return;

        using PdfDocument singlePageDoc = new();
        singlePageDoc.Compression = PdfCompressionLevel.AboveNormal;
        singlePageDoc.ImportPage(pdf, pageNo);
        singlePageDoc.Save(pagePath);
        singlePageDoc.Save(pagePath);
        singlePageDoc.Close(true);
    }

    private async Task ShowPdfPage(string pagePath) {

        try {
            if (!File.Exists(pagePath)) return;

            PdfViewer.Load(pagePath);
            decimal secs = 0;

            while (!PdfViewer.IsDocumentLoaded && secs < 3) {
                secs += 0.1m;
                await Task.Delay(100);
            }

            PdfViewer.Invalidate();
            await Task.Delay(1000);
        }
        catch (Exception ex) {
            TbError.Text = ex.Message;
        }
    }
}
