using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PASimpleApp
{
    public partial class AlertsForm : Form
    {
        public AlertsForm(string alerts)
        {
            InitializeComponent();
            label1.Text = alerts;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
