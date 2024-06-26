using BusinessLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestCase_Management
{
    public partial class Details_UI : UserControl
    {
        SourceClass theDetails = null;
        Panel thePanel=null;
        public Details_UI()
        {
            InitializeComponent();
        }
        public Details_UI(SourceClass Details,Panel theMainPanel)
        {
            InitializeComponent();
            theDetails = Details;
            thePanel = theMainPanel;
        }

        private void Details_UI_Load(object sender, EventArgs e)
        {
            button3.Text=theDetails.SourceClassFileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            thePanel.Controls.Clear();
            Details_Full_UI theDetails_Full = new Details_Full_UI(theDetails);
            theDetails_Full.Dock = DockStyle.Fill;
            theDetails_Full.TopLevel = false;
            thePanel.Controls.Add(theDetails_Full);
            theDetails_Full.Show();
        }
    }
}
