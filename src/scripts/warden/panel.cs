//------------------------------------------------------------------------------
// Wilderzone Warden - Bans and Panel tabs (TacoServer).
//
// TacoServer has no dedicated commands for its ban list or server settings:
// both live in the "Server Admin Panel" it draws on the score screen, where
// every option is a link that sends 'ProcessGameLink' with "S", a menu name and
// up to three arguments, and the server answers by redrawing score-screen
// lines ('ClearHud', 'SetLineHud', 'SetScoreHudHeader', ...).
//
// Warden sends the same links and listens to the same replies:
//   * the Panel tab mirrors the panel as it is, links and all;
//   * the Bans tab asks for the panel's ban page and reads the names and
//     "Unban" link numbers out of the lines.
//
// While the server is showing a client one of these menus it stops refreshing
// that client's real score screen, so the panel is handed back ("Reset") when
// the tab or the console is left.
//------------------------------------------------------------------------------

$Warden::PanelCapture = "";      // "", "bans" or "panel"
$Warden::PanelHeld = false;
$Warden::PLCount = 0;
$Warden::PanelCtrlCount = 0;

//------------------------------------------------------------------------------
function wardenPanelRequest( %menu, %a3, %a4, %a5 )
{
   if ( !isObject( ServerConnection ) )
      return;

   $Warden::PanelHeld = true;
   $Warden::PanelGotAny = false;
   $Warden::PanelRequestTime = getSimTime();
   commandToServer( 'ProcessGameLink', "S", %menu, %a3, %a4, %a5 );

   cancel( $Warden::PanelCheckEvent );
   $Warden::PanelCheckEvent = schedule( 2500, 0, wardenPanelCheck );
}

// Give the score screen back to the server's normal display.
function wardenPanelRelease()
{
   cancel( $Warden::PanelCheckEvent );
   cancel( $Warden::PanelRenderEvent );
   $Warden::PanelCapture = "";
   if ( $Warden::PanelHeld && isObject( ServerConnection ) )
      commandToServer( 'ProcessGameLink', "S", "Reset", 0 );
   $Warden::PanelHeld = false;
}

function wardenPanelClear()
{
   $Warden::PLCount = 0;
   $Warden::PanelHeader = "";
   $Warden::PanelSubheader = "";
   if ( $Warden::Tab == 4 )
      wardenPanelRender();
}

// No reply, or an empty one, a moment after asking.
function wardenPanelCheck()
{
   if ( !$Warden::Open )
      return;

   if ( !$Warden::PanelGotAny )
      %why = "No answer: this server does not run TacoServer's admin panel.";
   else if ( $Warden::PLCount == 0 && $Warden::PanelHeader $= "" )
      %why = "The server refused: its admin panel is for Super Admins only.";
   else
      return;

   if ( $Warden::Tab == 2 )
      wardenBansSetHelp( %why );
   else if ( $Warden::Tab == 4 )
      WdnPanelHint.setText( %why );
   wardenSetStatus( %why );
}

//------------------------------------------------------------------------------
// Score-screen traffic. The hud name arrives as "<tag> scoreScreen".

function wardenIsScoreHud( %hudName )
{
   return getWord( %hudName, 1 ) $= "scoreScreen" || detag( %hudName ) $= "scoreScreen";
}

function wardenPanelTouched()
{
   $Warden::PanelGotAny = true;
   cancel( $Warden::PanelRenderEvent );
   $Warden::PanelRenderEvent = schedule( 150, 0, wardenPanelSettled );
}

function wardenOnClearHud( %msgType, %msgString, %hudName, %a0 )
{
   if ( $Warden::PanelCapture $= "" || !wardenIsScoreHud( %hudName ) )
      return;

   %start = detag( %a0 );
   if ( %start $= "" || %start < 0 )
      %start = 0;
   if ( %start < $Warden::PLCount )
      $Warden::PLCount = %start;
   if ( %start == 0 )
   {
      $Warden::PanelHeader = "";
      $Warden::PanelSubheader = "";
   }
   wardenPanelTouched();
}

function wardenOnSetLineHud( %msgType, %msgString, %hudName, %lineNumber, %a0, %a1, %a2, %a3, %a4 )
{
   if ( $Warden::PanelCapture $= "" || !wardenIsScoreHud( %hudName ) )
      return;

   %line = detag( %lineNumber );
   if ( %line < 0 || %line > 400 )
      return;

   // Lines the server skipped are blank.
   for ( %i = $Warden::PLCount; %i < %line; %i++ )
      $Warden::PL[%i] = "";
   $Warden::PL[%line] = detag( %a0 );
   if ( %line >= $Warden::PLCount )
      $Warden::PLCount = %line + 1;
   wardenPanelTouched();
}

function wardenOnScoreHeader( %msgType, %msgString, %a0 )
{
   if ( $Warden::PanelCapture $= "" )
      return;
   $Warden::PanelHeader = detag( %a0 );
   wardenPanelTouched();
}

function wardenOnScoreSubheader( %msgType, %msgString, %a0 )
{
   if ( $Warden::PanelCapture $= "" )
      return;
   $Warden::PanelSubheader = detag( %a0 );
   wardenPanelTouched();
}

function wardenPanelSettled()
{
   if ( !$Warden::Open )
      return;

   if ( $Warden::PanelCapture $= "bans" )
      wardenBansParse();
   else if ( $Warden::PanelCapture $= "panel" )
      wardenPanelRender();
}

// Removes <...> markup, leaving the words.
function wardenStripTags( %text )
{
   %guard = 0;
   while ( %guard++ < 64 )
   {
      %open = strstr( %text, "<" );
      if ( %open == -1 )
         break;
      %rest = getSubStr( %text, %open, 100000 );
      %close = strstr( %rest, ">" );
      if ( %close == -1 )
         break;
      %text = getSubStr( %text, 0, %open ) @ getSubStr( %rest, %close + 1, 100000 );
   }
   return trim( %text );
}

//------------------------------------------------------------------------------
// Panel tab: the server's panel, line for line.

function wardenPanelEnter()
{
   $Warden::PanelCapture = "panel";
   $Warden::PLCount = 0;
   $Warden::PanelHeader = "";
   $Warden::PanelSubheader = "";
   WdnPanelHint.setText( "Links work as on the score screen." );
   wardenPanelRender();
   wardenPanelRequest( "SP", 0, 0, 0 );
}

function wardenPanelRender()
{
   if ( !isObject( WdnPanelLines ) )
      return;

   %title = wardenStripTags( $Warden::PanelHeader );
   WdnPanelTitle.setText( %title $= "" ? "Server panel" : %title );
   WdnPanelSub.setText( "<color:7E8A99>" @ $Warden::PanelSubheader );

   %w = $Warden::PanelW;
   for ( %i = 0; %i < $Warden::PLCount; %i++ )
   {
      if ( %i >= $Warden::PanelCtrlCount )
      {
         %ctrl = new GuiMLTextCtrl() {
            profile = "ScoreTextProfile"; position = "4 " @ ( %i * 20 + 4 ); extent = %w - 8 SPC 22; minExtent = "8 8"; visible = "1"; lineSpacing = "2";
         };
         WdnPanelLines.add( %ctrl );
         $Warden::PanelCtrl[%i] = %ctrl;
         $Warden::PanelCtrlCount++;
      }
      %ctrl = $Warden::PanelCtrl[%i];
      %ctrl.resize( 4, %i * 20 + 4, %w - 8, 22 );
      %ctrl.setVisible( true );
      %ctrl.setText( $Warden::PL[%i] );
   }
   for ( %i = $Warden::PLCount; %i < $Warden::PanelCtrlCount; %i++ )
      $Warden::PanelCtrl[%i].setVisible( false );

   wdnPlace( WdnPanelLines, 0, 0, %w, $Warden::PLCount * 20 + 12 );
}

//------------------------------------------------------------------------------
// Bans tab.

function wardenBansSetHelp( %status )
{
   %text = "<color:D2DCE8>" @ %status @ "\n\n"
         @ "<color:7E8A99>This is TacoServer's ban and kick list (temporary kick bans appear too). Removing an entry takes effect at once; the server saves its list a moment later.\n\n"
         @ "<color:D2DCE8>MAKE PERMANENT<color:7E8A99> turns the entry into a permanent ban and hides it from this list for good - after that it can only be lifted from the server console.";
   WdnBanHelp.setText( %text );
}

function wardenBansEnter()
{
   wardenBansSetHelp( "Asking the server..." );
   WdnBanList.clear();
   wardenBansRequest();
}

function wardenBansRequest()
{
   wardenDisarm();
   $Warden::PanelCapture = "bans";
   $Warden::PLCount = 0;
   $Warden::PanelHeader = "";
   $Warden::PanelSubheader = "";
   wardenPanelRequest( "SP", 0, "BL", "RS" );
}

// On the ban page each row of names ("\tName:minutes\tName:minutes...") is
// followed by a row of links whose last argument is "RMV <index>" / "HID <index>".
function wardenBansParse()
{
   if ( !isObject( WdnBanList ) )
      return;

   %selected = WdnBanList.getSelectedId();
   WdnBanList.clear();
   %found = 0;

   for ( %l = 1; %l < $Warden::PLCount; %l++ )
   {
      %links = $Warden::PL[%l];
      if ( strstr( %links, "RMV " ) == -1 )
         continue;

      %names = $Warden::PL[%l - 1];
      %nth = 0;
      %fields = getFieldCount( %links );
      for ( %f = 0; %f < %fields; %f++ )
      {
         %field = getField( %links, %f );
         if ( getSubStr( %field, 0, 4 ) !$= "RMV " )
            continue;

         %index = getWord( %field, 1 );
         %gt = strstr( %index, ">" );
         if ( %gt != -1 )
            %index = getSubStr( %index, 0, %gt );

         // Field 0 of the names row is its leading markup.
         %nth++;
         %entry = wardenStripTags( getField( %names, %nth ) );

         // "name:minutes" - the name may itself contain colons.
         %last = -1;
         %offset = 0;
         %pos = strstr( %entry, ":" );
         while ( %pos != -1 )
         {
            %last = %offset + %pos;
            %offset = %last + 1;
            %pos = strstr( getSubStr( %entry, %offset, 100000 ), ":" );
         }
         if ( %last == -1 )
         {
            %name = %entry;
            %left = "";
         }
         else
         {
            %name = getSubStr( %entry, 0, %last );
            %left = getSubStr( %entry, %last + 1, 100000 );
         }

         WdnBanList.addRow( %index, %name TAB %left );
         %found++;
      }
   }

   if ( %selected !$= "" && WdnBanList.getRowNumById( %selected ) != -1 )
      WdnBanList.setSelectedById( %selected );

   if ( strstr( strlwr( $Warden::PanelHeader ), "ban" ) == -1 && %found == 0 )
   {
      // Not the ban page: leave the verdict to wardenPanelCheck().
      return;
   }

   if ( %found == 0 )
      wardenBansSetHelp( "The list is empty: nobody is banned or kick-banned." );
   else
      wardenBansSetHelp( %found @ ( %found == 1 ? " entry." : " entries." ) @ " Select one, then UNBAN." );
}

function wardenBanUnban()
{
   %id = WdnBanList.getSelectedId();
   if ( %id $= "" || %id < 0 || WdnBanList.getRowNumById( %id ) == -1 )
   {
      wardenSetStatus( "Select a ban first." );
      return;
   }

   %name = getField( WdnBanList.getRowTextById( %id ), 0 );
   $Warden::PanelCapture = "bans";
   wardenPanelRequest( "SP", 0, "BL", "RMV" SPC %id );
   wardenLogAdd( "Unban requested: " @ %name );
   WdnBanList.clearSelection();
}

function wardenBanPerma()
{
   %id = WdnBanList.getSelectedId();
   if ( %id $= "" || %id < 0 || WdnBanList.getRowNumById( %id ) == -1 )
   {
      wardenSetStatus( "Select a ban first." );
      return;
   }

   if ( !wardenArm( WdnBanPermaBtn, "CONFIRM?", "MAKE PERMANENT", WdnBtnDangerProfile ) )
      return;

   %name = getField( WdnBanList.getRowTextById( %id ), 0 );
   $Warden::PanelCapture = "bans";
   wardenPanelRequest( "SP", 0, "BL", "HID" SPC %id );
   wardenLogAdd( "Permanent ban requested: " @ %name );
   WdnBanList.clearSelection();
}

addMessageCallback( 'ClearHud', wardenOnClearHud );
addMessageCallback( 'SetLineHud', wardenOnSetLineHud );
addMessageCallback( 'SetScoreHudHeader', wardenOnScoreHeader );
addMessageCallback( 'SetScoreHudSubheader', wardenOnScoreSubheader );
