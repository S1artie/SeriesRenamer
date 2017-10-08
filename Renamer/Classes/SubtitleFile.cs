#region SVN Info
/***************************************************************
 * $Author: matthias.bilger $
 * $Revision: 30 $
 * $Date: 2009-05-28 21:57:33 +0000 (Thu, 28 May 2009) $
 * $LastChangedBy: matthias.bilger $
 * $LastChangedDate: 2009-05-28 21:57:33 +0000 (Thu, 28 May 2009) $
 * $URL: http://seriesrenamer.googlecode.com/svn/trunk/Renamer/Classes/SubtitleFile.cs $
 * 
 * License: GPLv3
 * 
****************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace Renamer.Classes
{
    /// <summary>
    /// A collection of subtitle files matching to one season+episode
    /// </summary>
    public class SubtitleFile
    {
        /// <summary>
        /// List of subtitle files
        /// </summary>
        public List<string> Filenames = new List<string>();

        /// <summary>
        /// collective season value
        /// </summary>
        public int Season = -1;

        /// <summary>
        /// Collective episode value
        /// </summary>
        public int Episode = -1;
    }
}
