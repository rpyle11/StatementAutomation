
using FilePoller.Models;
using Serilog;

#pragma warning disable CA1416

namespace FilePoller
{
    public static class ServiceAcctName
    {

        public static string GetServiceAccountName(AppSettings settings)
        {
            var returnName = string.Empty;
            var wmiQuery =
                $"select startname from Win32_Service where name = '{settings.ServiceName}'";

            var sQuery = new System.Management.SelectQuery(wmiQuery);
            using var mgmtSearcher = new System.Management.ManagementObjectSearcher(sQuery);
            foreach (var service in mgmtSearcher.Get())
            {
                if (service["startname"].ToString()!.Contains('@'))
                {
                    returnName = service["startname"].ToString()!.Contains('@') ? service["startname"].ToString()?.Split(char.Parse("@"))[0] : service["startname"].ToString();
                }
                else
                {
                    returnName = service["startname"].ToString()!.Contains('\\') ? service["startname"].ToString()?.Split(char.Parse("\\"))[1] : service["startname"].ToString();
                }

                break;
            }

            return string.IsNullOrEmpty(returnName) ? "SvcAcct" : returnName;
        }

    }
}
