using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace SimpleScriptRunnerBto
{
    public class Updater 
    {
        private readonly IScriptSource<ITextScriptTarget> scriptSource;
        private readonly ITextScriptTarget scriptTarget;
        private readonly Options options;

        private readonly List<int> skippedMajor = new List<int>();
        private readonly List<int> skippedMinor = new List<int>();
        private List<ScriptVersion> existingPatches;
        
        public Updater(IScriptSource<ITextScriptTarget> scriptSource, ITextScriptTarget scriptTarget, Options options)
        {
            this.scriptSource = scriptSource;
            this.scriptTarget = scriptTarget;
            this.options = options;
        }
        
        public void applyScripts(ScriptVersion currentVersion)
        {
            foreach (IScript<ITextScriptTarget> script in scriptSource.Scripts)
            {
                if (script.Version.Major < currentVersion.Major)
                {
                    if (skippedMajor.Contains(script.Version.Major))
                        continue;

                    skippedMajor.Add(script.Version.Major);
                    Console.WriteLine("Skipping release: {0}.x", script.Version.Major);
                }
                else if (script.Version.Major == currentVersion.Major && script.Version.Minor < currentVersion.Minor)
                {
                    if (skippedMinor.Contains(script.Version.Minor))
                        continue;

                    skippedMinor.Add(script.Version.Minor);
                    Console.WriteLine("Skipping release: {0}.{1}", script.Version.Major, script.Version.Minor);
                }
                else if (options.MaxPatch != null && script.Version.Patch > options.MaxPatch.Value)
                {
                    Console.WriteLine("Stopping at maximum patch {0}", options.MaxPatch);
                    return;
                }
                else 
                {
                    existingPatches = existingPatches ?? scriptTarget.getPatches(currentVersion.Major, currentVersion.Minor);      
                    if (existingPatches.Contains(script.Version))
                    {
                        Console.WriteLine("Skipping patch: {0}", script);
                        continue;
                    }

                    Console.WriteLine("Running: {0}", script);
                    script.apply(scriptTarget, options);
                    
                    if (scriptTarget.CurrentVersion.CompareTo(currentVersion) > 0)
                        currentVersion = scriptTarget.CurrentVersion;
                }
            }
        }
    }
}