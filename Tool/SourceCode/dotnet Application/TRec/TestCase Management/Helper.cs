using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace TestCase_Management
{
    public static class Helper
    {
        private static readonly object lockObj = new object();
        private static int processedCounter = 0;

        public static Label StatusLabel = null;
        public static void SetStatus(String Message)
        {

            if (StatusLabel.InvokeRequired)
            {
                // Use BeginInvoke to asynchronously update the label on the UI thread
                StatusLabel.BeginInvoke((MethodInvoker)delegate
                {
                    // This code will run on the main UI thread
                    StatusLabel.Text = Message;
                });
            }
            else
            {
                // If we're already on the UI thread, update the label directly
                StatusLabel.Text = Message;
            }

        }
        public static void IncrementCounter()
        {
            lock (lockObj)
            {
                processedCounter++;
                // Update the label on the form's UI thread
                StatusLabel.BeginInvoke((MethodInvoker)delegate
                {
                    StatusLabel.Text = $"Processed: {processedCounter} / 1000";
                });
            }
        }
    }
}
