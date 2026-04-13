# ExportsToC++

**ExportsToC++** is a Windows GUI tool that parses `dumpbin.exe` output and converts a DLL's exported function list into C++ proxy code. This is useful for creating proxy DLLs to intercept API calls.

Last updated: April 14, 2026

## Requirements

- Windows
- .NET Framework 2.0
- `dumpbin.exe` (included with Visual Studio) — automatically located in Visual Studio 8 or 9 install directories, or anywhere on your `PATH`

## Usage

1. Open a DLL in the tool via **File > Open** (or pass it as a command-line argument)
2. The tool runs `dumpbin.exe /EXPORTS` and displays the raw export table in the top pane
3. Click **Generate** to produce C++ proxy code in the bottom pane
4. When prompted, enter the name of the proxy DLL (without `.dll`)
5. Copy the output to clipboard or save it as a `.cpp` file via **File > Save**

You can also launch with a file pre-loaded:

```
ExportsToC++.exe path\to\target.dll
```

## Input

The tool accepts any Windows **DLL** (`.dll`) that exports functions. Internally it runs:

```
dumpbin.exe /EXPORTS "target.dll"
```

The raw `dumpbin` output is displayed in the top pane. The export table must contain at least one function — if the DLL has no exports the tool will warn you and skip generation.

## Output

The generated `.cpp` file contains:

- Standard includes (`stdafx.h`, `windows.h`, `iostream`)
- A `#pragma comment(linker, ...)` directive for each exported function, forwarding it from the proxy DLL to the real one by name and ordinal:

```cpp
#include "stdafx.h"
#include <iostream>
#include <windows.h>

using namespace std;

#pragma comment (linker, "/export:SomeFunction=reallib.SomeFunction,@1")
#pragma comment (linker, "/export:AnotherFunction=reallib.AnotherFunction,@2")
// ...

BOOL WINAPI DllMain(HINSTANCE hInst, DWORD reason, LPVOID)
{
    return true;
}
```

The proxy DLL name used in the forwarding directives (e.g. `reallib` above) is entered when you click Generate. The output can be copied to the clipboard or saved as a `.cpp` source file.

## License

MIT License. See [LICENSE](LICENSE) for details.
