using System;
using Nancy;

namespace RSPO
{
	public class HelloModule : NancyModule
	{
		public HelloModule()
		{
			Get["/"] = parameters => "Hello World";
		}
	}
}
