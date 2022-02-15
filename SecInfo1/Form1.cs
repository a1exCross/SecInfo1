using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Security.Cryptography;
using Npgsql;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Net.NetworkInformation;

namespace SecInfo1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button1.Visible = false;
            richTextBox2.Visible = false;
            this.Size = new System.Drawing.Size(488, 200);
            label2.Visible = false;
        }

        public string Hash(string password)
        {
            byte[] data = Encoding.Default.GetBytes(password);
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] result = sha.ComputeHash(data);
            password = Convert.ToBase64String(result);
            return password;
        }

        public string Hashh(string password)
        {
            var alg = SHA512.Create();
            alg.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(alg.Hash);

        }

        public string read(string p1)
        {
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection("Server = 127.0.0.1; Port = 5432; User Id = postgres; Password = postgres; Database = secinf");
                conn.Open();

                NpgsqlCommand com = new NpgsqlCommand("SELECT user_token from license where license.proc_id='" + p1 + "'", conn); ;

                NpgsqlDataReader reader;
                reader = com.ExecuteReader();

                string[,] nums = new string[100, 100];
                int a = 0;
                
                while (reader.Read())
                {
                    try
                    {
                        nums[a, 0] = reader.GetString(0);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    a++;
                }

                conn.Close();
                string s = nums[0, 0];

                return s;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}","Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw ex;
            }

        }

        public bool regedit_check(string p1, string p2, string veryf, bool license)
        {
            RegistryKey currentUserKey = Registry.CurrentUser;
            if (currentUserKey.OpenSubKey("Application", true) != null)
            {
                RegistryKey Key = currentUserKey.OpenSubKey("Application", true);
                if (Key.GetValue(Hash(p1 + p2)) != null)
                {
                    if (veryf != Key.GetValue(Hash(p1 + p2)).ToString())
                        license = false;
                }
                else
                    license = false;

                Key.Close();
            }
            else return false;

            return license; 
        }

        public bool regedit(string p1, string p2, string veryf, bool license)
        {
            RegistryKey currentUserKey = Registry.CurrentUser;
            if (currentUserKey.OpenSubKey("Application", true) != null)
            {
                RegistryKey Key = currentUserKey.OpenSubKey("Application", true);
                if (Key.GetValue(Hash(p1 + p2)) != null)
                {
                    if (veryf != Key.GetValue(Hash(p1 + p2)).ToString())
                        license = false;
                }
                else
                if (veryf == Hashh(Hash(p1 + p2)))
                {
                    Key.SetValue(Hash(p1 + p2), veryf);
                    set = true;
                }

                Key.Close();
            }
            else
            {
                RegistryKey Key = currentUserKey.CreateSubKey("Application");
                Key.SetValue(Hash(p1 + p2), veryf);
                Key.Close();
            }

            return license;

        }
        bool active = false;
        bool set = false;

        private void Init()
        {
            string pid = null;
            string name = null;
            string h_id = null;
            string serial = null;
            string veryf = null;
            active = true;
            bool license = true;
            bool check = false;

            ManagementObjectSearcher searcher =
                   new ManagementObjectSearcher("root\\CIMV2",
                   "SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                serial = queryObj["SerialNumber"].ToString();
            }

            ManagementObjectSearcher searcher1 =
                   new ManagementObjectSearcher("root\\CIMV2",
                   "SELECT * FROM Win32_Processor");
            foreach (ManagementObject queryObj in searcher1.Get())
            {
                pid = queryObj["ProcessorId"].ToString();
            }

            veryf = read(Hash(serial + pid));

            label1.Text = "Проверка подключения к интернету";
            Application.DoEvents();
            Thread.Sleep(500);


            label2.Visible = true;
            label2.Text = "Проверка лицензии";
            Application.DoEvents();
            Thread.Sleep(500);

            if (!regedit_check(serial, pid, veryf, license))
            {

                IPStatus status = IPStatus.Unknown;
                try
                {
                    Ping p = new Ping();
                    PingReply pr = p.Send(@"google.com");
                    status = pr.Status;
                }
                catch { }
                if (status == IPStatus.Success)
                {
                    label1.Text = "Подключение к интернету установлено";
                }
                else
                {
                    label1.Text = "Проверьте подключение к интернету!";
                    return;
                }

                richTextBox2.Text = Hash(serial + pid);

                //MessageBox.Show("processor: " + pid + '\n' + name + "\nHardDrive: " + h_id + '\n' + serial, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (veryf == null)
                {
                    label2.Text = "Запуск программы невозможен.\nДля получения лицензии отправьте указанный ключ ниже разработчикам программы\nПрограмма активируется автоматически";
                    button1.Visible = true;
                    richTextBox2.Visible = true;
                    this.Size = new System.Drawing.Size(488, 330);
                    return;
                }
                else
                {
                    if (veryf == Hashh(Hash(serial + pid)))
                        license = regedit(serial, pid, veryf, license);
                    else license = false;
                }

                if (license)
                {
                    if (!set)
                        label2.Text = "Программа является лицензионной";
                    else label2.Text = "Лицензия установлена";

                    Thread.Sleep(500);
                    Application.DoEvents();
                }
                else
                {
                    MessageBox.Show("Данная копия программы не является лицензионной! Обратитесь к разработчку для приобритения лицензии!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Process.GetCurrentProcess().Kill();
                }
            }
            else
            {
                //MessageBox.Show("hmm");
                label2.Text = "Программа является лицензионной";
                Thread.Sleep(500);
                this.Hide();
                Form2 f2 = new Form2();
                f2.ShowDialog();

            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
           if (!active) Init();
        }

        private void Form1_VisibleChanged(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
