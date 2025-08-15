grammar Deco;

// --- Parser Rules ---

program: (function)* EOF;

function:
    'func' functionName=IDENTIFIER '(' (arguments)? ')' (':' returnType=IDENTIFIER)? '{'
        (statement)*
    '}';

arguments:
    argument (',' argument)*;

argument:
    IDENTIFIER IDENTIFIER;

statement:
    COMMAND
    | variableDefinition
    | return
    | IDENTIFIER '(' expression ')' ';'
    ;

variableDefinition:
    IDENTIFIER IDENTIFIER ';'
    ;

return: 'return' expression ';' ;

expression:
    NUMBER
    | IDENTIFIER
    ;

// --- Lexer Rules ---

COMMAND: '"""' .*? '"""';

IDENTIFIER: [a-zA-Z_] ( [a-zA-Z0-9_] )*;

NUMBER: [0-9]+;

WS: [ \t\r\n]+ -> skip;