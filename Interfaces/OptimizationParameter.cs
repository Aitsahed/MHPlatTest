using MHPlatTest.Divers;

namespace MHPlatTest.Interfaces11
{    /// <summary>
     /// Use to present a metaheuristic parametres values
     /// </summary>
    internal interface OptimizationParameter
    {
        /// <summary>
        /// The parameter name
        /// </summary>
        public MHAlgoParameters Name { get; set; }

        /// <summary>
        /// any type value (if needed)
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Indicate whether the parameter is an important one.
        /// the important parameter details are displayed to indicate the progress of the optimization process especially in a batch (sequence) of optimization processes
        /// </summary>
        public bool IsEssentialInfo { get; set; }
    }
}
