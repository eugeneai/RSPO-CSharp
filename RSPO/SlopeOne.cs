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

        public List<ILikes> Likes = null;

        public override bool Process ()
        {
            // Итак, У нас список объектов ILikes, хранящихся в input;
            // Надо рассчитать матрицу весов: сколько раз посещали квартирку.
            // Для этого надо матрицу создать n x n, где n - это количество
            // квартир в выборке элементов всех кластеров.
            //

            // Не, правильно я сделал. Иначе два раза запросы к БД делать.
            PrepareInput(likes:Likes);  // Загрузка данных лайков.

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
            v = new int[objects.Count];

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
            if (DEBUG)
            {
                Console.WriteLine("-------Indexes-----------");
                foreach (KeyValuePair<IAgent, int> kvp in agents)
                {
                    Console.WriteLine(string.Format("Key = {0}, Value = {1}", kvp.Key.Name, kvp.Value));
                }

                foreach (KeyValuePair<IObject, int> kvp in objects)
                {
                    Console.WriteLine(string.Format("Key = {0}, Value = {1}", kvp.Key.Name, kvp.Value));
                } // A bug found.

                Console.WriteLine("----- Source matix ------");
                int rowLength = m.GetLength(0);
                int colLength = m.GetLength(1);

                for (int i = 0; i < rowLength; i++)
                {
                    for (int j = 0; j < colLength; j++)
                    {
                        Console.Write(string.Format("{0} ", m[i, j]));
                    }
                    Console.WriteLine();
                }

                Console.WriteLine("----- distance matix ------");
                rowLength = d.GetLength(0);
                colLength = d.GetLength(1);

                for (int i = 0; i < rowLength; i++)
                {
                    for (int j = 0; j < colLength; j++)
                    {
                        Console.Write(string.Format("{0:f} ", d[i, j]));
                    }
                    Console.WriteLine();
                }

                Console.WriteLine("----- Sum values vector -------");
                for (int i=0; i<v.GetLength(0); i++)
                {
                    Console.Write(string.Format("{0} ", v[i]));
                }
                Console.WriteLine();
            }
        }

        public List<ILikes> Estimate(IAgent agent, int cluster)
            // Оценивает все квартиры в клатере cluster
            // для агента agent
        {
            List<ILikes> res = new List<ILikes>();
            MyEntityContext ctx = Application.Context;
            foreach (IObjectClass oc in ctx.ObjectClasss.Where(x=>x.Cluster == cluster).ToList())
            {
                ILikes like = ctx.Likess.Create();
                double? eval = Estimate(agent, oc.Object);
                if (eval !=null)
                {
                    like.Agent = agent;
                    like.Object=oc.Object;
                    like.Value = (double)eval;
                    like.Quality=OriginatingEnum.Evaluated;
                    res.Add(like);
                    // Эти лайки мы не записываем в БД.!!
                }
            }
            // Теперь надо упорядочить по убфывнию Value
            res.Sort(delegate(ILikes x, ILikes y)
                     {
                         if (x.Value==y.Value) return 0;
                         if (x.Value>y.Value) return 1;
                         return -1;
                     });
            return res;
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
            objects.SetDefault(like.Object, objects.Count);
            Add(like); // in input
        }


        protected void PrepareInput(int? cluster = null, List<ILikes> likes = null)
        {
            MyEntityContext ctx = Application.Context;
            Console.WriteLine(" RS: importing data ");

            agents = new Dictionary<IAgent, int>();
            objects = new Dictionary<IObject, int>();
            if(likes!=null)
            {
                foreach(ILikes like in likes)
                {
                    addLike(like);
                }
                return; // Этот вариант задуман для тестирования.
            }

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
                    PrepareInput(c); // Да. Сие является рекурсией.
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
