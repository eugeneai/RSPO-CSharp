using System;
using Nancy;

namespace RSPO
{
    public class MainModule : NancyModule
    {
        public MainModule()
        {
            Get["/"] = parameters => "Hello World";
        }
    }
}
