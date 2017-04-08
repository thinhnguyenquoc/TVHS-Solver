using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGenForTVHS
{
    class Program
    {
        #region input           
        public static List<string> fileList = new List<string>(){
                @"..\..\..\..\TVHS_Data_test\3-8_9-8_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\7-7_12-7_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\7-9_13-9_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\10-8_16-8_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\13-7_19-7_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\14-9_20-9_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\17-8_23-8_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\20-7_26-7_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\21-9_27-9_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\24-8_30-8_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\27-7_2-8_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\31-8_6-9_2015\F0-F10.xlsx",
            };

        #endregion
        public static Random r = new Random();
        public static double revenue = 0;
        public static int count = 0;
        static void Main(string[] args)
        {
            foreach (var filename in fileList)
            {
                MyCase input = InitData(filename);
                Gen(500, 500, 0.4, 360, 1000, 0.01, input);
            }
        }

        public static void Gen(int initSize, int populationSize, double percentCrossOver, int nochange, int noloop, double mutationPercent, MyCase input)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<Chromosome> initPopulation = CreateInitPopulation(initSize, input);

            for (int i = 0; i < initPopulation.Count; i++)
            {
                var rev = strategy(input, initPopulation[i]);
                var avl = Validate.ValidateResult(input, (int[])initPopulation[i].Solution);
                var id = "";
                foreach (var a in initPopulation[i].ATP)
                {
                    id += a.RandomNo.ToString();
                }
                if (rev.Revenue > revenue)
                {
                    revenue = rev.Revenue;
                }
            }

            for (var lop = 0; lop < noloop; lop++)
            {
                var parents = SelectParents(initPopulation, percentCrossOver);
                var children = MakeChildren(parents);
                initPopulation = initPopulation.Concat(children).ToList();
                initPopulation = Repopulate(input, initPopulation, populationSize);
                Mutate(initPopulation, mutationPercent, input);
                /*double re = initPopulation.OrderByDescending(x => x.revenue).FirstOrDefault().revenue;*/
                double eachRev = 0;
                for (int i = 0; i < initPopulation.Count; i++)
                {
                    var rev = strategy(input, initPopulation[i]);
                    if (rev.Revenue > revenue)
                    {
                        eachRev = rev.Revenue;
                    }
                }
                if (eachRev > revenue)
                {
                    revenue = eachRev;
                    count = 0;
                }
                count++;

                if (count > nochange)
                {
                    break;
                }
            }

            watch.Stop();
            var elapsedH1 = watch.ElapsedMilliseconds;
            //return result;
        }

        public static MyStatictis strategy(MyCase input, Chromosome chromosome)
        {
            var programs = input.Programs.DeepCopyByExpressionTree();
            for (int i = 0; i < programs.Count; i++)
            {
                programs[i].MaxShowTime = chromosome.ATP[i].RandomNo;
            }
            var myGroups = input.Groups.DeepCopyByExpressionTree();
            var myFrame = input.Frames.DeepCopyByExpressionTree();
            //var maxNos = chromosome.DeepCopyByExpressionTree();
            #region init array
            int[] Choosen = new int[input.Times.Count];
            for (int i = 0; i < input.Times.Count; i++)
            {
                Choosen[i] = -1;
            }
            #endregion
            // check ability of assignment
            var watch = System.Diagnostics.Stopwatch.StartNew();
            bool change = false;
            #region assign program to frame     
            // assign one program one time
            var listProgram = programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
            foreach (var item in listProgram)
            {
                var gr = myGroups.Where(x => x.Id == item.GroupId).FirstOrDefault();
                if (gr.TotalTime >= item.Duration)
                {
                    foreach (var frame in item.FrameList)
                    {
                        int startAv = Utility.GetFirstAvailableSlotInFrame(input, Choosen, frame);
                        #region find a empty space to assign
                        if (startAv != -1)
                        {
                            if (Utility.CheckAssignableProgram(input, Choosen, startAv, item))
                            {
                                if (Utility.CheckTooClose(input, Choosen, startAv, item))
                                {
                                    ////assign program to frame
                                    for (int j = 0; j < item.Duration; j++)
                                    {
                                        Choosen[startAv] = item.Id;
                                        startAv++;
                                    }
                                    //// decrease the maximum show time of this program
                                    item.MaxShowTime--;
                                    gr.TotalTime -= item.Duration;
                                    change = true;
                                    break;
                                }
                            }
                        }
                        #endregion
                    }
                }
            }

            while (true)
            {
                //find special program first
                var listSpecial = programs.Where(x => x.MaxShowTime > 0 && x.FrameList.Count < 11).OrderByDescending(x => x.Efficiency).ToList();
                if (listSpecial.Count > 0)
                {
                    foreach (var item in listSpecial)
                    {
                        var gr = myGroups.Where(x => x.Id == item.GroupId).FirstOrDefault();
                        #region group still has cota
                        if (gr.TotalTime >= item.Duration)
                        {
                            foreach (var frame in item.FrameList)
                            {
                                int startAv = Utility.GetFirstAvailableSlotInFrame(input, Choosen, frame);
                                #region find a empty space to assign
                                if (startAv != -1)
                                {
                                    if (Utility.CheckAssignableProgram(input, Choosen, startAv, item))
                                    {
                                        if (Utility.CheckTooClose(input, Choosen, startAv, item))
                                        {
                                            ////assign program to frame
                                            for (int j = 0; j < item.Duration; j++)
                                            {
                                                Choosen[startAv] = item.Id;
                                                startAv++;
                                            }
                                            //// decrease the maximum show time of this program
                                            item.MaxShowTime--;
                                            gr.TotalTime -= item.Duration;
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
                }
                else
                {
                    var listUsual = programs.Where(x => x.MaxShowTime > 0 && x.FrameList.Count == 11).OrderByDescending(x => x.Efficiency).ToList();
                    foreach (var item in listUsual)
                    {
                        var gr = myGroups.Where(x => x.Id == item.GroupId).FirstOrDefault();
                        #region group still has cota
                        if (gr.TotalTime >= item.Duration)
                        {
                            foreach (var frame in item.FrameList)
                            {
                                int startAv = Utility.GetFirstAvailableSlotInFrame(input, Choosen, frame);
                                #region find a empty space to assign
                                if (startAv != -1)
                                {
                                    if (Utility.CheckAssignableProgram(input, Choosen, startAv, item))
                                    {
                                        if (Utility.CheckTooClose(input, Choosen, startAv, item))
                                        {
                                            ////assign program to frame
                                            for (int j = 0; j < item.Duration; j++)
                                            {
                                                Choosen[startAv] = item.Id;
                                                startAv++;
                                            }
                                            //// decrease the maximum show time of this program
                                            item.MaxShowTime--;
                                            gr.TotalTime -= item.Duration;
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
                //// update frame unoccupate
                for (int i = 0; i < myFrame.Count; i++)
                {
                    var unocc = 0;
                    for (int j = myFrame[i].Start; j <= myFrame[i].End; j++)
                    {
                        if (Choosen[j - 1] == -1)
                        {
                            unocc += 1;
                        }
                    }
                    myFrame[i].Unoccupate = unocc;
                }
                List<int> Unoccupate = new List<int>();
                for (int i = 0; i < myFrame.Count - 1; i++)
                {
                    Unoccupate.Add(myFrame[i].Unoccupate + myFrame[i + 1].Unoccupate);
                }
                var list2 = programs.Where(x => x.MaxShowTime > 0).OrderByDescending(x => x.Efficiency).ToList();
                var item = list2.FirstOrDefault();

                var gr = myGroups.Where(x => x.Id == item.GroupId).FirstOrDefault();
                if (gr.TotalTime >= item.Duration)
                {
                    for (int i = 0; i < Unoccupate.Count; i++)
                    {
                        if (item.Duration <= Unoccupate[i])
                        {
                            //check allowed frames
                            var FrameIdList = item.FrameList.Select(x => x.Id).ToList();
                            //add program to time frame
                            if (FrameIdList.Contains(i))
                            {
                                ////check available slot                      
                                int startAv = myFrame[i].End - myFrame[i].Unoccupate;
                                if (startAv != myFrame[i].End)
                                {
                                    if (Utility.CheckTooClose(input, Choosen, startAv, item))
                                    {
                                        int shift = item.Duration - myFrame[i].Unoccupate;
                                        // ShiftRight
                                        Utility.ShiftRight(input, Choosen, shift, i + 1, Unoccupate);
                                        ////assign program to frame
                                        for (int j = 0; j < item.Duration; j++)
                                        {
                                            Choosen[startAv] = item.Id;
                                            startAv++;
                                        }
                                        //// decrease the maximum show time of this program
                                        item.MaxShowTime--;
                                        gr.TotalTime -= item.Duration;
                                        change = true;
                                        break;
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

            watch.Stop();
            var elapsedH1 = watch.ElapsedMilliseconds;
            MyStatictis result = new MyStatictis();
            result.Choosen = Choosen;
            result.Elapsed = elapsedH1;
            result.Revenue = Utility.CalculateRevenue(input, Choosen);
            chromosome.Solution = Choosen;
            return result;
        }



        public static List<Chromosome> CreateInitPopulation(int initSize, MyCase input)
        {
            var initPopulation = new List<Chromosome>();
            for (int i = 0; i < initSize; i++)
            {
                var individual = new Chromosome();
                for (int j = 0; j < input.Programs.Count; j++)
                {
                    individual.ATP.Add(new ATP()
                    {
                        RandomNo = GetRandom(input.Programs[j].MaxShowTime),
                        ProgramId = input.Programs[j].Id
                    });
                }
                initPopulation.Add(individual);
            }
            return initPopulation;
        }

        public static int GetRandom(int max)
        {
            return r.Next(1, max + 1);
        }

        public static List<Chromosome> SelectParents(List<Chromosome> initPopulation, double percentCrossOver)
        {
            var listIds = new List<int>();
            for (int i = 0; i < initPopulation.Count; i++)
                listIds.Add(i);
            var parents = new List<Chromosome>();
            var index = 0;
            while (parents.Count < initPopulation.Count() * percentCrossOver)
            {
                var randomIndex = r.Next(0, listIds.Count);
                parents.Add(initPopulation[randomIndex]);
                listIds.RemoveAt(randomIndex);
                index++;
            }
            return parents;
        }

        public static List<Chromosome> MakeChildren(List<Chromosome> parents)
        {
            var children = new List<Chromosome>();
            var listIds = new List<int>();
            for (int i = 0; i < parents.Count; i++)
                listIds.Add(i);
            while (children.Count < parents.Count())
            {
                var randomIndex1 = r.Next(0, listIds.Count);
                var parent1 = parents[randomIndex1];
                if (listIds.Count != 0)
                    listIds.RemoveAt(randomIndex1);
                var randomIndex2 = r.Next(0, listIds.Count);
                var parent2 = parents[randomIndex1];
                if (listIds.Count != 0)
                    listIds.RemoveAt(randomIndex2);
                var child = CrossOver(parent1, parent2);
                children = children.Concat(child).ToList();
            }
            return children;
        }

        public static List<Chromosome> CrossOver(Chromosome parent1, Chromosome parent2)
        {
            var child1 = new Chromosome();
            var child2 = new Chromosome();
            var splitPoint = 5;
            for (int i = 0; i < parent1.ATP.Count; i++)
            {
                if (i < splitPoint)
                {
                    child1.ATP.Add(parent1.ATP[i]);
                    child2.ATP.Add(parent2.ATP[i]);
                }
                else
                {
                    child1.ATP.Add(parent2.ATP[i]);
                    child2.ATP.Add(parent1.ATP[i]);
                }
            }
            var result = new List<Chromosome>();
            result.Add(child1);
            result.Add(child2);
            return result;
        }

        public static List<Chromosome> Repopulate(MyCase input, List<Chromosome> initPopulation, int populationSize)
        {
            var children = new List<MyStatictis>();
            for (int i = 0; i < initPopulation.Count; i++)
            {
                var rev = strategy(input, initPopulation[i]);
                rev.Ratio = i;
                children.Add(rev);
            }
            var choosen = children.OrderByDescending(x => x.Revenue).Take(populationSize).ToList();
            var choosenChromosome = new List<Chromosome>();
            choosen.ForEach(x => choosenChromosome.Add(initPopulation[(int)x.Ratio]));
            return choosenChromosome;
        }

        public static void Mutate(List<Chromosome> population, double mutationPercent, MyCase input)
        {
            var listIds = new List<int>();
            for (int i = 0; i < population.Count; i++)
                listIds.Add(i);
            var index = 0;
            while (index < population.Count() * mutationPercent)
            {
                var randomIndex = r.Next(0, listIds.Count);
                var chromosome = population[randomIndex];
                var indexMutation = r.Next(0, chromosome.ATP.Count);
                chromosome.ATP[indexMutation].RandomNo = r.Next(1, input.Programs[indexMutation].MaxShowTime + 1);
                listIds.RemoveAt(randomIndex);
                index++;
            }
        }

        public class Chromosome
        {
            public Chromosome()
            {
                ATP = new List<Program.ATP>();
            }

            public List<ATP> ATP { get; set; }
            public Object Solution { get; set; }
        }

        public class ATP
        {
            public int ProgramId { get; set; }
            public int RandomNo { get; set; }
            public int FixedNo { get; set; }
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
