# Wilderzone Warden

A client-side admin and super-admin console for Tribes 2 servers running Classic (1.5.2) and
TacoServer. It is one VL2; nothing is installed on the server. Everything it does goes through
commands and menus the server already offers, so what you see is what your rank allows there.

## Install

Copy `zz_WilderzoneWarden.vl2` into `GameData/base` of your Tribes 2 client and restart it.

Open the console in a game with **Ctrl-F9**, or bind your own key under
Options > Controls > "Wilderzone Warden". **Esc** closes it.

To change or remove the Ctrl-F9 binding, set `$pref::Warden::GlobalKey` in the client console
(for example `$pref::Warden::GlobalKey = "ctrl f10";`, or `""` for none) and restart.

## Tabs

| Tab | What it does | Works on |
|---|---|---|
| Players | Player list with rank, team, score, ping. Select a player to get the actions the server offers you for them: kick, ban, warn, gag, freeze, mute, team change, force observer, make/strip admin, admin lists, private message, client info. Destructive actions need a second, confirming click. | Any server; Classic adds the extended actions |
| Match | The server's vote/admin menu as a list: change mission, next mission, time limit, skip, tournament mode, team damage and whatever else the server lists. Multi-step choices continue in the same list, with a filter box for long mission lists. | Any server |
| Bans | TacoServer's ban/kick list with the time left, **Unban** and **Make permanent**. | TacoServer, Super Admin |
| Server | Admin login (with saved per-server Admin / Super Admin passwords: SAVE, USE and FORGET buttons; stored in the client prefs in plain text, sent only to the server they were saved for), centre/bottom-print broadcast, and the Classic super-admin settings (passwords, player limit, random/fair teams, auto-password, map and game-type rotation, restart). The login box and the admin / super admin password boxes are masked, and the server's "PW changed to ..." confirmation is masked in the chat hud and the log. | Classic |
| Panel | TacoServer's Server Admin Panel (whitelist, auth queue, IP ban list, team balancer, stats options) shown inside the console, with working links. | TacoServer, Super Admin |
| Log | Every server message with text, time-stamped: replies to your commands, kicks, votes, admin notices. | Any server |
| About | Credit for the console, its version and what it is built on. Set `$Warden::Author` in `core.cs` to name the author(s). | Any server |

## How it talks to the server

* **Players** asks for the player popup menu (`GetPlayerPopupMenu`) and sends, for the entry you
  pick, the command the stock lobby sends for that entry's id.
* **Match** asks for the vote menu (`GetVoteMenu`) and replays the stock lobby's sequence exactly,
  which TacoServer's nested menus (next mission, "Vote to ...", skip confirmation) depend on.
* **Bans** and **Panel** use TacoServer's score-screen panel: they send the same `ProcessGameLink`
  requests its links send and read the score-screen lines that come back. While a panel is shown the
  server pauses your real score screen, so Warden sends its "Reset" when you leave those tabs or
  close the console.

Known limits, all on the server side:

* A ban made permanent is hidden from TacoServer's list and can then only be lifted from the server
  console (`unbanIndex();`) or by editing `prefs/dtBanlist.cs`.
* Bans in Classic's separate permanent list (`prefs/banlist.cs`) are not shown; no server command
  exposes them.
* If the server was started with TacoServer's stats disabled, its panel and ban list do not exist.

## Building

```
python tools/make_brand.py <font.ttf> src/textures/gui/warden   (optional: redraws the logo, wordmark, About icon)
python tools/pack.py                                            (writes dist/zz_WilderzoneWarden.vl2)
```

`make_brand.py` draws the logo, wordmark and About-tab icon from primitives in the Wilderzone title
style (white WILDER, blue ZONE, red WARDEN). `assets/wilderzone_warden.svg` is the same title as vector art.

Layout:

```
src/scripts/autoexec/wilderzoneWarden.cs   loader
src/scripts/warden/                        profiles (the skin), gui, core, players, match, panel, server
src/textures/gui/warden/                   bitmaps
assets/wilderzone_warden.svg               title artwork
tools/                                     make_brand.py, pack.py
```
