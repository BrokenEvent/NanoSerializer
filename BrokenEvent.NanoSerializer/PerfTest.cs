using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

#pragma warning disable 1591

namespace BrokenEvent.NanoSerializer
{
  public static class PerfTest
  {
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

    private static Action<Test, string> actionDelegateStatic;

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
      actionDelegateStatic = (Action<Test, string>)Delegate.CreateDelegate(typeof(Action<Test, string>), method);

      const int ITERATIONS = 10000000;

      Stopwatch stopwatch = Stopwatch.StartNew();
      for (int i = 0; i < ITERATIONS; i++)
        info.SetValue(test, TEST);
      stopwatch.Stop();          
      Console.WriteLine($"SetValue               : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      stopwatch.Restart();
      object[] cache = { TEST };
      for (int i = 0; i < ITERATIONS; i++)
        method.Invoke(test, cache);
      stopwatch.Stop();
      Console.WriteLine($"Invoke                 : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        action(test, TEST);
      stopwatch.Stop();
      Console.WriteLine($"Action                 : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        InvocationHelper.SetProperty(test, test.GetType(), info, TEST);
      stopwatch.Stop();
      Console.WriteLine($"SetProperty            : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        test.A = TEST;
      stopwatch.Stop();
      Console.WriteLine($"Direct                 : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        SetTyped(test, TEST);
      stopwatch.Stop();
      Console.WriteLine($"SetTyped               : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        SetUntyped(test, TEST);
      stopwatch.Stop();
      Console.WriteLine($"SetUntyped             : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        actionTyped(test, TEST);
      stopwatch.Stop();
      Console.WriteLine($"SetTyped(action)       : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        actionUntyped(test, TEST);
      stopwatch.Stop();
      Console.WriteLine($"SetUntyped(action)     : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        actionLambdaTyped(test, TEST);
      stopwatch.Stop();
      Console.WriteLine($"LambdaTyped            : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        actionLambdaUntyped(test, TEST);
      stopwatch.Stop();
      Console.WriteLine($"LambdaUntyped          : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        actionDelegate(test, TEST);
      stopwatch.Stop();
      Console.WriteLine($"Delegate               : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        actionDelegateStatic(test, TEST);
      stopwatch.Stop();
      Console.WriteLine($"DelegateStatic         : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      stopwatch.Restart();
      for (int i = 0; i < ITERATIONS; i++)
        action.DynamicInvoke(test, TEST);
      stopwatch.Stop();
      Console.WriteLine($"DynamicInvoke          : {1000f * stopwatch.ElapsedTicks / ITERATIONS} / call");

      Console.WriteLine("-- All done.");
    }
  }  
}
