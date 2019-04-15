using System.IO;
using Newtonsoft.Json;

using static UnlimitedSteamworks.SteamworksUtils;

namespace UnlimitedSteamworks {
	internal class SteamworksConfig {

		/// <summary>
		///  Gets or sets if temporary files should be deleted once they are no longer needed.
		/// </summary>
		[JsonProperty("cleanup")]
		public bool CleanupOnClose { get; set; } = true;
		/// <summary>
		///  The name of the executable for reading the saved json arguments.
		/// </summary>
		[JsonProperty("proc_exe")]
		public string ProcExecutable { get; set; } = "UnlimitedSteamworksProc.exe";

		/// <summary>
		///  Gets or sets if a console window should open and complain if an error occurred.
		/// </summary>
		[JsonProperty("silent")]
		public bool Silent { get; set; } = false;
		/// <summary>
		///  Gets or sets if an error occurs when trying to initialize Steam activity.
		/// </summary>
		[JsonProperty("error_steam_activity")]
		public bool ErrorSteamActivity { get; set; } = true;

		[JsonProperty("error_steam_not_running")]
		public bool ErrorSteamNotRunning { get; set; } = false;

		public static bool CheckSilentSafe() {
			SteamworksConfig sa = new SteamworksConfig();
			try {
				if (File.Exists(GetLocal(SteamworksConfigFile)))
					sa = JsonConvert.DeserializeObject<SteamworksConfig>(File.ReadAllText(GetLocal(SteamworksConfigFile)));
			} catch { }
			return sa.Silent;
		}
		public static string CheckProcExecutableSafe() {
			SteamworksConfig sa = new SteamworksConfig();
			try {
				if (File.Exists(GetLocal(SteamworksConfigFile)))
					sa = JsonConvert.DeserializeObject<SteamworksConfig>(File.ReadAllText(GetLocal(SteamworksConfigFile)));
			} catch { }
			return sa.ProcExecutable;
		}


		public void Save() {
			File.WriteAllText(GetLocal(SteamworksConfigFile), JsonConvert.SerializeObject(this, Formatting.Indented));
		}
		public static SteamworksConfig Load() {
			if (File.Exists(GetLocal(SteamworksConfigFile)))
				return JsonConvert.DeserializeObject<SteamworksConfig>(File.ReadAllText(GetLocal(SteamworksConfigFile)));
			// Save the local settings for easy modification
			SteamworksConfig sc = new SteamworksConfig();
			sc.Save();
			return sc;
		}
	}
}
