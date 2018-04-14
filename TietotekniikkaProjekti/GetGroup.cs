using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;

namespace TietotekniikkaProjekti
{
    public class GetGroup
    {
        private const string LDAP_PATH = "CN=Users,DC=ryhma1,DC=local";


        public bool isAdmin (string username)
        {

            string stringDomainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            // set up domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, stringDomainName, LDAP_PATH);

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
