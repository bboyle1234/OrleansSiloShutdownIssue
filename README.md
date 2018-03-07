**Update**

Fix for the shutdown issue was this code - setting `a.Cancel = true;`: 
```
    Console.CancelKeyPress += (s, a) => {
        a.Cancel = true;
        Task.Run(StopSilo);
    };
```
I"m not sure what to do about the log warning though

# OrleansSiloShutdownIssue

An interesting investigation of console app shutdown behaviour. Now that I've come this far with the experiment, it looks like it may have nothing to do with Orleans itself. 

If the `Console.CancelKeyPressed` event handler returns before the `static void Main(string[] args)` method returns, the console app hangs.

In this project, the Orleans silo shutdown triggers `_siloShutdownEvent.Set()`. The two methods mentioned above are waiting on that event before they can return. If there are no `Thread.Sleep(100)` statements in the code, it's a random race to the finish. When `Main` finishes first, the program exits gracefully. When `Console.CancelKeyPress` finishes first, the program hangs.

I've added a bool flag that you can change to determine the race winner so you can see both outcomes. 

Either way, Orleans emits the following warning: 

```
warn: Orleans.Runtime.Catalog[100502]
      UnregisterManyAsync 1 failed.
System.InvalidOperationException: Grain directory is stopping
   at Orleans.Runtime.GrainDirectory.LocalGrainDirectory.CheckIfShouldForward(GrainId grainId, Int32 hopCount, String operationDescription)
   at Orleans.Runtime.GrainDirectory.LocalGrainDirectory.UnregisterOrPutInForwardList(IEnumerable`1 addresses, UnregistrationCause cause, Int32 hopCount, Dictionary`2& forward, List`1 tasks, String context)
   at Orleans.Runtime.GrainDirectory.LocalGrainDirectory.<UnregisterManyAsync>d__107.MoveNext()
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at Orleans.Runtime.Scheduler.AsyncClosureWorkItem.<Execute>d__8.MoveNext()
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at Orleans.Runtime.Catalog.<FinishDestroyActivations>d__76.MoveNext()
warn: Orleans.Runtime.Catalog[100502]
      UnregisterManyAsync 2 failed.
System.InvalidOperationException: Grain directory is stopping
   at Orleans.Runtime.GrainDirectory.LocalGrainDirectory.CheckIfShouldForward(GrainId grainId, Int32 hopCount, String operationDescription)
   at Orleans.Runtime.GrainDirectory.LocalGrainDirectory.UnregisterOrPutInForwardList(IEnumerable`1 addresses, UnregistrationCause cause, Int32 hopCount, Dictionary`2& forward, List`1 tasks, String context)
   at Orleans.Runtime.GrainDirectory.LocalGrainDirectory.<UnregisterManyAsync>d__107.MoveNext()
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
```
