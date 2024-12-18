using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication;
public interface IMalarkeyStateProvider
{
    Task<string> ProduceNewState();
    Task<MalarkeyAuthenticationSession?> SessionFor(string state);

    Task<MalarkeyAuthenticationSession> Save(MalarkeyAuthenticationSession session);
}

internal class MalarkeyDummyStateProvider : IMalarkeyStateProvider
{
    private readonly IDictionary<string, MalarkeyAuthenticationSession> _sessions = new Dictionary<string, MalarkeyAuthenticationSession>();
    private static long _currentId = 0L;
    private static object _lock = new object();
    private static long NextId()
    {
        lock (_lock)
        {
            return _currentId++;
        }
    }

    public Task<string> ProduceNewState() => Task.FromResult(
        Guid.NewGuid().ToString() 
    );
        

    public async Task<MalarkeyAuthenticationSession> Save(MalarkeyAuthenticationSession session)
    {
        await Task.CompletedTask;
        var insertee = new MalarkeyAuthenticationSession(
                SessionId: NextId(),
                State: session.State,
                Forwarder: session.Forwarder,
                CodeChallenge: session.CodeChallenge,
                CodeVerifier: session.CodeVerifier
            );
        _sessions[session.State] = session;
        return insertee;
            
    }

    public async Task<MalarkeyAuthenticationSession?> SessionFor(string state) => _sessions.TryGetValue(state, out var session) ? session : null;

}
