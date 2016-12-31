using System;
using System.Runtime.InteropServices;

namespace ZPBot.Common
{
    public class ExtendedProcess
    {
        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [Flags]
        public enum ProcessCreationFlags : uint
        {
            ZERO_FLAG = 0x00000000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00001000,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000
        }

        internal static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern bool CreateProcess(string lpApplicationName,
                   string lpCommandLine, IntPtr lpProcessAttributes,
                   IntPtr lpThreadAttributes,
                   bool bInheritHandles, ProcessCreationFlags dwCreationFlags,
                   IntPtr lpEnvironment, string lpCurrentDirectory,
                   ref STARTUPINFO lpStartupInfo,
                   out PROCESS_INFORMATION lpProcessInformation);

            [DllImport("kernel32.dll")]
            internal static extern uint ResumeThread(IntPtr hThread);

            [DllImport("kernel32.dll")]
            internal static extern bool WriteProcessMemory(IntPtr hProcess, uint lpBaseAddress, byte[] lpBuffer,
                int nSize, uint lpNumberOfBytesWritten);

            [DllImport("kernel32.dll")]
            internal static extern bool ReadProcessMemory(IntPtr hProcess, uint lpBaseAddress, byte[] lpBuffer,
                int nSize, uint lpNumberOfBytesWritten);

            [DllImport("kernel32.dll")]
            internal static extern uint VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize,
                uint flAllocationType, uint flProtect);

            [DllImport("kernel32")]
            internal static extern uint GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32.dll")]
            internal static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("user32.dll")]
            internal static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll")]
            internal static extern bool IsWindowVisible(IntPtr hWnd);
        }

    }
}
