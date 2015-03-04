using OxyPlot;
using OxyPlot.Wpf;
using OxyPlot.Series;
using ScatterSeries = OxyPlot.Series.ScatterSeries;
using LineSeries = OxyPlot.Series.LineSeries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace WpfApplication1
{
    class MainViewModel
    {

        #region vars

        public string Title { get; private set; }

        public PlotModel MyModel { get; private set; }

        public static List<Tuple<int, Point>> dataPoints = new List<Tuple<int, Point>>();

        public static List<List<Tuple<int, Point>>> clusters = new List<List<Tuple<int, Point>>>();

        public static List<Tuple<int, Point>> centers = new List<Tuple<int, Point>>();

        public static List<List<Tuple<int, Point>>> centerClusters = new List<List<Tuple<int, Point>>>();

        public string path = "C:\\Users\\taylor\\Desktop\\DataMining\\";

        public static int testCount = 200;

        public static int pointIndex = 0;

        #endregion

        public MainViewModel()
        {
            
            this.MyModel = new PlotModel { Title = "Example" };


            var lines = File.ReadAllLines(path + "c2.txt").ToList();

            transformData(lines);

            formClusters();

            //problem1();
            //problem2aa();
            //problem2ab();
            problem2ad();
            //problem2b(2);
            //plot();
            
        }

        #region problem1

        public void problem1()
        {
            while (clusters.Count != 4)
            {
                double minDistance = double.MaxValue;
                List<Tuple<int, Point>> minCluster1 = new List<Tuple<int, Point>>();
                List<Tuple<int, Point>> minCluster2 = new List<Tuple<int, Point>>();

                foreach (var cluster1 in clusters)
                {
                    foreach (var cluster2 in clusters)
                    {
                        if (cluster1 == cluster2) continue;

                        //double currentDistance = minSetDistance(cluster1, cluster2);
                        //double currentDistance = maxSetDistance(cluster1, cluster2);
                        double currentDistance = avgSetDistance(cluster1, cluster2);
                        if (currentDistance < minDistance)
                        {
                            minDistance = currentDistance;
                            minCluster1 = cluster1;
                            minCluster2 = cluster2;
                        }
                    }
                }


                mergeClusters(minCluster1, minCluster2);

                plotClusters();
            }
        }

        #endregion

        #region problem2

        #region problem2aa

        public void problem2aa()
        {
            centers.Add((from cluster in clusters where cluster.First().Item1 == 1 select cluster.First()).First());

            while (centers.Count < 3)
            {
                double max = 0;
                int maxIndex = -1;

                foreach (var cluster in clusters)
                {
                    double min = double.MaxValue;

                    foreach (var center in centers)
                    {
                        double currentDistance = distance(cluster.First().Item2, center.Item2);
                        if (currentDistance < min)
                        {
                            min = currentDistance;
                        }
                    }

                    if (min > max)
                    {
                        max = min;
                        maxIndex = cluster.First().Item1;
                    }
                }

                centers.Add((from cluster in clusters where cluster.First().Item1 == maxIndex select cluster.First()).First());
            }

        }

        #endregion

        #region problem2ab

        public List<double> problem2ab()
        {
            foreach (var center in centers)
            {
                centerClusters.Add(new List<Tuple<int, Point>>() { centers[centers.IndexOf(center)] });
            }

            foreach(var point in clusters)
            {
                double min = double.MaxValue;
                Tuple<int, Point> minCenter = null;

                foreach(var center in centers)
                {
                    double currentDistance = distance(point.First().Item2, center.Item2);
                    if(currentDistance < min)
                    {
                        min = currentDistance;
                        minCenter = center;
                    }
                }

                // if point is equal to a center, ignore
                if ((from cluster in centerClusters from p in cluster where p == point.First() select p).Any())
                {
                    continue; 
                }

                (from cluster in centerClusters where cluster.First() == minCenter select cluster).First().Add(point.First());

            }

            List<double> maximums = new List<double>();
            List<double> means = new List<double>();
            foreach(var cluster in centerClusters)
            {
                double max = 0;
                List<double> distances = new List<double>();
                foreach(var point in cluster)
                {
                    double currentDistance = distance(cluster.First().Item2, point.Item2);
                    distances.Add(Math.Pow(currentDistance, 2));
                    if (currentDistance > max)
                    {
                        max = currentDistance;
                    }

                }

                maximums.Add(max);
                means.Add(distances.Average());
            }

            return means;
            //plotCenterClusters();

        }

        #endregion

        #region problem2ac

        public void problem2ac()
        {
            // add the first center
            centers.Add((from cluster in clusters where cluster.First().Item1 == 1 select cluster.First()).First());

            // need to find 3 centers
            while (centers.Count < 3)
            {
                List<Tuple<int,double>> distances = new List<Tuple<int,double>>();
                // for each point
                foreach (var cluster in clusters)
                {
                    if (cluster.First().Item1 == 1) continue;
                    double min = double.MaxValue;
                    Tuple<int, Point> minCenter = null;

                    // find the closest center
                    foreach (var center in centers)
                    {
                        double currentDistance = distance(cluster.First().Item2, center.Item2);
                        if (currentDistance < min)
                        {
                            min = currentDistance;
                            minCenter = center;
                        }
                    }

                    // add the squared distance to our closest center
                    distances.Add(new Tuple<int,double>(cluster.First().Item1, Math.Pow(min, 2)));
                }

                // probabilistically choose the next center
                double sumOfWeights = (from point in distances select point.Item2).Sum();
                Random random = new Random();
                // get an index somewhere into the space of our total weights
                double index = random.NextDouble() * sumOfWeights;
                // sort them, dont know if this is necessary
                distances = distances.OrderBy(x => x.Item2).ToList();

                double count = 0;
                Tuple<int, double> chosenPoint = null;
                
                foreach(var point in distances)
                {
                    if((count+=point.Item2) > index)
                    {
                        chosenPoint = point;
                        break;
                    }
                }

                // add our center
                centers.Add((from cluster in clusters where cluster.First().Item1 == chosenPoint.Item1 select cluster).First().First());

            }

        }

        #endregion

        #region problem2ab

        public void problem2ad()
        {
            List<List<List<Tuple<int, Point>>>> pointResults = new List<List<List<Tuple<int, Point>>>>();
            List<List<List<Tuple<int, Point>>>> pointResultsGonzalez = new List<List<List<Tuple<int, Point>>>>();
            List<List<double>> meansList = new List<List<double>>();

            foreach (var i in Enumerable.Range(0, testCount))
            {
                problem2aa();
                problem2ab();
                pointResultsGonzalez.Add(new List<List<Tuple<int, Point>>>(centerClusters));
                centerClusters.Clear();
                centers.Clear();

                problem2ac();
                //meansList.Add(problem2ab());
                meansList.Add(problem2b(3));
                pointResults.Add(new List<List<Tuple<int, Point>>>(centerClusters));
                centerClusters.Clear();
                centers.Clear();
            }

            int similarityCount = (from i in Enumerable.Range(0, testCount) from result in pointResultsGonzalez[i] where pointResults[i].Contains(result) select i).Count();

            LineSeries series = new LineSeries();

            foreach (var i in Enumerable.Range(1, testCount + 1))
            {
                double threshhold = (double)i / (double)testCount;
                var result = (from mean in meansList where ((mean.Sum() / meansList.OrderBy(x => x.Sum()).Last().Sum()) <= threshhold) select mean.Sum()).Count() / (double)testCount;
                series.Points.Add(new DataPoint((double)i, result));
            }

            this.MyModel.Title = "similarity to gonzalez = " + Math.Round(((double)similarityCount / (double)testCount), 2);

            this.MyModel.Series.Add(series);
        }

        #endregion

        #endregion

        #region problem2b

        // lloyds
        public List<double> problem2b(int input)
        {
            pointIndex = clusters.Count + 1;
            if (input == 1)
            {
                // add initial centers
                centers.Add(clusters[0].First());
                centers.Add(clusters[1].First());
                centers.Add(clusters[2].First());
            }
            else if(input == 2)
            {
                problem2aa();
            }

            // group the initial clusters
            var means = problem2ab();

            //plotCenterClusters();

            // escape bool
            bool finished = false;

            // add the centers again since the first set will be ignored and cleared later
            foreach (var i in Enumerable.Range(0, centers.Count))
            {
                centerClusters[i].Add(centers[i]);
            }

            while(!finished)
            {
                centers.Clear();
                if(calculateCenters())
                {
                    break;
                }

                // is this necessary? dont think so.
                foreach(var cluster in centerClusters)
                {
                    cluster.RemoveAt(0);
                }

                centerClusters.Clear();
                
                means = problem2ab();

                //plotCenterClusters();
                
            }

            //this.MyModel.Title = "means = " + means.Sum();
            return means;
            
        }

        public bool calculateCenters()
        {
            List<Tuple<int, Point>> oldCenters = new List<Tuple<int, Point>>();
            List<Tuple<int, Point>> newCenters = new List<Tuple<int, Point>>();
            foreach(var cluster in centerClusters)
            {
                oldCenters.Add(new Tuple<int, Point>(pointIndex++, new Point(cluster.First().Item2.X, cluster.First().Item2.Y)));

                if(cluster.Count == 2)
                {
                    newCenters.Add(new Tuple<int, Point>(pointIndex++, new Point(cluster.First().Item2.X, cluster.First().Item2.Y)));
                    continue;
                }

                double newX = (from point in cluster where cluster.First() != point select point.Item2.X).ToList().Sum() / (cluster.Count - 1.0);
                double newY = (from point in cluster where cluster.First() != point select point.Item2.Y).ToList().Sum() / (cluster.Count - 1.0);

                newCenters.Add(new Tuple<int,Point>(pointIndex++, new Point(newX, newY)));
            }

            bool toReturn = true;
            foreach(var i in Enumerable.Range(0,oldCenters.Count))
            {
                if (newCenters[i].Item2.X != oldCenters[i].Item2.X || newCenters[i].Item2.Y != oldCenters[i].Item2.Y)
                {
                    toReturn = false;
                    break;
                }
            }

            centers.AddRange(newCenters);

            return toReturn;
        }

        #endregion

        #region set distances

        public static double minSetDistance(List<Tuple<int, Point>> set1, List<Tuple<int, Point>> set2)
        {
            var distances = new List<Tuple<double, int, int>>();

            foreach (var point1 in set1)
            {
                foreach (var point2 in set2)
                {
                    double d = distance(point1.Item2, point2.Item2);
                    distances.Add(new Tuple<double, int, int>(d, point1.Item1, point2.Item1));
                }
            }

            var orderedDistances = distances.OrderBy(x => x.Item1).ToList();
            return orderedDistances.First().Item1;
        }

        public static double maxSetDistance(List<Tuple<int, Point>> set1, List<Tuple<int, Point>> set2)
        {
            var distances = new List<Tuple<double, int, int>>();

            foreach (var point1 in set1)
            {
                foreach (var point2 in set2)
                {
                    double d = distance(point1.Item2, point2.Item2);
                    distances.Add(new Tuple<double, int, int>(d, point1.Item1, point2.Item1));
                }
            }

            var orderedDistances = distances.OrderBy(x => x.Item1).ToList();
            return orderedDistances.Last().Item1;
        }

        public static double avgSetDistance(List<Tuple<int, Point>> set1, List<Tuple<int, Point>> set2)
        {
            var distances = new List<double>();

            foreach (var point1 in set1)
            {
                foreach (var point2 in set2)
                {
                    double d = distance(point1.Item2, point2.Item2);
                    distances.Add(d);
                }
            }

            return distances.Average();

        }

        #endregion

        #region helpers

        public static void mergeClusters(List<Tuple<int, Point>> set1, List<Tuple<int, Point>> set2)
        {
            clusters.Remove(set1);
            clusters.Remove(set2);

            set1.AddRange(set2);

            clusters.Add(set1);

        }

        public static void formClusters()
        {
            foreach(var point in dataPoints)
            {
                var list = new List<Tuple<int, Point>>();
                list.Add(point);
                clusters.Add(list);
            }
        }

        

        public static double distance(Point start, Point end)
        {
            return Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
        }

        public static void transformData(List<string> lines)
        {
            foreach(var line in lines)
            {
                var splits = line.Split('\t');
                dataPoints.Add(new Tuple<int, Point>(Int32.Parse(splits[0]), new Point(double.Parse(splits[1]), double.Parse(splits[2]))));
            }
        }

        public void plot()
        {
            ScatterSeries Points = new ScatterSeries();
            foreach(var point in dataPoints)
            {
                Points.Points.Add(new ScatterPoint(point.Item2.X, point.Item2.Y, 1.5));
            }
            this.MyModel.Series.Add(Points);
        }

        public void plotClusters()
        {

            this.MyModel.Series.Clear();
            
            foreach(var dataPoints in clusters)
            {
                ScatterSeries Points = new ScatterSeries();
                Random r = new Random();
                var value = r.Next(100, 1000);
                foreach (var point in dataPoints)
                {
                    Points.Points.Add(new ScatterPoint(point.Item2.X, point.Item2.Y, 3) { Value = 1});
                }
                this.MyModel.Series.Add(Points);    
            }
        }

        public void plotCenterClusters()
        {

            this.MyModel.Series.Clear();

            foreach (var dataPoints in centerClusters)
            {
                ScatterSeries Points = new ScatterSeries();
                
                
                foreach (var point in dataPoints)
                {
                    Points.Points.Add(new ScatterPoint(point.Item2.X, point.Item2.Y, 3) { Value = 1 });
                }
                this.MyModel.Series.Add(Points);
            }
        }

        #endregion
    }
}
