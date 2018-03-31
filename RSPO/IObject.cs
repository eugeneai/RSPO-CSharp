using BrightstarDB.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSPO
{
    [Entity]
    public interface IObject
    {
        string Name { get; set; }
        ILocation Location { get; set; }
        string Address { get; set; }
        float Price { get; set; } // FIXME: Use as Currency type.
        CurrencyEnum CurType { get; set; }
        float Area { get; set; }
        AreaUnits AreaUnit { get; set; }
        string ImageURL { get; set; }
        string URL { get; set; }
        int Rooms { get; set; }
        int Floors { get; set; }
        int TotalFloors { get; set; }
        BuildingEnum BuildingType { get; set; }
        PropertyEnum PropertyType { get; set; }
        CategoryEnum Category { get; set; }
        string Description { get; set; }
        ICollection<IAgent> Agents { get; set; }
        ICollection<IProperty> Properties { get; set; }
    }

    [Entity]
    public interface IOffer
    {
        IObject Object { get; set; }
        OfferEnum OfferType { get; set; }
        string SiteId { get; set; }
        ISite Site { get; set; } // FIXME: Can be null if it is local data
        // FIXME: Creation and update time
    }

    [Entity]
    public interface ILocation
    {
        [Ignore]
        ICountry Country { get; } // As Region defines the Country it is in.
        IRegion Region { get; set; }
        string Name { get; set; }
        string Locality { get; set; }
    }

    public partial class Location : BrightstarEntityObject, ILocation
    {
        public ICountry Country {
            get
            {
                return Region.Country;
            }
        }
    }

    [Entity]
    public interface ICountry
    {
        string Name { get; set; }
    }

    [Entity]
    public interface IRegion
    {
        ICountry Country { get; set; }
        string Name { get; set; }
    }

    [Entity]
    public interface ISite
    {
        string Name { get; set; }
        string URL { get; set; }
    }

    public enum OfferEnum
    {
        Sale,      // Продажа
        Purchase,  // Покупка
        Rent       // Аренда
    }

    public enum CurrencyEnum
    {
        None,
        Rouble,
        Dollar,
        Euro
        // FIXME: Add more ...
    }

    public enum AreaUnits
    {
        SquaredMeters
        // FIXME: Add more ...
    }

    public enum BuildingEnum
    {
        Brick,
        BrickMonolyth,
        Monolythn,
        Panel,
        FoamConcrete
        // FIXME: Add more ...
    }

    public enum PropertyEnum
    {
        Residental,
        NonResidental,
        Commerсial
    }

    public enum CategoryEnum
    {
        Room,
        Flat,
        House,
        Area,
        Garage,
        Trading,
        Office,
        NonResidental
    }

    public partial class MyEntityContext : BrightstarEntityContext
    {

    }
}
