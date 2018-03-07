using Orleans;
using System;
using System.Threading.Tasks;

namespace OrleansShutdownIssue.Interfaces {

    public interface ISampleGrain : IGrainWithIntegerKey {
        Task HelloWorld();
    }
}
