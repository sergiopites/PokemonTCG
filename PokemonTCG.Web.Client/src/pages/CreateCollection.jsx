import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";

var ICO_WARN = "\u26A0\uFE0F";
var ICO_OK   = "\u2705";
var ICO_FAIL = "\u274C";
var ICO_SAVE = "\uD83D\uDCBE";
var ICO_PAGE = "\uD83D\uDCC4";

export default function CreateCollection() {
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
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
    const [exporting, setExporting] = useState(false);
    const [backImagePath, setBackImagePath] = useState("");

    const API_URL = import.meta.env.VITE_API_URL || "";

    const norm = (arr, keys = []) => {
        if (!Array.isArray(arr)) return [];
        return arr.map((item) => {
            if (item == null) return "";
            if (typeof item === "string") return item;
            for (const k of keys) if (item[k] && typeof item[k] === "string") return item[k];
            const p = Object.values(item).find((v) => typeof v === "string" || typeof v === "number");
            return p != null ? String(p) : JSON.stringify(item);
        });
    };

    useEffect(() => {
        fetch(API_URL + "/api/card/filters").then(r => r.json()).then(d => {
            setAvailableRarities(norm(d?.rarities ?? d?.Rarities ?? [], ["rarity", "name", "value"]));
            setAvailableSubTypes(norm(d?.subtypes ?? d?.SubTypes ?? [], ["subtype", "name", "value"]));
            setAvailableTypes(norm(d?.types ?? d?.Types ?? [], ["type", "name", "value"]));
            setAvailableSuperTypes(norm(d?.supertypes ?? d?.superTypes ?? d?.SuperTypes ?? [], ["supertype", "name", "value"]));
        }).catch(console.error);
    }, [API_URL]);

    useEffect(() => {
        fetch(API_URL + "/api/set/all").then(r => r.json()).then(d => setAvailableSets(Array.isArray(d) ? d : [])).catch(console.error);
    }, [API_URL]);

    useEffect(() => {
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
        fetch(API_URL + "/api/card/search?" + params).then(r => r.json()).then(d => {
            setCards(d?.items ?? []);
            const total = Number(d?.totalCount ?? 0), size = Number(d?.pageSize ?? 55);
            setTotalPages(size > 0 ? Math.max(1, Math.ceil(total / size)) : 1);
        }).catch(console.error);
    }, [search, number, setId, supertype, type, subtype, rarity, page, API_URL]);

    useEffect(() => {
        fetch("/config.json").then(r => r.json()).then(c => setBackImagePath(c.backImagePath)).catch(() => setBackImagePath(""));
    }, []);

    const toggleCard = (cardOrId) => {
        const id = typeof cardOrId === "string" ? cardOrId : cardOrId.cardId;
        const obj = typeof cardOrId === "object" ? cardOrId : cards.find(c => c.cardId === id) || {};
        setSelectedCards(prev => {
            const found = prev.find(c => c.cardId === id);
            if (found) return prev.map(c => c.cardId === id ? { ...c, quantity: c.quantity + 1 } : c);
            return [...prev, {
                cardId: id, name: obj?.name ?? "Unknown",
                supertype: obj?.supertype ?? "", subtype: obj?.subtype ?? "",
                quantity: 1, ptcgocode: obj?.ptcgocode ?? "",
                number: obj?.number ?? "", setName: obj?.setName ?? "", setId: obj?.setId ?? "",
                imageLarge: obj?.imageLarge ?? "",
            }];
        });
    };

    const removeCard = (cardId) => {
        setSelectedCards(prev => {
            const f = prev.find(c => c.cardId === cardId);
            if (!f) return prev;
            if (f.quantity > 1) return prev.map(c => c.cardId === cardId ? { ...c, quantity: c.quantity - 1 } : c);
            return prev.filter(c => c.cardId !== cardId);
        });
    };

    const deleteCard = (cardId) => setSelectedCards(prev => prev.filter(c => c.cardId !== cardId));
    const clearSelection = () => { setName(""); setDescription(""); setMessage(""); setSelectedCards([]); };
    const totalQty = selectedCards.reduce((s, c) => s + (Number(c.quantity) || 0), 0);

    const handleSubmit = async () => {
        if (!name.trim()) { setMessage(ICO_WARN + " Please enter a collection name."); return; }
        if (!selectedCards.length) { setMessage(ICO_WARN + " Select at least one card."); return; }
        try {
            const r = await fetch(API_URL + "/api/collection/create", {
                method: "POST", headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ name, description, cards: selectedCards.map(c => ({ cardId: String(c.cardId), quantity: Number(c.quantity) })) }),
            });
            if (!r.ok) throw new Error(await r.text());
            setMessage(ICO_OK + " Collection \"" + name + "\" created!");
            setName(""); setDescription(""); setSelectedCards([]);
        } catch (e) { setMessage(ICO_FAIL + " Error creating collection."); console.error(e); }
    };

    const handleExportPdf = async () => {
        if (!selectedCards.length) { alert("No cards to export."); return; }
        setExporting(true);
        try {
            const urls = [];
            for (const c of selectedCards) for (var i = 0; i < (c.quantity || 1); i++) if (c.imageLarge) urls.push(c.imageLarge);
            if (backImagePath) { var n = urls.length; for (var j = 0; j < n; j++) urls.push(backImagePath); }
            const safe = (name?.trim() || "Collection").replace(/\s+/g, "_").replace(/[^\w\-\.]/g, "");
            const r = await fetch(API_URL + "/api/printer/generatedeck", {
                method: "POST", headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ imageUrls: urls, fileName: safe }),
            });
            if (!r.ok) throw new Error("PDF error");
            const blob = await r.blob();
            const a = document.createElement("a");
            a.href = URL.createObjectURL(blob);
            a.download = safe + ".pdf";
            a.click();
            URL.revokeObjectURL(a.href);
        } catch (e) { alert("Could not export PDF."); console.error(e); }
        finally { setExporting(false); }
    };

    const fieldInput = (label, value, setter, placeholder) => (
        <div className="field-group">
            <label className="field-label">{label}</label>
            <input className="field-input" placeholder={placeholder} value={value} onChange={e => { setPage(1); setter(e.target.value); }} />
        </div>
    );

    const fieldSelect = (label, value, setter, options, allLabel) => (
        <div className="field-group">
            <label className="field-label">{label}</label>
            <select className="field-select" value={value} onChange={e => { setPage(1); setter(e.target.value); }}>
                <option value="">{allLabel}</option>
                {options.map((o, i) => <option key={i} value={typeof o === "object" ? (o.setId ?? o.id ?? i) : o}>{typeof o === "object" ? (o.name ?? String(o.setId)) : o}</option>)}
            </select>
        </div>
    );

    const msgClass = message.indexOf(ICO_OK) >= 0 ? "msg-success" : message.indexOf(ICO_FAIL) >= 0 ? "msg-error" : "msg-warning";

    return (
        <div className="page-content-v2">
            <h1 className="section-title">Collection Builder</h1>

            <div className="builder-layout">
                <div>
                    <div className="glass-panel" style={{ marginBottom: 16 }}>
                        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                            <div className="field-group">
                                <label className="field-label">Name</label>
                                <input className="field-input" placeholder="Collection name" value={name} onChange={e => setName(e.target.value)} />
                            </div>
                            <div className="field-group">
                                <label className="field-label">Description</label>
                                <textarea className="field-textarea" placeholder="Describe your collection\u2026" value={description} onChange={e => setDescription(e.target.value)} />
                            </div>
                        </div>
                        <div style={{ display: "flex", gap: 8, marginTop: 12, justifyContent: "flex-end" }}>
                            <button className="btn btn-outline btn-sm" onClick={clearSelection}>Clear</button>
                            <button className="btn btn-green btn-sm" onClick={handleSubmit}>{ICO_SAVE} Save Collection</button>
                        </div>
                        {message && <p className={msgClass} style={{ marginTop: 10 }}>{message}</p>}
                    </div>

                    <div className="glass-panel" style={{ marginBottom: 16 }}>
                        <div className="filter-grid">
                            {fieldInput("Number", number, setNumber, "#")}
                            {fieldInput("Name", search, setSearch, "Charizard")}
                            {fieldSelect("Set", setId, setSetId, availableSets, "All Sets")}
                            {fieldSelect("Super Type", supertype, setSuperType, availableSuperTypes, "All")}
                            {fieldSelect("Type", type, setType, availableTypes, "All")}
                            {fieldSelect("Sub Type", subtype, setSubType, availableSubTypes, "All")}
                            {fieldSelect("Rarity", rarity, setRarity, availableRarities, "All")}
                        </div>
                    </div>

                    <div className="card-grid-v2">
                        {cards.map(card => {
                            const sel = selectedCards.find(c => c.cardId === card.cardId);
                            return (
                                <div key={card.cardId} className={"card-thumb" + (sel ? " selected" : "")} onClick={() => toggleCard(card)}>
                                    <img src={card.imageLarge} alt={card.name} />
                                    {sel && (
                                        <div style={{ position: "absolute", inset: 0, display: "flex", alignItems: "center", justifyContent: "center", zIndex: 1, pointerEvents: "none" }}>
                                            <span style={{ background: "rgba(0,0,0,.75)", color: "var(--gold)", padding: "4px 14px", borderRadius: 8, fontSize: "1.5rem", fontWeight: 900 }}>
                                                {"\u00D7"}{sel.quantity}
                                            </span>
                                        </div>
                                    )}
                                </div>
                            );
                        })}
                    </div>

                    <div className="pagination-bar">
                        <button className="btn btn-outline btn-xs" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>{"\u25C0"} Prev</button>
                        <span>Page <strong>{page}</strong> of <strong>{totalPages}</strong></span>
                        <button className="btn btn-outline btn-xs" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>Next {"\u25B6"}</button>
                    </div>
                </div>

                <div className="builder-sidebar">
                    <div className="builder-sidebar-header">
                        <span className="builder-sidebar-title">Collection</span>
                        <div className="builder-count">
                            Unique: <strong>{selectedCards.length}</strong> {"\u00B7"} Total: <strong>{totalQty}</strong>
                        </div>
                    </div>

                    <div style={{ marginBottom: 10, display: "flex", justifyContent: "flex-end" }}>
                        <button className="btn btn-blue btn-xs" onClick={handleExportPdf} disabled={exporting}>
                            {exporting ? "Exporting\u2026" : ICO_PAGE + " Export PDF"}
                        </button>
                    </div>

                    {selectedCards.length === 0
                        ? <p style={{ textAlign: "center", color: "var(--text-dim)", padding: "20px 0" }}>No cards selected</p>
                        : selectedCards.map(c => (
                            <div key={c.cardId} className="builder-card-item">
                                {c.imageLarge && <img src={c.imageLarge} alt={c.name} className="builder-card-thumb" />}
                                <div className="builder-card-info">
                                    <div className="builder-card-name">{c.name}</div>
                                    <div className="builder-card-meta">{c.supertype} {c.number ? ("\u00B7 #" + c.number) : ""} {c.setName ? ("\u00B7 " + c.setName) : ""}</div>
                                </div>
                                <span className="builder-card-qty">x{c.quantity}</span>
                                <div className="builder-card-actions">
                                    <button className="btn btn-outline btn-xs" onClick={() => removeCard(c.cardId)} title="Remove one">{"\u2212"}</button>
                                    <button className="btn btn-red btn-xs" onClick={() => deleteCard(c.cardId)} title="Delete all">{"\u2715"}</button>
                                </div>
                            </div>
                        ))}
                </div>
            </div>
        </div>
    );
}
