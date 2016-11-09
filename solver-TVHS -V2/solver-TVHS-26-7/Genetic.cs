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
        public MyStatictis Solve(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, string filename)
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
            List<EvaluatePopulation> initPopulation = CreateInitPopulation(myOriginalCase, initSize);
            revenue = initPopulation.OrderByDescending(x => x.revenue).FirstOrDefault().revenue;
            int count = 0;
            using (System.IO.StreamWriter file = File.AppendText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
            {
                file.WriteLine(DateTime.Now.ToLongDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString());                    
                for (var lop = 0; lop < noloop; lop++)
                {
                    result.noGen++;
                    var Parents = SelectParents(myOriginalCase, initPopulation, percentCrossOver);
                    var Children = MakeChildren(myOriginalCase, Parents);
                    foreach(var child in Children)
                    {
                        initPopulation.Add(child);
                    }             
                    initPopulation = Resize(myOriginalCase, initPopulation, populationSize, 0.3);
                    double re = initPopulation.FirstOrDefault().revenue;
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
        
        public List<EvaluatePopulation> CreateInitPopulation(MyCase myOriginalCase, int Size)
        {
            List<EvaluatePopulation> result = new List<EvaluatePopulation>();

            while(result.Count < Size)
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
                            var FrameList = Utility.RandomFrameList(item.FrameList);
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
                EvaluatePopulation e = new EvaluatePopulation();
                e.Choosen = Choosen;
                if (!CheckExist(result, e))
                {
                    e.revenue = Utility.CalculateRevenue(myOriginalCase, Choosen);
                    result.Add(e);
                }
            }
            return result;
        }


        public List<EvaluatePopulation> CreateInitPopulationTree(MyCase myOriginalCase, int Size)
        {
            List<EvaluatePopulation> result = new List<EvaluatePopulation>();

            while (result.Count < Size)
            {
                MyCase myCase = myOriginalCase.DeepCopyByExpressionTree();
                int[] Choosen = new int[myCase.Times.Count];
                for (int j = 0; j < myCase.Times.Count; j++)
                {
                    Choosen[j] = -1;
                }
                bool change = false;
                #region assign program to frame
                while (true)
                {
                    var list1 = Utility.RandomProgramListTree(myCase.Programs.Where(x => x.MaxShowTime > 0).ToList());
                    foreach (var item in list1)
                    {
                        var gr = myCase.Groups.Where(x => x.Id == item.GroupId).FirstOrDefault();
                        #region group still has cota
                        if (gr.TotalTime >= item.Duration)
                        {
                            var FrameList = Utility.RandomFrameListTree(item.FrameList);
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
                                            Utility.AssignProgramToScheTree(Choosen, startAv, item, gr);
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
                                                Utility.AssignProgramToScheTree(Choosen, startAv, pro, gr);
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
                EvaluatePopulation e = new EvaluatePopulation();
                e.Choosen = Choosen;
                if (!CheckExist(result, e))
                {
                    e.revenue = Utility.CalculateRevenue(myOriginalCase, Choosen);
                    result.Add(e);
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

        public List<List<EvaluatePopulation>> SelectParents(MyCase myOriginalCase, List<EvaluatePopulation> population, double percentCrossOver)
        {
            population = population.OrderByDescending(x => x.revenue).ToList();
            double sum = 0;
            double total = population.Sum(x => x.revenue);
            foreach (var e in population)
            {
                e.min = sum;
                sum += e.revenue / total;
                e.max = sum;
            }

            Random rnd = new Random();
            List<List<EvaluatePopulation>> parents = new List<List<EvaluatePopulation>>();
            while (parents.Count() < population.Count() * percentCrossOver / 2)
            {
                double r = rnd.NextDouble();
                List<EvaluatePopulation> couple = new List<EvaluatePopulation>();
                var a = population.Where(x => x.min < r && x.max >= r).FirstOrDefault();
                if (a != null)
                {
                    couple.Add(a);
                }

                r = rnd.NextDouble();
                a = population.Where(x => x.min < r && x.max >= r).FirstOrDefault();
                if (a != null)
                {
                    couple.Add(a);
                }
                parents.Add(couple);
            }
            return parents;
        }

        public List<List<EvaluatePopulation>> SelectParentsTree(MyCase myOriginalCase, List<EvaluatePopulation> population, double percentCrossOver)
        {
            double sum = 0;
            double total = population.Sum(x => x.revenue);
            foreach (var e in population)
            {
                e.min = sum;
                sum += e.revenue / total;
                e.max = sum;
            }

            Random rnd = new Random();
            List<List<EvaluatePopulation>> parents = new List<List<EvaluatePopulation>>();
            while (parents.Count() < population.Count() * percentCrossOver / 2)
            {
                List<EvaluatePopulation> couple = new List<EvaluatePopulation>();
                double r1 = rnd.NextDouble();
                double r2 = rnd.NextDouble();
                var a1 = population.Where(x => x.min < r1 && x.max >= r1).FirstOrDefault();
                var a2 = population.Where(x => x.min < r2 && x.max >= r2).FirstOrDefault();
                if (a1 == null || a2 == null)
                {
                    continue;
                }
                couple.Add(a1);
                couple.Add(a2);               
                parents.Add(couple);
            }
            return parents;
        }
        
        public List<EvaluatePopulation> MakeChildren(MyCase myOriginalCase, List<List<EvaluatePopulation>> parents)
        {
            List<EvaluatePopulation> result = new List<EvaluatePopulation>();
            for (int i = 0; i < parents.Count(); i++)
            {
                List<EvaluatePopulation> r = SingleCross(myOriginalCase, parents[i]);
                if (r.FirstOrDefault() != null)
                    result.Add(r.FirstOrDefault());
                if (r.LastOrDefault() != null)
                    result.Add(r.LastOrDefault());              
            }
            return result;
        }

        public List<EvaluatePopulation> MakeChildrenTree(MyCase myOriginalCase, List<List<EvaluatePopulation>> parents)
        {
            List<EvaluatePopulation> result = new List<EvaluatePopulation>();
            for (int i = 0; i < parents.Count(); i++)
            {
                List<EvaluatePopulation> r = SingleCrossTree(myOriginalCase, parents[i]);
                if (r.FirstOrDefault() != null)
                    result.Add(r.FirstOrDefault());
                if (r.LastOrDefault() != null)
                    result.Add(r.LastOrDefault());
            }
            return result;
        }

        public List<EvaluatePopulation> SingleCrossTree(MyCase myOriginalCase, List<EvaluatePopulation> couple)
        {
            List<MyProgram> parent1 = Utility.ConvertToProgram(myOriginalCase, couple[0].Choosen);
            List<MyProgram> parent2 = Utility.ConvertToProgram(myOriginalCase, couple[1].Choosen);
            int splitPoint = Convert.ToInt32(Math.Min(parent1.Count(), parent2.Count()) * GetRandomNumber(0.1, 0.9));
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

            List<EvaluatePopulation> result = new List<EvaluatePopulation>();
            result.Add(FixChildTree(myOriginalCase, child1));
            result.Add(FixChildTree(myOriginalCase, child2));
            return result;
        }

        public EvaluatePopulation FixChildTree(MyCase myOriginalCase, List<MyProgram> originalChild)
        {
            MyCase myCase = myOriginalCase.DeepCopyByExpressionTree();
            List<MyProgram> child = originalChild.DeepCopyByExpressionTree();
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
                if (myCase.Programs.Where(x => x.Id == item.Id).First().MaxShowTime > 0)
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
                                        Utility.AssignProgramToScheTree(Choosen, startAv, item, gr);
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
                                            Utility.AssignProgramToScheTree(Choosen, startAv, pro, gr);
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
            if (va.Where(x => x != 0).Any())
                return null;
            else
            {
                EvaluatePopulation e = new EvaluatePopulation();
                e.Choosen = Choosen;
                e.revenue = Utility.CalculateRevenue(myOriginalCase, Choosen);
                return e;
            }

        }


        public List<EvaluatePopulation> SingleCross(MyCase myOriginalCase, List<EvaluatePopulation> couple)
        {
            List<MyProgram> parent1 = Utility.ConvertToProgram(myOriginalCase, couple[0].Choosen);
            List<MyProgram> parent2 = Utility.ConvertToProgram(myOriginalCase, couple[1].Choosen);
            int splitPoint = Convert.ToInt32(Math.Min(parent1.Count(), parent2.Count()) * GetRandomNumber(0.1,0.9));
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

            List<EvaluatePopulation> result = new List<EvaluatePopulation>();
            result.Add(FixChild(myOriginalCase, child1));
            result.Add(FixChild(myOriginalCase, child2));
            return result;
        }
           
        public EvaluatePopulation FixChild(MyCase myOriginalCase, List<MyProgram> child)
        {
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
            if(va.Where(x=>x!=0).Any())
                return null;
            else
            {
                EvaluatePopulation e = new EvaluatePopulation();
                e.Choosen = Choosen;
                e.revenue = Utility.CalculateRevenue(myOriginalCase, Choosen);
                return e;
            }
                
        }

        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        #endregion

        #region gen 1
        public MyStatictis Solve2(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename)
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
            List<EvaluatePopulation> initPopulation = CreateInitPopulation(myOriginalCase, initSize);
            revenue = initPopulation.OrderByDescending(x => x.revenue).FirstOrDefault().revenue;
            int count = 0;
            using (System.IO.StreamWriter file = File.AppendText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
            {
                file.WriteLine(DateTime.Now.ToLongDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString());
                for (var lop = 0; lop < noloop; lop++)
                {
                    result.noGen++;
                    var Parents = SelectParents(myOriginalCase, initPopulation, percentCrossOver);
                    var Children = MakeChildren(myOriginalCase, Parents);
                    foreach (var child in Children)
                    {
                        initPopulation.Add(child);
                    }
                    initPopulation = Resize2(initPopulation, populationSize);
                    initPopulation = Mutate(myOriginalCase, initPopulation, mutationPercent);
                    double re = initPopulation.OrderByDescending(x => x.revenue).FirstOrDefault().revenue;
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
        public List<EvaluatePopulation> Resize2(List<EvaluatePopulation> population, int Size)
        {
            var theBest = population.OrderByDescending(x => x.revenue).Take(Size).ToList();
            return theBest;
        }

        public List<EvaluatePopulation> Mutate(MyCase myOriginalCase, List<EvaluatePopulation> population, double percent)
        {
            Random ran = new Random();
            List<int> indexSave = new List<int>();
            while (indexSave.Count() <= population.Count * percent)
            {
                int index = ran.Next(0, population.Count - 1);
                if (!indexSave.Contains(index))
                {
                    List<MyProgram> chromosome = Utility.ConvertToProgram(myOriginalCase, population[index].Choosen);
                    int indexMutation = ran.Next(0, chromosome.Count - 1);
                    int indexProgram = ran.Next(0, myOriginalCase.Programs.Count - 1);
                    chromosome[indexMutation] = myOriginalCase.Programs[indexProgram];
                    var fixedChild = FixChild(myOriginalCase, chromosome);
                    if (fixedChild != null)
                    {
                        population[index] = fixedChild;
                        indexSave.Add(index);
                    }

                }
            }
            return population;
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
        public MyStatictis Solve2_1(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename)
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
                    double re = initPopulation.Max(x => x.revenue);
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

        #region gen 2
        public MyStatictis Solve3(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename, List<int[]> heuristics)
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
            List<EvaluatePopulation> initPopulation = CreateInitPopulation(myOriginalCase, initSize);
            foreach(var i in heuristics)
            {
                EvaluatePopulation e = new EvaluatePopulation();
                e.Choosen = i;
                e.revenue = Utility.CalculateRevenue(myOriginalCase, i);
                initPopulation.Add(e);
            }
            revenue = initPopulation.OrderByDescending(x => x.revenue).FirstOrDefault().revenue;
            int count = 0;
            using (System.IO.StreamWriter file = File.AppendText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultProcessGen.txt"))
            {
                file.WriteLine(DateTime.Now.ToLongDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString());
                for (var lop = 0; lop < noloop; lop++)
                {
                    result.noGen++;
                    var Parents = SelectParents(myOriginalCase, initPopulation, percentCrossOver);
                    var Children = MakeChildren(myOriginalCase, Parents);
                    foreach (var child in Children)
                    {
                        initPopulation.Add(child);
                    }
                    initPopulation = Resize2(initPopulation, populationSize);
                    initPopulation = Mutate(myOriginalCase, initPopulation, mutationPercent);
                    double re = initPopulation.OrderByDescending(x => x.revenue).FirstOrDefault().revenue;
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
