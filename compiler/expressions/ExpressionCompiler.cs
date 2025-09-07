using Deco.Compiler.Data;
using static Deco.Compiler.CompilerConstants;
using Deco.Compiler.Library;
using Deco.Compiler.Library.Types;

namespace Deco.Compiler.Expressions {
    public class ExpressionCompiler : DecoBaseVisitor<Operand> {
        private readonly DecoFunction _function;
        private readonly McFunction _mcFunction;
        private readonly DataPack _dataPack;
        private readonly SymbolTable _symbolTable;
        private readonly LibraryRegistry _registry;
        private static int _tempCounter = 0;

        public ExpressionCompiler(DecoFunction function, DataPack dataPack, SymbolTable symbolTable, LibraryRegistry registry) {
            _function = function;
            _mcFunction = function.McFunction;
            _dataPack = dataPack;
            _symbolTable = symbolTable;
            _registry = registry;
        }

        public string GetNextTemp() => $"tmp_expr_{_tempCounter++}";

        // Main entry point
        public Symbol Evaluate(DecoParser.ExpressionContext context) {
            var resultOperand = Visit(context);

            if (resultOperand is SymbolOperand symbolOperand) {
                return symbolOperand.Symbol;
            }

            if (resultOperand is ConstantOperand constantOperand) {
                if (constantOperand.Type == "void") { // Discard void results
                    return new Symbol("void", _registry.GetType("void"), "void");
                }

                var tempSymbol = new Symbol(GetNextTemp(), _registry.GetType(constantOperand.Type), GetNextTemp());
                AssignConstantToSymbol(tempSymbol, constantOperand);
                return tempSymbol;
            }

            throw new Exception("Expression did not resolve to a symbol or constant.");
        }

        private void AssignConstantToSymbol(Symbol symbol, ConstantOperand constant) {
            var libContext = new LibContext(_mcFunction, _dataPack, _symbolTable);
            var targetVariable = new Variable(symbol.Type, symbol.StorageName);

            // For constants, we need to initialize the variable first, then set the constant value
            symbol.Type.Initialize(libContext, targetVariable);

            // For simple constants, directly set the value
            if (symbol.Type.Equals(CoreTypeSingleton.Int) || symbol.Type.Equals(CoreTypeSingleton.Bool)) {
                _mcFunction.Commands.Add($"scoreboard players set {symbol.StorageName} {_dataPack.ID} {constant.Value}");
            } else if (symbol.Type.Equals(CoreTypeSingleton.Float) || symbol.Type.Equals(CoreTypeSingleton.String)) {
                _mcFunction.Commands.Add($"data modify storage {_dataPack.ID} {symbol.StorageName} set value {constant.Value}");
            }
        }



        public override Operand VisitPrimary(DecoParser.PrimaryContext context) {
            if (context.NUMBER() != null) {
                // [TODO] For now, assume int. Need to handle float.
                return new ConstantOperand(context.NUMBER().GetText(), "int");
            }
            if (context.STRING() != null) {
                return new ConstantOperand(context.STRING().GetText(), "string");
            }
            if (context.TRUE() != null) {
                return new ConstantOperand("1", "bool");
            }
            if (context.FALSE() != null) {
                return new ConstantOperand("0", "bool");
            }
            /* if (context.CONDITION() != null) {
                string rawCondition = context.CONDITION().GetText();
                // remove c` and ` and unescape
                string condition = rawCondition[2..^1].Replace("\\`", "`");

                var resultStorageName = GetNextTemp();
                var resultSymbol = new Symbol(resultStorageName, _registry.GetType("bool"), resultStorageName);

                // Initialize to false
                _mcFunction.Commands.Add($"scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 0");
                // Set to true if condition passes
                _mcFunction.Commands.Add($"execute if {condition} run scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 1");

                return new SymbolOperand(resultSymbol);
            } */
            if (context.IDENTIFIER() != null) {
                var symbol = _symbolTable.Get(context.IDENTIFIER().GetText());
                if (symbol == null) {
                    Console.Error.WriteLine($"Unknown identifier: {context.IDENTIFIER().GetText()}");
                }
                return new SymbolOperand(symbol);
            }
            if (context.expression() != null) {
                return Visit(context.expression());
            }
            if (context.functionCall() != null) {
                return VisitFunctionCall(context.functionCall());
            }
            return base.VisitPrimary(context);
        }

        public override Operand VisitFunctionCall(DecoParser.FunctionCallContext context) {
            string functionNameToCall = context.name.Text;

            // Handle built-in library functions first
            var libraryFunction = _registry.GetFunction(functionNameToCall);
            if (libraryFunction != null) {
                // Evaluate arguments first
                var arguments = new List<Operand>();
                foreach (var argExpr in context.expression()) {
                    var evaluatedArg = Evaluate(argExpr);
                    // [TODO] Should specifically handle arguments that are a single identifier.
                    arguments.Add(new SymbolOperand(evaluatedArg));
                }

                // Create LibContext
                var libContext = new Deco.Compiler.Library.Types.LibContext(_mcFunction, _dataPack, _symbolTable);

                // Execute the function
                var resultOperand = libraryFunction.Execute(libContext, arguments);
                return resultOperand;
            }

            // 1. Look up user-defined function
            if (!_dataPack.Functions.DecoFunctions.TryGetValue(functionNameToCall, out var calledDecoFunction)) {
                Console.Error.WriteLine($"Error: Attempt to call undefined function '{functionNameToCall}'.");
                return new ConstantOperand("0", "void"); // Return dummy
            }
            var signature = calledDecoFunction.Signature;
            var locationToCall = calledDecoFunction.McFunction.Location;
            var argumentsExpressions = context.expression();

            // 2. Check argument count
            if (argumentsExpressions.Length != signature.Parameters.Count) {
                Console.Error.WriteLine($"Error: Function '{functionNameToCall}' expects {signature.Parameters.Count} arguments, but received {argumentsExpressions.Length}.");
                return new ConstantOperand("0", "void");
            }

            // --- Function Call Implementation ---

            // Stage 1: Save the current values of the callee's parameters to the real stack.
            foreach (var parameter in signature.Parameters) {
                var storageName = parameter.StorageName;
                if (parameter.Type.Equals(CoreTypeSingleton.Int) || parameter.Type.Equals(CoreTypeSingleton.Bool)) {
                    _mcFunction.Commands.Add($"execute store result storage {_dataPack.ID} tmp_arg int 1 run scoreboard players get {storageName} {_dataPack.ID}");
                    _mcFunction.Commands.Add($"data modify storage {_dataPack.ID} stack_int prepend from storage {_dataPack.ID} tmp_arg");
                } else if (parameter.Type.Equals(CoreTypeSingleton.Float)) {
                    _mcFunction.Commands.Add($"data modify storage {_dataPack.ID} stack_float prepend from storage {_dataPack.ID} {storageName}");
                } else if (parameter.Type.Equals(CoreTypeSingleton.String)) {
                    _mcFunction.Commands.Add($"data modify storage {_dataPack.ID} stack_string prepend from storage {_dataPack.ID} {storageName}");
                }
            }

            // Stage 2: Evaluate each argument and assign it directly to the parameter's storage.
            // Evaluate arguments and store them in temporary storages to prevent aliasing issues
            var tempStorages = new List<string>();
            var validParameters = new List<ParameterInfo>();
            for (int i = 0; i < argumentsExpressions.Length; i++) {
                var argument = argumentsExpressions[i];
                var parameter = signature.Parameters[i];
                var evaluatedArg = Evaluate(argument);

                if (!parameter.Type.Equals(evaluatedArg.Type)) {
                    Console.Error.WriteLine($"Error: Type mismatch for parameter '{parameter.Name}' in function '{functionNameToCall}'. Expected {parameter.Type}, got {evaluatedArg.Type}.");
                    continue; // Skip assignment on type error
                }

                var tempStorage = GetNextTemp();
                tempStorages.Add(tempStorage);
                validParameters.Add(parameter);

                // Copy evaluated argument to temporary storage
                if (parameter.Type.Equals(CoreTypeSingleton.Int) || parameter.Type.Equals(CoreTypeSingleton.Bool)) {
                    _mcFunction.Commands.Add($"scoreboard players operation {tempStorage} {_dataPack.ID} = {evaluatedArg.StorageName} {_dataPack.ID}");
                } else if (parameter.Type.Equals(CoreTypeSingleton.Float) || parameter.Type.Equals(CoreTypeSingleton.String)) {
                    _mcFunction.Commands.Add($"data modify storage {_dataPack.ID} {tempStorage} set from storage {_dataPack.ID} {evaluatedArg.StorageName}");
                }
            }

            // Assign temporary storages to parameters
            for (int i = 0; i < tempStorages.Count; i++) {
                var parameter = validParameters[i];
                var temp = tempStorages[i];

                if (parameter.Type.Equals(CoreTypeSingleton.Int) || parameter.Type.Equals(CoreTypeSingleton.Bool)) {
                    _mcFunction.Commands.Add($"scoreboard players operation {parameter.StorageName} {_dataPack.ID} = {temp} {_dataPack.ID}");
                } else if (parameter.Type.Equals(CoreTypeSingleton.Float) || parameter.Type.Equals(CoreTypeSingleton.String)) {
                    _mcFunction.Commands.Add($"data modify storage {_dataPack.ID} {parameter.StorageName} set from storage {_dataPack.ID} {temp}");
                }
            }

            // Stage 3: Call the function
            _mcFunction.Commands.Add($"function {locationToCall}");

            // Stage 4: Handle the return value
            Symbol resultSymbol = null;
            if (signature.ReturnType.Name != "void") {
                var resultStorageName = GetNextTemp();
                resultSymbol = new Symbol(resultStorageName, signature.ReturnType, resultStorageName);

                // Copy the global return value into our new temporary symbol
                switch (signature.ReturnType.Name) {
                    case "int":
                    case "bool":
                        _mcFunction.Commands.Add($"scoreboard players operation {resultSymbol.StorageName} {_dataPack.ID} = {ReturnValueInt} {_dataPack.ID}");
                        break;
                    case "float":
                        _mcFunction.Commands.Add($"data modify storage {_dataPack.ID} {resultSymbol.StorageName} set from storage {_dataPack.ID} {ReturnValueFloat}");
                        break;
                    case "string":
                        _mcFunction.Commands.Add($"data modify storage {_dataPack.ID} {resultSymbol.StorageName} set from storage {_dataPack.ID} {ReturnValueString}");
                        break;
                }
            }

            // Stage 5: Restore the context. Pop values from the real stack back into parameter storage.
            for (int i = signature.Parameters.Count - 1; i >= 0; i--) {
                var parameter = signature.Parameters[i];
                var storageName = parameter.StorageName;
                if (parameter.Type.Equals(CoreTypeSingleton.Int) || parameter.Type.Equals(CoreTypeSingleton.Bool)) {
                    _mcFunction.Commands.Add($"execute store result score {storageName} {_dataPack.ID} run data get storage {_dataPack.ID} stack_int[0] 1");
                    _mcFunction.Commands.Add($"data remove storage {_dataPack.ID} stack_int[0]");
                } else if (parameter.Type.Equals(CoreTypeSingleton.Float)) {
                    _mcFunction.Commands.Add($"data modify storage {_dataPack.ID} {storageName} set from storage {_dataPack.ID} stack_float[0]");
                    _mcFunction.Commands.Add($"data remove storage {_dataPack.ID} stack_float[0]");
                } else if (parameter.Type.Equals(CoreTypeSingleton.String)) {
                    _mcFunction.Commands.Add($"data modify storage {_dataPack.ID} {storageName} set from storage {_dataPack.ID} stack_string[0]");
                    _mcFunction.Commands.Add($"data remove storage {_dataPack.ID} stack_string[0]");
                }
            }

            // Stage 6: Reset the return flag, so we consume the return event.
            _mcFunction.Commands.Add($"scoreboard players set {ReturnFlagPlayer} {ReturnFlagObjective} 0");

            return resultSymbol != null ? new SymbolOperand(resultSymbol) : new ConstantOperand("0", "void");
        }

        public override Operand VisitOr_expr(DecoParser.Or_exprContext context) {
            if (context.and_expr().Length == 1) {
                return Visit(context.and_expr(0));
            }

            var left = Visit(context.and_expr(0));

            for (int i = 1; i < context.and_expr().Length; i++) {
                var right = Visit(context.and_expr(i));

                if (GetOperandType(left) != "bool" || GetOperandType(right) != "bool") {
                    Console.Error.WriteLine("Error: Operator '||' can only be applied to booleans.");
                    return new ConstantOperand("0", "bool");
                }

                var resultStorageName = GetNextTemp();
                var resultSymbol = new Symbol(resultStorageName, _registry.GetType("bool"), resultStorageName);
                var leftName = GetOperandStorageName(left, GetNextTemp());
                var rightName = GetOperandStorageName(right, GetNextTemp());

                // Set the result to false(0) first, then if any of left and right operand is true, set the result to true(1).
                _mcFunction.Commands.Add($"scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 0");
                _mcFunction.Commands.Add($"execute if score {leftName} {_dataPack.ID} matches 1 run scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 1");
                _mcFunction.Commands.Add($"execute if score {rightName} {_dataPack.ID} matches 1 run scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 1");

                left = new SymbolOperand(resultSymbol);
            }

            return left;
        }

        public override Operand VisitAnd_expr(DecoParser.And_exprContext context) {
            if (context.eq_expr().Length == 1) {
                return Visit(context.eq_expr(0));
            }

            var left = Visit(context.eq_expr(0));

            for (int i = 1; i < context.eq_expr().Length; i++) {
                var right = Visit(context.eq_expr(i));

                if (GetOperandType(left) != "bool" || GetOperandType(right) != "bool") {
                    Console.Error.WriteLine("Error: Operator '&&' can only be applied to booleans.");
                    return new ConstantOperand("0", "bool");
                }

                left = PerformBooleanArithmetic(left, right, "*");
            }

            return left;
        }

        public override Operand VisitEq_expr(DecoParser.Eq_exprContext context) {
            if (context.rel_expr().Length == 1) {
                return Visit(context.rel_expr(0));
            }

            var left = Visit(context.rel_expr(0));
            var right = Visit(context.rel_expr(1));
            var op = context.GetChild(1).GetText();

            var leftType = GetOperandType(left);
            var rightType = GetOperandType(right);

            if (leftType != rightType || (leftType != "int" && leftType != "bool")) {
                Console.Error.WriteLine("Error: Equality operators currently only support matching integer or boolean types.");
                return new ConstantOperand("0", "bool");
            }

            var resultStorageName = GetNextTemp();
            var resultSymbol = new Symbol(resultStorageName, _registry.GetType("bool"), resultStorageName);
            var leftName = GetOperandStorageName(left, GetNextTemp());
            var rightName = GetOperandStorageName(right, GetNextTemp());

            if (op == "==") {
                _mcFunction.Commands.Add($"scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 0");
                _mcFunction.Commands.Add($"execute if score {leftName} {_dataPack.ID} = {rightName} {_dataPack.ID} run scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 1");
            } else {
                // !=
                _mcFunction.Commands.Add($"scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 0");
                _mcFunction.Commands.Add($"execute unless score {leftName} {_dataPack.ID} = {rightName} {_dataPack.ID} run scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 1");
            }

            return new SymbolOperand(resultSymbol);
        }

        public override Operand VisitRel_expr(DecoParser.Rel_exprContext context) {
            if (context.add_expr().Length == 1) {
                return Visit(context.add_expr(0));
            }

            var left = Visit(context.add_expr(0));
            var right = Visit(context.add_expr(1));
            var op = context.GetChild(1).GetText();

            if (GetOperandType(left) != "int" || GetOperandType(right) != "int") {
                Console.Error.WriteLine("Error: Relational operators currently only support integers.");
                return new ConstantOperand("0", "bool");
            }

            var resultStorageName = GetNextTemp();
            var resultSymbol = new Symbol(resultStorageName, _registry.GetType("bool"), resultStorageName);
            var leftName = GetOperandStorageName(left, GetNextTemp());
            var rightName = GetOperandStorageName(right, GetNextTemp());

            string condition;
            bool negate = false;
            switch (op) {
                case ">": condition = $"> {rightName} {_dataPack.ID}"; break;
                case "<": condition = $"< {rightName} {_dataPack.ID}"; break;
                case ">=": condition = $"< {rightName} {_dataPack.ID}"; negate = true; break;
                case "<=": condition = $"> {rightName} {_dataPack.ID}"; negate = true; break;
                default: throw new Exception($"Unsupported relational operator: {op}");
            }

            if (negate) {
                _mcFunction.Commands.Add($"scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 1");
                _mcFunction.Commands.Add($"execute if score {leftName} {_dataPack.ID} {condition} run scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 0");
            } else {
                _mcFunction.Commands.Add($"scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 0");
                _mcFunction.Commands.Add($"execute if score {leftName} {_dataPack.ID} {condition} run scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 1");
            }

            return new SymbolOperand(resultSymbol);
        }

        public override Operand VisitAdd_expr(DecoParser.Add_exprContext context) {
            if (context.mul_expr().Length == 1) {
                return Visit(context.mul_expr(0));
            }

            var left = Visit(context.mul_expr(0));

            for (int i = 1; i < context.mul_expr().Length; i++) {
                var right = Visit(context.mul_expr(i));
                var op = context.GetChild(i * 2 - 1).GetText(); // '+' or '-'

                left = PerformArithmetic(left, right, op);
            }

            return left;
        }

        public override Operand VisitMul_expr(DecoParser.Mul_exprContext context) {
            if (context.unary_expr().Length == 1) {
                return Visit(context.unary_expr(0));
            }

            var left = Visit(context.unary_expr(0));

            for (int i = 1; i < context.unary_expr().Length; i++) {
                var right = Visit(context.unary_expr(i));
                var op = context.GetChild(i * 2 - 1).GetText(); // '*' or '/'

                left = PerformArithmetic(left, right, op);
            }

            return left;
        }

        public override Operand VisitUnary_expr(DecoParser.Unary_exprContext context) {
            // Base case: `primary`
            if (context.primary() != null) {
                return Visit(context.primary());
            }

            // It's a unary operation. Get the operator token.
            string op = context.GetChild(0).GetText();
            var expr = context.unary_expr();

            if (op == "!") {
                // LOGICAL NOT

                // Optimization for !c`...`
                /* if (conditionNode != null) {
                    string rawCondition = conditionNode.GetText();
                    string condition = rawCondition[2..^1].Replace("\\`", "`");

                    var resultStorageName = GetNextTemp();
                    var resultSymbol = new Symbol(resultStorageName, _registry.GetType("bool"), resultStorageName);

                    _mcFunction.Commands.Add($"scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 0");
                    _mcFunction.Commands.Add($"execute unless {condition} run scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 1");

                    return new SymbolOperand(resultSymbol);
                } */

                // Fallback for other boolean expressions
                var operand = Visit(expr);
                if (GetOperandType(operand) != "bool") {
                    Console.Error.WriteLine("Error: Operator '!' can only be applied to booleans.");
                    return new ConstantOperand("0", "bool");
                }

                var resultStorageName2 = GetNextTemp();
                var resultSymbol2 = new Symbol(resultStorageName2, _registry.GetType("bool"), resultStorageName2);
                var operandName = GetOperandStorageName(operand, GetNextTemp());

                // Generic negation: result = 1 - operand
                _mcFunction.Commands.Add($"scoreboard players set {resultSymbol2.StorageName} {_dataPack.ID} 1");
                _mcFunction.Commands.Add($"scoreboard players operation {resultSymbol2.StorageName} {_dataPack.ID} -= {operandName} {_dataPack.ID}");

                return new SymbolOperand(resultSymbol2);
            } else if (op == "-") {
                // UNARY MINUS
                var operand = Visit(expr);

                // Constant folding for literals
                if (operand is ConstantOperand constOp) {
                    if (constOp.Type == "int") {
                        if (int.TryParse(constOp.Value, out int val)) {
                            return new ConstantOperand((-val).ToString(), "int");
                        }
                    }
                    // Note: No constant folding for float yet.
                }

                if (GetOperandType(operand) != "int") {
                    Console.Error.WriteLine("Error: Unary operator '-' can only be applied to integers.");
                    return new ConstantOperand("0", "int");
                }

                var resultStorageName = GetNextTemp();
                var resultSymbol = new Symbol(resultStorageName, _registry.GetType("int"), resultStorageName);
                var operandName = GetOperandStorageName(operand, GetNextTemp());

                // result = 0 - operand
                _mcFunction.Commands.Add($"scoreboard players set {resultSymbol.StorageName} {_dataPack.ID} 0");
                _mcFunction.Commands.Add($"scoreboard players operation {resultSymbol.StorageName} {_dataPack.ID} -= {operandName} {_dataPack.ID}");

                return new SymbolOperand(resultSymbol);
            }

            // Should not be reached
            throw new InvalidOperationException($"Unsupported unary operator: {op}");
        }

        private SymbolOperand PerformArithmetic(Operand left, Operand right, string operation) {
            // For now, only int is supported
            var nextTemp = GetNextTemp();
            var resultSymbol = new Symbol(nextTemp, _registry.GetType("int"), nextTemp);

            string leftName = GetOperandStorageName(left, GetNextTemp());
            string rightName = GetOperandStorageName(right, GetNextTemp());

            _mcFunction.Commands.Add($"scoreboard players operation {resultSymbol.StorageName} {_dataPack.ID} = {leftName} {_dataPack.ID}");
            _mcFunction.Commands.Add($"scoreboard players operation {resultSymbol.StorageName} {_dataPack.ID} {operation}= {rightName} {_dataPack.ID}");

            return new SymbolOperand(resultSymbol);
        }

        private SymbolOperand PerformBooleanArithmetic(Operand left, Operand right, string operation) {
            var nextTemp = GetNextTemp();
            var resultSymbol = new Symbol(nextTemp, _registry.GetType("bool"), nextTemp);

            string leftName = GetOperandStorageName(left, GetNextTemp());
            string rightName = GetOperandStorageName(right, GetNextTemp());

            _mcFunction.Commands.Add($"scoreboard players operation {resultSymbol.StorageName} {_dataPack.ID} = {leftName} {_dataPack.ID}");
            _mcFunction.Commands.Add($"scoreboard players operation {resultSymbol.StorageName} {_dataPack.ID} {operation}= {rightName} {_dataPack.ID}");

            return new SymbolOperand(resultSymbol);
        }

        private string GetOperandStorageName(Operand operand, string tempStorageName) {
            if (operand is SymbolOperand symbolOp) {
                return symbolOp.Symbol.StorageName;
            } else if (operand is ConstantOperand constOp) {
                if (constOp.Type != "int" && constOp.Type != "bool") {
                    Console.Error.WriteLine($"Deco only supports operations on INT or BOOL types currently.");
                } else {
                    _mcFunction.Commands.Add($"scoreboard players set {tempStorageName} {_dataPack.ID} {constOp.Value}");
                    return tempStorageName;
                }
            }
            throw new NotSupportedException("Unsupported operand type");
        }

        private string GetOperandType(Operand operand) {
            if (operand is SymbolOperand symbolOp) {
                return symbolOp.Symbol.Type.Name;
            }
            if (operand is ConstantOperand constOp) {
                return constOp.Type;
            }
            throw new NotSupportedException("Unsupported operand type");
        }
    }
}
