using System;
using System.Collections.Generic;

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
                object o;
                try
                {
                    o = this["user"];
                }
                catch (System.Collections.Generic.KeyNotFoundException)
                {
                    return false;
                }

                if (o == null) return false;
                IAgent a = (IAgent) o;
                return a.Role!=RoleEnum.Invalid && a.Role != RoleEnum.Unknown;
            }
        }


    }
}
