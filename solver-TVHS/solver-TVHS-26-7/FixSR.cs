using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace solver_TVHS_26_7
{
    public class FixSR
    {
        public int[] FindFeasibleSFS(MyCase input, string filename, ref double solverResult)
        {
            MyCase myCase = Utility.Clone<MyCase>(input);
            List<MyProgram> proList = new List<MyProgram>();
            string solverUrl = filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultBS.txt";
            string[] lines = System.IO.File.ReadAllLines(solverUrl);
            foreach (string line in lines)
            {
                if (line.Contains("RBS"))
                {
                    solverResult = Convert.ToDouble(line.Split(new string[] { "RBS" }, StringSplitOptions.None)[1]);
                    break;
                }
                string[] paras = line.Split('\t');
                MyProgram pro = new MyProgram();
                pro.Id = Convert.ToInt32(paras[0]);
                pro.Start = Convert.ToInt32(paras[1]);
                pro.Duration = Convert.ToInt32(paras[2]);
                pro.MaxShowTime = Convert.ToInt32(paras[3]);
                pro.Efficiency = Convert.ToDouble(paras[4]);
                pro.Probability = Convert.ToDouble(paras[5]);
                pro.GroupId = myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == pro.Id && y.BelongTo == 1).FirstOrDefault().GroupId).FirstOrDefault().Id;
                pro.FrameList = myCase.Frames.Where(x => myCase.Allocates.Where(y => y.ProgramId == pro.Id && y.Assignable == 1).Select(z => z.FrameId).ToList().Contains(x.Id)).ToList();
           
                proList.Add(pro);
            }

            #region init array
            int[] Choosen = new int[myCase.Times.Count];
            for (int i = 0; i < myCase.Times.Count; i++)
            {
                Choosen[i] = -1;
            }
            #endregion
            proList = proList.OrderByDescending(x => x.Probability).ToList();
            foreach (var item in proList)
            {
                var gr = myCase.Groups.Where(x => x.Id == item.GroupId).FirstOrDefault();
                #region group still has cota
                if (gr.TotalTime >= item.Duration)
                {
                    var FrameIdList = myCase.Frames.Select(x => x.Id).ToList();
                    int FrameID = Utility.FindTimeFrame(item.Start, myCase).Id;
                    if (FrameIdList.Contains(FrameID))
                    {
                        if (Utility.CheckAvailableSlot(Choosen, item))
                        {
                            if (Utility.CheckTooClose(myCase, Choosen, item.Start, item))
                            {
                                Utility.AssignProgramToSche(myCase, Choosen, item.Start, item);
                            }
                        }
                    }
                }
                #endregion
            }

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
           
            #region calculate revenue
            return Choosen;
            #endregion
        }
    }
}
