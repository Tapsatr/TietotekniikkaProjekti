using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
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
        public UserModel GetUserDetails(string username)
        {

            string filter = $"(&(objectClass=user)(sAMAccountName={username}))";//$ puts allows to use username syntax

            Console.WriteLine($"Searching {username}");

            DirectoryEntry directory = new DirectoryEntry("LDAP://DC=ryhma1,DC=local");//LDAP polku
            directory.AuthenticationType = AuthenticationTypes.Secure;

            DirectorySearcher searcher = new DirectorySearcher(directory, filter);
            searcher.SearchScope = SearchScope.Subtree;//from what level of the branches are we looking from

            var result = searcher.FindOne();//put result if found
            DirectoryEntry de = null;
            UserModel userModel = new UserModel();
            if (null != result)
            {
                de = result.GetDirectoryEntry();

                userModel.Osoite = (de.Properties["StreetAddress"].Value ?? "Not found").ToString();
                userModel.Email = (de.Properties["mail"].Value ?? "Not found").ToString();
                userModel.EmployeeType = (de.Properties["employeeType"].Value ?? "Not found").ToString();
                userModel.Nimi = (de.Properties["givenName"].Value ?? "Not found").ToString();
                userModel.Sukunimi = (de.Properties["sn"].Value ?? "Not found").ToString();
                userModel.Username = de.Properties["sAMAccountName"].Value.ToString();
               
                




                // ViewBag.data = result.Path;
                /*
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
                 */

            }
          
            searcher.Dispose();
            directory.Dispose();
            return userModel;
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

                if (user.Enabled == true)
                {
                    int val = (int)newUser.Properties["userAccountControl"].Value;
                    newUser.Properties["userAccountControl"].Value = val & ~0x2;
                }
                else
                {
                    int val = (int)newUser.Properties["userAccountControl"].Value;
                    newUser.Properties["userAccountControl"].Value = val | 0x2;
                }


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
                            userModel.Sukunimi = (de.Properties["sn"].Value ?? "Not found").ToString();
                            userModel.Nimi = (de.Properties["givenName"].Value ?? "Not found").ToString();
                            userModel.Group = (de.Properties["MemberOf"].Value ?? "Not found").ToString();
                            userModel.Email = (de.Properties["mail"].Value ?? "Not found").ToString();
                            userModel.Osoite = (de.Properties["StreetAddress"].Value ?? "Not found").ToString();
                            userModel.Username = de.Properties["SamAccountName"].Value.ToString();
                            userModel.EmployeeType = (de.Properties["employeeType"].Value ?? "Not found").ToString();
                            userModel.Enabled = IsActive(de);

                            usersList.Add(userModel);
                        }
                    }
                }
            }
            return usersList;
        }
        public string EditUser(UserModel user)
        {
            try
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
                    de.Properties["givenName"].Value = user.Nimi;
                    de.Properties["sn"].Value = user.Sukunimi;
                    de.Properties["mail"].Value = user.Email;
                    de.Properties["StreetAddress"].Value = user.Osoite;
                    de.Properties["employeeType"].Value = user.EmployeeType;
                    if (user.Enabled == true)
                    {
                        int val = (int)de.Properties["userAccountControl"].Value;
                        de.Properties["userAccountControl"].Value = val & ~0x2;
                    }
                    else
                    {
                        int val = (int)de.Properties["userAccountControl"].Value;
                        de.Properties["userAccountControl"].Value = val | 0x2;
                    }
                    //de.Properties["MemberOf"].Add(user.Group); 
                    de.CommitChanges();
                }

                searcher.Dispose();
                directory.Dispose();
                return "Succesfully edited user";
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException ex)
            {
                return "Failed to edit!";
            }
        }
        public string EditPassword(string username, string newpassword)
        {
                try
                {
                    string filter = $"(&(objectClass=user)(sAMAccountName={username}))";

                DirectoryEntry directory = new DirectoryEntry("LDAP://DC=ryhma1,DC=local", "Administrator", ADMIN_PASSWORD)
                {
                    AuthenticationType = AuthenticationTypes.Secure
                };//LDAP polku

                DirectorySearcher searcher = new DirectorySearcher(directory, filter)
                {
                    SearchScope = SearchScope.Subtree//from what level of the branches are we looking from
                };

                var result = searcher.FindOne();//put result if found
                    DirectoryEntry de = null;

                    if (null != result)
                    {
                        de = result.GetDirectoryEntry();
                        de.Invoke("SetPassword", new object[] { newpassword });
                        //de.Properties["LockOutTime"].Value = 0;
                        de.Close();
                    }

                    searcher.Dispose();
                    directory.Dispose();
                    return "OK";
                }
                catch (System.DirectoryServices.DirectoryServicesCOMException ex)
                {
                    return "ERROR";
                }
            
        }
        private bool IsActive(DirectoryEntry de)
        {
            if (de.NativeGuid == null) return false;

            int flags = (int)de.Properties["userAccountControl"].Value;

            return !Convert.ToBoolean(flags & 0x0002);
        }

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
        public void SendMail(MailMessage message, string destination)
        {
            #region formatter
            string text = string.Format("Please click on this link to {0}: {1}", message.Subject, message.Body);
            string html = "Please confirm your account by clicking this link: <a href=\"" + message.Body + "\">link</a><br/>";

            html += WebUtility.HtmlEncode(@"Or click on the copy the following link on the browser:" + message.Body);
            #endregion

            MailMessage msg = new MailMessage("vikkeongay@gay.gay",destination, text, html );
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));


            
            SmtpClient smtpClient = new SmtpClient("projekti-postip.RYHMA1.LOCAL");


            smtpClient.EnableSsl = true;
            smtpClient.Send(msg);
        }
    }
}
