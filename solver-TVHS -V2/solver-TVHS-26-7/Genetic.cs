using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace solver_TVHS_26_7
{
    public class Genetic
    {
        private Dictionary<string, EvaluatePopulation> _dnaDictionary = new Dictionary<string, EvaluatePopulation>();
        private Dictionary<string, List<EvaluatePopulation>> _coupleDictionary = new Dictionary<string, List<EvaluatePopulation>>();
        private Dictionary<string, EvaluatePopulation> _fixChildDictionary = new Dictionary<string, EvaluatePopulation>();
        private readonly MD5 _md5 = System.Security.Cryptography.MD5.Create();

        public List<EvaluatePopulation> CreateInitPopulationTree(MyCase myOriginalCase, int size, bool isInsert)
        {
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
                #region assign program to frame             
                while (true)
                {
                    var list1 = Utility.RandomProgramListTree(myCase.Programs.Where(x => x.MaxShowTime > 0).ToList());
                    foreach (var item in list1)
                    {
                        var gr = myCase.Groups.First(x => x.Id == item.GroupId);
                        #region group still has cota
                        if (gr.TotalTime >= item.Duration)
                        {
                            var frameList = Utility.RandomFrameListTree(item.FrameList);
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
                if (isInsert)
                {
                    change = false;
                    // calculate the unoccupate of two continue frame                    
                    while (true)
                    {
                        var list2 = Utility.RandomProgramListTree(myCase.Programs.Where(x => x.MaxShowTime > 0).ToList());
                        Utility.UpdateUnOccupiedFrameTime(myCase, choosen);
                        var unoccupate = new List<int>();
                        for (var i = 0; i < myCase.Frames.Count - 1; i++)
                        {
                            unoccupate.Add(myCase.Frames[i].Unoccupate + myCase.Frames[i + 1].Unoccupate);
                        }

                        foreach (var pro in list2)
                        {
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
                }

                #endregion
                var e = new EvaluatePopulation { Choosen = choosen };
                e.revenue = Utility.CalculateRevenue(myOriginalCase, choosen);
                result.Add(e);
            }
            return result;
        }

        public string ConvertArrayToString(int[] choose)
        {
            string result = "";
            foreach (var character in choose)
            {
                result += character.ToString();
            }
            return result;
        }

        public string CalculateMd5Hash(string input, MD5 md5)
        {

            // step 1, calculate MD5 hash from input

            
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)

            {

                sb.Append(hash[i].ToString("x2"));

            }

            return sb.ToString();

        }

        public List<EvaluatePopulation> CreateInitPopulationTreeV2(MyCase myOriginalCase, int size, bool isInsert)
        {
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
                #region assign program to frame             
                while (true)
                {
                    var list1 = Utility.RandomProgramListTree(myCase.Programs.Where(x => x.MaxShowTime > 0).ToList());
                    foreach (var item in list1)
                    {
                        var gr = myCase.Groups.First(x => x.Id == item.GroupId);
                        #region group still has cota
                        if (gr.TotalTime >= item.Duration)
                        {
                            var frameList = Utility.RandomFrameListTree(item.FrameList);
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
                if (isInsert)
                {
                    change = false;
                    // calculate the unoccupate of two continue frame                    
                    while (true)
                    {
                        var list2 = Utility.RandomProgramListTree(myCase.Programs.Where(x => x.MaxShowTime > 0).ToList());
                        Utility.UpdateUnOccupiedFrameTime(myCase, choosen);
                        var unoccupate = new List<int>();
                        for (var i = 0; i < myCase.Frames.Count - 1; i++)
                        {
                            unoccupate.Add(myCase.Frames[i].Unoccupate + myCase.Frames[i + 1].Unoccupate);
                        }

                        foreach (var pro in list2)
                        {
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
                }

                #endregion

                var md5String = CalculateMd5Hash(ConvertArrayToString(choosen), _md5);
                if (!_dnaDictionary.ContainsKey(md5String))
                {
                    var e = new EvaluatePopulation
                    {
                        Choosen = choosen,
                        revenue = Utility.CalculateRevenue(myOriginalCase, choosen)
                    };
                    result.Add(e);
                    _dnaDictionary.Add(md5String,e);
                }
            }
            return result;
        }

        public bool CheckExist(List<EvaluatePopulation> population, EvaluatePopulation e)
        {
            foreach (var p in population)
            {
                if (p.Choosen.SequenceEqual(e.Choosen))
                {
                    return true;
                }
            }
            return false;
        }

        // round robin - monte carlo
        public List<List<EvaluatePopulation>> SelectParentsTreeRoundRobin(MyCase myOriginalCase, List<EvaluatePopulation> population, double percentCrossOver)
        {
            List<EvaluatePopulation> cache = new List<EvaluatePopulation>();
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
            while (parents.Count() < population.Count() * percentCrossOver / 2)
            {
                var couple = new List<EvaluatePopulation>();
                var r1 = rnd.NextDouble();
                var r2 = rnd.NextDouble();
                var a1 = population.FirstOrDefault(x => x.min < r1 && x.max >= r1);
                var a2 = population.FirstOrDefault(x => x.min < r2 && x.max >= r2);
                if (a1 == null || a2 == null)
                {
                    continue;
                }
                couple.Add(a1);
                couple.Add(a2);
                if (CheckExist(cache, a1) && CheckExist(cache, a2))
                {
                    continue;
                }
                parents.Add(couple);
            }
            return parents;
        }

        // choose strong fathers
        public List<List<EvaluatePopulation>> SelectParentsTreeHalfRandom(MyCase myOriginalCase, List<EvaluatePopulation> population, double percentCrossOver)
        {
            var fathers =
                population.OrderByDescending(x => x.revenue)
                    .Take(Convert.ToInt32(population.Count() * percentCrossOver / 2) + 1)
                    .ToList();
            var mothers = population.OrderByDescending(x => x.revenue)
                    .Skip(Convert.ToInt32(population.Count() * percentCrossOver / 2))
                    .ToList();
            var rnd = new Random();
            var parents = new List<List<EvaluatePopulation>>();
            var i = 0;
            while (parents.Count() < population.Count() * percentCrossOver / 2)
            {
                var couple = new List<EvaluatePopulation> { fathers[i++] };
                var r1 = rnd.Next(0, mothers.Count - 1);
                couple.Add(mothers[r1]);
                parents.Add(couple);
            }
            return parents;
        }

        public List<EvaluatePopulation> MakeChildrenTree(MyCase myOriginalCase, List<List<EvaluatePopulation>> parents, bool IsInsert)
        {
            var result = new List<EvaluatePopulation>();
            for (int i = 0; i < parents.Count(); i++)
            {
                var md5String = CalculateMd5Hash(ConvertArrayToString(parents[i][0].Choosen)+"_"+ ConvertArrayToString(parents[i][1].Choosen), _md5);
                if (!_coupleDictionary.ContainsKey(md5String))
                {
                    var children = SingleCrossTree(myOriginalCase, parents[i], IsInsert);
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

        public List<EvaluatePopulation> SingleCrossTree(MyCase myOriginalCase, List<EvaluatePopulation> couple, bool IsInsert)
        {
            var parent1 = Utility.ConvertToProgram(myOriginalCase, couple[0].Choosen);
            var parent2 = Utility.ConvertToProgram(myOriginalCase, couple[1].Choosen);
            // random split => read more paper
            var splitPoint = Convert.ToInt32(Math.Min(parent1.Count(), parent2.Count()) * GetRandomNumber(0.1, 0.9));
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
            var newChild1 = FixChildTree(myOriginalCase, child1, IsInsert);
            if (newChild1 != null)
                result.Add(newChild1);
            var newChild2 = FixChildTree(myOriginalCase, child2, IsInsert);
            if (newChild2 != null)
                result.Add(newChild2);
            return result;
        }

        public EvaluatePopulation FixChildTreeV2(MyCase myOriginalCase, List<MyProgram> originalChild, bool IsInsert)
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
            var md5str = CalculateMd5Hash(ConvertArrayToString(list1.Select(x => x.Id).ToArray()),_md5);
            if (_fixChildDictionary.ContainsKey(md5str))
            {
                return _fixChildDictionary[md5str];
            }
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
            if (IsInsert)
            {
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
                    var list2 =
                        myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
                    foreach (var pro in list2)
                    {
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
            }

            #endregion

            var va = Validate.ValidateResult(myOriginalCase, choosen);
            if (va.Where(x => x != 0).Any())
            {
                _fixChildDictionary.Add(md5str, null);
                return null;
            }
            else
            {
                var e = new EvaluatePopulation();
                e.Choosen = choosen;
                e.revenue = Utility.CalculateRevenue(myOriginalCase, choosen);
                _fixChildDictionary.Add(md5str, e);
                return e;
            }

        }

        public EvaluatePopulation FixChildTree(MyCase myOriginalCase, List<MyProgram> originalChild, bool IsInsert)
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
            if (IsInsert)
            {
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
                    var list2 =
                        myCase.Programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
                    foreach (var pro in list2)
                    {
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

        public double GetRandomNumber(double minimum, double maximum)
        {
            var random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        //choose the best
        public List<EvaluatePopulation> ReproductionTheBest(List<EvaluatePopulation> population, int Size)
        {
            var theBest = population.OrderByDescending(x => x.revenue).Take(Size).ToList();
            return theBest;
        }

        public List<EvaluatePopulation> Reproduction8020(List<EvaluatePopulation> population, int size)
        {
            var ran = new Random();
            var theBest = population.OrderByDescending(x => x.revenue).Take(size * 20 / 100).ToList();
            var remain = population.OrderByDescending(x => x.revenue).Skip(size * 20 / 100).ToList();
            while (theBest.Count < size && theBest.Count < population.Count)
            {
                var index = ran.Next(0, remain.Count - 1);
                theBest.Add(remain[index]);
                remain.RemoveAt(index);
            }
            return theBest;
        }

        //replace the old chromosome by mutaion chromosome
        public List<EvaluatePopulation> MutateTree(MyCase myOriginalCase, List<EvaluatePopulation> population, double percent, bool IsInsert)
        {
            Random ran = new Random();
            List<int> indexSave = new List<int>();
            while (indexSave.Count() <= population.Count * percent)
            {
                int index = ran.Next(population.Count);
                if (!indexSave.Contains(index))
                {
                    List<MyProgram> chromosome = Utility.ConvertToProgram(myOriginalCase, population[index].Choosen);
                    int indexMutation = ran.Next(chromosome.Count);
                    int indexProgram = ran.Next(myOriginalCase.Programs.Count);
                    chromosome[indexMutation] = myOriginalCase.Programs[indexProgram];
                    var fixedChild = FixChildTree(myOriginalCase, chromosome, IsInsert);
                    if (fixedChild != null)
                    {
                        population[index] = fixedChild;
                        indexSave.Add(index);
                    }

                }
            }
            return population;
        }


        public MyStatictis Solve1(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename)
        {
            var myOriginalCase = input;
            double revenue = 0;
            var result = new MyStatictis();
            result.noGen = 1;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulationTree(myOriginalCase, initSize, false);
            revenue = initPopulation.Max(x => x.revenue);
            var elapsedH1 = 0.0;
            int count = 0;
            var db = new TVHS();
            var testcase = db.TestCases.Add(new TestCase
            {
                TestCase1 = filename.Split(new string[] { "TVHS_Data_test" }, StringSplitOptions.None).Last(),
                Kind = "RoundRobin - NoInsert - TheBestParent",
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
                var children = MakeChildrenTree(myOriginalCase, parents, false);
                initPopulation = initPopulation.Concat(children).ToList();
                initPopulation = MutateTree(myOriginalCase, initPopulation, mutationPercent, false);
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

        // insert
        public MyStatictis Solve2(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename)
        {
            var myOriginalCase = input;
            double revenue = 0;
            var result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulationTree(myOriginalCase, initSize, false);
            revenue = initPopulation.Max(x => x.revenue);
            var elapsedH1 = 0.0;
            int count = 0;
            var db = new TVHS();
            var testcase = db.TestCases.Add(new TestCase
            {
                TestCase1 = filename.Split(new string[] { "TVHS_Data_test" }, StringSplitOptions.None).Last(),
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
                var children = MakeChildrenTree(myOriginalCase, parents, true);
                initPopulation = initPopulation.Concat(children).ToList();
                initPopulation = MutateTree(myOriginalCase, initPopulation, mutationPercent, true);
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

        public MyStatictis Solve3(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename)
        {
            var myOriginalCase = input;
            double revenue = 0;
            var result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulationTree(myOriginalCase, initSize, false);
            revenue = initPopulation.Max(x => x.revenue);
            var elapsedH1 = 0.0;
            int count = 0;
            var db = new TVHS();
            var testcase = db.TestCases.Add(new TestCase
            {
                TestCase1 = filename.Split(new string[] { "TVHS_Data_test" }, StringSplitOptions.None).Last(),
                Kind = "HalfRandom - Insert - TheBestParent",
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
                // using half random
                var parents = SelectParentsTreeHalfRandom(myOriginalCase, initPopulation, percentCrossOver);
                var children = MakeChildrenTree(myOriginalCase, parents, true);
                initPopulation = initPopulation.Concat(children).ToList();
                initPopulation = MutateTree(myOriginalCase, initPopulation, mutationPercent, true);
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

        public MyStatictis Solve4(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename)
        {
            var myOriginalCase = input;
            double revenue = 0;
            var result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulationTree(myOriginalCase, initSize, false);
            revenue = initPopulation.Max(x => x.revenue);
            var elapsedH1 = 0.0;
            int count = 0;
            var db = new TVHS();
            var testcase = db.TestCases.Add(new TestCase
            {
                TestCase1 = filename.Split(new string[] { "TVHS_Data_test" }, StringSplitOptions.None).Last(),
                Kind = "HalfRandom - Insert - 8020",
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
                // using half random
                var parents = SelectParentsTreeHalfRandom(myOriginalCase, initPopulation, percentCrossOver);
                var children = MakeChildrenTree(myOriginalCase, parents, true);
                initPopulation = initPopulation.Concat(children).ToList();
                initPopulation = MutateTree(myOriginalCase, initPopulation, mutationPercent, true);
                initPopulation = Reproduction8020(initPopulation, populationSize);
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

        public MyStatictis Solve5(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename)
        {
            var myOriginalCase = input;
            double revenue = 0;
            var result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulationTree(myOriginalCase, initSize, false);
            revenue = initPopulation.Max(x => x.revenue);
            var elapsedH1 = 0.0;
            int count = 0;
            var db = new TVHS();
            var testcase = db.TestCases.Add(new TestCase
            {
                TestCase1 = filename.Split(new string[] { "TVHS_Data_test" }, StringSplitOptions.None).Last(),
                Kind = "RoundRobin - Insert - 8020",
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
                // using half random
                var parents = SelectParentsTreeRoundRobin(myOriginalCase, initPopulation, percentCrossOver);
                var children = MakeChildrenTree(myOriginalCase, parents, true);
                initPopulation = initPopulation.Concat(children).ToList();
                initPopulation = MutateTree(myOriginalCase, initPopulation, mutationPercent, true);
                initPopulation = Reproduction8020(initPopulation, populationSize);
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

        // add hueristic
        public MyStatictis Solve6(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename, List<int[]> heuristics)
        {
            if (!File.Exists(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_noInsertGen_Heuristic_" + percentCrossOver + "_" + mutationPercent + ".txt"))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_noInsertGen_Heuristic_" + percentCrossOver + "_" + mutationPercent + ".txt"))
                {
                }
            }
            var myOriginalCase = input;
            double revenue = 0;
            var result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulationTree(myOriginalCase, initSize, false);
            foreach (var i in heuristics)
            {
                EvaluatePopulation e = new EvaluatePopulation();
                e.Choosen = i;
                e.revenue = Utility.CalculateRevenue(myOriginalCase, i);
                initPopulation.Add(e);
            }
            revenue = initPopulation.Max(x => x.revenue);
            var elapsedH1 = 0.0;
            int count = 0;
            var db = new TVHS();
            var testcase = db.TestCases.Add(new TestCase
            {
                TestCase1 = filename.Split(new string[] { "TVHS_Data_test" }, StringSplitOptions.None).Last(),
                Kind = "RoundRobin - noInsert - 8020 - TheBestParent - Heuristic",
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
                var children = MakeChildrenTree(myOriginalCase, parents, false);
                initPopulation = initPopulation.Concat(children).ToList();
                initPopulation = MutateTree(myOriginalCase, initPopulation, mutationPercent, false);
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


        public MyStatictis SolveV2_1(MyCase input, int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, string filename)
        {
            var myOriginalCase = input;
            double revenue = 0;
            var result = new MyStatictis();
            result.noGen = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var initPopulation = CreateInitPopulationTreeV2(myOriginalCase, initSize, true);
            revenue = initPopulation.Max(x => x.revenue);
            var elapsedH1 = 0.0;
            int count = 0;
            var db = new TVHS();
            var testcase = db.TestCases.Add(new TestCase
            {
                TestCase1 = filename.Split(new string[] { "TVHS_Data_test" }, StringSplitOptions.None).Last() +"-V2",
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
                var children = MakeChildrenTree(myOriginalCase, parents, true);
                initPopulation = initPopulation.Concat(children).ToList();
                initPopulation = MutateTree(myOriginalCase, initPopulation, mutationPercent, true);
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
    public class EvaluatePopulation
    {
        public double revenue { get; set; }
        public double min { get; set; }
        public double max { get; set; }
        public int[] Choosen { get; set; }
        public bool CanBorn { get; set; }
    }

}
