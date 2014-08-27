//ottoUtils Help File System
//Author: ottosparks
//I'm a dingus dangus
$ottoUtils::HelpDebug = true;
$ottoUtils::HelpFolder = $ottoUtils::ModPath @ "help/";

function ottoUtils_LoadHelp(%filen, %noreset)
{
	if($ottoUtils::HelpDebug)
	{
		echo("ottoUtils_LoadHelp : Loading help file...");
		echo("   +--file:" SPC %filen);
	}

	if(!isFile(%filen))
	{
		warn("ottoUtils_LoadHelp : Could not find file at \'" SPC %filen SPC "\'");
		return false;
	}

	%file = new FileObject();
	%file.openForRead(%filen);
	%version = %file.readLine();
	if(firstWord(%version) !$= "helpv")
	{
		%file.close();
		%file.delete();
		warn("ottoUtils_LoadHelp : File at \'" SPC %filen SPC "\' is not a valid help file");
		return false;
	}
	%info = restWords(%version);
	%vnum = firstWord(%info);
	%hname = restWords(%info);

	if($ottoUtils::HelpDebug)
	{
		echo("   +-version:" SPC %vnum);
		echo("   +-name:" SPC %hname);
	}

	if($ottoUtils::HelpDebug)
		echo("\nBeginning file read...");

	if(!%noreset)
	{
		if($ottoUtils::HelpDebug)
			echo("Clearing current content...");

		deleteVariables("$ottoUtils::Help::Content" @ %hname @ "*");
	}

	while(!%file.isEOF())
	{
		%line = %file.readLine();
		for(%i = 0; %i < 10; %i++)
			%line = collapseEscape(%line);

		if(%line $= "")
			continue;

		%first = firstWord(%line);
		if(strCmp(%first, "CONTENT") == 0)
		{
			if(%currContent !$= "")
			{
				if($ottoUtils::HelpDebug)
					echo("Cannot nest content! Consider a more clever setup.");
				continue;
			}

			%name = restWords(%line);

			%currContent = %name;
			%currBody = "";

			if($ottoUtils::HelpDebug)
				echo("+NEW CONTENT :" SPC %name);

			continue;
		}
		else if(strCmp(%first, "ALIAS") == 0)
		{

			%name = restWords(%line);

			if(%currContent $= "")
			{
				if($ottoUtils::HelpDebug)
					echo("Cannot add alias \'" SPC %name SPC "\' because there is no current content.");
				continue;
			}

			%currAlias = trim(%currAlias TAB %name);

			if($ottoUtils::HelpDebug)
				echo("+NEW ALIAS FOR" SPC %currContent SPC ":" SPC %name);

			continue;
		}
		else if(strCmp(%first, "END") == 0)
		{
			%wat = restWords(%line);
			if(%wat $= "" || %wat $= "CONTENT")
			{
				$ottoUtils::Help::Content[%hname, %currContent] = %currBody;

				%ct = getFieldCount(%currAlias);
				for(%i = 0; %i < %ct; %i++)
					$ottoUtils::Help::Content[%hname, getField(%currAlias, %i)] = %currBody;

				if($ottoUtils::HelpDebug)
					echo("-END CONTENT:" SPC %currContent);

				%currContent = "";
				%currBody = "";
				%currAlias = "";
			}

			continue;
		}
		else
		{
			if(%currContent $= "")
				continue;

			%currBody = ltrim(%currBody NL ltrim(%line));

			if($ottoUtils::HelpDebug)
				echo(ltrim(%line));
		}
	}

	if(%currContent !$= "")
	{
		$ottoUtils::Help::Content[%hname, %currContent] = %currBody;

		%ct = getFieldCount(%currAlias);
		for(%i = 0; %i < %ct; %i++)
			$ottoUtils::Help::Content[%hname, getField(%currAlias, %i)] = %currBody;

		if($ottoUtils::HelpDebug)
			echo("-?EOF CONTENT:" SPC %currContent);

		%currContent = "";
		%currBody = "";
		%currAlias = "";
	}

	%file.close();
	%file.delete();

	if($ottoUtils::HelpDebug)
		echo("Finished.");

	warn("ottoUtils : Loaded help file \'" SPC %hname SPC "\'");

	return true;
}

function ottoUtils_GetHelpContent(%topic, %content)
{
	return ottoUtils_HelpParseTags($ottoUtils::Help::Content[%topic, %content]);
}

function ottoUtils_HelpParseTags(%str)
{
	%newstr = "";
	while(%str !$= "")
	{
		%str = nextToken(%str, "line", "\n");

		while((%pos = striPos(%line, "@v(|", %off)) != -1)
		{
			%end = striPos(%line, "|)", %pos);
			%off = %end;

			if(%end == -1)
				continue;

			%len = %end - (%pos + 4);
			eval("%repl = " SPC getSubStr(%line, %pos+4, %len) @ "; %s = true;");
			if(!%s)
				continue;

			%from = getSubStr(%line, %pos, %len+6);

			%prelen = strLen(%line);
			%line = strReplace(%line, %from, %repl);
			%postlen = strLen(%line);
			%off -= (%prelen - %postlen);
		}
		%newstr = ltrim(%newstr NL %line);
	}
	return %newstr;
}

function ottoUtils_HelpSubtopic(%str)
{
	return strReplace(%str, " ", ".");
}

function ottoUtils_ActiveHelp()
{
	return true;
}