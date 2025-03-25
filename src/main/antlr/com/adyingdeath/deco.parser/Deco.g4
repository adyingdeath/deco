grammar Deco;

// Parser rules
program
    : functionDeclaration* EOF
    ;

functionDeclaration
    : decorator* 'func' IDENTIFIER block
    ;

decorator
    : '@' IDENTIFIER
    ;

block
    : '{' '}'
    ;

// Lexer rules
IDENTIFIER
    : [a-zA-Z_][a-zA-Z0-9_]*
    ;

WS
    : [ \t\r\n]+ -> skip
    ;

COMMENT
    : '//' ~[\r\n]* -> skip
    ;

MULTILINE_COMMENT
    : '/*' .*? '*/' -> skip
    ; 