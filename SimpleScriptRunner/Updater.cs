using System;
using System.Collections.Generic;

namespace SimpleScriptRunner
{
    public class Updater 
    {
        private readonly IScriptSource<ITextScriptTarget> scriptSource;
        private readonly ITextScriptTarget scriptTarget;
        private readonly Options options;

        public Updater(IScriptSource<ITextScriptTarget> scriptSource, ITextScriptTarget scriptTarget, Options options)
        {
            this.scriptSource = scriptSource;
            this.scriptTarget = scriptTarget;
            this.options = options;
        }

        private static readonly List<int> skippedMajor = new List<int>();
        private static readonly List<int> skippedMinor = new List<int>();

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
                else if (script.Version.compareIgnoreDate(currentVersion) <= 0)
                {
                    Console.WriteLine("Skipping patch: {0}", script);
                }
                else if (options.MaxPatch != null && script.Version.Patch > options.MaxPatch.Value)
                {
                    Console.WriteLine("Stopping at maximum patch {0}", options.MaxPatch);
                    return;
                }
                else
                {
                    Console.WriteLine("Running: {0}", script);
                    script.apply(scriptTarget, options);
                    currentVersion = scriptTarget.CurrentVersion;
                }
            }
        }
    }
}