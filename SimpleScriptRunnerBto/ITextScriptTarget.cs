namespace SimpleScriptRunnerBto
{
	public interface ITextScriptTarget : IScriptTarget
	{
        void apply(string content);
        void updateVersion(ScriptVersion version);
    }
}