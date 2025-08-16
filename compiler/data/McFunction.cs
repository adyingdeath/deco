using System.Collections.Generic;

namespace Deco.Compiler.Data
{
    /// <summary>
    /// Represents a single Minecraft function.
    /// </summary>
    public class McFunction
    {
        public ResourceLocation Location { get; }
        public List<string> Commands { get; } = new List<string>();

        public McFunction(ResourceLocation location)
        {
            Location = location;
        }

        /// <summary>
        /// Prepend commands to command list.
        /// </summary>
        /// <param name="commands">Commands to prepend.</param>
        public void PrependCommands(string[] commands)
        {
            Commands.InsertRange(0, commands);
        }

        /// <summary>
        /// append commands to command list.
        /// </summary>
        /// <param name="commands">Commands to append.</param>
        public void AppendCommands(string[] commands)
        {
            Commands.AddRange(commands);
        }
    }
}
