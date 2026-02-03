@echo off
REM 使用 %~dp0 来引用当前批处理文件所在的目录
set GAME_PATH="%~dp0Banana\MyGame.exe"

REM 设置要传递的命令行参数
set PORT=12345
set FPS=60
set SCREEN_WIDTH=1920
set SCREEN_HEIGHT=1080

REM 检查命令行参数以选择图形 API
if "%1" == "-force-d3d12" (
    set GRAPHICS_API=-force-d3d12
) else (
    set GRAPHICS_API=-force-d3d11
)

REM 启动 Unity 游戏并传递参数
%GAME_PATH% -port=%PORT% -fps=%FPS% %GRAPHICS_API% -screenWidth=%SCREEN_WIDTH% -screenHeight=%SCREEN_HEIGHT%

REM 暂停以查看命令行输出（可选）
pause
