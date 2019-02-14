REM ---------------------------------------------------------------------------------
REM 	Prefix "+" means verbose (console display)
REM 	Prefix "-" means !verbose (no or very little console display)
REM 	Postfix "L" means = Do log to file
REM 	g means List existing grasshopper files (.gh)
REM 	X means Convert to .ghx (xml) format
REM 	x means List ghx files
REM 	Z means Compress to .ghx.gz  (GZip) format
REM 	z means List compressed files
REM 	d means List temp directories for ghx and gzip files
REM     .............................................................................
REM	? means pause (prompt ant wait for a key to be pressed
REM 	m Toggle menu visibility (relevant only to the Console Windows mode).
REM ---------------------------------------------------------------------------------

REM - LOG regular Grasshopper files (.gh)
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +gL
REM - CREATING Xml versions (ghx)
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +XL
REM - CREATING compressed GZip versions (gzip)
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +ZL
REM - LOG ghx file-lists to disk
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +xL
REM - LOG GZip file-lists to disk
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +zL
REM - LOG Ghx and GZip foldername lists to disk
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +dL
REM - REMOVING ghx folders and files ("L" doesn't play any role here)
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +R
REM - REMOVING GZip folders and files
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +r
REM - CLEAR Log files 
REM - Also pause (? = prompt) to avid terminating so that a screenshot can be taken
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +C?
