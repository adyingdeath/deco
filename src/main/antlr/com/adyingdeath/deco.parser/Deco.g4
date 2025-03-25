grammar Deco;

// Parser rules
program
    : (functionDeclaration | mcfuncStatement)* EOF
    ;

functionDeclaration
    : decorator* 'Func' IDENTIFIER block
    ;

decorator
    : '@' IDENTIFIER
    ;

block
    : '{' mcfuncStatement* '}'
    ;

// ================================
// Minecraft commands
// ================================
// Parser rules
mcfuncStatement
    : (BEDROCK_COMMAND | JAVA_COMMAND) .*? NEWLINE
    ;

// Lexer rules
BEDROCK_COMMAND
    : ('replaceitem' | 'testfor' | 'testforblock' | 'testforblocks'
    | 'toggledownfall' | 'clear' | 'clone' | 'damage' | 'deop' | 'difficulty'
    | 'effect' | 'enchant' | 'execute' | 'fill' | 'function' | 'gamemode'
    | 'gamerule' | 'give' | 'help' | 'kick' | 'kill' | 'list' | 'locate'
    | 'loot' | 'me' | 'msg' | 'op' | 'particle' | 'place' | 'playsound' | 'recipe'
    | 'reload' | 'ride' | 'say' | 'schedule' | 'scoreboard' | 'setblock'
    | 'setworldspawn' | 'spawnpoint' | 'spreadplayers' | 'stop' | 'stopsound'
    | 'summon' | 'tag' | 'teleport' | 'tell' | 'tellraw' | 'time' | 'title'
    | 'tp' | 'transfer' | 'w' | 'weather' | 'whitelist' | 'xp' | 'ability'
    | 'aimassist' | 'alwaysday' | 'camera' | 'camerashake' | 'changesetting'
    | 'clearspawnpoint' | 'connect' | 'daylock' | 'dedicatedwsserver' | 'dialogue'
    | 'event' | 'fog' | 'gametest' | 'gametips' | 'hud' | 'immutableworld'
    | 'inputpermission' | 'mobevent' | 'music' | 'ops' | 'permission' | 'playanimation'
    | 'reloadconfig' | 'save' | 'script' | 'scriptevent' | 'set_movement_authority'
    | 'setmaxplayers' | 'structure' | 'tickingarea' | 'titleraw' | 'wb' | 'worldbuilder'
    | 'wsserver') ' '
    ;

JAVA_COMMAND
    : ('advancement' | 'attribute' | 'ban' | 'ban-ip' | 'banlist' | 'bossbar'
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
    | 'w' | 'warden_spawn_tracker' | 'weather' | 'whitelist' | 'worldborder' | 'xp') ' '
    ;

IDENTIFIER
    : [a-zA-Z_][a-zA-Z0-9_]*
    ;

NEWLINE
    : [\r\n]+
    ;

WS
    : [ \t]+ -> skip
    ;

COMMENT
    : '//' ~[\r\n]* -> skip
    ;

MULTILINE_COMMENT
    : '/*' .*? '*/' -> skip
    ; 