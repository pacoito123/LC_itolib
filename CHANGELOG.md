# Changelog

## [v0.9.3]

Small hotfix for a renamed method.

- Fixed `PlayerLauncher` erroring out due to `PlayerControllerB.DropAllHeldItemsAndSync()` being renamed to `PlayerControllerB.DropAllHeldItemsAndSyncNonexact()`.
- Also added a missing null check to `PlayerSensor`.

## [v0.9.2]

Updated for `v80`!

- Added `Constant` ScreenShakeType to `ShakeEffect`.
- Made `HazardReplacer` use the new `IndoorMapHazard` type.
- Disabled `SpecificDoorway` patches for now (have not tested it alongside the new `DunGen` version).

## [v0.9.1]

A couple small fixes before updating game version.

- Added tag-based connection rules to `SpecificDoorway`, similar to the ones that can be set for a `Tile`.
  - Can define one or several (`Doorway`) tags a `SpecificDoorway` should or should not be able to connect to.
- Fixed clients not being able to attach to a `PlayerAttachable` when called from an item, due to the server not having ownership of it.

## [v0.9.0]

Added a couple niche scripts, reworked and overhauled various components, fixed several issues.

- Added `RendererGroup`, for components that inherit from `Renderer`.
  - Has functions for enabling/disabling every `Renderer`'s shadows, as well as changing their shadow casting mode.
- Added `PlayerAttachableGroup`, for components that inherit from `PlayerAttachable`.
  - Has functions for attaching/detaching players to/from each of them all.
- Added generic functions to enable/disable or toggle every component in a `ComponentGroup`.
- Added field to `ConnectedRope` to disable itself when not being looked at.
- Added field to `PlayerTracker` to tilt in relation to the player's camera, if set to rotate with the player.
- Added `onHiveSpawned` callback to `HiveSpawner`, for when the hive itself is spawned and fully synced with clients.
- Added curve field to `ShotgunSensor` and `SpraySensor` to scale angle tolerance required to trigger the sensor depending on distance away from it.
- Added event callbacks to `ApparatusEvent` for when emergency lights are turned on and off, after [FacilityMeltdown](https://thunderstore.io/c/lethal-company/p/loaforc/FacilityMeltdown) begins its meltdown sequence.
- Made various improvements to `PlayerAttachable`:
  - Added function to _transfer_ players, to gracefully _steal_ the spot of an attached player without having to manually perform the swap.
  - Swapped `attachDisabled` field with a function that enables/disables attaching, so it'll actually show up in event callbacks.
  - Added function to toggle attaching, which just inverts the current state of `attachDisabled`.
  - Made the `triggerOnce` field simply disable attaching, so it can be reenabled if so desired.
  - Made attach condition be taken into consideration when using `AttachPlayerLocal()`, while attaching locally.
- Made various improvements to `MovementSensor` and its inheriting scripts:
  - Made `MovementSensor` trigger immediately as the action is about to be performed, instead of a frame or two later.
  - Added curve field to both `ShotgunSensor` and `SpraySensor` to scale angle tolerance required to trigger the sensor depending on distance away from it.
  - Fixed `ShotgunSensor` being able to be triggered while paused, or with the `Shotgun` on cooldown.
  - Removed specialized event callbacks from `ShotgunSensor` and `SpraySensor` (`onShotPerformed` and `onSprayPerformed`, respectively), since `onMovementDetected` now acts the same for both.
- Made various improvements to `AnimationVelocity`:
  - Switched from lerping to damping for the speed smoothing, and thus from a smoothing _speed_ to a smoothing _time_.
  - Made initial speed syncing with clients also completely reset the `Animator`, in case the desired animation was already playing.
  - Made changing speed function first sync the given value across clients, if not yet initialized.
  - Added field to roll for the initial random speed using the current map seed.
  - Added function to lock the target speed to the given value.
  - Hashed any string values used for the `Animator`.
  - Added some comments and tooltips.
- Made various tweaks to `NetworkedAlert` and its inheriting scripts:
  - Added field to all alert entries to set if the specific entry should be displayed only once (per instance).
  - Fixed `AlertNotification` and `AlertToast` potentially causing some issues due to not resetting their respective trigger parameter.
  - Did a few minor optimizations.
- Made `SaneReverbTrigger` and `SpecificDoorway` copy the configuration of their respective parent components, to easily replace them.
- Made `Fish` parent summoned fish to the objects that define their location instead, if `parentToSelf` is enabled.
- Fixed `ScrapSpawner` and `EnemySpawner` not being able to spawn items and enemies registered through [LethalLib](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib).
- Fixed `ItemKickable` items not being... kickable.
- Fixed `ItemWhackable` not respecting variant index number, if updated at a later point.
- Fixed `InteractLockable` not showing the timer when unlocking a door with a custom tooltip.
- Fixed compatibility with newest versions of [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry).
  - Should also be backwards-compatible with older versions, too.

## [v0.8.1]

Added a niche event, did a couple small additions and small fixes.

- Added `RenderEvent`, which invokes an event callback when a `Renderer` becomes visible or invisible.
  - Relies on Unity's `OnBecameVisible` and `OnBecameInvisible` calls so it might not fully work as expected, particularly with a `LODGroup` in the mix.
- Added some stuff to `RigidbodyGroup`:
  - Added functions to add force relative to the position of each `Rigidbody`.
  - Added functions to to sleep and wake up each `Rigidbody`.
  - Reworked all `ComponentGroup` scripts internally, too.
- Added `attachDisabled` field to `PlayerAttachable`, which can be toggled to effectively disable `PlayerAttachable`-inheriting scripts.
  - `PlayerAttachable` disables itself without a player attached and reenables itself when a player attaches, so turning off the script doesn't actually work to prevent players from attaching.
- Made `ItemGrabbable` and `EventfulApparatus` invoke `onCollect` event when spawned inside the ship (for lobby reloads and/or late joins).
- Fixed `ItemDiscardable` items not being despawned in some circumstances, with `despawnOnDiscard` enabled.
  - Should be more reliable, in terms of networking.
- Fixed `ExplodeEffect` erroring out when damaging other players.
- Fixed `SunScreen` only actually working when used by interiors.

## [v0.8.0]

Added a few more alert scripts and an event, did several other small tweaks and fixes.

- Updated `README` to actually include ~~nearly~~ all components in the library.
  - `50+` previously unlisted ones were added, along with a short description of their intended usage.
- Added `PlayerTracker`, which can follow players and rotate multiple objects to point towards it at configurable speeds.
- Added `ApparatusEvent`, which simply invokes an event callback when an Apparatus is pulled by a player.
- Added `AlertToast`, which can display a toast message to players (e.g. dropship items missed alert).
- Added `AlertNotification`, which can display a notification message to players (e.g. new creature data alert).
- Added `AlertSignal`, which can display a signal translator message to players that can go over the vanilla letter cap.
- Made `AlertDialogue` inherit from the more abstract `NetworkedAlert`, just like the other alerts.
- Made `NetworkedHittable` and `ItemWhackable`'s weapon `hitID` fields a dropdown selection instead of a number value.
- Made `PlayerOuchie`'s player body ragdoll field a dropdown selection instead of a number value.
- Fixed `PlayerSeater` not resetting player animation state after sitting down (again).
- Readded a renamed (now deprecated) field to `ScrapSpawner`, for some backwards compatibility.
- Added a temporary hotfix for `Berunah` (from [Wesley's Moons](https://thunderstore.io/c/lethal-company/p/Magic_Wesley/Wesleys_Moons)).
  - Fixed error spam due to a leftover testing `Camera`.
- Made various other small additions and improvements.

## [v0.7.0]

Added various scripts and features, fixed a couple issues, removed some deprecated/legacy stuff.

- Added `ContentConditional`, for doing stuff that depends on certain modded content existing, or a specific plugin being found.
- Added `PlayerWeightEvent`, which periodically checks a player's weight to determine if it should trigger or not.
- Added `ShakeEffect`, which just triggers a screen shake effect for the local client.
  - Might swap it to something that inherits from `PlayerSensor` at some point.
- Added `VehicleSensor`, for checking if the Cruiser is inside, entering and/or exiting a region.
  - Will eventually add some bonus functions for doing stuff to the Cruiser (like applying force or dealing damage).
- Added `ReverbTriggerAdjuster`, to tweak certain `AudioReverbTriggers` not normally accessible from the moon scene.
  - Can adjust, replace, or disable `AudioReverbTriggers` present in the ship scene (`SampleSceneRelay`).
  - Should be careful with its use, since some of the triggers that can be replaced or disabled might be doing something important...
- Added `SyncedSpawn` activation time, to trigger scripts right before `SpawnSyncedObjects` begin their spawning.
- Added `onConditionalFail` to `BaseConditional`-inheriting scripts, which is called whenever the match in question fails.
- Added material overrides to `GiftSpawner`, which can be used to replace the gift's texture when spawned.
  - Also made it so multiple overrides can be selected for the gift's poof particle and opening sound effect.
- Changed `ScrapSpawner`'s moon scrap spawns field to add the scraps instead of completely overriding the weights.
- Changed `ScrapSpawner`'s `onSpawnPerformed` to be called only until after the scrap item has finished being initialized and synced.
- Changed `BaseConditional`-inheriting scripts to no longer stop after the first match, to continue applying other overrides.
  - Will likely add a field for this, in case cascading overrides are not desired.
- Changed `OutOfBoundsAdjuster` to not exit early after finding a kill trigger, in case the current moon has multiple for whatever reason.
- Renamed `DungeonStoryLog` to `EventfulStoryLog`, for it is no longer limited to just interiors.
- Fixed some issues with `IWeightedScript`-inheriting scripts when manually adding or modifying weights.
- Fixed `ScriptableEvents` with the same GUID not actually referring to the same instance.
- Fixed `MoonConditional` not matching moons properly due to a string-related issue.
- Fixed `TwinApparatus` shutting off power for good after only one being pulled.
- Fixed `ScrapSpawner`'s `onSpawnPerformed` callback only being called for the host.
- Fixed `ScrapSensor`'s `onRegionEntered` and `onRegionExited` callbacks only working for the host (probably).
- Removed a couple deprecated fields and scripts:
  - Removed deprecated `ActivationTime` field for the scripts that had it.
  - Removed obsolete (extra) `AudioGroup` script.
- Removed `PlatformUnstable` script, since its functionality can be recreated using other, more abstract scripts.

## [v0.6.1]

Some small optimizations and tweaks.

- Made `ConnectedRope` no longer allocate GC every frame...
- Removed some tag comparisons that weren't really needed.
- Added caching for the various `WaitFor` yield statements in `Coroutines`.
- Fixed original hives replaced by `HiveSpawner` still being able to be struck by lightning despite being deactivated.
- Made required components auto-assign the component in question to its relevant field (though there's not that many).
- Updated a few internal project dependencies.

## [v0.6.0]

Bunch of script reworks, some rather niche additions, a couple fixes.

- Added `ScriptableNetworkPrefab`, for registering one or several network prefabs from the editor.
  - Allows usage of `PrefabSpawner` for moons without manually registering prefabs.
  - A bit impractical to use at the moment, though...
- Added abstract `ComponentGroup`, for scripts that perform bulk actions with several components.
  - Made a separate `AudioGroup` that inherits from `ComponentGroup`, and deprecated the current one.
  - Added `RigidbodyGroup`, for applying force to several `Rigidbodies` at the same time.
- Added `RandomVector`, which just produces a random `Vector` within a set range and passes it onto an event to do stuff with on demand.
- Deprecated usage of blank references in `NetworkedSpawner` scripts to define what to spawn in favor of just item/enemy/prefab names.
  - Avoids potential issues with blank references being deleted by [LethalLevelLoader](https://thunderstore.io/c/lethal-company/p/pacoito/LethalLevelLoaderUpdated) after being replaced (as it's intended to), and I was already doing name comparisons to begin with.
  - Blank references in spawners will still work but will be removed at some point in the future.
- Added separate field for `EnemySpawner` to spawn a single specific enemy.
  - Will override any set weighted enemy spawns, _but_ allows switching the enemy to spawn through a method.
- Added option to immediately stop any spawned scrap's `AudioSource` components from playing audio after being spawned, to help a bit with potential missing sound issues.
  - _Looking at you, Dine scraps..._
- Added separate event callbacks to `NetworkedSpawner` for when the player has performed a hit using a `Shovel` or `Knife` specifically.
- Added methods for attaching a player or any transform to the start or end of a `ConnectedRope`.
- Added method to set `InteractPurchasable`'s interactable state.
- Internal tweaks to weighted scripts.
  - Added (default) function to `IWeightedScript` to add one or several weight entries to an already-initialized list of weights.
  - Added functions to the `IWeightedScript`-inheriting scripts to add and remove weights.
- Improved `WeatherConditional` handling of [WeatherTweaks](https://thunderstore.io/c/lethal-company/p/mrov/WeatherTweaks)' combined and progressing weathers a lot.
  - Notably now allows matching combined/progressing weathers with custom names, such as ones added through [Custom Weathers Toolkit](https://thunderstore.io/c/lethal-company/p/Zigzag/Combined_Weathers_Toolkit).
- Added an extra check to `ScrapSensor` to double check the item's spawned.
  - Should fix [PlayZone](https://thunderstore.io/c/lethal-company/p/LethalMatt/PlayZone) compatibility with some [BrutalCompanyMinusExtraReborn](https://thunderstore.io/c/lethal-company/p/SoftDiamond/BrutalCompanyMinusExtraReborn) modifiers.
- Removed temporary hotfix for `Abaddon` (from [Nightmare Moons](https://thunderstore.io/c/lethal-company/p/DemonMae/Nightmare_Moons)), for it has been fixed.

## [v0.5.1]

Added a new script, did a couple networking-related hotfixes.

- Added `AlertDialogue`, for sending dialogue messages to the player.
  - Uses the 'ship leaving early' UI to display messages.
  - Has a few customization options like sound effect override and (optional) delay between letters.
  - Several dialogue entries can be specified to display several messages, in sequence.
- Fixed `PlayerAttachable` not detaching clients properly over the network.
  - Also fixed some possible weirdness with despawning.
- Removed `Netcode` patching step from plugin initialization.

## [v0.5.0]

Updated for `v73` and above; reworked `ScriptableEvent`, added a couple new scripts and some small features.

- Updated library dependencies for `v73`, including `Unity` and `NetcodePatcher` versions.
  - **NOTE:** This release is _not_ backwards-compatible with `v72` and below, and downgrading to an older release is required to play on prior versions of the game.
- Made a few internal networking tweaks and improvements using the new `Rpc` attribute.
- Added `ConnectedRope`, for attaching a `LineRenderer` to several different points.
  - Similar to the game's `SetLineRendererPoints`, but for more than two points.
- Reworked `ScriptableEvent` and `ScriptableEventListener`.
  - Now uses a key system instead of a direct reference, and can thus handle being included in multiple bundles.
  - Added comments and tooltips to both of them.
- Added periodic stamina draining to `PlayerHinderer`.
  - Drain speed can be adjusted, as well as whether to take carry weight into account or not.
- Added `resetOnToggle` field for `DelayedEvent`, to determine whether the timer should reset when the script is disabled, or if it should be paused until re-enabled.
- Added a missing null check to `EventfulApparatus`.
- Added a small hotfix for `Abaddon` (from [Nightmare Moons](https://thunderstore.io/c/lethal-company/p/DemonMae/Nightmare_Moons)).
  - Fixed error spam due to a missing `NavMeshObstacle`.
  - Temporary fix since the moon cannot be updated at the moment.
- Added [Fish](https://www.youtube.com/watch?v=lPGipwoJiOM).

## [v0.4.6]

Actually updated `CHANGELOG` this time...

- I forgor to update the `CHANGELOG` for `v0.4.5`.

## [v0.4.5]

Added a new script, a couple new features, refactored some activation-related stuff.

- Added `PlayerElectrode`, for handling the draining of a player's chargeable items.
  - Held and/or pocketed items can be periodically drained (like active Flashlights), or manually drained by a percentage amount.
- Added drunkness (TZP) effect to `PlayerHinderer`.
  - Speed of the effect being applied to the player is configurable, and can be set to a high amount to immediately max out the effect.
  - Also added a separate field to disable slowing the player down, in case only the other `PlayerHinderer` features are needed.`
- Added a function to `HazardSensor` to deactivate (via Terminal code) any found hazards.
- Reworked all scripts that activate at a specified time to use the `IActivationScript` interface.
  - This includes `AnimationVelocity`, `AudioGroup`, `BaseConditional` (and its inheriting scripts), `DetectRegion` (and its inheriting scripts), `MaterialSwapper`, `NetworkedSpawner` (and its inheriting scripts), `ScrapTeleporter`, `ToggleEvent`, and `WeightedEvent`.
  - Should be backwards-compatible, but re-serializing any prefabs that use them (by saving the prefab) is recommended.
- Corrected some comments and tooltips here and there.

## [v0.4.4]

Added a new sensor, did some other small things, fixed Doorway stuff (again).

- Added `ShotgunSensor`, for detecting if the object is shot at by a player using a Shotgun.
  - Works by using the same principles as `SpraySensor`, so it also doesn't hook into any actual `ShotgunItem` code.
  - Will likely add compatibility with modded ranged weapons at some point, too.
- Added event callback to `IEventfulItem` for reacting to being placed on a `DepositItemsDesk`.
- Added a few comments and tooltips to `ItemGrabbable`, but it's not fully done yet.
- Fixed `SpecificDoorway` not working properly, but _actually_ this time...
- Removed some networking stuff from `PlatformGravity` that wasn't doing anything, since `PlayerPhysicsRegions` are not `NetworkBehaviours`.

## [v0.4.3]

Readded a small fix I undid by accident.

- Fixed `PlayerSeater` erroring out for clients other than the player sitting down.

## [v0.4.2]

Added a new spawner, fixed a couple things.

- Added `GiftSpawner`, for spawning (rigged) vanilla presents.
  - Item inside the gift can be randomly selected from a weighted list.
  - Minimum and maximum scrap values for the item inside the gift can be overridden.
  - Gift opening audio and poof particle can be overridden, too!
- Fixed `PlayerSeater` breaking player sitting under certain circumstances.
  - Now actually checking if the player is in a special animation before setting the `SA_stopAnimation` trigger.
- Fixed `SpecificDoorway` not actually working properly...

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

- Added `SpraySensor`, for detecting if the object is being sprayed with Spray Paint, Weed Killer, or any other item that uses or inherits `SprayPaintItem`.
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
