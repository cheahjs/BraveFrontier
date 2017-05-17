using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BfMemEdit
{
    public partial class frmMain : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer,
                                                     int dwSize, out int lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize,
                                                      out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtectEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            int nSize,      //UIntPtr dwSize,
            uint flNewProtect,
            out uint lpflOldProtect);

        public enum Protection : uint
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        private readonly byte[] _platformPattern =
        {
            0x83,
            0xEC,
            0x18,
            0xBA,
            0x07,
            0x00,
            0x00,
            0x00,
            0x8B,
            0xCC,
            0xE8,
            0x00,
            0x00,
            0x00,
            0x00,
            0x68
        };
        private readonly string _platformMask = "xxxxx???xxx????x";
        private static byte[] _battleSpeedPattern = Encoding.UTF8.GetBytes("battle_speed\0\0\0\0");
        private static string _battleSpeedMask = "xxxxxxxxxxxxxxxx";
        private static byte[] _resummonPattern = Encoding.UTF8.GetBytes("sg_resummon_gacha_enable\0");
        private static string _resummonMask = new String('x', _resummonPattern.Length);
        private static byte[] _freegemsPattern = Encoding.UTF8.GetBytes("freepaid_gems\0");
        private static string _freegemsMask = new String('x', _freegemsPattern.Length);

        private List<IntPtr> _platformPtrs = new List<IntPtr>();
        private IntPtr _battleSpeedPtr = IntPtr.Zero;
        private IntPtr _resummonPtr = IntPtr.Zero;
        private IntPtr _guildPtr = IntPtr.Zero;
        private IntPtr _guildVisiblePtr = IntPtr.Zero;
        private IntPtr _freeGemsPtr = IntPtr.Zero;
        private Process _bfProcess;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            cmbPlatform.SelectedIndex = 0;
        }

        private void btnAttach_Click(object sender, EventArgs e)
        {
            var processes = Process.GetProcessesByName("BraveFrontier.Windows");
            switch (processes.Length)
            {
                case 0:
                    lblStatus.Text = "BF was not found.";
                    return;
                case 1:
                    lblStatus.Text = "BF was found, attaching.";
                    break;
                case 2:
                    lblStatus.Text = "Multiple processes found, skipping.";
                    return;
            }
            var bfProcess = processes[0];
            var sigScan = new SigScan(bfProcess, bfProcess.MainModule.BaseAddress, bfProcess.MainModule.ModuleMemorySize);//0x3FFFFF);
            _platformPtrs = sigScan.FindAllPattern(_platformPattern, _platformMask, 4);
            if (_platformPtrs.Count != 9)
            {
                lblStatus.Text = string.Format("Warning, platform pointers is {0}, expected 9.", _platformPtrs.Count);
                return;
            }
            _battleSpeedPtr = sigScan.FindPattern(_battleSpeedPattern, _battleSpeedMask, 0);
            if (_battleSpeedPtr == IntPtr.Zero)
            {
                lblStatus.Text = "Unable to find battle_speed text";
                return;
            }
            _resummonPtr = sigScan.FindPattern(_resummonPattern, _resummonMask, 0);
            if (_resummonPtr == IntPtr.Zero)
            {
                lblStatus.Text = "Unable to find sg_resummon_gacha_enable text";
                return;
            }
            _guildPtr = sigScan.FindPattern("guild\0", "", 0);
            if (_guildPtr == IntPtr.Zero)
            {
                lblStatus.Text = "Unable to find guild text";
                return;
            }
            _guildVisiblePtr = sigScan.FindPattern("guild_visible\0", "", 0);
            if (_guildVisiblePtr == IntPtr.Zero)
            {
                lblStatus.Text = "Unable to find guild_visible text";
                return;
            }
            _freeGemsPtr = sigScan.FindPattern("freepaid_gems\0", "", 0);
            if (_freeGemsPtr == IntPtr.Zero)
            {
                lblStatus.Text = "Unable to find gems text";
                return;
            }
            lblStatus.Text = "Attached to BF.";
            _bfProcess = bfProcess;
            btnApply.Enabled = true;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            byte platformByte = 0x07;
            switch (cmbPlatform.SelectedIndex)
            {
                case 0:
                    platformByte = 0x07;
                    break;
                case 3:
                    platformByte = 0x01;
                    break;
                case 1:
                    platformByte = 0x02;
                    break;
                case 2:
                    platformByte = 0x03;
                    break;
            }
            foreach (var address in _platformPtrs)
            {
                WriteProcessMemory(address, new[] {platformByte});
            }
            WriteProcessMemory(_battleSpeedPtr, Encoding.UTF8.GetBytes("raid\0"));
            //WriteProcessMemory(_resummonPtr, Encoding.UTF8.GetBytes("raid\0"));
            WriteProcessMemory(_guildPtr, Encoding.UTF8.GetBytes("raid\0"));
            WriteProcessMemory(_guildVisiblePtr, Encoding.UTF8.GetBytes("raid\0"));
            WriteProcessMemory(_freeGemsPtr, Encoding.UTF8.GetBytes("raid\0"));
            lblStatus.Text = $"Applied {(string) cmbPlatform.Items[cmbPlatform.SelectedIndex]} platform and changed battle_speed.";
        }

        #region Memory Functions

        private void WriteProcessMemory(IntPtr hProcess, float lpBaseAddress)
        {
            WriteProcessMemory(hProcess, BitConverter.GetBytes(lpBaseAddress));
        }

        private void WriteProcessMemory(IntPtr hProcess, float lpBaseAddress, int[] lpBuffer)
        {
            WriteProcessMemory(hProcess, BitConverter.GetBytes(lpBaseAddress), lpBuffer);
        }

        private void WriteProcessMemory(IntPtr pointer, byte[] bytes, int[] offsets)
        {
            int a = 0;
            for (int i = 0; i < offsets.Length; i++)
            {
                var buffer = new byte[4];
                ReadProcessMemory(_bfProcess.Handle, pointer, buffer, 4, out a);
                pointer = (IntPtr)BitConverter.ToInt32(buffer, 0);
                pointer += offsets[i];
            }
            WriteProcessMemory(pointer, bytes);
        }

        private void WriteProcessMemory(IntPtr pointer, byte[] bytes)
        {
            int a = 0;
            uint b = 0;
            uint old = 0;
            VirtualProtectEx(_bfProcess.Handle, pointer, bytes.Length, (uint) Protection.PAGE_EXECUTE_READWRITE, out old);
            WriteProcessMemory(_bfProcess.Handle, pointer, bytes, (uint)bytes.Length, out a);
            VirtualProtectEx(_bfProcess.Handle, pointer, bytes.Length, old, out b);
        }

        private int ReadInt(IntPtr pointer)
        {
            byte[] bytes = ReadProcessMemory(pointer, 4);
            return BitConverter.ToInt32(bytes, 0);
        }

        private float ReadFloat(IntPtr pointer, int[] offsets)
        {
            byte[] bytes = ReadProcessMemory(pointer, offsets, 4);
            return BitConverter.ToSingle(bytes, 0);
        }

        private byte[] ReadProcessMemory(IntPtr pointer, int[] offsets, int size)
        {
            int a = 0;
            for (int i = 0; i < offsets.Length; i++)
            {
                var buffer = new byte[4];
                ReadProcessMemory(_bfProcess.Handle, pointer, buffer, 4, out a);
                pointer = (IntPtr)BitConverter.ToInt32(buffer, 0);
                pointer += offsets[i];
            }
            var retnbuffer = new byte[size];
            ReadProcessMemory(_bfProcess.Handle, pointer, retnbuffer, size, out a);
            return retnbuffer;
        }

        private byte[] ReadProcessMemory(IntPtr pointer, int size)
        {
            int a = 0;
            var retnbuffer = new byte[size];
            ReadProcessMemory(_bfProcess.Handle, pointer, retnbuffer, size, out a);
            return retnbuffer;
        }

        #endregion
    }
}
