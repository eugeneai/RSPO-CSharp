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
				return Render("index.pt");
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
            templateCache = new Dictionary<string, string>();
        }

        private Dictionary<string, string> templateCache = null ;

        private string DESIGN_DIR = "design-studio_one-page-template/build";
        private string TEMPLATE_LOCATION = null;

        public string Render(string templateFile,
                                object context=null,
                                Request request=null,
                                View view=null)
        {
            string templateString = "";
            bool gotCache = templateCache.TryGetValue(templateFile, out templateString);

            if (! gotCache) {
                string filePath = Path.Combine(TEMPLATE_LOCATION, templateFile);
                templateString = File.ReadAllText(filePath);
                templateCache.Add(templateFile, templateString);
            }

            var template = new Template(templateString);
            var dict = new Dictionary<string, object>();
            if (request==null) {
                request = this.Request;
            }
            if (context!=null)
            {
                dict.Add("context", context);
            }
            dict.Add("request", request);
            dict.Add("view", view);
            dict.Add("nothing", false);
            dict.Add("default", true);

            return template.Render(dict);
        }
	}

    public class View
    {

    }
}
