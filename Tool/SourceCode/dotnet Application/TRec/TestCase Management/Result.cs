using BusinessLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestCase_Management
{
    public partial class Result : Form
    {

        public Result()
        {
            InitializeComponent();
        }

        private void Result_Load(object sender, EventArgs e)
        {

            panel3.Controls.Clear();
            //Thread th = new Thread(() => new Loading().ShowDialog());
            //th.Start();


            foreach (SourceClass sourceClass in MasterObject.CurrentProject.theSourceClasses)
            {
                if (sourceClass != null)
                {
                    //new BusinessLogic().FindChangedSourceMethods(sourceClass);
                    //
                    if (MasterObject.CommitForRecomendation.Any(x => x.FileName == sourceClass.SourceClassFileName))

                        if (sourceClass.MethodSets.Any())
                        {
                            //new Bl_Antlr().ExtractMethodCalls(sourceClass);
                            Details_UI theDetailsUI = new Details_UI(sourceClass, pnl_Main);
                            theDetailsUI.Dock = DockStyle.Top;
                            theDetailsUI.Show();
                            panel3.Controls.Add(theDetailsUI);
                        }
                }

            }
            //th.Abort();

        }
    }
}
