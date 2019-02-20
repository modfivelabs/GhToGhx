/*
+------------------------------------------------------------------------------------+
| Author: RIL, 2019-02-12
| License: MIT, see separate license file LICENSE.md
+------------------------------------------------------------------------------------+
*/

// HISTORY
// 1.1.6: Enhancement: ColorTheme made changeable via CommandLine or console window (pick theme with digit 0, 1, or 2).
// 1.1.5: Refactoring. Dedicated write methods for Folder paths and Filenames. ColorTheme extended. Introduced Xml Documentation with GhostDoc). Fixed isse #4 (scrambled tmep-paths).
// 1.1.4: Heavy refactoring, move entire app content to a separate class. Introduced XUnit Test, and ColorTheme.
// 1.0.3: Added options to hide menu ("m" toggle)and menu-header ("h" toggle). Separated drawing of menu to Draw methods
// 1.0.2: Fixed erroneous log message in command for removing temp folders (_gzip was reported as _ghx)


namespace GhToGhx
{
    using System;
    using ConsoleApp;

    internal class RILGH_GhToGhx
    {
        /// <summary>  MAIN </summary>
        internal static void Main(string[] args)
        {
            try
            {
                //// Set theme
                //if (args != null && args.Length > 1)
                //{
                //    if (args[1].Contains("0"))
                //    {
                //        ConsoleApp.Theme = GhToGhxApp.ColorThemeDefault;
                //        args[1] = args[1].Remove(args[1].IndexOf('0'), 1);
                //    }
                //    else if (args[1].Contains("1"))
                //    {
                //        ConsoleApp.Theme = GhToGhxApp.ColorThemeWhite;
                //        args[1] = args[1].Remove(args[1].IndexOf('1'), 1);
                //    }
                //    else if (args[1].Contains("2"))
                //    {
                //        ConsoleApp.Theme = GhToGhxApp.ColorThemeSiemens;
                //        args[1] = args[1].Remove(args[1].IndexOf('2'), 1);
                //    }

                //    // Remove empty parameter as to enable interactiuve mode (requires that only the first parameter is set)
                //    if (string.IsNullOrEmpty(args[1]))
                //    {
                //        args = new string[1] { args[0] }; // copy first as to truncate the array
                //    }
                //}


                var exitcode = ConsoleApp.Run(args);

                if (exitcode == -1)
                {
                    // ExitCode.Error
                }
                else if (exitcode == 0)
                {
                    // Ok
                }
                else if (exitcode == 1)
                {
                    // Terminate
                }
                return;
            }
            finally
            {
                if (m_consoleapp != null) { m_consoleapp = null; }
            }
        }

        private static GhToGhxApp m_consoleapp;

        /// <summary> Class containing the actual application </summary>
        public static GhToGhxApp ConsoleApp {
            get {
                if (m_consoleapp == null)
                {
                    m_consoleapp = new GhToGhxApp();
                }
                return m_consoleapp;
            }
        }
    }
}
