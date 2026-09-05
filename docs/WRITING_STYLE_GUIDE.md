# Fronts of War — writing style guide

Governs **all player-facing text**: mission briefings, codex entries, tower
and enemy descriptions, results and post-mortem copy, achievement names,
menu and store copy, tutorial prompts, and UI labels. If a string ships in
the game, this document decides how it reads.

Subordinate to `docs/GDD.md` §14 (content policy) and §10.1 (no
stereotyping) in every case. Where voice and policy disagree, policy wins
and the line gets rewritten. Where voice and *clarity* disagree, clarity
wins — see §5.

---

## 1. The voice in one sentence

**A 1940s wire dispatch written by someone who has read the paperwork and
believes about 75% of it.**

---

## 2. The mix: 75% press prose, 25% Catch-22

### The 75% — WW2-era press and dispatch prose

The base register is the field dispatch and the wire-service communiqué:
plain, concrete, front-loaded, and unhurried by adjectives.

- **Declarative sentences.** Subject, verb, object. State the situation
  before interpreting it.
- **Short.** Most sentences under 18 words. Vary length so it doesn't
  clatter, but the average stays low.
- **Active voice**, except where the passive is doing real work ("the road
  was cut" is fine when who cut it doesn't matter yet).
- **Concrete nouns over abstractions.** "The hedgerow," "the north road,"
  "four tanks" — not "the tactical situation," "enemy assets."
- **Verbs carry the weight.** Adjectives are rationed; adverbs nearly
  banned.
- **Understatement.** The prose never tells the reader how to feel. A hard
  situation is described accurately and left alone.
- **No contractions** in dispatch-register text (briefings, codex, results
  flavor). They're fine in functional UI copy where they read more naturally.
- **No exclamation points.** Anywhere. If a line needs one, it is the wrong
  line.
- **Period-plausible vocabulary**, without jargon-dumping. The reader should
  never need a glossary.

### The 25% — the Catch-22 register

A dry, deadpan seam of institutional absurdity, running through the text
often enough to be a recognizable feature of the game's voice rather than an
occasional garnish.

**Frequency budget:**

- **Mission briefing:** two beats, or one sustained absurdity carried across
  a short paragraph. Never the opening line — the situation is stated
  straight first.
- **Codex entry:** one to two lines, and the entry may close on the joke.
- **Results / post-mortem:** the register can carry the whole flavor line.
- **Consecutive beats** are now permitted where they build (a rule, then its
  consequence). Three in a row is a bit.

What it actually is:

- **Circular official logic.** A rule that justifies itself and quietly
  eats its own tail.
- **Procedural over-precision applied to nonsense.** Exact figures,
  carefully filed, about something absurd.
- **The document that contradicts itself** without noticing.
- **Understatement pushed one inch too far**, delivered straight.
- **The competing authority.** Two offices, each correct, jointly producing
  something no one asked for.

What it is **not** — these do not loosen with the dose:

- Winking. No "am I right," no nudges, no jokes that announce themselves.
- Zany. No comic exaggeration, no funny names, no slapstick.
- Loud. The delivery stays deadpan no matter how often it appears.

**The dose governs frequency and prominence, never volume.** 25% means the
absurdity shows up more often and is allowed to carry more of a passage — it
does not mean the jokes get bigger, broader, or more pleased with
themselves. A reader should still be able to take every line at face value
and lose nothing but the second meaning. The target remains a short exhale
through the nose, arriving more often.

---

## 3. What the satire may and may not target

This is the hard boundary, and it is not a matter of taste.

**Permitted targets** — the machinery of war-as-administration:

- Paperwork, requisitions, allocations, forms, and filing.
- Doctrine as a document: the manual that is confidently wrong, or
  confidently right about something useless.
- Command abstraction — decisions made far from the position they concern.
- Supply arithmetic and the economics of materiel.
- Procedure that outlives its purpose.

**Forbidden targets** — absolute, per GDD §14.3 and §10.1:

- **Any nation, or its soldiers, or its units.** GDD §14.3: *"No nation's
  units are cowardly, fanatical, primitive, or comic."* National caricature
  is banned in "art, naming, audio, or codex text." There is no dose of this
  that is acceptable.
- Casualties, wounds, dying, or the dead. Nothing about a person being hurt
  is ever wry.
- Civilians, in any framing.
- Atrocity, war crimes, occupation, prisoners, reprisals — banned as
  content entirely (GDD §14.3), and therefore banned as material for wit.
- Ideology, political movements, real figures, real units, real places, real
  dates (GDD §14.2 — the theaters are fictional and unnamed).
- The player. Defeat copy is never mocking. See §5.

The distinction in one line: **the joke is on the filing cabinet, never on
the men.**

---

## 4. Prose mechanics

| Rule | Specification |
|---|---|
| Sentence length | Average 12–18 words. Hard ceiling around 30. |
| Paragraph length | 1–4 sentences. Dispatch text breaks often. |
| Tense | Present for the current situation, past for what happened. Never future-hedging ("will likely encounter"). |
| Numbers | Digits for quantities and figures ("four tanks" reads better under six; "1,240 Supply" as digits). Be consistent inside one string. |
| In-world proper nouns | Capitalized: Defense Line, Supply, Command Points, Suppressed, Spotted, Concealed, the Fronts. These are game terms and must match the UI exactly. |
| Em dashes | At most one per paragraph. |
| Rhetorical questions | No. |
| Second person | Sparingly, and only in briefings and tutorials ("Hold the crossroads"). Codex is impersonal. |
| Sign-offs / salutations | No "soldier," "commander," "good luck out there." |

**Anachronism ban list** (breaks the period instantly): okay, guys, team,
awesome, epic, cool, hey, folks, "let's," "gonna," tech, upgrade path, meta,
DPS, cooldown, buff, nerf, boss fight, level up, grind, spawn, respawn.

Function-facing UI may use plain modern words where they are the clearest
option ("Retry," "Settings," "Range"). The ban applies to fiction-facing
prose, not to a button that has one job.

---

## 5. Register by surface

Voice is not applied evenly. Some surfaces carry it, some carry none — and
clarity outranks voice everywhere. GDD §13.10 sets the bar that a player
learns the entire interface in Mission 1; no sentence is allowed to cost
that.

| Surface | Voice | Satire dose |
|---|---|---|
| **Mission briefing** (~120 words, GDD §9.2; typewriter/Courier per UI spec §5) | Full dispatch register | Two beats — situation stated straight first, never the opening line |
| **Codex — towers, enemies, damage table** | Dispatch register, impersonal | One to two dry lines per entry; may close on the joke |
| **Results / post-mortem flavor** | Dispatch register | The safest home for the 25% — may carry the whole line |
| **Achievement names** | Dispatch register, terse | Permitted |
| **Radio chatter and combat barks** | **None** | **Zero.** GDD §14.3: lines are "short, generic, tactical," operational only ("Contact, north road."). No wit, no personality, no exceptions. |
| **Tower and enemy names** | Flat and functional | None. A name is a label, per GDD §10.1's archetype naming. |
| **Tutorial prompts** | Plain instructional | None. Teaching text is never clever. |
| **UI labels, buttons, tooltips, stat readouts** | Plain and functional | None |
| **Defeat / failure copy** | Dispatch register, neutral | None aimed at the player. State what happened; the post-mortem already carries the analysis. |
| **Store and marketing copy** | Dispatch register, restrained | Very light. Note the GDD §14.4 storefront paragraph is fixed text — do not restyle it. |

---

## 6. Length budgets

- Mission briefing: **~120 words** (GDD §9.2 — two per mission, one per
  alliance).
- Codex entry: 70–100 words (room for the closing beat).
- Results flavor line: one or two sentences.
- Tutorial prompt: under 25 words.
- Tooltip: one sentence.
- Radio bark: under 8 words.

---

## 7. Pre-ship checklist

Run every authored string through this before it lands:

1. Does it name a real place, date, commander, unit, or political figure?
   → Rewrite (GDD §14.2).
2. Does the humor touch a nation, its soldiers, or its units? → Cut it
   (GDD §14.3).
3. Does it reference casualties, wounds, or the dead in any wry register?
   → Cut it.
4. Does the string exceed its §5 dose (more beats than budgeted, or a beat
   as the opening line of a briefing)? → Cut to budget.
5. Does it wink, or explain its own joke? → Rewrite straight.
6. Any exclamation points, rhetorical questions, or modern idiom? → Strike.
7. Do in-world capitalized terms match the UI exactly? → Fix.
8. Read it aloud. Does it sound like a dispatch, or like a game character
   talking? → It must be the dispatch.
9. Is it within its length budget (§6)?
10. Would removing the voice make it clearer? → Then remove the voice.

---

## 8. Samples

Samples are kept in comments so they are read as calibration references, not
as approved shipping copy. Authored strings live in mission `.tres` data and
the codex, not here.

<!--
SAMPLE A — Mission briefing (Bocage Crossroads, ~115 words).
Full dispatch register, two beats. Situation stated straight first (the
opening line carries no joke, per §5); one beat mid-text, one at the close.
No real place, no date.

    BOCAGE CROSSROADS — OPERATIONAL SUMMARY

    Enemy armor is probing the crossroads from the north road. Hold the
    Defense Line through twelve waves.

    The hedgerows here are old and deep-rooted. They will stop a man and
    will not stop a tank, an asymmetry Command's survey team recorded and,
    having recorded it, considered resolved.

    The finale brings a Breakthrough Panzer. Its skirt armor comes apart
    fastest under Explosive fire; once it is off, Armor-Piercing does the
    rest. Sequence matters. The skirt is indifferent to how much you spent
    shooting it with the wrong thing.

    The Arsenal of Democracy Factory is cleared for this position. It
    produces continuously and asks nothing of you but ground to stand on.
-->

<!--
SAMPLE B — Codex entry (Field Mortar, ~75 words).
Impersonal. Two beats that build on each other (the second closes the loop
the first opens) — permitted at 25% where it wasn't at 15%. The satire is
on doctrine-as-document, not on any nation.

    FIELD MORTAR

    A tube, a baseplate, and a shell that arrives roughly where the enemy
    used to be. Cheap, indirect, and blind up close: inside two tiles it
    is an expensive shovel.

    Doctrine holds that indirect fire is most efficient against massed
    infantry. Doctrine is correct, and is silent on how to persuade
    infantry to mass, a matter it leaves to the position. The position, in
    turn, leaves it to doctrine.
-->

<!--
SAMPLE C — Results screen (victory).
The safest home for the 25% — the flavor line is carried entirely by the
register. Bureaucratic, circular, harmless.

    POSITION HELD.

    Defense Line intact. Supply expended: 1,240.

    The allocation office notes the economy of the defense with approval,
    and has adjusted next month's allocation to match.
-->

<!--
SAMPLE D — Calibration pair. Same joke, two doses.
The dose controls how OFTEN this appears and how much of a passage it
carries — not how it's delivered. The "over the dose" fault list below is
about tone discipline, and holds at 25% exactly as it held at 15%.

    OVER THE DOSE (do not ship):
    "Command approved your anti-tank guns! Sadly, Command also approved
    the enemy's tanks. Bureaucracy, am I right?! Good luck out there,
    soldier!!"

    Faults: winks at the reader, two exclamation points, a rhetorical
    question, addresses the player as a character, modern idiom, and the
    joke announces itself.

    CORRECT DOSE:
    "Command has approved the anti-tank allocation. The armor it was
    requested against was approved some months earlier, by a different
    office, at greater expense."

    Same absurdity, delivered straight, aimed at the institution. No wink.
-->

<!--
SAMPLE E — Radio chatter, for contrast. ZERO voice, zero satire.
GDD §14.3 requires these be operational only.

    "Contact, north road."
    "Armor spotted. Request anti-tank."
    "Position holding."

    Anything wittier than this is a policy violation, not a style choice.
-->
