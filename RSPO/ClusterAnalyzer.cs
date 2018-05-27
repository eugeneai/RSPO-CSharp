using System;
using System.Collections.Generic;

namespace RSPO
{
	public abstract class Analyzer<T> // Класс абстратных анализаторов
	{
        // protected

		public Analyzer()
        {
            input = new List<T>();
        }

        public virtual bool Process() { return false; }

        public void Add(T row) // Вообще могут быть несколько таблиц данных
            // Т.е. матрица квартир.
            // Матрица пользовательского выбора.
        {
            input.Add(row);
        }

        public void Clear()
        {
            input.Clear();
        }

        protected List<T> input = null;
	}

	public class ClusterAnalyzer<T> : Analyzer<T> // Класс кластерных анализаторов
	{
		public ClusterAnalyzer() : base()
        {
            // Иницифлизвция кластеризатора.....
            alglib.clusterizercreate(out state);
        }

		public override bool Process()
		{
            double[,] distances // Как создать матрицу динамически???
                // Пока стырим пример.
                = new double[,]{{0,3,1},{3,0,3},{1,3,0}};

            alglib.clusterizersetdistances(state, distances, true);
            alglib.clusterizerrunahc(state, out report);
            return true;
		}

        protected alglib.clusterizerstate state; // Объект, выполняющий кластерный анализ
        protected alglib.ahcreport report;       // Результат кластеризации.

        public int[,] Z
        {
            get
            {
                return report.z;
            }
        }

	}

    // TODO: Реализовать анализатор рекоммендательной системы
    // Weighted Slope One: http://www.cnblogs.com/kuber/articles/SlopeOne_CSharp.html

    public class WeightedSlopeOne<T> : Analyzer<T>
    {
        public WeightedSlopeOne() : base() {}

        public override bool Process()
        {
            // TODO: Тело Метода.
            return true;
        }
    }
}
