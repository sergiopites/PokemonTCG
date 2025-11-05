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
    const [importMessage, setImportMessage] = useState(null);
    const [backImagePath, setBackImagePath] = useState("");

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
    }, [search, setId, supertype, type, subtype, rarity, page, API_URL]);

    useEffect(() => {
        // Cargar la configuración al montar el componente
        fetch("/config.json")
            .then(res => res.json())
            .then(config => setBackImagePath(config.backImagePath))
            .catch(() => setBackImagePath("")); // Valor por defecto si falla
    }, []);

    const handleDownloadDeckPdf = async () => {
        try {
            if (!selectedCards || selectedCards.length === 0) {
                alert("There are no cards in the deck to print..");
                return;
            }

            // 🔹 Crear una lista de URLs de imágenes (frentes + reverso)
            const imageUrls = [];

            for (const c of selectedCards) {
                if (c.imageLarge) {
                    // Repetimos según la cantidad de copias
                    for (let i = 0; i < (c.quantity || 1); i++) {
                        imageUrls.push(c.imageLarge);
                    }
                } else {
                    console.warn(`⚠️ Card without image: ${c.name}`);
                }
            }

            // 🔹 Agregar reversos si corresponde
            if (backImagePath) {
                const total = imageUrls.length;
                for (let i = 0; i < total; i++) {
                    imageUrls.push(backImagePath);
                }
            }

            if (imageUrls.length === 0) {
                alert("No images suitable for printing were found.");
                return;
            }

            // 🔹 Nombre del archivo
            const deckName = name?.trim() || "Deck";
            const safeFileName = deckName.replace(/\s+/g, "_").replace(/[^\w\-\.]/g, "");

            // 🔹 Enviar al backend
            const response = await fetch(`${API_URL}/api/printer/generatedeck`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    imageUrls,
                    fileName: safeFileName,
                }),
            });

            if (!response.ok) {
                throw new Error(`Error generating PDF: ${response.status} - ${response.statusText}`);
            }

            // 🔹 Descargar PDF
            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);

            const a = document.createElement("a");
            a.href = url;
            a.download = `${safeFileName}.pdf`;
            a.click();

            window.URL.revokeObjectURL(url);

            console.log(`✅ The PDF was generated successfully with ${imageUrls.length} images.`);

        } catch (error) {
            console.error("❌ Error generating PDF deck:", error);
            alert("The deck couldn't download. Try again later");
        }
    };
        
    const handleImportFromText = async () => {
        if (!importText.trim()) {
            alert("⚠️ Pegá el texto del mazo antes de importar.");
            return;
        }

        setLoading(true);

        try {
            // Filtramos líneas irrelevantes (títulos, totales, etc.)
            const lines = importText
                .split("\n")
                .map((l) => l.trim())
                .filter((l) => l && !/^pokémon|entrenador|energ[ií]a|cartas totales/i.test(l));

            const parsed = [];

            // 🔹 Regex robusto: soporta apóstrofos, paréntesis, etc.
            const regex = /^(\d+)\s+([\p{L}\p{N}\s'’"“”\.\-:,&()]+?)\s*(?:\(?([A-Z0-9\-]{2,6})\)?(?:\s+(\d+))?)?$/u;

            const unparsed = [];

            for (const line of lines) {
                const m = line.match(regex);
                if (m) {
                    parsed.push({
                        quantity: parseInt(m[1], 10),
                        name: (m[2] || "").trim(),
                        ptcgocode: (m[3] || "").trim(),
                        number: (m[4] || "").trim(),
                    });
                } else {
                    unparsed.push(line);
                }
            }

            if (parsed.length === 0) {
                alert("⚠️ No se detectaron cartas válidas para importar.");
                setLoading(false);
                return;
            }

            const found = [];
            const notFound = [];

            for (const card of parsed) {
                try {
                    const url = `${API_URL}/api/card/search?name=${encodeURIComponent(card.name)}&ptcgocode=${encodeURIComponent(card.ptcgocode)}&number=${encodeURIComponent(card.number)}&page=1&pageSize=10`;
                    const res = await fetch(url);
                    if (!res.ok) continue;

                    const data = await res.json();
                    const list = data.items ?? data.data ?? [];

                    const match =
                        list.find((c) => String(c.number) === card.number) ||
                        list.find((c) => (c.name || "").toLowerCase() === card.name.toLowerCase()) ||
                        null;

                    if (match) {
                        const foundCard = {
                            ...match,
                            quantity: card.quantity,
                            ptcgocode: match.ptcgocode ?? match.ptcgoCode ?? card.ptcgocode,
                        };
                        found.push(foundCard);
                    } else {
                        notFound.push(card);
                    }
                } catch (err) {
                    console.warn("Error buscando carta:", card, err);
                    notFound.push(card);
                }
            }

            if (found.length === 0) {
                alert("⚠️ No se encontraron cartas en la base de datos.");
                setLoading(false);
                return;
            }

            setSelectedCards((prev) => {
                const grouped = {};
                for (const c of prev) {
                    grouped[c.cardId] = { ...c };
                }

                for (const card of found) {
                    const id = card.cardId ?? card.id ?? card.externalId ?? `${card.name}-${card.ptcgocode ?? card.number}`;
                    const isEnergy = (card.supertype ?? "").toLowerCase() === "energy";

                    if (!grouped[id]) {
                        grouped[id] = {
                            cardId: id,
                            name: card.name,
                            supertype: card.supertype,
                            subtype: card.subtype,
                            quantity: 0,
                            ptcgocode: card.ptcgocode ?? card.ptcgoCode ?? "",
                            number: card.number ?? "",
                            imageLarge:
                                card.imageLarge ??
                                card.imageUrl ??
                                (card.images && (card.images.large ?? card.images.small)) ??
                                ""
                        };
                    }

                    const totalNow = Object.values(grouped).reduce((sum, c) => sum + c.quantity, 0);
                    const remaining = 60 - totalNow;
                    if (remaining <= 0) {
                        setMessage("⚠️ Límite de 60 cartas alcanzado. No se agregaron más cartas.");
                        break;
                    }

                    const canAdd = Math.min(
                        isEnergy ? card.quantity : Math.min(4 - grouped[id].quantity, card.quantity),
                        remaining
                    );

                    if (canAdd > 0) {
                        grouped[id].quantity += canAdd;
                    }
                }

                const newDeck = Object.values(grouped);
                const totalQty = newDeck.reduce((sum, c) => sum + c.quantity, 0);
                const importedCount = found.reduce((sum, c) => sum + (c.quantity ?? 0), 0);

                let message = `✅ Se importaron ${importedCount} cartas (${found.length} tipos) — total actual: ${totalQty}/60`;

                if (notFound.length > 0 || unparsed.length > 0) {
                    const list = [
                        ...notFound.map(nf => `• ${nf.quantity}x ${nf.name} (${nf.ptcgocode || "?"} ${nf.number || "?"})`),
                        ...unparsed.map(u => `• ${u}`)
                    ].join("\n");
                    message += `\n\n❌ No se pudieron validar:\n${list}`;
                }

                alert(message);
                return newDeck;
            });

            setShowImportModal(false);
            setImportText("");
        } catch (err) {
            console.error("❌ Error al importar: ", err);
            alert("Error al importar el mazo. Revisa la consola.");
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
            setMessage("⚠️ Please select at least 60 cards or import a deck.");
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
            ptcgocode: String(item.ptcgocode ?? item.ptcgoCode ?? ""),
            imageLarge: String(item.imageLarge),
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
    const getTotalSelected = (arr) =>
        arr.reduce((s, c) => s + (Number(c.quantity) || 0), 0);

    // toggleCard mejorado (ahora incluye ptcgocode)
    const toggleCard = (cardOrId) => {
        const id = typeof cardOrId === "string" ? cardOrId : cardOrId.cardId;
        const cardObj = typeof cardOrId === "object"
            ? cardOrId
            : cards.find((c) => c.cardId === id) || {};

        setSelectedCards((prev) => {
            const totalNow = getTotalSelected(prev);
            const found = prev.find((c) => c.cardId === id);

            if (totalNow >= 60 && !found) {
                setMessage("⚠️ The deck already has 60 cards. Remove or reduce cards to add others.");
                return prev;
            }

            if (found) {
                const isEnergy = ((found.supertype ?? cardObj.supertype ?? cardObj.superType) || "")
                    .toString()
                    .toLowerCase() === "energy";

                if (totalNow >= 60) {
                    setMessage("⚠️ The deck cannot exceed 60 cards.");
                    return prev;
                }

                if (!isEnergy && found.quantity >= 4) {
                    setMessage("⚠️ Limit of 4 copies reached for this card.");
                    return prev;
                }

                return prev.map((c) =>
                    c.cardId === id ? { ...c, quantity: c.quantity + 1 } : c
                );
            }

            if (totalNow < 60) {
                return [
                    ...prev,
                    {
                        cardId: id,
                        name: cardObj?.name ?? "Unknown",
                        supertype: cardObj?.supertype ?? cardObj?.superType ?? "",
                        subtype: cardObj?.subtype ?? cardObj?.subType ?? "",
                        quantity: 1,
                        ptcgocode: cardObj?.ptcgoCode ?? cardObj?.ptcgocode ?? "",
                        number: cardObj?.number ?? cardObj?.Number ?? "",
                        imageLarge:
                            cardObj?.imageLarge ??
                            cardObj?.imageUrl ??
                            (cardObj?.images && (cardObj.images.large ?? cardObj.images.small)) ??
                            ""
                    },
                ];
            }

            // fallback, no cambios
            setMessage("⚠️ No more cards can be added (limit 60).");
            return prev;
        });
    };
    const clearSelection = () => {
        // Entradas generales
        setName("");
        setDescription("");
        setError("");
        setMessage("");
        setLoading(false);

        // Listas y resultados
        /* setCards([]);*/
        setSelectedCards([]);

        // Paginación
        setPage(1);
        setTotalPages(1);

        // Listas de filtros (sets, types, etc.)
        //setAvailableSets([]);
        //setAvailableSuperTypes([]);
        //setAvailableTypes([]);
        //setAvailableSubTypes([]);
        //setAvailableRarities([]);

        // Import modal
        setShowImportModal(false);
        setImportText("");
        setImportMessage(null);
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
    //const [autoDeckCount, setAutoDeckCount] = useState(1);   
    const handleAutoDeck = async () => {
        try {
            setLoading(true);
            clearSelection();
            const res = await fetch(`${API_URL}/api/deck/autodeck`);
            const autoDeck = await res.json();
            autoDeck.forEach(card => toggleCard(card));

            setMessage("✅ Deck created successfully (60 cards).");

        } catch (err) {
            console.error(err);
            setMessage("❌ Error building deck.");
        } finally {
            setLoading(false);
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
                    {/* Title */}
                    <tr>
                        <td colSpan="6" style={{ textAlign: "center", padding: "12px 8px 6px" }}>
                        
                            <h2 style={{ margin: 0, color: "#075985", fontSize: "1.25rem", fontWeight: 700 }}>
                                Deck Builder
                            </h2>
                        </td>
                    </tr>

                    {/* Left inputs + Selected cards (right) */}
                    <tr>
                        {/* Left column: name + description */}
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
                                    {/* Deck Name */}
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
                                            placeholder="Enter deck name"
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

                                    {/* Description */}
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
                                            placeholder="Describe your deck..."
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


                                {/* compact action buttons */}
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
                                        onClick={handleAutoDeck}
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
                                        Auto Build
                                    </button>

                                    <button
                                        onClick={() => setShowImportModal(true)}
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
                                        Import/Add
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
                                        Save
                                    </button>
                                </div>
                            </div>
                        </td>

                        {/* Right: Selected cards list (bigger column) */}
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
                                <h3 style={{ margin: 0, color: "#1e40af", fontSize: "1rem", fontWeight: 700 }}>Selected Cards</h3>
                                <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                                    <span style={{ fontSize: "0.95rem", color: "#334155" }}>
                                        Total: <strong>{selectedCards.reduce((s, c) => s + c.quantity, 0)}</strong>
                                    </span>
                                    <button
                                        onClick={handleDownloadDeckPdf}
                                        style={{
                                            padding: "6px 10px",
                                            borderRadius: "8px",
                                            border: "1px solid #cbd5e1",
                                            background: "#3b82f6",
                                            color: "white",
                                            cursor: "pointer",
                                            fontSize: "0.85rem",
                                            fontWeight: 600,
                                        }}
                                    >
                                        Export PDF
                                    </button>
                                </div>
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
                                                    <div style={{ minWidth: 0 }}>
                                                        <div style={{ fontWeight: 600, fontSize: "0.95rem", color: "#0f172a", whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis", maxWidth: "320px" }}>
                                                            {c.name}
                                                        </div>
                                                        <div style={{ color: "#6b7280", fontSize: "0.8rem" }}>
                                                            {c.supertype ?? "—"} {c.number ? ` • ${c.number}` : ""} {(c.ptcgocode ?? c.ptcgoCode) ? ` • ${c.ptcgocode ?? c.ptcgoCode}` : ""}
                                                        </div>
                                                    </div>
                                                </div>

                                                <div style={{ display: "flex", alignItems: "center", gap: "6px" }}>
                                                    <span style={{ fontWeight: 700, color: "#2563eb", minWidth: "28px", textAlign: "center" }}>x{c.quantity}</span>

                                                    {/* small control buttons */}
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
                                                            −
                                                        </button>

                                                        <button
                                                            onClick={() => toggleCard(c)}
                                                            disabled={((c.supertype ?? "").toLowerCase() !== "energy" && c.quantity >= 4) || getTotalSelected(selectedCards) >= 60}
                                                            style={{
                                                                padding: "4px 6px",
                                                                borderRadius: "6px",
                                                                border: "1px solid #e6edf3",
                                                                background: "#ffffff",
                                                                cursor: "pointer",
                                                                fontSize: "0.85rem",
                                                                opacity: (((c.supertype ?? "").toLowerCase() !== "energy" && c.quantity >= 4) || getTotalSelected(selectedCards) >= 60) ? 0.45 : 1,
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
                                                            ✕
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

                    {/* Empty spacer row to ensure right panel doesn't overlap filters */}
                    <tr>
                        <td style={{ height: "6px", background: "transparent" }}></td>
                        <td></td>
                    </tr>

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
                                {/* Name */}
                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label
                                        style={{
                                            fontSize: "0.85rem",
                                            color: "#334155",
                                            marginBottom: "4px",
                                            fontWeight: "500",
                                        }}
                                    >
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

                                {/* Set */}
                                <div style={{ display: "flex", flexDirection: "column" }}>
                                    <label
                                        style={{
                                            fontSize: "0.85rem",
                                            color: "#334155",
                                            marginBottom: "4px",
                                            fontWeight: "500",
                                        }}
                                    >
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
                                    <label
                                        style={{
                                            fontSize: "0.85rem",
                                            color: "#334155",
                                            marginBottom: "4px",
                                            fontWeight: "500",
                                        }}
                                    >
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
                                    <label
                                        style={{
                                            fontSize: "0.85rem",
                                            color: "#334155",
                                            marginBottom: "4px",
                                            fontWeight: "500",
                                        }}
                                    >
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
                                    <label
                                        style={{
                                            fontSize: "0.85rem",
                                            color: "#334155",
                                            marginBottom: "4px",
                                            fontWeight: "500",
                                        }}
                                    >
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
                                    <label
                                        style={{
                                            fontSize: "0.85rem",
                                            color: "#334155",
                                            marginBottom: "4px",
                                            fontWeight: "500",
                                        }}
                                    >
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
                    <tr>
                        <td colSpan="6" style={{ textAlign: "center", padding: "12px 8px 6px" }}>
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
                        </td>
                    </tr>
                </tbody>
            </table>                      
            <br></br>

            {/* Contenedor de cartas con scroll */}
            <div className="all-cards">
                <div className="card-grid-search">
                    {cards.map((card) => (
                        <div
                            key={card.cardId}
                            onClick={() => toggleCard(card.cardId)}
                            style={{

                                border: selectedCards.some((c) => c.cardId === card.cardId),
                                //? "2px solid limegreen"
                                //: "1px solid gray",
                                position: "relative",
                                cursor: "pointer",
                            }}
                        >
                            <img className="card-item card-image"
                                src={card.imageLarge}
                                alt={card?.name}
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
                                background: "lightskyblue",
                                padding: "20px",
                                borderRadius: "12px",
                                maxWidth: "700px",   // 🔹 más ancho
                                width: "90%",
                                margin: "0 auto",    // 🔹 centrado
                                boxShadow: "0 4px 12px rgba(0,0,0,0.2)"
                            }}
                        >
                            <h3
                                style={{ marginBottom: "12px", fontWeight: "bold", textAlign: "center" }}>
                                📋 Import Deck
                            </h3>

                            <textarea
                                value={importText}
                                onChange={(e) => setImportText(e.target.value)}
                                placeholder="Paste the deck text exported from TCG Live here..."
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
                                    Import
                                </button>
                            </div>
                        </div>
                    </div>
                )
            }
        </div>
    );
}
