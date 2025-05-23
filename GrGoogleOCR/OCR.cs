

using System.Drawing.Imaging;
using System.Text.Json;
using Google.Cloud.DocumentAI.V1;
using Google.Protobuf;

namespace GrGoogleOCR;

public partial class MainForm {

    private async Task<JsonDocument?> GrOcr(byte[] pdfBytes, GrOcrSettings settings) {

        RawDocument rawDocument = new() {
            Content = ByteString.CopyFrom(pdfBytes),
            MimeType = "application/pdf"
        };

        return await GOcr(rawDocument, settings);
    }

    private async Task<JsonDocument?> GrOcr(Image image, GrOcrSettings settings) {
        MemoryStream imageStream = new();
        image.Save(imageStream, ImageFormat.Jpeg);
        imageStream.Position = 0;

        RawDocument rawDocument = new() {
            Content = await ByteString.FromStreamAsync(imageStream),
            MimeType = "image/jpeg"
        };

        return await GOcr(rawDocument, settings);
    }

    private async Task<JsonDocument?> GOcr(RawDocument rawDocument, GrOcrSettings settings) {

        try {

            ProcessRequest request = new() {
                Name = settings.Route,
                RawDocument = rawDocument,
                ImagelessMode = true,
                ProcessOptions = new() {
                    OcrConfig = new() {
                        EnableImageQualityScores = true,
                        EnableSymbol = settings.OcrMode == OcrMode.Symbols,
                        PremiumFeatures = new() { ComputeStyleInfo = settings.IsStyleInfoWanted },
                        Hints = new()
                    }
                }
            };

            // Add language hint
            request.ProcessOptions.OcrConfig.Hints.LanguageHints.Add(settings.OcrLanguage);

            // Make the request
            ProcessResponse? response = await _ocrClient.ProcessDocumentAsync(request);

            Document? document = response.Document;
            if (document is null) return null;

            document.Pages[0].Image = null;

            // Convert ByteString to a UTF-8 JSON string
            string jsonString = document.ToString();

            // Parse it into a JsonDocument
            return JsonDocument.Parse(jsonString);
        }
        catch (Exception ex) {
            return null;
        }
    }
}