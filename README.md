# Petal · 跨平台桌面挂件

独立 Avalonia / C# 版本。支持 macOS Apple Silicon、Intel 和 Windows。

- 350×180 逻辑像素，暗色半透明圆角，紫蓝配色
- 最近 15 个交易日的日线收盘价折线；当天未收盘时是最新值
- 每 30 秒刷新，顶部刷新按钮可立即更新
- 顶部装饰和空白区拖动；Esc / 减号隐藏；菜单栏图标恢复；× 退出
- 国际参考价 GC=F；国内为国际价 × USD/CNY ÷ 31.1034768 折算，不是上金所直接报价
- Yahoo Finance 数据可能延迟，仅供参考；断网保留旧数据，灰色状态点悬停查看错误

## macOS 使用预编译包

M1/M2/M3/M4 等 Apple Silicon 选 osx-arm64；Intel Mac 选 osx-x64。
将对应 .tar.gz 复制到 Mac，用终端解压（保留可执行权限）：

```bash
tar -xzf Petal-osx-arm64.tar.gz
chmod +x Petal.app/Contents/MacOS/Petal
codesign --force --deep --sign - Petal.app
open Petal.app
```

Intel 把压缩文件名换成 Petal-osx-x64.tar.gz。
发布包包含 .NET 运行时，无需安装 SDK。可将 Petal.app 移到 /Applications。

Windows 交叉发布包没有 Apple 开发者签名/公证。
若系统拦截，确认文件来自你自己构建的项目后，在系统设置的“隐私与安全性”允许打开；
不需要关闭全局 Gatekeeper。Mac 上执行上述 codesign 可生成本机临时签名。
已在 Windows 构建，macOS 的透明效果、菜单栏和拖动需真机确认。

## Mac 从源码构建

安装 .NET 9 SDK 后：

```bash
dotnet run
bash scripts/package-mac.sh osx-arm64
# Intel: bash scripts/package-mac.sh osx-x64
```

生成 release/<RID>/Petal.app。macOS 最低版本需满足 .NET 9 和 Avalonia 要求，建议 macOS 12+。

## Windows

```powershell
dotnet run
dotnet publish -c Release -r win-x64 --self-contained true -o release/win-x64
```
