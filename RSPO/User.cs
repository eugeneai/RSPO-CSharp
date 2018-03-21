using BrightstarDB.EntityFramework;
using HashLibrary;
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
            var hasher = new Hasher(); // https://github.com/tallesl/net-Hash

            PasswordHash = hasher.HashPassword(password).Hash; // FIXME: Convert to Base64 or hex.
        }   
    }
}