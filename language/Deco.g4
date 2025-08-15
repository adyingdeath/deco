grammar Deco;

// --- Parser Rules ---

program: (function)* EOF;

modifier:
    '@' name=IDENTIFIER ('(' (expression (',' expression)*)? ')')?
    ;

function:
    (modifier)*
    type=IDENTIFIER name=IDENTIFIER '(' (arguments)? ')' '{'
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
    | (assignment ';')
    | (return ';')
    ;

expression:
    IDENTIFIER
    | STRING
    | NUMBER
    | functionCall
    | expression ('+' | '-' | '*' | '/') expression
    ;

variableDefinition:
    type=IDENTIFIER name=IDENTIFIER
    ;

functionCall:
    name=IDENTIFIER '(' (expression (',' expression)*)? ')'
    ;

assignment:
    IDENTIFIER '=' expression
    ;

return: 'return' expression ;

// --- Lexer Rules ---

COMMAND: '@' '`' ( '\\' . | ~[`\\] )* '`';

STRING: '"' ( '"' | ~["] )* '"' ;

IDENTIFIER: [a-zA-Z_] ( [a-zA-Z0-9_] )*;

NUMBER: [0-9]+;

WS: [ \t\r\n]+ -> skip;