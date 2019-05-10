using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace Calculate_Probabilities
{
    public class ProbabilitiesSets
    {
        private BackgroundWorker bgwAlgorithm = new BackgroundWorker();

        private StreamWriter Output;
        public List<SetMembers> Sets = new List<SetMembers>();
        public int OutputCount =0;

        public int Percent 
        {
            get
            {
                return OutputCount * 100 / CountOfProbablity;
            }
        }

        public DateTime DateEngineStart { get; internal set; } = DateTime.MinValue;
        public int CountOfProbablity { get; internal set; }
        public TimeSpan EngineDuration { get; internal set; }
        public string OutputPath { get; private set; }

        public event ProgressChangedEventHandler ProgressChanged;
        public event RunWorkerCompletedEventHandler RunWorkerCompleted;

        private void AddOutput(SetMembers value)
        {
                OutputCount++;
                Output.WriteLine($"{OutputCount}," +value.ToString());
        }

public ProbabilitiesSets()
        {
            bgwAlgorithm.DoWork += BgwAlgorithm_DoWork;
            bgwAlgorithm.ProgressChanged += BgwAlgorithm_ProgressChanged;
            bgwAlgorithm.RunWorkerCompleted += BgwAlgorithm_RunWorkerCompleted;
            bgwAlgorithm.WorkerReportsProgress = true;
        }

        public ProbabilitiesSets(string v)
        {
            var prbType = new Probabilities_type(v);
            GetProbabilities(prbType);
            bgwAlgorithm.DoWork += BgwAlgorithm_DoWork;
            bgwAlgorithm.ProgressChanged += BgwAlgorithm_ProgressChanged;
            bgwAlgorithm.RunWorkerCompleted += BgwAlgorithm_RunWorkerCompleted;
            bgwAlgorithm.WorkerReportsProgress = true;
        }

        private void BgwAlgorithm_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EngineDuration = DateTime.Now.Subtract(DateEngineStart);
            Output.Close();
            RunWorkerCompleted?.Invoke(sender, e);
        }

        private void BgwAlgorithm_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            EngineDuration = DateTime.Now.Subtract(DateEngineStart);
            ProgressChanged?.Invoke(sender,e);
        }

        private void BgwAlgorithm_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int x = 0; x < Sets[0].Members.Count; x++)
                Do(x, 0, new SetMembers());
            e.Result = OutputCount;
        }

        private void GetProbabilities(Probabilities_type prbType)
        {

            int MemberNumber = 0;
            for (int x = 0; x < prbType.SetCount; x++)
            {
                var set = new SetMembers();
                for (int y = 0; y < prbType.SetMemberCount; y++)
                {
                    string MemberName = GetMemberName(MemberNumber);
                    MemberNumber++;
                    set.Members.Add(MemberName);
                }
                Sets.Add(set);
            }
            CalculateProbabilitieCount();
        }

        private void CalculateProbabilitieCount()
        {
            CountOfProbablity = Sets[0].Members.Count;
            for (int i = 1; i < Sets.Count; i++)
                CountOfProbablity *= Sets[i].Members.Count;
        }

        private int reportCountr = 50;

        public void Do(int x,int y,SetMembers output)
        {
            var localOutput = new SetMembers(output);
            localOutput.Members.Add(Sets[y].Members[x]);

            if (y + 1 == Sets.Count)
            {
                AddOutput(localOutput);
                if (reportCountr == 100)
                {
                    bgwAlgorithm.ReportProgress(Percent);
                    Thread.Sleep(1);
                    reportCountr = 0;
                }
                reportCountr++;
                return;
            }

            for (int xi = 0; xi < Sets[y].Members.Count; xi++)
            Do(xi,y + 1,localOutput);
        }

        public void Run()
        {
            DateEngineStart = DateTime.Now;
            Directory.CreateDirectory(Environment.CurrentDirectory + $"\\Output");
            OutputPath = Environment.CurrentDirectory + $"\\Output\\{DateEngineStart.Year}-{DateEngineStart.Month}-{DateEngineStart.Day} {DateEngineStart.GetHashCode()}.csv";
            OutputCount = 0;
            Output = new StreamWriter(OutputPath);
            bgwAlgorithm.RunWorkerAsync();
        }

        private string GetMemberName(int memberNumber)
        {
            int CharNumber = memberNumber % 26;
            if (memberNumber >25)
                return GetMemberName(memberNumber / 26) + (char)(CharNumber + 65);
            else
                return $"{(char)(CharNumber + 65)}";
        }

        internal void UpdateSets(List<string> setsString)
        {
            Sets.Clear();
            foreach (var item in setsString)
                Sets.Add(new SetMembers(item));
            CalculateProbabilitieCount();
        }
    }
}
