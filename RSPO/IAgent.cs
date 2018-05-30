using BrightstarDB.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSPO
{
    [Entity]
    public interface IAgent
    {
        string Name { get; set; }
        string NickName { get; set; }
        string PasswordHash { get; set; }
        string Phone { get; set; }
        string Email { get; set; }
        RoleEnum Role { get; set; }
        string GUID { get; set; } // Уже есть
        ICollection<IProperty> Properties { get; set; }

        [Ignore]
        bool Valid { get; }
    }

    public enum RoleEnum
    {
        Unknown,
        Agent,
        Owner,
        Buyer,
        Expert,
        Invalid
    }

    public partial class Agent : BrightstarEntityObject, IAgent
    {
        public bool Valid
        {
            get
            {
                return true;
            }
        }
    }

}
