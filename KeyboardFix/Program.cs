using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using System.Windows.Forms;

namespace KeyboardLogger
{
    class Program
    {
        // Windows API constants
        private const int WH_KEYBOARD_LL = 13; // Low-level keyboard hook
        private const int WM_KEYDOWN = 0x0100; // Key down event
        private const int WM_KEYUP = 0x0101;

        // Delegate for the hook callback function
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        // The callback function that will be called by the OS
        private static LowLevelKeyboardProc _proc = HookCallback;

        // Handle for the hook
        private static IntPtr _hookID = IntPtr.Zero;

        // Track key states and last press time
        private static readonly Dictionary<Keys, DateTime> keyDownTimes = new Dictionary<Keys, DateTime>();
        private static readonly HashSet<Keys> currentlyPressed = new HashSet<Keys>();

        private static Dictionary<string, DateTime> pressedKyes = new Dictionary<string, DateTime>();   

        private const int KEY_PRESS_THRESHOLD_MS = 50; // Minimum time between key presses to consider them separate

        public static void Main()
        {
            // Set the hook
            _hookID = SetHook(_proc);

            Console.WriteLine("Logging keyboard input in the background. Press ESC to exit.");

            // To make the application truly invisible, you can hide the console window.
            // Uncomment the two lines below to hide this window.
            //IntPtr handle = GetConsoleWindow();
            //ShowWindow(handle, 0); // 0 = SW_HIDE

            // Keep the application running. A more robust application might use a message loop,
            // but for this simple logger, this is sufficient.
            Application.Run();
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                // Set a global hook for all threads on the desktop
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
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
                        Console.WriteLine($"Key {key} pressed once.");
                    }
                    else
                    {
                        // Key is being held (repeat)
                        var heldFor = DateTime.Now - keyDownTimes[key];
                        Console.WriteLine($"Key {key} still held for {heldFor.TotalMilliseconds:F0} ms.");
                        return CallNextHookEx(_hookID, nCode, wParam, lParam);
                    }
                    if (pressedKyes.TryGetValue(key.ToString(), out DateTime lastPressedTime))
                    {
                        var timeSinceLastPress = DateTime.Now - lastPressedTime;
                        if (timeSinceLastPress.TotalMilliseconds < KEY_PRESS_THRESHOLD_MS)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Blocking key press: {key}");
                            Console.ResetColor();
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
                ConsoleKey consoleKey = (ConsoleKey)vkCode;
                if (consoleKey == ConsoleKey.Escape)
                {
                    Console.WriteLine("Escape key pressed. Unhooking and exiting...");
                    UnhookWindowsHookEx(_hookID);
                    Application.Exit();
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        //{
        //    if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        //    {
        //        bool isKeyDown = (lParam.ToInt32() & (1 << 30)) != 0;

        //        if (isKeyDown)
        //        {
        //            Console.WriteLine($"Was down");
        //            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        //        }

        //        int vkCode = Marshal.ReadInt32(lParam);
        //        ConsoleKey key = (ConsoleKey)vkCode;
        //        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //        string keyString = key.ToString();
        //        Console.WriteLine($"key press: {keyString}");
        //        if (pressedKyes.TryGetValue(keyString, out long lastPressedTime))
        //        {
        //            if (currentTime - lastPressedTime > KEY_PRESS_THRESHOLD_MS)
        //            {
        //                Console.WriteLine($"Blocking key press: {keyString}");
        //                return (IntPtr)1; // Block the input
        //            }

        //            pressedKyes[keyString] = currentTime;
        //        }
        //        else
        //        {
        //            pressedKyes.Add(keyString, currentTime);
        //        }


        //        if (key == ConsoleKey.Escape)
        //        {
        //            Console.WriteLine("Escape key pressed. Unhooking and exiting...");
        //            UnhookWindowsHookEx(_hookID);
        //            Application.Exit();
        //        }
        //    }

        //    return CallNextHookEx(_hookID, nCode, wParam, lParam);
        //}



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

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #endregion
    }
}