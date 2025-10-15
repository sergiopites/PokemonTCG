import React, { useState } from "react";

export default function SaveCards() {
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState("");

    const handleSave = async () => {
        setLoading(true);
        setMessage("");
        try {
            const response = await fetch("http://localhost:5202/api/Cards/addpokemoncards", {
                method: "POST"
            });


            console.log("Response status:", response.status);

            if (response.ok) {
                setMessage("✅ Pokemon cards saved successfully!");
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
                            <h2>Cards Management</h2>
                        </td>
                    </tr>
                    <tr>
                        <td>Sync your database with the Pokémon TCG world!
                            When you press the button, the latest cards will be fetched and saved from the official Pokémon TCG API.
                        </td></tr>
                    <tr>
                        <td>
                            <button onClick={handleSave} disabled={loading}>
                                {loading ? "Updating..." : "Save Cards"}
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



