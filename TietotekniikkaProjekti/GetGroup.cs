using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;

namespace TietotekniikkaProjekti
{
    public class GetGroup
    {

        public bool IsHR(string username)
        {

            string filter = $"(&(objectClass=user)(sAMAccountName={username}))";//$ puts allows to use username syntax

            Console.WriteLine($"Searching {username}");

            DirectoryEntry directory = new DirectoryEntry("LDAP://DC=ryhma1,DC=local");//LDAP polku
            directory.AuthenticationType = AuthenticationTypes.Secure;

            DirectorySearcher searcher = new DirectorySearcher(directory, filter);
            searcher.SearchScope = SearchScope.Subtree;//from what level of the branches are we looking from

            var result = searcher.FindOne();//put result if found
            DirectoryEntry de = null;
            if (null != result)
            {
                de = result.GetDirectoryEntry();

                if (de.Properties["employeeType"].Value != null)
                {
                    if (de.Properties["employeeType"].Value.Equals("HR"))
                    {
                        searcher.Dispose();
                        directory.Dispose();
                        return true;
                    }
                }

            }

            searcher.Dispose();
            directory.Dispose();
            return false;
        }

        public bool isAdmin (string username)
        {

            string stringDomainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            // set up domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, stringDomainName);

            // find a user
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, username);

            // find the group in question
            GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, "Administrators");

            if (user != null)
            {
                // check if user is member of that group
                if (user.IsMemberOf(group))
                {
                    return true;
                    // do something.....
                }
            }
            return false;
        }

    }
}
