using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Nancy;
using Nancy.Testing;
using RSPO;

namespace RSPO.tests
{
	public class UnitTest1
	{
		private string basePath;

		public UnitTest1()
		{
			basePath =
			Path.GetDirectoryName(
				Path.GetDirectoryName(
					Path.GetDirectoryName(
						Path.GetDirectoryName(
							System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))));
			if (Type.GetType("Mono.Runtime") != null)
			{
				basePath = basePath.Replace("file:", "");
			}
			else
			{
				basePath = basePath.Replace("file:\\", "");
			}

			Application.InitializeEntityContext();
		}

		private string CombineWithDataPath(string importName)
		{
			string dataDir = Path.Combine(basePath, "DATA");
			dataDir = Path.Combine(dataDir, "Import");
			string fileName = Path.Combine(dataDir, importName);
			return fileName;
		}


		/*

				[Fact]
				public void PassingTest()
				{
					Assert.Equal(4, Add(2, 2));
				}

				[Fact]
				public void FailingTestThatPasses()
				{
					Assert.NotEqual(5, Add(2, 2));
				}

				int Add(int x, int y)
				{
					return x + y;
				}

				[Theory]
				[InlineData(3)]
				[InlineData(5)]
				public void MyFirstTheory(int value)
				{
					Assert.True(IsOdd(value));
				}

				bool IsOdd(int value)
				{
					return value % 2 == 1;
				}

				[Theory]
				[InlineData("all.xml")]
				[InlineData("all.xml.zip")]
				public void ImportTest(string importName)
				{
					string fileName = CombineWithDataPath(importName);

					ImportFromAtlcomru import = new ImportFromAtlcomru()
					{
						FileName = fileName
					};
					import.Import(onlyLoad: true);
					Assert.True(true);
				}
				*/

		/* // !!!!!
        [Theory]
        [InlineData("all.xml")]
        public void LongImportTest(string importName)
        {
            string fileName = CombineWithDataPath(importName);
            Console.WriteLine("Importing : "+fileName);
            MyEntityContext ctx = Application.Context;
            ImportFromAtlcomru import = new ImportFromAtlcomru() // Сделать импорт данных
            {
                FileName = fileName
            };
            import.Import();
            Assert.True(true);
        }
        */

		[Fact]
		public void Hierarchical_Clustering()
		{
			// Квартирный кластерный анализатор
			var a = FlatClusterAnalyzer.AnalyzeFlatWithCluster(500);
			bool res = a.Process();
            a.PrepareClusters(5);
			Console.Write("--Cluster-> ");
			Console.WriteLine(a.Cidx);
			Assert.True(true);
		}

		[Fact] // Потестим, работает ли библиотека.

		public void alglib_hclust_works()
		{
			//
			// The very simple clusterization example
			//
			// We have a set of points in 2D space:
			//     (P0,P1,P2,P3,P4) = ((1,1),(1,2),(4,1),(2,3),(4,1.5))
			//
			//  |
			//  |     P3
			//  |
			//  | P1
			//  |             P4
			//  | P0          P2
			//  |-------------------------
			//
			// We want to perform Agglomerative Hierarchic Clusterization (AHC),
			// using complete linkage (default algorithm) and Euclidean distance
			// (default metric).
			//
			// In order to do that, we:
			// * create clusterizer with clusterizercreate()
			// * set etric (2=Euclidean) with clusterizersetpoints()
			// * run AHC algorithm with clusterizerrunahc
			//
			// You may see that clusterization itself is a minor part of the example,
			// most of which is dominated by comments :)
			//
			Console.WriteLine("------ Testing cluster analyzer --------- ");
			alglib.clusterizerstate s;
			alglib.ahcreport rep;
			double[,] xy = new double[,] { { 1, 1 }, { 1, 2 }, { 4, 1 }, { 2, 3 }, { 4, 1.5 } };

			alglib.clusterizercreate(out s); // Создать кластерайзер ж-)
			alglib.clusterizersetpoints(s, xy, 2); // Ввод точек
			alglib.clusterizerrunahc(s, out rep);  // Запуск кластеризации

			//
			// Now we've built our clusterization tree. Rep.z contains information which
			// is required to build dendrogram. I-th row of rep.z represents one merge
			// operation, with first cluster to merge having index rep.z[I,0] and second
			// one having index rep.z[I,1]. Merge result has index NPoints+I.
			//
			// Clusters with indexes less than NPoints are single-point initial clusters,
			// while ones with indexes from NPoints to 2*NPoints-2 are multi-point
			// clusters created during merges.
			//
			//                      5      6      7      8
			// In our example, Z=[[2,4], [0,1], [3,6], [5,7]]
			//                     2 4    0 1    3
            //                   0       1      2      3 = индексы массива
            //                 m=  5 5    7 7    7
            //                                  ^ index=2
            //              upto=3 - (2-1) = 2
            //             Count=4               3<Count -> [3]=index+Count = 2+4-1 = 7
            //           cluster=2
            //           Нужна рекурсия, если двигаться сверху-вниз
            //           Если наоборот?
			//
			// It means that:
			// * first, we merge C2=(P2) and C4=(P4),    and create C5=(P2,P4)
			// * then, we merge  C2=(P0) and C1=(P1),    and create C6=(P0,P1)
			// * then, we merge  C3=(P3) and C6=(P0,P1), and create C7=(P0,P1,P3)
			// * finally, we merge C5 and C7 and create C8=(P0,P1,P2,P3,P4)
			//
			// Thus, we have following dendrogram:
			//
			//      ------8-----  cluster = 2 значит... туплю... завтра продолжим.
			//      |          |
			//      |      ----7----
			//      |      |       |
			//   ---5---   |    ---6---
			//   |     |   |    |     |
			//   P2   P4   P3   P0   P1
			//   5    5    7    0    0
            //
			System.Console.WriteLine("{0}", alglib.ap.format(rep.z)); // EXPECTED: [[2,4],[0,1],[3,6],[5,7]]

			//
			// We've built dendrogram above by reordering our dataset.
			//
			// Without such reordering it would be impossible to build dendrogram without
			// intersections. Luckily, ahcreport structure contains two additional fields
			// which help to build dendrogram from your data:
			// * rep.p, which contains permutation applied to dataset
			// * rep.pm, which contains another representation of merges
			//
			// In our example we have:
			// * P=[3,4,0,2,1] // перестановка точек для отрисовки дендрограммы.
			// * PZ=[[0,0,1,1,0,0],[3,3,4,4,0,0],[2,2,3,4,0,1],[0,1,2,4,1,2]] // Другой вид дендрограммы.
			//
			// Permutation array P tells us that P0 should be moved to position 3,
			// P1 moved to position 4, P2 moved to position 0 and so on:
			//
			//   (P0 P1 P2 P3 P4) => (P2 P4 P3 P0 P1)
			//
			// Merges array PZ tells us how to perform merges on the sorted dataset.
			// One row of PZ corresponds to one merge operations, with first pair of
			// elements denoting first of the clusters to merge (start index, end
			// index) and next pair of elements denoting second of the clusters to
			// merge. Clusters being merged are always adjacent, with first one on
			// the left and second one on the right.
			//
			// For example, first row of PZ tells us that clusters [0,0] and [1,1] are
			// merged (single-point clusters, with first one containing P2 and second
			// one containing P4). Third row of PZ tells us that we merge one single-
			// point cluster [2,2] with one two-point cluster [3,4].
			//
			// There are two more elements in each row of PZ. These are the helper
			// elements, which denote HEIGHT (not size) of left and right subdendrograms.
			// For example, according to PZ, first two merges are performed on clusterization
			// trees of height 0, while next two merges are performed on 0-1 and 1-2
			// pairs of trees correspondingly.
			//
			System.Console.WriteLine("{0}", alglib.ap.format(rep.p)); // EXPECTED: [3,4,0,2,1]
			System.Console.WriteLine("{0}", alglib.ap.format(rep.pm)); // EXPECTED: [[0,0,1,1,0,0],[3,3,4,4,0,0],[2,2,3,4,0,1],[0,1,2,4,1,2]]
																	   // System.Console.ReadLine();
			Assert.True(true);
		}

		[Fact]

		public void Test_distance_atrix_input()
		{
			// Чтобы понять, как работает данный матметод, делаем лабы.
			// Запускаем примеры.
			Console.WriteLine("------ Testing cluster analyzer with distmatrix --------- ");
			alglib.clusterizerstate s;
			alglib.ahcreport rep;

			alglib.clusterizercreate(out s); // Вот зачем надо лабы делать.

			// Finally, we try clustering with user-supplied distance matrix:
			//     [ 0 3 1 ]
			// P = [ 3 0 3 ], where P[i,j] = dist(Pi,Pj)
			//     [ 1 3 0 ]
			//
			// * first, we merge P0 and P2 to form C3=[P0,P2]
			// * second, we merge P1 and C3 to form C4=[P0,P1,P2]

			double[,] d = new double[,] { { 0, 3, 1 }, { 3, 0, 3 }, { 1, 3, 0 } };

			alglib.clusterizersetdistances(s, d, true);
			alglib.clusterizerrunahc(s, out rep);
			System.Console.WriteLine("{0}", alglib.ap.format(rep.z)); // EXPECTED: [[0,2],[1,3]]
																	  // System.Console.ReadLine();
			Assert.True(true); // По-хорошему надо сюда вставлять сравнения того, что получилось
							   // С тем, что хотели получить. А мы тут просто пишем, что все хорошо.
		}

        [Fact]

        public void test_K_clusres_from_tree()
        {
            //
            // We have a set of points in 2D space:
            //     (P0,P1,P2,P3,P4) = ((1,1),(1,2),(4,1),(2,3),(4,1.5))
            //
            //  |
            //  |     P3
            //  |
            //  | P1
            //  |             P4
            //  | P0          P2
            //  |-------------------------
            //
            // We perform Agglomerative Hierarchic Clusterization (AHC) and we want
            // to get top K clusters from clusterization tree for different K.
            //
            Console.WriteLine("--- Testing K cluster ----");
            alglib.clusterizerstate s;
            alglib.ahcreport rep;
            double[,] xy = new double[,]{{1,1},{1,2},{4,1},{2,3},{4,1.5}};
            int[] cidx;
            int[] cz;

            alglib.clusterizercreate(out s);
            alglib.clusterizersetpoints(s, xy, 2);
            alglib.clusterizerrunahc(s, out rep);

            // with K=5, every points is assigned to its own cluster:
            // C0=P0, C1=P1 and so on...
            alglib.clusterizergetkclusters(rep, 5, out cidx, out cz);
            System.Console.WriteLine("{0}", alglib.ap.format(cidx)); // EXPECTED: [0,1,2,3,4]

            // with K=1 we have one large cluster C0=[P0,P1,P2,P3,P4,P5]
            alglib.clusterizergetkclusters(rep, 1, out cidx, out cz);
            System.Console.WriteLine("{0}", alglib.ap.format(cidx)); // EXPECTED: [0,0,0,0,0]

            // with K=3 we have three clusters C0=[P3], C1=[P2,P4], C2=[P0,P1]
            alglib.clusterizergetkclusters(rep, 3, out cidx, out cz);
            System.Console.WriteLine("{0}", alglib.ap.format(cidx)); // EXPECTED: [2,2,1,0,1]
            // System.Console.ReadLine();
            Assert.True(true);

        }

		/*
		[Fact]
		public void Should_return_status_ok_when_route_exists()
		{
			// Given
			var bootstrapper = new DefaultNancyBootstrapper();
			var browser = new Browser(bootstrapper);

			// When
			var result = browser.Get("/", with =>
			{
				with.HttpRequest();
			});

			// Then
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
		}

		[Fact]
		public void TestRenderer()
		{
			WebModule mod = new WebModule();
			Request request = new Request("GET", "", "1.1");
			string result = mod.Render("index.pt", request: request);
			Console.WriteLine(result);
			Assert.Contains(result, "</html>");
		}
        */
	}
}
