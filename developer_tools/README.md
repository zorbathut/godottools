**Developer Tools**

Have you ever wanted convenient in-game utilities for debugging and analysis? Well I sure did! So I made some!

Be warned: this is hackily extracted from my current project and *will not build* in its current state. It will take some work. If you do that work, could you send a pull request? Thanks!

Finally, **you have to include the LICENSES.txt somewhere* or remove the mentioned assets. Legal requirement. Your call on which!

Contents:

---

![Console window](https://github.com/zorbathut/godottools/blob/dev/developer_tools/README_console.png)

A CONSOLE WINDOW that displays Godot errors and messages as well as your own errors and messages! You can COPY STUFF OUT OF IT and maybe someday in the future it will have a BUILT-IN BUG REPORTER but it doesn't right now. Also it POPS UP ANNOYINGLY if you have a warning or an error! It is INTENSELY ANNOYING! This is a feature because it means you will actually see warnings and errors instead of not seeing them!

This currently relies on [an in-flight Godot patch](https://github.com/godotengine/godot/pull/87576) and you'll need to use a custom build of Godot to use it.

---

A DEBUG TOOL GUI that I cannot provide a screenshot of because right now I am in the middle of REWRITING IT! But it lets you see all your debug tools in one place and use mouse input for them.

You know what, here's a screenshot of Rimworld's, which is frankly what I'm cribbing off anyway:

![Console window](https://github.com/zorbathut/godottools/blob/dev/developer_tools/README_command.png)

Yeah! Imagine that! Only this isn't that yet because it's really early days. But imagine it anyway!

---

![Console window](https://github.com/zorbathut/godottools/blob/dev/developer_tools/README_prof.png)

A PROFILER that shows you what places in your code are slow! You need to INSTRUMENT THEM MANUALLY! This is actually A FEATURE because it means you can instrument ONLY THE REALLY IMPORTANT STUFF and run it in release without a significant performance hit! Then include a FULL PROFILER DUMP along with SLOW FRAME REPORTS so you can figure out why your code is slow! I don't yet have a way to provide a FULL PROFILER DUMP! Perhaps SOMEDAY!

---

A BLACKBOARD DEBUGGING WINDOW that is COMPLETELY USELESS because it's designed to work with [MY PERSONAL BEHAVIOR TREE SYSTEM](https://github.com/zorbathut/arbor) that quite frankly NOBODY SHOULD BE USING!

You should probably just take this out.

(And send me a pull request.)

---

A PAUSE BUTTON that DOESN'T WORK!

(You should probably take this out too. Or fix it.)