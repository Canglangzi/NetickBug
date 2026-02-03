using UnityEngine;

namespace CockleBurs.GameFramework.Utility
{
    public static class CommandLineParser
    {
        // 定义默认配置参数
        private static string serverPort = "12345";
        private static bool isHeadless = false;
        private static bool runInBackground = false;
        private static string logLevel = "Info";
        private static string configFilePath = "config.config";
        private static int maxPlayers = 10;
        private static int screenWidth = 1920;
        private static int screenHeight = 1080;
        private static bool fullscreen = true;
        private static string quality = "High";
        private static string language = "en";
        private static int fps = 60;
        private static string graphicsApi = "Auto";

        static CommandLineParser()
        {
            try
            {
                ParseCommandLineArguments();
                ApplySettings();
            }
            catch (System.Exception ex)
            {
                // 捕获并记录初始化过程中的异常
                Debug.LogError($"An error occurred during initialization: {ex.Message}");
            }
        }

        private static void ParseCommandLineArguments()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case string arg when arg.StartsWith("-port="):
                        serverPort = arg.Substring("-port=".Length);
                        Debug.Log($"Server port set to: {serverPort}");
                        break;
                    case "-nographics":
                        isHeadless = true;
                        Application.runInBackground = false;
                        Debug.Log("Running in headless mode.");
                        break;
                    case "-background":
                        runInBackground = true;
                        Application.runInBackground = true;
                        Debug.Log("Running in background mode.");
                        break;
                    case string arg when arg.StartsWith("-logLevel="):
                        logLevel = arg.Substring("-logLevel=".Length);
                        Debug.Log($"Log level set to: {logLevel}");
                        break;
                    case string arg when arg.StartsWith("-configFile="):
                        configFilePath = arg.Substring("-configFile=".Length);
                        Debug.Log($"Config file path set to: {configFilePath}");
                        break;
                    case string arg when arg.StartsWith("-maxPlayers="):
                        if (int.TryParse(arg.Substring("-maxPlayers=".Length), out int players))
                        {
                            maxPlayers = players;
                            Debug.Log($"Max players set to: {maxPlayers}");
                        }
                        else
                        {
                            Debug.LogWarning("Invalid value for -maxPlayers. Using default value.");
                        }
                        break;
                    case string arg when arg.StartsWith("-screenWidth="):
                        if (int.TryParse(arg.Substring("-screenWidth=".Length), out int width))
                        {
                            screenWidth = width;
                            Debug.Log($"Screen width set to: {screenWidth}");
                        }
                        else
                        {
                            Debug.LogWarning("Invalid value for -screenWidth. Using default value.");
                        }
                        break;
                    case string arg when arg.StartsWith("-screenHeight="):
                        if (int.TryParse(arg.Substring("-screenHeight=".Length), out int height))
                        {
                            screenHeight = height;
                            Debug.Log($"Screen height set to: {screenHeight}");
                        }
                        else
                        {
                            Debug.LogWarning("Invalid value for -screenHeight. Using default value.");
                        }
                        break;
                    case "-fullscreen":
                        fullscreen = true;
                        Debug.Log("Fullscreen mode enabled.");
                        break;
                    case "-windowed":
                        fullscreen = false;
                        Debug.Log("Windowed mode enabled.");
                        break;
                    case string arg when arg.StartsWith("-quality="):
                        quality = arg.Substring("-quality=".Length);
                        Debug.Log($"Quality set to: {quality}");
                        break;
                    case string arg when arg.StartsWith("-language="):
                        language = arg.Substring("-language=".Length);
                        Debug.Log($"Language set to: {language}");
                        break;
                    case string arg when arg.StartsWith("-fps="):
                        if (int.TryParse(arg.Substring("-fps=".Length), out int frameRate))
                        {
                            fps = frameRate;
                            Debug.Log($"Frame rate set to: {fps}");
                        }
                        else
                        {
                            Debug.LogWarning("Invalid value for -fps. Using default value.");
                        }
                        break;
                    case "-force-d3d11":
                        graphicsApi = "DirectX11";
                        Debug.Log("Forcing DirectX 11 graphics API.");
                        break;
                    case "-force-d3d12":
                        graphicsApi = "DirectX12";
                        Debug.Log("Forcing DirectX 12 graphics API.");
                        break;
                    default:
                        // 记录未知参数
                        Debug.LogWarning($"Unknown command line argument: {args[i]}");
                        break;
                }
            }
        }

        private static void ApplySettings()
        {
            // 设置图形 API
            switch (graphicsApi)
            {
                case "DirectX11":
                    Debug.Log("Using DirectX 11.");
                    break;
                case "DirectX12":
                    Debug.Log("Using DirectX 12.");
                    break;
                default:
                    Debug.Log("Auto-detecting graphics API.");
                    break;
            }

            // 设置分辨率和全屏模式
            try
            {
                Screen.SetResolution(screenWidth, screenHeight, fullscreen);
                Debug.Log($"Resolution set to: {screenWidth}x{screenHeight}, Fullscreen: {(fullscreen ? "Yes" : "No")}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to set screen resolution: {ex.Message}");
            }

            // 设置帧率
            try
            {
                Application.targetFrameRate = fps;
                Debug.Log($"Frame rate set to: {fps}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to set frame rate: {ex.Message}");
            }
        }

        public static string GetServerPort() => serverPort;
        public static bool IsHeadless() => isHeadless;
        public static bool RunInBackground() => runInBackground;
        public static string GetLogLevel() => logLevel;
        public static string GetConfigFilePath() => configFilePath;
        public static int GetMaxPlayers() => maxPlayers;
        public static int GetScreenWidth() => screenWidth;
        public static int GetScreenHeight() => screenHeight;
        public static bool IsFullscreen() => fullscreen;
        public static string GetQuality() => quality;
        public static string GetLanguage() => language;
        public static int GetFps() => fps;
        public static string GetGraphicsApi() => graphicsApi;
    }
}    