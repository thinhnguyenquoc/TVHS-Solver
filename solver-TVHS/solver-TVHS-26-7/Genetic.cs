using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace solver_TVHS_26_7
{
    public class Genetic
    {
        public MyStatictis Solve4(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, List<int[]> h)
        {
            MyCase myOriginalCase = input;
            double revenue = 0;
            MyStatictis result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulation2(myOriginalCase, initSize);
            List<Chromosome> hc = new List<Chromosome>();
            foreach (var i in h)
            {
                Chromosome c = new Chromosome();
                c.Choosen = i;
                c.CanBorn = true;
                hc.Add(c);
            }

            initPopulation = initPopulation.Concat(hc).ToList();

            List<EvaluatePopulation2> evaluations = new List<EvaluatePopulation2>();
            foreach (var i in initPopulation)
            {
                var eva = new EvaluatePopulation2();
                eva.resident = i;
                eva.revenue = Utility.CalculateRevenue(myOriginalCase, i.Choosen);
                evaluations.Add(eva);
            }
            result.Choosen = evaluations.OrderByDescending(x => x.revenue).FirstOrDefault().resident.Choosen;
            revenue = Utility.CalculateRevenue(myOriginalCase, result.Choosen);
            int count = 0; 
            for (var lop = 0; lop < noloop; lop++)
            {
                result.noGen++;
                var Parents = SelectParents(initPopulation, percentCrossOver);
                var Children = MakeChildren2(myOriginalCase, Parents);
                // add children to population
                initPopulation = initPopulation.Concat(Children).ToList();
                // resize of population
                evaluations = new List<EvaluatePopulation2>();
                foreach (var i in initPopulation)
                {
                    var eva = new EvaluatePopulation2();
                    eva.resident = i;
                    eva.revenue = Utility.CalculateRevenue(myOriginalCase, i.Choosen);
                    evaluations.Add(eva);
                }
                var theBest = evaluations.OrderByDescending(x => x.revenue).FirstOrDefault().resident.Choosen;
                double re = Utility.CalculateRevenue(myOriginalCase, theBest);
                if (re > revenue)
                {
                    revenue = re;
                    result.Choosen = theBest;
                    count = 0;
                }
                Debug.WriteLine("max:" + revenue);
                Debug.WriteLine("re" + re);
                count++;
                if (count > nochange)
                {
                    break;
                }
                initPopulation = evaluations.Where(x => x.resident.CanBorn == true).OrderByDescending(x => x.revenue).Take(populationSize).Select(x => x.resident).ToList();
            }
            watch.Stop();
            var elapsedH1 = watch.ElapsedMilliseconds;
            result.Elapsed = elapsedH1;
            result.Revenue = Utility.CalculateRevenue(myOriginalCase, result.Choosen);
            return result;
        }

        public MyStatictis Solve2(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop)
        {
            MyCase myOriginalCase = input;
            double revenue = 0;
            MyStatictis result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulation2(myOriginalCase, initSize);
            List<EvaluatePopulation2> evaluations = new List<EvaluatePopulation2>();
            foreach (var i in initPopulation)
            {                
                var eva = new EvaluatePopulation2();
                eva.resident = i;
                eva.revenue = Utility.CalculateRevenue(myOriginalCase, i.Choosen);
                evaluations.Add(eva);
            }
            result.Choosen = evaluations.OrderByDescending(x=>x.revenue).FirstOrDefault().resident.Choosen;
            revenue = Utility.CalculateRevenue(myOriginalCase, result.Choosen);
            int count = 0;
            for (var lop = 0; lop < noloop; lop++)
            {
                result.noGen++;
                var Parents = SelectParents(initPopulation, percentCrossOver);
                var Children = MakeChildren2(myOriginalCase, Parents);
                // add children to population
                initPopulation = initPopulation.Concat(Children).ToList();
                // resize of population
                evaluations = new List<EvaluatePopulation2>();
                foreach (var i in initPopulation)
                {
                    var eva = new EvaluatePopulation2();
                    eva.resident = i;
                    eva.revenue = Utility.CalculateRevenue(myOriginalCase, i.Choosen);
                    evaluations.Add(eva);
                }
                var theBest = evaluations.OrderByDescending(x => x.revenue).FirstOrDefault().resident.Choosen;
                double re = Utility.CalculateRevenue(myOriginalCase, theBest);
                if (re > revenue)
                {
                    revenue = re;
                    result.Choosen = theBest;
                    count = 0;
                }               
                count++;
                if (count > nochange)
                {
                    break;
                }
                initPopulation = evaluations.Where(x => x.resident.CanBorn == true).OrderByDescending(x => x.revenue).Take(populationSize).Select(x => x.resident).ToList();
            }
            watch.Stop();
            var elapsedH1 = watch.ElapsedMilliseconds;
            result.Elapsed = elapsedH1;
            result.Revenue = Utility.CalculateRevenue(myOriginalCase, result.Choosen);
            return result;
        }

        public List<Chromosome> MakeChildren2(MyCase myOriginalCase, List<Chromosome> parents)
        {
            List<Chromosome> result = new List<Chromosome>();
            for (int i = 0; i < parents.Count() - 1; i++)
            {
                int flag = -1;
                for (int j = i + 1; j < parents.Count(); j++)
                {
                    List<int[]> couple = new List<int[]>();
                    couple.Add(parents[i].Choosen);
                    couple.Add(parents[j].Choosen);
                    List<int[]> r = SingleCross(myOriginalCase, couple);
                    if (r.FirstOrDefault() != null)
                    {
                        Chromosome new1 = new Chromosome();
                        new1.Choosen = r.FirstOrDefault();
                        new1.CanBorn = true;
                        result.Add(new1);
                        flag = 1;
                    }
                    if (r.LastOrDefault() != null)
                    {
                        Chromosome new2 = new Chromosome();
                        new2.Choosen = r.LastOrDefault();
                        new2.CanBorn = true;
                        result.Add(new2);
                        flag = 1;
                    }
                }
                if (flag == -1)
                {
                    parents[i].CanBorn = false;
                }
            }
            return result;
        }


        public List<Chromosome> SelectParents(List<Chromosome> population, double percentCrossOver)
        {
            List<Chromosome> parents = new List<Chromosome>();
            Random rnd = new Random();
            List<int> saveIndex = new List<int>();
            while (saveIndex.Count() < population.Count() * percentCrossOver)
            {
                int r = rnd.Next(0, population.Count - 1);
                if (!saveIndex.Contains(r))
                {
                    parents.Add(population[r]);
                    saveIndex.Add(r);
                }
            }
            return parents;
        }


        public List<Chromosome> CreateInitPopulation2(MyCase myOriginalCase, int Size)
        {
            List<Chromosome> initPopulation = new List<Chromosome>();
            for (int l = 0; l < Size; l++)
            {
                MyCase myCase = Utility.Clone<MyCase>(myOriginalCase);
                int[] Choosen = new int[myCase.Times.Count];
                for (int j = 0; j < myCase.Times.Count; j++)
                {
                    Choosen[j] = -1;
                }
                bool change = false;
                #region assign program to frame
                while (true)
                {
                    var list1 = Utility.RandomProgramList(myCase.Programs.Where(x => x.MaxShowTime > 0).ToList());
                    foreach (var item in list1)
                    {
                        var gr = myCase.Groups.Where(x => x.Id == item.GroupId).FirstOrDefault();
                        #region group still has cota
                        if (gr.TotalTime >= item.Duration)
                        {
                            var FrameList = item.FrameList;
                            foreach (var frame in FrameList)
                            {
                                int startAv = Utility.GetFirstAvailableSlotInFrame(myCase, Choosen, frame);
                                #region find a empty space to assign
                                if (startAv != -1)
                                {
                                    if (Utility.CheckAssignableProgram(myCase, Choosen, startAv, item))
                                    {
                                        if (Utility.CheckTooClose(myCase, Choosen, startAv, item))
                                        {
                                            Utility.AssignProgramToSche(myCase, Choosen, startAv, item);
                                            change = true;
                                            break;
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                    #region if nothing changes, stop loop
                    if (change)
                        change = false;
                    else
                        break;
                    #endregion
                }
                #endregion
                #region try to assign programe between two frames
                change = false;
                // calculate the unoccupate of two continue frame
                while (true)
                {
                    Utility.UpdateUnOccupiedFrameTime(myCase, Choosen);
                    List<int> Unoccupate = new List<int>();
                    for (int i = 0; i < myCase.Frames.Count - 1; i++)
                    {
                        Unoccupate.Add(myCase.Frames[i].Unoccupate + myCase.Frames[i + 1].Unoccupate);
                    }
                    var list2 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
                    foreach (var pro in list2)
                    {
                        var gr = myCase.Groups.Where(x => x.Id == pro.GroupId).FirstOrDefault();
                        if (gr.TotalTime >= pro.Duration)
                        {
                            for (int i = 0; i < Unoccupate.Count; i++)
                            {
                                if (pro.Duration <= Unoccupate[i])
                                {
                                    //check allowed frames
                                    var FrameIdList = pro.FrameList.Select(x => x.Id).ToList();
                                    //add program to time frame
                                    if (FrameIdList.Contains(i))
                                    {
                                        ////check available slot                      
                                        int startAv = myCase.Frames[i].End - myCase.Frames[i].Unoccupate;
                                        if (startAv != myCase.Frames[i].End)
                                        {
                                            if (Utility.CheckTooClose(myCase, Choosen, startAv, pro))
                                            {
                                                int shift = pro.Duration - myCase.Frames[i].Unoccupate;
                                                // ShiftRight
                                                Utility.ShiftRight(myCase, Choosen, shift, i + 1, Unoccupate);
                                                Utility.AssignProgramToSche(myCase, Choosen, startAv, pro);
                                                change = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        if (change)
                            break;
                    }
                    if (change)
                        change = false;
                    else
                        break;
                }
                #endregion
                var chromosome = new Chromosome();
                chromosome.Choosen = Choosen;
                chromosome.CanBorn = true;
                initPopulation.Add(chromosome);
            }
            return initPopulation;
        }

        public MyStatictis Solve3(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, List<int[]> h)
        {
            MyCase myOriginalCase = input;
            double revenue = 0;
            MyStatictis result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulation(myOriginalCase, initSize);
            initPopulation = initPopulation.Concat(h).ToList();
            revenue = Utility.CalculateRevenue(myOriginalCase, EvaluateAndSelect(myOriginalCase, initPopulation, 1).FirstOrDefault());
            int count = 0;
            for (var lop = 0; lop < noloop; lop++)
            {
                result.noGen++;
                var Children = MakeChildren(myOriginalCase, initPopulation, percentCrossOver);
                // add children to population
                initPopulation = initPopulation.Concat(Children).ToList();
                // resize of population
                initPopulation = EvaluateAndSelect(myOriginalCase, initPopulation, populationSize);
                double re = Utility.CalculateRevenue(myOriginalCase, initPopulation.FirstOrDefault());
                if (re > revenue)
                {
                    revenue = re;
                    count = 0;
                }
                count++;
                if (count > nochange)
                {
                    break;
                }
            }
            watch.Stop();
            var elapsedH1 = watch.ElapsedMilliseconds;
            result.Choosen = EvaluateAndSelect(myOriginalCase, initPopulation, 1).FirstOrDefault();
            result.Elapsed = elapsedH1;
            result.Revenue = Utility.CalculateRevenue(myOriginalCase, result.Choosen);
            return result;
        }
        

        public MyStatictis Solve(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop)
        {
            MyCase myOriginalCase = input;
            double revenue = 0;
            MyStatictis result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulation(myOriginalCase, initSize);
           
            revenue = Utility.CalculateRevenue(myOriginalCase, EvaluateAndSelect(myOriginalCase, initPopulation, 1).FirstOrDefault());
            int count = 0;
            for (var lop = 0; lop < noloop; lop++)
            {
                result.noGen++;        
                var Children = MakeChildren(myOriginalCase, initPopulation, percentCrossOver);               
                // add children to population
                initPopulation = initPopulation.Concat(Children).ToList();
                // resize of population
                initPopulation = EvaluateAndSelect(myOriginalCase, initPopulation, populationSize);
                double re = Utility.CalculateRevenue(myOriginalCase, initPopulation.FirstOrDefault());
                if (re > revenue)
                {
                    revenue = re;
                    count = 0;
                }              
                count++;
                if (count > nochange)
                {
                    break;
                }
            }
            watch.Stop();
            var elapsedH1 = watch.ElapsedMilliseconds;
            result.Choosen = EvaluateAndSelect(myOriginalCase, initPopulation, 1).FirstOrDefault();
            result.Elapsed = elapsedH1;
            result.Revenue = Utility.CalculateRevenue(myOriginalCase, result.Choosen);
            return result;
        }

        public MyStatictis Solve5(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, string filename)
        {
            if (!File.Exists(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen5.txt"))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen5.txt"))
                {
                }
            }

                 
            MyCase myOriginalCase = input;
            double revenue = 0;
            MyStatictis result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulation(myOriginalCase, initSize);

            revenue = Utility.CalculateRevenue(myOriginalCase, EvaluateAndSelect(myOriginalCase, initPopulation, 1).FirstOrDefault());
            int count = 0;
            List<string> totalunbornParents = new List<string>();
            using (System.IO.StreamWriter file = File.AppendText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen5.txt"))
            {
                file.WriteLine(DateTime.Now.ToLongDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString());
                     
                for (var lop = 0; lop < noloop; lop++)
                {
                    result.noGen++;
                    var Parents = SelectParents5(myOriginalCase, initPopulation, percentCrossOver, totalunbornParents);
                    List<string> unbornParents = new List<string>();
                    var Children = MakeChildren5(myOriginalCase, Parents, out unbornParents);
                    // add children to population
                    initPopulation = initPopulation.Concat(Children).ToList();

                    foreach (var parents in unbornParents)
                    {
                        if (totalunbornParents.Contains(parents))
                            totalunbornParents.Add(parents);
                    }
                    initPopulation = EvaluateAndSelect2(myOriginalCase, initPopulation, populationSize, percentCrossOver);
                    double re = Utility.CalculateRevenue(myOriginalCase, initPopulation.FirstOrDefault());
                    if (re > revenue)
                    {
                        revenue = re;
                        count = 0;
                    }
                    count++;
                    Debug.WriteLine(lop + "\t" + revenue);
                    file.WriteLine(lop + "\t" + revenue);
                    if (count > nochange)
                    {
                        break;
                    }
                }
                file.WriteLine("");
            }
            watch.Stop();
            var elapsedH1 = watch.ElapsedMilliseconds;
            result.Choosen = EvaluateAndSelect(myOriginalCase, initPopulation, 1).FirstOrDefault();
            result.Elapsed = elapsedH1;
            result.Revenue = Utility.CalculateRevenue(myOriginalCase, result.Choosen);
            return result;
        }

        public List<List<int[]>> SelectParents5(MyCase myOriginalCase, List<int[]> population, double percentCrossOver, List<string> totalunbornParents)
        {
            List<EvaluatePopulation> eva = new List<EvaluatePopulation>();
            foreach (int[] pop in population as List<int[]>)
            {
                var e = new EvaluatePopulation();
                e.resident = pop;
                e.revenue = Utility.CalculateRevenue(myOriginalCase,pop);
                eva.Add(e);
            }
            eva = eva.OrderByDescending(x => x.revenue).ToList();
            double sum = 0;
            double total = eva.Sum(x => x.revenue);
            foreach (var e in eva)
            {
                e.min = sum;                
                sum += e.revenue / total;
                e.max = sum;
            }

            Random rnd = new Random();
            List<List<int[]>> parents = new List<List<int[]>>();
            while (parents.Count() < population.Count() * percentCrossOver/2)
            {
                double r = rnd.NextDouble();
                List<int[]> couple = new List<int[]>();
                var a = eva.Where(x => x.min < r && x.max >= r).FirstOrDefault();
                if (a != null)
                {
                    couple.Add(a.resident);
                }

                r = rnd.NextDouble();
                a = eva.Where(x => x.min < r && x.max >= r).FirstOrDefault();
                if (a != null)
                {
                    couple.Add(a.resident);
                }
                string couplestr = string.Join(", ", couple.First()) + ";" + string.Join(", ", couple.Last());
                if (!totalunbornParents.Contains(couplestr))
                    parents.Add(couple);
            }
            return parents;
        }

        public List<int[]> MakeChildren5(MyCase myOriginalCase, List<List<int[]>> parents, out List<string> unbornParents)
        {
            List<int[]> result = new List<int[]>();
            Random rnd = new Random();
            unbornParents = new List<string>();
            for (int i = 0; i < parents.Count(); i++)
            {                             
                List<int[]> r = SingleCross2(myOriginalCase, parents[i]);
                if (r.FirstOrDefault() != null)
                    result.Add(r.FirstOrDefault());
                if (r.LastOrDefault() != null)
                    result.Add(r.LastOrDefault());
                if (r.FirstOrDefault() == null && r.LastOrDefault() == null)
                {
                    unbornParents.Add(string.Join(", ", parents[i].First()) + ";" + string.Join(", ", parents[i].Last()));
                }
            }
            return result;
        }

        
        public List<int[]> CreateInitPopulation(MyCase myOriginalCase, int Size)
        {
            List<int[]> initPopulation = new List<int[]>();
            for (int l = 0; l < Size; l++)
            {
                MyCase myCase = Utility.Clone<MyCase>(myOriginalCase);
                int[] Choosen = new int[myCase.Times.Count];
                for (int j = 0; j < myCase.Times.Count; j++)
                {
                    Choosen[j] = -1;
                }
                bool change = false;
                #region assign program to frame
                while (true)
                {
                    var list1 = Utility.RandomProgramList(myCase.Programs.Where(x => x.MaxShowTime > 0).ToList());
                    foreach (var item in list1)
                    {
                        var gr = myCase.Groups.Where(x => x.Id == item.GroupId).FirstOrDefault();
                        #region group still has cota
                        if (gr.TotalTime >= item.Duration)
                        {
                            var FrameList = item.FrameList;
                            foreach (var frame in FrameList)
                            {
                                int startAv = Utility.GetFirstAvailableSlotInFrame(myCase, Choosen, frame);
                                #region find a empty space to assign
                                if (startAv != -1)
                                {
                                    if (Utility.CheckAssignableProgram(myCase, Choosen, startAv, item))
                                    {
                                        if (Utility.CheckTooClose(myCase, Choosen, startAv, item))
                                        {
                                            Utility.AssignProgramToSche(myCase, Choosen, startAv, item);
                                            change = true;
                                            break;
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                    #region if nothing changes, stop loop
                    if (change)
                        change = false;
                    else
                        break;
                    #endregion
                }
                #endregion
                #region try to assign programe between two frames
                change = false;
                // calculate the unoccupate of two continue frame
                while (true)
                {
                    Utility.UpdateUnOccupiedFrameTime(myCase, Choosen);
                    List<int> Unoccupate = new List<int>();
                    for (int i = 0; i < myCase.Frames.Count - 1; i++)
                    {
                        Unoccupate.Add(myCase.Frames[i].Unoccupate + myCase.Frames[i + 1].Unoccupate);
                    }
                    var list2 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
                    foreach (var pro in list2)
                    {
                        var gr = myCase.Groups.Where(x => x.Id == pro.GroupId).FirstOrDefault();
                        if (gr.TotalTime >= pro.Duration)
                        {
                            for (int i = 0; i < Unoccupate.Count; i++)
                            {
                                if (pro.Duration <= Unoccupate[i])
                                {
                                    //check allowed frames
                                    var FrameIdList = pro.FrameList.Select(x => x.Id).ToList();
                                    //add program to time frame
                                    if (FrameIdList.Contains(i))
                                    {
                                        ////check available slot                      
                                        int startAv = myCase.Frames[i].End - myCase.Frames[i].Unoccupate;
                                        if (startAv != myCase.Frames[i].End)
                                        {
                                            if (Utility.CheckTooClose(myCase, Choosen, startAv, pro))
                                            {
                                                int shift = pro.Duration - myCase.Frames[i].Unoccupate;
                                                // ShiftRight
                                                Utility.ShiftRight(myCase, Choosen, shift, i + 1, Unoccupate);
                                                Utility.AssignProgramToSche(myCase, Choosen, startAv, pro);
                                                change = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        if (change)
                            break;
                    }
                    if (change)
                        change = false;
                    else
                        break;
                }
                #endregion
            
                initPopulation.Add(Choosen);
            }
            return initPopulation;
        }

        public List<int[]> EvaluateAndSelect(MyCase myOriginalCase, List<int[]> population, int Size)
        {
            List<EvaluatePopulation> evaluations = new List<EvaluatePopulation>();
            List<int[]> result = new List<int[]>();
            foreach (var i in population)
            {
                var eva = new EvaluatePopulation();
                eva.resident = i;
                eva.revenue = Utility.CalculateRevenue(myOriginalCase, i);
                evaluations.Add(eva);
            }
            var parents = evaluations.OrderByDescending(x => x.revenue).Take(Size).ToList();
            foreach (var j in parents)
            {
                result.Add(j.resident);
            }
            return result;
        }

        public List<int[]> EvaluateAndSelect2(MyCase myOriginalCase, List<int[]> population, int Size, double percentCrossOver)
        {
            List<EvaluatePopulation> evaluations = new List<EvaluatePopulation>();
            List<int[]> result = new List<int[]>();
            foreach (var i in population)
            {
                var eva = new EvaluatePopulation();
                eva.resident = i;
                eva.revenue = Utility.CalculateRevenue(myOriginalCase, i);
                evaluations.Add(eva);
            }
            var parents = evaluations.OrderByDescending(x => x.revenue).Take(Size-Convert.ToInt32(Size * percentCrossOver)).ToList();
            List<EvaluatePopulation> randomparents = new List<EvaluatePopulation>();
            var half = evaluations.Where(x => !parents.Contains(x)).ToList();
            Random r = new Random();
            if (half.Count <= Convert.ToInt32(Size * percentCrossOver))
            {
                randomparents = half;
            }
            else
            {
                while (randomparents.Count <= Convert.ToInt32(Size * percentCrossOver))
                {
                    if (half.Count < 2)
                    {
                        break;
                    }
                    int j = r.Next(half.Count - 1);
                    if (!randomparents.Contains(half[j]))
                    {
                        randomparents.Add(half[j]);
                    }
                }
            }
            parents = parents.Concat(randomparents).ToList();
            foreach (var j in parents)
            {
                result.Add(j.resident);
            }
            return result;
        }

        public List<int[]> MakeChildren(MyCase myOriginalCase, List<int[]> initPopulation, double percentCrossOver)
        {
            List<int[]> result = new List<int[]>();
            List<int[]> parents = new List<int[]>();
            Random rnd = new Random();
            List<int> saveIndex = new List<int>();
            while (saveIndex.Count() < initPopulation.Count() * percentCrossOver)
            {              
                int r = rnd.Next(0,initPopulation.Count-1);
                if (!saveIndex.Contains(r))
                {
                    parents.Add(initPopulation[r]);
                    saveIndex.Add(r);
                }
            }
            
            for (int i = 0; i < parents.Count() -1; i++)
            {
                for (int j = i + 1; j < parents.Count(); j++)
                {
                    List<int[]> couple = new List<int[]>();
                    couple.Add(parents[i]);
                    couple.Add(parents[j]);
                    List<int[]> r = SingleCross(myOriginalCase, couple);
                    if (r.FirstOrDefault()!=null)
                        result.Add(r.FirstOrDefault()); 
                    if (r.LastOrDefault() != null)
                    result.Add(r.LastOrDefault());
                }
            }
            return result;
        }

        public List<int[]> SingleCross2(MyCase myOriginalCase, List<int[]> couple)
        {
            List<MyProgram> parent1 = Utility.ConvertToProgram(myOriginalCase, couple[0]);
            List<MyProgram> parent2 = Utility.ConvertToProgram(myOriginalCase, couple[1]);
            Random rd = new Random();
            int splitPoint = Convert.ToInt32(Math.Min(parent1.Count(), parent2.Count()) * rd.NextDouble());
            var child1 = new List<MyProgram>();
            var child2 = new List<MyProgram>();
            for (int i = 0; i < parent1.Count(); i++)
            {
                if (i < splitPoint)
                {
                    child1.Add(parent2.ElementAt(i));
                }
                else
                {
                    child1.Add(parent1.ElementAt(i));
                }
            }
            for (int i = 0; i < parent2.Count(); i++)
            {
                if (i < splitPoint)
                {
                    child2.Add(parent1.ElementAt(i));
                }
                else
                {
                    child2.Add(parent2.ElementAt(i));
                }
            }

            List<int[]> result = new List<int[]>();
            result.Add(FixChild(myOriginalCase, child1));
            result.Add(FixChild(myOriginalCase, child2));
            return result;
        }

        public List<int[]> SingleCross(MyCase myOriginalCase, List<int[]> couple)
        {
            List<MyProgram> parent1 = Utility.ConvertToProgram(myOriginalCase, couple[0]);
            List<MyProgram> parent2 = Utility.ConvertToProgram(myOriginalCase, couple[1]);
            int splitPoint = Convert.ToInt32(Math.Min(parent1.Count(), parent2.Count()) * 0.3);
            var child1 = new List<MyProgram>();
            var child2 = new List<MyProgram>();
            for (int i = 0; i < parent1.Count(); i++)
            {                
                if (i < splitPoint)
                {
                    child1.Add(parent2.ElementAt(i));
                }
                else
                {
                    child1.Add(parent1.ElementAt(i));
                }
            }
            for (int i = 0; i < parent2.Count(); i++)
            {
                if (i < splitPoint)
                {
                    child2.Add(parent1.ElementAt(i));
                }
                else
                {
                    child2.Add(parent2.ElementAt(i));
                }
            }

            List<int[]> result = new List<int[]>();
            result.Add(FixChild(myOriginalCase, child1));
            result.Add(FixChild(myOriginalCase, child2));
            return result;
        }

        public int[] FixChild(MyCase myOriginalCase, List<MyProgram> child)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            MyCase myCase = Utility.Clone<MyCase>(myOriginalCase);
            
            int[] Choosen = new int[myCase.Times.Count];
            for (int j = 0; j < myCase.Times.Count; j++)
            {
                Choosen[j] = -1;
            }
            #region assign program to frame
            var list1 = child;            
            var MissingPro = new List<MyProgram>();
            foreach (var pro in myCase.Programs)
            {
                if (list1.Where(x => x.Id == pro.Id).FirstOrDefault() == null)
                {
                    MissingPro.Add(pro);
                }
            }
            list1 = MissingPro.Concat(list1).ToList();

            foreach (var item in list1)
            {
                if (myCase.Programs.Where(x=>x.Id == item.Id).First().MaxShowTime > 0)
                {
                    var gr = myCase.Groups.Where(x => x.Id == item.GroupId).FirstOrDefault();
                    #region group still has cota
                    if (gr.TotalTime >= item.Duration)
                    {
                        var FrameList = item.FrameList;
                        foreach (var frame in FrameList)
                        {
                            int startAv = Utility.GetFirstAvailableSlotInFrame(myCase, Choosen, frame);
                            #region find a empty space to assign
                            if (startAv != -1)
                            {
                                if (Utility.CheckAssignableProgram(myCase, Choosen, startAv, item))
                                {
                                    if (Utility.CheckTooClose(myCase, Choosen, startAv, item))
                                    {
                                        Utility.AssignProgramToSche(myCase, Choosen, startAv, item);
                                        break;
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }
                #endregion                               
            }
            #endregion
            #region try to assign programe between two frames
            var change = false;
            // calculate the unoccupate of two continue frame
            while (true)
            {
                Utility.UpdateUnOccupiedFrameTime(myCase, Choosen);
                List<int> Unoccupate = new List<int>();
                for (int i = 0; i < myCase.Frames.Count - 1; i++)
                {
                    Unoccupate.Add(myCase.Frames[i].Unoccupate + myCase.Frames[i + 1].Unoccupate);
                }
                var list2 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
                foreach (var pro in list2)
                {
                    var gr = myCase.Groups.Where(x => x.Id == pro.GroupId).FirstOrDefault();
                    if (gr.TotalTime >= pro.Duration)
                    {
                        for (int i = 0; i < Unoccupate.Count; i++)
                        {
                            if (pro.Duration <= Unoccupate[i])
                            {
                                //check allowed frames
                                var FrameIdList = pro.FrameList.Select(x => x.Id).ToList();
                                //add program to time frame
                                if (FrameIdList.Contains(i))
                                {
                                    ////check available slot                      
                                    int startAv = myCase.Frames[i].End - myCase.Frames[i].Unoccupate;
                                    if (startAv != myCase.Frames[i].End)
                                    {
                                        if (Utility.CheckTooClose(myCase, Choosen, startAv, pro))
                                        {
                                            int shift = pro.Duration - myCase.Frames[i].Unoccupate;
                                            // ShiftRight
                                            Utility.ShiftRight(myCase, Choosen, shift, i + 1, Unoccupate);
                                            Utility.AssignProgramToSche(myCase, Choosen, startAv, pro);
                                            change = true;
                                            break;
                                        }
                                    }
                                }
                            }

                        }
                    }
                    if (change)
                        break;
                }
                if (change)
                    change = false;
                else
                    break;
            }
            #endregion
          
            var va = Validate.ValidateResult(myOriginalCase, Choosen);
            watch.Stop();
            var elapsedH1 = watch.ElapsedMilliseconds;
            if(va.Where(x=>x!=0).Any())
                return null;
            else
                return Choosen;
        }

        private class EvaluatePopulation
        {
            public int[] resident { get; set; }
            public double revenue { get; set; }
            public double min { get; set; }
            public double max { get; set; }
        }

        private class EvaluatePopulation2
        {
            public Chromosome resident { get; set; }
            public double revenue { get; set; }
        }


        public class Chromosome
        {
            public int[] Choosen { get; set; }
            public bool CanBorn { get; set; }
        }
    }
}
