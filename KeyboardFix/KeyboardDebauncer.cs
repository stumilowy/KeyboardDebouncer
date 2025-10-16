using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyboardFix
{
    internal class KeyboardDebauncer
    {
        private LogsWindow logsWindow;
        private readonly DataContainer dataContainer;
        // Windows API constants
        private const int WH_KEYBOARD_LL = 13; // Low-level keyboard hook
        private const int WM_KEYDOWN = 0x0100; // Key down event
        private const int WM_KEYUP = 0x0101;


        // Delegate for the hook callback function
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        // The callback function that will be called by the OS
        private LowLevelKeyboardProc _proc;

        // Handle for the hook
        private IntPtr _hookID = IntPtr.Zero;

        // Track key states and last press time
        private readonly Dictionary<Keys, DateTime> keyDownTimes = new Dictionary<Keys, DateTime>();
        private readonly HashSet<Keys> currentlyPressed = new HashSet<Keys>();

        private Dictionary<string, DateTime> pressedKyes = new Dictionary<string, DateTime>();

        private int totalBlocks = 0;

        public static int KEY_PRESS_THRESHOLD_MS = 50; // Minimum time between key presses to consider them separate

        // Constructor to initialize the _proc field
        public KeyboardDebauncer(LogsWindow logsWindow, DataContainer dataContainer)
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
            this.logsWindow = logsWindow;
            this.dataContainer = dataContainer;
        }

        public void SetKeyPressThreshold(int threshold)
        {
            KEY_PRESS_THRESHOLD_MS = threshold;
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                // Set a global hook for all threads on the desktop
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    if (!currentlyPressed.Contains(key))
                    {
                        // First press
                        currentlyPressed.Add(key);
                        keyDownTimes[key] = DateTime.Now;
                        logsWindow.AppendLog($"Key {key} pressed once.");
                    }
                    else
                    {
                        // Key is being held (repeat)
                        var heldFor = DateTime.Now - keyDownTimes[key];
                        logsWindow.AppendLog($"Key {key} still held for {heldFor.TotalMilliseconds:F0} ms.",Color.Orange);
                        return CallNextHookEx(_hookID, nCode, wParam, lParam);
                    }
                    if (pressedKyes.TryGetValue(key.ToString(), out DateTime lastPressedTime))
                    {
                        var timeSinceLastPress = DateTime.Now - lastPressedTime;
                        if (timeSinceLastPress.TotalMilliseconds < KEY_PRESS_THRESHOLD_MS)
                        {
                            totalBlocks++;
                            logsWindow.AppendLog($"Blocking key press: {key}", Color.Red);
                            dataContainer.IncrementBlockCount(key.ToString());
                            SendBlockedCountToLog();
                            return (IntPtr)1; // Block the input
                        }
                        pressedKyes[key.ToString()] = DateTime.Now;
                    }
                    else
                    {
                        pressedKyes.Add(key.ToString(), DateTime.Now);
                    }
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    if (currentlyPressed.Contains(key))
                    {
                        var heldDuration = DateTime.Now - keyDownTimes[key];
                        Console.WriteLine($"Key {key} released after {heldDuration.TotalMilliseconds:F0} ms.");
                        currentlyPressed.Remove(key);
                        keyDownTimes.Remove(key);
                    }
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void SendBlockedCountToLog()
        {
            logsWindow.UpdateTotalBlocksLabel(totalBlocks);
        }

        public void setThreshold(int threshold)
        {
            KEY_PRESS_THRESHOLD_MS = threshold;
        }

        #region Windows API Imports

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion
    }
}