using System.Runtime.InteropServices;

public class ProducerConsumerExperiment
{
    private const int BufferSize = 4; // la grandezza della tavola in cui mettere i piatti con il cibo
    private const int NProducers = 2; 
    private const int NConsumers = 2; 



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
        int articoliConsumati = 0;
        int articoliProdotti = 0; 


        // Satrt dei consumatori e produttori 
        for(int i = 1; i < NConsumers; i++)
        {
            int id = i + 1; 
            // i corrisponde all'id del thread corrente 
            consumatori[i] = new Thread(() => ConsumeSomeProduct(
                id,
                buffer,
                spaziDisponibili,
                elementiDisponibili,
                accessoTavola,
                ref nextID, // passaggio per riferimento (in memoria), anziché per copia
                ref articoliProdotti
            ));
            consumatori[i].Start();
        }

        for(int i = 1; i < NProducers; i++)
        {
            int id = i + 1;
            // i corrisponde all'id del thread corrente 
            produttori[i] = new Thread(() => ProduceSomeProduct(
                id,
                buffer,
                spaziDisponibili,
                elementiDisponibili,
                accessoTavola,
                ref nextID // passaggio per riferimento (in memoria), anziché per copia
                ref articoliConsumati 
            ));
            produttori[i].Start();
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

    private void ProduceSomeProduct(
        int id,
        Queue<int> buffer, 
        SemaphoreSlim spaziDisponibili,
        SemaphoreSlim elementiDisponibili,
        SemaphoreSlim accessoTavola,
        ref int nextID)
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
                    Interlocked.Increment(ref nextID);
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

    private void ConsumeSomeProduct(
        int id, 
        Queue<int> buffer, 
        SemaphoreSlim spaziDisponibili, 
        SemaphoreSlim elementiDisponibili, 
        SemaphoreSlim accessoTavola, 
        ref int nextID)
    {
        // voglio sempre consumare finché posso
        while (true)
        {
            // evita di accedere alla coda quando è vuota 
            elementiDisponibili.Wait();

            // rendo mutualmente esclusivo l'accesso alla tavola
            accessoTavola.Wait();

            // tentare di prelevare dalla coda (tavola) un elemento (numero, in un caso reale può essere una brioche)
            try
            {
                int item = buffer.Dequeue();
                Console.WriteLine($"Consumato elemento: {item} da consumerId: {id}");
            }
            finally
            {
                // rilascio l'accesso alla tavola dopo aver consumato.
                accessoTavola.Release();
            }

            // rilascio semaforo dopo aver eventualmente consumato. 
            spaziDisponibili.Release();
            Thread.Sleep(Random.Shared.Next(100, 250)); // i consumer aspettano tempi diversi. 

            Console.WriteLine($"consumerId: {id}, termina");
        }
    }
}