"""Generates the monochrome UI glyph set (docs/UI_DESIGN_SPEC.md §6).

Every icon is a 64x64 SVG, single white fill/stroke, tinted in place by the
game. Style: field-manual symbols — bold geometric silhouettes, minimum 4 px
strokes, no interior detail under 6 px, no text. Re-run after editing:

    python tools/art/generate_ui_icons.py

Output: godot-project/assets/ui/icons/<id>.svg (existing files overwritten).
Nation marks are deliberately abstract shapes (GDD §14.3): no crosses, no
eagles, no real insignia.
"""
from pathlib import Path
import math

OUT = Path(__file__).resolve().parents[2] / "godot-project" / "assets" / "ui" / "icons"
W = "#FFFFFF"


def svg(body: str) -> str:
    return (
        '<svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 64 64">'
        f'<g fill="{W}" stroke="{W}" stroke-linecap="round" stroke-linejoin="round">{body}</g></svg>'
    )


def stroke(d: str, w: float = 6) -> str:
    return f'<path d="{d}" fill="none" stroke-width="{w}"/>'


def fill(d: str) -> str:
    return f'<path d="{d}" stroke="none"/>'


def circle(cx, cy, r, w=None):
    if w is None:
        return f'<circle cx="{cx}" cy="{cy}" r="{r}" stroke="none"/>'
    return f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="none" stroke-width="{w}"/>'


def rect(x, y, w, h, rx=0, sw=None):
    if sw is None:
        return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" stroke="none"/>'
    return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="none" stroke-width="{sw}"/>'


def polygon(points, sw=None):
    pts = " ".join(f"{x:.1f},{y:.1f}" for x, y in points)
    if sw is None:
        return f'<polygon points="{pts}" stroke="none"/>'
    return f'<polygon points="{pts}" fill="none" stroke-width="{sw}"/>'


def star(cx, cy, r_outer, r_inner, n=5, rotation=-90):
    pts = []
    for i in range(n * 2):
        r = r_outer if i % 2 == 0 else r_inner
        a = math.radians(rotation + i * 180 / n)
        pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    return pts


def chevrons_right(count, sw=7):
    step = 14
    start = 32 - (count - 1) * step / 2
    parts = []
    for i in range(count):
        x = start + i * step
        parts.append(stroke(f"M{x - 6} 14 L{x + 8} 32 L{x - 6} 50", sw))
    return "".join(parts)


def shield_path(cx=32, cy=32, w=36, h=44):
    x0, y0 = cx - w / 2, cy - h / 2
    return (f"M{x0} {y0} H{x0 + w} V{y0 + h * 0.55} "
            f"Q{x0 + w} {y0 + h * 0.85} {cx} {y0 + h} "
            f"Q{x0} {y0 + h * 0.85} {x0} {y0 + h * 0.55} Z")


def soldier(cx, cy, scale=1.0):
    r = 6 * scale
    return (circle(cx, cy - 14 * scale, r)
            + fill(f"M{cx - 11 * scale} {cy + 18 * scale} V{cy - 2 * scale} "
                   f"Q{cx - 11 * scale} {cy - 8 * scale} {cx - 5 * scale} {cy - 8 * scale} "
                   f"H{cx + 5 * scale} Q{cx + 11 * scale} {cy - 8 * scale} {cx + 11 * scale} {cy - 2 * scale} "
                   f"V{cy + 18 * scale} Z"))


def tank(cx, cy, hull_w=44, barrel=18, heavy=False):
    x0 = cx - hull_w / 2
    body = rect(x0, cy - 2, hull_w, 14, 3)
    tracks = rect(x0 - 2, cy + 8, hull_w + 4, 10, 5)
    turret = rect(cx - 12, cy - 14, 24, 13, 3)
    gun = rect(cx + 10, cy - 11, barrel, 5, 2)
    extra = rect(cx - 8, cy - 20, 16, 7, 2) if heavy else ""
    return body + tracks + turret + gun + extra


ICONS = {}

# --- Resources ---------------------------------------------------------------
ICONS["resource_supply"] = (rect(10, 16, 44, 36, 3, 6)
                            + stroke("M10 16 L54 52 M54 16 L10 52", 5))
ICONS["resource_cp"] = (rect(12, 6, 6, 52, 3)
                        + polygon([(18, 8), (54, 20), (18, 32)]))
ICONS["resource_defense_line"] = (rect(6, 38, 24, 14, 7) + rect(34, 38, 24, 14, 7)
                                  + rect(20, 22, 24, 14, 7) + rect(8, 22, 10, 14, 5)
                                  + rect(46, 22, 10, 14, 5))

# --- Time ----------------------------------------------------------------------
ICONS["speed_1"] = chevrons_right(1)
ICONS["speed_2"] = chevrons_right(2)
ICONS["speed_3"] = chevrons_right(3, sw=6)
ICONS["pause"] = rect(14, 12, 12, 40, 3) + rect(38, 12, 12, 40, 3)
ICONS["play"] = polygon([(18, 10), (54, 32), (18, 54)])
gear_pts = []
for i in range(16):
    r = 26 if i % 2 == 0 else 19
    a = math.radians(i * 22.5)
    gear_pts.append((32 + r * math.cos(a), 32 + r * math.sin(a)))
ICONS["settings"] = (f'<path d="M{" L".join(f"{x:.1f} {y:.1f}" for x, y in gear_pts)} Z '
                     f'M32 22 A10 10 0 1 0 32.1 22 Z" fill-rule="evenodd" stroke="none"/>')
ICONS["close"] = stroke("M16 16 L48 48 M48 16 L16 48", 7)
ICONS["menu"] = rect(10, 14, 44, 8, 4) + rect(10, 28, 44, 8, 4) + rect(10, 42, 44, 8, 4)

# --- Waves -----------------------------------------------------------------------
ICONS["wave"] = (stroke("M14 14 L32 26 L50 14", 6) + stroke("M14 28 L32 40 L50 28", 6)
                 + stroke("M14 42 L32 54 L50 42", 6))
ICONS["air_warning"] = (polygon([(6, 20), (32, 8), (58, 20), (32, 30)])
                        + rect(28, 34, 8, 16, 3) + circle(32, 56, 4))
ICONS["call_wave_early"] = (polygon([(8, 12), (30, 32), (8, 52)])
                            + polygon([(32, 12), (54, 32), (32, 52)]))

# --- Damage types ---------------------------------------------------------------
ICONS["damage_small_arms"] = fill("M22 30 V54 H42 V30 Z M22 30 Q22 8 32 6 Q42 8 42 30 Z")
ICONS["damage_explosive"] = polygon(star(32, 32, 28, 13, n=8, rotation=0))
ICONS["damage_armor_piercing"] = polygon([(32, 6), (58, 40), (44, 40), (32, 24), (20, 40), (6, 40)]) + rect(20, 46, 24, 10, 2)
ICONS["damage_anti_air"] = polygon([(4, 26), (32, 12), (60, 26), (32, 40)]) + rect(28, 38, 8, 16, 3)

# --- Armor classes -----------------------------------------------------------------
ICONS["armor_soft"] = rect(12, 12, 40, 40, 4, 6)
ICONS["armor_hardened"] = (stroke(shield_path(), 5)
                           + fill(f"M32 10 V54 Q14 46 14 34 V10 Z"))
ICONS["armor_armored"] = fill(shield_path())
ICONS["armor_heavy"] = (fill(shield_path(24, 30, 30, 38))
                        + f'<path d="{shield_path(40, 34, 30, 38)}" stroke-width="5"/>')

# --- Status --------------------------------------------------------------------------
ICONS["status_suppressed"] = (rect(10, 44, 44, 10, 3)
                              + stroke("M18 12 L24 22 L30 12 M34 12 L40 22 L46 12", 5)
                              + stroke("M26 26 L32 36 L38 26", 5))
ICONS["status_spotted"] = circle(32, 32, 20, 6) + circle(32, 32, 7)
ICONS["status_shielded"] = stroke("M10 34 A22 22 0 0 1 54 34", 7) + circle(32, 44, 8)
ICONS["status_concealed"] = "".join(
    stroke(f"M{32 + 22 * math.cos(math.radians(a)):.1f} {32 + 22 * math.sin(math.radians(a)):.1f} "
           f"A22 22 0 0 1 {32 + 22 * math.cos(math.radians(a + 40)):.1f} {32 + 22 * math.sin(math.radians(a + 40)):.1f}", 6)
    for a in (0, 90, 180, 270)) + circle(32, 32, 6)

# --- Threat badges --------------------------------------------------------------------
ICONS["threat_air"] = polygon([(4, 30), (32, 14), (60, 30), (32, 42)]) + rect(29, 40, 6, 14, 3)
ICONS["threat_siege"] = (stroke("M12 50 Q32 6 54 22", 7) + circle(54, 22, 7)
                         + rect(6, 46, 20, 10, 3))
ICONS["threat_support"] = (stroke("M20 44 L44 20", 9)
                           + f'<path d="M40 10 A12 12 0 1 1 54 24 L46 24 L40 18 Z" stroke="none"/>'
                           + circle(16, 48, 8))
ICONS["threat_concealed"] = (stroke("M8 32 Q32 8 56 32 Q32 56 8 32 Z", 5)
                             + stroke("M12 52 L52 12", 6))
ICONS["threat_boss"] = (f'<path d="M32 6 L58 32 L32 58 L6 32 Z M32 20 L44 32 L32 44 L20 32 Z" '
                        f'fill-rule="evenodd" stroke="none"/>')

# --- Matchup ------------------------------------------------------------------------------
ICONS["matchup_strong"] = circle(32, 32, 26) + f'<path d="M18 33 L28 43 L46 22" fill="none" stroke="#000000" stroke-width="7"/>'
ICONS["matchup_partial"] = circle(32, 32, 23, 6) + fill("M32 9 A23 23 0 0 0 32 55 Z")
ICONS["matchup_weak"] = stroke("M18 18 L46 46 M46 18 L18 46", 8)
ICONS["ineffective"] = stroke("M14 22 L32 42 L50 22", 8)

# --- Abilities ---------------------------------------------------------------------------------
ICONS["ability_artillery_strike"] = (circle(32, 32, 20, 5) + circle(32, 32, 5)
                                     + stroke("M32 4 V16 M32 48 V60 M4 32 H16 M48 32 H60", 5))
ICONS["ability_rally"] = (rect(12, 6, 6, 52, 3)
                          + fill("M18 8 H54 L46 20 L54 32 H18 Z"))
ICONS["ability_emergency_repair"] = (stroke("M14 50 L36 28", 9)
                                     + f'<path d="M32 12 A13 13 0 1 1 52 32 L42 32 L32 22 Z" stroke="none"/>'
                                     + stroke("M50 46 V58 M44 52 H56", 5))
ICONS["ability_doctrine"] = (rect(14, 8, 36, 48, 3, 5) + rect(24, 4, 16, 8, 3)
                             + stroke("M22 26 H42 M22 36 H42 M22 46 H36", 5))

# --- Towers (by TowerDefinition.Id) ------------------------------------------------------------
ICONS["tower_t1_automatic_gun"] = (rect(14, 24, 24, 14, 3) + rect(36, 27, 24, 6, 3)
                                   + stroke("M20 38 L12 56 M32 38 L40 56", 5) + rect(6, 20, 10, 6, 2))
ICONS["tower_t2_marksman_post"] = (circle(24, 26, 12, 5) + circle(24, 26, 4)
                                   + rect(34, 23, 26, 6, 3) + stroke("M24 40 V58 M14 58 H34", 5))
ICONS["tower_t3_field_mortar"] = (f'<rect x="28" y="6" width="12" height="36" rx="4" transform="rotate(30 34 24)" stroke="none"/>'
                                  + rect(10, 50, 44, 8, 4) + stroke("M22 30 L14 50", 5))
ICONS["tower_t4_anti_tank_gun"] = (polygon([(8, 18), (30, 14), (30, 46), (8, 42)])
                                   + rect(28, 26, 30, 7, 3) + circle(16, 52, 7) + circle(30, 52, 7))
ICONS["tower_t5_flak_battery"] = (rect(28, 4, 8, 34, 3) + rect(16, 36, 32, 10, 4)
                                  + rect(10, 48, 44, 8, 4) + stroke("M14 12 L20 20 M50 12 L44 20", 5))
ICONS["tower_t6_armored_emplacement"] = (f'<path d="M6 54 V36 A26 26 0 0 1 58 36 V54 Z M20 34 H44 V42 H20 Z" '
                                         f'fill-rule="evenodd" stroke="none"/>' + rect(40, 36, 20, 6, 3))
ICONS["tower_t7_heavy_artillery"] = (f'<rect x="26" y="2" width="12" height="40" rx="4" transform="rotate(45 32 22)" stroke="none"/>'
                                     + rect(10, 40, 40, 10, 4) + circle(24, 54, 9) + circle(44, 54, 7))
ICONS["tower_t8_minefield"] = (circle(18, 40, 9) + circle(46, 40, 9) + circle(32, 20, 9)
                               + stroke("M32 6 V10 M18 26 V30 M46 26 V30", 4))
ICONS["tower_t9_command_post"] = (polygon([(8, 56), (32, 22), (56, 56)])
                                  + rect(30, 6, 5, 20, 2) + polygon([(35, 6), (52, 12), (35, 18)]))
ICONS["tower_signature"] = (circle(32, 24, 16) + polygon([(20, 34), (30, 34), (26, 58), (16, 50)])
                            + polygon([(34, 34), (44, 34), (48, 50), (38, 58)]))

# --- Enemies (by archetype) ---------------------------------------------------------------------
ICONS["enemy_infantry"] = soldier(32, 32, 1.3)
ICONS["enemy_fast_infantry"] = soldier(38, 32, 1.2) + stroke("M6 22 H20 M4 34 H18 M8 46 H20", 5)
ICONS["enemy_swarm"] = soldier(16, 36, 0.75) + soldier(32, 28, 0.75) + soldier(48, 36, 0.75)
ICONS["enemy_armored_infantry"] = soldier(32, 32, 1.3) + f'<path d="{shield_path(46, 42, 16, 20)}" stroke-width="4"/>'
ICONS["enemy_light_vehicle"] = (rect(8, 28, 48, 14, 4) + rect(30, 18, 22, 12, 3)
                                + circle(18, 48, 8) + circle(46, 48, 8))
ICONS["enemy_medium_armor"] = tank(32, 32)
ICONS["enemy_heavy_armor"] = tank(30, 34, hull_w=50, barrel=24, heavy=True)
ICONS["enemy_air"] = (rect(29, 6, 6, 52, 3) + polygon([(4, 30), (32, 20), (60, 30), (32, 36)])
                      + polygon([(20, 54), (32, 46), (44, 54), (32, 58)]))
ICONS["enemy_support"] = (rect(8, 26, 30, 18, 3) + rect(38, 32, 18, 12, 3)
                          + circle(18, 50, 7) + circle(46, 50, 7) + stroke("M23 30 V40 M18 35 H28", 4))
ICONS["enemy_escort"] = (rect(10, 32, 44, 14, 3) + circle(20, 52, 7) + circle(44, 52, 7)
                         + stroke("M12 28 A20 20 0 0 1 52 28", 6))
ICONS["enemy_recon"] = (circle(20, 36, 11, 5) + circle(44, 36, 11, 5) + rect(26, 22, 12, 8, 2)
                        + rect(30, 12, 4, 10, 2))
ICONS["enemy_siege"] = (f'<rect x="28" y="0" width="12" height="44" rx="4" transform="rotate(50 34 22)" stroke="none"/>'
                        + rect(10, 44, 36, 10, 4) + circle(20, 56, 6) + circle(38, 56, 6))
ICONS["enemy_boss"] = (rect(6, 30, 52, 18, 4) + rect(16, 16, 32, 16, 4) + circle(32, 24, 7)
                       + rect(2, 48, 60, 8, 3))

# --- Progress ---------------------------------------------------------------------------------------------
ICONS["star_filled"] = polygon(star(32, 33, 28, 12))
ICONS["star_empty"] = polygon(star(32, 33, 25, 11), sw=5)
ICONS["rank_chevron"] = stroke("M12 30 L32 14 L52 30", 7) + stroke("M12 50 L32 34 L52 50", 7)
ICONS["lock"] = rect(14, 28, 36, 28, 4) + stroke("M22 28 V20 A10 10 0 0 1 42 20 V28", 6)
ICONS["check"] = stroke("M12 34 L26 48 L52 18", 8)
ICONS["upgrade_arrow"] = polygon([(32, 6), (56, 32), (42, 32), (42, 56), (22, 56), (22, 32), (8, 32)])
ICONS["sell"] = polygon([(32, 40), (10, 16), (22, 16), (22, 4), (42, 4), (42, 16), (54, 16)]) + rect(8, 48, 48, 8, 4)
ICONS["level_pip_on"] = circle(32, 32, 20)
ICONS["level_pip_off"] = circle(32, 32, 17, 6)
ICONS["branch_a"] = stroke("M32 58 V34 M32 34 L14 16 M32 34 L50 16", 6) + circle(14, 16, 9)
ICONS["branch_b"] = stroke("M32 58 V34 M32 34 L14 16 M32 34 L50 16", 6) + circle(50, 16, 9)

# --- Nations (abstract marks only, GDD §14.3) --------------------------------------------------------------
ICONS["nation_united_states"] = circle(32, 32, 28, 5) + polygon(star(32, 33, 18, 8))
ICONS["nation_britain"] = "".join(
    stroke(f"M{32 + 26 * math.cos(math.radians(a)):.1f} {32 + 26 * math.sin(math.radians(a)):.1f} "
           f"A26 26 0 0 1 {32 + 26 * math.cos(math.radians(a + 70)):.1f} {32 + 26 * math.sin(math.radians(a + 70)):.1f}", 6)
    for a in (10, 100, 190, 280)) + circle(32, 32, 12)
ICONS["nation_soviet_union"] = (f'<path d="M32 4 L60 32 L32 60 L4 32 Z M32 18 L46 32 L32 46 L18 32 Z" '
                                f'fill-rule="evenodd" stroke="none"/>')
ICONS["nation_germany"] = fill(shield_path(32, 32, 40, 48))
ICONS["nation_italy"] = circle(32, 32, 28, 5) + polygon([(32, 14), (50, 40), (40, 40), (32, 28), (24, 40), (14, 40)])
ICONS["nation_japan"] = polygon([(32, 4), (56, 18), (56, 46), (32, 60), (8, 46), (8, 18)], sw=5) + circle(32, 32, 13)


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    for name, body in ICONS.items():
        (OUT / f"{name}.svg").write_text(svg(body), encoding="utf-8")
    print(f"wrote {len(ICONS)} icons to {OUT}")


if __name__ == "__main__":
    main()
