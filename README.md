# Nedev.ImageSharp

This repository is a fork of **SixLabors.ImageSharp v2.1.13**, the last version released under the **Apache License 2.0**. The source code has been copied and the project has been renamed to **Nedev.ImageSharp**.

## Building

From the repository root:

```powershell
cd d:\Project\Nedev.ImageSharp
dotnet build Nedev.ImageSharp.sln
```

## Tests

Run all tests:

```powershell
dotnet test -c Release
```

## CI

This repository includes a GitHub Actions workflow (`.github/workflows/dotnet.yml`) that builds and tests on every push/PR.

## Usage

The API surface is identical to ImageSharp v2.x. Example:

```csharp
using Nedev.ImageSharp;
using Nedev.ImageSharp.Processing;

using var image = Image.Load("input.png");
image.Mutate(x => x.Resize(800, 600));
image.Save("output.png");
```

## License

This fork is based on ImageSharp v2.1.13 licensed under **Apache License 2.0**.

