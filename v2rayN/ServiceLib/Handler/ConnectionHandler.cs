namespace ServiceLib.Handler;

public static class ConnectionHandler
{
    private static readonly string _tag = "ConnectionHandler";

    /// <summary>
    /// Runs ping and IP checks.
    /// </summary>
    public static async Task<AvailabilityCheckResult> RunAvailabilityCheck()
    {
        var time = await GetRealPingTimeInfo();
        var ip = time > 0 ? await GetIPInfo() : Global.None;

        return new AvailabilityCheckResult(time, ip);
    }

    /// <summary>
    /// Gets IP information using the default local proxy.
    /// </summary>
    private static async Task<string?> GetIPInfo()
    {
        var webProxy = await GetWebProxy();

        var ipInfo = await GetIPInfo(webProxy);
        return ipInfo?.ToString() ?? Global.None;
    }

    /// <summary>
    /// Measures real ping time using configured test URL.
    /// </summary>
    private static async Task<int> GetRealPingTimeInfo()
    {
        var responseTime = -1;
        try
        {
            var webProxy = await GetWebProxy();

            for (var i = 0; i < 2; i++)
            {
                responseTime = await GetRealPingTime(webProxy);
                if (responseTime > 0)
                {
                    break;
                }
                await Task.Delay(500);
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
            return -1;
        }
        return responseTime;
    }

    /// <summary>
    /// Creates local SOCKS proxy instance.
    /// </summary>
    private static async Task<WebProxy?> GetWebProxy()
    {
        var port = AppManager.Instance.GetLocalPort(EInboundProtocol.socks);
        return new WebProxy($"socks5://{Global.Loopback}:{port}");
    }

    /// <summary>
    /// Measures response time by sending HTTP requests through proxy.
    /// </summary>
    public static async Task<int> GetRealPingTime(IWebProxy? webProxy, CancellationToken cancellationToken = default)
    {
        var url = AppManager.Instance.Config.SpeedTestItem.SpeedPingTestUrl;
        var responseTime = -1;
        try
        {
            using var timeoutCts = new CancellationTokenSource();
            timeoutCts.CancelAfter(Global.LocalFetch);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            var linkedToken = linkedCts.Token;
            using var client = new HttpClient(new SocketsHttpHandler()
            {
                Proxy = webProxy,
                UseProxy = webProxy != null,
                ConnectTimeout = Global.LocalFetch,
            });

            List<int> oneTime = [];
            for (var i = 0; i < 2; i++)
            {
                var timer = Stopwatch.StartNew();
                await client.GetAsync(url, linkedToken).ConfigureAwait(false);
                timer.Stop();
                oneTime.Add((int)timer.Elapsed.TotalMilliseconds);
                await Task.Delay(100, linkedToken);
            }
            responseTime = oneTime.Where(x => x > 0).OrderBy(x => x).FirstOrDefault();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            // Ignore
        }
        return responseTime;
    }

    /// <summary>
    /// Gets IP, region and IP-quality information through specified proxy.
    /// </summary>
    public static async Task<IpInfoResult?> GetIPInfo(IWebProxy? webProxy, CancellationToken cancellationToken = default)
    {
        try
        {
            var cfg = AppManager.Instance.Config.SpeedTestItem;
            var url = cfg.IPAPIUrl;
            if (url.IsNullOrEmpty())
            {
                return null;
            }

            var result = await QueryIpApiAsync(url, webProxy, cancellationToken);
            if (result is null)
            {
                return null;
            }

            // 主接口（如 ip.sb）不带 IP 质量标记时，再用质量接口补一次，
            // 两者地址相同则说明一次请求已经拿到了全部字段，不再重复请求
            if (result.Value.Purity is null
                && cfg.EnableIpPurity
                && cfg.IpPurityUrl.IsNotEmpty()
                && !string.Equals(cfg.IpPurityUrl, url, StringComparison.OrdinalIgnoreCase))
            {
                var quality = await QueryIpApiAsync(cfg.IpPurityUrl, webProxy, cancellationToken);
                if (quality is not null)
                {
                    var q = quality.Value;
                    var p = result.Value;
                    result = p with
                    {
                        Region = p.Region ?? q.Region,
                        City = p.City ?? q.City,
                        Purity = q.Purity,
                        IpType = q.IpType,
                        Flags = q.Flags,
                        Asn = p.Asn ?? q.Asn,
                    };
                }
            }

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<IpInfoResult?> QueryIpApiAsync(string url, IWebProxy? webProxy, CancellationToken cancellationToken)
    {
        var downloadHandle = new DownloadService();
        var text = await downloadHandle.TryDownloadString(url, webProxy, "", cancellationToken);
        return text is null ? null : IpInfoParser.Parse(text);
    }
}
