import React, { useEffect, useState } from "react";
import "../global.css";
import { Link } from "react-router-dom";

export default function AllSets() {
    const [sets, setSets] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    useEffect(() => {
        const fetchSets = async () => {
            setLoading(true);
            try {
                const res = await fetch("http://localhost:5202/api/sets/getsets");
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                setSets(await res.json());
            } catch (err) {
                console.error(err);
                setError("⚠️ No se pudieron cargar los sets.");
            } finally {
                setLoading(false);
            }
        };
        fetchSets();
    }, []);

    if (loading) return <div className="loading">⏳ Loading...</div>;
    if (error) return <div className="error">{error}</div>;

    return (
        <div className="page-content">            
            <div className="set-grid" >
                {sets.map((set) => (                    
                    <table className="tcg-table pokemon-tcg-text">
                        <tbody>
                            <tr>
                                <td style={{ textAlign: "right", verticalAlign: "center" }}>
                                    <div key={set.setId} className="set-item">
                                        <Link to={`/cards/${set.setId}`}>
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
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td>
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

