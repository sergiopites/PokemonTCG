import React, { useState, useEffect } from "react";

var ICO_WARN = "\u26A0\uFE0F";
var ICO_OK   = "\u2705";
var ICO_FAIL = "\u274C";
var ICO_SAVE = "\uD83D\uDCBE";
var ICO_PAGE = "\uD83D\uDCC4";
var ICO_XLS  = "\uD83D\uDCCA";

export default function UpdateCollection() {
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
    const [exportProgress, setExportProgress] = useState(0);
    const [backImagePath, setBackImagePath] = useState("");
    const [collections, setCollections] = useState([]);
    const [selectedCollectionId, setSelectedCollectionId] = useState("");

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

    useEffect(() => { fetch(API_URL + "/api/collection/all").then(r => r.json()).then(d => setCollections(Array.isArray(d) ? d : [])).catch(console.error); }, [API_URL]);
    useEffect(() => { fetch(API_URL + "/api/card/filters").then(r => r.json()).then(d => { setAvailableRarities(norm(d?.rarities ?? d?.Rarities ?? [], ["rarity"])); setAvailableSubTypes(norm(d?.subtypes ?? d?.SubTypes ?? [], ["subtype"])); setAvailableTypes(norm(d?.types ?? d?.Types ?? [], ["type"])); setAvailableSuperTypes(norm(d?.supertypes ?? d?.superTypes ?? d?.SuperTypes ?? [], ["supertype"])); }).catch(console.error); }, [API_URL]);
    useEffect(() => { fetch(API_URL + "/api/set/all").then(r => r.json()).then(d => setAvailableSets(Array.isArray(d) ? d : [])).catch(console.error); }, [API_URL]);

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
            var t = Number(d?.totalCount ?? 0), s = Number(d?.pageSize ?? 55);
            setTotalPages(s > 0 ? Math.max(1, Math.ceil(t / s)) : 1);
        }).catch(console.error);
    }, [search, number, setId, supertype, type, subtype, rarity, page, API_URL]);

    useEffect(() => { fetch("/config.json").then(r => r.json()).then(c => setBackImagePath(c.backImagePath)).catch(() => setBackImagePath("")); }, []);

    const handleSelectCollection = async (collectionId) => {
        setSelectedCollectionId(collectionId); setMessage("");
        if (!collectionId) { setName(""); setDescription(""); setSelectedCards([]); return; }
        try {
            const r = await fetch(API_URL + "/api/collection/" + collectionId);
            if (!r.ok) throw new Error("Error");
            const d = await r.json();
            setName(d.name || ""); setDescription(d.description || "");
            setSelectedCards((d.cards || []).map(c => ({ cardId: c.cardId, name: c.name ?? c.cardId, supertype: c.supertype ?? "", subtype: c.subtype ?? "", quantity: c.quantity, ptcgocode: c.ptcgocode ?? "", number: c.number ?? "", setName: c.setName ?? "", setId: c.setId ?? "", imageLarge: c.imageLarge ?? "", type: c.type ?? "", rarity: c.rarity ?? "", artist: c.artist ?? "", hp: c.hp ?? null, evolvesFrom: c.evolvesFrom ?? "", evolvesTo: c.evolvesTo ?? "" })));
        } catch (e) { console.error(e); setMessage(ICO_FAIL + " Error loading collection."); }
    };

    const toggleCard = (cardOrId) => {
        const id = typeof cardOrId === "string" ? cardOrId : cardOrId.cardId;
        const obj = typeof cardOrId === "object" ? cardOrId : cards.find(c => c.cardId === id) || {};
        setSelectedCards(prev => {
            const found = prev.find(c => c.cardId === id);
            if (found) return prev.map(c => c.cardId === id ? { ...c, quantity: c.quantity + 1 } : c);
            return [...prev, { cardId: id, name: obj?.name ?? "Unknown", supertype: obj?.supertype ?? "", subtype: obj?.subtype ?? "", quantity: 1, ptcgocode: obj?.ptcgocode ?? "", number: obj?.number ?? "", setName: obj?.setName ?? "", setId: obj?.setId ?? "", imageLarge: obj?.imageLarge ?? "", type: obj?.type ?? "", rarity: obj?.rarity ?? "", artist: obj?.artist ?? "", hp: obj?.hp ?? null, evolvesFrom: obj?.evolvesFrom ?? "", evolvesTo: obj?.evolvesTo ?? "" }];
        });
    };

    const removeCard = (cardId) => setSelectedCards(prev => { const f = prev.find(c => c.cardId === cardId); if (!f) return prev; if (f.quantity > 1) return prev.map(c => c.cardId === cardId ? { ...c, quantity: c.quantity - 1 } : c); return prev.filter(c => c.cardId !== cardId); });
    const deleteCard = (cardId) => setSelectedCards(prev => prev.filter(c => c.cardId !== cardId));
    const clearSelection = () => { setSelectedCollectionId(""); setName(""); setDescription(""); setMessage(""); setSelectedCards([]); };
    const totalQty = selectedCards.reduce((s, c) => s + (Number(c.quantity) || 0), 0);

    const handleSubmit = async () => {
        if (!selectedCollectionId) { setMessage(ICO_WARN + " Select a collection to update."); return; }
        if (!name.trim()) { setMessage(ICO_WARN + " Enter a collection name."); return; }
        if (!selectedCards.length) { setMessage(ICO_WARN + " Select at least one card."); return; }
        try {
            const r = await fetch(API_URL + "/api/collection/update", { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ collectionId: Number(selectedCollectionId), name, description, cards: selectedCards.map(c => ({ cardId: String(c.cardId), quantity: Number(c.quantity) })) }) });
            if (!r.ok) throw new Error(await r.text());
            setMessage(ICO_OK + " Collection \"" + name + "\" updated!");
            const rr = await fetch(API_URL + "/api/collection/all"); if (rr.ok) { const d = await rr.json(); setCollections(Array.isArray(d) ? d : []); }
        } catch (e) { setMessage(ICO_FAIL + " Error updating collection."); console.error(e); }
    };

    const handleExportPdf = async () => {
        if (!selectedCards.length) { alert("No cards to export."); return; }
        setExporting(true); setExportProgress(0);
        try {
            const urls = [];
            for (const c of selectedCards) for (var i = 0; i < (c.quantity || 1); i++) if (c.imageLarge) urls.push(c.imageLarge);
            if (backImagePath) { var n = urls.length; for (var j = 0; j < n; j++) urls.push(backImagePath); }
            const safe = (name?.trim() || "Collection").replace(/\s+/g, "_").replace(/[^\w\-\.]/g, "");
            const r = await fetch(API_URL + "/api/printer/generatedeck/progress", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ imageUrls: urls, fileName: safe }) });
            if (!r.ok) throw new Error("PDF error");
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

    const handleExportXls = async () => {
        if (!selectedCards.length) { alert("No cards to export."); return; }
        setExporting(true); setExportProgress(0);
        try {
            const cardsPayload = selectedCards.map(c => ({ cardId: c.cardId || "", name: c.name || "", supertype: c.supertype || "", subtype: c.subtype || "", type: c.type || "", rarity: c.rarity || "", setName: c.setName || "", setId: c.setId || "", number: c.number || "", artist: c.artist || "", ptcgocode: c.ptcgocode || "", quantity: c.quantity || 1, imageLarge: c.imageLarge || "", hp: c.hp ?? null, evolvesFrom: c.evolvesFrom || "", evolvesTo: c.evolvesTo || "" }));
            const safe = (name?.trim() || "Collection").replace(/\s+/g, "_").replace(/[^\w\-\.]/g, "");
            const r = await fetch(API_URL + "/api/printer/generateexcel/progress", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ cards: cardsPayload, fileName: safe }) });
            if (!r.ok) throw new Error("XLS error");
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

    const fi = (label, value, setter, ph) => (<div className="field-group"><label className="field-label">{label}</label><input className="field-input" placeholder={ph} value={value} onChange={e => { setPage(1); setter(e.target.value); }} /></div>);
    const fs = (label, value, setter, opts, all) => (<div className="field-group"><label className="field-label">{label}</label><select className="field-select" value={value} onChange={e => { setPage(1); setter(e.target.value); }}><option value="">{all}</option>{opts.map((o, i) => <option key={i} value={typeof o === "object" ? (o.setId ?? o.id ?? i) : o}>{typeof o === "object" ? (o.name ?? String(o.setId)) : o}</option>)}</select></div>);

    const msgClass = message.indexOf(ICO_OK) >= 0 ? "msg-success" : message.indexOf(ICO_FAIL) >= 0 ? "msg-error" : "msg-warning";

    return (
        <div className="page-content-v2">
            <h1 className="section-title">Update Collection</h1>
            <div className="builder-layout">
                <div>
                    <div className="glass-panel" style={{ marginBottom: 16 }}>
                        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 12 }}>
                            <div className="field-group">
                                <label className="field-label">Collection</label>
                                <select className="field-select" value={selectedCollectionId} onChange={e => handleSelectCollection(e.target.value)}>
                                    <option value="">{"\u2014"} Select {"\u2014"}</option>
                                    {collections.map(c => <option key={c.collectionId} value={c.collectionId}>{c.name}</option>)}
                                </select>
                            </div>
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
                            <button className="btn btn-green btn-sm" onClick={handleSubmit}>{ICO_SAVE} Update Collection</button>
                        </div>
                        {message && <p className={msgClass} style={{ marginTop: 10 }}>{message}</p>}
                    </div>
                    <div className="glass-panel" style={{ marginBottom: 16 }}>
                        <div className="filter-grid">
                            {fi("Number", number, setNumber, "#")}
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
                        <span className="builder-sidebar-title">Collection</span>
                        <div className="builder-count">Unique: <strong>{selectedCards.length}</strong> {"\u00B7"} Total: <strong>{totalQty}</strong></div>
                    </div>
                    <div style={{ marginBottom: 10, display: "flex", justifyContent: "flex-end", gap: 6 }}>
                        <button className="btn btn-blue btn-xs" onClick={handleExportPdf} disabled={exporting}>
                            {exporting ? "Exporting\u2026" : ICO_PAGE + " Export PDF"}
                        </button>
                        <button className="btn btn-green btn-xs" onClick={handleExportXls} disabled={exporting}>
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
