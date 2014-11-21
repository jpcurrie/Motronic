using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace MotronicSuite
{
    public partial class frmSplash : DevExpress.XtraEditors.XtraForm
    {
        public frmSplash()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmSplash_Shown(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void frmSplash_Load(object sender, EventArgs e)
        {
            lblVersion.Text = Application.ProductVersion.ToString();
        }
    }
}