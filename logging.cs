//ottoUtils Logging Tools
//Author: ottosparks
//Logs various server actions to files.
$ottoUtils::ChatLogs = $ottoUtils::Path @ "chat";
$ottoUtils::PatternChatLog = "$0/$1/$2/$2 $3, $1.txt";
$ottoUtils::PatternChat = "[$0, $3] $1 : $2 $4";
$ottoUtils::PatternServerMsg= "[$0, $3] $1 $2 $4";
$ottoUtils::LoggerActive = true;

//ottoUtils_LogChat(%line)
//Function to log a line to the file cooresponding to the date
//%line : string to log
function ottoUtils_LogChat(%line)
{
	if(!$ottoUtils::LoggerActive)
		return;

	if(!isObject(ottoLogger))
		new FileObject(ottoLogger);

	%months = "January February March April May June July August September October November December";
	%date = strReplace(firstWord(getDateTime()), "/", " ");
	%month = getWord(%months, getWord(%date, 0)-1);
	%year = "20" @ getWord(%date, 2);
	%day = getWord(%date, 1);
	%str = ottoUtils_GetPattern("ChatLog", $ottoUtils::ChatLogs, %year, %month, %day);

	if(!isWriteableFileName(%str))
	{
		error("ottoUtils_LogChat - Error writing to file \'" SPC %str SPC "\'");
		return false;
	}

	ottoLogger.openForAppend(%str);
	ottoLogger.writeLine(%line);
	ottoLogger.close();
	return true;
}

//getMinuteThing()
//Returns the current minute in the year.
function getMinuteThing()
{
	%datetime = getDateTime();
	%month = getSubStr(%datetime, 0, 2);
	%day = getSubStr(%datetime, 3, 2);
	%time = getWord(%datetime, 1);
	%hours = getSubStr(%time, 0, 2);
	%minutes = getSubStr(%time, 3, 2);
	return (getDayOfYear(%month, %day) * 24 * 60) + (%hours * 60) + %minutes;
}

package ottoUtils_Logging
{
	function serverCmdMessageSent(%this, %msg) //Log chat, probably the biggest one here.
	{
		parent::serverCmdMessageSent(%this, %msg);

		%time = getWord(getDateTime(), 1);
		%name = %this.getPlayerName();
		%bl_id = %this.getBLID();

		ottoUtils_LogChat(ottoUtils_GetPattern("Chat", %time, %name, %msg, %bl_id));		
	}

	function GameConnection::autoAdminCheck(%this) //Log connects.
	{
		%r = parent::autoAdminCheck(%this);

		%time = getWord(getDateTime(), 1);
		%name = %this.getPlayerName();
		%bl_id = %this.getBLID();
		%str = "(autoadmin" SPC %r @ ", ip" SPC %this.getAddress() @ ")";

		ottoUtils_LogChat(ottoUtils_GetPattern("ServerMsg", %time, %name, "connected", %bl_id, %str));

		return %r;
	}

	function GameConnection::onClientEnterGame(%this) //Log spawning players.
	{
		%r = parent::onClientEnterGame(%this);

		%time = getWord(getDateTime(), 1);
		%name = %this.getPlayerName();
		%bl_id = %this.getBLID();

		ottoUtils_LogChat(ottoUtils_GetPattern("ServerMsg", %time, %name, "spawned", %bl_id));

		return %r;
	}

	function GameConnection::onClientLeaveGame(%this) //Log disconnects.
	{
		%time = getWord(getDateTime(), 1);
		%name = %this.getPlayerName();
		%bl_id = %this.getBLID();

		ottoUtils_LogChat(ottoUtils_GetPattern("ServerMsg", %time, %name, "disconnected", %bl_id, "(ip" SPC %this.getAddress() @ ")"));

		return parent::onClientLeaveGame(%this);
	}

	function MinigameSO::onAdd(%this) //Log minigame creation.
	{
		%r = parent::onAdd(%this);

		if(isObject(%this.owner))
		{
			%name = %this.owner.getPlayerName();
			%bl_id = %this.owner.getBLID();
		}
		else
		{
			%name = "Server";
			%bl_id = "N/A";
		}

		%time = getWord(getDateTime(), 1);
		%str = "(" @ %this.title @ ")";

		ottoUtils_LogChat(ottoUtils_GetPattern("ServerMsg", %time, %name, "created a minigame", %bl_id, %str));

		return %r;
	}

	function MinigameSO::addMember(%this, %client) //Log people joining minigames.
	{
		%r = parent::addMember(%this, %client);

		%time = getWord(getDateTime(), 1);
		%name = %client.getPlayerName();
		%bl_id = %client.getBLID();
		%str = "(" @ %this.title @ ")";

		ottoUtils_LogChat(ottoUtils_GetPattern("ServerMsg", %time, %name, "joined a minigame", %bl_id, %str));

		return %r;
	}

	function MinigameSO::removeMember(%this, %client) //Log people leaving minigames.
	{
		%r = parent::addMember(%this, %client);

		%time = getWord(getDateTime(), 1);
		%name = %client.getPlayerName();
		%bl_id = %client.getBLID();
		%str = "(" @ %this.title @ ")";

		ottoUtils_LogChat(ottoUtils_GetPattern("ServerMsg", %time, %name, "left a minigame", %bl_id, %str));

		return %r;
	}

	function serverCmdBan(%client, %victimName, %victimBLID, %time, %reason) //Log bans.
	{
		if(%client.isAdmin || %client.isSuperAdmin)
		{
			%t = getWord(getDateTime(), 1);
			%name = %client.getPlayerName();
			%bl_id = %client.getBLID();
			%msg = "banned" SPC %victimName SPC "(" @ %victimBLID @ ")" SPC "for" SPC %time SPC "minutes";
			%str = "(" @ %reason @ ")";

			ottoUtils_LogChat(ottoUtils_GetPattern("ServerMsg", %t, %name, %msg, %bl_id, %str));
		}

		parent::serverCmdBan(%client, %victimClientID, %victimBLID, %time, %reason);
	}

	function serverCmdKick(%client, %victim, %victimBLID) //Log kicks.
	{
		if(%client.isAdmin || %client.isSuperAdmin)
		{
			%time = getWord(getDateTime(), 1);
			%name = %client.getPlayerName();
			%bl_id = %client.getBLID();
			%msg = "kicked" SPC %victim;
			%str = (%victimBLID !$= "" ? "(" @ %victimBLID @ ")" : "");

			ottoUtils_LogChat(ottoUtils_GetPattern("ServerMsg", %time, %name, %msg, %bl_id, %str));
		}

		parent::serverCmdKick(%client, %victim, %victimBLID);
	}

	function serverCmdUnBan(%client, %banID) //Log unbanning.
	{
		if(%client.isAdmin || %client.isSuperAdmin)
		{
			%time = getWord(getDateTime(), 1);
			%btime = (BanManagerSO.expirationMinute[%banID] >= 0 ? BanManagerSO.expirationMinute[%banID] - getMinuteThing() : BanManagerSO.expirationMinute[%banID]);
			%name = %client.getPlayerName();
			%bl_id = %client.getBLID();
			%msg = "unbanned BL_ID" SPC BanManagerSO.victimBL_ID[%banID] @ (BanManagerSO.victimName[%banID] !$= "" ? " (" @ BanManagerSO.victimName[%banID] @ ")": "");
			%str = "(" @ BanManagerSO.adminName[%banID] @ "/" @ BanManagerSO.adminBL_ID[%banID] @ ",  " SPC %btime @ ";" SPC BanManagerSO.reason[%banID] @ ")";

			ottoUtils_LogChat(ottoUtils_GetPattern("ServerMsg", %time, %name, %msg, %bl_id, %str));
		}
		parent::serverCmdUnBan(%client, %banID);
	}

	function serverCmdTeamMessageSent(%this, %msg) //Log team chatting.
	{
		parent::serverCmdTeamMessageSent(%this, %msg);

		%time = getWord(getDateTime(), 1);
		%name = %this.getPlayerName();
		%bl_id = %this.getBLID();
		%mini = getMinigameFromObject(%this);
		%str = "(" @ %mini @ ";" SPC (isObject(%mini.owner) ? %mini.owner.getPlayerName() SPC "(" @ %mini.owner.getBLID() @ ")": "Server") @ ";" SPC %mini.title @ ")";

		ottoUtils_LogChat(ottoUtils_GetPattern("Chat", %time, %name, %msg, %bl_id, %str));	
	}
};
activatePackage(ottoUtils_Logging);

function serverCmdLogger(%this, %val)
{
	if(!%this.isSuperAdmin)
	{
		messageClient(%this, '', "Only Super Admins can use this command.");
		return;
	}

	switch$(%val)
	{
		//Take care of more 'abstract' cases...
		case "tog": $ottoUtils::LoggerActive = !$ottoUtils::LoggerActive;
		case "toggle": $ottoUtils::LoggerActive = !$ottoUtils::LoggerActive;
		case "t": $ottoUtils::LoggerActive = !$ottoUtils::LoggerActive;
		case "on": $ottoUtils::LoggerActive = true;
		case "off": $ottoUtils::LoggerActive = false;
		
		default:
			//And then handle these the easy way.
			if(%val) //Catches true and 1.
				$ottoUtils::LoggerActive = true;
			else if(%val $= "false" || %val $= "0") //We explicitly state false and 0 because...
				$ottoUtils::LoggerActive = false;
			else //...we wanna assume toggle if they don't provide anything valid.
				$ottoUtils::LoggerActive = !$ottoUtils::LoggerActive;
	}

	messageClient(%this, '', "\c5Logger is turned" SPC ($ottoUtils::LoggerActive ? "\c2on" : "\c0off") @ "\c5.");
}

function ottoUtils_ActiveLogging()
{
	return true; //Always return true, because all we need to know is that the module is /loaded/. We handle if it's on or not here.
}