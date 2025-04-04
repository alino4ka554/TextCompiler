﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextCompiler
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
            if (Settings.language == "Русский")
            {
                Text = "Справка";
                webBrowser1.DocumentText = Properties.Resources.Справка; 
            }
            else
            {
                Text = "Help";
                webBrowser1.DocumentText = Properties.Resources.Help;
            }

        }
    }
}
