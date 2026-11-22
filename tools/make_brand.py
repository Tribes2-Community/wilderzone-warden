"""Draws the Wilderzone Warden wordmark, shield logo and About tab icon.

Three-colour scheme, shared with the console skin (profiles.cs):
  white  #FFFFFF  - WILDER
  blue   #4FC3F7  - ZONE   (the Warden accent colour, "79 195 247")
  red    #FF6060  - WARDEN (the Warden danger colour, "255 96 96")

Usage: python tools/make_brand.py <font.ttf> src/textures/gui/warden
Font: Squada One (as in the Wilderzone SVG) if you have it, otherwise Impact.
"""
import sys
from PIL import Image, ImageChops, ImageDraw, ImageFont, ImageFilter

WHITE, BLUE, RED = (255, 255, 255), (79, 195, 247), (255, 96, 96)
BLACK = (0, 0, 0)
INK = (14, 18, 23)          # the console frame fill
SS = 8                      # supersampling

font_path, out_dir = sys.argv[1], sys.argv[2]


def shear(img, k):
    """Italic: slant to the right about the bottom edge."""
    w, h = img.size
    return img.transform((w + int(h * k), h), Image.AFFINE, (1, k, -h * k, 0, 1, 0), Image.BICUBIC)


def wordmark():
    W, H = 512 * SS, 32 * SS
    size = int(21 * SS / 0.74)
    f = ImageFont.truetype(font_path, size)
    parts = [("WILDER", WHITE), ("ZONE", BLUE), (" ", BLUE), ("WARDEN", RED)]
    bb = f.getbbox('WILDERZONE WARDEN')
    x0, y0 = 7 * SS, int(16 * SS - (bb[1] + bb[3]) / 2) + SS
    stroke = int(2.2 * SS)

    def layer(fill_pass):
        im = Image.new("RGBA", (W, H), (0, 0, 0, 0))
        d = ImageDraw.Draw(im)
        x = x0
        for text, col in parts:
            d.text((x, y0), text, font=f, fill=col if fill_pass else BLACK,
                   stroke_width=stroke, stroke_fill=BLACK)
            x += f.getlength(text) + 1.5 * SS
        return im

    base = layer(True)
    shadow = layer(False).filter(ImageFilter.GaussianBlur(1.6 * SS))
    shadow = ImageChops_offset(shadow, 0, int(1.6 * SS))
    out = Image.alpha_composite(shadow, base)
    out = shear(out, 0.16).crop((0, 0, W, H))
    return out.resize((512, 32), Image.LANCZOS)


def ImageChops_offset(im, dx, dy):
    from PIL import ImageChops
    return ImageChops.offset(im, dx, dy)


def shield_points(S, inset=0.0):
    """A heater shield, as a polygon, in an S x S box."""
    m = S * (0.07 + inset)
    l, r, t = m, S - m, S * 0.08 + m * 0.4
    pts = [(l, t + S * 0.06), (S / 2, t - S * 0.02), (r, t + S * 0.06), (r, S * 0.52)]
    # right curve down to the point, then back up the left side
    n = 24
    for i in range(1, n):
        u = i / n
        x = r + (S / 2 - r) * (u ** 1.6)
        y = S * 0.52 + (S * 0.93 - S * 0.52) * (u ** 0.8)
        pts.append((x, y))
    pts.append((S / 2, S * 0.93 - m * 0.3))
    for i in range(n - 1, 0, -1):
        u = i / n
        x = l + (S / 2 - l) * (u ** 1.6)
        y = S * 0.52 + (S * 0.93 - S * 0.52) * (u ** 0.8)
        pts.append((x, y))
    pts.append((l, S * 0.52))
    return pts


def logo():
    S = 64 * SS
    im = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.polygon(shield_points(S, -0.02), fill=BLACK)            # outline
    d.polygon(shield_points(S, 0.02), fill=BLUE)              # border
    d.polygon(shield_points(S, 0.075), fill=INK)              # field
    # red chevron near the point
    cx = S / 2
    d.polygon([(cx - S * .22, S * .60), (cx, S * .76), (cx + S * .22, S * .60),
               (cx + S * .22, S * .67), (cx, S * .83), (cx - S * .22, S * .67)], fill=RED)
    # "WZ" in the title style: W white (WILDER), Z blue (ZONE), italic, black stroke, drop shadow
    f = ImageFont.truetype(font_path, int(S * 0.50))
    st = int(S * 0.028)
    wl, zl = f.getlength("W"), f.getlength("Z")
    bb = f.getbbox("WZ")
    x0 = (S - (wl + zl)) / 2 - S * 0.03
    y0 = S * 0.36 - (bb[1] + bb[3]) / 2

    def tl(fill_pass):
        t = Image.new("RGBA", (S, S), (0, 0, 0, 0))
        td = ImageDraw.Draw(t)
        for ch, col, x in (("W", WHITE, x0), ("Z", BLUE, x0 + wl)):
            td.text((x, y0), ch, font=f, fill=col if fill_pass else BLACK, stroke_width=st, stroke_fill=BLACK)
        return t

    sh = ImageChops.offset(tl(False).filter(ImageFilter.GaussianBlur(S * 0.02)), 0, int(S * 0.025))
    txt = shear(Image.alpha_composite(sh, tl(True)), 0.16)
    txt = txt.crop((int(S * 0.08), 0, int(S * 0.08) + S, S))
    im = Image.alpha_composite(im, txt)
    return im.resize((64, 64), Image.LANCZOS)


def about_icon():
    S = 32 * SS
    im = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    c = S / 2
    d.ellipse((S * .08, S * .08, S * .92, S * .92), outline=(210, 220, 232), width=int(S * .07))
    d.ellipse((c - S * .06, S * .24, c + S * .06, S * .36), fill=(210, 220, 232))
    d.rounded_rectangle((c - S * .06, S * .44, c + S * .06, S * .74), radius=S * .03, fill=(210, 220, 232))
    return im.resize((32, 32), Image.LANCZOS)


wordmark().save(out_dir + "/wordmark.png")
logo().save(out_dir + "/logo.png")
about_icon().save(out_dir + "/tab_about.png")
print("ok")
