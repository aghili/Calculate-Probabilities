﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
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
            switch (btn.Content.ToString().ToLower())
            {
                case "custom1":
                    sets = new ProbabilitiesSets()
                    {
                        Sets = new List<SetMembers>()
                        {
                            new SetMembers{Members = new List<string>{"A","X","B"}},
                            new SetMembers{Members = new List<string>{"C","X","D"}},
                            new SetMembers{Members = new List<string>{"E","X","F"}},
                            new SetMembers{Members = new List<string>{"G","X","H"}},
                            new SetMembers{Members = new List<string>{"I","X","J"}},
                            new SetMembers{Members = new List<string>{"K","X","L"}}
                        }
                    };
                    break;
                case "custom2":
                    sets = new ProbabilitiesSets()
                    {
                        Sets = new List<SetMembers>()
                        {
                            new SetMembers{Members = new List<string>{"A","X","B"}},
                            new SetMembers{Members = new List<string>{"C","X","D"}},
                            new SetMembers{Members = new List<string>{"E","X","F"}},
                            new SetMembers{Members = new List<string>{"G","X","H"}},
                            new SetMembers{Members = new List<string>{"I","X","J"}},
                            new SetMembers{Members = new List<string>{"K","X","L"}},
                            new SetMembers{Members = new List<string>{"M","X","N"}},
                            new SetMembers{Members = new List<string>{"O","X","P"}},
                            new SetMembers{Members = new List<string>{"Q","X","R"}},
                            new SetMembers{Members = new List<string>{"S","X","T"}},
                            new SetMembers{Members = new List<string>{"U","X","V"}},
                            new SetMembers{Members = new List<string>{"Y","X","Z"}}
                        }
                    };
                    break;
                default:
                    sets = new ProbabilitiesSets(btn.Content.ToString());
                    break;
            }
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
}
