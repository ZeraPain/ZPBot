using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ZPBot.Common
{
    public class ClientManager : ExtendedProcess
    {
        private Process _sroProcess;
        private byte[] _fileArray;
        private const byte Push = 0x68;
        private uint _connectionStackAddr;

        public byte ReadByte(uint address)
        {
            if ((_sroProcess == null) || _sroProcess.HasExited)
                return 0;

            var buffer = new byte[sizeof(byte)];
            NativeMethods.ReadProcessMemory(_sroProcess.Handle, address, buffer, sizeof(byte), 0);

            return buffer[0];
        }

        public float ReadSingle(uint address)
        {
            if ((_sroProcess == null) || _sroProcess.HasExited)
                return 0;

            var buffer = new byte[sizeof(float)];
            NativeMethods.ReadProcessMemory(_sroProcess.Handle, address, buffer, sizeof(float), 0);

            return BitConverter.ToSingle(buffer, 0);
        }

        public bool is_running() => _sroProcess != null;

        public void Start(int localPort)
        {
            _fileArray = File.ReadAllBytes(Config.SroPath + "\\sro_client.exe");
            if (_fileArray.Length == 0)
            {
                Console.WriteLine(@"Could not read sro_client.exe");
                return;
            }

            //IntPtr Launcher = CreateMutex(IntPtr.Zero, false, "Silkroad Online Launcher");
            //IntPtr Launcher_Ready = CreateMutex(IntPtr.Zero, false, "Ready");

            PROCESS_INFORMATION pi;
            var si = new STARTUPINFO();
            var success = NativeMethods.CreateProcess(null, Config.SroPath + "\\sro_client.exe 0 /" + Client.ClientLocale + " 0 0", //Arguments = "0 /22 0 0"
                IntPtr.Zero, IntPtr.Zero, false,
                ProcessCreationFlags.CREATE_SUSPENDED,
                IntPtr.Zero, null, ref si, out pi);

            if (!success)
            {
                Console.WriteLine(@"Could not start sro_client.exe");
                return;
            }

            _sroProcess = Process.GetProcessById((int)pi.dwProcessId);

            Redirect();
            SetLocalConnection(localPort);
            StartMsg("Welcome to ZPBot (v." + Config.Version + ")!\ncopyright © 2016 by ZeraPain");
            MultiClient();
            Zoomhack();
            SwearFilter();
            NudePatch();
            SeedPatch();

            NativeMethods.ResumeThread(pi.hThread);
            NativeMethods.ResumeThread(pi.hProcess);
        }

        public void SetLocalConnection(int localPort)
        {
            if (_connectionStackAddr == 0) return;

            var port = BitConverter.GetBytes(localPort);
            byte[] connection =
            {
                0x02, 0x00,
                port[1], port[0], // PORT
                0x7F, 0x00, 0x00, 0x01 // IP (127.0.0.1)
            };

            NativeMethods.WriteProcessMemory(_sroProcess.Handle, _connectionStackAddr, connection, connection.Length, 0);
        }

        private void Redirect()
        {
            if ((_sroProcess == null) || _sroProcess.HasExited)
                return;

            byte[] redirectIpAddressPattern = { 0x89, 0x86, 0x2C, 0x01, 0x00, 0x00, 0x8B, 0x17, 0x89, 0x56, 0x50, 0x8B, 0x47, 0x04, 0x89, 0x46, 0x54, 0x8B, 0x4F, 0x08, 0x89, 0x4E, 0x58, 0x8B, 0x57, 0x0C, 0x89, 0x56, 0x5C, 0x5E, 0xB8, 0x01, 0x00, 0x00, 0x00, 0x5D, 0xC3 };
            var results = FindPattern(redirectIpAddressPattern, _fileArray);
            if (results.Count != 1)
            {
                Console.WriteLine(@"Redirect: {0} results were returned. Only {1} were expected. Please use an updated signature.", results.Count, 1);
                return;
            }

            var redirectIpAddr = results[0] - 0x35;
            _connectionStackAddr = NativeMethods.VirtualAllocEx(_sroProcess.Handle, IntPtr.Zero, 8, 0x1000, 0x4);
            var connectionStackArray = BitConverter.GetBytes(_connectionStackAddr);

            var codecave = NativeMethods.VirtualAllocEx(_sroProcess.Handle, IntPtr.Zero, 16, 0x1000, 0x4);
            var codecaveArray = BitConverter.GetBytes(codecave - redirectIpAddr - 0x05);
            byte[] codeCaveFunc = {
                                      0xBF,connectionStackArray[0],connectionStackArray[1],connectionStackArray[2],connectionStackArray[3],
                                      0x8B,0x4E,0x04,
                                      0x6A,0x10,
                                      0x68,0xA6,0x08,0x4B,0x00,
                                      0xC3
                                  };
            byte[] jmpCodeCave = { 0xE9, codecaveArray[0], codecaveArray[1], codecaveArray[2], codecaveArray[3] };

            NativeMethods.WriteProcessMemory(_sroProcess.Handle, codecave, codeCaveFunc, codeCaveFunc.Length, 0);
            NativeMethods.WriteProcessMemory(_sroProcess.Handle, redirectIpAddr, jmpCodeCave, jmpCodeCave.Length, 0);
        }

        private void Zoomhack()
        {
            if ((_sroProcess == null) || _sroProcess.HasExited)
                return;

            byte[] zoomhackPattern = { 0xDF, 0xE0, 0xF6, 0xC4, 0x41, 0x7A, 0x08, 0xD9, 0x9E };
            var results = FindPattern(zoomhackPattern, _fileArray);
            if (results.Count != 2)
            {
                Console.WriteLine(@"Zoomhack: {0} results were returned. Only {1} were expected. Please use an updated signature.", results.Count, 2);
                return;
            }

            var zoomHackAddr = (uint) (results[1] + zoomhackPattern.Length - 4);
            byte[] patch = { 0xEB };

            NativeMethods.WriteProcessMemory(_sroProcess.Handle, zoomHackAddr, patch, patch.Length, 0);
        }

        private void SwearFilter()
        {
            if ((_sroProcess == null) || _sroProcess.HasExited)
                return;

            var swearFilterStringPattern = Encoding.Unicode.GetBytes("UIIT_MSG_CHATWND_MESSAGE_FILTER");
            var results = FindStringPattern(swearFilterStringPattern, _fileArray, Push);
            if (results.Count < 4)
            {
                Console.WriteLine(@"SwearFilter: {0} results were returned. {1} or more were expected. Please use an updated signature.", results.Count, 4);
                return;
            }

            byte[] patch = { 0xEB };
            for (var i = 0; i < 4; i++)
                NativeMethods.WriteProcessMemory(_sroProcess.Handle, results[i] - 0x2, patch, patch.Length, 0);
        }

        private void NudePatch()
        {
            byte[] nudePatchPattern = { 0x8B, 0x84, 0xEE, 0x1C, 0x01, 0x00, 0x00, 0x3B, 0x44, 0x24, 0x14 };
            var results = FindPattern(nudePatchPattern, _fileArray);
            if (results.Count != 1)
            {
                Console.WriteLine(@"NudePatch: {0} results were returned. Only {1} were expected. Please use an updated signature.", results.Count, 1);
                return;
            }

            byte[] patch = { 0x90, 0x90 };
            NativeMethods.WriteProcessMemory(_sroProcess.Handle, (uint)(results[0] + nudePatchPattern.Length), patch, patch.Length, 0);
        }

        private void StartMsg(string startingText)
        {
            if ((_sroProcess == null) || _sroProcess.HasExited)
                return;

            var startingMsgStringPattern = Encoding.Unicode.GetBytes("UIIT_STT_STARTING_MSG");
            var results = FindStringPattern(startingMsgStringPattern, _fileArray, Push);
            if (results.Count == 0)
            {
                Console.WriteLine(@"Unable to get startMsgAddr");
                return;
            }

            var startMsgAddr = results[0] + 0x18;

            var bytes = Encoding.Unicode.GetBytes(startingText);
            var codecave = NativeMethods.VirtualAllocEx(_sroProcess.Handle, IntPtr.Zero, 16, 0x1000, 0x4);
            var codeCaveArray = BitConverter.GetBytes(codecave);
            var jmpCodeCave = new byte[] { 0xb8, codeCaveArray[0], codeCaveArray[1], codeCaveArray[2], codeCaveArray[3] };
            byte[] hexColor = { 0xFF, 0xAE, 0xC3, 0xFF };

            NativeMethods.WriteProcessMemory(_sroProcess.Handle, codecave, bytes, bytes.Length, 0);
            NativeMethods.WriteProcessMemory(_sroProcess.Handle, startMsgAddr, jmpCodeCave, jmpCodeCave.Length, 0);
            NativeMethods.WriteProcessMemory(_sroProcess.Handle, startMsgAddr + 0x9, hexColor, hexColor.Length, 0);
        }

        private void SeedPatch()
        {
            byte[] seedPatchPattern = { 0x8B, 0x4C, 0x24, 0x04, 0x81, 0xE1, 0xFF, 0xFF, 0xFF, 0x7F };
            var results = FindPattern(seedPatchPattern, _fileArray);
            if (results.Count != 1)
            {
                Console.WriteLine(@"SeedPatch: {0} results were returned. Only {1} were expected. Please use an updated signature.", results.Count, 1);
                return;
            }

            byte[] patch = { 0xB9, 0x33, 0x00, 0x00, 0x00, 0x90, 0x90, 0x90, 0x90, 0x90 };
            NativeMethods.WriteProcessMemory(_sroProcess.Handle, results[0], patch, patch.Length, 0);
        }

        private void MultiClient()
        {
            if ((_sroProcess == null) || _sroProcess.HasExited)
                return;

            byte[] patch = { 0xEB };

            //Already Program Exe
            var alreadyProgramExeStringPattern = Encoding.ASCII.GetBytes("//////////////////////////////////////////////////////////////////");
            var results = FindStringPattern(alreadyProgramExeStringPattern, _fileArray, Push);
            if (results.Count == 0)
            {
                Console.WriteLine(@"Unable to get alreadyProgramExeAddr");
                return;
            }

            var alreadyProgramExeAddr = results[0] - 0x2;
            NativeMethods.WriteProcessMemory(_sroProcess.Handle, alreadyProgramExeAddr, patch, patch.Length, 0);


            //Multiclient Error MessageBox
            var multiClientErrorStringPattern = Encoding.Default.GetBytes("½ÇÅ©·Îµå°¡ ÀÌ¹Ì ½ÇÇà Áß ÀÔ´Ï´Ù.");
            results = FindStringPattern(multiClientErrorStringPattern, _fileArray, Push);
            if (results.Count == 0)
            {
                Console.WriteLine(@"Unable to get multiClientErrorAddr");
                return;
            }

            var multiClientErrorAddr = results[0] - 0x8;
            NativeMethods.WriteProcessMemory(_sroProcess.Handle, multiClientErrorAddr, patch, patch.Length, 0);


            //Mac Address
            byte[] macAddressPattern = { 0x6A, 0x06, 0x8D, 0x44, 0x24, 0x48, 0x50, 0x8B, 0xCF };
            results = FindPattern(macAddressPattern, _fileArray);
            if (results.Count != 1)
            {
                Console.WriteLine(@"Mac Address: {0} results were returned. Only {1} were expected. Please use an updated signature.", results.Count, 1);
                return;
            }

            var macAddrAddr = results[0] + 0x9;


            //Callforward Address
            byte[] callForwardPattern = { 0x56, 0x8B, 0xF1, 0x0F, 0xB7, 0x86, 0x3E, 0x10, 0x00, 0x00, 0x57, 0x66, 0x8B, 0x7C, 0x24, 0x10, 0x0F, 0xB7, 0xCF, 0x8D, 0x14, 0x01, 0x3B, 0x96, 0x4C, 0x10, 0x00, 0x00 };
            var results1 = FindPattern(callForwardPattern, _fileArray);
            if (results1.Count != 1)
            {
                Console.WriteLine(@"Callforward Address: {0} results were returned. Only {1} were expected. Please use an updated signature.", results1.Count, 1);
                return;
            }

            var callforwardAddr = results1[0];


            //Patch
            var multiClientCodeCave = NativeMethods.VirtualAllocEx(_sroProcess.Handle, IntPtr.Zero, 45, 0x1000, 0x4);
            var macCodeCave = NativeMethods.VirtualAllocEx(_sroProcess.Handle, IntPtr.Zero, 4, 0x1000, 0x4);
            var gtc = NativeMethods.GetProcAddress(NativeMethods.GetModuleHandle("kernel32.dll"), "GetTickCount");

            var callBack = BitConverter.GetBytes(multiClientCodeCave + 41);
            var callForward = BitConverter.GetBytes(callforwardAddr - multiClientCodeCave - 34);
            var macAddress = BitConverter.GetBytes(macCodeCave);
            var gtcAddress = BitConverter.GetBytes(gtc - multiClientCodeCave - 18);

            var multiClientArray = BitConverter.GetBytes(multiClientCodeCave - macAddrAddr - 5);
            byte[] multiClientCodeArray = { 0xE8, multiClientArray[0], multiClientArray[1], multiClientArray[2], multiClientArray[3] };

            byte[] multiClientCode = {   0x8F, 0x05, callBack[0], callBack[1], callBack[2], callBack[3], //POP DWORD PTR DS:[xxxxxxxx]
                                         0xA3, macAddress[0], macAddress[1], macAddress[2], macAddress[3], //MOV DWORD PTR DS:[xxxxxxxx],EAX
                                         0x60, //PUSHAD
                                         0x9C, //PUSHFD
                                         0xE8, gtcAddress[0], gtcAddress[1], gtcAddress[2], gtcAddress[3], // Call KERNEL32.gettickcount
                                         0x8B, 0x0D, macAddress[0], macAddress[1], macAddress[2], macAddress[3], //MOV ECX,DWORD PTR DS:[xxxxxxxx]
                                         0x89, 0x41, 0x02, // MOV DWORD PTR DS:[ECX+2],EAX
                                         0x9D, //POPFD
                                         0x61, //POPAD
                                         0xE8, callForward[0], callForward[1], callForward[2], callForward[3], //CALL xxxxxxxx
                                         0xFF, 0x35, callBack[0], callBack[1], callBack[2], callBack[3], // PUSH DWORD PTR DS:[xxxxxxxx]
                                         0xC3 //RETN
                                       };

            NativeMethods.WriteProcessMemory(_sroProcess.Handle, multiClientCodeCave, multiClientCode, multiClientCode.Length, 0);
            NativeMethods.WriteProcessMemory(_sroProcess.Handle, macAddrAddr, multiClientCodeArray, multiClientCodeArray.Length, 0);
        }

        public bool Hide()
        {
            if (NativeMethods.IsWindowVisible(_sroProcess.MainWindowHandle))
            {
                NativeMethods.ShowWindowAsync(_sroProcess.MainWindowHandle, 0);
                return true;
            }

            NativeMethods.ShowWindowAsync(_sroProcess.MainWindowHandle, 1);
            return false;
        }

        public void Kill()
        {
            if ((_sroProcess == null) || _sroProcess.HasExited)
                return;

            _sroProcess.Kill();
        }

        private static List<uint> FindStringPattern(ICollection<byte> stringByteArray, IList<byte> fileArray, byte stringWorker)
        {
            var results = FindPattern(stringByteArray, fileArray);
            if (results.Count == 0)
                return new List<uint>();

            var bytes = BitConverter.GetBytes(results[0]);
            byte[] pattern = { stringWorker, bytes[0], bytes[1], bytes[2], bytes[3] };

            return FindPattern(pattern, fileArray);
        }

        private static List<uint> FindPattern(ICollection<byte> pattern, IList<byte> fileByteArray)
        {
            var results = new List<uint>();

            for (uint i = 0; i < fileByteArray.Count - pattern.Count; i++)
            {
                var flag = !pattern.Where((t, j) => fileByteArray[(int) (i + j)] != t).Any();
                if (flag)
                {
                    results.Add(0x400000 + i);
                }
            }

            return results;
        }
    }
}
