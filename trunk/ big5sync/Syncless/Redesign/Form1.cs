using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Redesign
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> paths = new List<string>();
            paths.Add(@"C:\Documents and Settings\Wysie\Desktop\SyncTest\TestA");
            paths.Add(@"C:\Documents and Settings\Wysie\Desktop\SyncTest\TestB");
            paths.Add(@"C:\Documents and Settings\Wysie\Desktop\SyncTest\TestC");
            paths.Add(@"C:\Documents and Settings\Wysie\Desktop\SyncTest\TestD");

            new Comparer().RawComparer(paths);
        }
    }
}
