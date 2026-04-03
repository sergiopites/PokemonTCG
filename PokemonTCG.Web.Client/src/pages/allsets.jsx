import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

var ICO_WARN = "\u26A0\uFE0F";

export default function AllSets() {
    const [sets, setSets] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    const API_URL = import.meta.env.VITE_API_URL;

    useEffect(() => {
        const fetchSets = async () => {
            setLoading(true);
            try {
                const res = await fetch(API_URL + "/api/Set/all");
                if (!res.ok) throw new Error("HTTP " + res.status);
                const data = await res.json();
                setSets(data);
            } catch (err) {
                console.error(err);
                setError(ICO_WARN + " The sets couldn't be loaded.");
            } finally {
                setLoading(false);
            }
        };
        fetchSets();
    }, [API_URL]);

    if (loading) return (
        <div className="loading-screen">
            <div className="pokeball-spinner" />
            <span className="loading-text">Loading Sets\u2026</span>
        </div>
    );

    if (error) return (
        <div className="page-content-v2">
            <div className="msg-error">{error}</div>
        </div>
    );

    return (
        <div className="page-content-v2">
            <h1 className="section-title">All Sets</h1>
            <div className="set-grid-v2">
                {sets.map((set) => (
                    <Link key={set.setId} to={"/card/" + set.setId} style={{ textDecoration: "none" }}>
                        <div className="set-tile">
                            <img src={set.logo} alt={set.name} />
                            <div className="set-tile-name">{set.name}</div>
                        </div>
                    </Link>
                ))}
            </div>
        </div>
    );
}
