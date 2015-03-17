# Requirements #
.NET Framework 3.5 or newer (http://www.microsoft.com/downloads/en/details.aspx?FamilyId=333325fd-ae52-4e35-b531-508d977d32a6&displaylang=en)<br>
Note: Windows 7 users do not need to install .NET Framework 3.5, as it is already pre-installed in Windows 7<br>
<br>
<h1>Description</h1>
None yet<br>
<br>
I apologize for not using Subversion as intended, I haven't gotten around to understanding it yet<br>
<br>
<h1>Changelog</h1>
v1.57<br>
<b>TibiaCam</b><br>
Fixed .cam files not showing up in browser<br>
Fixed .cam files not being detected as playable format when launching from file manager<br>
<br>

v1.56<br>
<b>Experience Counter</b><br>
Fixed using Level% causing the program to crash if you did not gain >=1%<br>
<b>TibiaCam</b><br>
Added support for .cam files (created by the new custom client)<br>
<br>

v1.55<br>
<b>TibiaCam</b><br>
Added support for 7.72<br>
<br>

v1.54<br>
<b>TibiaCam</b><br>
Fixed a bug that would cause the client to crash if a packet bigger than 8192 bytes was received<br>
<br>

v1.53<br>
<b>TibiaCam</b><br>
Fixed a bug that prevented playback hotkeys from working<br>
Added text commands<br>
<b>Hotkeys</b><br>
Added gold counter<br>
Made it easier to create new hotkeys<br>
<b>General</b><br>
Fixed not loading some settings properly<br>
<br>

v1.52<br>
<b>TibiaCam</b><br>
Fixed a bug that would cause only the test login server IP to be overwritten<br>
Fixed a bug that prevented rewinding in playing recordings the non-traditional way<br>
<b>Experience Counter</b><br>
Added things that was supposed to be in 1.5<br>
<br>

v1.51<br>
Fixed the program crashing upon startup for some users<br>
<br>

v1.5<br>
<b>TibiaCam</b><br>
Added MoTD<br>
Added playback options<br>
Added recording of mouse movements<br>
Duration is now shown correctly on certain recordings<br>
Now automatically associates .kcam files with Tibianic Tools<br>
<b>Experience Counter</b><br>
Added option to count time TNL based on gained level percent or the experience TNL formula<br>
Added option to estimate experience TNL based on gained experience and gained level percent<br>
Changed experience table to Tibianic's experience table (only up to level 110)<br>
<b>UI</b><br>
Replaced the images for closing, minimizing and hiding to tray<br>
Added context menu (right-click on tray icon)<br>
Fixed client chooser only showing max 2 clients on first dropdown<br>
<br>

v1.44<br>
<b>Source</b><br>
Some more code cleanup (sigh...)<br>
<b>TibiaCam</b><br>
Fixed a critical error introduced in 1.43 that didn't save metadata in new recordings,<br>
which made them unplayable<br>
<br>
v1.43<br>
<b>Source</b>
Added Packet class (..\Objects\Packet.cs)<br>
Added Network class and ThreadSafe class (..\Utils.cs)<br>
Cross-threading errors should now be gone while debugging<br>
More code cleanup<br>
<b>General</b><br>
Disabled visual styles (i.e. Vista and W7 had big ugly white borders on buttons)<br>
<b>TibiaCam</b><br>
Fixed compability with tibianic.org (DNS only)<br>
<br>
v1.42<br>
<b>General</b><br>
Minor code cleanup<br>
Slightly improved memory reading<br>
Will now automatically position itself in the top left window of Tibia when run<br>
Now open source<br>
<b>HUD</b><br>
Added FPS counter for 7.6<br>
<b>TibiaCam</b><br>
Slightly improved playback performance (only users with low-end CPUs will notice any difference)<br>
Made the automatic playback better<br>