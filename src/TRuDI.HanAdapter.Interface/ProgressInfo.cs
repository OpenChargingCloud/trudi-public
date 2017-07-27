namespace TRuDI.HanAdapter.Interface
{
    /// <summary>
    /// Callback data for UI updates during device readout operations.
    /// </summary>
    public class ProgressInfo
    {
        /// <summary>
        /// No information about the actual progress available.
        /// </summary>
        public const int UnknownProgress = -1;

        /// <summary>
        /// Message to show on the UI.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Indicates the overall readout progress in percent (0 to 100). 
        /// Set to <see cref="UnknownProgress"/> if there's no information about the actual progress available.
        /// </summary>
        public int Progress { get; set; }
    }
}