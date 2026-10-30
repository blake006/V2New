using System.Collections;

namespace ServiceLib.Helper;

public sealed class SQLiteHelper
{
    private static readonly Lazy<SQLiteHelper> _instance = new(() => new());
    public static SQLiteHelper Instance => _instance.Value;
    private readonly string _connstr;
    private SQLiteConnection _db;
    private SQLiteAsyncConnection _dbAsync;
    private readonly string _configDB = "guiNDB.db";

    public SQLiteHelper()
    {
        _connstr = Utils.GetConfigPath(_configDB);
        _db = new SQLiteConnection(_connstr, false);
        _dbAsync = new SQLiteAsyncConnection(_connstr, false);
        ApplyPragmas();
    }

    /// <summary>
    /// 测速时多个线程会同时写 ProfileExItem，统计与日志也在写同一个库。
    /// 开 WAL 后读写互不阻塞，并把同步级别降为 NORMAL，明显减少锁等待。
    /// </summary>
    private void ApplyPragmas()
    {
        try
        {
            _db.Execute("PRAGMA journal_mode=WAL;");
            _db.Execute("PRAGMA synchronous=NORMAL;");
            _db.Execute("PRAGMA busy_timeout=5000;");
        }
        catch
        {
            // 数据库不可写时退回默认行为，不影响启动
        }
    }

    public CreateTableResult CreateTable<T>()
    {
        return _db.CreateTable<T>();
    }

    public async Task<int> InsertAllAsync(IEnumerable models)
    {
        return await _dbAsync.InsertAllAsync(models, runInTransaction: true).ConfigureAwait(false);
    }

    public async Task<int> InsertAsync(object model)
    {
        return await _dbAsync.InsertAsync(model);
    }

    public async Task<int> ReplaceAsync(object model)
    {
        return await _dbAsync.InsertOrReplaceAsync(model);
    }

    public async Task<int> UpdateAsync(object model)
    {
        return await _dbAsync.UpdateAsync(model);
    }

    public async Task<int> UpdateAllAsync(IEnumerable models)
    {
        return await _dbAsync.UpdateAllAsync(models, runInTransaction: true).ConfigureAwait(false);
    }

    public async Task<int> DeleteAsync(object model)
    {
        return await _dbAsync.DeleteAsync(model);
    }

    public async Task<int> DeleteAllAsync<T>()
    {
        return await _dbAsync.DeleteAllAsync<T>();
    }

    public async Task<int> ExecuteAsync(string sql)
    {
        return await _dbAsync.ExecuteAsync(sql);
    }

    public async Task<List<T>> QueryAsync<T>(string sql) where T : new()
    {
        return await _dbAsync.QueryAsync<T>(sql);
    }

    public AsyncTableQuery<T> TableAsync<T>() where T : new()
    {
        return _dbAsync.Table<T>();
    }

    public async Task DisposeDbConnectionAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                _db?.Close();
                _db?.Dispose();
            }
            finally
            {
                _db = null;
            }

            try
            {
                var conn = _dbAsync?.GetConnection();
                conn?.Close();
                conn?.Dispose();
            }
            finally
            {
                _dbAsync = null;
            }
        });
    }
}
