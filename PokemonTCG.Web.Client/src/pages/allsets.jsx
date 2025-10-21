import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
export default function AllSets() {
    const [sets, setSets] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    const API_URL = import.meta.env.VITE_API_URL;   

    useEffect(() => {
        const fetchSets = async () => {
            setLoading(true);
            try {                
                const res = await fetch(`${API_URL}/api/Set/all`);                
                if (!res.ok) {
                    throw new Error(`HTTP ${res.status}`);
                }
                const data = await res.json();                
                setSets(data);
            } catch (err) {
                console.error(err);
                setError("⚠️ The sets couldn't be loaded.");
            } finally {
                setLoading(false);
            }
        };
        fetchSets();
    }, [API_URL]);

    if (loading) return <div className="loading">⏳ Loading...</div>;
    if (error) return <div className="error">{error}</div>;

    return (
        <div className="page-content">
            <div className="set-grid">
                {sets.map((set) => (
                    <table
                        key={set.setId}
                        className="tcg-table pokemon-tcg-text"
                        style={{ marginBottom: "1rem" }}
                    >
                        <tbody>
                            <tr>
                                <td style={{ textAlign: "center", verticalAlign: "middle" }}>
                                    <Link to={`/card/${set.setId}`}>
                                        <img
                                            src={set.logo}
                                            alt={set.name}
                                            className="card-image hover:scale-105 transition-transform duration-300"
                                            style={{
                                                maxWidth: "100%",
                                                height: "auto",
                                            }}
                                        />
                                    </Link>
                                </td>
                            </tr>
                            <tr>
                                <td style={{ textAlign: "center" }}>
                                    <span className="pokemon-tcg-text">{set.name}</span>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                ))}
            </div>
        </div>
    );
}

