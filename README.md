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

* **NOTE 2:** This is not intended to be a [JLL](https://thunderstore.io/c/lethal-company/p/JacobG5/JLL) replacement and follows a pretty different design philosophy, though there _are_ several overlapping features. It should be fine to use both libraries in the same project without issues, however.

## Features (~~but here's the yapper~~)

A proper write-up and documentation for all components and their intended usage is planned, but here's a quick rundown of them all:

* **PlayerAttachable:** An abstract effect or concept that continually affects a player (attach), and eventually stops (detach).
  * **PlatformGrabbable:** Physically attaches a player and makes them 'grab' on to a 'platform', making the player's position follow said platform's until either a certain action stops being held (e.g. `Jump`), or they are detached through some other means. Used for Bozoros' balloon rides and PlayZone's slides.
  * **PlayerElectrode:** Drains the batteries of any attached player's held and/or pocketed items, at a configurable rate.
  * **PlayerHinderer:** Slows down any player who attaches in a similar way to the vanilla spider web, up until the moment they detach. Has options to act as quicksand and sink the player (as long as the `Collider` underneath it has an appropriate tag), with a few customizable traits. Players can also be inflicted with the TZP 'drunk' effect, at a configurable rate. Used for PlayZone's ball pit.
  * **PlayerLauncher:** Launches any player who attaches, with heavily customizable trajectory parameters, and some additional optional features like fall damage prevention (until detached) and camera/player model tilting. Used for Bozoros' banana peels and PlayZone's trampolines.
  * **PlayerSeater:** Makes any player who attaches enter a sitting animation until detaching, without reparenting the player or having to use an `InteractTrigger`. Used for PlayZone's slides.
  * **PlayerTracker:** Physically attaches to a player and follows them around, at a configurable speed. Has an optional list of `Transform` 'pivots' to rotate to look towards the target, also at configurable speeds.
  * **PlayerWeightEvent:** An event that has a chance to be invoked periodically, with the chance being dependant on the attached player's weight.
  * **MovementSensor:** Checks if the attached player performs a specific movement action (e.g. `Jump`, `Move`, `Crouch`), and invokes an event callback if so. A cooldown can be applied so as to not trigger continuously, which can even be used for things like fake custom footstep sound effects. Used for PlayZone's ball pit movement effects.
    * **ShotgunSensor:** 'Detects' a shotgun being fired by an attached player towards the target, and invokes an event callback if it meets the (configurable) parameters for both range and precision.
    * **SpraySensor:** 'Detects' spray paint being used by an attached player towards the target. Has various thresholds for number of sprays required before invoking an event. Also works with weed killer!
* **DetectRegion:**  An abstract region within which to detect or perform (non-allocating) searches for overlapping `Collider` instances belonging to objects of a certain type.
  * **EnemySensor:** Detects any enemies inside, entering, and/or exiting the region, with some additional filtering for whitelisting specific enemies, as well as requiring a certain amount of them before triggering events.
  * **HazardSensor:** Detects any objects in the `MapHazards` layer inside the region, with an additional function to despawn found hazards.
  * **PlayerSensor:** Detects any players inside, entering, and/or exiting the region, with some additional event callbacks specifically filtering players that are alive.
  * **ScrapSensor:** Detects any scrap inside, entering, and/or exiting the region, with some additional functions for causing them to drop to the ground, or disable its `MeshRenderer` and/or `Collider` instances.
  * **ExplodeEffect:** Implementation of vanilla's `Landmine.SpawnExplosion` using `DetectRegion`, which performs non-allocating searches inside a `Collider` (instead of a radius), contains some additional customizability for explosion properties, and has an adjustable collision mask to define which layers should count as 'cover' from the explosion.
  * **VehicleSensor:** Detects any vehicles inside, entering, and/or exiting the region.
  * **ConnectorMerger:** _(Niche)_ Detects any instances of itself within the region, disables one of them and (optionally) moves the remaining one to the center. Has a priority system so certain connectors are preferred from others.
* **ItemGrabbable:** A `GrabbableObject` but with a bunch of event callbacks that can mimic an inheriting class (e.g. `SoccerBallProp`) without actually inheriting it, sacrificing polymorphism for modularity. All these components can be mixed and matched to create items with multiple properties (e.g. `ItemKickable` + `ItemThrowable` to make a throwable soccer ball).
  * **ItemAudible:** Mimics `NoisemakerProp`, with pretty much the same properties save for a few additional ones.
  * **ItemDiscardable:** Adds _discardability_ to any item, allowing it to be forcibly dropped from a player's inventory, and (optionally) despawned.
  * **ItemKickable:** Mimics `SoccerBallProp`, with some added customizability for kick trajectory parameters, event callbacks, and an adjustable collision mask for objects it can land on top of.
  * **ItemThrowable:** Mimics `StunGrenadeItem`, with some added customizability for throw trajectory parameters, event callbacks, and an adjustable collision mask for objects it can land on top of.
  * **ItemWearable:** Mimics `BeltBagItem`'s wearable properties, specifically 'attaching' to either the player's head, belt, or a custom bone when pocketed.
  * **ItemWhackable:** Mimics `Shovel`, with added customizability for its properties (e.g. hit cooldown or hit speed), event callbacks for every stage of the 'whacking' process, and adjustable collision masks for hittable objects, with the added bonus of not allocating GC on every swing.
  * **EventfulApparatus:** A `LungProp` but with all the event callbacks from `ItemGrabbable`, plus a few Apparatus-related ones. Allows selectively customizing which parts of the Apparatus sequence are actually triggered (e.g. showing the alert without shutting off power).
    * **ActivateApparatus:** Can automatically activate an Apparatus at the start of the round, so only a single deactivated prefab is needed for it. `EventfulApparatus` already includes this functionality, this is just for when a standard `LungProp` is preferred.
* **Interactables:** Various `InteractTrigger`-related components.
  * **InteractClimbable:** An `InteractTrigger` for a ladder with adjustable climbing speed.
  * **InteractLockable:** A `DoorLock` implementation that allows custom tooltips that don't get overwritten when using a key. Doesn't inherit from `InteractTrigger` but is used alongside them for locked doors.
  * **InteractPurchasable:** An `InteractTrigger` that can invoke an event, but _for a fee_.
  * **InteractSeatable:** An `InteractTrigger` that acts like a Cruiser seat, but requiring a specific button press to get back up.
    * **NOTE:** Can be replaced with `PlayerSeater` for functionally the same effect, without the vanilla bug where two players get softlocked if they sit down at the same time.
  * **InteractTalkable:** An `InteractTrigger` that can transmit a player's voice over the Walkie while held; though only for one-way communication.
* **Events:** Assortment of various events invoked in different ways.
  * **ApparatusEvent:** An event that gets invoked after an Apparatus gets pulled by a player.
  * **DelayedEvent:** An event that gets invoked after a specified amount of time, either continuously or only once (until re-enabled).
  * **HourEvent:** An event that gets invoked at a certain time of the day, when the hour changes (while enabled).
  * **RenderEvent:** _(Niche)_ An event that gets invoked when a `Renderer` becomes visible or invisible to any `Camera` displayed on the local client.
    * **NOTE:** Relies on Unity's `OnBecameVisible()` and `OnBecameInvisible()` calls so it might not fully work as expected, particularly with a `LODGroup`.
  * **ToggleEvent:** An event that simply has two states that can be toggled, and is synced across clients.
  * **WeightedEvent:** Invokes an event (or several) from a specified list, each with its own weighted chance of being picked.
  * **ScriptableEvent:** A `ScriptableObject` that can define an arbitrary 'global' event that can be invoked from any event callback, as long as their parameters match.
    * **ScriptableEventListener:** Can be used in combination with a `ScriptableEvent` to create an arbitrary 'global' event. This event can be raised from within any other event callback, to trigger something to happen on another, completely detached object.
* **NetworkedSpawner:** An abstract spawner for `NetworkObject` types, with a lot of varying customizability for the spawns.
  * **EnemySpawner:** Can spawn one or many instances of either a specific enemy type (that _can_ be switched), or various enemy types randomly picked using a weighted list.
    * **EnemyAnnihilator:** Not a spawner at all, but can be used alongside an `EnemySpawner` or `EnemySensor` to create 'temporary' enemies that will eventually die (e.g. after a certain amount of time passes, or after a player interacts with something). Also has some functions for dealing damage to enemies.
  * **HiveSpawner:** Can spawn one or many beehives at specific (exact) locations, with options to override scrap value, scan node text and parameters, as well as which type of item should actually be used as a 'hive'.
  * **PrefabSpawner:** Can spawn one or many instances of a (registered) network prefab at specific locations.
    * **ScriptableNetworkPrefab:** A `ScriptableObject` that includes a list of objects to register as network prefabs at the start of the game, in order for them to be spawned by `PrefabSpawner` without registering through other means. Needs to be manually added to a bundle that loads at the start of the game (e.g. an `ExtendedMod` bundle).
  * **ScrapSpawner:** Can spawn one or many instances of various item types randomly picked using a weighted list, with options for overriding minimum and maximum scrap values, scrap multiplier being applied, and several other things. Items are able to be spawned at exact positions and rotations (e.g. a `Shovel` buried in the ground), as long as `fallToGround` is disabled. Has the option of using the current moon's scrap weights (in addition to the weighted list) for item selection.
    * **GiftSpawner:** Can spawn one or many instances of the vanilla `GiftBoxItem`, with their contents being randomly picked using a weighted list, instead of being dependant on spawn position. Has options for overriding minimum and maximum scrap values for the item inside, sound and particle effects played when opening the gift, scan node text and parameters for the gift, and the material used for the gift.
    * **ScrapTeleporter:** _(Niche)_ Moves existing scrap items from the current round to specified locations. Alternative to spawning scrap items, if creating additional items is not desired.
  * **Fish:** [🐟](https://www.youtube.com/watch?v=lPGipwoJiOM).
* **NetworkedHittable:** An abstract object that implements `IHittable`, and can thus be hit.
  * **DamageHittable:** A hittable object with a specified amount of health, which can invoke events depending on various health thresholds (e.g. upon reaching `0` health). Also doubles as a generic counter script, if abstracting what 'getting hit' means.
  * **PhysicsHittable:** A hittable object that can apply a force to a `Rigidbody` upon being hit, with force depending on the damage dealt and direction depending on where it was hit from.
* **NetworkedAlert:** An abstract alert message that can be displayed to any or all players.
  * **AlertDialogue:** Displays one or several dialogue alert messages to players (e.g. 'ship leaving early' alert).
  * **AlertNotification:** Displays one or several notification alerts to players (e.g. 'new creature data' alert).
  * **AlertSignal:** Displays one or several Signal Translator message to players, which _can_ go over the vanilla letter limit.
  * **AlertToast:** Displays one or several toast alerts to players (e.g. 'dropship items missed' alert).
* **BaseConditional:** An abstract list of overrides that depend on a specific condition to be applied.
  * **ContentConditional:** Performs searches of a specific type of content (e.g. an item, enemy, or even a plugin), and invokes an event if they exist and are loaded.
  * **DungeonConditional:** Performs a search of the current dungeon's name in a list of override entries to apply, and invokes the events of any that match.
  * **MoonConditional:** Performs a search of the current moon's name in the a list of override entries to apply, and invokes the events of any that match.
  * **WeatherConditional:** Performs a search of the current weather's name in a list of override entries to apply, and invokes the events of any that match. Compatible with weather being changed mid-round by [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry) (through [WeatherTweaks](https://thunderstore.io/c/lethal-company/p/mrov/WeatherTweaks)) or [CrowdControl](https://thunderstore.io/c/lethal-company/p/CrowdControl/CrowdControl_LethalCompany).
* **ComponentGroup:** Groups of components of a specific type (collected dynamically), to simply call functions in bulk for them all.
  * **AudioGroup:** A group of multiple `AudioSource` components, for performing audio operations (e.g. pausing and unpausing) to each of them all.
  * **RigidbodyGroup:** A group of multiple `Rigidbody` components, for applying a force of a certain type (e.g. `ForceMode.Impulse`) to each of them all.
    * **RandomVector:** _(Niche)_ Simply outputs a random `Vector3` within a specified range to an event callback, where it can be used by other scripts (e.g. `RigidbodyGroup`). Has functions for modifying the minimum and maximum values for each axis, if so desired.
  * **RendererGroup:** A group of multiple `Renderer` components, for performing render operations (e.g. enabling/disabling shadow casting) to each of them all.
  * **PlayerAttachableGroup:** _(Niche)_ A group of multiple `PlayerAttachable` components, for performing attach operations (e.g. attaching and detaching) to each of them all.
* **Animations:**
  * **AnimationParamSetter:** Targets a specific `Animator` parameter (that _can_ be switched) to allow setting its value from an event call, while also syncing said value across clients.
  * **AnimationVelocity:** Can set a float parameter in an `Animator` to use as a speed multiplier for an `AnimationClip`, allowing smoothly changing between speed values and syncing it across clients.
  * **ConnectedRope:** Similar to the vanilla `SetLineRendererPoints` script (used when the Cruiser is being delivered), except allowing more than just two points, and also able to handle the `LineRenderer` having `useWorldSpace` disabled.
  * **MultiAnimationEvent:** Similar to the vanilla `PlayAudioAnimationEvent` script, except only its event callback (everything else it offers can be handled through other scripts instead), of which there can be multiple defined within the same component.
* **Effects:**
  * **AttachedEffect:** _(Niche)_ A particle effect or sound `GameObject` with object pooling capabilities, instances of which can be assigned to specific objects to play effects simultaneously.
  * **FearInducer:** _Inflicts fear upon a player's soul!_ Works like a player's `DeadBodyInfo` (triggers fear effect when looked at), but has various customizable parameters (e.g. cooldown, max distance, angle, and amount of fear relative to distance).
  * **MaterialSwapper:** Can perform one or multiple material swaps for specified objects and their children, if a given keyword is found in the names of the materials of each `Renderer`.
  * **NetworkedSource:** An `AudioSource` but with networking capabilities, so all clients can hear it being played. Has a few additional configurable parameters, including whether sounds made by it should alert enemies or not.
  * **PlayerOuchie:** Deals damage to a player, with some customizable parameters.
  * **SpookyDefog:** Removes interior 'Halloween Fog' introduced in `v67`, if enabled for the current round.
  * **SunScreen:** _(Niche)_ Disables the sun's renderer when entering the facility (if the moon has one), for 'open' interiors that would otherwise have the sun visible. Doubles as an event for players entering and exiting the facility, too.
* **Other:**
  * **CeilingAdjuster:** _(Niche)_ Moves an object to the highest point in the current dungeon generation.
  * **EventfulStoryLog:** A vanilla `StoryLog` but with a few added event callbacks (e.g. when a player collects it, or when it tries spawning while having already been collected).
  * **DungeonTelevision:** A vanilla `TVScript` that works when inside the facility, instead of immediately shutting itself off. Works even if most fields (e.g. all `VideoClip` and `Material` ones) are left blank to reduce bundle size, since they get replaced with the values present in the vanilla prefab. Also compatible with [TVLoader](https://thunderstore.io/c/lethal-company/p/Rattenbonkers/TVLoader), as long as there's no TV playing in the ship.
  * **HazardReplacer:** _(Niche)_ Can 'replace' a vanilla `SpawnableMapObject` by copying its `AnimationCurve` for the current moon over to another added through `ExtendedDungeonFlow` at the start of the round. Relies on the vanilla `SpawnableMapObject` not being present in any of the `RandomMapObject` spawners for the interior, so it's a bit wonky in its implementation.
  * **OutOfBoundsAdjuster:** _(Niche)_ Lowers the current moon's `OutOfBoundsTrigger` to the lowest point in the current dungeon generation, allowing for extremely vertical interiors without having to worry about intersecting it.
    * **NOTE:** This functionality exists in `ExtendedDungeonFlow` since LethalLevelLoader `v1.6.0`, with the `EnableDynamicOutOfBoundsTrigger` field. This script is only kept here for backwards compatibility.
  * **ReverbTriggerAdjuster:** _(Niche)_ Can adjust, replace, or disable any `AudioReverbTrigger` present in the ship scene (`SampleSceneRelay`), which would otherwise be inaccessible from the moon side of things.
    * **NOTE:** Should be careful with its use, since some of the triggers that can be replaced or disabled might be doing something important...
  * **SaneReverbTrigger:** An `AudioReverbTrigger` but with a customizable delay per activation, instead of running every frame. Also has an option to trigger only upon entering it.
  * **SpecificDoorway:** A `Doorway` with the option of specifying whether it can be picked as an entrance, exit, or neither while generating the dungeon, in order to have more than just one to choose from. Also has options for modifying their weights, which can increase or lower their chances of being picked to generate a path.
    * **NOTE:** The first tile generated by the dungeon needs to have a `SpecificDoorway` for them to apply everywhere else.

A lot of these scripts can be quite niche, and may require further explanation to employ properly. If you're curious about any of them and/or have any questions regarding usage of a particular script, I've kept my [commit messages](https://github.com/pacoito123/LC_itolib/commits/main) fairly lengthy when adding new scripts, but also feel free to ping me in the [Lethal Company Modding Discord](https://discord.com/invite/XeyYqRdRGC) server. Feedback, suggestions, and bug reports are also welcome!

## Credits

* The LC Modding Community — For support, ideas, encouragement, and just good vibes in general.
* [LethalMatt](https://www.artstation.com/mattryszkowskiart) — For [Bozoros](https://thunderstore.io/c/lethal-company/p/LethalMatt/Bozoros), my all-time favorite moon (~~I am _not_ biased at all...~~), but also for coming up with wacky concepts for [PlayZone](https://thunderstore.io/c/lethal-company/p/LethalMatt/PlayZone) that necessitated additional scripting functionality, which was then added to this library.
* [IAmBatby](https://github.com/IAmBatby) — For [LethalLevelLoader](https://thunderstore.io/c/lethal-company/p/IAmBatby/LethalLevelLoader), the backbone for a significant chunk of custom content for this game. A couple scripts in here also require it or make use of its features.
* [PF1MIL](https://thunderstore.io/c/lethal-company/p/PF1MIL) — For Early Access™ testing of various scripts, suggesting additions and improvements, and just generally waiting patiently for this library to release.
* _You!_ — ![alt](https://cdn.betterttv.net/emote/642f4905a3c841a2f9ef2a94/1x.webp "pepeSTARE")
