//------------------------------------------------------------------------------
// Wilderzone Warden - control profiles (the dark skin).
//
// Everything is drawn from flat fills, borders and the bitmaps in
// textures/gui/warden; no stock shell art is used.
//------------------------------------------------------------------------------

if ( !isObject( WdnDimProfile ) )
{
   // Full-screen dim behind the console; modal, so clicks do not reach the game.
   new GuiControlProfile( WdnDimProfile )
   {
      opaque = true;
      fillColor = "0 0 0 150";
   };

   new GuiControlProfile( WdnFrameProfile )
   {
      opaque = true;
      fillColor = "14 18 23 250";
      border = true;
      borderColor = "58 72 92";
   };

   new GuiControlProfile( WdnHeaderProfile )
   {
      opaque = true;
      fillColor = "21 27 35";
   };

   new GuiControlProfile( WdnPanelProfile )
   {
      opaque = true;
      fillColor = "21 27 35";
      border = true;
      borderColor = "46 58 74";
   };

   new GuiControlProfile( WdnAccentProfile )
   {
      opaque = true;
      fillColor = "79 195 247";
   };

   // Brand red (the console's danger colour), used for the header underline and the About tab only.
   new GuiControlProfile( WdnRedLineProfile )
   {
      opaque = true;
      fillColor = "255 96 96";
   };

   new GuiControlProfile( WdnLineProfile )
   {
      opaque = true;
      fillColor = "46 58 74";
   };

   // Plain container: draws nothing.
   new GuiControlProfile( WdnNoneProfile )
   {
      opaque = false;
   };

   // Icons sit on top of buttons. What lets a click through to the button is the
   // icon control's own modal = "0" (set in wdnIcon), as stock overlay dialogs do.
   new GuiControlProfile( WdnIconProfile )
   {
      opaque = false;
      modal = false;
   };

   // Buttons are drawn in three layers, back to front: a "face" box (fill and
   // border), the icon, and the button itself with a see-through fill. The
   // button is on top so every click on it, icon included, is the button's.
   // Each button profile below therefore has a matching ...FaceProfile.
   new GuiControlProfile( WdnBtnFaceProfile )
   {
      opaque = true;
      fillColor = "29 37 48";
      border = true;
      borderColor = "46 58 74";
   };

   new GuiControlProfile( WdnBtnProfile )
   {
      opaque = true;
      fillColor = "0 0 0 0";
      fillColorHL = "255 255 255 22";
      fillColorNA = "0 0 0 0";
      border = true;
      borderColor = "46 58 74";
      borderColorHL = "79 195 247";
      borderColorNA = "34 42 54";
      fontType = "Univers Condensed";
      fontSize = 16;
      fontColor = "210 220 232";
      fontColorHL = "255 255 255";
      fontColorNA = "86 96 110";
      fixedExtent = true;
      justify = "center";
      soundButtonDown = sButtonDown;
      soundButtonOver = sButtonOver;
      canKeyFocus = false;
   };

   // Destructive actions (kick, ban, strip, explode, ...).
   new GuiControlProfile( WdnBtnDangerFaceProfile )
   {
      opaque = true;
      fillColor = "44 28 32";
      border = true;
      borderColor = "96 48 52";
   };

   new GuiControlProfile( WdnBtnDangerProfile )
   {
      opaque = true;
      fillColor = "0 0 0 0";
      fillColorHL = "255 255 255 22";
      fillColorNA = "0 0 0 0";
      border = true;
      borderColor = "96 48 52";
      borderColorHL = "255 96 96";
      borderColorNA = "34 42 54";
      fontType = "Univers Condensed";
      fontSize = 16;
      fontColor = "255 170 170";
      fontColorHL = "255 255 255";
      fontColorNA = "86 96 110";
      fixedExtent = true;
      justify = "center";
      soundButtonDown = sButtonDown;
      soundButtonOver = sButtonOver;
      canKeyFocus = false;
   };

   // A button armed for its second, confirming click.
   new GuiControlProfile( WdnBtnArmedFaceProfile )
   {
      opaque = true;
      fillColor = "150 40 44";
      border = true;
      borderColor = "255 96 96";
   };

   new GuiControlProfile( WdnBtnArmedProfile )
   {
      opaque = true;
      fillColor = "0 0 0 0";
      fillColorHL = "255 255 255 22";
      border = true;
      borderColor = "255 96 96";
      borderColorHL = "255 255 255";
      fontType = "Univers Condensed";
      fontSize = 16;
      fontColor = "255 255 255";
      fontColorHL = "255 255 255";
      fontColorNA = "86 96 110";
      fixedExtent = true;
      justify = "center";
      soundButtonDown = sButtonDown;
      soundButtonOver = sButtonOver;
      canKeyFocus = false;
   };

   new GuiControlProfile( WdnBtnPrimaryFaceProfile )
   {
      opaque = true;
      fillColor = "24 70 96";
      border = true;
      borderColor = "79 195 247";
   };

   new GuiControlProfile( WdnBtnPrimaryProfile )
   {
      opaque = true;
      fillColor = "0 0 0 0";
      fillColorHL = "255 255 255 22";
      fillColorNA = "0 0 0 0";
      border = true;
      borderColor = "79 195 247";
      borderColorHL = "190 232 255";
      borderColorNA = "34 42 54";
      fontType = "Univers Condensed";
      fontSize = 16;
      fontColor = "230 244 255";
      fontColorHL = "255 255 255";
      fontColorNA = "86 96 110";
      fixedExtent = true;
      justify = "center";
      soundButtonDown = sButtonDown;
      soundButtonOver = sButtonOver;
      canKeyFocus = false;
   };

   new GuiControlProfile( WdnTabFaceProfile )
   {
      opaque = true;
      fillColor = "21 27 35";
   };

   new GuiControlProfile( WdnTabProfile )
   {
      opaque = true;
      fillColor = "0 0 0 0";
      fillColorHL = "255 255 255 22";
      border = false;
      fontType = "Univers Condensed";
      fontSize = 18;
      fontColor = "126 138 153";
      fontColorHL = "210 220 232";
      fixedExtent = true;
      justify = "center";
      soundButtonDown = sButtonDown;
      soundButtonOver = sButtonOver;
      canKeyFocus = false;
   };

   new GuiControlProfile( WdnTabActiveFaceProfile )
   {
      opaque = true;
      fillColor = "14 18 23";
   };

   new GuiControlProfile( WdnTabActiveProfile )
   {
      opaque = true;
      fillColor = "0 0 0 0";
      fillColorHL = "255 255 255 22";
      border = false;
      fontType = "Univers Condensed";
      fontSize = 18;
      fontColor = "255 255 255";
      fontColorHL = "255 255 255";
      fixedExtent = true;
      justify = "center";
      soundButtonDown = sButtonDown;
      soundButtonOver = sButtonOver;
      canKeyFocus = false;
   };

   new GuiControlProfile( WdnTextProfile )
   {
      fontType = "Univers";
      fontSize = 16;
      fontColor = "210 220 232";
      autoSizeWidth = false;
      autoSizeHeight = false;
   };

   new GuiControlProfile( WdnTextDimProfile )
   {
      fontType = "Univers";
      fontSize = 14;
      fontColor = "126 138 153";
      autoSizeWidth = false;
      autoSizeHeight = false;
   };

   new GuiControlProfile( WdnTextRightProfile )
   {
      fontType = "Univers";
      fontSize = 16;
      fontColor = "126 138 153";
      justify = "right";
      autoSizeWidth = false;
      autoSizeHeight = false;
   };

   // Small capitals over a section.
   new GuiControlProfile( WdnLabelProfile )
   {
      fontType = "Univers Condensed";
      fontSize = 16;
      fontColor = "79 195 247";
      autoSizeWidth = false;
      autoSizeHeight = false;
   };

   new GuiControlProfile( WdnTitleProfile )
   {
      fontType = "Univers Condensed";
      fontSize = 22;
      fontColor = "255 255 255";
      autoSizeWidth = false;
      autoSizeHeight = false;
   };

   new GuiControlProfile( WdnMLProfile )
   {
      fontType = "Univers";
      fontSize = 16;
      fontColor = "210 220 232";
      fontColorHL = "79 195 247";
      autoSizeWidth = false;
      autoSizeHeight = false;
      tab = false;
      canKeyFocus = false;
   };

   new GuiControlProfile( WdnListProfile )
   {
      fontType = "Univers";
      fontSize = 16;
      fontColor = "196 208 222";
      fontColorHL = "255 255 255";
      fontColorNA = "79 195 247";
      fontColorSEL = "255 255 255";
      fillColorHL = "30 84 116";
      tab = true;
      canKeyFocus = true;
   };

   new GuiControlProfile( WdnScrollProfile )
   {
      opaque = true;
      fillColor = "10 13 17";
      border = true;
      borderColor = "46 58 74";
      bitmap = "gui/warden/scroll";
   };

   new GuiControlProfile( WdnScrollContentProfile )
   {
      opaque = false;
      autoSizeWidth = true;
      autoSizeHeight = true;
      border = false;
   };

   new GuiControlProfile( WdnEditProfile )
   {
      opaque = true;
      fillColor = "10 13 17";
      fillColorHL = "30 84 116";
      border = true;
      borderColor = "58 72 92";
      fontType = "Univers";
      fontSize = 16;
      fontColor = "230 238 246";
      fontColorHL = "255 255 255";
      fontColorNA = "86 96 110";
      cursorColor = "79 195 247";
      textOffset = "6 2";
      autoSizeWidth = false;
      autoSizeHeight = false;
      tab = true;
      canKeyFocus = true;
   };
}
