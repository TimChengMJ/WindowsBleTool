# Windows BLE Tool — 设计规格

**日期**: 2026-05-11  
**目标**: Windows 桌面 BLE 交互工具，兼顾开发者调试与普通用户数据查看

## 1. 架构

分层 .NET 方案，共享 BLE 核心，两个前端消费面：

```
WinUI 3 GUI          CLI 控制台
     │                    │
     └────────┬───────────┘
              │
     ┌────────┴──────────┐
     │  脚本引擎 (V8)     │
     │  数据格式 & 日志   │
     └────────┬──────────┘
              │
     ┌────────┴──────────┐
     │  BleCore 类库      │
     │  (Windows.Devices  │
     │   .Bluetooth 封装)  │
     └───────────────────┘
```

**项目结构**:
- `WindowsBleTool.sln`
- `src/BleCore/` — BLE 核心类库（扫描、连接、GATT、通知）
- `src/BleTool.Gui/` — WinUI 3 桌面应用（开发者面板 + 简化视图 + 脚本编辑器）
- `src/BleTool.Cli/` — 控制台应用
- `src/BleTool.Shared/` — 脚本引擎、数据格式化服务

**关键规则**:
- BleCore 零 UI/网络依赖，可独立单元测试
- 两个前端通过 BleCore 共享连接状态
- HTTP API 不在范围内

## 2. GUI 布局

### 整体结构
- 顶部 Tab: 扫描设备 | GATT 浏览器 | 脚本编辑器 | 设置
- 模式切换: 开发者模式 / 简化模式（Switch 一键切换）

### 扫描面板
**三区布局**: 设备列表（左）| 广播数据（右上）| 服务交互（右下）

**设备列表**:
- 实时显示扫描发现的设备：名称、MAC、RSSI、连接状态
- RSSI 信号强度颜色指示（绿/黄/橙）
- 双击设备发起连接

**过滤规则**（参考 nRF Connect）:
- 维度: RSSI (>= / <=)、设备名称（包含/正则/完全匹配/排除）、广播 UUID、MAC 地址、厂商 ID、Raw Hex 模式、地址类型 (Public/Random)
- 组合: AND / OR 逻辑
- UI: Filter Chip 展示活跃规则，点击 × 移除
- 不匹配设备折叠显示并标注原因
- 过滤预设可保存/加载

**广播数据面板**:
- 点击设备后展示完整 AdData 解析：AdType 分类显示
- 字段: Flags、Complete Local Name、16-bit UUIDs、Service Data、Manufacturer Data
- 显示最后更新时间

**服务交互**:
- 连接设备后展示 GATT 服务列表
- 选择服务 → 特征值表格（UUID、权限标志 [R/W/N]、当前值）
- 操作按钮: 读取、写入、订阅通知、取消订阅

### GATT 浏览器
- 经典树形视图: 设备 → Service → Characteristic → Descriptor
- 树节点标注操作权限图标 (R/W/N/I)
- 点击特征值 → 底部值详情面板

### 特征值详情/交互区
- 读写操作栏: 输入框 + 读/写/订阅按钮
- 多格式显示行: Hex | Dec | Bin | UTF-8 | Base64 一键切换
- 已知 UUID 智能格式提示（如 0x2A37 → HR bpm 解析）

### 简化模式
- 隐藏 UUID、Raw Hex、GATT 树细节
- 只展示已解析的数据卡片
- 一键连接向导

## 3. 脚本引擎

### 宿主
- ClearScript (V8) 嵌入 WinUI 进程
- `ble` 全局对象由 C# 注入，Promise 异步 API
- 沙箱: 仅暴露 `ble` + `console` + 定时器，无文件/网络/进程权限

### BLE JS API
```js
// 扫描
const devices = await ble.scanAsync({ filters: [...], duration: 5000 });

// 连接
const device = await ble.connectAsync(address);

// GATT
const service = await device.getServiceAsync("180D");
const char = await service.getCharacteristicAsync("2A37");
const data = await char.readAsync();                    // → Uint8Array
await char.writeAsync(new Uint8Array([0x01]));
await char.writeWithoutResponseAsync(data);
await char.subscribeAsync((data) => { /* callback */ });
char.unsubscribe();

// 断开
device.disconnect();
```

### 脚本编辑器 Tab
- 左侧: 代码编辑器（语法高亮），右侧: 控制台输出 + API 速览
- 预设脚本模板（数据记录器、批量扫描等）
- 运行/停止按钮、保存/加载脚本
- GUI 已连接设备可通过 `ble.getConnectedDevices()` 直接在脚本中使用

### 脚本执行入口
- GUI 编辑器 Tab — 编写和运行
- CLI: `ble-tool run script.js` — 执行脚本文件

## 4. CLI

### 命令
```
ble-tool scan [--rssi N] [--timeout N] [--format json|text]
ble-tool connect <address>
ble-tool disconnect [--all]
ble-tool list-services [<address>]
ble-tool read --service <uuid> --char <uuid> [--format hex|dec|bin|utf8|base64]
ble-tool write --service <uuid> --char <uuid> --data <hex> [--with-response|--without-response]
ble-tool subscribe --service <uuid> --char <uuid>
ble-tool run <script-file>
```

### 输出
- 默认: 人类可读表格
- `--format json`: JSON 输出，适用于管道消费（jq、Python 等）

## 5. 数据格式

### 支持格式
- Hex（默认）、Decimal、Binary、UTF-8、Base64
- 特征值详情区一键切换，所有格式实时显示
- 写入数据支持 Hex 输入，按当前格式显示

### 智能提示
- 根据已知 UUID 自动建议最优格式和解析方式
- 例: 0x2A37 → Flags + HR bpm，0x2A19 → Battery Level %

## 6. 日志

### 会话日志
- 自动记录所有 BLE 操作: [SCAN] [CONNECT] [GATT] [READ] [WRITE] [SUBSCRIBE] [NOTIFY] [ERROR]
- 带时间戳的完整操作时间线
- 导出为 .txt 或 .json

### 通知数据记录器
- 结构化表格: 时间 | 设备 | 特征值 UUID | Hex 值 | 解析值
- 可配置上限（默认 10,000 条），到达上限滚动覆盖
- 导出 CSV（可导入 Excel/Python）

## 7. 技术选型

| 项 | 选择 |
|---|---|
| UI 框架 | WinUI 3 |
| BLE API | Windows.Devices.Bluetooth |
| 脚本引擎 | ClearScript (V8) |
| 数据格式 | 内置服务，5 种格式 |
| 目标平台 | Windows 11 |
| 语言 | C# (.NET 9) |

## 8. 错误处理

- BleCore 所有异步操作通过 `Result<T>` 或异常返回错误（连接超时、GATT 读写失败、设备断开等）
- GUI 层错误通过 Toast 通知 + 会话日志 [ERROR] 记录
- CLI 错误输出到 stderr，非零退出码
- 脚本异常通过 `catch` 捕获，输出到控制台，不中断宿主进程

## 9. 测试策略

- **BleCore 单元测试**: 使用模拟 BLE 适配器（或实际硬件）测试 GATT 解析、过滤逻辑、数据格式转换
- **GUI 集成测试**: 关键用户流程手动测试（扫描 → 过滤 → 连接 → 读写 → 订阅）
- **CLI 测试**: 端到端命令测试，验证输出格式和错误处理

## 10. 范围边界

**在范围内**:
- 扫描、RSSI 过滤、广播数据解析
- 多设备连接、GATT 浏览、读写、通知订阅
- 脚本编辑器和 JS 自动化
- CLI 工具
- 多格式数据展示、会话日志、通知数据记录器

**不在范围内**:
- HTTP API / Web 控制
- 移动平台（Android/iOS）
- BLE Mesh
- OTA DFU 升级（可后续通过脚本扩展）
