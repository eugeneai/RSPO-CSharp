using System;
using System.Collections.Generic;
using Nancy;
using SharpTAL;
using System.Linq.Expressions;
using System.IO;

namespace RSPO
{
	public class WebModule : NancyModule
	{
		public WebModule(): base()
		{
            InitializeTemplating();
			Get["/"] = parameters =>
			{
				return Render("index.pt", context: new TestUser());
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
            templateCache = new Dictionary<string, string>();
        }

        private Dictionary<string, string> templateCache = null ;

        private string DESIGN_DIR = Path.Combine("design-studio_one-page-template","build");
                    private string TEMPLATE_LOCATION = null;

        public string Render(string templateFile,
                             object context=null,  // Model
                             Request request=null, // Request
                             View view=null)       // View
        {
            string templateString = "";
            bool gotCache = templateCache.TryGetValue(templateFile, out templateString);

            if (! gotCache) {
                string filePath = Path.Combine(TEMPLATE_LOCATION, templateFile);
                string tempPath123 = Path.Combine(TEMPLATE_LOCATION, "_!123!_").Replace("_!123!_","");
                Console.WriteLine("Template Path:" + filePath);
                templateString = File.ReadAllText(filePath);
                templateString = templateString.Replace("@TEMPLATEDIR@", tempPath123);  // Подстановка местораположения шаблонов в текст шаблона.
                templateCache.Add(templateFile, templateString);
            }

            var template = new Template(templateString);
            var dict = new Dictionary<string, object>();

            if (request==null) {
                request = this.Request;
                dict.Add("request", request);
            }
            if (context!=null)
            {
                //dict.Add("context", context);
                // dict.Add("model", new List<string> { "alien", "star wars", "star trek" });
                // dict.Add("model", context);
                dict.Add("model", context);
            }
            if (view == null)
            {
                view = new View();
                Console.WriteLine("created new view");
            }

            // dict.Add("view", view);

            // dict.Add("nothing", false);
            // dict.Add("default", true);
            Console.WriteLine("Dict:");
            foreach (KeyValuePair<string, object> kvp in dict)
            {
                Console.WriteLine(string.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value));
            }
            return template.Render(dict);
            // return "11";
        }
    }

    internal class TestUser
    {
        public string Name = ".... ogly";
        public TestUser()
        {
        }
    }

    public class View
    {
        public string Title="A Default page!";
    }

    public class StupidView: View {
        public new string Title="A Stupid EMPTY PAGE!!!!!";
    }
}
