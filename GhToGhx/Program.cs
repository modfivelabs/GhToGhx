/*
+------------------------------------------------------------------------------------+
| Author: RIL, 2019-02-12
| License: MIT, see separate license file LICENSE.md
+------------------------------------------------------------------------------------+
*/
namespace GhToGhx
{
    using GH_IO.Serialization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.IO.Compression;

    internal class RILGH_GhToGhx
    {
        const string APPNAME = "GhToGhx";

        const string EXTENSION_GH = ".gh"; // std ext
        const string EXTENSION_GHX = ".ghx"; // std ext
        const string EXTENSION_GZIP = ".gz"; // std ext
        const string EXTENSION_MASK_GH = "*.gh"; // filename.gh
        const string EXTENSION_MASK_GHX = "*.ghx"; // filename.ghx
        const string EXTENSION_MASK_GZIP = "*.ghx.gz"; // filename.ghx.gz

        const string REGEX_EXTENSION_MASK_GH = @"^[^[]*\" + EXTENSION_GH + "$";
        const string REGEX_EXTENSION_MASK_GHX = @"^[^[]*\" + EXTENSION_GHX + "$";
        const string REGEX_EXTENSION_MASK_GZIP = @"^[^[]*\" + EXTENSION_GHX + EXTENSION_GZIP + "$";
        const string REGEX_ALL_LOGFILE_NAMES = @"^.+\GhToGhx-.+\.log$";

        const string EXTENSION_GH_GHX_POSTFIX = "x"; // filename.gh -> filename.ghx.gz
        const string EXTENSION_GH_GZIP_POSTFIX = "x" + EXTENSION_GZIP; // filename.gh -> filename.ghx.gz
        const string FOLDERNAME_GHX_TMP = "_ghx";
        const string FOLDERNAME_GZIP_TMP = "_gzip";
        const string LOGFILENAME_GHX_FOLDERS = @"\" + APPNAME + "-ghx-folders.log";
        const string LOGFILENAME_GZIP_FOLDERS = @"\" + APPNAME + "-gzip-folders.log";
        const string LOGFILENAME_GH = @"\" + APPNAME + "-gh-files.log";
        const string LOGFILENAME_GHX = @"\" + APPNAME + "-ghx-files.log";
        const string LOGFILENAME_GZIP = @"\" + APPNAME + "-gzip-files.log";

        // COLOR THEMES
        const ConsoleColor COLOR_CMD_EXTRA = ConsoleColor.Yellow;
        const ConsoleColor COLOR_GZIP = ConsoleColor.Yellow;
        const ConsoleColor COLOR_GHX = ConsoleColor.Green;
        const ConsoleColor COLOR_DOTTED_LINE = ConsoleColor.DarkGray;
        const ConsoleColor COLOR_HEADER_DECORATOR = ConsoleColor.DarkGray;

        private static bool m_verbose = true; // whether to log to the UI and only dumpt the output to the disk log file
        private static bool m_log = false; // whether to log to the UI and only dumpt the output to the disk log file
        //private static bool m_unique = true; // whether to skip .gh files which has a .ghx version with the same name in the same folder

        //
        // MAIN =====================================================================
        //

        internal static void Main(string[] args)
        {
            string startpath = string.Empty;
            string filename = string.Empty;
            string extension = string.Empty;

            string options = "";

            if (args.Length == 0)
            {
                Console.WriteLine("-----------------------------------------------");
                WriteLine(ConsoleColor.DarkRed,
                                  "    Missing command line option. Please enter\n"
                                + "    a path or a filename and try again.\n"
                                + "    Press any key to return to the prompt.");
                Console.WriteLine("-----------------------------------------------");
                Console.ReadKey();
                return;
            }

            if (args.Length > 0)
            {
                // Ensure that only the path and not the filename is part of the path variable.
                // It also ensures that the path is not truncated if no filename is provided.
                ExtractFilePathsParts(args[0], out startpath, out extension, out filename);
            }

            var cnt = 0;
            // ------------------------
            // COMMAND LINE (AUTOMATED)
            // ------------------------
            if (args.Length > 1)
            {
                options = args[1];

                // checks the argument string for a command line option (second argument) 
                // to be executed and then terminate the application immediately  after.
                // Additional options can be padded together, but the command must come 
                // first or second (verbose + or - can come first), and afther that the 
                // other options, if any, are consumed from the first towards the first.
                //
                // EXAMPLE: filepath\<filename.gh> -GL?
                //
                // This means; "-" = Verbose OFF, "G" = List gh files, "L" log to file and 
                // ? stops the comman from terminating immediately, instead it prompts for 
                // any key to be pressed before terminating (this is to enable inspection 
                // of the commandline console output.

                // PROMPT?
                var prompt = false;
                if (options.Length > 1 && options.Contains('?')) { prompt = true; ConsumeOption(ref options, '?'); }

                // LOG TO DISK FILE?
                m_log = false; // default
                if (options.Length > 1 && options.Contains('L')) { m_log = true; ConsumeOption(ref options, 'L'); }

                // VERBOSE?
                if (options.Length > 1 && options.Contains('-'))
                {
                    m_verbose = false; ConsumeOption(ref options, '-');
                }
                else if (options.Length > 1 && options.Contains('+'))
                {
                    m_verbose = true; ConsumeOption(ref options, '+');
                }

                // Now the options string should consist of only one char, unless some
                // unknown crap was padded to it which could not be consumed above

                // ------------
                // COMMAND
                // ------------
                switch (options)
                {
                    case "c":
                        Console.Clear(); // not so meaningful in batch mode, but while we're at it we might as well...
                        break;

                    case "C":
                        // CLEAR ALL LOG FILES
                        cnt = ClearGhLogfiles(startpath);
                        PrintCountResult(@"Deleted {0} GhToGhx-related log files from disk.", cnt);
                        break;

                    case "d":
                        cnt = ListGhxTempFolders(startpath);
                        PrintCountResult(@"Found {0} \" + FOLDERNAME_GHX_TMP + " folders", cnt);
                        break;

                    // LIST
                    case "g":
                        // LIST regular .gh files
                        cnt = ListGhFilenames(startpath, false); // must NOT be single-unique (meaning, no ghx-version)
                        PrintCountResult("Found {0} .gh files", cnt);
                        break;

                    // CONVERT
                    case "X":
                        // CONVERT to Ghx (XML)
                        var folders_cnt = 0; cnt = ConvertToGhxFiles(startpath, out folders_cnt);
                        var sub_msg = string.Format("in {0} new '" + FOLDERNAME_GHX_TMP + "' sub-folders.", folders_cnt);
                        PrintCountResult("Converted {0} files to .ghx and placed them " + sub_msg, cnt);
                        break;

                    // LIST
                    case "x":
                        // LIST Ghx files
                        cnt = ListGhxFilenames(startpath, EXTENSION_MASK_GHX, LOGFILENAME_GHX);
                        PrintCountResult("Found {0} .ghx (Xml) files", cnt);
                        break;

                    // REMOVE GHX
                    case "R":
                        // REMOVE _ghx folders (Xml)
                        cnt = RemoveGhxTempFolders(startpath, FOLDERNAME_GHX_TMP);
                        PrintCountResult(@"Removed {0} '\" + FOLDERNAME_GHX_TMP + "' (Xml) folders", cnt);
                        break;

                    // REMOVE GZIP
                    case "r":
                        // REMOVE _gzip folders (GZip)
                        cnt = RemoveGhxTempFolders(startpath, FOLDERNAME_GZIP_TMP);
                        PrintCountResult(@"Removed {0} '\" + FOLDERNAME_GZIP_TMP + "' (GZip) folders", cnt);
                        break;

                    // GZIP
                    case "Z":
                        // COMPRESS -> GZIP
                        folders_cnt = 0; cnt = CompressToGZipFiles(startpath, out folders_cnt);
                        sub_msg = string.Format(@"in {0} new '" + FOLDERNAME_GZIP_TMP + "' sub-folders.", folders_cnt);
                        PrintCountResult("Compressed {0} files to *.ghx.gz (GZip) and placed them " + sub_msg, cnt);
                        break;

                    case "z":
                        // LIST GZIP files
                        cnt = ListGhxFilenames(startpath, EXTENSION_MASK_GZIP, LOGFILENAME_GZIP);
                        PrintCountResult("Found {0} .ghx.gz (GZip) files", cnt);
                        break;

                    default:
                        Console.WriteLine("-----------------------------------------------");
                        Console.WriteLine("Unknown command line option: '{0}'", options);
                        Console.WriteLine("-----------------------------------------------");
                        break;
                } // switch

                if (prompt)
                {
                    Console.Write("Press any key to continue:\n>");
                    Console.ReadKey();
                }
            }
            else
            {
                // ------------------
                // manual interaction
                // ------------------
                Console.WriteLine();

                // loop util escaping (ESC) or quitting (Q)
                while (true)
                {
                    // "application menu" repeatedly shown before and after each command until 
                    // selecting Q (quit) or pressing the Escape key.
                    var mid_column = 34;
                    var ruler_width = mid_column * 2 + 2;
                    // --------------------------------------------------------------------------------------------
                    WriteLine(COLOR_HEADER_DECORATOR, new string('-', ruler_width));
                    Write(COLOR_HEADER_DECORATOR, "||||  "); Write(ConsoleColor.Red, "RIL"); Write(ConsoleColor.Green, " GRASSHOPPER TOOLS"); Write(COLOR_HEADER_DECORATOR, "  ||||".PadRight(mid_column + 9, '|')); Console.WriteLine();
                    WriteLine(COLOR_HEADER_DECORATOR, new string('-', ruler_width));

                    WriteLine(ConsoleColor.White, $"Path '{startpath}' ");
                    if (!string.IsNullOrEmpty(filename)) WriteLine(ConsoleColor.DarkGray, $"File '{filename}' (not being used)");
                    // --------------------------------------------------------------------------------------------
                    WriteLine(COLOR_DOTTED_LINE, new string('.', ruler_width));
                    // left+right column
                    Write(ConsoleColor.DarkCyan, "  g  : List *.gh files".PadRight(mid_column)); Write(COLOR_DOTTED_LINE, "|");
                    Write(COLOR_CMD_EXTRA, "  +/- : Verbose ON/OFF".PadRight(mid_column)); WriteLine(COLOR_DOTTED_LINE, "|");
                    // left+right column
                    Write(ConsoleColor.DarkCyan, $"  x  : List {EXTENSION_MASK_GHX} (Xml) files".PadRight(mid_column)); Write(COLOR_DOTTED_LINE, "|");
                    Write(COLOR_CMD_EXTRA, "  L/l : Log to disk ON/OFF".PadRight(mid_column)); WriteLine(COLOR_DOTTED_LINE, "|");
                    // left+right column
                    Write(ConsoleColor.DarkCyan, $"  z  : List {EXTENSION_MASK_GZIP} (GZip) files".PadRight(mid_column)); Write(COLOR_DOTTED_LINE, "|");
                    Write(COLOR_CMD_EXTRA, "  Q   : Quit".PadRight(mid_column)); WriteLine(COLOR_DOTTED_LINE, "|");
                    // left+right column
                    Write(ConsoleColor.DarkCyan, $"  d  : List {FOLDERNAME_GHX_TMP} folders".PadRight(mid_column)); Write(COLOR_DOTTED_LINE, "|");
                    Write(COLOR_CMD_EXTRA, "  c/C : Clear console/Log files".PadRight(mid_column)); WriteLine(COLOR_DOTTED_LINE, "|");
                    // --------------------------------------------------------------------------------------------
                    WriteLine(COLOR_DOTTED_LINE, new string('.', ruler_width));
                    Write(COLOR_GHX, $@"  X  : Convert to Xml   *.ghx".PadRight(mid_column)); Write(COLOR_DOTTED_LINE, "|");
                    Write(ConsoleColor.DarkRed, $@"  R   : Remove {FOLDERNAME_GHX_TMP}\*.*".PadRight(mid_column)); WriteLine(COLOR_DOTTED_LINE, "|");
                    Write(COLOR_GZIP, $@"  Z  : Compress to GZip *.ghz.gz".PadRight(mid_column)); Write(COLOR_DOTTED_LINE, "|");
                    Write(ConsoleColor.DarkRed, $@"  r   : Remove {FOLDERNAME_GZIP_TMP}\*.*".PadRight(mid_column)); WriteLine(COLOR_DOTTED_LINE, "|");
                    WriteLine(COLOR_DOTTED_LINE, new string('.', ruler_width));
                    // --------------------------------------------------------------------------------------------
                    WriteLine(ConsoleColor.White, $" - Select any of the above operations;");
                    Write(ConsoleColor.White, ">> ");

                    var key = Console.ReadKey(true);
                    Write(ConsoleColor.Red, key.Key.ToString());

                    if (key.Key != ConsoleKey.W)
                        Console.WriteLine();

                    // essentially the same commands as in the previous switch above, but
                    // these keyboard commands are adpted for manual input and UI response

                    switch (key.Key)
                    {
                        case ConsoleKey.Escape:
                        case ConsoleKey.Q:
                            // case insensitive
                            return;

                        case ConsoleKey.D:
                            if (!IsSHIFT(key)) // d
                            {
                                cnt = ListGhxTempFolders(startpath);
                                var msg = "Found {0} " + string.Format(@"\{0} folders", FOLDERNAME_GHX_TMP);
                                PrintCountResult(msg, cnt);
                                continue;
                            }
                            WriteUnknownCommand();
                            continue;

                        case ConsoleKey.G:
                            if (!IsSHIFT(key)) // g
                            {
                                cnt = ListGhFilenames(startpath, false); // must NOT be single-unique (meaning, no ghx-version)
                                PrintCountResult("Found {0} .gh-files", cnt);
                                continue;
                            }
                            WriteUnknownCommand();
                            continue;

                        case ConsoleKey.X:
                            // (X)
                            if (IsSHIFT(key))
                            {
                                // CONVERT to Ghx (XML)
                                var folders_cnt = 0; cnt = ConvertToGhxFiles(startpath, out folders_cnt);
                                var sub_msg = string.Format("in {0} new '{1}' sub-folders.", folders_cnt, FOLDERNAME_GHX_TMP);
                                PrintCountResult("Converted {0} files to .ghx and placed them " + sub_msg, cnt);
                            }
                            // (x)
                            else
                            {
                                // LIST Ghx files
                                cnt = ListGhxFilenames(startpath, EXTENSION_MASK_GHX, LOGFILENAME_GHX);
                                PrintCountResult("Found {0} .ghx (Xml) files", cnt);
                            }
                            break;

                        // CLEAR
                        case ConsoleKey.C:
                            if (IsSHIFT(key))
                            {
                                // CLEAR ALL LOG FILES
                                cnt = ClearGhLogfiles(startpath);
                                PrintCountResult(@"Deleted {0} GhToGhx-related log files from disk.", cnt);
                            }
                            else // Small "c"
                            {
                                Console.Clear(); // Clears only the console
                            }
                            break;

                        case ConsoleKey.R:
                            // REMOVE TEMP FOLDERS
                            if (IsSHIFT(key))
                            {
                                // "R" = Ghx (Xml folders)
                                cnt = RemoveGhxTempFolders(startpath, FOLDERNAME_GHX_TMP);
                                PrintCountResult(@"Removed {0} '\" + FOLDERNAME_GHX_TMP + "' (Xml) folders", cnt);
                            }
                            else
                            {
                                // "r" = Ghz (GZip folders)
                                cnt = RemoveGhxTempFolders(startpath, FOLDERNAME_GZIP_TMP);
                                PrintCountResult(@"Removed {0} '\" + FOLDERNAME_GZIP_TMP + "' (GZip) folders", cnt);
                            }
                            break;

                        // GZIP 
                        case ConsoleKey.Z:
                            // SHIFT (Z)
                            if (IsSHIFT(key))
                            {
                                // COMPRESS -> GZIP
                                var folders_cnt = 0; cnt = CompressToGZipFiles(startpath, out folders_cnt);
                                var sub_msg = string.Format(@"in {0} new '" + FOLDERNAME_GZIP_TMP + "' sub-folders.", folders_cnt);
                                PrintCountResult("Compressed {0} files to *.ghx.gz (GZip) and placed them " + sub_msg, cnt);
                            }
                            // !SHIFT (z)
                            else
                            {
                                // LIST GZIP files
                                cnt = ListGhxFilenames(startpath, EXTENSION_MASK_GZIP, LOGFILENAME_GZIP);
                                PrintCountResult("Found {0} .ghx.gz (GZip) files", cnt);
                            }
                            break;

                        case ConsoleKey.Add:
                        case ConsoleKey.OemPlus:
                            m_verbose = true;
                            PrintColoredResult(" Verbose: ", ConsoleColor.Green, "ON");
                            break;

                        case ConsoleKey.Subtract:
                        case ConsoleKey.OemMinus:
                            // NUMPAD "-", see also OenMinus below (non-shift)
                            m_verbose = false;
                            PrintColoredResult(" Verbose: ", ConsoleColor.Red, "OFF");
                            break;

                        // enable/disable writing log file to disk
                        case ConsoleKey.L:
                            // SHIFT (L)
                            if (IsSHIFT(key))
                            {
                                m_log = true; // enable writing log to disk
                                PrintColoredResult(" Log: ", ConsoleColor.Green, "ON");
                            }
                            // !SHIFT (l)
                            else
                            {
                                m_log = false;
                                PrintColoredResult(" Log: ", ConsoleColor.Red, "OFF");
                            }
                            break;

                        default:
                            WriteUnknownCommand();
                            break;

                    } // switch
                }
            }

            // ---------------------------------------------------------------------
            // Example code by David Rutten. To be re-implemented in future versions
            // ---------------------------------------------------------------------
            //    string uri = string.Empty;
            //    if (args.Length == 1)
            //        uri = args[0];

            //    while (!File.Exists(uri))
            //    {
            //        WriteLine(ConsoleColor.Green, "Type the uri of the file to load.");
            //        uri = Console.ReadLine();
            //    }

            //    var archive = new GH_Archive();
            //    if (!archive.ReadFromFile(uri))
            //    {
            //        WriteLine(ConsoleColor.Red, "Could not deserialize that file.");
            //        Console.ReadKey();
            //        return;
            //    }
            //    Console.WriteLine();

            //    while (true)
            //    {
            //        WriteLine(ConsoleColor.DarkGreen, $"File '{Path.GetFileName(uri)}' read, what data would you like to display?");
            //        WriteLine(ConsoleColor.DarkGreen, "A = Author data");
            //        WriteLine(ConsoleColor.DarkGreen, "F = File data");
            //        WriteLine(ConsoleColor.DarkGreen, "N = Object count");
            //        WriteLine(ConsoleColor.DarkGreen, "P = Plugin list");
            //        WriteLine(ConsoleColor.DarkGreen, "Q = Quit");
            //        Console.WriteLine();
            //        var key = Console.ReadKey(true); // namespace?

            //        switch (key.Key)
            //        {
            //            case ConsoleKey.Escape:
            //            case ConsoleKey.Q:
            //                return;

            //            case ConsoleKey.A:
            //                DisplayAuthorStatistics(archive);
            //                break;

            //            case ConsoleKey.F:
            //                DisplayDocumentStatistics(archive);
            //                break;

            //            case ConsoleKey.N:
            //                DisplayObjectStatistics(archive);
            //                break;

            //            case ConsoleKey.P:
            //                DisplayPluginStatistics(archive);
            //                break;

            //            default:
            //                WriteLine(ConsoleColor.Red, $"{key.Key} is not a recognised command.");
            //                break;
            //        }
            //    }
        }

        private static bool IsSHIFT(ConsoleKeyInfo key)
        {
            return (key.Modifiers & ConsoleModifiers.Shift) != 0;
        }

        private static void WriteUnknownCommand()
        {
            WriteLine(ConsoleColor.Red, "=================");
            WriteLine(ConsoleColor.Red, " Unknown command ");
            WriteLine(ConsoleColor.Red, "=================");
        }

        private static int LogFileNames(List<string> filenames, string filepath)
        {
            using (TextWriter tw = new StreamWriter(filepath))
            {
                foreach (string fname in filenames) { tw.WriteLine(fname); }
            }
            return filenames.Count();
        }

        private static int LogDirectoryNames(List<string> directories, string filepath, bool append_file = false)
        {
            using (TextWriter tw = new StreamWriter(filepath, append_file))
            {
                foreach (string dirpath in directories) { tw.WriteLine(dirpath); }
            }
            return directories.Count();
        }

        /// <summary>
        /// Splits the string options with the option-char and removes the char from the string.
        /// </summary>
        private static void ConsumeOption(ref string options, Char option)
        {
            var optionlist = options.Split(option);
            var _options = "";
            if (optionlist != null && optionlist.Length > 0)
            {
                foreach (var opt in optionlist)
                {
                    _options += opt;
                }
            }
            options = _options;
        }

        /// <summary>
        /// Ensures that only the path and not the filename is part of the path variable.
        /// Also ensures that the path is not truncated if no filename is provided.
        /// </summary>
        private static void ExtractFilePathsParts(string arg_filepath, out string path, out string extension, out string filename)
        {
            // ensure that only the path is assigned to this variable
            path = Path.GetDirectoryName(arg_filepath) + "\\";
            var tmp_name = Path.GetFileName(arg_filepath);
            extension = Path.GetExtension(tmp_name);
            filename = "";
            if (!string.IsNullOrEmpty(extension))
            {
                filename = tmp_name;
            }
        }

        private static void PrintCountResult(string fmt_message, int cnt, bool suppress_firstline = false, bool suppress_secondline = false)
        {
            // adapt line width to the string message:
            var msg = string.Format(fmt_message, cnt);
            var line_width = msg.Length;
            // print message
            if (!suppress_firstline) Console.WriteLine(new string('=', line_width));
            Console.WriteLine(msg);
            if (!suppress_secondline) Console.WriteLine(new string('=', line_width));
            Console.WriteLine();
        }

        private static void PrintColoredResult(string leadstring, ConsoleColor color, string colorstring, bool suppress_firstline = false, bool suppress_secondline = false)
        {
            // adapt line width to the string message:
            var msg = leadstring + colorstring;
            var line_width = msg.Length;
            // print message
            if (!suppress_firstline) Console.WriteLine(new string('=', line_width));
            Console.Write(leadstring); WriteLine(color, colorstring);
            if (!suppress_secondline) Console.WriteLine(new string('=', line_width));
            Console.WriteLine();
        }


        /// <summary>
        /// Returns the number of .ghx files found under the filepath. If m_log = true the 
        /// list of filenames is written to a log file on disk.
        private static int ListGhFilenames(string startpath, bool unique)
        {
            List<string> filenames = null;
            var do_sort = true;
            if (GetGhFileNames(startpath, do_sort, out filenames, m_verbose, unique))
            {
                if (m_log) LogFileNames(filenames, startpath + LOGFILENAME_GH);
                return filenames.Count;
            }
            return 0;
        }

        /// <summary>
        /// Returns the number of .ghx or .ghx.gz files found under the filepath. 
        /// If m_log = true the list of filenames is written to a log-file on disk.
        /// </summary>
        private static int ListGhxFilenames(string startpath, string extension_mask, string logfilename)
        {
            List<string> filenames = null;
            if (GetGhxFileNames(startpath, extension_mask, true, out filenames, m_verbose))
            {
                if (m_log) LogFileNames(filenames, startpath + logfilename);
                return filenames.Count;
            }
            return 0;
        }

        /// <summary>
        /// Scans recursively for temp folders named ...\_ghx or \_gzip under the startpath. 
        /// If m_log = true then the list of filenames is written to a log file on disk.
        /// </summary>
        private static int ListGhxTempFolders(string startpath)
        {
            List<string> directories;
            var tmp = Directory.GetDirectories(startpath, "*", SearchOption.AllDirectories)
                .Where(path => Regex.IsMatch(path, $@"^.*\\{FOLDERNAME_GHX_TMP}|{FOLDERNAME_GZIP_TMP}$"));
            if (tmp == null || tmp.Count() == 0)
            {
                return 0;
            }

            var ghx_path = "";
            try
            {
                directories = tmp.ToList();
                directories.Sort();
                var inc = 0;
                if (m_log)
                {
                    LogDirectoryNames(directories, startpath + LOGFILENAME_GHX_FOLDERS);
                }
                // UI display
                foreach (var path in directories)
                {
                    inc++;
                    // split and remove last part as to write that part in other color
                    var path_arr = path.Split('\\');
                    var short_path = "";
                    for (var i = 0; i < path_arr.Length - 1; i++)
                    {
                        short_path += path_arr[i] + "\\";
                    }
                    if (m_verbose)
                    {
                        Write(ConsoleColor.DarkCyan, " x"); Console.Write("[{0}] ", inc); Write(ConsoleColor.DarkCyan, short_path);
                        var temp_path = path_arr[path_arr.Length - 1];
                        if (temp_path == FOLDERNAME_GHX_TMP)
                        {
                            WriteLine(ConsoleColor.Cyan, temp_path); // XML
                        }
                        else if (temp_path == FOLDERNAME_GZIP_TMP)
                        {
                            WriteLine(ConsoleColor.Yellow, temp_path); // ZIP
                        }
                    }
                }
                return inc;
            }
            catch (Exception e)
            {
                Console.WriteLine("Attempt to list {0} & {1} directory paths failed: '{2}' : {3}", FOLDERNAME_GHX_TMP, FOLDERNAME_GZIP_TMP, ghx_path, e.Message);
            }
            return 0;
        }

        /// <summary>
        /// ConvertToGhxFiles converts regular.gh files to xml (.ghx) format and places the files 
        /// into new subfolders under each .gh file and names then \_ghx. Existing ghx files in the 
        /// subfolders are overwritten without notification.
        /// 
        /// The temp folders are meant to be easy to removed later without risk for deleting any 
        /// pre-existing original files.
        /// </summary>
        private static int ConvertToGhxFiles(string startpath, out int folders_count)
        {
            string filename = "(Unknown)";
            folders_count = 0;
            try
            {
                folders_count = CreateTempFolders(startpath, FOLDERNAME_GHX_TMP);
                List<string> filenames = null;
                if (GetGhFileNames(startpath, true, out filenames, false))
                {
                    if (m_verbose) { WriteLine(ConsoleColor.Green, "Converting to Xml..."); }

                    // filenames.Sort(); - files are already sorted (see param "true" in the GetGhFilenames above
                    var inc = 0;
                    var ghx_path = "";
                    for (var i = 0; i < filenames.Count; i++)
                    {
                        filename = filenames[i]; // used due to the exception handling below

                        if (!File.Exists(filename))
                        {
                            Console.WriteLine("The .gh file doesn't exist; {0}", filename);
                            continue;
                        }

                        // current path
                        if (ghx_path != Path.GetDirectoryName(filename))
                        {
                            // log the same pathname only once
                            ghx_path = Path.GetDirectoryName(filename);
                            if (m_verbose) { Write(ConsoleColor.DarkCyan, "..... " + ghx_path + "\\"); WriteLine(ConsoleColor.Green, FOLDERNAME_GHX_TMP); }
                        }

                        var archive = new GH_Archive();
                        if (archive.ReadFromFile(filename))
                        {
                            inc++;
                            var ghx_filename = Path.GetFileName(filename) + "x";
                            // write to screen
                            if (m_verbose) { Write(ConsoleColor.Green, " >"); Console.Write("[{0}] ", inc); Write(ConsoleColor.Green, $"gh->{EXTENSION_GHX}: "); Console.WriteLine(ghx_filename + " ..."); }
                            // extract plain xml/text
                            var ghx_xml = archive.Serialize_Xml();
                            // write to disk
                            var ghx_filepath = ghx_path + $@"\{FOLDERNAME_GHX_TMP}\" + ghx_filename;
                            using (TextWriter ghx_writer = new StreamWriter(ghx_filepath))
                            {
                                ghx_writer.WriteLine(ghx_xml);
                            }
                        }
                    } // for
                    return inc; // number of files converted
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read file {0}: '{1}'", filename, e.Message);
            }
            return 0;
        }

        /// <summary>
        /// CompressToGZipFiles converts regular .gh files to xml (.ghx) and compresses them to gzip 
        /// format and places the files into a new subfolder named \_gzip. Existing gzip files are
        /// overwritten without notification.
        /// The temp folders are meant to be easy to removed later without risk for deleting any 
        /// pre-existing original files.
        /// </summary>
        private static int CompressToGZipFiles(string startpath, out int folders_count)
        {
            // TODO: Return also the number of newly compressed files, not only the new temp folders

            string filename = "(Unknown)";
            folders_count = 0;
            try
            {
                folders_count = CreateTempFolders(startpath, FOLDERNAME_GZIP_TMP);
                List<string> filenames = null;
                var sortnames = true;
                var unique_paths = true;
                var not_verbose = false;
                if (GetGhFileNames(startpath, sortnames, out filenames, not_verbose, unique_paths))
                {
                    if (m_verbose) { WriteLine(COLOR_GZIP, "Compressing to GZip..."); }
                    // filenames.Sort(); - files are already sorted (see param "true" in the GetGhFilenames above
                    var inc = 0;
                    var ghz_path = "";
                    for (var i = 0; i < filenames.Count; i++)
                    {
                        filename = filenames[i]; // used due to the exception handling below
                        if (!File.Exists(filename))
                        {
                            Console.WriteLine("The .gh file doesn't exist; {0}", filename); // this should never be possible
                            continue;
                        }
                        // ...........................................
                        // create the new path to the new filename.ghx
                        // ...........................................
                        // log the same pathname only once
                        if (ghz_path != Path.GetDirectoryName(filename))
                        {
                            ghz_path = Path.GetDirectoryName(filename);
                            if (m_verbose) { Write(ConsoleColor.DarkCyan, "..... " + ghz_path + "\\"); WriteLine(COLOR_GZIP, FOLDERNAME_GZIP_TMP); }
                        }
                        // --------------------------
                        // Unpack from  Binary to Xml 
                        // --------------------------
                        var archive = new GH_Archive();
                        if (archive.ReadFromFile(filename))
                        {
                            inc++;
                            var ghz_filename = Path.GetFileName(filename) + EXTENSION_GH_GZIP_POSTFIX; // filename.gh -> filename.ghx.ghz

                            // write to screen
                            if (m_verbose) { Write(COLOR_GZIP, " >"); Console.Write("[{0}] ", inc); Write(COLOR_GZIP, $@"gh->{EXTENSION_GZIP}: "); Console.WriteLine(ghz_filename); }
                            // extract plain xml/text
                            var xml = archive.Serialize_Xml();

                            // compress while writing to disk
                            var ghz_filepath = ghz_path + $@"\{FOLDERNAME_GZIP_TMP}\" + ghz_filename;

                            // Compress
                            FileStream outfile = null;
                            GZipStream compress = null;
                            StreamWriter writer = null;
                            try
                            {
                                outfile = File.Create(ghz_filepath);
                                compress = new GZipStream(outfile, CompressionLevel.Optimal, false);
                                writer = new StreamWriter(compress);
                                writer.WriteLine(xml);
                            }
                            finally
                            {
                                if (writer != null) writer.Dispose();
                                if (compress != null) compress.Dispose();
                                if (outfile != null) outfile.Dispose();
                            }

                        }
                    } // for
                    return inc; // number of files compressed
                }
            }
            catch (Exception e)
            {
                WriteLine(ConsoleColor.DarkRed, string.Format("Failed to read file {0}: '{1}'", filename, e.Message));
            }
            return 0;
        }

        /// <summary>
        /// Recursively creates temp folders (\_ghx or \_gzip) under folders which contains .gh files.
        /// </summary>
        private static int CreateTempFolders(string startpath, string temp_foldername)
        {
            List<string> directories = null;
            List<string> filenames = null;
            var sortnames = true;
            if (GetGhFileNames(startpath, sortnames, out filenames, verbose: false, unique: false) && ExtractUniqueFilePaths(filenames, out directories))
            {
                var new_folder = "(undefined path)";
                try
                {
                    if (m_verbose) { Console.WriteLine(); WriteLine(ConsoleColor.Green, "Creating temp folders..."); }
                    directories.Sort();
                    var folders_cnt = 0;
                    foreach (var path in directories)
                    {
                        new_folder = path + @"\" + temp_foldername;
                        // checking if directory exists is only for counting how many new temp directories
                        // are actually being created (they may have already been created earlier)
                        if (!Directory.Exists(new_folder))
                        {
                            Directory.CreateDirectory(new_folder);
                            folders_cnt++;
                            if (m_verbose) { Write(ConsoleColor.Green, " +"); Console.Write("[{0}] ", folders_cnt); Write(ConsoleColor.Green, "Created "); Console.Write("folder: {0}", path); WriteLine(ConsoleColor.Green, $@"\{temp_foldername}"); }
                        }
                    }

                    // Log to disk
                    if (m_log)
                    {
                        var logfilename = "";
                        if (temp_foldername == FOLDERNAME_GHX_TMP)
                        {
                            logfilename = LOGFILENAME_GHX_FOLDERS;
                        }
                        else if (temp_foldername == FOLDERNAME_GZIP_TMP)
                        {
                            logfilename = LOGFILENAME_GZIP_FOLDERS;
                        }


                        // Collect all temp directory names
                        var ghx_directories = new List<string>();
                        foreach (var path in directories)
                        {
                            ghx_directories.Add(path + $@"\{temp_foldername}");
                        }
                        // Log directory names to file
                        LogDirectoryNames(ghx_directories, startpath + logfilename);
                        // Log filenames to file
                        LogFileNames(filenames, startpath + LOGFILENAME_GH);
                    }
                    return folders_cnt;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Attempt to create temp path ({0}) failed: '{1}'", new_folder, e.Message);
                }
            }
            return 0;
        }

        /// <summary>
        /// Recursively removes folders named (like "\_ghx" ir \_ghz) created by the 
        /// Convert or Compress functions.
        /// </summary>
        private static int RemoveGhxTempFolders(string startpath, string temp_folder)
        {
            List<string> directories = null;
            List<string> filenames = null;
            if (GetGhFileNames(startpath, true, out filenames, false) && ExtractUniqueFilePaths(filenames, out directories))
            {
                // remove "...\_ghx or \ghz" folders
                try
                {
                    var folders_cnt = 0;
                    foreach (var path in directories)
                    {
                        // recursive - deletes also sub directories
                        var temp_path = path + $@"\{temp_folder}";
                        if (Directory.Exists(temp_path))
                        {
                            folders_cnt++;
                            Directory.Delete(temp_path, true);
                            if (m_verbose) { Write(ConsoleColor.DarkRed, " -"); Console.Write("[{0}] ", folders_cnt); Write(ConsoleColor.DarkRed, "Removed"); Console.Write(" folder: {0}", path); WriteLine(ConsoleColor.DarkRed, @"\" + FOLDERNAME_GHX_TMP); }
                        }
                    }
                    return folders_cnt;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Attempt to delete path failed: '{0}'", e.Message);
                }
            }
            return 0;
        }

        /// <summary>
        /// Extracts folders names, or filters away duplicate folder names, from the input list of file paths.
        /// </summary>
        private static bool ExtractUniqueFilePaths(List<string> filenames, out List<string> directories)
        {
            // ensure valid out parameter
            directories = new List<string>();
            if (filenames == null || filenames.Count == 0)
            {
                return false;
            }
            // Since multiple files may reside in the same folder, here we
            // check to exclude those dupes and collect only unique paths
            foreach (var fullpath in filenames)
            {
                var path = Path.GetDirectoryName(fullpath);
                if (!directories.Contains(path))
                {
                    directories.Add(path);
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a list of filenames with extension .ghx or .ghx.gz in temp folders under 
        /// the startfolder. Optionally sorts the filenames.
        /// </summary>
        private static bool GetGhxFileNames(string startfolder, string extension_mask, bool sortnames, out List<string> filenames, bool verbose = false)
        {
            filenames = new List<string>(); // ensure a valid output
            var startpath = "";
            // "fallback" default folder
            if (string.IsNullOrEmpty(startfolder))
            {
                startpath = System.Reflection.Assembly.GetEntryAssembly().Location;
            }
            else
            {
                startpath = startfolder;
            }
            if (!Directory.Exists(startpath))
            {
                Console.WriteLine("No such path found: ({0})", startpath);
                return false;
            }
            // Scan for file with extension ".ghx" or .ghx.gz
            List<string> ghxfiles = null;
            if (extension_mask == EXTENSION_MASK_GHX)
            {
                ghxfiles = Directory.GetFiles(startpath, EXTENSION_MASK_GHX, SearchOption.AllDirectories).Where(file => Regex.IsMatch(file, REGEX_EXTENSION_MASK_GHX)).ToList(); // @"^[^[]*\.ghx$"
            }
            else if (extension_mask == EXTENSION_MASK_GZIP)
            {
                ghxfiles = Directory.GetFiles(startpath, EXTENSION_MASK_GZIP, SearchOption.AllDirectories).Where(file => Regex.IsMatch(file, REGEX_EXTENSION_MASK_GZIP)).ToList(); //@"^[^[]*\.ghx.gz$"
            }
            else
            {
                Console.WriteLine("Unknown file extension! ('{0}')", extension_mask);
                return false;
            }

            if (ghxfiles == null || ghxfiles.Count() == 0)
            {
                return false;
            }

            filenames.AddRange(ghxfiles);
            if (sortnames) { ghxfiles.Sort(); }

            if (verbose)
            {
                var cnt = 0;
                foreach (string filename in ghxfiles)
                {
                    cnt++;
                    var path = Path.GetDirectoryName(filename);
                    var file = Path.GetFileName(filename);
                    Console.Write("  [{0}] ", cnt); Write(ConsoleColor.DarkCyan, path); Console.WriteLine("\\{0}", file);
                }
            }
            return true;
        }

        private static int ClearGhLogfiles(string startpath)
        {
            var deleted_cnt = 0;
            var log_files = Directory.GetFiles(startpath, $"{APPNAME}*.log", SearchOption.AllDirectories);
            // .Where(file => Regex.IsMatch(file, REGEX_ALL_LOGFILE_NAMES)).ToList();
            if (log_files != null && log_files.Length > 0)
            {
                deleted_cnt = log_files.Length;
                var inc = 0;
                foreach (string f in log_files)
                {
                    inc++;
                    if (m_verbose)
                    {
                        var path = Path.GetDirectoryName(f);
                        var fname = Path.GetFileName(f);
                        if (m_verbose)
                        {
                            Console.Write("  [{0}] ", inc); Write(ConsoleColor.Red, "Deleting log file ");
                            Console.Write(path + @"\"); Write(ConsoleColor.Red, fname);
                            Console.WriteLine(" from disk");
                        }
                    }
                    File.Delete(f);
                }
            }
            return deleted_cnt;
        }

        /// <summary>
        /// Returns a list of regular Grasshopper file names (extension .gh) found recursively  under 
        /// the startfolder. Optionally sorts the filenames.
        /// </summary>
        private static bool GetGhFileNames(string startfolder, bool sortnames, out List<string> filenames, bool verbose = false, bool unique = true)
        {
            filenames = new List<string>(); // ensure valid output

            var startpath = "";
            // "fallback" default folder
            if (string.IsNullOrEmpty(startfolder))
            {
                startpath = System.Reflection.Assembly.GetEntryAssembly().Location;
                startpath = Path.GetDirectoryName(startpath);
            }
            else
            {
                startpath = startfolder;
            }
            if (!Directory.Exists(startpath))
            {
                WriteLine(ConsoleColor.Red, "Path not found: " + startpath);
                return false;
            }

            var ghfiles = Directory.GetFiles(startpath, "*.gh", SearchOption.AllDirectories)
                .Where(file => Regex.IsMatch(file, REGEX_EXTENSION_MASK_GH)).ToList();

            // Collect also any ghx files to be used for excluding such .gh files which already has a ghx version
            List<string> ghxfiles = null;
            if (unique) GetGhxFileNames(startfolder, EXTENSION_MASK_GHX, sortnames, out ghxfiles, verbose); // ghxfiles are used for checking uniqueness of .gh files

            if (ghfiles == null || ghfiles.Count() == 0)
                return false;
            var cnt = 0;
            foreach (string filename in ghfiles)
            {
                var path = Path.GetDirectoryName(filename);
                var file = Path.GetFileName(filename);
                if (!unique)
                {
                    cnt++;
                    filenames.Add(filename);
                    if (verbose) { Console.Write("  [{0}] ", cnt); Write(ConsoleColor.DarkCyan, path); Console.WriteLine("\\{0}", file); }
                    continue; // skip checking for uniqueness (that is, for existing similar ghx filename below)
                }
                if (ghxfiles == null || ghxfiles.Count == 0 || !ghxfiles.Contains(filename + "x")) // exclude .ghx files
                {
                    cnt++;
                    filenames.Add(filename);
                    if (verbose) { Console.Write("  [{0}] ", cnt); Write(ConsoleColor.DarkCyan, path); Console.WriteLine("\\{0}", file); }
                }
                else if (verbose) { Console.Write(" -[{0}] Skipped file {1}\\", cnt, path); WriteLine(ConsoleColor.DarkRed, file); }
            }
            if (sortnames) { filenames.Sort(); }
            return true;
        }

        /// <summary>
        /// Colored Write
        /// </summary>
        private static void Write(ConsoleColor colour, string line)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.Write(line);
            Console.ForegroundColor = current;
        }

        /// <summary>
        /// Colored WriteLine
        /// </summary>
        private static void WriteLine(ConsoleColor colour, string line)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.WriteLine(line);
            Console.ForegroundColor = current;
        }

        // ---------------------------------------------------------------------
        // Example code by David Rutten. To be re-implemented in future versions
        // Downloaded from forum post: 
        // https://discourse.mcneel.com/t/get-grasshopper-document-object-count-without-opening-grasshopper/78311/4
        // ---------------------------------------------------------------------
        /*
        private static void DisplayAuthorStatistics(GH_Archive archive)
        {
        }

        private static void DisplayDocumentStatistics(GH_Archive archive)
        {
            var root = archive.GetRootNode;
            var definition = root.FindChunk("Definition");
            var header = definition.FindChunk("DocumentHeader");
            var properties = definition.FindChunk("DefinitionProperties");

            var archiveVersion = root.GetVersion("ArchiveVersion");
            var pluginVersion = definition.GetVersion("plugin_version");
            var documentId = header.GetGuid("DocumentID");
            var documentDate = properties.GetDate("Date");
            var documentName = properties.GetString("Name");

            Console.Write("Archive Version: "); WriteLine(ConsoleColor.DarkCyan, archiveVersion.ToString());
            Console.Write("Plug-In Version: "); WriteLine(ConsoleColor.DarkCyan, pluginVersion.ToString());
            Console.Write("Document ID:     "); WriteLine(ConsoleColor.DarkCyan, $"{{{documentId}}}");
            Console.Write("Document Date:   "); WriteLine(ConsoleColor.DarkCyan, documentDate.ToLongDateString() + " " + documentDate.ToLongTimeString());
            Console.Write("Document Name:   "); WriteLine(ConsoleColor.DarkCyan, $"'{documentName}'");
        }

        private static void DisplayObjectStatistics(GH_Archive archive)
        {
            var root = archive.GetRootNode;
            var definition = root.FindChunk("Definition");
            var objects = definition.FindChunk("DefinitionObjects");

            var count = objects.GetInt32("ObjectCount");
            var missing = 0;

            var guids = new List<Guid>(count);
            var plurality = new List<int>(count);
            var names = new List<string>(count);

            for (int i = 0; i < count; i++)
            {
                var chunk = objects.FindChunk("Object", i);
                if (chunk is null)
                    missing++;
                else
                    ParseObjectChunk(chunk, guids, plurality, names);
            }

            var descriptors = new string[guids.Count];
            for (int i = 0; i < guids.Count; i++)
            {
                var prefix = $"{plurality[i]} × {names[i]} ";
                if (prefix.Length < 30)
                    prefix += new string(' ', 30 - prefix.Length);
                descriptors[i] = $"{prefix}({guids[i]})";
            }
            Array.Sort(plurality.ToArray(), descriptors);
            Array.Reverse(descriptors);

            Console.Write("Object Count:   "); WriteLine(ConsoleColor.DarkCyan, count.ToString());
            Console.Write("Missing Chunks: "); WriteLine(ConsoleColor.DarkRed, missing.ToString());

            for (int i = 0; i < descriptors.Length; i++)
                WriteLine(ConsoleColor.Magenta, descriptors[i]);
        }

        private static void ParseObjectChunk(GH_IReader chunk, List<Guid> guids, List<int> plurality, List<string> names)
        {
            var id = chunk.GetGuid("GUID");
            var name = chunk.GetString("Name");

            var index = guids.IndexOf(id);
            if (index >= 0)
                plurality[index]++;
            else
            {
                guids.Add(id);
                names.Add(name);
                plurality.Add(1);
            }
        }

        private static void DisplayPluginStatistics(GH_Archive archive)
        {
            var root = archive.GetRootNode;
            var definition = root.FindChunk("Definition");
            var libraries = definition.FindChunk("GHALibraries");

            if (libraries is null)
            {
                WriteLine(ConsoleColor.DarkRed, "Grasshopper file contains no plug-in library data...");
                return;
            }

            var count = libraries.GetInt32("Count");
            WriteLine(ConsoleColor.DarkCyan, $"Grasshopper file contains components from {count} non-standard plug-ins.");

            for (int i = 0; i < count; i++)
            {
                WriteLine(ConsoleColor.DarkCyan, $"Plugin {i + 1}:");
                var chunk = libraries.FindChunk("Library", i);
                DisplayPluginStatistics(chunk);
                Console.WriteLine();
            }
        }

        private static void DisplayPluginStatistics(GH_IReader chunk)
        {
            if (chunk is null)
            {
                WriteLine(ConsoleColor.Red, "Missing plug-in record in file.");
                return;
            }

            var id = chunk.GetGuid("Id");
            var name = chunk.GetString("Name");
            var author = chunk.GetString("Author");
            var version = chunk.GetString("Version");

            Console.Write("  Name:    "); WriteLine(ConsoleColor.DarkCyan, name);
            Console.Write("  Author:  "); WriteLine(ConsoleColor.DarkCyan, author);
            Console.Write("  Version: "); WriteLine(ConsoleColor.DarkCyan, version);
            Console.Write("  ID:      "); WriteLine(ConsoleColor.DarkCyan, $"{{{id}}}");
        }
        */
    }
}
