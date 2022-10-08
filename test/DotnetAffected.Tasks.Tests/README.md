# DotnetAffected.Tasks.Tests

We don't get a lot of value from unit testing an MSBuild task.  
`DotnetAffected.Tasks` is mostly a wrapper around `DotnetAffected.Core` which
execute it within the context of an MSBuild process.

To get proper feedback we need to test the task in a realistic build context.

In other words, we need to perform integration tests where we run a build process
with `DotnetAffected.Tasks` registered and evaluate the results.

## Why integration testing is required

> It is always required in MSBuild Task packages but in this case even more!

`DotnetAffected.Tasks` is an MSBuild task package.  
MSBuild task packages run within the context of the build, not the application it is building.  

I.E MSBuild task package is a dynamic plugin, an external library to the executing assembly that was not present
when the assembly was compiled (MSBuild dll).

This is why MSBuild task packages can not declare external dependencies (package references)
because there is not build process that will restore them.

Instead MSBuild task packages must contain a physical copy of all dependencies they require within the distributed nuget file.  
It is straight forward in most cases however, `DotnetAffected.Core`
depends on `LibGit2Sharp` which introduce some challenges.

To simplify, `LibGit2Sharp` depends on native, os based, libraries which it dynamically link 
at runtime. `LibGit2Sharp` comes with those libraries but it will load them assuming
they are physically located in a location relative to the executing assembly.  
Oh no, the executing assembly is MSBuild thus **it will not be able to load them** because
they are in a location relative to physical location of `LibGit2Sharp` (which is where `DotnetAffected.Tasks` is)

Luckily, `LibGit2Sharp` accept as an input the custom library to load the dynamic
libraries from. It has a bug which we also workaround here where it will look
for a file in the format `libgit2-XYZ.QQ` but in linux/osx the files are `git2-XYZ.QQ`.  
We can only provide the path to the folder holding the libraries, we can't modify the name so
this package, when packaged, will ensure each `libgit2-XYZ.QQ` is also duplicated as `git2-XYZ.QQ`.  

> The workaround makes the lib's payload a bit bigger.  
> TODO: Open issue in `LibGit2Sharp`

## How testing is performed

Each test has a dedicated `Repository` created in a temporary folder.  
Each repo comes with 2 files, located in the repo root:

- **Directory.Build.props**  
Add the `PrintAffectedProjects` which runs after `DotnetAffected.Tasks` finished.  
`PrintAffectedProjects` prints the full path to all projects detected by `DotnetAffected.Tasks`  
The output then processed by `BaseAffectedTaskBuildTest`, which expose it for every
test class to use.  
General output, errors and projects are available.  


- **ci.props**
Registers `DotnetAffected.Tasks` as an Sdk.  
Imports the test scenario `props` file to run.

> **ci.props** can run without an additional file to import

When a test execute a `Process` is spun that run's `dotne ci.props` with a specific target
that will execute `DotnetAffected.Tasks` and populate the item `ProjectReference` with
the project's detected by `DotnetAffected.Core`.

## Test scenarios

When we start a new repository we can provide an additional MSBuild project
file that will allow us to simulate test scenarios that will reflect different outcomes.

We can do this using **before/after** events on the `PrintAffectedProjects` task.

For example:
```xml
<Project>
    <ItemGroup>
        <AffectedFilterClass Include="IsClientLibrary">
            <IsClientLibrary />
        </AffectedFilterClass>
    </ItemGroup>

    <Target Name="_BeforePrintAffectedProjects" BeforeTargets="PrintAffectedProjects">
        <ItemGroup>
            <ProjectReference Remove="@(ProjectReference)" />
            <ProjectReference Include="@(AffectedFilterInstance)" Condition="'%(AffectedFilterInstance.IsClientLibrary)' == 'true'" />
        </ItemGroup>
    </Target>
</Project>
```

The above example will filter the `ProjectReference` collection to
contain affected projects that have the property `IsClientLibrary` set to `true`.

Because it run **before** `PrintAffectedProjects`, `PrintAffectedProjects` will
print the filtered results thus in the test class we should expect only those filtered projects.

