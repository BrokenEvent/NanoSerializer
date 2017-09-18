using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

#pragma warning disable 1591

namespace BrokenEvent.NanoSerializer.Tests
{
  public static class PerfTest
  {
    private const int ITERATIONS = 10000000;
    private static int consoleX, consoleY;

    private static void WriteTestStart(string name)
    {
      Console.Write("{0,-48}: ", name);
      consoleX = Console.CursorLeft;
      consoleY = Console.CursorTop;
      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write("testing...");
      Console.ForegroundColor = ConsoleColor.Gray;
    }

    private static void WriteTestFinish(Stopwatch stopwatch)
    {
      Console.SetCursorPosition(consoleX, consoleY);
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("{0} / call", 1000f * stopwatch.ElapsedTicks / ITERATIONS);
      Console.ForegroundColor = ConsoleColor.Gray;
    }

    public class Test
    {
      public string A { get; set; }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SetTyped(Test test, string s)
    {
      test.A = s;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SetUntyped(object test, object s)
    {
      ((Test)test).A = (string)s;
    }

    // ReSharper disable once UnusedMember.Local
    private static Func<object> GenericConstructorHelper<TResult>()
      where TResult : class, new()
    {
      return () => new TResult();
    }

    public static Func<object> CreateConstructorDelegate(Type type)
    {
      MethodInfo actualHelper = typeof(PerfTest).GetMethod("GenericConstructorHelper", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type);
      return (Func<object>)actualHelper.Invoke(null, null);
    }

    public static void TestPerformance()
    {
      Test test = new Test();
      PropertyInfo info = typeof(Test).GetProperty("A");

      const string TEST = "1";

      info.SetValue(test, TEST);
      Action<object, object> action = InvocationHelper.GetSetDelegate(typeof(Test), typeof(string), info.SetMethod);
      MethodInfo method = info.SetMethod;

      action(test, TEST);

      Action<object, object> actionUntyped = SetUntyped;
      Action<Test, string> actionTyped = SetUntyped;
      Action<Test, string> actionLambdaTyped = (target, arg) => target.A = arg;
      Action<object, object> actionLambdaUntyped = (target, arg) => ((Test)target).A = (string)arg;

      Action<Test, string> actionDelegate = (Action<Test, string>)Delegate.CreateDelegate(typeof(Action<Test, string>), method);
      Func<object> actionCreateLambda = CreateConstructorDelegate(typeof(Test));
      Func<object> actionCreateEmit = InvocationHelper.CreateConstructorDelegate(typeof(Test));
      ConstructorInfo ctor = typeof(Test).GetConstructor(new Type[0]);

      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("Performance test: Property set...");
      Console.ForegroundColor = ConsoleColor.Gray;

      WriteTestStart("PropertyInfo.SetValue");
      Stopwatch stopwatch = Stopwatch.StartNew();
      for (int i = 0; i < ITERATIONS; i++)
        info.SetValue(test, TEST);
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("MethodInfo.Invoke");
      stopwatch.Restart();
      object[] cache = { TEST };
      for (int i = 0; i < ITERATIONS; i++)
        method.Invoke(test, cache);
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("InvokationHelper.GetSetDelegate");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        action(test, TEST);
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("InvokationHelper.SetProperty");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        InvocationHelper.SetProperty(test, test.GetType(), info, TEST);
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("test.A = TEST");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        test.A = TEST;
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("SetTyped(Test test, string s)");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        SetTyped(test, TEST);
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("SetUntyped(object test, object s)");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        SetUntyped(test, TEST);
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("Action<Test, string>");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        actionTyped(test, TEST);
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("Action<object, object>");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        actionUntyped(test, TEST);
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("=> target.A = arg");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        actionLambdaTyped(test, TEST);
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("=> ((Test)target).A = (string)arg");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        actionLambdaUntyped(test, TEST);
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("Delegate.CreateDelegate");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        actionDelegate(test, TEST);
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("Delegate.DynamicInvoke");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        action.DynamicInvoke(test, TEST);
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine();
      Console.WriteLine("Performance test: Object creation...");
      Console.ForegroundColor = ConsoleColor.Gray;

      WriteTestStart("Activator.Create");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        Activator.CreateInstance(typeof(Test));
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("InvocationHelper.CreateConstructorDelegate");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        actionCreateEmit();
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("() => new TResult()");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        actionCreateLambda();
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("ConstructorInfo.Invoke");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        ctor.Invoke(null);
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      WriteTestStart("new Test()");
      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        // ReSharper disable once ObjectCreationAsStatement
        new Test();
      stopwatch.Stop();
      WriteTestFinish(stopwatch);

      Console.WriteLine("-- All done.");
      Console.WriteLine();
    }
  }  
}
