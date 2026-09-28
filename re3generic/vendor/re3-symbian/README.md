# re3-symbian

Work-in-progress port of GTA III for Symbian devices.

Based on re3, uses some code from [Dreamcast](https://gitlab.com/skmp/dca3-game) and [Vita](https://github.com/Rinnegatamante/librw-vita) ports.

## State

See [TODO.md](/TODO.md) for more details on project state.

## Device requirements

- S60 3rd Edition FP1, S60 5th Edition (non-Nokia), Symbian^3 or later
- GPU
- 128 MB RAM with at least 50 MB free (won't run on regular 64 MB N95 unless you disable all textures)
- Functional E: drive with 400 MB of free space (will be more as it'll start to support audio)

Tested devices:
- Belle with BCM2763 (700) - GLES 2.0
- Anna/Belle with BCM2727 (E7, N8, E6) - GLES 2.0
- S60v3.1 with PowerVR MBX (E90, N95) - GLES 1.1

## Building

Import bld.inf in Carbide.c++ with Symbian^3 or newer SDK, add PKG file to SIS Builder and build the project.

I use Carbide.c++ 3.2, Symbian Belle SDK with sbsv2 from QtSDK and RVCT 4.0, but it should also work with GCCE 4.4.1.

## License <small>(copied from original README)</small>

We don't feel like we're in a position to give this code a license.\
The code should only be used for educational, documentation and modding purposes.\
We do not encourage piracy or commercial use.\
Please keep derivate work open source and give proper credit.
