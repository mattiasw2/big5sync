using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace SynclessUpdateChecker
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            UpdateChecker updateChecker = new UpdateChecker();
            int newUpdate = updateChecker.GetNewVersion();

            switch (newUpdate)
            {
                case 0:
                    MessageBox.Show("New Update!" + "\n" + updateChecker.GetUpdateUrl);
                    break;
                case 1:
                    MessageBox.Show("You Are Currently Running The Latest Version!");
                    break;
                case -1:
                    MessageBox.Show("Error!");
                    break;
                default:
                    Debug.Fail("Invalid value!");
                    break;
            }                
        }
    }
}
