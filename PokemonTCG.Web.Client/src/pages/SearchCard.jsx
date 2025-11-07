import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import "../global.css";

export default function SearchCard() {
    const API_URL = import.meta.env.VITE_API_URL || "";

    const [availableRarities, setAvailableRarities] = useState([]);
    const [availableSubTypes, setAvailableSubTypes] = useState([]);
    const [availableTypes, setAvailableTypes] = useState([]);
    const [availableSuperTypes, setAvailableSuperTypes] = useState([]);
    const [availableSets, setAvailableSets] = useState([]);

    const [cards, setCards] = useState([]);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [search, setSearch] = useState("");
    const [number, setNumber] = useState(""); // 🔹 NUEVO estado para el filtro Number
    const [setId, setSetId] = useState("");
    const [supertype, setSuperType] = useState("");
    const [type, setType] = useState("");
    const [subtype, setSubType] = useState("");
    const [rarity, setRarity] = useState("");
    const [error, setError] = useState(null);

    const normalizeToStrings = (arr, keysToCheck = []) => {
        if (!Array.isArray(arr)) return [];
        return arr.map((item) => {
            if (item == null) return "";
            if (typeof item === "string") return item;
            for (const k of keysToCheck) {
                if (item[k] && typeof item[k] === "string") return item[k];
            }
            const primitive = Object.values(item).find(
                (v) => typeof v === "string" || typeof v === "number"
            );
            if (primitive != null) return String(primitive);
            return JSON.stringify(item);
        });
    };

    // Cargar filtros
    useEffect(() => {
        const fetchFilters = async () => {
            try {
                const res = await fetch(`${API_URL}/api/card/filters`);
                if (!res.ok) throw new Error("Error to get filters");
                const data = await res.json();

                setAvailableRarities(
                    normalizeToStrings(data?.rarities ?? data?.Rarities ?? [], ["rarity", "name", "value"])
                );
                setAvailableSubTypes(
                    normalizeToStrings(data?.subtypes ?? data?.SubTypes ?? [], ["subtype", "name", "value"])
                );
                setAvailableTypes(
                    normalizeToStrings(data?.types ?? data?.Types ?? [], ["type", "name", "value"])
                );
                setAvailableSuperTypes(
                    normalizeToStrings(
                        data?.supertypes ?? data?.superTypes ?? data?.SuperTypes ?? [],
                        ["supertype", "name", "value"]
                    )
                );
            } catch (err) {
                console.error("Error loading filters:", err);
            }
        };

        fetchFilters();
    }, [API_URL]);

    // Cargar sets
    useEffect(() => {
        const fetchSets = async () => {
            try {
                const res = await fetch(`${API_URL}/api/set/all`);
                if (!res.ok) throw new Error("Error getting sets");
                const data = await res.json();
                setAvailableSets(Array.isArray(data) ? data : []);
            } catch (err) {
                console.error(err);
                setError("⚠️ The sets could not be loaded.");
            }
        };

        fetchSets();
    }, [API_URL]);

    // Buscar cartas (con filtros)
    useEffect(() => {
        const fetchCards = async () => {
            try {
                const params = new URLSearchParams({
                    name: search || "",
                    number: number || "", // 🔹 ahora se envía correctamente al backend
                    setId: setId || "",
                    subtype: subtype || "",
                    type: type || "",
                    supertype: supertype || "",
                    rarity: rarity || "",
                    page: String(page),
                    pageSize: "55",
                });

                const res = await fetch(`${API_URL}/api/card/search?${params.toString()}`);
                if (!res.ok) throw new Error("Error searching for cards");
                const data = await res.json();

                setCards(data?.items ?? []);
                const totalCount = Number(data?.totalCount ?? 0);
                const pageSize = Number(data?.pageSize ?? 55);
                setTotalPages(pageSize > 0 ? Math.max(1, Math.ceil(totalCount / pageSize)) : 1);
            } catch (err) {
                console.error("Error loading cards:", err);
            }
        };

        fetchCards();
    }, [search, number, setId, supertype, type, subtype, rarity, page, API_URL]);

    return (
        <div className="page-content">
            <table
                width="100%"
                className="tcg-table pokemon-tcg-text"
                style={{
                    borderCollapse: "separate",
                    borderSpacing: "0 10px",
                    width: "100%",
                    background: "#f8fafc",
                    borderRadius: "12px",
                    overflow: "hidden",
                    boxShadow: "0 2px 8px rgba(0,0,0,0.06)",
                    padding: "8px",
                }}
            >
                <tbody>
                    <tr>
                        <td colSpan="6" style={{ textAlign: "center", padding: "12px 8px 6px" }}>
                            <h2 style={{ margin: 0, color: "#075985", fontSize: "1.25rem", fontWeight: 700 }}>
                                Search Card
                            </h2>
                        </td>
                    </tr>

                    {/* Filtros */}
                    <tr>
                        <td
                            colSpan="6"
                            style={{
                                padding: "10px 14px",
                                background: "#f8fafc",
                                borderTop: "1px solid #e6edf3",
                            }}
                        >
                            <div
                                style={{
                                    display: "grid",
                                    gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))",
                                    gap: "10px 16px",
                                    alignItems: "center",
                                    justifyContent: "center",
                                }}
                            >
                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label style={{ fontSize: "0.85rem", color: "#334155", marginBottom: "4px", fontWeight: "500" }}>
                                        Number:
                                    </label>
                                    <input
                                        type="text"
                                        placeholder="Search by number"
                                        value={number}
                                        onChange={(e) => {
                                            setPage(1);
                                            setNumber(e.target.value);
                                        }}
                                        style={{
                                            padding: "6px 8px",
                                            width: "220px",
                                            borderRadius: "6px",
                                            border: "1px solid #d1d5db",
                                            fontSize: "0.85rem",
                                        }}
                                    />
                                </div>
                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label style={{ fontSize: "0.85rem", color: "#334155", marginBottom: "4px", fontWeight: "500" }}>
                                        Name:
                                    </label>
                                    <input
                                        type="text"
                                        placeholder="Search by name"
                                        value={search}
                                        onChange={(e) => {
                                            setPage(1);
                                            setSearch(e.target.value);
                                        }}
                                        style={{
                                            padding: "6px 8px",
                                            width: "220px",
                                            borderRadius: "6px",
                                            border: "1px solid #d1d5db",
                                            fontSize: "0.85rem",
                                        }}
                                    />
                                </div>
                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label style={{ fontSize: "0.85rem", color: "#334155", marginBottom: "4px", fontWeight: "500" }}>
                                        Set:
                                    </label>
                                    <select
                                        value={setId}
                                        onChange={(e) => {
                                            setPage(1);
                                            setSetId(e.target.value);
                                        }}
                                        style={{
                                            padding: "6px 8px",
                                            width: "220px",
                                            borderRadius: "6px",
                                            border: "1px solid #d1d5db",
                                            fontSize: "0.85rem",
                                        }}
                                    >
                                        <option value="">All Sets</option>
                                        {availableSets.map((s, i) => {
                                            const id = s?.setId ?? s?.id ?? s?.ptcgoCode ?? i;
                                            const nm = s?.name ?? s?.setName ?? s?.serie ?? String(id);
                                            return (
                                                <option key={i} value={id}>
                                                    {nm}
                                                </option>
                                            );
                                        })}
                                    </select>
                                </div>

                                {/* Super Type */}
                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label style={{ fontSize: "0.85rem", color: "#334155", marginBottom: "4px", fontWeight: "500" }}>
                                        Super Type:
                                    </label>
                                    <select
                                        value={supertype}
                                        onChange={(e) => {
                                            setPage(1);
                                            setSuperType(e.target.value);
                                        }}
                                        style={{
                                            padding: "6px 8px",
                                            width: "220px",
                                            borderRadius: "6px",
                                            border: "1px solid #d1d5db",
                                            fontSize: "0.85rem",
                                        }}
                                    >
                                        <option value="">All Super Types</option>
                                        {availableSuperTypes.map((s, i) => (
                                            <option key={i} value={s}>
                                                {s}
                                            </option>
                                        ))}
                                    </select>
                                </div>

                                {/* Type */}
                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label style={{ fontSize: "0.85rem", color: "#334155", marginBottom: "4px", fontWeight: "500" }}>
                                        Type:
                                    </label>
                                    <select
                                        value={type}
                                        onChange={(e) => {
                                            setPage(1);
                                            setType(e.target.value);
                                        }}
                                        style={{
                                            padding: "6px 8px",
                                            width: "220px",
                                            borderRadius: "6px",
                                            border: "1px solid #d1d5db",
                                            fontSize: "0.85rem",
                                        }}
                                    >
                                        <option value="">All Types</option>
                                        {availableTypes.map((t, i) => (
                                            <option key={i} value={t}>
                                                {t}
                                            </option>
                                        ))}
                                    </select>
                                </div>

                                {/* Sub Type */}
                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label style={{ fontSize: "0.85rem", color: "#334155", marginBottom: "4px", fontWeight: "500" }}>
                                        Sub Type:
                                    </label>
                                    <select
                                        value={subtype}
                                        onChange={(e) => {
                                            setPage(1);
                                            setSubType(e.target.value);
                                        }}
                                        style={{
                                            padding: "6px 8px",
                                            width: "220px",
                                            borderRadius: "6px",
                                            border: "1px solid #d1d5db",
                                            fontSize: "0.85rem",
                                        }}
                                    >
                                        <option value="">All SubTypes</option>
                                        {availableSubTypes.map((b, i) => (
                                            <option key={i} value={b}>
                                                {b}
                                            </option>
                                        ))}
                                    </select>
                                </div>

                                {/* Rarity */}
                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label style={{ fontSize: "0.85rem", color: "#334155", marginBottom: "4px", fontWeight: "500" }}>
                                        Rarity:
                                    </label>
                                    <select
                                        value={rarity}
                                        onChange={(e) => {
                                            setPage(1);
                                            setRarity(e.target.value);
                                        }}
                                        style={{
                                            padding: "6px 8px",
                                            width: "220px",
                                            borderRadius: "6px",
                                            border: "1px solid #d1d5db",
                                            fontSize: "0.85rem",
                                        }}
                                    >
                                        <option value="">All Rarities</option>
                                        {availableRarities.map((r, i) => (
                                            <option key={i} value={r}>
                                                {r}
                                            </option>
                                        ))}
                                    </select>
                                </div>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>

            {/* Cards */}
            <br />
            <div className="all-cards">
                <div className="card-grid-search">
                    {cards.map((card) => (
                        <div key={card.cardId} className="card-item">
                            <Link to={`/card/cardid/${card.cardId}`}>
                                <img src={card.imageLarge} alt={card.name} className="card-image" />
                            </Link>
                        </div>
                    ))}
                </div>
            </div>

            {/* Pagination */}
            <div
                style={{
                    gap: "8px",
                    position: "fixed",
                    bottom: 0,
                    left: 0,
                    width: "100%",
                    background: "#ffffff",
                    borderTop: "2px solid #3b82f6",
                    padding: "3px 0",
                    display: "flex",
                    justifyContent: "center",
                    alignItems: "center",
                    boxShadow: "0 -2px 8px rgba(0,0,0,0.1)",
                    zIndex: 1000,
                }}
            >
                <button
                    disabled={page <= 1}
                    onClick={() => setPage((p) => p - 1)}
                    style={{ padding: "4px 6px", borderRadius: "4px", border: "1px solid #ddd" }}
                >
                    ⬅
                </button>

                <span style={{ fontWeight: "bold", color: "#1f2937" }}>
                    Page {page} of {totalPages}
                </span>

                <button
                    disabled={page >= totalPages}
                    onClick={() => setPage((p) => p + 1)}
                    style={{ padding: "4px 6px", borderRadius: "4px", border: "1px solid #ddd" }}
                >
                    ➡
                </button>
            </div>
        </div>
    );
}

