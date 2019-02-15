using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp
{

    public class GhToGhxApp
    {
        // CONSTRUCTOR
        public GhToGhxApp()
        {
            Console.WindowWidth = 140;
            m_args = null;
            StartFolder = string.Empty;
            Extension = string.Empty;
            CurrentOptions = string.Empty;
            IsVerbose = true;
            IsDrawMenu = true;
            IsDrawMenuHeader = true;
            Theme = new ColorTheme()
            {
                ColCmdExtra = ConsoleColor.Yellow,
                ColDefault = ConsoleColor.White,
                ColDottedLine = ConsoleColor.DarkGray,
                ColFolderNames = ConsoleColor.DarkCyan,
                ColGhx = ConsoleColor.Green,
                ColGzip = ConsoleColor.Yellow,
                ColMissingArgument = ConsoleColor.DarkRed,
                ColMnuCommand_gdxz = ConsoleColor.DarkCyan,
                ColMnuCommand_Rr = ConsoleColor.DarkRed,
                ColMnuFileName = ConsoleColor.DarkGray,
                ColMnuGrasshopperName = ConsoleColor.Green,
                ColMnuHeaderLine = ConsoleColor.DarkGray,
                ColMnuStartFolder = ConsoleColor.White,
                ColMnuRILText = ConsoleColor.Red,
                ColPromt = ConsoleColor.DarkGray,
                ColPressKey = ConsoleColor.White,
                ColReadKey = ConsoleColor.Red,
                ColTmpFolderGhx = ConsoleColor.Green,
                ColTmpFolderGZip = ConsoleColor.Yellow,
                ColUnknownCommand = ConsoleColor.Red,
                ColSelectCommand = ConsoleColor.White
            };
        }

        public enum ExitCode
        {
            Error = -1,
            Ok,
            Terminate
        }

        // COLOR THEME
        public struct ColorTheme
        {
            public ConsoleColor ColCmdExtra; //  = ConsoleColor.Yellow;
            public ConsoleColor ColDefault; //  = ConsoleColor.White;
            public ConsoleColor ColDottedLine; //  = ConsoleColor.DarkGray;
            public ConsoleColor ColFolderNames; //  = ConsoleColor.DarkCyan;
            public ConsoleColor ColGzip; //  = ConsoleColor.Yellow;
            public ConsoleColor ColGhx; //  = ConsoleColor.Green;
            public ConsoleColor ColMissingArgument; //  = ConsoleColor.DarkRed;
            public ConsoleColor ColMnuCommand_gdxz; //  = ConsoleColor.DarkCyan;
            public ConsoleColor ColMnuCommand_Rr; //  = ConsoleColor.DarkRed;
            public ConsoleColor ColMnuFileName; //  = ConsoleColor.DarkGray;
            public ConsoleColor ColMnuGrasshopperName; //  = ConsoleColor.Green;
            public ConsoleColor ColMnuHeaderLine; //  = ConsoleColor.DarkGray;
            public ConsoleColor ColMnuStartFolder; //  = ConsoleColor.White;
            public ConsoleColor ColMnuRILText; //  = ConsoleColor.Red;
            public ConsoleColor ColPromt; //  = ConsoleColor.DarkGray;
            public ConsoleColor ColPressKey; //  = ConsoleColor.White;
            public ConsoleColor ColReadKey; //  = ConsoleColor.Red;
            public ConsoleColor ColTmpFolderGhx; // = ColGhx;
            public ConsoleColor ColTmpFolderGZip; // = ColGzip;
            public ConsoleColor ColUnknownCommand; //  = ConsoleColor.Red;
            public ConsoleColor ColSelectCommand; //  = ConsoleColor.White;
        }

        private const bool VERBOSE_DEFAULT = true;
        private const bool LOG_DEFAULT = false;
        private const int MNU_COLUMN_MID = 34;
        private const int MNU_RULER_WIDTH = MNU_COLUMN_MID * 2 + 2;

        // STRING CONSTANTS
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
        const string LOGFILENAME_GH = @"\" + APPNAME + "-gh-files.log";
        const string LOGFILENAME_GHX = @"\" + APPNAME + "-ghx-files.log";
        const string LOGFILENAME_GHX_FOLDERS = @"\" + APPNAME + "-ghx-folders.log";
        const string LOGFILENAME_GZIP = @"\" + APPNAME + "-gzip-files.log";
        const string LOGFILENAME_GZIP_FOLDERS = @"\" + APPNAME + "-gzip-folders.log";

        // PROPERTIES
        public string[] m_args;
        private string m_currentoptions;
        private bool m_islogfiles = false; // whether to log to the UI and only dumpt the output to the disk log file
        private string m_startfolder;
        public ColorTheme Theme { get; set; }
        public string CurrentOptions {
            get { return m_currentoptions; }
            set { if (value != m_currentoptions) { m_currentoptions = value; } }
        }
        private string Extension { get; set; }
        public string FileName { get; set; }
        private bool IsDrawMenu { get; set; } // whether to draw a "menu" to the UI at each (manual) console loop
        private bool IsDrawMenuHeader { get; set; } // whether to draw a "menu header" to the UI at each (manual) console loop
        public bool IsLogFiles {
            get { return m_islogfiles; }
            set { if (value != m_islogfiles) { m_islogfiles = value; } }
        }
        private bool IsPrompt { get; set; }
        private bool IsVerbose { get; set; } // whether to log to the UI and only dumpt the output to the disk log file
        public string StartFolder {
            get { return m_startfolder; }
            set {
                if (m_startfolder != value)
                {
                    m_startfolder = value;
                    // append a trailing backslash?
                    if (!string.IsNullOrEmpty(value) && value[value.Length - 1] != '\\')
                    {
                        m_startfolder += '\\';
                    }
                }
            }
        }

        /// <summary>
        /// Whether the '?' char was provided. WARNING: Characters in CurrentOptions will be consumed
        /// in the command loop so this property will answer correctly only once.
        /// </summary>
        public bool GetOptionIsPrompt()
        {
            var result = !string.IsNullOrEmpty(CurrentOptions) && CurrentOptions.Length > 1 && CurrentOptions.Contains('?');
            if (result)
            {
                this.ConsumeOption('?');
            }
            return result;
        }

        /// <summary>
        /// Whether to Log file- or foldernames to a logfile on disk
        /// </summary>
        public bool GetOptionIsLogFiles()
        {
            if (CurrentOptions.Length == 0)
            {
                return LOG_DEFAULT;
            }
            if (CurrentOptions.Contains('L'))
            {
                ConsumeOption('L');
                return true;
            }
            if (CurrentOptions.Contains('l'))
            {
                ConsumeOption('l');
                return false;
            }
            return LOG_DEFAULT;
        }

        private bool HasVerbosityOption()
        {
            var result = HasCommandOptions &&
                (
                CurrentOptions.Contains('-')
                ||
                CurrentOptions.Contains('+')
                );
            return result;
        }

        private bool GetOptionIsVerbose()
        {
            if (CurrentOptions.Length == 0)
            {
                return VERBOSE_DEFAULT;
            }
            if (CurrentOptions.Contains('-'))
            {
                ConsumeOption('-');
                return false;
            }
            if (CurrentOptions.Contains('+'))
            {
                ConsumeOption('+');
                return true;
            }
            return VERBOSE_DEFAULT;
        }


        /// <summary>
        /// MAIN method
        /// </summary>
        public int Run(string[] args)
        {
            if (Args(ref args) && TerminateImmediately)
            {
                if (ArgsCount == 0) { this.DrawMessageMissingArgs(); }
                return (int)ExitCode.Terminate;
            }
            // ...........................................................................,
            // Ensure that only the path and not the filename is part of the path variable.
            // It also ensures that the path is not truncated if no filename is provided.
            // ...........................................................................,
            if (!SeparateFileInfo())
            {
                return (int)ExitCode.Error;
            }

            if (RunAsCommandLine)
            {
                RunCommandLine(); // For batch processing
            }
            else if (RunAsConsoleWindow)
            {
                RunConsoleWindow(); // Interactive ConsoleWindow
            }

            return (int)ExitCode.Ok;

            /*
            // ---------------------------------------------------------------------
            // Example code by David Rutten. To be re-implemented in future versions
            // ---------------------------------------------------------------------
            string uri = string.Empty;
            if (args.Length == 1)
                uri = args[0];

            while (!File.Exists(uri))
            {
                WriteLine(ConsoleColor.Green, "Type the uri of the file to load.");
                uri = Console.ReadLine();
            }

            var archive = new GH_Archive();
            if (!archive.ReadFromFile(uri))
            {
                WriteLine(ConsoleColor.Red, "Could not deserialize that file.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine();
            while (true)
            {
                WriteLine(ConsoleColor.DarkGreen, $"File '{Path.GetFileName(uri)}' read, what data would you like to display?");
                WriteLine(ConsoleColor.DarkGreen, "A = Author data");
                WriteLine(ConsoleColor.DarkGreen, "F = File data");
                WriteLine(ConsoleColor.DarkGreen, "N = Object count");
                WriteLine(ConsoleColor.DarkGreen, "P = Plugin list");
                WriteLine(ConsoleColor.DarkGreen, "Q = Quit");
                Console.WriteLine();
                var key = Console.ReadKey(true); // namespace?

                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                    case ConsoleKey.Q:
                        return;

                    case ConsoleKey.A:
                        DisplayAuthorStatistics(archive);
                        break;

                    case ConsoleKey.F:
                        DisplayDocumentStatistics(archive);
                        break;

                    case ConsoleKey.N:
                        DisplayObjectStatistics(archive);
                        break;

                    case ConsoleKey.P:
                        DisplayPluginStatistics(archive);
                        break;

                    default:
                        WriteLine(ConsoleColor.Red, $"{key.Key} is not a recognised command.");
                        break;
                }
            }
            */
        }

        /// ------------------------------------------------------------------------------
        /// <summary>
        /// MANUAL CONSOLE WINDOW - A "application menu" repeatedly shown before and after 
        /// each command until selecting Q (quit) or pressing the Escape key. </summary>
        /// ------------------------------------------------------------------------------
        public bool RunConsoleWindow()
        {
            while (true)
            {
                DrawMainMenuHeader();
                DrawMainMenu();
                DrawPrompt();

                var key = Console.ReadKey(true);
                Write(Theme.ColReadKey, key.Key.ToString());

                if (key.Key != ConsoleKey.W)
                    Console.WriteLine();

                // essentially the same commands as in the previous switch above, but
                // these keyboard commands are adpted for manual input and UI response

                var cnt = 0;
                switch (key.Key)
                {

                    case ConsoleKey.Escape:
                    case ConsoleKey.Q:
                        // case insensitive
                        return true;

                    case ConsoleKey.C:
                        // CLEAR ALL LOG FILES
                        if (IsSHIFT(key))
                        {
                            cnt = ClearGhLogfiles();
                            ColorWriteConsoleResultCount(@"Deleted {0} GhToGhx-related log files from disk.", cnt);
                            break;
                        }
                        // else lowercase "c"
                        Console.Clear(); // Clears only the console
                        continue; // avoids writing an empty row (at the end of the switch)

                    case ConsoleKey.D:
                        if (!IsSHIFT(key)) // d
                        {
                            cnt = ListTempFolders();
                            var msg = "Found {0} " + string.Format(@"\{0} folders", FOLDERNAME_GHX_TMP);
                            ColorWriteConsoleResultCount(msg, cnt);
                            continue;
                        }
                        WriteUnknownCommand();
                        break;

                    case ConsoleKey.G:
                        if (!IsSHIFT(key)) // lowercase g
                        {
                            cnt = ListGhFilenames(false); // must NOT be single-unique (meaning, no ghx-version)
                            ColorWriteConsoleResultCount("Found {0} .gh-files", cnt);
                            break;
                        }
                        WriteUnknownCommand();
                        break;

                    case ConsoleKey.H:
                        if (!IsSHIFT(key)) // lowercase h
                        {
                            IsDrawMenuHeader = !IsDrawMenuHeader;
                            break;
                        }
                        WriteUnknownCommand();
                        break;

                    case ConsoleKey.M:
                        if (!IsSHIFT(key)) // lowecase "m"
                        {
                            IsDrawMenu = !IsDrawMenu; // toggle
                            break;
                        }
                        WriteUnknownCommand();
                        break;

                    case ConsoleKey.X:
                        // (X)
                        if (IsSHIFT(key))
                        {
                            // CONVERT to Ghx (XML)
                            var folders_cnt = 0; cnt = ConvertToGhxFiles(out folders_cnt);
                            var sub_msg = string.Format("in {0} new '{1}' sub-folders.", folders_cnt, FOLDERNAME_GHX_TMP);
                            ColorWriteConsoleResultCount("Converted {0} files to .ghx and placed them " + sub_msg, cnt);
                            break;
                        }
                        // else (x):
                        // LIST Ghx files
                        cnt = ListGhxFilenames(EXTENSION_MASK_GHX, LOGFILENAME_GHX);
                        ColorWriteConsoleResultCount("Found {0} .ghx (Xml) files", cnt);
                        break;

                    case ConsoleKey.R:
                        // REMOVE TEMP FOLDERS
                        if (IsSHIFT(key))
                        {
                            // "R" = Ghx (Xml folders)
                            cnt = RemoveGhxTempFolders(FOLDERNAME_GHX_TMP);
                            ColorWriteConsoleResultCount(@"Removed {0} '\" + FOLDERNAME_GHX_TMP + "' (Xml) folders", cnt);
                            break;
                        }
                        // else lowercae "r" = Ghz (GZip folders)
                        cnt = RemoveGhxTempFolders(FOLDERNAME_GZIP_TMP);
                        ColorWriteConsoleResultCount(@"Removed {0} '\" + FOLDERNAME_GZIP_TMP + "' (GZip) folders", cnt);
                        break;

                    // GZIP 
                    case ConsoleKey.Z:
                        if (IsSHIFT(key))  // Capital "Z"
                        {
                            // COMPRESS -> GZIP
                            var folders_cnt = 0; cnt = CompressToGZipFiles(out folders_cnt);
                            var sub_msg = string.Format(@"in {0} new '" + FOLDERNAME_GZIP_TMP + "' sub-folders.", folders_cnt);
                            ColorWriteConsoleResultCount("Compressed {0} files to *.ghx.gz (GZip) and placed them " + sub_msg, cnt);
                            break;
                        }
                        // else lowercase z 
                        // LIST GZIP files
                        cnt = ListGhxFilenames(EXTENSION_MASK_GZIP, LOGFILENAME_GZIP);
                        ColorWriteConsoleResultCount("Found {0} .ghx.gz (GZip) files", cnt);
                        break;

                    case ConsoleKey.Add:
                    case ConsoleKey.OemPlus:
                        IsVerbose = true;
                        ColorWriteConsoleResult(" Verbose: ", ConsoleColor.Green, "ON");
                        break;

                    case ConsoleKey.Subtract:
                    case ConsoleKey.OemMinus:
                        // NUMPAD "-", see also OenMinus below (non-shift)
                        IsVerbose = false;
                        ColorWriteConsoleResult(" Verbose: ", ConsoleColor.Red, "OFF");
                        break;

                    // enable/disable writing log file to disk
                    case ConsoleKey.L:
                        // SHIFT (L)
                        if (IsSHIFT(key))
                        {
                            IsLogFiles = true; // enable writing log to disk
                            ColorWriteConsoleResult(" Log: ", ConsoleColor.Green, "ON");
                            break;
                        }
                        // else !SHIFT (l)
                        IsLogFiles = false;
                        ColorWriteConsoleResult(" Log: ", ConsoleColor.Red, "OFF");
                        break;

                    default:
                        WriteUnknownCommand();
                        break;
                } // switch

                Console.WriteLine();
            }
            //return true; // unreachable
            // RunConsoleWindow
        }

        /// <summary>
        // ----------------------
        /// COMMANDLINE PARAMETERS
        /// ----------------------
        /// </summary>
        public bool RunCommandLine()
        {
            // -----------------------
            // COMMANDLINE (AUTOMATED)
            // -----------------------
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

            IsPrompt = GetOptionIsPrompt(); // can be checked only once since option chars are "consumed" as they are read
            IsLogFiles = GetOptionIsLogFiles() ? true : LOG_DEFAULT; // default;
            if (HasVerbosityOption()) { IsVerbose = GetOptionIsVerbose(); }

            // Now the CurrentOptions string should consist of only one char, unless some
            // unknown crap was padded to it which could not be consumed above

            var cnt = 0;
            switch (CurrentOptions)
            {
                case "c":
                    Console.Clear(); // not so meaningful in batch mode, but while we're at it we might as well...
                    break;

                case "C":
                    // CLEAR ALL LOG FILES
                    cnt = ClearGhLogfiles();
                    ColorWriteConsoleResultCount(@"Deleted {0} GhToGhx-related log files from disk.", cnt);
                    break;

                case "d":
                    cnt = ListTempFolders();
                    ColorWriteConsoleResultCount(@"Found {0} \" + FOLDERNAME_GHX_TMP + " folders", cnt);
                    break;

                // LIST
                case "g":
                    // LIST regular .gh files
                    cnt = ListGhFilenames(false); // must NOT be single-unique (meaning, no ghx-version)
                    ColorWriteConsoleResultCount("Found {0} .gh files", cnt);
                    break;

                // CONVERT
                case "X":
                    // CONVERT to Ghx (XML)
                    var folders_cnt = 0; cnt = ConvertToGhxFiles(out folders_cnt);
                    var sub_msg = string.Format("in {0} new '" + FOLDERNAME_GHX_TMP + "' sub-folders.", folders_cnt);
                    ColorWriteConsoleResultCount("Converted {0} files to .ghx and placed them " + sub_msg, cnt);
                    break;

                // LIST
                case "x":
                    // LIST Ghx files
                    cnt = ListGhxFilenames(EXTENSION_MASK_GHX, LOGFILENAME_GHX);
                    ColorWriteConsoleResultCount("Found {0} .ghx (Xml) files", cnt);
                    break;

                // REMOVE GHX
                case "R":
                    // REMOVE _ghx folders (Xml)
                    cnt = RemoveGhxTempFolders(FOLDERNAME_GHX_TMP);
                    ColorWriteConsoleResultCount(@"Removed {0} '\" + FOLDERNAME_GHX_TMP + "' (Xml) folders", cnt);
                    break;

                // REMOVE GZIP
                case "r":
                    // REMOVE _gzip folders (GZip)
                    cnt = RemoveGhxTempFolders(FOLDERNAME_GZIP_TMP);
                    ColorWriteConsoleResultCount(@"Removed {0} '\" + FOLDERNAME_GZIP_TMP + "' (GZip) folders", cnt);
                    break;

                // GZIP
                case "Z":
                    // COMPRESS -> GZIP
                    folders_cnt = 0; cnt = CompressToGZipFiles(out folders_cnt);
                    sub_msg = string.Format(@"in {0} new '" + FOLDERNAME_GZIP_TMP + "' sub-folders.", folders_cnt);
                    ColorWriteConsoleResultCount("Compressed {0} files to *.ghx.gz (GZip) and placed them " + sub_msg, cnt);
                    break;

                case "z":
                    // LIST GZIP files
                    cnt = ListGhxFilenames(EXTENSION_MASK_GZIP, LOGFILENAME_GZIP);
                    ColorWriteConsoleResultCount("Found {0} .ghx.gz (GZip) files", cnt);
                    break;

                default:
                    if (string.IsNullOrEmpty(CurrentOptions))
                    {
                        // An empty options can happen if running only aettings switches 
                        // which then has been "consumed" by now after reading the options
                        break;
                    }
                    Console.WriteLine("-----------------------------------------------");
                    Console.WriteLine("Unknown command line option: '{0}'", CurrentOptions);
                    Console.WriteLine("-----------------------------------------------");
                    break;
            } // switch

            if (IsPrompt)
            {
                Console.Write("Press any key to continue:\n>");
                Console.ReadKey();
            }
            return true;
        }

        /// <summary>
        /// Splits the string options with the option-char and removes the char from the string.
        /// </summary>
        public void ConsumeOption(char option)
        {
            var optionlist = m_currentoptions.Split(option);
            var _options = "";
            // remove option char from the option list and rebuild a new (remaining) options string
            if (optionlist != null && optionlist.Length > 0)
            {
                foreach (var opt in optionlist)
                {
                    _options += opt;
                }
            }
            m_currentoptions = _options; // store the rebuilt remaining option list
        }

        /// <summary>
        /// Ensures that only the path and not the filename is part of the path variable.
        /// Also ensures that the path is not truncated if no filename is provided.
        /// </summary>
        private bool SeparateFileInfo()
        {
            if (m_args == null || m_args.Length == 0)
                return false;
            var arg_path = m_args[0];

            // Ensure separated path
            StartFolder = Path.GetDirectoryName(arg_path) + "\\";

            var tmp_fname = Path.GetFileName(arg_path);
            Extension = Path.GetExtension(tmp_fname);

            // Save as filename only if a file-extension was found
            if (!string.IsNullOrEmpty(Extension))
            {
                FileName = tmp_fname;
                return true; // done
            }
            // Only a path was provided
            FileName = "";
            return true;
        }


        /// <summary>
        /// Input setter for CommandLine Args
        /// </summary>
        public bool Args(ref string[] _args)
        {
            m_args = _args;
            if (m_args == null || m_args.Length == 0)
                return false;
            // input is valid 
            if (m_args.Length > 0)
            {
                StartFolder = m_args[0];
            }
            if (m_args.Length > 1)
            {
                CurrentOptions = m_args[1];
            }
            return true; // enable nested calls
        }

        public string Args(int index)
        {
            if (m_args == null || m_args.Length == 0 || index > m_args.Length - 1 || index < 0)
                return ""; // safe, no nulls!
            return m_args[index];
        }

        /// <summary>
        /// Validates folder path and parameters. Returning 0 if all values are valid. 
        /// Returns < 0 if anything is wrong. Returns -1 if folder path is invalid.
        /// Returns -2 if parameters are invalid.
        /// </summary>
        public bool ValidateInputData()
        {
            if (m_args == null)
                return false;
            if (ArgsCount > 0)
            {

            }
            return true;
        }

        /// <summary>
        /// If returning true the main program should enter a user 
        /// interactive loop checking entered commands
        /// </summary>
        public bool RunAsConsoleWindow {
            get {
                return m_args != null && m_args.Length > 0;
            }
        }

        /// <summary>
        /// If returning true the main program should start examining parameters
        /// </summary>
        public bool RunAsCommandLine {
            get {
                return HasCommandOptions;
            }
        }

        public bool HasCommandOptions {
            get {
                return CommandOptionsCount > 0;
            }
        }

        public int ArgsCount {
            get {
                if (m_args == null)
                    return 0;
                return m_args.Length;
            }
        }

        public int CommandOptionsCount {
            get {
                if (ArgsCount < 2)
                    return 0;
                return ArgsCount - 1; // remove first parameter, which is a file or folder
            }
        }

        public bool TerminateImmediately {
            get {
                return ArgsCount == 0;
            }
        }

        private void WriteUnknownCommand()
        {
            WriteLine(Theme.ColUnknownCommand, "=================");
            WriteLine(Theme.ColUnknownCommand, " Unknown command ");
            WriteLine(Theme.ColUnknownCommand, "=================");
        }

        public bool DrawMessageMissingArgs()
        {
            WriteLine(Theme.ColMissingArgument, "+-----------------------------------------------+");
            WriteLine(Theme.ColMissingArgument, "|    Missing command line option. Please enter  |");
            WriteLine(Theme.ColMissingArgument, "|    a path or a filename and try again.        |");
            WriteLine(Theme.ColMissingArgument, "+-----------------------------------------------+");
            WriteLine(Theme.ColPressKey, "     Press any key to return to the prompt:");
            Console.ReadKey();
            return true;
        }

        private void DrawMainMenuHeader()
        {
            if (!IsDrawMenuHeader)
            {
                return;
            }
            WriteLine(Theme.ColMnuHeaderLine, new string('-', MNU_RULER_WIDTH));
            {
                // --------------------------------------------------------------------------------------------
                var version = GetProductVersion();
                // lead text: 
                // "|||| RIL GRASSHOPPER TOOLS"
                Write(Theme.ColMnuHeaderLine, "||||  ");
                Write(Theme.ColMnuRILText, "RIL"); Write(Theme.ColMnuGrasshopperName, " GRASSHOPPER "); Write(Theme.ColMnuRILText, "TOOLS");
                // fill the rest of the line, and include version info, like so:
                // "|||| RIL GRASSHOPPER TOOLS "||||||||||||||||||| version ||"
                Write(Theme.ColMnuHeaderLine, " ||||".PadRight(7, '|')); // <-- exactly in the middle
                var from_middle = MNU_COLUMN_MID - (version.Length + 2);
                Write(Theme.ColMnuHeaderLine, "||||".PadRight(from_middle, '|')); // <-- fill up until version
                Console.Write(" {0}", version.ToString());
                WriteLine(Theme.ColMnuHeaderLine, " ||");
                // --------------------------------------------------------------------------------------------
            }
            WriteLine(Theme.ColMnuHeaderLine, new string('-', MNU_RULER_WIDTH));
        }

        private void DrawMainMenu()
        {
            WriteLine(Theme.ColMnuStartFolder, $"Path '{StartFolder}' ");
            if (!IsDrawMenu)
            {
                return;
            }
            if (!string.IsNullOrEmpty(FileName)) WriteLine(Theme.ColMnuFileName, $"File '{FileName}' (not being used)");
            WriteLine(Theme.ColDottedLine, new string('.', MNU_RULER_WIDTH));
            // left+right column
            Write(Theme.ColMnuCommand_gdxz, "  g  : List *.gh files".PadRight(MNU_COLUMN_MID)); Write(Theme.ColDottedLine, "|");
            Write(Theme.ColCmdExtra, "  +/- : Verbose ON/OFF".PadRight(MNU_COLUMN_MID)); WriteLine(Theme.ColDottedLine, "|");
            // left+right column
            Write(Theme.ColMnuCommand_gdxz, $"  x  : List {EXTENSION_MASK_GHX} (Xml) files".PadRight(MNU_COLUMN_MID)); Write(Theme.ColDottedLine, "|");
            Write(Theme.ColCmdExtra, "  L/l : Log to disk ON/OFF".PadRight(MNU_COLUMN_MID)); WriteLine(Theme.ColDottedLine, "|");
            // left+right column
            Write(Theme.ColMnuCommand_gdxz, $"  z  : List {EXTENSION_MASK_GZIP} (GZip) files".PadRight(MNU_COLUMN_MID)); Write(Theme.ColDottedLine, "|");
            Write(Theme.ColCmdExtra, "  Q   : Quit".PadRight(MNU_COLUMN_MID)); WriteLine(Theme.ColDottedLine, "|");
            // left+right column
            Write(Theme.ColMnuCommand_gdxz, $"  d  : List {FOLDERNAME_GHX_TMP}/{FOLDERNAME_GZIP_TMP}".PadRight(MNU_COLUMN_MID)); Write(Theme.ColDottedLine, "|");
            Write(Theme.ColCmdExtra, "  c/C : Clear console/Log files".PadRight(MNU_COLUMN_MID)); WriteLine(Theme.ColDottedLine, "|");
            // --------------------------------------------------------------------------------------------
            WriteLine(Theme.ColDottedLine, new string('.', MNU_RULER_WIDTH));
            Write(Theme.ColGhx, $@"  X  : Convert to Xml   *.ghx".PadRight(MNU_COLUMN_MID)); Write(Theme.ColDottedLine, "|");
            Write(Theme.ColMnuCommand_Rr, $@"  R   : Remove {FOLDERNAME_GHX_TMP}\*.*".PadRight(MNU_COLUMN_MID)); WriteLine(Theme.ColDottedLine, "|");
            Write(Theme.ColGzip, $@"  Z  : Compress to GZip *.ghz.gz".PadRight(MNU_COLUMN_MID)); Write(Theme.ColDottedLine, "|");
            Write(Theme.ColMnuCommand_Rr, $@"  r   : Remove {FOLDERNAME_GZIP_TMP}\*.*".PadRight(MNU_COLUMN_MID)); WriteLine(Theme.ColDottedLine, "|");
            WriteLine(Theme.ColDottedLine, new string('.', MNU_RULER_WIDTH));
            // --------------------------------------------------------------------------------------------
            WriteLine(Theme.ColSelectCommand, $"  - Select any of the above operations:");
        }

        private static string GetProductVersion()
        {
            return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion;
        }

        private void DrawPrompt()
        {
            Write(Theme.ColPromt, ">> ");
        }

        private static bool IsSHIFT(ConsoleKeyInfo key)
        {
            return (key.Modifiers & ConsoleModifiers.Shift) != 0;
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
        /// Returns the number of .ghx files found under the filepath. If m_islogfiles = true the 
        /// list of filenames is written to a log file on disk.
        private int ListGhFilenames(bool unique)
        {
            List<string> filenames = null;
            var do_sort = true;
            if (GetGhFileNames(do_sort, out filenames, IsVerbose, unique))
            {
                if (IsLogFiles) LogFileNames(filenames, StartFolder + LOGFILENAME_GH);
                return filenames.Count;
            }
            return 0;
        }

        /// <summary>
        /// Returns the number of .ghx or .ghx.gz files found under the filepath. 
        /// If m_islogfiles = true the list of filenames is written to a log-file on disk.
        /// </summary>
        private int ListGhxFilenames(string extension_mask, string logfilename)
        {
            List<string> filenames = null;
            if (GetGhxFileNames(extension_mask, true, out filenames, IsVerbose))
            {
                if (IsLogFiles) LogFileNames(filenames, StartFolder + logfilename);
                return filenames.Count;
            }
            return 0;
        }

        /// <summary>
        /// Scans recursively for temp folders named ...\_ghx or \_gzip under the startpath. 
        /// If m_islogfiles = true then the list of filenames is written to a log file on disk.
        /// </summary>
        private int ListTempFolders()
        {
            List<string> directories;
            var tmp = Directory.GetDirectories(StartFolder, "*", SearchOption.AllDirectories)
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
                if (IsLogFiles)
                {
                    LogDirectoryNames(directories, StartFolder + LOGFILENAME_GHX_FOLDERS);
                }
                // UI display
                foreach (var path in directories)
                {
                    inc++;

                    if (IsVerbose)
                    {
                        // split and remove last part as to write that part in other color
                        var short_path = "";
                        var path_arr = GetShrinkedPath(path, out short_path);

                        // Pick color depending on type of trailing temp-folder (ghx or gzip)
                        var temp_folder = path_arr[path_arr.Length - 1];
                        var print_char = "";
                        ConsoleColor color = Theme.ColDefault;
                        if (temp_folder == FOLDERNAME_GHX_TMP)
                        {
                            print_char = "x";
                            color = Theme.ColTmpFolderGhx;
                        }
                        else if (temp_folder == FOLDERNAME_GZIP_TMP)
                        {
                            print_char = "z";
                            color = Theme.ColTmpFolderGZip;
                        }
                        Write(color, $" {print_char}"); Console.Write("[{0}] ", inc); Write(Theme.ColFolderNames, short_path); WriteLine(color, temp_folder);
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

        private string[] GetShrinkedPath(string path, out string short_path)
        {
            var path_arr = path.Split('\\');
            short_path = "";
            for (var i = 0; i < path_arr.Length - 1; i++)
            {
                short_path += path_arr[i] + "\\";
            }
            return path_arr;
        }
        /// <summary>
        /// ConvertToGhxFiles converts regular.gh files to xml (.ghx) format and places the files 
        /// into new subfolders under each .gh file and names then \_ghx. Existing ghx files in the 
        /// subfolders are overwritten without notification.
        /// 
        /// The temp folders are meant to be easy to removed later without risk for deleting any 
        /// pre-existing original files.
        /// </summary>
        private int ConvertToGhxFiles(out int folders_count)
        {
            string filename = "(Unknown)";
            folders_count = 0;
            try
            {
                folders_count = CreateTempFolders(FOLDERNAME_GHX_TMP);
                List<string> filenames = null;
                if (GetGhFileNames(true, out filenames, false))
                {
                    if (IsVerbose) { WriteLine(ConsoleColor.Green, "Converting to Xml..."); }

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
                            if (IsVerbose) { Write(ConsoleColor.DarkCyan, "..... " + ghx_path + "\\"); WriteLine(ConsoleColor.Green, FOLDERNAME_GHX_TMP); }
                        }

                        var archive = new GH_IO.Serialization.GH_Archive();
                        if (archive.ReadFromFile(filename))
                        {
                            inc++;
                            var ghx_filename = Path.GetFileName(filename) + "x";
                            // write to screen
                            if (IsVerbose) { Write(ConsoleColor.Green, " >"); Console.Write("[{0}] ", inc); Write(ConsoleColor.Green, $"gh->{EXTENSION_GHX}: "); Console.WriteLine(ghx_filename + " ..."); }
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
        private int CompressToGZipFiles(out int folders_count)
        {
            // TODO: Return also the number of newly compressed files, not only the new temp folders

            string filename = "(Unknown)";
            folders_count = 0;
            try
            {
                folders_count = CreateTempFolders(FOLDERNAME_GZIP_TMP);
                List<string> filenames = null;
                var sortnames = true;
                var unique_paths = true;
                var not_verbose = false;
                if (GetGhFileNames(sortnames, out filenames, not_verbose, unique_paths))
                {
                    if (IsVerbose) { WriteLine(Theme.ColGzip, "Compressing to GZip..."); }
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
                            if (IsVerbose) { Write(ConsoleColor.DarkCyan, "..... " + ghz_path + "\\"); WriteLine(Theme.ColGzip, FOLDERNAME_GZIP_TMP); }
                        }
                        // --------------------------
                        // Unpack from  Binary to Xml 
                        // --------------------------
                        var archive = new GH_IO.Serialization.GH_Archive();
                        if (archive.ReadFromFile(filename))
                        {
                            inc++;
                            var ghz_filename = Path.GetFileName(filename) + EXTENSION_GH_GZIP_POSTFIX; // filename.gh -> filename.ghx.ghz

                            // write to screen
                            if (IsVerbose) { Write(Theme.ColGzip, " >"); Console.Write("[{0}] ", inc); Write(Theme.ColGzip, $@"gh->{EXTENSION_GZIP}: "); Console.WriteLine(ghz_filename); }
                            // extract plain xml/text
                            var xml = archive.Serialize_Xml();

                            // compress while writing to disk
                            var ghz_filepath = ghz_path + $@"\{FOLDERNAME_GZIP_TMP}\" + ghz_filename;

                            // Compress
                            FileStream outfile = null;
                            StreamWriter writer = null;
                            System.IO.Compression.GZipStream compress = null;
                            try
                            {
                                outfile = File.Create(ghz_filepath);
                                compress = new System.IO.Compression.GZipStream(outfile, System.IO.Compression.CompressionLevel.Optimal, false);
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
        private int CreateTempFolders(string temp_foldername)
        {
            List<string> directories = null;
            List<string> filenames = null;
            var sortnames = true;
            if (GetGhFileNames(sortnames, out filenames, verbose: false, unique: false) && ExtractUniqueFilePaths(filenames, out directories))
            {
                var new_folder = "(undefined path)";
                try
                {
                    if (IsVerbose) { Console.WriteLine(); WriteLine(ConsoleColor.Green, "Creating temp folders..."); }
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
                            if (IsVerbose) { Write(ConsoleColor.Green, " +"); Console.Write("[{0}] ", folders_cnt); Write(ConsoleColor.Green, "Created "); Console.Write("folder: {0}", path); WriteLine(ConsoleColor.Green, $@"\{temp_foldername}"); }
                        }
                    }

                    // Log to disk
                    if (IsLogFiles)
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
                        LogDirectoryNames(ghx_directories, StartFolder + logfilename);
                        // Log filenames to file
                        LogFileNames(filenames, StartFolder + LOGFILENAME_GH);
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
        private int RemoveGhxTempFolders(string temp_folder)
        {
            List<string> directories = null;
            List<string> filenames = null;
            if (GetGhFileNames(true, out filenames, false) && ExtractUniqueFilePaths(filenames, out directories))
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
                            if (IsVerbose) { Write(ConsoleColor.DarkRed, " -"); Console.Write("[{0}] ", folders_cnt); Write(ConsoleColor.DarkRed, "Removed"); Console.Write(" folder: {0}", path); WriteLine(ConsoleColor.DarkRed, $@"\{temp_folder}"); }
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

        /* ****
                                // split and remove last part as to write that part in other color
                        var short_path = "";
                        var path_arr = GetShrinkedPath(path, out short_path);

                        // Pick color depending on type of trailing temp-folder (ghx or gzip)
                        var temp_folder = path_arr[path_arr.Length - 1];
                        var print_char = "";
                        ConsoleColor color = ConsoleColor.White;
                        if (temp_folder == FOLDERNAME_GHX_TMP)
                        {
                            print_char = "x";
                            color = Theme.ColTmpFolderGhx;
                        }
                        else if (temp_folder == FOLDERNAME_GZIP_TMP)
                        {
                            print_char = "z";
                            color = Theme.ColTmpFolderGZip;
                        }
                        Write(color, $" {print_char}"); Console.Write("[{0}] ", cnt); Write(Theme.ColFolderNames, short_path); WriteLine(color, temp_folder);


         */

        /// <summary>
        /// Returns a list of filenames with extension .ghx or .ghx.gz in temp folders under 
        /// the startfolder. Optionally sorts the filenames.
        /// </summary>
        private bool GetGhxFileNames(string extension_mask, bool sortnames, out List<string> filenames, bool verbose = false)
        {
            filenames = new List<string>(); // ensure a valid output
            var startpath = "";
            // "fallback" default folder
            if (string.IsNullOrEmpty(StartFolder))
            {
                startpath = System.Reflection.Assembly.GetEntryAssembly().Location;
            }
            else
            {
                startpath = StartFolder;
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
                ghxfiles = Directory.GetFiles(StartFolder, EXTENSION_MASK_GHX, SearchOption.AllDirectories)
                    .Where(file => Regex.IsMatch(file, REGEX_EXTENSION_MASK_GHX)).ToList(); // @"^[^[]*\.ghx$"
            }
            else if (extension_mask == EXTENSION_MASK_GZIP)
            {
                ghxfiles = Directory.GetFiles(StartFolder, EXTENSION_MASK_GZIP, SearchOption.AllDirectories)
                    .Where(file => Regex.IsMatch(file, REGEX_EXTENSION_MASK_GZIP)).ToList(); //@"^[^[]*\.ghx.gz$"
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

                    // split and remove last part as to write that part in other color
                    var short_path = "";
                    var path_arr = GetShrinkedPath(path, out short_path);

                    // Pick color depending on type of trailing temp-folder (ghx or gzip)
                    var print_char = " ";
                    ConsoleColor color = Theme.ColFolderNames;
                    var temp_folder = path_arr[path_arr.Length - 1];
                    if (temp_folder == FOLDERNAME_GHX_TMP)
                    {
                        print_char = "x";
                        color = Theme.ColTmpFolderGhx;
                    }
                    else if (temp_folder == FOLDERNAME_GZIP_TMP)
                    {
                        print_char = "z";
                        color = Theme.ColTmpFolderGZip;
                    }
                    Write(color, $" {print_char}"); Write(Theme.ColDefault, $"[{cnt}] "); Write(Theme.ColFolderNames, short_path); Write(color, temp_folder); WriteLine(color, @"\" + file);
                }
            }
            return true;
        }

        private int ClearGhLogfiles()
        {
            var deleted_cnt = 0;
            var log_files = Directory.GetFiles(StartFolder, $"{APPNAME}*.log", SearchOption.AllDirectories);
            // .Where(file => Regex.IsMatch(file, REGEX_ALL_LOGFILE_NAMES)).ToList();
            if (log_files != null && log_files.Length > 0)
            {
                deleted_cnt = log_files.Length;
                var inc = 0;
                foreach (string f in log_files)
                {
                    inc++;
                    if (IsVerbose)
                    {
                        var path = Path.GetDirectoryName(f);
                        var fname = Path.GetFileName(f);
                        if (IsVerbose)
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
        private bool GetGhFileNames(bool sortnames, out List<string> filenames, bool verbose = false, bool unique = true)
        {
            filenames = new List<string>(); // ensure valid output

            var startpath = "";
            // "fallback" default folder
            if (string.IsNullOrEmpty(StartFolder))
            {
                startpath = System.Reflection.Assembly.GetEntryAssembly().Location;
                startpath = Path.GetDirectoryName(startpath);
            }
            else
            {
                startpath = StartFolder;
            }
            if (!Directory.Exists(startpath))
            {
                WriteLine(ConsoleColor.Red, "Path not found: " + startpath);
                return false;
            }

            var ghfiles = Directory.GetFiles(StartFolder, "*.gh", SearchOption.AllDirectories)
                .Where(file => Regex.IsMatch(file, REGEX_EXTENSION_MASK_GH)).ToList();

            // Collect also any ghx files to be used for excluding such .gh files which already has a ghx version
            List<string> ghxfiles = null;
            if (unique) GetGhxFileNames(EXTENSION_MASK_GHX, sortnames, out ghxfiles, verbose); // ghxfiles are used for checking uniqueness of .gh files

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
                    if (verbose)
                    {
                        Console.Write("  [{0}] ", cnt); Write(ConsoleColor.DarkCyan, path); Console.WriteLine("\\{0}", file);
                    }
                    continue; // skips checking for uniqueness below (that is, for twin .gh<-->.ghx filenames)
                }

                // Unique check:
                if (ghxfiles == null || ghxfiles.Count == 0 || !ghxfiles.Contains(filename + "x")) // exclude .ghx files
                {
                    // IsUnique (has no twin .ghx file among the collected filenames):
                    cnt++;
                    filenames.Add(filename);
                    if (verbose) { Console.Write("  [{0}] ", cnt); Write(Theme.ColFolderNames, path); Console.WriteLine("\\{0}", file); }
                }
                else if (verbose) { Console.Write(" -[{0}] Skipped file {1}\\", cnt, path); WriteLine(ConsoleColor.DarkRed, file); }
            }
            if (sortnames) { filenames.Sort(); }
            return true;
        }

        private static void ColorWriteConsoleResultCount(string fmt_message, int cnt, bool suppress_firstline = false, bool suppress_secondline = false)
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

        private static void ColorWriteConsoleResult(string leadstring, ConsoleColor color, string colorstring, bool suppress_firstline = false, bool suppress_secondline = false)
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
        /// Colored Write
        /// </summary>
        private static void Write(ConsoleColor colour = ConsoleColor.White, string line = "")
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.Write(line);
            Console.ForegroundColor = current;
        }

        /// <summary>
        /// Colored WriteLine
        /// </summary>
        private static void WriteLine(ConsoleColor colour = ConsoleColor.White, string line = "")
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.WriteLine(line);
            Console.ForegroundColor = current;
        }

        /*
        // =====================================================================
        // Example code by David Rutten. To be re-implemented in future versions
        // Downloaded from forum post: 
        // https://discourse.mcneel.com/t/get-grasshopper-document-object-count-without-opening-grasshopper/78311/4
        // =====================================================================
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
