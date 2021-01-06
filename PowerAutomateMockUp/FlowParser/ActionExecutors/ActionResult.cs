using Parser.ExpressionParser;

namespace Parser.FlowParser.ActionExecutors
{
    /// <summary>
    /// An ActionResult contains updated/retrieved variables, as well as
    /// the name of the next action to run.
    /// If the RunAfter is null, the Mock decides which action to run.
    /// </summary>
    public class ActionResult
    {
        /// <summary>
        /// RunAfter contains the name of the flow, which should be run immediately after this flow.
        /// </summary>
        public string NextAction { get; set; }

        /// <summary>
        /// This is perhaps not necessary. 
        /// </summary>
        public ActionStatus ActionStatus { get; set; } = ActionStatus.Succeeded;

        /// <summary>
        /// If for some reason you want to stop the execution, this can
        /// be set to false
        /// </summary>
        public bool ContinueExecution { get; set; } = true;
        
        /// <summary>
        /// When and if an action is done and the output have been produced,
        /// the output is returned to the flow runner using the ActionResult object and
        /// the output is added correctly to the state.
        /// </summary>
        public ValueContainer ActionOutput { get; set; }
    }

    public enum ActionStatus
    {
        Succeeded,
        Failed,
        Skipped,
        TimedOut
    }
}