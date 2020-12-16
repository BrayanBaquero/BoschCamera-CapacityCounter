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
    public partial class CamaraConfig : Form
    {
        public delegate void sendCred(string dato, int entreda, int salida);
        public event sendCred pasado;
       
        public CamaraConfig()
        {
            InitializeComponent();
            textBoxIP.Text = Properties.Settings.Default.ip.ToString();
            textBoxUSR.Text = Properties.Settings.Default.usuario.ToString();
            textBoxPass.Text = Properties.Settings.Default.contraseña.ToString();
            comboBox1.Text = Properties.Settings.Default.TaskInt.ToString();
            comboBox2.Text = Properties.Settings.Default.TaskOut.ToString();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            String url = textBoxUSR.Text + ":" + textBoxPass.Text + "@" + textBoxIP.Text + ":" + "1756";
            pasado(url,int.Parse(comboBox1.Text),int.Parse(comboBox2.Text));
            Properties.Settings.Default["ip"] = textBoxIP.Text;
            Properties.Settings.Default["usuario"] = textBoxUSR.Text;
            Properties.Settings.Default["contraseña"] = textBoxPass.Text;
            Properties.Settings.Default["TaskInt"] = comboBox1.Text.ToString();
            Properties.Settings.Default["TaskOut"] = comboBox2.Text.ToString();
            Properties.Settings.Default.Save();
            this.Dispose();
        }

        private void CamaraConfig_Load(object sender, EventArgs e)
        {
            UpdateGui();

        }

        private void UpdateGui()
        {
            if(textBoxIP.Text.Length>0 && textBoxUSR.Text.Length>0 && textBoxPass.Text.Length > 0 && comboBox1.SelectedIndex!=-1 && comboBox2.SelectedIndex!=-1)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }

        }

        private void TextBoxIP_TextChanged(object sender, EventArgs e)
        {
            UpdateGui();
        }

        private void TextBoxUSR_TextChanged(object sender, EventArgs e)
        {
            UpdateGui();
        }

        private void TextBoxPass_TextChanged(object sender, EventArgs e)
        {
            UpdateGui();
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGui();
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGui();
        }
    }
}
