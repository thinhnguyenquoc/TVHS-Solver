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
    class Program
    {
        private static List<List<MyResult>> AllPath = new List<List<MyResult>>();
        static void Main(string[] args)
        {           
            #region input           
            double solverResult = 700000000;
            List<string> fileList = new List<string>(){
                @"..\..\..\..\TVHS_Data_test\3-8_9-8_2015\F0-F10.xlsx"
                , @"..\..\..\..\TVHS_Data_test\7-7_12-7_2015\F0-F10.xlsx"
                , @"..\..\..\..\TVHS_Data_test\7-9_13-9_2015\F0-F10.xlsx"
                , @"..\..\..\..\TVHS_Data_test\10-8_16-8_2015\F0-F10.xlsx"
                , @"..\..\..\..\TVHS_Data_test\13-7_19-7_2015\F0-F10.xlsx"
                , @"..\..\..\..\TVHS_Data_test\14-9_20-9_2015\F0-F10.xlsx"
            };
           
            #endregion
            foreach (var filename in fileList)
            {
                MyCase input = InitData(filename);
                MyCase data = Clone<MyCase>(input);
                MyCase data2 = Clone<MyCase>(input);
                MyCase data3 = Clone<MyCase>(input);
                #region solve
                //var solveResult = MySolver(data, filename);  
                //var solveResult = 591858256.955947;
                //GetIntegerResultFromSolverResult();

                var hueristicResult = Hueristic(data, filename);
                var hueristicResult2 = Hueristic2(data2, filename);
                var hueristicResult3 = Hueristic3(data3, filename);
                /*var byHandResult = revenueByHand;
                var ratioHand = byHandResult / solveResult;*/
                var ratioHue = hueristicResult / solverResult;
                var ratioHue2 = hueristicResult2 / solverResult;
                var ratioHue3 = hueristicResult3 / solverResult;

                Debug.WriteLine("solver: " + solverResult);
                Debug.WriteLine("hueristic:" + hueristicResult + "  ratio:" + ratioHue);
                Debug.WriteLine("hueristic2:" + hueristicResult2 + "  ratio2:" + ratioHue2);
                Debug.WriteLine("hueristic3:" + hueristicResult3 + "  ratio3:" + ratioHue3);
                /*Debug.WriteLine("hand:" + byHandResult + "  ratio:" + ratioHand);*/
            }
            #endregion
        }

        #region init test
        private static MyCase InitData(string filename)
        {
            IWorkbook wb = null;
            MyCase data = new MyCase();
            data.Delta = 60;
            data.Alpha = 0.5;
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
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename.Split('.').FirstOrDefault() + "_testcase.txt"))
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
       
        #region solve
        private static double MySolver(MyCase myCase, string filename)
        {
            try
            {
                var context = SolverContext.GetContext();
                var model = context.CreateModel();             

                #region Decision variable
                var programs = new Set(Domain.Any, "programs");
                var times = new Set(Domain.Any, "times");
                var choose = new Decision(Domain.RealRange(0, 1), "choose", programs, times);
                //var choose = new Decision(Domain.IntegerRange(0, 1), "choose", programs, times);
                model.AddDecision(choose);
                #endregion

                #region Constraint H1: program P is only aired at allowed time frame
                for (int i = 0; i < myCase.Programs.Count; i++)
                {
                    for (int j = 0; j < myCase.Frames.Count; j++)
                    {
                        Term[] terms = new Term[myCase.Frames[j].Duration];
                        int index = 0;
                        for (int k = myCase.Frames[j].Start; k <= myCase.Frames[j].End; k++)
                        {
                            // time starts from 1 but index of time starts from 0 => k-1
                            terms[index++] = choose[i, k - 1];
                        }
                        var allocate = myCase.Allocates.Where(x => x.FrameId == j && x.ProgramId == i).FirstOrDefault().Assignable;
                        model.AddConstraint("TimeFrame" + i.ToString() + "_" + j.ToString(), Model.Sum(terms) <= allocate * myCase.Programs[i].MaxShowTime
                        );
                    }
                }
                #endregion

                #region Constraint H2: each program is showed at leat once time
                for (int i = 0; i < myCase.Programs.Count; i++)
                {
                    Term[] terms = new Term[myCase.Times.Count - myCase.Programs[i].Duration + 1];
                    var index = 0;
                    for (int j = 0; j < myCase.Times.Count - myCase.Programs[i].Duration + 1; j++)
                    {
                        terms[index++] = choose[i, j];
                    }
                    model.AddConstraint("Exist" + i.ToString(), Model.Sum(terms) >= 1);
                }
                #endregion

                #region Constranst H3: Can not show two programs simultaneously.
                for (int i = 0; i < myCase.Times.Count; i++)
                {
                    Term[] termSumPs = new Term[myCase.Programs.Count];
                    for (int j = 0; j < myCase.Programs.Count; j++)
                    {
                        Term[] terms = null;
                        if (myCase.Times[i].Time - myCase.Programs[j].Duration < 0)
                        {
                            terms = new Term[myCase.Times[i].Time];
                            for (int k = 0; k < myCase.Times[i].Time; k++)
                                terms[k] = choose[j, k];
                        }
                        else
                        {
                            terms = new Term[myCase.Programs[j].Duration];
                            int index = 0;
                            for (int k = myCase.Times[i].Time - myCase.Programs[j].Duration; k < myCase.Times[i].Time; k++)
                                terms[index++] = choose[j, k];
                        }
                        termSumPs[j] = Model.Sum(terms);
                    }
                    model.AddConstraint("Simultaneous" + i.ToString(),
                        Model.Sum(termSumPs) <= 1
                    );
                }
                #endregion

                #region Constraint H4: each program is showed not great than maximum show time
                for (int i = 0; i < myCase.Programs.Count; i++)
                {
                    Term[] terms = new Term[myCase.Times.Count - myCase.Programs[i].Duration + 1];
                    var index = 0;
                    for (int j = 0; j < myCase.Times.Count - myCase.Programs[i].Duration + 1; j++)
                    {
                        terms[index++] = choose[i, j];
                    }
                    model.AddConstraint("Max" + i.ToString(), Model.Sum(terms) <= myCase.Programs[i].MaxShowTime);
                }
                #endregion

                #region Constraint H5: Can not show one program too close previous it's show
                var delta = myCase.Delta;
                for (int i = 0; i < myCase.Times.Count; i++)
                {
                    for (int j = 0; j < myCase.Programs.Count; j++)
                    {
                        Term[] terms = null;
                        if (myCase.Times[i].Time - delta < 0)
                        {
                            terms = new Term[myCase.Times[i].Time];
                            for (int k = 0; k < myCase.Times[i].Time; k++)
                                terms[k] = choose[j, k];
                        }
                        else
                        {
                            terms = new Term[delta];
                            int index = 0;
                            for (int k = myCase.Times[i].Time - delta; k < myCase.Times[i].Time; k++)
                                terms[index++] = choose[j, k];
                            model.AddConstraint("TooClose" + i.ToString() + "_" + j.ToString(), Model.Sum(terms) <= 1
                           );
                        }

                    }
                }
                #endregion

                #region Constraint H6: Total time of programs which belong to a group is less or equal to group allowed time
                for (int i = 0; i < myCase.Groups.Count; i++)
                {
                    Term[] SumGterms = new Term[myCase.Programs.Count];
                    for (int j = 0; j < myCase.Programs.Count; j++)
                    {
                        Term[] terms = new Term[myCase.Times.Count - myCase.Programs[j].Duration + 1];
                        for (int k = 0; k < myCase.Times.Count - myCase.Programs[j].Duration + 1; k++)
                        {
                            terms[k] = choose[j, k] * myCase.Programs[j].Duration * myCase.BTGroups.Where(x => x.GroupId == i && x.ProgramId == j).FirstOrDefault().BelongTo;
                        }
                        SumGterms[j] = Model.Sum(terms);
                    }
                    model.AddConstraint("Group" + i.ToString(), Model.Sum(SumGterms) <= myCase.Groups[i].TotalTime);
                }
                #endregion

                #region Objective function: Get maximum revenue
                Term[] termSumTPs = new Term[myCase.Programs.Count];
                for (int i = 0; i < myCase.Programs.Count; i++)
                {
                    Term[] terms = new Term[myCase.Times.Count - myCase.Programs[i].Duration + 1];
                    for (int j = 0; j < myCase.Times.Count - myCase.Programs[i].Duration + 1; j++)
                    {
                        terms[j] = choose[i, j] * myCase.Programs[i].Duration * myCase.Programs[i].Efficiency;
                    }
                    termSumTPs[i] = Model.Sum(terms);
                }

                model.AddGoal("revenue", GoalKind.Maximize, Model.Sum(termSumTPs));
                #endregion

                #region result
                var directive = new SimplexDirective();
                //directive.TimeLimit = 60000;
                var solution = context.Solve(directive);
                context.PropagateDecisions();
                var obs = choose.GetValues().ToList().Where(x => Convert.ToDouble(x.First()) > 0).ToList();
                foreach (var i in obs)
                {
                    Debug.WriteLine(i[1].ToString() + "\t" + i[2] + "\t" + myCase.Programs[Convert.ToInt32(i[1].ToString())].Duration + "\t" + myCase.Programs[Convert.ToInt32(i[1].ToString())].MaxShowTime + " \t " + myCase.Programs[Convert.ToInt32(i[1].ToString())].Efficiency + "\t" + i[0]);
                }

                Report report = solution.GetReport();
                Debug.WriteLine("This is the custom report: ");
                Debug.WriteLine("The {0} model used the {1} capability and {2} solution directive and had an {3} quality setting. \n Goal: {4}",
                    report.ModelName.ToString(),
                    report.SolverCapability.ToString(),
                    report.SolutionDirective.ToString(),
                    report.SolutionQuality.ToString(), solution.Goals.FirstOrDefault().ToDouble());
                Debug.WriteLine("The {0} solver finished in {1} ms with a total time of {2} ms.",
                    report.SolverType.ToString(),
                    report.SolveTime.ToString(),
                    report.TotalTime.ToString());
                #endregion

                #region generate result
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(filename.Split('.').FirstOrDefault() + "_resultBS.txt"))
                {
                    foreach (var i in obs)
                    {
                        file.WriteLine(i[1].ToString() + "\t" + i[2] + "\t" + myCase.Programs[Convert.ToInt32(i[1].ToString())].Duration + "\t" + myCase.Programs[Convert.ToInt32(i[1].ToString())].MaxShowTime + " \t " + myCase.Programs[Convert.ToInt32(i[1].ToString())].Efficiency + "\t" + i[0]);
                    }
                    file.WriteLine("RBS\t" + solution.Goals.FirstOrDefault().ToDouble());
                    file.WriteLine("This is the custom report: ");
                    file.WriteLine("The {0} model used the {1} capability and {2} solution directive and had an {3} quality setting.",
                        report.ModelName.ToString(),
                        report.SolverCapability.ToString(),
                        report.SolutionDirective.ToString(),
                        report.SolutionQuality.ToString());
                    file.WriteLine("The {0} solver finished in {1} ms with a total time of {2} ms.",
                        report.SolverType.ToString(),
                        report.SolveTime.ToString(),
                        report.TotalTime.ToString());
                }
                #endregion

                return solution.Goals.FirstOrDefault().ToDouble();
            }
            catch (Exception e)
            {
                return -1;
            }
        }
        private static MyCase GetCase(string url)
        {
            MyCase myCase = new MyCase();
            myCase.Programs = new List<MyProgram>();
            myCase.Frames = new List<MyTimeFrame>();
            myCase.Groups = new List<MyGroup>();
            myCase.Allocates = new List<MyAssignment>();
            myCase.BTGroups = new List<MyBelongToGroup>();
            System.IO.StreamReader file = new System.IO.StreamReader(url);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                var ls = line.Split('\t');

                if (ls[0] == "T")
                {
                    myCase.Times = GetTime(Convert.ToInt32(ls[1]));
                }
                else if (ls[0] == "P")
                {
                    var Name = "";
                    if (ls.Count() <= 5)
                        Name = ls[1];
                    else
                    {
                        for (int i = 5; i < ls.Count(); i++)
                        {
                            Name += ls[i] + " ";
                        }
                    }
                    myCase.Programs.Add(new MyProgram()
                    {
                        Id = Convert.ToInt32(ls[1]),
                        Name = Name,
                        Duration = Convert.ToInt32(ls[2]),
                        Efficiency = Convert.ToDouble(ls[3]),
                        MaxShowTime = Convert.ToInt32(ls[4]),
                    });
                }
                else if (ls[0] == "F")
                {
                    var Name = "";
                    if (ls.Count() <= 4)
                        Name = ls[1];
                    else
                        Name = ls[4];
                    myCase.Frames.Add(new MyTimeFrame()
                    {
                        Id = Convert.ToInt32(ls[1]),
                        Name = Name,
                        Start = Convert.ToInt32(ls[2]),
                        End = Convert.ToInt32(ls[3])
                    });
                }
                else if (ls[0] == "G")
                {
                    var Name = "";
                    if (ls.Count() <= 3)
                        Name = ls[1];
                    else
                        Name = ls[3];
                    myCase.Groups.Add(new MyGroup()
                    {
                        Id = Convert.ToInt32(ls[1]),
                        Name = Name,
                        TotalTime = Convert.ToInt32(ls[2])
                    });
                }
                else if (ls[0] == "A")
                {
                    myCase.Allocates.Add(new MyAssignment()
                    {
                        FrameId = Convert.ToInt32(ls[1]),
                        ProgramId = Convert.ToInt32(ls[2]),
                        Assignable = Convert.ToInt32(ls[3])
                    });
                }
                else if (ls[0] == "B")
                {
                    myCase.BTGroups.Add(new MyBelongToGroup()
                    {
                        GroupId = Convert.ToInt32(ls[1]),
                        ProgramId = Convert.ToInt32(ls[2]),
                        BelongTo = Convert.ToInt32(ls[3])
                    });
                }
                else if (ls[0] == "D")
                {
                    myCase.Delta = Convert.ToInt32(ls[1]);
                }
            }
            return myCase;
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
        #endregion

        #region find integer result
        private static void GetIntegerResultFromSolverResult()
        {
            var realtest3 = @"D:\solver-TVHS-26-8\24_30_8-9_30-19_30result.txt";
            List<MyResult> listResult = new List<MyResult>();
            System.IO.StreamReader file = new System.IO.StreamReader(realtest3);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                var ls = line.Split('\t');
                MyResult result = new MyResult();
                result.proramId = Convert.ToInt32(ls[0]);
                result.time = Convert.ToInt32(ls[1]);
                result.duration = Convert.ToInt32(ls[2]);
                result.maxShowTime = Convert.ToInt32(ls[3]);
                result.eff = Convert.ToDouble(ls[4]);
                result.decision = Convert.ToDouble(ls[5]);
                listResult.Add(result);
            }
            listResult = listResult.Where(x => x.decision > 0.1).ToList();
            List<Node> nodes = new List<Node>();
            foreach (var i in listResult)
            {
                nodes.Add(GetNode(i, listResult));

            }
            List<MyResult> save = new List<MyResult>();

            /// change node index
            GetTree(nodes[0], nodes, save);

            var candidate = AllPath.Where(x => x.Count >= 24).ToList();
            List<List<MyResult>> candidate2 = new List<List<MyResult>>();
            foreach (var k in candidate)
            {
                var flg = 0;
                for (int l = 0; l < 24; l++)
                {
                    if (k.Where(x => x.proramId == l).FirstOrDefault() == null)
                    {
                        flg = 1;
                        break;
                    }
                    else
                    {
                        if (k.Where(x => x.proramId == l).Count() > listResult.Where(x => x.proramId == l).FirstOrDefault().maxShowTime)
                        {
                            flg = 1;
                            break;
                        }
                    }

                }
                if (flg == 0)
                {
                    candidate2.Add(k);
                }
            }
            List<double> revenue = new List<double>();
            foreach (var p in candidate2)
            {
                double rev = 0;
                foreach (var h in p)
                {
                    rev += listResult.Where(x => x.proramId == h.proramId).FirstOrDefault().duration * listResult.Where(x => x.proramId == h.proramId).FirstOrDefault().eff;
                }
                revenue.Add(rev);
            }
            var max = revenue.Max();
            int index = -1;
            for (int i = 0; i < revenue.Count; i++)
            {
                if (revenue[i] == max)
                {
                    index = i;
                    break;
                }

            }
            var solution = candidate2[index];
            foreach (var i in solution)
            {
                Debug.WriteLine(i.proramId.ToString() + "\t" + i.time + "\t" + i.duration + "\t" + i.maxShowTime + " \t " + i.eff + " \t " + i.decision);
            }
        }
        private static void GetTree(Node n, List<Node> nodes, List<MyResult> save)
        {
            save.Add(n.Current);
            if (n.Children.Count == 0)
            {
                AllPath.Add(save);
            }
            else
            {
                foreach (var i in n.Children.OrderBy(x => x.time).ToList())
                {
                    var childNode = nodes.Where(x => x.Current == i).FirstOrDefault();
                    if (childNode != null)
                    {
                        List<MyResult> saveRep = new List<MyResult>();
                        foreach (var j in save)
                        {
                            saveRep.Add(j);
                        }
                        GetTree(childNode, nodes, saveRep);
                    }
                }

            }
        }
        private static Node GetNode(MyResult m, List<MyResult> r)
        {
            Node a = new Node();
            a.Current = m;
            a.Children = new List<MyResult>();
            foreach (var item in r)
            {
                if (m.time + m.duration <= item.time && item.time < m.time + m.duration + 15 && item.proramId != m.proramId)
                {
                    a.Children.Add(item);
                }
            }
            return a;
        }
        #endregion

        #region heuristic
        private static double Hueristic(MyCase myCase, string filename)
        {
            //sort program by efficiency
            myCase.Programs = myCase.Programs.OrderByDescending(x => x.Efficiency).ToList();

            int[] Choosen = new int[myCase.Times.Count];
            for (int i = 0; i < myCase.Times.Count; i++)
            {
                Choosen[i] = -1;
            }

            // assign program to dynamic time
            while (true)
            {
                //find program which has max eff and still available
                //// find programs still are live
                var list1 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
                if (list1.Count == 0)
                {
                    break;
                }

                foreach (var item in list1)
                {
                    var gr = myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == item.Id).FirstOrDefault().GroupId).FirstOrDefault();
                    #region group still has cota
                    if (gr.TotalTime >= item.Duration)
                    {
                        //check allowed frames
                        var FrameIdList = myCase.Allocates.Where(x => x.ProgramId == item.Id && x.Assignable == 1).Select(x => x.FrameId).ToList();

                        //add program to time frame
                        ////check program assigned in frame
                        foreach (var frameId in FrameIdList)
                        {
                            int startAv = -1;
                            for (int i = myCase.Frames.Where(x => x.Id == frameId).FirstOrDefault().Start; i <= myCase.Frames.Where(x => x.Id == frameId).FirstOrDefault().End; i++)
                            {
                                if (Choosen[i - 1] == -1)
                                {
                                    startAv = i - 1;
                                    break;
                                }
                            }
                            #region find a empty space to assign
                            if (startAv != -1)
                            {
                                ////check available slot
                                bool available = true;
                                if (startAv + item.Duration >= myCase.Times.Count)
                                {
                                    available = false;
                                }
                                else
                                {
                                    for (int m = startAv; m < startAv + item.Duration; m++)
                                    {
                                        if (Choosen[m] != -1)
                                        {
                                            available = false;
                                            break;
                                        }
                                    }
                                }
                                if (available)
                                {
                                    //// check too close
                                    var meet = false;
                                    for (int m = startAv + item.Duration; m < Math.Min(startAv + myCase.Delta, myCase.Times.Count); m++)
                                    {
                                        if (Choosen[m] == item.Id)
                                        {
                                            meet = true;
                                            break;
                                        }
                                    }
                                    if (!meet)
                                    {
                                        var meetbefore = false;
                                        for (int m = Math.Max(startAv - myCase.Delta, 0); m < startAv; m++)
                                        {
                                            if (Choosen[m] == item.Id)
                                            {
                                                meetbefore = true;
                                                break;
                                            }
                                        }
                                        
                                        if (!meetbefore)
                                        {
                                            ////assign program to frame
                                            for (int j = 0; j < item.Duration; j++)
                                            {
                                                Choosen[startAv] = item.Id;
                                                startAv++;
                                            }
                                            //// decrease the maximum show time of this program
                                            myCase.Programs.Where(x => x.Id == item.Id).FirstOrDefault().MaxShowTime--;
                                            //pr.MaxShowTime--;
                                            myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == item.Id && y.BelongTo == 1).FirstOrDefault().GroupId).FirstOrDefault().TotalTime -= item.Duration;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    //// decrease the maximum show time of this program
                                    myCase.Programs.Where(x => x.Id == item.Id).FirstOrDefault().MaxShowTime--;
                                }
                            }
                            #endregion
                            else
                            {
                                //// decrease the maximum show time of this program
                                myCase.Programs.Where(x => x.Id == item.Id).FirstOrDefault().MaxShowTime--;
                            }
                        }
                    }
                    #endregion
                    else
                    {
                        //// decrease the maximum show time of this program
                        myCase.Programs.Where(x => x.Id == item.Id).FirstOrDefault().MaxShowTime = 0;
                    }
                }
            }
            #region calculate revenue
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
                revenue += myCase.Programs.Where(x => x.Id == i.proId).FirstOrDefault().RevenuePerTime * i.numShow;
            }
            return revenue;
            #endregion
        }

        private static double Hueristic2(MyCase myCase, string filename)
        {
            //sort program by efficiency
            myCase.Programs = myCase.Programs.OrderByDescending(x => x.Efficiency).ToList();

            int[] Choosen = new int[myCase.Times.Count];
            for (int i = 0; i < myCase.Times.Count; i++)
            {
                Choosen[i] = -1;
            }

            // assign program to dynamic time
            while (true)
            {
                //find program which has max eff and still available
                //// find programs still are live
                var list1 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
                if (list1.Count == 0)
                {
                    break;
                }

                foreach (var item in list1)
                {
                    var gr = myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == item.Id).FirstOrDefault().GroupId).FirstOrDefault();
                    #region group still has cota
                    if (gr.TotalTime >= item.Duration)
                    {
                        //check allowed frames
                        var FrameIdList = myCase.Allocates.Where(x => x.ProgramId == item.Id && x.Assignable == 1).Select(x => x.FrameId).ToList();
                        // sort and update frame by unoccupied slot
                        var orderFrameList = FindBiggestFrame(myCase, Choosen, FrameIdList);
                        //add program to time frame
                        ////check program assigned in frame
                        foreach (var frameId in orderFrameList)
                        {
                            int startAv = -1;
                            for (int i = myCase.Frames.Where(x => x.Id == frameId).FirstOrDefault().Start; i <= myCase.Frames.Where(x => x.Id == frameId).FirstOrDefault().End; i++)
                            {
                                if (Choosen[i - 1] == -1)
                                {
                                    startAv = i - 1;
                                    break;
                                }
                            }
                            #region find a empty space to assign
                            if (startAv != -1)
                            {
                                ////check available slot
                                bool available = true;
                                if (startAv + item.Duration >= myCase.Times.Count)
                                {
                                    available = false;
                                }
                                else
                                {
                                    for (int m = startAv; m < startAv + item.Duration; m++)
                                    {
                                        if (Choosen[m] != -1)
                                        {
                                            available = false;
                                            break;
                                        }
                                    }
                                }
                                if (available)
                                {
                                    //// check too close
                                    var meet = false;
                                    for (int m = startAv + item.Duration; m < Math.Min(startAv + myCase.Delta, myCase.Times.Count); m++)
                                    {
                                        if (Choosen[m] == item.Id)
                                        {
                                            meet = true;
                                            break;
                                        }
                                    }
                                    if (!meet)
                                    {
                                        var meetbefore = false;
                                        for (int m = Math.Max(startAv - myCase.Delta, 0); m < startAv; m++)
                                        {
                                            if (Choosen[m] == item.Id)
                                            {
                                                meetbefore = true;
                                                break;
                                            }
                                        }

                                        if (!meetbefore)
                                        {
                                            ////assign program to frame
                                            for (int j = 0; j < item.Duration; j++)
                                            {
                                                Choosen[startAv] = item.Id;
                                                startAv++;
                                            }
                                            //// decrease the maximum show time of this program
                                            myCase.Programs.Where(x => x.Id == item.Id).FirstOrDefault().MaxShowTime--;
                                            //pr.MaxShowTime--;
                                            myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == item.Id && y.BelongTo == 1).FirstOrDefault().GroupId).FirstOrDefault().TotalTime -= item.Duration;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    //// decrease the maximum show time of this program
                                    myCase.Programs.Where(x => x.Id == item.Id).FirstOrDefault().MaxShowTime--;
                                }
                            }
                            #endregion
                            else
                            {
                                //// decrease the maximum show time of this program
                                myCase.Programs.Where(x => x.Id == item.Id).FirstOrDefault().MaxShowTime--;
                            }
                        }
                    }
                    #endregion
                    else
                    {
                        //// decrease the maximum show time of this program
                        myCase.Programs.Where(x => x.Id == item.Id).FirstOrDefault().MaxShowTime = 0;
                    }
                }
            }
           
            #region calculate revenue
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
                revenue += myCase.Programs.Where(x => x.Id == i.proId).FirstOrDefault().RevenuePerTime * i.numShow;
            }
            return revenue;
            #endregion
        }

        private static double Hueristic3(MyCase myCase, string filename)
        {
            //sort program by efficiency
            myCase.Programs = myCase.Programs.OrderByDescending(x => x.Efficiency).ToList();

            int[] Choosen = new int[myCase.Times.Count];
            for (int i = 0; i < myCase.Times.Count; i++)
            {
                Choosen[i] = -1;
            }

            // assign program to dynamic time
            while (true)
            {
                //find program which has max eff and still available
                //// find programs still are live
                var list1 = myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
                if (list1.Count == 0)
                {
                    break;
                }

                foreach (var item in list1)
                {
                    var gr = myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == item.Id).FirstOrDefault().GroupId).FirstOrDefault();
                    #region group still has cota
                    if (gr.TotalTime >= item.Duration)
                    {
                        //check allowed frames
                        var FrameIdList = myCase.Allocates.Where(x => x.ProgramId == item.Id && x.Assignable == 1).Select(x => x.FrameId).ToList();
                        // sort and update frame by unoccupied slot
                        var orderFrameList = FindBiggestFrame(myCase, Choosen, FrameIdList);
                        //add program to time frame
                        ////check program assigned in frame
                        foreach (var frameId in orderFrameList)
                        {
                            int startAv = -1;
                            for (int i = myCase.Frames.Where(x => x.Id == frameId).FirstOrDefault().Start; i <= myCase.Frames.Where(x => x.Id == frameId).FirstOrDefault().End; i++)
                            {
                                if (Choosen[i - 1] == -1)
                                {
                                    startAv = i - 1;
                                    break;
                                }
                            }
                            #region find a empty space to assign
                            if (startAv != -1)
                            {
                                ////check available slot
                                bool available = true;
                                if (startAv + item.Duration >= myCase.Times.Count)
                                {
                                    available = false;
                                }
                                else
                                {
                                    for (int m = startAv; m < startAv + item.Duration; m++)
                                    {
                                        if (Choosen[m] != -1)
                                        {
                                            available = false;
                                            break;
                                        }
                                    }
                                }
                                if (available)
                                {
                                    //// check too close
                                    var meet = false;
                                    for (int m = startAv + item.Duration; m < Math.Min(startAv + myCase.Delta, myCase.Times.Count); m++)
                                    {
                                        if (Choosen[m] == item.Id)
                                        {
                                            meet = true;
                                            break;
                                        }
                                    }
                                    if (!meet)
                                    {
                                        var meetbefore = false;
                                        for (int m = Math.Max(startAv - myCase.Delta, 0); m < startAv; m++)
                                        {
                                            if (Choosen[m] == item.Id)
                                            {
                                                meetbefore = true;
                                                break;
                                            }
                                        }

                                        if (!meetbefore)
                                        {
                                            ////assign program to frame
                                            for (int j = 0; j < item.Duration; j++)
                                            {
                                                Choosen[startAv] = item.Id;
                                                startAv++;
                                            }
                                            //// decrease the maximum show time of this program
                                            myCase.Programs.Where(x => x.Id == item.Id).FirstOrDefault().MaxShowTime--;
                                            //pr.MaxShowTime--;
                                            myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == item.Id && y.BelongTo == 1).FirstOrDefault().GroupId).FirstOrDefault().TotalTime -= item.Duration;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    //// decrease the maximum show time of this program
                                    myCase.Programs.Where(x => x.Id == item.Id).FirstOrDefault().MaxShowTime--;
                                }
                            }
                            #endregion
                            else
                            {
                                //// decrease the maximum show time of this program
                                myCase.Programs.Where(x => x.Id == item.Id).FirstOrDefault().MaxShowTime--;
                            }
                        }
                    }
                    #endregion
                    else
                    {
                        //// decrease the maximum show time of this program
                        myCase.Programs.Where(x => x.Id == item.Id).FirstOrDefault().MaxShowTime = 0;
                    }
                }
            }
            // calculate the unoccupate of two continue frame
            List<int> Unoccupate = new List<int>();
            for (int i = 0; i < myCase.Frames.Count - 1; i++)
            {
                Unoccupate.Add(myCase.Frames[i].Unoccupate + myCase.Frames[i + 1].Unoccupate);
            }
            var list2 = myCase.Programs.OrderByDescending(x => x.Efficiency).ToList();
            foreach (var pro in list2)
            {
                var gr = myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == pro.Id).FirstOrDefault().GroupId).FirstOrDefault();
               
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
                                //// check too close
                                var meet = false;
                                for (int m = startAv + pro.Duration; m < Math.Min(startAv + myCase.Delta, myCase.Times.Count); m++)
                                {
                                    if (Choosen[m] == pro.Id)
                                    {
                                        meet = true;
                                        break;
                                    }
                                }
                                if (!meet)
                                {
                                    var meetbefore = false;
                                    for (int m = Math.Max(startAv - myCase.Delta, 0); m < startAv; m++)
                                    {
                                        if (Choosen[m] == pro.Id)
                                        {
                                            meetbefore = true;
                                            break;
                                        }
                                    }

                                    if (!meetbefore)
                                    {
                                        int shift = Unoccupate[i] - myCase.Frames[i].Unoccupate;
                                        // ShiftRight
                                        ShiftRight(myCase, Choosen, shift, i + 1);
                                        ////assign program to frame
                                        for (int j = 0; j < pro.Duration; j++)
                                        {
                                            Choosen[startAv] = pro.Id;
                                            startAv++;
                                        }
                                        // sort and update frame by unoccupied slot
                                        var orderFrameList = FindBiggestFrame(myCase, Choosen, FrameIdList);
                                        //// decrease the maximum show time of this program
                                        myCase.Programs.Where(x => x.Id == pro.Id).FirstOrDefault().MaxShowTime--;
                                        //pr.MaxShowTime--;
                                        myCase.Groups.Where(x => x.Id == myCase.BTGroups.Where(y => y.ProgramId == pro.Id && y.BelongTo == 1).FirstOrDefault().GroupId).FirstOrDefault().TotalTime -= pro.Duration;
                                        break;
                                    }
                                }
                            }
                        }

                    }
                    //// decrease the maximum show time of this program
                    myCase.Programs.Where(x => x.Id == pro.Id).FirstOrDefault().MaxShowTime--;
                }
                else
                {
                    //// decrease the maximum show time of this program
                    myCase.Programs.Where(x => x.Id == pro.Id).FirstOrDefault().MaxShowTime = 0;
                }
            }
 
   
            #region calculate revenue
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
                revenue += myCase.Programs.Where(x => x.Id == i.proId).FirstOrDefault().RevenuePerTime * i.numShow;
            }
            return revenue;
            #endregion
        }

        private static List<int> FindBiggestFrame(MyCase myCase, int[] Choosen, List<int> listFrameId)
        {
            int[] Cache = new int[listFrameId.Count];
            for (int i = 0; i < listFrameId.Count; i++)
            {
                Cache[i] = 0;
            }
            for (int i = 0; i < listFrameId.Count; i++)
            {
                for (int j = myCase.Frames.Where(x => x.Id == listFrameId[i]).First().Start; j <= myCase.Frames.Where(x => x.Id == listFrameId[i]).First().End; j++)
                {
                    if (Choosen[j-1] == -1)
                    {
                        Cache[i] += 1;
                    }
                }
                myCase.Frames.Where(x => x.Id == listFrameId[i]).First().Unoccupate = Cache[i];
            }
            List<int> result = new List<int>();
            for (int i = 0; i < listFrameId.Count; i++)
            {
                for (int j = 0; j < listFrameId.Count; j++)
                {
                    int max = Cache.Max();
                    if (max == -1)
                        break;
                    if (Cache[j] == max)
                    {
                        result.Add(j);
                        Cache[j] = -1;
                    }
                }
            }
            return result;
        }
        #endregion

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

        static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        static void ShiftRight(MyCase myCase, int[] Choosen, int k, int FrameId)
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
            for (int i = start -1; i >= myCase.Frames[FrameId].Start - 1; i--)
            {
                Choosen[i + k] = Choosen[i];                
            }
            for (int i = start - 1; i < start - 1 + k; i++)
            {
                Choosen[i] = -1;
            }
        }
    }

    #region define object
    public class TempResult
    {
        public int proId { get; set; }
        public int numShow { get; set; }
    }
    public class Node
    {
        public MyResult Current { get; set; }
        public List<MyResult> Children { get; set; }
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
