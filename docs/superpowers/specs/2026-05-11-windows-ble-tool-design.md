# Windows BLE Tool — 设计规格

## 概述

Windows 下的 BLE 低功耗蓝牙交互调试工具。面向开发者 + 普通用户，提供 WinUI 3 桌面 GUI + CLI 控制台两种交互方式。

## 技术栈

- **GUI**: WinUI 3 桌面应用 (.NET)
- **CLI**: .NET 控制台应用
- **BLE 核心**: BleCore 类库（封装 Windows.Devices.Bluetooth）
- **脚本引擎**: ClearScript (V8) 嵌入 JavaScript
- **项目结构**: 分层 .NET 解决方案，共享 BleCore 类库

## 项目结构

```
WindowsBleTool.sln
/src/BleCore/           — BLE 核心类库 + 脚本引擎 + 数据服务
/src/BleTool.Gui/       — WinUI 3 桌面应用
/src/BleTool.Cli/       — 控制台应用
```

## GUI 界面

### 整体布局

经典三栏式，顶部 Tab 切换功能模块：
- **扫描设备** — 设备发现 + 过滤 + 广播数据
- **GATT 浏览器** — 服务/特征值树 + 数据交互
- **脚本编辑器** — 代码编辑 + 控制台
- **设置** — 偏好配置

开发者模式 / 简化模式一键切换。

### 扫描面板

**过滤规则**（参考 nRF Connect）：
- RSSI：>= / <= 指定值
- 设备名称：包含 / 正则 / 完全匹配 / 排除
- 广播 UUID：精确匹配 Service UUID（16/32/128-bit）
- MAC 地址：通配符 + 正则
- 厂商 ID：Company ID 精确匹配
- Raw 数据：Hex 模式匹配
- 地址类型：Public / Random

多规则支持 AND / OR 逻辑。匹配设备高亮，不匹配设备折叠并标注原因。规则可保存为预设。

**广播数据面板**：解析 AdType（Flags、Local Name、UUIDs、Service Data、Manufacturer Data 等）。

**设备列表**：显示名称、MAC 地址、RSSI 信号强度、连接状态。

### GATT 交互

连接设备后，显示服务列表和特征值表（UUID / 权限 / 当前值）。选中特征值执行读、写、订阅通知操作。

底部多格式显示：Hex / Decimal / Binary / UTF-8 / Base64 切换。已知 UUID 智能提示格式。

### 数据格式

Hex / Decimal / Binary / UTF-8 / Base64 五种格式可切换。根据已知 UUID（如 0x2A37 = HR Measurement）智能建议显示格式。

### 日志

- **会话日志**：所有 BLE 操作时间线（扫描/连接/读/写/通知/错误），导出为 .txt 或 .json
- **通知数据记录器**：结构化表格（时间/设备/UUID/Hex/解析值），导出为 CSV，可设滚动上限

## 脚本引擎

### 架构

ClearScript (V8) 嵌入 WinUI 进程。`ble` 全局对象由 C# 注入，返回 Promise。沙箱化：仅暴露 `ble` + `console` + 定时器，无文件/网络/进程权限。

### BLE JS API

```js
// 扫描
const devices = await ble.scanAsync({ filters: [{ name: "Heart*" }], duration: 5000 });

// 连接
const device = await ble.connectAsync(devices[0]);
// 或复用 GUI 已连接的设备: ble.getConnectedDevice(address)

// GATT 操作
const service = await device.getServiceAsync("180D");
const char = await service.getCharacteristicAsync("2A37");
const value = await char.readAsync();           // → Uint8Array
await char.writeAsync(new Uint8Array([0x01]));
await char.writeWithoutResponseAsync(data);

// 通知订阅
await char.subscribeAsync((data) => {
    console.log(`HR: ${data[1]} bpm`);
});
await char.unsubscribe();

// 断开
await device.disconnectAsync();
```

### 脚本编辑器

左侧代码编辑器（语法高亮），右侧控制台输出 + API 速览面板。运行/停止按钮，脚本保存/加载，内置示例模板。

## CLI 命令行

```
ble-tool scan [--rssi <dBm>] [--timeout <ms>] [--format json]
ble-tool connect <address>
ble-tool disconnect [--all]
ble-tool read --service <uuid> --char <uuid> [--device <addr>] [--format hex]
ble-tool write --service <uuid> --char <uuid> --data <hex> [--device <addr>]
ble-tool subscribe --service <uuid> --char <uuid> [--device <addr>]
ble-tool run <script-file>
```

- 默认人类可读输出，`--format json` 切换为 JSON
- 多设备连接时通过 `--device` 指定目标

## 多设备连接

支持同时连接多台设备。设备列表每台独立维护连接和 GATT 缓存。切换设备时 GATT 树随之切换。CLI 通过 `--device` 参数指定操作目标。

## 连接管理

- 设备绑定/Pairing 走 Windows 系统蓝牙设置
- 历史设备列表本地保存，支持快速重连
- 连接断开 → BleCore 统一抛异常 → 上层显示友好提示

## 设置持久化

本地 JSON 配置文件存储：RSSI 默认阈值、日志条数上限、脚本模板路径、数据格式首选项、历史设备列表。

## 错误处理

BleCore 统一封装 Windows BLE 异常（`DeviceUnreachable`、`GattReadFailed`、`CharacteristicNotWritable` 等），应用层转换为中文友好提示。脚本中映射为标准 JS Error。

## 测试策略

- BleCore：`IBluetoothAdapter` 接口抽象，单元测试 mock BLE 硬件
- CLI：集成测试脚本验证命令输出
- GUI：WinUI 自动化测试工具
