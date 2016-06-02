using NxpPAApiLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bulb;
using TopazCommonDef;
using MeasurementsToolsClassLib;


namespace PASimpleApp
{
    public partial class Form1 : Form
    {
        bool m_close = false;
        bool m_initialize = true;
        string[] m_sensorsName;
        float m_startFreq = 2450;
        float m_stopFreq = 2500;
        float m_stepFreq = 0.5f;
        Thread m_powerMeterThread;
        bool m_powerMeterThreadRunning = false;
        CouplerFileReader m_coupler;
        NRP_Z211PowerMeter[] m_powerMeter = { null, null, null, null };
        double m_r_powerValue = 0;
        double m_f_powerValue = 0;


        NxpPAApi m_pa;
        Thread m_thread = null;

        Thread m_threadfreqScan = null;

        EventWaitHandle m_freqScanStartEvent = new ManualResetEvent(false);

        Thread m_errorThread = null;
        bool m_running = true;
        bool m_adc_running = true;
        bool m_enableOperationalCombo = false;
        bool m_rfEnable = false;
        AutoResetEvent m_sensorEvent = new AutoResetEvent(false);

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            comboBox1.SelectedIndex = 0;
            guiEnable(false);
            ledBulb1.On = false;
            m_pa = new NxpPAApi();
            try
            {
                m_pa.connect(string.Empty, 115200, true);
                m_pa.CheckI2CDeviceExistance();
                //readInfo();
            }
            catch (Exception err)
            {
                MessageBox.Show("Com port: " + m_pa.Port + " " + err.Message);
            }


            try
            {
                FillRodeSwartzPowerMeterList();
            }
            catch (Exception err)
            {
                MessageBox.Show("Initialize power meter list error:" + err.Message);
            }

            try
            {
                m_coupler = new CouplerFileReader(1);
            }
            catch (Exception er)
            {
                MessageBox.Show("Initialize couple files error: " + er.Message);
            }
            try
            {
                Reset(false);
                SetOperational();
                m_initialize = false;
                m_enableOperationalCombo = true;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
                m_close = true;
                return;
            }


            try
            {

                PowerMeterSelect[] pmSelect = new PowerMeterSelect[2];
                for (int i = 0; i < 2; i++)
                    pmSelect[i].ampId = -1;
                LoadAssigments(ref pmSelect);
                AllocatePowerMeterResources(pmSelect);

                if (pmSelect[0].ampId != -1 && pmSelect[0].sensorName != string.Empty)
                {
                    m_powerMeterThread = new Thread(PowerMeterThreadOnly);
                    m_powerMeterThread.Start();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }



        }

        void writeMaxLimits()
        {
            if (m_pa != null)
            {
                PALimits p = new PALimits();
                p.MaxReflectedPower = 127;
                p.HardwareMaxReflectedPower = 127;
                p.VSWR = 127;
                p.PACurrent = 127;
                p.PATemperature = 75;
                m_pa.WriteLimit(p);
            }
        }

        void FrequencyScan()
        {

            try
            {
                m_startFreq = float.Parse(textBox1.Text);
                m_stopFreq = float.Parse(textBox3.Text);
                m_stepFreq = float.Parse(textBox10.Text);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

            while (m_running)
            {

                m_freqScanStartEvent.WaitOne();
                if (m_running == false)
                    return;

                if (m_thread != null)
                {
                    m_adc_running = false;
                    m_thread.Join();
                    m_adc_running = true;
                    m_thread = null;
                }

                for (float f = m_startFreq; f <= m_stopFreq; f += m_stepFreq)
                {

                    try
                    {
                        m_pa.SetFrequency(f);
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.Message);
                        break;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (m_pa != null)
            {
                try
                {
                    float freq = float.Parse(textBox1.Text);
                    m_pa.SetFrequency(freq);
                    m_pa.GetFrequency();
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
        }

        void guiEnable(bool enable)
        {
            textBox1.Enabled = enable;
            textBox2.Enabled = enable;
            textBox3.Enabled = enable;
            textBox4.Enabled = enable;
            textBox5.Enabled = enable;
            button1.Enabled = enable;
            button2.Enabled = enable;
            button6.Enabled = enable;
            label2.Enabled = enable;
            label3.Enabled = enable;
            label5.Enabled = enable;
            label6.Enabled = enable;
            label7.Enabled = enable;
            label25.Enabled = enable;
            label26.Enabled = enable;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (m_pa != null)
            {
                try
                {
                    byte mag = byte.Parse(textBox2.Text);
                    m_pa.SetOutputPower(mag);
                    checkBox1.Enabled = true;
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            Reset(true);
        }

        void Reset(bool prompt)
        {
            if (prompt == true)
            {
                DialogResult d = MessageBox.Show("Do you want to reset the power module?", "PA Simple app", MessageBoxButtons.YesNo);
                if (d == System.Windows.Forms.DialogResult.No)
                    return;
            }
            guiEnable(false);
            try
            {

                if (m_thread != null)
                {
                    m_adc_running = false;
                    m_thread.Join();
                    m_adc_running = true;
                    m_thread = null;
                }

                m_pa.Reset();
                m_pa.AssignID();
                showAlerts();
                readInfo();
                showAlerts();
                bool txEnable = m_pa.ReadTXEnable();
                showAlerts();
                PALimits l = m_pa.ReadLimit();
                label17.Text = l.PATemperature.ToString();
                label16.Text = l.PACurrent.ToString();
                label15.Text = l.VSWR.ToString();
                label21.Text = l.MaxReflectedPower.ToString();
                label23.Text = l.HardwareMaxReflectedPower.ToString();

                showAlerts();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
                return;
            }

            try
            {
                showAlerts();

                NxpPAApi.READ_MODE_VALUES pamode = m_pa.ReadMode();
                ShowMode(pamode);
                if (pamode == NxpPAApi.READ_MODE_VALUES.Standby_Mode)
                    comboBox1.SelectedIndex = 0;
                else if (pamode == NxpPAApi.READ_MODE_VALUES.Operating_Mode_Post_PulseTest)
                    comboBox1.SelectedIndex = 1;
                else
                    comboBox1.SelectedIndex = 0;

                showAlerts();

                AmplifierParams par = m_pa.ReadParams();
                textBox1.Text = "2400";// par.Frequency.ToString();
                textBox2.Text = par.OutputPower.ToString();
                textBox8.Text = par.Phase.ToString();

                showAlerts();

                if (m_thread == null)
                {
                    m_thread = new Thread(ReadSensors);
                    m_thread.Start();
                }
                if (m_threadfreqScan == null)
                {
                    m_threadfreqScan = new Thread(FrequencyScan);
                    m_threadfreqScan.Start();
                }


            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
        void ShowMode(NxpPAApi.READ_MODE_VALUES pamode)
        {
            switch (pamode)
            {
                case NxpPAApi.READ_MODE_VALUES.Operating_Mode_Post_PulseTest:
                    label14.Text = "Operating_Mode_Post_PulseTest";
                    break;
                case NxpPAApi.READ_MODE_VALUES.Operating_Mode_with_active_ShutdownState:
                    label14.Text = "Operating_Mode_with_active_ShutdownState";
                    break;
                case NxpPAApi.READ_MODE_VALUES.Operating_Mode_Pre_PulseTest:
                    label14.Text = "Operating_Mode_Pre_PulseTest";
                    break;
                case NxpPAApi.READ_MODE_VALUES.Standby_Mode:
                    label14.Text = "Standby_Mode";
                    break;
            }

        }

        void readInfo()
        {
            try
            {
                ReadInfoStruct r = m_pa.ReadInfo();

            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }
        void ReadSensors()
        {

            int i = 0;
            int time = 0;
            while (m_adc_running == true)
            {
                try
                {
                    time = int.Parse(textBox9.Text);
                }
                catch (Exception err)
                {
                    time = 0;
                }
                if (time > 0)
                    m_sensorEvent.WaitOne(200);
                if (checkBox2.Checked)
                {
                    try
                    {
                        //ADCReadings adc = m_pa.ReadADC();
                        ADCForRedReadings adc = m_pa.ReadADC_ForRef();
                        //showAlerts();
                        textBox7.AppendText(adc.ForwardPower + Environment.NewLine);
                        textBox6.AppendText(adc.ReflectedPower + Environment.NewLine);
                    }
                    catch (Exception err)
                    {
                        try
                        {
                            shutdown();
                        }
                        catch (Exception err1)
                        {
                            MessageBox.Show(err1.Message);
                        }
                        MessageBox.Show(err.Message);
                        return;
                    }
                }
                if (checkBox3.Checked)
                {
                    try
                    {
                        SensorReadings r = m_pa.ReadSensor();
                        //showAlerts();
                        textBox4.AppendText(r.ForwardPower + Environment.NewLine);
                        textBox5.AppendText(r.ReflectedPower + Environment.NewLine);
                        label4.Text = r.PATemperature.ToString("0.000");
                        label9.Text = r.PACurrent.ToString("0.000");
                        label11.Text = r.PACurrent.ToString("0.000");


                    }
                    catch (Exception err)
                    {
                        try
                        {
                            shutdown();
                        }
                        catch (Exception err1)
                        {
                            MessageBox.Show(err1.Message);
                        }
                        MessageBox.Show(err.Message);
                        return;
                    }
                }
                i++;
                if (i == 1000)
                {
                    textBox7.Clear();
                    textBox6.Clear();
                    textBox4.Clear();
                    textBox5.Clear();
                    i = 0;
                }

            }
        }

        void SetOperational()
        {
            try
            {
                m_enableOperationalCombo = false;
                comboBox1.SelectedIndex = 1;
                NxpPAApi.PA_OPERATION_MODE mode = (NxpPAApi.PA_OPERATION_MODE)comboBox1.SelectedIndex;
                m_pa.SetMode((NxpPAApi.PA_OPERATION_MODE)comboBox1.SelectedIndex);
                NxpPAApi.READ_MODE_VALUES pamode;
                pamode = m_pa.ReadMode();
                ShowMode(pamode);
                m_enableOperationalCombo = true;
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_enableOperationalCombo == false)
                return;

            DialogResult d = MessageBox.Show("Do you want to change mode to power module?", "PA Simple app", MessageBoxButtons.YesNo);
            if (d == System.Windows.Forms.DialogResult.No)
                return;
            try
            {
                if (comboBox1.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select mode");
                    return;
                }
                NxpPAApi.PA_OPERATION_MODE mode = (NxpPAApi.PA_OPERATION_MODE)comboBox1.SelectedIndex;
                m_pa.SetMode((NxpPAApi.PA_OPERATION_MODE)comboBox1.SelectedIndex);
                NxpPAApi.READ_MODE_VALUES pamode;
                pamode = m_pa.ReadMode();
                ShowMode(pamode);

                if (m_errorThread == null && mode == NxpPAApi.PA_OPERATION_MODE.OPERATING)
                {

                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        void shutdown()
        {
            try
            {
                m_pa.WriteTXEnable(false);
                m_pa.SetMode(NxpPAApi.PA_OPERATION_MODE.STANDBY);
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {

                m_powerMeterThreadRunning = false;
                if (m_powerMeterThread != null)
                {
                    m_powerMeterThread.Join();
                }

                if (m_pa != null)
                {
                    m_pa.WriteTXEnable(false);
                    m_pa.SetMode(NxpPAApi.PA_OPERATION_MODE.STANDBY);
                }
            }
            catch
            {

            }

            m_running = false;
            m_adc_running = false;
            m_sensorEvent.Set();
            m_freqScanStartEvent.Set();
            Thread.Sleep(100);

            if (m_thread != null)
                m_thread.Join();
            if (m_errorThread != null)
                m_errorThread.Join();

            checkBox1.Checked = false;
            if (m_threadfreqScan != null)
                m_threadfreqScan.Join();
        }

        void showAlerts()
        {

            byte[] asources = m_pa.ReadAlertSources();

            {
                string alerts;
                string errCode;
                alerts = m_pa.ReadAlerts(out errCode);
                if (errCode != "No Error")
                {
                    MessageBox.Show(errCode);
                }
                if (alerts == "No Error")
                {
                    return;
                }

                AlertsForm f = new AlertsForm(alerts);
                f.ShowDialog();
            }
        }
        private void viewAlertsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                showAlerts();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                m_pa.SetMode(NxpPAApi.PA_OPERATION_MODE.OPERATING);
                NxpPAApi.READ_MODE_VALUES pamode;
                pamode = m_pa.ReadMode();
                ShowMode(pamode);

                //showAlerts();
                m_pa.PulseTest();
                pamode = m_pa.ReadMode();
                ShowMode(pamode);
                //showAlerts();

                guiEnable(true);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

        }

        private void moduleInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {

            try
            {
                ReadInfoStruct r = m_pa.ReadInfo();
                showAlerts();
                ModuleInfoForm f = new ModuleInfoForm(r);
                f.ShowDialog();

            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                int x = short.Parse(textBox8.Text);
                if (x > 360)
                {
                    MessageBox.Show("Phase cannot be greater then 360");
                    return;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                m_rfEnable = !m_rfEnable;
                m_pa.WriteTXEnable(m_rfEnable);

                bool txEnable = m_pa.ReadTXEnable();
                Console.WriteLine(txEnable);

                NxpPAApi.READ_MODE_VALUES pamode = m_pa.ReadMode();
                ShowMode(pamode);

                ledBulb1.On = m_rfEnable;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                try
                {
                    m_startFreq = float.Parse(textBox1.Text);
                    m_stopFreq = float.Parse(textBox3.Text);
                    m_stepFreq = float.Parse(textBox10.Text);
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                    return;
                }
                m_freqScanStartEvent.Set();
            }
            else
            {
                m_freqScanStartEvent.Reset();
                Thread.Sleep(500);
                if (m_thread == null)
                {
                    m_thread = new Thread(ReadSensors);
                    m_thread.Start();
                }

            }
        }

        void PowerMeterThreadOnly()
        {
            m_powerMeterThreadRunning = true;

            double f_powerValue = 0;
            double r_powerValue = 0;
            while (m_powerMeterThreadRunning == true)
            {
                try
                {
                    if (m_powerMeter[0] != null)
                        f_powerValue = m_powerMeter[0].Read(10);
                    if (m_powerMeter[1] != null)
                        r_powerValue = m_powerMeter[1].Read(10);
                    float freq = 2400;
                    try
                    {
                        freq = float.Parse(textBox1.Text);
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err.Message);
                    }
                    //double cp = m_couplerReader[i].getPower(m_frequency);
                    //powerValue = powerValue - cp;
                    Dictionary<double, double> d = m_coupler.m_capDataPower[0];
                    m_f_powerValue = f_powerValue - d[freq];
                    m_r_powerValue = r_powerValue - d[freq];
                    label31.Text = m_f_powerValue.ToString("0.000");
                    label30.Text = m_r_powerValue.ToString("0.000");
                    Thread.Sleep(1);
                }
                catch (Exception err)
                {
                    Thread.Sleep(100);
                }
            }
        }

        void LoadAssigments(ref PowerMeterSelect[] pmSelect)
        {
            pmSelect[0].sensorName = Properties.Settings.Default.PowerMeterID_1;
            pmSelect[0].ampId = Properties.Settings.Default.PowerMeterID_1_ToAmp;

            pmSelect[1].sensorName = Properties.Settings.Default.PowerMeterID_2;
            pmSelect[1].ampId = Properties.Settings.Default.PowerMeterID_2_ToAmp;

        }
        bool AllocatePowerMeterResources(PowerMeterSelect[] pmSelect)
        {
            Label[] lbl = { label1 };

            lock (this)
            {
                try
                {
                    string str = string.Empty;
                    for (int i = 0; i < 2; i++)
                    {
                        if (m_powerMeter[i] != null)
                            m_powerMeter[i].Close();
                        m_powerMeter[i] = null;
                        if (pmSelect[i].ampId != -1)
                        {
                            str += "Channel " + (i + 1) + " assign to " + pmSelect[i].sensorName + Environment.NewLine;

                            m_powerMeter[i] = new NRP_Z211PowerMeter();
                            NRP_Z211PowerMeter m_nrp = (NRP_Z211PowerMeter)m_powerMeter[i];
                            m_nrp.Mode = NRP_Z211_Modes.NONBURSTED;
                            if (m_powerMeter[i].Initialize(pmSelect[i].sensorName) == true)
                            {
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
            return true;
        }

        void SaveAssigments(PowerMeterSelect[] pmSelect)
        {
            Properties.Settings.Default.PowerMeterID_1 = pmSelect[0].sensorName;
            Properties.Settings.Default.PowerMeterID_1_ToAmp = pmSelect[0].ampId;


            Properties.Settings.Default.PowerMeterID_2 = pmSelect[1].sensorName;
            Properties.Settings.Default.PowerMeterID_2_ToAmp = pmSelect[1].ampId;

            Properties.Settings.Default.Save();
        }


        private void assignPowerMeterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

                PowerMeterSelect[] pmSelect;

                PowerMeterForm m_powerForm = new PowerMeterForm();
                for (int i = 0; i < m_sensorsName.Length; i++)
                    m_powerForm.Add(m_sensorsName[i]);
                m_powerForm.ShowDialog();
                if (m_powerForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    pmSelect = m_powerForm.getSelection();
                    AllocatePowerMeterResources(pmSelect);
                    SaveAssigments(pmSelect);

                    m_powerMeterThread = new Thread(PowerMeterThreadOnly);
                    m_powerMeterThread.Start();

                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private int FillRodeSwartzPowerMeterList()
        {
            try
            {
                int c = NRP_Z211PowerMeter.GetSensorCount();
                if (c == 0)
                {
                    return c;
                }
                m_sensorsName = new string[c];

                string SensorType = string.Empty;
                string SensorName = string.Empty;
                string SensorSerial = string.Empty;
                for (int i = 1; i < (c + 1); i++)
                {
                    NRP_Z211PowerMeter.GetSensorInfo(i,
                                         out SensorType,
                                         out SensorName,
                                         out SensorSerial);

                    m_sensorsName[i - 1] = SensorName;
                }
                return c;
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }

        private void refreshPowerMeterListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FillRodeSwartzPowerMeterList();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (m_close == true)
            {
                Close();
            }
        }
    }
}
