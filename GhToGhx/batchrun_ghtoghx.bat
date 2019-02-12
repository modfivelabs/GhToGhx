REM ---------------------------------------------------------------------------------
REM 	Prefix "+" means verbose (console display)
REM 	Prefix "-" means !verbose (no or very little console display)
REM 	Postfix "L" means = Do log to file
REM 	X means Convert to .ghx (xml) format
REM 	x means List ghx files
REM 	Z means Compress to .ghx.gz  (GZip) format
REM 	z means List compressed files
REM 	D means List temp directories for ghx and gzip files
REM ---------------------------------------------------------------------------------
REM
REM - CREATING Xml versions (ghx)
.\ghtoghx.exe "D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\" +XL?
REM - CREATING compressed GZip versions (gzip)
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +ZL
REM - LOG ghx file-lists to disk
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +xL
REM - LOG GZip file-lists to disk
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +zL
REM - LOG Ghx and GZip foldername lists to disk
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +DL
REM - REMOVING ghx folders and files ("L" doesn't play any role here)
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +R
REM - REMOVING GZip folders and files
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +r
REM - CLEAR Log files
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\ +C?
