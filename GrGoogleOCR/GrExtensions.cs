using System.IO.Compression;
using System.Text.Json;

namespace GrGoogleOCR;

public static class GrExtensions {

    public static readonly char[] LineEndingChars = "\r\n".ToCharArray();

    private static readonly JsonSerializerOptions JsonSerializerOptions = new() {
        WriteIndented = true
    };

    public static string ExtractText(this JsonDocument jsonDocument) =>
        jsonDocument.RootElement.GetProperty("text").GetString() ?? "";

    public static string ToJsonString(this JsonDocument json) => JsonSerializer.Serialize(
        json.RootElement, JsonSerializerOptions
    );

    public static string RemoveChars(this string input, char[] chrs) {

        if (string.IsNullOrEmpty(input)) return string.Empty;

        return chrs.Length > 0
            ? new string(input.Where(chr => !chrs.Contains(chr)).ToArray()).Trim()
            : input;
    }

    public static async Task<bool> WriteToDiskAsync(this string jsonString, string fileName, bool isZip = true) {

        if (isZip) {
            string zipPath = Path.ChangeExtension(fileName, ".zip");

            await using FileStream zipStream = new(zipPath, FileMode.Create);
            using ZipArchive archive = new(zipStream, ZipArchiveMode.Create);

            ZipArchiveEntry zipEntry = archive.CreateEntry(fileName + ".json", CompressionLevel.SmallestSize);
            await using Stream entryStream = zipEntry.Open();
            await using StreamWriter writer = new(entryStream);
            await writer.WriteAsync(jsonString);
        }
        else {
            await File.WriteAllTextAsync(fileName, jsonString);
        }

        return true;
    }
}
