using System;
using Eduard;
using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Windows.Documents;
using System.Security.Cryptography;
using System.Threading;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Configuration;
using System.Windows.Controls;
using Eduard.Cryptography;

namespace Elliptic_Curve_Primality_Proving
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RandomNumberGenerator rand;
        private BackgroundWorker bw;
        private int resultCode = 0;
        private StringBuilder sb;
        private string buffer;
        private Certificate cert;
        private TimeSpan ts;

        public MainWindow()
        {
            InitializeComponent();
            rand = RandomNumberGenerator.Create();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(textBox.Text))
            {
                new Thread(() =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        string[] all = comboBox.Text.Split(' ');
                        int bits = int.Parse(all[0]);
                        BigInteger field = BigInteger.GenProbablePrime(rand, bits, 50);
                        Polynomial.SetField(field);
                        textBox.Text = field.ToString();
                        richTextBox.Document.Blocks.Clear();
                    }));
                }).Start();
            }
            else
            {
                new Thread(() =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        BigInteger field = new BigInteger(textBox.Text);
                        bool isPrime = BigInteger.IsProbablePrime(rand, field, 50);

                        if (!isPrime)
                        {
                            if (MessageBox.Show("Number is not prime.") == MessageBoxResult.OK)
                            {
                                groupBox.Focus();
                                textBox.Clear();
                            }
                        }
                        else
                            Polynomial.SetField(field);
                    }));
                }).Start();
            }
            
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            // There will make primality proving certificate...
            richTextBox.Document.Blocks.Clear();
            buffer = textBox.Text;
            groupBox.Focus();

            if (string.IsNullOrEmpty(buffer))
            {
                textBox.Focus();
                return;
            }
            else
            {
                bw = new BackgroundWorker();
                bw.WorkerReportsProgress = false;
                bw.WorkerSupportsCancellation = true;
                bw.DoWork += Verify;
                bw.RunWorkerCompleted += WorkComplete;
                bw.RunWorkerAsync();
                button_Copy.IsEnabled = false;
            }
        }

        private void TestComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if(bw.CancellationPending)
            {
                MessageBox.Show("Certificate validation has been canceled.", "Elliptic Curve Primality Proving");
                button_Copy.IsEnabled = true;
                sb.Clear();
            }
            else
            {
                Paragraph p = new Paragraph();
                p.FontSize = 12;
                p.Inlines.Add(sb.ToString());
                richTextBox.Document.Blocks.Add(p);
                p.Inlines.Add(new Bold(new Run("Number is proven prime.\n"){ Foreground = Brushes.ForestGreen }));
                p.Inlines.Add(new Bold(new Run(string.Format("{0}", ts)) { Foreground = Brushes.Blue }));
                button_Copy.IsEnabled = true;
                sb.Clear();
            }
        }

        private void VerifyCert(object sender, DoWorkEventArgs e)
        {
            Atkin test = new Atkin();
            sb = new StringBuilder();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            test.VerifyCert(sb, bw, cert);
            stopwatch.Stop();
            ts = stopwatch.Elapsed;
        }

        private void WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!bw.CancellationPending)
            {
                if(resultCode == -2)
                {
                    sb.Clear();
                    MessageBox.Show("Number is definitely composite.", "Elliptic Curve Primality Proving");
                    return;
                }

                Paragraph p = new Paragraph();
                p.FontSize = 12;

                p.Inlines.Add(sb.ToString());
                richTextBox.Document.Blocks.Add(p);

                p.Inlines.Add(new Bold(new Run("Number is proven prime.\n") { Foreground = Brushes.ForestGreen}));
                p.Inlines.Add(new Bold(new Run(string.Format("{0}", ts.FormatTime())) { Foreground = Brushes.Blue }));

                button_Copy.IsEnabled = true;
                sb.Clear();
            }
            else
            {
                button_Copy.IsEnabled = true;
                sb.Clear();
            }
        }

        private void Verify(object sender, DoWorkEventArgs e)
        {
            Atkin test = new Atkin();
            sb = new StringBuilder();
            cert = new Certificate();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            resultCode = test.Start(new BigInteger(buffer), sb, bw, cert);
            stopwatch.Stop();
            ts = stopwatch.Elapsed;
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            if(bw == null || (bw != null && !bw.IsBusy))
            {
                groupBox.Focus();
                return;
            }

            if (!bw.CancellationPending)
            {
                bw.CancelAsync();
                MessageBox.Show("Primality proving was canceled.", "Elliptic Curve Primality Proving");
                richTextBox.Document.Blocks.Clear();
                button_Copy.IsEnabled = true;
                textBox.Clear();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            buffer = textBox.Text;
            groupBox.Focus();

            richTextBox.Document.Blocks.Clear();
            // Now, we will check the ECPP certificate from XML file.
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "Xml files (*.xml)|*.xml";

            if (opf.ShowDialog() == true)
            {
                cert = new Certificate(opf.FileName);
                bw = new BackgroundWorker();
                bw.WorkerReportsProgress = false;
                bw.WorkerSupportsCancellation = true;
                bw.DoWork += VerifyCert;
                bw.RunWorkerCompleted += TestComplete;
                bw.RunWorkerAsync();
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (bw.IsBusy) return;
            textBox.Clear();
            richTextBox.Document.Blocks.Clear();
            // Now, the ECPP certificate will be saved on XML file.
            SaveFileDialog opf = new SaveFileDialog();
            opf.Filter = "Xml files (*.xml)|*.xml";

            if (opf.ShowDialog() == true)
                cert.Save(opf.FileName);

            groupBox.Focus();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            textBox.Clear();
            richTextBox.Document.Blocks.Clear();

            if (bw == null) return;
            if (!bw.CancellationPending) bw.CancelAsync();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void textBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(button1);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
            }
        }
    }
}
