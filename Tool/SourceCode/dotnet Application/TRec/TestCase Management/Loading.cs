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
    public partial class Loading : Form
    {
        public Loading()
        {
            InitializeComponent();
        }
        public Loading(string Message)
        {
            InitializeComponent();
            label1.Text = Message;
        }
    }
}
