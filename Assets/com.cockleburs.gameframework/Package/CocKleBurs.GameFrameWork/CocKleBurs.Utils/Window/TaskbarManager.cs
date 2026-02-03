
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using AOT;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.IO;
using System.Collections;


namespace CockleBurs.GameFramework.Utility
{
[DisallowMultipleComponent]
public class TaskbarManager : MonoBehaviour
{
    public IntPtr WindowHandle => _hwnd;
    public IntPtr AppIconHandle => _nid.hIcon;


    #region Windows API 定义
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hwnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public int dwState;        // 新增状态字段
        public int dwStateMask;   // 新增状态掩码字段
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, StringBuilder lpIconPath, out ushort lpiIcon);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll")]
    private static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uID, string lpNewItem);

    [DllImport("user32.dll")]
    private static extern bool TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool SetWindowText(IntPtr hWnd, string lpString);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X, Y; }

    private const int GWL_STYLE = -16;
    private const int WS_CHILD = 0x40000000;
    private const int WS_VISIBLE = 0x10000000;
    #endregion

    #region 常量定义
    private const int WM_SYSCOMMAND = 0x0112;
    private const int WM_LBUTTONDBLCLK = 0x0203;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_COMMAND = 0x0111;
    private const int SC_MINIMIZE = 0xF020;
    private const int SC_CLOSE = 0xF060;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;
    private const int GWL_WNDPROC = -4;
    private const int NIF_ICON = 0x0002;
    private const int NIF_TIP = 0x0004;
    private const int NIF_MESSAGE = 0x0001;
    private const int WM_NOTIFY_TRAY = 0x0800;
    private const int IDM_SHOW = 1001;
    private const int IDM_HIDE = 1002;
    private const int IDM_EXIT = 1003;
    private const int IDI_APPLICATION = 32512;
    #endregion

    #region 成员变量
    private IntPtr _hwnd;
    private IntPtr _oldWndProc;
    private NOTIFYICONDATA _nid;
    private string _baseWindowTitle = "Unity桌面应用";
    private string _windowTitle;
    private static Dictionary<IntPtr, TaskbarManager> _instances = new Dictionary<IntPtr, TaskbarManager>();
    private static WndProcDelegate _staticWndProcDelegate;

    private Coroutine _flashCoroutine;
    private const int NIF_STATE = 0x0008;
    private const int NIS_HIDDEN = 0x0001;
    private const int NIM_MODIFY = 0x0001;
    private bool _isFlashing = false;
    private float _flashDuration = 0f;

    #endregion

    #region 委托定义
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    #endregion
    
#if !UNITY_EDITOR
    #region Unity生命周期
    private void Awake()
    {;
        InitializeWindow();
        InitializeTaskbarIcon();
    }

    private void OnDestroy()
    {
        DestroyTaskbarIcon();
        RestoreWindowProc();

        if (_hwnd != IntPtr.Zero)
        {
            _instances.Remove(_hwnd);
        }
    }
    #endregion

#endif
    #region 窗口管理
    private void InitializeWindow()
    {
        // 生成唯一窗口标题
        _windowTitle = $"{_baseWindowTitle} [{Process.GetCurrentProcess().Id}]";

        // 优先通过标题查找窗口
        _hwnd = FindWindow(null, _windowTitle);

        // 未找到则枚举进程窗口
        if (_hwnd == IntPtr.Zero)
        {
            FindMainWindowByProcess();
        }

        // 确保窗口标题唯一
        if (_hwnd != IntPtr.Zero)
        {
            SetWindowText(_hwnd, _windowTitle);
            SetupWindowProc();
        }
        else
        {
            Debug.LogError("窗口句柄获取失败，任务栏功能不可用");
            enabled = false;
        }
    }
    /// <summary>
    /// 开始任务栏图标闪烁
    /// </summary>
    /// <param name="duration">闪烁持续时间（秒），0表示无限</param>
    /// <param name="interval">闪烁间隔（秒）</param>
    public void StartFlashing(float duration = 0f, float interval = 0.5f)
    {
        if (_isFlashing) return;

        _isFlashing = true;
        _flashDuration = duration;

        // 启动闪烁协程
        _flashCoroutine = StartCoroutine(FlashIcon(interval));
    }

    /// <summary>
    /// 停止闪烁
    /// </summary>
    public void StopFlashing()
    {
        if (!_isFlashing) return;

        _isFlashing = false;
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }

        // 重置图标状态
        _nid.uFlags = NIF_STATE;
        _nid.dwState = 0;
        _nid.dwStateMask = NIS_HIDDEN;
        Shell_NotifyIcon(NIM_MODIFY, ref _nid);
    }

    private IEnumerator FlashIcon(float interval)
    {
        float startTime = Time.time;
        bool visibleState = true;

        while (_isFlashing)
        {
            // 切换可见状态
            visibleState = !visibleState;

            _nid.uFlags = NIF_STATE;
            _nid.dwState = visibleState ? 0 : NIS_HIDDEN;
            _nid.dwStateMask = NIS_HIDDEN;
            Shell_NotifyIcon(NIM_MODIFY, ref _nid);

            // 检查持续时间
            if (_flashDuration > 0 && (Time.time - startTime) >= _flashDuration)
            {
                StopFlashing();
                yield break;
            }

            yield return new WaitForSecondsRealtime(interval);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowEnumData
    {
        public int processId;
        public TaskbarManager manager;
        public List<IntPtr> candidates;
    }
    private void FindMainWindowByProcess()
    {
        var currentProcessId = Process.GetCurrentProcess().Id;
        var candidates = new List<IntPtr>();

        // 使用静态回调 + GCHandle 传递参数
        GCHandle gch = GCHandle.Alloc(new WindowEnumData
        {
            processId = currentProcessId,
            manager = this,
            candidates = candidates
        });

        EnumWindowsProc callback = StaticEnumWindowsCallback;
        EnumWindows(callback, GCHandle.ToIntPtr(gch));

        // 释放 GCHandle 资源
        gch.Free();

        _hwnd = candidates.Count > 0 ? candidates[0] : IntPtr.Zero;
    }
    [MonoPInvokeCallback(typeof(EnumWindowsProc))]
    private static bool StaticEnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
    {
        GCHandle gch = GCHandle.FromIntPtr(lParam);
        WindowEnumData data = (WindowEnumData)gch.Target;

        uint processId;
        GetWindowThreadProcessId(hWnd, out processId);

        if (processId == data.processId && data.manager.IsMainWindow(hWnd))
        {
            data.candidates.Add(hWnd);
        }
        return true;
    }

    private bool IsMainWindow(IntPtr hWnd)
    {
        if (GetWindowTextLength(hWnd) == 0) return false;

        IntPtr stylePtr = GetWindowLongPtr(hWnd, GWL_STYLE);
        long style = stylePtr.ToInt64();
        return (style & WS_CHILD) == 0 && (style & WS_VISIBLE) != 0;
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        return IntPtr.Size == 8 ?
            GetWindowLongPtr64(hWnd, nIndex) :
            GetWindowLong32(hWnd, nIndex);
    }
    #endregion

    private void SetupWindowProc()
    {
        _instances[_hwnd] = this;

        if (_staticWndProcDelegate == null)
        {
            _staticWndProcDelegate = StaticWndProc;
        }

        _oldWndProc = SetWindowLongPtr(
            _hwnd,
            GWL_WNDPROC,
            Marshal.GetFunctionPointerForDelegate(_staticWndProcDelegate)
        );
    }

    [MonoPInvokeCallback(typeof(WndProcDelegate))]
    private static IntPtr StaticWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (_instances.TryGetValue(hWnd, out TaskbarManager instance))
        {
            return instance.InstanceWndProc(hWnd, msg, wParam, lParam);
        }
        return CallWindowProc(IntPtr.Zero, hWnd, msg, wParam, lParam);
    }

    private IntPtr InstanceWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_SYSCOMMAND:
                HandleSystemCommand(wParam);
                return IntPtr.Zero;
            case WM_NOTIFY_TRAY:
                HandleTrayMessage(lParam);
                break;
            case WM_COMMAND:
                HandleMenuCommand(wParam.ToInt32());
                break;
        }
        return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    #region 消息处理
    private void HandleSystemCommand(IntPtr wParam)
    {
        int cmd = wParam.ToInt32();
        if (cmd == SC_MINIMIZE || cmd == SC_CLOSE)
        {
            ShowWindowAsync(_hwnd, SW_HIDE);
        }
    }

    private void HandleTrayMessage(IntPtr lParam)
    {
        int msg = lParam.ToInt32();
        switch (msg)
        {
            case WM_LBUTTONDBLCLK:
                ShowWindowAsync(_hwnd, SW_SHOW);
                break;
            case WM_RBUTTONDOWN:
                ShowContextMenu();
                break;
        }
    }
    #endregion

    #region 任务栏图标
    private void InitializeTaskbarIcon()
    {
        try
        {
            _nid = new NOTIFYICONDATA
            {
                cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                hwnd = _hwnd,
                uID = 1,
                uFlags = NIF_ICON | NIF_TIP | NIF_MESSAGE,
                uCallbackMessage = WM_NOTIFY_TRAY,
                szTip = "Unity应用 - 右键菜单"
            };

            LoadApplicationIcon();
            Shell_NotifyIcon(0x0000 /*NIM_ADD*/, ref _nid);
        }
        catch (Exception e)
        {
            Debug.LogError($"任务栏图标初始化失败: {e.Message}");
        }
    }

    private void LoadApplicationIcon()
    {
        try
        {
            // 通过Application路径推算EXE位置
            DirectoryInfo assetData = new DirectoryInfo(Application.dataPath);

            // 空值防御
            if (assetData.Parent == null)
            {
                Debug.LogError("无法获取应用程序父目录，可能处于非标准构建环境");
                throw new DirectoryNotFoundException("Application.dataPath的父目录不存在");
            }

            // 安全路径构建
            string exeName = Application.productName; // 使用Unity项目名称
            string exeFilePath = Path.Combine(assetData.Parent.FullName, $"{exeName}.exe");

            // 验证路径有效性
            if (!File.Exists(exeFilePath))
            {
                Debug.LogError($"可执行文件不存在: {exeFilePath}");
                throw new FileNotFoundException("无法找到可执行文件");
            }

            // 使用StringBuilder传递路径（容量设为MAX_PATH）
            StringBuilder exeFileSb = new StringBuilder(exeFilePath);
            Debug.Log($"图标加载路径: {exeFileSb}");

            // 提取图标
            ushort uIcon;
            _nid.hIcon = ExtractAssociatedIcon(IntPtr.Zero, exeFileSb, out uIcon);

            // 备用方案：使用系统默认图标
            if (_nid.hIcon == IntPtr.Zero)
            {
                Debug.LogWarning("提取程序图标失败，使用系统默认图标");
                _nid.hIcon = LoadIcon(IntPtr.Zero, (IntPtr)IDI_APPLICATION);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"图标加载失败: {ex}");
            // 强制使用系统默认图标保证基本功能
            _nid.hIcon = LoadIcon(IntPtr.Zero, (IntPtr)IDI_APPLICATION);
        }
    }

    private void DestroyTaskbarIcon()
    {
        if (_nid.hwnd != IntPtr.Zero)
        {
            Shell_NotifyIcon(0x0002 /*NIM_DELETE*/, ref _nid);
        }
    }
    #endregion

    #region 右键菜单
    private void ShowContextMenu()
    {
        try
        {
            POINT cursorPos;
            GetCursorPos(out cursorPos);

            IntPtr menu = CreatePopupMenu();
            AppendMenu(menu, 0x0000 /*MF_STRING*/, IDM_SHOW, "显示窗口");
            AppendMenu(menu, 0x0000 /*MF_STRING*/, IDM_HIDE, "隐藏窗口");
            AppendMenu(menu, 0x0800 /*MF_SEPARATOR*/, 0, null);
            AppendMenu(menu, 0x0000 /*MF_STRING*/, IDM_EXIT, "退出程序");

            SetForegroundWindow(_hwnd);
            TrackPopupMenuEx(menu, 0x0002 /*TPM_RIGHTBUTTON*/, cursorPos.X, cursorPos.Y, _hwnd, IntPtr.Zero);
            DestroyMenu(menu);
        }
        catch (Exception e)
        {
            Debug.LogError($"右键菜单创建失败: {e.Message}");
        }
    }

    private void HandleMenuCommand(int cmdId)
    {
        switch (cmdId)
        {
            case IDM_SHOW:
                ShowWindowAsync(_hwnd, SW_SHOW);
                break;
            case IDM_HIDE:
                ShowWindowAsync(_hwnd, SW_HIDE);
                break;
            case IDM_EXIT:
                QuitApplication();
                break;
        }
    }

    private void QuitApplication()
    {

        Application.Quit();
    }
    #endregion

    #region 清理资源
    private void RestoreWindowProc()
    {
        if (_hwnd != IntPtr.Zero && _oldWndProc != IntPtr.Zero)
        {
            SetWindowLongPtr(_hwnd, GWL_WNDPROC, _oldWndProc);
            _oldWndProc = IntPtr.Zero;
        }
    }
    #endregion
}
}