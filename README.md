# V2New

基于 [2dust/v2rayN](https://github.com/2dust/v2rayN) 的个人定制分支。面向 Windows 的 Xray / sing-box / mihomo 图形客户端，在保留原版全部协议与路由能力的前提下，强化了**节点质量可视化**与**测速后的自动整理**。

> 上游版本基线：v2rayN 7.25.6（commit `57f17d5`）
> 许可证：GPL-3.0（继承上游，见 [LICENSE](LICENSE)）。本分支的所有修改同样以 GPL-3.0 发布。

---

## 跟随上游自动更新

上游一有提交，本分支会自动出新版本，客户端也能自己升级：

```
2dust/v2rayN 有新提交
        │
        ▼  每天 04:00（北京时间）检查
Sync upstream   ── 合并成功 ──▶  push 到 main
        │                              │
        └── 合并冲突 ──▶ 开 issue          ▼
                                   Build & Release
                                          │
                                          ▼
                              发布 Release（v1.0.<run_number>）
                                          │
                                          ▼
                              客户端「检查更新」检测到新版本
                                          │
                                          ▼
                              AmazTool 解压覆盖，保留配置与订阅
```

| 工作流 | 触发 | 作用 |
|---|---|---|
| `Sync upstream` | 每天 04:00 / 手动 | 拉取 `2dust/v2rayN` master 合并进 main；冲突则回滚并开 issue |
| `Build & Release` | main 有提交 / 手动 | 构建主程序 + 升级工具，打包并发布 Release |

几个关键设计：

- **升级不会覆盖用户数据**：`AmazTool` 只替换程序文件，`bin/` 下已存在的内核不会被覆盖，`guiNConfig.json`、订阅库、节点库都保留。
- **版本号单调递增**（`v1.0.<run_number>`），客户端据此判断「有新版本」。
- **合并冲突不会静默跳过**：工作流会回滚并在 issue 里列出冲突文件，避免把上游改动悄悄丢掉。
- 发布包必须保留 `V2New/` 顶层目录——`AmazTool` 解压时会去掉第一段路径，扁平结构会导致升级后什么都没变。

手动操作：

```bash
# 立刻检查一次上游（不用等定时任务）
gh workflow run sync-upstream.yml
# 只重新构建发版
gh workflow run release.yml
```

---

## 相对原版的改动

### 1. 新增「IP 地区」与「IP 纯净度」列

测速时顺带采集节点出口 IP 的画像，直接显示在节点列表里：

| 列 | 示例 | 说明 |
|---|---|---|
| IP 信息 | `(US) 142.252.100.45` | 原有列，保留 |
| **IP 地区** | `美国 · 洛杉矶` | 新增：国家中文名 + 城市，随接口返回的州/省兜底 |
| **IP 纯净度** | `机房 55` | 新增：类型徽章 + 0–100 评分，按等级着色 |

纯净度评分规则（100 分起，逐项扣分）：

| 判定 | 扣分 | 类型徽章 |
|---|---|---|
| 机房 / 托管 IP（`hosting`） | −45 | 机房 |
| 代理（`proxy`） | −35 | 代理 |
| VPN（`is_vpn`） | −30 | 代理 |
| Tor 出口（`is_tor`） | −45 | 代理 |
| 有滥用记录（`is_abuser`） | −25 | 不变 |
| 爬虫（`is_crawler`） | −10 | 不变 |
| 保留地址（`is_bogon`） | −50 | 不变 |

住宅 IP 且 ≥90 分显示为 **原生**（绿色）；≥85 绿、≥60 琥珀、≥30 橙、<30 红。

数据源默认使用 `ip-api.com`（免费版即返回 `hosting` / `proxy` / `countryCode` / `regionName` / `city`，一次请求拿全）。可在「设置 → 测速」里换成别的接口；若主接口不带质量字段，程序会用独立的 `IpPurityUrl` 补查一次，两者地址相同时不会重复请求。

> 说明：`ipapi.is` 免费版只返回基础地理信息，**不含** `is_datacenter` 等质量字段（已实测确认），所以没有选它做默认源。想要更精细的欺诈分（fraud score）需要付费接口，见下方路线图。

### 2. 右键「测试后自动排序」

节点列表右键菜单新增：

```
测试后自动排序
 ├ 关闭
 ├ 按延迟排序
 ├ 按速度排序
 └ 测试后自动移除无效节点
```

- 一轮测速结束后自动按结果重排：有结果的在前，未测 / 超时的沉底（不会像手动排序那样把 `Delay=0` 的节点顶到最前面）。
- 三个选项互斥，状态写入 `guiNConfig.json` 的 `speedTestItem.autoSortAfterTest`，重启保持。
- 顺带提供「测试后自动移除无效节点」开关，对应 `autoRemoveInvalidAfterTest`，默认关闭。

### 3. 品牌化与去推广

- 程序名 / 窗口标题 / 托盘 / 关于信息统一为 **V2New**，产物为 `V2New.exe`。
- 移除了原版工具栏的「推广」入口及其硬编码推广链接（`Global.PromotionUrl`）。
- 安全提示文案里的上游讨论区链接改为本仓库地址。
- **程序本体更新改指向本仓库**：原版「检查更新」会下载上游 v2rayN 发行包并覆盖自身，对分支是致命的。现在改为查询 `blake006/V2New` 的发行版，升级包由 CI 构建，不会把定制功能覆盖掉。

### 4. 性能与体积

- SQLite 启用 `WAL` + `synchronous=NORMAL` + `busy_timeout`：测速时多线程写 `ProfileExItem`、统计线程写流量数据不再互相阻塞。
- `ProfileExManager` 保存时不再每次整表 `SELECT` 来判断新增/更新，改为内存维护已落库 ID 集合。
- 单文件发布启用压缩：产物从 191 MB 降到 93 MB（`V2New.exe` 178 MB → 75 MB）。
- 自包含发布，目标机器无需预装 .NET 10 运行时。

---

## 构建

需要 .NET 10 SDK（**10.0.4xx**）。注意：10.0.100 编译 WPF 项目会报 `System.Private.Windows.GdiPlus` 缺少引用，属于该 SDK 版本的问题，请使用 10.0.403 或更新版本。

```bash
# 调试
dotnet build v2rayN/v2rayN.csproj -c Debug

# 发布（自包含单文件，win-x64）
dotnet publish v2rayN/v2rayN.csproj -c Release -r win-x64 --self-contained true -o dist/app
```

首次运行需要在「检查更新 → 内核」里下载 Xray / sing-box / mihomo 内核，或手动把内核放进 `bin/` 目录。

---

## 同类工具调研：还可以加什么

调研了 v2rayNG、NekoBox、Clash Verge Rev、Mihomo Party、FlClash、Karing、Hiddify、v2rayA、Sub-Store 等项目的功能设计，按「价值 / 成本」排序，以下候选值得后续做：

| 优先级 | 功能 | 价值 | 参考 | 成本 |
|---|---|---|---|---|
| P0 | **真实欺诈分**：接入 Scamalytics / IPQualityScore 的 fraud score，替代自算评分 | 机场质量判断更准，和 Ant-Browser 的 `fraud 72` 对齐 | Ant-Browser | 低（需用户填 API Key） |
| P0 | **流媒体解锁检测**：Netflix / Disney+ / YouTube Premium / ChatGPT 可用性 | 选节点最实用的维度之一 | Karing、NekoBox | 中 |
| P1 | **自动优选 / 故障切换**：当前节点连续超时自动切到延迟最低的可用节点 | 免去手动换节点 | Clash Verge、Mihomo Party | 中 |
| P1 | **订阅流量与到期提醒**：解析 `subscription-userinfo`，托盘显示剩余流量/天数 | 机场用户刚需 | v2rayNG、FlClash | 低 |
| P1 | **延迟历史曲线**：记录每个节点近 N 次测速，画折线 | 判断节点稳定性 | Karing | 中 |
| P2 | **节点批量重命名**：正则替换 + 按地区自动命名 | 整理订阅 | Sub-Store | 低 |
| P2 | **规则集订阅**：一键导入 GeoSite/GeoIP 规则集并跟随更新 | 分流维护成本 | Clash Verge | 中 |
| P2 | **迷你悬浮窗**：置顶小窗显示当前节点/延迟/上下行 | 桌面常驻体验 | Mihomo Party | 中 |
| P3 | **剪贴板自动导入**：检测到分享链接直接提示添加 | 小便利 | v2rayNG | 低 |
| P3 | **按 IP 地区分组**：在现有「按地区分组」基础上改用实测 IP 而非节点名 | 命名混乱的订阅也能分组 | — | 低 |

---

## 已知限制

- IP 地区/城市由接口返回，城市名是英文原文（与 Ant-Browser 一致），未做中文城市映射。
- `ip-api.com` 免费版仅支持 HTTP，且限制 45 次/分钟（按请求方 IP 计）。走节点代理时请求方就是节点出口 IP，通常不会触发限流；大规模测速可换付费接口。
- 纯净度是「基于公开标记的启发式评分」，不等于风控厂商的真实风控分。
- 仅验证了 Windows x64 构建；上游的 Linux / macOS（Avalonia）工程未同步改动。
