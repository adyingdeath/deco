grammar Deco;

// --- Parser Rules ---

program: (function)* EOF;

modifier:
    '@' name=IDENTIFIER ('(' (expression (',' expression)*)? ')')?
    ;

function:
    (modifier)*
    type=IDENTIFIER name=IDENTIFIER '(' (arguments)? ')' block;

arguments:
    argument (',' argument)*;

argument:
    type=IDENTIFIER name=IDENTIFIER;

statement:
    (COMMAND ';')
    | (expression ';')
    | (variableDefinition ';')
    | (assignment ';')
    | (return_statement ';')
    | block
    | if_statement
    | while_statement
    | for_statement
    ;

return_statement:
    'return' expression?
    ;

if_statement:
    'if' '(' expression ')' block ( 'else' (if_statement | block) )?
    ;

while_statement:
    'while' '(' expression ')' block
    ;

for_inner_expression:
    expression
    | variableDefinition
    | assignment
    ;

for_statement:
    'for' '(' init=for_inner_expression? ';' cond=expression ';' iter=for_inner_expression? ')' block
    ;

block:
    '{' statement* '}'
    ;

expression:
    or_expr
    ;

or_expr:
    and_expr ('||' and_expr)*
    ;

and_expr:
    eq_expr ('&&' eq_expr)*
    ;

eq_expr:
    rel_expr (('==' | '!=') rel_expr)*
    ;

rel_expr:
    add_expr (('>=' | '<=' | '>' | '<') add_expr)*
    ;

add_expr:
    mul_expr (('+' | '-') mul_expr)*
    ;

mul_expr:
    unary_expr (('*' | '/') unary_expr)*
    ;

unary_expr:
    op='!' right=unary_expr
    | op='-' right=unary_expr
    | primary
    ;

primary:
    NUMBER
    | STRING
    | TRUE
    | FALSE
    | IDENTIFIER
    | functionCall
    | '(' expression ')'
    ;

variableDefinition:
    type=IDENTIFIER name=IDENTIFIER ('=' expression)?
    ;

functionCall:
    name=IDENTIFIER '(' (expression (',' expression)*)? ')'
    ;

assignment:
    IDENTIFIER '=' expression
    ;

// --- Lexer Rules ---

COMMAND: '@' '`' ( '\\' . | ~[`\\] )* '`';

STRING: '"' ( '\\"' | ~["] )* '"' ;

TRUE: 'true';
FALSE: 'false';

IDENTIFIER: [a-zA-Z_] ( [a-zA-Z0-9_] )*;

NUMBER: [0-9]+ ('.' [0-9]+)?;

WS: [ \t\r\n]+ -> skip;
