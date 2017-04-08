using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matching
{
    class Program
    {
        static void Main(string[] args)
        {
            List<MyTimeFrame> frames = new List<MyTimeFrame>() { 
            new MyTimeFrame()
            {
                Id = 1,
                Duration = 60
            },
             new MyTimeFrame()
            {
                Id = 2,
                Duration = 90
            },
              new MyTimeFrame()
            {
                Id = 3,
                Duration = 120
            },
            };
            List<MyProgram> programs = new List<MyProgram>()
            {
                new MyProgram()
                {
                    Id= 1,
                    Duration = 20,
                    MaxShowTime = 3,
                    FrameList = new List<MyTimeFrame>()
                    {
                        frames[0], frames[2]
                    }
                },
                 new MyProgram()
                {
                    Id= 2,
                    Duration = 25,
                    MaxShowTime = 2,
                    FrameList = new List<MyTimeFrame>()
                    {
                        frames[1], frames[2]
                    }
                },
                  new MyProgram()
                {
                    Id= 3,
                    Duration = 20,
                    MaxShowTime = 3,
                    FrameList = new List<MyTimeFrame>()
                    {
                        frames[1]
                    }
                },
                   new MyProgram()
                {
                    Id= 4,
                    Duration = 25,
                    MaxShowTime = 3,
                    FrameList = new List<MyTimeFrame>()
                    {
                        frames[2]
                    }
                },

            };
            List<AssPtoF> result = new List<AssPtoF>();
            while (true)
            {
                foreach (var myProgram in programs)
                {
                    if (myProgram.FrameList.Any(x => x.Duration - myProgram.Duration > 0)&&myProgram.MaxShowTime>0)
                    {
                        myProgram.MaxShowTime--;
                        var firstFrame = myProgram.FrameList.First(x => x.Duration - myProgram.Duration > 0);
                        firstFrame.Duration -= myProgram.MaxShowTime;
                        result.Add(new AssPtoF()
                        {
                            FrameId = firstFrame.Id,
                            ProgramId = myProgram.Id
                        });
                    }
                }
                if (!frames.Any(x => x.Duration > programs.Min(y => y.Duration)))
                {
                    break;
                }
                if (!programs.Any(x => x.MaxShowTime > 0))
                {
                    break;
                }
            }
            var summary = result.GroupBy(x => x.FrameId);
            var a = 0;
        }
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
        public int GroupId { get; set; }
        public List<MyTimeFrame> FrameList { get; set; }
        public double RevenuePerShow
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

    public class AssPtoF
    {
        public int ProgramId { get; set; }
        public int FrameId { get; set; }
    }
}
