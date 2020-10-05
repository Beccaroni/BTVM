using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTVM
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        private void OK_button_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                if (System.IO.Directory.Exists(textBox1.Text))
                {
                    BTVM.Properties.Settings.Default.MediaFolder = textBox1.Text;
                    BTVM.Properties.Settings.Default.Save();
                    this.Close();

                }
                else
                {
                    MessageBox.Show("Your media directory does not exist.", "ERROR");
                }
            }
            else
            {
                MessageBox.Show("You must type in media directory.", "ERROR");
            }
            
        }
    }
}
