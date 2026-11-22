//------------------------------------------------------------------------------
// Wilderzone Warden - the dialog and its layout.
//
// The controls are created once (wardenBuildGui) with placeholder geometry and
// positioned by wardenLayout() from the canvas size each time the console
// opens, so it fills a sensible part of the screen at any resolution.
//------------------------------------------------------------------------------

$Warden::TabCount = 7;
$Warden::TabName[0] = "Players";  $Warden::TabIcon[0] = "tab_players";
$Warden::TabName[1] = "Match";    $Warden::TabIcon[1] = "tab_match";
$Warden::TabName[2] = "Bans";     $Warden::TabIcon[2] = "tab_bans";
$Warden::TabName[3] = "Server";   $Warden::TabIcon[3] = "tab_server";
$Warden::TabName[4] = "Panel";    $Warden::TabIcon[4] = "tab_panel";
$Warden::TabName[5] = "Log";      $Warden::TabIcon[5] = "tab_log";
$Warden::TabName[6] = "About";    $Warden::TabIcon[6] = "tab_about";

// Action buttons shown for the selected player (2 columns).
$Warden::ActionSlots = 18;

//------------------------------------------------------------------------------
// Small constructors. Geometry is assigned later by wardenLayout().

function wdnBox( %name, %profile )
{
   return new GuiControl( %name ) { profile = %profile; position = "0 0"; extent = "8 8"; minExtent = "1 1"; visible = "1"; };
}

function wdnText( %name, %profile, %text )
{
   return new GuiTextCtrl( %name ) { profile = %profile; position = "0 0"; extent = "8 18"; minExtent = "1 1"; visible = "1"; text = %text; };
}

function wdnIcon( %name, %bitmap )
{
   return new GuiBitmapCtrl( %name ) { profile = "WdnIconProfile"; modal = "0"; position = "0 0"; extent = "20 20"; minExtent = "1 1"; visible = "1"; bitmap = "gui/warden/" @ %bitmap; wrap = "0"; };
}

function wdnButton( %name, %profile, %text, %command )
{
   $Warden::Button[$Warden::ButtonCount + 0] = %name;
   $Warden::ButtonProfile[$Warden::ButtonCount + 0] = %profile;
   $Warden::ButtonCount++;
   return new GuiButtonCtrl( %name ) { profile = %profile; position = "0 0"; extent = "80 24"; minExtent = "1 1"; visible = "1"; text = wdnCaption( %text ); command = %command; };
}

// The engine keeps control text in its case-insensitive string table, so a
// caption such as "KICK" comes back as "kick" if that spelling was stored
// first. Padding makes the caption a string nothing else uses.
function wdnCaption( %text )
{
   return %text $= "" ? "" : " " @ %text @ " ";
}

function wdnSetCaption( %ctrl, %text )
{
   %ctrl.setValue( wdnCaption( %text ) );
}

// Gives every button its face box (named <button>Bg) at the back of its parent
// and moves the button itself to the front, above its icon. See profiles.cs.
function wdnFinishButtons()
{
   for ( %i = 0; %i < $Warden::ButtonCount; %i++ )
   {
      %btn = $Warden::Button[%i];
      %parent = %btn.getGroup();
      %face = wdnBox( %btn @ "Bg", strreplace( $Warden::ButtonProfile[%i], "Profile", "FaceProfile" ) );
      %parent.add( %face );
      // Children draw in list order, last on top; in this engine bringToFront
      // moves an object to the head of the list and pushToBack to its tail.
      %parent.bringToFront( %face );
      %parent.pushToBack( %btn );
   }
}

// Switches a button, and its face, to another button profile.
function wdnSetStyle( %btn, %profile )
{
   %btn.setProfile( %profile );
   %face = %btn @ "Bg";
   if ( isObject( %face ) )
      %face.setProfile( strreplace( %profile, "Profile", "FaceProfile" ) );
}

// Shows or hides a button together with its face.
function wdnShowButton( %btn, %visible )
{
   %btn.setVisible( %visible );
   %face = %btn @ "Bg";
   if ( isObject( %face ) )
      %face.setVisible( %visible );
}

function wdnEdit( %name, %command, %altCommand, %password )
{
   return new GuiTextEditCtrl( %name ) { profile = "WdnEditProfile"; position = "0 0"; extent = "80 24"; minExtent = "1 1"; visible = "1";
      command = %command; altCommand = %altCommand; historySize = "0"; maxLength = "255"; password = %password; };
}

function wdnML( %name )
{
   return new GuiMLTextCtrl( %name ) { profile = "WdnMLProfile"; position = "0 0"; extent = "80 20"; minExtent = "1 1"; visible = "1"; lineSpacing = "2"; allowColorChars = "0"; };
}

// A scroll view holding one text list. The scroll is %name @ "Scroll".
function wdnList( %name, %altCommand )
{
   %scroll = new GuiScrollCtrl( %name @ "Scroll" ) {
      profile = "WdnScrollProfile"; position = "0 0"; extent = "100 100"; minExtent = "1 1"; visible = "1";
      willFirstRespond = "1"; hScrollBar = "alwaysOff"; vScrollBar = "alwaysOn"; constantThumbHeight = "0";

      new GuiScrollContentCtrl() {
         profile = "WdnScrollContentProfile"; position = "1 1"; extent = "80 98"; minExtent = "1 1"; visible = "1";

         new GuiTextListCtrl( %name ) {
            profile = "WdnListProfile"; position = "0 0"; extent = "80 16"; minExtent = "1 1"; visible = "1";
            enumerate = "0"; resizeCell = "1"; columns = "0"; fitParentWidth = "1"; clipColumnText = "1"; altCommand = %altCommand;
         };
      };
   };
   return %scroll;
}

// A scroll view holding one free-form content control (%name), sized by its owner.
function wdnScrollBox( %name )
{
   %scroll = new GuiScrollCtrl( %name @ "Scroll" ) {
      profile = "WdnScrollProfile"; position = "0 0"; extent = "100 100"; minExtent = "1 1"; visible = "1";
      willFirstRespond = "1"; hScrollBar = "alwaysOff"; vScrollBar = "alwaysOn"; constantThumbHeight = "0";

      new GuiScrollContentCtrl() {
         profile = "WdnScrollContentProfile"; position = "1 1"; extent = "80 98"; minExtent = "1 1"; visible = "1";

         new GuiControl( %name ) { profile = "WdnNoneProfile"; position = "0 0"; extent = "80 16"; minExtent = "1 1"; visible = "1"; };
      };
   };
   return %scroll;
}

//------------------------------------------------------------------------------
function wardenBuildGui()
{
   if ( isObject( WardenDlg ) )
      return;

   new GuiControl( WardenDlg ) {
      profile = "WdnDimProfile"; horizSizing = "width"; vertSizing = "height"; position = "0 0"; extent = "640 480"; minExtent = "8 8"; visible = "1";
   };
   WardenDlg.add( wdnBox( WdnFrame, "WdnFrameProfile" ) );

   // --- header ---------------------------------------------------------------
   WdnFrame.add( wdnBox( WdnHeader, "WdnHeaderProfile" ) );
   WdnHeader.add( wdnIcon( WdnLogo, "logo" ) );
   WdnHeader.add( wdnIcon( WdnWordmark, "wordmark" ) );
   WdnHeader.add( wdnText( WdnServerText, "WdnTextRightProfile", "" ) );
   WdnHeader.add( wdnText( WdnRankText, "WdnTextRightProfile", "" ) );
   WdnHeader.add( wdnButton( WdnCloseBtn, "WdnBtnProfile", "", "wardenClose();" ) );
   WdnHeader.add( wdnIcon( WdnCloseIcon, "icon_close" ) );
   WdnFrame.add( wdnBox( WdnHeaderLine, "WdnRedLineProfile" ) );
   WdnFrame.add( wdnBox( WdnFrameTop, "WdnRedLineProfile" ) );

   // --- tab bar --------------------------------------------------------------
   WdnFrame.add( wdnBox( WdnTabBar, "WdnHeaderProfile" ) );
   for ( %i = 0; %i < $Warden::TabCount; %i++ )
   {
      WdnTabBar.add( wdnButton( "WdnTabBtn" @ %i, "WdnTabProfile", strupr( $Warden::TabName[%i] ), "wardenSetTab(" @ %i @ ");" ) );
      WdnTabBar.add( wdnIcon( "WdnTabIcon" @ %i, $Warden::TabIcon[%i] ) );
   }
   WdnTabBar.add( wdnBox( WdnTabMark, "WdnAccentProfile" ) );
   WdnFrame.add( wdnBox( WdnTabLine, "WdnLineProfile" ) );

   // --- tab pages ------------------------------------------------------------
   WdnFrame.add( wdnBox( WdnContent, "WdnNoneProfile" ) );
   for ( %i = 0; %i < $Warden::TabCount; %i++ )
      WdnContent.add( wdnBox( "WdnPage" @ %i, "WdnNoneProfile" ) );

   wardenBuildPlayersPage( WdnPage0 );
   wardenBuildMatchPage( WdnPage1 );
   wardenBuildBansPage( WdnPage2 );
   wardenBuildServerPage( WdnPage3 );
   wardenBuildPanelPage( WdnPage4 );
   wardenBuildLogPage( WdnPage5 );
   wardenBuildAboutPage( WdnPage6 );

   // --- status bar -----------------------------------------------------------
   WdnFrame.add( wdnBox( WdnStatusBar, "WdnHeaderProfile" ) );
   WdnStatusBar.add( wdnText( WdnStatusText, "WdnTextProfile", "" ) );
   WdnStatusBar.add( wdnText( WdnHintText, "WdnTextRightProfile", "" ) );

   wdnFinishButtons();
}

//------------------------------------------------------------------------------
function wardenBuildPlayersPage( %page )
{
   %page.add( wdnText( WdnPlHeadName, "WdnLabelProfile", "PLAYER" ) );
   %page.add( wdnText( WdnPlHeadTeam, "WdnLabelProfile", "TEAM" ) );
   %page.add( wdnText( WdnPlHeadScore, "WdnLabelProfile", "SCORE" ) );
   %page.add( wdnText( WdnPlHeadPing, "WdnLabelProfile", "PING" ) );
   %page.add( wdnText( WdnPlHeadPL, "WdnLabelProfile", "PL" ) );
   %page.add( wdnList( WdnPlayerList, "" ) );

   %page.add( wdnEdit( WdnPmEdit, "", "wardenSendPM();", "0" ) );
   %page.add( wdnButton( WdnPmBtn, "WdnBtnProfile", "SEND PM", "wardenSendPM();" ) );
   %page.add( wdnIcon( WdnPmIcon, "icon_message" ) );

   // Selected-player card.
   %page.add( wdnBox( WdnCard, "WdnPanelProfile" ) );
   WdnCard.add( wdnText( WdnCardName, "WdnTitleProfile", "" ) );
   WdnCard.add( wdnText( WdnCardSub, "WdnTextDimProfile", "" ) );
   WdnCard.add( wdnText( WdnCardSub2, "WdnTextDimProfile", "" ) );
   WdnCard.add( wdnBox( WdnCardLine, "WdnLineProfile" ) );
   WdnCard.add( wdnText( WdnCardLabel, "WdnLabelProfile", "ACTIONS" ) );
   WdnCard.add( wdnText( WdnCardEmpty, "WdnTextDimProfile", "" ) );
   for ( %i = 0; %i < $Warden::ActionSlots; %i++ )
   {
      WdnCard.add( wdnButton( "WdnActBtn" @ %i, "WdnBtnProfile", "", "wardenActionClick(" @ %i @ ");" ) );
      WdnCard.add( wdnIcon( "WdnActIcon" @ %i, "icon_generic" ) );
   }
}

//------------------------------------------------------------------------------
function wardenBuildMatchPage( %page )
{
   %page.add( wdnText( WdnVoteCrumb, "WdnLabelProfile", "SERVER MENU" ) );
   %page.add( wdnList( WdnVoteList, "wardenVoteRun();" ) );
   %page.add( wdnButton( WdnVoteBackBtn, "WdnBtnProfile", "BACK", "wardenVoteBack();" ) );
   %page.add( wdnIcon( WdnVoteBackIcon, "icon_back" ) );
   %page.add( wdnEdit( WdnVoteFilter, "wardenVoteFilterChanged();", "wardenVoteRun();", "0" ) );
   %page.add( wdnButton( WdnVoteRunBtn, "WdnBtnPrimaryProfile", "RUN", "wardenVoteRun();" ) );
   %page.add( wdnIcon( WdnVoteRunIcon, "icon_go" ) );

   %page.add( wdnBox( WdnMatchCard, "WdnPanelProfile" ) );
   WdnMatchCard.add( wdnText( WdnMatchLabel, "WdnLabelProfile", "NOW PLAYING" ) );
   WdnMatchCard.add( wdnText( WdnMatchMission, "WdnTitleProfile", "" ) );
   WdnMatchCard.add( wdnText( WdnMatchType, "WdnTextDimProfile", "" ) );
   WdnMatchCard.add( wdnBox( WdnMatchLine, "WdnLineProfile" ) );
   WdnMatchCard.add( wdnML( WdnMatchHelp ) );
   WdnMatchCard.add( wdnButton( WdnVoteRefreshBtn, "WdnBtnProfile", "REFRESH MENU", "wardenVoteReset();" ) );
   WdnMatchCard.add( wdnIcon( WdnVoteRefreshIcon, "icon_refresh" ) );
}

//------------------------------------------------------------------------------
function wardenBuildBansPage( %page )
{
   %page.add( wdnText( WdnBanHeadName, "WdnLabelProfile", "BANNED / KICKED PLAYER" ) );
   %page.add( wdnText( WdnBanHeadTime, "WdnLabelProfile", "MINUTES LEFT" ) );
   %page.add( wdnList( WdnBanList, "" ) );
   %page.add( wdnButton( WdnBanRefreshBtn, "WdnBtnProfile", "REFRESH", "wardenBansRequest();" ) );
   %page.add( wdnIcon( WdnBanRefreshIcon, "icon_refresh" ) );
   %page.add( wdnButton( WdnBanUnbanBtn, "WdnBtnPrimaryProfile", "UNBAN", "wardenBanUnban();" ) );
   %page.add( wdnIcon( WdnBanUnbanIcon, "icon_apply" ) );
   %page.add( wdnButton( WdnBanPermaBtn, "WdnBtnDangerProfile", "MAKE PERMANENT", "wardenBanPerma();" ) );
   %page.add( wdnIcon( WdnBanPermaIcon, "icon_ban" ) );

   %page.add( wdnBox( WdnBanCard, "WdnPanelProfile" ) );
   WdnBanCard.add( wdnText( WdnBanLabel, "WdnLabelProfile", "BAN LIST" ) );
   WdnBanCard.add( wdnML( WdnBanHelp ) );
}

//------------------------------------------------------------------------------
function wardenBuildServerPage( %page )
{
   %page.add( wdnBox( WdnSrvLeft, "WdnPanelProfile" ) );
   WdnSrvLeft.add( wdnText( WdnAuthLabel, "WdnLabelProfile", "ADMIN LOGIN" ) );
   WdnSrvLeft.add( wdnText( WdnAuthHint, "WdnTextDimProfile", "Admin or Super Admin password" ) );
   WdnSrvLeft.add( wdnEdit( WdnAuthEdit, "", "wardenLogin();", "1" ) );
   WdnSrvLeft.add( wdnButton( WdnAuthBtn, "WdnBtnPrimaryProfile", "LOG IN", "wardenLogin();" ) );
   WdnSrvLeft.add( wdnIcon( WdnAuthIcon, "icon_key" ) );
   WdnSrvLeft.add( wdnBox( WdnSrvLine, "WdnLineProfile" ) );
   WdnSrvLeft.add( wdnText( WdnCastLabel, "WdnLabelProfile", "BROADCAST TO ALL PLAYERS" ) );
   WdnSrvLeft.add( wdnEdit( WdnCastEdit, "", "wardenBroadcast(0);", "0" ) );
   WdnSrvLeft.add( wdnButton( WdnCastCenterBtn, "WdnBtnProfile", "CENTER PRINT", "wardenBroadcast(0);" ) );
   WdnSrvLeft.add( wdnIcon( WdnCastCenterIcon, "icon_broadcast" ) );
   WdnSrvLeft.add( wdnButton( WdnCastBottomBtn, "WdnBtnProfile", "BOTTOM PRINT", "wardenBroadcast(1);" ) );
   WdnSrvLeft.add( wdnIcon( WdnCastBottomIcon, "icon_broadcast" ) );

   %page.add( wdnBox( WdnSrvRight, "WdnPanelProfile" ) );
   WdnSrvRight.add( wdnText( WdnSetLabel, "WdnLabelProfile", "SERVER SETTINGS  (SUPER ADMIN)" ) );
   WdnSrvRight.add( wdnList( WdnSetList, "" ) );
   WdnSrvRight.add( wdnML( WdnSetHelp ) );
   WdnSrvRight.add( wdnEdit( WdnSetEdit, "", "wardenSettingApply();", "0" ) );
   // Same place, masked: shown instead for the admin passwords.
   WdnSrvRight.add( wdnEdit( WdnSetEditMasked, "", "wardenSettingApply();", "1" ) );
   WdnSrvRight.add( wdnButton( WdnSetBtn, "WdnBtnPrimaryProfile", "APPLY", "wardenSettingApply();" ) );
   WdnSrvRight.add( wdnIcon( WdnSetIcon, "icon_apply" ) );
}

//------------------------------------------------------------------------------
function wardenBuildPanelPage( %page )
{
   %page.add( wdnText( WdnPanelTitle, "WdnTitleProfile", "" ) );
   %page.add( wdnML( WdnPanelSub ) );
   %page.add( wdnScrollBox( WdnPanelLines ) );
   %page.add( wdnButton( WdnPanelHomeBtn, "WdnBtnPrimaryProfile", "SERVER PANEL", "wardenPanelRequest(\"SP\", 0, 0, 0);" ) );
   %page.add( wdnIcon( WdnPanelHomeIcon, "tab_panel" ) );
   %page.add( wdnButton( WdnPanelResetBtn, "WdnBtnProfile", "CLOSE PANEL", "wardenPanelRelease(); wardenPanelClear();" ) );
   %page.add( wdnIcon( WdnPanelResetIcon, "icon_close" ) );
   %page.add( wdnText( WdnPanelHint, "WdnTextDimProfile", "" ) );
}

//------------------------------------------------------------------------------
function wardenBuildLogPage( %page )
{
   %page.add( wdnText( WdnLogLabel, "WdnLabelProfile", "SERVER MESSAGES" ) );
   %page.add( wdnList( WdnLogList, "" ) );
   %page.add( wdnButton( WdnLogClearBtn, "WdnBtnProfile", "CLEAR", "wardenLogClear();" ) );
   %page.add( wdnText( WdnLogEmpty, "WdnTextDimProfile", "" ) );
}

//------------------------------------------------------------------------------
function wardenBuildAboutPage( %page )
{
   %page.add( wdnBox( WdnAboutCard, "WdnPanelProfile" ) );
   WdnAboutCard.add( wdnIcon( WdnAboutLogo, "logo" ) );
   WdnAboutCard.add( wdnIcon( WdnAboutWordmark, "wordmark" ) );
   WdnAboutCard.add( wdnText( WdnAboutVersion, "WdnTextDimProfile", "" ) );
   WdnAboutCard.add( wdnBox( WdnAboutLine, "WdnLineProfile" ) );
   WdnAboutCard.add( wdnML( WdnAboutText ) );
   WdnAboutCard.add( wdnBox( WdnAboutTop, "WdnRedLineProfile" ) );
}

//------------------------------------------------------------------------------
// Placement helpers.

function wdnPlace( %ctrl, %x, %y, %w, %h )
{
   %ctrl.resize( mFloor( %x ), mFloor( %y ), mFloor( %w ), mFloor( %h ) );
   // A button's face goes where the button goes.
   %face = %ctrl @ "Bg";
   if ( isObject( %face ) )
      %face.resize( mFloor( %x ), mFloor( %y ), mFloor( %w ), mFloor( %h ) );
}

// Places a button and puts its icon on the left edge, vertically centred.
function wdnPlaceBtn( %btn, %icon, %x, %y, %w, %h )
{
   wdnPlace( %btn, %x, %y, %w, %h );
   %s = %h > 24 ? 20 : 16;
   wdnPlace( %icon, %x + 6, %y + ( %h - %s ) / 2, %s, %s );
}

// Sizes a list scroll and the list/content inside it.
function wdnPlaceScroll( %scroll, %inner, %x, %y, %w, %h )
{
   wdnPlace( %scroll, %x, %y, %w, %h );
   %content = %scroll.getObject( 0 );
   wdnPlace( %content, 1, 1, %w - 20, %h - 2 );
   %ext = %inner.getExtent();
   wdnPlace( %inner, 0, 0, %w - 20, getWord( %ext, 1 ) );
}

function wdnClamp( %v, %lo, %hi )
{
   if ( %v < %lo )
      return %lo;
   if ( %v > %hi )
      return %hi;
   return %v;
}

//------------------------------------------------------------------------------
function wardenLayout()
{
   %cext = Canvas.getExtent();
   %cw = getWord( %cext, 0 );
   %ch = getWord( %cext, 1 );

   wdnPlace( WardenDlg, 0, 0, %cw, %ch );

   %W = wdnClamp( %cw - 80, 620, 1040 );
   %H = wdnClamp( %ch - 80, 440, 720 );
   if ( %W > %cw ) %W = %cw;
   if ( %H > %ch ) %H = %ch;
   $Warden::W = %W;
   $Warden::H = %H;
   wdnPlace( WdnFrame, ( %cw - %W ) / 2, ( %ch - %H ) / 2, %W, %H );

   // Header.
   wdnPlace( WdnHeader, 1, 1, %W - 2, 46 );
   wdnPlace( WdnLogo, 10, 5, 36, 36 );
   // The wordmark bitmap is 512x32; shown at 5/8 size, or half size in a narrow window.
   %wm = %W >= 800 ? 320 : 256;
   wdnPlace( WdnWordmark, 54, %W >= 800 ? 13 : 15, %wm, %wm / 16 );
   %sx = %W - 420;
   if ( %sx < 54 + %wm + 8 )
      %sx = 54 + %wm + 8;
   wdnPlace( WdnServerText, %sx, 6, %W - 54 - %sx, 18 );
   wdnPlace( WdnRankText, %sx, 25, %W - 54 - %sx, 16 );
   wdnPlace( WdnCloseBtn, %W - 42, 9, 28, 28 );
   wdnPlace( WdnCloseIcon, %W - 38, 13, 20, 20 );
   wdnPlace( WdnHeaderLine, 1, 46, %W - 2, 2 );
   wdnPlace( WdnFrameTop, 1, 1, %W - 2, 1 );

   // Tab bar.
   wdnPlace( WdnTabBar, 1, 48, %W - 2, 32 );
   %tw = mFloor( ( %W - 18 ) / $Warden::TabCount );
   if ( %tw > 124 ) %tw = 124;
   $Warden::TabW = %tw;
   for ( %i = 0; %i < $Warden::TabCount; %i++ )
   {
      wdnPlace( "WdnTabBtn" @ %i, 8 + %i * %tw, 0, %tw - 2, 30 );
      wdnPlace( "WdnTabIcon" @ %i, 8 + %i * %tw + 8, 6, 18, 18 );
   }
   wdnPlace( WdnTabLine, 1, 80, %W - 2, 1 );

   // Pages.
   %CW = %W - 20;
   %CH = %H - 90 - 34;
   $Warden::CW = %CW;
   $Warden::CH = %CH;
   wdnPlace( WdnContent, 10, 90, %CW, %CH );
   for ( %i = 0; %i < $Warden::TabCount; %i++ )
      wdnPlace( "WdnPage" @ %i, 0, 0, %CW, %CH );

   // Status bar.
   wdnPlace( WdnStatusBar, 1, %H - 27, %W - 2, 26 );
   wdnPlace( WdnStatusText, 10, 4, %W - 260, 18 );
   wdnPlace( WdnHintText, %W - 250, 4, 238, 18 );

   wardenLayoutPlayers( %CW, %CH );
   wardenLayoutMatch( %CW, %CH );
   wardenLayoutBans( %CW, %CH );
   wardenLayoutServer( %CW, %CH );
   wardenLayoutPanel( %CW, %CH );
   wardenLayoutLog( %CW, %CH );
   wardenLayoutAbout( %CW, %CH );

   wardenShowTabMark();
}

//------------------------------------------------------------------------------
function wardenLayoutPlayers( %CW, %CH )
{
   %cardW = %CW >= 760 ? 348 : 250;
   %LW = %CW - %cardW - 10;

   // Column starts inside the list (the scroll bar takes the last 20).
   %inner = %LW - 22;
   %cPL = %inner - 34;
   %cPing = %cPL - 46;
   %cScore = %cPing - 56;
   %cTeam = %cScore - ( %inner > 420 ? 130 : 96 );
   WdnPlayerList.columns = "4 34" SPC %cTeam SPC %cScore SPC %cPing SPC %cPL;

   wdnPlace( WdnPlHeadName, 36, 0, 120, 18 );
   wdnPlace( WdnPlHeadTeam, %cTeam + 2, 0, 80, 18 );
   wdnPlace( WdnPlHeadScore, %cScore + 2, 0, 54, 18 );
   wdnPlace( WdnPlHeadPing, %cPing + 2, 0, 44, 18 );
   wdnPlace( WdnPlHeadPL, %cPL + 2, 0, 30, 18 );
   wdnPlaceScroll( WdnPlayerListScroll, WdnPlayerList, 0, 20, %LW, %CH - 20 - 32 );

   wdnPlace( WdnPmEdit, 0, %CH - 26, %LW - 116, 26 );
   wdnPlaceBtn( WdnPmBtn, WdnPmIcon, %LW - 110, %CH - 26, 110, 26 );

   // Card.
   wdnPlace( WdnCard, %LW + 10, 0, %cardW, %CH );
   wdnPlace( WdnCardName, 12, 8, %cardW - 24, 24 );
   wdnPlace( WdnCardSub, 12, 33, %cardW - 24, 16 );
   wdnPlace( WdnCardSub2, 12, 49, %cardW - 24, 16 );
   wdnPlace( WdnCardLine, 1, 70, %cardW - 2, 1 );
   wdnPlace( WdnCardLabel, 12, 76, %cardW - 24, 18 );
   wdnPlace( WdnCardEmpty, 12, 100, %cardW - 24, 16 );

   %rows = mCeil( $Warden::ActionSlots / 2 );
   %pitch = wdnClamp( mFloor( ( %CH - 100 - 6 ) / %rows ), 22, 32 );
   %bw = mFloor( ( %cardW - 24 - 6 ) / 2 );
   // Captions are centred; on a narrow card the icon would sit on top of them.
   $Warden::ActionIcons = %bw >= 130;
   for ( %i = 0; %i < $Warden::ActionSlots; %i++ )
   {
      %col = %i % 2;
      %row = mFloor( %i / 2 );
      wdnPlaceBtn( "WdnActBtn" @ %i, "WdnActIcon" @ %i, 12 + %col * ( %bw + 6 ), 98 + %row * %pitch, %bw, %pitch - 4 );
   }
}

//------------------------------------------------------------------------------
function wardenLayoutMatch( %CW, %CH )
{
   %cardW = %CW >= 760 ? 300 : 230;
   %LW = %CW - %cardW - 10;

   wdnPlace( WdnVoteCrumb, 2, 0, %LW - 4, 18 );
   wdnPlaceScroll( WdnVoteListScroll, WdnVoteList, 0, 20, %LW, %CH - 20 - 32 );
   wdnPlaceBtn( WdnVoteBackBtn, WdnVoteBackIcon, 0, %CH - 26, 90, 26 );
   wdnPlace( WdnVoteFilter, 96, %CH - 26, %LW - 96 - 106, 26 );
   wdnPlaceBtn( WdnVoteRunBtn, WdnVoteRunIcon, %LW - 100, %CH - 26, 100, 26 );

   wdnPlace( WdnMatchCard, %LW + 10, 0, %cardW, %CH );
   wdnPlace( WdnMatchLabel, 12, 8, %cardW - 24, 18 );
   wdnPlace( WdnMatchMission, 12, 28, %cardW - 24, 24 );
   wdnPlace( WdnMatchType, 12, 54, %cardW - 24, 16 );
   wdnPlace( WdnMatchLine, 1, 78, %cardW - 2, 1 );
   wdnPlace( WdnMatchHelp, 12, 88, %cardW - 24, %CH - 88 - 44 );
   wdnPlaceBtn( WdnVoteRefreshBtn, WdnVoteRefreshIcon, 12, %CH - 36, %cardW - 24, 26 );
}

//------------------------------------------------------------------------------
function wardenLayoutBans( %CW, %CH )
{
   %cardW = %CW >= 760 ? 300 : 230;
   %LW = %CW - %cardW - 10;
   %cTime = %LW - 22 - 110;
   WdnBanList.columns = "6" SPC %cTime;

   wdnPlace( WdnBanHeadName, 8, 0, 240, 18 );
   wdnPlace( WdnBanHeadTime, %cTime + 2, 0, 110, 18 );
   wdnPlaceScroll( WdnBanListScroll, WdnBanList, 0, 20, %LW, %CH - 20 - 32 );
   wdnPlaceBtn( WdnBanRefreshBtn, WdnBanRefreshIcon, 0, %CH - 26, 110, 26 );
   wdnPlaceBtn( WdnBanPermaBtn, WdnBanPermaIcon, %LW - 270, %CH - 26, 160, 26 );
   wdnPlaceBtn( WdnBanUnbanBtn, WdnBanUnbanIcon, %LW - 104, %CH - 26, 104, 26 );

   wdnPlace( WdnBanCard, %LW + 10, 0, %cardW, %CH );
   wdnPlace( WdnBanLabel, 12, 8, %cardW - 24, 18 );
   wdnPlace( WdnBanHelp, 12, 32, %cardW - 24, %CH - 44 );
}

//------------------------------------------------------------------------------
function wardenLayoutServer( %CW, %CH )
{
   %LW = mFloor( ( %CW - 10 ) * 0.46 );
   %RW = %CW - 10 - %LW;

   wdnPlace( WdnSrvLeft, 0, 0, %LW, %CH );
   wdnPlace( WdnAuthLabel, 12, 8, %LW - 24, 18 );
   wdnPlace( WdnAuthHint, 12, 28, %LW - 24, 16 );
   wdnPlace( WdnAuthEdit, 12, 48, %LW - 24 - 106, 26 );
   wdnPlaceBtn( WdnAuthBtn, WdnAuthIcon, %LW - 112, 48, 100, 26 );
   wdnPlace( WdnSrvLine, 1, 88, %LW - 2, 1 );
   wdnPlace( WdnCastLabel, 12, 98, %LW - 24, 18 );
   wdnPlace( WdnCastEdit, 12, 120, %LW - 24, 26 );
   %bw = mFloor( ( %LW - 24 - 6 ) / 2 );
   wdnPlaceBtn( WdnCastCenterBtn, WdnCastCenterIcon, 12, 152, %bw, 26 );
   wdnPlaceBtn( WdnCastBottomBtn, WdnCastBottomIcon, 12 + %bw + 6, 152, %bw, 26 );

   wdnPlace( WdnSrvRight, %LW + 10, 0, %RW, %CH );
   wdnPlace( WdnSetLabel, 12, 8, %RW - 24, 18 );
   %listH = %CH - 30 - 100;
   wdnPlaceScroll( WdnSetListScroll, WdnSetList, 12, 30, %RW - 24, %listH );
   wdnPlace( WdnSetHelp, 12, 30 + %listH + 6, %RW - 24, 52 );
   wdnPlace( WdnSetEdit, 12, %CH - 36, %RW - 24 - 106, 26 );
   wdnPlace( WdnSetEditMasked, 12, %CH - 36, %RW - 24 - 106, 26 );
   wdnPlaceBtn( WdnSetBtn, WdnSetIcon, %RW - 112, %CH - 36, 100, 26 );
}

//------------------------------------------------------------------------------
function wardenLayoutPanel( %CW, %CH )
{
   wdnPlace( WdnPanelTitle, 2, 0, %CW - 4, 24 );
   wdnPlace( WdnPanelSub, 2, 26, %CW - 4, 20 );
   wdnPlaceScroll( WdnPanelLinesScroll, WdnPanelLines, 0, 50, %CW, %CH - 50 - 32 );
   wdnPlaceBtn( WdnPanelHomeBtn, WdnPanelHomeIcon, 0, %CH - 26, 150, 26 );
   wdnPlaceBtn( WdnPanelResetBtn, WdnPanelResetIcon, 156, %CH - 26, 140, 26 );
   wdnPlace( WdnPanelHint, 306, %CH - 22, %CW - 306, 16 );
   $Warden::PanelW = %CW - 22;
}

//------------------------------------------------------------------------------
function wardenLayoutLog( %CW, %CH )
{
   WdnLogList.columns = "6 74";
   wdnPlace( WdnLogLabel, 2, 0, 300, 18 );
   wdnPlaceScroll( WdnLogListScroll, WdnLogList, 0, 20, %CW, %CH - 20 - 32 );
   wdnPlace( WdnLogClearBtn, 0, %CH - 26, 90, 26 );
   wdnPlace( WdnLogEmpty, 100, %CH - 22, %CW - 100, 16 );
}

//------------------------------------------------------------------------------
function wardenLayoutAbout( %CW, %CH )
{
   %w = wdnClamp( %CW, 400, 640 );
   wdnPlace( WdnAboutCard, 0, 0, %w, %CH );
   wdnPlace( WdnAboutLogo, 16, 14, 56, 56 );
   // The wordmark bitmap is 512x32; its text fills roughly the left half.
   wdnPlace( WdnAboutWordmark, 84, 24, 384, 24 );
   wdnPlace( WdnAboutVersion, 86, 52, %w - 100, 16 );
   wdnPlace( WdnAboutTop, 1, 1, %w - 2, 2 );
   wdnPlace( WdnAboutLine, 1, 82, %w - 2, 1 );
   wdnPlace( WdnAboutText, 16, 94, %w - 32, %CH - 94 - 10 );
}

//------------------------------------------------------------------------------
function wardenShowTabMark()
{
   %tab = $Warden::Tab;
   for ( %i = 0; %i < $Warden::TabCount; %i++ )
   {
      %page = "WdnPage" @ %i;
      %btn = "WdnTabBtn" @ %i;
      %page.setVisible( %i == %tab );
      if ( %i == %tab )
         wdnSetStyle( %btn, WdnTabActiveProfile );
      else
         wdnSetStyle( %btn, WdnTabProfile );
   }
   wdnPlace( WdnTabMark, 8 + %tab * $Warden::TabW, 30, $Warden::TabW - 2, 2 );
}
