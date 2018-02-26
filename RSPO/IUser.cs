using System;
using BrightstarDB.EntityFramework;
namespace RSPO
{
    [Entity]
    public interface IUser
    {
        string Name { get; set; }
        string Family { get; set; }
        string UserName { get; set; }
        string PasswordHash { get; set; }
    }
}
