namespace SimpleScriptRunner
{
	public interface ITextScriptTarget : IScriptTarget
	{
        void apply(string content);
        void updateVersion(ScriptVersion version);
    }
}