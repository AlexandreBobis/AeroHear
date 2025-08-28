using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AeroHear.Audio
{
    public class SpotifyAudioHandler
    {
        private readonly HttpClient _httpClient;

        public SpotifyAudioHandler()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Downloads a Spotify preview track to a temporary file for playback
        /// Note: This only works with 30-second previews. Full track streaming would require
        /// additional licensing and premium API access.
        /// </summary>
        public async Task<string> DownloadPreviewTrackAsync(string previewUrl, string trackName)
        {
            if (string.IsNullOrEmpty(previewUrl))
                return null;

            try
            {
                // Create temp directory if it doesn't exist
                var tempDir = Path.Combine(Path.GetTempPath(), "AeroHear", "SpotifyPreviews");
                Directory.CreateDirectory(tempDir);

                // Clean track name for filename
                var safeFileName = string.Join("_", trackName.Split(Path.GetInvalidFileNameChars()));
                var tempFilePath = Path.Combine(tempDir, $"{safeFileName}_{DateTime.Now.Ticks}.mp3");

                // Download the preview
                var response = await _httpClient.GetAsync(previewUrl);
                response.EnsureSuccessStatusCode();

                await using var fileStream = File.Create(tempFilePath);
                await response.Content.CopyToAsync(fileStream);

                return tempFilePath;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Cleans up temporary files older than 1 hour
        /// </summary>
        public void CleanupTempFiles()
        {
            try
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "AeroHear", "SpotifyPreviews");
                if (!Directory.Exists(tempDir))
                    return;

                var files = Directory.GetFiles(tempDir, "*.mp3");
                var cutoffTime = DateTime.Now.AddHours(-1);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffTime)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            // Ignore deletion errors
                        }
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}