using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;
using TietotekniikkaProjekti.Models;

namespace TietotekniikkaProjekti
{
    public class AdHelper
    {
        private const string ADMIN_PASSWORD = "Qwerty12";
        private const string LDAP_PATH = "CN=Users,DC=ryhma1,DC=local";
        public void AddToGroup(string userDn, string groupDn)
        {
            try
            {
                DirectoryEntry dirEntry = new DirectoryEntry("LDAP://" + groupDn);
                dirEntry.Properties["member"].Add(userDn);
                dirEntry.CommitChanges();
                dirEntry.Close();
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                //doSomething with E.Message.ToString();

            }
        }
        public static void Rename(string server, string userName, string password, string objectDn, string newName)
        {
            DirectoryEntry child = new DirectoryEntry("LDAP://" + server + "/" +
                objectDn, userName, password);
            child.Rename("CN=" + newName);
        }

        public bool Authenticate(string userName, string password)
        {
            bool authentic = false;
            try
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + "ryhma1",
                    userName, password);
                object nativeObject = entry.NativeObject;
                authentic = true;
            }
            catch (DirectoryServicesCOMException) { }
            return authentic;
        }

        public ArrayList GetGroup(string username)
        {

            string filter = $"(&(objectClass=user)(sAMAccountName={username}))";//$ puts allows to use username syntax

            Console.WriteLine($"Searching {username}");

            DirectoryEntry directory = new DirectoryEntry("LDAP://DC=ryhma1,DC=local");//LDAP polku
            directory.AuthenticationType = AuthenticationTypes.Secure;

            DirectorySearcher searcher = new DirectorySearcher(directory, filter);
            searcher.SearchScope = SearchScope.Subtree;//from what level of the branches are we looking from

            var result = searcher.FindOne();//put result if found
            DirectoryEntry de = null;
            ArrayList data = new ArrayList();
     
            if (null != result)
            {
                de = result.GetDirectoryEntry();

                if (de.Properties["memberOf"].Value != null)
                {

                    foreach (var val in de.Properties["memberOf"])
                    {
                        data.Add(val);
           
                    }

                    var data2 = de.Properties["memberOf"].Value;

                    //  data = "memberOf " + de.Properties["memberOf"].Value.ToString();

                }

            }

            searcher.Dispose();
            directory.Dispose();
            return data;
        }

        public string GetUserDetails(string username)
        {

            string filter = $"(&(objectClass=user)(sAMAccountName={username}))";//$ puts allows to use username syntax

            Console.WriteLine($"Searching {username}");

            DirectoryEntry directory = new DirectoryEntry("LDAP://DC=ryhma1,DC=local");//LDAP polku
            directory.AuthenticationType = AuthenticationTypes.Secure;

            DirectorySearcher searcher = new DirectorySearcher(directory, filter);
            searcher.SearchScope = SearchScope.Subtree;//from what level of the branches are we looking from

            var result = searcher.FindOne();//put result if found
            DirectoryEntry de = null;
            string data = "";
            if (null != result)
            {
                de = result.GetDirectoryEntry();

                //if (de.Properties["streetAddress"].Value != null)
                //{
                //    data = "Street Address: " + de.Properties["streetAddress"].Value.ToString();
                //}
                //if (de.Properties["mail"].Value != null)
                //{
                //    data += "Email: " + de.Properties["mail"].Value.ToString();
                //}
                //data += "Display Name: " + de.Properties["displayName"].Value.ToString();
                //data += "UserName: " + de.Properties["sAMAccountName"].Value.ToString()+"\n\n";



                
                Console.WriteLine($"Found: {result.Path}");
               // ViewBag.data = result.Path;

                foreach (var item in de.Properties.PropertyNames)
                {
                    //Console.Write($"\n{item}");
                    //ViewBag.data += $"\n{item}";
                    data += $"\n{item}";
                    foreach (var val in de.Properties[item.ToString()])
                    {
                        // Console.Write($"\n{val}");
                        //ViewBag.data += $"\n{val}";
                        data += $"\n{val}";
                    }
                }
                
            }
          
            searcher.Dispose();
            directory.Dispose();
            return data;
        }
        public string AttributeValuesSingleString(string attributeName, string objectDn)
        {
            string strValue;
            DirectoryEntry ent = new DirectoryEntry(objectDn);
            strValue = ent.Properties[attributeName].Value.ToString();
            ent.Close();
            ent.Dispose();
            return strValue;
        }
        public string CreateUser(UserModel user)
        {
            try
            {
                string stringDomainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
                PrincipalContext PrincipalContext4 = new PrincipalContext(ContextType.Domain, stringDomainName,
                  LDAP_PATH, ContextOptions.SimpleBind, @"ryhma1\Administrator", ADMIN_PASSWORD);
                UserPrincipal UserPrincipal1 = new UserPrincipal(PrincipalContext4,
                  user.Username, user.Password, true);

                //User Logon Name
                UserPrincipal1.UserPrincipalName = user.Username;
                UserPrincipal1.Name = user.Nimi;
                UserPrincipal1.GivenName = user.Username;
                UserPrincipal1.DisplayName = user.Nimi;
                UserPrincipal1.EmailAddress = user.Email;
                UserPrincipal1.Save();

                UserPrincipal1.Enabled = true;
                return "Succesfully created user";
            }
            catch (Exception ex)
            {
                
                return ex.ToString();
            }

        }
    public List<UserModel> GetAllUsers()
        {
            List<UserModel> usersList = new List<UserModel>();

            using (var context = new PrincipalContext(ContextType.Domain, "ryhma1.local"))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        UserModel userModel = new UserModel();
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        if (de.Properties["givenName"].Value != null && de.Properties["sn"].Value != null)
                        {
                            userModel.Nimi = de.Properties["givenName"].Value + " - " + de.Properties["sn"].Value;
                            try
                            {
                                userModel.EmployeeType = de.Properties["MemberOf"].Value.ToString();
                            }
                            catch (System.NullReferenceException) { }
                            try
                            {
                                userModel.Email = de.Properties["EmailAddress"].Value.ToString();
                            }
                            catch(System.NullReferenceException){ }
                            try
                            {
                                userModel.Osoite = de.Properties["StreetAddress"].Value.ToString();
                            }
                            catch (System.NullReferenceException) { }
                            try
                            {
                                userModel.Username = de.Properties["SamAccountName"].Value.ToString();
                            }
                            catch (System.NullReferenceException) { }
                            userModel.Enabled = IsActive(de);

                            usersList.Add(userModel);
                        }
                    }
                }
            }
            return usersList;
        }
        public void EditUser(UserModel user)
        {
            string stringDomainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            PrincipalContext PrincipalContext4 = new PrincipalContext(ContextType.Domain, stringDomainName,
            LDAP_PATH, ContextOptions.SimpleBind, @"ryhma1\Administrator", ADMIN_PASSWORD);
            using (var context = new PrincipalContext(ContextType.Domain, "ryhma1.local"))
            {
                UserPrincipal userPrincipal = UserPrincipal.FindByIdentity
                (context, user.Username);

                userPrincipal.GivenName = user.Nimi;

                userPrincipal.Save();
            }
            
        }
        private bool IsActive(DirectoryEntry de)
        {
            if (de.NativeGuid == null) return false;

            int flags = (int)de.Properties["userAccountControl"].Value;

            return !Convert.ToBoolean(flags & 0x0002);
        }
    }
}
