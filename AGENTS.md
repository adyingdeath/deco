# AGENTS.md - Deco Compiler

This file provides guidelines for agentic coding assistants working on the Deco compiler project.

## Build/Test Commands

```bash
# Build the project
dotnet build

# Run the main program (test mode)
dotnet run

# Run with arguments (not fully implemented yet)
dotnet run -- arg1 arg2

# Run tests (NUnit - Debug configuration only)
dotnet test

# Run a specific test
dotnet test --filter "TestName=YourTestMethodName"

# Run tests in verbose mode
dotnet test --logger "console;verbosity=detailed"
```

## Project Overview

Deco is a custom language compiler that compiles to Minecraft datapacks. The compiler pipeline:
1. **Preprocessing** - DecoPreprocessor handles macro/imports
2. **Parsing** - ANTLR4 generates lexer/parser from Deco.g4
3. **AST Building** - AstBuilder creates abstract syntax tree
4. **Semantic Analysis** - Multiple passes:
   - GlobalSymbolTableBuilder
   - ScopedSymbolTableBuilder  
   - IdentifierUsageChecker
   - TypeResolver
5. **AST Transforms** - ForLoopToWhilePass, ConstantFoldingPass
6. **IR Generation** - IRBuilder creates intermediate representation
7. **Datapack Generation** - DatapackBuilder outputs Minecraft datapack

## Code Style Guidelines

### Namespaces
- Use `Deco.Compiler.*` for most compiler code
- Root namespace for utilities: `Deco`
- Compiler sub-namespaces: `Ast`, `IR`, `Pack`, `Diagnostics`, `Types`, `Lib`

### Formatting (.editorconfig)
- No new line before open braces: `csharp_new_line_before_open_brace = none`
- No new line before else/catch/finally
- Use `target-type` new expression patterns where applicable
- Use primary constructor syntax (available in .NET 8)

### Imports
- Implicit usings are enabled in project file
- Group imports by namespace, no `using static` unless necessary
- Standard ordering: System, third-party, project namespaces

### Types and Naming
- Enable nullable reference types project-wide
- Use `IType` interface for all type representations
- Primitive types: `IntType`, `FloatType`, `BoolType`, `StringType`, `VoidType`
- AST nodes end with `Node`: `ProgramNode`, `FunctionNode`, etc.
- IR classes end with `Instruction` or are prefixed: `IrFunction`, `IrProgram`
- Use record types where appropriate for immutable data
- Interface names prefix with `I`: `IAstVisitor<T>`, `IPlugin`

### AST Node Pattern
All AST nodes inherit from `AstNode`:
- Store line/column for error reporting
- Implement `Accept<T>(IAstVisitor<T> visitor)` for visitor pattern
- Implement `GetChildren()` for parent/child navigation
- Use `SetChildrenParent()` after creating nodes
- Support immutable updates via `With()` methods

### Error Handling
- Use `ErrorReporter` in `CompilationContext` to report errors
- Create error messages in `Compiler.Diagnostics.Errors` namespace
- Errors should include phase, line, column, and message
- Check `HasErrors` before proceeding with compilation
- Do not throw exceptions for semantic errors; report and continue

### Symbol Table
- `Scope` class manages symbols with hierarchical lookup
- Build global symbols first, then scoped symbols
- Resolve types from `UnresolvedType` to actual types during type checking
- Use `Scope` references on AST nodes for lookup

### IR Pattern
- IR uses list-based instructions: `List<IRInstruction>`
- Operands: `ConstantOperand`, `VariableOperand`, `FunctionOperand`
- Instructions: `MoveInstruction`, `CallInstruction`, etc.
- Use `ExpressionEvaluator` for expression evaluation during IR building

### File Structure
```
compiler/
  ast/              # AST node definitions
  ast/passes/       # AST transformation passes
  ir/               # IR instruction definitions
  ir/passes/        # IR optimization passes (future)
  pack/             # Datapack output generation
  diagnostics/      # Error reporting
  diagnostics/errors/ # Error message definitions
  types/            # Type system
  lib/              # Plugin/library system
language/           # ANTLR grammar and generated files
test/               # Test .deco files
```

### Testing
- Test files are `.deco` files in the `test/` directory
- Tests are run by modifying `Program.cs` RunTest() method
- No automated test framework exists yet (test framework packages included but not used)
- When adding tests, update the test .deco files and modify RunTest() as needed

### Constants
- Use `Compiler.Constants` for magic strings/numbers
- Datapack constants: Minecraft 1.21 target version
- Function/variable naming uses `Base36Counter` for unique IDs

### Visitor Pattern
- AST uses visitor pattern: `IAstVisitor<T>`
- IR uses visitor pattern: `IRVisitor<T>`
- Passes inherit from `AstTransformVisitor` for AST transformations
- Always call `SetChildrenParent()` after creating new nodes in passes

### Memory and Performance
- Use `List<T>` for collections, prefer array for fixed-size
- Avoid LINQ in hot paths, use loops instead
- `Base36Counter` is optimized for ID generation (no allocations)
- Reuse objects where possible in passes

### Adding New Features
1. Add grammar rule to `Deco.g4` and regenerate ANTLR files
2. Add AST node class in `compiler/ast/`
3. Add visitor methods in `AstBuilder`
4. Add semantic analysis in passes if needed
5. Add IR generation in `IRBuilder`
6. Add datapack generation in `DatapackBuilder` if needed

### Regenerating ANTLR Files
```bash
cd language
java -jar antlr-4.13.2-complete.jar Deco.g4 -visitor
```
This generates lexer, parser, and visitor files.

## Project-Specific Notes

- Built-in standard library loaded via `PluginLoader.LoadStandardLibrary()`
- External plugins can be loaded via `PluginLoader.LoadPlugin()`
- `print()` function currently only accepts `int` type
- Datapack exports to configurable path (see `RunTest()` in Program.cs)
- IR debug output written to `irs.txt` in project root
