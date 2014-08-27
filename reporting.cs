//ottoUtils Reporting System
//Author: ottosparks
//System for users to report problem behaviour for admins to review at a later time.
$ottoUtils::Reports = $ottoUtils::Path @ "reports";
$ottoUtils::ReportList = $ottoUtils::Reports @ "/reports.txt";
$ottoUtils::MaxLines = 8;
$ottoUtils::MaxPerPlayer = 16;
$ottoUtils::GenericTitle = "N/A";
$ottoUtils::GenericBody = "No summary.";
$ottoUtils::AutoSaveReports = true;
$ottoUtils::EchoReports = true;

$ottoUtils::ReportError[0] = "\c6Reached max reports per player; consider using \c3/manageReports";
$ottoUtils::ReportError[-1] = "Jerkhole detected";

$ottoUtils::AddError[0] = "Given reporter BL_ID is invalid";
$ottoUtils::AddError[-1] = "Given reported BL_ID is invalid";
$ottoUtils::AddError[-2] = $ottoUtils::ReportError[0];
$ottoUtils::AddError[-3] = $ottoUtils::ReportError[-1];

//0	:	date/time
//1 :	reporting user
//2 :	reported user
//3 :	title
//4 :	summary
$ottoUtils::PatternReportHeader = "\c5Report on \c3$0 \c5by \c3$1\n\c5+--Reported user: \c3$2\n\c5+--Title: \c3$3";
$ottoUtils::PatternReportBody = "$4";
$ottoUtils::BodyPaddingL = "   \c6";
$ottoUtils::BodyPaddingR = "";

function removeThisWord(%sourceString, %searchWord)
{
	for(%i = getWordCount(%sourceString)-1; %i >= 0; %i--)
	{
		%word = getWord(%sourceString, %i);
		if(%word $= %searchWord)
			%sourceString = removeWord(%sourceString, %i);
	}

	return %sourceString;
}

function ottoReportSO_Init(%c)
{
	if(isObject(ottoReportSO))
	{
		if(%c)
			ottoReportSO.cleanImportList($ottoUtils::ReportList);
		return ottoReportSO;
	}

	new ScriptObject(ottoReportSO)
		{
			reports = 0;
			loaded = false;
		};

	ottoReportSO.importList($ottoUtils::ReportList);

	return ottoReportSO;
}

function ottoReportSO::playerCanReport(%this, %bl_id)
{
	if(%this.isJerkhole[%bl_id])
		return -1;
	if($ottoUtils::MaxPerPlayer > 0 && (%this.playerReports[%bl_id] >= $ottoUtils::MaxPerPlayer && !%this.justiceWarrior[%bl_id]))
		return 0;

	return true;
}

function ottoReportSO::addReport(%this, %bl_id, %problemBL_ID, %title, %summary, %date)
{
	if((%id = %this.playerCanReport(%bl_id)) <= 0)
		return -2 + %id;

	if((%bl_id = %bl_id | 0) !$= %bl_id)
		return 0;

	if((%problemBL_ID = %problemBL_ID | 0) !$= %problemBL_ID)
		return -1;

	if(%title $= "")
		%title = $ottoUtils::GenericTitle;

	if(%summary $= "")
		%summary = $ottoUtils::GenericBody;

	%this.reporter[%this.reports] = %bl_id;
	%this.reported[%this.reports] = %problemBL_ID;
	%this.title[%this.reports] = %title;
	%this.summary[%this.reports] = %summary;
	%this.time[%this.reports] = (%date !$= "" ? %date : getDateTime());

	%this.reportList[%problemBL_ID] = trim(%this.reportList[%problemBL_ID] SPC %this.reports);
	%this.reportsMade[%bl_id] = trim(%this.reportsMade[%bl_id] SPC %this.reports);
	%this.playerReports[%bl_id] = getWordCount(%this.reportsMade[%bl_id]);
	%this.timesReported[%problemBL_ID] = getWordCount(%this.reportList[%problemBL_ID]);
	%this.reports++;

	if($ottoUtils::AutoSaveReports)
		%this.saveToDefault();

	return 1;
}

function ottoReportSO::removeReport(%this, %id)
{
	if(%id >= %this.reports)
		return false;

	%bl_id0 = %this.reporter[%id];
	%bl_id1 = %this.reported[%id];

	%this.reporter[%id] = "";
	%this.reported[%id] = "";
	%this.title[%id] = "";
	%this.summary[%id] = "";
	%this.time[%id] = "";

	%this.reportsMade[%bl_id0] = removeThisWord(%this.reportsMade[%bl_id0], %id);
	%this.reportList[%bl_id1] = removeThisWord(%this.reportList[%bl_id1], %id);
	%this.playerReports[%bl_id0] = getWordCount(%this.reportsMade[%bl_id0]);
	%this.timesReported[%bl_id1] = getWordCount(%this.reportList[%bl_id1]);

	for(%i = %id; %i < %this.reports; %i++)
	{
		%j = %i + 1;
		%this.reporter[%i] = %this.reporter[%j];
		%this.reported[%i] = %this.reported[%j];
		%this.title[%i] = %this.title[%j];
		%this.summary[%i] = %this.summary[%j];
		%this.time[%i] = %this.time[%j];
	}

	%this.reports--;
	%this.reporter[%this.reports] = "";
	%this.reported[%this.reports] = "";
	%this.title[%this.reports] = "";
	%this.summary[%this.reports] = "";
	%this.time[%this.reports] = "";

	for(%i = 0; %i < %this.timesReported[%bl_id1]; %i++)
	{
		%lid = getWord(%this.reportList[%bl_id1], %i);

		if(%lid <= %id)
			continue;

		%this.reportList[%bl_id1] = setWord(%this.reportList[%bl_id1], %i, %lid-1);
	}

	for(%i = 0; %i < %this.playerReports[%bl_id0]; %i++)
	{
		%lid = getWord(%this.reportsMade[%bl_id0], %i);

		if(%lid <= %id)
			continue;

		%this.reportsMade[%bl_id0] = setWord(%this.reportsMade[%bl_id0], %i, %lid-1);
	}

	return true;
}

function ottoReportSO::clearReports(%this)
{
	while(%this.reports)
		%this.removeReport(0);
}

function ottoReportSO::exportList(%this, %path)
{
	if(!isWriteableFileName(%path))
		return false;

	%file = new FileObject();
	%file.openForWrite(%path);
	for(%i = 0; %i < %this.reports; %i++)
	{
		%file.writeLine("REPORT" SPC %this.reporter[%i] SPC %this.reported[%i] SPC %this.time[%i] SPC %this.title[%i]);

		%sum = %this.summary[%i];
		while(%sum !$= "")
		{
			%sum = nextToken(%sum, "line", "\n");

			%file.writeLine("\t" @ %line);
		}
		%file.writeLine("END");
	}
	%file.close();
	%file.delete();
	return true;
}

function ottoReportSO::importList(%this, %path)
{
	if(!isFile(%path))
		return 0;

	%file = new FileObject();
	%file.openForRead(%path);

	%ct = 0;
	while(!%file.isEOF())
	{
		%line = %file.readLine();

		%cmd = firstWord(%line);
		if(%cmd $= "REPORT")
		{
			%currBLID0 = getWord(%line, 1);
			%currBLID1 = getWord(%line, 2);
			%currTime = getWords(%line, 3, 4);
			%currTitle = getWords(%line, 5, getWordCount(%line)-1);
			%currSummary = "";
			%currLines = 0;
			%curr = true;
			continue;
		}
		else if(!%curr)
			continue;
		else if(%cmd $= "END" || %currLines >= $ottoUtils::MaxLines)
		{
			%r = %this.addReport(%currBLID0, %currBLID1, %currTitle, %currSummary, %currTime);
			%currBLID0 = "";
			%currBLID1 = "";
			%currTitle = "";
			%currSummary = "";
			%currLines = 0;
			%currTime = "";
			%curr = false;
			if(%r == 1)
				%ct++;
			continue;
		}

		%currSummary = ltrim(%currSummary NL ltrim(%line));
		%currLines++;
	}
	if(%curr)
	{
		%r = %this.addReport(%currBLID0, %currBLID1, %currTitle, %currSummary);
		if(%r == 1)
			%ct++;
	}

	%file.close();
	%file.delete();

	%this.loaded = true;

	return %ct;
}

function ottoReportSO::cleanImportList(%this, %path)
{
	%this.clearReports();
	%this.importList(%path);
}

function ottoReportSO::saveToDefault(%this)
{
	%this.exportList($ottoUtils::ReportList);
}

function ottoReportSO::getReportHeader(%this, %id)
{
	%blid0 = ottoReportSO.reporter[%id];
	%blid1 = ottoReportSO.reported[%id];

	%title = ottoReportSO.title[%id];
	%summ = ottoReportSO.summary[%id];
	%time = ottoReportSO.time[%id];

	return ottoUtils_GetPattern("ReportHeader", %time, %blid0, %blid1, %title, %summ);
}

function ottoReportSO::getReportBody(%this, %id)
{
	%blid0 = ottoReportSO.reporter[%id];
	%blid1 = ottoReportSO.reported[%id];

	%title = ottoReportSO.title[%id];
	%summ = ottoReportSO.summary[%id];
	%time = ottoReportSO.time[%id];

	return ottoUtils_GetPattern("ReportBody", %time, %blid0, %blid1, %title, %summ);
}

function GameConnection::printReport(%this, %id)
{
	if(!isObject(ottoReportSO) || ottoReportSO.reports <= %id)
		return false;

	%header = ottoReportSO.getReportHeader(%id);
	%body = ottoReportSO.getReportBody(%id);

	%this.printLines(%header);
	%this.printLines(%body, $ottoUtils::BodyPaddingL, $ottoUtils::BodyPaddingR);

	return true;
}

function serverCmdstartReport(%this, %target)
{
	if(!isObject(ottoReportSO))
		ottoReportSO_Init();

	if(%this.reportMode)
	{
		messageClient(%this, '', "\c5Could not make a report because: \c0Currently creating a report; say \c3!quit \c0to exit or \c3!done \c0to finish");
		return;
	}

	if(isObject(%tcl = findClientByName(%target)))
		%target = %tcl.getBLID();
	else if(%target < 0 || %target > 999999) //p relaxed detection here uh w/e lol gets obvious cases out of the way
	{
		messageClient(%this, '', "\c5Could not make a report because: \c0Phony BL_ID detected");
		return;
	}

	%id = ottoReportSO.playerCanReport(%this.getBLID());
	if(%id < 0)
	{
		messageClient(%this, '', "\c5Could not make a report because: \c0" @ $ottoUtils::ReportError[%id]);
		return;
	}

	messageClient(%this, '', "\c5Please enter an appropriate report title. Say \c3!quit \c5to exit.");
	%this.reportMode = 1;
	%this.currReportTitle = "";
	%this.currReportBody = "";
	%this.currReportLines = 0;
	%this.currReportTarget = %target;
}

function GameConnection::finishReport(%this)
{
	if(!isObject(ottoReportSO))
		ottoReportSO_Init();

	if(!%this.reportMode)
		return false;

	%blid0 = %this.getBLID();
	%blid1 = %this.currReportTarget;
	%title = %this.currReportTitle;
	%summ = %this.currReportBody;

	%r = ottoReportSO.addReport(%blid0, %blid1, %title, %summ);

	%this.reportMode = 0;

	%this.currReportTitle = "";
	%this.currReportBody = "";
	%this.currReportLines = "";
	%this.currReportTarget = "";

	if(%r > 0)
	{
		messageClient(%this, '', "\c5Successfully submitted report.");

		if(ottoUtils_GetActive("Logging"))
			ottoUtils_LogChat(ottoUtils_GetPattern("ServerMsg", getWord(getDateTime, 1), %this.getPlayerName(), "reported", "BL_ID" SPC %blid1, "(Report ID:" SPC ottoReportSO.reports-1 @ ")"));

		return true;
	}
	else
	{
		messageClient(%this, '', "\c0There was an error in submitting the report:\c0" SPC $ottoUtils::AddError[%r]);
		return false;
	}
}

function serverCmdReport(%this, %cmd, %a0, %a1, %a2, %a3, %a4)
{
	switch$(%cmd)
	{
		case "help":
			if(!ottoUtils_GetActive("Help"))
			{
				messageClient(%this, '', "\c6The help module is not currently active, sorry.");
				return;
			}

			%module = trim(%a0 SPC %a1 SPC %a2 SPC %a3 SPC %a4);
			if(%module $= "")
			{
				%this.printLines(ottoUtils_GetHelpContent("Report", "help"));
				return;
			}
			else
			{
				%content = ottoUtils_GetHelpContent("Report", "help." @ ottoUtils_HelpSubtopic(%module));
				if(%content $= "")
					messageClient(%this, '', "\c6This help topic is empty.");
				else
					%this.printLines(%content);
				return;
			}

		default:
			if(!ottoUtils_GetActive("Help"))
			{
				messageClient(%this, '', "\c6The help module is not currently active, sorry.");
				return;
			}

			%this.printLines(ottoUtils_GetHelpContent("Report", "base"));
	}
}

package ottoUtils_Reporting
{
	function serverCmdStartTalking(%this)
	{
		if(%this.reportMode)
			return;

		parent::serverCmdStartTalking(%this);
	}

	function serverCmdMessageSent(%this, %msg)
	{
		if(%this.reportMode)
		{
			if(getSubStr(%msg, 0, 1) $= "!")
			{
				if(%msg $= "!exit" || %msg $= "!break" || %msg $= "!quit")
				{
					messageClient(%this, '', "\c5Quit creating report");
					%this.reportMode = 0;
					%this.currReportTitle = "";
					%this.currReportBody = "";
					%this.currReportLines = "";
				}
				else if(%msg $= "!done" || %msg $= "!finish" || %msg $= "!submit")
					%this.finishReport();
				else if(%msg $= "!title" || %msg $= "!reset")
				{
					%this.reportMode = 1;
					%this.currReportTitle = "";
					%this.currReportBody = "";
					%this.currReportLines = 0;
					messageClient(%this, '', "\c5Please enter an appropriate report title. Say \c3!quit \c5to exit.");
				}
				else
				{
					messageClient(%this, '', "\c3Available Commands");
					messageClient(%this, '', "\c3!exit\c5, \c3!break\c5, \c3!quit \c5: Discard report and exit");
					messageClient(%this, '', "\c3!done\c5, \c3!finish\c5, \c3!submit \c5: Submit report and exit");
					messageClient(%this, '', "\c3!title\c5, \c3!reset \c5: Restart report");
				}

				return;
			}

			switch(%this.reportMode)
			{
				case 1:
					%this.currReportTitle = %msg;
					messageClient(%this, '', "\c5Title set to \'\c3" @ %msg @ "\c5\'");
					messageClient(%this, '', "\c5Say \c3!title \c5to go back to this step.");
					messageClient(%this, '', "\c5Begin typing a summary of the event. You have \c3" @ $ottoUtils::MaxLines SPC "\c5lines left. Say \c3!done \c5to submit.");
					%this.reportMode = 2;
					%this.currReportLines = 0;
				case 2:
					if((%msg = trim(%msg)) $= "")
						return;

					%this.currReportBody = ltrim(%this.currReportBody NL %msg);
					%num = $ottoUtils::MaxLines - %this.currReportLines++;
					if(%num > 0)
						messageClient(%this, '', "\c5You have \c3" @ %num SPC "\c5line" @ (%num != 1 ? "s" : "") SPC "left. Say \c3!done \c5to submit.");
					else
						%this.finishReport();

			}
			return;
		}

		parent::serverCmdMessageSent(%this, %msg);
	}
};
activatePackage(ottoUtils_Reporting);

schedule(0, 0, ottoReportSO_Init);

function ottoUtils_ActiveReporting()
{
	return isObject(ottoReportSO);
}