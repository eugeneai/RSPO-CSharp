using BrightstarDB.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSPO
{

    public partial class User : BrightstarEntityObject, IUser
    {
        public void GenerateHash(string password) {
            PasswordHash = password;
        }   
    }
}