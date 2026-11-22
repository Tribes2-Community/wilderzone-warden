//------------------------------------------------------------------------------
// Wilderzone Warden - Server tab.
//
// Admin login, broadcast messages, and the Classic super-admin settings that
// z0dd's Admin Hud exposes ('Set', 'AutoPWSetup', map/type rotation), with what
// each one expects written out.
//------------------------------------------------------------------------------

$Warden::SetCount = 0;

// %command/%type: commandToServer( %command, %type, value ), or with no %type
// commandToServer( %command, value ).
// %secret: the value is typed into a masked field.
function wardenAddSetting( %label, %command, %type, %help, %confirm, %secret )
{
   %n = $Warden::SetCount;
   $Warden::SetSecret[%n] = %secret;
   $Warden::SetLabel[%n] = %label;
   $Warden::SetCommand[%n] = %command;
   $Warden::SetType[%n] = %type;
   $Warden::SetHelp[%n] = %help;
   $Warden::SetConfirm[%n] = %confirm;
   $Warden::SetCount++;
}

wardenAddSetting( "Join password", "Set", "joinpw", "The password players need to join. Enter \"remove\" to clear it.", false );
wardenAddSetting( "Admin password", "Set", "adminpw", "The password that grants Admin when entered under Admin login. Hidden as you type.", false, true );
wardenAddSetting( "Super Admin password", "Set", "superpw", "The password that grants Super Admin when entered under Admin login. Hidden as you type.", false, true );
wardenAddSetting( "Player limit", "Set", "maxplayers", "Maximum number of players: a positive number.", false );
wardenAddSetting( "Random teams", "Set", "random", "1 turns random teams on, 0 off. Takes effect next mission.", false );
wardenAddSetting( "Fair teams", "Set", "fairteams", "1 turns fair teams on, 0 off.", false );
wardenAddSetting( "Auto-password", "AutoPWSetup", "autopw", "1 turns the automatic join password on, 0 off.", false );
wardenAddSetting( "Auto-password: password", "AutoPWSetup", "autopwpass", "The join password applied automatically.", false );
wardenAddSetting( "Auto-password: player count", "AutoPWSetup", "autopwcount", "Player count at which the automatic password is applied.", false );
wardenAddSetting( "Take map out of rotation", "AddMap", "", "Mission file name to skip in the rotation, e.g. Katabatic.", false );
wardenAddSetting( "Put map back in rotation", "RemoveMap", "", "Mission file name to stop skipping.", false );
wardenAddSetting( "Take game type out of rotation", "AddType", "", "Game type to skip, e.g. CTF.", false );
wardenAddSetting( "Put game type back in rotation", "RemoveType", "", "Game type to stop skipping.", false );
wardenAddSetting( "Restart server", "Set", "restart", "1 restarts the server in 30 seconds. 0 cancels a restart scheduled for mission end.", true );

//------------------------------------------------------------------------------
function wardenServerEnter()
{
   if ( WdnSetList.rowCount() == 0 )
   {
      for ( %i = 0; %i < $Warden::SetCount; %i++ )
         WdnSetList.addRow( %i, $Warden::SetLabel[%i] );
      WdnSetList.setSelectedById( 0 );
   }
   wardenSettingShow();
}

function WdnSetList::onSelect( %this, %id, %text )
{
   wardenDisarm();
   wardenSettingShow();
}

function wardenSettingShow()
{
   %id = WdnSetList.getSelectedId();
   if ( %id $= "" || %id < 0 || %id >= $Warden::SetCount )
   {
      WdnSetHelp.setText( "<color:7E8A99>Select a setting." );
      WdnSetEdit.setVisible( true );
      WdnSetEditMasked.setVisible( false );
      return;
   }
   WdnSetHelp.setText( "<color:D2DCE8>" @ $Warden::SetLabel[%id] @ "\n<color:7E8A99>" @ $Warden::SetHelp[%id] );

   // Swap the plain and masked fields; nothing typed carries over between them.
   %secret = $Warden::SetSecret[%id] ? true : false;
   WdnSetEdit.setValue( "" );
   WdnSetEditMasked.setValue( "" );
   WdnSetEdit.setVisible( !%secret );
   WdnSetEditMasked.setVisible( %secret );
}

// The field in use for the selected setting.
function wardenSettingField()
{
   %id = WdnSetList.getSelectedId();
   if ( %id !$= "" && %id >= 0 && $Warden::SetSecret[%id] )
      return WdnSetEditMasked;
   return WdnSetEdit;
}

function wardenSettingApply()
{
   %id = WdnSetList.getSelectedId();
   if ( %id $= "" || %id < 0 || %id >= $Warden::SetCount )
      return;

   %field = wardenSettingField();
   %value = trim( %field.getValue() );
   if ( %value $= "" )
   {
      wardenSetStatus( "Enter a value first." );
      return;
   }

   if ( $Warden::SetConfirm[%id] && !wardenArm( WdnSetBtn, "CONFIRM?", "APPLY", WdnBtnPrimaryProfile ) )
      return;

   %type = $Warden::SetType[%id];
   switch$ ( $Warden::SetCommand[%id] )
   {
      case "Set":
         commandToServer( 'Set', %type, %value );
      case "AutoPWSetup":
         commandToServer( 'AutoPWSetup', %type, %value );
      case "AddMap":
         commandToServer( 'AddMap', %value );
      case "RemoveMap":
         commandToServer( 'RemoveMap', %value );
      case "AddType":
         commandToServer( 'AddType', %value );
      case "RemoveType":
         commandToServer( 'RemoveType', %value );
   }

   wardenLogAdd( "Sent: " @ $Warden::SetLabel[%id] );
   %field.setValue( "" );
}

//------------------------------------------------------------------------------
function wardenLogin()
{
   %password = WdnAuthEdit.getValue();
   if ( %password $= "" )
      return;

   commandToServer( 'SAD', %password );
   WdnAuthEdit.setValue( "" );
   wardenSetStatus( "Password sent." );
   // The rank change, if any, comes back as a message; show it when it lands.
   schedule( 800, 0, wardenUpdateHeader );
}

function wardenBroadcast( %bottom )
{
   %text = trim( WdnCastEdit.getValue() );
   if ( %text $= "" )
      return;

   commandToServer( 'aprint', %text, %bottom );
   wardenLogAdd( ( %bottom ? "Bottom print: " : "Center print: " ) @ %text );
   WdnCastEdit.setValue( "" );
}
