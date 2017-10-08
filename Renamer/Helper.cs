﻿#region SVN Info
/***************************************************************
 * $Author: matthias.bilger@gmail.com $
 * $Revision: 104 $
 * $Date: 2010-05-09 21:28:28 +0000 (Sun, 09 May 2010) $
 * $LastChangedBy: matthias.bilger@gmail.com $
 * $LastChangedDate: 2010-05-09 21:28:28 +0000 (Sun, 09 May 2010) $
 * $URL: http://seriesrenamer.googlecode.com/svn/trunk/Renamer/Helper.cs $
 * 
 * License: GPLv3
 * 
****************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using Renamer.Classes;
using Renamer.Classes.Configuration.Keywords;
using Renamer.Classes.Configuration;
using Renamer.Logging;
using System.ComponentModel;
namespace Renamer
{
    /// <summary>
    /// Helper class offering all kinds of functions, config file caching, logging, helper functions ;)
    /// </summary>
    public class Helper
    {
        public enum Languages : int { None, German, English, French, Italian };

        /// <summary>
        /// Converts a string to a bool, by compare it to some string values
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool StringToBool(string str) {
            try {
                return Convert.ToInt32(str) > 0 || str == System.Boolean.TrueString;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Checks if a string contains a series of letters, like hlo in hello
        /// </summary>
        /// <param name="letters">string which contains the letters which will be checked for in the other string</param>
        /// <param name="container">string in which the letters should be contained</param>
        /// <returns>true if container contains those letters, false otherwise</returns>
        public static bool ContainsLetters(string letters, string container) {
            foreach (char c in letters) {
                bool found = false;
                if (container.Contains(c.ToString())) {
                    found = true;
                }
                if (!found) {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Make the first letter of every word UPPERCASE, words must be sepperated by space
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UpperEveryFirst(string str) {
            str = str.ToLower();
            string[] words = str.Split(' ');
            string result = "";
            for (int i = 0; i < words.Length; i++) {
                string word = words[i];
                if (word.Length == 0) {
                    continue;
                }

                // Is this Part really needed? why care if there is a number on the beginning of a word
                // TODO: chris
                int firstAlphaIndex = 0;
                for (int j = 0; j < word.Length; j++) {
                    if (char.IsLetter(word[j])) {
                        firstAlphaIndex = j;
                        break;
                    }
                }

                if (char.IsLower(word[firstAlphaIndex])) {
                    word = word.Substring(0, firstAlphaIndex) + char.ToUpper(word[firstAlphaIndex]) + word.Substring(firstAlphaIndex + 1);
                }
                words[i] = word;
            }
            result = String.Join(" ", words);
            return result;
        }

        /// <summary>
        /// Is string a number?
        /// </summary>
        /// <param name="str">string to check</param>
        /// <returns>true if strig is numeric</returns>
        public static bool IsNumeric(string str) {
            double x;
            return Double.TryParse(str, out x);
        }

        /// <summary>
        /// Figures out if this is a movie file by looking at the destination path. Right now this only works
        /// if season subdirectories are used, as this check looks for the season directory folder in the destination
        /// path. Future versions might also check for similarity between the name of the file and the destination folder,
        /// since movie files are to be put in the same folder as their name (minus part identifiers, i.e. "CD1").
        /// </summary>
        /// <param name="ie">the file which is checked</param>
        /// <returns>true if ie is a movie file, false otherwise</returns>
        public static bool IsMovie(InfoEntry ie) {
            if (ie.Destination == "") return false;
            string[] patterns = Helper.ReadProperties(Config.Extract);
            for (int i = patterns.Length - 1; i >= 0; i--) {
                string seasondir = patterns[i].Replace("%E", "\\d*");
                seasondir = seasondir.Replace("%S", "\\d*");
                if (Regex.Match(ie.Destination, seasondir).Success) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Find similar files from subfolders
        /// </summary>
        /// <param name="source">source files</param>
        /// <param name="Basepath">base path to look for</param>
        /// <returns>a list of matches</returns>
        public static List<InfoEntry> FindSimilarByPath(List<InfoEntry> source, string Basepath) {
            List<InfoEntry> matches = new List<InfoEntry>();
            foreach (InfoEntry ie in source) {
                if (ie.FilePath.Path.StartsWith(Basepath)) {
                    matches.Add(ie);
                }
            }
            return matches;
        }

        public static bool ReadBool(string identifier, string filename) {
            string result = ReadProperty(identifier, filename);
            return StringToBool(result);
        }
        public static bool ReadBool(string identifier) {
            string result = ReadProperty(identifier);
            return StringToBool(result);
        }
        public static int ReadInt(string Identifier) {
            string result = ReadProperty(Identifier);
            int value = -1;
            try {
                Int32.TryParse(result, out value);
            }
            catch (Exception) {
                Logger.Instance.LogMessage("Couldn't parse property to int " + Identifier + " = " + result, LogLevel.ERROR);
            }
            return value;
        }
        /// <summary>
        /// Read a enum value from the Configuration
        /// </summary>
        /// <typeparam name="T">Type of expected enum value</typeparam>
        /// <param name="identifier">Identifier the enum value is stored</param>
        /// <returns></returns>
        public static T ReadEnum<T>(string identifier) {
            string result = null;
            try {
                result = ReadProperty(identifier);
                return (T)Enum.Parse(typeof(T), ReadProperty(identifier));
            }
            catch{
                Logger.Instance.LogMessage("Couldn't parse property to Enum<" + typeof(T).ToString() + "> " + identifier + " = " + result, LogLevel.ERROR);
                return default(T);
            }
        }
        /// <summary>
        /// Read a enum value from the Configuration
        /// </summary>
        /// <typeparam name="T">Type of expected enum value</typeparam>
        /// <param name="identifier">Identifier the enum value is stored</param>
        /// <returns></returns>
        public static T ReadEnum<T>(string identifier, string file) {

            string result = null;
            try {
                result = ReadProperty(identifier, file);
                return (T)Enum.Parse(typeof(T), result);
            }
            catch {
                Logger.Instance.LogMessage("Couldn't parse property to Enum<" + typeof(T).ToString() + "> " + identifier + " = " + result, LogLevel.ERROR);
                return default(T);
            }
        }

        /// <summary>
        /// Returns a variable as a string, adds a delemiter between fields of an array
        /// </summary>
        /// <param name="variable">variable to turn into a string</param>
        /// <returns></returns>
        private static string MakeConfigString(object variable, bool toLower) {
            if (variable == null) {
                return "";
            }
            //note: if this is an array really but this function is called, return it in one string form
            if (variable is string[]) {
                if (((string[])variable).Length == 0) {
                    return "";
                }
                string value = "";
                foreach (string s in ((string[])variable)) {
                    value += (toLower? s.ToLower():s) + ConfigFile.Delimiter;
                }
                return value.Substring(0, value.Length - ConfigFile.Delimiter.Length);
            }
            return (toLower ? ((string)variable).ToLower() : ((string)variable));
        }

        /// <summary>
        /// Returns a variable as a string array
        /// </summary>
        /// <param name="variable">variable to turn into a string array</param>
        /// <returns></returns>
        private static string[] MakeConfigStringArray(object variable, bool toLower) {
            if (variable == null) {
                return new string[0];
            }
            //note: if this is an array really but this function is called, return it in one string form
            if (variable is string[]) {
                if (toLower) {
                    for (int i = 0; i < ((string[])variable).Length; i++) {
                        ((string[])variable)[i] = ((string[])variable)[i].ToLower();
                    }
                }
                return (string[])variable;
            }
            return new string[] { (toLower ? ((string)variable).ToLower() : (string)variable) };
        }

        /// <summary>
        /// reads a property from cache or from a file
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <param name="FilePath">Path of the config file</param>
        /// <returns>value of the property, or null</returns>
        public static string ReadProperty(string Identifier, string FilePath) {
            return ReadProperty(Identifier, false, FilePath);
        }
        /// <summary>
        /// reads a property from cache or from a file
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <param name="FilePath">Path of the config file</param>
        /// <returns>value of the property, or null</returns>
        public static string ReadProperty(string Identifier, bool toLower, string FilePath) {
            Settings settings = Settings.Instance;

            ConfigFile config = settings[FilePath];
            if (config == null) {
                return null;
            }
            return (string)MakeConfigString(config[Identifier], toLower).Clone();
        }

        /// <summary>
        /// reads a property from main config cache/file
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <returns>value of the property, or null</returns>
        public static string ReadProperty(string Identifier) {
            return ReadProperty(Identifier, false, DefaultConfigFile());
        }

        /// <summary>
        /// reads a property from main config cache/file
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <returns>value of the property, or null</returns>
        public static string ReadProperty(string Identifier, bool toLower) {
            return ReadProperty(Identifier, toLower, DefaultConfigFile());
        }

        /// <summary>
        /// generates the default filepath for the configuration file
        /// </summary>
        /// <returns>path to the configuration file</returns>
        public static string DefaultConfigFile() {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + Settings.MainConfigFileName;
        }

        /// <summary>
        /// reads a property that consists of more than one value from a file
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <param name="FilePath">Path of the config file</param>
        /// <returns>string[] Array containing values, or null</returns>
        public static string[] ReadProperties(string Identifier, bool toLower, string FilePath) {
            Settings settings = Settings.Instance;

            ConfigFile config = settings[FilePath];
            if (config == null) {
                return null;
            }
            return (string[])MakeConfigStringArray(config[Identifier], toLower).Clone();
        }
        /// <summary>
        /// reads a property that consists of more than one value from a file
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <param name="FilePath">Path of the config file</param>
        /// <returns>string[] Array containing values, or null</returns>
        public static string[] ReadProperties(string Identifier, string FilePath) {
            return ReadProperties(Identifier, false, FilePath);
        }

        /// <summary>
        /// reads a property that consists of more than one value from default config file
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <returns>string[] Array containing values, or null</returns>
        public static string[] ReadProperties(string Identifier) {
            return ReadProperties(Identifier, false, DefaultConfigFile());
        }

        /// <summary>
        /// reads a property that consists of more than one value from default config file
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <returns>string[] Array containing values, or null</returns>
        public static string[] ReadProperties(string Identifier, bool toLower) {
            return ReadProperties(Identifier, toLower, DefaultConfigFile());
        }

        /// <summary>
        /// writes a property to the cache
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <param name="Value">Value to write</param>
        /// <param name="FilePath">Path of the config file</param>
        public static void WriteProperty(string Identifier, string Value, string FilePath) {
            Settings settings = Settings.Instance;

            ConfigFile config = settings[FilePath];
            if (config == null) {
                return;
            }
            config[Identifier] = Value;
        }

        /// <summary>
        /// writes a property to the main config cache
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <param name="Value">Value to write</param>
        public static void WriteProperty(string Identifier, string Value) {
            WriteProperty(Identifier, Value, DefaultConfigFile());
        }

        /// <summary>
        /// writes a property with more than one Value to a file,
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <param name="Value">string[] containing values to write</param>
        /// <param name="FilePath">Path of the config file</param>
        public static void WriteProperties(string Identifier, string[] Value, string FilePath) {
            Settings settings = Settings.Instance;

            ConfigFile config = settings[FilePath];
            if (config == null) {
                return;
            }
            config[Identifier] = new List<string>(Value);
        }

        /// <summary>
        /// writes a property with more than one Value to default config file
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <param name="Value">string[] containing values to write</param>
        public static void WriteProperties(string Identifier, string[] Value) {
            WriteProperties(Identifier, Value, DefaultConfigFile());

        }

        /// <summary>
        /// writes a property with more than one Value to default config file
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <param name="Value">delimiter separated string of values</param>
        public static void WriteProperties(string Identifier, string Value) {
            WriteProperties(Identifier, Value, DefaultConfigFile());
        }

        /// <summary>
        /// writes a property with more than one Value to config file
        /// </summary>
        /// <param name="Identifier">Name of the property</param>
        /// <param name="Value">delimiter separated string of values</param>
        /// /// <param name="FilePath">Path of the config file</param>
        public static void WriteProperties(string Identifier, string Value, string FilePath) {
            WriteProperties(Identifier, Value.Split(new string[] { Helper.ReadProperty(Config.Delimiter) }, StringSplitOptions.RemoveEmptyEntries), FilePath);
        }

        public static void WriteBool(string Identifier, bool value)
        {
            if (value)
            {
                WriteProperty(Identifier, "1");
            }
            else
            {
                WriteProperty(Identifier, "0");
            }
        }

        /// <summary>
        /// gets all files recursively from subdirectories using MaxDepth property
        /// </summary>
        /// <param name="directory">root folder</param>
        /// <param name="pattern">Pattern for file matching, "*" for all</param>
        /// <returns>List of FileSystemInfo classes from all matched files</returns>
        public static List<FileSystemInfo> GetAllFilesRecursively(string directory, string pattern, ref int count, BackgroundWorker worker) {
            DirectoryInfo dir = new DirectoryInfo(directory);
            return GetAllFilesRecursively(dir, pattern, 0, ref count, worker);
        }

        /// <summary>
        /// internal recursive function for getting subdirectories
        /// </summary>
        /// <param name="dir">current recursive root folder</param>
        /// <param name="pattern">Pattern for file matching, "*" for all</param>
        /// <param name="depth">Current recursive depth for cancelling recursion</param>
        /// <param name="count">Total count of all listed files</param>
        /// <returns>List of FileSystemInfo classes from all matched files</returns>
        private static List<FileSystemInfo> GetAllFilesRecursively(DirectoryInfo dir, string pattern, int depth, ref int count, BackgroundWorker worker) {
            if (worker!=null && worker.CancellationPending) return new List<FileSystemInfo>();
            List<FileSystemInfo> files;
            try {
                files = new List<FileSystemInfo>(dir.GetFileSystemInfos(pattern));
            }
            catch (Exception) {
                return null;
            }
            //remove directories? Why are they even there :(
            for (int i = 0; i < files.Count; i++) {
                if (Directory.Exists(files[i].FullName)) {
                    files.RemoveAt(i);
                }
                else
                {
                    count++;
                }
            }
            List<FileSystemInfo> all = new List<FileSystemInfo>(dir.GetFileSystemInfos());
            if (depth >= Convert.ToInt32(Helper.ReadProperty(Config.MaxDepth))) return files;
            foreach (FileSystemInfo f in all) {
                if (f is DirectoryInfo)
                {
                    List<FileSystemInfo> deeperfiles = GetAllFilesRecursively((DirectoryInfo)f, pattern, depth + 1, ref count, worker);
                    if (deeperfiles != null)
                    {
                        files.AddRange(deeperfiles);
                    }
                }
                
            }            
            //weird threading issues force me to create a new var here which is no reference
            int count2 = count;
            if (Form1.Instance.lblFileListingProgress.InvokeRequired)
            {
                Form1.Instance.lblFileListingProgress.Invoke(new EventHandler(delegate
                {
                    Form1.Instance.lblFileListingProgress.Text = "Found " + count2 + " files so far...";
                }));
            }
            else
            {
                Form1.Instance.lblFileListingProgress.Text = "Found " + count2 + " files so far...";
            }
            return files;
        }
        public static string[] splitFilePath(string path) {
            return path.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// figure out if shortstring is contained in longstring somehow
        /// </summary>
        /// <param name="longstring">longer string</param>
        /// <param name="shortstring">shorter string</param>
        /// <returns>true if longstring contains shortstring</returns>
        public static bool InitialsMatch(string longstring, string shortstring)
        {
            if (longstring != null && shortstring != null && longstring.ToLower() == shortstring.ToLower()) return true;
            //treat all word starts as capitals
            longstring = Helper.UpperEveryFirst(longstring);
            //remove spaces, because we want to match strings which are equal except of some spaces
            shortstring = shortstring.Replace(" ", "");
            longstring = longstring.Replace(" ", "");
            if (shortstring.Length > longstring.Length) return false;
            //ignore roman digits(not optimal yet, but whatever)
            string pattern = " [iI]+[ $]";
            shortstring = Regex.Replace(shortstring, pattern, "");
            int matches = 0;
            //only for statistics
            int loopcount = 0;
            //startmatches are the matches on the long string at the start (without additional characters in between, they are weighted stronger)
            int startmatches = 0;
            bool found = false;
            int pos = 0;
            for(int j=0;j<shortstring.Length;j++){
                loopcount++;
                char c = shortstring[j];
                if (c != ' ' && !Char.IsDigit(c))
                {
                    found = false;
                    for (int i = pos; i < longstring.Length; i++)
                    {
                        loopcount++;
                        char c2 = longstring[i];
                        if (Char.ToLower(c) == Char.ToLower(c2))
                        {
                            found = true;
                            matches++;
                            if (startmatches == j)
                            {
                                startmatches++;
                            }
                            pos = i + 1;
                            break;
                        }
                    }
                    if (!found && shortstring.Length < 10)
                    {
                        break;
                    }
                }
            }

            //check for single words contained in both strings, example: "MrBrooks-DerMörderInDir" and "Asp-brooks"
            int maxwordlength=0;
            int tempwordlength=0;
            pos=0;
            string longestword = "";
            string longtempword = "";
            for (int j = 0; j < shortstring.Length; j++)
            {
                loopcount++;
                char c = shortstring[j];
                if (c != ' ' && !Char.IsDigit(c))
                {
                    for (int i = 0; i < longstring.Length; i++)
                    {
                        loopcount++;
                        char c2 = longstring[i];
                        if (Char.ToLower(c) == Char.ToLower(c2))
                        {
                            tempwordlength=1;
                            longtempword = c2.ToString();
                            int shortpos = 1;
                            for (int k = i+1; k < longstring.Length&&j+shortpos<shortstring.Length; k++)
                            {
                                loopcount++;
                                char c4 = shortstring[j+shortpos];
                                char c3 = longstring[k];
                                if (Char.ToLower(c4) == Char.ToLower(c3))
                                {
                                    tempwordlength++;
                                    longtempword += c4;
                                    if (maxwordlength < tempwordlength)
                                    {
                                        maxwordlength = tempwordlength;
                                        longestword = longtempword;
                                    }
                                }
                                else
                                {
                                    tempwordlength = 0;
                                    break;
                                }
                                shortpos++;
                            }
                        }
                        else
                        {
                            tempwordlength = 0;
                        }
                    }
                }
            }

            //check in opposite direction, if short string consists of garbage mostly, check if the captial letters of long string are included in short string
            pos=0;
            bool reversefound=false;
            //only do this if longstring has more than one capital letter
            int Capitals = 0;
            for (int i = 0; i < longstring.Length; i++)
            {
                loopcount++;
                if (char.IsUpper(longstring[i]))
                {
                    Capitals++;
                    reversefound = false;
                    if(longstring[i] == char.ToUpper(shortstring[pos]))
                    {
                        reversefound = true;
                        pos++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (Capitals < 2) reversefound = false;
            //if everything matched or atleast 70% on longer strings matched or atleast 40% of the start of longer strings matched, return true
            if ((shortstring.Length<10 && found) || (shortstring.Length >= 10 && ((float)matches / ((float)shortstring.Length) > 0.7)||startmatches>3))
            {
                return true;
            }
            //Logger.Instance.LogMessage("Loopcount=" + loopcount, LogLevel.LOG);
            return reversefound || maxwordlength > 3;
        }

        /// <summary>
        /// Deletes all empty folders recursively, ignoring files from IgnoredFiles list
        /// </summary>
        /// <param name="path">Path from which to delete folders</param>
        /// <param name="IgnoredFiletypes">List of extensions(without '.' at start) of filetypes which may be deleted</param>
        public static void DeleteAllEmptyFolders(string path, List<string> IgnoredFiletypes,BackgroundWorker worker, DoWorkEventArgs e)
        {
            bool delete = true;
            string[] folders = Directory.GetDirectories(path);
            if (folders.GetLength(0) > 0)
            {
                foreach (string folder in folders)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    DeleteAllEmptyFolders(folder, IgnoredFiletypes, worker, e);
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            folders = Directory.GetDirectories(path);
            if (folders.Length != 0)
            {
                return;
            }
            string[] files = Directory.GetFiles(path);
            if (files.Length != 0)
            {
                foreach (string s in files)
                {
                    if (Path.GetExtension(s) == "" || !IgnoredFiletypes.Contains(Path.GetExtension(s).Substring(1)))
                    {
                        delete = false;
                        break;
                    }
                }
            }
            if (delete)
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage("Couldn't delete " + path + ": " + ex.Message, LogLevel.ERROR);
                }
            }
        }
        static public string GetLogfileDataPath()
        {
            return GetUserDataPath() + Path.DirectorySeparatorChar + "Renamer.log";
        }
        static public string GetUserDataPath()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            dir = System.IO.Path.Combine(dir,"SeriesRenamer");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }


    }
}