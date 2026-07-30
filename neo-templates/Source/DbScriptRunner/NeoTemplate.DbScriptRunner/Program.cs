namespace NeoTemplate.DbScriptRunner
{
  using System.Collections.Generic;
  using System.Linq;
  using Neo.DbScriptRunner.Commands;
  using Neo.DbScriptRunner.Helpers;
  using Neo.DbScriptRunner.Models;
  using Neo.DbScriptRunner.Services;
  using Neo.DbScriptRunner.Types;
  using Neo.DbScriptRunner.Validators;
  using Neo.DbScriptRunner.Validators.FullyQualifiedTableNames;
  using Neo.DbScriptRunner.Validators.RestrictAffectedDatabases;
  using Neo.DbScriptRunner.Validators.ScriptHasUseStatements;

  /// <summary>
  /// DB Script Runner entrypoint class
  /// </summary>
  public class Program
  {
    /// <summary>
    /// The application's entrypoint method.
    /// </summary>
    /// <param name="args">Command line arguments passed to the application</param>
    /// <returns>The exit code.</returns>
    public static int Main(string[] args)
    {
      ConsoleHelper.WriteHeading("NeoTemplate Database Script Runner", TextBorderStyle.SingleBorder, TextAlign.Left, borderColour: Ansi.White);

      ScriptRunnerCommand scriptRunnerCommand = new(
        new ConfigurationService(args),
        config =>
        {
          config.DbScriptRunnerProjectName = "NeoTemplate.DbScriptRunner";

          // config.RowCountsBeforeQueries = false; // Uncomment if you want to put the Expected Row Counts after the queries in the scripts (instead of before).
          config.ScriptValidators = GetScriptValidators(config);
        });

      int exitCode = scriptRunnerCommand.Execute();

      return exitCode;
    }

    private static List<IScriptValidator> GetScriptValidators(ScriptRunnerConfig config)
    {
      var validators = new List<IScriptValidator>();

      var allowedDatabaseNames = config.AllowedDbNamePrefixes;

      var allowedAffectedDatabaseNames = config.AllowedAffectedDbNamePrefixes;

      // Setup validators
      var useStatementValidator = new ScriptHasUseStatementValidator(
        new ScriptHasUseStatementValidatorOptions()
        {
          CanHaveMultipleUseStatements = true,
          UseStatementRules = allowedDatabaseNames.Any() ? [new UseStatementMustBeInAllowedDatabasesRule(allowedDatabaseNames)] : [],
        });

      var fullyQualifiedTableNameValidator = new FullyQualifiedTableNameValidator(
        new FullyQualifiedTableNameValidatorOptions()
        {
          TableNameRules = allowedDatabaseNames.Any() ? [new FullyQualifiedNamePrefixRule(allowedDatabaseNames)] : [],
        });

      // - Does not contain any transaction statements (BEGIN TRAN, COMMIT, ROLLBACK)
      validators.Add(new NoTransactionStatementsValidator());

      // combine the 2 together so that either can pass
      // - Each script either has a USE statement that points to an allowed database, or all table names are fully qualified with an allowed database name prefix
      validators.Add(new ScriptOrValidator(useStatementValidator, fullyQualifiedTableNameValidator));

      if (allowedAffectedDatabaseNames.Any())
      {
        // - Only affects databases with allowed name prefixes
        validators.Add(new AllowedAffectedDbNamePrefixesValidator(new AllowedAffectedDbNamePrefixesOptions() { AllowedNames = allowedAffectedDatabaseNames }));
      }

      // Note: Additional validators can be added here as required. You can implement your own Validator by implementing the IScriptValidator interface.

      return validators;
    }
  }
}
