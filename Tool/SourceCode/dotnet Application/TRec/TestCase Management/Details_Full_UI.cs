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
    public partial class Details_Full_UI : Form
    {
        SourceClass theDetails = null;

        public Details_Full_UI(SourceClass Details)
        {
            InitializeComponent();
            theDetails = Details;
        }
        private void Details_Full_UI_Load(object sender, EventArgs e)
        {
            //theDetails.MethodSets.Clear();
            //tb_Old.Text =theDetails.SourceClass_Raw_V1.Name +" - "+ theDetails.SourceClass_Raw_V1.FullName;
            //tb_new.Text = theDetails.SourceClass_Raw_V2.Name + " - " + theDetails.SourceClass_Raw_V2.FullName;

            //Find the Changed Methods using our Algorithm 
            //new BusinessLogic().FindChangedSourceMethods(theDetails);
            //new BusinessLogic().FindAssociatedTestMethods(theDetails, MasterObject.version1AllFiles);

            foreach (MethodSet methodSet in theDetails.MethodSets)
            {
                if (!methodSet.SourceMethodV1Snip.ToLower().Contains("public"))
                    continue;
                Details_Method theDetailsMethod= new Details_Method(methodSet);
                theDetailsMethod.Show();
                flowLayoutPanel1.Controls.Add(theDetailsMethod);
                flowLayoutPanel1.SetFlowBreak(theDetailsMethod, true);
            }

            //theDetails.TestMethods_HistoricalData = new BusinessLogic().ShowTestMethodsFromHistoricalData(theDetails).Keys.ToList();


            ////if (theDetails.TestMethods_Algo == null || theDetails.TestMethods_Algo.Count)
            //{
            //    new BusinessLogic().FindSourceMethodAndTestMethod(theDetails);
            //    theDetails.TestMethods_Algo = Sets.theSets.FirstOrDefault()?.TestMethods.FirstOrDefault().TestMethodNames ?? new List<string>();
            //    theDetails.TestMethods_Algo = Sets.theSets.FindAll(x => x.SourceMethodFileName == Path.GetFileName(theDetails.OldFile)).SelectMany(y => y.TestMethods.SelectMany(z => z.TestMethodNames)).ToList().Distinct().ToList();
            //}



            //lbl_Recomen_Hist.Text = String.Join("\n", theDetails.TestMethods_HistoricalData);
            //lbl_Recomen_All.Text = String.Join("\n", theDetails.TestMethods_Algo);
        }

        private void btn_Diff_Click(object sender, EventArgs e)
        {
            new ShowDiff(theDetails.SourceClass_Raw_V1_FileContent, theDetails.SourceClass_Raw_V2_FileContent).ShowDialog();
        }
    }
}
