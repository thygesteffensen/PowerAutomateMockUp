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
    }

    public enum ActionStatus
    {
        Succeeded,
        Failed,
        Skipped,
        TimedOut
    }
}