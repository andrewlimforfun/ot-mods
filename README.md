# On-Together Mods

A collection of mods for the game On-Together, built with BepInEx.

## Mods in this Repository

### DnDUtil
A mod to help players play chat-based Dungeons & Dragons within On-Together.

**Features:**
- Roll dice with customizable broadcasting (self/local/global)
- Configure announcer name and settings
- Multiple dice rolling in single command
- In-game command management

[View detailed documentation](DnDUtil/README.md)

## Development Setup

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (compatible with netstandard2.1)
- On-Together game installed
- BepInEx for On-Together

### Initial Setup

1. **Clone the repository:**
   ```sh
   git clone <repository-url>
   cd ot-mods
   ```

2. **Add NuGet sources:**
   ```sh
   dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
   dotnet nuget add source https://nuget.bepinex.dev/v3/index.json -n bepinex
   ```

3. **Restore dependencies:**
   ```sh
   dotnet restore
   ```

### Building

#### Development Build
Build and install the mod locally for testing:
```sh
dotnet build -v d
```

#### Release Build
Build and generate a Thunderstore-ready package:
```sh
dotnet build -v d -c Release
```

The release build creates a deployment package in:
```
DnDUtil/bin/Release/netstandard2.1/Staging/
```

### Testing

Run unit tests:
```sh
dotnet test
```

Run tests with detailed output:
```sh
dotnet test -v n
```

### Project Structure

```
ot-mods/
├── DnDUtil/              # Main mod project
│   ├── Core/             # Core functionality
│   │   ├── Commands/     # Chat command implementations
│   │   ├── ChatUtils.cs  # Chat utility methods
│   │   └── CommandManager.cs
│   ├── Patches/          # Harmony patches
│   ├── Plugin.cs         # Main plugin entry point
│   └── DnDUtil.csproj    # Project configuration
├── DnDUtil.Tests/        # Unit tests
└── AndrewMod.slnx        # Solution file
```

## Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork the repository** and create a feature branch
2. **Write tests** for new functionality
3. **Follow C# coding conventions** and existing code style
4. **Test thoroughly** in-game before submitting
5. **Update documentation** to reflect your changes
6. **Submit a pull request** with a clear description

### Code Style
- Use meaningful variable and method names
- Add comments for complex logic
- Keep methods focused and concise
- Follow existing patterns in the codebase

## Debugging

To debug the mod in-game:

1. Build in Debug mode: `dotnet build`
2. Copy the DLL to your BepInEx plugins folder
3. Enable BepInEx console logging in `BepInEx/config/BepInEx.cfg`:
   ```ini
   [Logging.Console]
   Enabled = true
   ```
4. Launch the game and check the BepInEx console for logs

## Resources

- [BepInEx Documentation](https://docs.bepinex.dev/)
- [HarmonyX Documentation](https://github.com/BepInEx/HarmonyX/wiki)
- [On-Together Modding Community](https://discord.gg/ontogether) *(if applicable)*

## License

See [LICENSE](LICENSE) file for details.

## Additional Notes

- For detailed development notes and setup instructions, see [project_notes.md](project_notes.md)
- For version history and changes, see [CHANGELOG.md](DnDUtil/CHANGELOG.md)
