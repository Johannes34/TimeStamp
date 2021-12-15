using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeStamp.Tools
{
    public class SwitchNetworkAdapter
    {
        public List<StateSetup> States { get; } = new List<StateSetup>();

        public class StateSetup
        {
            public string StateName { get; set; }

            public string AdapterName { get; set; }

            public bool ShouldAdapterBeEnabled { get; set; }

            public string WifiNetworkName { get; set; }

            public bool ShouldWifiNetworkBeConnected { get; set; }
        }

        // Group "Mentor Network" -> Disable "Ethernet 3", Connect to WIFI "SWSDIMGDEV"
        // Group "LDorado Network" -> Enable "Ethernet 3", Disconnect from any WIFI

        public void Execute(string stateName)
        {
            var setup = States.FirstOrDefault(s => s.StateName == stateName);

            if (!String.IsNullOrEmpty(setup.AdapterName))
            {
                SelectQuery wmiQuery = new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionId != NULL");
                ManagementObjectSearcher searchProcedure = new ManagementObjectSearcher(wmiQuery);
                foreach (ManagementObject item in searchProcedure.Get())
                {
                    if (((string)item["NetConnectionId"]) == "Local Network Connection")
                    {
                        item.InvokeMethod("Disable", null); // TODO: requires admin rights?
                    }
                }
            }

            //Wlan.WlanConnectionParameters connectionParams = new Wlan.WlanConnectionParameters();
            //connectionParams.wlanConnectionMode = connectionMode;
            //connectionParams.profile = profile;
            //connectionParams.dot11BssType = bssType;
            //connectionParams.flags = 0;
            //Connect(connectionParams);

            //WlanClient client = new WlanClient();
            //foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            //{
            //    // Lists all networks with WEP security
            //    Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
            //    foreach (Wlan.WlanAvailableNetwork network in networks)
            //    {
            //        if (network.dot11DefaultCipherAlgorithm == Wlan.Dot11CipherAlgorithm.WEP)
            //        {
            //            Console.WriteLine("Found WEP network with SSID {0}.", GetStringForSSID(network.dot11Ssid));
            //        }
            //    }

            //    // Retrieves XML configurations of existing profiles.
            //    // This can assist you in constructing your own XML configuration
            //    // (that is, it will give you an example to follow).
            //    foreach (Wlan.WlanProfileInfo profileInfo in wlanIface.GetProfiles())
            //    {
            //        string name = profileInfo.profileName; // this is typically the network's SSID
            //        string xml = wlanIface.GetProfileXml(profileInfo.profileName);
            //    }

            //    // Connects to a known network with WEP security
            //    string profileName = "Cheesecake"; // this is also the SSID
            //    string mac = "52544131303235572D454137443638";
            //    string key = "hello";
            //    string profileXml = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><MSM><security><authEncryption><authentication>open</authentication><encryption>WEP</encryption><useOneX>false</useOneX></authEncryption><sharedKey><keyType>networkKey</keyType><protected>false</protected><keyMaterial>{2}</keyMaterial></sharedKey><keyIndex>0</keyIndex></security></MSM></WLANProfile>", profileName, mac, key);
            //    wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
            //    wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName);
        }
    }

    private void ExecuteWifiConnection(string name, bool connect)
    {
        WLAN_CONNECTION_PARAMETERS wlanConnParam = new WLAN_CONNECTION_PARAMETERS();
        wlanConnParam.wlanConnectionMode = WLAN_CONNECTION_MODE.wlan_connection_mode_profile;
        wlanConnParam.strProfile = name;
        wlanConnParam.pDot11Ssid = IntPtr.Zero;
        wlanConnParam.pDesiredBssidList = IntPtr.Zero;
        wlanConnParam.dot11BssType = DOT11_BSS_TYPE.dot11_BSS_type_infrastructure;
        wlanConnParam.dwFlags = 0;

        IntPtr ptrParam = Marshal.AllocHGlobal(Marshal.SizeOf(wlanConnParam));

        Marshal.StructureToPtr(wlanConnParam, ptrParam, true);

        IntPtr ptrGuid = Marshal.AllocHGlobal(Marshal.SizeOf(interfaceList.InterfaceInfo[0].InterfaceGuid));

        Marshal.StructureToPtr(interfaceList.InterfaceInfo[0].InterfaceGuid, ptrGuid, true);

        WlanConnect(handle, ptrGuid, ptrParam, IntPtr.Zero);
    }

    public enum WLAN_CONNECTION_MODE
    {
        wlan_connection_mode_profile = 0,
        wlan_connection_mode_temporary_profile,
        wlan_connection_mode_discovery_secure,
        wlan_connection_mode_discovery_unsecure,
        wlan_connection_mode_auto,
        wlan_connection_mode_invalid
    }

    public enum DOT11_BSS_TYPE
    {
        dot11_BSS_type_infrastructure = 1,
        dot11_BSS_type_independent = 2,
        dot11_BSS_type_any = 3
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct WLAN_CONNECTION_PARAMETERS
    {
        public WLAN_CONNECTION_MODE wlanConnectionMode;

        [MarshalAs(UnmanagedType.LPWStr, SizeConst = 256)] public string strProfile;
        public IntPtr pDot11Ssid;

        public IntPtr pDesiredBssidList;

        public DOT11_BSS_TYPE dot11BssType;

        public UInt32 dwFlags;
    };

    [DllImport("Wlanapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern uint WlanConnect(uint clientHandle, IntPtr pInterfaceGuid, IntPtr connParam, IntPtr pReserved);

}
}
