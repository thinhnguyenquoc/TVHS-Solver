using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace statistic
{
    class Program
    {
        static void Main(string[] args)
        {
            List<MyTimeFrame> timeframelist = GetTimeFrame();
            XSSFWorkbook xssfwb;
            List<string> filenames = new List<string>();
            filenames.Add(@"D:\thesis\TVHS_Data_test\statistic\3-8_9-8_2015.xlsx");
            List<List<MyProgram>> programlist = new List<List<MyProgram>>();
            foreach (var filename in filenames)
            {
                using (FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    xssfwb = new XSSFWorkbook(file);
                }
                ISheet sheet = xssfwb.GetSheetAt(0);
                List<MyProgram> list = new List<MyProgram>();
                for (int row = 0; row <= sheet.LastRowNum; row++)
                {
                    if (sheet.GetRow(row) != null)
                    {
                        if (sheet.GetRow(row).GetCell(1) != null)
                        {
                            MyProgram pr = new MyProgram();
                            pr.Name = sheet.GetRow(row).GetCell(2).StringCellValue;
                            if (pr.Name.ToLower().Contains("trailer"))
                                continue;
                            if (pr.Name.ToLower().Contains("live"))
                            {
                                pr.Live = true;
                            }
                            else
                                pr.Live = false;
                            pr.Start = sheet.GetRow(row).GetCell(1).DateCellValue;
                            pr.Frame = timeframelist.Where(x => (x.Start.Hour * 60 + x.Start.Minute <= pr.Start.Hour * 60 + pr.Start.Minute) && (x.End.Hour * 60 + x.End.Minute >= pr.Start.Hour * 60 + pr.Start.Minute)).FirstOrDefault().Id;
                            list.Add(pr);
                        }
                    }
                }
                programlist.Add(list);
            }
        }
        static List<MyTimeFrame> GetTimeFrame()
        {
            List<MyTimeFrame> baseList = new List<MyTimeFrame>();
            #region init
            baseList.Add(new MyTimeFrame()
            {
                Id = 0,
                Duration = 60,
                Live = false,
                Start = new DateTime(2015,7,2,5,0,0),
                End = new DateTime(2015, 7, 2, 6, 29, 59)
            });
            baseList.Add(new MyTimeFrame()
            {
                Id = 1,
                Duration = 60,
                Live = true,
                Start = new DateTime(2015, 7, 2, 6, 30, 0),
                End = new DateTime(2015, 7, 2, 7, 29, 59)
            });
            baseList.Add(new MyTimeFrame()
            {
                Id = 2,
                Duration = 120,
                Live = false,
                Start = new DateTime(2015, 7, 2, 7, 30, 0),
                End = new DateTime(2015, 7, 2, 9, 29, 59)
            });
            baseList.Add(new MyTimeFrame()
            {
                Id = 3,
                Duration = 60,
                Live = true,
                Start = new DateTime(2015, 7, 2, 9, 30, 0),
                End = new DateTime(2015, 7, 2, 10, 29, 59)
            });
            baseList.Add(new MyTimeFrame()
            {
                Id = 4,
                Duration = 240,
                Live = false,
                Start = new DateTime(2015, 7, 2, 10, 30, 0),
                End = new DateTime(2015, 7, 2, 14, 29, 59)
            });
            baseList.Add(new MyTimeFrame()
            {
                Id = 5,
                Duration = 60,
                Live = true,
                Start = new DateTime(2015, 7, 2, 14, 30, 0),
                End = new DateTime(2015, 7, 2, 15, 29, 59)
            });
            baseList.Add(new MyTimeFrame()
            {
                Id = 6,
                Duration = 240,
                Live = false,
                Start = new DateTime(2015, 7, 2, 15, 30, 0),
                End = new DateTime(2015, 7, 2, 19, 29, 59)
            });
            baseList.Add(new MyTimeFrame()
            {
                Id = 7,
                Duration = 60,
                Live = true,
                Start = new DateTime(2015, 7, 2, 19, 30, 0),
                End = new DateTime(2015, 7, 2, 20, 29, 59)
            });
            baseList.Add(new MyTimeFrame()
            {
                Id = 8,
                Duration = 60,
                Live = false,
                Start = new DateTime(2015, 7, 2, 20, 30, 0),
                End = new DateTime(2015, 7, 2, 21, 29, 59)
            });
            baseList.Add(new MyTimeFrame()
            {
                Id = 9,
                Duration = 60,
                Live = true,
                Start = new DateTime(2015, 7, 2, 21, 30, 0),
                End = new DateTime(2015, 7, 2, 22, 29, 59)
            });
            baseList.Add(new MyTimeFrame()
            {
                Id = 10,
                Duration = 90,
                Live = false,
                Start = new DateTime(2015, 7, 2, 22, 30, 0),
                End = new DateTime(2015, 7, 2, 23, 59, 59)
            });
            #endregion

            return baseList;
        }
    }
    public class MyProgram
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Live { get; set; }
        public DateTime Start { get; set; }
        public int Frame { get; set; }
        public MyProgram()
        {

        }
    }
    public class MyTimeFrame
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool Live { get; set; }
        public int Duration { get; set; }
        public MyTimeFrame()
        {

        }
        public MyTimeFrame(int Id, string Name, DateTime Start, DateTime End)
        {
            this.Id = Id;
            this.Name = Name;
            this.Start = Start;
            this.End = End;
        }
    }
}
