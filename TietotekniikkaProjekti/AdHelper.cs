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

        public ArrayList GetGroup(string username)// ei käytössä
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
        public bool isAdmin(string username)
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
   
                DirectoryEntry directory = new DirectoryEntry("LDAP://CN=Users,DC=ryhma1,DC=local", "Administrator", ADMIN_PASSWORD);//LDAP polku
                directory.AuthenticationType = AuthenticationTypes.Secure;

                DirectoryEntry newUser = directory.Children.Add
                ("CN=" + user.Username, "user");
                newUser.Properties["displayName"].Add(user.Username);
                newUser.Properties["userPrincipalName"].Add(user.Username + "@RYHMA1.LOCAL");
                newUser.Properties["samAccountName"].Value = user.Username;
                newUser.Properties["givenName"].Value = user.Nimi;
                newUser.Properties["sn"].Value = user.Sukunimi;
                newUser.Properties["mail"].Value = user.Email;
                newUser.Properties["StreetAddress"].Value = user.Osoite;
                newUser.Properties["employeeType"].Value = user.EmployeeType;

                newUser.CommitChanges();


                newUser.Invoke("SetPassword", new object[] { user.Password });
                newUser.CommitChanges();
                directory.Close();
                newUser.Close();

                return "Succesfully created user";
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException ex)
            {
                
                return "Failed to create!";
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
                            try
                            {
                                userModel.Sukunimi = de.Properties["sn"].Value.ToString();
                            }
                            catch (System.NullReferenceException) { }
                            try
                            {
                                userModel.Nimi = de.Properties["givenName"].Value.ToString();
                            }
                            catch (System.NullReferenceException) { }
                            try
                            {
                                userModel.Group = de.Properties["MemberOf"].Value.ToString();
                            }
                            catch (System.NullReferenceException) { }
                            try
                            {
                                userModel.Email = de.Properties["mail"].Value.ToString();
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
                            try
                            {
                                userModel.EmployeeType = de.Properties["employeeType"].Value.ToString();
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
            string filter = $"(&(objectClass=user)(sAMAccountName={user.Username}))";

            DirectoryEntry directory = new DirectoryEntry("LDAP://DC=ryhma1,DC=local", "Administrator", ADMIN_PASSWORD);//LDAP polku
            directory.AuthenticationType = AuthenticationTypes.Secure;

            DirectorySearcher searcher = new DirectorySearcher(directory, filter);
            searcher.SearchScope = SearchScope.Subtree;//from what level of the branches are we looking from

            var result = searcher.FindOne();//put result if found
            DirectoryEntry de = null;

            if (null != result)
            {
                de = result.GetDirectoryEntry();
                try
                {
                    de.Properties["givenName"].Value = user.Nimi;
                }
                catch (Exception e) { }
                try
                {
                    de.Properties["sn"].Value = user.Sukunimi;
                }
                catch (Exception e) { }
                /*try
                {
                    de.Properties["SamAccountName"].Add(user.Username);
                }
                catch (Exception e) { }*/
                try
                {
                    de.Properties["mail"].Value = user.Email;
                }
                catch (Exception e) { }
                try
                {
                    de.Properties["StreetAddress"].Value = user.Osoite;
                }
                catch (Exception e) { }
                try
                {
                    de.Properties["employeeType"].Value = user.EmployeeType;
                }
                catch (Exception e) { }
                //de.Properties["MemberOf"].Add(user.Group); 
                de.CommitChanges();
                
            }

            searcher.Dispose();
            directory.Dispose();
        }
        private bool IsActive(DirectoryEntry de)
        {
            if (de.NativeGuid == null) return false;

            int flags = (int)de.Properties["userAccountControl"].Value;

            return !Convert.ToBoolean(flags & 0x0002);
        }
    }
}
