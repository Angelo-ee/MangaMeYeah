<p align="center">
  <img src="docs/logo/logo_with_icon_transparent.png" alt="MangaMeYeah! Logo" width="560">
</p>

# MangaMeYeah!（简体中文版）

本仓库是 [emmmanime/MangaMeYeah](https://github.com/emmmanime/MangaMeYeah) 的**中文汉化 fork**，基于上游提交 `40016c6`（v0.4.1）。

原项目是一款面向 Windows 10/11（64 位）的高速漫画 / 图像浏览器，用 .NET 8 / WPF 重新实现经典查看器 MangaMeeya 的操作手感，支持压缩包免解压直接浏览、分割插图自动拼接还原、缩略图列表等。完整的功能介绍与技术说明请参阅日文原版 [README.jp](README.jp)。

## 本 fork 改了什么

**只做文本汉化，未改动任何功能逻辑、构建配置或资源文件：**

- **界面文本**：18 个源文件中的菜单、按钮、对话框、提示信息、图像信息、资源管理器右键菜单等 UI 文本由日文改为简体中文。菜单逻辑仍由 XAML 的 `Uid` 驱动，未作改动。
- **README**：重写为本文；日文原版完整保留为 [README.jp](README.jp)。
- **帮助文档**：[docs/help/](docs/help/index.md) 下 7 份用户手册与设置参考翻译为中文，可按 `F1` 查看帮助。
- **未改动**：`HISTORY.md` 更新历史，以及 `docs/spec`、`docs/roadmap`、`docs/test` 等开发文档仍为日文；源码注释保持原样。

## 使用与构建

只需要 Windows 免安装版：直接运行单文件 `MangaMeYeah.exe` 即可，自带运行时、无需安装 .NET，设置保存在同目录的 `MangaMeYeah.ini` 中。

从源码构建免安装单文件版：

```bash
dotnet publish src/MangaMeYeah/MangaMeYeah.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:EnableWindowsTargeting=true -o publish/win-x64
```

## 上游与许可

- 原仓库：[emmmanime/MangaMeYeah](https://github.com/emmmanime/MangaMeYeah)
- 本 fork：[Angelo-ee/MangaMeYeah](https://github.com/Angelo-ee/MangaMeYeah)
- 许可：[MIT](LICENSE)，与原项目一致。
