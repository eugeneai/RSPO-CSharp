using System;
using System.Collections.Generic;
using Nancy;
using SharpTAL;
using System.Linq.Expressions;

namespace RSPO
{
	public class MainModule : NancyModule
	{
		public MainModule()
		{
			Get["/"] = parameters =>
			{
				var globals = new Dictionary<string, object>
				{
					{ "movies", new List<string> { "alien", "star wars", "star trek" } }
				};

				const string body = @"<!DOCTYPE html>
					<html tal:define='textInfo new System.Globalization.CultureInfo(""en-US"", false).TextInfo'>
    					Favorite sci-fi movies:
    				<div tal:repeat='movie movies'>${textInfo.ToTitleCase(movie)}</div>
					</html>";

				var template = new Template(body);

				var result = template.Render(globals);

				return result;
			};
			Get["/test"] = parameters =>
			{
				var testModel = new TestModel("Вано");
				return View["Hello.html", testModel];
			};
		}
	}
}
