using System;

using static UnlimitedSteamworks.SteamworksUtils;

namespace UnlimitedSteamworks.Proc {
	class Program {
		static void Main(string[] args) {
			Title = "Unlimited Steamworks Process";

			SteamworksArgs sa = null;
			try {
				sa = SteamworksArgs.Load();
			} catch (Exception ex) {
				ExitWithException($"Error loading {SteamworksArgsFile}!", ex, sa);
			}

			try {
				bool? result = sa.InitSteamActivity();
				if (result.HasValue && !result.Value && sa.Config.ErrorSteamActivity) {
					ExitWithError("Failed to initialize Steam!", sa);
				}
				else if (!result.HasValue && sa.Config.ErrorSteamNotRunning) {
					ExitWithError("Steam is not running!", sa);
				}
			} catch (Exception ex) {
				sa.CleanupLocal();
				ExitWithException("Error launching application!", ex, sa);
			}

			try {
				sa.RunProcess();
			} catch (Exception ex) {
				ExitWithException("Error launching application!", ex, sa);
			}

			try {
				sa.WaitForProcessToExit();
			} catch (Exception ex) {
				ExitWithException("Error while detecting process!", ex, sa);
			}

			ExitAndDispose(0);
		}
	}
}
