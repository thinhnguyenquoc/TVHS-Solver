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
        public static int GetFirstAvailableSlotInFrame(MyCase myCase, int[] Choosen, MyTimeFrame frame)
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
            var nearest = -1;

            for (int i = startAv; i >=0 ; i--)
            {
                if (Choosen[i] == item.Id)
                {
                    nearest = i;
                    break;
                }
            }

            if(nearest != -1 && startAv-nearest + item.Duration <= myCase.Delta )               
                return false;
                  
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
            myCase.Groups.Where(x => x.Id == item.GroupId).FirstOrDefault().TotalTime -= item.Duration;       
        }

        public static void UpdateUnOccupiedFrameTime(MyCase myCase, int[] Choosen)
        {
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
            Random r = new Random();
            int[] a = new int[OriginalPrograms.Count];
            for (int i = 0; i < OriginalPrograms.Count; i++)
            {
                a[i] = i;
            }
            for (int i = 0; i < OriginalPrograms.Count; i++)
            {
                int j = r.Next(OriginalPrograms.Count - 1);
                int t = a[0];
                a[0] = a[j];
                a[j] = t;
            }     
            List<MyProgram> result = new List<MyProgram>();
            for (int i = 0; i < a.Count(); i++ )
            {
                result.Add(OriginalPrograms[a[i]]);
            }
            return result;
        }

        public static List<MyTimeFrame> RandomFrameList(List<MyTimeFrame> OriginalFrames)
        {
            Random r = new Random();
            int[] a = new int[OriginalFrames.Count];
            for (int i = 0; i < OriginalFrames.Count; i++)
            {
                a[i] = i;
            }
            for (int i = 0; i < OriginalFrames.Count; i++)
            {
                int j = r.Next(OriginalFrames.Count - 1);
                int t = a[0];
                a[0] = a[j];
                a[j] = t;
            }
            List<MyTimeFrame> result = new List<MyTimeFrame>();
            for (int i = 0; i < a.Count(); i++)
            {
                result.Add(OriginalFrames[a[i]]);
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
                revenue += myCase.Programs.Where(x => x.Id == i.proId).FirstOrDefault().RevenuePerShow * i.numShow * myCase.Theta1 + i.numShow * 1000000 * myCase.Theta2;
            }
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

        public static List<MyTimeFrame> OrderOccupiedFrame(MyCase myCase, int[] Choosen, List<int> frameList)
        {
            int[] Cache = new int[frameList.Count];
            for (int i = 0; i < frameList.Count; i++)
            {
                Cache[i] = myCase.Frames.Where(x=>x.Id == frameList[i]).FirstOrDefault().Unoccupate;
            }
            List<MyTimeFrame> result = new List<MyTimeFrame>();
            for (int i = 0; i < frameList.Count; i++)
            {
                int max = Cache.Max();
                for (int j = 0; j < frameList.Count; j++)
                {
                    if (max == -1)
                        break;
                    if (Cache[j] == max)
                    {
                        result.Add(myCase.Frames.Where(x => x.Id == frameList[j]).FirstOrDefault());
                        Cache[j] = -1;
                        break;
                    }
                }
            }
            return result;
        }

        public static List<MyTimeFrame> OrderMinimumPro(MyCase myCase, int[] Choosen, List<int> frameList)
        {
            int[] Cache = new int[frameList.Count];
            for (int i = 0; i < frameList.Count; i++)
            {
                Cache[i] = 0;
                int check = -1;
                for(int j = myCase.Frames.Where(x => x.Id == frameList[i]).FirstOrDefault().Start-1; j < myCase.Frames.Where(x => x.Id == frameList[i]).FirstOrDefault().End; j++)
                {
                    if (Choosen[j] != check)
                    {
                        Cache[i]++;
                        check = Choosen[j];
                    }
                }
                
            }
            List<MyTimeFrame> result = new List<MyTimeFrame>();
            for (int i = 0; i < frameList.Count; i++)
            {
                int max = Cache.Max();
                for (int j = 0; j < frameList.Count; j++)
                {
                    if (max == -1)
                        break;
                    if (Cache[j] == max)
                    {
                        result.Add(myCase.Frames.Where(x => x.Id == frameList[j]).FirstOrDefault());
                        Cache[j] = -1;
                        break;
                    }
                }
            }
            return result;
        }

        public static MyTimeFrame FindTimeFrame(int start, MyCase myCase)
        {
            return myCase.Frames.Where(x => x.Start - 1 <= start && x.End - 1 >= start).FirstOrDefault();
        }

        public static bool CheckAvailableSlot(int[] Choosen, MyProgram item)
        {
            for (int i = item.Start; i < item.Start + item.Duration; i++)
            {
                if (Choosen[i] != -1)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CheckOneShow(int[] Choosen, MyCase myCase)
        {
            foreach (var pro in myCase.Programs)
            {
                bool flag2 = false;
                for (int i = 0; i < Choosen.Count(); i++)
                {
                    if (Choosen[i] == pro.Id)
                    {
                        flag2 = true;
                        break;
                    }
                }
                if (flag2)
                    break;
                else
                    return false;
            }
            return true;
        }

        public static List<MyProgram> ConvertToProgram(MyCase myCase, int[] array)
        {
            int mark = -1;
            List<MyProgram> proList = new List<MyProgram>();
            for (int i = 0; i < array.Count(); i++)
            {
                if (array[i] != -1 && array[i] != mark)
                {
                    proList.Add(myCase.Programs.Where(x => x.Id == array[i]).FirstOrDefault());
                    mark = array[i];
                }
            }
            return proList;
        } 
    }
}
