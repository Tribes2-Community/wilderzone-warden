//------------------------------------------------------------------------------
// Wilderzone Warden - admin and super-admin console for Tribes 2 Classic /
// TacoServer.
//
// Client side only: it uses the commands and menus the server already offers.
// Open it with the "Wilderzone Warden" key (Options > Controls) or CTRL-F9.
//------------------------------------------------------------------------------

// A dedicated server has no canvas; the console is for clients only.
if ( !$Warden::Loaded && $LaunchMode !$= "DedicatedServer" )
{
   $Warden::Loaded = true;

   exec( "scripts/warden/profiles.cs" );
   exec( "scripts/warden/gui.cs" );
   exec( "scripts/warden/players.cs" );
   exec( "scripts/warden/match.cs" );
   exec( "scripts/warden/panel.cs" );
   exec( "scripts/warden/server.cs" );
   exec( "scripts/warden/update.cs" );
   exec( "scripts/warden/core.cs" );
}
