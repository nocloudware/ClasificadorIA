using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NoCloudware.UI.Core.Services;

public class UpdateInfo
{
    public string TagName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public bool IsNewerVersion { get; set; }
    public DateTime PublishedDate { get; set; }
}

public class UpdateService
{
    private readonly string _repoOwner;
    private readonly string _repoName;

    public UpdateService(string repoOwner, string repoName)
    {
        _repoOwner = repoOwner;
        _repoName = repoName;
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync(string currentVersion)
    {
        try
        {
            string apiUrl = $"https://api.github.com/repos/{_repoOwner}/{_repoName}/releases/latest";

            using var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };
            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"{_repoName}/1.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");

            using var response = await client.GetAsync(apiUrl);
            if ((int)response.StatusCode == 404) return null;
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var release = JsonDocument.Parse(json).RootElement;

            var tagName = release.GetProperty("tag_name").GetString() ?? "0.0.0";
            var version = tagName.TrimStart('v');
            var downloadUrl = release.GetProperty("html_url").GetString() ?? "";
            var releaseNotes = release.GetProperty("body").GetString() ?? "";
            var publishedDate = release.GetProperty("published_at").GetDateTime();

            return new UpdateInfo
            {
                TagName = tagName,
                Version = version,
                DownloadUrl = downloadUrl,
                ReleaseNotes = releaseNotes,
                IsNewerVersion = IsNewerThan(version, currentVersion),
                PublishedDate = publishedDate
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking updates: {ex.Message}");
            return null;
        }
    }

    private static bool IsNewerThan(string v1, string v2)
    {
        try { return Version.Parse(v1) > Version.Parse(v2); }
        catch { return string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase) > 0; }
    }
}
