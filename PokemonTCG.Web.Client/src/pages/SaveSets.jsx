import React, { useState } from "react";

var ICO_OK   = "\u2705";
var ICO_FAIL = "\u274C";
var ICO_WARN = "\u26A0\uFE0F";

export default function SaveSets() {
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState("");
    const API_URL = import.meta.env.VITE_API_URL || "";

    const handleSave = async () => {
        setLoading(true);
        setMessage("");
        try {
            const r = await fetch(API_URL + "/api/set/create", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
            });
            if (r.ok) setMessage(ICO_OK + " Sets saved successfully!");
            else setMessage(ICO_FAIL + " Error: " + (await r.text()));
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
                    Sync your database with the Pok\u00e9mon TCG world!
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
            </div>
        </div>
    );
}
