# dotnet-affected SDK

The `dotnet-affected` MSBuild project SDK allows project tree owners the ability to build projects which have changed or if their have a dependency which have changed.

In an enterprise-level CI build, you want to have a way to control what projects are built in your hosted build system based
on their modified state.

`dotnet-affected` SDK provides the automated filtering while [Microsoft.Build.Traversal](https://github.com/microsoft/MSBuildSdks/tree/main/src/Traversal) is used
for execution.

## Example

1. Ensure `dotnet-affected` is installed **globally** as a tool
2. Ensure `dotnet-affected` is availble locally, as an SDK  
   This can be done in one of:
   - Adding it a build time package (private runetime asset)
   - Adding it to `global.json`
```json
{
  "msbuild-sdks": {
    "dotnet-affected" : "3.0.0"
  }
}
```

3. Create a dedicated `props` file **in the root of the git repo**:

```xml
<Project Sdk="dotnet-affected;Microsoft.Build.Traversal">
</Project>
```

> The actual filename is for you to choose, here we set it to `ci.props`

4. Run the build/test/clean etc...

```bash
dotnet build ./ci.props
```

## Extensibility

You can add/remove projects after the affected projects resolved, in `ci.props`:

```xml
<Project Sdk="dotnet-affected;Microsoft.Build.Traversal">
    <PropertyGroup>
        <CustomAfterAffectedTargets>$(CustomAfterAffectedTargets);$(MSBuildThisFileDirectory)ci.targets</CustomAfterAffectedTargets>
    </PropertyGroup>
</Project>
```

We register a `ci.targets` file to load **after** the SDK `targets` file have loaded:

```xml
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <Target Name="_DetectAffected" AfterTargets="DetectAffected">
        <ItemGroup>
            <ProjectReference Remove="$(MSBuildThisFileDirectory)/src/DevTools/**/*.csproj" />
        </ItemGroup>
    </Target>
</Project>
```

Setting the following properties control how `dotnet-affected` SDK works.

| Property                            | Description |
|-------------------------------------|-------------|
| `CustomBeforeAffectedProps `  | A list of custom MSBuild projects to import **before** `dotnet-affected` properties are declared. |
| `CustomAfterAffectedProps`    | A list of custom MSBuild projects to import **after** `dotnet-affected` properties are declared.|
| `CustomBeforeAffectedTargets` | A list of custom MSBuild projects to import **before** `dotnet-affected` targets are declared.|
| `CustomAfterAffectedTargets`  | A list of custom MSBuild projects to import **after** `dotnet-affected` targets are declared.|

<br />

Note that all [Microsoft.Build.Traversal](https://github.com/microsoft/MSBuildSdks/tree/main/src/Traversal) extensibility options
are also valid!
