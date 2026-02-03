
using System.Diagnostics;


namespace CockleBurs.GameFramework.Utility
{
    internal static class ProcessHelper
    {
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
        internal static void LaunchLinuxTerminalAndLaunchProcess(string file, string arguments)
        {
            LinuxTerminalSettings terminalSettings = GameSettings.LinuxSettings.linuxTerminalSettings;
            string executeArgument = string.Format(terminalSettings.TerminalExecute, file, arguments);
            ProcessStartInfo startInfo = new()
            {
                FileName = terminalSettings.TerminalCommand,
                Arguments = executeArgument,
                WorkingDirectory = Game.GetGameExecutePath()
            };

            Process process = new()
            {
                StartInfo = startInfo
            };
            process.Start();
        }
#endif
    }
}