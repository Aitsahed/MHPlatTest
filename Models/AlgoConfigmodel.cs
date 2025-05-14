using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHPlatTest.Models
{
    internal class AlgoConfigmodel
    {
        public AlgoConfigmodel(Int16 populationSize, Int16 iteratonMaximalValue, Int16 problemDimension)
        {
            PopulationSize = populationSize;
            IteratonMaximalValue = iteratonMaximalValue;
            ProblemDimension = problemDimension;
        }

        public Int16 PopulationSize { get; set; }
        public Int16 IteratonMaximalValue { get; set; }
        public Int16 ProblemDimension { get; set; }
    }
}
