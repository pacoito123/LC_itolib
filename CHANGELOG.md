# Changelog

## [v0.4.1]

Added a few comments and tooltips, fixed some incompatibilities and minor bugs, did a bit of refactoring and tweaking.

- Added comments and tooltips for all types that inherit `PlayerAttachable`.
  - Also corrected some erroneous and/or outdated tooltips.
- Added some fields to configure camera clamping for `PlayerSeater`.
  - Players are also now teleported to the intended seat position, instead of relying on other means (e.g. `PlatformGrabbable`) to reposition them.
  - Tooltips for items are now properly hidden when hiding a player's held item, too.
- Added (basic) support for `ScrapSpawner` and `EnemySpawner` spawning items and enemies added through [DawnLib](https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib).
  - Only does a name comparison, will likely add namespace searching and tag matching at some point in the near future.
- Made it so if a player action is not found for whatever reason (e.g. for `MovementSensor`), it'll throw a warning and disable the script instead of spamming errors.
  - Also _maybe_ fixed not being able to find the player action in the first place, though I haven't been able to reproduce this issue so I can't confirm.
- Made range parameter for `SpraySensor` and `FearInducer` a floating point number instead of an integer.
- Fixed error thrown in `ScrapSensor` when a scrap item entered (or was created inside) the region before being spawned.
- Fixed `WeatherConditional` compatibility with the latest [CrowdControl](https://thunderstore.io/c/lethal-company/p/CrowdControl/CrowdControl_LethalCompany) versions.
- Fixed `SpecificDoorway` incompatibility with [Loadstone](https://thunderstore.io/c/lethal-company/p/AdiBTW/Loadstone).
  - Changed how `specificDoorwayActive` is toggled to not depend on `PauseBetweenRooms` being greater than zero.
- Fixed `ToggleEvent` not syncing properly between clients.

## [v0.4.0]

Added a couple new spawners, several useful fields and methods, some small fixes; also did some internal refactoring for a few scripts.

- Added `EnemySpawner`, to spawn any number of specified enemies on demand.
- Added `HiveSpawner`, for spawning Circuit Bees at specific locations.
  - Has fields to override the scrap item that bees consider to be their hive, modify the scan nodes for both the bees and hive, and scale hive scrap value depending on its distance from the ship.
- Added `EnemyAnnihilator`, to queue up the killing of any given enemies.
  - Intended for 'temporary' enemies that despawn after a certain amount of time, but can also simply be used for general enemy killing.
  - Can employ `EnemySpawner`'s `onSpawnPerformed` callback to add enemies to the list, as well as `EnemySensor`'s enemy filters to kill specific enemies that enter a region.
- Added fields to `EventfulApparatus` to toggle pretty much every step of the Apparatus pulling sequence (e.g. playing particle effects, flickering lights, waking up Old Birds).
  - Has fields for toggling triggering [FacilityMeltdown](https://thunderstore.io/c/lethal-company/p/loaforc/FacilityMeltdown) and [PizzaTowerEscapeMusic](https://thunderstore.io/c/lethal-company/p/BGN/PizzaTowerEscapeMusic), too.
- Added some fields to `NetworkedSpawner` to include every `AINode` (inside and/or outside) in the list of spawn locations.
  - Also added a field to include the children of any specified spawn locations as well.
- Added function to `EnemySensor` for setting a given enemy's `NavMeshAgent` speed to 0 for one second, to semi-reliably handle any pathfinding changes for fast-moving enemies (e.g. disabling a `NavMeshObstacle` or `OffMeshLink`).
  - Warps the enemy to the beginning of any `OffMeshLink` they may be traveling through right as they are stopped.
  - Will _probably_ end up being moved to its own dedicated `EnemyHinderer` script at some point.
- Added function to sync playback time for every `AudioSource` in an `AudioGroup`, as well as a field to automatically 'initialize' them all (`Play()` followed by `Pause()`).
- Added a networked `AttachPlayer()` function to `PlayerAttachable`, instead of only having `AttachPlayerLocal()` available for manual attaching.
- Rewrote `NetworkedSpawner`-inheriting scripts to use `NetworkLists` to sync spawned object properties with all clients, instead of periodically checking if they have spawned for the local client.
- Improved `ISeededScript` interface and implemented it on all scripts that use randomization.
- Reworked `ToggleEvent` and added networking to it, to properly sync toggling across clients.
- Reworked `HourEvent` to subscribe to `TimeOfDay`'s `onHourChanged` event, instead of only checking the time after being enabled.
- Fixed `ExplodeEffect` throwing errors if used while in orbit, due to a null check I forgor.
- Fixed `OutOfBoundsAdjuster` not adjusting the kill floor on moons that deviate from a vanilla hierarchy a bit.
- Fixed two-handed `ItemWhackable` items being able to be pocketed, if scrolling during the swinging animation.
- Removed prefab spawning stuff in `InteractPurchasable`, as it can be handled better via `PrefabSpawner`.
- Removed `AudioSource` and `Animator` stuff from `PlatformGrabbable`, as it can be handled better through other means.
- Removed (unused) `ContentTag`-related fields in `ScrapSpawner`.
  - Will likely re-add tag spawning at some point, but it's been unused since the addition of weighted scrap spawning.

## [v0.3.2]

Small fix for `NetworkedHittable`, added LayerMask fields to a couple scripts.

- Fixed `NetworkedHittable` objects erroring out when hit by a non-player.
- Added a LayerMask field to both `SpraySensor` and `FearInducer`, which determine the layers that should block the player's line of sight.

## [v0.3.1]

Couple small improvements and fixes.

- Added `ISeededScript` interface, for scripts that need randomization using the current map seed.
  - Only used for `ScrapSpawner` at the moment, still thinking about a few things with its implementation.
- Made `ScrapSpawner`'s `respectSingleItemDay` field actually work, and made its `seededRandom` field affect a couple things I missed.
- Added default curves for `ItemKickable` and `ItemThrowable` that correspond with the `Soccer ball` and `Stun grenade` curves, respectively.
- Made `ItemTargetable`'s fall curve override be reset upon being picked up, so items don't act weird when normally dropped.
- Switched to using an `AABB` check of the local player for `PlayerSensor`, if the `onlyAffectsLocalPlayer` field is enabled.
  - Most likely will end up switching to `AABB` checks for all players at some point, instead of overlap stuff.
- Fixed `DetectRegion` scripts sometimes 'remembering' objects that had been previously found, but are no longer present.

## [v0.3.0]

Did a couple changes, I think...

- Added `SpraySensor`, for detecting if an object is being sprayed with Spray Paint, Weed Killer, or any other item that uses or inherits `SprayPaintItem`.
  - Multiple spray 'treshholds' can be defined, each with event callbacks, to have stuff happen depending on the number of times sprayed (e.g. to have something happen after 3 sprays specifically).
  - Does not actually hook into any `SprayPaintItem` code, so it should be compatible with anything that modifies it (e.g. [BetterSprayPaint](https://thunderstore.io/c/lethal-company/p/taffyko/BetterSprayPaint)).
- Added `FearInducer`, which increases a player's fear level and plays the fear effect when looked at.
  - Has some customizability for things like range, angle, and amount of fear to instill upon the player depending on how close they are.
  - Disables itself after triggering once, but can be re-enabled to give the player another spook.
- Added `ItemDiscardable`, for items that drop themselves from the player's inventory.
  - Has a function to cause the item to drop itself from the player's inventory (even while pocketed), as well as a despawn timer after getting discarded.
- Added `EventfulApparatus`, a `LungProp`-inheriting object with a bunch of events similar to `ItemGrabbable`.
  - (PlayZone) `TwinApparatus` now inherits from `EventfulApparatus`.
- Added `MultiAnimationEvent`, which is similar to `PlayAudioAnimationEvent` but without a few features that can be done in a better way with other components (e.g. with `NetworkedSource`), and with a list of event callbacks to execute instead of the single `OnAnimationEvent`.
- Added `AudioGroup`, which checks objects and their children for any `AudioSource`, and allows some basic audio functions to be run on all sources at once.
- Added `CeilingAdjuster`, which just raises whichever object it's attached to to the highest point in the dungeon + a specified additional offset.
- Added `DungeonConditional`, for doing stuff whenever specific interiors generate.
- Added lerping to `PlayerLauncher`, so it smoothly ramps up towards the applied force instead of immediately applying it.
  - Ramping speed is adjustable, and it comes with a new detach condition for once the force is fully applied.
  - Also fixed the unintended rocket jump whenever players jump right before touching the `PlayerLauncher`, but it can be turned back on!
- Added lerping to `PlatformGrabbable`, to smoothly move the player towards the center of the platform, instead of teleporting them to it.
  - Not teleporting instantly means the player will be slightly behind the intended position, but this grabbing speed can be adjusted.
- Added `IEventfulItem` interface, which includes every event available in `ItemGrabbable`.
  - Made all modular item scripts (e.g. `ItemWhackable`) require using items that implement the `IEventfulItem` interface.
  - Made `ItemGrabbable` and `EventfulApparatus` implement `IEventfulItem`, so they're both compatible with all modular item scripts!
- Added abstract `ItemTargetable`, which represents items that follow a trajectory towards a set destination.
  - `ItemKickable` and `ItemThrowable` now both inherit from it, and thus share some common functionality.
  - Both `ItemKickable` and `ItemThrowable` had some revisions done to their trajectory logic, too.
- Added `IWeightedScript` interface, which includes a bunch of default method implementations to handle weighted randomization.
  - `WeightedEvent` and `ScrapSpawner` now implement said interface.
- Added generics to the `IPooledObject` interface.
  - Now the abstract `PooledObject` contains Unity-related object pooling stuff, and `AttachedEffect` inherits from it.
  - It also now actually supports creating a given number of instances to have ready from the start.
- Overhauled `ScrapSpawner` a bit (using the `IWeightedScript` interface):
  - Added a weighted list of items to spawn, instead of just a single item.
    - Blank references (even for modded items!) should be working correctly, too.
  - Added field to `ScrapSpawner` to allow it to use the current moon's spawn weights, instead of specifying a list.
    - Overrides any items set in the weighted list.
  - Added a minimum and maximum set amount of items to spawn, instead of simply spawning one at every defined location.
  - Added able to spawn scrap at a random location within specified area bounds, instead of only at set points.
  - Made spawned scrap actually count towards the current round's total scrap value amount.
- Added pretty much all `ItemAudible` fields to `NetworkedSource`, and improved its networking a bit.
  - `NetworkedSource` now has functionality to, for instance, alert nearby enemies or play sounds over the walkie.
- Made `MaterialSwapper` able to do a set amount of swaps per activation, instead of doing all of them at once.
  - Allows for 'cycling' through various material states by only doing a certain number at a time.
- Improved enemy filtering for `EnemySensor`.
  - Added callback events for individual filters, as well as enemy blacklisting.
- Added `GrabbableObject` attaching to `AttachedEffect`.
  - Switched `AttachedEffect` generic type to `Collider`, and made it detach upon disabling.
- Added a sitting animation field to `PlayerSeater`, to be able to use the sofa and electric chair sitting animations, too.
- Added a stamina requirement field for triggering `MovementSensor` events.
- Added a player stamina draining function to `PlayerHinderer`.
- Added field to mute quicksand sinking sounds for `PlayerHinderer`.
- Added a networked `onLogCollected` event callback to `DungeonStoryLog`.
- Made `DetectRegion` scripts take (lossy) scale into account when performing searches.
- Made `InteractClimbable`'s `specialCharacterAnimation` field automatically disable itself, if `twoHandedItemAllowed` is enabled.
  - Allows players to climb with two-handed items.
- Merged `WallBreaker` script into `ConnectorMerger`, which can now be used to disable either connector, or both.
- Fixed `PlayerSensor` player search counting players twice.
- Fixed `ExplodeEffect` explosion spawning a fair distance away from where it was actually supposed to.
  - Also fixed its `spawnExplosionEffect` field not actually doing anything.
- Fixed all `BaseConditional` scripts not applying on dungeon completion.
- Fixed `PrefabSpawner` not working without the spawner itself being spawned.
  - _But who spawns the spawner?_
- Fixed `NetworkedHittable` not actually serializing `hitID` and the player who hit when sending hit information to other clients.
- Fixed sun not actually being hidden by the `SunScreen` script.
  - Switching spectating camera also no longer toggles the sun, but I don't think it was even working in the first place...
- Fixed `detachTimer` field for `PlayerAttachable` not actually starting when manually attaching the player (instead of with `attachOnEnter`).

## [v0.2.0]

Did some pretty substantial refactoring; added and fixed a couple things, too.

- Added some stuff to `ExplodeEffect`:
  - Made `ExplodeEffect` able to target any object that implements the `IHittable` interface.
  - Added separate enemy and `IHittable` curves to `ExplodeEffect`, to deal specific damage to non-player targets.
  - Replaced `damageRange` and `killRange` with `damageBounds` and `killBounds`, for visualization purposes.
- Added player sinking curve overriding to `PlayerHinderer`, to control how deep the player actually sinks before dying.
- Added `OutOfBoundsAdjuster`, which just moves the current moon's `OutOfBoundsTrigger` to the lowest point in the dungeon + a specified additional offset.
  - Intended for more vertically-oriented dungeons.
- Made `DetectRegion` actually take region rotation into account when performing searches.
- Did a lot of refactoring under the hood, based on [IAmBatby](https://github.com/IAmBatby)'s suggestions and feedback!
  - Made scripts with update loops disable themselves when not in use, the most important one being `PlayerAttachable`.
  - Removed all uses of null propagation on `UnityObject` stuff, fixing some rare `NullReferenceException` errors.
  - Switched from using `NetworkObjectReferences` to `NetworkBehaviourReferences` when networking stuff, thus skipping a step.
  - Some other miscellaneous tweaks and fixes here and there.

## [v0.1.4]

Reworked `PlayerLauncher` a bit, fixed [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry) compatibility.

- `PlayerLauncher` now uses a list of forces to apply to the player, to combine multiple sources of rotation (e.g. where the launcher is facing + where the player's looking towards).
- Added some drowning/quicksand-related stuff to `PlayerHinderer`, but it's not quite working just yet...
- Fixed `WeatherConditional` compatibility with [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry), I forgor to actually apply my patch for it...

## [v0.1.3]

Added player callbacks to `NetworkedHittable`, fixed some stuff with `WeightedEvent`.

- Added information to `NetworkedHittable` about the player that performed the hit, as well as some separate hit event callbacks with said player given as an invoke parameter.
- `WeightedEvent` rolls should now actually roll when initiated by clients.

## [v0.1.2]

Added `DamageHittable` and `ToggleEvent`, fixed some stuff with scrap-related scripts.

- `DamageHittable` is a `NetworkedHittable` with health, it's got a list of conditions with event callbacks that are invoked when its health falls to or below specified numbers.
- `ToggleEvent` is just a behaviour with event callbacks for `OnEnable()` and `OnDisable()`... there ain't much more to it, I just needed it for something.
- `ScrapSpawner` should actually sync scrap position now, I forgor to add it...
- `ScrapTeleporter` now uses a seeded `Random` instance, takes teleport area colliders' center point into account, and should properly set item rotations if set to activate on scrap spawn (as it was supposed to have been doing).

## [0.1.1]

Added some compatibility for [PizzaTowerEscapeMusic](https://thunderstore.io/c/lethal-company/p/BGN/PizzaTowerEscapeMusic).

- Pulling only one `TwinApparatus` will no longer trigger escape music.

## [0.1.0]

Initial release!

- Documentation is lacking for most scripts and there's a good amount of jank, but it should be stable enough for a release.
- Proper documentation and wiki pages for all features is planned, alongside some example prefabs used in [Bozoros](https://thunderstore.io/c/lethal-company/p/LethalMatt/Bozoros) and [PlayZone](https://thunderstore.io/c/lethal-company/p/LethalMatt/PlayZone).
