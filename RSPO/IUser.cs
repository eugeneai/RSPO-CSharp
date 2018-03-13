using BrightstarDB.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSPO
{
    [Entity]
    interface IUser
    {
        string Name { get; set; }
        string NickName { get; set; }
        string PasswordHash { get; set; }
        string Email { get; set; }
        ICollection <IRole> Roles { get; set; }
        ICollection <IProperty> Properties { get; set; }
    }
}