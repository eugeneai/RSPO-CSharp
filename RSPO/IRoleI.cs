using BrightstarDB.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSPO
{
    [Entity]
    interface IRole
    {
        string Name { get; set; }
        string Description { get; set; }
        [InverseProperty("Roles")]
        ICollection<IUser> Users { get; set; }
    }
}