using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace solver_TVHS_26_7
{
    public class Solver
    {
        #region solve
        public double Solve(MyCase myCase, string filename)
        {
            try
            {
                var context = SolverContext.GetContext();
                context.ClearModel();
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
                        terms[j] = choose[i, j] * myCase.Programs[i].Duration * myCase.Programs[i].Efficiency * myCase.Theta1 + choose[i, j] * myCase.Theta2 * 1000000;
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
                    new System.IO.StreamWriter(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultBS.txt"))
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
       
        #endregion
    }
}
