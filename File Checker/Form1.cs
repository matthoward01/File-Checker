using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Deployment.Application;

namespace File_Checker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                this.Text += " (" + ApplicationDeployment.CurrentDeployment.CurrentVersion + ")";
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            bw.ReportProgress(0, "Starting...\r\n");

            System.IO.DirectoryInfo dir1 = new System.IO.DirectoryInfo(textBox1.Text);
            System.IO.DirectoryInfo dir2 = new System.IO.DirectoryInfo(textBox2.Text);
            bw.ReportProgress(20, "Capturing Main Directories...\r\n");
            IEnumerable<System.IO.FileInfo> list1 = dir1.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            bw.ReportProgress(40, "Capturing Backup Directories...\r\n");
            IEnumerable<System.IO.FileInfo> list2 = dir2.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            FileCompare myFileCompare = new FileCompare();

            
            if ((bw.CancellationPending == true))
            {
                e.Cancel = true;

            }
            else
            {
                
                var queryList1Only = (from file in list1
                                      select file).Except(list2, myFileCompare);
                bw.ReportProgress(60, "Comparing Directories...\r\n");
                bw.ReportProgress(80, "Preparing List...\r\n");
                foreach (var v in queryList1Only)
                {
                    
                    string badFile = v.FullName.ToString() + "\r\n";
                    if (!badFile.Contains("._"))
                    {
                        
                        bw.ReportProgress(80, badFile + "\r\n");
                    }

                }
                bw.ReportProgress(100, "Finishing...\r\n");
                
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Change the value of the ProgressBar to the BackgroundWorker progress.
            progressBar1.Value = e.ProgressPercentage;
            // Set the text.
            //this.Text = e.ProgressPercentage.ToString();
            //this.textBox3.Text = (e.ProgressPercentage.ToString() + "%");
            this.textBox3.Text += (e.UserState.ToString());
            this.textBox3.ScrollToCaret();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (bw.WorkerSupportsCancellation == true)
            {
                bw.CancelAsync();
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                this.textBox3.Text += "Canceled!\r\n";
                this.textBox3.ScrollToCaret();
            }

            else if (!(e.Error == null))
            {
                this.textBox3.Text += ("Error: " + e.Error.Message + "\r\n");
                this.textBox3.ScrollToCaret();
            }

            else
            {
                this.textBox3.Text += "Done!\r\n";
                this.textBox3.ScrollToCaret();
            }
        }

       }


    class FileCompare : System.Collections.Generic.IEqualityComparer<System.IO.FileInfo>
    {
        public FileCompare() { }

        public bool Equals(System.IO.FileInfo f1, System.IO.FileInfo f2)
        {
            return (f1.Name == f2.Name &&
                    f1.Length == f2.Length);
        }

        // Return a hash that reflects the comparison criteria. According to the  
        // rules for IEqualityComparer<T>, if Equals is true, then the hash codes must 
        // also be equal. Because equality as defined here is a simple value equality, not 
        // reference identity, it is possible that two or more objects will produce the same 
        // hash code. 
        public int GetHashCode(System.IO.FileInfo fi)
         {
           string s = String.Format("{0}{1}", fi.Name, fi.Length);
            return s.GetHashCode();
        }
    }
}

