using System.Collections.Generic;

namespace SimpleScriptRunner
{
	public interface IScriptSource<T> where T : IScriptTarget
	{
		IEnumerable<IScript<T>> Scripts { get; }
	}
}
