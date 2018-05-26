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
                ApplicationModel appModel = new ApplicationModel(Application.APPLICATION_NAME);
				return Render("index.pt", context: appModel, view: new ApplicationView(appModel));
			};

			Get["/objs"] = parameters => // Это страница сайта с квартирами.
			{
				ObjectList objList = new ObjectList();
				// Надо отлаживать в монодевелоп...
				ObjectListView objView = new ObjectListView(objList);
				return Render("objlist.pt", context: objList, view: objView);
			};

			Get["/offers"] = parameters =>
			{
				OfferList model = new OfferList();
				OfferListView view = new OfferListView(model);
				return Render("offerlist.pt", context: model, view: view);
			};

			Get["/offer/{GUID}"] = parameters => // Эта страница с индивидуальной квартирой
			{
                string GUID = parameters.GUID;
				IOffer model = Application.Context.Offers.Where(x => x.GUID==GUID).FirstOrDefault();

                // По идее в BrightStarDB есть у каждого объекта свой ID и наш
                // GUID можно к нему привязать. FIXME: Привязать!

                string msg = "Объект (Offer) не найден!: "+GUID;
                if (model==null)
                {
                    Console.WriteLine(msg);
                    // и я НЕ понял почему....
                    return "msg";
                } else Console.WriteLine(model);
                // Надо нудно искать ошибку в основном шаблоне....
                // Завтра. Вырубает....

				OfferView view = new OfferView(model);
				return Render("offer.pt", context: model, view: view);
			};

			Get["/agents"] = parameters =>
			{
				AgentList model = new AgentList();
				AgentListView view = new AgentListView(model);
				return Render("agentlist.pt", context: model, view: view);
			};

			Get["/login"] = parameters => // Эта страница уже лет 20 не нужна.
                {
                    LoginObject model = new LoginObject();
                    LoginView view = new LoginView(model);

                    // return View["login.pt", testModel]; // Оставим для истории.
                    // Это, к стати правильный вариант отрисовки по шаблону.

                    return Render("login.pt", context: model, view:view);
                };

            // Принимаем данные пользователя из формы регистрации
            Post["/login"] = parameters =>
                {
                    LoginObject model = new LoginObject();
                    LoginView view = new LoginView(model, request: this.Request);
                    if (view.Process())
                    {
                        return new RedirectResponse("/");
                    } else
                    {
                        return new RedirectResponse("/login");
                        /*
                        return new Response()
                        {
                            StatusCode = HttpStatusCode.Forbidden
                        };
                        */
                    }
                    // Перенаправить браузер на домашнюю страницу.
                };
		}


		public string Render(string templateFile,
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

			if (this.Request == null)
			{
				Request = this.Request;
				dict.Add("request", Request);
			}
			if (context != null)
			{
				dict.Add("model", context);
			}

            if (view == null) throw new RenderException("Null view");

			dict.Add("view", view);
            dict.Add("application", Application.APPLICATION);
            dict.Add("appview", new ApplicationView(Application.APPLICATION));

            MessageModel message = (MessageModel) Request.Session["message"];
            if (message == null)
            {
                message = new MessageModel(); // Пустое сообщение.
                Console.WriteLine("->>>> Empty message");
            }

            IAgent user = (IAgent) Request.Session["user"];
            if (user == null)
            {
                user = new InvalidUser();
            }

            dict.Add("message", message);
            dict.Add("user", user);
            dict.Add("nothing", "");

			string result = template.Render(dict);
            Request.Session.Delete("message");
            return result;
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

    public class RenderException:Exception
    {
        public RenderException(string msg):base(msg) {}
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
		private Func<T, bool> filter = null;
		private int start = 0, size = DEFAULT_SIZE;

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

		private IEnumerable<T> objectQuery = null;

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

		public void Update()
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

        protected MessageModel info(string message=null, string msg="", AlertType alert=AlertType.Info)
        {
            return new MessageModel(message, msg, alert);
        }

        protected MessageModel error(string message=null, string msg="", AlertType alert=AlertType.Danger)
        {
            return info(message, msg, alert);
        }
	}


	public class EntityListView<T> : View<T> where T : class
	{
		public EntityListView(T context) : base(context)
		{

		}
	}
}
