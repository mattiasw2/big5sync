using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SynclessSeamlessTester
{
    public partial class FormSeamlessTester : Form
    {
        private TestLogic testLogic;

        public FormSeamlessTester()
        {
            InitializeComponent();
            Init();
        }


        private void Init()
        {
            testLogic = new TestLogic();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxSourcePath.Text = folderBrowserDialog1.SelectedPath;
                folderBrowserDialog1.Reset();
            }
        }

        private void buttonDestBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxDest.Text = folderBrowserDialog1.SelectedPath;
                folderBrowserDialog1.Reset();
            }
        }

        private void buttonSourceAdd_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBoxSourcePath.Text))
            {
                listBoxSourcePaths.Items.Clear();
                listBoxSourcePaths.Items.AddRange(testLogic.AddToSource(textBoxSourcePath.Text).ToArray());
            }
            textBoxSourcePath.Text = string.Empty;
        }

        private void buttonDestAdd_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBoxDest.Text))
            {
                listBoxDestPaths.Items.Clear();
                listBoxDestPaths.Items.AddRange(testLogic.AddToDest(textBoxDest.Text).ToArray());
            }
            textBoxDest.Text = string.Empty;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBoxSourcePaths.Items.Count > 0 && listBoxDestPaths.Items.Count > 0 &&
                    Convert.ToInt32(textBoxDuration.Text) > 0)
                    testLogic.StartTest(Convert.ToInt32(textBoxDuration.Text));
            }
            catch (FormatException) { }
            catch (OverflowException) { }
        }
    }
}
