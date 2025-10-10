This project is still under development.

# Deco

A language designed to help you develop Minecraft datapacks faster.

*(Note: This project is currently under active development.)*

## Give it a try

We are excited about Deco, but please note that the core components are still evolving. Consequently, a dedicated cli has not yet been implemented.

To experiment with Deco, you will need the .NET SDK installed.

1.  Open `Program.cs`.
2.  Locate the `RunTest` method. Inside this method, you will find a block containing an example Deco code string.
3.  Replace the example code with your own Deco code.
4.  Update the output path specified in `DatapackExporter.Export` below the code block.
5.  Run the project from your terminal:
    ```bash
    dotnet run
    ```

The generated datapack will be placed in the specified output directory.

### Important Notes

It is highly recommended to test the output in Minecraft 1.21, as Deco does not yet support multi-version compilation.

The built-in `print` function currently only accepts a single argument of the `int` type.