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
        string Address { get; set; }
        float Area { get; set; }
        int Rooms { get; set; }
        ILocation Location { get; set; }
        string Address;
        float Price; // FIXME: Use as Currency type.
        CurrencyEnum CurType;
        float Area;
        AreaUnits AreaUnit;
        string ImageURL;
        int Rooms;
        int Floors;
        int TotalFloors;
        BuildingEnum BuildingType;
        string Description;
        ICollection<IUser> Users { get; set; }
        ICollection<IProperty> Properties { get; set; }
    }

    [Entity]
    public interface Offer
    {
        IObject Object;
        OfferEnum OfferType;
        string ID;
        ISite Site; // FIXME: Can be null if it is local data
        // FIXME: Creation and update time
    }

    [Entity]
    public interface ILocation
    {
        ICountry Country;
        IRegion Region;
        string Name;
        string Locality
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
