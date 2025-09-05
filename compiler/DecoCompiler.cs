using Deco.Compiler.Data;
using Deco.Compiler.Expressions;
using Deco.Compiler.Library;
using Deco.Compiler.Library.Types;
using Deco.Compiler.Library.Functions;
using static Deco.Compiler.CompilerConstants;
using Deco.Compiler.Core;

namespace Deco.Compiler {
    /// <summary>
    /// Main Deco Compiler with full library system support.
    /// This replaces the old compiler with extensible type and function systems.
    /// </summary>
    public class DecoCompiler {
        private readonly DataPack _dataPack;
        private readonly LibraryRegistry _registry = new LibraryRegistry();
        private readonly List<IDecoLibrary> _loadedLibraries = new List<IDecoLibrary>();

        public LibraryRegistry Registry => _registry;

        public DecoCompiler(DataPack dataPack) {
            _dataPack = dataPack;
        }

        /// <summary>
        /// Initialize the library system by loading all built-in types and functions
        /// </summary>
        public void InitializeLibrarySystem() {
            LoadLibraries();
        }

        private void LoadLibraries() {
            // Load core library with built-in types and functions
            var coreLibrary = new CoreLibrary();
            coreLibrary.Register(_registry);
            _loadedLibraries.Add(coreLibrary);

            // [TODO]: Add support for loading external libraries from DLL files
            // This would involve scanning a directory and using reflection to load IDecoLibrary implementations
        }

        /// <summary>
        /// Get type from library registry
        /// </summary>
        public IDecoType GetLibraryType(string name) {
            return _registry.GetType(name);
        }

        /// <summary>
        /// Get function from library registry
        /// </summary>
        public IDecoFunction GetLibraryFunction(string name) {
            return _registry.GetFunction(name);
        }

        /// <summary>
        /// Get a variable instance with the specified type
        /// </summary>
        public Variable CreateVariable(string typeName, string storageName) {
            var type = GetLibraryType(typeName);
            if (type == null) {
                throw new ArgumentException($"Unknown type: {typeName}");
            }
            return new Variable(type, storageName);
        }

        /// <summary>
        /// Create a library codegen context for the current function
        /// </summary>
        public LibContext CreateLibraryContext(McFunction currentFunction, SymbolTable symbolTable) {
            return new LibContext(currentFunction, _dataPack, symbolTable);
        }

        public void GenerateCode() {
            foreach (var decoFunction in _dataPack.Functions.DecoFunctions.Values) {
                // Process each statement in the function body
                foreach (var statement in decoFunction.Context.block().statement()) {
                    ProcessStatement(statement, decoFunction);
                }
            }
        }

        private void ProcessStatement(DecoParser.StatementContext statement, DecoFunction decoFunction) {
            var expressionCompiler = new ExpressionCompiler(decoFunction, _dataPack, decoFunction.SymbolTable);

            if (statement.COMMAND() != null) {
                string rawCommand = statement.COMMAND().GetText();
                if (rawCommand.StartsWith("@`") && rawCommand.EndsWith('`')) {
                    string command = rawCommand[2..^1];
                    decoFunction.McFunction.Commands.Add(command);
                }
            } else if (statement.variableDefinition() != null) {
                var varDef = statement.variableDefinition();
                string varName = varDef.name.Text;
                string varTypeString = varDef.type.Text;

                string varType = "int"; // Default
                if (varTypeString == "int" || varTypeString == "float" || varTypeString == "string" || varTypeString == "bool") {
                    varType = varTypeString;
                } else {
                    Console.Error.WriteLine($"Warning: Unknown variable type '{varTypeString}' for variable '{varName}'. Defaulting to int.");
                }

                // Generate a unique storage name for the variable
                string storageName = _dataPack.Functions.ParameterIdCounter.ToString("x");
                _dataPack.Functions.ParameterIdCounter++;

                var newSymbol = new Symbol(varName, varType, storageName);
                if (!decoFunction.SymbolTable.Add(newSymbol)) {
                    Console.Error.WriteLine($"Error: Variable '{varName}' already defined in function '{decoFunction.Name}'.");
                }
                // Initialize with default value (0 for int/float, empty string for string)
                switch (varType) {
                    case "int":
                    case "bool":
                        decoFunction.McFunction.Commands.Add($"scoreboard players set {newSymbol.StorageName} {_dataPack.ID} 0");
                        break;
                    case "float":
                        decoFunction.McFunction.Commands.Add($"data modify storage {_dataPack.ID} {newSymbol.StorageName} set value 0.0f");
                        break;
                    case "string":
                        decoFunction.McFunction.Commands.Add($"data modify storage {_dataPack.ID} {newSymbol.StorageName} set value \"\"");
                        break;
                }
            } else if (statement.assignment() != null) {
                var assignment = statement.assignment();
                string varName = assignment.IDENTIFIER().GetText();
                var targetSymbol = decoFunction.SymbolTable.Get(varName);

                if (targetSymbol == null) {
                    Console.Error.WriteLine($"Error: Attempt to assign to undefined variable '{varName}' in function '{decoFunction.Name}'.");
                    return;
                }

                var evaluatedExpression = expressionCompiler.Evaluate(assignment.expression());

                // Assign the result of the expression to the variable
                switch (targetSymbol.Type) {
                    case "int":
                    case "bool":
                        if (targetSymbol.Type != evaluatedExpression.Type) {
                            Console.Error.WriteLine($"Error: Type mismatch for assignment to '{varName}'. Expected {targetSymbol.Type}, got {evaluatedExpression.Type}.");
                            return;
                        }
                        decoFunction.McFunction.Commands.Add($"scoreboard players operation {targetSymbol.StorageName} {_dataPack.ID} = {evaluatedExpression.StorageName} {_dataPack.ID}");
                        break;
                    case "float":
                    case "string":
                        if (targetSymbol.Type != evaluatedExpression.Type) {
                            Console.Error.WriteLine($"Error: Type mismatch for assignment to '{varName}'. Expected {targetSymbol.Type}, got {evaluatedExpression.Type}.");
                            return;
                        }
                        decoFunction.McFunction.Commands.Add($"data modify storage {_dataPack.ID} {targetSymbol.StorageName} set from storage {_dataPack.ID} {evaluatedExpression.StorageName}");
                        break;
                }
                targetSymbol.IsInitialized = true;
            } else if (statement.expression() != null) {
                // Evaluate the expression and discard the result.
                // This handles standalone function calls.
                expressionCompiler.Evaluate(statement.expression());
            } else if (statement.if_statement() != null) {
                ProcessIfStatement(statement.if_statement(), decoFunction);
            } else if (statement.while_statement() != null) {
                ProcessWhileStatement(statement.while_statement(), decoFunction);
            } else if (statement.return_statement() != null) {
                ProcessReturnStatement(statement.return_statement(), decoFunction);
            }
        }

        private void ProcessReturnStatement(DecoParser.Return_statementContext context, DecoFunction currentFunction) {
            var expressionCompiler = new ExpressionCompiler(currentFunction, _dataPack, currentFunction.SymbolTable);
            var signature = currentFunction.Signature;

            if (context.expression() != null) {
                if (signature.ReturnType.Name == "void") {
                    Console.Error.WriteLine($"Error: Function '{currentFunction.Name}' with void return type cannot return a value.");
                    return;
                }

                var returnValue = expressionCompiler.Evaluate(context.expression());

                if (returnValue.Type != signature.ReturnType.Name) {
                    Console.Error.WriteLine($"Error: Return type mismatch in function '{currentFunction.Name}'. Expected '{signature.ReturnType.Name}', got '{returnValue.Type}'.");
                    return;
                }

                // Store the return value in the global return location
                switch (returnValue.Type) {
                    case "int":
                    case "bool":
                        currentFunction.McFunction.Commands.Add($"scoreboard players operation {ReturnValueInt} {_dataPack.ID} = {returnValue.StorageName} {_dataPack.ID}");
                        break;
                    case "float":
                        currentFunction.McFunction.Commands.Add($"data modify storage {_dataPack.ID} {ReturnValueFloat} set from storage {_dataPack.ID} {returnValue.StorageName}");
                        break;
                    case "string":
                        currentFunction.McFunction.Commands.Add($"data modify storage {_dataPack.ID} {ReturnValueString} set from storage {_dataPack.ID} {returnValue.StorageName}");
                        break;
                }
            } else {
                if (signature.ReturnType.Name != "void") {
                    Console.Error.WriteLine($"Error: Function '{currentFunction.Name}' must return a value of type '{signature.ReturnType}'.");
                }
            }

            // Set the flag and return to stop execution in the current McFunction
            currentFunction.McFunction.Commands.Add($"scoreboard players set {ReturnFlagPlayer} {ReturnFlagObjective} 1");
            currentFunction.McFunction.Commands.Add("return 1");
        }

        private void AddReturnCheck(McFunction mcFunction) {
            mcFunction.Commands.Add($"execute if score {ReturnFlagPlayer} {ReturnFlagObjective} matches 1 run return 1");
        }

        private void ProcessIfStatement(DecoParser.If_statementContext context, DecoFunction currentFunction) {
            var parentSymbolTable = currentFunction.SymbolTable;
            var expressionCompiler = new ExpressionCompiler(currentFunction, _dataPack, parentSymbolTable);

            // 1. Evaluate condition
            var condition = expressionCompiler.Evaluate(context.expression());
            if (condition.Type != "bool") {
                Console.Error.WriteLine("Error: if statement condition must be a boolean expression.");
                return;
            }

            var ifBlock = context.block(0);
            var ifBodyStatements = ifBlock.statement();

            var elseIfNode = context.if_statement();
            var elseBlockNode = context.block().Length > 1 ? context.block(1) : null;

            bool hasElse = elseIfNode != null || elseBlockNode != null;

            // Optimization for single command with no else
            if (ifBodyStatements.Length == 1 && !hasElse && ifBodyStatements[0].COMMAND() != null) {
                var singleStatement = ifBodyStatements[0];
                string rawCommand = singleStatement.COMMAND().GetText();
                if (rawCommand.StartsWith("@`") && rawCommand.EndsWith('`')) {
                    string command = rawCommand[2..^1];
                    currentFunction.McFunction.Commands.Add($"execute if score {condition.StorageName} {_dataPack.ID} matches 1 run {command}");
                    return;
                }
            }

            var ifMcFunction = CreateFunctionForBlock(ifBodyStatements, currentFunction, parentSymbolTable);
            currentFunction.McFunction.Commands.Add($"execute if score {condition.StorageName} {_dataPack.ID} matches 1 run function {ifMcFunction.Location}");
            AddReturnCheck(currentFunction.McFunction); // Check if the if-block returned

            if (hasElse) {
                var randomCode = Util.GenerateRandomString(8);
                var elseMcFunction = new McFunction(new ResourceLocation(randomCode, _dataPack.MainNamespace));
                _dataPack.Functions.McFunctions.Add(elseMcFunction);

                var elseSymbolTable = new SymbolTable(parentSymbolTable);
                var elseDecoFunction = new DecoFunction($"{currentFunction.Name}_{randomCode}", currentFunction.Signature, elseMcFunction, currentFunction.Context, elseSymbolTable);

                if (elseIfNode != null) {
                    ProcessIfStatement(elseIfNode, elseDecoFunction);
                } else if (elseBlockNode != null) {
                    // elseBlockNode must be non-null
                    var elseBodyStatements = elseBlockNode.statement();
                    foreach (var statement in elseBodyStatements) {
                        ProcessStatement(statement, elseDecoFunction);
                    }
                }

                currentFunction.McFunction.Commands.Add($"execute if score {condition.StorageName} {_dataPack.ID} matches 0 run function {elseMcFunction.Location}");
                AddReturnCheck(currentFunction.McFunction); // Check if the else-block returned
            }
        }

        private void ProcessWhileStatement(DecoParser.While_statementContext context, DecoFunction currentFunction) {
            var parentSymbolTable = currentFunction.SymbolTable;

            // 1. Create two functions: one for the condition, one for the body.
            var conditionRandomCode = Util.GenerateRandomString(8);
            var conditionLocation = new ResourceLocation($"while_cond_{conditionRandomCode}", _dataPack.MainNamespace);
            var conditionMcFunction = new McFunction(conditionLocation);
            _dataPack.Functions.McFunctions.Add(conditionMcFunction);

            var bodyRandomCode = Util.GenerateRandomString(8);
            var bodyLocation = new ResourceLocation($"while_body_{bodyRandomCode}", _dataPack.MainNamespace);
            var bodyMcFunction = new McFunction(bodyLocation);
            _dataPack.Functions.McFunctions.Add(bodyMcFunction);

            // Create a DecoFunction for the condition part to use the ExpressionCompiler
            var conditionDecoFunction = new DecoFunction(
                $"{currentFunction.Name}_while_cond_{conditionRandomCode}",
                currentFunction.Signature,
                conditionMcFunction,
                currentFunction.Context,
                parentSymbolTable
            );
            var expressionCompiler = new ExpressionCompiler(conditionDecoFunction, _dataPack, parentSymbolTable);

            // 2. Compile the condition check.
            var condition = expressionCompiler.Evaluate(context.expression());
            if (condition.Type != "bool") {
                Console.Error.WriteLine("Error: while statement condition must be a boolean expression.");
                return;
            }
            conditionMcFunction.Commands.Add($"execute if score {condition.StorageName} {_dataPack.ID} matches 1 run function {bodyLocation}");
            AddReturnCheck(conditionMcFunction); // Check if the body returned

            // 3. Compile the loop body.
            var bodyStatements = context.block().statement();
            var bodySymbolTable = new SymbolTable(parentSymbolTable);
            var bodyDecoFunction = new DecoFunction(
                $"{currentFunction.Name}_while_body_{bodyRandomCode}",
                currentFunction.Signature,
                bodyMcFunction,
                currentFunction.Context,
                bodySymbolTable
            );

            foreach (var statement in bodyStatements) {
                ProcessStatement(statement, bodyDecoFunction);
            }

            // 4. Add the loop-back command to the body function.
            bodyMcFunction.Commands.Add($"function {conditionLocation}");

            // 5. Add the initial call to the condition check function in the current function.
            currentFunction.McFunction.Commands.Add($"function {conditionLocation}");
            AddReturnCheck(currentFunction.McFunction); // Check if the whole loop returned
        }

        private McFunction CreateFunctionForBlock(DecoParser.StatementContext[] statements, DecoFunction parentFunction, SymbolTable parentSymbolTable) {
            var randomCode = Util.GenerateRandomString(8);
            var mcFunction = new McFunction(new ResourceLocation(randomCode, _dataPack.MainNamespace));
            _dataPack.Functions.McFunctions.Add(mcFunction);

            var blockSymbolTable = new SymbolTable(parentSymbolTable);
            var blockDecoFunction = new DecoFunction(
                $"{parentFunction.Name}_{randomCode}",
                parentFunction.Signature,
                mcFunction,
                parentFunction.Context,
                blockSymbolTable
            );

            foreach (var statement in statements) {
                ProcessStatement(statement, blockDecoFunction);
            }

            return mcFunction;
        }
    }
}
