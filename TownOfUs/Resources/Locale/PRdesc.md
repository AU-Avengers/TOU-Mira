# Localization Update
This time I think there isn't any hardcoded texts remaining, but I'm human and might have missed something.

I encourage someone to review these changes bc I do this *while working retail in my third world country lol* so I might do stupid stuff sometimes bc of distractions. 

I avoided including stuff like [color="whatever"] inside the strings as much as possible as to not confuse the people working on weblate.

I tried reusing as much existing strings as possible (Seer doesn't need a custom string for the factions, for example)

I purposefully left KillFrenzy strings out of what I'm doing bc it's not public yet.

## What I changed
### 1. Team Chat Display names
### 2. Seer feedback being very confusing to translate
#### String Construction
    
The Seer's reveal logic required special handling because several messages are dynamically constructed. The original produced results such as:

> You have revealed that Player is evil!
> You have revealed that Player is possibly evil!
> You have revealed that Player is good!
> You have revealed that Player is likely good!
> You have revealed that Player is probably good!
> You have revealed that Player is possibly good!

These are now complete localization strings rather than a translated adjective/adverb inserted into an English sentence.

For example:
```
var evilRevealKey =
    options.ShowCrewmateKillingAsRed.Value ||
    options.ShowNeutralBenignAsRed.Value
        ? "TouSeerPossiblyEvilReveal"
        : "TouSeerEvilReveal";
```
This approach is preferable to translating individual fragments such as:

> possibly
> probably
> likely

because those words may need to appear in different positions depending on the language.
### 3. Remaining strings for:
- Miner, Sentry, Seer, Mercenary, Venerer, Morphling, Ambusher, Puppeteer, Traitor, Ambassador, Prosecutor, etc
- Disperser, Double Shot, Warden protected, etc

### Fixed some stringnames that were wrong and causing the game to use the english fallbacks
Scatter: `public override string ModifierName => MiraLocaleManager.Get("Scatter", "Scatter");` (The string is `TouScatter` in the XML)

### Random strings that could be localized
- Autorejoin
- Game Timer
- Ping Tracker Region text
- Draft