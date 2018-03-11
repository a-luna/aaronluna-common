﻿namespace AaronLuna.Common.Console
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    public static class SendEnterToConsole
    {
        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        const int VK_RETURN = 0x0D;
        const int WM_KEYDOWN = 0x100;

        public static void Execute()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(500);

                var hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                PostMessage(hWnd, WM_KEYDOWN, VK_RETURN, 0);
            });
        }
    }
}
