# Contributing to Nedev.ImageSharp

Thanks for your interest in contributing! This project follows standard GitHub workflows.

## Getting Started

1. Fork the repo and clone it locally.
2. Create a feature branch:
   ```powershell
   git checkout -b feature/your-idea
   ```
3. Make your changes.
4. Add tests where possible (see `tests/Nedev.ImageSharp.Tests`).
5. Run the tests:
   ```powershell
   dotnet test -c Release
   ```
6. Commit and push your changes.

## Pull Request Guidelines

- Keep changes focused and atomic.
- Provide a clear PR description describing what changed and why.
- Ensure all tests pass and the project builds cleanly.

## Code Style

- The repository uses `.editorconfig` for formatting and code-style guidelines.
- Nullable reference types are enabled and should be respected.

## Reporting Issues

If you find a bug or have a feature request, please open an issue with a minimal reproduction case.
