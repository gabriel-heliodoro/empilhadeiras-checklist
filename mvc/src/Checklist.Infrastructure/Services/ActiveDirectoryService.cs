using System;
using System.DirectoryServices.AccountManagement;

namespace Checklist.Infrastructure.Services;

public static class ActiveDirectoryService
{
    
     private const string dominio = "schott.org";
     private const string folders = "OU=Users,OU=RI1,OU=BR,DC=schott,DC=org";

    public static bool AuthenticateAD(string user, string password)
    {
        try
        {
            using (var context = folders is null
                ? new PrincipalContext(ContextType.Domain, dominio)
                : new PrincipalContext(ContextType.Domain, dominio, folders))
            {
                return context.ValidateCredentials(user, password);
            }
        }
        catch (Exception)
        {
            return false;
        }
    }
}
