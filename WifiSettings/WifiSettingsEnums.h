namespace WifiSettings {

	public enum class WlanInterfaceState {
		NotReady = 0,
		Connected = 1,
		AdHocNetworkFormed = 2,
		Disconnecting = 3,
		Disconnected = 4,
		Associating = 5,
		Discovering = 6,
		Authenticating = 7
	};

	public enum class  AuthAlgorithm {
		Open = 1,
		Wep = 2,
		Wpa = 3,
		WpaPsk = 4,
		Wpa2 = WpaPsk,
		WpaNone = 5,
		Rsna = 6,
		RsnaPsk = 7
		//IhvStart = 0x80000000,
		//IhvEnd = 0xffffffff
	};
	public enum class CipherAlgorithm {
		None = 0x00,
		Wep40 = 0x01,
		Tkip = 0x02,
		Ccmp = 0x04,
		Wep104 = 0x05,
		WpaUseGroup = 0x100,
		RsnUseGroup = 0x100,
		Wep = 0x101,
		IhvStart = 0x80000000,
		IhvEnd = 0xffffffff
	};

	[FlagsAttribute]
	public enum class WlanNetworkFlags {
		Connected = 0x01,
		HasProfile = 0x02,
	};

	public enum class  BssType {
		Infrastructure = 1,
		Independent = 2,
		Any = 3
	};
}