//------------------------------------------------------------------------------
// Wilderzone Warden - Players tab.
//
// The list comes from the client's own player table (PlayerListGroup). The
// actions offered for the selected player are whatever the server lists in its
// player popup menu for you and that player ('GetPlayerPopupMenu'), so the tab
// shows exactly what your rank allows on this server. Each entry carries the
// numeric id the stock lobby popup switches on; wardenRunAction() sends the
// same command the lobby would for that id.
//------------------------------------------------------------------------------

// Keys well away from the lobby popup's own counter, so the two never match.
$Warden::PopupKey = 7000;
$Warden::ActionCount = 0;

// Popup entry id -> icon, and whether the action asks for a confirming click.
$Warden::ActIcon[1]  = "icon_mute";
$Warden::ActIcon[2]  = "icon_admin";     $Warden::ActConfirm[2]  = true;
$Warden::ActIcon[3]  = "icon_kick";      $Warden::ActConfirm[3]  = true;   $Warden::ActDanger[3]  = true;
$Warden::ActIcon[4]  = "icon_ban";       $Warden::ActConfirm[4]  = true;   $Warden::ActDanger[4]  = true;
$Warden::ActIcon[5]  = "icon_bench";
$Warden::ActIcon[6]  = "icon_team";
$Warden::ActIcon[7]  = "icon_team";
$Warden::ActIcon[8]  = "icon_join";
$Warden::ActIcon[9]  = "icon_voice";
$Warden::ActIcon[10] = "icon_admin";     $Warden::ActConfirm[10] = true;
$Warden::ActIcon[11] = "icon_super";     $Warden::ActConfirm[11] = true;
$Warden::ActIcon[12] = "icon_observe";
$Warden::ActIcon[13] = "icon_warn";
$Warden::ActIcon[14] = "icon_strip";     $Warden::ActConfirm[14] = true;   $Warden::ActDanger[14] = true;
$Warden::ActIcon[15] = "icon_message";
$Warden::ActIcon[16] = "icon_info";
$Warden::ActIcon[17] = "icon_gag";
$Warden::ActIcon[18] = "icon_freeze";
$Warden::ActIcon[19] = "icon_boot";      $Warden::ActConfirm[19] = true;   $Warden::ActDanger[19] = true;
$Warden::ActIcon[20] = "icon_explode";   $Warden::ActConfirm[20] = true;   $Warden::ActDanger[20] = true;

//------------------------------------------------------------------------------
function wardenTeamName( %teamId )
{
   if ( %teamId == 0 )
      return "Observer";
   %name = $clTeamScore[%teamId, 0];
   return %name $= "" ? "-" : %name;
}

function wardenRankTag( %rep )
{
   if ( %rep.isSuperAdmin )
      return "SA";
   if ( %rep.isAdmin )
      return "A";
   if ( %rep.isBot )
      return "B";
   return "";
}

function wardenPlayerRow( %rep )
{
   return wardenRankTag( %rep ) TAB wardenPlainName( %rep.name ) TAB wardenTeamName( %rep.teamId )
         TAB %rep.score TAB %rep.ping TAB %rep.packetLoss;
}

//------------------------------------------------------------------------------
function wardenPlayersRefresh()
{
   if ( !isObject( WdnPlayerList ) )
      return;

   if ( !isObject( PlayerListGroup ) )
   {
      WdnPlayerList.clear();
      wardenSelectPlayer( "" );
      return;
   }

   // Drop rows for clients that left.
   for ( %row = WdnPlayerList.rowCount() - 1; %row >= 0; %row-- )
   {
      %id = WdnPlayerList.getRowId( %row );
      if ( !isObject( $PlayerList[%id] ) )
         WdnPlayerList.removeRowById( %id );
   }

   %added = false;
   %count = PlayerListGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
   {
      %rep = PlayerListGroup.getObject( %i );
      %text = wardenPlayerRow( %rep );
      if ( WdnPlayerList.getRowNumById( %rep.clientId ) == -1 )
      {
         WdnPlayerList.addRow( %rep.clientId, %text );
         %added = true;
      }
      else
         WdnPlayerList.setRowById( %rep.clientId, %text );
   }
   if ( %added )
      WdnPlayerList.sort( 1 );

   if ( $Warden::SelectedClient !$= "" )
   {
      if ( isObject( $PlayerList[$Warden::SelectedClient] ) )
         wardenUpdateCard();
      else
         wardenSelectPlayer( "" );
   }
   else
      wardenUpdateCard();
}

function WdnPlayerList::onSelect( %this, %id, %text )
{
   if ( %id !$= $Warden::SelectedClient )
      wardenSelectPlayer( %id );
}

//------------------------------------------------------------------------------
function wardenSelectPlayer( %clientId )
{
   wardenDisarm();
   $Warden::SelectedClient = %clientId;
   $Warden::ActionCount = 0;
   wardenRenderActions();
   wardenUpdateCard();

   if ( %clientId !$= "" )
      wardenRequestActions();
}

function wardenRequestActions()
{
   if ( $Warden::SelectedClient $= "" || !isObject( ServerConnection ) )
      return;

   $Warden::PopupKey++;
   $Warden::ActionCount = 0;
   commandToServer( 'GetPlayerPopupMenu', $Warden::SelectedClient, $Warden::PopupKey );
   // The reply is a burst of messages with no terminator; draw once it settles.
   cancel( $Warden::ActionRenderEvent );
   $Warden::ActionRenderEvent = schedule( 400, 0, wardenRenderActions );
}

function wardenUpdateCard()
{
   if ( !isObject( WdnCardName ) )
      return;

   %rep = $Warden::SelectedClient $= "" ? 0 : $PlayerList[$Warden::SelectedClient];
   if ( !isObject( %rep ) )
   {
      WdnCardName.setText( "No player selected" );
      WdnCardSub.setText( "Pick a player on the left." );
      WdnCardSub2.setText( "" );
      return;
   }

   if ( %rep.isSuperAdmin )
      %rank = "Super Admin";
   else if ( %rep.isAdmin )
      %rank = "Admin";
   else if ( %rep.isBot )
      %rank = "Bot";
   else
      %rank = "Player";

   // Trailing space: see wdnCaption().
   WdnCardName.setText( wardenPlainName( %rep.name ) @ " " );
   WdnCardSub.setText( %rank @ "  -  " @ wardenTeamName( %rep.teamId ) );
   %line = "Ping " @ %rep.ping @ "   PL " @ %rep.packetLoss @ "   Score " @ %rep.score;
   if ( %rep.guid > 0 )
      %line = %line @ "   GUID " @ %rep.guid;
   WdnCardSub2.setText( %line );
}

//------------------------------------------------------------------------------
// One entry of the server's popup menu for the selected player.
function wardenOnPopupItem( %msgType, %msgString, %key, %voteName, %voteActionMsg, %voteText, %popupEntryId )
{
   if ( %key != $Warden::PopupKey )
      return;
   if ( $Warden::ActionCount >= $Warden::ActionSlots )
      return;

   %n = $Warden::ActionCount;
   $Warden::ActionId[%n] = %popupEntryId;
   $Warden::ActionText[%n] = detag( %voteText );
   $Warden::ActionCount++;

   cancel( $Warden::ActionRenderEvent );
   $Warden::ActionRenderEvent = schedule( 120, 0, wardenRenderActions );
}

// The server's wording, shortened where it would not fit a button.
function wardenActionLabel( %text )
{
   switch$ ( %text )
   {
      case "Add to Server Admin List":
         %text = "Add to admin list";
      case "Add to Server SuperAdmin List":
         %text = "Add to super list";
      case "Send Private Message":
         %text = "Private message";
      case "Disable Voice Com":
         %text = "Voice com off";
      case "Enable Voice Com":
         %text = "Voice com on";
      case "Mute Text Chat":
         %text = "Mute chat";
      case "Unmute Text Chat":
         %text = "Unmute chat";
   }
   return strupr( %text );
}

function wardenRenderActions()
{
   if ( !isObject( WdnCard ) )
      return;

   for ( %i = 0; %i < $Warden::ActionSlots; %i++ )
   {
      %btn = "WdnActBtn" @ %i;
      %icon = "WdnActIcon" @ %i;
      if ( %i < $Warden::ActionCount )
      {
         %id = $Warden::ActionId[%i];
         %bitmap = $Warden::ActIcon[%id] $= "" ? "icon_generic" : $Warden::ActIcon[%id];
         if ( $Warden::ActDanger[%id] )
            wdnSetStyle( %btn, WdnBtnDangerProfile );
         else
            wdnSetStyle( %btn, WdnBtnProfile );
         wdnSetCaption( %btn, wardenActionLabel( $Warden::ActionText[%i] ) );
         %icon.setBitmap( "gui/warden/" @ %bitmap );
         wdnShowButton( %btn, true );
         %icon.setVisible( $Warden::ActionIcons );
      }
      else
      {
         wdnShowButton( %btn, false );
         %icon.setVisible( false );
      }
   }

   if ( $Warden::SelectedClient $= "" )
      WdnCardEmpty.setText( "" );
   else if ( $Warden::ActionCount == 0 )
      WdnCardEmpty.setText( "The server offers you no actions here." );
   else
      WdnCardEmpty.setText( "" );
}

//------------------------------------------------------------------------------
function wardenActionClick( %slot )
{
   if ( %slot >= $Warden::ActionCount || $Warden::SelectedClient $= "" )
      return;

   %id = $Warden::ActionId[%slot];
   %btn = "WdnActBtn" @ %slot;
   if ( $Warden::ActConfirm[%id] )
   {
      %label = wardenActionLabel( $Warden::ActionText[%slot] );
      %profile = $Warden::ActDanger[%id] ? WdnBtnDangerProfile : WdnBtnProfile;
      if ( !wardenArm( %btn, "CONFIRM?", %label, %profile ) )
         return;
   }

   wardenRunAction( %id, $Warden::SelectedClient );

   // Entries such as Gag/UnGag flip after use; ask for the menu again.
   cancel( $Warden::ActionRequestEvent );
   $Warden::ActionRequestEvent = schedule( 450, 0, wardenRequestActions );
}

// The ids are the ones DefaultGame::sendGamePlayerPopupMenu attaches to its
// entries: 1-11 are stock Tribes 2, 12-20 are Classic's additions.
function wardenRunAction( %id, %client )
{
   %rep = $PlayerList[%client];
   switch ( %id )
   {
      case 1:
         commandToServer( 'togglePlayerMute', %client );
      case 2:
         startNewVote( "VoteAdminPlayer", %client, 0, 0, 0, true );
      case 3:
         startNewVote( "VoteKickPlayer", %client, 0, 0, 0, true );
      case 4:
         startNewVote( "BanPlayer", %client, 0, 0, 0, true );
      case 5:
         commandToServer( 'forcePlayerToObserver', %client );
      case 6:
         commandToServer( 'changePlayersTeam', %client, 1 );
      case 7:
         commandToServer( 'changePlayersTeam', %client, 2 );
      case 8:
         commandToServer( 'clientAddToGame', %client );
      case 9:
         commandToServer( 'ListenTo', %client, !%rep.voiceEnabled, true );
      case 10:
         commandToServer( 'AddToAdminList', %client );
      case 11:
         commandToServer( 'AddToSuperAdminList', %client );
      case 12:
         commandToServer( 'ProcessGameLink', %client );
      case 13:
         commandToServer( 'WarnPlayer', %client );
      case 14:
         commandToServer( 'StripAdmin', %client );
      case 15:
         WdnPmEdit.makeFirstResponder( true );
         wardenSetStatus( "Type the private message below the list and press Enter." );
      case 16:
         commandToServer( 'PrintClientInfo', %client );
      case 17:
         commandToServer( 'TogglePlayerGag', %client );
      case 18:
         commandToServer( 'TogglePlayerFreeze', %client );
      case 19:
         commandToServer( 'BootToTheRear', %client );
      case 20:
         commandToServer( 'ExplodePlayer', %client );
      default:
         wardenSetStatus( "This server's action #" @ %id @ " is not one Warden knows how to send." );
   }
}

//------------------------------------------------------------------------------
function wardenSendPM()
{
   %text = WdnPmEdit.getValue();
   if ( %text $= "" )
      return;

   if ( $Warden::SelectedClient $= "" || !isObject( $PlayerList[$Warden::SelectedClient] ) )
   {
      wardenSetStatus( "Select the player to message first." );
      return;
   }

   commandToServer( 'PrivateMessageSent', $Warden::SelectedClient, %text );
   wardenLogAdd( "PM to " @ wardenPlainName( $PlayerList[$Warden::SelectedClient].name ) @ ": " @ %text );
   WdnPmEdit.setValue( "" );
}

//------------------------------------------------------------------------------
// Rank changes arrive as messages; the stock handlers update the player table,
// these refresh what is on screen.
function wardenOnRankChange( %msgType, %msgString, %a1, %a2, %a3 )
{
   if ( !$Warden::Open )
      return;
   wardenUpdateHeader();
   if ( $Warden::Tab == 0 )
   {
      wardenPlayersRefresh();
      wardenRequestActions();
   }
}

addMessageCallback( 'MsgPlayerPopupItem', wardenOnPopupItem );
addMessageCallback( 'MsgAdminPlayer', wardenOnRankChange );
addMessageCallback( 'MsgAdminAdminPlayer', wardenOnRankChange );
addMessageCallback( 'MsgSuperAdminPlayer', wardenOnRankChange );
addMessageCallback( 'MsgStripAdminPlayer', wardenOnRankChange );
