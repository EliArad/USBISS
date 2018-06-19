using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using USB_ISSApi;

namespace USBISS_IOApp
{
    public partial class Form1 : Form
    {
        USBISS_IO m_io = new USBISS_IO();
        public Form1()
        {
            InitializeComponent();

            try
            {
                m_io.connect("COM31");
                m_io.set_io_mode();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            m_io.set_io(1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m_io.set_io(0);
        }
    }
}
