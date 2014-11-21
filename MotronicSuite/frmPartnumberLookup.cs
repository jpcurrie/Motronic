using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.IO;

namespace MotronicSuite
{
    public partial class frmPartnumberLookup : DevExpress.XtraEditors.XtraForm
    {
        private bool m_open_File = false;

        private bool m_compare_File = false;

        public bool Compare_File
        {
            get { return m_compare_File; }
            set { m_compare_File = value; }
        }

        public bool Open_File
        {
            get { return m_open_File; }
            set { m_open_File = value; }
        }

        public frmPartnumberLookup()
        {
            InitializeComponent();
        }

        private void ConvertPartNumber()
        {
            PartNumberConverter pnc = new PartNumberConverter();
            ECUInformation ecuinfo = pnc.GetECUInfo(buttonEdit1.Text, "");
            lblCarModel.Text = "---";
            lblEngineType.Text = "---";
            lblPower.Text = "---";
            lblTorque.Text = "---";
            lblDescription.Text = "---";
            checkEdit1.Checked = false;
            checkEdit2.Checked = false;
            checkEdit4.Checked = false;

            if (ecuinfo.Valid)
            {
                lblCarModel.Text = ecuinfo.Carmodel.ToString();
                lblEngineType.Text = ecuinfo.Enginetype.ToString();
                lblDescription.Text = ecuinfo.SoftwareID;
                lblPower.Text = ecuinfo.Bhp.ToString() + " bhp";
                if (ecuinfo.Is2point3liter)
                {
                    checkEdit1.Checked = false;
                    checkEdit2.Checked = true;
                }
                else
                {
                    checkEdit1.Checked = true;
                    checkEdit2.Checked = false;
                }
                if (ecuinfo.Isturbo) checkEdit4.Checked = true;
                if (ecuinfo.Isfpt)
                {
                    checkEdit4.Checked = true;
                }
                lblTorque.Text = ecuinfo.Torque.ToString() + " Nm";

                if (comboBoxEdit1.EditValue == null) comboBoxEdit1.EditValue = "";
                try
                {

                    if (System.IO.File.Exists(Path.Combine(Application.StartupPath, "Binaries\\" + buttonEdit1.Text + ".BIN")))
                    {
                        simpleButton2.Enabled = true;
                        simpleButton3.Enabled = true;
                        simpleButton4.Enabled = true;
                    }
                    else if (System.IO.File.Exists(Path.Combine(Application.StartupPath, "Binaries\\" + buttonEdit1.Text + "_" + comboBoxEdit1.EditValue.ToString() + ".BIN")))
                    {
                        simpleButton2.Enabled = true;
                        simpleButton3.Enabled = true;
                        simpleButton4.Enabled = true;
                    }
                    else if (System.IO.File.Exists(Path.Combine(Application.StartupPath, "Binaries\\" + buttonEdit1.Text + "_" + comboBoxEdit1.EditValue.ToString() + "_1.BIN")))
                    {
                        simpleButton2.Enabled = true;
                        simpleButton3.Enabled = true;
                        simpleButton4.Enabled = true;
                    }
                    else if (System.IO.File.Exists(Path.Combine(Application.StartupPath, "Binaries\\" + buttonEdit1.Text + "_" + comboBoxEdit1.EditValue.ToString() + "_0.BIN")))
                    {
                        simpleButton2.Enabled = true;
                        simpleButton3.Enabled = true;
                        simpleButton4.Enabled = true;
                    }
                    else
                    {
                        simpleButton2.Enabled = false;
                        simpleButton3.Enabled = false;
                        simpleButton4.Enabled = false;
                    }
                }
                catch (Exception E)
                {
                    Console.WriteLine("Failed to check for library availability: " + E.Message);
                }
            }
            else
            {
                frmInfoBox info = new frmInfoBox("The entered partnumber was not recognized by MotronicSuite");
            }
        }

        private void buttonEdit1_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            frmPartNumberList pnl = new frmPartNumberList();
            pnl.ShowDialog();
            if (pnl.Selectedpartnumber != null)
            {
                if (pnl.Selectedpartnumber != string.Empty)
                {
                    buttonEdit1.Text = pnl.Selectedpartnumber;
                    SetSoftwareVersions();
                    comboBoxEdit1.EditValue = pnl.SelectedSoftwareID;
                }
            }
            if (buttonEdit1.Text != "")
            {
                ConvertPartNumber();
            }
            else
            {
                simpleButton2.Enabled = false;
                simpleButton3.Enabled = false;
                simpleButton4.Enabled = false;
            }

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonEdit1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetSoftwareVersions();
                ConvertPartNumber();
            }
        }

        private void SetSoftwareVersions()
        {
            PartNumberConverter pnc = new PartNumberConverter();
            ECUInformation ecuinfo = pnc.GetECUInfo(buttonEdit1.Text, "");
            comboBoxEdit1.Properties.Items.Clear();
            comboBoxEdit1.Enabled = false;
            foreach (string s in ecuinfo.Swversions)
            {
                if (s != null)
                {
                    comboBoxEdit1.Properties.Items.Add(s);
                }
            }
            if (ecuinfo.Swversions.Length > 0) comboBoxEdit1.SelectedIndex = 0;
            if (ecuinfo.Swversions.Length > 1) comboBoxEdit1.Enabled = true;

        }

        internal void LookUpPartnumber(string p)
        {
            buttonEdit1.Text = p;
            SetSoftwareVersions();
            ConvertPartNumber();
        }

        public string GetFileToOpen()
        {
            string retval = string.Empty;
            if (buttonEdit1.Text != string.Empty)
            {
                string path2search = Path.Combine(Application.StartupPath, "Binaries\\" + buttonEdit1.Text + ".BIN");
                if (comboBoxEdit1.EditValue.ToString() != "")
                {
                    path2search = Path.Combine(Application.StartupPath, "Binaries\\" + buttonEdit1.Text + "_" + comboBoxEdit1.EditValue.ToString() + ".BIN");
                }
                if (System.IO.File.Exists(path2search))
                {
                    retval = path2search;
                }
                if (comboBoxEdit1.EditValue.ToString() != "")
                {
                    path2search = Path.Combine(Application.StartupPath, "Binaries\\" + buttonEdit1.Text + "_" + comboBoxEdit1.EditValue.ToString() + "_0.BIN");
                }
                if (System.IO.File.Exists(path2search))
                {
                    retval = path2search;
                }
                if (comboBoxEdit1.EditValue.ToString() != "")
                {
                    path2search = Path.Combine(Application.StartupPath, "Binaries\\" + buttonEdit1.Text + "_" + comboBoxEdit1.EditValue.ToString() + "_1.BIN");
                }
                if (System.IO.File.Exists(path2search))
                {
                    retval = path2search;
                }
            }
            return retval;
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            m_open_File = true;
            this.Close();
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            m_compare_File = true;
            this.Close();
        }

        private bool m_createNewFile = false;

        public bool CreateNewFile
        {
            get { return m_createNewFile; }
            set { m_createNewFile = value; }
        }

        private string m_fileNameToSave = string.Empty;

        public string FileNameToSave
        {
            get { return m_fileNameToSave; }
            set { m_fileNameToSave = value; }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            // show a savefile dialog
            m_createNewFile = true;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Binary files|*.bin";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // copy ori to 
                m_fileNameToSave = sfd.FileName;
                m_createNewFile = true;
                this.Close();
            }
        }
    }
}