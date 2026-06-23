# Credits & third-party notices

InDows is released under the [MIT License](LICENSE).

The tweak values, app lists, and revert approaches behind the modules are **distilled from** these
open-source projects. No code is copied verbatim — registry paths/values and methods are reused with
attribution. Big thanks to their authors:

| Project | Author | Link | License |
| --- | --- | --- | --- |
| **WinUtil** | Chris Titus Tech | <https://github.com/christitustech/winutil> | MIT |
| **Win11Debloat** | Raphire | <https://github.com/Raphire/Win11Debloat> | MIT |
| **optimizerDuck** | itsfatduck | <https://github.com/itsfatduck/optimizerDuck> | see repo |
| **unattend-generator** | Christoph Schneegans | <https://github.com/cschneegans/unattend-generator> | see repo |

The base `autounattend.xml` is produced with Christoph Schneegans' **unattend-generator** (see
[`docs/base-recipe.md`](docs/base-recipe.md)); InDows grafts its bootstrap, catalog and module anchors
on top.

If you are one of these authors and want the attribution changed or removed, please open an issue.
