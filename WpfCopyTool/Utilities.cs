using System.Runtime.InteropServices;
/// <summary>
/// Zum Einbinden von Netzwerklaufwerken
/// </summary>
/// <remarks>
/// https://stackoverflow.com/questions/10736790/how-to-access-shared-network-drive-file-using-c-sharp-code
/// 
/// https://pastebin.com/RZnydz4Z
/// </remarks>
namespace Utilities.Network
{
    public class NetworkDrive
    {
        public enum ResourceScope
        {
            RESOURCE_CONNECTED = 1,
            RESOURCE_GLOBALNET,
            RESOURCE_REMEMBERED,
            RESOURCE_RECENT,
            RESOURCE_CONTEXT
        }

        public enum ResourceType
        {
            RESOURCETYPE_ANY,
            RESOURCETYPE_DISK,
            RESOURCETYPE_PRINT,
            RESOURCETYPE_RESERVED
        }

        public enum ResourceUsage
        {
            RESOURCEUSAGE_CONNECTABLE = 0x00000001,
            RESOURCEUSAGE_CONTAINER = 0x00000002,
            RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
            RESOURCEUSAGE_SIBLING = 0x00000008,
            RESOURCEUSAGE_ATTACHED = 0x00000010,
            RESOURCEUSAGE_ALL = (RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED),
        }

        public enum ResourceDisplayType
        {
            RESOURCEDISPLAYTYPE_GENERIC,
            RESOURCEDISPLAYTYPE_DOMAIN,
            RESOURCEDISPLAYTYPE_SERVER,
            RESOURCEDISPLAYTYPE_SHARE,
            RESOURCEDISPLAYTYPE_FILE,
            RESOURCEDISPLAYTYPE_GROUP,
            RESOURCEDISPLAYTYPE_NETWORK,
            RESOURCEDISPLAYTYPE_ROOT,
            RESOURCEDISPLAYTYPE_SHAREADMIN,
            RESOURCEDISPLAYTYPE_DIRECTORY,
            RESOURCEDISPLAYTYPE_TREE,
            RESOURCEDISPLAYTYPE_NDSCONTAINER
        }

        [StructLayout(LayoutKind.Sequential)]
        private class NETRESOURCE
        {
            public ResourceScope dwScope = 0;
            public ResourceType dwType = 0;
            public ResourceDisplayType dwDisplayType = 0;
            public ResourceUsage dwUsage = 0;
            public string lpLocalName = null;
            public string lpRemoteName = null;
            public string lpComment = null;
            public string lpProvider = null;
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NETRESOURCE lpNetResource, string lpPassword, string lpUsername, int dwFlags);
        /// <summary>
        /// Verbindet ein Netzlaufwerk
        /// </summary>
        /// <param name="unc">NetwerkPfad \\ svmp0300\... /param>
        /// <param name="drive"> Z: </param>
        /// <param name="user"> ED5830 </param>
        /// <param name="password"></param>
        /// <returns>Return Code</returns>
        /// <seealso cref="https://docs.microsoft.com/en-us/windows/desktop/api/winnetwk/nf-winnetwk-wnetaddconnection2a"/>
        public int MapNetworkDrive(string unc, string drive, string user, string password, int dwFlag)
        {
            NETRESOURCE myNetResource = new NETRESOURCE();
            myNetResource.lpLocalName = drive;
            myNetResource.lpRemoteName = unc;
            myNetResource.lpProvider = null;
            int result = WNetAddConnection2(myNetResource, password, user, dwFlag);
            return result;
        }
    }
}
