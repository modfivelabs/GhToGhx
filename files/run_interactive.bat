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

REM Start Interactive. Must provide a strat folder. Pauses with a Prompt
.\ghtoghx.exe D:\DEV\CAD\GH\GH_Workbench\__ScriptComponents\
