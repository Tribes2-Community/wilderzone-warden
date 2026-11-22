# Wilderzone Warden

![Wilderzone Warden](assets/wilderzone_warden.svg)

A client-side admin and super-admin console for Tribes 2 servers running Classic (1.5.2) and
TacoServer. It is one VL2; nothing is installed on the server. Everything it does goes through
commands and menus the server already offers, so what you see is what your rank allows there.

Tabs: **Players**, **Match**, **Bans**, **Server**, **Panel**, **Log**, **About**.

## Install

Build the VL2 (below) or take it from a release, copy `zz_WilderzoneWarden.vl2` into
`GameData/base` of your Tribes 2 client and restart. Open the console in a game with **Ctrl-F9**
(or bind a key under Options > Controls > "Wilderzone Warden"); **Esc** closes it.

## Build

```
python tools/pack.py        # -> dist/zz_WilderzoneWarden.vl2  (needs Python 3)
```

`python tools/make_brand.py <font.ttf> src/textures/gui/warden` redraws the logo, wordmark and
About icon (needs Pillow). Full documentation, including how each tab talks to the server and its
known limits, is in [`src/WilderzoneWarden_readme.txt`](src/WilderzoneWarden_readme.txt).

## Releases and update check

- **Shipping a version:** bump `$Warden::Version` in `src/scripts/warden/core.cs` (for example `"1.1"`)
  and push to `main`. The [Release workflow](.github/workflows/release.yml) builds the VL2 and publishes
  a `v1.1` release with `zz_WilderzoneWarden.vl2` attached. A push that does not change the version
  builds nothing.
- **Update check in the console:** when the console is first opened in a session it asks GitHub for the
  latest release and says so in the About tab and the Log if a newer version exists. The About tab has a
  CHECK FOR UPDATES / GET UPDATE button (GET UPDATE opens the releases page). This needs a client whose
  `HTTPObject` supports HTTPS, such as the QOL client; on other clients it reports that GitHub could not
  be reached and the button opens the releases page. Set `$pref::Warden::CheckUpdates = false;` to turn the
  automatic check off.

## Credits

Created by [GeekOfWires](https://github.com/GeekOfWires), with co-authors Claude Opus 5.5 and
Claude Sonnet 5.5 (Anthropic).

Built on Tribes 2 by Dynamix, the admin commands of Classic (z0dd's Admin Hud) and TacoServer's
Server Admin Panel.

## License

[MIT](LICENSE)
