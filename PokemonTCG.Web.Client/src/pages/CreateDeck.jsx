import React, { useState, useEffect } from "react";

export default function CreateDeck() {
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [error, setError] = useState("");
    const [message, setMessage] = useState("");
    const [loading, setLoading] = useState(false);
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
    const [showImportModal, setShowImportModal] = useState(false);
    const [importText, setImportText] = useState("");

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
    const handleImportFromText = async () => {
        if (!importText.trim()) {
            alert("⚠️ Pegá el texto del mazo primero.");
            return;
        }

        setLoading(true);

        try {
            const lines = importText
                .split("\n")
                .map((l) => l.trim())
                .filter((l) => l && !/^pokémon|entrenador|energ[ií]a|cartas totales/i.test(l));

            const parsed = [];
            // 📘 Formato: cantidad nombre ptcgocode número
            const regex = /^(\d+)\s+(.+?)\s+([A-Z0-9\-]{2,6})\s+(\d+)$/i;

            for (const line of lines) {
                const m = regex.exec(line);
                if (m) {
                    parsed.push({
                        quantity: parseInt(m[1]),
                        name: m[2].trim(),
                        ptcgocode: m[3].trim(),
                        number: m[4].trim(),
                    });
                }
            }

            if (parsed.length === 0) {
                alert("⚠️ No se detectaron cartas válidas.");
                setLoading(false);
                return;
            }

            const found = [];
            const notFound = [];

            for (const card of parsed) {
                try {
                    // 🔍 Buscar por name + ptcgocode
                    const url = `${API_URL}/api/card/search?name=${encodeURIComponent(card.name)}&ptcgocode=${encodeURIComponent(card.ptcgocode)}&number=${encodeURIComponent(card.number)}&page=1&pageSize=10`;
                    const res = await fetch(url);
                    if (!res.ok) continue;

                    const data = await res.json();
                    const list = data.items ?? data.data ?? [];

                    // Buscar coincidencia exacta por número o nombre
                    const match =
                        list.find((c) => String(c.number) === card.number) ||
                        list.find((c) => (c.name || "").toLowerCase() === card.name.toLowerCase()) ||
                        null;

                    if (match) {
                        found.push({
                            ...match,
                            quantity: card.quantity,
                        });
                    } else {
                        notFound.push(card);
                    }
                } catch (err) {
                    console.warn("Error buscando carta:", card, err);
                    notFound.push(card);
                }
            }

            if (found.length === 0) {
                alert("⚠️ No se encontró ninguna carta en la base de datos.");
                setLoading(false);
                return;
            }

            // 🔢 Agrupar y respetar las reglas (máx. 4 copias salvo energía)
            const grouped = {};
            for (const card of found) {
                const id = card.cardId ?? card.id ?? card.externalId ?? `${card.name}-${card.ptcgocode}-${card.number}`;
                const isEnergy = (card.supertype ?? "").toLowerCase() === "energy";
                const existing = grouped[id];

                if (existing) {
                    existing.quantity += isEnergy
                        ? card.quantity
                        : Math.min(card.quantity, 4 - existing.quantity);
                } else {
                    grouped[id] = {
                        cardId: id,
                        name: card.name,
                        supertype: card.supertype,
                        subtype: card.subtype,
                        quantity: isEnergy ? card.quantity : Math.min(card.quantity, 4),
                    };
                }
            }

            const newSelected = Object.values(grouped);
            setSelectedCards(newSelected);
            setShowImportModal(false);
            setImportText("");

            const totalQty = newSelected.reduce((sum, c) => sum + c.quantity, 0);
            alert(`✅ Se importaron ${totalQty} cartas (${newSelected.length} únicas).`);

            if (notFound.length > 0) {
                console.warn("❌ No encontradas:", notFound);
            }
        } catch (err) {
            console.error("❌ Error al importar:", err);
            alert("Error al importar mazo. Ver consola.");
        } finally {
            setLoading(false);
        }
    };
    const handleSubmit = async () => {
        if (!name.trim()) {
            setMessage("⚠️ Please enter a deck name.");
            return;
        }

        if (!selectedCards || selectedCards.length === 0) {
            setMessage("⚠️ Please select at least one card.");
            return;
        }

        const totalCards = selectedCards.reduce((sum, c) => sum + c.quantity, 0);
        if (totalCards !== 60) {
            setMessage(`⚠️ Your deck must have exactly 60 cards. Current: ${totalCards}`);
            return;
        }

        const payloadCards = selectedCards.map((item) => ({
            cardId: String(item.cardId),
            quantity: Number(item.quantity),
        }));


        if (payloadCards.length === 0) {
            setMessage("⚠️ No valid cards to submit.");
            return;
        }

        const newDeck = {
            deckId: null,
            name,
            description,
            cards: payloadCards
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

            setMessage("✅ Deck created successfully!");
            setName("");
            setDescription("");
            setSelectedCards([]);
        } catch (err) {
            console.error("Error creating deck:", err);
            setMessage("❌ Error creating deck. Check console for details.");
        }
    };

    // función utilitaria: total actual de cartas seleccionadas
    // Calcula el total actual
    const getTotalSelected = (arr) =>
        arr.reduce((s, c) => s + (Number(c.quantity) || 0), 0);

    // toggleCard mejorado
    const toggleCard = (cardOrId) => {
        const id = typeof cardOrId === "string" ? cardOrId : cardOrId.cardId;
        const cardObj = typeof cardOrId === "object"
            ? cardOrId
            : cards.find((c) => c.cardId === id) || {};

        setSelectedCards((prev) => {
            const totalNow = getTotalSelected(prev);
            const found = prev.find((c) => c.cardId === id);

            // Si ya hay 60 cartas, no permitimos sumar más (también evita añadir nuevas)
            if (totalNow >= 60 && !found) {
                // opcional: mostrar mensaje breve
                setMessage("⚠️ El mazo ya tiene 60 cartas. Elimina o reduce cartas para añadir otras.");
                return prev;
            }

            // Si existe la carta en la selección: intentamos incrementar 1
            if (found) {
                const isEnergy = ((found.supertype ?? cardObj.supertype ?? cardObj.superType) || "")
                    .toString()
                    .toLowerCase() === "energy";

                // Si incrementar excede 60 totales, bloquear
                if (totalNow >= 60) {
                    setMessage("⚠️ No se puede superar 60 cartas en el mazo.");
                    return prev;
                }

                // Si no es Energy, respetar max 4 por carta
                if (!isEnergy && found.quantity >= 4) {
                    setMessage("⚠️ Límite de 4 copias alcanzado para esta carta.");
                    return prev;
                }

                // incrementar 1 (energía puede crecer sin el tope 4)
                return prev.map((c) =>
                    c.cardId === id ? { ...c, quantity: c.quantity + 1 } : c
                );
            }

            // Si no estaba y podemos agregar (totalNow < 60)
            if (totalNow < 60) {
                return [
                    ...prev,
                    {
                        cardId: id,
                        name: cardObj?.name ?? "Unknown",
                        supertype: cardObj?.supertype ?? cardObj?.superType ?? "",
                        subtype: cardObj?.subtype ?? cardObj?.subType ?? "",
                        quantity: 1,
                    },
                ];
            }

            // fallback, no cambios
            setMessage("⚠️ No se pueden añadir más cartas (límite 60).");
            return prev;
        });
    };
    const clearSelection = () => {
        setSelectedCards([]);
        setMessage("");
        setError("");
    };

    const removeCard = (cardId) => {
        setSelectedCards((prev) => {
            const found = prev.find((c) => c.cardId === cardId);
            if (!found) return prev;
            if (found.quantity > 1) {
                // decrementar
                return prev.map((c) =>
                    c.cardId === cardId ? { ...c, quantity: c.quantity - 1 } : c
                );
            }
            // si queda 0, eliminar
            return prev.filter((c) => c.cardId !== cardId);
        });
    };
    const deleteCard = (cardId) => {
        // eliminar completamente (botón ❌)
        setSelectedCards((prev) => prev.filter((c) => c.cardId !== cardId));
    };
    const [autoDeckCount, setAutoDeckCount] = useState(1);   
    const handleAutoDeck = async () => {
        try {
            setLoading(true);
            clearSelection();
            const res = await fetch(`${API_URL}/api/deck/autodeck`);
            const autoDeck = await res.json();
            autoDeck.forEach(card => toggleCard(card));

            setMessage("✅ Auto Deck generado correctamente (60 cartas).");

        } catch (err) {
            console.error(err);
            setMessage("❌ Error generando el mazo automático.");
        } finally {
            setLoading(false);
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
            <table width="100%" className="tcg-table pokemon-tcg-text">
                <tbody>
                    <tr>
                        <td colSpan="6" width="100%" style={{ textAlign: "center" }}>
                            <h2 className="text-blue-700 font-bold text-2xl mb-4">Deck Management</h2>
                        </td>
                    </tr>
                    <tr>                
                        <td colSpan="2" style={{ verticalAlign: "top", paddingTop: "10px" }}>
                            <div style={{ display: "flex", flexDirection: "column", gap: "14px" }}>

                                {/* 🔹 Name */}
                                <div style={{ display: "flex", alignItems: "flex-start", gap: "10px" }}>
                                    <label style={{ width: "90px", textAlign: "right", marginTop: "6px" }}>Name:</label>
                                    <input
                                        type="text"
                                        placeholder="Deck name"
                                        value={name}
                                        onChange={(e) => setName(e.target.value)}
                                        style={{ padding: "8px", width: "300px" }}
                                    />
                                </div>

                                {/* 🔹 Description */}
                                <div style={{ display: "flex", alignItems: "flex-start", gap: "10px" }}>
                                    <label style={{ width: "90px", textAlign: "right", marginTop: "6px" }}>Description:</label>
                                    <textarea
                                        placeholder="Description"
                                        value={description}
                                        onChange={(e) => setDescription(e.target.value)}
                                        style={{ padding: "8px", width: "300px", height: "80px" }}
                                    />
                                </div>
                            </div>
                        </td>

                        <td
                            rowSpan="2"
                            colSpan="2"
                            style={{
                                verticalAlign: "top",
                                padding: "10px",
                                minWidth: "450px",
                                maxWidth: "600px",
                                width: "auto",
                            }}
                        >
                            {/*Selected Cards*/}
                            <h3 style={{ textAlign: "center", fontWeight: "bold", marginBottom: "8px" }}>Selected Cards</h3>

                            {selectedCards.length === 0 ? (
                                <p style={{ textAlign: "center", color: "#777" }}>No cards selected</p>
                            ) : (
                                <ul
                                    style={{
                                        listStyle: "none",
                                        paddingLeft: 0,
                                        margin: 0,
                                        maxHeight: "260px",
                                        overflowY: "auto",
                                    }}
                                >
                                    {selectedCards.map((c) => (
                                        <li
                                            key={c.cardId}
                                            style={{
                                                display: "flex",
                                                justifyContent: "space-between",
                                                alignItems: "center", // ✅ centrado vertical
                                                padding: "6px 8px",
                                                borderBottom: "1px solid #eee",
                                                fontSize: "0.95rem",
                                            }}
                                        >
                                            {/* 🔹 Info de la carta (izquierda) */}
                                            <div style={{ flex: 1, textAlign: "left" }}>
                                                <div style={{ fontWeight: 600, marginBottom: "2px" }}>{c.name}</div>
                                                <div style={{ color: "#555", fontSize: "0.85rem" }}>
                                                    {c.supertype || "—"}
                                                    {c.subtype ? ` • ${c.subtype}` : ""}
                                                </div>
                                            </div>

                                            {/* 🔹 Controles (derecha) */}
                                            <div
                                                style={{
                                                    display: "flex",
                                                    alignItems: "center",
                                                    gap: "8px",
                                                    marginLeft: "12px",
                                                }}
                                            >
                                                {/* Cantidad */}
                                                <div style={{ fontWeight: "700", color: "#1e40af", minWidth: "28px", textAlign: "center" }}>
                                                    x{c.quantity}
                                                </div>

                                                {/* Botones */}
                                                <div style={{ display: "flex", gap: "4px" }}>
                                                    {/* Decrementar */}
                                                    <button
                                                        type="button"
                                                        onClick={() => removeCard(c.cardId)}
                                                        style={{
                                                            padding: "4px 6px",
                                                            borderRadius: "4px",
                                                            border: "1px solid #ddd",
                                                            cursor: "pointer",
                                                            backgroundColor: "#f3f4f6",
                                                        }}
                                                        title="Quitar una"
                                                    >
                                                        −
                                                    </button>

                                                    {/* Incrementar */}
                                                    <button
                                                        type="button"
                                                        onClick={() => toggleCard(c)}
                                                        disabled={
                                                            ((c.supertype ?? "").toLowerCase() !== "energy" && c.quantity >= 4) ||
                                                            getTotalSelected(selectedCards) >= 60
                                                        }
                                                        style={{
                                                            padding: "4px 6px",
                                                            borderRadius: "4px",
                                                            border: "1px solid #ddd",
                                                            cursor: "pointer",
                                                            backgroundColor: "#f3f4f6",
                                                            opacity:
                                                                ((c.supertype ?? "").toLowerCase() !== "energy" && c.quantity >= 4) ||
                                                                    getTotalSelected(selectedCards) >= 60
                                                                    ? 0.4
                                                                    : 1,
                                                        }}
                                                        title="Agregar una"
                                                    >
                                                        +
                                                    </button>

                                                    {/* Eliminar todas */}
                                                    <button
                                                        type="button"
                                                        onClick={() => deleteCard(c.cardId)}
                                                        style={{
                                                            padding: "4px 6px",
                                                            borderRadius: "4px",
                                                            border: "1px solid #ddd",
                                                            cursor: "pointer",
                                                            backgroundColor: "#f3f4f6",
                                                        }}
                                                        title="Eliminar todas las copias"
                                                    >
                                                        ❌
                                                    </button>
                                                </div>
                                            </div>
                                        </li>
                                    ))}
                                </ul>
                            )}
                            <div style={{ display: "flex", justifyContent: "space-between", marginTop: "8px", alignItems: "center" }}>
                                <div style={{ fontSize: "0.95rem" }}>
                                    Total cards: <strong>{selectedCards.reduce((s, c) => s + c.quantity, 0)}</strong>
                                </div>                               
                            </div>
                        </td>
                    </tr>
                    <tr><td> <button onClick={clearSelection} style={{ padding: "6px 10px", borderRadius: "6px", border: "1px solid #ddd" }}>
                        Clear
                    </button>
                        <button
                            onClick={handleAutoDeck}
                            style={{ padding: "6px 10px", borderRadius: "6px", border: "1px solid #ddd" }}
                        >
                            Auto Deck
                        </button>
                        <button
                            onClick={() => setShowImportModal(true)}
                            style={{ padding: "6px 10px", borderRadius: "6px", border: "1px solid #ddd" }}
                        >
                            Import Deck
                        </button>


                        <button
                            onClick={handleSubmit}
                            style={{ padding: "6px 10px", borderRadius: "6px", border: "1px solid #ddd" }}
                        >
                            Save
                        </button></td></tr>
                </tbody>
            </table>
            <br></br>
            <table width="100%">
                <tbody>
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

            {message && (
                <p
                    style={{
                        textAlign: "center",
                        fontWeight: "bold",
                        marginTop: "10px",
                        marginBottom: "10px",
                        color: message.includes("✅")
                            ? "green"
                            : message.includes("❌")
                                ? "red"
                                : message.includes("⚠️")
                                    ? "orange"
                                    : "blue",
                    }}
                >
                    {message}
                </p>
            )}
            <br></br>
            {/* Contenedor de cartas con scroll */}
            <div>
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
                            key={card.cardId}
                            onClick={() => toggleCard(card.cardId)}
                            style={{
                                border: selectedCards.some((c) => c.cardId === card.cardId)
                                    ? "2px solid limegreen"
                                    : "1px solid gray",
                                position: "relative",
                                cursor: "pointer",
                            }}
                        >
                            <img className="card-item-search"
                                src={card.imageLarge}
                                alt={card?.name}
                                style={{ width: "100%", borderRadius: "4px", display: "block", margin: "0 auto" }}
                            />
                            <p style={{ fontSize: "14px", color: "#333", textAlign: "center", margin: "6px 0 0" }}>
                                {card?.name}
                            </p>

                            {/* Mostrar contador si fue seleccionada */}
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
                                    ×
                                    {
                                        selectedCards.find((c) => c.cardId === card.cardId)
                                            ?.quantity
                                    }
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            </div>
            <div style={{
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
            }}>
                <button
                    disabled={page <= 1}
                    onClick={() => setPage((p) => p - 1)}
                    style={{ padding: "4px 6px", borderRadius: "4px", border: "1px solid #ddd" }}>
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
            {
                showImportModal && (
                    <div
                        style={{
                            position: "fixed",
                            top: 0,
                            left: 0,
                            width: "100%",
                            height: "100%",
                            background: "rgba(0,0,0,0.5)",
                            display: "flex",
                            justifyContent: "center",
                            alignItems: "center",
                            zIndex: 1000,
                        }}
                    >
                        <div
                            style={{
                                background: "white",
                                padding: "20px",
                                borderRadius: "12px",
                                width: "500px",
                                boxShadow: "0 4px 20px rgba(0,0,0,0.3)",
                            }}
                        >
                            <h3 style={{ marginBottom: "12px", fontWeight: "bold", textAlign: "center" }}>
                                📋 Import Deck (Pokémon TCG Live)
                            </h3>

                            <textarea
                                value={importText}
                                onChange={(e) => setImportText(e.target.value)}
                                placeholder="Pegá aquí el texto del mazo exportado desde TCG Live..."
                                style={{
                                    width: "100%",
                                    height: "200px",
                                    padding: "10px",
                                    borderRadius: "8px",
                                    border: "1px solid #ccc",
                                    resize: "none",
                                    marginBottom: "12px",
                                }}
                            />

                            <div style={{ display: "flex", justifyContent: "space-between" }}>
                                <button
                                    onClick={() => setShowImportModal(false)}
                                    style={{
                                        padding: "8px 14px",
                                        borderRadius: "8px",
                                        background: "#e5e7eb",
                                        border: "1px solid #ccc",
                                        cursor: "pointer",
                                    }}
                                >
                                    Cancelar
                                </button>

                                <button
                                    onClick={handleImportFromText}
                                    style={{
                                        padding: "8px 14px",
                                        borderRadius: "8px",
                                        background: "#16a34a",
                                        color: "white",
                                        border: "none",
                                        cursor: "pointer",
                                        fontWeight: 600,
                                    }}
                                >
                                    Importar
                                </button>
                            </div>
                        </div>
                    </div>
                )
            }
        </div>
    );
}
