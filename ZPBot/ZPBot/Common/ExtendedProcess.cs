using System;
using System.Runtime.InteropServices;

namespace ZPBot.Common
{
    public class ExtendedProcess
    {
        public struct Startupinfo
        {
            public uint Cb;
            public string LpReserved;
            public string LpDesktop;
            public string LpTitle;
            public uint DwX;
            public uint DwY;
            public uint DwXSize;
            public uint DwYSize;
            public uint DwXCountChars;
            public uint DwYCountChars;
            public uint DwFillAttribute;
            public uint DwFlags;
            public short WShowWindow;
            public short CbReserved2;
            public IntPtr LpReserved2;
            public IntPtr HStdInput;
            public IntPtr HStdOutput;
            public IntPtr HStdError;
        }

        public struct ProcessInformation
        {
            public IntPtr HProcess;
            public IntPtr HThread;
            public uint DwProcessId;
            public uint DwThreadId;
        }

        [Flags]
        public enum ProcessCreationFlags : uint
        {
            ZeroFlag = 0x00000000,
            CreateBreakawayFromJob = 0x01000000,
            CreateDefaultErrorMode = 0x04000000,
            CreateNewConsole = 0x00000010,
            CreateNewProcessGroup = 0x00000200,
            CreateNoWindow = 0x08000000,
            CreateProtectedProcess = 0x00040000,
            CreatePreserveCodeAuthzLevel = 0x02000000,
            CreateSeparateWowVdm = 0x00001000,
            CreateSharedWowVdm = 0x00001000,
            CreateSuspended = 0x00000004,
            CreateUnicodeEnvironment = 0x00000400,
            DebugOnlyThisProcess = 0x00000002,
            DebugProcess = 0x00000001,
            DetachedProcess = 0x00000008,
            ExtendedStartupinfoPresent = 0x00080000,
            InheritParentAffinity = 0x00010000
        }

        internal static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern bool CreateProcess(string lpApplicationName,
                   string lpCommandLine, IntPtr lpProcessAttributes,
                   IntPtr lpThreadAttributes,
                   bool bInheritHandles, ProcessCreationFlags dwCreationFlags,
                   IntPtr lpEnvironment, string lpCurrentDirectory,
                   ref Startupinfo lpStartupInfo,
                   out ProcessInformation lpProcessInformation);

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
