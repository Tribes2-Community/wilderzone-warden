"""Draws the Discord release banner (1600x900).

Usage: python tools/make_banner.py <SquadaOne-Regular.ttf> <screenshot.png|-> <out.png> [version]

The title uses the same font and treatment as assets/wilderzone_warden.svg:
Squada One, italic, condensed, black stroke and drop shadow, white WILDER /
blue ZONE / red WARDEN. Pass "-" instead of a screenshot to leave a placeholder.
"""
import sys
from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

font_path, shot_path, out_path = sys.argv[1:4]
version = sys.argv[4] if len(sys.argv) > 4 else "1.2"

WHITE, BLUE, RED, BLACK = (255, 255, 255), (79, 195, 247), (255, 96, 96), (0, 0, 0)
INK, PANEL, LINE, DIM, TEXT = (14, 18, 23), (21, 27, 35), (46, 58, 74), (126, 138, 153), (210, 220, 232)
W, H = 1600, 900


def shear(img, k):
    w, h = img.size
    return img.transform((w + int(h * k), h), Image.AFFINE, (1, k, -h * k, 0, 1, 0), Image.BICUBIC)


def title_line(parts, size, stroke, xscale=0.62, slant=0.16):
    """One line of the title in the SVG's style; returns an RGBA image."""
    f = ImageFont.truetype(font_path, size)
    pad = stroke * 3 + 20
    total = sum(f.getlength(t) for t, _ in parts)
    lw, lh = int(total) + pad * 2, int(size * 1.3) + pad * 2

    def layer(fill):
        im = Image.new("RGBA", (lw, lh), (0, 0, 0, 0))
        d = ImageDraw.Draw(im)
        x = pad
        for text, col in parts:
            d.text((x, pad), text, font=f, fill=col if fill else BLACK, stroke_width=stroke, stroke_fill=BLACK)
            x += f.getlength(text)
        return im

    shadow = ImageChops.offset(layer(False).filter(ImageFilter.GaussianBlur(stroke * 1.4)), 0, int(stroke * 1.6))
    im = Image.alpha_composite(shadow, layer(True))
    im = im.resize((int(im.width * xscale), im.height), Image.LANCZOS)
    return shear(im, slant)


def glow(canvas, cx, cy, r, col, alpha):
    g = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    ImageDraw.Draw(g).ellipse((cx - r, cy - r, cx + r, cy + r), fill=col + (alpha,))
    canvas.alpha_composite(g.filter(ImageFilter.GaussianBlur(r / 2)))


# ---- background
bg = Image.new("RGBA", (W, H), INK)
d = ImageDraw.Draw(bg)
for y in range(H):
    t = y / H
    d.line([(0, y), (W, y)], fill=(int(16 - 8 * t), int(21 - 10 * t), int(28 - 12 * t), 255))
for x in range(-H, W, 56):                      # faint diagonal grid
    d.line([(x, H), (x + H, 0)], fill=(255, 255, 255, 3), width=1)
glow(bg, 1250, 470, 420, BLUE, 38)
glow(bg, 150, 120, 320, RED, 30)
d = ImageDraw.Draw(bg)
d.rectangle((0, 0, W, 8), fill=RED)
d.rectangle((0, H - 4, W, H), fill=BLUE)

# ---- shield logo + single-line title (as in the SVG)
try:
    logo = Image.open("src/textures/gui/warden/logo.png").convert("RGBA").resize((150, 150), Image.LANCZOS)
    bg.alpha_composite(logo, (70, 70))
except OSError:
    pass
title = [("WILDER", WHITE), ("ZONE", BLUE), (" ", BLUE), ("WARDEN", RED)]
tl = title_line(title, 215, 9, xscale=0.66)
bg.alpha_composite(tl, (250, 40))

# ---- dark panel behind the text so it stays readable over the pattern
panel = Image.new("RGBA", (W, H), (0, 0, 0, 0))
pd = ImageDraw.Draw(panel)
pd.rounded_rectangle((52, 296, 640, H - 26), radius=14, fill=(10, 13, 18, 232), outline=LINE + (255,), width=2)
bg.alpha_composite(panel)

# ---- release pill
pill_f = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 40)
pill = "NOW AVAILABLE  -  v" + version
pw = int(pill_f.getlength(pill)) + 56
d = ImageDraw.Draw(bg)
d.rounded_rectangle((80, 318, 80 + pw, 318 + 68), radius=10, fill=RED)
d.text((80 + 28, 318 + 34), pill, font=pill_f, fill=BLACK, anchor="lm")

# ---- tagline + feature rows (icon, title, one line each)
tag_f = ImageFont.truetype(font_path, 34)
d.text((84, 404), "ADMIN CONSOLE FOR TRIBES 2", font=tag_f, fill=WHITE)
d.text((84, 438), "CLASSIC AND TACOSERVER", font=tag_f, fill=WHITE)

title_f = ImageFont.truetype(font_path, 36)
desc_f = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 23)
rows = [
    ("tab_players", "ONE CONSOLE", "Players, matches, bans, server settings"),
    ("icon_key", "SAVED LOGINS", "Admin passwords saved per server"),
    ("icon_refresh", "UPDATE CHECK", "In-game notice of new versions"),
    ("tab_server", "CLIENT-SIDE", "Nothing to install on the server"),
]
y = 498
for i, (icon, head, desc) in enumerate(rows):
    if i:
        d.line([(84, y - 12), (608, y - 12)], fill=LINE, width=1)
    box = Image.new("RGBA", (56, 56), (79, 195, 247, 38))
    bd = ImageDraw.Draw(box)
    bd.rectangle((0, 0, 55, 55), outline=BLUE + (160,), width=2)
    ic = Image.open("src/textures/gui/warden/%s.png" % icon).convert("RGBA").resize((32, 32), Image.LANCZOS)
    tint = Image.new("RGBA", ic.size, BLUE + (255,))
    tint.putalpha(ic.getchannel("A"))
    box.alpha_composite(tint, (12, 12))
    bg.alpha_composite(box, (84, y))
    d.text((158, y - 6), head, font=title_f, fill=BLUE)
    d.text((158, y + 31), desc, font=desc_f, fill=TEXT)
    y += 80

# ---- footer
url_f = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 23)
d.line([(84, H - 84), (608, H - 84)], fill=RED, width=2)
d.text((84, H - 70), "github.com/Tribes2-Community/wilderzone-warden", font=url_f, fill=WHITE)

# ---- screenshot frame
bx, by, bw, bh = 672, 300, 888, 548
shadow = Image.new("RGBA", (W, H), (0, 0, 0, 0))
ImageDraw.Draw(shadow).rectangle((bx + 8, by + 14, bx + bw + 8, by + bh + 14), fill=(0, 0, 0, 200))
bg.alpha_composite(shadow.filter(ImageFilter.GaussianBlur(18)))
d = ImageDraw.Draw(bg)
d.rectangle((bx - 3, by - 3, bx + bw + 3, by + bh + 3), fill=BLUE)
d.rectangle((bx, by, bx + bw, by + bh), fill=PANEL)
if shot_path != "-":
    shot = Image.open(shot_path).convert("RGBA")
    if shot.size == (1280, 720):                      # in-game shot: keep just the console
        shot = shot.crop((120, 40, 1160, 682))
    s = min(bw / shot.width, bh / shot.height)
    shot = shot.resize((int(shot.width * s), int(shot.height * s)), Image.LANCZOS)
    bg.alpha_composite(shot, (bx + (bw - shot.width) // 2, by + (bh - shot.height) // 2))
else:
    d.text((bx + bw / 2, by + bh / 2), "SCREENSHOT HERE", font=pill_f, fill=DIM, anchor="mm")

bg.convert("RGB").save(out_path, optimize=True)
print("wrote", out_path)
