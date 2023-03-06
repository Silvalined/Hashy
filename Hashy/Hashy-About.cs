using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Hashy
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        // Force form size and prevent any adjustment:

        private void About_Load(object sender, EventArgs e)
        {
            this.MinimumSize = new Size(600, 400);
            this.MaximumSize = new Size(600, 400);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Silvalined/Hashy");
        }
    }
}
