#region SVN Info
/***************************************************************
 * $Author: matthias.bilger $
 * $Revision: 38 $
 * $Date: 2009-06-11 15:59:54 +0000 (Thu, 11 Jun 2009) $
 * $LastChangedBy: matthias.bilger $
 * $LastChangedDate: 2009-06-11 15:59:54 +0000 (Thu, 11 Jun 2009) $
 * $URL: http://seriesrenamer.googlecode.com/svn/trunk/Renamer/Classes/Relation.cs $
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
    /// A season/episode&lt;-&gt;name relation
    /// </summary>
    public class Relation
    {

        #region Members
        private int season = -1;
        private int episode = -1;
        private string name = "";
        #endregion

        #region Properties
        /// <summary>
        /// Season this relation stands for
        /// </summary>
        public int Season {
            get { return season; }
            set { season = value; }
        }

        /// <summary>
        /// Episode this relation stands for
        /// </summary>
        public int Episode {
            get { return episode; }
            set { episode = value; }
        }

        /// <summary>
        /// Name this relation stands for
        /// </summary>
        public string Name {
            get { return name; }
            set { name = value; }
        }
        #endregion
        #region Public methods
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="season">Season</param>
        /// <param name="episode">Episode</param>
        /// <param name="name">Name</param>
        public Relation(int season, int episode, string name)
        {
            Season = season;
            Episode = episode;
            Name = name;
        }

        public override string ToString() {
            return "S" + Season + "E" + Episode + " - " + Name;
        }

        /// <summary>
        /// Converts the relation to a string, matching the given pattern
        /// </summary>
        /// <param name="pattern">pattern to format the string, see <see cref=""/> for details</param>
        /// <returns>relations string</returns>
        public string ToString(string pattern) {
            pattern = pattern.Replace("%S", this.season.ToString("00"));
            pattern = pattern.Replace("%s", this.season.ToString());
            pattern = pattern.Replace("%E", this.episode.ToString("00"));
            pattern = pattern.Replace("%e", this.episode.ToString());
            pattern = pattern.Replace("%N", this.name);
            return pattern;
        }
        #endregion
    }
    
}
