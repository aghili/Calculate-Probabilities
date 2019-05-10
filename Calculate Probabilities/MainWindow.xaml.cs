using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Calculate_Probabilities
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ProbabilitiesSets sets { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            sets = new ProbabilitiesSets();
            sets.ProgressChanged += BgwAlgorithm_ProgressChanged;
            sets.RunWorkerCompleted += Sets_RunWorkerCompleted;
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            sets = new ProbabilitiesSets(btn.Content.ToString());
            sets.ProgressChanged += BgwAlgorithm_ProgressChanged;
            sets.RunWorkerCompleted += Sets_RunWorkerCompleted;
            FlowDocument fd = new FlowDocument();
            foreach (var line in sets.Sets)
            {
                Paragraph p = new Paragraph();
                p.Inlines.Add(line.ToString());
                fd.Blocks.Add(p);
            }
            RtbSets.Document = fd; 

        }

        private void Sets_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BtnRun.Content = $"Run ({e.Result.ToString()}) 100%";
            UpdateResultPanel("Done",sets.CountOfProbablity,sets.OutputCount,sets.DateEngineStart,sets.EngineDuration);
            Process.Start("Notepad.exe", sets.OutputPath);
        }

        private void UpdateResultPanel(string status, int countOfProbablity, int count, DateTime dateEngineStart, TimeSpan engineDuration)
        {
            txtEngineStatus.Text = status;
            txtAlgorithmStartDate.Text = dateEngineStart.ToString();
            txtPropabCount.Text = countOfProbablity.ToString();
            txtPropabIndex.Text = count.ToString();
            txtAlgorithmRuntime.Text = engineDuration.ToString();
        }

        private void BgwAlgorithm_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BtnRun.Content = $"Finded Sets ({sets.OutputCount}) {e.ProgressPercentage}%";
            UpdateResultPanel("Running", sets.CountOfProbablity, sets.OutputCount, sets.DateEngineStart, sets.EngineDuration);

        }

        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            string[] SetsString ;

            string tmp = new TextRange(RtbSets.Document.ContentStart, RtbSets.Document.ContentEnd).Text;
                SetsString = tmp.Split(new char[] { '\n','\r' },StringSplitOptions.RemoveEmptyEntries);

            sets.UpdateSets(new List<string>(SetsString));
            sets.Run();
        }

    }

    public class Probabilities_type
    {
        public Probabilities_type(string format)
        {
            var split = format.Split('x');
            SetCount = GetStrToSetField(split[0]);
            SetMemberCount = GetStrToSetField(split[1]);

        }

        private int GetStrToSetField(string v)
        {
            if (v.Equals("RND", StringComparison.CurrentCultureIgnoreCase))
            {
                return new Random(DateTime.Now.Millisecond).Next(99);
            }
            else
                return Convert.ToInt32(v);
        }

        public int SetMemberCount { get; set; } = 3;
        public int SetCount { set; get; } = 3;
    }

    public class SetMembers:IEquatable<SetMembers>
    {
        public SetMembers()
        {
        }
        public SetMembers(string value)
        {
            Members = new List<string>(value.Split(','));
        }

        public SetMembers(SetMembers output)
        {
            this.Members.AddRange( output.Members);
        }

        public List<string> Members { set; get; } = new List<string>();

        public bool Equals(SetMembers other)
        {
            if (Members.Count != other.Members.Count)
                return false;
            for (int index = 0; index < Members.Count; index++)
                if (!Members[index].Equals(other.Members[index]))
                    return false;
            return true;
        }

        public override string ToString()
        {
            return string.Join(",",Members);
        }
    }

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
            //lock (Output)
            {
                //if (Output.Contains(value))
                //    return;
                OutputCount++;
                Output.WriteLine($"{OutputCount}\t" +value.ToString());
            }
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
            OutputPath = Environment.CurrentDirectory + $"\\Output\\{DateEngineStart.Year}-{DateEngineStart.Month}-{DateEngineStart.Day} {DateEngineStart.GetHashCode()}.txt";
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
