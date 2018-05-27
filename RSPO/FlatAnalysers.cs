using System;
using System.Linq;

namespace RSPO
{
    public class FlatClusterAnalyser : ClusterAnalyzer<IObject>
    {
        public FlatClusterAnalyser() : base() {}

        protected double diff(double a, double b)
        {
            return Math.Abs(a-b)/(a+b);
        }

        protected override double compare(IObject o1, IObject o2)
        {
            // Надо сравнить два оюъекта по полям.
            double s = 0.0; // Чем больше s, тем сильнее различаются объекты.
            double q = 0.0; // Сумма максимумов важностей
            double v ;

            // Используем манхэттеновскую меру. |x1-x2| + |y1-y2| + ...

            // + нам надо что-то вроде важности поля.
            // (район города) - это очень важно.
            v = 10;
            if (o1.Location != o2.Location)
            {
                s+=v;
            }
            q+=v; // s суммируется не всегда.

            // Address compare


            v = 25;
            s+=diff(o1.Price, o2.Price) * v;
            q+=v;

            v = 35;
            s+=diff(o1.Area, o2.Area) * v;
            q+=v;

            v = 100;
            bool r1 = o1.Rooms > 0, r2 = o2.Rooms > 0;
            bool ro1 = o1.RoomsOffered > 0, ro2 = o2.RoomsOffered > 0;
            if (r1 != r2)
            {
                s+=1 * v; // Разные объекты
            }
            else // Объекты похожи в смысле измерения комнат
            {
                if (! r1) // Оба объекта без комнат.
                {
                    // Ничего не добавляем к s
                }
                else // Объекты с комнатами
                {
                    double v1 = 100;
                    if (ro1 == ro2)
                    {
                        if (ro1) // Сдают комнаты.
                        {
                            s+=diff(o1.RoomsOffered, o2.RoomsOffered) * v1;
                        }
                        // Иначе они одинаковы относительно доли комнат.
                    }
                    else
                    {
                        s+=v1; // Один сдает комнаты, другой все сразу.
                    }

                    q+=v1;

                    s+=diff(o1.Rooms, o2.Rooms) * v;
                }
            }
            q+=v; // Суммируем всегда

            v = 10; // Размер здания не сильно важен.
            bool f1 = o1.FloorTotal > 0, f2 = o2.FloorTotal > 0;
            if (f1 == f2)
            {
                if (f1) // Здания с этажами
                {
                    s+=diff(o1.FloorTotal, o2.FloorTotal) * v;
                }
                // else Здания без этажей
            }
            else
            {
                s+=v; // Один с этажами, др. нет.
            }
            q+=v;

            v=30; // Этажы одни и те же.
            f1 = o1.Floor > 0;
            f2 = o2.Floor > 0;
            if (f1 == f2)
            {
                if (f1)
                {
                    s+=diff(o1.Floor, o2.Floor)*v;
                }
            }
            else
            {
                s+=v;
            }
            q+=v;

            v=30;
            if (o1.BuildingType == o2.BuildingType) s+=v;
            q+=v;

            v=30;
            if (o1.BuildingSeries == o2.BuildingSeries) s+=v;
            q+=v;

            v=30;
            if (o1.PropertyType == o2.PropertyType) s+=v;
            q+=v;

            v=100;
            if (o1.Category == o2.Category) s+=v;
            q+=v;

            // v=20; // Один и тот же агент продает
            // if (o1.Agents == o2.Agents) s+=v;
            // q+=v;

            double rate = s/q;

            if (rate<0.0 || rate>1.0)
                throw new Exception(string.Format("wrong rate {0}", rate));

            return rate;
        }

        public  FlatClusterAnalyser AnalyzeFlatWithCluster()
        {
            FlatClusterAnalyser fca = new FlatClusterAnalyser();
            MyEntityContext ctx = Application.Context;

            foreach (IObject o in ctx.Agents)
            {
                fca.Add(o);
            }

            if (! fca.Process())
                throw new ProcessingException("cannot process data");

            return fca;
        }

        // Сохранить результат в базе данных

        // В винде сейчас перегенерю БД.
    }

    public class ProcessingException : Exception
    {
        public ProcessingException(string msg) : base (msg) { }
    }
}
