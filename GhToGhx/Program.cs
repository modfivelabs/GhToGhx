
namespace ghtoghx
{
    using GH_IO.Serialization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class Program
    {
        const string TEMP_FOLDER = "\\_ghx_tmp";

        internal static void Main(string[] args)
        {
            bool do_log = true;
            bool unique = true;

            string startpath = string.Empty;
            string filename = string.Empty;
            string extension = string.Empty;

            string operation = "";

            if (args.Length == 0)
            {
                Console.WriteLine("-----------------------------------------------");
                WriteLine(ConsoleColor.DarkRed, "    Missing command line option. Please enter\n"
                + "    a path or a filename and try again.\n"
                + "    Press any key to return to the prompt.");
                Console.WriteLine("-----------------------------------------------");
                Console.ReadKey();
                return;
            }

            if (args.Length > 0)
            {
                var temp_path = args[0];
                // ensure that only the path is assigned to this variable
                startpath = Path.GetDirectoryName(temp_path) +"\\";
                var tmp_name = Path.GetFileName(temp_path);
                extension = Path.GetExtension(tmp_name);
                if (!string.IsNullOrEmpty(extension)){ filename = tmp_name; }
            }

            var cnt = 0;
            // ------------
            // command line
            // ------------
            if (args.Length > 1)
            {
                operation = args[1];
                //if (!(operation == "X" || operation == "H" || operation == "C" || operation == "R")) { return; }
                switch (operation)
                {
                    case "D":
                        cnt = ListGhxTempFolders(startpath, do_log);
                        LogResult("Found {0} \\_ghx_tmp folders", cnt, true);
                        break;

                    case "G":
                        cnt = ListGhFilenames(startpath, do_log, !unique); // must NOT be single-unique (meaning, no ghx-version)
                        LogResult("Found {0} .gh-files", cnt, true);
                        break;

                    case "X":
                        cnt = ListGhxFilenames(startpath, do_log);
                        LogResult("Found {0} .ghx-files", cnt, true);
                        break;

                    case "C":
                        //cnt = CreateGhxTempFolders(startpath, do_log);
                        cnt = CreateTempGhxFiles(startpath, do_log);
                        LogResult("Created {0} files in sub folders to the gh file: '\\_ghx_tmp'", cnt, true);
                        break;

                    case "R":
                        cnt = RemoveGhxTempFolders(startpath, do_log);
                        LogResult("Removed {0} '\\_ghx_tmp' folders", cnt, true);
                        break;

                    default:
                        Console.WriteLine("-----------------------------------------------");
                        Console.WriteLine("Unknown command (option: '{0}')", operation);
                        Console.WriteLine("-----------------------------------------------");
                        break;
                }
            }
            else
            // ------------------
            // manual interaction
            // ------------------
            {
                Console.WriteLine();
                while (true)
                {
                    
                    WriteLine(ConsoleColor.White, "----------------------------------------------------------------------------");
                    WriteLine(ConsoleColor.White, $"Path '{startpath}' ");
                    if (!string.IsNullOrEmpty(filename)) WriteLine(ConsoleColor.White, $"File '{filename}' ");
                    WriteLine(ConsoleColor.White, "----------------------------------------------------------------------------");
                    WriteLine(ConsoleColor.DarkCyan, "   G = List *.gh files");
                    WriteLine(ConsoleColor.DarkCyan, "   X = List *.ghx files");
                    WriteLine(ConsoleColor.DarkCyan, "   D = List _ghx_tmp folders");
                    WriteLine(ConsoleColor.Green, "   C = Create temp .ghx versions (_ghx_tmp folders)");
                    WriteLine(ConsoleColor.DarkRed, "   R = Remove temp .ghx versions (_ghx_tmp folders)");
                    WriteLine(ConsoleColor.Yellow, "   Q = Quit");
                    WriteLine(ConsoleColor.White, "----------------------------------------------------------------------------");
                    WriteLine(ConsoleColor.White, $" - What operation do you want to perform?");

                    var key = Console.ReadKey(true);
                    WriteLine(ConsoleColor.Red, key.Key.ToString());
                    Console.WriteLine();

                    switch (key.Key)
                    {
                        case ConsoleKey.Escape:
                        case ConsoleKey.Q:
                            return;

                        case ConsoleKey.D:
                            cnt = ListGhxTempFolders(startpath, do_log);
                            LogResult("Found {0} \\_ghx_tmp folders", cnt, true);
                            break;

                        case ConsoleKey.G:
                            cnt = ListGhFilenames(startpath, do_log, !unique); // must NOT be single-unique (meaning, no ghx-version)
                            LogResult("Found {0} .gh-files", cnt, true);
                            break;

                        case ConsoleKey.X:
                            cnt = ListGhxFilenames(startpath, do_log);
                            LogResult("Found {0} .ghx-files", cnt, true);
                            break;

                        case ConsoleKey.C:
                            //cnt = CreateGhxTempFolders(startpath, do_log);
                            cnt = CreateTempGhxFiles(startpath, do_log);
                            LogResult("Created {0} files in sub folders to the gh file: '\\_ghx_tmp'", cnt, true);
                            break;

                        case ConsoleKey.R:
                            cnt = RemoveGhxTempFolders(startpath, do_log);
                            LogResult("Removed {0} '\\_ghx_tmp' folders", cnt, true);
                            break;
                    }
                }
            }
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

        private static int LogFileNames(List<string> filenames, string filepath)
        {
            using (TextWriter tw = new StreamWriter(filepath))
            {
                foreach (string fname in filenames) { tw.WriteLine(fname); }
            }
            return filenames.Count();
        }

        private static int LogDirectoryNames(List<string> directories, string filepath)
        {
            using (TextWriter tw = new StreamWriter(filepath))
            {
                foreach (string dirpath in directories) { tw.WriteLine(dirpath); }
            }
            return directories.Count();
        }

        private static void LogResult(string fmt_message, int cnt, bool do_log = true, bool suppress_line = false)
        {
            if (!do_log)
                return;
            if (!suppress_line) Console.WriteLine("------------------------------------");
            Console.WriteLine(fmt_message, cnt);
            if (!suppress_line) Console.WriteLine("------------------------------------");
        }

        private static int ListGhFilenames(string startpath, bool do_log, bool unique)
        {
            List<string> filenames = null;
            if (GetGhFileNames(startpath, true, out filenames, do_log, unique))
            {
                LogFileNames(filenames, startpath + "\\ghtoghx_gh_namelist.log");
                return filenames.Count;
            }
            return 0;
        }

        private static int ListGhxFilenames(string startpath, bool do_log)
        {
            List<string> filenames = null;
            if (GetGhxFileNames(startpath, true, out filenames, do_log))
            {
                LogFileNames(filenames, startpath + "\\ghtoghx_ghx_namelist.log");
                return filenames.Count;
            }
            return 0;
        }

        private static int ListGhxTempFolders(string startpath, bool do_log)
        {
            List<string> directories;
            var tmp = Directory.GetDirectories(startpath, "*", SearchOption.AllDirectories).Where(path => Regex.IsMatch(path, @"^.*\\_ghx_tmp$"));
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
                LogFileNames(directories, startpath + "\\ghtoghx_ghx_tmpfolders.log");
                if (do_log)
                {
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
                        // UI log
                        if (do_log)
                        {
                            Write(ConsoleColor.DarkCyan, " x");  Console.Write("[{0}] ", inc); Write(ConsoleColor.DarkCyan, short_path); Console.WriteLine(path_arr[path_arr.Length - 1]);
                        }
                    }
                }
                return inc;
            }
            catch (Exception e)
            {
                Console.WriteLine("Attempt to list _ghx_tmp directory paths failed: '{1}'", ghx_path, e.Message);
            }
            return 0;
        }

        private static int CreateTempGhxFiles(string startpath, bool do_log)
        {
            string filename = "(Unknown)";
            try
            {
                CreateGhxTempFolders(startpath, do_log);
                List<string> filenames = null;
                if (GetGhFileNames(startpath, true, out filenames, !do_log))
                {
                    if (do_log)
                    {
                        Console.WriteLine(" "); WriteLine(ConsoleColor.Green, "Converting...");
                    }

                        var inc = 0;
                    var ghx_path = "";
                    for (var i = 0; i < filenames.Count; i++)
                    {
                        filename = filenames[i];
                        if (!File.Exists(filenames[i]))
                        {
                            Console.WriteLine("The .gh file doesn't exist; {0}", filenames[i]);
                            continue;
                        }
                        // ...........................................
                        // create the new path to the new filenames[i].ghx
                        // ...........................................
                        // log the path only once
                        if (do_log && ghx_path != Path.GetDirectoryName(filenames[i]))
                        {
                            ghx_path = Path.GetDirectoryName(filenames[i]);
                            Console.Write("----- " + ghx_path + "\\"); WriteLine(ConsoleColor.Green, "_ghx_tmp");
                            //Console.Write("[{0}] ", i); Write(ConsoleColor.Green, "Converting path: "); WriteLine(ConsoleColor.Green, ghx_path); Console.WriteLine("\\_ghx_tmp");
                        }
                        else
                            ghx_path = Path.GetDirectoryName(filenames[i]);

                        var ghx_filename = Path.GetFileName(filenames[i]);
                        ghx_filename += "x";
                        var ghx_filepath = ghx_path + "\\_ghx_tmp\\" + ghx_filename;

                        var archive = new GH_Archive();
                        if (archive.ReadFromFile(filenames[i]))
                        {
                            inc++;
                            // logging
                            if (do_log) { Write(ConsoleColor.Green, " >");  Console.Write("[{0}] ", inc); Write(ConsoleColor.Green, "  Gh->ghx: "); Console.WriteLine(ghx_filename + " ..."); }
                            // extract plain xml/text
                            var ghx_xml = archive.Serialize_Xml();

                            // write to disk
                            using (TextWriter ghx_writer = new StreamWriter(ghx_filepath))
                            {
                                ghx_writer.WriteLine(ghx_xml);
                            }
                            
                        }
                    }
                    return inc;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read file {0}: '{1}'", filename, e.Message);
            }
            return 0;
        }

        private static int CreateGhxTempFolders(string startpath, bool do_log)
        {
            List<string> directories = null;
            List<string> filenames = null;
            if (GetGhFileNames(startpath, true, out filenames, !do_log) && ExtractUniqueFilePaths(filenames, out directories))
            {
                if (do_log)
                {
                    LogFileNames(filenames, startpath + "\\ghtoghx_gh_namelist.log");
                    LogDirectoryNames(directories, startpath + "\\ghtoghx_gh_dirlist.log");
                }
                var newpath = "(undefined path)";
                try
                {
                    directories.Sort();
                    var cnt = 0;
                    foreach (var path in directories)
                    {
                        newpath = path + TEMP_FOLDER;
                        // checking exists only to count how many directories are actually being created
                        if (!Directory.Exists(newpath))
                        {
                            cnt++;
                            Directory.CreateDirectory(newpath);
                            if (do_log)
                            {
                                Write(ConsoleColor.Green, " +"); Console.Write("[{0}] ", cnt); Write(ConsoleColor.Green, "Created "); Console.Write("temp folder: {0}", path); WriteLine(ConsoleColor.Green, TEMP_FOLDER);
                            }
                        }
                    }
                    return cnt;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Attempt to create temp path ({0}) failed: '{1}'", newpath, e.Message);
                }
            }
            return 0;
        }

        private static int RemoveGhxTempFolders(string startpath, bool do_log)
        {
            List<string> directories = null;
            List<string> filenames = null;
            if (GetGhFileNames(startpath, true, out filenames, !do_log) && ExtractUniqueFilePaths(filenames, out directories))
            {
                // remove "...\_ghx_tmp" directories
                try
                {
                    var cnt = 0;
                    foreach (var path in directories)
                    {
                        // recursive - deletes also sub directories
                        var temp_path = path + TEMP_FOLDER;
                        if (Directory.Exists(temp_path))
                        {
                            cnt++;
                            Directory.Delete(temp_path, true);
                            if (do_log)
                            {
                                Write(ConsoleColor.DarkRed, " -"); Console.Write("[{0}] ", cnt); Write(ConsoleColor.DarkRed, "Removed "); Console.Write("temp folder: {0}", path); WriteLine(ConsoleColor.DarkRed, TEMP_FOLDER);
                            }
                        }
                    }
                    return cnt;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Attempt to delete path failed: '{0}'", e.Message);
                }
            }
            return 0;
        }

        private static bool ExtractUniqueFilePaths(List<string> filenames, out List<string> directories)
        {
            directories = new List<string>(); // ensure a valid output
            if (filenames == null || filenames.Count == 0)
            {
                return false;
            }
            foreach (var fullpath in filenames)
            {
                var path = Path.GetDirectoryName(fullpath);
                if (!directories.Contains(path)) // collect only unique paths
                {
                    directories.Add(path);
                }
            }

            return true;
        }

        private static bool GetGhxFileNames(string startfolder, bool sortnames, out List<string> filenames, bool log = false)
        {
            filenames = new List<string>(); // ensure a valid output
            var startpath = "";
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
            // Scan for file with extension ".ghx", excluding copies containing [...] in their file names
            var ghxfiles = Directory.GetFiles(startpath, "*.ghx", SearchOption.AllDirectories).Where(file => Regex.IsMatch(file, @"^[^[]*\.ghx$")).ToList();
            if (ghxfiles == null || ghxfiles.Count() == 0)
            {
                return false;
            }

            // sort result
            filenames.AddRange(ghxfiles);
            if (sortnames) { ghxfiles.Sort(); }

            if (log) // TODO: DEBUG
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

        private static bool GetGhFileNames(string startfolder, bool sortnames, out List<string> filenames, bool log = false, bool unique = true)
        {
            filenames = new List<string>(); // ensure a valid output

            var startpath = "";
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

            // Collect also ghx files, to be used for excluding gh-files which already has a ghx version
            var ghfiles = Directory.GetFiles(startpath, "*.gh", SearchOption.AllDirectories).Where(file => Regex.IsMatch(file, @"^[^[]*\.gh$")).ToList();
            //var ghxfiles = Directory.GetFiles(startpath, "*.ghx", SearchOption.AllDirectories).Where(file => Regex.IsMatch(file, @"^[^[]*\.ghx$")).ToList();

            List<string> ghxfiles = null;
            if (unique) GetGhxFileNames(startfolder, sortnames, out ghxfiles, log);

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
                    if (log)
                    {
                        Console.Write("  [{0}] ", cnt); Write(ConsoleColor.DarkCyan, path); Console.WriteLine("\\{0}", file);
                    }
                    continue; // skip checking for uniqueness (that is, for existing similar ghx filename below)
                }
                if (ghxfiles == null || ghxfiles.Count == 0 || !ghxfiles.Contains(filename + "x")) // exclude .ghx files
                {
                    cnt++;
                    filenames.Add(filename);
                    if (log)
                    {
                        Console.Write("  [{0}] ", cnt); Write(ConsoleColor.DarkCyan, path); Console.WriteLine("\\{0}", file);
                    }
                }
                else if (log)
                {
                    Console.Write("Skipped file {1}\\", cnt, path); WriteLine(ConsoleColor.DarkRed, file);
                }
            }
            if (sortnames) { filenames.Sort(); }
            return true;
        }

        private static void Write(ConsoleColor colour, string line)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.Write(line);
            Console.ForegroundColor = current;
        }

        private static void WriteLine(ConsoleColor colour, string line)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.WriteLine(line);
            Console.ForegroundColor = current;
        }


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
    }
}
