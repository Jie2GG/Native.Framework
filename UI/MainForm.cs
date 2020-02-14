using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName);
            this.Controls.Add(new TextBox()
            {
                Text = "Demo",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0xff, 0x66, 0xcc, 0xff)
            });
        }
    }
}
