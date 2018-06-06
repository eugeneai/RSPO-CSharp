using System;
using System.Collections.Generic;
using System.Linq;

namespace RSPO
{
	public class ApplicationModel
	{
		public ApplicationModel(string Name = "Рекомендательная система по рынку недвижимости Иркутской области")
		{
			this.Name = Name;
		}
		public string Name;
	}

    public class LoginObject // FIXME: Использован как заглушка.
    {
        public LoginObject() {}
    }

    [Serializable]
    public class MessageModel // Класс, показывающий сообщения
    {
        public MessageModel(string message=null, string msg="", AlertType alert=AlertType.Info)
        {
            Message = message;
            Msg   = msg;
            this.alert = alert;
        }

        public readonly string Msg = null;
        protected readonly AlertType alert = AlertType.Info;
        public readonly string Message = null;

        public string Alert
        {
            get
            {
                return alerts[alert];
            }
        }

        public bool Exists
        {
            get
            {
                return ! String.IsNullOrEmpty(Message);
            }
        }

        private Dictionary<AlertType,string> alerts = new Dictionary<AlertType,string>
        {
            {AlertType.Primary, "primary"},
            {AlertType.Secondary, "secondary"},
            {AlertType.Success, "success"},
            {AlertType.Danger, "danger"},
            {AlertType.Warning, "warning"},
            {AlertType.Info, "info"},
            {AlertType.Light, "light"},
            {AlertType.Dark, "dark"}
        };
    }

    public enum AlertType
    {
        Primary,
        Secondary,
        Success,
        Danger,
        Warning,
        Info,
        Light,
        Dark
    }

    public class SessionModel : Dictionary<string, object> // Ничего особенного, просто словарь
    {
        public SessionModel(): base() {}

        public string GUID
        {
            set
            {
                this["GUID"] = value;
            }

            get
            {
                return (string) this["GUID"];
            }
        }

        public bool Valid // Переделываем объект-сессию.
        {
            get
            {
                IAgent a=Agent;
                return a.Role != RoleEnum.Invalid &&
                    a.Role != RoleEnum.Unknown;
            }
        }

        public IAgent Agent // Agent in the session
        {
            get
            {
                object o = null;
                bool s = TryGetValue("agent", out o);
                if (s) return (IAgent) o;
                return createAnonymousAgent();
            }
            set
            {
                this["agent"] = value;
            }
        }

        private IAgent createAnonymousAgent()
        {
            MyEntityContext ctx = Application.Context;
            if (GUID == null)
            {
                GUID = ImportFromAtlcomru.GetGUID();
            }
            else
            {
                // Try load from database
                IAgent agent = ctx.Agents.Where(x => x.GUID == GUID).FirstOrDefault();
                if (agent != null) Agent = agent;
            }

            IAgent anonym = ctx.Agents.Create();
            anonym.GUID = GUID;
            anonym.Role = RoleEnum.Unknown;
            this.Agent = anonym;
            ctx.Add(this.Agent);
            ctx.SaveChanges();

            return this.Agent;
        }

        public SessionModel Prev = null;

        public SessionModel Invalidate()
            // Метод отменяет сессию зарегистрированного пользователя
            // и заменяет ее анонимной, причем предыдущей, если она записалась,
            // или новой анонимной, если нет.
        {
            SessionModel prev = Prev;
            if (Valid) // Была валидная сессия?
            {
                WebModule.activeSessions.Remove(GUID); // Remove self from active sessions.

                if (prev!=null) return prev;

                this.GUID=null;
                createAnonymousAgent();
                return this;
            }
            else return this; // Сессия уже была анонимная;
        }
    }
}
