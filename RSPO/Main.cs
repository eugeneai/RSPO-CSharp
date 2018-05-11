using System;
using System.Collections.Generic;
using Nancy;
using SharpTAL;
using System.Linq.Expressions;
using System.IO;
using BrightstarDB.EntityFramework;
using System.Linq; // Этого не хватало.

namespace RSPO
{
	public class WebModule : NancyModule
	{
		public WebModule(): base()
		{
            InitializeTemplating();
			Get["/"] = parameters =>
			{
                TestUser user = new TestUser()
                {
                    Name = "Ivanov Ivan"
                };
				return Render("index.pt", context: user, view: new TestUserView(user));
			};

            Get["/objs"] = parameters => // Это новая страница сайта.
                {
                    ObjectList objList = new ObjectList(); // Этот класс не существует пока.
                    ObjectListView objView = new ObjectListView(objList);
                    return Render("objlist.pt", context: objList, view: objView);
                };
			Get["/hello/{Name}"] = parameters =>
			{
				IAgent testModel = Application.Context.Agents.Create();
                testModel.Name = parameters.Name;
				return View["hello.html", testModel];
			};
		}

        private void InitializeTemplating()
        {
            string basePath =
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))));

            if (Type.GetType("Mono.Runtime") != null)
            {
                basePath = basePath.Replace("file:","");
            }
            else
            {
                basePath = basePath.Replace("file:\\", "");
            }

            TEMPLATE_LOCATION = Path.Combine(basePath, DESIGN_DIR);
            Console.WriteLine("Templates are at " + TEMPLATE_LOCATION);
        }

        private string DESIGN_DIR = Path.Combine("design-studio_one-page-template","build");
                    private string TEMPLATE_LOCATION = null;

        public string Render(string templateFile,
                             object context=null,  // Model
                             Request request=null, // Request
                             object view=null)       // View
        {
            string templateString = "";
            Template template = null;
            bool gotCache = Application.templateCache.TryGetValue(templateFile, out template);

            if (! gotCache) {
                string filePath = Path.Combine(TEMPLATE_LOCATION, templateFile);
                string tempPath123 = Path.Combine(TEMPLATE_LOCATION, "_!123!_").Replace("_!123!_","");
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
        int Size {get; }         // колиество объектов
        ICollection<T> Objects { get; }      // список объектов
        void SetFilter (Func<T, int, bool> filter);      // Условие формирования списка.
        void SetLimits (int start, int size); // Диапазон объектов для вывода на экран
        void Update ();          // Обновить список согласно условиям.
    }

    public interface IFilter {}  // Интерфейс, представляющий условие фильтрации.

    public class EntityList<T> : IObjectList<T> where T:class // Nice, Заглушки реализовались...
    // Это спсок квартир.... будет.
    {
        public static int DEFAULT_SIZE = 50;
        private Func<T, int, bool> filter = null;
        private int start = 0, size=DEFAULT_SIZE;

        public ICollection<T> Objects
        {
            get
            {
                return objectQuery.Skip(start).Take(size).ToList(); // Хотя мне не нравиться так.... но посмотрим.
            }
        }

        private IEnumerable<T> objectQuery = null;

        public int Size {
            get
            {
                return objectQuery.Count();
            }
         }

        public void SetFilter(Func<T, int, bool> filter)
        {
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
            // Работаем дальше по новому соединению. К стати потом можно скорость померить.
            MyEntityContext ctx = Application.Context;
            // Zerotier - хитрый, теперь по моему каналу свои пакеты гоняет. Этот, желтенький у тебя в трее

            // Все было просто..... да не просто.
            objectQuery = ctx.EntitySet<T>().Where(filter); // Ух ты. Жесть.
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

    public class ObjectList: EntityList<IObject>
    {
    }

    /// <summary>
    ///   Вспомогательный класс, помогающий рисовать изображение в HTML
    /// </summary>
    public class ObjectListView: EntityListView<ObjectList>
    {
        public ObjectListView(ObjectList context) : base(context)
        {
        }
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
