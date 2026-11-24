# Witcher Direct Combat

An experimental controller and combat mod for **The Witcher: Enhanced Edition** on Windows. This beta widens the game's sword combo timing window and maps an Xbox controller to movement, camera, sword styles, dodging, and Signs. You can press an attack button repeatedly without watching for the flaming sword cursor. The installer offers a bundled unsigned controller executable or the option to compile it from source on your PC.

This is an early community beta, not a replacement for The Witcher 1 combat engine. The game still chooses targets under the cursor, resolves hits and parries, and plays its original attack animations. There is no lock-on targeting. It has been tested with the Steam build whose game window reports `1.4.5.1304`, in F3 over-the-shoulder view.

## Download and install

1. Download the repository as a ZIP from GitHub and extract it.
2. Close Witcher completely.
3. Double-click `Install.cmd`. The first prompt defaults to **P** so you can paste your game folder path; choose **S** to search common Steam library locations. Spaces and surrounding quotes are accepted. The installer verifies `System\witcher.exe` and asks you to confirm the exact folder.
4. Choose **U** to install the bundled unsigned controller executable (default), or **C** to compile the included C# source yourself.
5. Double-click `DirectCombat\Play.cmd` inside the game folder. It starts the controller bridge and launches the game through Steam; choose **Play The Witcher** in Steam's launch options.

The compile option builds [`DirectCombatController.cs`](Payload/DirectCombatController.cs) with the Windows .NET Framework C# compiler. The bundled executable is built from that same source and is not digitally signed. Both options install our `testtools.lua` and launcher. The installer refuses to overwrite an existing `System\Scripts\testtools.lua` that differs from ours. It does not change saves or the game executable. It saves the verified game path as `location.txt` beside `Install.cmd` and `Uninstall.cmd` in the extracted installer folder. Keep that folder until you uninstall. To remove the beta, run `Uninstall.cmd`; it reads `location.txt`, waits for Witcher to close if needed, stops the installed controller bridge, and removes only files that still match the install manifest.

## Controller layout

| Input | Action |
| --- | --- |
| Left stick | Move Geralt (F3 view) |
| Right stick | Turn the camera / cursor in F3 view |
| X | Attack with the selected style |
| Y | Group-style attack |
| A | Interact: talk, pick up, use |
| B + left stick direction | Dodge in that direction |
| Left stick click | Sheathe or draw the last used sword |
| D-pad Up | Cycle combat style: fast, strong, group |
| D-pad Left | Cycle weapon: steel, silver, extra weapon 1 |
| D-pad Right | Cycle Sign: Aard, Quen, Yrden, Igni, Axii |
| D-pad Down | Open the inventory |
| View / Back | Esc: game menu, where the game can be saved |
| Menu (hamburger) button | Pause or resume combat (Space) |
| RB or RT | Hold, then let go to cast the selected Sign |

X attacks with the style chosen on D-pad Up; Y always uses the group style. A sends a left click, which talks, picks up and uses, and also attacks when the cursor is on an enemy with the sword drawn. The left stick click presses the sword key last used, which sheathes it; the same press draws it again. The game may parry attacks made with a poor style match.

## How it works

The controller bridge reads XInput and sends the game's existing keyboard and mouse commands while the Witcher window is in the foreground. The window is matched by its owning process, so localized titles such as `Ведьмак (...)` work. The Lua startup hook sets `g_cAuroraSettings.m_bUseOldAttackMechanics = true` and `g_cAuroraSettings.m_fCombatTimeAdvance = 10.0`. The wider timing window is what lets repeated presses continue the flaming combo without watching its cue. Game behavior and balance outside the tested fights may vary.

The controller source is [`Payload/DirectCombatController.cs`](Payload/DirectCombatController.cs). The Lua source is [`Payload/testtools.lua`](Payload/testtools.lua). The package contains a prebuilt unsigned controller executable and no original game files. Source code is MIT licensed; The Witcher and its assets belong to their respective owners.

## Known limitations

- Requires 64-bit Windows and an XInput controller. Compiling from source also requires the .NET Framework 4 C# compiler. The bridge requests F3 over-the-shoulder view once when Witcher first becomes active; the player confirmed the right stick turns the camera normally in F3. If you change camera views later, press F3 on the keyboard to return.
- The game can still parry or miss; Fast style landed more hits than Strong against one agile test enemy.
- The dodge needs a sword drawn and may require holding B while moving the stick.
- Sheathing presses the sword key the bridge last sent. If the sword is changed with the keyboard or an in-game menu, the first left stick click may switch weapons instead of sheathing; click again to sheathe.
- D-pad Up changes the combat style immediately. The game may draw the sword when a style key is pressed.
- The bridge uses controller slot 0. It waits up to 30 minutes for `witcher.exe` to start, because Steam shows three launch options and the choice can take a while. During play it reuses the controller input it already reads: after 30 seconds without controller input it checks whether the remembered Witcher process still exists, repeating only every 30 seconds while idle. A player can put the controller down and pick it up again while the game remains open. The bridge has a 24-hour maximum session length.
- The launcher and controller program prevent a second bridge instance; an earlier beta launcher could start duplicates and make right-stick camera movement erratic.
- The player confirmed the Menu button toggles combat pause on and off.

Bug reports and improvements are welcome. Please include the game version, controller type, camera view, and exact buttons pressed.
