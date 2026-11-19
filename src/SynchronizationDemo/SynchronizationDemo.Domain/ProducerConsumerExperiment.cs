public class ProducerConsumerExperiment
{
    private const int BufferSize = 4; // la grandezza della tavola in cui mettere i piatti con il cibo
    private const int NProducers = 2; 
    private const int NConsumers = 3; 



    public void Run()
    {
        // Mi serve una struttura dati per posizionare i prodotti creati e consumati 
        Queue<int> buffer = new();

        var spaziDisponibili = new SemaphoreSlim(BufferSize, BufferSize);
        var elementiDisponibili = new SemaphoreSlim(0, BufferSize);
        var accessoTavola = new SemaphoreSlim(1,1);

        Thread[] produttori = new Thread[NProducers];
        Thread[] consumatori = new Thread[NConsumers];

        int nextID = 0;
    
        // TODO: implementazione dei metodi per produrre e per consumare
        void ConsumeSomeProduct(int consumerID)
        {
            
        }

        void ProduceSomeProduct(int producerID)
        {
            // Per sempre, finchè thread non viene killato o fermato per altri motivi di syncronization
            while(true)
            {
                // // per evitare il fatto che più Thread possano creare item identici, devo garantire l'unicità
                int foodItem = Interlocked.Increment(ref nextID); // è come fare la lock nel laboratorio "ThreadDemo"

                //Attendo uno spazio libero nel buffer
                spaziDisponibili.Wait();

                try
                {
                    // Se ho questo permesso, ovvero posso accedere alla tavola, allora riempio un posto della tavola
                    try
                    {
                        accessoTavola.Wait();  
                        buffer.Enqueue(foodItem); //aggiungo un elemento alla mia coda di produzione (tavola)
                        // Interlocked.Increment(ref nextID)
                        Console.WriteLine($"ProducerID: {producerID}, Item: {foodItem}, Buffer count: {buffer.Count}");
                        accessoTavola.Release();
                    }
                    finally
                    {
                        accessoTavola.Release();
                    }
                }
                catch
                {
                    spaziDisponibili.Release();
                }

                // Segnalare che è possibile d'ora in poi poter accedere a nuovi foodItens ai consumatori.
                elementiDisponibili.Release();
                Thread.Sleep(Random.Shared.Next(100, 250)); // i producer aspetteranno tempi diversi. 
            }
        }

        for(int i = 1; i < NProducers; i++)
        {
            // i corrisponde all'id del thread corrente 
            produttori[i] = new Thread(() => ProduceSomeProduct(i));
            produttori[i].Start();
        }
    
        for(int i = 1; i < NConsumers; i++)
        {
            // i corrisponde all'id del thread corrente 
            consumatori[i] = new Thread(() => ConsumeSomeProduct(i));
            consumatori[i].Start();
        }

        // Attendo terminazione dei consumatori e dei produttori
        foreach(Thread consumatore in consumatori)
        {
            consumatore.Join();
        }
    
        foreach(Thread produttore in consumatori)
        {
            produttore.Join();
        }
    }   
}