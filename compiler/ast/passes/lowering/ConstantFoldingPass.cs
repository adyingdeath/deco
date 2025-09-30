using Deco.Compiler.Ast;
using Deco.Types;

namespace Deco.Compiler.Ast.Passes.Lowering;

/// <summary>
/// A transformation pass that performs constant folding.
/// It evaluates constant expressions at compile time to simplify the AST.
/// Supports arithmetic, comparison, logical operations, and string concatenation.
/// </summary>
public class ConstantFoldingPass : AstTransformVisitor {
    // [TODO] We still need to handle this type of constant folding:
    // (a op b) op c
    // and
    // (a op b) op (c op d)
    // and
    // a op (b op c)
    public override AstNode VisitBinaryOp(BinaryOpNode node) {
        // First, transform the children. This allows nested constant folding
        var newLeft = (ExpressionNode)Visit(node.Left);
        var newRight = (ExpressionNode)Visit(node.Right);

        // Handle different binary operations based on operand types
        if (newLeft is LiteralNode leftLit && newRight is LiteralNode rightLit) {
            return FoldBinaryOp(node, leftLit, rightLit);
        }

        // If we cannot fold the expression, return a new BinaryOpNode with the
        // potentially transformed children.
        return new BinaryOpNode(newLeft, node.Operator, newRight, node.Line, node.Column);
    }

    private AstNode FoldBinaryOp(BinaryOpNode original, LiteralNode left, LiteralNode right) {
        switch (original.Operator) {
            // Arithmetic operations (require both operands to be numbers)
            case BinaryOperator.Add when TypeUtils.IsNumeric(left.Type) && TypeUtils.IsNumeric(right.Type):
            case BinaryOperator.Subtract when TypeUtils.IsNumeric(left.Type) && TypeUtils.IsNumeric(right.Type):
            case BinaryOperator.Multiply when TypeUtils.IsNumeric(left.Type) && TypeUtils.IsNumeric(right.Type):
            case BinaryOperator.Divide when TypeUtils.IsNumeric(left.Type) && TypeUtils.IsNumeric(right.Type):
                return FoldArithmeticOp(original, left, right);

            // String concatenation (ADD applied to strings)
            case BinaryOperator.Add when left.Type.Equals(TypeUtils.StringType) && right.Type.Equals(TypeUtils.StringType):
                return FoldStringConcatOp(original, left, right);

            // Comparison operations (require same types or compatible types)
            case BinaryOperator.Equal:
            case BinaryOperator.NotEqual:
            case BinaryOperator.LessThan:
            case BinaryOperator.LessThanOrEqual:
            case BinaryOperator.GreaterThan:
            case BinaryOperator.GreaterThanOrEqual:
                return FoldComparisonOp(original, left, right);

            // Logical operations (require boolean operands)
            case BinaryOperator.LogicalAnd when left.Type.Equals(TypeUtils.BoolType) && right.Type.Equals(TypeUtils.BoolType):
            case BinaryOperator.LogicalOr when left.Type.Equals(TypeUtils.BoolType) && right.Type.Equals(TypeUtils.BoolType):
                return FoldLogicalOp(original, left, right);

            // Other combinations cannot be folded
            default:
                var newNode = (BinaryOpNode)new BinaryOpNode(left, original.Operator, right, original.Line, original.Column).CloneContext(original);
                newNode.Type = original.Type;
                return newNode;
        }
    }

    private static AstNode FoldArithmeticOp(BinaryOpNode original, LiteralNode left, LiteralNode right) {
        if (
            !double.TryParse(left.Value, out var leftVal) ||
            !double.TryParse(right.Value, out var rightVal)
        ) {
            return new BinaryOpNode(left, original.Operator, right, original.Line, original.Column);
        }

        double? result = original.Operator switch {
            BinaryOperator.Add => leftVal + rightVal,
            BinaryOperator.Subtract => leftVal - rightVal,
            BinaryOperator.Multiply => leftVal * rightVal,
            BinaryOperator.Divide => rightVal != 0 ? leftVal / rightVal : null,
            _ => throw new InvalidOperationException("Unexpected operator in arithmetic folding")
        };

        if (result.HasValue) {
            // Handle integer results specially (avoid unnecessary decimal places)
            bool isInteger = Math.Floor(result.Value) == result.Value;
            string resultStr = isInteger
                ? ((int)result.Value).ToString()
                : result.Value.ToString();
            return new LiteralNode(
                isInteger ? TypeUtils.IntType : TypeUtils.FloatType, resultStr,
                original.Line, original.Column
            );
        }

        return new BinaryOpNode(left, original.Operator, right, original.Line, original.Column);
    }

    private static LiteralNode FoldStringConcatOp(BinaryOpNode original, LiteralNode left, LiteralNode right) {
        string leftStr = left.Value.Trim('"');
        string rightStr = right.Value.Trim('"');
        string concatenated = $"\"{leftStr}{rightStr}\"";
        return new LiteralNode(TypeUtils.StringType, concatenated, original.Line, original.Column);
    }

    private static AstNode FoldComparisonOp(BinaryOpNode original, LiteralNode left, LiteralNode right) {
        // Only fold comparisons between compatible types
        if (!left.Type.Equals(right.Type)) {
            return new BinaryOpNode(left, original.Operator, right, original.Line, original.Column);
        }

        bool result;

        if (TypeUtils.IsNumeric(left.Type)) {
            if (!double.TryParse(left.Value, out var leftNum) ||
                !double.TryParse(right.Value, out var rightNum)) {
                return new BinaryOpNode(left, original.Operator, right, original.Line, original.Column);
            }

            result = original.Operator switch {
                BinaryOperator.Equal => leftNum == rightNum,
                BinaryOperator.NotEqual => leftNum != rightNum,
                BinaryOperator.LessThan => leftNum < rightNum,
                BinaryOperator.LessThanOrEqual => leftNum <= rightNum,
                BinaryOperator.GreaterThan => leftNum > rightNum,
                BinaryOperator.GreaterThanOrEqual => leftNum >= rightNum,
                _ => false
            };
        } else if (left.Type.Equals(TypeUtils.BoolType)) {
            if (!bool.TryParse(left.Value, out var leftBool) ||
                !bool.TryParse(right.Value, out var rightBool)) {
                return new BinaryOpNode(left, original.Operator, right, original.Line, original.Column);
            }

            result = original.Operator switch {
                BinaryOperator.Equal => leftBool == rightBool,
                BinaryOperator.NotEqual => leftBool != rightBool,
                _ => false // Other comparisons don't make sense for booleans
            };
        } else if (left.Type.Equals(TypeUtils.StringType)) {
            string leftStr = left.Value.Trim('"');
            string rightStr = right.Value.Trim('"');

            result = original.Operator switch {
                BinaryOperator.Equal => leftStr == rightStr,
                BinaryOperator.NotEqual => leftStr != rightStr,
                _ => false // Other comparisons not supported for strings
            };
        } else {
            return new BinaryOpNode(left, original.Operator, right, original.Line, original.Column);
        }

        return new LiteralNode(TypeUtils.BoolType, result.ToString().ToLower(), original.Line, original.Column);
    }

    private static AstNode FoldLogicalOp(BinaryOpNode original, LiteralNode left, LiteralNode right) {
        if (!bool.TryParse(left.Value, out var leftBool) ||
            !bool.TryParse(right.Value, out var rightBool)) {
            return new BinaryOpNode(left, original.Operator, right, original.Line, original.Column);
        }

        bool result = original.Operator switch {
            BinaryOperator.LogicalAnd => leftBool && rightBool,
            BinaryOperator.LogicalOr => leftBool || rightBool,
            _ => throw new InvalidOperationException("Unexpected operator in logical folding")
        };

        return new LiteralNode(TypeUtils.BoolType, result.ToString().ToLower(), original.Line, original.Column);
    }

    public override AstNode VisitUnaryOp(UnaryOpNode node) {
        // First, transform the operand. This allows nested constant folding
        var newOperand = (ExpressionNode)Visit(node.Operand);

        // Check if the operand is a constant literal
        if (newOperand is LiteralNode operand) {
            return FoldUnaryOp(node, operand);
        }

        // If we cannot fold the expression, return a new UnaryOpNode with the
        // potentially transformed operand.
        return new UnaryOpNode(node.Operator, newOperand, node.Line, node.Column);
    }

    private static AstNode FoldUnaryOp(UnaryOpNode original, LiteralNode operand) {
        switch (original.Operator) {
            case UnaryOperator.Negate when TypeUtils.IsNumeric(operand.Type):
                if (double.TryParse(operand.Value, out var num)) {
                    double negated = -num;
                    bool isInteger = Math.Floor(negated) == negated;
                    string resultStr = isInteger
                        ? ((int)negated).ToString()
                        : negated.ToString();
                    return new LiteralNode(
                        isInteger ? TypeUtils.IntType : TypeUtils.FloatType, resultStr,
                        original.Line, original.Column
                    );
                }
                break;

            case UnaryOperator.LogicalNot when operand.Type.Equals(TypeUtils.BoolType):
                if (bool.TryParse(operand.Value, out var boolVal)) {
                    bool negated = !boolVal;
                    return new LiteralNode(TypeUtils.BoolType, negated.ToString().ToLower(), original.Line, original.Column);
                }
                break;
        }

        // Cannot fold this unary operation
        return new UnaryOpNode(original.Operator, operand, original.Line, original.Column);
    }
}
