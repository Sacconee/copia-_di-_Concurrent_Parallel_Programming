namespace ThreadDemo.Domain;

public class ThreadExperiments
{
    public void RunSequential()
    {
        Work("A");
        Work("B");
    }

    public void RunThreads()
    {
        // lambda function to assing a certain work to a thread in his constructor
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

    // Avere una risorsa condivisa su cui più thread devono agire. può essere pericoloso
    // in quanto potrebbero esserci problemi sull'aggiornamento della variabile condivisa
    // 1° test: i thread contano sulla stessa variabile condivisa senza sincronizzazione
    // 2° test: i thread contano sulla stessa variabile condivisa con sincronizzazione
    // il metodo ritorna una tupla (Expected, Actual)
    public (int Expected, int Actual) RunCounter(bool useLock, int iterations=100_000)
    {
        int counter = 0;
        object gateLock = new();

        // functions used by our threads
        void Increment()
        {
            for (int i = 0; i <= iterations; i++)
            {

                if (useLock)
                {

                    lock (gateLock)
                    {
                        counter += 1;
                    }
                }else {
                    counter += 1;    
                }
            }
        }

        Thread thread1 = new(Increment);
        Thread thread2 = new(Increment);

        thread1.Start();
        thread2.Start();
        
        thread1.Join();
        thread2.Join();

        return (iterations*2, counter);
    }

    // Funzione per simulare del lavor
    private static void Work(string name)
    {
        for(int i=0; i<= 5; i++)
        {
            Console.WriteLine($"{name}; Thread: {Thread.CurrentThread.ManagedThreadId}; index {i}");
            Thread.Sleep(100);
        }
    }
}