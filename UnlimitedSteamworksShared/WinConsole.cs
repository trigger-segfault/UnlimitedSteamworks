using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace UnlimitedSteamworks {
	internal static class WinConsole {
		private static Version Windows7 => new Version(6, 1);
		private static Version Windows8 => new Version(6, 2);
		private static Version Windows81 => new Version(6, 3);
		private static Version Windows10 => new Version(10, 0);

		public static void Initialize() {
			//TODO: Figure out which side of the fence Windows 8 lies on.

			if (Environment.OSVersion.Version < Windows8) {
				Initialize7();
			}
			else {
				Initialize10();
			}
		}
		
		private static void Initialize7() {
			SafeFileHandle hStdIn, hStdOut, hStdErr, hStdInDup, hStdOutDup, hStdErrDup;
			BY_HANDLE_FILE_INFORMATION bhfi;
			hStdIn = GetStdHandle(STD_INPUT_HANDLE);
			hStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
			hStdErr = GetStdHandle(STD_ERROR_HANDLE);
			// Get current process handle
			IntPtr hProcess = Process.GetCurrentProcess().Handle;
			// Duplicate Stdin handle to save initial value
			DuplicateHandle(hProcess, hStdIn, hProcess, out hStdInDup, 0, true, DUPLICATE_SAME_ACCESS);
			// Duplicate Stdout handle to save initial value
			DuplicateHandle(hProcess, hStdOut, hProcess, out hStdOutDup, 0, true, DUPLICATE_SAME_ACCESS);
			// Duplicate Stderr handle to save initial value
			DuplicateHandle(hProcess, hStdErr, hProcess, out hStdErrDup, 0, true, DUPLICATE_SAME_ACCESS);
			// Attach to console window – this may modify the standard handles
			AllocConsole();
			// Adjust the standard handles
			if (GetFileInformationByHandle(GetStdHandle(STD_INPUT_HANDLE), out bhfi))
				SetStdHandle(STD_INPUT_HANDLE, hStdInDup);
			else
				SetStdHandle(STD_INPUT_HANDLE, hStdIn);

			if (GetFileInformationByHandle(GetStdHandle(STD_OUTPUT_HANDLE), out bhfi))
				SetStdHandle(STD_OUTPUT_HANDLE, hStdOutDup);
			else
				SetStdHandle(STD_OUTPUT_HANDLE, hStdOut);

			if (GetFileInformationByHandle(GetStdHandle(STD_ERROR_HANDLE), out bhfi))
				SetStdHandle(STD_ERROR_HANDLE, hStdErrDup);
			else
				SetStdHandle(STD_ERROR_HANDLE, hStdErr);

			// Reopen StdOut
			var writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
			Console.SetOut(writer);
			Console.SetError(writer);
			var reader = new StreamReader(Console.OpenStandardInput());
			Console.SetIn(reader);
		}

		private static void Initialize10(bool alwaysCreateNewConsole = true) {
			AllocConsole();
			
			var ofs = CreateFileStream("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, FileAccess.Write);
			if (ofs != null) {
				var writer = new StreamWriter(ofs) { AutoFlush = true };
				Console.SetOut(writer);
				Console.SetError(writer);
			}
			var ifs = CreateFileStream("CONIN$", GENERIC_READ, FILE_SHARE_READ, FileAccess.Read);
			if (ifs != null) {
				Console.SetIn(new StreamReader(ifs));
			}
		}

		private static FileStream CreateFileStream(string name, uint win32DesiredAccess, uint win32ShareMode,
			FileAccess dotNetFileAccess)
		{
			var file = new SafeFileHandle(CreateFile(name, win32DesiredAccess, win32ShareMode, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero), true);
			if (!file.IsInvalid) {
				var fs = new FileStream(file, dotNetFileAccess);
				return fs;
			}
			return null;
		}

		#region Win API Functions and Constants
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool AllocConsole();
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool AttachConsole(
			uint dwProcessId);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool GetFileInformationByHandle(
			SafeFileHandle hFile,
			out BY_HANDLE_FILE_INFORMATION lpFileInformation);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern SafeFileHandle GetStdHandle(
			uint nStdHandle);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetStdHandle(
			uint nStdHandle,
			SafeFileHandle hHandle);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetStdHandle(
			uint nStdHandle,
			IntPtr hHandle);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool DuplicateHandle(
			IntPtr hSourceProcessHandle,
			SafeFileHandle hSourceHandle,
			IntPtr hTargetProcessHandle,
			out SafeFileHandle lpTargetHandle,
			uint dwDesiredAccess,
			bool bInheritHandle,
			uint dwOptions);
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr CreateFile(
			  string lpFileName,
			  uint dwDesiredAccess,
			  uint dwShareMode,
			  IntPtr lpSecurityAttributes,
			  uint dwCreationDisposition,
			  uint dwFlagsAndAttributes,
			  IntPtr hTemplateFile);

		private struct BY_HANDLE_FILE_INFORMATION {
			public uint FileAttributes;
			public FILETIME CreationTime;
			public FILETIME LastAccessTime;
			public FILETIME LastWriteTime;
			public uint VolumeSerialNumber;
			public uint FileSizeHigh;
			public uint FileSizeLow;
			public uint NumberOfLinks;
			public uint FileIndexHigh;
			public uint FileIndexLow;
		}

		private static readonly IntPtr DEFAULT_STD_IN = new IntPtr(8);
		private static readonly IntPtr DEFAULT_STD_OUT = new IntPtr(7);
		private static readonly IntPtr DEFAULT_STD_ERR = new IntPtr(6);


		private const uint GENERIC_WRITE = 0x40000000;
		private const uint GENERIC_READ = 0x80000000;
		private const uint FILE_SHARE_READ = 0x00000001;
		private const uint FILE_SHARE_WRITE = 0x00000002;
		private const uint OPEN_EXISTING = 0x00000003;
		private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
		private const uint ERROR_ACCESS_DENIED = 5;

		private const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;
		private const uint STD_INPUT_HANDLE = 0xFFFFFFF6;
		private const uint STD_OUTPUT_HANDLE = 0xFFFFFFF5;
		private const uint STD_ERROR_HANDLE = 0xFFFFFFF4;
		private const uint DUPLICATE_SAME_ACCESS = 2;
		#endregion
	}
}
