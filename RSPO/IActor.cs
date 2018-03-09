
using BrightstarDB.EntityFramework;
using System;
using System.Collections.Generic;

namespace RSPO
{
    [Entity]
    public interface IActor
    {
        string Name { get; set; }
        DateTime DateOfBirth { get; set; }
        ICollection<IFilm> Films { get; set; }
    }
}