using Deco.Compiler.Data;
using System;
using System.Collections.Generic;

namespace Deco.Compiler.Expressions {
    public class ExpressionCompiler : DecoBaseVisitor<Operand> {
        private readonly DecoFunction _function;
        private readonly McFunction _mcFunction;
        private readonly DataPack _dataPack;
        private readonly SymbolTable _symbolTable;
        private int _tempCounter = 0;

        public ExpressionCompiler(DecoFunction function, DataPack dataPack, SymbolTable symbolTable) {
            _function = function;
            _mcFunction = function.McFunction;
            _dataPack = dataPack;
            _symbolTable = symbolTable;
        }

        private string GetNextTemp() => $"tmp_expr_{_tempCounter++}";

        // Main entry point
        public Symbol Evaluate(DecoParser.ExpressionContext context) {
            var resultOperand = Visit(context);

            if (resultOperand is SymbolOperand symbolOperand) {
                return symbolOperand.Symbol;
            }

            if (resultOperand is ConstantOperand constantOperand) {
                var tempSymbol = new Symbol(GetNextTemp(), constantOperand.Type, GetNextTemp());
                AssignConstantToSymbol(tempSymbol, constantOperand);
                return tempSymbol;
            }

            throw new Exception("Expression did not resolve to a symbol or constant.");
        }

        private void AssignConstantToSymbol(Symbol symbol, ConstantOperand constant) {
            switch (symbol.Type) {
                case SymbolType.Int:
                    _mcFunction.Commands.Add($"scoreboard players set {symbol.StorageName} {_dataPack.ID} {constant.Value}");
                    break;
                case SymbolType.Float:
                case SymbolType.String:
                    _mcFunction.Commands.Add($"data modify storage {_dataPack.ID} {symbol.StorageName} set value {constant.Value}");
                    break;
            }
        }

        public override Operand VisitPrimary(DecoParser.PrimaryContext context) {
            if (context.NUMBER() != null) {
                // [TODO] For now, assume int. Need to handle float.
                return new ConstantOperand(context.NUMBER().GetText(), SymbolType.Int);
            }
            if (context.STRING() != null) {
                return new ConstantOperand(context.STRING().GetText(), SymbolType.String);
            }
            if (context.IDENTIFIER() != null) {
                var symbol = _symbolTable.Get(context.IDENTIFIER().GetText());
                if (symbol == null) {
                    throw new Exception($"Unknown identifier: {context.IDENTIFIER().GetText()}");
                }
                return new SymbolOperand(symbol);
            }
            if (context.expression() != null) {
                return Visit(context.expression());
            }
            // [TODO] functionCall
            return base.VisitPrimary(context);
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

        private SymbolOperand PerformArithmetic(Operand left, Operand right, string operation) {
            // For now, only int is supported
            var nextTemp = GetNextTemp();
            var resultSymbol = new Symbol(nextTemp, SymbolType.Int, nextTemp);

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
                if (constOp.Type != SymbolType.Int) {
                    Console.Error.WriteLine($"Deco only supports operations on INT type currently.");
                } else {
                    // Assume int
                    _mcFunction.Commands.Add($"scoreboard players set {tempStorageName} {_dataPack.ID} {constOp.Value}");
                    return tempStorageName;
                }
            }
            throw new NotSupportedException("Unsupported operand type");
        }
    }
}
