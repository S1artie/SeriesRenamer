#region SVN Info
/***************************************************************
 * $Author: matthias.bilger $
 * $Revision: 20 $
 * $Date: 2009-05-22 11:23:37 +0000 (Fri, 22 May 2009) $
 * $LastChangedBy: matthias.bilger $
 * $LastChangedDate: 2009-05-22 11:23:37 +0000 (Fri, 22 May 2009) $
 * $URL: http://seriesrenamer.googlecode.com/svn/trunk/Renamer/Dialogs/ReplaceWindow.cs $
 * 
 * License: GPLv3
 * 
****************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Renamer.Dialogs
{
    public partial class ReplaceWindow : Form
    {
        Form1 MainWindow = null;
        public ReplaceWindow(Form1 parent)
        {
            MainWindow = parent;
            InitializeComponent();
            cbReplaceIn.SelectedIndex = 0;
            txtSearch.Focus();
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            MainWindow.Replace(txtSearch.Text, txtReplace.Text, cbReplaceIn.SelectedItem.ToString());
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
