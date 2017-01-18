# AsyncAwaitTaskDemo

The sample code in this repo tries to explain async, await, and Task in C#.  
async/await and Task are keywords in C# that have to do with async functions.  They are similar and related, but they do not mean the same things.

## What is a Task?
`Task` is a return type.  It tells the compiler that your function returns an object that might do work on a background (ThreadPool) thread.  

Simply returning a Task does not cause your function to be non-blockig or to run in a background thread.  You have to do something explicit to make your code non-blocking.

For exmple, this code will execute in the caller's thread and sleep for 100 before returning to the caller's thread.  Even though this function returns a `Task` object, that `Task` is already Completed before the calling code gets it.
```
static Task Task_That_Returns_Immediately_Without_Spawning_Any_Work()
{
    Log("entering Task_That_Returns_Immediately_Without_Spawning_Any_Work");
    Rest(100);  // sleep for 100 ms
    Log("exiting Task_That_Returns_Immediately_Without_Spawning_Any_Work");
    return CompletedTask;
}
``` 

You can call this from the main thread and see where it executes.
```
Log("Calling Task_That_Returns_Immediately_Without_Spawning_Any_Work without waiting.  This will run sync on the foreground thread because it doesn't ever yeild the thread.");
Task_That_Returns_Immediately_Without_Spawning_Any_Work();
Rest(400);
Log("Done with Task_That_Returns_Immediately_Without_Spawning_Any_Work");
```

If you run this code, you'll see that `Task_That_Returns_Immediately_Without_Spawning_Any_Work` runs on the main thread and blocks the caller.  You can tell this because the `Rest(100)` call inside `Task_That_Returns_Immediately_Without_Spawning_Any_Work` completes before the `Rest(400)` call even starts.  You can tell that all the code runs in the main thread because the `Log()` function prepends 'M' if it's running in the main thread and 'B' if it's running in a background thread.

```
M: Calling Task_That_Returns_Immediately_Without_Spawning_Any_Work without waiting.  This will run sync on the foreground thread because it doesn't ever yeild the thread.
M: entering Task_That_Returns_Immediately_Without_Spawning_Any_Work
M: Started sleeping for 100
M: Done sleeping for 100
M: exiting Task_That_Returns_Immediately_Without_Spawning_Any_Work
M: Started sleeping for 400
M: Done sleeping for 400
M: Done with Task_That_Returns_Immediately_Without_Spawning_Any_Work
```

Because `Task_That_Returns_Immediately_Without_Spawning_Any_Work` returns a `Task` object, you can call `Wait()` on that object, but it doesn't change the results:

```
Log("Calling Task_That_Returns_Immediately_Without_Spawning_Any_Work with waiting.  This will run sync on the foreground thread because it doesn't ever yield the thread and returns a completed Task object.");
Task_That_Returns_Immediately_Without_Spawning_Any_Work().Wait();
Rest(400);
Log("Done with Task_That_Returns_Immediately_Without_Spawning_Any_Work");
Console.ReadLine();
```

This produces the following.  Again, you see the `Rest(100)` complete before the `Rest(400)` starts.  Since the function returns a completed `Task`, the `Wait` call does nothing.  The function is still blocking and it still runs in the main thread.

```
M: Calling Task_That_Returns_Immediately_Without_Spawning_Any_Work with waiting.  This will run sync on the foreground thread because it doesn't ever yield the thread and returns a completed Task object.
M: entering Task_That_Returns_Immediately_Without_Spawning_Any_Work
M: Started sleeping for 100
M: Done sleeping for 100
M: exiting Task_That_Returns_Immediately_Without_Spawning_Any_Work
M: Started sleeping for 400
M: Done sleeping for 400
```

## Make a function non-blocking by putting a Task on an arbitrary thread
If you want to make your code non-blocking and put it onto an arbitrary thread, the best way is to use `Task.Run()`.  This returns a `Task` object and queues the code to run on  _some_ thread.

When you use `Task.Run()` or other methods to spawn a background thread, what you're really doing is putting the code onto an _arbitrary_ thread.  It might be background or it might be foreground.  The fact is that we shouldn't care.

This is a very important concept, so I'm going to repeat it: Simply returning a Task object does not make your function asynchronous.  You need to go out of your way to put your code into an arbitrary thread.  Task.Run() is the easiest way, but it's not the only way.

```
static Task Task_That_Runs_On_Background_Thread()
{
    return Task.Run(() =>
    {
        Log("Entering Task_That_Runs_On_Background_Thread");
        Rest(100);
        Log("Exiting Task_That_Runs_On_Background_Thread");
    });
}
```

Since this function returns a Task, it (probably) returns before the Task executes.  If we call this without waiting like this:
```
Log("Calling Task_That_Runs_On_Background_Thread without waiting.  This will run on a background thread because it returns a true Task.");
Task_That_Runs_On_Background_Thread();
Rest(400);
```

We see the following, which shows the main thread starting to rest for 400 ms before the `Task` even begins executing.  We can also see that the `ThreadPool` scheduler put the `Task` onto a background thread because the lines begin with 'B':
```
M: Calling Task_That_Runs_On_Background_Thread without waiting.  This will run on a background thread because it returns a true Task.
M: Started sleeping for 400
B: Entering Task_That_Runs_On_Background_Thread
B: Started sleeping for 100
B: Done sleeping for 100
B: Exiting Task_That_Runs_On_Background_Thread
M: Done sleeping for 400
M: Done with Task_That_Runs_On_Background_Thread
```

The compiler helps us here by giving us a warning that we should probably be waiting for the Task to complete:
```
1>blah.cs: warning CS4014: Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
```

We haven't talked about `await` yet, but we're about to get there.  To start simple, if you want to wait for the Task to complete, the easiest (and sometimes very un-advidable) way is to call `Task.Wait()`:
```
Log("Calling Task_That_Runs_On_Background_Thread with waiting.  This will run on a background thread. But the wait will cause it to block the foreground thread");
Task_That_Runs_On_Background_Thread().Wait();
Rest(400);
```

When we run this, we see the `Task` run on a background thread, and we also see the main thread block until the `Task` is complete.  We can see this happening because the main thread doesn't execute The `Rest(400)` call until the background thread exits `Task_That_Runs_On_Background_Thread`.
```
M: Calling Task_That_Runs_On_Background_Thread with waiting.  This will run on a background thread. But the wait will cause it to block the foreground thread
B: Entering Task_That_Runs_On_Background_Thread
B: Started sleeping for 100
B: Done sleeping for 100
B: Exiting Task_That_Runs_On_Background_Thread
M: Started sleeping for 400
M: Done sleeping for 400
M: Done with Task_That_Runs_On_Background_Thread
```

I say that using `Wait()` is unadvisable because `Wait()` it a blocking operation.  This means that the main thread is unavailable to do anything else until the `Wait` call returns.  The net effect is that all the effort that you put into making your function run in a background thread was just wasted becuase your foreground thread is blocking anyway.  To get around this, you can use the async/await keywords.

## The await keyword
await is a special keyword introduced in .net 4.5.  It looks similar to .Wait(), but it has several big distinctions:
```            
Log("entering Async_Task_That_Awaits_On_Background_Thread_Task");
await Task_That_Runs_On_Background_Thread();
Log("exiting Async_Task_That_Awaits_On_Background_Thread_Task");
```

Just like calling .Wait(), like we did above, the second `Log()` call doesn't execute until `Task_That_Runs_On_Background_Thread` is complete.  The big distinction here is that:
1. While the `Task` executes, the calling thread returns to the `ThreadPool` to do other work.  This is especially important if the calling thread is the UI thread because this makes it available to service other UI requests while the code is waiting for the `Task` to complete.
2. The code after `await` call _might_ be in a different thread than the code before the `await`.

You can see this operate in the test code:
```
M: entering Async_Task_That_Awaits_On_Background_Thread_Task
B: Entering Task_That_Runs_On_Background_Thread
B: Started sleeping for 100
B: Done sleeping for 100
B: Exiting Task_That_Runs_On_Background_Thread
B: exiting Async_Task_That_Awaits_On_Background_Thread_Task
```

In this code, we see the main thread executing the first call to `Log()`, then we see a background (`ThreadPool`) thread run the `Task`.  Finally, a background thread executes the second call to `Log()`.  This is interesting because part of the calling function runs in the context of the main thread and part of the calling function exceutes in the context of a `ThreadPool` thread.  (If you want to geek out on this, the await keyword implements a [Continuation](https://en.wikipedia.org/wiki/Continuation))

You can only use `await` on functions that return `Task` objects.  If you try to `await` functions that return other types, you get an error:
```
1>blah.cs: error CS4008: Cannot await 'void'
```
or
```
1>blah.cs: error CS1061: 'int' does not contain a definition for 'GetAwaiter' and no extension method 'GetAwaiter' accepting a first argument of type 'int' could be found (are you missing a using directive or an assembly reference?)
```

## The async keyword.

Contrary to intuition, the `async` keyword does not make a function asynchronous.  Just as I said above (and I'm repeating myself again because this is confusing) Using `Task.Run() `to put code into a `ThreadPool` thread is what makes the code asynchronous.  As we see above, you can implement asynchronous code without ever using the `async` keyword.

I can't repeat this often enough.  Say it with me: "The async keyword does not make your code run asynchronously".

The `async` keyword is part of a function definition.  It doesn't change what the function returns.  Instead, it tells the compiler that the function wants to use the `await` keyword.  A better name might have been `uses_await`

If you try to use `await` in a function that's not tagged with the `async` keyword, you get an error:
```
1>blah.cs: error CS4032: The 'await' operator can only be used within an async method. Consider marking this method with the 'async' modifier and changing its return type to 'Task'.
```

In addition to enabling the `await` keyword, the `async` modifier does more.  In particular, it saves you from having to explicitely return a Task object from your function.  In this code, you'll see that the function is supposed to return a `Task` object, but you don't have to write any code to create this object.
```
static async Task void_function_that_you_can_await()
{
    await Task_That_Runs_On_Background_Thread();
}
```

Under the covers, the compiler created the code to return the `Task` object to the caller.  In it's simplest form,  the above code could be written as follows:
```
static Task void_function_that_you_can_await()
{
    return Task.Run( () => 
    {
      Task_That_Runs_On_Background_Thread().Wait();
    });
}
```

But, this isn't quite right because the `Task.Run` code above consumes two threads: one for the outer thread, which calls `Task_That_Runs_On_Background_Thread` and then blocks to wait on the result, and a different thread to execute `Task_That_Runs_On_Background_Thread`.

There's another advantage to using the await keyword and that's exception handling.  If you didn't have the `await` keyword at your disposal, and you were worried about the `Task` throwing an exception, you would have to inspect the Task object and see if `IsFaulted` is set on the `Task`.  With `await`, you can use `try`/`catch` just like you were running in a single thread:

```
static async Task void_function_that_you_can_await()
{
    try
    {
        await Task_That_Runs_On_Background_Thread();
    }
    catch(Exception e)
    {
        HandleException(e);
    }
}
```

In this code, the `async` and` `await keywords allow the compiler to generate code to intercept any exceptions that happen inside the `Task` (which could be in a completely different thread) and throw it so it can be caught in the calling code.  Pretty neat.

One final note is that the compiler will warn you if you're using `async` when you don't need to.  If you have this code (which doesn't use `await` at all):
```
static async Task Async_Task_That_Returns_Immediately_Without_Spawning_Any_Work()
{
    Log("entering Async_Task_That_Returns_Immediately_Without_Spawning_Any_Work");
    Rest(100);
    Log("exiting Async_Task_That_Returns_Immediately_Without_Spawning_Any_Work");
}
```

You get this warning:
```
1>blah.cs warning CS1998: This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
```


## References:

Here are a few references that might help to explain this better than I can:
1) https://msdn.microsoft.com/en-us/library/dd537609(v=vs.110).aspx
2) https://www.wintellectnow.com/Videos/Watch?videoId=performing-i-o-bound-asynchronous-operations (The relevant part is from 6:55 to 17:00).

Thanks to @tameraw, @zolvarga, and @jasmineymlo for noodling this over with me. 






