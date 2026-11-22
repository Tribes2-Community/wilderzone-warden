//------------------------------------------------------------------------------
// Wilderzone Warden - Match tab.
//
// The server decides what is on its vote/admin menu ('GetVoteMenu'); for an
// admin most entries act at once instead of starting a vote. This tab shows
// that menu as a list and drives it with the same commands, in the same order,
// as the stock lobby (lobbyVote in LobbyGui.cs) - servers such as TacoServer
// build multi-step menus that depend on that exact sequence.
//------------------------------------------------------------------------------

// Keys well away from the lobby menu's own counter.
$Warden::VoteKey = 9000;
$Warden::VoteMode = "";
$Warden::VoteCount = 0;

//------------------------------------------------------------------------------
function wardenVoteForget()
{
   $Warden::VoteMode = "";
   $Warden::VoteTourney = false;
   $Warden::VoteCount = 0;
   $Warden::VoteSort = false;
}

function wardenMatchEnter()
{
   WdnMatchMission.setText( $clMissionName );
   WdnMatchType.setText( $clMissionType );
   WdnMatchHelp.setText( "<color:7E8A99>The list is this server's own menu for your rank. Select an entry and press <color:D2DCE8>RUN<color:7E8A99> (or double-click it).\n\n"
         @ "Entries that open a choice - mission, time limit, team - continue in the same list; <color:D2DCE8>BACK<color:7E8A99> returns.\n\n"
         @ "Type in the box under the list to filter long lists such as missions." );
   wardenVoteReset();
}

//------------------------------------------------------------------------------
// Requests. Each one takes a fresh key, so late replies to an older request
// are ignored.

function wardenVoteBegin( %mode )
{
   $Warden::VoteKey++;
   $Warden::VoteMode = %mode;
   $Warden::VoteCount = 0;
   $Warden::VoteSort = false;
   WdnVoteFilter.setValue( "" );
   wardenVoteRender();
}

function wardenVoteFill()
{
   wardenVoteBegin( "" );
   if ( isObject( ServerConnection ) )
      commandToServer( 'GetVoteMenu', $Warden::VoteKey );
}

function wardenVoteReset()
{
   $Warden::VoteTourney = false;
   wardenVoteFill();
}

function wardenVoteFillTeams()
{
   wardenVoteBegin( "team" );
   commandToServer( 'GetTeamList', $Warden::VoteKey );
}

function wardenVoteFillTypes()
{
   wardenVoteBegin( "type" );
   commandToServer( 'GetMissionTypes', $Warden::VoteKey );
}

function wardenVoteFillMissions( %type, %typeName )
{
   wardenVoteBegin( "mission" );
   $Warden::VoteMissionType = %type;
   $Warden::VoteTypeName = %typeName;
   commandToServer( 'GetMissionList', $Warden::VoteKey, %type );
}

function wardenVoteFillTimeLimits()
{
   wardenVoteBegin( "timeLimit" );
   commandToServer( 'GetTimeLimitList', $Warden::VoteKey );
}

function wardenVoteBack()
{
   wardenVoteReset();
}

//------------------------------------------------------------------------------
function wardenOnVoteItem( %msgType, %msgString, %key, %voteName, %voteActionMsg, %voteText, %sort )
{
   if ( %key != $Warden::VoteKey )
      return;

   %n = $Warden::VoteCount;
   $Warden::VoteCmd[%n] = detag( %voteName );
   $Warden::VoteAction[%n] = detag( %voteActionMsg );
   $Warden::VoteText[%n] = detag( %voteText );
   $Warden::VoteCount++;
   if ( %sort )
      $Warden::VoteSort = true;

   // Replies arrive as a burst with no terminator; draw once it settles.
   cancel( $Warden::VoteRenderEvent );
   $Warden::VoteRenderEvent = schedule( 100, 0, wardenVoteRender );
}

function wardenVoteRender()
{
   cancel( $Warden::VoteRenderEvent );
   if ( !isObject( WdnVoteList ) )
      return;

   switch$ ( $Warden::VoteMode )
   {
      case "team":
         %crumb = "SERVER MENU  >  CHOOSE TEAM";
      case "type":
         %crumb = "SERVER MENU  >  GAME TYPE";
      case "mission":
         %crumb = "SERVER MENU  >  " @ strupr( $Warden::VoteTypeName ) @ "  >  MISSION";
      case "timeLimit":
         %crumb = "SERVER MENU  >  TIME LIMIT";
      default:
         %crumb = "SERVER MENU";
   }
   WdnVoteCrumb.setText( %crumb );

   %filter = strlwr( trim( WdnVoteFilter.getValue() ) );
   WdnVoteList.clear();
   for ( %i = 0; %i < $Warden::VoteCount; %i++ )
   {
      %text = $Warden::VoteText[%i];
      if ( %filter !$= "" && strstr( strlwr( %text ), %filter ) == -1 )
         continue;

      WdnVoteList.addRow( %i, %text );
      // An entry with no command is a heading (TacoServer's mission line).
      if ( $Warden::VoteMode $= "" && $Warden::VoteCmd[%i] $= "" )
         WdnVoteList.setRowActive( WdnVoteList.rowCount() - 1, false );
   }
   if ( $Warden::VoteSort )
      WdnVoteList.sort( 0 );
}

function wardenVoteFilterChanged()
{
   wardenVoteRender();
}

//------------------------------------------------------------------------------
// Run the selected entry. Mirrors lobbyVote().
function wardenVoteRun()
{
   %id = WdnVoteList.getSelectedId();
   if ( %id < 0 || %id $= "" || %id >= $Warden::VoteCount )
      return;

   %cmd = $Warden::VoteCmd[%id];
   %text = $Warden::VoteText[%id];

   switch$ ( $Warden::VoteMode )
   {
      case "team":
         commandToServer( 'ClientJoinTeam', %id + 1 );
         wardenVoteReset();
         return;

      case "type":
         wardenVoteFillMissions( %cmd, %text );
         return;

      case "mission":
         if ( $Warden::VoteTourney )
            startNewVote( "VoteTournamentMode", %text, $Warden::VoteTypeName, %cmd, $Warden::VoteMissionType );
         else
            startNewVote( "VoteChangeMission", %text, $Warden::VoteTypeName, %cmd, $Warden::VoteMissionType );
         wardenVoteReset();
         return;

      case "timeLimit":
         startNewVote( "VoteChangeTimeLimit", %cmd );
         wardenVoteReset();
         return;
   }

   if ( %cmd $= "" )
      return;

   switch$ ( %cmd )
   {
      case "JoinGame":
         commandToServer( 'clientJoinGame' );
         wardenVoteFill();
         return;

      case "ChooseTeam":
         commandToServer( 'ClientJoinTeam', -1, true );
         wardenClose();
         return;

      case "VoteTournamentMode":
         $Warden::VoteTourney = true;
         wardenVoteFillTypes();
         return;

      case "VoteMatchStart":
         startNewVote( "VoteMatchStart" );
         wardenVoteFill();
         return;

      case "MakeObserver":
         commandToServer( 'ClientMakeObserver' );
         wardenVoteFill();
         return;

      case "VoteChangeMission":
         wardenVoteFillTypes();
         return;

      case "VoteChangeTimeLimit":
         wardenVoteFillTimeLimits();
         return;

      case "Addbot":
         commandToServer( 'addBot' );
         return;
   }

   startNewVote( %cmd, $Warden::VoteAction[%id] );
   wardenVoteFill();
}

//------------------------------------------------------------------------------
function wardenOnVoteDone( %msgType, %msgString, %voteName, %voteText )
{
   if ( $Warden::Open && $Warden::Tab == 1 && $Warden::VoteMode $= "" )
      wardenVoteFill();
}

addMessageCallback( 'MsgVoteItem', wardenOnVoteItem );
addMessageCallback( 'MsgVotePassed', wardenOnVoteDone );
addMessageCallback( 'MsgVoteFailed', wardenOnVoteDone );
