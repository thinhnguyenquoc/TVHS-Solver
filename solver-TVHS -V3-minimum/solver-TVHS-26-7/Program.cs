﻿using Microsoft.SolverFoundation.Services;
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
            double elapsedSolver = 0;
            List<string> fileList = new List<string>(){
                @"..\..\..\..\TVHS_Data_test\3-8_9-8_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\7-7_12-7_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\7-9_13-9_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\10-8_16-8_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\13-7_19-7_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\14-9_20-9_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\17-8_23-8_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\20-7_26-7_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\21-9_27-9_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\24-8_30-8_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\27-7_2-8_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\31-8_6-9_2015\F0-F10.xlsx",
            };

            #endregion
            #region header           
           
            #endregion
            foreach (var filename in fileList)
            {
                MyCase input = InitData(filename);
                #region call solver
                //var solver = new Solver();
                //var solveResult = solver.Solve(input, filename);
                #endregion

                #region read solver result 

               string solverUrl = filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultminiBS.txt";
               string[] lines = System.IO.File.ReadAllLines(solverUrl);
               foreach (string line in lines)
               {
                   if (line.Contains("RBS"))
                   {
                        var a = line.Split(new string[] { "\t" }, StringSplitOptions.None);                       
                        solverResult = Convert.ToDouble(a[1]);                       
                   }
                   if (line.Contains("total time of"))
                   {
                       var a = line.Split(new string[] { "total time of" }, StringSplitOptions.None)[1];
                       var b = a.Split(new string[] { "ms" }, StringSplitOptions.None)[0];
                       elapsedSolver = Convert.ToDouble(b);
                   }
               }
                #endregion

                #region calculate heuristic  
                   
                var heuristic = new Heuristic();
                var heuristicResult1 = heuristic.strategy1(input, filename);
                heuristicResult1.Ratio = heuristicResult1.Revenue / solverResult;
                var heuristicResult2 = heuristic.strategy2(input, filename);
                heuristicResult2.Ratio = heuristicResult2.Revenue / solverResult;
                
                var v = Validate.ValidateResult(input, heuristicResult1.Choosen);
                foreach (var i in v)
                {
                    Debug.WriteLine(i);
                }
                Debug.WriteLine("");

                v = Validate.ValidateResult(input, heuristicResult2.Choosen);
                foreach (var i in v)
                {
                    Debug.WriteLine(i);
                }
                Debug.WriteLine("");
               
                //if (!File.Exists(@"..\..\..\..\TVHS_Data_test\Result\Mini\Heuristic.txt"))
                //{
                //    // Create a file to write to.
                //    using (StreamWriter sw = File.CreateText(@"..\..\..\..\TVHS_Data_test\Result\Heuristic.txt"))
                //    {
                //        sw.WriteLine("Test name \t Solver \t Solver elasped \t H1 \t H1 elapsed \t H2 \t H2 elapsed \t H3 \t H3 elapsed \t H4 \t H4 elapsed \t time");
                //    }
                //}

                using (System.IO.StreamWriter file = File.AppendText(@"..\..\..\..\TVHS_Data_test\Result\Heuristic.txt"))
                {
                    string testName = filename.Split(new string[] { "TVHS_Data_test\\" }, StringSplitOptions.None).Last().Split(new string[] { "\\F0" }, StringSplitOptions.None).First();
                    string time = DateTime.Now.ToLongDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString();
                    file.WriteLine(testName +" \t "+ solverResult+ " \t " + elapsedSolver + " \t " + heuristicResult1.Revenue + " \t " + heuristicResult1.Elapsed + " \t " + heuristicResult2.Revenue + " \t " + heuristicResult2.Elapsed +  " \t " + time);                               
                }

                #endregion
            }

        }


        #region init test
        private static MyCase InitData(string filename)
        {
            IWorkbook wb = null;
            MyCase data = new MyCase();
            data.Theta1 = 1;
            data.Theta2 = 0;
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
                        pr.Live = pr.Name.ToLower().Contains("live") ? true : false;
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
            #region add groupId, available time frame
            foreach (var item in data.Programs)
            {
                item.GroupId = data.Groups.Where(x => x.Id == data.BTGroups.Where(y => y.ProgramId == item.Id && y.BelongTo == 1).FirstOrDefault().GroupId).FirstOrDefault().Id;
                item.FrameList = data.Frames.Where(x => data.Allocates.Where(y => y.ProgramId == item.Id && y.Assignable == 1).Select(z => z.FrameId).ToList().Contains(x.Id)).ToList();
            }
            #endregion
            #region generate test
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_testcase.txt"))
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
        #endregion

    }
}