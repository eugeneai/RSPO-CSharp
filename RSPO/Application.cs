using System;
using System.Collections.Generic;
using System.IO;
using Mono.Unix;
using Mono.Unix.Native;
using Nancy;
using Nancy.Hosting.Self;

namespace RSPO
{
	public partial class Application
	{
		static public MyEntityContext Context = null;
        static public ApplicationModel APPLICATION = null;

		[STAThread]
		static void Main(string[] args)
		{
			InitializeEntityContext();
			InitializeTemplating();

            APPLICATION = new ApplicationModel();

			var uri = "http://localhost:8888";
			Console.WriteLine("Starting Nancy on " + uri + "\n Ctrl-C to Stop.");

			// initialize an instance of NancyHost
			HostConfiguration hostConfigs = new HostConfiguration();
			hostConfigs.UrlReservations.CreateAutomatically = true;
			var host = new NancyHost(new Uri(uri),
				new Bootstrapper(),
				hostConfigs);


			host.Start();  // start hosting

			RunWithoutInterface();

			Console.WriteLine("Stopping Nancy");
			host.Stop();  // stop hosting
		}

		public static string GenerateHash(string input)
		{
			return input;
		}

		private static void RunWithoutInterface()
		{
			// check if we're running on mono
			if (Type.GetType("Mono.Runtime") != null)
			{
				// on mono, processes will usually run as daemons - this allows you to listen
				// for termination signals (ctrl+c, shutdown, etc) and finalize correctly
				UnixSignal.WaitAny(new[] {
					new UnixSignal(Signum.SIGINT),
					new UnixSignal(Signum.SIGTERM),
					new UnixSignal(Signum.SIGQUIT),
					new UnixSignal(Signum.SIGHUP)
				});
			}
			else
			{
				Console.ReadLine();
			}
		}
	}

	public static class VariousExtensionMethods
	{
		public static Dictionary<TValue, TKey> Reverse<TKey, TValue>(this IDictionary<TKey, TValue> source)
		{
			var dictionary = new Dictionary<TValue, TKey>();
			foreach (var entry in source)
			{
				if (!dictionary.ContainsKey(entry.Value))
					dictionary.Add(entry.Value, entry.Key);
			}
			return dictionary;
		}
	}
}
