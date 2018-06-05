using System;
using System.Collections.Generic;
using System.Linq;

namespace RSPO
{
    public class SlopeOne : Analyzer <ILikes>
    // Класс рассчитывает по квартирам в кластере (это фича будет)
    // вес квартиры, которую пользователь еще не смотрел.
    {
        public SlopeOne() : base() { }

        public List<int> Clusters; // Список кластеров, которые нас интересует

        public override bool Process ()
        {
            // Итак, У нас список объектов ILikes, хранящихся в input;
            // Надо рассчитать матрицу весов: сколько раз посещали квартирку.
            // Для этого надо матрицу создать n x n, где n - это количество
            // квартир в выборке элементов всех кластеров.
            //

            // Не, правильно я сделал. Иначе два раза запросы к БД делать.
            prepareInput();  // Загрузка данных лайков.

            calcMatrix();

            return true;
        }

        private double [,] m = null; // Матрица оценок.
        private double [,] d = null; // Differences
        private int []  v = null; // Sum of votes per object

        protected void calcMatrix()
        {
            m = new double[agents.Count,objects.Count]; // Все обнулено.

            foreach (var i in input.Select((v, i) => new {v,i}))
            {
                m[agents[i.v.Agent], objects[i.v.Object]] += i.v.Value;
                // https://ru.wikipedia.org/wiki/Slope_One#Коллаборативная_фильтрация_Slope_One_для_предметов_с_оценками
            }

            d = new double[objects.Count, objects.Count];
            v = new double[objects.Count];

            for (int row = 0; row<objects.Count; row++)
            {
                int col = row+1;
                while (col<objects.Count)
                {
                    double s = 0;
                    int num = 0;
                    for (int ag=0; ag<agents.Count; ag++)
                    {
                        double a = m[ag,row], b=m[ag,col];
                        if (a>0 && b>0)
                        {
                           s+=a-b;
                           num++;
                        }
                    }
                    s/=num;
                    d[row,col] = s;
                    d[col,row] = -s;
                    col++;
                }

                int agnum = 0;
                for (int ag=0; ag<agents.Count; ag++ )
                {
                    if (m[ag, row]>0) agnum++;
                }
                v[row]=agnum;
            }
        }

        public double? Estimate(IAgent agent, IObject obj)
        {
            if (m == null) throw new EstimationException("matrix is not constructed");
            int ia, io;
            if (!agents.TryGetValue(agent, out ia))
            {
                return null;
            }
            if (!objects.TryGetValue(obj, out io))
            {
                return null;
            }


            if (m[ia,io]>0)
            {
                // Оценка им самим уже есть, вернуть ее
                return m[ia,io];
            }

            double value = 0;
            int num = 0;

            for (int row=0; row<objects.Count; row++)
            {
                double a = d[io,row];
                if (! (a>0)) continue;
                double theirest = m[ia, row]; // Моя оценка квартиры, с кот. сравниваем
                if (! (theirest>0)) continue;
                theirest+=a; // Добавить среднюю оценку FIXME: Проверить знак.
                int cnt = v[row];
                num+=cnt;
                theirest*=cnt;
                value+=theirest;
            }

            if (num==0) return null; // Никто не голосовал за эту квартиру вообще

            return value/num;
        }

        protected void addLike(ILikes like)
        {
            agents.SetDefault(like.Agent, agents.Count);
            objects.SetDefault(like.Object, agents.Count);
            Add(like); // in input
        }


        protected void prepareInput(int? cluster = null)
        {
            MyEntityContext ctx = Application.Context;
            Console.WriteLine(" RS: importing data ");

            agents = new Dictionary<IAgent, int>();
            objects = new Dictionary<IObject, int>();

            if (cluster != null)
            {
                foreach (IObjectClass oc in ctx.ObjectClasss.Where(x=>x.Cluster == cluster).ToList())
                {
                    ILikes like = ctx.Likess.Where(x=>x.Object.GUID == oc.Object.GUID).FirstOrDefault();
                    // Нельзя написать проще x=>x.Object == oc.Object - Linq тупит.
                    addLike(like);
                }
            }
            else
            {
                foreach (int c in Clusters)
                {
                    prepareInput(c); // Да. Сие является рекурсией.
                }
            }
        }

        Dictionary<IAgent, int> agents; // Отображение агента на его индекс в матрице.
        Dictionary<IObject, int> objects; // Аналогично... для квартирке.
    }

    public class EstimationException:Exception
    {
        public EstimationException(string msg) : base(msg) { }
    }

}
