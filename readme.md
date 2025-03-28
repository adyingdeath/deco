# Deco

Deco is a newly designed language specifically for creating Minecraft datapacks. It follows a progressive design philosophy, meaning that original Minecraft datapacks and mcfunction files are valid Deco code, allowing you to gradually incorporate Deco syntax to leverage its power.

Deco is currently under development. The current version supports tick, load, and function tags, as well as onPlaceBlock event binding.

## Table of Contents

- [Introduction](#deco)
- [Features](#features)
- [Installation](#installation)
- [Basic Usage](#basic-usage)
  - [Command Format](#basic-usage)
  - [Specify Output Directory](#basic-usage)
- [Registering as System Command](#registering-deco-as-a-system-command)
  - [Windows](#windows)
  - [macOS](#macos)
  - [linux](#linux)
- [Syntax Examples](#syntax-examples)
  - [Event Binding](#event-binding)
  - [Function Definition](#function-definition)

## Features

- Fully compatible with vanilla datapacks
- Support for tick, load, and function tags
- Block placement event binding
- Clean syntax for improved development efficiency

## Installation

1. Download the latest version of the `deco.jar` file from [here](#)
2. Place the file in your preferred directory

## Basic Usage

Basic command format:

```
java -jar deco.jar <your_datapack_folder>
```

Specify an output directory:

```
java -jar deco.jar -o <output_folder> <your_datapack_folder>
```

## Registering Deco as a System Command

**This is optional, you can jump directly to [Deco Syntax](#deco-syntax) for basic syntax.**

### Windows

1. Create a file named `deco.bat` with the following content:

```
@echo off
java -jar C:\path\to\deco.jar %*
```

2. Save this file to a directory in your system PATH (e.g., `C:\Windows`) or create a new directory and add it to PATH
3. Now you can use the `deco` command directly in the command prompt:

```
deco <your_datapack_folder>
```

#### Adding a Directory to PATH in Windows
- Right-click on "This PC" and select "Properties"
- Click on "Advanced system settings"
- Click the "Environment Variables" button
- Under "System variables", find and select the "Path" variable, then click "Edit"
- Click "New" and add the directory containing your `deco.bat` file
- Click "OK" to close all dialogs

### macOS

1. Create a file named `deco` with the following content:

```bash
#!/bin/bash
java -jar /path/to/deco.jar "$@"
```

2. Add execution permissions:

```
chmod +x /path/to/deco
```

3. Move the file to a directory in your PATH, such as `/usr/local/bin`:

```
sudo mv /path/to/deco /usr/local/bin/
```

4. Now you can use the `deco` command directly:

```
deco <your_datapack_folder>
```

### Linux

1. Create a file named `deco` with the following content:

```bash
#!/bin/bash
java -jar /path/to/deco.jar "$@"
```

2. Add execution permissions:

```
chmod +x /path/to/deco
```

3. Move the file to a directory in your PATH, such as `/usr/local/bin`:

```
sudo mv /path/to/deco /usr/local/bin/
```

4. Now you can use the `deco` command directly:

```
deco <your_datapack_folder>
```

# Deco Syntax

## Deco Project Structure

The file structure of a Deco project is the same as the original datapack structure, which means you don't need to change your entire project to take advantage of Deco's features.

To start using Deco, create a `.deco` file in the functions folder, like this:

```
// Here we use "example" as the datapack name
example/
├── pack.mcmeta
└── data/
    └── <namespace>/
        └── functions/
            └── <filename>.deco
```

## Example Code

Place the following example code in your `.deco` file:

```
@onPlaceBlock("minecraft:black_wool")
func onBlackWool {
    tellraw @a [{"selector":"@s"},{"text":" just placed a black wool."}]
}

@onPlaceBlock("minecraft:sand")
func onSand {
    tellraw @a [{"selector":"@s"},{"text":" just placed a sand."}]
}
```

Then, run:

```
deco -o /example_compiled ./example
```

Or if you haven't set up the command alias:

```
java -jar deco.jar -o /example_compiled ./example
```

You'll see a folder called `example_compiled` generated. Place it in Minecraft's datapack folder and enter the game. When you place black wool, you'll see a message in the chat box: "xxx just placed a black wool.", and similarly for sand.

The same ways for `@load`, `@tick` and `@tag()`:
```
@load
func xxx {
    xxx
}

@tick
func xxx {
    xxx
}

@tag("deco:core")
func xxx {
    xxx
}
```