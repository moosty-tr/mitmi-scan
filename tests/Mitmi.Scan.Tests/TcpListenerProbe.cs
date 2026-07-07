using System.Net;
using System.Net.Sockets;

namespace Mitmi.Scan.Tests;

internal sealed class TcpListenerProbe : IDisposable
{
    private readonly TcpListener _listener;

    private TcpListenerProbe(TcpListener listener)
    {
        _listener = listener;
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
    }

    public int Port { get; }

    public static TcpListenerProbe Start()
    {
        TcpListener listener = new(IPAddress.Loopback, port: 0);
        listener.Start();
        return new TcpListenerProbe(listener);
    }

    public void Dispose()
    {
        _listener.Stop();
    }
}
