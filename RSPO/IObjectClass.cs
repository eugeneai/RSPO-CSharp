﻿using System;
using System.Collections.Generic;

namespace RSPO
{
	public interface IObjectClass
	{
		IObject Object { get; set; }
		// В принципе кластер зависит от желаний юзера.
		// IAgent Agent { get; set; } 
		int Cluster { get; set; } // Cluster number
	}

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
		Set // Устанвлено вручную
	}
}