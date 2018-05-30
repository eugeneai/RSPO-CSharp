using System;
using System.Collections.Generic;
using BrightstarDB.EntityFramework;

namespace RSPO
{
	[Entity]
	public interface IObjectClass
	{
		IObject Object { get; set; }
		// В принципе кластер зависит от желаний юзера.
		// IAgent Agent { get; set; }
		int Cluster { get; set; } // Cluster number
	}

    [Entity]
    public interface IClassName
    {
        int Cluster { get; set; }
        string Name { get; set; }
    }

	[Entity]
	public interface ILikes // An IAgent likes an IObject
	{
		IAgent Agent { get; set; }
		IObject Object { get; set; }
		double Value { get; set; }
		OriginatingEnum Quality { get; set; } // Качество величины
	}

	public enum OriginatingEnum
	{
		Measured,  // Изерено
		Evaluated, // Вычислено
		Set // Установлено вручную
	}
}
