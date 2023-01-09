using System;

namespace WpfCopyTool
{
    [Serializable]
    public struct DruckerInfo
    {
        public string MessageInfo { get; set; }
        public bool isDefault { get; set; }
        public bool isNetworkPrinter { get; set; }
        public string name { get; set; }
        public string status { get; set; }

        public DruckerInfo(string name, string status, bool isDefault, bool isNetworkPrinter)
        {
            this.name = name;
            this.status = status;
            this.isDefault = isDefault;
            this.isNetworkPrinter = isNetworkPrinter;
            MessageInfo = String.Format("{0} (Status: {1}, Default: {2}, Network: {3}",
                           name, status, isDefault, isNetworkPrinter);
        }
    }
}