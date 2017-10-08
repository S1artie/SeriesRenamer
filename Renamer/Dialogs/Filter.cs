#region SVN Info
/***************************************************************
 * $Author: matthias.bilger $
 * $Revision: 20 $
 * $Date: 2009-05-22 11:23:37 +0000 (Fri, 22 May 2009) $
 * $LastChangedBy: matthias.bilger $
 * $LastChangedDate: 2009-05-22 11:23:37 +0000 (Fri, 22 May 2009) $
 * $URL: http://seriesrenamer.googlecode.com/svn/trunk/Renamer/Dialogs/Filter.cs $
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
    /// <summary>
    /// File selection filter dialog
    /// </summary>
    public partial class Filter : Form
    {
        /// <summary>
        /// entered filter string
        /// </summary>
        public string result;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="str">Initial value to show in textbox</param>
        public Filter(string str)
        {
            InitializeComponent();
            txtFilter.Text = str;
        }

        /// <summary>
        /// Sets entered filter text and DialogResult.OK and closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult=DialogResult.OK;
            result=txtFilter.Text;
            Close();
        }

        /// <summary>
        /// Sets DialogResult.Cancel and closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
