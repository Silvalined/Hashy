using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

/*
NOTES TO HELP HIGHLIGHT HOW I HAVE BUILT THIS:

To get the hash of a file, I looked here:
https://stackoverflow.com/questions/10520048/calculate-md5-checksum-for-a-file

To work out how to store more then 1 value in a list I used this:
https://stackoverflow.com/questions/8477843/storing-2-columns-into-a-list

To work out how to change values from different threads I read this:
https://docs.microsoft.com/en-gb/dotnet/desktop/winforms/controls/how-to-make-thread-safe-calls-to-windows-forms-controls?view=netframeworkdesktop-4.8

To work out to adjust the delegate called "SafeCallDelegate" to add a second parameter I read this:
https://stackoverflow.com/questions/729430/c-sharp-how-to-invoke-with-more-than-one-parameter

How to read text files:
https://www.tutorialspoint.com/how-to-read-a-csv-file-and-store-the-values-into-an-array-in-chash

*/

// TODO: Disable all buttons while running!
// TODO: Percent scanning status for large files back to status console
// TODO: Check existing files against previously created Output.txt. Log output to Report.txt
// TODO: Create startup form to select between creating a new output or scanning a previous one?
// TODO: Comment for what everything does
// TODO: Add an 'About' section
// TODO: Scaling form size control?
// TODO: Sort button sizes!
// TODO: Identifier in report and check.
// TODO: Restructure and tidy.

namespace Hashy
{
    public partial class Form1 : Form
    {
        private delegate void ConsoleTBSafeCallDelegate(System.Windows.Forms.TextBox tb, string text);
        private delegate void ConsoleRTBSafeCallDelegate(System.Windows.Forms.RichTextBox rtb, string text);
        private delegate void ConsoleRTBColorSafeCallDelegate(System.Windows.Forms.RichTextBox rtb, string text, Color color);
        private delegate void TotalProgressBarSafeCallDelegate(System.Windows.Forms.ProgressBar pb, int value);
        //private delegate void ScanButtonSafeCallDelegate(bool status);
        private delegate void CheckButtonSafeCallDelegate(bool status);
        private delegate void SourceBrowseButtonSafeCallDelegate(bool status);
        private delegate void OutputBrowseButtonSafeCallDelegate(bool status);
        private delegate void TotalPercentageLabelSafeCallDelegate(System.Windows.Forms.Label label, string value);

        private string hashMode;

        public Form1()
        {
            InitializeComponent();
            hashModeComboBox.SelectedIndex = 0;
        }

        private void scanButton_Click(object sender, EventArgs e)
        {
            hashMode = hashModeComboBox.SelectedItem.ToString();
            Thread scanThread = new Thread(InitialScan);
            scanThread.Start();
        }

        private void checkButton_Click(object sender, EventArgs e)
        {
            Thread checkThread = new Thread(Check);
            checkThread.Start();
        }

        private void Check()
        {
            // Disable the scan button using a safe call via a delegate to the main thread:
            //Thread disableScanButtonThread = new Thread(new ThreadStart(disableScanButton));
            //disableScanButtonThread.Start();
            scanButton.Invoke((MethodInvoker)delegate
            {
                scanButton.Enabled = false;
            });

            List<FileDetails> report = GetFileContents(reportTextBox.Text);

            SetText(consoleRichTextBox, "------------------------------------------------------------");

            SetText(consoleRichTextBox, "Checking files against existing report:");
            SetText(consoleRichTextBox, reportTextBox.Text);

            if (UIDCheck(reportTextBox.Text)){
                SetText(consoleRichTextBox, "Report file passed UID check", Color.Green);

                // Set totalProgressBar back to 0 (in case previously run):
                SetProgressBar(totalProgressBar, 0);

                SetPercentageLabel(totalPercentageLabel, "0%");

                // Set totalProgressBar max value to the total of the files we are going to loop through:
                SetProgressBarMax(totalProgressBar, report.Count);

                int i = 1;

                bool corruptFile = false;

                string reportHashMode = HashMode(reportTextBox.Text);

                SetText(consoleRichTextBox, $"Report hash mode is {reportHashMode}");

                foreach (FileDetails file in report)
                {
                    string hash = "";
                    SetText(consoleRichTextBox, $"Checking {file.FilePath}");
                    if (reportHashMode == "MD5")
                    {
                        hash = CalculateMD5(file.FilePath);
                    }
                    else if (reportHashMode == "SHA256")
                    {
                        hash = CalculateSHA256(file.FilePath);
                    }
                    
                    DateTime lastModified = GetFileLastModified(file.FilePath);

                    if (file.Hash == hash & DateTimeSecondsEquals(file.LastModified, lastModified))
                    {
                        SetText(consoleRichTextBox, "Good", Color.Green);
                    }
                    else if (file.Hash != hash & DateTimeSecondsEquals(file.LastModified, lastModified))
                    {
                        SetText(consoleRichTextBox, "!!!BAD!!!", Color.Red);
                        corruptFile = true;
                    }
                    else if (file.Hash != hash & !(DateTimeSecondsEquals(file.LastModified, lastModified)))
                    {
                        SetText(consoleRichTextBox, "!!!FILE HAS BEEN USER MODIFIED!!!", Color.Orange);
                    }
                    else
                    {
                        SetText(consoleRichTextBox, "!!!ERROR!!!", Color.Red);
                    }
                    SetProgressBar(totalProgressBar, i);
                    string d = 100 / report.Count * i + "%";
                    SetPercentageLabel(totalPercentageLabel, d);
                    if (i == report.Count)
                    {
                        SetPercentageLabel(totalPercentageLabel, "100%");

                        if (corruptFile)
                        {
                            SetText(consoleRichTextBox, "THERE WAS CORRUPTION DETECTED", Color.Red);
                        }
                        else
                        {
                            SetText(consoleRichTextBox, "---No corruption detected---", Color.Green);
                            SetText(consoleRichTextBox, "---------------------------------Check Finished---------------------------------");
                        }
                    }
                    i = i + 1;
                }
            } else
            {
                SetText(consoleRichTextBox, "!!!ERROR!!! Report file failed UID check", Color.Red);
            }
            scanButton.Invoke((MethodInvoker)delegate
            {
                scanButton.Enabled = true;
            });
        }

        private void InitialScan()
        {
            SetText(consoleRichTextBox, "------------------------------------------------------------");
            if (File.Exists(outputTextBox.Text))
            {
                DialogResult dialogResult = MessageBox.Show("Output file already exists, do you want to overwrite?", "ALERT", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.No)
                {
                    SetText(consoleRichTextBox, "!!!SCAN ABORTED!!!", Color.Red);
                    return;
                }
            }

            // Variable to hold when we started the scan:
            DateTime started = DateTime.Now;
            SetText(consoleRichTextBox, $"Scan started at {started}");
            SetText(consoleRichTextBox, "Report will be saved here:");
            SetText(consoleRichTextBox, outputTextBox.Text);

            // Create an empty list that will hold the fileDetails in the for loop:
            //List<FileDetails> fileDetails = new List<FileDetails>();

            // Disable the scan button using a safe call via a delegate to the main thread:
            //Thread disableScanButtonThread = new Thread(new ThreadStart(disableScanButton));
            //disableScanButtonThread.Start();

            // Create a list called files that will hold the output from the recursive file processor:
            List<string> files = RecursiveFileProcessor.RecursiveFileSearch(dirTextBox.Text);

            // Set totalProgressBar back to 0 (in case previously run):
            SetProgressBar(totalProgressBar, 0);

            SetPercentageLabel(totalPercentageLabel, "0%");

            // Set totalProgressBar max value to the total of the files we are going to loop through:
            SetProgressBarMax(totalProgressBar, files.Count);

            StreamWriter outputFile = OpenFile(outputTextBox.Text);

            WriteLine(outputFile, @"f7c07c6ab1c9faaccee57b77b826ff51\r\ncf3b9982a86afb4dd9b50556f1817aa9\r\ne1d4aa4e629ee2d4e553c538bfcad177\r\ne724f3d18bf2985fea81a912c642ffba\r\nc37dd1fc6a3ddd4fdd77fe3ceedf8974\r\n96d0ee391f51eb3e2a00064ed3a25eac\r\n11ae1e159900bfd8fe6731825ffe43e9\r\nbaa9d174ce36a1ac00a5fdbde0b55898\r\n0957faec8615cc14a0c3b229c92d38ab\r\nfa388c8c3a97671b1557a4ae53bc468b");

            string outputHeader = $"{started.ToString()},{dirTextBox.Text},{hashMode}";
            WriteLine(outputFile, outputHeader);

            int i = 1;

            // Loop through all the files:
            foreach (var file in files)
            {
                string value;
                SetText(consoleRichTextBox, $"Checking {file}");

                //string hashMode = this.hashModeComboBox.GetItemText(this.hashModeComboBox.SelectedItem);
                if (hashMode == "MD5")
                {
                    // Fill value with the output from the MD5 file calculation:
                    value = CalculateMD5(file);
                }
                else if (hashMode == "SHA256")
                {
                    value = CalculateSHA256(file);
                }
                else
                {
                    value = "!!!ERROR!!!";
                }

                // Add the value output to the fileDetails list along with the current file:
                string line = $"{file},{value},{GetFileLastModified(file)}";
                WriteLine(outputFile, line);
                //fileDetails.Add(new FileDetails { FilePath = file, Hash = value, LastModified = GetFileLastModified(file) });
                SetProgressBar(totalProgressBar, i);
                string d = 100 / files.Count * i + "%";
                SetPercentageLabel(totalPercentageLabel, d);
                if (i == files.Count)
                {
                    SetPercentageLabel(totalPercentageLabel, "100%");
                }
                i = i + 1;
            }

            // Clear the fileDetails list:
            CloseFile(outputFile);

            // Clear the files list:
            files.Clear();

            // Enable the scan button using a safe call via a delegate to the main thread:
            //Thread enableScanButtonThread = new Thread(new ThreadStart(enableScanButton));
            //enableScanButtonThread.Start();
            scanButton.Invoke((MethodInvoker)delegate
            {
                scanButton.Enabled = true;
            });

            DateTime finished = DateTime.Now;
            SetText(consoleRichTextBox, $"Scan finished at {finished}");
            var scanDuration = finished - started;
            // TODO: Rework logic so that if it takes longer then X time its minutes (instead of seconds), more then Y its hours?
            // Update the console with how long the scan took. Formatting to remove decimal places.
            SetText(consoleRichTextBox, $"Scan took {scanDuration.TotalSeconds.ToString("N0")} seconds");
            hashMode = "";

            outputFile.Close();
        }

        private bool UIDCheck(string file)
        {
            if (File.Exists(file))
            {
                StreamReader reader = new StreamReader(File.OpenRead(file));
                List<FileDetails> outputList = new List<FileDetails>();
                string uid = reader.ReadLine();
                if (uid != @"f7c07c6ab1c9faaccee57b77b826ff51\r\ncf3b9982a86afb4dd9b50556f1817aa9\r\ne1d4aa4e629ee2d4e553c538bfcad177\r\ne724f3d18bf2985fea81a912c642ffba\r\nc37dd1fc6a3ddd4fdd77fe3ceedf8974\r\n96d0ee391f51eb3e2a00064ed3a25eac\r\n11ae1e159900bfd8fe6731825ffe43e9\r\nbaa9d174ce36a1ac00a5fdbde0b55898\r\n0957faec8615cc14a0c3b229c92d38ab\r\nfa388c8c3a97671b1557a4ae53bc468b")
                {
                    return false;
                }
                reader.Close();
                return true;
            }
            return false;
        }

        private string HashMode(string file)
        {
            if (File.Exists(file))
            {
                StreamReader reader = new StreamReader(File.OpenRead(file));
                List<FileDetails> outputList = new List<FileDetails>();
                string uid = reader.ReadLine();
                string header = reader.ReadLine();
                var values = header.Split(',');
                reader.Close();
                return values[values.Length-1];
            }
            throw new ArgumentException();
        }

        private List<FileDetails> GetFileContents(string file)
        {
            if (File.Exists(file))
            {
                StreamReader reader = new StreamReader(File.OpenRead(file));
                List<FileDetails> outputList = new List<FileDetails>();
                string uid = reader.ReadLine();
                string headerline = reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    outputList.Add(line);
                }
                reader.Close();
                return outputList;
            }
            return null;
        }

        //private void toggleScanButtonSafe(bool status)
        //{
        //    // If we are calling across threads (so invoke is required) then call the Control.Invoke method with a delegate from the main thread:
        //    if (scanButton.InvokeRequired)
        //    {
        //        var d = new ScanButtonSafeCallDelegate(toggleScanButtonSafe);
        //        scanButton.Invoke(d, new object[] { status });
        //    }
        //    // Else thread ID's are the same (we are in the same thread), then call the control directly:
        //    else
        //    {
        //        scanButton.Enabled = status;
        //    }
        //}

        //private void disableScanButton()
        //{
        //    toggleScanButtonSafe(false);
        //}
        //private void enableScanButton()
        //{
        //    toggleScanButtonSafe(true);
        //}

        //        private delegate void ScanButtonSafeCallDelegate(bool status);

        private void toggleButtonSafe(System.Windows.Forms.Button btn, Delegate del, bool status)
        {
            // If we are calling across threads (so invoke is required) then call the Control.Invoke method with a delegate from the main thread:
            if (btn.InvokeRequired)
            {
                //var d = new del(toggleButtonSafe);
                //dynamic d = del;
                dynamic d = del.GetType().GetProperty("Value").GetValue(del, null);
                btn.Invoke(d, new object[] { status });
            }
            // Else thread ID's are the same (we are in the same thread), then call the control directly:
            else
            {
                btn.Enabled = status;
            }
        }

        private void disableButton(System.Windows.Forms.Button btn, Delegate del)
        {
            toggleButtonSafe(btn, del, false);
        }

        private void enableButton(System.Windows.Forms.Button btn, Delegate del)
        {
            toggleButtonSafe(btn, del, true);
        }

        private StreamWriter OpenFile(string filePath)
        {
            // TODO: System.IO.IOException: 'The process cannot access the file 'E:\deleteme\output.csv' because it is being used by another process.'
            StreamWriter writer = new StreamWriter(filePath);
            return writer;
        }

        private void WriteLine(StreamWriter writer, string line)
        {
            writer.WriteLine(line);
        }

        private void CloseFile(StreamWriter writer)
        {
            writer.Close();
        }

        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        static string CalculateSHA256(string filename)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        static DateTime GetFileLastModified(string filename)
        {
            return File.GetLastWriteTime(filename);
        }

        private void WriteTextSafe(System.Windows.Forms.TextBox tb, string text)
        {
            // If we are calling across threads (so invoke is required) then call the Control.Invoke method with a delegate from the main thread:
            if (tb.InvokeRequired)
            {
                var d = new ConsoleTBSafeCallDelegate(WriteTextSafe);
                consoleRichTextBox.Invoke(d, new object[] { tb, text });
            }
            // Else thread ID's are the same (we are in the same thread), then call the control directly:
            else
            {
                tb.AppendText(text);
                tb.AppendText(Environment.NewLine);
            }
        }

        private void SetText(System.Windows.Forms.TextBox tb, string text)
        {
            WriteTextSafe(tb, text);
        }

        private void WriteTextSafe(System.Windows.Forms.RichTextBox rtb, string text)
        {
            // If we are calling across threads (so invoke is required) then call the Control.Invoke method with a delegate from the main thread:
            if (rtb.InvokeRequired)
            {
                var d = new ConsoleRTBSafeCallDelegate(WriteTextSafe);
                consoleRichTextBox.Invoke(d, new object[] { rtb, text });
            }
            // Else thread ID's are the same (we are in the same thread), then call the control directly:
            else
            {
                rtb.AppendText(text);
                rtb.AppendText(Environment.NewLine);
            }
        }

        private void SetText(System.Windows.Forms.RichTextBox rtb, string text)
        {
            WriteTextSafe(rtb, text);
        }

        private void WriteTextSafe(System.Windows.Forms.RichTextBox rtb, string text, Color color)
        {
            // If we are calling across threads (so invoke is required) then call the Control.Invoke method with a delegate from the main thread:
            if (rtb.InvokeRequired)
            {
                var d = new ConsoleRTBColorSafeCallDelegate(WriteTextSafe);
                consoleRichTextBox.Invoke(d, new object[] { rtb, text, color });
            }
            // Else thread ID's are the same (we are in the same thread), then call the control directly:
            else
            {
                rtb.SelectionColor = color;
                rtb.AppendText(text);
                rtb.AppendText(Environment.NewLine);
            }
        }

        private void SetText(System.Windows.Forms.RichTextBox rtb, string text, Color color)
        {
            WriteTextSafe(rtb, text, color);
        }

        private void SetProgressBarSafe(System.Windows.Forms.ProgressBar pb, int value)
        {
            if (pb.InvokeRequired)
            {
                var d = new TotalProgressBarSafeCallDelegate(SetProgressBarSafe);
                totalProgressBar.Invoke(d, new object[] {pb, value});
            }
            else
            {
                pb.Value = value;
            }
        }

        private void SetProgressBar(System.Windows.Forms.ProgressBar pb, int value)
        {
            SetProgressBarSafe(pb, value);
        }

        private void SetProgressBarMaxSafe(System.Windows.Forms.ProgressBar pb, int value)
        {
            if (pb.InvokeRequired)
            {
                var d = new TotalProgressBarSafeCallDelegate(SetProgressBarMax);
                totalProgressBar.Invoke(d, new object[] { pb, value });
            }
            else
            {
                pb.Maximum = value;
            }
        }

        private void SetProgressBarMax(System.Windows.Forms.ProgressBar pb, int value)
        {
            SetProgressBarMaxSafe(pb, value);
        }

        private void SetPercentageLabelSafe(System.Windows.Forms.Label label, string value)
        {
            if (label.InvokeRequired)
            {
                var d = new TotalPercentageLabelSafeCallDelegate(SetPercentageLabelSafe);
                totalPercentageLabel.Invoke(d, new object[] {label, value});
            }
            else
            {
                label.Text = value;
            }
        }

        private void SetPercentageLabel(System.Windows.Forms.Label label, string value)
        {
            SetPercentageLabelSafe(label, value);
        }

        public static bool DateTimeSecondsEquals(DateTime dt1, DateTime dt2)
        {
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day &&
                dt1.Hour == dt2.Hour && dt1.Minute == dt2.Minute && dt1.Second == dt2.Second;
        }

        private void debugButton_Click(object sender, EventArgs e)
        {
            //Thread disableScanButtonThread = new Thread(new ThreadStart(disableButton(scanButton, ScanButtonSafeCallDelegate)));
            //disableScanButtonThread.Start();
            scanButton.Invoke((MethodInvoker)delegate
            {
                scanButton.Enabled = false;
            });
            Console.ReadLine();
        }

        private void sourceButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog sourceFolderDialog = new FolderBrowserDialog();
            sourceFolderDialog.ShowDialog();
            dirTextBox.Text = sourceFolderDialog.SelectedPath;
        }

        private void outputButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog sourceFolderDialog = new FolderBrowserDialog();
            sourceFolderDialog.ShowDialog();
            outputTextBox.Text = sourceFolderDialog.SelectedPath + "\\Output.csv";
        }

        private void clearTerminalButton_Click(object sender, EventArgs e)
        {
            consoleRichTextBox.Text = "";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MinimumSize = new Size(600, 400);
            this.MaximumSize = new Size(600, 400);
        }

        private void consoleRichTextBox_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            consoleRichTextBox.SelectionStart = consoleRichTextBox.Text.Length;
            // scroll it automatically
            consoleRichTextBox.ScrollToCaret();
        }
    }
}