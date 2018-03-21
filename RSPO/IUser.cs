using BrightstarDB.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSPO
{
    [Entity]
    public interface IUser
    {
        string Name { get; set; }
        string NickName { get; set; }
        string PasswordHash { get; set; }
        string Email { get; set; }
        string Telephone { get; set; }
        // Добавь сюда телефон.
        ICollection <IRole> Roles { get; set; }
        ICollection <IProperty> Properties { get; set; }

        void GenerateHash(string password);
    }
}