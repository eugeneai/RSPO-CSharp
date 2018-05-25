using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DevOne.Security.Cryptography.BCrypt;
using Nancy.Session;

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
                    menu("Войти", "/login") // Обычно пользователю предлагается страница
                    // входа, а на ней есть вариант "Зарегистрироваться"
                    // Чтоб страниц было не очень много.
                    // Если регистраию ыстро сделаем попроуем переделать БД на MySQL или Postgres
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
        public IObject Object
        {
            get
            {
                return context.Object;
            }
        }

        public ObjectView ObjView = null;

		public OfferView(IOffer context) : base(context)
        {
            ObjView = new ObjectView(Object);
        }

        public new string Title
        {
            get
            {
                return ObjView.RuCategory+", "+Object.Address;
            }
        }
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

    public class LoginView : View<LoginObject>
    {
        public LoginView(LoginObject context, Nancy.Request request = null) : base(context)
        {
            this.request = request;
        }

        protected bool UserBad(string message)
        {
            request.Session.DeleteAll(); // Аннулировать сессию, однако...
            request.Session["message"] = message+", однако.";
            return false;
        }

        public bool Process()
        {
            MyEntityContext ctx = Application.Context;

            string name = request.Form.name;
            string phone = request.Form.phone;

            IAgent user = ctx.Agents.Where(x=>x.Name==name &&
                                            x.Phone==phone).FirstOrDefault();
            string register = request.Form["register"];

			if (register != null && user != null)
            {
                request.Session.DeleteAll();

                return UserBad("Пользователь уже зарегистрирован");
            }
			else if (register == null && user == null)
			{
				return UserBad("Пользователь не найден");
			}
			else if (register != null && user == null)
            {
				// FIXME: Проверки правильности данных не сделаны.

                string password = request.Form.password;
                string repeat = request.Form.repeat;

                if (password != repeat)
                {
                    return UserBad("Пароли не совпадают");
                }

                user = ctx.Agents.Create();
                user.Name = request.Form.firstname + " " + request.Form.surname + " " + request.Form.lastname;
                user.PasswordHash = BCryptHelper.HashPassword(password, SALT);
                user.Phone = request.Form.phone;
                user.GUID = ImportFromAtlcomru.GetGUID();
                if (request.Form.realtor == "checked")
                {
                    user.Role = RoleEnum.Agent;
                }
                else
                {
                    user.Role = RoleEnum.Buyer;
                }
                user.NickName = request.Form.name;
                user.Email = request.Form.email;
                ctx.Add(user);
                ctx.SaveChanges();
            }
            else // register == null && user != null
            {
                string password = request.Form.password;
                if (! BCryptHelper.CheckPassword(password, user.PasswordHash))
                {
                    return UserBad("Неправильный пароль");
                }
            }
            // К этому моменту пользователь или аутентифицирован
            // или создан.
            // Установить сессию.

            // Сессии бывают двух типов
            // 1. На время одного сеанса
            // 2. Между сеансами.
            // Мы будем делать 2 из 1.
            // Т.е. в сессии типа 1 собирать (обновлять) данные
            //      зарегистрированного пользователя.

            request.Session["message"] = "Спокойно! Вы вошли в сисиему.";
            request.Session["user"] = user; // В принципе больше ничего не надо.

            // Надо в конце рендеринга убивать сообщение.

            return true;
        }

        protected static string SALT="saltsaltsaltsaltsaltsalt";

        protected Nancy.Request request = null;

        public new string Title="Идентификация пользователя";

    }
}
