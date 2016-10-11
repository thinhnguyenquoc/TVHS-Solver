using Microsoft.SolverFoundation.Services;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace solver_TVHS_26_7
{
    public class Program
    {
        private static List<List<MyResult>> AllPath = new List<List<MyResult>>();
        static void Main(string[] args)
        {           
            #region input           
            double solverResult = 7000000000;
            List<string> fileList = new List<string>(){
                /*@"..\..\..\..\TVHS_Data_test\3-8_9-8_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\7-7_12-7_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\7-9_13-9_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\10-8_16-8_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\13-7_19-7_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\14-9_20-9_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\17-8_23-8_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\20-7_26-7_2015\F0-F10.xlsx",*/
                @"..\..\..\..\TVHS_Data_test\21-9_27-9_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\8-1_9-1_2015\F0-F10.xlsx"
            };
           
            #endregion
           
            //var olveResult = MySolver(input, fileList.FirstOrDefault());  
            foreach (var filename in fileList)
            {
                MyCase input = InitData(filename);
                MyCase data = Utility.Clone<MyCase>(input);
                MyCase data2 = Utility.Clone<MyCase>(input);
                MyCase data3 = Utility.Clone<MyCase>(input);
                MyCase data4 = Utility.Clone<MyCase>(input);
                #region solve
                #region call solver
                /*var solver = new Solver();
                var solveResult = solver.Solve(data3, filename);*/
                #endregion
                #region calculate heuristic
                //var solverR = FindFeasibleSFS(data3, filename,ref solverResult);// 0   -2   -3   0   0   0
                var H = new Heuristic();
                //var heuResult = H.H1(data, filename); // Pass
                var heuResult = H.H2(data, filename); 
                int[] validate = Validate.ValidateResult(input, heuResult);
                if (validate.Sum() == 0)
                {
                    Debug.WriteLine("Pass");
                }
                else
                {
                    foreach (var c in validate)
                    {
                        Debug.Write("   "+c.ToString());
                    }
                    Debug.WriteLine("");
                }
                
                //var hueristicResult2 = Hueristic2(data2, filename);

                //var ratioFeasible = solverR / solverResult;
                //var ratioHue = hueristicResult / solverResult;
                //var ratioHue2 = hueristicResult2 / solverResult;
                #region call genetic
                //Genetic gen = new Genetic();
                //var initPopulation = gen.Solve(data4);
                #endregion

                //Debug.WriteLine("solver:" + solverResult);
                //Debug.WriteLine("solverR:" + solverR + "  ratio:" + ratioFeasible);
                //Debug.WriteLine("hueristic:" + hueristicResult + "  ratio:" + ratioHue);
                //Debug.WriteLine("hueristic2:" + hueristicResult2 + "  ratio2:" + ratioHue2);
                //Debug.WriteLine("gen:" + initPopulation + "  ratio2:" + initPopulation / solverResult);
                #endregion                
                
                #endregion
            }
            
        }

        #region find feasible solution from solver
        static int[] FindFeasibleSFS(MyCase myCase, string filename,ref double solverResult)
        {
            List<MyProgram> proList = new List<MyProgram>();
            string solverUrl = filename.Split(new string[]{".xlsx"}, StringSplitOptions.None).FirstOrDefault() + "_resultBS.txt";
            string[] lines = System.IO.File.ReadAllLines(solverUrl);
            foreach (string line in lines)
            {
                if (line.Contains("RBS"))
                {
                    solverResult = Convert.ToDouble(line.Split(new string[]{"RBS"}, StringSplitOptions.None)[1]);
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
                var gr = myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == item.Id && y.BelongTo == 1).FirstOrDefault().GroupId).FirstOrDefault();
                #region group still has cota
                if (gr.TotalTime >= item.Duration)
                {
                    var FrameIdList = myCase.Allocates.Where(x => x.ProgramId == item.Id && x.Assignable == 1).Select(x => x.FrameId).ToList();
                    int FrameID = FindTimeFrame(item.Start, myCase).Id;
                    if (FrameIdList.Contains(FrameID))
                    {
                        if (CheckAvailableSlot(Choosen, item))
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
                List<int> Unoccupate = new List<int>();
                for (int i = 0; i < myCase.Frames.Count - 1; i++)
                {
                    Unoccupate.Add(myCase.Frames[i].Unoccupate + myCase.Frames[i + 1].Unoccupate);
                }
                var list2 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
                foreach (var pro in list2)
                {
                    var gr = myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == pro.Id && y.BelongTo == 1).FirstOrDefault().GroupId).FirstOrDefault();
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
            return Choosen;        
        }
        #endregion
       
        #region init test
        private static MyCase InitData(string filename)
        {
            IWorkbook wb = null;
            MyCase data = new MyCase();
            data.Theta1 = 10;
            data.Theta2 = 1;
            data.Delta = 60;
            data.Alpha = 0.6;
            data.Allocates = new List<MyAssignment>();
            data.BTGroups = new List<MyBelongToGroup>();
            data.Programs = new List<MyProgram>();
            #region create time frame
            using (FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                wb = new XSSFWorkbook(file);
            }
            data.Frames = GetTimeFrame(Convert.ToInt32(filename.Split('F')[1].Split('-').FirstOrDefault()), Convert.ToInt32(filename.Split('F')[2].Split('.').FirstOrDefault()));
            #endregion
            data.Groups = GetGroup();
            data.Times = GetTime(data.Frames.LastOrDefault().End);
            #region read data to object
            ISheet sheet = wb.GetSheetAt(0);
            List<MyProgram> list = new List<MyProgram>();
            for (int row = 0; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    if (sheet.GetRow(row).GetCell(5) != null)
                    {
                        MyProgram pr = new MyProgram();
                        pr.Name = sheet.GetRow(row).GetCell(2).StringCellValue;
                        pr.Live = pr.Name.ToLower().Contains("live")? true: false;                       
                        pr.Duration = Convert.ToInt32(sheet.GetRow(row).GetCell(5).NumericCellValue);
                        pr.Efficiency = Convert.ToDouble(sheet.GetRow(row).GetCell(6) != null ? sheet.GetRow(row).GetCell(6).NumericCellValue : 0);
                        pr.Group = sheet.GetRow(row).GetCell(7) != null ? sheet.GetRow(row).GetCell(7).StringCellValue : "";
                        if (pr.Group == "E")
                            pr.Group = "D";
                        if (!string.IsNullOrEmpty(pr.Group))
                        {
                            list.Add(pr);
                        }
                    }
                }
            }
            #endregion
            #region create program list
            int programIndex = 0;
            foreach (var item in list)
            {
                if (data.Programs.Where(x => x.Name == item.Name).FirstOrDefault() == null)
                {
                    item.Id = programIndex;
                    data.Programs.Add(item);
                    programIndex++;
                }
            }
            #endregion
            #region calculate group duration
            var totaltime = data.Frames.LastOrDefault().End;
            var staticTime = data.Programs.Sum(x => x.Duration);
            var totalGroup = data.Programs.Select(x => x.Group).Distinct().Count();
            var dynamicTime = totaltime - staticTime;
            double[] ratio = GetTimeRatio();
            double totalRatio = 0.0;
            for (int j = 0; j < totalGroup; j++)
            {
                totalRatio += ratio[j];
            }
            int sum = 0;

            for (int k = totalGroup - 1; k > 0; k--)
            {
                data.Groups[k].TotalTime = Convert.ToInt32(data.Programs.Where(x => x.Group == data.Groups[k].Name).Sum(t => t.Duration) + dynamicTime * ratio[k] / totalRatio);
                sum += data.Groups[k].TotalTime;
                data.Groups[k].MaxShow = Convert.ToInt32(Math.Ceiling(data.Groups[k].TotalTime / (data.Programs.Where(x => x.Group == data.Groups[k].Name).Sum(t => t.Duration) * data.Alpha)));
            }
            data.Groups[0].TotalTime = totaltime - sum;
            data.Groups[0].MaxShow = Convert.ToInt32(Math.Ceiling(data.Groups[0].TotalTime / (data.Programs.Where(x => x.Group == data.Groups[0].Name).Sum(t => t.Duration) * data.Alpha)));
            #endregion
            #region calculate max show time
            foreach (var pr in data.Programs)
            {
                foreach (var gr in data.Groups)
                {
                    if (pr.Group == gr.Name)
                    {
                        pr.MaxShowTime = gr.MaxShow;
                    }
                }
            }
            #endregion
            #region assign program to time frame
            foreach (var fr in data.Frames)
            {
                foreach (var pr in data.Programs)
                {
                    if (fr.Live)
                    {
                        data.Allocates.Add(new MyAssignment()
                        {
                            FrameId = fr.Id,
                            ProgramId = pr.Id,
                            Assignable = 1
                        });
                    }
                    // normal frame
                    else
                    {
                        if (pr.Live)
                        {
                            data.Allocates.Add(new MyAssignment()
                            {
                                FrameId = fr.Id,
                                ProgramId = pr.Id,
                                Assignable = 0
                            });
                        }
                        else
                        {
                            data.Allocates.Add(new MyAssignment()
                            {
                                FrameId = fr.Id,
                                ProgramId = pr.Id,
                                Assignable = 1
                            });
                        }
                    }

                }
            }
            #endregion
            #region program belong to group
            foreach (var gr in data.Groups)
            {
                foreach (var pr in data.Programs)
                {
                    if (pr.Group == gr.Name)
                    {
                        data.BTGroups.Add(new MyBelongToGroup()
                        {
                            GroupId = gr.Id,
                            ProgramId = pr.Id,
                            BelongTo = 1
                        });
                    }
                    else
                    {
                        data.BTGroups.Add(new MyBelongToGroup()
                        {
                            GroupId = gr.Id,
                            ProgramId = pr.Id,
                            BelongTo = 0
                        });
                    }
                }
            }
            #endregion
            #region generate test
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename.Split(new string[]{".xlsx"}, StringSplitOptions.None).FirstOrDefault() + "_testcase.txt"))
            {
                //total time 
                file.WriteLine("T\t" + totaltime);
                //delay repeat program show
                file.WriteLine("D\t" + data.Delta);
                //program
                foreach (var pr in data.Programs)
                {
                    file.WriteLine("P\t" + pr.Id + "\t" + pr.Duration + "\t" + pr.Efficiency + "\t" + pr.MaxShowTime + "\t" + pr.Name);
                }
                //time frame
                foreach (var tf in data.Frames)
                {
                    file.WriteLine("F\t" + tf.Id + "\t" + tf.Start + "\t" + tf.End);
                }
                // group
                foreach (var gr in data.Groups)
                {
                    file.WriteLine("G\t" + gr.Id + "\t" + gr.TotalTime + "\t" + gr.Name);
                }
                // assign
                foreach (var ass in data.Allocates)
                {
                    file.WriteLine("A\t" + ass.FrameId + "\t" + ass.ProgramId + "\t" + ass.Assignable);
                }
                // belong to
                foreach (var blt in data.BTGroups)
                {
                    file.WriteLine("B\t" + blt.GroupId + "\t" + blt.ProgramId + "\t" + blt.BelongTo);
                }              
            }
            #endregion
            return data;
        }
        #endregion

        private static List<MyTime> GetTime(int time)
        {
            var timeList = new List<MyTime>();
            for (int i = 0; i < time; i++)
            {
                timeList.Add(new MyTime()
                {
                    Id = i,
                    Time = i + 1
                });
            }
            return timeList;
        }       
       
        static bool CheckAvailableSlot(int[] Choosen, MyProgram item){ 
            for (int i = item.Start; i < item.Start + item.Duration; i++)
            {
                if (Choosen[i] != -1)
                {
                    return false;
                }
            }
            return true;           
        }
             
        
        static MyTimeFrame FindTimeFrame(int start, MyCase myCase)
        {
            return myCase.Frames.Where(x => x.Start - 1 <= start && x.End - 1 >= start).FirstOrDefault();
        }
        
        static double[] GetTimeRatio()
        {
            double[] ratio = new double[4];
            ratio[0] = 3;
            ratio[1] = 1.7;
            ratio[2] = 0.7;
            ratio[3] = 2;
            return ratio;
        }

        static List<MyTimeFrame> GetTimeFrame(int start, int end)
        {
            List<MyTimeFrame> baseList = new List<MyTimeFrame>();
            #region init
            baseList.Add(new MyTimeFrame() { Id = 0, Duration = 60, Live = false });
            baseList.Add(new MyTimeFrame() { Id = 1, Duration = 60, Live = true });
            baseList.Add(new MyTimeFrame() { Id = 2, Duration = 120, Live = false });
            baseList.Add(new MyTimeFrame() { Id = 3, Duration = 60, Live = true });
            baseList.Add(new MyTimeFrame() { Id = 4, Duration = 240, Live = false });
            baseList.Add(new MyTimeFrame() { Id = 5, Duration = 60, Live = true });
            baseList.Add(new MyTimeFrame() { Id = 6, Duration = 240, Live = false });
            baseList.Add(new MyTimeFrame() { Id = 7, Duration = 60, Live = true });
            baseList.Add(new MyTimeFrame() { Id = 8, Duration = 60, Live = false });
            baseList.Add(new MyTimeFrame() { Id = 9, Duration = 60, Live = true });
            baseList.Add(new MyTimeFrame() { Id = 10, Duration = 90, Live = false });
            #endregion
            List<MyTimeFrame> resultList = new List<MyTimeFrame>();
            int time = 1;
            for (int i = 0; i <= end - start; i++)
            {
                var frame = baseList.Where(x => x.Id == start + i).FirstOrDefault();
                if (frame != null)
                {
                    frame.Id = i;
                    frame.Start = time;
                    frame.End = time + frame.Duration - 1;                  
                    frame.Unoccupate = frame.Duration;
                    resultList.Add(frame);
                    time = frame.End + 1;
                }
            }
            return resultList;
        }

        static List<MyGroup> GetGroup()
        {
            List<MyGroup> listGroup = new List<MyGroup>();
            listGroup.Add(new MyGroup() { Id = 0, Name = "A" });
            listGroup.Add(new MyGroup() { Id = 1, Name = "B" });
            listGroup.Add(new MyGroup() { Id = 2, Name = "C" });
            listGroup.Add(new MyGroup() { Id = 3, Name = "D" });
            return listGroup;
        }

        
        }

    #region define object
    public class MySchedule
    {
        public MyProgram Program { get; set; }
        public int Start { get; set; }
    }
    public class TempResult
    {
        public int proId { get; set; }
        public int numShow { get; set; }
    }
    public class MyResult
    {
        public int proramId { get; set; }
        public int time { get; set; }
        public int duration { get; set; }
        public double decision { get; set; }
        public double eff { get; set; }
        public int maxShowTime { get; set; }
    }
    public class MyCase 
    {
        public List<MyProgram> Programs { get; set; }
        public List<MyTimeFrame> Frames { get; set; }
        public List<MyAssignment> Allocates { get; set; }
        public List<MyTime> Times { get; set; }
        public List<MyGroup> Groups { get; set; }
        public List<MyBelongToGroup> BTGroups { get; set; }
        public int Delta { get; set; }
        public double Alpha { get; set; }
        public double Theta1 { get; set; }
        public double Theta2 { get; set; }

    }
    public class MyProgram
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public double Efficiency { get; set; }
        public int MaxShowTime { get; set; }
        public bool Live { get; set; }
        public string Group { get; set; }
        public double RevenuePerTime
        {
            get
            {
                return this.Duration * this.Efficiency;
            }
        }
        public double Probability { get; set; }
        public int Start { get; set; }
        public MyProgram()
        {

        }
        public MyProgram(int Id, string Name, int Duration, int Efficiency)
        {
            this.Id = Id;
            this.Name = Name;
            this.Duration = Duration;
            this.Efficiency = Efficiency;
        }
    }
    public class MyTimeFrame
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public bool Live { get; set; }
        public int Duration { get; set; }
        public int Unoccupate { get; set; }
        public MyTimeFrame()
        {

        }
        public MyTimeFrame(int Id, string Name, int Start, int End)
        {
            this.Id = Id;
            this.Name = Name;
            this.Start = Start;
            this.End = End;
        }
    }
    public class MyTime
    {
        public int Id { get; set; }
        public int Time { get; set; }
    }
    public class MyGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TotalTime { get; set; }
        public int MaxShow { get; set; }
    }
    public class MyBelongToGroup
    {
        public int GroupId { get; set; }
        public int ProgramId { get; set; }
        public int BelongTo { get; set; }
    }
    public class MyAssignment
    {
        public int FrameId { get; set; }
        public int ProgramId { get; set; }
        public int Assignable { get; set; }
    }
    #endregion
}
