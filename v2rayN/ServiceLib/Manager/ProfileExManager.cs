namespace ServiceLib.Manager;

public class ProfileExManager
{
    private static readonly Lazy<ProfileExManager> _instance = new(() => new());
    private ConcurrentBag<ProfileExItem> _lstProfileEx = [];
    private readonly ConcurrentQueue<string> _queIndexIds = new();
    private readonly Lock _profileExLock = new();
    // 已落库的 IndexId，避免每次保存都整表读一遍来判断新增还是更新
    private readonly HashSet<string> _knownIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _knownIdsLock = new();
    public static ProfileExManager Instance => _instance.Value;
    private static readonly string _tag = "ProfileExHandler";

    public ProfileExManager()
    {
        //Init();
    }

    public async Task Init()
    {
        await InitData();
    }

    public async Task<ConcurrentBag<ProfileExItem>> GetProfileExs()
    {
        return await Task.FromResult(_lstProfileEx);
    }

    private async Task InitData()
    {
        await SQLiteHelper.Instance.ExecuteAsync($"delete from ProfileExItem where indexId not in ( select indexId from ProfileItem )");

        var lstProfileEx = new ConcurrentBag<ProfileExItem>(await SQLiteHelper.Instance.TableAsync<ProfileExItem>().ToListAsync());
        lock (_profileExLock)
        {
            _lstProfileEx = lstProfileEx;
        }
        lock (_knownIdsLock)
        {
            _knownIds.Clear();
            foreach (var item in lstProfileEx)
            {
                _knownIds.Add(item.IndexId);
            }
        }
    }

    private void IndexIdEnqueue(string indexId)
    {
        if (indexId.IsNotEmpty())
        {
            //Speed tests touch this from many threads; duplicates are harmless here since
            //the save loop just writes the same row more than once
            _queIndexIds.Enqueue(indexId);
        }
    }

    private async Task SaveQueueIndexIds()
    {
        var lstIds = new List<string>();
        while (_queIndexIds.TryDequeue(out var id))
        {
            lstIds.Add(id);
        }

        if (lstIds.Count > 0)
        {
            List<ProfileExItem> lstInserts = [];
            List<ProfileExItem> lstUpdates = [];

            foreach (var id in lstIds)
            {
                var itemNew = _lstProfileEx?.FirstOrDefault(t => t.IndexId == id);
                if (itemNew is null)
                {
                    continue;
                }

                bool exists;
                lock (_knownIdsLock)
                {
                    exists = _knownIds.Contains(id);
                }
                if (exists)
                {
                    lstUpdates.Add(itemNew);
                }
                else
                {
                    lstInserts.Add(itemNew);
                }
            }

            try
            {
                if (lstInserts.Count > 0)
                {
                    await SQLiteHelper.Instance.InsertAllAsync(lstInserts);
                    lock (_knownIdsLock)
                    {
                        foreach (var item in lstInserts)
                        {
                            _knownIds.Add(item.IndexId);
                        }
                    }
                }

                if (lstUpdates.Count > 0)
                {
                    await SQLiteHelper.Instance.UpdateAllAsync(lstUpdates);
                }
            }
            catch (Exception ex)
            {
                Logging.SaveLog(_tag, ex);
            }
        }
    }

    private ProfileExItem AddProfileEx(string indexId)
    {
        var profileEx = new ProfileExItem()
        {
            IndexId = indexId,
            Delay = 0,
            Speed = 0,
            Sort = 0,
            Message = string.Empty
        };
        _lstProfileEx.Add(profileEx);
        IndexIdEnqueue(indexId);
        return profileEx;
    }

    private ProfileExItem GetProfileExItem(string? indexId)
    {
        var profileEx = _lstProfileEx.FirstOrDefault(t => t.IndexId == indexId);
        if (profileEx is not null)
        {
            return profileEx;
        }

        lock (_profileExLock)
        {
            profileEx = _lstProfileEx.FirstOrDefault(t => t.IndexId == indexId);
            return profileEx ?? AddProfileEx(indexId);
        }
    }

    public async Task ClearAll()
    {
        await SQLiteHelper.Instance.ExecuteAsync($"delete from ProfileExItem ");
        lock (_profileExLock)
        {
            _lstProfileEx = [];
        }
        lock (_knownIdsLock)
        {
            _knownIds.Clear();
        }
    }

    public async Task SaveTo()
    {
        try
        {
            await SaveQueueIndexIds();
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    public void SetTestDelay(string indexId, int delay)
    {
        var profileEx = GetProfileExItem(indexId);

        profileEx.Delay = delay;
        IndexIdEnqueue(indexId);
    }

    public void SetTestSpeed(string indexId, decimal speed)
    {
        var profileEx = GetProfileExItem(indexId);

        profileEx.Speed = speed;
        IndexIdEnqueue(indexId);
    }

    public void SetTestMessage(string indexId, string message)
    {
        var profileEx = GetProfileExItem(indexId);

        profileEx.Message = message;
        IndexIdEnqueue(indexId);
    }

    public void SetTestIpInfo(string indexId, string ipInfo)
    {
        var profileEx = GetProfileExItem(indexId);

        profileEx.IpInfo = ipInfo;
        IndexIdEnqueue(indexId);
    }

    /// <summary>
    /// 写入完整的出口 IP 画像：地区、城市、纯净度、扣分原因
    /// </summary>
    public void SetTestIpInfo(string indexId, IpInfoResult info)
    {
        var profileEx = GetProfileExItem(indexId);

        profileEx.IpInfo = info.ToString();
        profileEx.IpCountry = info.Country;
        profileEx.IpRegion = info.Region;
        profileEx.IpCity = info.City;
        profileEx.IpType = info.IpType;
        profileEx.IpPurity = info.Purity;
        profileEx.IpFlags = info.Flags;
        IndexIdEnqueue(indexId);
    }

    public void SetSort(string indexId, int sort)
    {
        var profileEx = GetProfileExItem(indexId);

        profileEx.Sort = sort;
        IndexIdEnqueue(indexId);
    }

    public int GetSort(string indexId)
    {
        var profileEx = _lstProfileEx.FirstOrDefault(t => t.IndexId == indexId);
        if (profileEx == null)
        {
            return 0;
        }
        return profileEx.Sort;
    }

    public int GetMaxSort()
    {
        if (_lstProfileEx.Count <= 0)
        {
            return 0;
        }
        return _lstProfileEx.Max(t => t?.Sort ?? 0);
    }
}
