# Background Task
A small package to create asynchronous timers using the periodic timer class.

Example usage:

```csharp
static void MyAction()
{
    Console.WriteLine("Hello world!");
}

// BackgroundTask(Action, milliseconds or Timespan, repeats [default is unlimited])
var backgroundTask = new BackgroundTask(MyAction, 1000);

backgroundTask.Start();

Console.ReadKey();

backgroundTask.StopAsync();
```
