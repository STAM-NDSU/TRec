using BusinessLogic;
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
    public partial class Details_Method : UserControl
    {
        MethodSet theMethodSet = null;
        public Details_Method(MethodSet theMethodSet)
        {
            InitializeComponent();
            this.theMethodSet = theMethodSet;
        }

        public Details_Method()
        {
        }

        private void Details_Method_Load(object sender, EventArgs e)
        {

            List<TestMethod_Analytic> theTestAna = new List<TestMethod_Analytic>();

            groupBox1.Text = theMethodSet.SourceMethodName;

            //foreach (var testmethodanaly in theMethodSet.TestMethods_Analytics)
            //{
            //    testmethodanaly.Parsed_ChangedDateTime.Clear();

            //    foreach (string dateTimeString in testmethodanaly.ChangedDateTime)
            //    {
            //        if (DateTimeOffset.TryParse(dateTimeString, out DateTimeOffset parsedDateTime))
            //        {
            //            testmethodanaly.Parsed_ChangedDateTime.Add(parsedDateTime);
            //        }
            //        if (parsedDateTime < MasterObject.CommitForRecomendation.FirstOrDefault().DateTimeCommited)
            //            if (!theTestAna.Any(x => x.TestMethodName == testmethodanaly.TestMethodName))
            //            {
            //                var tsts = new TestMethod_Analytic();
            //                tsts.TestMethodName = testmethodanaly.TestMethodName;
            //                tsts.Parsed_ChangedDateTime.Add(parsedDateTime);
            //                theTestAna.Add(tsts);
            //            }
            //            else
            //            {
            //                theTestAna.FirstOrDefault(x => x.TestMethodName == testmethodanaly.TestMethodName).Parsed_ChangedDateTime.Add(parsedDateTime);
            //            }
            //    }
            //}
            var filteredAndSorted = FilterAndSortByDate(theMethodSet.TestMethods_Analytics, MasterObject.CommitForRecomendation.FirstOrDefault().DateTimeCommited);

            var sortedAnalytics = filteredAndSorted
                                    .Take(5)
                                    .Select(item => item.TestMethodName)
                                    .ToList();

            if (sortedAnalytics.Any())
                lbl_Recomen_Hist.Text = String.Join("\n", sortedAnalytics);
            else
                lbl_Recomen_Hist.Text = "We Could not find any Recomendations.";

            lbl_Recomen_All.Text = String.Join("\n", theMethodSet.TestMethods_Analytics.Select(x => x.TestMethodName).ToList());
            // lbl_Recomen_All.Text = "Test \n Test \n Test";

        }

        public static List<(string TestMethodName, int Frequency, List<DateTimeOffset> FilteredDates)> FilterAndSortByDate(List<TestMethod_Analytic> methods, DateTimeOffset myDateTimeValue)
        {
            var filteredList = methods
                .Select(method => new
                {
                    method.TestMethodName,
                    FilteredDates = method.ParsedChangedDateTime.Where(date => date <= myDateTimeValue).ToList()//Fix It
                })
                .Where(x => x.FilteredDates.Count > 0) // Optional: Filter out methods with 0 relevant dates
                .Select(x => (x.TestMethodName, Frequency: x.FilteredDates.Count, x.FilteredDates))
                .OrderByDescending(x => x.Frequency) // Sort by Frequency (count of FilteredDates) in descending order
                .ThenBy(x => x.TestMethodName) // Then by TestMethodName in ascending order for tie-breakers
                .ToList();

            return filteredList;
        }


        private void btn_Diff_Click(object sender, EventArgs e)
        {

            Clipboard.SetText(lbl_Recomen_Hist.Text);
            //new ShowDiff(theMethodSet.SourceMethodV1Snip, theMethodSet.SourceMethodV2Snip).ShowDialog();
        }

        private void btn_copymethodname_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(groupBox1.Text);
        }

        public List<string> EvaluationBL(MethodSet theMethodSet,DateTimeOffset theDateTime)
        {
            var filteredAndSorted = FilterAndSortByDate(theMethodSet.TestMethods_Analytics.FindAll(x=>x.TestMethodName.ToLower().Contains("test")), theDateTime);

            var sortedAnalytics = filteredAndSorted
                                    //.Take(5)
                                    .Select(item => item.TestMethodName)
                                    .ToList();
            return sortedAnalytics;
        }
    }
}
