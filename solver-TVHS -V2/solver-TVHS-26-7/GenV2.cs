using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPOI.HSSF.Record;

namespace solver_TVHS_26_7
{
    public class GenV2
    {
        private Dictionary<string, List<EvaluatePopulation>> _coupleDictionary = new Dictionary<string, List<EvaluatePopulation>>();
        private Dictionary<string, EvaluatePopulation> _fixChildDictionary = new Dictionary<string, EvaluatePopulation>();


        public string ConvertArrayToString(int[] choose)
        {
            string result = "";
            foreach (var character in choose)
            {
                result += character.ToString();
            }
            return result;
        }
        public List<EvaluatePopulation> CreateInitPopulationTreeV2(MyCase myOriginalCase, int size)
        {
            var frameWareDictionary = new HashSet<string>();
            var result = new List<EvaluatePopulation>();
            while (result.Count < size)
            {
                var myCase = myOriginalCase.DeepCopyByExpressionTree();
                var choosen = new int[myCase.Times.Count];
                for (var j = 0; j < myCase.Times.Count; j++)
                {
                    choosen[j] = -1;
                }
                bool change = false;
                #region order assign program to frame   
                // only random one time
                var list1 = Utility.RandomProgramListTree(myCase.Programs.Where(x => x.MaxShowTime > 0).ToList());
                var md5String = ConvertArrayToString(list1.Select(x => x.Id).ToArray());
                if (frameWareDictionary.Contains(md5String))
                {
                    continue;
                }
                frameWareDictionary.Add(md5String);
                while (true)
                {
                    foreach (var item in list1)
                    {
                        if (item.MaxShowTime <= 0)
                            break;
                        var gr = myCase.Groups.First(x => x.Id == item.GroupId);
                        #region group still has cota
                        if (gr.TotalTime >= item.Duration)
                        {
                            var frameList = item.FrameList;
                            foreach (var frame in frameList)
                            {
                                var startAv = Utility.GetFirstAvailableSlotInFrame(myCase, choosen, frame);
                                #region find a empty space to assign
                                if (startAv != -1)
                                {
                                    if (Utility.CheckAssignableProgram(myCase, choosen, startAv, item))
                                    {
                                        if (Utility.CheckTooClose(myCase, choosen, startAv, item))
                                        {
                                            Utility.AssignProgramToScheTree(choosen, startAv, item, gr);
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
                    Utility.UpdateUnOccupiedFrameTime(myCase, choosen);
                    var unoccupate = new List<int>();
                    for (var i = 0; i < myCase.Frames.Count - 1; i++)
                    {
                        unoccupate.Add(myCase.Frames[i].Unoccupate + myCase.Frames[i + 1].Unoccupate);
                    }

                    foreach (var pro in list1)
                    {
                        if (pro.MaxShowTime <= 0)
                            break;
                        var gr = myCase.Groups.First(x => x.Id == pro.GroupId);
                        if (gr.TotalTime >= pro.Duration)
                        {
                            for (int i = 0; i < unoccupate.Count; i++)
                            {
                                if (pro.Duration <= unoccupate[i])
                                {
                                    //check allowed frames
                                    var frameIdList = pro.FrameList.Select(x => x.Id).ToList();
                                    //add program to time frame
                                    if (frameIdList.Contains(i))
                                    {
                                        ////check available slot                      
                                        int startAv = myCase.Frames[i].End - myCase.Frames[i].Unoccupate;
                                        if (startAv != myCase.Frames[i].End)
                                        {
                                            if (Utility.CheckTooClose(myCase, choosen, startAv, pro))
                                            {
                                                int shift = pro.Duration - myCase.Frames[i].Unoccupate;
                                                // ShiftRight
                                                Utility.ShiftRight(myCase, choosen, shift, i + 1, unoccupate);
                                                Utility.AssignProgramToScheTree(choosen, startAv, pro, gr);
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

                var e = new EvaluatePopulation
                {
                    Choosen = choosen,
                    revenue = Utility.CalculateRevenue(myOriginalCase, choosen)
                };
                result.Add(e);
            }
            return result;
        }

        public List<List<EvaluatePopulation>> SelectParentsTreeRoundRobin(MyCase myOriginalCase, List<EvaluatePopulation> population, double percentCrossOver)
        {
            HashSet<string> cache = new HashSet<string>();
            double sum = 0;
            double total = population.Sum(x => x.revenue);
            foreach (var e in population)
            {
                e.min = sum;
                sum += e.revenue / total;
                e.max = sum;
            }

            var rnd = new Random();
            var parents = new List<List<EvaluatePopulation>>();
            for (int i = 0; i < population.Count() * percentCrossOver / 2; i++)
            {
                var couple = new List<EvaluatePopulation>();
                var r1 = rnd.NextDouble();
                var r2 = rnd.NextDouble();
                var a1 = population.FirstOrDefault(x => x.min < r1 && x.max >= r1);
                while (true)
                {
                    if (a1 != null)
                        break;
                    else
                    {
                        r1 = rnd.NextDouble();
                        a1 = population.FirstOrDefault(x => x.min < r1 && x.max >= r1);
                    }
                }
                var a2 = population.FirstOrDefault(x => x.min < r2 && x.max >= r2);
                while (true)
                {
                    if (a2 != null)
                        break;
                    else
                    {
                        r2 = rnd.NextDouble();
                        a2 = population.FirstOrDefault(x => x.min < r2 && x.max >= r2);
                    }
                }
                string key = ConvertArrayToString(a1.Choosen) + "_" + ConvertArrayToString(a2.Choosen);
                if (cache.Contains(key))
                {
                    continue;
                }
                cache.Add(key);
                couple.Add(a1);
                couple.Add(a2);
                parents.Add(couple);
            }
            return parents;
        }

        public EvaluatePopulation FixChildTreeV2(MyCase myOriginalCase, List<MyProgram> originalChild)
        {
            var myCase = myOriginalCase.DeepCopyByExpressionTree();
            var child = originalChild.DeepCopyByExpressionTree();
            var choosen = new int[myCase.Times.Count];
            for (var j = 0; j < myCase.Times.Count; j++)
            {
                choosen[j] = -1;
            }
            #region assign program to frame
            var list1 = child;
            var missingPro = new List<MyProgram>();
            foreach (var pro in myCase.Programs)
            {
                if (list1.FirstOrDefault(x => x.Id == pro.Id) == null)
                {
                    missingPro.Add(pro);
                }
            }
            list1 = missingPro.Concat(list1).ToList();
            foreach (var item in list1)
            {
                if (myCase.Programs.First(x => x.Id == item.Id).MaxShowTime > 0)
                {
                    var gr = myCase.Groups.First(x => x.Id == item.GroupId);
                    #region group still has cota
                    if (gr.TotalTime >= item.Duration)
                    {
                        var frameList = item.FrameList;
                        foreach (var frame in frameList)
                        {
                            int startAv = Utility.GetFirstAvailableSlotInFrame(myCase, choosen, frame);
                            #region find a empty space to assign
                            if (startAv != -1)
                            {
                                if (Utility.CheckAssignableProgram(myCase, choosen, startAv, item))
                                {
                                    if (Utility.CheckTooClose(myCase, choosen, startAv, item))
                                    {
                                        Utility.AssignProgramToScheTree(choosen, startAv, item, gr);
                                        break;
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }
                #endregion
            }
            #endregion
            #region try to assign programe between two frames

            var change = false;
            // calculate the unoccupate of two continue frame
            while (true)
            {
                Utility.UpdateUnOccupiedFrameTime(myCase, choosen);
                var unoccupate = new List<int>();
                for (int i = 0; i < myCase.Frames.Count - 1; i++)
                {
                    unoccupate.Add(myCase.Frames[i].Unoccupate + myCase.Frames[i + 1].Unoccupate);
                }
                foreach (var pro in list1)
                {
                    if (pro.MaxShowTime <= 0)
                        break;
                    var gr = myCase.Groups.FirstOrDefault(x => x.Id == pro.GroupId);
                    if (gr.TotalTime >= pro.Duration)
                    {
                        for (int i = 0; i < unoccupate.Count; i++)
                        {
                            if (pro.Duration <= unoccupate[i])
                            {
                                //check allowed frames
                                var frameIdList = pro.FrameList.Select(x => x.Id).ToList();
                                //add program to time frame
                                if (frameIdList.Contains(i))
                                {
                                    ////check available slot                      
                                    int startAv = myCase.Frames[i].End - myCase.Frames[i].Unoccupate;
                                    if (startAv != myCase.Frames[i].End)
                                    {
                                        if (Utility.CheckTooClose(myCase, choosen, startAv, pro))
                                        {
                                            int shift = pro.Duration - myCase.Frames[i].Unoccupate;
                                            // ShiftRight
                                            Utility.ShiftRight(myCase, choosen, shift, i + 1, unoccupate);
                                            Utility.AssignProgramToScheTree(choosen, startAv, pro, gr);
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

            var va = Validate.ValidateResult(myOriginalCase, choosen);
            if (va.Where(x => x != 0).Any())
            {
                return null;
            }
            else
            {
                var e = new EvaluatePopulation();
                e.Choosen = choosen;
                e.revenue = Utility.CalculateRevenue(myOriginalCase, choosen);
                return e;
            }

        }


        public List<EvaluatePopulation> SingleCrossTreeV2(MyCase myOriginalCase, List<EvaluatePopulation> couple)
        {
            var parent1 = Utility.ConvertToProgram(myOriginalCase, couple[0].Choosen);
            var parent2 = Utility.ConvertToProgram(myOriginalCase, couple[1].Choosen);
            // random split => read more paper
            var splitPoint = Convert.ToInt32(Math.Min(parent1.Count(), parent2.Count()) * 0.3);
            var child1 = new List<MyProgram>();
            var child2 = new List<MyProgram>();
            for (var i = 0; i < parent1.Count(); i++)
            {
                child1.Add(i < splitPoint ? parent2.ElementAt(i) : parent1.ElementAt(i));
            }
            for (var i = 0; i < parent2.Count(); i++)
            {
                child2.Add(i < splitPoint ? parent1.ElementAt(i) : parent2.ElementAt(i));
            }

            var result = new List<EvaluatePopulation>();
            var newChild1 = FixChildTreeV2(myOriginalCase, child1);
            if (newChild1 != null)
                result.Add(newChild1);
            var newChild2 = FixChildTreeV2(myOriginalCase, child2);
            if (newChild2 != null)
                result.Add(newChild2);
            return result;
        }


        public List<EvaluatePopulation> MakeChildrenTreeV2(MyCase myOriginalCase, List<List<EvaluatePopulation>> parents, bool IsInsert)
        {
            var result = new List<EvaluatePopulation>();
            for (int i = 0; i < parents.Count(); i++)
            {
                var md5String = ConvertArrayToString(parents[i][0].Choosen) + "_" + ConvertArrayToString(parents[i][1].Choosen);
                if (!_coupleDictionary.ContainsKey(md5String))
                {
                    var children = SingleCrossTreeV2(myOriginalCase, parents[i]);
                    if (children.FirstOrDefault() != null)
                        result.Add(children.FirstOrDefault());
                    if (children.LastOrDefault() != null)
                        result.Add(children.LastOrDefault());
                    _coupleDictionary.Add(md5String, children);
                }
                else
                {
                    var children = _coupleDictionary[md5String];
                    if (children.FirstOrDefault() != null)
                        result.Add(children.FirstOrDefault());
                    if (children.LastOrDefault() != null)
                        result.Add(children.LastOrDefault());
                }
            }
            return result;
        }

        public List<EvaluatePopulation> MutateTreeV2(MyCase myOriginalCase, List<EvaluatePopulation> population, double percent, bool IsInsert)
        {
            var ran = new Random();
            for (int i = 0; i <= population.Count * percent; i++)
            {
                int index = ran.Next(population.Count);
                List<MyProgram> chromosome = Utility.ConvertToProgram(myOriginalCase, population[index].Choosen);
                int indexMutation = ran.Next(chromosome.Count);
                int indexProgram = ran.Next(myOriginalCase.Programs.Count);
                chromosome[indexMutation] = myOriginalCase.Programs[indexProgram];
                var md5 = ConvertArrayToString(chromosome.Select(x => x.Id).ToArray());
                if (_fixChildDictionary.ContainsKey(md5))
                {
                    var fixedChild = _fixChildDictionary[md5];
                    if (fixedChild != null)
                    {
                        population[index] = fixedChild;
                    }
                }
                else
                {
                    var fixedChild = FixChildTreeV2(myOriginalCase, chromosome);
                    _fixChildDictionary.Add(md5,fixedChild);
                    if (fixedChild != null)
                    {
                        population[index] = fixedChild;
                    }
                }
            }
            return population;
        }

        public List<EvaluatePopulation> ReproductionTheBest(List<EvaluatePopulation> population, int size)
        {
            var theBest = population.OrderByDescending(x => x.revenue).Take(size).ToList();
            return theBest;
        }


        public MyStatictis SolveV2_1(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename)
        {
            var myOriginalCase = input;
            double revenue = 0;
            var result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulationTreeV2(myOriginalCase, initSize);
            revenue = initPopulation.Max(x => x.revenue);
            var elapsedH1 = 0.0;
            int count = 0;
            var db = new TVHS();
            var testcase = db.TestCases.Add(new TestCase
            {
                TestCase1 = filename.Split(new string[] { "TVHS_Data_test" }, StringSplitOptions.None).Last() + "-V2",
                Kind = "RoundRobin - Insert - TheBestParent",
                CrossOver = percentCrossOver,
                Mutation = mutationPercent,
                Population = populationSize,
                LimitLoop = noloop,
                NoChange = nochange,
                Date = DateTime.Now,
                MaxGen = revenue
            });

            for (var lop = 0; lop < noloop; lop++)
            {
                result.noGen++;
                // using round robin - monte carlo
                var parents = SelectParentsTreeRoundRobin(myOriginalCase, initPopulation, percentCrossOver);
                var children = MakeChildrenTreeV2(myOriginalCase, parents, true);
                initPopulation = initPopulation.Concat(children).ToList();
                initPopulation = MutateTreeV2(myOriginalCase, initPopulation, mutationPercent, true);
                initPopulation = ReproductionTheBest(initPopulation, populationSize);
                var re = initPopulation.Max(x => x.revenue);
                if (re > revenue)
                {
                    revenue = re;
                    count = 0;
                }
                count++;
                var test = db.Tests.Add(new Test
                {
                    TestCaseId = testcase.Id,
                    NoGen = result.noGen,
                    Revenue = (float)revenue,
                    RevenueIteration = (float)re,
                    Children = children.Count
                });
                if (count > nochange)
                {
                    break;
                }
            }

            watch.Stop();
            elapsedH1 = watch.ElapsedMilliseconds;

            testcase.NoGen = result.noGen;
            testcase.MaxGen = revenue;
            testcase.ElapseTime = (int)elapsedH1;

            db.SaveChanges();

            var firstOrDefault = initPopulation.FirstOrDefault();
            if (firstOrDefault != null) result.Choosen = firstOrDefault.Choosen;
            result.Elapsed = (long)elapsedH1;
            result.Revenue = Utility.CalculateRevenue(myOriginalCase, result.Choosen);
            return result;
        }
    }
}
