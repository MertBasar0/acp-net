#!/usr/bin/env python3
import json
import os
import sys
import time


def emit(message):
    sys.stdout.write(json.dumps(message, separators=(",", ":")) + "\n")
    sys.stdout.flush()


def log(message):
    sys.stderr.write(f"[mock-agent] {message}\n")
    sys.stderr.flush()


def result(request_id, payload):
    emit({"jsonrpc": "2.0", "id": request_id, "result": payload})


def handle(request):
    method = request.get("method")
    request_id = request.get("id")
    params = request.get("params") or {}

    log(f"method={method} id={request_id}")

    if method == "initialize":
        result(
            request_id,
            {
                "protocolVersion": params.get("protocolVersion", 1),
                "agentCapabilities": {},
                "agentInfo": {"name": "mock-acp-agent", "version": "0.1.0"},
                "authMethods": [],
            },
        )
        return

    if method == "session/new":
        result(request_id, {"sessionId": "mock-session-1"})
        return

    if method == "session/prompt":
        session_id = params.get("sessionId", "mock-session-1")
        emit(
            {
                "jsonrpc": "2.0",
                "method": "session/update",
                "params": {
                    "sessionId": session_id,
                    "update": {
                        "sessionUpdate": "agent_message_chunk",
                        "content": {"type": "text", "text": "stream:"},
                    },
                },
            }
        )
        time.sleep(0.05)
        emit(
            {
                "jsonrpc": "2.0",
                "method": "session/update",
                "params": {
                    "sessionId": session_id,
                    "update": {
                        "sessionUpdate": "agent_message_chunk",
                        "content": {"type": "text", "text": " ok"},
                    },
                },
            }
        )
        result(request_id, {"stopReason": "end_turn"})
        return

    if method == "session/cancel":
        log(f"cancel received for sessionId={params.get('sessionId')}")
        return

    emit(
        {
            "jsonrpc": "2.0",
            "id": request_id,
            "error": {"code": -32601, "message": f"unknown method: {method}"},
        }
    )


def main():
    log(f"pid={os.getpid()}")
    for line in sys.stdin:
        try:
            handle(json.loads(line))
        except Exception as exc:
            log(f"error={exc}")


if __name__ == "__main__":
    main()
