# IniParse

A complete INI parser

## Features

### .NET Standard

This library is implemented in **.NET standard 2.0**.
No native code is used and no operating system specific components are in use.
This library should run everywhere, where .NET runs too.

Note: You normally have to match the versions of the framework.
To use a .NET standard library, your application should use .NET 4.6.1 or Core 2.0.

The framework version can easily be changed.
Care has been taken to not use deprecated components,
so the version can be moved forwards without any conflicts.

### Stream Support

This component can read and write data to any stream.
No file system is necessary.

### Comment handling

This implementation retains comments from sections and settings.
You can also add/edit/delete the comments.

### Custom comment character specifier

Comments in INI files almost always start with `;` but you can change this to any other character.
`#` is sometimes used too.

### Editing capabilities

You can add/edit/delete settings and sections

### Case sensitivity toggle

You can enable case sensitive mode for sections and settings to make editing easier.

### Whitespace filter toggle

Sections and setting names can be stripped of leading and trailing whitespace.
This is useful if the original INI file has indented settings

### Sorting ability

The INI file section and settings can be sorted ascending or descending for easier manual editing.
Comments are ignored when comparing values for sorting but are moved with the section/setting.

### Supports null-section

Some INI files have a null-section.
This happens if the INI file contains settings before the first section header.
This implementation fully supports this type of INI file.

Note: This is not the same as a section with an empty name.
Check the test.ini in the Test project for an example.

### Supports duplicate settings

Duplicate setting names are not uncommon in INI files.
This implementation supports them and provides the ability to edit each individual duplicate.

### Combines duplicate sections

When loading an INI file, it will combine multiple identical sections into one.
The sections don't need to be placed consecutively in the INI file.
Comments are retained and combined too.

### Invalid line handler

Invalid lines throw an exception by default,
but you can set the parser to either ignore them or convert them into comments.

### Async

Loading and writing INI files are implemented using asynchronous methods.
Synchronous loading is supported using the constructor.

### Purely managed

No unmanaged code is needed and no platform specific components are used.
You don't need to dispose the component either.

## Settings

Various settings can be modified to change how the component interacts with the INI file.

### IniFile.WhitespaceHandling

This decides how whitespace is treated **during loading** of the file.

This setting has no effect when editing or writing the INI file.
This is always done by keeping whitespace as-is.

Default: keep whitespace as-is

### IniFile.CaseHandling

Handles upper/lowercase handling when loading the file and when editing the file.
Section names and setting names can individually be chosen to be case insensitive.

This setting will not convert the actual names in the INI file,
it merely compares strings in a case insensitive manner if enabled.

This setting also exists as an individual setting in each `IniSection` class.
This allows you to set case insensitive mode of settings different from the global file setting.

Please note that using `CaseSensitivity.CaseInsensitiveSection` value in an individual section has no effect.
This value only works on the `IniFile` itself.

Default: Case sensitive mode

### IniFile.InvalidLineHandling

Defines how invalid lines are treated.
The component can either throw an exception,
silently skip invalid lines,
or convert them into comments.

Note that converting them into comments will prefix them with the chosen comment character.

Default: Throw an exception

### IniFile.CommentChar

Defines the character that indicates a comment.
The default is usually correct, but some INI files require this to be `#`.

This has an effect on loading as well as writing.
It can be changed at any time to convert comments from one system into another.

Default: `;`

## Usage

The component can be used in many ways.

Default settings are as follows:

- Case sensitive sections and settings
- Preserve whitespace in sections and settings
- Throw Exception on invalid lines
- Comment character: `;`

### Creating a new file

A new file can be created by using the empty constructor: `IniFile IF = new IniFile();`

### Opening an existing file

Simplest way is `IniFile IF = new IniFile("Test.INI");` to load an existing INI file synchronously.

Loading an INI file asynchronously can be done using the static
`IniFile.FromFile(string)` or `IniFile.FromFile(Stream)` methods.

This will load an INI file using the default settings.

### Opening a file with custom settings

The example below shows how to change all settings before loading a file.
You just need to uncomment the appropriate (a)sync loading line.

	var IF = new IniParse.IniFile();
	IF.WhitespaceHandling = IniParse.WhitespaceMode.TrimNames | IniParse.WhitespaceMode.TrimSections;
	IF.CaseHandling = IniParse.CaseSensitivity.CaseInsensitiveSection;
	IF.InvalidLineHandling = IniParse.InvalidLineMode.Skip;
	//Async:
	// await IF.Load("Test.INI");
	//Sync:
	// IF.Load("Test.INI").Wait();

### Accessing a file

Accessing settings and sections can be done in a simple and in a complex mode.
Simple mode is suitable for most needs,
complex mode is for when you want to perform more uncommon tasks like renaming settings
or modifying comments.

Both modes can be used simultaneously.

#### Simple mode

	//Open the file
	IniFile IF = new IniFile("Test.INI");
	//Add a setting
	IF["Section"]["Setting"] = SomeValue;
	//Read a setting
	string Value = IF["Section"]["Setting"];

Properties of using simple mode:

- Overwrites the setting if it already exists (only the first occurence if multiple exist)
- Creates the setting if it doesn't exists
- Creates the section if it doesn't exists
- Deletes the setting if the supplied value is `null` (setting is kept if the value is an empty string)
- Deletes the section if no settings remain
- Trying to read a non-existing setting/section returns `null`

This means that the simple mode can essentially be used to process most ini files.

#### Complex mode

Complex mode can be used if you want more in depth access to the ini file.
Use complex mode for these cases:

- Accessing and modifying comments
- Accessing and modifying multiple settings that have the same name
- Reordering sections

<!--This comment is here to fix a github rendering error -->

	//Open the file
	IniFile IF = new IniFile("Test.INI");
	//Add a new setting
	IniSection IS = IF.AddSection("NewSection");
	IS.Settings["Setting"] = SomeValue;
	//Read a setting
	string Value = IS["Setting"].Value;
	//Remove a section
	IF.RemoveSection("SomeSectionName");

Add/Insert of sections can be done either by the section name or a section object itself.
If you use a name, a section will be created and returned (as seen in the example above).

### Reference types

`IniFile`, `IniSection`, `IniSetting` are all reference types.

This means that if you add a section to two different files,
changes made to that section will propagate into both files.

Use the `.Clone()` method if you want to create an independent copy of a setting or section.

