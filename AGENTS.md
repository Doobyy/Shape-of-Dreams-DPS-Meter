# AGENTS.md — Shape of Dreams DPS Meter

## Purpose

This file is the project memory for AI-assisted development. The goal is to prevent regressions, repeated reverse-engineering, and redoing behavior that has already been solved and verified in-game.

Before changing existing behavior, read this file and inspect the current source. Do not assume an older implementation is still present.

## Development Rules

- Inspect the current repository/file and current SHA before making a patch.
- Do not guess about game/API members. Only use members confirmed by the current source or runtime diagnostics.
- Patches must be compile-ready. Do not remove helpers until all references have been checked.
- Before pushing any patch, perform at least one independent compile-safety pass over the complete changed files: verify file structure/braces, method placement, signatures, references, and obvious syntax/type errors. This pass is mandatory even for small patches; do not rely solely on the edit operation succeeding. If practical, re-fetch the committed files and inspect them before asking the user to build.
- Prefer the smallest targeted change over refactoring unrelated code.
- If a patch fails, clean up/revert the failed approach before trying another approach. Do not stack bandaid patches.
- Do not spend time on unnecessary automated test benches or syntax checks unless specifically useful; manual in-game testing is the primary validation.
- Do not change unrelated working systems while implementing a feature.
- Every real feature patch increments the displayed version by 0.1. Compile-only fixes and diagnostics do not need a version bump unless explicitly intended.
- When behavior is confirmed working in-game, record the solution here so it is not accidentally lost later.

## Compile-Safety Rules

- **Compile-ready means structurally complete, not just logically plausible.** Before committing a patch, verify every newly referenced type, method, property, field, enum, and signature actually exists in the current source or in a confirmed game API. In particular, if a patch introduces a new helper/data type (such as a nested class), it must be explicitly added to the file before any property or caller references it.
- **Trace the full compile-time dependency chain before patching.** For every changed method signature, update all declarations and every call site. For every renamed/introduced symbol, search the entire repository for references and definitions before considering the patch complete.
- **Do not claim a patch is compile-ready based only on inspecting the changed snippet.** The current repository must be re-fetched after each related file change and checked for missing definitions, stale signatures, and mismatched types.
- **When using GitHub file edits, fetch the resulting file after the edit and inspect the actual committed content.** Do not rely on the intended patch text or tool success response as proof that the resulting source contains all required definitions.
- **A compile failure is a patch failure, not a user testing step.** If the assistant introduced the compile error, immediately fix the repository to a buildable state before asking the user to test anything.

## Critical Identity Rule: Skill Gems

Skill tracking has two different concepts:

1. Stable skill-gem identity/source
   - The skill gem is identified internally by the game's HeroSkillLocation returned from Hero.Skill.TryGetSkillLocation(skill, out location).
   - This game-provided slot identity is used internally to recognize that damage/healing belongs to the same equipped skill gem.
   - Do NOT hardcode current keybind labels such as Q, E, R, or RM; players can change those bindings.
   - These identifiers are NOT player-facing labels.

2. Display name
   - The visible row uses the formatted skill name, such as Frostbite or Frostbite +4.

### Required aggregation behavior

If a skill gem is upgraded during the same instance, it must remain one source.

Example:
- Q = Frostbite
- Later in the same instance Q becomes Frostbite +4

Damage from both versions must aggregate into the same Q/Frostbite source instead of creating two rows.

The same rule applies to healing.

Do NOT use the formatted display title as the sole internal aggregation key.
Do NOT replace the visible skill name with Q/E/R/RM.

The correct model is:
    game HeroSkillLocation identity
        -> internal aggregation key
        -> current formatted skill name for display

This is currently a known regression to fix: Frostbite and Frostbite +4 are appearing as separate current-DPS sources after an in-instance upgrade. The fix must also be applied to the healing pipeline.

## Damage Tracking

- Produced damage = applied damage + discarded/overkill damage.
- Direct Essence damage is tracked separately from its parent Memory/Skill so the same damage is not double-counted.
- Skill rows, Essence rows, Basic Attack, and Other sources have separate display behavior.
- Essence contributions use the underlying Gem as the source and are aggregated separately.
- Current and cumulative tracking are separate.
- Elemental/scaling information is stored alongside the source aggregation.

## Healing Tracking

- Healing throughput tracks generated healing, including effective healing plus overheal/discarded healing.
- EventInfoHeal.amount = effective healing.
- EventInfoHeal.discardedAmount = overheal/discarded healing.
- Total generated healing is: max(0, amount) + max(0, discardedAmount).
- Current HPS and total HPS are already working.
- Healing rows have icons.
- Healing source attribution is imperfect for some internally generated effects and should be investigated from runtime data rather than guessed.
- Skill-gem identity rules above apply to healing as well as damage.

## UI / Resize — Known Working State

The healing toggle expands the outer overlay while preserving the user's configured/collapsed DPS viewport.

- Manual resize defines the collapsed/base size.
- Opening healing increases the outer height to make room for the healing section.
- Collapsing returns to the configured/base size.
- This behavior was confirmed working in-game after v4.30.
- Do NOT modify the resize system unless a real regression is demonstrated.
- Do not reintroduce old resize diagnostic logging.

## Icons — Known Working State

- Skill icons work.
- Essence icons work.
- Healing icons work.
- Basic Attack icon works.
- Other-source icons are supported.
- Keep the icon/bar layout: icon is a separate square at the left and the colored bar begins immediately at its right edge.
- Do not remove working icon fallback logic without tracing its callers.

## Version / Current State

Current displayed version: v4.32.

Recent verified work includes:
- Healing generated amount / overheal tracking.
- HPS.
- Healing icons and toggle.
- Working expandable healing overlay resize.
- Skill/Essence icon handling.
- Damage scaling detection improvements.
- Barrier generation tracking is implemented from ClientEventManager.OnTakeShield using EventInfoShield.finalAmount.
- Barrier rows and BPS are implemented; per-source attribution falls back to the generated shield/status effect when a source skill cannot be resolved.

## Diagnostics / Logging

Keep diagnostics focused and capped so normal game logs do not become flooded.

Old healing diagnostics and resize diagnostics have been removed. Do not restore them unless specifically needed.
