using Orleans;
using OrleansShutdownIssue.Interfaces;
using System;
using System.Threading.Tasks;

namespace OrleansShutdownIssue.Grains {
    public class SampleGrain : Grain, ISampleGrain {

        public Task HelloWorld() {
            return Task.CompletedTask;
        }
    }
}
