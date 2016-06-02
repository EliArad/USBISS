using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TopazCommonDef;

namespace PASimpleApp
{

    public partial class PowerMeterForm : Form
    {
        string m_str = string.Empty;
        PowerMeterSelect[] pm = new PowerMeterSelect[4]; 
        public PowerMeterForm()
        {
            InitializeComponent();
            for (int i = 0; i < 4; i++)
            {
                pm[i].ampId = -1;
            }
        }
        public void Add(string SensorName)
        {
            comboBox1.Items.Add(SensorName);
            comboBox2.Items.Add(SensorName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int x = comboBox1.SelectedIndex;
            if (x == -1)
            {
                MessageBox.Show("Select sensor name");
                return;
            }
            int x1 = 0;
            pm[x1].ampId = x1;
            pm[x1].sensorName = comboBox1.Text;
            m_str += "Channel " + (x1 + 1) + " assign to " + pm[x1].sensorName + Environment.NewLine;
            label2.Text = m_str;

            x1 = 1;
            pm[x1].ampId = x1;
            pm[x1].sensorName = comboBox2.Text;
            m_str += "Channel " + (x1 + 1) + " assign to " + pm[x1].sensorName + Environment.NewLine;
            label2.Text += m_str;

            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }
        public PowerMeterSelect [] getSelection()
        {
            return pm;
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }
    }
}
