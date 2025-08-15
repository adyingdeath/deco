grammar Deco;

// --- Parser Rules ---

program: (function)* EOF;

function:
    'func' name=IDENTIFIER '(' (arguments)? ')' (':' type=IDENTIFIER)? '{'
        (statement)*
    '}';

arguments:
    argument (',' argument)*;

argument:
    type=IDENTIFIER name=IDENTIFIER;

statement:
    (COMMAND ';')
    | (expression ';')
    | (variableDefinition ';')
    | (return ';')
    ;

expression:
    NUMBER
    | IDENTIFIER
    | functionCall
    ;

variableDefinition:
    type=IDENTIFIER name=IDENTIFIER
    ;

functionCall:
    name=IDENTIFIER '(' (expression (',' expression)*)? ')'
    ;

return: 'return' expression ;

// --- Lexer Rules ---

COMMAND: '"""' .*? '"""';

IDENTIFIER: [a-zA-Z_] ( [a-zA-Z0-9_] )*;

NUMBER: [0-9]+;

WS: [ \t\r\n]+ -> skip;