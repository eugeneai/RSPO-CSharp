using System;
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
}
