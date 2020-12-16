using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpRuntimeCameo
{
    public partial class Cloud : Form
    {
        public delegate void sendCred(string di, string ak, string ass);
        public event sendCred pasado;

        public Cloud()
        {
            InitializeComponent();
            textBoxDI.Text = Properties.Settings.Default.DI.ToString();
            textBoxAK.Text = Properties.Settings.Default.AK.ToString();
            textBoxAS.Text = Properties.Settings.Default.AS.ToString();
            
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            pasado(textBoxDI.Text, textBoxAK.Text, textBoxAS.Text);
            Properties.Settings.Default["DI"] = textBoxDI.Text;
            Properties.Settings.Default["AK"] = textBoxAK.Text;
            Properties.Settings.Default["AS"] = textBoxAS.Text;
            Properties.Settings.Default.Save();
            this.Dispose();
        }

        private void TextBoxDI_TextChanged(object sender, EventArgs e)
        {
            UpdateGui();
        }

        private void UpdateGui() {
            Console.WriteLine(textBoxDI.Text.Length);

            if (textBoxDI.Text.Length == 24 && textBoxAK.Text.Length == 36 && textBoxAS.Text.Length == 64)
            {
                button1.Enabled = true;

            }
            else {
                button1.Enabled = false;
            }
        }

        private void TextBoxAK_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine(textBoxAK.Text.Length);
            UpdateGui();
        }

        private void TextBoxAS_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine(textBoxAS.Text.Length);
            UpdateGui();
        }

        private void Cloud_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
            UpdateGui();
        }
    }
}
