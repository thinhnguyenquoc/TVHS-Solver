using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace solver_TVHS_26_7
{
    public class Heuristic
    {
        public MyStatictis strategy1(MyCase input, string filename)
        {
            MyCase myCase = Utility.Clone<MyCase>(input);
            #region init array
            int[] Choosen = new int[myCase.Times.Count];
            for (int i = 0; i < myCase.Times.Count; i++)
            {
                Choosen[i] = -1;
            }
            #endregion
            // check ability of assignment
            var watch = System.Diagnostics.Stopwatch.StartNew();
            bool change = false;
            #region assign program to frame
            
            while (true)
            {
                //find program which has max eff and still available
                var list1 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
                foreach (var item in list1)
                {
                    var gr = myCase.Groups.Where(x => x.Id == item.GroupId).FirstOrDefault();
                    #region group still has cota
                    if (gr.TotalTime >= item.Duration)
                    {
                        foreach (var frame in item.FrameList)
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
            watch.Stop();
            var elapsedH1 = watch.ElapsedMilliseconds;
            MyStatictis result = new MyStatictis();
            result.Choosen = Choosen;
            result.Elapsed = elapsedH1;
            result.Revenue = Utility.CalculateRevenue(input, Choosen);
            return result;
        }

        public MyStatictis strategy2(MyCase input, string filename)
        {
            MyCase myCase = Utility.Clone<MyCase>(input);
            #region init array
            int[] Choosen = new int[myCase.Times.Count];
            for (int i = 0; i < myCase.Times.Count; i++)
            {
                Choosen[i] = -1;
            }
            #endregion
            // check ability of assignment
            bool change = false;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            #region assign program to frame            
            while (true)
            {
                //find program which has max eff and still available
                var list1 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
                foreach (var item in list1)
                {
                    var gr = myCase.Groups.Where(x => x.Id == item.GroupId).FirstOrDefault();
                    #region group still has cota
                    if (gr.TotalTime >= item.Duration)
                    {
                        var FrameIdList = item.FrameList.Select(x => x.Id).ToList();
                        Utility.UpdateUnOccupiedFrameTime(myCase, Choosen);
                        //order frame by unoccupied slot
                        var orderFrameList = Utility.OrderOccupiedFrame(myCase, Choosen, FrameIdList);
                        foreach (var frame in orderFrameList)
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

            watch.Stop();
            var elapsedH1 = watch.ElapsedMilliseconds;
            MyStatictis result = new MyStatictis();
            result.Choosen = Choosen;
            result.Elapsed = elapsedH1;
            result.Revenue = Utility.CalculateRevenue(input, Choosen);
            return result;

        }

        public MyStatictis strategy3(MyCase input, string filename)
        {
            MyCase myCase = Utility.Clone<MyCase>(input);
            #region init array
            int[] Choosen = new int[myCase.Times.Count];
            for (int i = 0; i < myCase.Times.Count; i++)
            {
                Choosen[i] = -1;
            }
            #endregion
            // check ability of assignment
            var watch = System.Diagnostics.Stopwatch.StartNew();
            bool change = false;
            #region assign program to frame
            while (true)
            {
                //find program which has max eff and still available
                var list1 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.RevenuePerShow).ToList();
                foreach (var item in list1)
                {
                    var gr = myCase.Groups.Where(x => x.Id == item.GroupId).FirstOrDefault();
                    #region group still has cota
                    if (gr.TotalTime >= item.Duration)
                    {
                        foreach (var frame in item.FrameList)
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
                var list2 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.RevenuePerShow).ToList();
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

            watch.Stop();
            var elapsedH1 = watch.ElapsedMilliseconds;
            MyStatictis result = new MyStatictis();
            result.Choosen = Choosen;
            result.Elapsed = elapsedH1;
            result.Revenue = Utility.CalculateRevenue(input, Choosen);
            return result;
        }

        public MyStatictis strategy4(MyCase input, string filename)
        {
            MyCase myCase = Utility.Clone<MyCase>(input);
            #region init array
            int[] Choosen = new int[myCase.Times.Count];
            for (int i = 0; i < myCase.Times.Count; i++)
            {
                Choosen[i] = -1;
            }
            #endregion
            // check ability of assignment
            bool change = false;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            #region assign program to frame
            while (true)
            {
                //find program which has max eff and still available
                var list1 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.RevenuePerShow).ToList();
                foreach (var item in list1)
                {
                    var gr = myCase.Groups.Where(x => x.Id == item.GroupId).FirstOrDefault();
                    #region group still has cota
                    if (gr.TotalTime >= item.Duration)
                    {
                        var FrameIdList = item.FrameList.Select(x => x.Id).ToList();
                        Utility.UpdateUnOccupiedFrameTime(myCase, Choosen);
                        var orderFrameList = Utility.OrderOccupiedFrame(myCase, Choosen, FrameIdList);
                        foreach (var frame in orderFrameList)
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
                var list2 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.RevenuePerShow).ToList();
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

            watch.Stop();
            var elapsedH1 = watch.ElapsedMilliseconds;
            MyStatictis result = new MyStatictis();
            result.Choosen = Choosen;
            result.Elapsed = elapsedH1;
            result.Revenue = Utility.CalculateRevenue(input, Choosen);
            return result;
        }

    }
}
