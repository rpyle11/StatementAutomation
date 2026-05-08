using System;
using System.Collections.Generic;
using System.Text;

namespace StatementProcessorModels
{
    public class JobDataDto
    {
        public JobDto? Job { get; set; }

        public List<JobStepsDto>? JobSteps { get; set; }
    }
}
