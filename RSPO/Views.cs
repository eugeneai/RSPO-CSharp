using System;
using System.Collections.Generic;
using System.Globalization;

namespace RSPO
{
    public class Pair<T1,T2>
    {
        public T1 First;
        public T2 Second;
        public Pair(T1 first, T2 second)
        {
            First=first;
            Second=second;
        }
    }

    public class ApplicationView: View<ApplicationModel>
    {
        public ApplicationView(ApplicationModel context): base(context) {}

        public string Name
        {
            get
            {
                return context.Name;
            }
        }

        public new string Title
        {
            get
            {
                return Name;
            }
        }

        public List<Pair<string,Uri>> Menu
        {
            get
            {
                List<Pair<string,Uri>> l = new List<Pair<string,Uri>>{
                    menu("Главная", "/"),
                    menu("Предложения", "/offers"),
                    menu("Агенты", "/agents"),
                    menu("Войти", "/login")
                };
                return l;
            }
        }

        private Pair<string, Uri> menu(string name, string part)
        {
            return new Pair<string, Uri>(name, new Uri(part, UriKind.Relative));
        }
    }

	public class ObjectList : EntityList<IObject>
	{
	}

	public class ObjectListView : EntityListView<ObjectList>
	{
		public new string Title = "Список объектов недвижимости";
		public ObjectListView(ObjectList context) : base(context) { }
		public ObjectView AsObjectView(IObject obj) { return new ObjectView(obj); }
	}

	public class ObjectView : View<IObject>
	{
		public ObjectView(IObject context) : base(context) { } // context == model
															   // Убрали левые функции из модели.
		public string SubLoc
		{
			get
			{
				return context.Location.SubLocalityName.Substring(0, 3).ToUpper();
			}
		}
		public string FOF
		{
			get
			{
				var o = context;
				if (o.FloorTotal <= 0) return "";
				if (o.Floor <= 0) return o.FloorTotal.ToString();
				return o.Floor.ToString() + "/" + o.FloorTotal.ToString();
			}
		}

		public string ROR
		{
			get
			{
				var o = context;
				if (o.Rooms <= 0) return "";
				if (o.RoomsOffered <= 0) return o.Rooms.ToString() + "к";
				return o.RoomsOffered.ToString() + "/" + o.Rooms.ToString() + "к";
			}
		}

		private static Dictionary<CategoryEnum, string> categoryEnumToRuString = ImportFromAtlcomru.categoryTypes.Reverse();
		private static Dictionary<BuildingEnum, string> buildingEnumToRuString = ImportFromAtlcomru.buildingTypes.Reverse();

		public string RuCategory
		{
			get
			{
				return categoryEnumToRuString[context.Category];
			}
		}

		public string RuType // FIXME: Похоже поля перепутаны....
		{
			get
			{
				string val = null;
				bool rc = buildingEnumToRuString.TryGetValue(context.BuildingType, out val);
				if (rc)
				{
					return val.Substring(0, 4).ToLower();
				}
				return "";
			}
		}

		public string Price
		{
			get
			{
				numberFormatInfo.CurrencySymbol = "";
				return context.Price.ToString("C", numberFormatInfo).Replace(' ', '\u00A0');
			}
		}

		static CultureInfo culture = new CultureInfo("ru-RU", false);
		static NumberFormatInfo numberFormatInfo = (NumberFormatInfo)culture.NumberFormat.Clone();
	}

	/// <summary>
	///   Список предложений.
	/// </summary>

	public class OfferList : EntityList<IOffer> { } // Список предложений (Модель)

	public class OfferListView : EntityListView<OfferList> // Представление (View) для списка предложений.
	{
		public new string Title = "Список предложений";
		public OfferListView(OfferList context) : base(context) { }
		public OfferView AsOfferView(IOffer offer) { return new OfferView(offer); }
		public ObjectView AsObjectView(IObject obj) { return new ObjectView(obj); }
		public ObjectView AsObjectView(IOffer offer)
		{
			return AsObjectView(offer.Object);
		}
	}

    public class OfferView : View<IOffer>
	{
		public OfferView(IOffer context) : base(context) { }

        public new string Title = "Работай таки, машина!";
	}

	public class AgentList : EntityList<IAgent> { } // Список пользователей (Модель)

	public class AgentListView : EntityListView<AgentList>
	{
		public new string Title = "Список агентов и клиентов";
		public AgentListView(AgentList context) : base(context) { }
        public AgentView AsAgentView(IAgent agent)
        {
            return new AgentView(agent);
        }
    }

    public class AgentView : View<IAgent>
	{
		private static Dictionary<RoleEnum, string> roleEnumToRuString = ImportFromAtlcomru.roles.Reverse();

		public AgentView(IAgent context) : base(context) { }
        public string RuRole
        {
            get
            {
				return roleEnumToRuString[context.Role];
            }
        }
	}
}
