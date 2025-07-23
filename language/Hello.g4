// 语法文件的名字必须和文件名一致
grammar Hello;

// --- 语法分析器规则 (Parser Rules) ---
// 我们的语言程序入口点是一个 'greet' 语句
// greet 规则: 必须以 'hello' 关键字开头，后面跟着一个 ID
greet : 'hello' ID;

// --- 词法分析器规则 (Lexer Rules) ---
// 定义 ID 是一个或多个字母
ID : [a-zA-Z]+ ;

// 定义 WS (Whitespace) 是空格、制表符、换行符等，并告诉分析器跳过它们
WS : [ \t\r\n]+ -> skip;