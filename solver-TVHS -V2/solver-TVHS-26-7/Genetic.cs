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
        #region temp
        //public MyStatictis Solve(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, string filename)
        //{
        //    if (!File.Exists(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
        //    {
        //        // Create a file to write to.
        //        using (StreamWriter sw = File.CreateText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
        //        {
        //        }
        //    }                 
        //    MyCase myOriginalCase = input;
        //    double revenue = 0;
        //    MyStatictis result = new MyStatictis();
        //    result.noGen = 0;
        //    var watch = System.Diagnostics.Stopwatch.StartNew();
        //    List<EvaluatePopulation> initPopulation = CreateInitPopulation(myOriginalCase, initSize);
        //    revenue = initPopulation.OrderByDescending(x => x.revenue).FirstOrDefault().revenue;
        //    int count = 0;
        //    using (System.IO.StreamWriter file = File.AppendText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
        //    {
        //        file.WriteLine(DateTime.Now.ToLongDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString());                    
        //        for (var lop = 0; lop < noloop; lop++)
        //        {
        //            result.noGen++;
        //            var Parents = SelectParents(myOriginalCase, initPopulation, percentCrossOver);
        //            var Children = MakeChildren(myOriginalCase, Parents);
        //            foreach(var child in Children)
        //            {
        //                initPopulation.Add(child);
        //            }             
        //            initPopulation = Resize(myOriginalCase, initPopulation, populationSize, 0.3);
        //            double re = initPopulation.FirstOrDefault().revenue;
        //            if (re > revenue)
        //            {
        //                revenue = re;
        //                count = 0;
        //            }
        //            count++;
        //            Debug.WriteLine(lop + "\t" + revenue);
        //            file.WriteLine(lop + "\t" + revenue);
        //            if (count > nochange)
        //            {
        //                break;
        //            }
        //        }
        //        file.WriteLine("");
        //    }
        //    watch.Stop();
        //    var elapsedH1 = watch.ElapsedMilliseconds;
        //    result.Choosen = initPopulation.FirstOrDefault().Choosen;
        //    result.Elapsed = elapsedH1;
        //    result.Revenue = Utility.CalculateRevenue(myOriginalCase, result.Choosen);
        //    return result;
        //}
        
        

        public List<EvaluatePopulation> CreateInitPopulationTree(MyCase myOriginalCase, int size)
        {
            var result = new List<EvaluatePopulation>();
            while (result.Count < size)
            {
                var myCase = myOriginalCase.DeepCopyByExpressionTree();
                var choosen = new int[myCase.Times.Count];
                for (var j = 0; j < myCase.Times.Count; j++)
                {
                    choosen[j] = -1;
                }
                bool change = false;
                #region assign program to frame
                var list1 = Utility.RandomProgramListTree(myCase.Programs.Where(x => x.MaxShowTime > 0).ToList());
                while (true)
                {                   
                    foreach (var item in list1)
                    {
                        var gr = myCase.Groups.First(x => x.Id == item.GroupId);
                        #region group still has cota
                        if (gr.TotalTime >= item.Duration)
                        {
                            var frameList = Utility.RandomFrameListTree(item.FrameList);
                            foreach (var frame in frameList)
                            {
                                var startAv = Utility.GetFirstAvailableSlotInFrame(myCase, choosen, frame);
                                #region find a empty space to assign
                                if (startAv != -1)
                                {
                                    if (Utility.CheckAssignableProgram(myCase, choosen, startAv, item))
                                    {
                                        if (Utility.CheckTooClose(myCase, choosen, startAv, item))
                                        {
                                            Utility.AssignProgramToScheTree(choosen, startAv, item, gr);
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
                var list2 = Utility.RandomProgramListTree(myCase.Programs.Where(x => x.MaxShowTime > 0).ToList());
                while (true)
                {
                    Utility.UpdateUnOccupiedFrameTime(myCase, choosen);
                    var unoccupate = new List<int>();
                    for (var i = 0; i < myCase.Frames.Count - 1; i++)
                    {
                        unoccupate.Add(myCase.Frames[i].Unoccupate + myCase.Frames[i + 1].Unoccupate);
                    }

                    foreach (var pro in list2)
                    {
                        var gr = myCase.Groups.First(x => x.Id == pro.GroupId);
                        if (gr.TotalTime >= pro.Duration)
                        {
                            for (int i = 0; i < unoccupate.Count; i++)
                            {
                                if (pro.Duration <= unoccupate[i])
                                {
                                    //check allowed frames
                                    var frameIdList = pro.FrameList.Select(x => x.Id).ToList();
                                    //add program to time frame
                                    if (frameIdList.Contains(i))
                                    {
                                        ////check available slot                      
                                        int startAv = myCase.Frames[i].End - myCase.Frames[i].Unoccupate;
                                        if (startAv != myCase.Frames[i].End)
                                        {
                                            if (Utility.CheckTooClose(myCase, choosen, startAv, pro))
                                            {
                                                int shift = pro.Duration - myCase.Frames[i].Unoccupate;
                                                // ShiftRight
                                                Utility.ShiftRight(myCase, choosen, shift, i + 1, unoccupate);
                                                Utility.AssignProgramToScheTree(choosen, startAv, pro, gr);
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
                var e = new EvaluatePopulation {Choosen = choosen};
                if (!CheckExist(result, e))
                {
                    var vals = Validate.ValidateResult(myOriginalCase, choosen);
                    var error = false;
                    foreach (var val in vals)
                    {
                        if (val < 0)
                        {
                            error = true;
                            break;
                        }
                    }
                    if (!error)
                    {
                        e.revenue = Utility.CalculateRevenue(myOriginalCase, choosen);
                        result.Add(e);
                    }
                }
            }
            return result;
        }

        public bool CheckExist (List<EvaluatePopulation> population, EvaluatePopulation e)
        {
            foreach(var p in population)
            {
                if (p.Choosen.SequenceEqual(e.Choosen))
                {
                    return true;
                }
            }
            return false;
        }
        

        // have to make it clare => read more papers
        public List<List<EvaluatePopulation>> SelectParentsTree(MyCase myOriginalCase, List<EvaluatePopulation> population, double percentCrossOver)
        {
            List < EvaluatePopulation > cache= new List<EvaluatePopulation>();
            double sum = 0;
            double total = population.Sum(x => x.revenue);
            foreach (var e in population)
            {
                e.min = sum;
                sum += e.revenue / total;
                e.max = sum;
            }

            var rnd = new Random();
            var parents = new List<List<EvaluatePopulation>>();
            while (parents.Count() < population.Count() * percentCrossOver / 2)
            {
                var couple = new List<EvaluatePopulation>();
                var r1 = rnd.NextDouble();
                var r2 = rnd.NextDouble();
                var a1 = population.FirstOrDefault(x => x.min < r1 && x.max >= r1);
                var a2 = population.FirstOrDefault(x => x.min < r2 && x.max >= r2);
                if (a1 == null || a2 == null)
                {
                    continue;
                }
                couple.Add(a1);
                couple.Add(a2);
                if (CheckExist(cache, a1) && CheckExist(cache, a2))
                {
                    continue;
                }
                parents.Add(couple);
            }
            return parents;
        }
        
        
        public List<EvaluatePopulation> MakeChildrenTree(MyCase myOriginalCase, List<List<EvaluatePopulation>> parents)
        {
            var result = new List<EvaluatePopulation>();
            for (int i = 0; i < parents.Count(); i++)
            {
                var children = SingleCrossTree(myOriginalCase, parents[i]);
                if (children.FirstOrDefault() != null)
                    result.Add(children.FirstOrDefault());
                if (children.LastOrDefault() != null)
                    result.Add(children.LastOrDefault());
            }
            return result;
        }

        public List<EvaluatePopulation> SingleCrossTree(MyCase myOriginalCase, List<EvaluatePopulation> couple)
        {
            var parent1 = Utility.ConvertToProgram(myOriginalCase, couple[0].Choosen);
            var parent2 = Utility.ConvertToProgram(myOriginalCase, couple[1].Choosen);
            // random split => read more paper
            var splitPoint = Convert.ToInt32(Math.Min(parent1.Count(), parent2.Count()) * GetRandomNumber(0.1, 0.9));
            var child1 = new List<MyProgram>();
            var child2 = new List<MyProgram>();
            for (var i = 0; i < parent1.Count(); i++)
            {
                child1.Add(i < splitPoint ? parent2.ElementAt(i) : parent1.ElementAt(i));
            }
            for (var i = 0; i < parent2.Count(); i++)
            {
                child2.Add(i < splitPoint ? parent1.ElementAt(i) : parent2.ElementAt(i));
            }

            var result = new List<EvaluatePopulation>();
            var newChild1 = FixChildTree(myOriginalCase, child1);
            if(newChild1 != null)
                result.Add(newChild1);
            var newChild2 = FixChildTree(myOriginalCase, child2);
            if (newChild2 != null)
                result.Add(newChild2);
            return result;
        }

        public EvaluatePopulation FixChildTree(MyCase myOriginalCase, List<MyProgram> originalChild)
        {
            var myCase = myOriginalCase.DeepCopyByExpressionTree();
            var child = originalChild.DeepCopyByExpressionTree();
            var choosen = new int[myCase.Times.Count];
            for (var j = 0; j < myCase.Times.Count; j++)
            {
                choosen[j] = -1;
            }
            #region assign program to frame
            var list1 = child;
            var missingPro = new List<MyProgram>();
            foreach (var pro in myCase.Programs)
            {
                if (list1.FirstOrDefault(x => x.Id == pro.Id) == null)
                {
                    missingPro.Add(pro);
                }
            }
            list1 = missingPro.Concat(list1).ToList();
            foreach (var item in list1)
            {
                if (myCase.Programs.First(x => x.Id == item.Id).MaxShowTime > 0)
                {
                    var gr = myCase.Groups.First(x => x.Id == item.GroupId);
                    #region group still has cota
                    if (gr.TotalTime >= item.Duration)
                    {
                        var frameList = item.FrameList;
                        foreach (var frame in frameList)
                        {
                            int startAv = Utility.GetFirstAvailableSlotInFrame(myCase, choosen, frame);
                            #region find a empty space to assign
                            if (startAv != -1)
                            {
                                if (Utility.CheckAssignableProgram(myCase, choosen, startAv, item))
                                {
                                    if (Utility.CheckTooClose(myCase, choosen, startAv, item))
                                    {
                                        Utility.AssignProgramToScheTree(choosen, startAv, item, gr);
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
            //var change = false;
            //// calculate the unoccupate of two continue frame
            //while (true)
            //{
            //    Utility.UpdateUnOccupiedFrameTime(myCase, choosen);
            //    var Unoccupate = new List<int>();
            //    for (int i = 0; i < myCase.Frames.Count - 1; i++)
            //    {
            //        Unoccupate.Add(myCase.Frames[i].Unoccupate + myCase.Frames[i + 1].Unoccupate);
            //    }
            //    var list2 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
            //    foreach (var pro in list2)
            //    {
            //        var gr = myCase.Groups.Where(x => x.Id == pro.GroupId).FirstOrDefault();
            //        if (gr.TotalTime >= pro.Duration)
            //        {
            //            for (int i = 0; i < Unoccupate.Count; i++)
            //            {
            //                if (pro.Duration <= Unoccupate[i])
            //                {
            //                    //check allowed frames
            //                    var FrameIdList = pro.FrameList.Select(x => x.Id).ToList();
            //                    //add program to time frame
            //                    if (FrameIdList.Contains(i))
            //                    {
            //                        ////check available slot                      
            //                        int startAv = myCase.Frames[i].End - myCase.Frames[i].Unoccupate;
            //                        if (startAv != myCase.Frames[i].End)
            //                        {
            //                            if (Utility.CheckTooClose(myCase, choosen, startAv, pro))
            //                            {
            //                                int shift = pro.Duration - myCase.Frames[i].Unoccupate;
            //                                // ShiftRight
            //                                Utility.ShiftRight(myCase, choosen, shift, i + 1, Unoccupate);
            //                                Utility.AssignProgramToScheTree(choosen, startAv, pro, gr);
            //                                change = true;
            //                                break;
            //                            }
            //                        }
            //                    }
            //                }

            //            }
            //        }
            //        if (change)
            //            break;
            //    }
            //    if (change)
            //        change = false;
            //    else
            //        break;
            //}
            #endregion

            var va = Validate.ValidateResult(myOriginalCase, choosen);
            if (va.Where(x => x != 0).Any())
                return null;
            else
            {
                var e = new EvaluatePopulation();
                e.Choosen = choosen;
                e.revenue = Utility.CalculateRevenue(myOriginalCase, choosen);
                return e;
            }

        }
           
        
        public double GetRandomNumber(double minimum, double maximum)
        {
            var random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        #endregion

        #region gen 1
        //public MyStatictis Solve2(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename)
        //{
        //    if (!File.Exists(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
        //    {
        //        // Create a file to write to.
        //        using (StreamWriter sw = File.CreateText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
        //        {
        //        }
        //    }
        //    MyCase myOriginalCase = input;
        //    double revenue = 0;
        //    MyStatictis result = new MyStatictis();
        //    result.noGen = 0;
        //    var watch = System.Diagnostics.Stopwatch.StartNew();
        //    List<EvaluatePopulation> initPopulation = CreateInitPopulation(myOriginalCase, initSize);
        //    revenue = initPopulation.OrderByDescending(x => x.revenue).FirstOrDefault().revenue;
        //    int count = 0;
        //    using (System.IO.StreamWriter file = File.AppendText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
        //    {
        //        file.WriteLine(DateTime.Now.ToLongDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString());
        //        for (var lop = 0; lop < noloop; lop++)
        //        {
        //            result.noGen++;
        //            var Parents = SelectParents(myOriginalCase, initPopulation, percentCrossOver);
        //            var Children = MakeChildren(myOriginalCase, Parents);
        //            foreach (var child in Children)
        //            {
        //                initPopulation.Add(child);
        //            }
        //            initPopulation = Resize2(initPopulation, populationSize);
        //            initPopulation = Mutate(myOriginalCase, initPopulation, mutationPercent);
        //            double re = initPopulation.OrderByDescending(x => x.revenue).FirstOrDefault().revenue;
        //            if (re > revenue)
        //            {
        //                revenue = re;
        //                count = 0;
        //            }
        //            count++;
        //            Debug.WriteLine(lop + "\t" + revenue);
        //            file.WriteLine(lop + "\t" + revenue);
        //            if (count > nochange)
        //            {
        //                break;
        //            }
        //        }
        //        file.WriteLine("");
        //    }
        //    watch.Stop();
        //    var elapsedH1 = watch.ElapsedMilliseconds;
        //    result.Choosen = initPopulation.FirstOrDefault().Choosen;
        //    result.Elapsed = elapsedH1;
        //    result.Revenue = Utility.CalculateRevenue(myOriginalCase, result.Choosen);
        //    return result;
        //}
        public List<EvaluatePopulation> Resize2(List<EvaluatePopulation> population, int Size)
        {
            var theBest = population.OrderByDescending(x => x.revenue).Take(Size).ToList();
            return theBest;
        }
       
        public List<EvaluatePopulation> MutateTree(MyCase myOriginalCase, List<EvaluatePopulation> population, double percent)
        {
            Random ran = new Random();
            List<int> indexSave = new List<int>();
            while (indexSave.Count() <= population.Count * percent)
            {
                int index = ran.Next(population.Count);
                if (!indexSave.Contains(index))
                {
                    List<MyProgram> chromosome = Utility.ConvertToProgram(myOriginalCase, population[index].Choosen);
                    int indexMutation = ran.Next(chromosome.Count);
                    int indexProgram = ran.Next(myOriginalCase.Programs.Count);
                    chromosome[indexMutation] = myOriginalCase.Programs[indexProgram];
                    var fixedChild = FixChildTree(myOriginalCase, chromosome);
                    if (fixedChild != null)
                    {
                        population[index] = fixedChild;
                        indexSave.Add(index);
                    }

                }
            }
            return population;
        }


        public List<EvaluatePopulation> Resize(MyCase myOriginalCase, List<EvaluatePopulation> population, int Size, double percentCrossOver)
        {
            var theBest = population.OrderByDescending(x => x.revenue).Take(Size - Convert.ToInt32(Size * percentCrossOver)).ToList();
            List<EvaluatePopulation> randomparents = new List<EvaluatePopulation>();
            var ran = population.Where(x => !theBest.Contains(x)).ToList();
            Random r = new Random();
            if (ran.Count <= Convert.ToInt32(Size * percentCrossOver))
            {
                randomparents = ran;
            }
            else
            {
                while (randomparents.Count <= Convert.ToInt32(Size * percentCrossOver))
                {
                    if (ran.Count < 2)
                    {
                        break;
                    }
                    int j = r.Next(ran.Count - 1);
                    if (!randomparents.Contains(ran[j]))
                    {
                        randomparents.Add(ran[j]);
                    }
                }
            }
            theBest = theBest.Concat(randomparents).ToList();
            return theBest;
        }
        #endregion

        #region gen 1_1
        public MyStatictis Solve1(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename)
        {
            if (!File.Exists(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
                {
                }
            }
            var myOriginalCase = input;
            double revenue = 0;
            var result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulationTree(myOriginalCase, initSize);
            revenue = initPopulation.Max(x => x.revenue);
            int count = 0;
            using (System.IO.StreamWriter file = File.AppendText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultGen1.txt"))
            {
                file.WriteLine(DateTime.Now.ToLongDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString());
                for (var lop = 0; lop < noloop; lop++)
                {
                    result.noGen++;
                    var parents = SelectParentsTree(myOriginalCase, initPopulation, percentCrossOver);
                    var children = MakeChildrenTree(myOriginalCase, parents);
                    initPopulation = initPopulation.Concat(children).ToList();
                    initPopulation = MutateTree(myOriginalCase, initPopulation, mutationPercent);
                    initPopulation = Resize2(initPopulation, populationSize);
                    var re = initPopulation.Max(x => x.revenue);
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
            var firstOrDefault = initPopulation.FirstOrDefault();
            if (firstOrDefault != null) result.Choosen = firstOrDefault.Choosen;
            result.Elapsed = elapsedH1;
            result.Revenue = Utility.CalculateRevenue(myOriginalCase, result.Choosen);
            return result;
        }
       
        #endregion

        #region gen 2
        //public MyStatictis Solve3(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename, List<int[]> heuristics)
        //{
        //    if (!File.Exists(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
        //    {
        //        // Create a file to write to.
        //        using (StreamWriter sw = File.CreateText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
        //        {
        //        }
        //    }
        //    MyCase myOriginalCase = input;
        //    double revenue = 0;
        //    MyStatictis result = new MyStatictis();
        //    result.noGen = 0;
        //    var watch = System.Diagnostics.Stopwatch.StartNew();
        //    List<EvaluatePopulation> initPopulation = CreateInitPopulation(myOriginalCase, initSize);
        //    foreach(var i in heuristics)
        //    {
        //        EvaluatePopulation e = new EvaluatePopulation();
        //        e.Choosen = i;
        //        e.revenue = Utility.CalculateRevenue(myOriginalCase, i);
        //        initPopulation.Add(e);
        //    }
        //    revenue = initPopulation.OrderByDescending(x => x.revenue).FirstOrDefault().revenue;
        //    int count = 0;
        //    using (System.IO.StreamWriter file = File.AppendText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
        //    {
        //        file.WriteLine(DateTime.Now.ToLongDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString());
        //        for (var lop = 0; lop < noloop; lop++)
        //        {
        //            result.noGen++;
        //            var Parents = SelectParents(myOriginalCase, initPopulation, percentCrossOver);
        //            var Children = MakeChildren(myOriginalCase, Parents);
        //            foreach (var child in Children)
        //            {
        //                initPopulation.Add(child);
        //            }
        //            initPopulation = Resize2(initPopulation, populationSize);
        //            initPopulation = Mutate(myOriginalCase, initPopulation, mutationPercent);
        //            double re = initPopulation.OrderByDescending(x => x.revenue).FirstOrDefault().revenue;
        //            if (re > revenue)
        //            {
        //                revenue = re;
        //                count = 0;
        //            }
        //            count++;
        //            Debug.WriteLine(lop + "\t" + revenue);
        //            file.WriteLine(lop + "\t" + revenue);
        //            if (count > nochange)
        //            {
        //                break;
        //            }
        //        }
        //        file.WriteLine("");
        //    }
        //    watch.Stop();
        //    var elapsedH1 = watch.ElapsedMilliseconds;
        //    result.Choosen = initPopulation.FirstOrDefault().Choosen;
        //    result.Elapsed = elapsedH1;
        //    result.Revenue = Utility.CalculateRevenue(myOriginalCase, result.Choosen);
        //    return result;
        //}

        #endregion

        #region gen 2_1
        public MyStatictis Solve3_1(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename, List<int[]> heuristics)
        {
            if (!File.Exists(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
                {
                }
            }
            MyCase myOriginalCase = input;
            double revenue = 0;
            MyStatictis result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<EvaluatePopulation> initPopulation = CreateInitPopulationTree(myOriginalCase, initSize);
            foreach (var i in heuristics)
            {
                EvaluatePopulation e = new EvaluatePopulation();
                e.Choosen = i;
                e.revenue = Utility.CalculateRevenue(myOriginalCase, i);
                initPopulation.Add(e);
            }
            revenue = initPopulation.Max(x => x.revenue);
            int count = 0;
            using (System.IO.StreamWriter file = File.AppendText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
            {
                file.WriteLine(DateTime.Now.ToLongDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString());
                for (var lop = 0; lop < noloop; lop++)
                {
                    result.noGen++;
                    var Parents = SelectParentsTree(myOriginalCase, initPopulation, percentCrossOver);
                    var Children = MakeChildrenTree(myOriginalCase, Parents);

                    initPopulation = initPopulation.Concat(Children).ToList();
                    initPopulation = Resize2(initPopulation, populationSize);
                    initPopulation = MutateTree(myOriginalCase, initPopulation, mutationPercent);
                    double re = initPopulation.Max(x=>x.revenue);
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
            result.Choosen = initPopulation.FirstOrDefault().Choosen;
            result.Elapsed = elapsedH1;
            result.Revenue = Utility.CalculateRevenue(myOriginalCase, result.Choosen);
            return result;
        }

        #endregion
    }
    public class EvaluatePopulation
    {
        public double revenue { get; set; }
        public double min { get; set; }
        public double max { get; set; }
        public int[] Choosen { get; set; }
        public bool CanBorn { get; set; }
    }

}
