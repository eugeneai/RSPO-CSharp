using BrightstarDB.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSPO
{
    [Entity]
    interface IObject
    {
        string Name { get; set; }
        string Address { get; set; }
        float Area { get; set; }
        int Rooms { get; set; }
        ICollection<IUser> Users { get; set; }
        ICollection<IProperty> Properties { get; set; }
    }
}
