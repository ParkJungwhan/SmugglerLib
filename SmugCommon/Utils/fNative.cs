using System.Diagnostics;
using System.Runtime.InteropServices;
using SmugCommon.OS;

namespace SmugCommon.Utils
{
    public class fNative
    {
        #region ConsolePosition

        private const int SWP_NOZORDER = 0x4;
        private const int SWP_NOACTIVATE = 0x10;

        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern IntPtr GetConsoleWindow();

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        public static IntPtr Handle
        {
            get
            {
                return GetConsoleWindow();
            }
        }

        private const uint ENABLE_QUICK_EDIT = 0x0040;

        // is the standard input device.
        private const int STD_INPUT_HANDLE = -10;

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        #endregion ConsolePosition

        //[Conditional("Debug")]
        public static void SetPosition(int x, int y, int cx, int cy)
        {
            SetWindowPos(Handle, IntPtr.Zero, x, y, cx, cy, SWP_NOZORDER | SWP_NOACTIVATE);
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            // get current console mode
            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                // ERROR: Unable to get console mode.
                return;
            }

            // Clear the quick edit bit in the mode flags
            consoleMode &= ~ENABLE_QUICK_EDIT;

            // set the new mode
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode
                return;
            }
        }

        public static void SetDumpWrite()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        // Dump
        [DllImport("dbghelp.dll")]
        public static extern bool MiniDumpWriteDump(IntPtr hProcess, Int32 ProcessId, IntPtr hFile, int DumpType, IntPtr ExceptionParam, IntPtr UserStreamParam, IntPtr CallackParam);

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CreateMiniDump();
        }

        private static void CreateMiniDump()
        {
            using (FileStream fs = new FileStream("GameServer.dmp", FileMode.Create))
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    MiniDumpWriteDump(process.Handle, process.Id, fs.SafeFileHandle.DangerousGetHandle(), MinidumpType.MiniDumpNormal, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                }
            }
        }
    }
}