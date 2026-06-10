namespace AcpNet.Testing;

public static class FakeAcpAgentScript
{
    public static string WriteDefault(string directory)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "fake_acp_agent.py");
        File.WriteAllText(path, DefaultScript);
        return path;
    }

    public static string WriteHanging(string directory)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "fake_acp_agent_hanging.py");
        File.WriteAllText(path, HangingScript);
        return path;
    }

    const string DefaultScript = """
#!/usr/bin/env python3
import json
import os
import sys
import time

def emit(message):
    sys.stdout.write(json.dumps(message, separators=(",", ":")) + "\n")
    sys.stdout.flush()

def log(message):
    sys.stderr.write(f"[fake-acp-agent] {message}\n")
    sys.stderr.flush()

def result(request_id, payload):
    emit({"jsonrpc": "2.0", "id": request_id, "result": payload})

def handle(request):
    method = request.get("method")
    request_id = request.get("id")
    params = request.get("params") or {}
    log(f"method={method} id={request_id}")

    if method == "initialize":
        result(request_id, {
            "protocolVersion": params.get("protocolVersion", 1),
            "agentCapabilities": {},
            "agentInfo": {"name": "fake-acp-agent", "version": "0.1.0"},
            "authMethods": []
        })
        return

    if method == "session/new":
        result(request_id, {"sessionId": "fake-session-1"})
        return

    if method == "session/prompt":
        session_id = params.get("sessionId", "fake-session-1")
        for chunk in ["hello", " world"]:
            emit({
                "jsonrpc": "2.0",
                "method": "session/update",
                "params": {
                    "sessionId": session_id,
                    "update": {
                        "sessionUpdate": "agent_message_chunk",
                        "content": {"type": "text", "text": chunk}
                    }
                }
            })
            time.sleep(0.02)
        result(request_id, {"stopReason": "end_turn"})
        return

    if method == "session/cancel":
        log(f"cancel received sessionId={params.get('sessionId')}")
        return

    emit({
        "jsonrpc": "2.0",
        "id": request_id,
        "error": {"code": -32601, "message": f"unknown method: {method}"}
    })

def main():
    log(f"pid={os.getpid()}")
    for line in sys.stdin:
        handle(json.loads(line))

if __name__ == "__main__":
    main()
""";

    const string HangingScript = """
#!/usr/bin/env python3
import os
import sys
import time

sys.stderr.write(f"[fake-acp-agent-hanging] pid={os.getpid()}\n")
sys.stderr.flush()

while True:
    time.sleep(1)
""";
}
