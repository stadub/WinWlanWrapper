using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WiFiSettingsShell;
using WifiSettings;

namespace WifiSettingsTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void InterfacesListShoudBeSameAsReceivedByNetSh()
        {
            List<WLanIntreface> interfaces;
            using (var native = new WlanNative())
            {
                native.OpenHandle();

                interfaces=native.GetInterfaces();
            }
            NetShell shell = new NetShell();
            var shellInterfaces=shell.NetShInterfaces().ToLookup(@interface => @interface.Description);
            foreach (var wLanIntreface in interfaces)
            {
                Assert.IsTrue(shellInterfaces.Contains(wLanIntreface.Name));
                Assert.IsTrue(shellInterfaces[wLanIntreface.Name].First().Guid == wLanIntreface.WlanGuid);
            }

        }
        
        [TestMethod]
        public void WlanNetworkListShoudBeSameAsReceivedByNetSh()
        {
            List<WLanIntreface> interfaces;
            List<AvailableNetwork> networks;
            using (var native = new WlanNative())
            {
                native.OpenHandle();

                interfaces=native.GetInterfaces();
                networks = native.GetNetworks(interfaces.First());
            }
            NetShell shell = new NetShell();
            var shellWlanNetworks = shell.NetShWlanNetworkInfo().ToLookup(wlanNetwork => wlanNetwork.Name);
            foreach (var network in networks)
            {
                if (string.IsNullOrWhiteSpace(network.ProfileName))continue;
                Assert.IsTrue(shellWlanNetworks.Contains(network.ProfileName));
                WlanNetworkInfo curNetwork=shellWlanNetworks[network.ProfileName].First();
                //Seems that console utill shows incorrect Auth Types
                //Assert.IsTrue((int)curNetwork.AuthenticationType==(int)network.DefaultAuthAlgorithm);
                Assert.IsTrue((int)curNetwork.NetworkType==(int)network.Dot11BssType);
                Assert.IsTrue(curNetwork.BssIds.Count==network.NumberOfBssids);
                Assert.IsTrue((int)curNetwork.EncryptionType==(int)network.DefaultCipherAlgorithm);
            }

        }

    }
}
