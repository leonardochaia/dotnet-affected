

`DotnetAffected.Core` uses `LibGit2Sharp` to analyse changes using the git history/log.  

`LibGit2Sharp` uses native, os based, git libraries to perform all git operations.  

Loading of the native libraries is based on predefined locations relative to the application/assembly.
It works fine when used in the context of an application.
With a Task however, it does not.

A task is loaded in the context of the build running it.  
When the task activates `LibGit2Sharp`, it will try to load the native libraries, however in the
context of the build application which does not have access to the native libraries.

This is known issue when using dependencies in a build library using Tasks.
The solution is to omit dependency declaration and instead physically include them with the
shipped library.

With `LibGit2Sharp` we also need to tell it where to look for the native libraries.
`LibGit2Sharp`' will load the file from the path we provide, however for osx/linux it will
try to load a file which does not exists since it comes with a `lib` prefix which the 
os native library resolver knows how to handle but with custom load it fails.

To workaround that we just add an additional build step to the library to also have a copy
of the native library without the `lib` prefix.

So we have 2 files for each implementation, for example: `libgit2-XYZ.so` and `git2-XYZ.so`
