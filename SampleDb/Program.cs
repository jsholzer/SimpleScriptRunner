using SimpleScriptRunnerBto;

namespace SampleDb
{
    class Program
    {
        static void Main(string[] args)
        {
            Options options = new Options();
            options.UseTransactions = true;
            options.Params.Add("localhost");
            options.Params.Add("simple_script_runner_sample");
            options.Params.Add("simplescriptadmin");
            options.Params.Add("abc123");
            options.Params.Add("sample");
            TopProgram.simpleScriptRunnerProgramMain(options);
        }
    }
}