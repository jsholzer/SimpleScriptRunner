using System.Collections.Generic;

namespace SimpleScriptRunnerBto
{
	public interface IScriptSource<T> where T : IScriptTarget
	{
		IEnumerable<IScript<T>> Scripts { get; }
	}
}
