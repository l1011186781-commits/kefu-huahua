@echo off
chcp 65001 >nul
echo 正在编译 客服花花 (最终版)...

set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist %CSC% (
    set CSC="C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

%CSC% /target:winexe /r:System.IO.Compression.FileSystem.dll /win32icon:"app.ico" /out:"客服花花.exe" Program.cs

if %ERRORLEVEL% equ 0 (
    echo [成功] 编译完成！生成文件：客服花花.exe
) else (
    echo [错误] 编译失败，请检查错误提示。
)
pause
