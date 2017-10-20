using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.Net;
using System.Collections.Specialized;
using System.Threading;

namespace eQSL_Downloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.anchor.Trim().Length > 0 )
            {
                tbUserName.Text = BinaryToString(Properties.Settings.Default.anchor);
                ckSaveCR.Checked = true;
            }
            if (Properties.Settings.Default.keyhole.Trim().Length > 0)
            {
                tbPassword.Text = BinaryToString(Properties.Settings.Default.keyhole);
                ckSaveCR.Checked = true;
            }

            try
            {
                tbFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\IEN\\eQSL"; 
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
                tbFolder.Text = "c:\\eQSL";
            }
            if (System.IO.Directory.Exists(tbFolder.Text))
            {
                int c = System.IO.Directory.GetFiles(tbFolder.Text).Count();
                toolStripStatusLabel1.Text = "Cards downloaded: " + c.ToString();

            }
            //check for saved username and password
            

        }
        public static string StringToBinary(string data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in data.ToCharArray())
            {
                sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }
        
        public static string BinaryToString(string data)
        {
            List<Byte> byteList = new List<Byte>();

            for (int i = 0; i < data.Length; i += 8)
            {
                byteList.Add(Convert.ToByte(data.Substring(i, 8), 2));
            }
            return Encoding.ASCII.GetString(byteList.ToArray());
        }


        void saveUser()
        {
            if (ckSaveCR.Checked)
            {
                string s = tbPassword.Text;
                Properties.Settings.Default.anchor = StringToBinary(tbUserName.Text);
                Properties.Settings.Default.keyhole = StringToBinary(tbPassword.Text);
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.anchor = ""; 
                Properties.Settings.Default.keyhole = ""; 
                Properties.Settings.Default.Save();
            }

        }

        public Thread workerThread = null;
        public Worker worker = null;

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                saveUser();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            } 

            worker = new Worker();
            worker.Error = Error;
            worker.Done = Done;
            worker.Status = Status;
            worker.CallSign = tbUserName.Text;
            worker.PassWord = tbPassword.Text;
            worker.StoragePath = tbFolder.Text;
            worker.Archive = cbArchive.Checked;

            workerThread = new Thread(worker.DoWork);
            // Start the worker thread.
            workerThread.Start();
            btnStart.Enabled = false;
        }

        private void Done(string result)
        {
            if (statusStrip1.InvokeRequired)
            {
                this.Invoke(new Action(() => Done(result)));
            }
            else
            {
                if (System.IO.Directory.Exists(tbFolder.Text))
                {
                    int c = System.IO.Directory.GetFiles(tbFolder.Text).Count();
                    toolStripStatusLabel1.Text = "Cards downloaded: " + c.ToString();
                }
                btnStart.Enabled = true;
            }
        }

        private void Error(string error)
        {
            UpdateStatus("ERROR > " + error);
        }

        private void Status(string status)
        {
            UpdateStatus(status);
        }

        private void UpdateStatus(string status)
        {
            if (statusStrip1.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateStatus(status)));
            }
            else
            {
                toolStripStatusLabel1.Text = status;
                System.Threading.Thread.Sleep(1);
            }
        } 

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://itunes.apple.com/us/app/learning-morse-code/id735785166?mt=8");
            }
            catch (Exception ex)
            {
                System.Console.Write(ex.ToString());
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.indianaelmernetwork.us");
            }
            catch (Exception ex)
            {
                System.Console.Write(ex.ToString());
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(tbFolder.Text); 
            }
            catch (Exception ex)
            {
            
            }
            
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 f = new Form3();
            f.Show(this); 

        }

    }

}
