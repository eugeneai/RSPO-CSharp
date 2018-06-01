using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DevOne.Security.Cryptography.BCrypt;
using Nancy.Session;

namespace RSPO
{
	public class Pair<T1, T2>
	{
		public T1 First;
		public T2 Second;
		public Pair(T1 first, T2 second)
		{
			First = first;
			Second = second;
		}
	}

	public class ApplicationView : View<ApplicationModel>
	{
		public ApplicationView(ApplicationModel context, SessionModel session) : base(context)
        {
            this.session = session;
        }

        protected SessionModel session;

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

		public List<Pair<string, Uri>> Menu
		{
			get
			{
				List<Pair<string, Uri>> l = new List<Pair<string, Uri>>{
					menu("Главная", "/"),
					menu("Предложения", "/offers"),
					menu("Агенты", "/agents"),
					menu("Анализ", "/analysis"),
                };
                if (session.Valid)
                {
                    l.Add(menu("Выйти", "/logout"));
                }
                else
                {
                    l.Add(menu("Войти", "/login"));
                }
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

	public class OfferList : EntityList<IOffer> // Список предложений (Модель)
    {
        protected int? clid = null;

		public OfferList(int? clid) : base(update: false)
        {
            this.clid = clid;
			Update();
        }

        protected List<IObjectClass> objectClasses = null;
        protected List<IOffer> offers = null;

        public override void Update()
        {
            if (clid == null)
            {
                base.Update();
            }
            else
            {
                prepareLists();
                objectQuery = offers.Where(filter);
            }
        }

        public void prepareLists()
        {
            MyEntityContext ctx = Application.Context;
            objectClasses = ctx.ObjectClasss.Where(x => x.Cluster == clid).Skip(start).Take(size).ToList();
            offers = new List<IOffer>();
            foreach (IObjectClass oc in objectClasses)
            {
                IOffer offer = ctx.Offers.Where(x=>x.Object.GUID == oc.Object.GUID).FirstOrDefault();
                offers.Add(offer);
            }
        }
    }

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
				return ObjView.RuCategory + ", " + Object.Address;
			}
		}

		public string ImageTag
		{
			get
			{
				if (Object.ImageURL != null)
				{
					return string.Format("<img src=\"{0}\" alt=\"Фото квартиры по адресу: {1}\" height=\"400pt\" />",
										 Object.ImageURL, Object.Address);
				}
				else
				{
					return "<a href=\"https://www.freeiconspng.com/img/23501\" " +
						"title=\"Image from freeiconspng.com\">" +
						"<img src=\"https://www.freeiconspng.com/uploads/no-image-icon-24.jpg\" " +
						"width=\"350\" alt=\"No Icon Transparent\" /></a>";
				}
			}
		}
		public string Maps
		{
			get
			{
				return Object.Address.Trim().Replace(" ", "+") + ",+Иркутск";
			}
		}
		public bool Valid
		{
			get
			{
				return !String.IsNullOrEmpty(Object.Address);
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

	public class InvalidSession : Exception
	{
		public InvalidSession(string message) : base(message) { }
	}

	public class LoginView : View<LoginObject>
	{
		public LoginView(LoginObject context, Nancy.Request request,
						 SessionModel session) : base(context)
		{
			this.request = request;
			if (session == null)
			{
				throw new InvalidSession("forgot to supply sesstion object");
			}
			this.Session = session;
		}

		protected bool UserBad(string message)
		{
            string GUID = Session.GUID;
			Session.Clear(); // Аннулировать сессию, однако...
			Session.GUID = GUID;
			Session["valid"] = false;
			Session["message"] = error(message + ", однако.", msg: "Неуспешная идентификация");
			return false;
		}

		public bool Process()
		{
			MyEntityContext ctx = Application.Context;

			string nick = request.Form.user;
			string phone = request.Form.phone;
            Console.WriteLine("---> FORM:"+nick);
			IAgent user = ctx.Agents.Where(x => x.NickName == nick).FirstOrDefault();
			string register = request.Form["register"];

            MessageModel success = null;

			if (register != null && user != null)
			{
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
				user.NickName = nick;
				user.Email = request.Form.email;
				ctx.Add(user);
				ctx.SaveChanges();
                success = info("Теперь вы зарегистрированы в системе. Можно начинать бояться.",
                               msg: "Успешная регистрация");
			}
			else // register == null && user != null
			{
				string password = request.Form.password;
				if (!BCryptHelper.CheckPassword(password, user.PasswordHash))
				{
					return UserBad("Неправильный пароль");
				}
                success = info("Ну вот вы и вошли в систему.", msg: "Успешная идентикация");

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

			Session = new SessionModel(); //Создание новой сессии
			Session["valid"] = true;
			Session["user"] = user;   // Объект пользователя в сессии
			Session.GUID = user.GUID; // Идентификатор сессии пользователя.

			Session["message"] = success;

			Console.WriteLine("Linux rulez!");

			// TODO: Еще надо сделать выход из сессии при разлогигивании. Но пока у нас нет такой команды.

			return true;
		}

        public void Logout()
        {
            Session = new SessionModel();
            Session["valid"] = false;
            Session.GUID = ImportFromAtlcomru.GetGUID();
            Session["message"] = info("Вы вышли из системы и мы про вас забыли.", "Успехов");
            // Плохо то, что у нас будут копиться анонимные сессии.
        }

		protected static string SALT = "$2a$10$.lvjuUJj9nor/DArhPtrgu"; // BCryptHelper.GenerateSalt();

		public SessionModel Session = null; // Если пользователь будет валидный, то
											// этот объект заменится на новую сессию.

		protected Nancy.Request request = null;

		public new string Title = "Идентификация пользователя";

	}
}
