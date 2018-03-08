using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace AaronLuna.Common.Console
{
    public static class SendEnterToConsole
    {
        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        private const int VK_RETURN = 0x0D;
        private const int WM_KEYDOWN = 0x100;

        public static void Execute()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Thread.Sleep(500);

                var hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                PostMessage(hWnd, WM_KEYDOWN, VK_RETURN, 0);
            });
        }
    }
}
