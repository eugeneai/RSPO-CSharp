using System;
using System.Collections.Generic;
using System.Linq;

namespace RSPO
{
	public class DataSet<T> // Класс абстратных анализаторов
	{
        // protected

		public DataSet()
        {
            input = new List<T>();
        }

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

        public int Count
        {
            get
            {
                return input.Count;
            }
        }

        protected List<T> input = null;
	}

    public class Analyzer<T> : DataSet<T>
    {
        public Analyzer() : base() {}
        public virtual bool Process() { return false; }

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
            double[,] distances = new double[Count, Count]; // It's matrix!
                //
                // пример.
                // = new double[,]{{0,3,1},{3,0,3},{1,3,0}};

            prepareDistMatrix(distances);

            // Капец. Привык, что все динамично.

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

        protected void prepareDistMatrix(double[,] m)
        {
            foreach (var o1 in input.Select((v, i) => new {v,i})) // Верхний объект
            {
                foreach(var o2 in input.Select((v, i) => new {v,i})) // Левый объект
                {
                    double v = compare(o1.v,o2.v);
                    m[o1.i, o2.i] = v;
                }
            }
        }

        protected virtual double compare(T v1, T v2)
        {
            throw new NotImplementedException(); // Сейчас я не знаю, что есть тип T.
        }
    }

    // TODO: Реализовать анализатор рекоммендательной системы
    // Weighted Slope One: http://www.cnblogs.com/kuber/articles/SlopeOne_CSharp.html
    // Я пишу как художник.

    public class WeightedSlopeOne<T1, T2> : Analyzer<T1>
    {
        public WeightedSlopeOne() : base() {}

        public override bool Process()
        {
            // TODO: Тело Метода.
            return true;
        }

        public DataSet<T2> Choices = new DataSet<T2>();
    }
}
