# npl
No PowerShell Logging

Derived Entirely From: <br />
[https://github.com/Ben0xA/nps](https://github.com/Ben0xA/nps) <br />
[https://github.com/leechristensen/Random/blob/master/CSharp/DisablePSLogging.cs](https://github.com/leechristensen/Random/blob/master/CSharp/DisablePSLogging.cs)


This is npl.exe or No PowerShell Logging.  Completely derived from others' work, just a way to illustrate how to entirely bypass PowerShell logging and AMSI. This is not intended to be used as is on anything other than a simple one-off penetration test.  For a real, long term, engagement only the necessary functionality should be extracted from this and weaponized.

Compile
```
 C:\Users\reigningshells>c:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /reference:C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\system.management.automation.dll /out:npl.exe npl.cs
```

Usage
```C:\Users\reigningshells>npl.exe
 usage:
 npl.exe -shell
 npl.exe "{powershell single command}"
 npl.exe "& {commands; semi-colon; separated}"
 npl.exe -encodedcommand {base64_encoded_command}
 npl.exe -encode "commands to encode to base64"
 npl.exe -decode {base64_encoded_command}
```

Interactive Shell
```
 C:\Users\reigningshells>npl.exe -shell
 PS C:\Users\reigningshells> ($PSVersionTable).PSVersion.Major
 
 5
```

Single Commands
```
 C:\Users\reigningshells>npl.exe Get-Date
 12/18/2015 2:19:37 PM
```

Multiple Commands 
```
 C:\Users\reigningshells>npl.exe "& Get-Date; Write-Output 'Ohai there'"
 12/18/2015 2:19:49 PM
 Ohai there
```

Encoding
```
 C:\Users\reigningshells>npl.exe -encode "& Get-Date; Write-Output 'Ohai there'"
 JgAgAEcAZQB0AC0ARABhAHQAZQA7ACAAVwByAGkAdABlAC0ATwB1AHQAcAB1AHQAIAAnAE8AaABhAGkAIAB0AGgAZQByAGUAJwA=
```

Decoding
```
 C:\Users\reigningshells>npl.exe -decode JgAgAEcAZQB0AC0ARABhAHQAZQA7ACAAVwByAGkAdABlAC0ATwB1AHQAcAB1AHQAIAAnAE8
 AaABhAGkAIAB0AGgAZQByAGUAJwA=
 & Get-Date; Write-Output 'Ohai there'
```

Running Encoded Command
```
 C:\Users\reigningshells>npl.exe -encodedcommand JgAgAEcAZQB0AC0ARABhAHQAZQA7ACAAVwByAGkAdABlAC0ATwB1AHQAcAB1AHQ
 AIAAnAE8AaABhAGkAIAB0AGgAZQByAGUAJwA=
 12/18/2015 2:20:19 PM
 Ohai there
```

Same Encoded Command works in PowerShell
```
 C:\Users\reigningshells>powershell.exe -noprofile -encodedcommand JgAgAEcAZQB0AC0ARABhAHQAZQA7ACAAVwByAGkAdABlA
 C0ATwB1AHQAcAB1AHQAIAAnAE8AaABhAGkAIAB0AGgAZQByAGUAJwA=

 Friday, December 18, 2015 2:20:30 PM
 Ohai there
```
