# DotnetAffected.Tasks

`DotnetAffected.Tasks` is an MSBuild project SDK that allows project tree owners the ability to build projects which
have changed or if their have a dependency which have changed.

In an enterprise-level CI build, you want to have a way to control what projects are built in your hosted build system
based on their modified state.

`DotnetAffected.Tasks` SDK provides the automated filtering
while [Microsoft.Build.Traversal](https://github.com/microsoft/MSBuildSdks/tree/main/src/Traversal) is used
for execution.

## Example

1. Ensure `DotnetAffected.Tasks` SDK is registered as an SDK  
   This can be done in one of:
    - Adding it to `global.json`
    ```json
    {
        "msbuild-sdks": {
            "DotnetAffected.Tasks": "3.0.0"
        }
    }
    ```
    - Alternatively, you need to specify the version together with SDK name
    ```xml
    <Project Sdk="DotnetAffected.Tasks/3.0.0">
        <!-- ... -->
    </Project>
    ```
2. Create a dedicated `props` file **in the root of the git repo**:
    ```xml
    <Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    </Project>
    ```
   The actual filename is for you to choose, here we set it to `ci.props`
3. Run the build/test/clean/etc using the `ci.props` file.
    ```bash
    dotnet build ./ci.props
    ```

## Context Filtering

With `Context Filtering` you can control which projects are references based on evaluated
properties from the references project itself.

For every project that a was impacted from the change, you can analyze and filter based on the
evaluated properties of the project.

First you need to define the properties you want to retrieve from each project
so you can evaluate them before running it and optionally filter them out.

We group a collection of such properties and call it `AffectedFilterClass`.  
Each project is then assigned a new instance of `AffectedFilterClass` representing the values in the project.

```xml

<ItemGroup>
    <AffectedFilterClass Include="No Backoffice">
        <IsBackofficeLibrary/>
        <!-- Add more project properties here... -->
    </AffectedFilterClass>
    <!-- Add more AffectedFilterClass items here... -->
</ItemGroup>
```

`Include="No Backoffice"` (ItemSpec) is used to assign an identity for the group so
we can use it to create smart filtering based on different filter classes.

Now, for every project, the property `IsBackofficeLibrary` will evaluate and assigned to a new
object, along with other properties defined.

> If a defined property does not exist in the project, the value in the class is used.
> I.E you can apply default values!
> Note that only for properties that **DOES NOT EXISTS**, empty values == exists!

**ci.props**

```xml

<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    <!--
        Define a filter class, each class will later have multiple instances.
        The amount of instances is equal to the total amount of projects with changes.
        Each instance will hold the properties defined, however they will contain the actual
        values from the project.
        
        The "Identity" will be the full path to the project's file.
        A special property "AffectedFilterClassName" will be used to reflect the "Include" attribute of the class.
        You can use it to create smart filtering based on different filter classes.
    -->
    <ItemGroup>
        <AffectedFilterClass Include="No Backoffice">
            <IsBackofficeLibrary/>
            <!-- Add more project properties here... -->
        </AffectedFilterClass>
        <!-- Add more AffectedFilterClass items here... -->
    </ItemGroup>

    <Target Name="_DotnetAffectedCheck" AfterTargets="DotnetAffectedCheck">
        <ItemGroup>
            <ProjectReference Remove="@(AffectedFilterInstance)"
                              Condition="'%(AffectedFilterInstance.IsBackofficeLibrary)' == true"/>
        </ItemGroup>
        <Message
            Text="Role: %(AffectedFilterInstance.AffectedFilterClassName) | Filtered: %(AffectedFilterInstance.Identity)"
            Condition="'%(AffectedFilterInstance.IsBackofficeLibrary)' == true" Importance="high"/>
    </Target>
</Project>
```

## Extensibility

You can add/remove projects after the affected projects resolved, ad-hoc in `ci.props`:

```xml

<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    <Target Name="_DotnetAffectedCheck" AfterTargets="DotnetAffectedCheck">
        <Message Text="Found $(DotnetAffectedProjectCount) projects: " Importance="high"/>

        <ItemGroup>
            <ProjectReference Remove="$(MSBuildThisFileDirectory)/src/DevTools/**/*.csproj"/>
        </ItemGroup>
    </Target>
</Project>
```

Setting the following properties control how `DotnetAffected.Tasks` SDK works.

| Property                      | Description                                                                                             |
|-------------------------------|---------------------------------------------------------------------------------------------------------|
| `CustomBeforeAffectedProps`   | A list of custom MSBuild projects to import **before** `DotnetAffected.Tasks` **props** are declared.   |
| `CustomAfterAffectedProps`    | A list of custom MSBuild projects to import **after** `DotnetAffected.Tasks` **targets** are declared.  |
| `CustomBeforeAffectedTargets` | A list of custom MSBuild projects to import **before** `DotnetAffected.Tasks` **targets** are declared. |
| `CustomAfterAffectedTargets`  | A list of custom MSBuild projects to import **after** `DotnetAffected.Tasks` **targets** are declared.  |

<br />

So instead of using ad-hoc `.targets` code in your `.props` file, you can:

**ci.props**:

```xml

<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    <PropertyGroup>
        <CustomAfterAffectedTargets>$(CustomAfterAffectedTargets);$(MSBuildThisFileDirectory)ci.targets
        </CustomAfterAffectedTargets>
    </PropertyGroup>
</Project>
```

**ci.targets**:

```xml

<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <Target Name="_DotnetAffectedCheck" AfterTargets="DotnetAffectedCheck">
        <ItemGroup>
            <ProjectReference Remove="$(MSBuildThisFileDirectory)/src/DevTools/**/*.csproj"/>
        </ItemGroup>
    </Target>
</Project>
```

Note that all [Microsoft.Build.Traversal](https://github.com/microsoft/MSBuildSdks/tree/main/src/Traversal)extensibility
options
are also valid!

## Input / Output API

| Name                                                                    | Type                  | Data Type | Description                                                                                                                                                                                                                                                                                                          |
|-------------------------------------------------------------------------|-----------------------|-----------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| DotnetAffectedCheck                                                     | Target                |           | Actual target that calculate the affected projects.<br/> You can execute a task **BEFORE** this task to setup stuff or **AFTER** this task to post-process / filter projects.                                                                                                                                        |
| UsingDotnetAffectedTasks                                                | `Input`<br/>Property  | bool      | Indicating if the `DotnetAffectedCheck` should run. <br/>If `empty string` or `true` it will run, any other value **disable** this plugin.                                                                                                                                                                           |
| DotnetAffectedRoot                                                      | `Input`<br/>Property  | string    | Path to the root directory of the project, where `.git` folder is. <br/> **Default**: [MSBuildStartupDirectory](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-reserved-and-well-known-properties?view=vs-2022)                                                                                      |
| [DotnetAffectedFromRef](./Examples/from-ref-to-ref.props)               | `Input`<br/>Property  | string    | A branch or commit to compare against **ToRef**. <br/> **Default**: Working directory (when value is null/empty string)                                                                                                                                                                                              |
| [DotnetAffectedToRef](./Examples/from-ref-to-ref.props)                 | `Input`<br/>Property  | string    | A branch or commit to compare against **FromRef**. <br/> **Default**: HEAD (when value is null/empty string)                                                                                                                                                                                                         |
| [DotnetAffectedAssumeChanges](./Examples/assume-changes.props)          | `Input`<br/>Property  | string[]  | Forces referenced projects as changed instead of using Git diff to determine them. <br/> Set references similar to how [&lt;ProjectReference&gt;](https://learn.microsoft.com/en-us/visualstudio/msbuild/common-msbuild-project-items?view=vs-2022#projectreference) is used. <br/>**Default**: Empty (Use Git Diff) |
| DotnetAffectedProjectCount                                              | `Output`<br/>Property | int       | The amount of projects detected to be modified by `DotnetAffected`                                                                                                                                                                                                                                                   |
| CustomBeforeAffectedProps                                               | Property              | string    | A list of custom MSBuild projects to import **before** `DotnetAffected.Tasks` **props** are declared                                                                                                                                                                                                                 |
| CustomAfterAffectedProps                                                | Property              | string    | A list of custom MSBuild projects to import **after** `DotnetAffected.Tasks` **props** are declared.                                                                                                                                                                                                                 |
| CustomBeforeAffectedTargets                                             | Property              | string    | A list of custom MSBuild projects to import **before** `DotnetAffected.Tasks` **targets** are declared                                                                                                                                                                                                               |
| CustomAfterAffectedTargets                                              | Property              | string    | A list of custom MSBuild projects to import **after** `DotnetAffected.Tasks` **targets** are declared.                                                                                                                                                                                                               |
| [AffectedFilterClass](./Examples/per-project-evaulated-filtering.props) | PropertyGroup         | string    | Definition of properties to extract from each evaluated project instance for post-processing                                                                                                                                                                                                                         |

## How can I use these SDKs?

When using an MSBuild Project SDK obtained via NuGet (such as the SDKs in this repo) a specific version **must** be
specified.

Either append the version to the package name:

```xml

<Project Sdk="Microsoft.Build.Traversal/2.0.12">
    ...
```

Or omit the version from the SDK attribute and specify it in the version in `global.json`, which can be useful to
synchronise versions across multiple projects in a solution:

```json
{
    "msbuild-sdks": {
        "Microsoft.Build.Traversal": "2.0.12"
    }
}
```

Since MSBuild 15.6, SDKs are downloaded as NuGet packages automatically. Earlier versions of MSBuild 15 required SDKs to
be installed.

For more information, [read the documentation](https://docs.microsoft.com/visualstudio/msbuild/how-to-use-project-sdk).

## Troubleshooting

### Invalid Git repository or workdir

If your getting the error:

```
error : Path '/x/y/z' doesn't point at a valid Git repository or workdir.
```

You are probably executing the build from a folder that is not the root of the git repository.

Either execute it from the root or explicitly set the root in the `.props` file

**ci.props**

```xml

<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    <PropertyGroup>
        <DotnetAffectedRoot>$(MSBuildThisFileDirectory)..\..</DotnetAffectedRoot>
    </PropertyGroup>
</Project>
```

### TargetFramework resolution [Error MSB4062]

If your getting the error:

```
error MSB4062: The "DotnetAffected.Tasks.AffectedTask" task could not be loaded from the assembly ....
```

It is most probably due to invalid `TargetFramework` resolution.

Project files that use `DotnetAffected.Tasks` or `Microsoft.Build.Traversal` are framework agnostic
as they don't actually build projects, they just delegate the build to a different execution with it's
own configuration.

However, `DotnetAffected.Tasks` itself contains code to execute in build time, which
support TFMs `netcore3.1`, `net6.0` and `net7.0`.

The TFM must be known so the proper TFM facing assembly is used.

In most cases it is automatically resolved using the following logic:

- The value of the build property `MicrosoftNETBuildTasksTFM`
- The value of the build property `MSBuildVersion`
    - If >= `17.0.0` it will resolve to `net6.0`
    - Else if >= `16.11.0` it will resolve to `net5.0`
    - Else it will resolve to `netcoreapp3.1`

If you have issues, you can override the logic by specifically setting the `<TargetFramework>`.

```xml

<Project Sdk="DotnetAffected.Tasks;Microsoft.Build.Traversal">
    <PropertyGroup>
        <TargetFramework>net6.0</TargetFramework>
    </PropertyGroup>
</Project>
```

> `DotnetAffected.Tasks` provides MSBuild integration using `DotnetAffected.Core` under the hood.  
`DotnetAffected.Core` support TFMs `netcore3.1`, `net6.0` and `net7.0`.

