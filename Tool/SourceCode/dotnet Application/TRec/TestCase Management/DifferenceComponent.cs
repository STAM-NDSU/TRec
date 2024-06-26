using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestCase_Management
{
    public partial class DifferenceComponent : UserControl
    {
        public DifferenceComponent(string file1,string file2)
        {
            InitializeComponent();
            label1.Text = file1;
            label2.Text = file2;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var theShowDiff = new ShowDiff(label1.Text, label2.Text);
            theShowDiff.ShowDialog();
        }
    }
}
