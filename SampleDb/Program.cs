﻿using SimpleScriptRunnerBto;

Options options = new Options();
options.UseTransactions = true;
options.ServerName = "localhost";
options.DatabaseName = "simple_script_runner_sample";
options.UserName = "simplescriptadmin";
options.Password = "abc123";
options.Path = "sample";

TopProgram.simpleScriptRunnerProgramMain(options);