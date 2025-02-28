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
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            if(Settings.language == "Русский")
            {
                Text = "О программе";
                label1.Text = "Версия 0.0.1";
                label2.Text = "Автор: Борисова Алина Алексеевна";
                label3.Text = "Языковой процессор";
            }
            else if (Settings.language == "English")
            {
                Text = "About";
                label1.Text = "Version 0.0.1";
                label2.Text = "Author: Borisova Alina Alekseevna";
                label3.Text = "Language processor";
            }
        }
    }
}
