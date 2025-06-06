# Changelog

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
