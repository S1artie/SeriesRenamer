﻿#region SVN Info
/***************************************************************
 * $Author: DANoWAR2k@googlemail.com $
 * $Revision: 113 $
 * $Date: 2013-03-28 15:40:08 +0000 (Thu, 28 Mar 2013) $
 * $LastChangedBy: DANoWAR2k@googlemail.com $
 * $LastChangedDate: 2013-03-28 15:40:08 +0000 (Thu, 28 Mar 2013) $
 * $URL: http://seriesrenamer.googlecode.com/svn/trunk/Renamer/Dialogs/SetEpisodes.cs $
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
    /// Dialog to set episode values ascending or descending for multiple files at once
    /// </summary>
    public partial class SetEpisodes : Form
    {
        /// <summary>
        /// number of files
        /// </summary>
        int Count = 0;

        /// <summary>
        /// first selected episode nr
        /// </summary>
        int firstEpNr = 1;


        /// <summary>
        /// start index
        /// </summary>
        public int From = 1;
        
        /// <summary>
        /// end index
        /// </summary>
        public int To = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="SelectionCount">Number of files</param>
        public SetEpisodes(int SelectionCount)
        {
            Count = SelectionCount;
            firstEpNr = 1;
            InitializeComponent();
        }

        public SetEpisodes(int SelectionCount, int newFirstEpNr)
        {
            Count = SelectionCount;
            firstEpNr = newFirstEpNr;
            InitializeComponent();
        }

        private void nudFrom_ValueChanged(object sender, EventArgs e)
        {
            From = ((int)nudFrom.Value);
            if (nudTo.Value != nudFrom.Value + Count-1)
            {
                nudTo.Value = nudFrom.Value + Count-1;
            }
        }

        private void nudTo_ValueChanged(object sender, EventArgs e)
        {
            To = ((int)nudTo.Value);
            if (nudFrom.Value != nudTo.Value - Count+1)
            {
                nudFrom.Value = nudTo.Value - Count+1;
            }
        }

        private void SetEpisodes_Load(object sender, EventArgs e)
        {
            nudFrom.Value = firstEpNr;
            nudTo.Value = nudFrom.Value + Count - 1;
            nudTo.Minimum = nudTo.Value;
            nudTo.Maximum = nudFrom.Maximum + Count - 1;
            nudFrom.Select();
        }
    }
}
