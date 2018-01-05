using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Configuration;

namespace TeamCityMonitor
{
    public partial class Form2 : Form
    {
        string thisExePath = System.IO.Directory.GetCurrentDirectory() + "\\" + "TeamCityMonitor.exe";
        string encodedPassword;
        string configParameter;
        string codeParameter;
        public Form2()
        {
            InitializeComponent();
            textBox1.Text = ConfigurationManager.AppSettings["URL"];
            textBox2.Text = ConfigurationManager.AppSettings["user"];
            textBox3.Text = Base64Decode(ConfigurationManager.AppSettings["password"]);
            for (int i = 0; i <= 9; i++)
                {
                configParameter = ConfigurationManager.AppSettings[String.Format("projects{0}", i)];
                codeParameter = ConfigurationManager.AppSettings[String.Format("code{0}", i)];
                if (configParameter != "")
                    {
                    textBox4.Text += String.Format("{0};{1}{2}",configParameter, codeParameter, Environment.NewLine);
                    }
            }
            textBox4.Text = textBox4.Text.TrimEnd();
            checkBox1.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["automaticChecking"]);
            domainUpDown1.Text = ConfigurationManager.AppSettings["automaticCheckingPeriod"];
        }

        private void Close_Click(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }

        private void UpdateConfig(string key, string value)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(thisExePath);
            configFile.AppSettings.Settings[key].Value = value;
            configFile.Save();

            /*
             пишет в конфиг файл самой программы нужную нам информацию
            */
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string errorInfo = "";
            bool err = false;
            try
            {
                UpdateConfig("URL", textBox1.Text);
                UpdateConfig("user", textBox2.Text);
                encodedPassword = Base64Encode(textBox3.Text);//кодируем пароль
                UpdateConfig("password", encodedPassword);
                UpdateConfig("automaticChecking", Convert.ToString(checkBox1.Checked));//записываем параметр автопроверки
                if (checkBox1.Checked == true)
                    UpdateConfig("automaticCheckingPeriod", Convert.ToString(domainUpDown1.Text));//записываем интервал автопроверки, если чекбокс установлен
                if (textBox4.Lines.Count() > 0 && textBox4.Lines.Count() <= 10)
                {
                    //очищаем все значения, это нужно в случае, если текущее кол-во textBox4.Lines меньше, чем было 
                    for (int i = textBox4.Lines.Count(); i < 0; i--)
                    {
                        UpdateConfig(String.Format("projects{0}", i), "");
                    }
                    //здесь записываем новые значения
                    for (int i = 0; i < textBox4.Lines.Count(); i++)
                    {
                        string[] parts = textBox4.Lines[i].Split(';');
                        if (parts.Length == 2)
                        {
                            UpdateConfig(String.Format("projects{0}", i), parts[0]);
                            UpdateConfig(String.Format("code{0}", i), parts[1]);
                        }
                        else
                        {
                            err = true;
                            errorInfo =
                             String.Format("Ошибка заполнения строки '{0}'. Пожалуйста, заполните значения проектов по формату: имя;btId",
                             textBox4.Lines[i]);
                        }
                    }
                    UpdateConfig("valueOfProjects", Convert.ToString(textBox4.Lines.Count()));
                }
                else if (textBox4.Lines.Count() > 10)
                {
                    err = true;
                    errorInfo = "Можно добавить до 10 проектов (включительно). Уменьшите количество проектов и попробуйте ещё.";
                }
                    
                if (err == true)
                    MessageBox.Show(errorInfo, "Ошибка!");

            }

            catch (OutOfMemoryException)
            {
                
            }
            finally
            {
                this.Close();
                Application.Restart();
            }

        }
    }

}
