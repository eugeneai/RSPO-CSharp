using System;
using System.Collections.Generic;
using Nancy;
using SharpTAL;
using System.Linq.Expressions;
using System.IO;
using BrightstarDB.EntityFramework;
using System.Linq; // Этого не хватало.
using System.Globalization;
using Nancy.Responses;

namespace RSPO
{
	public class WebModule : NancyModule
	{
		public WebModule() : base()
		{
			Get["/"] = parameters =>
			{
				RestoreSession();
				ApplicationModel appModel = new ApplicationModel(Application.APPLICATION_NAME);
				return Render("index.pt", context: appModel,
							  view: new ApplicationView(appModel, CurrentSession));
			};

			Get["/objs"] = parameters => // Это страница сайта с квартирами.
			{
				RestoreSession();

				ObjectList objList = new ObjectList();
				// Надо отлаживать в монодевелоп...
				ObjectListView objView = new ObjectListView(objList);
				return Render("objlist.pt", context: objList, view: objView);
			};

			Get["/offers"] = parameters =>
			{
				RestoreSession();
				OfferList model = new OfferList(null);
				OfferListView view = new OfferListView(model);
				return Render("offerlist.pt", context: model, view: view);
			};

			Get["/offers/{clid}"] = parameters =>
			{
				int clid = int.Parse(parameters.clid);
				RestoreSession();
				OfferList model = new OfferList(clid: clid);
				OfferListView view = new OfferListView(model);
				return Render("offerlist.pt", context: model, view: view);
			};

			Get["/offer/{GUID}"] = parameters => // Эта страница с индивидуальной квартирой
			{
				RestoreSession();
				string GUID = parameters.GUID;
				IOffer model = Application.Context.Offers.Where(x => x.GUID == GUID).FirstOrDefault();

				// По идее в BrightStarDB есть у каждого объекта свой ID и наш
				// GUID можно к нему привязать. FIXME: Привязать!

				string msg = "Объект (Offer) не найден!: " + GUID;
				if (model == null)
				{
					Console.WriteLine(msg);
					// и я НЕ понял почему....
					return "msg";
				}
				else Console.WriteLine(model);
				// Надо нудно искать ошибку в основном шаблоне....
				// Завтра. Вырубает....

				OfferView view = new OfferView(model);
				return Render("offer.pt", context: model, view: view);
			};

			Get["/agents"] = parameters =>
			{
				RestoreSession();
				AgentList model = new AgentList();
				AgentListView view = new AgentListView(model);
				return Render("agentlist.pt", context: model, view: view);
			};

			Get["/login"] = parameters => // Эта страница уже лет 20 не нужна.
			{
				RestoreSession();
				LoginObject model = new LoginObject();
				LoginView view = new LoginView(model, this.Request, CurrentSession);

				// return View["login.pt", testModel]; // Оставим для истории.
				// Это, к стати правильный вариант отрисовки по шаблону.

				return Render("login.pt", context: model, view: view);
			};

			// Принимаем данные пользователя из формы регистрации
			Post["/login"] = parameters =>
				{
					RestoreSession();

					LoginObject model = new LoginObject();
					LoginView view = new LoginView(model, this.Request, CurrentSession);

					Response response = null;
					bool res = view.Process();

					CurrentSession = view.Session; // Обновление сессии
					if (res)
					{
						response = Response.AsRedirect("/");
					}
					else // Неуданая идентификация
					{
						response = Response.AsRedirect("/login");
					}
					// Перенаправить браузер на домашнюю страницу.
					return InSession(response);
				};

			Get["/logout"] = parameters => // Эта страница уже лет 20 не нужна.
			{
				RestoreSession();
				LoginObject model = new LoginObject();
				LoginView view = new LoginView(model, this.Request, CurrentSession);

				// return View["login.pt", testModel]; // Оставим для истории.
				// Это, к стати правильный вариант отрисовки по шаблону.
				view.Logout();
				CurrentSession = view.Session;

				return Render("login.pt", context: model, view: view);
			};

			Post["/clustering"] = parameters =>
				{
					RestoreSession();

					Response response = null;

					int num = 0;
					try
					{
						num = int.Parse(this.Request.Form.max);
						int clnum = 5;
						FlatClusterAnalyzer a = FlatClusterAnalyzer.AnalyzeFlatWithCluster(num);
						a.Store(clnum);

						CurrentSession["message"] = info("Обработано для " + num + " квартир, " + clnum + " кластеров",
													   msg: "Успешный запуск");
						CurrentSession["analysis_data"] = a;
					}
					catch (FormatException)
					{
						CurrentSession["message"] = error("Неправильное число квартир", msg: "Неуспешный запуск");
					}

					response = Response.AsRedirect("/analysis");
					return InSession(response);
				};

			Get["/analysis"] = parameters =>
			{
				RestoreSession();
				ClusterList model = new ClusterList();
				ClusterListView view = new ClusterListView(model);
				return Render("clusters.pt", context: model, view: view);
			};

			Post["/analysis"] = parameters =>
				{
					RestoreSession();
					ClusterList model = new ClusterList();
					ClusterListView view = new ClusterListView(model);
					FlatClusterAnalyzer analyzer = null;
					var form = this.Request.Form;
					if (form.reconstruct != null)
					{
						try
						{
							analyzer = (FlatClusterAnalyzer)CurrentSession["analysis_data"];
							int k = int.Parse(form.numclusters);
							Console.WriteLine("---> K=" + k);
							analyzer.Store(k);
							CurrentSession["message"] = info("Произведена перестройка кластера", msg: "Удачное завершение операции");
						}
						catch
						{
							// В сессии нет данных по кластеру.
							CurrentSession["message"] = error("Похоже кластер не рассчитан", msg: "Неудачная операция");

						}
					}
					return InSession(Response.AsRedirect("/analysis"));
				};
		}

		protected static string IN_SESSION_COOKIE_NAME = "_rspo_state";

		public static Dictionary<string, SessionModel> activeSessions = new Dictionary<string, SessionModel>();


		protected Nancy.Response InSession(Nancy.IResponseFormatter response = null)
		{
			if (response == null) response = this.Response;
			return InSession((Nancy.Response)response);
		}

		protected Nancy.Response InSession(Nancy.Response response = null)
		{
			if (response == null) throw new RenderException("null response object");

			if (String.IsNullOrEmpty(CurrentSession.GUID))
			{
				CurrentSession.GUID = ImportFromAtlcomru.GetGUID();
				CurrentSession["valid"] = "false";
			}

			activeSessions[CurrentSession.GUID] = CurrentSession;

			return response.WithCookie(IN_SESSION_COOKIE_NAME, CurrentSession.GUID);
		}

		protected void RestoreSession()
		{
			string value = "";
			try
			{
				value = this.Request.Cookies[IN_SESSION_COOKIE_NAME];
			}
			catch (KeyNotFoundException)
			{
				value = "";
			}

			CurrentSession = new SessionModel();
			CurrentSession["valid"] = false;

			if (String.IsNullOrEmpty(value))
			{
				// Сессия не устанвлена, т.е. пользователь не зарегистрирован
				CurrentSession.GUID = ImportFromAtlcomru.GetGUID();
				// Сделать сессии идентификатор.
				return;
			}
			try
			{
				CurrentSession = activeSessions[value]; // По идее там будет где-то пользователь.
				object ouser = null;
				CurrentSession["valid"] = CurrentSession.TryGetValue("user", out ouser);
				// Сессия валидна, если оттуда можно вытащить пользователя.
				CurrentSession["valid"] = ouser != null;
				CurrentSession["GUID"] = value;
				return;
			}
			catch (System.Collections.Generic.KeyNotFoundException)
			{
				// Беспонтовая сессия, нам оно не надо
				// Возвращяем невалидную сессию по умолчанию
				CurrentSession.GUID = ImportFromAtlcomru.GetGUID();
				// Пусть будет беспонтовая анонимная новая сессия.
			}
		}

		public static MessageModel info(string message = null, string msg = "", AlertType alert = AlertType.Info)
		{
			return new MessageModel(message, msg, alert);
		}

		public static MessageModel error(string message = null, string msg = "", AlertType alert = AlertType.Danger)
		{
			return info(message, msg, alert);
		}


		public SessionModel CurrentSession = null;

		public Nancy.Response Render(string templateFile,
									 object context = null,  // Model
									 object view = null)       // View
		{
			string templateString = "";
			Template template = null;

			bool gotCache = false;

			if (Application.USE_TEMPLATE_CACHE)
			{
				gotCache = Application.templateCache.TryGetValue(templateFile, out template);
			}

			// В режиме отладки иногда удобно, если при каждом запросе шаблон заново верстается.
			// Не надо сервак перезапускать при изменении шаблона.

			if (!gotCache)
			{
				string filePath = Path.Combine(Application.TEMPLATE_LOCATION, templateFile);
				string tempPath123 = Path.Combine(Application.TEMPLATE_LOCATION, "_!123!_").Replace("_!123!_", "");
				Console.WriteLine("Template Path:" + filePath);
				templateString = File.ReadAllText(filePath);
				templateString = templateString.Replace("@TEMPLATEDIR@", tempPath123);  // Подстановка местораположения шаблонов в текст шаблона.
				template = new Template(templateString);

				if (Application.USE_TEMPLATE_CACHE)
				{
					Application.templateCache.Add(templateFile, template);
				}
			}

			var dict = new Dictionary<string, object>();

			if (this.Request != null)
			{
				Request = this.Request;
				dict.Add("request", Request);
			}
			if (context != null)
			{
				dict.Add("model", context);
			}

			if (view == null) throw new RenderException("null view");

			dict.Add("view", view);
			dict.Add("application", Application.APPLICATION);
			dict.Add("appview", new ApplicationView(Application.APPLICATION, CurrentSession));

			object omessage = null;
			MessageModel message = null;
			bool bmsg = CurrentSession.TryGetValue("message", out omessage);

			if (!bmsg)
			{
				message = new MessageModel(); // Пустое сообщение.
			}
			else
			{
				message = (MessageModel)omessage;
			}

			IAgent user = null;
			object ouser = null;
			bool buser = CurrentSession.TryGetValue("user", out ouser);
			if (!buser)
			{
				user = new InvalidUser();
			}
			else
			{
				user = (IAgent)ouser;
			}

			dict.Add("message", message);
			dict.Add("user", user);
			dict.Add("nothing", "");

			string result = template.Render(dict);
			CurrentSession.Remove("message");
			return InSession(result);
		}
	}

	public class InvalidUser : IAgent
	{
		public InvalidUser() { }

		public bool Valid
		{
			get
			{
				return false;
			}
		}

		string IAgent.Name { get => "Invalid User"; set => throw new NotImplementedException(); }
		string IAgent.NickName { get => "undefined"; set => throw new NotImplementedException(); }
		string IAgent.PasswordHash { get => ""; set => throw new NotImplementedException(); }
		string IAgent.Phone { get => ""; set => throw new NotImplementedException(); }
		string IAgent.Email { get => ""; set => throw new NotImplementedException(); }
		RoleEnum IAgent.Role { get => RoleEnum.Unknown; set => throw new NotImplementedException(); }
		string IAgent.GUID { get => ""; set => throw new NotImplementedException(); }
		ICollection<IProperty> IAgent.Properties { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		bool IAgent.Valid { get => false; }
	}

	public class RenderException : Exception
	{
		public RenderException(string msg) : base(msg) { }
	}

	public partial class Application
	{
		public static Dictionary<string, Template> templateCache = new Dictionary<string, Template>();
	} // TODO: Навожу порядок в monodevelop...

	// Это общая часть машей надстройки (микрофрейморк MVVM - MVC)
	// Это у нас абстрация модели. Здесь мы работаем с объектами, получаемыми из базы данных
	public interface IObjectList<T> where T : class // Интерфес списков объектов недвижимости
	{
		int Size { get; }                      // колиество объектов
		ICollection<T> Objects { get; }       // список объектов
		void SetFilter(Func<T, bool> filter);// Условие формирования списка.
		void SetLimits(int start, int size); // Диапазон объектов для вывода на экран
		void Update();                       // Обновить список согласно условиям.
	}

	public class EntityList<T> : IObjectList<T> where T : class
		// Это список сущностей. Реализовано при помои обобщенного программирования.
		// <T>. Вместо T подставляется тип или интерфейс элемента.
	{
		public static int DEFAULT_SIZE = 50; // 50 из 5200 прим.
		protected Func<T, bool> filter = null;
		protected int start = 0, size = DEFAULT_SIZE;

		public EntityList(Func<T, bool> filter = null, bool update = true)
		{
			SetFilter(filter);
			if (update) Update();
		}

		public class BadQueryException : Exception
		{
			public BadQueryException(string msg) : base(msg) { }
		}

		public ICollection<T> Objects
		{
			get
			{
				ICollection<T> res = objectQuery.Skip(start).Take(size).ToList();
				if (res == null) throw new BadQueryException("query returned null"); // Для отслаживания плохих запросов
				return res; // Хотя мне не нравиться так.... но посмотрим.
			}
		}

		protected IEnumerable<T> objectQuery = null;

		public int Size
		{
			get
			{
				return objectQuery.Count();
			}
		}

		public void SetFilter(Func<T, bool> filter = null)
		{
			if (filter == null) filter = x => true; // Функция (лямбда) с одним аргументом x,
													// .... возвращает истину, т.е. все записи.
			this.filter = filter;
		}

		public void SetLimits(int start = 0, int size = 50)
		{
			this.start = start;
			this.size = size;
		}

		public virtual void Update()
		{
			// Здесь сделаем запрос и присвоим objects список - результат запроса.
			MyEntityContext ctx = Application.Context;

			objectQuery = ctx.EntitySet<T>().Where(filter);
		}
	}

	public class View<T> where T : class // жесть прошла.
	{
		public string Title = "Заголовок надо поменять!";
		public T context;
		public View(T context)
		{
			this.context = context;
		}

		protected View() { }

		public static MessageModel error(string message = null, string msg = "", AlertType alert = AlertType.Danger)
		{
			return WebModule.error(message, msg, alert);
		}

		public static MessageModel info(string message = null, string msg = "", AlertType alert = AlertType.Info)
		{
			return WebModule.info(message, msg, alert);
		}

	}


	public class EntityListView<T> : View<T> where T : class
	{
		public EntityListView(T context) : base(context)
		{

		}
	}
}
