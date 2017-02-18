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
                //@"..\..\..\..\TVHS_Data_test\7-7_12-7_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\13-7_19-7_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\20-7_26-7_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\27-7_2-8_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\3-8_9-8_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\10-8_16-8_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\17-8_23-8_2015\F0-F10.xlsx",
                @"..\..\..\..\TVHS_Data_test\24-8_30-8_2015\F0-F10.xlsx",

                //@"..\..\..\..\TVHS_Data_test\7-9_13-9_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\14-9_20-9_2015\F0-F10.xlsx",
                //@"..\..\..\..\TVHS_Data_test\21-9_27-9_2015\F0-F10.xlsx",

                //@"..\..\..\..\TVHS_Data_test\31-8_6-9_2015\F0-F10.xlsx",
                //2016

            };

            #endregion
            #region header
            if (!File.Exists(@"..\..\..\..\TVHS_Data_test\Result\Gen.txt"))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(@"..\..\..\..\TVHS_Data_test\Result\Gen.txt"))
                {
                    sw.WriteLine("Test name \t Solver \t Solver elasped \t G1 \t G1 elapsed \t gennum \t time");
                }
            }
            using (System.IO.StreamWriter file = File.AppendText(@"..\..\..\..\TVHS_Data_test\Result\Gen.txt"))
            {
                file.WriteLine("");
            }
            if (!File.Exists(@"..\..\..\..\TVHS_Data_test\Result\Heuristic.txt"))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(@"..\..\..\..\TVHS_Data_test\Result\Heuristic.txt"))
                {
                    sw.WriteLine("Test name \t Solver \t Solver elasped \t H1 \t H1 elapsed \t H2 \t H2 elapsed \t H3 \t H3 elapsed \t H4 \t H4 elapsed \t time");
                }
            }

            using (System.IO.StreamWriter file = File.AppendText(@"..\..\..\..\TVHS_Data_test\Result\Heuristic.txt"))
            {
                file.WriteLine("");
            }

            if (!File.Exists(@"..\..\..\..\TVHS_Data_test\Result\GA1.txt"))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(@"..\..\..\..\TVHS_Data_test\Result\GA1.txt"))
                {
                    sw.WriteLine("Test name \t Solver \t Solver elasped \t G1 \t G1 elapsed \t gennum \t time");
                }
            }
            using (System.IO.StreamWriter file = File.AppendText(@"..\..\..\..\TVHS_Data_test\Result\GA1.txt"))
            {
                file.WriteLine("");
            }
            #endregion
            foreach (var filename in fileList)
            {
                var input = InitData(filename);
                #region call solver
                //var solver = new Solver();
                //var solveResult = solver.Solve(input, filename);
                #endregion

                #region read solver result 

                //string solverUrl = filename.Split(new string[] { ".xlsx" }, StringSplitOptions.None).FirstOrDefault() + "_resultBS.txt";
                //string[] lines = System.IO.File.ReadAllLines(solverUrl);
                //foreach (string line in lines)
                //{
                //    if (line.Contains("RBS"))
                //    {
                //        var a = line.Split(new string[] { "\t" }, StringSplitOptions.None);
                //        solverResult = Convert.ToDouble(a[1]);
                //    }
                //    if (line.Contains("total time of"))
                //    {
                //        var a = line.Split(new string[] { "total time of" }, StringSplitOptions.None)[1];
                //        var b = a.Split(new string[] { "ms" }, StringSplitOptions.None)[0];
                //        elapsedSolver = Convert.ToDouble(b);
                //    }
                //}
                #endregion

                #region calculate heuristic  

                var heuristic = new Heuristic();
                var heuristicResult1 = heuristic.strategy1(input, filename);
                var heuristicResult2 = heuristic.strategy2(input, filename);
                var heuristicResult3 = heuristic.strategy3(input, filename);
                var heuristics = new List<int[]>
                {
                    heuristicResult1.Choosen,
                    heuristicResult2.Choosen,
                    heuristicResult3.Choosen
                };

                #endregion

                //#region gen parameter
                //var gen = new Genetic();
                ////10% 0.5
                //var genResult1 = gen.Solve1(input, 100, 500, 0.1, 60, 500, 0.005, filename);
                //var genResult2 = gen.Solve2(input, 100, 500, 0.1, 60, 500, 0.005, filename);
                //var genResult3 = gen.Solve3(input, 100, 500, 0.1, 60, 500, 0.005, filename);
                //var genResult4 = gen.Solve4(input, 100, 500, 0.1, 60, 500, 0.005, filename);
                //var genResult5 = gen.Solve5(input, 100, 500, 0.1, 60, 500, 0.005, filename);
                //var genResult6 = gen.Solve6(input, 100, 500, 0.1, 60, 500, 0.005, filename, heuristics);
                ////20% 0.5coi 
                //var genResult21 = gen.Solve1(input, 100, 500, 0.2, 60, 500, 0.005, filename);
                //var genResult22 = gen.Solve2(input, 100, 500, 0.2, 60, 500, 0.005, filename);
                //var genResult23 = gen.Solve3(input, 100, 500, 0.2, 60, 500, 0.005, filename);
                //var genResult24 = gen.Solve4(input, 100, 500, 0.2, 60, 500, 0.005, filename);
                //var genResult25 = gen.Solve5(input, 100, 500, 0.2, 60, 500, 0.005, filename);
                //var genResult26 = gen.Solve6(input, 100, 500, 0.2, 60, 500, 0.005, filename, heuristics);
                //////30% 0.5
                //var genResult31 = gen.Solve1(input, 100, 500, 0.3, 60, 500, 0.005, filename);
                //var genResult32 = gen.Solve2(input, 100, 500, 0.3, 60, 500, 0.005, filename);
                //var genResult33 = gen.Solve3(input, 100, 500, 0.3, 60, 500, 0.005, filename);
                //var genResult34 = gen.Solve4(input, 100, 500, 0.3, 60, 500, 0.005, filename);
                //var genResult35 = gen.Solve5(input, 100, 500, 0.3, 60, 500, 0.005, filename);
                //var genResult36 = gen.Solve6(input, 100, 500, 0.3, 60, 500, 0.005, filename, heuristics);

                ////10% 1
                //var genResult41 = gen.Solve1(input, 100, 500, 0.1, 60, 500, 0.01, filename);
                //var genResult42 = gen.Solve2(input, 100, 500, 0.1, 60, 500, 0.01, filename);
                //var genResult43 = gen.Solve3(input, 100, 500, 0.1, 60, 500, 0.01, filename);
                //var genResult44 = gen.Solve4(input, 100, 500, 0.1, 60, 500, 0.01, filename);
                //var genResult45 = gen.Solve5(input, 100, 500, 0.1, 60, 500, 0.01, filename);
                //var genResult46 = gen.Solve6(input, 100, 500, 0.1, 60, 500, 0.01, filename, heuristics);
                ////20% 1
                //var genResult421 = gen.Solve1(input, 100, 500, 0.2, 60, 500, 0.01, filename);
                //var genResult422 = gen.Solve2(input, 100, 500, 0.2, 60, 500, 0.01, filename);
                //var genResult423 = gen.Solve3(input, 100, 500, 0.2, 60, 500, 0.01, filename);
                //var genResult424 = gen.Solve4(input, 100, 500, 0.2, 60, 500, 0.01, filename);
                //var genResult425 = gen.Solve5(input, 100, 500, 0.2, 60, 500, 0.01, filename);
                //var genResult426 = gen.Solve6(input, 100, 500, 0.2, 60, 500, 0.01, filename, heuristics);
                //////30% 1
                //var genResult431 = gen.Solve1(input, 100, 500, 0.3, 60, 500, 0.01, filename);
                //var genResult432 = gen.Solve2(input, 100, 500, 0.3, 60, 500, 0.01, filename);
                //var genResult433 = gen.Solve3(input, 100, 500, 0.3, 60, 500, 0.01, filename);
                //var genResult434 = gen.Solve4(input, 100, 500, 0.3, 60, 500, 0.01, filename);
                //var genResult435 = gen.Solve5(input, 100, 500, 0.3, 60, 500, 0.01, filename);
                //var genResult436 = gen.Solve6(input, 100, 500, 0.3, 60, 500, 0.01, filename, heuristics);

                ////10% 5
                //var genResult51 = gen.Solve1(input, 100, 500, 0.1, 60, 500, 0.05, filename);
                //var genResult52 = gen.Solve2(input, 100, 500, 0.1, 60, 500, 0.05, filename);
                //var genResult53 = gen.Solve3(input, 100, 500, 0.1, 60, 500, 0.05, filename);
                //var genResult54 = gen.Solve4(input, 100, 500, 0.1, 60, 500, 0.05, filename);
                //var genResult55 = gen.Solve5(input, 100, 500, 0.1, 60, 500, 0.05, filename);
                //var genResult56 = gen.Solve6(input, 100, 500, 0.1, 60, 500, 0.05, filename, heuristics);
                ////20% 5
                //var genResult521 = gen.Solve1(input, 100, 500, 0.2, 60, 500, 0.05, filename);
                //var genResult522 = gen.Solve2(input, 100, 500, 0.2, 60, 500, 0.05, filename);
                //var genResult523 = gen.Solve3(input, 100, 500, 0.2, 60, 500, 0.05, filename);
                //var genResult524 = gen.Solve4(input, 100, 500, 0.2, 60, 500, 0.05, filename);
                //var genResult525 = gen.Solve5(input, 100, 500, 0.2, 60, 500, 0.05, filename);
                //var genResult526 = gen.Solve6(input, 100, 500, 0.2, 60, 500, 0.05, filename, heuristics);
                //////30% 5
                //var genResult531 = gen.Solve1(input, 100, 500, 0.3, 60, 500, 0.05, filename);
                //var genResult532 = gen.Solve2(input, 100, 500, 0.3, 60, 500, 0.05, filename);
                //var genResult533 = gen.Solve3(input, 100, 500, 0.3, 60, 500, 0.05, filename);
                //var genResult534 = gen.Solve4(input, 100, 500, 0.3, 60, 500, 0.05, filename);
                //var genResult535 = gen.Solve5(input, 100, 500, 0.3, 60, 500, 0.05, filename);
                //var genResult536 = gen.Solve6(input, 100, 500, 0.3, 60, 500, 0.05, filename, heuristics);
                //#endregion

                var gen = new Genetic();
                //10% 0.5
                //var genResult1 = gen.Solve1(input, 100, 900, 0.1, 60, 500, 0.005, filename);
                //var genResult2 = gen.Solve2(input, 100, 900, 0.1, 60, 500, 0.005, filename);
                //var genResult3 = gen.Solve3(input, 100, 900, 0.1, 60, 500, 0.005, filename);
                //var genResult4 = gen.Solve4(input, 100, 900, 0.1, 60, 500, 0.005, filename);
                //var genResult5 = gen.Solve5(input, 100, 900, 0.1, 60, 500, 0.005, filename);
                //var genResult6 = gen.Solve6(input, 100, 900, 0.1, 60, 500, 0.005, filename, heuristics);
                ////20% 0.5coi 
                //var genResult21 = gen.Solve1(input, 100, 900, 0.2, 60, 500, 0.005, filename);
                //var genResult22 = gen.Solve2(input, 100, 900, 0.2, 60, 500, 0.005, filename);
                //var genResult23 = gen.Solve3(input, 100, 900, 0.2, 60, 500, 0.005, filename);
                //var genResult24 = gen.Solve4(input, 100, 900, 0.2, 60, 500, 0.005, filename);
                //var genResult25 = gen.Solve5(input, 100, 900, 0.2, 60, 500, 0.005, filename);
                //var genResult26 = gen.Solve6(input, 100, 900, 0.2, 60, 500, 0.005, filename, heuristics);
                //////30% 0.5
                //var genResult31 = gen.Solve1(input, 100, 900, 0.3, 60, 500, 0.005, filename);
                //var genResult32 = gen.Solve2(input, 100, 900, 0.3, 60, 500, 0.005, filename);
                //var genResult33 = gen.Solve3(input, 100, 900, 0.3, 60, 500, 0.005, filename);
                //var genResult34 = gen.Solve4(input, 100, 900, 0.3, 60, 500, 0.005, filename);
                //var genResult35 = gen.Solve5(input, 100, 900, 0.3, 60, 500, 0.005, filename);
                //var genResult36 = gen.Solve6(input, 100, 900, 0.3, 60, 500, 0.005, filename, heuristics);

                ////10% 1
                //var genResult41 = gen.Solve1(input, 100, 900, 0.1, 60, 500, 0.01, filename);
                //var genResult42 = gen.Solve2(input, 100, 900, 0.1, 60, 500, 0.01, filename);
                //var genResult43 = gen.Solve3(input, 100, 900, 0.1, 60, 500, 0.01, filename);
                //var genResult44 = gen.Solve4(input, 100, 900, 0.1, 60, 500, 0.01, filename);
                //var genResult45 = gen.Solve5(input, 100, 900, 0.1, 60, 500, 0.01, filename);
                //var genResult46 = gen.Solve6(input, 100, 900, 0.1, 60, 500, 0.01, filename, heuristics);
                ////20% 1
                //var genResult421 = gen.Solve1(input, 100, 900, 0.2, 60, 500, 0.01, filename);
                //var genResult422 = gen.Solve2(input, 100, 900, 0.2, 60, 500, 0.01, filename);
                //var genResult423 = gen.Solve3(input, 100, 900, 0.2, 60, 500, 0.01, filename);
                //var genResult424 = gen.Solve4(input, 100, 900, 0.2, 60, 500, 0.01, filename);
                //var genResult425 = gen.Solve5(input, 100, 900, 0.2, 60, 500, 0.01, filename);
                //var genResult426 = gen.Solve6(input, 100, 900, 0.2, 60, 500, 0.01, filename, heuristics);
                //////30% 1
                //var genResult431 = gen.Solve1(input, 100, 900, 0.3, 60, 500, 0.01, filename);
                //var genResult432 = gen.Solve2(input, 100, 900, 0.3, 60, 500, 0.01, filename);
                //var genResult433 = gen.Solve3(input, 100, 900, 0.3, 60, 500, 0.01, filename);
                //var genResult434 = gen.Solve4(input, 100, 900, 0.3, 60, 500, 0.01, filename);
                //var genResult435 = gen.Solve5(input, 100, 900, 0.3, 60, 500, 0.01, filename);
                //var genResult436 = gen.Solve6(input, 100, 900, 0.3, 60, 500, 0.01, filename, heuristics);

                ////10% 5
                //var genResult51 = gen.Solve1(input, 100, 900, 0.1, 60, 500, 0.05, filename);
                //var genResult52 = gen.Solve2(input, 100, 900, 0.1, 60, 500, 0.05, filename);
                //var genResult53 = gen.Solve3(input, 100, 900, 0.1, 60, 500, 0.05, filename);
                //var genResult54 = gen.Solve4(input, 100, 900, 0.1, 60, 500, 0.05, filename);
                //var genResult55 = gen.Solve5(input, 100, 900, 0.1, 60, 500, 0.05, filename);
                //var genResult56 = gen.Solve6(input, 100, 900, 0.1, 60, 500, 0.05, filename, heuristics);
                ////20% 5
                //var genResult521 = gen.Solve1(input, 100, 900, 0.2, 60, 500, 0.05, filename);
                //var genResult522 = gen.Solve2(input, 100, 900, 0.2, 60, 500, 0.05, filename);
                //var genResult523 = gen.Solve3(input, 100, 900, 0.2, 60, 500, 0.05, filename);
                //var genResult524 = gen.Solve4(input, 100, 900, 0.2, 60, 500, 0.05, filename);
                //var genResult525 = gen.Solve5(input, 100, 900, 0.2, 60, 500, 0.05, filename);
                //var genResult526 = gen.Solve6(input, 100, 900, 0.2, 60, 500, 0.05, filename, heuristics);
                //////30% 5
                //var genResult531 = gen.Solve1(input, 100, 900, 0.3, 60, 500, 0.05, filename);
                //var genResult532 = gen.Solve2(input, 100, 900, 0.3, 60, 500, 0.05, filename);
                //var genResult533 = gen.Solve3(input, 100, 900, 0.3, 60, 500, 0.05, filename);
                //var genResult534 = gen.Solve4(input, 100, 900, 0.3, 60, 500, 0.05, filename);
                //var genResult535 = gen.Solve5(input, 100, 900, 0.3, 60, 500, 0.05, filename);
                //var genResult536 = gen.Solve6(input, 100, 900, 0.3, 60, 500, 0.05, filename, heuristics);


                ///
                /// test for 1 strategy
                /// 
                var genResult11 = gen.SolveV2_1(input, 16000, 16000, 0.1, 30, 1000, 0.005, filename);
                //var genResult21 = gen.Solve2(input, 100, 500, 0.2, 60, 500, 0.005, filename);
                //var genResult31 = gen.Solve2(input, 100, 500, 0.3, 60, 500, 0.005, filename);
                //var genResult41 = gen.Solve2(input, 100, 500, 0.4, 60, 500, 0.005, filename);
                //var genResult211 = gen.Solve2(input, 100, 500, 0.5, 60, 500, 0.005, filename);

                //var genResult51 = gen.Solve2(input, 100, 500, 0.1, 60, 500, 0.01, filename);
                //var genResult61 = gen.Solve2(input, 100, 500, 0.2, 60, 500, 0.01, filename);
                //var genResult71 = gen.Solve2(input, 100, 500, 0.3, 60, 500, 0.01, filename);
                //var genResult81 = gen.Solve2(input, 100, 500, 0.4, 60, 500, 0.01, filename);
                //var genResult221 = gen.Solve2(input, 100, 500, 0.5, 60, 500, 0.01, filename);

                //var genResult91 = gen.Solve2(input, 100, 500, 0.1, 60, 500, 0.02, filename);
                //var genResult101 = gen.Solve2(input, 100, 500, 0.2, 60, 500, 0.02, filename);
                //var genResult111 = gen.Solve2(input, 100, 500, 0.3, 60, 500, 0.02, filename);
                //var genResult121 = gen.Solve2(input, 100, 500, 0.4, 60, 500, 0.02, filename);
                //var genResult231 = gen.Solve2(input, 100, 500, 0.5, 60, 500, 0.02, filename);

                //var genResult241 = gen.Solve2(input, 100, 500, 0.1, 60, 500, 0.03, filename);
                //var genResult251 = gen.Solve2(input, 100, 500, 0.2, 60, 500, 0.03, filename);
                //var genResult261 = gen.Solve2(input, 100, 500, 0.3, 60, 500, 0.03, filename);
                //var genResult271 = gen.Solve2(input, 100, 500, 0.4, 60, 500, 0.03, filename);
                //var genResult281 = gen.Solve2(input, 100, 500, 0.5, 60, 500, 0.03, filename);

                //var genResult131 = gen.Solve2(input, 100, 500, 0.1, 60, 500, 0.04, filename);
                //var genResult141 = gen.Solve2(input, 100, 500, 0.2, 60, 500, 0.04, filename);
                //var genResult151 = gen.Solve2(input, 100, 500, 0.3, 60, 500, 0.04, filename);
                //var genResult161 = gen.Solve2(input, 100, 500, 0.4, 60, 500, 0.04, filename);
                //var genResult291 = gen.Solve2(input, 100, 500, 0.5, 60, 500, 0.04, filename);

                //var genResult171 = gen.Solve2(input, 100, 500, 0.1, 60, 500, 0.05, filename);
                //var genResult181 = gen.Solve2(input, 100, 500, 0.2, 60, 500, 0.05, filename);
                //var genResult191 = gen.Solve2(input, 100, 500, 0.3, 60, 500, 0.05, filename);
                //var genResult201 = gen.Solve2(input, 100, 500, 0.4, 60, 500, 0.05, filename);
                //var genResult301 = gen.Solve2(input, 100, 500, 0.5, 60, 500, 0.05, filename);

            }

        }


        #region init test
        private static MyCase InitData(string filename)
        {
            IWorkbook wb = null;
            MyCase data = new MyCase();
            data.Theta1 = 0.9;
            data.Theta2 = 0.1;
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