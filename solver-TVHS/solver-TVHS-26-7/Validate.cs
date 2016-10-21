using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace solver_TVHS_26_7
{
    public static class Validate
    {
       
        public static int[] ValidateResult(MyCase myCase, int[] Choosen)
        {
            int[] result = new int[6];
            if (!ValidateTimeFrame(myCase, Choosen))
                result[0] = -1;
            else
                result[0] = 0;

            if (!ValidateMinShowTime(myCase, Choosen))
                result[1] = -2;
            else
                result[1] = 0;

            if (!ValidateProgramLength(myCase, Choosen))
                result[2] = -3;
            else
                result[2] = 0;

            if (!ValidateMaxShowTime(myCase, Choosen))
                result[3] = -4;
            else
                result[3] = 0;

            if (!ValidateTooClose(myCase, Choosen))
                result[4] = -5;
            else
                result[4] = 0;

            if (!ValidateGrouptime(myCase, Choosen))
                result[5] = -6;
            else
                result[5] = 0;
            return result;
        }
        //H1
        public static bool ValidateTimeFrame(MyCase myCase, int[] Choosen)
        {
            foreach (var pro in myCase.Programs)
            {
                var startPoints = GetShowTime(Choosen, pro.Id);
                foreach (var point in startPoints)
                {
                    if (!CheckValidFrame(point + 1, myCase, pro.Id))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        //H2
        public static bool ValidateMinShowTime(MyCase myCase, int[] Choosen)
        {
            foreach (var pro in myCase.Programs)
            {
                if (Choosen.Where(x => x == pro.Id).Count() <= 0)
                    return false;
            }
            return true;
        }
        //H3
        public static bool ValidateProgramLength(MyCase myCase, int[] Choosen)
        {
            foreach(var pro in myCase.Programs)
            {
                int mark = 0;
                for (int i = 0; i < Choosen.Count(); i++)
                {
                    if (Choosen[i] == pro.Id)
                    {
                        mark++;
                    }
                    if (Choosen[i] != -1 && Choosen[i] != pro.Id)
                    {
                        if (mark != 0)
                        {
                            if (mark != pro.Duration)
                                return false;
                            mark = 0;
                        }
                    }
                }
            }
            return true;
        }
        //H4
        public static bool ValidateMaxShowTime(MyCase myCase, int[] Choosen)
        {
            foreach (var pro in myCase.Programs)
            {
                int count = 0;
                int mark = -1;
                for (int i = 0; i < Choosen.Count(); i++)
                {
                    if (Choosen[i] == pro.Id && pro.Id != mark)
                    {
                        count++;
                        mark = pro.Id;
                    }
                    if (Choosen[i] != -1 && Choosen[i] != pro.Id)
                    {
                        mark = Choosen[i];
                    }
                }
                if (count > pro.MaxShowTime)
                {
                    return false;
                }
            }
            return true;
        }
        //H5
        public static bool ValidateTooClose(MyCase myCase, int[] Choosen)
        {
            foreach (var pro in myCase.Programs)
            {
                List<List<int>> p = new List<List<int>>();
                List<int> o = new List<int>();
                int mark = -1;
                for (int i = 0; i < Choosen.Count(); i++)
                {
                    if (Choosen[i] == pro.Id && pro.Id != mark)
                    {
                        mark = pro.Id;
                        o.Add(i);
                    }
                    if (Choosen[i] != -1 && Choosen[i] != pro.Id)
                    {
                        if (mark == pro.Id)
                        {
                            o.Add(i-1);
                            p.Add(o);
                            o = new List<int>();
                        }                      
                        mark = Choosen[i];
                    }
                }
                for (int k = 0; k < p.Count-1; k++)
                {
                    for (int l = p[k][1] + 1; l < p[k][0] + myCase.Delta; l++)
                    {
                        if (l < Choosen.Count())
                        {
                            if (Choosen[l] == pro.Id)
                                return false;
                        }
                    }
                }
                
            }
            return true;
        }
        //H6
        public static bool ValidateGrouptime(MyCase myCase, int[] Choosen)
        {
            foreach (var gr in myCase.Groups)
            {
                List<int> ProgramId = myCase.BTGroups.Where(x => x.GroupId == gr.Id && x.BelongTo == 1).Select(x => x.ProgramId).ToList();
                int TotalTime = Choosen.Where(x => ProgramId.Contains(x)).Count();
                if (TotalTime > gr.TotalTime)
                    return false;
            }
            return true;
        }


        private static List<int> GetShowTime(int[] Choosen, int proId)
        {
            List<int> result = new List<int>();
            int mark = -1;
            for (int i = 0; i < Choosen.Count(); i++)
            {
                if (Choosen[i] != -1 && Choosen[i] == proId && mark != proId)
                {
                    result.Add(i);
                    mark = proId;
                }
                if (Choosen[i] != -1 && Choosen[i] != mark)
                {
                    mark = Choosen[i];
                }
            }
            return result;
        }
        private static bool CheckValidFrame(int start, MyCase myCase, int proId)
        {
            int FrameId = myCase.Frames.Where(x => x.Start <= start && x.End >= start).FirstOrDefault().Id;
            if (myCase.Allocates.Where(x => x.FrameId == FrameId && x.ProgramId == proId && x.Assignable == 1).Count() > 0)
                return true;
            return false;
        }
    }
}
