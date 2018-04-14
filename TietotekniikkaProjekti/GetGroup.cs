using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

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
        public string Excel()
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            string str = "";
            int rCnt;
            int cCnt;
            int rw = 0;
            int cl = 0;

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Open(@"d:\csharp-Excel.xls", 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            range = xlWorkSheet.UsedRange;
            rw = range.Rows.Count;
            cl = range.Columns.Count;


            for (rCnt = 1; rCnt <= rw; rCnt++)
            {
                for (cCnt = 1; cCnt <= cl; cCnt++)
                {
                    str = (string)(range.Cells[rCnt, cCnt] as Excel.Range).Value2;
                    
                }
            }

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);

            return str;
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
