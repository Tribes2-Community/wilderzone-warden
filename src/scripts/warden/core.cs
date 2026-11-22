//------------------------------------------------------------------------------
// Wilderzone Warden - opening/closing, key binding, tabs, status line and
// the server-message log.
//------------------------------------------------------------------------------

$Warden::Version = "1.1";
$Warden::Tab = 0;
$Warden::Open = false;
$Warden::LogMax = 150;
$Warden::LogCount = 0;
$Warden::LogHead = 0;

//------------------------------------------------------------------------------
// Key binding.
//
// "Wilderzone Warden" is added to the Options > Controls list (moveMap) the
// way the Classic client adds its huds. A second, fixed binding in the global
// map opens the console from the lobby as well; change it with
// $pref::Warden::GlobalKey (set it to "" for none).

if ( $pref::Warden::GlobalKey $= "" && !$pref::Warden::GlobalKeySet )
{
   $pref::Warden::GlobalKey = "ctrl f9";
   $pref::Warden::GlobalKeySet = true;
}

function toggleWardenAdmin( %val )
{
   if ( %val )
      wardenToggle();
}

function wardenBindKeys()
{
   if ( $pref::Warden::GlobalKey !$= "" )
      GlobalActionMap.bind( keyboard, $pref::Warden::GlobalKey, toggleWardenAdmin );
}

package WardenAdmin
{
   function OptionsDlg::onWake( %this )
   {
      if ( !$Warden::RemapAdded )
      {
         $RemapName[$RemapCount] = "Wilderzone Warden";
         $RemapCmd[$RemapCount] = "toggleWardenAdmin";
         $RemapCount++;
         $Warden::RemapAdded = true;
      }
      Parent::onWake( %this );
   }

   // The server confirms a new admin password by sending it back in plain text
   // ('"Admin" PW changed to: ...'). Mask it before the chat hud, or anything
   // else listening, shows it.
   function clientCmdServerMessage( %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 )
   {
      %masked = wardenMaskSecret( detag( %msgString ) );
      if ( %masked !$= "" )
         %msgString = %masked;
      Parent::clientCmdServerMessage( %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
   }

   // Remember which server is being joined: saved logins are kept per server address.
   function JoinGame( %address )
   {
      $Warden::ServerAddress = %address;
      Parent::JoinGame( %address );
   }

   // Leaving the server: forget everything tied to that connection.
   function DisconnectedCleanup()
   {
      if ( $Warden::Open )
         wardenClose();
      wardenResetSession();
      Parent::DisconnectedCleanup();
   }
};
activatePackage( WardenAdmin );

// Returns %text with the password cut out if it is the server's "Admin" or
// "Super Admin" password confirmation, or "" if it is anything else.
function wardenMaskSecret( %text )
{
   %marker = "Admin\" PW changed to:";
   %at = strstr( %text, %marker );
   if ( %at == -1 )
      return "";
   return getSubStr( %text, 0, %at + strlen( %marker ) ) @ " ********";
}

//------------------------------------------------------------------------------
function wardenToggle()
{
   if ( $Warden::Open )
      wardenClose();
   else
      wardenOpen();
}

function wardenOpen()
{
   if ( $Warden::Open )
      return;

   if ( !isObject( ServerConnection ) && !$Warden::AllowOffline )
      return;

   wardenBuildGui();
   Canvas.pushDialog( WardenDlg );
   wardenUpdateAuto();
}

function wardenClose()
{
   if ( isObject( WardenDlg ) )
      Canvas.popDialog( WardenDlg );
}

//------------------------------------------------------------------------------
function WardenDlg::onWake( %this )
{
   $Warden::Open = true;
   wardenLayout();
   // Hide the action buttons that have nothing to offer yet.
   wardenRenderActions();

   if ( isObject( WardenMap ) )
   {
      WardenMap.pop();
      WardenMap.delete();
   }
   new ActionMap( WardenMap );
   WardenMap.bindCmd( keyboard, escape, "", "wardenClose();" );
   WardenMap.push();

   wardenUpdateHeader();
   wardenSetStatus( $Warden::Status );
   wardenSetHint( "ESC closes  -  Wilderzone Warden " @ $Warden::Version );
   wardenSetTab( $Warden::Tab );
   wardenTick();
}

function WardenDlg::onSleep( %this )
{
   $Warden::Open = false;
   cancel( $Warden::TickEvent );

   if ( isObject( WardenMap ) )
   {
      WardenMap.pop();
      WardenMap.delete();
   }

   wardenDisarm();
   wardenPanelRelease();
}

// Once a second while open: keep scores/pings and the header current.
function wardenTick()
{
   cancel( $Warden::TickEvent );
   if ( !$Warden::Open )
      return;

   $Warden::TickCount++;
   if ( $Warden::Tab == 0 )
   {
      if ( $Warden::TickCount % 4 == 1 && isObject( ServerConnection ) )
         commandToServer( 'getScores' );
      wardenPlayersRefresh();
   }
   wardenUpdateHeader();
   $Warden::TickEvent = schedule( 1000, 0, wardenTick );
}

//------------------------------------------------------------------------------
function wardenSetTab( %tab )
{
   if ( %tab < 0 || %tab >= $Warden::TabCount )
      %tab = 0;

   %previous = $Warden::Tab;
   $Warden::Tab = %tab;
   wardenDisarm();
   wardenShowTabMark();

   // The Bans and Panel tabs borrow the server's score-screen menu; hand it
   // back when leaving them.
   if ( ( %previous == 2 || %previous == 4 ) && %tab != 2 && %tab != 4 )
      wardenPanelRelease();

   switch ( %tab )
   {
      case 0:
         wardenPlayersRefresh();
      case 1:
         wardenMatchEnter();
      case 2:
         wardenBansEnter();
      case 3:
         wardenServerEnter();
      case 4:
         wardenPanelEnter();
      case 5:
         wardenLogRender();
      case 6:
         wardenAboutEnter();
   }
}

//------------------------------------------------------------------------------
// Header: server name on the right, the local player's rank beneath it.

function wardenUpdateHeader()
{
   if ( !isObject( WdnServerText ) )
      return;

   WdnServerText.setText( $clServerName );

   %count = isObject( PlayerListGroup ) ? PlayerListGroup.getCount() : 0;
   %me = wardenFindSelf();
   if ( isObject( %me ) )
   {
      if ( %me.isSuperAdmin )
         %rank = "SUPER ADMIN";
      else if ( %me.isAdmin )
         %rank = "ADMIN";
      else
         %rank = "PLAYER";
      %line = %rank @ "  -  " @ %count @ ( %count == 1 ? " client" : " clients" );
   }
   else
      %line = %count @ ( %count == 1 ? " client" : " clients" );

   WdnRankText.setText( %line );
}

// The name this client joined with, to find its own entry in the player list.
function wardenMyName()
{
   if ( $PlayingOnline )
      %name = getField( WONGetAuthInfo(), 0 );
   else
      %name = getField( $pref::Player[$pref::Player::Current], 0 );
   return %name;
}

function wardenPlainName( %name )
{
   return trim( StripMLControlChars( stripChars( %name, "\cp\co\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9" ) ) );
}

function wardenFindSelf()
{
   if ( !isObject( PlayerListGroup ) )
      return 0;

   if ( isObject( $Warden::Self ) && PlayerListGroup.isMember( $Warden::Self ) )
      return $Warden::Self;

   $Warden::Self = 0;
   %mine = wardenMyName();
   if ( %mine $= "" )
      return 0;

   %count = PlayerListGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
   {
      %rep = PlayerListGroup.getObject( %i );
      if ( %rep.isBot )
         continue;
      // A joined name carries the tribe tag around the warrior name.
      if ( strstr( wardenPlainName( %rep.name ), %mine ) != -1 )
      {
         $Warden::Self = %rep;
         return %rep;
      }
   }
   return 0;
}

//------------------------------------------------------------------------------
// Status line and hint.

function wardenSetStatus( %text )
{
   $Warden::Status = %text;
   if ( isObject( WdnStatusText ) )
      WdnStatusText.setText( %text );
}

function wardenSetHint( %text )
{
   if ( isObject( WdnHintText ) )
      WdnHintText.setText( %text );
}

//------------------------------------------------------------------------------
// Two-click confirmation. The first click on a destructive button arms it
// (label and colour change); a second click within four seconds runs it.

function wardenArm( %btn, %label, %restoreLabel, %restoreProfile )
{
   if ( $Warden::Armed $= %btn && isEventPending( $Warden::ArmEvent ) )
   {
      wardenDisarm();
      return true;
   }

   wardenDisarm();
   $Warden::Armed = %btn;
   $Warden::ArmLabel = %restoreLabel;
   $Warden::ArmProfile = %restoreProfile;
   wdnSetStyle( %btn, WdnBtnArmedProfile );
   wdnSetCaption( %btn, %label );
   $Warden::ArmEvent = schedule( 4000, 0, wardenDisarm );
   return false;
}

function wardenDisarm()
{
   cancel( $Warden::ArmEvent );
   %btn = $Warden::Armed;
   $Warden::Armed = "";
   if ( %btn !$= "" && isObject( %btn ) )
   {
      wdnSetStyle( %btn, $Warden::ArmProfile );
      wdnSetCaption( %btn, $Warden::ArmLabel );
   }
}

//------------------------------------------------------------------------------
// Log: every server message with text, newest last, in a ring of LogMax lines.

function wardenLogMessage( %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 )
{
   if ( %msgString $= "" )
      return;

   %text = detag( %msgString );
   %wav = strstr( %text, "~w" );
   if ( %wav != -1 )
      %text = getSubStr( %text, 0, %wav );
   %text = wardenPlainName( %text );
   if ( %text $= "" )
      return;

   wardenLogAdd( %text );
}

function wardenLogAdd( %text )
{
   %slot = ( $Warden::LogHead + $Warden::LogCount ) % $Warden::LogMax;
   if ( $Warden::LogCount < $Warden::LogMax )
      $Warden::LogCount++;
   else
      $Warden::LogHead = ( $Warden::LogHead + 1 ) % $Warden::LogMax;
   $Warden::LogLine[%slot] = formatTimeString( "HH:nn:ss" ) @ "  " @ %text;

   wardenSetStatus( %text );
   if ( $Warden::Open && $Warden::Tab == 5 )
      wardenLogRender();
}

function wardenLogRender()
{
   if ( !isObject( WdnLogList ) )
      return;

   WdnLogList.clear();
   for ( %i = 0; %i < $Warden::LogCount; %i++ )
   {
      %line = $Warden::LogLine[( $Warden::LogHead + %i ) % $Warden::LogMax];
      WdnLogList.addRow( %i, getSubStr( %line, 0, 8 ) TAB getSubStr( %line, 10, 1000 ) );
   }
   if ( $Warden::LogCount > 0 )
      WdnLogList.scrollVisible( $Warden::LogCount - 1 );

   WdnLogEmpty.setText( $Warden::LogCount == 0 ? "Server messages - kicks, votes, admin notices, replies to your commands - are collected here." : "" );
}

function wardenLogClear()
{
   $Warden::LogCount = 0;
   $Warden::LogHead = 0;
   wardenLogRender();
}

//------------------------------------------------------------------------------
// About: credit for the console. Edit $Warden::Author to name the author(s).

$Warden::Author = "GeekOfWires, with Claude Opus 5.5 and Claude Sonnet 5.5";

function wardenAboutEnter()
{
   wardenUpdateShow();
   %nl = "\n";
   %text = "<color:FF6060>CREDIT" @ %nl;
   %text = %text @ "<color:D2DCE8>Wilderzone Warden was made for the Wilderzone community by " @ $Warden::Author @ "." @ %nl @ %nl;
   %text = %text @ "<color:FF6060>BUILT ON" @ %nl;
   %text = %text @ "<color:D2DCE8>Tribes 2, by Dynamix. The console is client-side only and uses the commands and menus the server already offers, ";
   %text = %text @ "including the admin commands of Classic (z0dd's Admin Hud) and TacoServer's Server Admin Panel." @ %nl @ %nl;
   %text = %text @ "<color:FF6060>ARTWORK" @ %nl;
   %text = %text @ "<color:D2DCE8>Logo, wordmark and icons are drawn from primitives for this console. The wordmark follows the Wilderzone title style." @ %nl @ %nl;
   %text = %text @ "<color:7E8A99>Nothing here is installed on the server: what you can do depends on the rank the server gives you.";
   WdnAboutText.setText( %text );
}

//------------------------------------------------------------------------------
function wardenResetSession()
{
   $Warden::Self = 0;
   $Warden::ServerAddress = "";
   $Warden::SelectedClient = "";
   $Warden::LogCount = 0;
   $Warden::LogHead = 0;
   $Warden::Status = "";
   $Warden::PanelHeld = false;
   wardenVoteForget();
}

// "" is the catch-all message type: called for every server message.
addMessageCallback( "", wardenLogMessage );

wardenBindKeys();
