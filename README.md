# itolib

[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/pacoito/itolib?style=for-the-badge&logo=thunderstore&color=mediumseagreen
)](https://thunderstore.io/c/lethal-company/p/pacoito/itolib)
[![GitHub Releases](https://img.shields.io/github/v/release/pacoito123/LC_itolib?display_name=tag&style=for-the-badge&logo=github&color=steelblue
)](https://github.com/pacoito123/LC_itolib/releases)
[![License](https://img.shields.io/github/license/pacoito123/LC_itolib?style=for-the-badge&logo=github&color=teal
)](https://github.com/pacoito123/LC_itolib/blob/main/LICENSE)

> Wondrous gizmos and gadgets for the restless mind.

---

A collection of wacky scripts I've written for projects I'm involved in, most prominently [Bozoros](https://thunderstore.io/c/lethal-company/p/LethalMatt/Bozoros) and [PlayZone](https://thunderstore.io/c/lethal-company/p/LethalMatt/PlayZone).

Everything's kept fairly abstract so it can be generally applied for many use cases. Feel free to add this to your project to play around with, if anything catches your eye!

* **NOTE:** Expect a few breaking changes here and there (at least while everything is being polished), should you choose to add this as a dependency.

* **NOTE 2:** This is not intended to be a [JLL](https://thunderstore.io/c/lethal-company/p/JacobG5/JLL) replacement, though there _are_ a couple overlapping features. It should be fine to use both in the same project without issues, but I'd still recommend opting for JLL's implementations for a more tried and tested approach.

## Features (~~but here's the yapper~~)

A proper write-up and documentation for all components and their intended usage is planned, but here's a quick rundown of some of the more interesting ones:

* **PlayerAttachable:** An abstract effect or concept that continually affects a player (attach), and eventually stops (detach).
  * **PlatformGrabbable:** Physically attaches a player and makes them 'grab' on to a 'platform', making the player's position match said platform's until either a certain action stops being held (e.g. `Jump`), or they are detached through some other means. Used for Bozoros' balloon rides and PlayZone's slides.
  * **PlayerLauncher:** Launches any player who attaches, with heavily customizable trajectory parameters, and some additional optional features like fall damage prevention (until detached) and camera/player model tilting. Used for Bozoros' banana peels and PlayZone's trampolines.
  * **MovementSensor:** Checks if the attached player performs a specific movement action (e.g. `Jump`, `Move`, `Crouch`), and invokes an event callback if so. A cooldown can be applied so as to not trigger continuously, which can even be used for things like fake custom footstep sound effects. Used for PlayZone's ball pit movement effects.
  * **PlayerHinderer:** Slows down any player who attaches in a similar way to the vanilla spider web, up until the moment they detach. Has a field to allow the player to jump while hindered, even without stamina. Used for PlayZone's ball pit.
  * **PlayerSeater:** Makes any player who attaches enter the Cruiser sitting animation until detaching, without reparenting the player or having to use an `InteractTrigger`. Used for PlayZone's slides.
* **DetectRegion:**  An abstract region within which to detect or perform (non-allocating) searches for overlapping `Collider` instances belonging to objects of a certain type.
  * **PlayerSensor:** Detects any players inside, entering, and/or exiting the region, with some additional event callbacks specifically filtering players that are alive.
  * **EnemySensor:** Detects any enemies inside, entering and/or exiting the region, with some additional filtering for whitelisting specific enemies, as well as requiring a certain amount of them before triggering events.
  * **ScrapSensor:** Detects any scrap inside, entering and/or exiting the region, with some additional functions for causing them to drop to the ground, or disable its `MeshRenderer` and/or `Collider` instances.
  * **HazardSensor:** Detects any objects in the `MapHazards` layer inside the region, with an additional function to despawn found hazards.
  * **ExplodeEffect:** Implementation of vanilla's `Landmine.SpawnExplosion` using `DetectRegion`, which performs non-allocating searches inside a `Collider` (instead of a radius), contains some additional customizability for explosion properties, and has an adjustable collision mask to define which layers should count as 'cover' from the explosion.
  * **ConnectorMerger:** Detects any instances of itself within the region, disables one of them and (optionally) moves the remaining one to the center. Has a priority system so certain connectors are preferred from others.
* **ItemGrabbable:** A `GrabbableObject` but with a bunch of event callbacks that can mimic an inheriting class (e.g. `SoccerBallProp`) without actually inheriting it, sacrificing polymorphism for modularity. All these components can be mixed and matched to create items with multiple properties (e.g. `ItemKickable` + `ItemThrowable` to make a throwable soccer ball).
  * **ItemAudible:** Mimics `NoisemakerProp`, with pretty much the same properties save for a few additional ones.
  * **ItemKickable:** Mimics `SoccerBallProp`, with some added customizability for kick trajectory parameters, event callbacks, and an adjustable collision mask for objects it can land on top of.
  * **ItemThrowable:** Mimics `StunGrenadeItem`, with some added customizability for throw trajectory parameters, event callbacks, and an adjustable collision mask for objects it can land on top of.
  * **ItemWearable:** Mimics `BeltBagItem`'s wearable properties, specifically 'attaching' to either the player's head, belt, or a custom bone when pocketed.
  * **ItemWhackable:** Mimics `Shovel`, with added customizability for its properties (e.g. hit cooldown or hit speed), event callbacks for every stage of the 'whacking' process, and adjustable collision masks for hittable objects, with the added bonus of not allocating GC on every swing.
* **Interactables:** Components that inherit from `InteractTrigger` to fulfill various purposes.
  * **InteractClimbable:** An `InteractTrigger` for a ladder with adjustable climbing speed.
  * **InteractLockable:** A `DoorLock` implementation that allows custom tooltips that don't get overwritten when using a key. Doesn't inherit from `InteractTrigger` but is used alongside them for locked doors.
  * **InteractPurchasable:** An `InteractTrigger` that can spawn a prefab or run an event, but _for a fee_.
  * **InteractSeatable:** An `InteractTrigger` that acts like a Cruiser seat, but requiring a specific button press to get back up.
    * **NOTE:** Can be replaced with `PlayerSeater` + `PlatformGrabbable` for functionally the same effect, without the vanilla bug where two players get softlocked if they sit down at the same time.
  * **InteractTalkable:** An `InteractTrigger` that can transmit a player's voice over the Walkie while held; though only for one-way communication.
* **Events:**
  * **DelayedEvent:** An event that gets invoked after a given interval, either continuously or only once (until re-enabled).
  * **WeightedEvent:** Invokes an event (or several) from a specified list, each with its own weighted chance of being picked.
  * **ScriptableEventListener:** Can be used in combination with a `ScriptableEvent` to create an arbitrary 'global' event. This event can be raised from within any other event callback, to trigger something to happen on another, completely detached object.

There's a _bunch_ more scripts that are very niche, require further explanation to employ, or are in need of some refactoring (as they're a bit old now). If you're curious about any of them and/or have any questions regarding usage of a particular script, I've kept my [commit messages](https://github.com/pacoito123/LC_itolib/commits/main) fairly lengthy when adding new scripts, but also feel free to ping me in the [Lethal Company Modding Discord](https://discord.com/invite/XeyYqRdRGC) server. Feedback, suggestions, and bug reports are also welcome!

## Credits

* The LC Modding Community — For support, ideas, encouragement, and just good vibes in general.
* [LethalMatt](https://www.artstation.com/mattryszkowskiart) — For [Bozoros](https://thunderstore.io/c/lethal-company/p/LethalMatt/Bozoros), my all-time favorite moon (~~I am _not_ biased at all...~~), but also for coming up with wacky concepts for [PlayZone](https://thunderstore.io/c/lethal-company/p/LethalMatt/PlayZone) that necessitated additional scripting functionality, which was then added to this library.
* [IAmBatby](https://github.com/IAmBatby) — For [LethalLevelLoader](https://thunderstore.io/c/lethal-company/p/IAmBatby/LethalLevelLoader), the backbone for a significant chunk of custom content for this game. A couple scripts in here also require it or make use of its features.
* [PF1MIL](https://thunderstore.io/c/lethal-company/p/PF1MIL) — For Early Access™ testing of various scripts, suggesting additions and improvements, and just generally waiting patiently for this library to release.
* _You!_ — ![alt](https://cdn.betterttv.net/emote/642f4905a3c841a2f9ef2a94/1x.webp "pepeSTARE")
