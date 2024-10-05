**Zorba's Godot Tools**


[![Language: C#](https://img.shields.io/badge/language-C%23-blue)](https://docs.microsoft.com/en-us/dotnet/csharp/) [![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT) [![Support](https://img.shields.io/discord/703688553707601962?label=support&logo=discord)](https://discord.gg/vQv9DMA)

This repository contains a bunch of Godot-related game code that people might find useful. Much of it is kind of yanked unceremoniously out of my current project and may require some massaging to fit nicely into a new project. Browse the directories to see what you can use, then use it. Enjoy!

Pull requests welcome, bug reports welcome, requests welcome, feedback in general welcome. If you want to talk about things in realtime, hit up [the Dec discord](https://discord.gg/vQv9DMA), which isn't *really* related to this project but isn't entirely unrelated either.

----

[**vendoring**](https://github.com/zorbathut/godottools/tree/dev/vendoring): This contains a set of scripts and tooling for making your own custom changes to Godot and making it relatively easy to use. Right now this assumes everyone working on the game will be willing to build the engine, but it does make that into a one-click process. Read [SETUP.md](https://github.com/zorbathut/godottools/tree/dev/vendoring/SETUP.md) for instructions on how to get it set up, and pester me on Discord if you're having trouble.

[**developer_tools**](https://github.com/zorbathut/godottools/tree/dev/developer_tools): This contains a bunch of in-engine convenient developer tools, both for your own purposes and for the benefit of modders. Right now this includes an ingame debug log, a simple visual profiler, and a debug-command GUI. Be aware that this is hackily extracted from my current project and will require some work to get functioning properly.

----

This isn't part of *this* repo, but if you want a good framework for a data-driven game and moddability, check out [Dec](https://github.com/zorbathut/dec).
