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
        int Rooms { get; set; }
        int Floors { get; set; }
        int TotalFloors { get; set; }
        BuildingEnum BuildingType { get; set; }
        string Description { get; set; }
        ICollection<IAgent> Agents { get; set; }
        ICollection<IProperty> Properties { get; set; }
    }

    [Entity]
    public interface IOffer
    {
        IObject Object { get; set; }
        OfferEnum OfferType { get; set; }
        string ID { get; set; }
        ISite Site { get; set; } // FIXME: Can be null if it is local data
        // FIXME: Creation and update time
    }

    [Entity]
    public interface ILocation
    {
        ICountry Country { get; set; }
        IRegion Region { get; set; }
        string Name { get; set; }
        string Locality { get; set; }
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
        Monolythn,
        Panel
        // FIXME: Add more ...
    }
}
