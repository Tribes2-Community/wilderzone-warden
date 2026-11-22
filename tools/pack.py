"""Packs the VL2: python tools/pack.py  ->  dist/zz_WilderzoneWarden.vl2"""
import os, zipfile
os.chdir(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'src'))
readme = 'WilderzoneWarden_readme.txt'
s = open(readme, encoding='utf-8', newline='').read()
row = '| About | Credit for the console, its version and what it is built on. Set `$Warden::Author` in `core.cs` to name the author(s). | Any server |'
if '| About |' not in s:
    nl = '\r\n' if '\r\n' in s else '\n'
    old = '| Log | Every server message with text, time-stamped: replies to your commands, kicks, votes, admin notices. | Any server |'
    assert old in s
    s = s.replace(old, old + nl + row)
    open(readme, 'w', encoding='utf-8', newline='').write(s)
os.makedirs('../dist', exist_ok=True)
with zipfile.ZipFile('../dist/zz_WilderzoneWarden.vl2', 'w', zipfile.ZIP_DEFLATED) as z:
    for root in ('scripts', 'textures'):
        for d, _, fs in os.walk(root):
            for f in fs:
                if not f.endswith('.dso'):
                    p = os.path.join(d, f)
                    z.write(p, p.replace(os.sep, '/'))
    z.write(readme, readme)
    print(len(z.namelist()), 'entries')
