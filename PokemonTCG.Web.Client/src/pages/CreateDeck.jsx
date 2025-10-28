import React, { useState, useEffect } from "react";

export default function CreateDeck() {
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [error, setError] = useState("");
    const [cards, setCards] = useState([]);
    const [selectedCards, setSelectedCards] = useState([]);
    const [search, setSearch] = useState("");
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
            // fallback: if object has a single primitive value, use it
            const primitive = Object.values(item).find((v) => typeof v === "string" || typeof v === "number");
            if (primitive != null) return String(primitive);
            return JSON.stringify(item);
        });
    };

    // Cargar filtros (tipos, rarezas, etc.)
    useEffect(() => {
        const fetchFilters = async () => {
            try {
                const res = await fetch(`${API_URL}/api/card/filters`);
                if (!res.ok) throw new Error("Error al obtener filtros");
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
                console.error("Error cargando filtros:", err);
            }
        };

        fetchFilters();
    }, [API_URL]);

    // Cargar sets
    useEffect(() => {
        const fetchSets = async () => {
            try {
                const res = await fetch(`${API_URL}/api/set/all`);
                if (!res.ok) throw new Error("Error al obtener sets");
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
                    setId: setId || "",
                    subtype: subtype || "",
                    type: type || "",
                    supertype: supertype || "",
                    rarity: rarity || "",
                    page: String(page),
                    pageSize: "55",
                });

                const res = await fetch(`${API_URL}/api/card/search?${params.toString()}`);
                if (!res.ok) throw new Error("Error al buscar cartas");
                const data = await res.json();

                setCards(data?.items ?? []);
                const totalCount = Number(data?.totalCount ?? 0);
                const pageSize = Number(data?.pageSize ?? 55);
                setTotalPages(pageSize > 0 ? Math.max(1, Math.ceil(totalCount / pageSize)) : 1);
            } catch (err) {
                console.error("Error cargando cartas:", err);
            }
        };

        fetchCards();
    }, [search, setId, supertype, type, subtype, rarity, page, API_URL]);

    const toggleCard = (cardId) => {
        setSelectedCards((prev) => (prev.includes(cardId) ? prev.filter((id) => id !== cardId) : [...prev, cardId]));
    };

    const handleSubmit = async () => {
        if (!name.trim()) {
            alert("⚠️ Please enter a deck name.");
            return;
        }
        if (selectedCards.length === 0) {
            alert("⚠️ Please select at least one card.");
            return;
        }

        const newDeck = {
            deckId: 0,
            name,
            description,
            cards: selectedCards.map((cardId) => ({ cardId, quantity: 1 })),
        };

        try {
            const res = await fetch(`${API_URL}/api/deck/create`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(newDeck),
            });

            if (!res.ok) {
                const text = await res.text();
                throw new Error(text || "Error creating deck");
            }

            alert("✅ Deck created successfully!");
            setName("");
            setDescription("");
            setSelectedCards([]);
        } catch (err) {
            console.error("Error creando deck:", err);
            alert("❌ Error creating deck. Check console for details.");
        }
    };

    return (
        <div
            className="page-content"
            style={{
                padding: "20px",
                display: "flex",
                flexDirection: "column",
                height: "100vh",
                boxSizing: "border-box",
            }}
        >
            {/* Header y filtros */}
            <div style={{ flexShrink: 0 }}>
                <table width="100%">
                    <tbody>
                        <tr>
                            <td colSpan="6" width="17%" style={{ textAlign: "center" }}>
                                <h2 className="text-blue-700 font-bold text-2xl mb-4">Deck Management</h2>
                            </td>
                        </tr>

                        <tr>
                            <td width="17%"></td>
                            <td width="17%"></td>
                            <td width="17%" style={{ marginBottom: "20px" }}>
                                <label>Name:</label>
                            </td>
                            <td width="17%">
                                <input
                                    type="text"
                                    placeholder="Deck name"
                                    value={name}
                                    onChange={(e) => setName(e.target.value)}
                                    style={{ display: "block", margin: "10px 0", padding: "8px", width: "300px" }}
                                />
                            </td>
                            <td width="17%"></td>
                            <td width="17%"></td>
                        </tr>

                        <tr>
                            <td width="17%"></td>
                            <td width="17%"></td>
                            <td>
                                <label>Description:</label>
                            </td>
                            <td width="17%">
                                <textarea
                                    placeholder="Description"
                                    value={description}
                                    onChange={(e) => setDescription(e.target.value)}
                                    style={{ display: "block", margin: "10px 0", padding: "8px", width: "300px", height: "80px" }}
                                />
                            </td>
                            <td width="17%"></td>
                            <td width="17%"></td>
                        </tr>

                        <tr>
                            <td width="17%">
                                <input
                                    type="text"
                                    placeholder="🔍 Search by name"
                                    value={search}
                                    onChange={(e) => {
                                        setPage(1);
                                        setSearch(e.target.value);
                                    }}
                                    style={{ padding: "8px", width: "200px" }}
                                />
                            </td>

                            <td width="17%">
                                <select
                                    value={setId}
                                    onChange={(e) => {
                                        setPage(1);
                                        setSetId(e.target.value);
                                    }}
                                    style={{ padding: "8px", width: "160px" }}
                                >
                                    <option value="">All Sets</option>
                                    {availableSets.map((s, index) => {
                                        const id = s?.setId ?? s?.id ?? s?.ptcgoCode ?? index;
                                        const name = s?.name ?? s?.setName ?? s?.serie ?? String(id);
                                        return (
                                            <option key={index} value={id}>
                                                {name}
                                            </option>
                                        );
                                    })}
                                </select>
                            </td>

                            <td width="17%">
                                <select
                                    value={supertype}
                                    onChange={(e) => {
                                        setPage(1);
                                        setSuperType(e.target.value);
                                    }}
                                    style={{ padding: "8px", width: "160px" }}
                                >
                                    <option value="">All Super Types</option>
                                    {availableSuperTypes.map((ss, index) => (
                                        <option key={index} value={ss}>
                                            {ss}
                                        </option>
                                    ))}
                                </select>
                            </td>

                            <td width="17%">
                                <select
                                    value={type}
                                    onChange={(e) => {
                                        setPage(1);
                                        setType(e.target.value);
                                    }}
                                    style={{ padding: "8px", width: "160px" }}
                                >
                                    <option value="">All Types</option>
                                    {availableTypes.map((t, index) => (
                                        <option key={index} value={t}>
                                            {t}
                                        </option>
                                    ))}
                                </select>
                            </td>

                            <td width="17%">
                                <select
                                    value={subtype}
                                    onChange={(e) => {
                                        setPage(1);
                                        setSubType(e.target.value);
                                    }}
                                    style={{ padding: "8px", width: "160px" }}
                                >
                                    <option value="">All SubTypes</option>
                                    {availableSubTypes.map((b, index) => (
                                        <option key={index} value={b}>
                                            {b}
                                        </option>
                                    ))}
                                </select>
                            </td>

                            <td width="17%">
                                <select
                                    value={rarity}
                                    onChange={(e) => {
                                        setPage(1);
                                        setRarity(e.target.value);
                                    }}
                                    style={{ padding: "8px", width: "160px" }}
                                >
                                    <option value="">All Rarities</option>
                                    {availableRarities.map((r, index) => (
                                        <option key={index} value={r}>
                                            {r}
                                        </option>
                                    ))}
                                </select>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            {/* Contenedor de cartas con scroll */}
            <div
                style={{
                    flexGrow: 1,
                    overflowY: "auto",
                    paddingRight: "10px",
                    marginTop: "10px",
                    border: "1px solid #ddd",
                    borderRadius: "8px",
                }}
            >
                <div
                    style={{
                        display: "grid",
                        gridTemplateColumns: "repeat(auto-fill, minmax(150px, 1fr))",
                        gap: "10px",
                        padding: "10px",
                    }}
                >
                    {cards.map((card) => (
                        <div
                            key={card?.cardId ?? card?.id ?? card?.number}
                            onClick={() => toggleCard(card?.cardId ?? card?.id ?? card?.number)}
                            style={{
                                border: selectedCards.includes(card?.cardId ?? card?.id ?? card?.number) ? "3px solid #3b82f6" : "1px solid #ccc",
                                borderRadius: "8px",
                                padding: "5px",
                                cursor: "pointer",
                                textAlign: "center",
                                background: "white",
                            }}
                        >
                            <img src={card?.imageLarge || card?.imageUrl} alt={card?.name} style={{ width: "100%", borderRadius: "4px" }} />
                            <p style={{ fontSize: "14px", color: "#333" }}>{card?.name}</p>
                        </div>
                    ))}
                </div>
            </div>

            <div
                style={{
                    position: "fixed",
                    bottom: 0,
                    left: 0,
                    width: "100%",
                    background: "#f9fafb",
                    borderTop: "1px solid #ddd",
                    padding: "15px 30px",
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    zIndex: 1000,
                }}
            >
                <div>
                    <button disabled={page <= 1} onClick={() => setPage((p) => Math.max(1, p - 1))} style={{ marginRight: "10px" }}>
                        ⬅ Prev
                    </button>
                    <span>
                        Page {page} of {totalPages}
                    </span>
                    <button disabled={page >= totalPages} onClick={() => setPage((p) => Math.min(totalPages, p + 1))} style={{ marginLeft: "10px" }}>
                        Next ➡
                    </button>
                </div>

                <button
                    onClick={handleSubmit}
                    style={{
                        padding: "10px 20px",
                        backgroundColor: "#3b82f6",
                        color: "white",
                        border: "none",
                        borderRadius: "8px",
                        cursor: "pointer",
                    }}
                >
                    💾 Save Deck
                </button>
            </div>
        </div>
    );
}
