using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace solver_TVHS_26_7
{
    public class Genetic
    {
        public double Solve(MyCase myOriginalCase)
        {
            double revenue = 0;
            var initPopulation = CreateInitPopulation(myOriginalCase, 50);
            revenue = Utility.CalculateRevenue(myOriginalCase, EvaluateAndSelect(myOriginalCase, initPopulation, 1).FirstOrDefault());
            Debug.WriteLine(revenue);
            for (var lop = 0; lop < 20; lop++)
            {             
                var Parents = EvaluateAndSelect(myOriginalCase, initPopulation, 20);
                var Children = MakeChildren(myOriginalCase, initPopulation);
                // add children to population
                initPopulation = initPopulation.Concat(Children).ToList();
                // resize of population
                initPopulation = EvaluateAndSelect(myOriginalCase, Children, 50);
                double re = Utility.CalculateRevenue(myOriginalCase, initPopulation.FirstOrDefault());
                if(re > revenue)
                    revenue = re;
                Debug.WriteLine(revenue);
                Debug.WriteLine(re);
            }
            Debug.WriteLine(revenue);
            return revenue;
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
                        var gr = myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == item.Id && y.BelongTo==1).FirstOrDefault().GroupId).FirstOrDefault();
                        #region group still has cota
                        if (gr.TotalTime >= item.Duration)
                        {
                            var FrameIdList = myCase.Allocates.Where(x => x.ProgramId == item.Id && x.Assignable == 1).Select(x => x.FrameId).ToList();
                            foreach (var frameId in FrameIdList)
                            {
                                int startAv = Utility.GetFirstAvailableSlotInFrame(myCase, Choosen, frameId);
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
                    List<int> Unoccupate = new List<int>();
                    for (int i = 0; i < myCase.Frames.Count - 1; i++)
                    {
                        Unoccupate.Add(myCase.Frames[i].Unoccupate + myCase.Frames[i + 1].Unoccupate);
                    }
                    var list2 = Utility.RandomProgramList(myCase.Programs.Where(x => x.MaxShowTime > 0).ToList());
                    foreach (var pro in list2)
                    {
                        var gr = myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == pro.Id&& y.BelongTo==1).FirstOrDefault().GroupId).FirstOrDefault();
                        if (gr.TotalTime >= pro.Duration)
                        {
                            for (int i = 0; i < Unoccupate.Count; i++)
                            {
                                if (pro.Duration <= Unoccupate[i])
                                {
                                    //check allowed frames
                                    var FrameIdList = myCase.Allocates.Where(x => x.ProgramId == pro.Id && x.Assignable == 1).Select(x => x.FrameId).ToList();

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
            int mark = -1;
            List<MyProgram> parent1 = new List<MyProgram>();
            for (int i = 0; i < couple[0].Count(); i++)
            {
                if (couple[0][i] != -1 && couple[0][i] != mark)
                {
                    parent1.Add(myOriginalCase.Programs.Where(x=>x.Id==couple[0][i]).FirstOrDefault());
                    mark = couple[0][i];
                }
            }
            mark = -1;
            List<MyProgram> parent2 = new List<MyProgram>();
            for (int i = 0; i < couple[1].Count(); i++)
            {
                if (couple[1][i] != -1 && couple[1][i] != mark)
                {
                    parent2.Add(myOriginalCase.Programs.Where(x => x.Id == couple[1][i]).FirstOrDefault());
                    mark = couple[1][i];
                }
            }

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
            if (MissingPro.Count() > 0)
            {
                List<int> doublePro = new List<int>();
                var ids = child.Select(x => x.Id).Distinct().ToList();
                foreach (var id in ids)
                {
                    if (child.Where(x => x.Id == id).Count() > 1)
                    {
                        doublePro.Add(id);
                    }
                }
                if (MissingPro.Count() > doublePro.Count)
                {
                    return null;
                }
                foreach (var item in MissingPro)
                {
                    for(int k=0; k <child.Count; k++){
                        if (child[k].Id == doublePro.FirstOrDefault())
                        {
                            child[k] = item;
                            doublePro.RemoveAt(0);
                            break;
                        }
                    }
                   
                }
            }
            
           
            foreach (var item in list1)
            {
                var gr = myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == item.Id && y.BelongTo == 1).FirstOrDefault().GroupId).FirstOrDefault();
                #region group still has cota
                if (gr.TotalTime >= item.Duration)
                {
                    var FrameIdList = myCase.Allocates.Where(x => x.ProgramId == item.Id && x.Assignable == 1).Select(x => x.FrameId).ToList();
                    foreach (var frameId in FrameIdList)
                    {
                        int startAv = Utility.GetFirstAvailableSlotInFrame(myCase, Choosen, frameId);
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
                List<int> Unoccupate = new List<int>();
                for (int i = 0; i < myCase.Frames.Count - 1; i++)
                {
                    Unoccupate.Add(myCase.Frames[i].Unoccupate + myCase.Frames[i + 1].Unoccupate);
                }
                var list2 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x=>x.Efficiency).ToList();
                foreach (var pro in list2)
                {
                    var gr = myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == pro.Id && y.BelongTo==1).FirstOrDefault().GroupId).FirstOrDefault();
                    if (gr.TotalTime >= pro.Duration)
                    {
                        for (int i = 0; i < Unoccupate.Count; i++)
                        {
                            if (pro.Duration <= Unoccupate[i])
                            {
                                //check allowed frames
                                var FrameIdList = myCase.Allocates.Where(x => x.ProgramId == pro.Id && x.Assignable == 1).Select(x => x.FrameId).ToList();

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

                }
                if (change)
                    change = false;
                else
                    break;
            }
            #endregion
            foreach (var pro in myCase.Programs)
            {
                if (Choosen.Where(x => x == pro.Id).Count() ==0)
                {
                    return null;
                }
            }
            return Choosen;
        }

        private class EvaluatePopulation
        {
            public int[] resident { get; set; }
            public double revenue { get; set; }
        }
    }
}
