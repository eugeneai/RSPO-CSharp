using System;
using System.Collections.Generic;
using Nancy;
using SharpTAL;
using System.Linq.Expressions;
using System.IO;
using BrightstarDB.EntityFramework;
using System.Linq; // Этого не хватало.
using System.Globalization;

namespace RSPO
{
	public class WebModule : NancyModule
	{
		public WebModule(): base()
		{
			Get["/"] = parameters =>
			{
                TestUser user = new TestUser()
                {
                    Name = "Ivanov Ivan"
                };
				return Render("index.pt", context: user, view: new TestUserView(user));
			};

            Get["/objs"] = parameters => // Это страница сайта с квартирами.
                {
                    ObjectList objList = new ObjectList();
                    // Надо отлаживать в монодевелоп...
                    ObjectListView objView = new ObjectListView(objList);
                    return Render("objlist.pt", context: objList, view: objView);
                };

            Get["/offers"] = parameters => // Это новая страница сайта.
                {
                    OfferList model = new OfferList();
                    OfferListView view = new OfferListView(model);
                    return Render("offerlist.pt", context: model, view: view);
                };

			Get["/hello/{Name}"] = parameters =>
			{
				IAgent testModel = Application.Context.Agents.Create();
                testModel.Name = parameters.Name;
				return View["hello.html", testModel];
			};
		}


        public string Render(string templateFile,
                             object context=null,  // Model
                             Request request=null, // Request
                             object view=null)       // View
        {
            string templateString = "";
            Template template = null;
            bool gotCache = Application.templateCache.TryGetValue(templateFile, out template);

            if (! gotCache) {
                string filePath = Path.Combine(Application.TEMPLATE_LOCATION, templateFile);
                string tempPath123 = Path.Combine(Application.TEMPLATE_LOCATION, "_!123!_").Replace("_!123!_","");
                Console.WriteLine("Template Path:" + filePath);
                templateString = File.ReadAllText(filePath);
                templateString = templateString.Replace("@TEMPLATEDIR@", tempPath123);  // Подстановка местораположения шаблонов в текст шаблона.
                template = new Template(templateString);
                Application.templateCache.Add(templateFile, template);
            }

            var dict = new Dictionary<string, object>();

            if (request==null) {
                request = this.Request;
                dict.Add("request", request);
            }
            if (context!=null)
            {
                dict.Add("model", context);
            }

            dict.Add("view", view);

            return template.Render(dict);
        }
    }

	public partial class Application
	{
		public static Dictionary<string, Template> templateCache = new Dictionary<string, Template>();
	}

    // Это у нас абстрация модели. Здесь мы работаем с объектами, получаемыми из базы данных
    public interface IObjectList<T> where T: class // Интерфес списков объектов недвижимости
    {
        int Size {get; }                      // колиество объектов
        ICollection<T> Objects { get; }       // список объектов
        void SetFilter (Func<T, bool> filter);// Условие формирования списка.
        void SetLimits (int start, int size); // Диапазон объектов для вывода на экран
        void Update ();                       // Обновить список согласно условиям.
    }

    public class EntityList<T> : IObjectList<T> where T:class // Nice, Заглушки реализовались...
    // Это список сущностей. Реализовано при помои обоенного программирования.
    // <T>. Вместо T подставляется тип или интерфейс элемента.
    // переклась на  студентку...

    {
        public static int DEFAULT_SIZE = 50; // 50 из 5200 прим.
        private Func<T, bool> filter = null;
        private int start = 0, size=DEFAULT_SIZE;

        public EntityList(Func<T, bool> filter = null, bool update = true)
        {
            SetFilter(filter);
            if (update) Update();
        }

        public class BadQueryException:Exception
        {
            public BadQueryException(string msg) : base(msg) {}
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

        public int Size {
            get
            {
                return objectQuery.Count();
            }
         }

        public void SetFilter(Func<T, bool> filter = null)
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

    public class View<T> where T:class // жесть прошла.
    {
        public string Title="A Default page!";
        public T context;
        public View(T context)
        {
            this.context = context;
        }

        protected View() {}
    }


    public class EntityListView<T>: View<T> where T:class
    {
        public EntityListView(T context) : base(context)
        {

        }
    }

    /// <summary>
    ///   Список объектов недвижимости
    /// </summary>
    public class ObjectList: EntityList<IObject>
    {
    }

    public class ObjectListView: EntityListView<ObjectList>
    {
        public new string Title = "Список объектов недвижимости";
        public ObjectListView(ObjectList context) : base(context) { }
        public ObjectView AsObjectView(IObject obj) {return new ObjectView(obj);}
    }

    public class ObjectView: View<IObject>
    {
        public ObjectView(IObject context): base(context) {} // context == model
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
                var o=context;
                if (o.FloorTotal<=0) return "";
                if (o.Floor<=0) return o.FloorTotal.ToString();
                return o.Floor.ToString()+"/"+o.FloorTotal.ToString();
            }
        }

        public string ROR
        {
            get
            {
                var o = context;
                if (o.Rooms<=0) return "";
                if (o.RoomsOffered<=0) return o.Rooms.ToString()+"к";
                return o.RoomsOffered.ToString()+"/"+o.Rooms.ToString()+"к";
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
                    return val.Substring(0,4).ToLower();
                }
                return "";
            }
        }

        public string Price
        {
            get
            {
                numberFormatInfo.CurrencySymbol="";
                return context.Price.ToString("C", numberFormatInfo).Replace(' ', '\u00A0');
            }
        }

        static CultureInfo culture = new CultureInfo("ru-RU", false);
        static NumberFormatInfo numberFormatInfo = (NumberFormatInfo)culture.NumberFormat.Clone();
    }

    /// <summary>
    ///   Список предложений.
    /// </summary>

    public class OfferList: EntityList<IOffer> { } // Список предложений (Модель)

    public class OfferListView: EntityListView<OfferList> // Представление (View) для списка предложений.
    {
        public new string Title = "Список предложений";
        public OfferListView(OfferList context) : base(context) { }
        public OfferView AsOfferView(IOffer offer) {return new OfferView(offer);}
        public ObjectView AsObjectView(IObject obj) {return new ObjectView(obj);}
        public ObjectView AsObjectView(IOffer offer)
        {
            return AsObjectView(offer.Object);
        }
    }

    public class OfferView: View<IOffer>
    {
        public OfferView(IOffer context): base(context) {} // context == model
    }


    //---------------- Testing

        public class TestUser
    {
        public string Name = ".... ogly";
        public TestUser()
        {
        }
    }

    public class TestUserView : View<TestUser>
    {
        public string Name
        {
            get {
                return context.Name;
            }
        }

        public TestUserView(TestUser context) : base(context) { }
    }


}
