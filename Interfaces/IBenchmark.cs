using MHPlatTest.BenchmarkFunctions;
using MHPlatTest.Divers;
using MHPlatTest.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MHPlatTest.Interfaces
{
    //[JsonInterfaceConverter(typeof(InterfaceConverter<IBenchmark>))]
    [JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
    [JsonDerivedType(typeof(Ackley), typeDiscriminator: "Ackley")]
    [JsonDerivedType(typeof(AlpineNumber1), typeDiscriminator: "AlpineNumber1")]
    [JsonDerivedType(typeof(Discus), typeDiscriminator: "Discus")]
    [JsonDerivedType(typeof(ExpandedScaffers), typeDiscriminator: "ExpandedScaffers")]
    [JsonDerivedType(typeof(Exponential), typeDiscriminator: "Exponential")]
    internal interface IBenchmark
    {
        /// <summary>
        /// Name of current BenchmarkFunction
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Provide a short description, if available, for current BenchmarkFunction
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// A unique integer identifier to dfferntiate between benchmark functions
        /// to not be mixed with ParentInstanceID which identify instance of the object
        /// </summary>
        public int IDNumero { get; set; }
        /// <summary>
        /// The lower bound of the search space for the current BenchmarkFunction
        /// </summary>
        public double[] SearchSpaceMinValue { get; set; }
        /// <summary>
        /// The upper bound of the search space for the current BenchmarkFunction
        /// </summary>
        public double[] SearchSpaceMaxValue { get; set; }


        /// <summary>
        /// Contains an unique idenifier used to identify the instances
        /// that were created from the same parent instance
        /// </summary>
        public int ParentInstanceID { get; set; }

        /// <summary>
        /// this minimum dimension for the current BenchmarkFunction
        /// </summary>
        public short MinProblemDimension { get; set; }

        /// <summary>
        /// the maximum dimension for the current BenchmarkFunction
        /// </summary>
        public short MaxProblemDimension { get; set; }

        /// <summary>
        /// Evaluate the function and returns the computed value
        /// </summary>
        /// <param name="PointToBeEvaluated">The point forwhich the fonction will be evaluated</param>
        /// <param name="nbrProblemDimension">Number of problem dimension</param>
        /// <param name="ShiftOptimumToZero">whether the optimum for the current benchmark function will be shifted to zero automatically</param>
        /// <returns>the computed value</returns>
        public double ComputeValue(double[] functionParameter, ref int currentNumberofunctionEvaluation, bool ShiftOptimumToZero);

        /// <summary>
        /// Return the theoritical optimal value for current function
        /// </summary>
        /// <param name="nbrProblemDimension">Number of problem dimension </param>
        /// <returns></returns>
        public double OptimalFunctionValue(int nbrProblemDimension);



        /// <summary>
        /// Return the theoritical optimal point at which the function will be optimized
        /// </summary>
        /// <param name="nbrProblemDimension">Number of problem dimension </param>
        /// <returns></returns>
        public List<double[]> OptimalPoint(int nbrProblemDimension);


        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; }

    }
}
