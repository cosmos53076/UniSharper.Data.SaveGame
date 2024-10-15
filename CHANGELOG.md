# Changelog

All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).



## [4.0.0] - 2024-10-15

### Added

- Adds interface **ICompressionProvider**.
- Adds class **DefaultCompressionProvider**.



### Changed

- Rename interface **ISaveGameDataCryptoProvider** as **ICryptoProvider**.
- Rename class **SaveGameDataCryptoProvider** as **DefaultCryptoProvider**.



## [3.0.1] - 2024-07-02

### Fixed

- Fixed compilation errors in Unity 2019.4.x.



## [3.0.0] - 2024-04-11

### Added

- Adds interface **ISaveGameManager**.



## [2.0.0] - 2021-11-10

### Changed

- Update the dependency version of **UniSharper.Core**.
- Change the data encryption/decryption. No need decryption key to load game data.



## [1.0.1] - 2021-03-08

### Changed

- Update the dependency version of **UniSharper.Core**.
- Refactor package layout as [Unity Manual](https://docs.unity3d.com/2019.4/Documentation/Manual/cus-layout.html).



## [1.0.0] - 2020-04-23

 - Initial unity project.

