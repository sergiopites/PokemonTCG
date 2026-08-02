import React, { useState, useRef } from "react";

var ICO_OK   = "\u2705";
var ICO_FAIL = "\u274C";
var ICO_WARN = "\u26A0\uFE0F";

export default function SaveSets() {
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState("");
    const [logs, setLogs] = useState([]);
    const logRef = useRef(null);
    const API_URL = import.meta.env.VITE_API_URL || "";

    const handleSave = async () => {
        setLoading(true);
        setMessage("");
        setLogs([]);
        try {
            const r = await fetch(API_URL + "/api/set/create/progress", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
            });
            if (!r.ok) { setMessage(ICO_FAIL + " Error: " + (await r.text())); setLoading(false); return; }
            const reader = r.body.getReader();
            const decoder = new TextDecoder();
            let buf = "";
            while (true) {
                const { value, done } = await reader.read();
                if (done) break;
                buf += decoder.decode(value, { stream: true });
                const lines = buf.split("\n");
                buf = lines.pop();
                for (const line of lines) {
                    if (!line.startsWith("data: ")) continue;
                    try {
                        const evt = JSON.parse(line.slice(6));
                        if (evt.log) {
                            setLogs(prev => [...prev, evt.log]);
                            if (logRef.current) logRef.current.scrollTop = logRef.current.scrollHeight;
                        }
                        if (evt.done) setMessage(ICO_OK + " Sets synced successfully!");
                        if (evt.error) setMessage(ICO_FAIL + " Error: " + evt.error);
                    } catch (pe) { /* ignore parse errors */ }
                }
            }
            if (!message) setMessage(ICO_OK + " Sets synced successfully!");
        } catch (e) {
            console.error(e);
            setMessage(ICO_WARN + " Request failed, check if the API is running.");
        } finally {
            setLoading(false);
        }
    };

    const msgClass = message.indexOf(ICO_OK) >= 0 ? "msg-success" : message.indexOf(ICO_FAIL) >= 0 ? "msg-error" : "msg-warning";

    return (
        <div className="page-content-v2">
            <div className="admin-panel">
                <h2>Sets Management</h2>
                <p>
                    Sync your database with the Pok&eacute;mon TCG world!
                    Press the button to fetch and save the latest sets from the official API.
                </p>
                <button className="btn btn-gold" onClick={handleSave} disabled={loading}>
                    {loading ? "Syncing\u2026" : "\u26A1 Sync Sets"}
                </button>
                {message && (
                    <p className={msgClass} style={{ marginTop: 16 }}>
                        {message}
                    </p>
                )}
                {logs.length > 0 && (
                    <div ref={logRef} className="sync-log">
                        {logs.map((l, i) => <div key={i} className="sync-log-line">{l}</div>)}
                    </div>
                )}
            </div>
        </div>
    );
}
