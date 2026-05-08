using System.DirectoryServices.AccountManagement;

namespace StatementProcessorApi.Services
{
    public class AppUserData
    {
        public string? EmailAddress { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public AppUserData(string? userName)
        {
            using var pc = new PrincipalContext(ContextType.Domain);

            var up = UserPrincipal.FindByIdentity(pc, userName ?? "unknown");
          
            EmailAddress = up?.EmailAddress;

           

        }
    }
}
