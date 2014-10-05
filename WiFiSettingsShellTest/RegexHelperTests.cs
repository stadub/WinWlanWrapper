using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WiFiSettingsShell;

namespace WiFiSettingsShellTest
{
    [TestClass]
    public class RegexHelperTest
    {
        [TestMethod]
        public void TestInterfacesRegex()
        {
            RegexHelper helper= new RegexHelper();
            var interfacesString=TestTextLoader.LoadText("Interfaces1.txt");
            var interfaces=helper.GetInterfaces(interfacesString).ToArray();
            Assert.IsTrue(interfaces.Count()==1);
            Assert.IsTrue(interfaces[0].Description == "Ralink RT3050 802.11b/g/n WiFi Adapter");
            Assert.IsTrue(interfaces[0].Guid.ToString() == "b2e5dcaa-dba5-4663-ba87-d6519e2fab4c");
        }
        
        [TestMethod]
        public void TestavAlibleNetworksRegex()
        {
            RegexHelper helper= new RegexHelper();
            var avalibleNetworksString = TestTextLoader.LoadText("Networks.txt");
            var avalibleNetworks = helper.GetAvalibleNetworks(avalibleNetworksString).ToArray();
            Assert.IsTrue(avalibleNetworks.Count() == 3);
            Assert.IsTrue(avalibleNetworks[0].Name == "Network1");
            Assert.IsTrue(avalibleNetworks[1].EncryptionType == CipherAlgorithm.None);
            Assert.IsTrue(avalibleNetworks[1].BssIds.Count == 2);


            Assert.IsTrue(avalibleNetworks[2].AuthenticationType == AuthAlgorithm.Open);
            Assert.IsTrue(avalibleNetworks[2].NetworkType ==BssType.Infrastructure);

            var bssId3 = avalibleNetworks[2].BssIds[0];

            Assert.IsTrue(bssId3.SignalStreich == 42);
            Assert.IsTrue(bssId3.RadioType == "802.11g");
            Assert.IsTrue(bssId3.Channel == 8);
            Assert.IsTrue(bssId3.Rates == "1 2 5.5 11");
            Assert.IsTrue(bssId3.OtherRates == "6 9 12 18 24 36 48 54");
        }
    }

    public class TestTextLoader
    {
        private static string currentDir;
        static TestTextLoader()
        {
            var assembly=Assembly.GetExecutingAssembly();
            currentDir = System.IO.Path.GetDirectoryName(assembly.Location);
        }

        private static string TestsPath(string fileName)
        {
            return string.Format("{0}\\{1}\\{2}", currentDir, "TestCases", fileName);
        }

        public static string LoadText(string file)
        {
            return File.ReadAllText(TestsPath(file));
        }
    }
}
