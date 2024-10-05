Note: If you're reading this, you are probably the first person to use this! This might be a bit of a pain to set up. Sorry. It also might require some tweaking to get working; it's really designed *for me* and hasn't been properly customized with config options to work for other people. But this will do most of the work for you, and it took a while to put it all together, so it should still be a big timesave.

This does assume your project is using C#. You can simplify the scripts considerably if you're not using C#. But I *am* using C#, so I haven't made those changes.

* First, move your entire project into a `project` subdirectory. This means `project.godot` will now be `project/project.godot`, and all your other files will also be in `project`. Remember to move your .gitignore. If you have a .github directory, don't move it, and you'll probably have to fix up some patches.
* Download the Godot source from Git and check out the specific version you want to start with. For the purposes of this documentation, we'll assume it's 4.3.0; that would be `git checkout 4.3.0-stable`.
* Go to the root directory of your project in your command line.
* `git checkout --orphan thirdparty_godot`. This will make all your stuff vanish; don't worry, we just need a new branch.
* Make a directory named `godot`.
* Copy the entire contents of the Godot source into that directory. Make sure you're including hidden files like `.gitignore`.
* `git add -f godot`, `git commit -m "Godot 4.3.0-stable"`.
* Go back to your main branch, which is probably called `main`. (It might be called `master` or `dev`.)
* `git merge --allow-unrelated-histories thirdparty_godot`.
* You should now have a `godot` directory and a `project` directory, where `godot` contains the engine source and `project` contains your game source.
* Copy everything from `godottools/vendoring` into your project's root. This will add a few batch files, a `nuget.config`, a `tools` directory, and a single file under `godot`. If you already have a `README.md`, append the contents of the current one to the end of your old one, or the beginning of it, or whatever.
* Check that in too.
* Go to `tools/build_utils.py` and edit the `get_project_name()` function to be whatever your actual project name is.
* Go to `godot` and rename `yourprojectname.build_profile.json` to `whateveryouractualprojectnameis.build_profile.json`.
* Check those in as well.
* Read the `README.md` and get your system set up for building Godot.

With luck, you can now run `build_and_run_editor.bat` and it'll build you up a brand-new fresh editor. Hooray! And now you can also edit the .cpp files, run that same batch file, and update the editor easily. Hooray!

**Make sure to push the `thirdparty_godot` branch**; it contains info you need to update properly. You can recover it if you need to, it's not a disaster if you forget, but it'll be a bit of a pain and you'll have to know how to work with Godot.

If/when you want to update the engine, run `tool.bat update_godot 4.4.2-stable` or whatever version you want to update to. It will *mostly* do everything, except it will likely run into merge conflicts if you've made significant changes, which you then get to resolve. Also, then remember the push the `thirdparty_godot` branch again.

I should probably set this up as a script, but I'm not doing that right now.

If you want to throw some money at me to set this up for you, let me know, but it'll probably cost a few hundred bucks. If you want to turn it into a script yourself, I will thank you profusely and add it to this repo.
