using System.Runtime.InteropServices;

namespace UnlimitedSteamworks {
#if STEAMAPI
	public static class SteamAPI {
		
		[DllImport("CSteamworks", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Init")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool Init();

		[DllImport("CSteamworks", CallingConvention = CallingConvention.Cdecl, EntryPoint = "IsSteamRunning")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsSteamRunning();

		[DllImport("CSteamworks", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Shutdown")]
		public static extern void Shutdown();

		[DllImport("CSteamworks", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RestartAppIfNecessary")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool RestartAppIfNecessary(uint steamAppId);
	}
#endif
}
