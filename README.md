# ExportsToC++

**ExportsToC++** is a Windows GUI tool that parses `dumpbin.exe` output and converts a DLL's exported function list into C++ proxy code. This is useful for creating proxy DLLs to intercept API calls.

## Requirements

- Windows
- .NET Framework 2.0
- `dumpbin.exe` (included with Visual Studio) on your `PATH`

## Usage

1. Open a DLL or executable in the tool (or pass it as a command-line argument)
2. The tool runs `dumpbin.exe` and displays the exported functions
3. Click **Generate** to produce C++ proxy code ready to use in a proxy DLL project

You can also launch it directly from the command line:

```
ExportsToC++.exe path\to\target.dll
```

## How It Works

The tool invokes `dumpbin.exe /exports` on the target binary, parses the exported function names and ordinals, and generates C++ `#pragma comment(linker, ...)` directives and forwarding stubs — the boilerplate needed to build a drop-in proxy DLL.

## License

GNU General Public License v3. See [COPYING](COPYING) for details.
