import React, { useState, useEffect } from "react";

var ICO_WARN = "\u26A0\uFE0F";
var ICO_OK   = "\u2705";
var ICO_FAIL = "\u274C";
var ICO_CLIP = "\uD83D\uDCCB";
var ICO_SAVE = "\uD83D\uDCBE";
var ICO_PAGE = "\uD83D\uDCC4";

export default function UpdateDeck() {
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
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
    const [backImagePath, setBackImagePath] = useState("");
    const [exporting, setExporting] = useState(false);
    const [decks, setDecks] = useState([]);
    const [selectedDeckId, setSelectedDeckId] = useState("");

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

    useEffect(() => { fetch(API_URL + "/api/deck/all").then(r => r.json()).then(d => setDecks(Array.isArray(d) ? d : [])).catch(console.error); }, [API_URL]);
    useEffect(() => { fetch(API_URL + "/api/card/filters").then(r => r.json()).then(d => { setAvailableRarities(norm(d?.rarities ?? d?.Rarities ?? [], ["rarity", "name", "value"])); setAvailableSubTypes(norm(d?.subtypes ?? d?.SubTypes ?? [], ["subtype", "name", "value"])); setAvailableTypes(norm(d?.types ?? d?.Types ?? [], ["type", "name", "value"])); setAvailableSuperTypes(norm(d?.supertypes ?? d?.superTypes ?? d?.SuperTypes ?? [], ["supertype", "name", "value"])); }).catch(console.error); }, [API_URL]);
    useEffect(() => { fetch(API_URL + "/api/set/all").then(r => r.json()).then(d => setAvailableSets(Array.isArray(d) ? d : [])).catch(console.error); }, [API_URL]);

    useEffect(() => {
        const params = new URLSearchParams();
        if (search) params.append("name", search);
        if (setId) params.append("setId", setId);
        if (subtype) params.append("subtype", subtype);
        if (type) params.append("type", type);
        if (supertype) params.append("supertype", supertype);
        if (rarity) params.append("rarity", rarity);
        params.append("page", String(page));
        params.append("pageSize", "55");
        fetch(API_URL + "/api/card/search?" + params).then(r => r.json()).then(d => {
            setCards(d?.items ?? []);
            var t = Number(d?.totalCount ?? 0), s = Number(d?.pageSize ?? 55);
            setTotalPages(s > 0 ? Math.max(1, Math.ceil(t / s)) : 1);
        }).catch(console.error);
    }, [search, setId, supertype, type, subtype, rarity, page, API_URL]);

    useEffect(() => { fetch("/config.json").then(r => r.json()).then(c => setBackImagePath(c.backImagePath)).catch(() => setBackImagePath("")); }, []);

    const getTotalSelected = (arr) => arr.reduce((s, c) => s + (Number(c.quantity) || 0), 0);

    const handleSelectDeck = async (deckId) => {
        setSelectedDeckId(deckId); setMessage("");
        if (!deckId) { setName(""); setDescription(""); setSelectedCards([]); return; }
        try {
            const r = await fetch(API_URL + "/api/deck/" + deckId);
            if (!r.ok) throw new Error("Error");
            const d = await r.json();
            setName(d.name || ""); setDescription(d.description || "");
            setSelectedCards((d.cards || []).map(c => ({ cardId: c.cardId, name: c.name ?? c.cardId, supertype: c.supertype ?? "", subtype: c.subtype ?? "", quantity: c.quantity, ptcgocode: c.ptcgocode ?? "", number: c.number ?? "", setName: c.setName ?? "", setId: c.setId ?? "", imageLarge: c.imageLarge ?? "" })));
        } catch (e) { console.error(e); setMessage(ICO_FAIL + " Error loading deck."); }
    };

    const toggleCard = (cardOrId) => {
        const id = typeof cardOrId === "string" ? cardOrId : cardOrId.cardId;
        const obj = typeof cardOrId === "object" ? cardOrId : cards.find(c => c.cardId === id) || {};
        setSelectedCards(prev => {
            const total = getTotalSelected(prev);
            const found = prev.find(c => c.cardId === id);
            if (total >= 60 && !found) { setMessage(ICO_WARN + " Deck already has 60 cards."); return prev; }
            if (found) {
                const isEnergy = ((found.supertype ?? obj.supertype) || "").toLowerCase() === "energy";
                if (total >= 60) { setMessage(ICO_WARN + " Cannot exceed 60 cards."); return prev; }
                if (!isEnergy && found.quantity >= 4) { setMessage(ICO_WARN + " Limit of 4 copies reached."); return prev; }
                return prev.map(c => c.cardId === id ? { ...c, quantity: c.quantity + 1 } : c);
            }
            if (total < 60) return [...prev, { cardId: id, name: obj?.name ?? "Unknown", supertype: obj?.supertype ?? "", subtype: obj?.subtype ?? "", quantity: 1, ptcgocode: obj?.ptcgocode ?? obj?.ptcgoCode ?? "", number: obj?.number ?? "", imageLarge: obj?.imageLarge ?? "" }];
            setMessage(ICO_WARN + " No more cards (limit 60)."); return prev;
        });
    };

    const removeCard = (cardId) => setSelectedCards(prev => { const f = prev.find(c => c.cardId === cardId); if (!f) return prev; if (f.quantity > 1) return prev.map(c => c.cardId === cardId ? { ...c, quantity: c.quantity - 1 } : c); return prev.filter(c => c.cardId !== cardId); });
    const deleteCard = (cardId) => setSelectedCards(prev => prev.filter(c => c.cardId !== cardId));
    const clearSelection = () => { setSelectedDeckId(""); setName(""); setDescription(""); setMessage(""); setLoading(false); setSelectedCards([]); setShowImportModal(false); setImportText(""); };

    const handleSubmit = async () => {
        if (!selectedDeckId) { setMessage(ICO_WARN + " Select a deck to update."); return; }
        if (!name.trim()) { setMessage(ICO_WARN + " Please enter a deck name."); return; }
        if (!selectedCards.length) { setMessage(ICO_WARN + " Select at least 60 cards."); return; }
        const total = getTotalSelected(selectedCards);
        if (total !== 60) { setMessage(ICO_WARN + " Deck must have exactly 60 cards. Current: " + total); return; }
        try {
            const r = await fetch(API_URL + "/api/deck/update", { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ deckId: Number(selectedDeckId), name, description, cards: selectedCards.map(c => ({ cardId: String(c.cardId), quantity: Number(c.quantity), ptcgocode: String(c.ptcgocode ?? ""), imageLarge: String(c.imageLarge), supertype: String(c.supertype ?? ""), name: String(c.name ?? "") })) }) });
            if (!r.ok) throw new Error(await r.text());
            setMessage(ICO_OK + " Deck \"" + name + "\" updated!");
            const rr = await fetch(API_URL + "/api/deck/all"); if (rr.ok) { const d = await rr.json(); setDecks(Array.isArray(d) ? d : []); }
        } catch (e) { setMessage(ICO_FAIL + " Error updating deck."); console.error(e); }
    };

    const handleDownloadDeckPdf = async () => {
        if (!selectedCards.length) { alert("No cards in deck."); return; }
        setExporting(true);
        try {
            const urls = [];
            for (const c of selectedCards) for (var i = 0; i < (c.quantity || 1); i++) if (c.imageLarge) urls.push(c.imageLarge);
            if (backImagePath) { var n = urls.length; for (var j = 0; j < n; j++) urls.push(backImagePath); }
            const safe = (name?.trim() || "Deck").replace(/\s+/g, "_").replace(/[^\w\-\.]/g, "");
            const r = await fetch(API_URL + "/api/printer/generatedeck", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ imageUrls: urls, fileName: safe }) });
            if (!r.ok) throw new Error("PDF error");
            const blob = await r.blob(); const a = document.createElement("a"); a.href = URL.createObjectURL(blob); a.download = safe + ".pdf"; a.click(); URL.revokeObjectURL(a.href);
        } catch (e) { alert("Could not export PDF."); console.error(e); } finally { setExporting(false); }
    };

    const handleImportFromText = async () => {
        if (!importText.trim()) { alert(ICO_WARN + " Paste the deck text before importing."); return; }
        setLoading(true);
        try {
            const lines = importText.split("\n").map(l => l.trim()).filter(l => l && !/^pok\u00e9mon|trainer|energies|cards totals/i.test(l));
            const regex = /^(\d+)\s+([\p{L}\p{N}\s''\u201C\u201D"\.\-:,&()]+?)\s*(?:\(?([A-Z0-9\-]{2,6})\)?(?:\s+(\d+))?)?$/u;
            const parsed = [], unparsed = [];
            for (const line of lines) { const m = line.match(regex); if (m) parsed.push({ quantity: parseInt(m[1], 10), name: (m[2] || "").trim(), ptcgocode: (m[3] || "").trim(), number: (m[4] || "").trim() }); else unparsed.push(line); }
            if (!parsed.length) { alert(ICO_WARN + " No valid lines detected."); setLoading(false); return; }
            const found = [], notFound = [];
            for (const card of parsed) {
                try {
                    const url = API_URL + "/api/card/search?name=" + encodeURIComponent(card.name) + "&ptcgocode=" + encodeURIComponent(card.ptcgocode) + "&number=" + encodeURIComponent(card.number) + "&page=1&pageSize=10";
                    const res = await fetch(url); if (!res.ok) continue; const data = await res.json(); const list = data.items ?? data.data ?? [];
                    const match = list.find(c => String(c.number) === card.number) || list.find(c => (c.name || "").toLowerCase() === card.name.toLowerCase()) || null;
                    if (match) found.push({ ...match, quantity: card.quantity, ptcgocode: match.ptcgocode ?? match.ptcgoCode ?? card.ptcgocode }); else notFound.push(card);
                } catch (err) { notFound.push(card); }
            }
            if (!found.length) { alert(ICO_WARN + " No cards found."); setLoading(false); return; }
            setSelectedCards(prev => {
                const grouped = {}; for (const c of prev) grouped[c.cardId] = { ...c };
                for (const card of found) {
                    const id = card.cardId ?? card.id ?? (card.name + "-" + (card.ptcgocode ?? card.number));
                    const isEnergy = (card.supertype ?? "").toLowerCase() === "energy";
                    if (!grouped[id]) grouped[id] = { cardId: id, name: card.name, supertype: card.supertype, subtype: card.subtype, quantity: 0, ptcgocode: card.ptcgocode ?? "", number: card.number ?? "", imageLarge: card.imageLarge ?? "" };
                    const totalNow = Object.values(grouped).reduce((s, c) => s + c.quantity, 0);
                    const remaining = 60 - totalNow; if (remaining <= 0) break;
                    const canAdd = Math.min(isEnergy ? card.quantity : Math.min(4 - grouped[id].quantity, card.quantity), remaining);
                    if (canAdd > 0) grouped[id].quantity += canAdd;
                }
                const newDeck = Object.values(grouped);
                var msg = ICO_OK + " " + found.reduce((s, c) => s + (c.quantity ?? 0), 0) + " cards imported \u2014 total: " + newDeck.reduce((s, c) => s + c.quantity, 0) + "/60";
                if (notFound.length || unparsed.length) msg += "\n\n" + ICO_FAIL + " Not found:\n" + [...notFound.map(nf => "\u2022 " + nf.quantity + "x " + nf.name), ...unparsed.map(u => "\u2022 " + u)].join("\n");
                alert(msg); return newDeck;
            });
            setShowImportModal(false); setImportText("");
        } catch (e) { alert("Error importing deck."); console.error(e); } finally { setLoading(false); }
    };

    const fi = (label, value, setter, ph) => (<div className="field-group"><label className="field-label">{label}</label><input className="field-input" placeholder={ph} value={value} onChange={e => { setPage(1); setter(e.target.value); }} /></div>);
    const fs = (label, value, setter, opts, all) => (<div className="field-group"><label className="field-label">{label}</label><select className="field-select" value={value} onChange={e => { setPage(1); setter(e.target.value); }}><option value="">{all}</option>{opts.map((o, i) => <option key={i} value={typeof o === "object" ? (o.setId ?? o.id ?? i) : o}>{typeof o === "object" ? (o.name ?? String(o.setId)) : o}</option>)}</select></div>);

    const totalQty = getTotalSelected(selectedCards);
    const msgClass = message.indexOf(ICO_OK) >= 0 ? "msg-success" : message.indexOf(ICO_FAIL) >= 0 ? "msg-error" : "msg-warning";

    return (
        <div className="page-content-v2">
            <h1 className="section-title">Update Deck</h1>
            <div className="builder-layout">
                <div>
                    <div className="glass-panel" style={{ marginBottom: 16 }}>
                        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 12 }}>
                            <div className="field-group">
                                <label className="field-label">Deck</label>
                                <select className="field-select" value={selectedDeckId} onChange={e => handleSelectDeck(e.target.value)}>
                                    <option value="">{"\u2014"} Select {"\u2014"}</option>
                                    {decks.map(d => <option key={d.deckId} value={d.deckId}>{d.name}</option>)}
                                </select>
                            </div>
                            <div className="field-group">
                                <label className="field-label">Deck Name</label>
                                <input className="field-input" placeholder="Enter deck name" value={name} onChange={e => setName(e.target.value)} />
                            </div>
                            <div className="field-group">
                                <label className="field-label">Description</label>
                                <textarea className="field-textarea" placeholder="Describe your deck\u2026" value={description} onChange={e => setDescription(e.target.value)} />
                            </div>
                        </div>
                        <div style={{ display: "flex", gap: 8, marginTop: 12, justifyContent: "flex-end", flexWrap: "wrap" }}>
                            <button className="btn btn-outline btn-sm" onClick={clearSelection}>Clear</button>
                            <button className="btn btn-outline btn-sm" onClick={() => setShowImportModal(true)}>{ICO_CLIP} Import</button>
                            <button className="btn btn-green btn-sm" onClick={handleSubmit}>{ICO_SAVE} Update Deck</button>
                        </div>
                        {message && <p className={msgClass} style={{ marginTop: 10 }}>{message}</p>}
                    </div>

                    <div className="glass-panel" style={{ marginBottom: 16 }}>
                        <div className="filter-grid">
                            {fi("Name", search, setSearch, "Charizard")}
                            {fs("Set", setId, setSetId, availableSets, "All Sets")}
                            {fs("Super Type", supertype, setSuperType, availableSuperTypes, "All")}
                            {fs("Type", type, setType, availableTypes, "All")}
                            {fs("Sub Type", subtype, setSubType, availableSubTypes, "All")}
                            {fs("Rarity", rarity, setRarity, availableRarities, "All")}
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
                        <span className="builder-sidebar-title">Deck ({totalQty}/60)</span>
                        <button className="btn btn-blue btn-xs" onClick={handleDownloadDeckPdf} disabled={exporting}>
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
                                    <div className="builder-card-meta">{c.supertype} {c.number ? ("\u00B7 #" + c.number) : ""} {c.ptcgocode ? ("\u00B7 " + c.ptcgocode) : ""}</div>
                                </div>
                                <span className="builder-card-qty">x{c.quantity}</span>
                                <div className="builder-card-actions">
                                    <button className="btn btn-outline btn-xs" onClick={() => removeCard(c.cardId)} title="Remove one">{"\u2212"}</button>
                                    <button className="btn btn-outline btn-xs" onClick={() => toggleCard(c)} disabled={((c.supertype ?? "").toLowerCase() !== "energy" && c.quantity >= 4) || totalQty >= 60} title="Add one">+</button>
                                    <button className="btn btn-red btn-xs" onClick={() => deleteCard(c.cardId)} title="Delete all">{"\u2715"}</button>
                                </div>
                            </div>
                        ))}
                </div>
            </div>

            {showImportModal && (
                <div className="modal-overlay">
                    <div className="modal-box">
                        <h3 className="modal-title">{ICO_CLIP} Import Deck</h3>
                        <textarea className="field-textarea" style={{ width: "100%", minHeight: 180 }} value={importText} onChange={e => setImportText(e.target.value)} placeholder="Paste deck text from TCG Live\u2026" />
                        <div style={{ display: "flex", justifyContent: "flex-end", gap: 8, marginTop: 14 }}>
                            <button className="btn btn-outline btn-sm" onClick={() => setShowImportModal(false)}>Cancel</button>
                            <button className="btn btn-green btn-sm" onClick={handleImportFromText}>Import</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
