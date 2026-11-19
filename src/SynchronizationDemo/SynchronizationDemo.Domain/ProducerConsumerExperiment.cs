public class ProducerConsumerExperiment
{
    private const int BufferSize = 4; // la grandezza della tavola in cui mettere i piatti con il cibo
    private const int NProducers = 2; 
    private const int NConsumers = 3; 

    public void Run()
    {
        var producersSemaphore = new SemaphoreSlim(BufferSize, BufferSize);
    } 
    
}