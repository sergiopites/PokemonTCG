import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

var ICO_WARN = "\u26A0\uFE0F";

function EnergyIcon({ type }) {
    if (!type) return null;
    var MAP = {
        fire: "fire.png", water: "water.png", grass: "grass.png",
        fairy: "fairy.png", lightning: "lightning.png", psychic: "psychic.png",
        fighting: "fighting.png", darkness: "darkness.png",
        steel: "steel.png", metal: "steel.png",
        dragon: "dragon.png", colorless: "colorless.png",
    };
    var file = MAP[type.toLowerCase().trim()];
    if (!file) return <span style={{ color: "var(--text-dim)" }}>?</span>;
    return <img src={"/icons/" + file} alt={type} className="energy-icon-v2" />;
}

export default function CardDetail() {
    const { cardId } = useParams();
    const [card, setCard] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [backImagePath, setBackImagePath] = useState("");
    const [exporting, setExporting] = useState(false);
    const [exportProgress, setExportProgress] = useState(0);
    const API_URL = import.meta.env.VITE_API_URL;

    useEffect(() => {
        const fetchCard = async () => {
            setLoading(true);
            try {
                const res = await fetch(API_URL + "/api/card/cardid/" + cardId);
                if (!res.ok) throw new Error("HTTP " + res.status);
                const data = await res.json();
                setCard(data[0]);
            } catch (err) {
                console.error(err);
                setError(ICO_WARN + " The card could not be loaded.");
            } finally {
                setLoading(false);
            }
        };
        fetchCard();
    }, [cardId, API_URL]);

    useEffect(() => {
        fetch("/config.json")
            .then(function(r) { return r.json(); })
            .then(function(c) { setBackImagePath(c.backImagePath); })
            .catch(function() { setBackImagePath(""); });
    }, []);

    var handleDownloadPdf = async function() {
        if (!card?.imageLarge) { alert("Card was not found"); return; }
        setExporting(true); setExportProgress(0);
        var safe = (card.cardId + "_" + card.supertype + "_" + card.name).replace(/\s+/g, "_").replace(/[^\w\-\.]/g, "");
        try {
            var r = await fetch(API_URL + "/api/printer/generate/progress", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ imageUrls: [card.imageLarge, backImagePath], fileName: safe }),
            });
            if (!r.ok) throw new Error("PDF error");
            var reader = r.body.getReader();
            var decoder = new TextDecoder();
            var buf = "";
            var pdfData = null;
            var pdfFileName = safe + ".pdf";
            while (true) {
                var chunk = await reader.read();
                if (chunk.done) break;
                buf += decoder.decode(chunk.value, { stream: true });
                var lines = buf.split("\n");
                buf = lines.pop();
                for (var li = 0; li < lines.length; li++) {
                    var line = lines[li];
                    if (!line.startsWith("data: ")) continue;
                    try {
                        var evt = JSON.parse(line.slice(6));
                        if (evt.progress != null) setExportProgress(evt.progress);
                        if (evt.done && evt.pdf) { pdfData = evt.pdf; pdfFileName = evt.fileName || pdfFileName; }
                        if (evt.error) throw new Error(evt.error);
                    } catch (pe) { if (pe.message && !pe.message.includes("JSON")) throw pe; }
                }
            }
            if (!pdfData) throw new Error("No PDF received");
            setExportProgress(100);
            var byteChars = atob(pdfData);
            var byteArr = new Uint8Array(byteChars.length);
            for (var k = 0; k < byteChars.length; k++) byteArr[k] = byteChars.charCodeAt(k);
            var blob = new Blob([byteArr], { type: "application/pdf" });
            var a = document.createElement("a");
            a.href = URL.createObjectURL(blob);
            a.download = pdfFileName;
            a.click();
            URL.revokeObjectURL(a.href);
        } catch (e) {
            console.error(e);
            alert("Could not download the card.");
        } finally {
            setExporting(false); setExportProgress(0);
        }
    };

    if (loading) return (
        <div className="loading-screen">
            <div className="pokeball-spinner" />
            <span className="loading-text">Loading\u2026</span>
        </div>
    );
    if (error) return <div className="page-content-v2"><div className="msg-error">{error}</div></div>;
    if (!card) return null;

    var parseCost = function(raw) {
        try {
            if (typeof raw === "string") return JSON.parse(raw);
            if (Array.isArray(raw)) return raw;
        } catch (e) { /* ignore */ }
        return [];
    };

    return (
        <div className="page-content-v2">
            <div className="detail-layout">
                <div className="detail-image-col">
                    {card.imageLarge
                        ? <img src={card.imageLarge} alt={card.name} />
                        : <p style={{ color: "var(--text-dim)" }}>No Image</p>}
                    <div className="detail-meta">
                        {card.setSymbol && (
                            <img src={card.setSymbol} alt="" style={{ width: 28, display: "inline-block", verticalAlign: "middle", marginRight: 6 }} />
                        )}
                        {card.number && <span>{card.number}/{card.setTotal}</span>}
                        {card.artist && <div style={{ marginTop: 4 }}>Illustration: {card.artist}</div>}
                    </div>
                </div>

                <div className="detail-info">
                    <div style={{ display: "flex", alignItems: "center", gap: 16 }}>
                        <h1 className="detail-name" style={{ flex: 1 }}>{card.name}</h1>
                        {card.setImage && <img src={card.setImage} alt={card.setName} style={{ height: 48 }} />}
                    </div>

                    <div className="detail-badges">
                        {card.supertype && <span className="badge badge-type">{card.supertype}</span>}
                        {card.type && <span className="badge badge-type">{card.type}</span>}
                        {card.subtype && <span className="badge badge-type">{card.subtype}</span>}
                        {card.hp != null && card.hp !== 0 && <span className="badge badge-hp">HP {card.hp}</span>}
                        {card.rarity && <span className="badge badge-rarity">{card.rarity}</span>}
                    </div>

                    {(card.evolvesFrom || card.evolvesTo) && (
                        <div style={{ color: "var(--text-muted)", fontSize: ".9rem" }}>
                            {card.evolvesFrom && <div>Evolves from <strong style={{ color: "#fff" }}>{card.evolvesFrom}</strong></div>}
                            {card.evolvesTo && <div>Evolves to <strong style={{ color: "#fff" }}>{card.evolvesTo}</strong></div>}
                        </div>
                    )}

                    <div className="detail-section">
                        <div className="stat-row">
                            <div className="stat-item">
                                <span className="stat-label">Weakness</span>
                                {card.weaknessDetails?.length > 0
                                    ? card.weaknessDetails.map(function(w, i) { return <span key={i}><EnergyIcon type={w.type} /> {w.value}</span>; })
                                    : <span style={{ color: "var(--text-dim)" }}>{"\u2014"}</span>}
                            </div>
                            <div className="stat-item">
                                <span className="stat-label">Resistance</span>
                                {card.resistanceDetails?.length > 0
                                    ? card.resistanceDetails.map(function(r, i) { return <span key={i}><EnergyIcon type={r.type} /> {r.value}</span>; })
                                    : <span style={{ color: "var(--text-dim)" }}>{"\u2014"}</span>}
                            </div>
                            <div className="stat-item">
                                <span className="stat-label">Retreat</span>
                                {(function() {
                                    var costs = parseCost(card.retreatCost);
                                    return costs.length > 0
                                        ? costs.map(function(c, i) { return <EnergyIcon key={i} type={c} />; })
                                        : <span style={{ color: "var(--text-dim)" }}>{"\u2014"}</span>;
                                })()}
                            </div>
                        </div>
                    </div>

                    {card.abilityDetails?.length > 0 && (
                        <div className="detail-section">
                            <div className="detail-section-title">Abilities</div>
                            {card.abilityDetails.map(function(ab, i) {
                                return (
                                    <div key={i} className="ability-card">
                                        <div className="ability-header">
                                            <span className="ability-tag">Ability</span>
                                            <span className="ability-name">{ab.name}</span>
                                        </div>
                                        <div className="ability-text">{ab.text}</div>
                                    </div>
                                );
                            })}
                        </div>
                    )}

                    {card.attackDetails?.length > 0 && (
                        <div className="detail-section">
                            <div className="detail-section-title">Attacks</div>
                            {card.attackDetails.map(function(atk, i) {
                                return (
                                    <React.Fragment key={i}>
                                        <div className="attack-row">
                                            <div className="attack-cost">
                                                {parseCost(atk.attackCost).map(function(c, j) { return <EnergyIcon key={j} type={c} />; })}
                                            </div>
                                            <span className="attack-name">{atk.attackName}</span>
                                            <span className="attack-dmg">{atk.attackDamage ?? ""}</span>
                                        </div>
                                        {atk.attackDescription && <div className="attack-desc">{atk.attackDescription}</div>}
                                    </React.Fragment>
                                );
                            })}
                        </div>
                    )}

                    {card.rule && (
                        <div className="detail-section">
                            <div className="detail-section-title">Rules</div>
                            <p style={{ color: "var(--text-muted)", fontStyle: "italic" }}>{card.rule}</p>
                        </div>
                    )}

                    <div className="detail-section" style={{ display: "flex", gap: 12, flexWrap: "wrap", alignItems: "center" }}>
                        {card.tcgPlayerUrl && (
                            <button className="btn btn-outline btn-sm" onClick={function() { window.open(card.tcgPlayerUrl, "_blank"); }}>
                                {"\uD83D\uDCB0"} TCG Player Prices
                            </button>
                        )}
                        <button className="btn btn-gold btn-sm" onClick={handleDownloadPdf} disabled={exporting}>
                            {exporting ? "Exporting\u2026" : "\uD83D\uDCE5 Export PDF"}
                        </button>
                    </div>
                    {exporting && (
                        <div className="export-progress-wrap" style={{ marginTop: 8 }}>
                            <div className="export-progress-track">
                                <div className="export-progress-fill" style={{ width: exportProgress + "%" }} />
                            </div>
                            <div className="export-progress-label">{exportProgress}%</div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
