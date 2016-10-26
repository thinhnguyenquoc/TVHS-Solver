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
            double elapsedSolver = 0;
            List<string> fileList = new List<string>(){
                //@"..\..\..\..\TVHS_Data_test\3-8_9-8_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\7-7_12-7_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\7-9_13-9_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\10-8_16-8_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\13-7_19-7_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\14-9_20-9_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\17-8_23-8_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\20-7_26-7_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\21-9_27-9_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\8-1_9-1_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\24-8_30-8_2015\F0-F10.xlsx"
            };
           
            #endregion

            foreach (var filename in fileList)
            {
                MyCase input = InitData(filename);
                #region call solver
                //var solver = new Solver();
                //var solveResult = solver.Solve(input, filename);
                #endregion
                
                #region read solver result 
                string solverUrl = filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultBS.txt";
                string[] lines = System.IO.File.ReadAllLines(solverUrl);
                foreach (string line in lines)
                {
                    if (line.Contains("RBS"))
                    {
                        solverResult = Convert.ToDouble(line.Split(new string[] { "RBS" }, StringSplitOptions.None)[1]);
                      
                    }
                    if (line.Contains("total time of"))
                    {
                        var a = line.Split(new string[] { "total time of" }, StringSplitOptions.None)[1];
                        var b = a.Split( new string[] { "ms" }, StringSplitOptions.None)[0];
                        elapsedSolver = Convert.ToDouble(b);
                    }
                }
                #endregion
                #region calculate heuristic              
                var heuristic = new Heuristic();
                var watch = System.Diagnostics.Stopwatch.StartNew();
                // the code that you want to measure comes here         
     
                var heuristicResult = heuristic.strategy1(input, filename);
                heuristicResult.Ratio = heuristicResult.Revenue / solverResult;
                var heuristicResult2 = heuristic.strategy2(input, filename);
                heuristicResult2.Ratio = heuristicResult2.Revenue / solverResult;
                var heuristicResult3 = heuristic.strategy3(input, filename);
                heuristicResult3.Ratio = heuristicResult3.Revenue / solverResult;
                var heuristicResult4 = heuristic.strategy4(input, filename);
                heuristicResult4.Ratio = heuristicResult4.Revenue / solverResult;

                Genetic gen = new Genetic();
                var genResult = gen.Solve(input, 50, 200, 0.2, 10, 200);
                genResult.Ratio = genResult.Revenue / solverResult;

                var genResult2 = gen.Solve2(input, 50, 200, 0.2, 10, 200);
                genResult2.Ratio = genResult2.Revenue / solverResult;

                List<int[]> h = new List<int[]>();
                h.Add(heuristicResult.Choosen);
                h.Add(heuristicResult2.Choosen);
                h.Add(heuristicResult3.Choosen);
                h.Add(heuristicResult4.Choosen);

                var genResult3 = gen.Solve3(input, 50, 200, 0.2, 10, 200, h);
                genResult3.Ratio = genResult3.Revenue / solverResult;

                var genResult4 = gen.Solve3(input, 50, 200, 0.2, 10, 200, h);
                genResult4.Ratio = genResult4.Revenue / solverResult;

                Debug.WriteLine("h1 r:\t" + heuristicResult.Revenue + "\t ratio:\t" + heuristicResult.Ratio + "\t elasped: \t"+ heuristicResult.Elapsed + " ms");
                Debug.WriteLine("h2 r:\t" + heuristicResult2.Revenue + "\t ratio:\t" + heuristicResult2.Ratio + "\t elasped: \t" + heuristicResult2.Elapsed + " ms");
                Debug.WriteLine("h3 r:\t" + heuristicResult3.Revenue + "\t ratio:\t" + heuristicResult3.Ratio + "\t elasped: \t" + heuristicResult3.Elapsed + " ms");
                Debug.WriteLine("h4 r:\t" + heuristicResult4.Revenue + "\t ratio:\t" + heuristicResult4.Ratio + "\t elasped: \t" + heuristicResult4.Elapsed + " ms");
                Debug.WriteLine("g1 r:\t" + genResult.Revenue + "\t ratio:\t" + genResult.Ratio + "\t elasped: \t" + genResult.Elapsed + " ms"+ "\t noGen: \t" + genResult.noGen);//g1 r:	5220124453.79103	 ratio:	0.967199329015871	 elasped: 	896640 ms
                Debug.WriteLine("g2 r:\t" + genResult2.Revenue + "\t ratio:\t" + genResult2.Ratio + "\t elasped: \t" + genResult2.Elapsed + " ms" + "\t noGen: \t" + genResult2.noGen);// g2 r:	5221982089.49032	 ratio:	0.967543516978785	 elasped: 	491167 ms	 noGen: 	12
                Debug.WriteLine("g3 r:\t" + genResult3.Revenue + "\t ratio:\t" + genResult3.Ratio + "\t elasped: \t" + genResult3.Elapsed + " ms" + "\t noGen: \t" + genResult3.noGen);//g3 r:	5201718810.36878	 ratio:	0.963789079676856	 elasped: 	884550 ms	 noGen: 	21
                Debug.WriteLine("g4 r:\t" + genResult4.Revenue + "\t ratio:\t" + genResult4.Ratio + "\t elasped: \t" + genResult4.Elapsed + " ms" + "\t noGen: \t" + genResult4.noGen);// g4 r:	5211478657.07107	 ratio:	0.965597411502145	 elasped: 	672431 ms	 noGen: 	17
                Debug.WriteLine(" ");


              
                //var v = Validate.ValidateResult(input, hueristicResult);
                //foreach (var i in v)
                //{
                //    Debug.WriteLine(i);
                //}
                //Debug.WriteLine("");

                //v = Validate.ValidateResult(input, hueristicResult2);
                //foreach (var i in v)
                //{
                //    Debug.WriteLine(i);
                //}
                //Debug.WriteLine("");
                //var v = Validate.ValidateResult(input, initPopulation);
                //foreach (var i in v)
                //{
                //    Debug.WriteLine(i);
                //}
                //Debug.WriteLine("");
                //v = Validate.ValidateResult(input, initPopulation2);
                //foreach (var i in v)
                //{
                //    Debug.WriteLine(i);
                //}
                //Debug.WriteLine("");

                
                //#endregion                

                if (!File.Exists(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultGen.txt"))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultGen.txt"))
                    {
                    }
                }

                 using (System.IO.StreamWriter file =File.AppendText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultGen.txt"))
                 {
                     file.WriteLine(DateTime.Now.ToLongDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString());
                     file.WriteLine("solver r:\t" + solverResult + "\t elasped: \t" + elapsedSolver);
                     file.WriteLine("50, 200, 0.2, 10, 200");
                     file.WriteLine("h1 r:\t" + heuristicResult.Revenue + "\t ratio:\t" + heuristicResult.Ratio + "\t elasped: \t" + heuristicResult.Elapsed + " ms");
                     file.WriteLine("h2 r:\t" + heuristicResult2.Revenue + "\t ratio:\t" + heuristicResult2.Ratio + "\t elasped: \t" + heuristicResult2.Elapsed + " ms");
                     file.WriteLine("h3 r:\t" + heuristicResult3.Revenue + "\t ratio:\t" + heuristicResult3.Ratio + "\t elasped: \t" + heuristicResult3.Elapsed + " ms");
                     file.WriteLine("h4 r:\t" + heuristicResult4.Revenue + "\t ratio:\t" + heuristicResult4.Ratio + "\t elasped: \t" + heuristicResult4.Elapsed + " ms");
                     file.WriteLine("g1 r:\t" + genResult.Revenue + "\t ratio:\t" + genResult.Ratio + "\t elasped: \t" + genResult.Elapsed + " ms" + "\t noGen: \t" + genResult.noGen);//g1 r:	5220124453.79103	 ratio:	0.967199329015871	 elasped: 	896640 ms
                     file.WriteLine("g2 r:\t" + genResult2.Revenue + "\t ratio:\t" + genResult2.Ratio + "\t elasped: \t" + genResult2.Elapsed + " ms" + "\t noGen: \t" + genResult2.noGen);// g2 r:	5221982089.49032	 ratio:	0.967543516978785	 elasped: 	491167 ms	 noGen: 	12
                     file.WriteLine("g3 r:\t" + genResult3.Revenue + "\t ratio:\t" + genResult3.Ratio + "\t elasped: \t" + genResult3.Elapsed + " ms" + "\t noGen: \t" + genResult3.noGen);//g3 r:	5201718810.36878	 ratio:	0.963789079676856	 elasped: 	884550 ms	 noGen: 	21
                     file.WriteLine("g4 r:\t" + genResult4.Revenue + "\t ratio:\t" + genResult4.Ratio + "\t elasped: \t" + genResult4.Elapsed + " ms" + "\t noGen: \t" + genResult4.noGen);// g4 r:	5211478657.07107	 ratio:	0.965597411502145	 elasped: 	672431 ms	 noGen: 	17
                     file.WriteLine(" ");
                 }
                #endregion
                
            }
            
        }

       
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
            #region add groupId, available time frame
            foreach (var item in data.Programs)
            {
                item.GroupId = data.Groups.Where(x => x.Id == data.BTGroups.Where(y => y.ProgramId == item.Id && y.BelongTo == 1).FirstOrDefault().GroupId).FirstOrDefault().Id;
                item.FrameList = data.Frames.Where(x => data.Allocates.Where(y => y.ProgramId == item.Id && y.Assignable == 1).Select(z => z.FrameId).ToList().Contains(x.Id)).ToList();
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

}
