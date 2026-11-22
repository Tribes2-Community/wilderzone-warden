//------------------------------------------------------------------------------
// Wilderzone Warden - update check.
//
// Asks GitHub for the newest release of this console and compares its version
// with $Warden::Version. Needs a client whose HTTPObject speaks HTTPS (the QOL
// client does; the stock engine cannot) - elsewhere the check simply reports
// that it could not reach GitHub, and the button opens the releases page.
//
// $pref::Warden::CheckUpdates = false  turns the automatic check off.
//------------------------------------------------------------------------------

$Warden::Repo = "Tribes2-Community/wilderzone-warden";
$Warden::ReleasesUrl = "https://github.com/Tribes2-Community/wilderzone-warden/releases/latest";

if ( $pref::Warden::CheckUpdates $= "" )
   $pref::Warden::CheckUpdates = true;

// "", "checking", "current", "newer" or "failed"
$Warden::UpdateState = "";
$Warden::UpdateLatest = "";

// True when version %a is newer than %b ("1.10" is newer than "1.9").
function wardenVersionNewer( %a, %b )
{
   %a = strreplace( %a, ".", " " );
   %b = strreplace( %b, ".", " " );
   for ( %i = 0; %i < 3; %i++ )
   {
      %x = getWord( %a, %i ) + 0;
      %y = getWord( %b, %i ) + 0;
      if ( %x > %y )
         return true;
      if ( %x < %y )
         return false;
   }
   return false;
}

//------------------------------------------------------------------------------
function wardenUpdateCheck( %manual )
{
   if ( $Warden::UpdateState $= "checking" )
      return;

   $Warden::UpdateState = "checking";
   $Warden::UpdateLatest = "";
   $Warden::UpdateManual = %manual;

   if ( isObject( WardenUpdate ) )
   {
      WardenUpdate.disconnect();
      WardenUpdate.delete();
   }
   new HTTPObject( WardenUpdate );
   WardenUpdate.setHeader( "Accept", "application/vnd.github+json" );
   WardenUpdate.get( "https://api.github.com", "repos/" @ $Warden::Repo @ "/releases/latest" );

   cancel( $Warden::UpdateTimeout );
   $Warden::UpdateTimeout = schedule( 15000, 0, wardenUpdateFinish );
   wardenUpdateShow();
}

// Called when the console is opened: one automatic check per game session.
function wardenUpdateAuto()
{
   if ( $pref::Warden::CheckUpdates && $Warden::UpdateState $= "" )
      wardenUpdateCheck( false );
}

// The reply is JSON, one field per line: pick out  "tag_name": "v1.2",
function WardenUpdate::onLine( %this, %line )
{
   %at = strstr( %line, "\"tag_name\"" );
   if ( %at == -1 )
      return;

   %rest = getSubStr( %line, %at + 10, 80 );
   %q1 = strstr( %rest, "\"" );
   if ( %q1 == -1 )
      return;
   %rest = getSubStr( %rest, %q1 + 1, 80 );
   %q2 = strstr( %rest, "\"" );
   if ( %q2 == -1 )
      return;

   %tag = getSubStr( %rest, 0, %q2 );
   if ( getSubStr( %tag, 0, 1 ) $= "v" )
      %tag = getSubStr( %tag, 1, 40 );
   $Warden::UpdateLatest = %tag;
}

function WardenUpdate::onDisconnect( %this )
{
   wardenUpdateFinish();
}

function WardenUpdate::onConnectFailed( %this )
{
   wardenUpdateFinish();
}

function WardenUpdate::onDNSFailed( %this )
{
   wardenUpdateFinish();
}

function wardenUpdateFinish()
{
   cancel( $Warden::UpdateTimeout );
   if ( $Warden::UpdateState !$= "checking" )
      return;

   if ( $Warden::UpdateLatest $= "" )
      $Warden::UpdateState = "failed";
   else if ( wardenVersionNewer( $Warden::UpdateLatest, $Warden::Version ) )
      $Warden::UpdateState = "newer";
   else
      $Warden::UpdateState = "current";

   wardenUpdateShow();

   if ( $Warden::UpdateState $= "newer" )
      wardenLogAdd( "Wilderzone Warden " @ $Warden::UpdateLatest @ " is available (this is " @ $Warden::Version @ "). See the About tab." );
   else if ( $Warden::UpdateManual )
      wardenSetStatus( wardenUpdateText() );
}

function wardenUpdateText()
{
   switch$ ( $Warden::UpdateState )
   {
      case "checking":
         return "Checking GitHub for updates...";
      case "current":
         return "You have the latest version.";
      case "newer":
         return "Version " @ $Warden::UpdateLatest @ " is available.";
      case "failed":
         return "Could not reach GitHub (the update check needs a client with HTTPS support, such as QOL).";
   }
   return "";
}

// Refreshes the About tab: version line and the button.
function wardenUpdateShow()
{
   if ( !isObject( WdnAboutVersion ) )
      return;

   %line = "Version " @ $Warden::Version;
   %text = wardenUpdateText();
   if ( %text !$= "" )
      %line = %line @ "  -  " @ %text;
   WdnAboutVersion.setText( %line );

   switch$ ( $Warden::UpdateState )
   {
      case "newer":
         wdnSetCaption( WdnAboutUpdateBtn, "GET UPDATE" );
      case "failed":
         wdnSetCaption( WdnAboutUpdateBtn, "OPEN RELEASES" );
      default:
         wdnSetCaption( WdnAboutUpdateBtn, "CHECK FOR UPDATES" );
   }
}

function wardenUpdateButton()
{
   if ( $Warden::UpdateState $= "newer" || $Warden::UpdateState $= "failed" )
      gotoWebPage( $Warden::ReleasesUrl );
   else
      wardenUpdateCheck( true );
}
