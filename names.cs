//Name Database
//Author: Armageddon
//Stores history of names for players

$ottoUtils::Names::File = $ottoUtils::Path @ "names/nameDatabase.cs";

package nameDatabasePackage
{
	function GameConnection::autoAdminCheck(%this)
	{
		parent::autoAdminCheck(%this);
		
		if($ottoUtils::Names::Data[%this.bl_id] $= "")
		{
			$ottoUtils::Names::Data[%this.bl_id] = %this.getPlayerName();
			return;
		}
		
		%fieldCount = getFieldCount($ottoUtils::Names::Data[%this.bl_id]);
		for(%q = 0; %q < %fieldCount; %q++)
		{
			%f = getField($ottoUtils::Names::Data[%this.bl_id], %q);

			if(%this.getPlayerName() $= %f) return;
		}
		$ottoUtils::Names::Data[%this.bl_id] = $ottoUtils::Names::Data[%this.bl_id] TAB %this.getPlayerName();
		
			%c = clientGroup.getCount();
			for(%i = 0; %i < %c; %i++)
			{
				%client = clientGroup.getObject(%i);
				if(%client.isAdmin)
				{
				messageClient(%client,'',"\c6New name \c7[\c3" @ %this.getPlayerName() @ "\c7]\c6 has been added to list\c2" SPC %this.bl_id);
				}
			}	
			export("$ottoUtils::Names::Data*", $ottoUtils::Names::File);
	}
};
activatePackage(nameDatabasePackage);


function serverCmdList(%client, %nameID)
{
	if(%nameID $= "") return;

	%target = findClientByBL_ID(%nameID)?findClientByBL_ID(%nameID):findClientByName(%nameID);

	if((%nameID * 1) > 0)
	{
		if($ottoUtils::Names::Data[%nameID] $= "")
		{
			messageClient(%client,'',"\c6BL_ID does not exist in the database");
			return;
		}

		%nameValue = 0;
		while(%nameValue != getFieldCount($ottoUtils::Names::Data[%nameID]))
		{
			%name = getField($ottoUtils::Names::Data[%nameID], %nameValue);
			if(%names $= "")
			{
				%names = %name;
			}
			else
			{
			%names = %names SPC "\c6|\c3" SPC %name;
			}
			%nameValue++;
		}
		%nameList = %names;
		messageClient(%client,'',"\c6Name list \c7[\c2" @ %nameID @ "\c7]\c6:\c3" SPC %nameList);
		return;
	}

	if(!isObject(%target)) return;

	%nameValue = 0;
	while(%nameValue != getFieldCount($ottoUtils::Names::Data[%target.bl_id]))
	{
		%name = getField($ottoUtils::Names::Data[%target.bl_id], %nameValue);
		if(%names $= "")
		{
			%names = %name;
		}
		else
		{
			%names = %names SPC "\c6|\c3" SPC %name;
		}
		%nameValue++;
	}
	%nameList = %names;
	messageClient(%client,'',"\c6Name list \c7[\c2" @ %target.bl_id @ "\c7]\c6:\c3" SPC %nameList);
}

function servercmdlisttotal(%client)
{
	%file = new FileObject();
	%file.openForRead($ottoUtils::Names::File);
	while(!%file.isEOF())
	{
		%line = %file.readLine();
		if(%line !$= "")
		{
			%count++;
		}
	}
	%total = %count;
	%file.close();
	%file.delete();
	messageClient(%client,'',"\c3Total players in database\c2" SPC %total);
}

exec($ottoUtils::Names::File);