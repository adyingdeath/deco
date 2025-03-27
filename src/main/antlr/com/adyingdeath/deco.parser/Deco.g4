grammar Deco;

// Parser rules
program
    : statement* EOF
    ;

statement
    : MC_COMMAND
    | COMMENT
    | BLOCK_COMMENT
    | function
    | blockStatement
    ;

function
    : function_decorator* FUNC name=IDENTIFIER blockStatement
    ;

function_decorator
    : '@' name=IDENTIFIER
    ;

blockStatement
    : '{' statement* '}'
    ;

// Lexical rules
// Commands

// Minecraft commands - match at the beginning of a line, consume the entire line
MC_COMMAND
    : {getCharPositionInLine() == 0}?
      [ \t]*
      (
        'advancement' | 'attribute' | 'ban' | 'ban-ip' | 'banlist' | 'bossbar'
        | 'clear' | 'clone' | 'damage' | 'data' | 'datapack' | 'debug'
        | 'defaultgamemode' | 'deop' | 'difficulty' | 'effect' | 'enchant'
        | 'execute' | 'experience' | 'fill' | 'fillbiome' | 'forceload'
        | 'function' | 'gamemode' | 'gamerule' | 'give' | 'help' | 'item'
        | 'jfr' | 'kick' | 'kill' | 'list' | 'locate' | 'loot' | 'me' | 'msg'
        | 'op' | 'pardon' | 'pardon-ip' | 'particle' | 'perf' | 'place'
        | 'playsound' | 'publish' | 'random' | 'recipe' | 'reload' | 'return'
        | 'ride' | 'rotate' | 'save-all' | 'save-off' | 'save-on' | 'say'
        | 'schedule' | 'scoreboard' | 'seed' | 'setblock' | 'setidletimeout'
        | 'setworldspawn' | 'spawnpoint' | 'spectate' | 'spreadplayers' | 'stop'
        | 'stopsound' | 'summon' | 'tag' | 'team' | 'teammsg' | 'teleport' | 'tell'
        | 'tellraw' | 'tick' | 'time' | 'title' | 'tm' | 'tp' | 'transfer' | 'trigger'
        | 'w' | 'warden_spawn_tracker' | 'weather' | 'whitelist' | 'worldborder' | 'xp'
      )
      // 匹配命令后面的所有内容直到行尾
      (~[\r\n])*
      [ \t]*
      (NL | EOF)
    ;

// Function
FUNC
    : 'func'
    ;

IDENTIFIER
    : [a-zA-Z][a-zA-Z0-9_\-]*
    ;

// Comment
COMMENT
    : ('//' | '#') ~[\r\n]* -> channel(HIDDEN)
    ;

BLOCK_COMMENT
    : '/*' .*? '*/' -> channel(HIDDEN)
    ;

NL
    : '\r'? '\n' -> skip
    ;

// Ignore whitespace
WS
    : [ \t]+ -> skip
    ;