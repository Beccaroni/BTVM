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

//DO FIRST FIX CODE FOR DIRECTORY: 
//FIGURE OUT HOW TO SAVE AS A SETTTING
//GOING TO NEED A CLASS MEMBER FOR WORKING FOLDER
//NEED A TEXTBOX TO PULL FROM CONFIG 
//CHECKBOX PROMPT FOR ADD
//SAVE TO CLASS MEMBER

//TODO list
//1. method to read list of show folders and put into DB - DONE
//2. method to read show info from API and put into DB
//3. method to read list of show episodes and put into DB - 
//4. method to read episode info from API and put into DB
//5. code to iterate through list of shows and make API calls
//6. code to put show meta data into DB
//7. code to put episode info into DB - from api (name of ep)
//8. code to put episode info into DB - from my files (video type, size, lenght etc. USE windows system32.dll??)
//9. missing episodes search
//10. upcoming episodes search
//11. episode file metadata, size, frame height/width, file type


//LISTENER STUFF
//need to add to current listener to pass a string from form1 to form2 of apiresponse already have code to parse it out
//at some point need to delete any extra listener stuff

namespace BTVM
{

    public partial class Form1 : Form, IMessageListener1
    {
        const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\PROJECTS\C#\BTVM\BTVM.mdf;Integrated Security=True;Connect Timeout=30";
        static readonly HttpClient client = new HttpClient();
        string apiResponse = "";
        

        public Form1()
        {
            //initialize the messeging components
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
            //textBox1.AppendText(apiResponse);
            //textBox1.AppendText(System.Environment.NewLine + System.Environment.NewLine);


            //get_show_id(apiResponse);
            //textBox1.AppendText(apiResponse);
            //textBox1.AppendText(System.Environment.NewLine + System.Environment.NewLine);

            //pulls JSON data from TVdb and adds to SQL
            update_shows(config);

            //code to com
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
            List<string> showList = new List<string>();

            folderData = read_show_folder("D:\\PROJECTS\\Fake show folder");

            //get show series ID, name, status and add to shows table
            foreach (string fItem in folderData)
            {
                get_show_metadata(fItem, config.Token);
            }            

            //get all show IDs from SHOW table
            showList = SQL_get_all_show_IDs();
            
            //get episode metadata from API using show IDs
            foreach (string item in showList)
            {
                get_episode_metadata(item, config.Token);
            }

            read_show_episodes();

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

        private List<string> read_show_episodes()
        {
            List<string> shows = new List<string>();
            List<string> episodes = new List<string>();



            //read directory of shows into list
            //for loop to iterate through show list
            //code to go into season directories and add episdoes to list
            //SQL method to insert episodes as into Episodes table as found

            return episodes;
        }

        async void get_show_metadata(string seriesName, string authToken) //gets the series meta data (name, id, status etc)
        {
            //curl -X GET --header 'Accept: application/json' --header 'Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1NjI1NTE5NjIsImlkIjoiIiwib3JpZ19pYXQiOjE1NjI0NjU1NjJ9.kIzQdUDKDC7Apk3clfm9aiT8e17pRyjUw0agt9sehEZMAHI_AvYKl9__8OznMEwRCU8EW-rBbxqFTg4gG_choFMTa5KNFuGa4KRB2VZX07FhrSbmRhk12WRYQ04U_bRpiY3Np95Sey88h2n001XdkXdBdd5wk6ShmG9yIfAojby1sDqvXeOgUwoxdvBttQxWfCNQdQDCBlQ0TCA6z9jhTu7qP_g_qrCXzW7kA6FlgC-hXTsqL25ITA57W2-4Zq-vgssQFKvCKkBkNTCtSacFhiIzeiqyj9tONCqb962QfdAHht4VNGPn3n0nAtwFTqJrI_mHm7DJH4LznCPLNRwYDQ' 'https://api.thetvdb.com/search/series?name=APB'
            string URI = "https://api.thetvdb.com/search/series?name=" + seriesName;
            var response = new HttpResponseMessage();
            List<string> idList = new List<string>();
            List<string> showInfo = new List<string>();
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
                    temp = JSON_parse_show_id(apiResponse, i);
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
                showInfo.Add(JSON_parse_show_id(apiResponse, 0));
                showInfo.Add(JSON_parse_show_name(apiResponse, 0));
                showInfo.Add(JSON_parse_show_status(apiResponse, 0));
                SQL_insert_show(showInfo);
            }                      
        }

        async void get_episode_metadata(string seriesID, string authToken)
        {
            string URI = "https://api.thetvdb.com/series/" + seriesID + "/episodes";
            var response = new HttpResponseMessage();
            List<string> allEpisodes = new List<string>();
            
            try
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                response = client.GetAsync(URI).Result;
                apiResponse = await response.Content.ReadAsStringAsync();
                textBox1.AppendText(apiResponse);
            }
            catch (Exception e)
            {
                textBox1.AppendText(e.Source);
            }            

            allEpisodes = JSON_parse_episode_metadata(apiResponse);

            for (int i = 0; i < allEpisodes.Count; i+=5)
            {
                SQL_insert_episode(allEpisodes.GetRange(i, 5));
            }          
        }

        
        private string JSON_parse_show_id(String jsonData, int iterator)
        {
            JObject data = JObject.Parse(jsonData);
            string id = (string)data["data"][iterator]["id"];
            return id;
        }

        private string JSON_parse_show_name(String jsonData, int iterator)
        {
            JObject data = JObject.Parse(jsonData);
            string name = (string)data["data"][iterator]["seriesName"];
            return name;
        }
        private string JSON_parse_show_status(String jsondata, int iterator)
        {
            JObject data = JObject.Parse(jsondata);
            string status = (string)data["data"][iterator]["status"];
            return status;
        }
        private List<string> JSON_parse_episode_metadata(string jsonData)
        {
            List<string> metaData = new List<string>();
                       
            JObject episodeData = JObject.Parse(jsonData);
            int count = episodeData["data"].Count();

            for (int i = 0; i < count; i++)
            { 
                metaData.Add((string)episodeData["data"][i]["id"]);
                metaData.Add((string)episodeData["data"][i]["airedSeason"]);
                metaData.Add((string)episodeData["data"][i]["airedEpisodeNumber"]);
                metaData.Add((string)episodeData["data"][i]["episodeName"]);
                metaData.Add((string)episodeData["data"][i]["seriesId"]);
            }  

            return metaData;
        }



        //*********************************************************************************************************
        //SQL Methods
        //*********************************************************************************************************
               
        //NEED TO INCLUDE ID ADND STATUS FOR COMAPRO
        private List<string> SQL_get_all_show_IDs()
        {
            List<string> showIDs = new List<string>();
            
            using (SqlConnection SQLconn = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("SELECT seriesId FROM  SHOWS", SQLconn);

                try
                {
                    if (SQLconn.State == ConnectionState.Closed)
                    {
                        SQLconn.Open();
                    }

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        showIDs.Add(reader.GetString(0));
                        textBox1.AppendText(reader.GetString(0));
                    }
                    reader.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "ERROR");
                }
                SQLconn.Close();
            }
            return showIDs;
        }

        private  bool SQL_check_show_record(string showID)
        {
            using (SqlConnection SQLconn = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("SELECT seriesId FROM SHOWS WHERE seriesId = '" + showID + "'", SQLconn);
                
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
                SqlCommand cmdExists = new SqlCommand("SELECT * FROM SHOWS WHERE SHOWS.seriesId = '" + showData[0] + "'", SQLconn);
                SqlCommand cmdInsert = new SqlCommand("INSERT INTO SHOWS (seriesId, seriesName, status) VALUES ('" + showData[0] + "', '" + showData[1] + "', '" + showData[2] +"')", SQLconn);

                //need this if else for checking if ID already exists
                
                if (SQLconn.State == ConnectionState.Closed)
                {
                    SQLconn.Open();
                }

                object showExists = cmdExists.ExecuteScalar();
                
                if (showExists == null)
                {
                    try
                    {
                        cmdInsert.ExecuteScalar();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "ERROR");
                    }
                }                                    
                SQLconn.Close();                
            }
           
        }

        private void SQL_insert_episode(List<string> episodeData)
        {
            //SQL -  seriesId seasonNum episodeNum episodeName
            //JSON - seriesId airedSeason airedEpisodeNumber episodeName
            
            using (SqlConnection SQLconn = new SqlConnection(connectionString))
            { 
                SqlCommand cmdInsert = new SqlCommand("INSERT INTO EPISODES (episodeId, seasonNum, episodeNum, episodeName,seriesId) VALUES ('" + episodeData[0] + "', '" + episodeData[1] + "', '" + episodeData[2] + "', '" + episodeData[3] + "', '" + episodeData[4] + "')", SQLconn);

                try
                {
                    if (SQLconn.State == ConnectionState.Closed)
                    {
                        SQLconn.Open();
                    }

                    cmdInsert.ExecuteScalar();
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
            private string showDir;

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
            public string ShowDir
            {        
                get
                {
                    return showDir;
                }
                set
                {
                    showDir = value;
                }
            }

        }
    }
}
    

