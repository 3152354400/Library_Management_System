# ============================================================
# 图书馆管理系统 - 本机一键打包脚本（Windows PowerShell）
# 运行前：已安装 Node 22+；.NET SDK 已安装（自动探测）
# 产物：  frontend/dist/  （前端静态文件）
#         publish/       （后端发布包）
# ============================================================

$ErrorActionPreference = "Stop"

# 定位 .NET SDK（用户级安装优先）
$dotnet = "$env:USERPROFILE\.dotnet\dotnet.exe"
if (-not (Test-Path $dotnet)) { $dotnet = "dotnet" }
Write-Host "使用 dotnet: $dotnet"

# ---------- 1. 前端打包 ----------
Write-Host "`n[1/2] 前端打包 (npm run build) ..."
Push-Location "$PSScriptRoot\..\frontend"
if (-not (Test-Path node_modules)) {
    Write-Host "  首次运行，安装依赖 (npm install) ..."
    npm install
}
npm run build
if ($LASTEXITCODE -ne 0) { throw "前端打包失败" }
Pop-Location
Write-Host "  前端完成 -> frontend\dist"

# ---------- 2. 后端发布 ----------
Write-Host "`n[2/2] 后端发布 (dotnet publish) ..."
Push-Location "$PSScriptRoot\..\backend"
& $dotnet publish -c Release -o "$PSScriptRoot\..\publish"
if ($LASTEXITCODE -ne 0) { throw "后端发布失败" }
Pop-Location
Write-Host "  后端完成 -> publish"

Write-Host "`n打包完成！上传命令示例："
Write-Host "  scp -r frontend\dist  root@<服务器IP>:/var/www/library/"
Write-Host "  scp -r publish       root@<服务器IP>:/opt/library/"
