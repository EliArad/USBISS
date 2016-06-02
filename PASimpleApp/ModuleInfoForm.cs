using NxpPAApiLib;
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
    public partial class ModuleInfoForm : Form
    {
        public ModuleInfoForm(ReadInfoStruct r)
        {
            InitializeComponent();

            lblI2CAddress.Text = "I2C Address: " + r.I2CAddress.ToString();
            lblSWVersion.Text = "SW Version: " + r.SWVersion.ToString();
            lblHwVersion.Text = "HW Version: " + r.HWVersion.ToString();
            lblName.Text = r.Name.ToString();
            lblSerialNum.Text = "Serial:" + r.SerialNumVersion.ToString();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
