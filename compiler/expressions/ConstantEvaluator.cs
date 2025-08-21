using System;

namespace Deco.Compiler.Expressions {
    /// <summary>
    /// Handles the evaluation of compile-time constant expressions by visiting the parse tree.
    /// It computes the value of an expression if it's composed entirely of literals and constant operations.
    /// Inherits from DecoBaseVisitor for standard ANTLR tree traversal.
    /// </summary>
    public class ConstantEvaluator : DecoBaseVisitor<ConstantOperand> {
        /// <summary>
        /// Evaluates an expression context to a constant operand.
        /// </summary>
        /// <param name="context">The expression context to evaluate.</param>
        /// <returns>A ConstantOperand with the result.</returns>
        /// <exception cref="InvalidOperationException">If the expression is not a compile-time constant.</exception>
        public ConstantOperand Evaluate(DecoParser.ExpressionContext context) {
            return Visit(context);
        }

        public override ConstantOperand VisitPrimary(DecoParser.PrimaryContext context) {
            if (context.NUMBER() != null) {
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
            if (context.expression() != null) {
                return Visit(context.expression());
            }

            throw new InvalidOperationException("Expression is not a compile-time constant. It may contain variables, function calls, or runtime conditions.");
        }

        public override ConstantOperand VisitUnary_expr(DecoParser.Unary_exprContext context) {
            if (context.primary() != null) {
                return Visit(context.primary());
            }

            string op = context.GetChild(0).GetText();
            var operand = Visit(context.unary_expr());

            if (op == "!") {
                if (operand.Type != "bool") {
                    throw new InvalidOperationException("Operator '!' can only be applied to booleans.");
                }
                bool val = operand.Value == "1";
                return new ConstantOperand(!val ? "1" : "0", "bool");
            } else if (op == "-") {
                if (operand.Type != "int") {
                    throw new InvalidOperationException("Unary operator '-' can only be applied to integers.");
                }
                int val = int.Parse(operand.Value);
                return new ConstantOperand((-val).ToString(), "int");
            }

            throw new InvalidOperationException($"Unsupported unary operator: {op}");
        }

        public override ConstantOperand VisitMul_expr(DecoParser.Mul_exprContext context) {
            if (context.unary_expr().Length == 1) {
                return Visit(context.unary_expr(0));
            }

            var left = Visit(context.unary_expr(0));

            for (int i = 1; i < context.unary_expr().Length; i++) {
                var right = Visit(context.unary_expr(i));
                var op = context.GetChild(i * 2 - 1).GetText();

                if (left.Type != "int" || right.Type != "int") {
                    throw new InvalidOperationException($"Operator '{op}' can only be applied to integers.");
                }

                int leftInt = int.Parse(left.Value);
                int rightInt = int.Parse(right.Value);
                int result = op switch {
                    "*" => leftInt * rightInt,
                    "/" => leftInt / rightInt,
                    _ => throw new InvalidOperationException($"Unsupported operator '{op}'")
                };
                left = new ConstantOperand(result.ToString(), "int");
            }
            return left;
        }

        public override ConstantOperand VisitAdd_expr(DecoParser.Add_exprContext context) {
            if (context.mul_expr().Length == 1) {
                return Visit(context.mul_expr(0));
            }

            var left = Visit(context.mul_expr(0));

            for (int i = 1; i < context.mul_expr().Length; i++) {
                var right = Visit(context.mul_expr(i));
                var op = context.GetChild(i * 2 - 1).GetText();

                if (op == "+") {
                    if (left.Type == "string" || right.Type == "string") {
                        string leftVal = left.Type == "string" ? left.Value.Trim('"') : left.Value;
                        string rightVal = right.Type == "string" ? right.Value.Trim('"') : right.Value;
                        left = new ConstantOperand($"\"{leftVal}{rightVal}\"", "string");
                        continue;
                    }
                }

                if (left.Type != "int" || right.Type != "int") {
                    throw new InvalidOperationException($"Operator '{op}' can only be applied to integers.");
                }

                int leftInt = int.Parse(left.Value);
                int rightInt = int.Parse(right.Value);
                int result = op switch {
                    "+" => leftInt + rightInt,
                    "-" => leftInt - rightInt,
                    _ => throw new InvalidOperationException($"Unsupported operator '{op}'")
                };
                left = new ConstantOperand(result.ToString(), "int");
            }
            return left;
        }

        public override ConstantOperand VisitRel_expr(DecoParser.Rel_exprContext context) {
            if (context.add_expr().Length == 1) {
                return Visit(context.add_expr(0));
            }

            var left = Visit(context.add_expr(0));
            var right = Visit(context.add_expr(1));
            var op = context.GetChild(1).GetText();

            if (left.Type != "int" || right.Type != "int") {
                throw new InvalidOperationException("Relational operators currently only support integers.");
            }

            int leftInt = int.Parse(left.Value);
            int rightInt = int.Parse(right.Value);
            bool result = op switch {
                ">" => leftInt > rightInt,
                "<" => leftInt < rightInt,
                ">=" => leftInt >= rightInt,
                "<=" => leftInt <= rightInt,
                _ => throw new InvalidOperationException($"Unsupported relational operator: {op}")
            };

            return new ConstantOperand(result ? "1" : "0", "bool");
        }

        public override ConstantOperand VisitEq_expr(DecoParser.Eq_exprContext context) {
            if (context.rel_expr().Length == 1) {
                return Visit(context.rel_expr(0));
            }

            var left = Visit(context.rel_expr(0));
            var right = Visit(context.rel_expr(1));
            var op = context.GetChild(1).GetText();

            if (left.Type != right.Type) {
                throw new InvalidOperationException($"Cannot compare values of different types: {left.Type} and {right.Type}.");
            }

            bool result;
            switch (left.Type) {
                case "int":
                case "bool":
                case "string":
                    result = left.Value == right.Value;
                    break;
                default:
                    throw new InvalidOperationException($"Equality operators do not support type '{left.Type}'.");
            }

            if (op == "!=") {
                result = !result;
            }

            return new ConstantOperand(result ? "1" : "0", "bool");
        }

        public override ConstantOperand VisitAnd_expr(DecoParser.And_exprContext context) {
            if (context.eq_expr().Length == 1) {
                return Visit(context.eq_expr(0));
            }

            var left = Visit(context.eq_expr(0));
            if (left.Type != "bool") throw new InvalidOperationException("Operator '&&' can only be applied to booleans.");

            if (left.Value == "0") return new ConstantOperand("0", "bool");

            for (int i = 1; i < context.eq_expr().Length; i++) {
                var right = Visit(context.eq_expr(i));
                if (right.Type != "bool") throw new InvalidOperationException("Operator '&&' can only be applied to booleans.");
                if (right.Value == "0") return new ConstantOperand("0", "bool");
            }

            return new ConstantOperand("1", "bool");
        }

        public override ConstantOperand VisitOr_expr(DecoParser.Or_exprContext context) {
            if (context.and_expr().Length == 1) {
                return Visit(context.and_expr(0));
            }

            var left = Visit(context.and_expr(0));
            if (left.Type != "bool") throw new InvalidOperationException("Operator '||' can only be applied to booleans.");

            if (left.Value == "1") return new ConstantOperand("1", "bool");

            for (int i = 1; i < context.and_expr().Length; i++) {
                var right = Visit(context.and_expr(i));
                if (right.Type != "bool") throw new InvalidOperationException("Operator '||' can only be applied to booleans.");
                if (right.Value == "1") return new ConstantOperand("1", "bool");
            }

            return new ConstantOperand("0", "bool");
        }

        protected override ConstantOperand DefaultResult {
            get { throw new InvalidOperationException("This part of the expression is not a compile-time constant."); }
        }

        protected override ConstantOperand AggregateResult(ConstantOperand aggregate, ConstantOperand nextResult) =>
            nextResult ?? aggregate;
    }
}