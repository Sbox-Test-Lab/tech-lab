# Copilot Instructions

## Project Guidelines
- When working in this s&box project (tech-lab), always reference https://sbox.game/llms.txt as documentation context for understanding s&box APIs, shaders, and patterns.
- Ensure that code inside block scopes (HEADER, FEATURES, MODES, COMMON, VS, PS) uses literal tab indentation. Code at column 0 inside a block can cause "Unknown text found" or "'{' or '}' not tabbed properly" compile errors.
- Prefer an ItemResource that references a prefab; behaviors live on the prefab and mark mutable fields with [BehaviorState] or [SaveState]. Use s&box CodeGenerator with a [BehaviorState] attribute to wrap property setters for item behaviors and mark items dirty for state capture. 
- Prefer an editor tool to create ItemResource assets and link prefabs to reduce errors. Additionally, prefer minimal item persistence: store prefab/resource path plus a small per-behavior property dictionary (only properties marked with [BehaviorState] or [SaveState]) rather than full GameObject JSON or heavy codegen. Ensure that behavior defaults are automatically treated as save-state for item persistence by using GameResource-based behavior resource extensions.

## Coding Style
- Follow Microsoft .NET capitalization conventions for all C# code: use PascalCase for types, methods, properties, and events; camelCase for parameters and local variables; and _camelCase for private fields. Refer to the guidelines here: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/capitalization-conventions.