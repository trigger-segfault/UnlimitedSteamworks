using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace UnlimitedSteamworks {
	internal static class SteamworksUtils {
		public const string SteamworksArgsFile = "steamworks_args.json";
		public const string SteamworksConfigFile = "steamworks_config.json";
		public const string SteamworksLogFile = "steamworks_errors.log";
		public const string SteamAppIdFile = "steam_appid.txt";

		public static string LocalDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		public static string WorkingDir => Directory.GetCurrentDirectory();
		public static bool IsWorkingDirLocal {
			get {
				string localDirNorm = Path.GetFullPath(LocalDir).ToLower();
				string workingDirNorm = Path.GetFullPath(WorkingDir).ToLower();

				return string.Compare(localDirNorm, workingDirNorm, true) == 0;
			}
		}

		public static bool IsSilent(this SteamworksArgs sa) {
			return sa?.Config?.Silent ?? SteamworksConfig.CheckSilentSafe();
		}

		public static string GetLocal(string fileName) {
			return Path.Combine(LocalDir, fileName);
		}
		public static string GetWorking(string fileName) {
			return Path.Combine(WorkingDir, fileName);
		}

		public static string Title { get; set; }

		private static bool consoleOpened = false;
		private static StreamWriter logWriter = null;

		public static void ExitAndDispose(int exitCode) {
			if (logWriter != null)
				logWriter.Close();
			Environment.Exit(exitCode);
		}
		public static void EnsureLog() {
			if (logWriter == null) {
				logWriter = new StreamWriter(GetLocal(SteamworksLogFile), true) {
					AutoFlush = true,
				};
			}
		}
		public static void EnsureConsole(SteamworksArgs sa) {
			if (sa.IsSilent() || consoleOpened)
				return;
			WinConsole.Initialize();
			Console.Title = Title;
			consoleOpened = true;
		}
		public static void EnterToExit(int exitCode, SteamworksArgs sa) {
			EnsureLog();
			EnsureConsole(sa);
			Console.Write("Press enter to exit...");
			Console.ReadLine();
			ExitAndDispose(exitCode);
		}

		public static void LogLineHeader(string line) {
			EnsureLog();
			logWriter.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {line}");
		}
		public static void LogLine(string line) {
			EnsureLog();
			logWriter.WriteLine(line);
		}

		public static void WriteLine(string line, SteamworksArgs sa) {
			LogLineHeader(line);
			try {
				EnsureConsole(sa);
				Console.WriteLine(line);
			} catch (Exception conEx) {
				LogLine("Error displaying console!");
				LogLine($"{conEx}");
			}
		}
		public static void ExitWithError(string line, SteamworksArgs sa) {
			EnsureLog();
			LogLineHeader(line);
			try {
				EnsureConsole(sa);
				//Console.WriteLine(Title);
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(line);
				Console.ResetColor();
				Console.WriteLine();
				EnterToExit(1, sa);
			} catch (Exception conEx) {
				LogLine("Error displaying console!");
				LogLine($"{conEx}");
			}
			ExitAndDispose(1);
		}
		public static void ExitWithException(string line, Exception ex, SteamworksArgs sa) {
			EnsureLog();
			LogLineHeader(line);
			LogLine($"{ex}");
			try {
				EnsureConsole(sa);
				//Console.WriteLine(Title);
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(line);
				Console.WriteLine($"  {ex.Message}");
				Console.ResetColor();
				Console.WriteLine();
				EnterToExit(1, sa);
			} catch (Exception conEx) {
				LogLine("Error displaying console!");
				LogLine($"{conEx}");
			}
			ExitAndDispose(1);
		}

		public static string EscapeArguments(IEnumerable<string> args) {
			return string.Join(" ", args.Select(EscapeArgument));
		}
		public static string EscapeArgument(string arg) {
			if (!arg.Contains(" ") && !arg.Contains("\t") && !arg.Contains("\""))
				return arg; // No need to escape

			arg = Regex.Replace(arg, @"(\\*)", @"$1$1\" + @"\");
			arg = "\"" + Regex.Replace(arg, @"(\\+)$", @"$1$1");
			return @"\"+ Regex.Replace(arg, @"(\\*)(\\$|"")", @"$1$1\$2") + @"\";
		}
	}
}
