/* eslint-disable react/prop-types */
import React, { useState } from "react";

export default function SaveSets() {
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState("");
    const API_URL = import.meta.env.VITE_API_URL || "";

    const handleSave = async () => {
        setLoading(true);
        setMessage("");
        try {
            const url = `${API_URL}/api/set/create`;
            const response = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                }
            });

            console.log("Response status:", response.status);

            if (response.ok) {
                setMessage("✅ Sets saved successfully!");
            } else {
                const errorText = await response.text();
                setMessage(`❌ Error: ${errorText}`);
            }
        } catch (error) {
            console.error(error);
            setMessage("⚠️ Request failed, check if the API is running.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="page-content">
            <table border="0" className="tcg-table pokemon-tcg-text" width="100%">
                <tbody>
                    <tr>
                        <td style={{ textAlign: "center", verticalAlign: "middle" }}>
                            <h2>Sets Management</h2>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Sync your database with the Pokémon TCG world!
                            When you press the button, the latest sets will be fetched and saved from the official Pokémon TCG API.
                        </td>
                    </tr>
                    <tr>                    
                        <td>
                            <button onClick={handleSave} disabled={loading}>
                                {loading ? "Updating..." : "Save Sets"}
                            </button>

                            {message && (
                                <p
                                    className={
                                        message.includes("✅")
                                            ? "success"
                                            : message.includes("❌")
                                                ? "error"
                                                : "warning"
                                    }
                                >
                                    {message}
                                </p>
                            )}
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    );
}