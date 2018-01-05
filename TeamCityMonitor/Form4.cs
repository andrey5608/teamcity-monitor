using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Configuration;
using RestSharp;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using RestSharp.Authenticators;
using Newtonsoft.Json.Linq;
using System.Timers;
using System.Threading;

namespace TeamCityMonitor
{
    
    public partial class Form4 : Form
    {

        string thisExePath = System.IO.Directory.GetCurrentDirectory() + "\\" + "workPanel.exe";
        string url = ConfigurationManager.AppSettings["teamCityUrl"];
        //string[] teamCityProjectId = { "bt173", "WebQms_37Kazan", "bt199", "bt202", "bt114", "TransportGate_480" };
        string apiURL = "httpAuth/action.html?add2Queue=";
        string configKey;
        string configParameter;
        List<string> projectIds = new List<string>();
        bool automaticChecking;// параметр автоматической проверки коммитов
        int automaticCheckingPeriod;// период автопроверки


        public Form4()
        {

            InitializeComponent();

            for (int i = 0; i <= 9; i++)
            {
                configKey = String.Format("code{0}", i);
                configParameter = ConfigurationManager.AppSettings[configKey];
                projectIds.Add(configParameter);
            }

            var list = new List<string>();

            for (int i = 0; i <= 9; i++)
            {
                configKey = String.Format("projects{0}", i);
                configParameter = ConfigurationManager.AppSettings[configKey];
                if (configParameter != "")
                {
                    list.Add(configParameter);
                }
            }

            checkedListBox1.Items.Clear();

            foreach (string parameter in list)
            {
                checkedListBox1.Items.AddRange(new object[] {
            parameter});
            }

            textBox5.Text = ConfigurationManager.AppSettings["code0"]; //устанавливаем первый код проекта в текстовое поле 5

            automaticCheck();
        }
        /*
        private void Close_Click(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }*/


        System.Timers.Timer autoCheckingPendingsTimer;

        public void CreateTimer(int minutes)
        {
            if (autoCheckingPendingsTimer == null)
            {
                autoCheckingPendingsTimer = new System.Timers.Timer();
                autoCheckingPendingsTimer.AutoReset = false; // Чтобы операции не перекрывались
                autoCheckingPendingsTimer.Interval = minutes * 60 * 1000;
                autoCheckingPendingsTimer.Elapsed += OnAutoCheckingPendingsTimerElapsed;
                autoCheckingPendingsTimer.Enabled = true;
                autoCheckingPendingsTimer.Start();
            }
        }

        private void OnAutoCheckingPendingsTimerElapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                checkPendings();

            }
            finally
            {
                autoCheckingPendingsTimer.Enabled = true;
            }
        }
        private void automaticCheck()
        {
            automaticChecking = Convert.ToBoolean(ConfigurationManager.AppSettings["automaticChecking"]);
            automaticCheckingPeriod = Convert.ToInt32(ConfigurationManager.AppSettings["automaticCheckingPeriod"]);
            if (automaticChecking == true)
            {
                /*var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromMinutes(automaticCheckingPeriod);

                var timer = new System.Threading.Timer((e) =>
                {
                    checkPendings();
                }, null, startTimeSpan, periodTimeSpan);*/

                CreateTimer(automaticCheckingPeriod);
            }
        }

        private void checkPendings()
        {
            var client = new RestClient();
            string baseUrlString = ConfigurationManager.AppSettings["URL"];// берём URL из конфигурационного файла
            string apiPart = String.Format("httpAuth/app/rest/changes?locator=buildType:(id:{0}),pending:true", textBox5.Text);//берём часть URL с API из полей 1 и 5
            client.BaseUrl = new Uri(baseUrlString);
            string decodedPassword = Form2.Base64Decode(ConfigurationManager.AppSettings["password"]);//декодируем пароль
            client.Authenticator = new HttpBasicAuthenticator(ConfigurationManager.AppSettings["user"], decodedPassword);//авторизуемся под логином и паролем из полей 3 и 4


            var request = new RestRequest(apiPart, Method.GET);// создаём REST-запрос GET методом


            IRestResponse response = client.Execute(request);
            string excMessage = "";
            string resp = Convert.ToString(response.Content);
            if (Convert.ToString(response.Content) == "" || Convert.ToString(response.ErrorException) != "")
            {
                if (Convert.ToString(response.ErrorException) != "")
                {
                    excMessage = ". Произошла ошибка. Текст исключения: " + Convert.ToString(response.ErrorException);
                    MessageBox.Show(String.Format("Successful:{0} {1}", Convert.ToString(response.IsSuccessful), excMessage));
                }
            }

            try
            {
                dynamic stuff = JObject.Parse(resp);
                if (Convert.ToString(stuff.count) == "0")
                {
                    label8.ForeColor = Color.Black;
                    label8.Text = "0";//если количества нет или ответ пуст
                }

                else
                {
                    label8.ForeColor = Color.Green;
                    label8.Text = stuff.count;//если количество всё же есть
                }


            }


            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                /*label8.Text = "н/д (ошибка)";//если вместо json пришёл мусор/исключение/ошибка
                label8.ForeColor = Color.Red;*/
                MessageBox.Show("Newtonsoft.Json.JsonReaderException. Вместо json пришёл мусор/исключение/ошибка. " + ex.Message, "Ошибка");
            }
            catch(InvalidOperationException)
            {

            }






        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var list = sender as CheckedListBox;//снимаем все остальные элементы при одном поставленном
            if (e.NewValue == CheckState.Checked)
                foreach (int index in list.CheckedIndices)
                    if (index != e.Index)
                        list.SetItemChecked(index, false);
            if (projectIds.Count > checkedListBox1.SelectedIndex)
                textBox5.Text = projectIds.ElementAt(checkedListBox1.SelectedIndex);//записывает в поле 5 значение из связанного массива teamCityProjectId(bt144 и т.д.)
            else textBox5.Text = "";
        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            label7.Text = "-";
            var client = new RestClient();
            string baseUrlString = ConfigurationManager.AppSettings["URL"];// берём
            string apiPart = apiURL + textBox5.Text;//берём часть URL с API из полей 1 и 5
            client.BaseUrl = new Uri(baseUrlString);
            client.Authenticator = new HttpBasicAuthenticator(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);//авторизуемся под логином и паролем из полей 3 и 4


            var request = new RestRequest(apiPart, Method.POST);

            var path = System.IO.Directory.GetCurrentDirectory() + "\\" + "build.xml";
            string xmlString = System.IO.File.ReadAllText(path);

            request.AddParameter("application/xml", xmlString, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            string excMessage = "";
            if (Convert.ToString(response.Content) != "" || Convert.ToString(response.ErrorException) != "")
            {
                if (Convert.ToString(response.ErrorException) != "") excMessage = ". Произошла ошибка. Текст исключения: " + Convert.ToString(response.ErrorException);
                label7.ForeColor = Color.Red;
                label7.Text = "Неудачно";
                MessageBox.Show(String.Format("Successful:{0} {1}", Convert.ToString(response.IsSuccessful), excMessage));
            }
            else
            {
                label7.ForeColor = Color.Green;
                label7.Text = "Успешно";
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            checkPendings();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();
        }
    }

}
