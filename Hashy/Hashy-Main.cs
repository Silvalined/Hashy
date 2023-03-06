using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using File = System.IO.File;

/*
NOTES TO HELP HIGHLIGHT HOW I HAVE BUILT THIS:

To get the hash of a file, I looked here:
https://stackoverflow.com/questions/10520048/calculate-md5-checksum-for-a-file

To work out how to store more then 1 value in a list I used this:
https://stackoverflow.com/questions/8477843/storing-2-columns-into-a-list

To work out how to change values from different threads I read this:
https://docs.microsoft.com/en-gb/dotnet/desktop/winforms/controls/how-to-make-thread-safe-calls-to-windows-forms-controls?view=netframeworkdesktop-4.8

REWORK REMOVED THIOS: To work out to adjust the delegate called "SafeCallDelegate" to add a second parameter I read this:
https://stackoverflow.com/questions/729430/c-sharp-how-to-invoke-with-more-than-one-parameter

How to read text files:
https://www.tutorialspoint.com/how-to-read-a-csv-file-and-store-the-values-into-an-array-in-chash

*/

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
    public partial class Main : Form
    {
        private string hashMode;

        public Main()
        {
            InitializeComponent();
            // Set the default hash mode to 0 (being MD5):
            hashModeComboBox.SelectedIndex = 0;
        }

        // Force form size and prevent any adjustment:
        private void Form1_Load(object sender, EventArgs e)
        {
            this.MinimumSize = new Size(600, 400);
            this.MaximumSize = new Size(600, 400);
        }

        private void scanButton_Click(object sender, EventArgs e)
        {
            // Grab the selected hash mode into a string:
            hashMode = hashModeComboBox.SelectedItem.ToString();

            // Create and start the scan thread:
            Thread scanThread = new Thread(InitialScan);
            scanThread.Start();
        }

        private void checkButton_Click(object sender, EventArgs e)
        {
            // Create and start the check thread:
            Thread checkThread = new Thread(Check);
            checkThread.Start();
        }

        private void Check()
        {
            // Disable UI:
            DisableUI();

            // Create a list of files from the existing report via a method:
            List<FileDetails> report = GetFileContents(reportTextBox.Text);

            AppendLine(consoleRichTextBox, "------------------------------------------------------------");
            AppendLine(consoleRichTextBox, "Checking files against existing report:");
            AppendLine(consoleRichTextBox, reportTextBox.Text);

            if (UIDCheck(reportTextBox.Text)){
                AppendLine(consoleRichTextBox, "Report file passed UID check", Color.Green);

                // Set totalProgressBar back to 0 (in case previously run):
                SetProgressBar(totalProgressBar, 0);

                SetPercentageLabel(totalPercentageLabel, "0%");

                // Set totalProgressBar max value to the total of the files we are going to loop through:
                SetProgressBarMax(totalProgressBar, report.Count);

                int i = 1;

                bool corruptFile = false;

                string reportHashMode = HashMode(reportTextBox.Text);

                AppendLine(consoleRichTextBox, $"Report hash mode is {reportHashMode}");

                foreach (FileDetails file in report)
                {
                    string hash = "";

                    AppendLine(consoleRichTextBox, $"Checking {file.FilePath}");

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
                        AppendLine(consoleRichTextBox, "Good", Color.Green);
                    }
                    else if (file.Hash != hash & DateTimeSecondsEquals(file.LastModified, lastModified))
                    {
                        corruptFile = true;
                        AppendLine(consoleRichTextBox, "!!!BAD!!!", Color.Red);
                    }
                    else if (file.Hash != hash & !(DateTimeSecondsEquals(file.LastModified, lastModified)))
                    {
                        AppendLine(consoleRichTextBox, "!!!FILE HAS BEEN USER MODIFIED!!!", Color.Orange);
                    }
                    else
                    {
                        AppendLine(consoleRichTextBox, "!!!ERROR!!!", Color.Red);
                    }
                    SetProgressBar(totalProgressBar, i);
                    string d = 100 / report.Count * i + "%";
                    SetPercentageLabel(totalPercentageLabel, d);
                    if (i == report.Count)
                    {
                        SetPercentageLabel(totalPercentageLabel, "100%");

                        if (corruptFile)
                        {
                            AppendLine(consoleRichTextBox, "THERE WAS CORRUPTION DETECTED", Color.Red);
                        }
                        else
                        {
                            AppendLine(consoleRichTextBox, "---No corruption detected---", Color.Green);
                            AppendLine(consoleRichTextBox, "---------------------------------Check Finished---------------------------------");
                        }
                    }
                    i = i + 1;
                }
            } else
            {
                consoleRichTextBox.Invoke((MethodInvoker)delegate
                {
                    consoleRichTextBox.SelectionColor = Color.Red;
                    consoleRichTextBox.AppendText("!!!ERROR!!! Report file failed UID check");
                    consoleRichTextBox.AppendText(Environment.NewLine);
                });
            }
            scanButton.Invoke((MethodInvoker)delegate
            {
                scanButton.Enabled = true;
            });

            // Enable UI:
            EnableUI();
        }

        private void InitialScan()
        {
            // Disable UI:
            DisableUI();

            AppendLine(consoleRichTextBox, "------------------------------------------------------------");

            if (File.Exists(outputTextBox.Text))
            {
                DialogResult dialogResult = MessageBox.Show("Output file already exists, do you want to overwrite?", "ALERT", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.No)
                {
                    AppendLine(consoleRichTextBox, "!!!SCAN ABORTED!!!", Color.Red);

                    // Enable UI:
                    EnableUI();

                    return;
                }
            }

            // Variable to hold when we started the scan:
            DateTime started = DateTime.Now;

            AppendLine(consoleRichTextBox, $"Scan started at {started}");
            AppendLine(consoleRichTextBox, "Report will be saved here:");
            AppendLine(consoleRichTextBox, outputTextBox.Text);

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

                AppendLine(consoleRichTextBox, $"Checking {file}");

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

            DateTime finished = DateTime.Now;

            AppendLine(consoleRichTextBox, $"Scan finished at {finished}");

            var scanDuration = finished - started;
            // TODO: Rework logic so that if it takes longer then X time its minutes (instead of seconds), more then Y its hours?
            // Update the console with how long the scan took. Formatting to remove decimal places.

            AppendLine(consoleRichTextBox, $"Scan took {scanDuration.TotalSeconds.ToString("N0")} seconds");

            hashMode = "";

            outputFile.Close();

            // Enable UI:
            EnableUI();
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

        private void DisableUI()
        {
            // Disable all buttons:
            DisableButton(checkButton);
            DisableButton(scanButton);
            DisableButton(sourceButton);
            DisableButton(outputButton);

            // Disable all textbox's:
            DisableTextBox(dirTextBox);
            DisableTextBox(reportTextBox);
            DisableTextBox(outputTextBox);

            // Disable all combobox's:
            DisableComboBox(hashModeComboBox);
        }

        private void EnableUI()
        {
            // Enable all buttons:
            EnableButton(checkButton);
            EnableButton(scanButton);
            EnableButton(sourceButton);
            EnableButton(outputButton);

            // Enable all textbox's:
            EnableTextBox(dirTextBox);
            EnableTextBox(reportTextBox);
            EnableTextBox(outputTextBox);

            // Enable all combobox's:
            EnableComboBox(hashModeComboBox);
        }

        private void DisableButton(System.Windows.Forms.Button btn)
        {
            SetButtonStatus(btn, false);
        }

        private void EnableButton(System.Windows.Forms.Button btn)
        {
            SetButtonStatus(btn, true);
        }

        private void SetButtonStatus(System.Windows.Forms.Button btn, bool status)
        {
            if (btn.InvokeRequired)
            {
                btn.Invoke((MethodInvoker)delegate
                {
                    btn.Enabled = status;
                });
            }
            else
            {
                btn.Enabled = status;
            }
        }

        private void DisableTextBox(System.Windows.Forms.TextBox tb)
        {
            SetTextBoxStatus(tb, false);
        }

        private void EnableTextBox(System.Windows.Forms.TextBox tb)
        {
            SetTextBoxStatus(tb, true);
        }

        private void SetTextBoxStatus(System.Windows.Forms.TextBox tb, bool status)
        {
            if (tb.InvokeRequired)
            {
                tb.Invoke((MethodInvoker)delegate
                {
                    tb.Enabled = status;
                });
            }
            else
            {
                tb.Enabled = status;
            }
        }

        private void DisableComboBox(System.Windows.Forms.ComboBox cb)
        {
            SetComboBoxStatus(cb, false);
        }

        private void EnableComboBox(System.Windows.Forms.ComboBox cb)
        {
            SetComboBoxStatus(cb, true);
        }

        private void SetComboBoxStatus(System.Windows.Forms.ComboBox cb, bool status)
        {
            if (cb.InvokeRequired)
            {
                cb.Invoke((MethodInvoker)delegate
                {
                    cb.Enabled = status;
                });
            }
            else
            {
                cb.Enabled = status;
            }
        }

        private void AppendLine(System.Windows.Forms.RichTextBox rtb, string text)
        {
            if (rtb.InvokeRequired)
            {
                rtb.Invoke((MethodInvoker)delegate
                {
                    rtb.AppendText(text);
                    rtb.AppendText(Environment.NewLine);
                });
            }
            else
            {
                rtb.AppendText(text);
                rtb.AppendText(Environment.NewLine);
            }
        }

        private void AppendLine(System.Windows.Forms.RichTextBox rtb, string text, Color color)
        {
            if (rtb.InvokeRequired)
            {
                rtb.Invoke((MethodInvoker)delegate
                {
                    rtb.SelectionColor = color;
                    rtb.AppendText(text);
                    rtb.AppendText(Environment.NewLine);
                });
            }
            else
            {
                rtb.SelectionColor = color;
                rtb.AppendText(text);
                rtb.AppendText(Environment.NewLine);
            }
        }

        private void SetProgressBar(System.Windows.Forms.ProgressBar pb, int value)
        {
            if (pb.InvokeRequired)
            {
                pb.Invoke((MethodInvoker)delegate
                {
                    pb.Value = value;
                });
            }
            else
            {
                pb.Value = value;
            }
        }

        private void SetProgressBarMax(System.Windows.Forms.ProgressBar pb, int value)
        {
            if (pb.InvokeRequired)
            {
                pb.Invoke((MethodInvoker)delegate
                {
                    pb.Maximum = value;
                });
            }
            else
            {
                pb.Maximum = value;
            }
        }

        private void SetPercentageLabel(System.Windows.Forms.Label label, string value)
        {
            if (label.InvokeRequired)
            {
                label.Invoke((MethodInvoker)delegate
                {
                    label.Text = value;
                });
            }
            else
            {
                label.Text = value;
            }
        }

        public static bool DateTimeSecondsEquals(DateTime dt1, DateTime dt2)
        {
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day &&
                dt1.Hour == dt2.Hour && dt1.Minute == dt2.Minute && dt1.Second == dt2.Second;
        }

        // TEST debug button not meant for production version:
        private void debugButton_Click(object sender, EventArgs e)
        {
            var about = new About();
            about.Show();
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

        private void consoleRichTextBox_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            consoleRichTextBox.SelectionStart = consoleRichTextBox.Text.Length;

            // scroll it automatically
            consoleRichTextBox.ScrollToCaret();
        }
    }
}