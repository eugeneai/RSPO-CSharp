using BrightstarDB.EntityFramework;
using System;
using System.Collections.Generic;

namespace RSPO
{

    [Entity]
    public interface IFilm
    {
        string Name { get; set; }

        [InverseProperty("Films")]
        ICollection<IActor> Actors { get; }
    }
}