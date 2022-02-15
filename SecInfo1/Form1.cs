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
            Init();
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

        public void insert(string p1, string p2, int key)
        {
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection("Server = 127.0.0.1; Port = 5432; User Id = postgres; Password = postgres; Database = secinf");
                conn.Open();
                NpgsqlCommand com;
                if (key == 0)
                    com = new NpgsqlCommand("INSERT INTO public.Processor(proc_id,proc_name) VALUES(@p1, @p2)", conn);
                else com = new NpgsqlCommand("INSERT INTO public.harddrive(hard_model,hard_serial) VALUES(@p1, @p2)", conn);
                var a = new NpgsqlParameter("@p1", NpgsqlTypes.NpgsqlDbType.Varchar);
                var b = new NpgsqlParameter("@p2", NpgsqlTypes.NpgsqlDbType.Varchar);

                a.Value = Hash(p1);
                b.Value = Hash(p2);

                com.Parameters.Add(a);
                com.Parameters.Add(b);

                com.ExecuteNonQuery();


                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string read(string p1, int key)
        {
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection("Server = 127.0.0.1; Port = 5432; User Id = postgres; Password = postgres; Database = secinf");
                conn.Open();
                NpgsqlCommand com;

                if (key == 0)
                     com = new NpgsqlCommand("SELECT proc_id from processor where processor.proc_name='"+ Hash(p1)+"'", conn);
                else com = new NpgsqlCommand("SELECT hard_serial from harddrive where harddrive.hard_model='" + Hash(p1) + "'", conn);

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

        string veryf = "";

        private void Init()
        {
            string pid = null;
            string name = null;
            string h_id = null;
            string serial = null;

            //IPStatus status = IPStatus.Unknown;
            //try
            //{
            //    Ping p = new Ping();
            //    PingReply pr = p.Send(@"google.com");
            //    status = pr.Status;
            //}
            //catch { }
            //if (status == IPStatus.Success)
            //{
            //    MessageBox.Show("Подключение к интернету установлено");
            //}
            //else
            //{
            //    MessageBox.Show("Проверьте подключение к интернету!");
            //}

            ManagementObjectSearcher searcher =
                   new ManagementObjectSearcher("root\\CIMV2",
                   "SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                h_id = queryObj["Model"].ToString();
                serial = queryObj["SerialNumber"].ToString();
            }

            ManagementObjectSearcher searcher1 =
                   new ManagementObjectSearcher("root\\CIMV2",
                   "SELECT * FROM Win32_Processor");
            foreach (ManagementObject queryObj in searcher1.Get())
            {
                pid = queryObj["ProcessorId"].ToString();
                name = queryObj["Name"].ToString();
            }

            veryf = serial + pid;
            richTextBox2.Text = Hash(veryf);
            //richTextBox1.Text = "asd";

            //MessageBox.Show("processor: " + pid + '\n' + name + "\nHardDrive: " + h_id + '\n' + serial, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //insert(pid, name, 0);
            //insert(h_id, serial, 1);
       
            //string veryf_proc = read(name, 0);
            //string veryf_hard = read(h_id, 1);

            //RegistryKey currentUserKey = Registry.CurrentUser;

            bool license = true;



            //if (veryf_proc != null && veryf_hard != null)
            //{
            //    if (currentUserKey.OpenSubKey("Application", true) != null)
            //    {
            //        RegistryKey Key = currentUserKey.OpenSubKey("Application",true);
            //        if (Key.GetValue(Hash("KEY")) != null)
            //        {
            //            if (veryf_proc != Key.GetValue(Hash("KEY")).ToString())
            //                license = false;
            //        }
            //        else
            //        if (veryf_proc == Hash(pid) && veryf_hard == Key.GetValue(Hash("KEYY")).ToString())
            //        {
            //            Key.SetValue(Hash("KEY"), veryf_proc);
            //            license = true;
            //        }
            //        else license = false;

            //        if (Key.GetValue(Hash("KEYY")) != null)
            //        {
            //            if (veryf_hard != Key.GetValue(Hash("KEYY")).ToString())
            //                license = false;
            //        }
            //        else
            //        if (veryf_hard == Hash(serial) && veryf_proc == Key.GetValue(Hash("KEY")).ToString())
            //        {
            //            Key.SetValue(Hash("KEYY"), veryf_hard);
            //            license = true;
            //        }
            //        else license = false;

            //        Key.Close();
            //    }
            //    else
            //    {
            //        if (veryf_proc == Hash(pid) && veryf_hard == Hash(serial))
            //        {
            //            RegistryKey Key = currentUserKey.CreateSubKey("Application");
            //            Key.SetValue(Hash("KEY"), veryf_proc);
            //            Key.SetValue(Hash("KEYY"), veryf_hard);
            //            Key.Close();
            //        }
            //        else license = false;
            //    }
            //}
            //if (!license)
            //{
            //    MessageBox.Show("Данная копия программы не является лицензионной! Обратитесь к разработчку для приобритения лицензии!","Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    Process.GetCurrentProcess().Kill();
            //}    

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text == Hashh(Hash(veryf)))
                MessageBox.Show("OK", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }
    }
}
