using System.Diagnostics.Metrics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ThreadDemo.Domain;

public sealed class ThreadExperiments
{
    private static int counter = 0;
    private static object gate = new();

    public void RunSequential()
    {
        Work("A");
        Work("B");
    }

    public void RunThreads()
    {
        // lambda function to assign a certain work to a thread in its constructor
        Thread thread1 = new(() => Work("A"));
        Thread thread2 = new(() => Work("B"));
        Console.WriteLine("Starting threads....");

        // This is a non-deterministic run
        thread1.Start();
        thread2.Start();

        Console.WriteLine("This line is written by main thread");

        thread1.Join(); // Waiting thread to finish
        thread2.Join();

        // This is a deterministic run, due to the threads waiting (carried out by join)
        Console.WriteLine("All finished");
    }

    public void ShowThreadProperties()
    {
        Thread thread = Thread.CurrentThread;
        Console.WriteLine($"Thread ID: {thread.ManagedThreadId}");
        Console.WriteLine($"Thread Name: {thread.Name}");
        Console.WriteLine($"Thread Priority: {thread.Priority}");
        Console.WriteLine($"Is Background Thread: {thread.IsBackground}");
    }

    public void RunBackgroundThreadExperiment()
    {
        Thread worker = new(() =>
        {
            Console.WriteLine($"[Background worker] Started on managed thread {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(3000);
            Console.WriteLine($"[Background worker] Finished after 3 seconds on managed thread {Thread.CurrentThread.ManagedThreadId}");
        })
        {
            IsBackground = true
        };

        worker.Start();
        Console.WriteLine("[Main thread] Background worker started without Join.");
    }

    public void RunForegroundThreadExperiment()
    {
        Thread worker = new(() =>
        {
            Console.WriteLine($"[Foreground worker] Started on managed thread {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(3000);
            Console.WriteLine($"[Foreground worker] Finished after 3 seconds on managed thread {Thread.CurrentThread.ManagedThreadId}");
        })
        {
            IsBackground = false
        };

        worker.Start();
        Console.WriteLine("[Main thread] Foreground worker started without Join.");
    }

    public (int Expected, int Actual) RunCounter(bool useLock, int iterations = 100_000)
    {
        counter = 0;

        Thread t1 = new Thread(() => Increment(iterations, useLock));
        Thread t2 = new Thread(() => Increment(iterations, useLock));

        t1.Start();
        t2.Start();

        t1.Join();
        t2.Join();

        return (iterations * 2, counter);
    }


    private static void Increment(int iterations, bool useLock)
    {
        for (int i = 0; i < iterations; i++)
        {
            if (useLock)
            {
                lock (gate)
                {
                    counter++;
                }
            }
            else
            {
                counter++;
            }
        }
    }

    // Funzione per simulare del lavoro
    private static void Work(string name)
    {
        for (int i = 0; i <= 5; i++)
        {
            Console.WriteLine($"{name}; Thread: {Thread.CurrentThread.ManagedThreadId}; index {i}");
            Thread.Sleep(100);
        }
    }
}