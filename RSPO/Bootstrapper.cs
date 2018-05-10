using System;
using System.Collections.Generic;
using Nancy;
using SharpTAL;
using System.Linq.Expressions;
using System.IO;
using System.Reflection;
using Nancy.Conventions;

namespace RSPO
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        public static string TEMPLATE_LOCATION = null;

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            string staticDir = Path.Combine(TEMPLATE_LOCATION, "static");
            base.ConfigureConventions(nancyConventions);

            Console.WriteLine("Initializing Bootstrapper");

            Conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("static",staticDir)
                );
        }
    }
}
