using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextCompiler
{
    public partial class SettingsForm : Form
    {
        public TabControl editTabControl;
        public bool update = false;
        public SettingsForm()
        {
            InitializeComponent();
            numericUpDown1.Value = (decimal)Settings.font.Size;
            comboBox1.Text = Settings.language;
            Text = (Settings.language == "Русский") ? "Настройки" : "Settings";
            label1.Text = (Settings.language == "Русский") ? "Размер шрифта" : "Font Size";
            label2.Text = (Settings.language == "Русский") ? "Язык" : "Language";
            button1.Text = (Settings.language == "Русский") ? "Сохранить" : "Save";
            //editTabControl = tabControl;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            float fontSize = (float)numericUpDown1.Value;
            Font font = new Font(Settings.font.FontFamily, fontSize);
            Settings.font = font;
            if (Settings.language != comboBox1.Text)
            {
                Settings.language = comboBox1.Text;
                //Settings.UpdateFont(editTabControl);
                
                update = true;
            }
            Settings.SaveSettings();
            this.Close();
        }
    }
}
