# zmBox Tweaks（造梦盒子增强工具 / 补丁说明）

本仓库只包含**独立工具与补丁说明**，**不包含**造梦盒子（zmBox）本体、`AppEntry.swf`、游戏资源或 Adobe AIR 运行时——这些版权归原作者 / Adobe 所有，请自备**合法获取**的造梦盒子 v1.9 使用。

> 免责声明：仅供学习与个人使用；请勿分发造梦盒子本体或其资源。修改前务必备份，风险自负。

## 下载原版造梦盒子（必需）

本仓库只提供补丁与工具，**不含盒子本体**。请先从原作者处下载 v1.9（64 位）：

- 发行版页面：<https://gitee.com/duskeye/zmBox/releases>
- v1.9 直接下载：<https://gitee.com/duskeye/zmBox/releases/download/v1.9/zmBox_x64.zip>
- 源码仓库（MIT，作者 duskeye）：<https://gitee.com/duskeye/zmBox>

解压后应能看到 `造梦盒子.exe`、`AppEntry.swf`、`META-INF/`、`Adobe AIR/`，再按 [`patches/README.md`](patches/README.md) 打补丁。

## 一键补丁包（推荐，不想折腾 FFDec / AIR SDK 的人用）

- 下载地址：<https://github.com/xpjlhxhkh/zmbox-tweaks/releases/latest>（`zmBox-enhance-v1.9.zip`）
- 用法：先解压原版 v1.9 → 再解压一键包 → 双击 `安装.bat` → 选择盒子文件夹。
  会自动备份原文件、替换 `AppEntry.swf` 与 `META-INF`、带上金币和帧率标签；`卸载.bat` 可还原。
- 一键包只含修改后的 `AppEntry.swf`、自签名 `META-INF` 和几个小工具，**不含**盒子本体、AIR 运行时和游戏素材。

## 功能

| 功能 | 说明 |
| --- | --- |
| 透明帧率标签 | 独立小窗口贴在盒子窗口左上角，显示 `FPS xx`，随窗口缩放移动，不挡游戏画面 |
| 角落金币动画 | 16px 金币在窗口右下角持续转动，避免小黄鸭（Lossless Scaling）在静态画面掉到 ~1fps 导致鼠标卡顿 |
| StartBox 启动器 | 先起金币 + 帧率标签，再启动造梦盒子；桌面快捷方式可指向它 |
| 倍速键补丁 | 数字键 `1 / 2 / 3` = 1x / 1.5x / 2x（需改 SWF 并重签名，见 patches/） |
| UI 精简补丁 | 去掉左上角 ram/fps 浮层；登录页三个入口改为单一“开始游戏”（进造梦西游4） |

## 工具源码（tools/）

- `FpsLabel.cs`：帧率标签。盒子每秒把实测 FPS 写入 `%TEMP%\zmBoxFps.txt`，本工具读取并在窗口左上角绘制；透明背景，颜色键 + 无抗锯齿白字黑描边。
- `CornerCoin.cs`：角落金币动画（需自备 `coinNN` 帧，例如从素材图提取的 16px 序列）。
- `StartBox.cs`：启动器，依次启动金币、帧率标签、盒子主程序。
- `launch.ps1` / `OverlayStop.cmd`：带 token 直登 / 关闭两个角标。

编译示例（Windows 自带 .NET Framework 的 csc）：

```bat
csc /nologo /target:winexe /out:fpslabel.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll tools\FpsLabel.cs
csc /nologo /target:winexe /out:cornercoin.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll tools\CornerCoin.cs
csc /nologo /target:winexe /out:StartBox.exe /r:System.Windows.Forms.dll tools\StartBox.cs
```

## 补丁（patches/）

见 [`patches/README.md`](patches/README.md)：

1. 用 FFDec 导出 `AppEntry.swf` 的 AS3；
2. 按说明修改 `AppEntry.as`、`LoginLayer.as`、`GameMediator.as`；
3. 用 FFDec `-importScript` 导入并保存；
4. 用 AIR SDK 的 `adt` 生成自签证书、重新打包；
5. 解压生成的 `.air` 覆盖原目录的 `META-INF` 与 `AppEntry.swf`。

**重要**：只导入改动过的脚本（整树导入会因 `com.greenAnimate.core.Animation.as` 等非标准代码编译失败）。

## 已知限制

- 高帧率模式与倍速互斥是原作者的设定：在 60fps 补丁下强行拉高 `stage.frameRate` 会导致“倍数越高人物越慢”（`GameFrame.deltaTime` / `Physics.deltaTime` 被固定）。补丁保留作者的限制。
- 造梦盒子更新/重装会覆盖 `AppEntry.swf` 与 `META-INF`，需要重新打补丁与签名。

## License

本仓库中的工具源码采用 MIT License（见 `LICENSE`）。造梦盒子及其资源不属于本仓库。
