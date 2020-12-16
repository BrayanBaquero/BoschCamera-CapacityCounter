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
    public partial class SetPoint : Form
    {
        public delegate void sendCred(string sp,string Messmx, string Messnorm, bool outX);
        public event sendCred pasado;
        public SetPoint()
        {
            
            InitializeComponent();
            textBox1.Text = Properties.Settings.Default.SetPoint.ToString();
            textBoxMax.Text = Properties.Settings.Default.MessMax.ToString();
            textBoxNorm.Text = Properties.Settings.Default.MessNorm.ToString();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            pasado(textBox1.Text, textBoxMax.Text,textBoxNorm.Text,checkBox1.Checked);
            Properties.Settings.Default["SetPoint"] = textBox1.Text;
            Properties.Settings.Default["MessMax"] = textBoxMax.Text;
            Properties.Settings.Default["MessNorm"] = textBoxNorm.Text;
            Properties.Settings.Default["outEx"] = checkBox1.Checked;
            Properties.Settings.Default.Save();
            this.Dispose();
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            UpdateGui();
        }

        private void TextBoxMax_TextAlignChanged(object sender, EventArgs e)
        {
            UpdateGui();
        }

        private void TextBoxNorm_TextChanged(object sender, EventArgs e)
        {
            UpdateGui();
        }
        void UpdateGui()
        {
            if (textBox1.Text.Length > 0 && textBoxMax.Text.Length > 0 && textBoxNorm.Text.Length > 0)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }
    }
}
