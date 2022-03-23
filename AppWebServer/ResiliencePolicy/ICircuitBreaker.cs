using Polly.CircuitBreaker;
using Polly.Wrap;

public interface ICircuitBreaker
{
    public AsyncPolicyWrap GetInstance();
}