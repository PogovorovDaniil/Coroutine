using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public interface IDeltaProvider
{
    float Delta { get; }
}
public class Coroutine : IDeltaProvider
{
    private IEnumerator<float> action;
    private bool move;
    private Task task;
    public float Delta { get; private set; }

    public Coroutine(Func<IDeltaProvider, IEnumerator<float>> action)
    {
        move = true;
        this.action = action(this);
        task = Task.Run(Move);
    }

    public void Stop()
    {
        move = false;
        task.GetAwaiter().GetResult();
    }

    private async void Move()
    {
        long lastTime = 0;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        while (move && action.MoveNext())
        {
            if (action.Current > 0f) await Task.Delay((int)(1000 * action.Current));
            long elapsed = sw.ElapsedMilliseconds;
            Delta = (elapsed - lastTime) / 1000f;
            lastTime = elapsed;
        }
        sw.Stop();
    }
}

public class Program
{
    static void Main()
    {
        Coroutine coroutine = new Coroutine(Loop);
        Task.Delay(1000).Wait();
        coroutine.Stop();
    }

    static IEnumerator<float> Loop(IDeltaProvider deltaProvider)
    {
        for (float i = 0; ; i += deltaProvider.Delta)
        {
            Console.WriteLine(i);
            yield return 0.01f;
        }
    }
}