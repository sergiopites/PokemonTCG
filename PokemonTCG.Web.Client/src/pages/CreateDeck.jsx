import React, { useState, useEffect } from "react";

// Unicode-safe emoji constants
var ICO_WARN  = "\u26A0\uFE0F";
var ICO_OK    = "\u2705";
var ICO_FAIL  = "\u274C";
var ICO_DICE  = "\uD83C\uDFB2";
var ICO_CLIP  = "\uD83D\uDCCB";
var ICO_SAVE  = "\uD83D\uDCBE";
var ICO_PAGE  = "\uD83D\uDCC4";
var ICO_XLS   = "\uD83D\uDCCA";

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
    const [backImagePath, setBackImagePath] = useState("");
    const [exporting, setExporting] = useState(false);
    const [exportProgress, setExportProgress] = useState(0);

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
        fetch(API_URL + "/api/set/all").then(r => r.json()).then(d => setAvailableSets(Array.isArray(d) ? d : [])).catch(e => { console.error(e); setError(ICO_WARN + " Sets could not be loaded."); });
    }, [API_URL]);

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
            if (total < 60) return [...prev, { cardId: id, name: obj?.name ?? "Unknown", supertype: obj?.supertype ?? "", subtype: obj?.subtype ?? "", quantity: 1, ptcgocode: obj?.ptcgocode ?? obj?.ptcgoCode ?? "", number: obj?.number ?? "", imageLarge: obj?.imageLarge ?? "", type: obj?.type ?? "", rarity: obj?.rarity ?? "", setName: obj?.setName ?? "", setId: obj?.setId ?? "", artist: obj?.artist ?? "", hp: obj?.hp ?? null, evolvesFrom: obj?.evolvesFrom ?? "", evolvesTo: obj?.evolvesTo ?? "" }];
            setMessage(ICO_WARN + " No more cards (limit 60)."); return prev;
        });
    };

    const removeCard = (cardId) => setSelectedCards(prev => { const f = prev.find(c => c.cardId === cardId); if (!f) return prev; if (f.quantity > 1) return prev.map(c => c.cardId === cardId ? { ...c, quantity: c.quantity - 1 } : c); return prev.filter(c => c.cardId !== cardId); });
    const deleteCard = (cardId) => setSelectedCards(prev => prev.filter(c => c.cardId !== cardId));

    const clearSelection = () => { setName(""); setDescription(""); setError(""); setMessage(""); setLoading(false); setSelectedCards([]); setPage(1); setShowImportModal(false); setImportText(""); };

    const handleSubmit = async () => {
        if (!name.trim()) { setMessage(ICO_WARN + " Please enter a deck name."); return; }
        if (!selectedCards.length) { setMessage(ICO_WARN + " Select at least 60 cards."); return; }
        const total = getTotalSelected(selectedCards);
        if (total !== 60) { setMessage(ICO_WARN + " Deck must have exactly 60 cards. Current: " + total); return; }
        try {
            const r = await fetch(API_URL + "/api/deck/create", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ deckId: null, name, description, cards: selectedCards.map(c => ({ cardId: String(c.cardId), quantity: Number(c.quantity), ptcgocode: String(c.ptcgocode ?? ""), imageLarge: String(c.imageLarge), supertype: String(c.supertype ?? ""), name: String(c.name ?? "") })) }) });
            if (!r.ok) throw new Error(await r.text());
            setMessage(ICO_OK + " Deck \"" + name + "\" created!"); setName(""); setDescription(""); setSelectedCards([]);
        } catch (e) { setMessage(ICO_FAIL + " Error creating deck."); console.error(e); }
    };

    const handleAutoDeck = async () => {
        try {
            setLoading(true); clearSelection(); setMessage("");
            const res = await fetch(API_URL + "/api/deck/autodeck");
            const data = await res.json();
            if (!data || !Array.isArray(data.cards)) throw new Error("Invalid response");
            const grouped = {};
            for (const card of data.cards) { const id = card.cardId ?? card.id ?? (card.name + "-" + card.setId); if (!grouped[id]) grouped[id] = { cardId: id, name: card.name ?? "Unknown", supertype: card.supertype ?? "", subtype: card.subtype ?? "", quantity: 0, ptcgocode: card.ptcgocode ?? "", number: card.number ?? "", imageLarge: card.imageLarge ?? "", type: card.type ?? "", rarity: card.rarity ?? "", setName: card.setName ?? "", setId: card.setId ?? "", artist: card.artist ?? "", hp: card.hp ?? null, evolvesFrom: card.evolvesFrom ?? "", evolvesTo: card.evolvesTo ?? "" }; grouped[id].quantity += 1; }
            const newDeck = Object.values(grouped);
            const totalQty = newDeck.reduce((s, c) => s + c.quantity, 0);
            setSelectedCards(newDeck);
            const deckName = data.deckName ?? ((data["powerPok\u00e9mon"]?.[0] ?? newDeck[0]?.name ?? "Unknown") + " - " + (data.dominantType ?? "Unknown"));
            setName(deckName);
            setDescription("Pok\u00e9mon: " + (data["pok\u00e9mon"] ?? 0) + " | Trainers: " + (data.trainers ?? 0) + " | Energy: " + (data.energy ?? 0));
            setMessage(ICO_OK + " Deck \"" + deckName + "\" built (" + totalQty + " cards)");
        } catch (e) { setMessage(ICO_FAIL + " Error building deck."); console.error(e); } finally { setLoading(false); }
    };

    const handleDownloadDeckPdf = async () => {
        if (!selectedCards.length) { alert("No cards in deck."); return; }
        setExporting(true); setExportProgress(0);
        try {
            const urls = [];
            for (const c of selectedCards) for (var i = 0; i < (c.quantity || 1); i++) if (c.imageLarge) urls.push(c.imageLarge);
            if (!urls.length) { alert("No card images available to export."); return; }
            if (backImagePath) { var n = urls.length; for (var j = 0; j < n; j++) urls.push(backImagePath); }
            const safe = (name?.trim() || "Deck").replace(/\s+/g, "_").replace(/[^\w\-\.]/g, "");
            const r = await fetch(API_URL + "/api/printer/generatedeck/progress", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ imageUrls: urls, fileName: safe }) });
            if (!r.ok) { const errText = await r.text().catch(() => "PDF error"); throw new Error(errText); }
            const reader = r.body.getReader();
            const decoder = new TextDecoder();
            let buf = "";
            let pdfData = null;
            let pdfFileName = safe + ".pdf";
            while (true) {
                const { value, done } = await reader.read();
                if (done) break;
                buf += decoder.decode(value, { stream: true });
                const lines = buf.split("\n");
                buf = lines.pop();
                for (const line of lines) {
                    if (!line.startsWith("data: ")) continue;
                    try {
                        const evt = JSON.parse(line.slice(6));
                        if (evt.progress != null) setExportProgress(evt.progress);
                        if (evt.done && evt.pdf) { pdfData = evt.pdf; pdfFileName = evt.fileName || pdfFileName; }
                        if (evt.error) throw new Error(evt.error);
                    } catch (pe) { if (pe.message && !pe.message.includes("JSON")) throw pe; }
                }
            }
            if (!pdfData) throw new Error("No PDF received");
            setExportProgress(100);
            const byteChars = atob(pdfData);
            const byteArr = new Uint8Array(byteChars.length);
            for (let k = 0; k < byteChars.length; k++) byteArr[k] = byteChars.charCodeAt(k);
            const blob = new Blob([byteArr], { type: "application/pdf" });
            const a = document.createElement("a"); a.href = URL.createObjectURL(blob); a.download = pdfFileName; a.click(); URL.revokeObjectURL(a.href);
        } catch (e) { alert("Could not export PDF."); console.error(e); } finally { setExporting(false); setExportProgress(0); }
    };

    const handleDownloadDeckXls = async () => {
        if (!selectedCards.length) { alert("No cards in deck."); return; }
        setExporting(true); setExportProgress(0);
        try {
            const cardsPayload = selectedCards.map(c => ({ cardId: c.cardId || "", name: c.name || "", supertype: c.supertype || "", subtype: c.subtype || "", type: c.type || "", rarity: c.rarity || "", setName: c.setName || "", setId: c.setId || "", number: c.number || "", artist: c.artist || "", ptcgocode: c.ptcgocode || "", quantity: c.quantity || 1, imageLarge: c.imageLarge || "", hp: c.hp ?? null, evolvesFrom: c.evolvesFrom || "", evolvesTo: c.evolvesTo || "" }));
            const safe = (name?.trim() || "Deck").replace(/\s+/g, "_").replace(/[^\w\-\.]/g, "");
            const r = await fetch(API_URL + "/api/printer/generateexcel/progress", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ cards: cardsPayload, fileName: safe }) });
            if (!r.ok) { const errText = await r.text().catch(() => "XLS error"); throw new Error(errText); }
            const reader = r.body.getReader();
            const decoder = new TextDecoder();
            let buf = "";
            let xlsData = null;
            let xlsFileName = safe + ".xlsx";
            while (true) {
                const { value, done } = await reader.read();
                if (done) break;
                buf += decoder.decode(value, { stream: true });
                const lines = buf.split("\n");
                buf = lines.pop();
                for (const line of lines) {
                    if (!line.startsWith("data: ")) continue;
                    try {
                        const evt = JSON.parse(line.slice(6));
                        if (evt.progress != null) setExportProgress(evt.progress);
                        if (evt.done && evt.excel) { xlsData = evt.excel; xlsFileName = evt.fileName || xlsFileName; }
                        if (evt.error) throw new Error(evt.error);
                    } catch (pe) { if (pe.message && !pe.message.includes("JSON")) throw pe; }
                }
            }
            if (!xlsData) throw new Error("No XLS received");
            setExportProgress(100);
            const byteChars = atob(xlsData);
            const byteArr = new Uint8Array(byteChars.length);
            for (let k = 0; k < byteChars.length; k++) byteArr[k] = byteChars.charCodeAt(k);
            const blob = new Blob([byteArr], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
            const a = document.createElement("a"); a.href = URL.createObjectURL(blob); a.download = xlsFileName; a.click(); URL.revokeObjectURL(a.href);
        } catch (e) { alert("Could not export XLS."); console.error(e); } finally { setExporting(false); setExportProgress(0); }
    };

    const handleImportFromText = async () => {
        if (!importText.trim()) { alert(ICO_WARN + " Paste the deck text before importing."); return; }
        setLoading(true);
        try {
            const lines = importText.split("\n").map(l => l.trim()).filter(l => l && !/^pok\u00e9mon|trainer|energies|cards totals/i.test(l));
            const regex = /^(\d+)\s+([\p{L}\p{N}\s''\u201C\u201D"\.\-:,&()\[\]]+?)\s*(?:\(?([A-Z0-9\-]{2,6})\)?(?:\s+(\d+))?)?$/u;
            const parsed = [], unparsed = [];
            for (const line of lines) { const m = line.match(regex); if (m) parsed.push({ quantity: parseInt(m[1], 10), name: (m[2] || "").trim(), ptcgocode: (m[3] || "").trim(), number: (m[4] || "").trim() }); else unparsed.push(line); }
            if (!parsed.length) { alert(ICO_WARN + " No valid lines detected."); setLoading(false); return; }
            const found = [], notFound = [];
            const searchName = (n) => n.replace(/\s*\[.*?\]\s*$/, "").trim();
            const findMatch = (list, card) => (card.number ? list.find(c => String(c.number) === card.number) : null) || list.find(c => (c.name || "").toLowerCase() === card.name.toLowerCase()) || list.find(c => (c.name || "").toLowerCase() === searchName(card.name).toLowerCase()) || (list.length > 0 ? list[0] : null);
            for (const card of parsed) {
                try {
                    const cleanName = searchName(card.name);
                    const url = API_URL + "/api/card/search?name=" + encodeURIComponent(cleanName) + (card.ptcgocode ? "&ptcgocode=" + encodeURIComponent(card.ptcgocode) : "") + (card.number ? "&number=" + encodeURIComponent(card.number) : "") + "&page=1&pageSize=10";
                    const res = await fetch(url); if (!res.ok) continue; const data = await res.json(); var list = data.items ?? data.data ?? [];
                    if (!list.length && card.ptcgocode) { const url2 = API_URL + "/api/card/search?name=" + encodeURIComponent(cleanName) + "&page=1&pageSize=10"; const res2 = await fetch(url2); if (res2.ok) { const data2 = await res2.json(); list = data2.items ?? data2.data ?? []; } }
                    const match = findMatch(list, card);
                    if (match) found.push({ ...match, quantity: card.quantity, ptcgocode: match.ptcgocode ?? match.ptcgoCode ?? card.ptcgocode }); else notFound.push(card);
                } catch (err) { notFound.push(card); }
            }
            if (!found.length) { alert(ICO_WARN + " No cards found."); setLoading(false); return; }
            setSelectedCards(prev => {
                const grouped = {}; for (const c of prev) grouped[c.cardId] = { ...c };
                for (const card of found) {
                    const id = card.cardId ?? card.id ?? (card.name + "-" + (card.ptcgocode ?? card.number));
                    const isEnergy = (card.supertype ?? "").toLowerCase() === "energy";
                    if (!grouped[id]) grouped[id] = { cardId: id, name: card.name, supertype: card.supertype, subtype: card.subtype, quantity: 0, ptcgocode: card.ptcgocode ?? "", number: card.number ?? "", imageLarge: card.imageLarge ?? "", type: card.type ?? "", rarity: card.rarity ?? "", setName: card.setName ?? "", setId: card.setId ?? "", artist: card.artist ?? "", hp: card.hp ?? null, evolvesFrom: card.evolvesFrom ?? "", evolvesTo: card.evolvesTo ?? "" };
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
            <h1 className="section-title">Deck Builder</h1>

            <div className="builder-layout">
                <div>
                    {/* Form */}
                    <div className="glass-panel" style={{ marginBottom: 16 }}>
                        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
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
                            <button className="btn btn-blue btn-sm" onClick={handleAutoDeck} disabled={loading}>{loading ? "Building\u2026" : ICO_DICE + " AutoDeck"}</button>
                            <button className="btn btn-outline btn-sm" onClick={() => setShowImportModal(true)}>{ICO_CLIP} Import</button>
                            <button className="btn btn-green btn-sm" onClick={handleSubmit}>{ICO_SAVE} Save Deck</button>
                        </div>
                        {message && <p className={msgClass} style={{ marginTop: 10 }}>{message}</p>}
                    </div>

                    {/* Filters */}
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

                    {/* Card Grid */}
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
                        <button className="btn btn-outline btn-xs" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>
                            {"\u25C0"} Prev
                        </button>
                        <span>Page <strong>{page}</strong> of <strong>{totalPages}</strong></span>
                        <button className="btn btn-outline btn-xs" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>
                            Next {"\u25B6"}
                        </button>
                    </div>
                </div>

                {/* Sidebar */}
                <div className="builder-sidebar">
                    <div className="builder-sidebar-header">
                        <span className="builder-sidebar-title">Deck ({totalQty}/60)</span>
                        <button className="btn btn-blue btn-xs" onClick={handleDownloadDeckPdf} disabled={exporting}>
                            {exporting ? "Exporting\u2026" : ICO_PAGE + " Export PDF"}
                        </button>
                        <button className="btn btn-green btn-xs" onClick={handleDownloadDeckXls} disabled={exporting}>
                            {exporting ? "Exporting\u2026" : ICO_XLS + " Export XLS"}
                        </button>
                    </div>
                    {exporting && (
                        <div className="export-progress-wrap">
                            <div className="export-progress-track">
                                <div className="export-progress-fill" style={{ width: exportProgress + "%" }} />
                            </div>
                            <div className="export-progress-label">{exportProgress}%</div>
                        </div>
                    )}

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

            {/* Import Modal */}
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
