namespace SimpleScriptRunnerBto;

public interface IScript<T> where T : IScriptTarget
{
	ScriptVersion Version { get; }
	void apply(T scriptTarget, Options options);
}