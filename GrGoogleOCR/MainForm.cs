using Google.Cloud.DocumentAI.V1;
using Google.Cloud.Location;
using GrGoogleOCR.Properties;
using System.Text.Json;

namespace GrGoogleOCR;

public partial class MainForm : Form {

    private readonly GrOcrSettings _grOcrSettings = new();

    private DocumentProcessorServiceClient _ocrClient;

    public MainForm() {
        InitializeComponent();

        _grOcrSettings = JsonSerializer.Deserialize<GrOcrSettings>(Settings.Default.OcrSettings) ??
                         new GrOcrSettings();

        PropGridSettings.SelectedObject = _grOcrSettings;

    }

    private async void BtnGo_Click(object sender, EventArgs e) {

        try {

            Settings.Default.OcrSettings = JsonSerializer.Serialize(_grOcrSettings);
            Settings.Default.Save();

            if (string.IsNullOrEmpty(_grOcrSettings.FilePath)) return;

            string projectId = _grOcrSettings.ServiceProject; // Your project ID
            string location = _grOcrSettings.ServiceLocation; // Your location (e.g., "us" or "eu")
            string processorId = _grOcrSettings.ServiceProcessor; // Your processor ID
            _grOcrSettings.Route = $"projects/{projectId}/locations/{location}/processors/{processorId}";

            string endPoint =
                $"https://{location}-documentai.googleapis.com/v1/{_grOcrSettings.Route}:process";

            _ocrClient = await new DocumentProcessorServiceClientBuilder {
                Endpoint = endPoint
            }.BuildAsync();

            await OcrPdf();
        }
        catch (Exception ex) {
            TbError.Text = ex.Message;
        }
    }
}

public class GrOcrSettings {
    public string FilePath { get; set; } = string.Empty;
    public bool IsStyleInfoWanted { get; set; } = false;
    public OcrMode OcrMode { get; set; } = OcrMode.Tokens;
    public bool IsTextVisible { get; set; } = false;
    public bool IsTextBoxVisible { get; set; } = false;
    public string OcrLanguage { get; set; } = "en";
    public string Route { get; set; } = "";
    public float DpiX { get; set; } = 150;
    public float DpiY { get; set; } = 150;
    public string ServiceLocation { get; set; } = "eu";
    public string ServiceProcessor { get; set; } = "";
    public string ServiceProject { get; set; } = "";
}

public enum OcrMode {
    //Blocks,
    //Paragraphs,
    Lines, // Uses line-based OCR output (faster, but no font info)
    Tokens, // Uses word-level OCR tokens (preserves font info)
    Symbols // Uses character-level symbols (maximum detail, slower)
}
