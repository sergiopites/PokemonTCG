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

    // 🔹 Opciones de filtros
    const [availableSets, setAvailableSets] = useState([]);
    const [availableSuperTypes, setAvailableSuperTypes] = useState([]);
    const [availableTypes, setAvailableTypes] = useState([]);
    const [availableSubTypes, setAvailableSubTypes] = useState([]);
    const [availableRarities, setAvailableRarities] = useState([]);

    const API_URL = import.meta.env.VITE_API_URL;

    // 🔹 Cargar filtros (tipos, rarezas, etc)
    useEffect(() => {
        const fetchFilters = async () => {
            try {
                const res = await fetch(`${API_URL}/api/card/filters`);
                if (!res.ok) throw new Error("Error al obtener filtros");

                const data = await res.json();

                // 🔸 Ajuste: usar las claves exactas del endpoint (ojo a mayúsculas/minúsculas)
                setAvailableRarities(data.rarities || data.Rarities || []);
                setAvailableSubTypes(data.subtypes || data.SubTypes || []);
                setAvailableTypes(data.types || data.Types || []);
                setAvailableSuperTypes(data.supertypes || data.superTypes || []);
            } catch (err) {
                console.error("Error cargando filtros:", err);
            }
        };
        fetchFilters();
    }, [API_URL]);

    // 🔹 Cargar todos los sets disponibles
    useEffect(() => {
        const fetchSets = async () => {            
            try {
                const res = await fetch(`${API_URL}/api/set/all`);
                if (!res.ok) throw new Error("Error al obtener sets");

                const data = await res.json();

                // 🔸 data probablemente sea un array de sets [{ setId, name, ptcgoCode }]
                setAvailableSets(data || []);
            } catch (err) {                
                console.error(err);
                setError("⚠️ The sets could not be loaded.");
            } 
        };
        fetchSets();
    }, [API_URL]);

    // 🔹 Buscar cartas (con filtros)
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
                    page: page,
                    pageSize: 55,
                });

                const res = await fetch(`${API_URL}/api/card/search?${params.toString()}`);
                if (!res.ok) throw new Error("Error al buscar cartas");

                const data = await res.json();
                setCards(data.items || []);
                setTotalPages(Math.ceil((data.totalCount || 0) / (data.pageSize || 55)));
            } catch (err) {
                console.error("Error cargando cartas:", err);
            }
        };

        fetchCards();
    }, [search, setId, supertype, type, subtype, rarity, page, API_URL]);

    const toggleCard = (cardId) => {
        setSelectedCards((prev) =>
            prev.includes(cardId)
                ? prev.filter((id) => id !== cardId)
                : [...prev, cardId]
        );
    };

    const handleSubmit = async () => {
        const newDeck = {
            name,
            description,
            cards: selectedCards.map((id) => ({ cardId: id })),
        };

        try {
            const res = await fetch(`${API_URL}/api/deck`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(newDeck),
            });

            if (!res.ok) throw new Error("Error al crear deck");

            alert("✅ Deck created successfully!");
            setName("");
            setDescription("");
            setSelectedCards([]);
        } catch (err) {
            console.error("Error creando deck:", err);
            alert("❌ Error creating deck");
        }
    };

    /* ------------------- Render ------------------- */   
    if (error) return <div className="error">{error}</div>;

    return (
        <div className="page-content" style={{ padding: "20px" }}>
            <h2 className="text-blue-700 font-bold text-2xl mb-4">Create a New Deck</h2>

            {/* --- Formulario --- */}
            <div style={{ marginBottom: "20px" }}>
                <label>Deck Name:</label>
                <input
                    type="text"
                    placeholder="Deck name"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    style={{ display: "block", margin: "10px 0", padding: "8px", width: "300px" }}
                />

                <label>Description:</label>
                <textarea
                    placeholder="Description"
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                    style={{
                        display: "block",
                        margin: "10px 0",
                        padding: "8px",
                        width: "300px",
                        height: "80px",
                    }}
                />
            </div>

            {/* --- Filtros --- */}
            <div
                style={{
                    display: "flex",
                    gap: "10px",
                    flexWrap: "wrap",
                    marginBottom: "20px",
                }}
            >
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

                {/* --- Set Filter --- */}
                <select
                    value={setId}
                    onChange={(e) => {
                        setPage(1);
                        setSetId(e.target.value);
                    }}
                    style={{ padding: "8px", width: "160px" }}
                >
                    <option value="">All Sets</option>
                    {availableSets.map((s,index) => (
                        <option key={index} value={s.setId}>
                            {s.name}
                        </option>
                    ))}
                </select>

                {/* --- Super Type Filter --- */}
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
                        <option key={index} value={ss.supertype}>
                            {ss.supertype}
                        </option>
                    ))}
                </select>

                {/* --- Type Filter --- */}
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
                        <option key={index} value={t.type}>
                            {t.type}
                        </option>
                    ))}
                </select>

                {/* --- Type Filter --- */}
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
                        <option key={index} value={b.subtype}>
                            {b.subtype}
                        </option>
                    ))}
                </select>

                {/* --- Rarity Filter --- */}
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
                        <option key={index} value={r.rarity}>
                            {r.rarity}
                        </option>
                    ))}
                </select>
            </div>

            {/* --- Resultados de cartas --- */}
            <div>
                <h3>Select Cards:</h3>
                <div
                    style={{
                        display: "grid",
                        gridTemplateColumns: "repeat(auto-fill, minmax(150px, 1fr))",
                        gap: "10px",
                    }}
                >
                    {cards.map((card) => (
                        <div className="card-item-search"
                            key={card.cardId}
                            onClick={() => toggleCard(card.cardId)}
                            style={{
                                border: selectedCards.includes(card.cardId)
                                    ? "3px solid #3b82f6"
                                    : "1px solid #ccc",
                                borderRadius: "8px",
                                padding: "5px",
                                cursor: "pointer",
                                textAlign: "center",
                            }}
                        >
                            <img
                                src={card.imageLarge || card.imageUrl}
                                alt={card.name}
                                style={{ width: "100%", borderRadius: "4px" }}
                            />
                            <p style={{ fontSize: "14px", color: "#333" }}>{card.name}</p>
                        </div>
                    ))}
                </div>

                {/* --- Paginación --- */}
                <div style={{ marginTop: "20px", textAlign: "center" }}>
                    <button
                        disabled={page <= 1}
                        onClick={() => setPage((p) => p - 1)}
                        style={{ marginRight: "10px" }}
                    >
                        ⬅ Prev
                    </button>
                    <span>
                        Page {page} of {totalPages}
                    </span>
                    <button
                        disabled={page >= totalPages}
                        onClick={() => setPage((p) => p + 1)}
                        style={{ marginLeft: "10px" }}
                    >
                        Next ➡
                    </button>
                </div>

                {/* --- Botón Guardar --- */}
                <button
                    onClick={handleSubmit}
                    style={{
                        marginTop: "30px",
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
