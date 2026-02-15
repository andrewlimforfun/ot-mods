# Development Notes

Personal development notes for setting up and maintaining this C# modding project.

## Initial Project Setup

### Creating the Project

Create a new C# class library project that outputs a DLL:
```sh
dotnet new classlib -n DnDUtil
```

### Configuring NuGet Sources

Add the official NuGet and BepInEx package sources:
```sh
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
dotnet nuget add source https://nuget.bepinex.dev/v3/index.json -n bepinex
```

### Installing Dependencies

Add required packages for BepInEx modding:
```sh
dotnet add package BepInEx.Core --version 5.4.21
dotnet add package BepInEx.Analyzers --version 1.0.8
dotnet add package HarmonyX --version 2.10.1
```

**Note:** Link the project to On-Together DLLs in the game's `Managed` directory by adding references in the `.csproj` file.

### Setting Up the Solution

Create and configure the solution file:
```sh
dotnet new sln -n AndrewMod
dotnet sln add DnDUtil
```

### Setting Up Tests

Create a test project with NUnit:
```sh
dotnet new nunit -n DnDUtil.Tests  
dotnet sln add DnDUtil.Tests
dotnet add DnDUtil.Tests reference DnDUtil 
dotnet add DnDUtil.Tests package Moq
```

## Common Development Tasks

### Building the Project
```sh
# Debug build (local testing)
dotnet build

# Release build (Thunderstore package)
dotnet build -c Release

# Verbose output for debugging build issues
dotnet build -v d
```

### Running Tests
```sh
# Run all tests
dotnet test

# Run with detailed output
dotnet test -v n

# Run specific test
dotnet test --filter "TestName~RollCommand"
```

### Cleaning Build Artifacts
```sh
dotnet clean
```

## Project Architecture

### Key Components

- **Plugin.cs**: Main entry point, registers with BepInEx
- **CommandManager.cs**: Dynamically discovers and executes commands via reflection
- **IChatCommand**: Interface for all chat commands
- **Harmony Patches**: Used to intercept game methods (e.g., TextChannelManagerPatch)

### Adding a New Command

1. Create a new class in `Core/Commands/` that implements `IChatCommand`
2. Implement the `Name` and `Description` properties
3. Implement the `Execute(string[] args)` method
4. The command is automatically registered via reflection in `CommandManager`

Example:
```csharp
public class MyCommand : IChatCommand
{
    public string Name => "mycommand";
    public string Description => "Does something cool";
    
    public void Execute(string[] args)
    {
        // Implementation
        ChatUtils.AddGlobalNotification("Command executed!");
    }
}
```

## Troubleshooting

### Build Errors

**Missing game references:**
- Ensure `Reference` elements in `.csproj` point to valid game DLL paths
- Check that On-Together is installed and paths are correct

**NuGet restore fails:**
- Verify NuGet sources are configured correctly
- Try `dotnet nuget locals all --clear` to clear caches

### Runtime Issues

**Mod not loading:**
- Check BepInEx console for errors
- Verify mod DLL is in `BepInEx/plugins/`
- Ensure BepInEx is properly installed

**Commands not working:**
- Check that command names don't conflict
- Verify `EnableFeature` config is set to `true`
- Look for exceptions in BepInEx logs

## Useful Commands

### Package Management
```sh
# List installed packages
dotnet list package

# Update a specific package
dotnet add package BepInEx.Core --version <version>

# Remove a package
dotnet remove package <package-name>
```

### Project Information
```sh
# Show project references
dotnet list reference

# Show solution projects
dotnet sln list
```

## C# Learning Notes

As my first C# project, key learnings:

- **Reflection**: Used for auto-discovering command classes
- **Configuration**: BepInEx provides `ConfigEntry<T>` for persistent settings
- **Harmony Patching**: Powerful way to modify game behavior without source code
- **Delegates/Events**: Used for hooking into game events
- **Nullable Reference Types**: Handled with `?` operator for safety

## References

- [BepInEx Docs](https://docs.bepinex.dev/)
- [Harmony Patching Guide](https://harmony.pardeike.net/)
- [C# Language Reference](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [.NET API Browser](https://learn.microsoft.com/en-us/dotnet/api/)
