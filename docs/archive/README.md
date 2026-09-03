# Archive — superseded documents

These four documents predate GDD v1.1 and are **no longer authoritative**.
They are kept for historical reference only — do not implement from them.
`docs/GDD.md` is the single source of truth for design, and it has already
absorbed everything of value from these files (see
[docs/DECISIONS.md](../DECISIONS.md) for how).

| File | Superseded by | What changed |
|---|---|---|
| `COUNTRIES_AND_TOWERS.md` | GDD §6 (archetypes), §8 (nations & signatures) | Tower *naming* survived almost verbatim into GDD §8's national rosters (Browning MG Nest, Bazooka Squad, Pak 40, Katyusha, etc.). Everything else changed: towers are now 9 shared archetypes reskinned per nation (not 10 independent rosters per nation), stats use the damage-type/armor-class system (§5) instead of flat damage/range numbers, and the roster of playable nations is fixed at 6 — the "Future Expansion" nations (France, Canada, Australia, Poland, China) here are explicitly out of scope in GDD §18.1. |
| `ENEMIES_REFERENCE.md` | GDD §10 (enemy system) | Enemies are now defined by 12 shared **archetypes** with nation-specific skins (never nation-specific stats) — the old flat HP/Speed/Armor-number model is replaced entirely by the armor-class + damage-type multiplier table (§5.4). Several named mechanics were renamed or reworked: "Stealth/Cloak" → **Concealed** (§5.5, §10.2 E11), "Rush/Banzai" units → **Swarm Infantry** (E3) and **Fast Infantry** (E2) as separate archetypes, flying enemies → **Air Unit** (E8) with authored air corridors. The old doc's per-enemy $ rewards, HP ranges, and armor-value scales no longer apply. |
| `UNITY_TRANSITION_GUIDE.md` | GDD §3.2, §15 (technical architecture) | The Unity migration this guide describes was reconsidered and reversed — GDD v1.1's revision note explains why the project moved to **Godot 4.x + C#** instead (§3.2: Godot's text-native scene/resource format is agent-editable without a running, GUI-attached editor, which matters because this project's programming is done entirely by AI coding agents). None of this guide's Unity-specific code (`MonoBehaviour`, `ScriptableObject`, isometric camera setup) applies. Its isometric-grid framing is also superseded — GDD §3.1 commits to top-down, not isometric. |
| `WW2TD_Unity_Guide.docx` | (same as above) | Docx counterpart of `UNITY_TRANSITION_GUIDE.md`; same status. |

**Why kept, not deleted:** they're the paper trail for how the GDD's tower and
enemy naming was arrived at, and deleting project history isn't free. If
you're implementing anything, read GDD.md — never these.
