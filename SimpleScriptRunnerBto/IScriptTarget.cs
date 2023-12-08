using System.Collections.Generic;

namespace SimpleScriptRunnerBto;

public interface IScriptTarget
{
	ScriptVersion CurrentVersion { get; }
	List<ScriptVersion> getPatches(int major, int minor);
}