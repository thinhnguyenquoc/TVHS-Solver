using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace solver_TVHS_26_7
{
    public class Genetic
    {
        public int[] Solve(MyCase myOriginalCase)
        {
            double revenue = 0;
            var initPopulation = CreateInitPopulation(myOriginalCase, 30);
            revenue = Utility.CalculateRevenue(myOriginalCase, EvaluateAndSelect(myOriginalCase, initPopulation, 1).FirstOrDefault());
            Debug.WriteLine(revenue);
            for (var lop = 0; lop < 20; lop++)
            {             
                var Parents = EvaluateAndSelect(myOriginalCase, initPopulation, 14);
                var Children = MakeChildren(myOriginalCase, initPopulation);
                // add children to population
                initPopulation = initPopulation.Concat(Children).ToList();
                // resize of population
                initPopulation = EvaluateAndSelect(myOriginalCase, Children, 30);
                double re = Utility.CalculateRevenue(myOriginalCase, initPopulation.FirstOrDefault());
                if(re > revenue)
                    revenue = re;
                Debug.WriteLine(revenue);
                Debug.WriteLine(re);
            }
            return EvaluateAndSelect(myOriginalCase, initPopulation, 1).FirstOrDefault();
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

        public List<int[]> MakeChildren(MyCase myOriginalCase, List<int[]> parents)
        {
            List<int[]> result = new List<int[]>();
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

            if (Utility.CheckOneShow(Choosen, myCase))
                return Choosen;
            else
                return null;
        }

        private class EvaluatePopulation
        {
            public int[] resident { get; set; }
            public double revenue { get; set; }
        }

    }
}
