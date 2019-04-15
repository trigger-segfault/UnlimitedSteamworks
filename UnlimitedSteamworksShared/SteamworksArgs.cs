using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;

#if OPTGET
using NMaier.GetOptNet;
#endif

using static UnlimitedSteamworks.SteamworksUtils;

namespace UnlimitedSteamworks {
#if OPTGET
	[GetOptOptions(
		AcceptPrefixType = ArgumentPrefixTypes.Dashes,
		CaseType = ArgumentCaseType.AsDefined,
		OnUnknownArgument = UnknownArgumentsAction.Throw,
		UsageStatement = "-a <appid> -r <path> [-e] [-p name] [-w dir] [-d sec] [-i sec] [-- args...]",
		UsageIntro = "A launcher for Steam apps to signal Steam activity, " +
					"protect against crashing apps breaking relaunching, " +
					"and circumventing Steam's terrible command line argument security measures.",
		//UsagePrefix = ArgumentPrefixTypes.Dashes,
		UsageShowAliases = UsageAliasShowOption.Show,
		UsageEpilog = "UnlimitedSteamworksProc will run in the background to keep your Steam status active until it no longer detects the process name. Specify a custom process name if the program being run is a launcher for a different program.\n"
	)]
#endif
	internal class SteamworksArgs
#if OPTGET
		: GetOpt
#endif
	{
		[JsonIgnore]
		public static SteamworksArgs NotSilent { get; } = new SteamworksArgs {
			Config = new SteamworksConfig { Silent = false, }
		};

#if OPTGET
		[Argument("app", HelpVar = "appid", HelpText = "ID of the Steam App to launch.")]// (REQUIRED)", Required = true)]
		[ShortArgument('a')]
#endif
		[JsonProperty("app")]
		public uint? AppId { get; set; }

#if OPTGET
		[Argument("run", HelpVar = "path", HelpText = "Path to the program to run.")]// (REQUIRED)", Required = true)]
		[ShortArgument('r')]
#endif
		[JsonIgnore]
		public FileInfo ExecutableArg { set => Executable = value.FullName; }

		[JsonProperty("run")]
		public string Executable { get; set;  }

#if OPTGET
		[Argument("elevate", HelpText = "Run the program as an administrator.")]
		[ShortArgument('e')]
		[FlagArgument(true)]
#endif
		[JsonIgnore]
		public bool ElevateArg { set => Elevate = value ? value : (bool?) null; }

		[JsonProperty("elevate")]
		public bool? Elevate { get; set; }

#if OPTGET
		[Argument("proc", HelpVar = "name", HelpText = "Name of the process to detect. (without extension)")]
		[ShortArgument('p')]
#endif
		[JsonProperty("proc")]
		public string ProcessName { get; set; }

#if OPTGET
		[Argument("working", HelpVar = "directory", HelpText = "Working directory to start in.")]
		[ShortArgument('w')]
#endif
		[JsonIgnore]
		public DirectoryInfo WorkingDirArg { set => WorkingDir = value.FullName; }

		[JsonProperty("working")]
		public string WorkingDir { get; set; }

#if OPTGET
		[Argument("delay", HelpVar = "seconds", HelpText = "Delay before process detection starts. (DEFAULT=1.0)")]
		[ShortArgument('d')]
#endif
		[JsonProperty("delay")]
		public double? Delay { get; set; }

#if OPTGET
		[Argument("interval", HelpVar = "seconds", HelpText = "Process detection interval. (DEFAULT=1.0)")]
		[ShortArgument('i')]
#endif
		[JsonProperty("interval")]
		public double? Interval { get; set; }

#if OPTGET
		[Parameters(HelpVar = "arguments")]
#endif
		[JsonProperty("args")]
		public List<string> Arguments { get; set; } = new List<string>();

#if OPTGET
		[Argument("help", HelpText = "Show this help message.")]
		[ShortArgument('h')]
		[FlagArgument(true)]
#endif
		[JsonIgnore]
		public bool Help { get; set; } = false;

		[JsonIgnore]
		public string Name => Path.GetFileNameWithoutExtension(Executable);
		[JsonIgnore]
		public string Directory => Path.GetDirectoryName(Executable);
		[JsonIgnore]
		public string ArgumentsString => EscapeArguments(Arguments ?? (IEnumerable<string>) new string[0]);

		[JsonProperty("config")]
		public SteamworksConfig Config { get; set; }

		public void Save() {
			File.WriteAllText(GetLocal(SteamworksArgsFile), JsonConvert.SerializeObject(this, Formatting.Indented));
			//WriteLocal(SteamAppIdFile, AppId.Value.ToString());
		}
		public static SteamworksArgs Load() {
			var sa = JsonConvert.DeserializeObject<SteamworksArgs>(File.ReadAllText(GetLocal(SteamworksArgsFile)));
			if (sa.Config == null)
				sa.Config = SteamworksConfig.Load();
			return sa;
		}
		public static SteamworksArgs LoadWorking() {
			if (File.Exists(GetWorking(SteamworksArgsFile))) {
				var sa = JsonConvert.DeserializeObject<SteamworksArgs>(File.ReadAllText(GetWorking(SteamworksArgsFile)));
				if (sa.Config == null)
					sa.Config = SteamworksConfig.Load();
				return sa;
			}
			return null;
		}


		public bool DetectSteamAppId() {
			if (!AppId.HasValue) {
				if (File.Exists(GetWorking(SteamAppIdFile))) {
					string content = File.ReadAllText(GetWorking(SteamAppIdFile)).Trim();
					if (uint.TryParse(content, out uint appId)) {
						AppId = appId;
						return true;
					}
				}
				return false;
			}
			return true;
		}

#if STEAMAPI
		public bool? InitSteamActivity() {
			bool? result = null;
			if (SteamAPI.IsSteamRunning()) {
				// The steam_appid.txt is used to identify the game when SteamApi.Init is called
				File.WriteAllText(GetLocal(SteamAppIdFile), AppId.Value.ToString());

				result = SteamAPI.Init();
			}
			// Cleanup after this point as we won't be needing the local files anymore
			CleanupLocal();
			return result;
		}

		public void RunProcess() {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = Executable,
				Arguments = ArgumentsString,
				WorkingDirectory = WorkingDir ?? Directory,
				WindowStyle = ProcessWindowStyle.Normal,
				UseShellExecute = true,
				CreateNoWindow = true,
			};
			if (Elevate.HasValue && Elevate.Value)
				startInfo.Verb = "runas";
			Process.Start(startInfo).Dispose();
		}

		public void WaitForProcessToExit() {
			TimeSpan delay = TimeSpan.FromSeconds(Delay ?? 1.0);
			TimeSpan interval = TimeSpan.FromSeconds(Interval ?? 1.0);
			// We can't rely on the returned process being the end process we want. Scan to see if one is running
			Thread.Sleep(delay);
			while (Process.GetProcessesByName(ProcessName ?? Name).Length != 0) {
				// We have to close AFTER the process closes. The whole point of this is
				// to prevent Steam from refusing to relaunch a program after it crashes.
				Thread.Sleep(interval);
			}
		}

		public void CleanupLocal() {
			string localDirNorm = Path.GetFullPath(LocalDir).ToLower();
			string dirNorm = Path.GetFullPath(Directory).ToLower();

			// Don't cleanup files if we're running the program in the same directory as the executable
			if (Config.CleanupOnClose && string.Compare(localDirNorm, dirNorm, true) != 0) {
				try {
					if (File.Exists(GetLocal(SteamAppIdFile)))
						File.Delete(GetLocal(SteamAppIdFile));
				} catch { }
				try {
					if (File.Exists(GetLocal(SteamworksArgsFile)))
						File.Delete(GetLocal(SteamworksArgsFile));
				} catch { }
			}
		}
#endif
	}
}
