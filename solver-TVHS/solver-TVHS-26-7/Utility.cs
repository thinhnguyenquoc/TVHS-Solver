using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace solver_TVHS_26_7
{
    public static class Utility
    {
        public static int GetFirstAvailableSlotInFrame(MyCase myCase, int[] Choosen, int frameId)
        {
            var frame = myCase.Frames.Where(x => x.Id == frameId).FirstOrDefault();
            if (frame == null)
                return -1;
            else
            {
                for (int i = frame.Start; i <= frame.End; i++)
                {
                    if (Choosen[i - 1] == -1)
                    {
                        return i - 1;
                    }
                }
                return -1;
            }
        }

        public static bool CheckAssignableProgram(MyCase myCase, int[] Choosen, int startAv, MyProgram item)
        {
            // out bound of array time
            if (startAv + item.Duration - 1 >= myCase.Times.Count)
            {
                return false;
            }
            else
            {
                // check space which has enough free space to assign program
                for (int m = startAv; m < startAv + item.Duration; m++)
                {
                    if (Choosen[m] != -1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool CheckTooClose(MyCase myCase, int[] Choosen, int startAv, MyProgram item)
        {
            // check successor program
            for (int m = startAv; m < Math.Min(startAv + myCase.Delta, myCase.Times.Count); m++)
            {
                if (Choosen[m] == item.Id)
                {
                    return false;
                }
            }

            // check previous program
            for (int m = Math.Max(startAv - myCase.Delta + 1, 0); m <= startAv; m++)
            {
                if (Choosen[m] == item.Id)
                {
                    return false;
                }
            }
            return true;
        }

        public static void AssignProgramToSche(MyCase myCase, int[] Choosen, int startAv, MyProgram item)
        {
            ////assign program to frame
            for (int j = 0; j < item.Duration; j++)
            {
                Choosen[startAv] = item.Id;
                startAv++;
            }
            //// decrease the maximum show time of this program
            myCase.Programs.Where(x => x.Id == item.Id).FirstOrDefault().MaxShowTime--;
            myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == item.Id && y.BelongTo == 1).FirstOrDefault().GroupId).FirstOrDefault().TotalTime -= item.Duration;
            //// update frame unoccupate
            for (int i = 0; i < myCase.Frames.Count; i++)
            {
                var unocc = 0;
                for (int j = myCase.Frames[i].Start; j <= myCase.Frames[i].End; j++)
                {
                    if (Choosen[j - 1] == -1)
                    {
                        unocc += 1;
                    }
                }
                myCase.Frames[i].Unoccupate = unocc;
            }
        }

        public static void ShiftRight(MyCase myCase, int[] Choosen, int k, int FrameId, List<int> Unoccupate = null)
        {
            int start = -1;
            for (int i = myCase.Frames[FrameId].Start - 1; i < myCase.Frames[FrameId].End; i++)
            {
                if (Choosen[i] == -1)
                {
                    start = i;
                    break;
                }
            }
            for (int i = start - 1; i >= myCase.Frames[FrameId].Start - 1; i--)
            {
                Choosen[i + k] = Choosen[i];
            }
            for (int i = myCase.Frames[FrameId].Start - 1; i < myCase.Frames[FrameId].Start - 1 + k; i++)
            {
                Choosen[i] = -1;
            }
        }

        public static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public static List<MyProgram> RandomProgramList(List<MyProgram> OriginalPrograms)
        {
            List<MyProgram> Programs = Utility.Clone<List<MyProgram>>(OriginalPrograms);
            Random a = new Random();            
            List<MyProgram> result = new List<MyProgram>();
            while (Programs.Count()!= 0)
            {
                int index = a.Next(0, Programs.Count() - 1);
                result.Add(Programs.ElementAt(index));
                Programs.RemoveAt(index);
            }
            return result;
        }

        public static double CalculateRevenue(MyCase myCase, int[] Choosen)
        {
            List<TempResult> results = new List<TempResult>();
            var resultIds = Choosen.Distinct().Where(x => x != -1).ToList();
            foreach (var id in resultIds)
            {
                var pr = new TempResult();
                pr.proId = id;
                pr.numShow = Choosen.Where(x => x == id).ToList().Count / myCase.Programs.Where(x => x.Id == id).FirstOrDefault().Duration;
                results.Add(pr);
            }
            double revenue = 0;
            foreach (var i in results)
            {
                revenue += myCase.Programs.Where(x => x.Id == i.proId).FirstOrDefault().RevenuePerTime * i.numShow * myCase.Theta1 + i.numShow * 1000000 * myCase.Theta2;
            }
            /*List<MySchedule> sche = GetSchedule(myCase, Choosen);
            string str = "";
            foreach (var pr in sche)
            {
                str += pr.Program.Id.ToString() + ", " + pr.Program.Duration + ", " + pr.Start.ToString() + " ; ";
            }
            Debug.WriteLine(str);*/
            return revenue;
        }

        public static List<MySchedule> GetSchedule(MyCase myCase, int[] Choosen)
        {
            List<MySchedule> sche = new List<MySchedule>();
            int anker = -1;
            for (int j = 0; j < myCase.Times.Count; j++)
            {
                if (Choosen[j] != -1 && anker != Choosen[j])
                {
                    anker = Choosen[j];
                    MySchedule p = new MySchedule();
                    p.Program = myCase.Programs.Where(x => x.Id == anker).FirstOrDefault();
                    p.Start = j;
                    sche.Add(p);
                }
            }
            return sche;
        }

    }
}
