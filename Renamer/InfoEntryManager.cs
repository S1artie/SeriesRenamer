﻿using System;
using System.Collections.Generic;
using System.Text;
using Renamer.Classes;
using System.IO;
using System.Collections;
using Renamer.Classes.Configuration.Keywords;
using System.Text.RegularExpressions;
using Renamer.Dialogs;
using System.Windows.Forms;
using Renamer.Logging;
using BrightIdeasSoftware;
using System.ComponentModel;

namespace Renamer
{
    class InfoEntryManager : IEnumerable
    {
        protected static InfoEntryManager instance;
        private static object m_lock = new object();
        private BackgroundWorker worker = null;
        private DoWorkEventArgs dwea = null;
        private double ProgressAtCopyStart=0;
        private double ProgressAtCopyEnd = 0;
        public static InfoEntryManager Instance {
            get {
                if (instance == null) {
                    lock (m_lock) { if (instance == null) instance = new InfoEntryManager(); }
                }
                return instance;
            }
        }


        /// <summary>
        /// List of files, their target destinations etc
        /// </summary>
        protected List<InfoEntry> episodes = new List<InfoEntry>();

        public void Clear() {
            this.episodes.Clear();
        }

        public InfoEntry this[int index] {
            get {
                return this.episodes[index];
            }
        }

        public int Count {
            get {
                return this.episodes.Count;
            }
        }

        public void Remove(InfoEntry ie) {
            this.episodes.Remove(ie);
        }
        public int IndexOf(InfoEntry ie)
        {
            for (int i = 0; i < episodes.Count; i++)
            {
                if (episodes[i] == ie)
                {
                    return i;
                }
            }
            return -1;
        }
        public void RemoveMissingFileEntries() {
            //scan for files which got deleted so we can remove them
            InfoEntry ie;
            for (int i = 0; i < this.episodes.Count; i++) {
                ie = this.episodes[i];
                if (!File.Exists(ie.FilePath.Path + Path.DirectorySeparatorChar + ie.Name)) {
                    this.episodes.Remove(ie);
                    i--;
                }
            }
        }

        /// <summary>
        /// creates names for all entries using season, episode and name and the target pattern
        /// <param name="movie">If used on movie files, target pattern will be ignored and only name property is used</param>
        /// </summary>
        public void CreateNewNames() {
            for (int i = 0; i < this.episodes.Count; i++) {
                this.episodes[i].CreateNewName();
            }
        }

        /// <summary>
        /// Gets video files matching season and episode number
        /// </summary>
        /// <param name="season">season to search for</param>
        /// <param name="episode">episode to search for</param>
        /// <returns>List of all matching InfoEntries, never null, but may be empty</returns>
        public List<InfoEntry> GetMatchingVideos(string showname, int season, int episode) {
            List<InfoEntry> lie = new List<InfoEntry>();
            foreach (InfoEntry ie in this.episodes) {
                if (ie.Showname==showname && ie.Season == season && ie.Episode == episode && ie.IsVideofile) {
                    lie.Add(ie);
                }
            }
            return lie;
        }

        /// <summary>
        /// Gets video files matching season and episode number
        /// </summary>
        /// <param name="season">season to search for</param>
        /// <param name="episode">episode to search for</param>
        /// <returns>List of all matching InfoEntries, never null, but may be empty</returns>
        public List<InfoEntry> GetVideos(string showname) {
            List<InfoEntry> lie = new List<InfoEntry>();
            foreach (InfoEntry ie in this.episodes) {
                if (ie.Showname == showname) {
                    lie.Add(ie);
                }
            }
            return lie;
        }

        /// <summary>
        /// Gets subtitle files matching season and episode number
        /// </summary>
        /// <param name="season">season to search for</param>
        /// <param name="episode">episode to search for</param>
        /// <returns>List of all matching InfoEntries, never null, but may be empty</returns>
        public List<InfoEntry> GetMatchingSubtitles(int season, int episode) {
            List<InfoEntry> lie = new List<InfoEntry>();
            foreach (InfoEntry ie in this.episodes) {
                if (ie.Season == season && ie.Episode == episode && ie.IsSubtitle) {
                    lie.Add(ie);
                }
            }
            return lie;
        }

        /// <summary>
        /// Gets video file matching to subtitle
        /// </summary>
        /// <param name="ieSubtitle">InfoEntry of a subtitle to find matching video file for</param>
        /// <returns>Matching video file</returns>
        public InfoEntry GetVideo(InfoEntry ieSubtitle) {
            List<string> vidext = new List<string>(Helper.ReadProperties(Config.Extensions));
            foreach (InfoEntry ie in this.episodes) {
                if (Path.GetFileNameWithoutExtension(ieSubtitle.Filename) == Path.GetFileNameWithoutExtension(ie.Filename)) {
                    if (vidext.Contains(ie.Extension)) {
                        return ie;
                    }
                }
            }
            return null;
        }



        /// <summary>
        /// Gets subtitle file matching to video
        /// </summary>
        /// <param name="ieVideo">InfoEntry of a video to find matching subtitle file for</param>
        /// <returns>Matching subtitle file</returns>
        public InfoEntry GetSubtitle(InfoEntry ieVideo) {
            List<string> subext = new List<string>(Helper.ReadProperties(Config.SubtitleExtensions));
            foreach (InfoEntry ie in this.episodes) {
                if (Path.GetFileNameWithoutExtension(ieVideo.Filename) == Path.GetFileNameWithoutExtension(ie.Filename)) {
                    if (subext.Contains(ie.Extension)) {
                        return ie;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get a video file matching season and episode number
        /// 
        /// </summary>
        /// <param name="season">season to search for</param>
        /// <param name="episode">episode to search for</param>
        /// <returns>InfoEntry of matching video file, null if not found or more than one found</returns>
        public InfoEntry GetMatchingVideo(string showname, int season, int episode) {
            List<InfoEntry> lie = GetMatchingVideos(showname, season, episode);
            InfoEntry ie = (lie.Count == 1) ? lie[0] : null;
            return ie;
        }

        #region IEnumerable Member

        public IEnumerator GetEnumerator() {
            return this.episodes.GetEnumerator();
        }

        #endregion

        internal void Add(InfoEntry ie) {
            this.episodes.Add(ie);
        }

        public InfoEntry GetCollidingInfoEntry(InfoEntry ie){
            foreach(InfoEntry ie2 in this){
                if(IsSameTarget(ie,ie2)){
                    return ie2;
                }
            }
            return null;
        }
        /// <summary>
        /// figures out if 2 infoentry target locations collide
        /// </summary>
        /// <param name="ie1"></param>
        /// <param name="ie2"></param>
        /// <returns></returns>
        public bool IsSameTarget(InfoEntry ie1, InfoEntry ie2)
        {
            if (ie1 == ie2) return false;
            string name1, name2, dest1, dest2;
            if (ie1.Destination == "")
            {
                dest1 = ie1.FilePath.Path;
            }
            else
            {
                dest1 = ie1.Destination;
                if (!ie1.ProcessingRequested) return false;
            }
            if (ie2.Destination == "")
            {
                dest2 = ie2.FilePath.Path;
            }
            else
            {
                dest2 = ie2.Destination;
                if (!ie2.ProcessingRequested) return false;
            }
            if (ie1.NewFilename == "")
            {
                name1 = ie1.Filename;
            }
            else
            {
                name1 = ie1.NewFilename;
                if (!ie1.ProcessingRequested) return false;
            }
            if (ie2.NewFilename == "")
            {
                name2 = ie2.Filename;
            }
            else
            {
                name2 = ie2.NewFilename;
                if (!ie2.ProcessingRequested) return false;
            }

            return name1 == name2 && dest1 == dest2;
        }

        /// <summary>
        /// Main Rename function
        /// </summary>
        public void Rename(BackgroundWorker worker, DoWorkEventArgs e) {
            this.worker = worker;
            this.dwea = e;

            long TotalBytes = 0;
            long Bytes = 0;
            foreach (InfoEntry ie in episodes)
            {
                if (ie.ProcessingRequested && ie.Destination != "" && ie.FilePath.Fullfilename.ToLower()[0] != ie.Destination.ToLower()[0])
                {
                    FileInfo info = new FileInfo(ie.FilePath.Fullfilename);
                    TotalBytes += info.Length;
                }
            }
            //Go through all files and do stuff
            for (int i = 0; i < this.episodes.Count; i++) {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    Logger.Instance.LogMessage("Renaming Cancelled.", LogLevel.INFO);
                    return;
                }
                
                InfoEntry ie = InfoEntryManager.Instance[i];
                if (ie.MarkedForDeletion&&ie.ProcessingRequested)
                {
                    try
                    {
                        File.Delete(ie.FilePath.Fullfilename);
                        episodes.Remove(ie);
                        //Go back so no entry is skipped after removal of current entry
                        i--;
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogMessage("Couldn't delete " + ie.FilePath.Fullfilename + ": " + ex.Message, LogLevel.ERROR);
                    }
                }

                //need to set before ie.Rename because it resets some conditions
                bool copyfile = ie.ProcessingRequested && ie.Destination != "" && ie.FilePath.Fullfilename.ToLower()[0] != ie.Destination.ToLower()[0];

                //if there are files which actually need to be copied since they're on a different drive, only count those for the progress bar status since they take much longer
                if (TotalBytes > 0)
                {
                    ProgressAtCopyStart = ((double)Bytes / (double)TotalBytes * 100);
                    if (copyfile)
                    {
                        ProgressAtCopyEnd = Math.Min(ProgressAtCopyStart + (long)(((double)new FileInfo(ie.FilePath.Fullfilename).Length) / (double)TotalBytes * 100), 100);
                    }
                    else
                    {
                        ProgressAtCopyEnd = ProgressAtCopyStart;
                    }
                }
                else
                {
                    ProgressAtCopyStart = i;
                    ProgressAtCopyEnd = i;
                }
                
                worker.ReportProgress((int)ProgressAtCopyStart);

                //if a InfoEntry is moved to a different destination directory that isn't visible in the current basedir, remove it
                int subdirlevel=Filepath.GetSubdirectoryLevel(Helper.ReadProperty(Config.LastDirectory), ie.Destination);
                bool remove =  subdirlevel > Helper.ReadInt(Config.MaxDepth)||subdirlevel==-1;
                //this call will also report progress more detailed by calling ReportSingleFileProgress()
                ie.Rename(worker, e);
                if (remove)
                {
                    episodes.Remove(ie);
                    //Go back so no entry is skipped after removal of current entry
                    i--;
                }
                //after file is (really) copied, add its size
                if (copyfile)
                {
                    Bytes += new FileInfo(ie.FilePath.Fullfilename).Length;
                }
            }
            if (Helper.ReadBool(Config.DeleteEmptyFolders)) {
                //Delete all empty folders code
                Helper.DeleteAllEmptyFolders(Helper.ReadProperty(Config.LastDirectory), new List<string>(Helper.ReadProperties(Config.IgnoreFiles)),worker,e);
            }
            if (!e.Cancel)
            {
                Logger.Instance.LogMessage("Renaming (and possibly moving and deleting) finished!", LogLevel.INFO);
            }
            else
            {
                Logger.Instance.LogMessage("Renaming Cancelled.", LogLevel.INFO);
            }
        }

        public CopyFileCallbackAction ReportSingleFileProgress(FileInfo source, FileInfo destination, object state,
        long totalFileSize, long totalBytesTransferred)
        {
            worker.ReportProgress((int)(ProgressAtCopyStart + (double)totalBytesTransferred / (double)totalFileSize * (ProgressAtCopyEnd - ProgressAtCopyStart)));
            if (worker.CancellationPending)
            {
                dwea.Cancel = true;
                return CopyFileCallbackAction.Cancel;
            }
            return CopyFileCallbackAction.Continue;
        }
        public void RenameShow(string from, string to)
        {
            from = from.ToLower();
            foreach (InfoEntry ie in episodes)
            {
                if (ie.Showname.ToLower() == from)
                {
                    ie.Showname = to;
                }
            }
        }
        public bool ContainsShow(string showname)
        {
            showname = showname.ToLower();
            foreach (InfoEntry ie in episodes)
            {
                if (ie.Showname.ToLower() == showname)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Decides which files should be marked for processing
        /// </summary>
        /// <param name="Basepath">basepath, as always</param>
        /// <param name="Showname">user entered showname</param>
        public void SelectSimilarFilesForProcessing(string Basepath, string Showname) {
            List<InfoEntry> matches = FindSimilarByName(Showname);
            foreach (InfoEntry ie in this.episodes) {
                if (matches.Contains(ie)) {
                    ie.ProcessingRequested = true;
                    ie.Movie = false;
                }
                else {
                    ie.ProcessingRequested = false;
                }
            }
            return;
        }

        /// <summary>
        /// Sets new title to some files and takes care of storing it properly (last [TitleHistorySize] Titles are stored)
        /// </summary>
        /// <param name="files">files to which this title should be set to</param>
        /// <param name="title">name to be set</param>
        public void SetNewTitle(List<InfoEntry> files, string title) {
            string[] LastTitlesOld = Helper.ReadProperties(Config.LastTitles);
            foreach (InfoEntry ie in files) {
                if (ie.Showname != title) {
                    ie.Showname = title;
                }
            }

            //check if list of titles contains new title
            int Index = -1;
            for (int i = 0; i < LastTitlesOld.Length; i++) {
                string str = LastTitlesOld[i];
                if (str == title) {
                    Index = i;
                    break;
                }
            }

            //if the title is new
            if (Index == -1) {
                List<string> LastTitlesNew = new List<string>();
                LastTitlesNew.Add(title);
                foreach (string s in LastTitlesOld) {
                    LastTitlesNew.Add(s);
                }
                int size = Helper.ReadInt(Config.TitleHistorySize);
                Helper.WriteProperties(Config.LastTitles, LastTitlesNew.GetRange(0, Math.Min(LastTitlesNew.Count, size)).ToArray());
            }
            //if the title is in the list already, bring it to the front
            else {
                List<string> items = new List<string>(LastTitlesOld);
                items.RemoveAt(Index);
                items.Insert(0, title);
                Helper.WriteProperties(Config.LastTitles, items.ToArray());
            }
        }
        public void SetPath(string path) {
            string refPath = path;
            SetPath(ref refPath);
        }
        /// <summary>
        /// Sets a new path for the list view
        /// </summary>
        /// <param name="path">Path to be set</param>
        public void SetPath(ref string path) {
            if (path == null || path == "" || !Directory.Exists(path))
                return;

            if (path.Length == 2) {
                if (char.IsLetter(path[0]) && path[1] == ':') {
                    path = path + Path.DirectorySeparatorChar;
                }
            }
            DirectoryInfo currentpath = new DirectoryInfo(path);

            //Skip this for now on networks
            
            if(!path.StartsWith("\\\\")){
                //fix casing of the path if user entered it
                string fixedpath = "";
                while (currentpath.Parent != null) {
                    fixedpath = currentpath.Parent.GetDirectories(currentpath.Name)[0].Name + Path.DirectorySeparatorChar + fixedpath;
                    currentpath = currentpath.Parent;
                }
                fixedpath = currentpath.Name.ToUpper() + fixedpath;
                fixedpath = fixedpath.TrimEnd(new char[] { Path.DirectorySeparatorChar });
                if (fixedpath.Length == 2) {
                    if (char.IsLetter(fixedpath[0]) && fixedpath[1] == ':') {
                        fixedpath = fixedpath + Path.DirectorySeparatorChar;
                    }
                }
                path = fixedpath;
            }
            //Same path, ignore
            if (Helper.ReadProperty(Config.LastDirectory).ToLower() == path.ToLower()) {
                return;
            }
            else {
                Helper.WriteProperty(Config.LastDirectory, path);
                Environment.CurrentDirectory = path;
            }
        }

        /// <summary>
        /// Finds similar files by looking at the filename and comparing it to a showname
        /// </summary>
        /// <param name="Basepath">basepath of the show</param>
        /// <param name="Showname">name of the show to filter</param>
        /// <param name="source">source files</param>
        /// <returns>a list of matches</returns>
        public List<InfoEntry> FindSimilarByName(string Showname) {
            List<InfoEntry> matches = new List<InfoEntry>();
            Showname = Showname.ToLower();
            //whatever, just check path and filename if it contains the showname
            foreach (InfoEntry ie in this.episodes) {
                string[] folders = Helper.splitFilePath(ie.FilePath.Path);
                string processed = ie.Filename.ToLower();

                //try to extract the name from a shortcut, i.e. sga for Stargate Atlantis
                string pattern = "[^\\w]";
                Match m = Regex.Match(processed, pattern, RegexOptions.IgnoreCase);
                if (m != null && m.Success) {
                    string abbreviation = processed.Substring(0, m.Index);
                    if (abbreviation.Length > 0 && Helper.ContainsLetters(abbreviation, Showname)) {
                        matches.Add(ie);
                        continue;
                    }
                }

                //now check if whole showname is in the filename
                string CleanupRegex = Helper.ReadProperty(Config.CleanupRegex);
                processed = Regex.Replace(processed, CleanupRegex, " ");
                if (processed.Contains(Showname)) {
                    matches.Add(ie);
                    continue;
                }

                //or in some top folder
                foreach (string str in folders) {
                    processed = str.ToLower();
                    processed = Regex.Replace(processed, CleanupRegex, " ");
                    if (processed.Contains(Showname)) {
                        matches.Add(ie);
                        break;
                    }
                }
            }
            return matches;
        }


        #region Unsused for now!
        public static int GetNumberOfVideoFilesInFolder(string path) {
            List<string> vidext = new List<string>(Helper.ReadProperties(Config.Extensions));
            int count = 0;
            foreach (string file in Directory.GetFiles(path)) {
                if (vidext.Contains(Path.GetFileNameWithoutExtension(file))) {
                    count++;
                }
            }
            return count;
        }
        #endregion

        public void ClearRelation(string showname)
        {
            foreach (InfoEntry ie in episodes)
            {
                if (ie.Showname == showname)
                {
                    ie.Name = "";
                }
            }
        }
        public void ClearRelations()
        {
            foreach (InfoEntry ie in episodes)
            {
                ie.Name = "";
            }
        }
    }
}
