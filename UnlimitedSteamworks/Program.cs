using System;
using System.Diagnostics;

using static UnlimitedSteamworks.SteamworksUtils;

namespace UnlimitedSteamworks {
	class Program {
		static void Main(string[] args) {
			Title = "Unlimited Steamworks";

			SteamworksArgs wsa = null;
			SteamworksArgs sa = null;

			if (args.Length == 1 && (args[0] == "-h" || args[0] == "--help")) {
				EnsureConsole(SteamworksArgs.NotSilent);
				sa.PrintUsage();
				EnterToExit(0, SteamworksArgs.NotSilent);
			}

			if (args.Length == 0 && !IsWorkingDirLocal) {
				try {
					wsa = SteamworksArgs.LoadWorking();
				} catch (Exception ex) {
					ExitWithException($"Error loading working directory {SteamworksArgsFile}!", ex, sa);
				}
			}

			if (wsa != null && args.Length == 0) {
				sa = wsa;
			}
			else {
				sa = new SteamworksArgs();
				try {
					if (sa.Config == null)
						sa.Config = SteamworksConfig.Load();
				} catch (Exception ex) {
					ExitWithException($"Error loading {SteamworksConfigFile}!", ex, sa);
				}

				if (args.Length == 0) {
					EnsureConsole(SteamworksArgs.NotSilent);
					sa.PrintUsage();
					EnterToExit(0, SteamworksArgs.NotSilent);
				}
				else {
					try {
						sa.Parse(args);
						if (sa.Help) {
							ExitWithError("Help option cannot be used with any other options!", sa);
						}

						// Merge settings
						/*if (wsa != null) {
							sa.AppId = sa.AppId ?? wsa.AppId;
							sa.Executable = sa.Executable ?? wsa.Executable;
							sa.Elevate = sa.Elevate ?? wsa.Elevate;
							sa.ProcessName = sa.ProcessName ?? wsa.ProcessName;
							sa.WorkingDir = sa.WorkingDir ?? wsa.WorkingDir;
							sa.Delay = sa.Delay ?? wsa.Delay;
							sa.Interval = sa.Interval ?? wsa.Interval;
							if (sa.Arguments?.Any() ?? false)
								sa.Arguments = wsa.Arguments;
							sa.Config = sa.Config ?? wsa.Config;
						}*/

					} catch (Exception ex) {
						ExitWithException($"Argument error:", ex, wsa?.Config != null ? wsa : sa);
					}
				}
			}

			if (!sa.DetectSteamAppId()) {
				ExitWithError($"No valid {SteamAppIdFile} was present and no App ID was specified!", sa);
			}
			if (sa.Executable == null) {
				ExitWithError($"No program path specified!", sa);
			}
			sa.Save();

			LaunchProc(sa);

			ExitAndDispose(0);
		}

		static void LaunchProc(SteamworksArgs sa) {
			string procExe = SteamworksConfig.CheckProcExecutableSafe();
			try {
				ProcessStartInfo startInfo = new ProcessStartInfo {
					FileName = GetLocal(procExe),
					WorkingDirectory = WorkingDir,
					WindowStyle = ProcessWindowStyle.Normal,
					UseShellExecute = true,
					CreateNoWindow = true,
				};
				Process.Start(startInfo).Dispose();
			} catch (Exception ex) {
				ExitWithException($"Failed to start {procExe}!", ex, sa);
			}
		}
	}
}
