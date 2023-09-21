namespace RqliteNet.Orleans.Example.Interfaces;

public interface IHello : IGrainWithIntegerKey
{
    Task<string> SayHello(string greeting);
}