namespace Deco.Compiler.Lib;

public class Context {
    public List<string> CommandList = [];
    public List<string> AdvancementList = [];
    public void Command(string command) {
        CommandList.Add(command);
    }
    public void Advancement(string advancement) {
        AdvancementList.Add(advancement);
    }
}