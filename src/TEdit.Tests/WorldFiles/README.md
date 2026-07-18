# Test world fixtures

Terraria `.wld` fixtures are stored as immutable, single-file `.wld.zip` archives in Git LFS.
Tests extract missing worlds into their output directory during module initialization.

Add a fixture from the repository root with:

```powershell
./tools/Add-TestWorld.ps1 C:\path\to\new-version.wld
```

Use the optional second argument to place or rename a fixture:

```powershell
./tools/Add-TestWorld.ps1 C:\path\to\world.wld calamity/Calamity1.wld
```

Archives are immutable. Add a new archive instead of replacing an existing fixture.
Uncompressed `.wld` files under this directory are ignored by Git.
