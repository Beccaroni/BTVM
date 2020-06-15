using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Windows.Forms;

// My using Declarations
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BTVM.Interfaces;
using static BTVM.Classes.Factory;



namespace BTVM
{
    
    public partial class Form2 : Form, IMessageListener1

    {
        
        public Form2()
        {
            InitializeComponent();

            Broadcaster().AddListener(this);

            Closing += Form2_Closing;    
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            
        }


        //****************************************************************************************************************************
        // NEW FANGLED LISTENER SHIT

        /// <summary>
        /// Listener here only reacts to Form1 messages
        /// </summary>
        /// <param name="Message">Incoming message</param>
        /// <param name="sender">Calling form</param>
        public void OnListen(List<string> idArray, string apiResponse, Form sender)
        {
            if (sender is Form1)
            {
                List<string> cbList = new List<string>();
                cbList = idArray;

                foreach (string item in cbList)
                {
                    comboBox1.Items.Add(item);
                }
                string formated = JValue.Parse(apiResponse).ToString(Formatting.Indented);
                richTextBox1.AppendText(formated);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        // NEW FANGLED LISTENER SHIT
        //****************************************************************************************************************************

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > -1)
            {
                //work around because I haven't figured out how to have a second Listener
                List<string> showName= new List<string>();

                
                string jsonData = richTextBox1.Text.ToString();
                JObject data = JObject.Parse(jsonData);
                
                showName.Add(comboBox1.SelectedItem.ToString());
                showName.Add((string)data["data"][comboBox1.SelectedIndex]["seriesName"]);
                showName.Add((string)data["data"][comboBox1.SelectedIndex]["status"]);

                string blank= null;
                Broadcaster().Broadcast(showName, blank, this);
                this.Close();
            }
            else
            {
                MessageBox.Show("You must select a ID from dropdown list", "ERROR");
            }
        }
               
        private void Form2_Closing(object sender, CancelEventArgs e)
        {
            Broadcaster().RemoveListener(this);
        }
        

        
    }
    
}
