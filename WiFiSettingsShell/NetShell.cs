using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFiSettingsShell
{
    public class NetShell
    {
        private RegexHelper regexHelper;

        public NetShell()
        {
            regexHelper = new RegexHelper();
        }
        public IEnumerable<Interface> NetShInterfaces()
        {
            var interfacesString = NetSh("show interfaces");
            return regexHelper.GetInterfaces(interfacesString);
        }

        public IEnumerable<WlanNetworkInfo> NetShWlanNetworkInfo()
        {
            var interfacesString = NetSh("show all");
            return regexHelper.GetAvalibleNetworks(interfacesString);
        }

        protected static string NetSh(string action)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = "netsh",
                Arguments = "wlan " + action,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }
    }

    public class Interface
    {
        static Interface()
        {
            TypeDescriptor.AddAttributes(typeof(Guid), new TypeConverterAttribute(typeof(GuidTypeConverter)));

        }
        public string Description { get; set; }
        public Guid Guid { get; set; }
    }
    

    public class BssId
    {
        public string Mac { get; set; }
        public int SignalStreich { get; set; }
        public string RadioType { get; set; }
        public int Channel { get; set; }
        public string Rates { get; set; }
        public string OtherRates { get; set; }
    }

    public class WlanNetworkInfo
    {
        public string Name { get; set; }
        public BssType NetworkType { get; set; }
        public AuthAlgorithm AuthenticationType { get; set; }
        public CipherAlgorithm EncryptionType { get; set; }
        public List<BssId> BssIds { get; set; }
    }

    [TypeConverter(typeof(EnumConverter ))]
    public enum BssType {//typedescriptor shoud be used
		Infrastructure = 1,
		Independent = 2,
		Any = 3
	}

    public enum AuthAlgorithm {
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

    [TypeConverter(typeof(EnumConverter))]
    public enum CipherAlgorithm {
		None = 0x00,
		Wep40 = 0x01,
		Tkip = 0x02,
		Ccmp = 0x04,
		Wep104 = 0x05,
		WpaUseGroup = 0x100,
		RsnUseGroup = 0x100,
		Wep = 0x101,
	};
    //public class WlanNetwork
    //{
    //    static WlanNetwork()
    //    {
    //        TypeDescriptor.AddAttributes(typeof(Guid), new TypeConverterAttribute(typeof(GuidTypeConverter)));
    //        TypeDescriptor.AddAttributes(typeof(BssType), new TypeConverterAttribute(typeof(EnumConverter)));
    //        TypeDescriptor.AddAttributes(typeof(AuthAlgorithm), new TypeConverterAttribute(typeof(EnumConverter)));

    //    }
    //    public int Index { get; set; }
    //    public BssType NetworkType { get; set; }
    //    public AuthAlgorithm AuthenticationType { get; set; }
    //    public CipherAlgorithm Encryption { get; set; }
    //}

}
