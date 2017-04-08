using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewGenForTVHS
{
    public class MyModel
    {
    }

    public class MySchedule
    {
        public MyProgram Program { get; set; }
        public int Start { get; set; }
    }
    public class TempResult
    {
        public int proId { get; set; }
        public int numShow { get; set; }
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
        public double Theta1 { get; set; }
        public double Theta2 { get; set; }

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

    public class MyStatictis
    {
        public int[] Choosen { get; set; }
        public long Elapsed { get; set; }
        public double Ratio { get; set; }
        public double Revenue { get; set; }
        public int noGen { get; set; }
    }
}
