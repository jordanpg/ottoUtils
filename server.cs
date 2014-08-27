//ottoUtils Universal Functionality
//Author: ottosparks
//Helper functions and the sort. (also we execute things here)

$ottoUtils::Path = "config/server/ologs/";
$ottoUtils::ModPath = "config/scripts/mod/Server_ottoUtils/";
if(!isFile($ottoUtils::ModPath @ "server.cs"))
	$ottoUtils::ModPath = "Add-Ons/Server_ottoUtils/";

//ottoUtils_GetPattern(%type, %a0, ..., %a7)
//Returns a string in the format of the specified type using $ottoUtils::Pattern[%type].
//%type : Pattern type to use
//%a0-7 : Arguments for replacement. %a0 cooresponds to $0, and so on.
function ottoUtils_GetPattern(%type, %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7)
{
	%str = strReplace($ottoUtils::Pattern[%type], "$0", %a0);
	for(%i = 1; %i < 8; %i++)
		%str = strReplace(%str, "$" @ %i, %a[%i]);

	return %str;
}

//ottoUtils_GetActive(%module)
//Returns true or false if the specified module is active. Assumes false if the ottoUtils_Active* function for that module is missing.
//%module	:	Module to check
function ottoUtils_GetActive(%module)
{
	if(!isFunction(%f = "ottoUtils_Active" @ %module))
		return false;
	return (call(%f) == true);
}

//GameConnection::printLines(%this, %string, %lpad, %rpad)
//Prints each line in a string, separated by newlines, to the given client.
//%this		:	GameConnection object
//%string	:	String to output
//%lpad		:	Left padding on each line
//%rpad		:	Right padding on each line
function GameConnection::printLines(%this, %string, %lpad, %rpad)
{
	while(%string !$= "")
	{
		%string = nextToken(%string, "line", "\n");
		messageClient(%this, '', %lpad @ %line @ %rpad);
	}
}

exec("./logging.cs");
exec("./help.cs");
exec("./reporting.cs");

schedule(0, 0, exec, $ottoUtils::ModPath @ "postload.cs");