using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace Stronghold.AppDashboard.Api.Services;

public interface IPdfGeneratorService
{
    Task<byte[]> GeneratePdfAsync(string html, bool landscape = false, CancellationToken ct = default);
}

/// <summary>
/// Singleton service that wraps PuppeteerSharp / headless Chromium.
/// Chromium is downloaded lazily on first call (~300 MB, cached locally).
/// The browser instance is reused across requests; one new page is opened
/// per PDF job and closed immediately after.
/// </summary>
public sealed class PdfGeneratorService : IPdfGeneratorService, IAsyncDisposable
{
    private IBrowser? _browser;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogger<PdfGeneratorService> _logger;

    public PdfGeneratorService(ILogger<PdfGeneratorService> logger)
    {
        _logger = logger;
    }

    private async Task<IBrowser> EnsureBrowserAsync(CancellationToken ct)
    {
        if (_browser != null) return _browser;

        await _lock.WaitAsync(ct);
        try
        {
            if (_browser != null) return _browser;

            _logger.LogInformation("PdfGenerator: downloading Chromium revision…");
            var fetcher = new BrowserFetcher();
            await fetcher.DownloadAsync();
            _logger.LogInformation("PdfGenerator: Chromium download complete. Launching browser…");

            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu",
                    "--font-render-hinting=none",
                },
            });

            _logger.LogInformation("PdfGenerator: browser ready.");
            return _browser;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<byte[]> GeneratePdfAsync(string html, bool landscape = false, CancellationToken ct = default)
    {
        var browser = await EnsureBrowserAsync(ct);
        var page    = await browser.NewPageAsync();

        try
        {
            await page.SetContentAsync(html, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
                Timeout   = 30_000,
            });

            return await page.PdfDataAsync(new PdfOptions
            {
                Format           = PaperFormat.A4,
                Landscape        = landscape,
                PrintBackground  = true,
                MarginOptions    = new MarginOptions
                {
                    Top    = "0.45in",
                    Bottom = "0.45in",
                    Left   = "0.5in",
                    Right  = "0.5in",
                },
            });
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }
    }
}
