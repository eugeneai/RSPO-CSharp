using System;
using Mono.Unix;
using Mono.Unix.Native;
using Nancy;
using Nancy.Hosting.Self;

namespace RSPO
{
	class Application
	{
        static public MyEntityContext Context = null;
        static string ConnectionString = "type=embedded;storesdirectory=./;storename=RSPO";

        static void Main(string[] args)
        {
            InitializeEntityContext();
            var uri = "http://localhost:8888";
            Console.WriteLine("Starting Nancy on " + uri + "\n Ctrl-C to Stop.");

            // initialize an instance of NancyHost
            HostConfiguration hostConfigs = new HostConfiguration();
            hostConfigs.UrlReservations.CreateAutomatically = true;
            var host = new NancyHost(new Uri(uri),
                new DefaultNancyBootstrapper(),
                hostConfigs);


            host.Start();  // start hosting

            // RunWithoutInterface();
            RunWindowsFormUI();

            Console.WriteLine("Stopping Nancy");
            host.Stop();  // stop hosting
        }

        private static void InitializeEntityContext()
        {
            Context = new MyEntityContext(ConnectionString);
        }

        private static void RunWindowsFormUI()
        {
            IUser testUser = Context.Users.Create();
            testUser.Email = "vano@gnail.ru";
            testUser.Name = "Цискаридзе Вано ибн Петро аглы";
            testUser.NickName = "hottubych";
            UserForm view = new UserForm
            {
                Context = testUser,
                Visible = false
            };
            view.ShowDialog();

            String msg = String.Format("<{0}:{1}> {2} #{3}", 
                testUser.Name, 
                testUser.NickName,
                testUser.Email,
                testUser.PasswordHash);
            Console.WriteLine(msg);
            Console.ReadLine();
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
}
