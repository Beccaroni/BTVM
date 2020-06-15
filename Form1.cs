using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

// My using Declarations
using System.Data.SqlClient;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BTVM.Interfaces;
using static BTVM.Classes.Factory;

//TESTING RESPONSE DATA

//TODO list
//1. method to read list of show folders and put into DB
//2. method to read list of show episodes and put into DB
//3. code to iterate through list of shows and make API calls
//4. code to put show meta data into DB
//5. code to put episode info into DB - from api (name of ep)
//6. code to put episode info into DB - from my files (video type, size, lenght etc. USE windows system32.dll??)

//LISTENER STUFF
//need to add to current listener to pass a string from form1 to form2 of apiresponse already have code to parse it out
//at some point need to delete any extra listener stuff

namespace BTVM
{

    public partial class Form1 : Form, IMessageListener1
    {
        const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\PROJECTS\C#\BTVM_TEST\BTVM.mdf;Integrated Security=True;Connect Timeout=30";
        static readonly HttpClient client = new HttpClient();
        string apiResponse = "";

        public Form1()
        {
            InitializeComponent();
            Broadcaster().AddListener(this);
            Closing += Form1_Closing;
            Shown += Form1_Shown;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            last_update();
        }

        public void OnListen(List<string> listMessage, string strMessage, Form sender)
        {
            if (sender is Form2)
            {
                SQL_insert_show(listMessage);
            }
        }


        private void last_update()
        {
            if ((DateTime.Now - Properties.Settings.Default.LastUpdate).TotalDays > 7)
            {
                //Will put code here to update the time ran and call login and update shows or whatnot.
            }
            else
            {
                //staying in this loop to test for now
                textBox1.AppendText("Old time: " + Properties.Settings.Default.LastUpdate + Environment.NewLine);
                Properties.Settings.Default.LastUpdate = System.DateTime.Now;
                Properties.Settings.Default.Save();
                textBox1.AppendText("New time: " + Properties.Settings.Default.LastUpdate);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var config = new AppConfig();

            //add folder location to config class


            //export config date, folder to xml file
            login(config);
            textBox1.AppendText(apiResponse);
            textBox1.AppendText(System.Environment.NewLine + System.Environment.NewLine);


            //get_show_id(apiResponse);
            //textBox1.AppendText(apiResponse);
            //textBox1.AppendText(System.Environment.NewLine + System.Environment.NewLine);

            update_shows(config);

            //SQL_add_shows();
            //SQL_get_shows();
        }

        async void login(AppConfig config) //logs into API using API key, sets token from reply
        {
            string URI = "https://api.thetvdb.com/login";
            string responsebody = "";
            var response = new HttpResponseMessage();

            //convert api object to json formatting
            string loginJSON = JsonConvert.SerializeObject(config, Formatting.Indented);

            //removes the token from api class so it doesnt get sent with login.
            loginJSON = loginJSON.Substring(0, loginJSON.IndexOf(",") + 1);
            loginJSON = loginJSON.Replace(",", "" + Environment.NewLine + "}");

            var buffer = System.Text.Encoding.UTF8.GetBytes(loginJSON);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            try
            {
                response = client.PostAsync(URI, byteContent).Result;
                responsebody = await response.Content.ReadAsStringAsync();
            }

            catch (Exception e)
            {
                textBox1.AppendText(e.Source);
            }

            var reader = new JsonTextReader(new StringReader(responsebody));
            while (reader.Read())
            {
                if (reader.TokenType.ToString() == "String")
                {
                    config.Token = String.Format(reader.Value.ToString());
                }
            }
        }

        private void update_shows(AppConfig config)
        {
            List<string> folderData = new List<string>();

            folderData = read_show_folder("D:\\PROJECTS\\Fake show folder");

            foreach (string fItem in folderData)
            {
                get_show_metadata(fItem, config.Token);
            }


            //needs Update has list of shows we need to get show ID and add to table
        }


        private List<string> read_show_folder(string dirPath)
        {
            List<string> showList = new List<string>();

            foreach (string dirName in Directory.GetDirectories(dirPath))
            {
                string temp = dirName.Substring(dirPath.Length);
                showList.Add(temp);
            }

            return showList;
        }

        async void get_show_metadata(string seriesName, string authToken) //gets the series meta data (name, id, status etc)
        {
            //curl -X GET --header 'Accept: application/json' --header 'Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1NjI1NTE5NjIsImlkIjoiIiwib3JpZ19pYXQiOjE1NjI0NjU1NjJ9.kIzQdUDKDC7Apk3clfm9aiT8e17pRyjUw0agt9sehEZMAHI_AvYKl9__8OznMEwRCU8EW-rBbxqFTg4gG_choFMTa5KNFuGa4KRB2VZX07FhrSbmRhk12WRYQ04U_bRpiY3Np95Sey88h2n001XdkXdBdd5wk6ShmG9yIfAojby1sDqvXeOgUwoxdvBttQxWfCNQdQDCBlQ0TCA6z9jhTu7qP_g_qrCXzW7kA6FlgC-hXTsqL25ITA57W2-4Zq-vgssQFKvCKkBkNTCtSacFhiIzeiqyj9tONCqb962QfdAHht4VNGPn3n0nAtwFTqJrI_mHm7DJH4LznCPLNRwYDQ' 'https://api.thetvdb.com/search/series?name=APB'
            string URI = "https://api.thetvdb.com/search/series?name=" + seriesName;
            var response = new HttpResponseMessage();
            List<string> idList = new List<string>();
            bool found = false;

            try
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                response = client.GetAsync(URI).Result;
                apiResponse = await response.Content.ReadAsStringAsync();
                textBox1.AppendText(apiResponse);
                textBox1.AppendText(System.Environment.NewLine + System.Environment.NewLine);
            }

            catch (Exception e)
            {
                MessageBox.Show(e.Source, "ERROR");
            }

            //code to parse if more than one series was returned open up a new form to chose the correct one
            JObject showData = JObject.Parse(apiResponse);
            textBox1.AppendText(showData["data"].Count().ToString());

            if (showData["data"].Count() > 1)
            {
                //loop to grab show id' 
                for (int i = 0; i < showData["data"].Count(); i++)
                {
                    string temp;
                    temp = get_show_id(apiResponse, i);
                    found = SQL_check_show_record(temp);
                    if (found == true)
                    {
                        break;
                    }
                    else
                    {
                        idList.Add(temp);
                    }
                }

                if (found == false)
                {
                    Form2 idSelForm = new Form2();
                    idSelForm.ControlBox = false;
                    idSelForm.FormClosing += delegate { this.Show(); };
                    Broadcaster().Broadcast(idList, apiResponse, this);
                    idSelForm.Show();
                    this.Hide();
                }
            }
            else
            {
                //need to convert response to show id, name, status and insert
                //SQL_insert_show()
            }
        }

        async void get_epi_info(string seriesID, string authToken)
        {
            string URI = "https://api.thetvdb.com/series/" + seriesID + "/episodes";
            var response = new HttpResponseMessage();

            try
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                response = client.GetAsync(URI).Result;
                apiResponse = await response.Content.ReadAsStringAsync();
            }

            catch (Exception e)
            {
                textBox1.AppendText(e.Source);
            }


        }

        private string JSON_parse_show_id(String jsonData)
        {
            JObject data = JObject.Parse(jsonData);
            string id = (string)data["data"][0]["id"];
            return id;
        }
        private string JSON_parse_show_id(String jsonData, int iterator)
        {
            JObject data = JObject.Parse(jsonData);
            string id = (string)data["data"][iterator]["id"];
            return id;
        }

        private List<string> JSON_parse_show_metadata(string jsonData)
        {
            List<string> metaData = new List<string>;
            JObject data = JObject.Parse(jsonData);            
            
           return metaData;
        }


        //*********************************************************************************************************
        //SQL Methods
        //*********************************************************************************************************
               
        //NEED TO INCLUDE ID ADND STATUS FOR COMAPRO
        private List<string> SQL_get_all_shows()
        {
            List<string> shows = new List<string>();
            
            using (SqlConnection SQLconn = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM  SHOWS", SQLconn);

                try
                {
                    if (SQLconn.State == ConnectionState.Closed)
                    {
                        SQLconn.Open();
                    }

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        textBox1.AppendText(reader.GetString(1));
                    }
                    reader.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "ERROR");
                }
                SQLconn.Close();
            }
            return shows;
        }

        private  bool SQL_check_show_record(string showID)
        {
            


            using (SqlConnection SQLconn = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("SELECT id FROM SHOWS WHERE id = '" + showID + "'", SQLconn);
                
                try
                {
                    if (SQLconn.State == ConnectionState.Closed)
                    {
                        SQLconn.Open();
                    }
                    object data = command.ExecuteScalar();
                    SQLconn.Close();

                    if (data == null)
                    {
                        return false;
                    }
                   else
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "ERROR");
                    return false;
                }
                
            }           
        }

        private void SQL_insert_show(List<string> showData)
        {
            using (SqlConnection SQLconn = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("INSERT INTO SHOWS (id, seriesName, status) VALUES('" + showData[0] + "', '" + showData[1] + "', '" + showData[2] +"')", SQLconn);
                
                try
                {
                    if (SQLconn.State == ConnectionState.Closed)
                    {
                        SQLconn.Open();
                    }
                   command.ExecuteScalar();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "ERROR");
                }
                SQLconn.Close();                
            }
           
        }

        //****************************************************************************************************************************
        // NEW FANGLED LISTENER SHIT


        /// <summary>
        /// Listener here only reacts to Form2 messages
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <param name="sender">Calling form</param>


        /// <summary>
        /// Send simple text message to all listeners
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>


        private void Form1_Shown(object sender, EventArgs e)
        {
            //SimpleMessageToChildTextBox.Text = $"From {Name} at {DateTime.Now:F}";
        }

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            Broadcaster().RemoveListener(this);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
        /// <summary>
        /// Show two instances of Form2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowChildFormButton_Click(object sender, EventArgs e)
        {
            var childForm1 = new Form2 { Top = Top, Left = (Left + Width) + 20, Tag = "Right" };
            childForm1.Show();

            var childForm2 = new Form2 { Top = Top, Left = (Left - Width) + 120, Tag = "Left" };
            childForm2.Show();
        }

        /// <summary>
        /// Move any Form2 instance in sync with this form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        // NEW FANGLED LISTENER SHIT
        //****************************************************************************************************************************


        public class AppConfig
        {
            private string apikey = "AAT3EO3BZULRNBKS";
            private string token;
            private DateTime date;
            public string Apikey
            {
                get
                {
                    return apikey;
                }
                set
                {
                    apikey = value;
                }
            }
            public string Token
            {
                get
                {
                    return token;
                }
                set
                {
                    token = value;
                }
            }

            public DateTime Date
            {
                get
                {
                    return date;
                }
                set
                {
                    date = value;
                }
            }

        }
    }
}
    

