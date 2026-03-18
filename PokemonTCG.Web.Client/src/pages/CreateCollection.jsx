import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import "../global.css";

export default function CreateCollection() {
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [error, setError] = useState("");
    const [message, setMessage] = useState("");
    const [cards, setCards] = useState([]);
    const [selectedCards, setSelectedCards] = useState([]);
    const [search, setSearch] = useState("");
    const [number, setNumber] = useState("");
    const [setId, setSetId] = useState("");
    const [subtype, setSubType] = useState("");
    const [type, setType] = useState("");
    const [supertype, setSuperType] = useState("");
    const [rarity, setRarity] = useState("");
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [availableSets, setAvailableSets] = useState([]);
    const [availableSuperTypes, setAvailableSuperTypes] = useState([]);
    const [availableTypes, setAvailableTypes] = useState([]);
    const [availableSubTypes, setAvailableSubTypes] = useState([]);
    const [availableRarities, setAvailableRarities] = useState([]);

    const API_URL = import.meta.env.VITE_API_URL || "";

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

    useEffect(() => {
        const fetchSets = async () => {
            try {
                const res = await fetch(`${API_URL}/api/set/all`);
                if (!res.ok) throw new Error("Error getting sets");
                const data = await res.json();
                setAvailableSets(Array.isArray(data) ? data : []);
            } catch (err) {
                console.error(err);
                setError("?? The sets could not be loaded.");
            }
        };

        fetchSets();
    }, [API_URL]);

    useEffect(() => {
        const fetchCards = async () => {
            try {
                const params = new URLSearchParams();

                if (search) params.append("name", search);
                if (number) params.append("number", number);
                if (setId) params.append("setId", setId);
                if (subtype) params.append("subtype", subtype);
                if (type) params.append("type", type);
                if (supertype) params.append("supertype", supertype);
                if (rarity) params.append("rarity", rarity);
                params.append("page", String(page));
                params.append("pageSize", "55");

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

    const getTotalSelected = (arr) =>
        arr.reduce((s, c) => s + (Number(c.quantity) || 0), 0);

    const toggleCard = (cardOrId) => {
        const id = typeof cardOrId === "string" ? cardOrId : cardOrId.cardId;
        const cardObj = typeof cardOrId === "object"
            ? cardOrId
            : cards.find((c) => c.cardId === id) || {};

        setSelectedCards((prev) => {
            const found = prev.find((c) => c.cardId === id);

            if (found) {
                return prev.map((c) =>
                    c.cardId === id ? { ...c, quantity: c.quantity + 1 } : c
                );
            }

            return [
                ...prev,
                {
                    cardId: id,
                    name: cardObj?.name ?? "Unknown",
                    supertype: cardObj?.supertype ?? cardObj?.superType ?? "",
                    subtype: cardObj?.subtype ?? cardObj?.subType ?? "",
                    quantity: 1,
                    ptcgocode: cardObj?.ptcgoCode ?? cardObj?.ptcgocode ?? "",
                    number: cardObj?.number ?? "",
                    setName: cardObj?.setName ?? "",
                    setId: cardObj?.setId ?? "",
                    imageLarge:
                        cardObj?.imageLarge ??
                        cardObj?.imageUrl ??
                        (cardObj?.images && (cardObj.images.large ?? cardObj.images.small)) ??
                        "",
                },
            ];
        });
    };

    const removeCard = (cardId) => {
        setSelectedCards((prev) => {
            const found = prev.find((c) => c.cardId === cardId);
            if (!found) return prev;
            if (found.quantity > 1) {
                return prev.map((c) =>
                    c.cardId === cardId ? { ...c, quantity: c.quantity - 1 } : c
                );
            }
            return prev.filter((c) => c.cardId !== cardId);
        });
    };

    const deleteCard = (cardId) => {
        setSelectedCards((prev) => prev.filter((c) => c.cardId !== cardId));
    };

    const clearSelection = () => {
        setName("");
        setDescription("");
        setError("");
        setMessage("");
        setSelectedCards([]);
    };

    const handleSubmit = async () => {
        if (!name.trim()) {
            setMessage("?? Please enter a collection name.");
            return;
        }

        if (!selectedCards || selectedCards.length === 0) {
            setMessage("?? Please select at least one card for the collection.");
            return;
        }

        const payloadCards = selectedCards.map((item) => ({
            cardId: String(item.cardId),
            quantity: Number(item.quantity),
        }));

        const newCollection = {
            name,
            description,
            cards: payloadCards,
        };

        try {
            const res = await fetch(`${API_URL}/api/collection/create`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(newCollection),
            });

            if (!res.ok) {
                const text = await res.text();
                throw new Error(text || "Error creating collection");
            }

            setMessage(`? Collection "${name}" created successfully!`);
            setName("");
            setDescription("");
            setSelectedCards([]);
        } catch (err) {
            console.error("Error creating collection:", err);
            setMessage("? Error creating collection. Check console for details.");
        }
    };

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
                                Collection Builder
                            </h2>
                        </td>
                    </tr>

                    <tr>
                        <td
                            colSpan="2"
                            style={{
                                verticalAlign: "top",
                                padding: "12px",
                                background: "white",
                                borderRight: "1px solid #e6edf3",
                            }}
                        >
                            <div style={{ display: "flex", flexDirection: "column", gap: "10px" }}>
                                <div
                                    style={{
                                        display: "flex",
                                        flexDirection: "column",
                                        gap: "10px",
                                        width: "100%",
                                        maxWidth: "800px",
                                        margin: "0 auto",
                                    }}
                                >
                                    <div
                                        style={{
                                            display: "grid",
                                            gridTemplateColumns: "100px 1fr",
                                            alignItems: "center",
                                            gap: "10px",
                                        }}
                                    >
                                        <label
                                            style={{
                                                fontSize: "0.85rem",
                                                color: "#334155",
                                                fontWeight: "500",
                                                textAlign: "right",
                                            }}
                                        >
                                            Name:
                                        </label>
                                        <input
                                            type="text"
                                            placeholder="Enter collection name"
                                            value={name}
                                            onChange={(e) => setName(e.target.value)}
                                            style={{
                                                width: "100%",
                                                padding: "8px 10px",
                                                borderRadius: "8px",
                                                border: "1px solid #d1d5db",
                                                fontSize: "0.9rem",
                                            }}
                                        />
                                    </div>
                                    <div
                                        style={{
                                            display: "grid",
                                            gridTemplateColumns: "100px 1fr",
                                            alignItems: "start",
                                            gap: "10px",
                                        }}
                                    >
                                        <label
                                            style={{
                                                fontSize: "0.85rem",
                                                color: "#334155",
                                                fontWeight: "500",
                                                textAlign: "right",
                                                paddingTop: "4px",
                                            }}
                                        >
                                            Description:
                                        </label>
                                        <textarea
                                            placeholder="Describe your collection..."
                                            value={description}
                                            onChange={(e) => setDescription(e.target.value)}
                                            style={{
                                                width: "100%",
                                                height: "76px",
                                                padding: "8px 10px",
                                                borderRadius: "8px",
                                                border: "1px solid #d1d5db",
                                                fontSize: "0.9rem",
                                                resize: "vertical",
                                            }}
                                        />
                                    </div>
                                </div>
                                <div style={{ display: "flex", gap: "8px", alignItems: "center", marginTop: "6px" }}>
                                    <button
                                        onClick={clearSelection}
                                        style={{
                                            marginLeft: "auto",
                                            padding: "6px 10px",
                                            borderRadius: "6px",
                                            border: "none",
                                            background: "#10b981",
                                            color: "white",
                                            cursor: "pointer",
                                            fontSize: "0.9rem",
                                            fontWeight: 600,
                                        }}
                                    >
                                        Clear
                                    </button>

                                    <button
                                        onClick={handleSubmit}
                                        style={{
                                            marginLeft: "auto",
                                            padding: "6px 10px",
                                            borderRadius: "6px",
                                            border: "none",
                                            background: "#10b981",
                                            color: "white",
                                            cursor: "pointer",
                                            fontSize: "0.9rem",
                                            fontWeight: 600,
                                        }}
                                    >
                                        Save Collection
                                    </button>
                                </div>
                            </div>
                        </td>
                        <td
                            colSpan="4"
                            rowSpan="2"
                            style={{
                                verticalAlign: "top",
                                padding: "12px",
                                background: "white",
                            }}
                        >
                            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "8px" }}>
                                <h3 style={{ margin: 0, color: "#1e40af", fontSize: "1rem", fontWeight: 700 }}>Collection Cards</h3>
                                <span style={{ fontSize: "0.95rem", color: "#334155" }}>
                                    Unique: <strong>{selectedCards.length}</strong> — Total: <strong>{getTotalSelected(selectedCards)}</strong>
                                </span>
                            </div>

                            <div
                                style={{
                                    border: "1px solid #e6edf3",
                                    borderRadius: "8px",
                                    maxHeight: "320px",
                                    overflowY: "auto",
                                    padding: "6px",
                                    background: "#fbfdff",
                                }}
                            >
                                {selectedCards.length === 0 ? (
                                    <p style={{ textAlign: "center", color: "#94a3b8", margin: "14px 0" }}>No cards selected</p>
                                ) : (
                                    <ul style={{ listStyle: "none", padding: 0, margin: 0 }}>
                                        {selectedCards.map((c) => (
                                            <li
                                                key={c.cardId}
                                                style={{
                                                    display: "flex",
                                                    justifyContent: "space-between",
                                                    alignItems: "center",
                                                    padding: "8px",
                                                    borderBottom: "1px solid #f1f5f9",
                                                    gap: "8px",
                                                }}
                                            >
                                                <div style={{ display: "flex", gap: "10px", alignItems: "center", minWidth: 0 }}>
                                                    {c.imageLarge ? (
                                                        <img
                                                            src={c.imageLarge}
                                                            alt={c.name}
                                                            style={{ width: "44px", height: "62px", objectFit: "cover", borderRadius: "6px", flexShrink: 0 }}
                                                        />
                                                    ) : null}
                                                    <div
                                                        style={{
                                                            minWidth: 0,
                                                            display: "flex",
                                                            flexDirection: "column",
                                                            alignItems: "flex-start",
                                                            textAlign: "left",
                                                        }}
                                                    >
                                                        <div
                                                            style={{
                                                                fontWeight: 600,
                                                                fontSize: "0.95rem",
                                                                color: "#0f172a",
                                                                whiteSpace: "nowrap",
                                                                overflow: "hidden",
                                                                textOverflow: "ellipsis",
                                                                maxWidth: "280px",
                                                                width: "100%",
                                                            }}
                                                        >
                                                            {c.name}
                                                        </div>
                                                        <div
                                                            style={{
                                                                color: "#6b7280",
                                                                fontSize: "0.8rem",
                                                                width: "100%",
                                                                textAlign: "left",
                                                                whiteSpace: "nowrap",
                                                                overflow: "hidden",
                                                                textOverflow: "ellipsis",
                                                                maxWidth: "280px",
                                                            }}
                                                        >
                                                            {c.supertype ?? "—"} {c.number ? ` • #${c.number}` : ""}{" "}
                                                            {c.setName ? ` • ${c.setName}` : ""}
                                                        </div>
                                                    </div>
                                                </div>

                                                <div style={{ display: "flex", alignItems: "center", gap: "6px" }}>
                                                    <span style={{ fontWeight: 700, color: "#2563eb", minWidth: "28px", textAlign: "center" }}>x{c.quantity}</span>

                                                    <div style={{ display: "flex", gap: "6px" }}>
                                                        <button
                                                            onClick={() => removeCard(c.cardId)}
                                                            style={{
                                                                padding: "4px 6px",
                                                                borderRadius: "6px",
                                                                border: "1px solid #e6edf3",
                                                                background: "#ffffff",
                                                                cursor: "pointer",
                                                                fontSize: "0.85rem",
                                                            }}
                                                            title="Remove one"
                                                        >
                                                            ?
                                                        </button>

                                                        <button
                                                            onClick={() => toggleCard(c)}
                                                            style={{
                                                                padding: "4px 6px",
                                                                borderRadius: "6px",
                                                                border: "1px solid #e6edf3",
                                                                background: "#ffffff",
                                                                cursor: "pointer",
                                                                fontSize: "0.85rem",
                                                            }}
                                                            title="Add one"
                                                        >
                                                            +
                                                        </button>

                                                        <button
                                                            onClick={() => deleteCard(c.cardId)}
                                                            style={{
                                                                padding: "4px 6px",
                                                                borderRadius: "6px",
                                                                border: "1px solid #fee2e2",
                                                                background: "#fff5f5",
                                                                color: "#b91c1c",
                                                                cursor: "pointer",
                                                                fontSize: "0.85rem",
                                                            }}
                                                            title="Remove all"
                                                        >
                                                            ?
                                                        </button>
                                                    </div>
                                                </div>
                                            </li>
                                        ))}
                                    </ul>
                                )}
                            </div>
                        </td>
                    </tr>

                    <tr>
                        <td style={{ height: "6px", background: "transparent" }}></td>
                        <td></td>
                    </tr>

                    {/* Filters */}
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
                                        onChange={(e) => { setPage(1); setNumber(e.target.value); }}
                                        style={{ padding: "6px 8px", width: "220px", borderRadius: "6px", border: "1px solid #d1d5db", fontSize: "0.85rem" }}
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
                                        onChange={(e) => { setPage(1); setSearch(e.target.value); }}
                                        style={{ padding: "6px 8px", width: "220px", borderRadius: "6px", border: "1px solid #d1d5db", fontSize: "0.85rem" }}
                                    />
                                </div>

                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label style={{ fontSize: "0.85rem", color: "#334155", marginBottom: "4px", fontWeight: "500" }}>
                                        Set:
                                    </label>
                                    <select
                                        value={setId}
                                        onChange={(e) => { setPage(1); setSetId(e.target.value); }}
                                        style={{ padding: "6px 8px", width: "220px", borderRadius: "6px", border: "1px solid #d1d5db", fontSize: "0.85rem" }}
                                    >
                                        <option value="">All Sets</option>
                                        {availableSets.map((s, i) => {
                                            const id = s?.setId ?? s?.id ?? s?.ptcgoCode ?? i;
                                            const nm = s?.name ?? s?.setName ?? s?.serie ?? String(id);
                                            return (<option key={i} value={id}>{nm}</option>);
                                        })}
                                    </select>
                                </div>

                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label style={{ fontSize: "0.85rem", color: "#334155", marginBottom: "4px", fontWeight: "500" }}>
                                        Super Type:
                                    </label>
                                    <select
                                        value={supertype}
                                        onChange={(e) => { setPage(1); setSuperType(e.target.value); }}
                                        style={{ padding: "6px 8px", width: "220px", borderRadius: "6px", border: "1px solid #d1d5db", fontSize: "0.85rem" }}
                                    >
                                        <option value="">All Super Types</option>
                                        {availableSuperTypes.map((s, i) => (<option key={i} value={s}>{s}</option>))}
                                    </select>
                                </div>

                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label style={{ fontSize: "0.85rem", color: "#334155", marginBottom: "4px", fontWeight: "500" }}>
                                        Type:
                                    </label>
                                    <select
                                        value={type}
                                        onChange={(e) => { setPage(1); setType(e.target.value); }}
                                        style={{ padding: "6px 8px", width: "220px", borderRadius: "6px", border: "1px solid #d1d5db", fontSize: "0.85rem" }}
                                    >
                                        <option value="">All Types</option>
                                        {availableTypes.map((t, i) => (<option key={i} value={t}>{t}</option>))}
                                    </select>
                                </div>

                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label style={{ fontSize: "0.85rem", color: "#334155", marginBottom: "4px", fontWeight: "500" }}>
                                        Sub Type:
                                    </label>
                                    <select
                                        value={subtype}
                                        onChange={(e) => { setPage(1); setSubType(e.target.value); }}
                                        style={{ padding: "6px 8px", width: "220px", borderRadius: "6px", border: "1px solid #d1d5db", fontSize: "0.85rem" }}
                                    >
                                        <option value="">All SubTypes</option>
                                        {availableSubTypes.map((b, i) => (<option key={i} value={b}>{b}</option>))}
                                    </select>
                                </div>

                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label style={{ fontSize: "0.85rem", color: "#334155", marginBottom: "4px", fontWeight: "500" }}>
                                        Rarity:
                                    </label>
                                    <select
                                        value={rarity}
                                        onChange={(e) => { setPage(1); setRarity(e.target.value); }}
                                        style={{ padding: "6px 8px", width: "220px", borderRadius: "6px", border: "1px solid #d1d5db", fontSize: "0.85rem" }}
                                    >
                                        <option value="">All Rarities</option>
                                        {availableRarities.map((r, i) => (<option key={i} value={r}>{r}</option>))}
                                    </select>
                                </div>
                            </div>
                        </td>
                    </tr>

                    <tr>
                        <td colSpan="6" style={{ textAlign: "center", padding: "12px 8px 6px" }}>
                            {message && (
                                <p
                                    style={{
                                        textAlign: "center",
                                        fontWeight: "bold",
                                        marginTop: "10px",
                                        marginBottom: "10px",
                                        color: message.includes("?")
                                            ? "green"
                                            : message.includes("?")
                                                ? "red"
                                                : message.includes("??")
                                                    ? "orange"
                                                    : "blue",
                                    }}
                                >
                                    {message}
                                </p>
                            )}
                        </td>
                    </tr>
                </tbody>
            </table>

            {/* Cards Grid */}
            <br />
            <div className="all-cards">
                <div className="card-grid-search">
                    {cards.map((card) => (
                        <div
                            key={card.cardId}
                            className="card-item"
                            onClick={() => toggleCard(card.cardId)}
                            style={{ position: "relative", cursor: "pointer" }}
                        >
                            <img src={card.imageLarge} alt={card.name} className="card-image" />

                            {selectedCards.some((c) => c.cardId === card.cardId) && (
                                <div
                                    style={{
                                        position: "absolute",
                                        top: "4px",
                                        right: "6px",
                                        background: "rgba(0,0,0,0.7)",
                                        color: "white",
                                        padding: "2px 6px",
                                        borderRadius: "8px",
                                        fontSize: "0.8rem",
                                    }}
                                >
                                    ×{selectedCards.find((c) => c.cardId === card.cardId)?.quantity}
                                </div>
                            )}
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
                    ?
                </button>

                <span style={{ fontWeight: "bold", color: "#1f2937" }}>
                    Page {page} of {totalPages}
                </span>

                <button
                    disabled={page >= totalPages}
                    onClick={() => setPage((p) => p + 1)}
                    style={{ padding: "4px 6px", borderRadius: "4px", border: "1px solid #ddd" }}
                >
                    ?
                </button>
            </div>
        </div>
    );
}
