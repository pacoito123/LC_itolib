# Changelog

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
