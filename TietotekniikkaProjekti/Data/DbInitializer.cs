using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TietotekniikkaProjekti.Data
{
    public class DbInitializer 
    {
        public static void Initialize(PassWordContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}
