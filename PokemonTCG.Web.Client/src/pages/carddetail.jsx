import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import "../global.css";
import { Link } from "react-router-dom";
import abilityIcon from "../images/abilityIcon.png";

export default function CardDetail() {
    const { cardId } = useParams();
    const [card, setCard] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [backImagePath, setBackImagePath] = useState("");

    useEffect(() => {
        const fetchCard = async () => {
            setLoading(true);
            try {
                const res = await fetch(
                    `http://localhost:5202/api/cards/getcardsbycardid/${cardId}`
                );
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                const data = await res.json();
                console.log("👉 Card recibido:", data);

                setCard(data[0]);

            } catch (err) {
                console.error(err);
                setError("⚠️ The card could not be loaded.");
            } finally {
                setLoading(false);
            }
        };
        fetchCard();
    }, [cardId]);

    useEffect(() => {
        // Cargar la configuración al montar el componente
        fetch("/config.json")
            .then(res => res.json())
            .then(config => setBackImagePath(config.backImagePath))
            .catch(() => setBackImagePath("")); // Valor por defecto si falla
    }, []);

    if (loading) return <div className="loading">⏳ Loading...</div>;
    if (error) return <div className="error">{error}</div>;
    if (!card) return null;
    const handleDownloadPdf = async () => {
        if (!card || !card.imageLarge) {
            alert("Card was not found");
            return;
        }

        const safeFileName = `${card.cardId}_${card.supertype}_${card.name}`
            .replace(/\s+/g, "_")
            .replace(/[^\w\-\.]/g, "");

        try {
            const response = await fetch("http://localhost:5202/api/printer/generate", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    imageUrls: [
                        card.imageLarge,
                        backImagePath,
                    ],
                    fileName: safeFileName,
                }),
            });

            if (!response.ok) {
                throw new Error(`Error generating PDF: ${response.status} - ${response.statusText}`);
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
                        
            const a = document.createElement("a");
            a.href = url;
            a.download = `${safeFileName}.pdf`;
            a.click();
                        
            window.URL.revokeObjectURL(url);

        } catch (error) {
            console.error("PDF download error:", error);
            alert("The card could not be downloaded"); //  O usa un mensaje más amigable
        }

    };

    return (
        <div className="page-content">
            <div className="card-detail-container">
                <div className="card-detail-image">
                    <table width="100%">
                        <tbody>
                            <tr>
                                <td colSpan={2} width="50%" style={{ textAlign: "center", verticalAlign: "middle" }}>                                
                                    {card.imageLarge ? (
                                        <img
                                            src={card.imageLarge}
                                            alt={card.name}
                                            className="shadow-lg rounded"
                                            style={{ maxWidth: "100%", height: "auto" }}
                                        />
                                    ) : (
                                        <p>No Image</p>
                                    )}
                                </td>
                            </tr>
                            <tr>
                                <td width="50%" style={{ textAlign: "right", verticalAlign: "middle" }}>
                                    {card.setSymbol && (
                                        <img
                                            src={card.setSymbol}
                                            alt={card.setName}
                                            className="object-contain"
                                            style={{ width: "35px", height: "auto" }}
                                        />
                                    )}
                                </td>
                                <td style={{ textAlign: "left", verticalAlign: "top" }}>                                
                                    {card.number && (
                                        <span> {card.number}/{card.setTotal}</span>
                                    )}
                                </td>
                            </tr>
                            <tr>
                                <td
                                    colSpan={2}
                                    style={{ textAlign: "center", verticalAlign: "middle", paddingTop: "5px" }}
                                >
                                    {card.artist && (
                                        <span> Illustration: {card.artist}</span>
                                    )}
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                {/* Info de la carta */}
                <div className="card-detail-info">
                    <table width="100%" className="pokemon-tcg-text" >
                        <tr><td width="80%">
                            <h2 className="text-2xl font-bold">{card.name}</h2>
                        </td>
                            <td width="20%">
                                {card.setImage && (
                                    <img
                                        src={card.setImage}
                                        alt={card.setName}
                                        className="object-contain"
                                        style={{ width: "120px", height: "auto" }}
                                    />
                                )}
                            </td>
                        </tr>
                        <tr>
                            <td>
                                {card.type ? (
                                    <span>
                                        {card.supertype} · {card.type} · {card.subtype}
                                    </span>
                                ) : (
                                        <span>
                                        {card.supertype} · {card.subtype}
                                    </span>
                                )}
                            </td>
                        </tr>
                        <tr>
                            <td>
                                {card.hp != null && card.hp !== 0 && (
                                    <>
                                        <span>HP {card.hp}</span>
                                    </>
                                )}
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <span> {card.rarity}</span>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                {card.evolvesFrom && (
                                    <>
                                        <span>Evolves from {card.evolvesFrom}</span>
                                    </>
                                )}
                            </td>
                        </tr>
                        <tr>
                            <td>
                                {card.evolvesTo && (
                                    <>
                                        <span>Evolves to {card.evolvesTo}</span>
                                    </>
                                )}
                            </td>
                        </tr>
                    </table>
                    <table width="100%" className="section pokemon-tcg-text" >
                        <tr>
                            <td width="33%">
                                <span>Weakness: </span>
                                {card.weaknessDetails && card.weaknessDetails.length > 0 ? (
                                    card.weaknessDetails.map((w, index) => (
                                        <span key={index} className="ml-1">
                                            <EnergyIcon type={w.type} /> {w.value}
                                        </span>
                                    ))
                                ) : (
                                    <span className="text-gray-500">--</span>
                                )}

                            </td>
                            <td width="33%">                               
                                <span>Resistance: </span>
                                    {card.resistanceDetails && card.resistanceDetails.length > 0 ? (
                                        card.resistanceDetails.map((res, index) => (
                                            <span key={index} className="ml-1">
                                                <EnergyIcon type={res.type} /> {res.value}
                                            </span>
                                        ))
                                    ) : (
                                        <span className="text-gray-500">--</span>
                                )}
                            </td>
                            <td width="33%">                                
                                <span>Retreat: </span>
                                {(() => {
                                    let costs = [];
                                    try {
                                        if (typeof card.retreatCost === "string") {
                                            costs = JSON.parse(card.retreatCost);
                                        } else if (Array.isArray(card.retreatCost)) {
                                            costs = card.retreatCost;
                                        }
                                    } catch {
                                        costs = [];
                                    }

                                    if (!costs || costs.length === 0) {
                                        return <span className="text-gray-500">--</span>;
                                    }

                                    return costs.map((cost, i) => <EnergyIcon key={i} type={cost} />);
                                })()}
                            </td>
                        </tr>
                    </table>
                    {card.abilityDetails?.length > 0 && (
                        <table className="section pokemon-tcg-text" style={{ width: "100%" }}>
                            <tbody>
                                {card.abilityDetails.map((ab, i) => (
                                    <React.Fragment key={i}>
                                        <tr>
                                            <td style={{ width: "50px", verticalAlign: "top", textAlign: "left", padding: "4px" }}>
                                                <img
                                                    src={abilityIcon}
                                                    alt="Ability Icon"
                                                    style={{ width: "70px", height: "19px" }}
                                                />
                                            </td>
                                            <td style={{ verticalAlign: "middle", padding: "4px" }}>
                                                <span style={{ color: "red" }}>{ab.name}</span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colSpan="2" style={{ padding: "6px", verticalAlign: "top" }}>
                                                {ab.text}
                                            </td>
                                        </tr>
                                    </React.Fragment>
                                ))}
                            </tbody>
                        </table>
                    )}

                    {card.attackDetails?.length > 0 && (
                        card.attackDetails.map((atk, i) => (
                            <div key={i} className="mb-2">
                                <table width="100%" className="section pokemon-tcg-text">
                                    <tbody>
                                        <tr>
                                            <td width="33%" style={{ textAlign: "center" }}>
                                                {" "}
                                                {(() => {
                                                    let costs = [];
                                                    try {
                                                        if (typeof atk.attackCost === "string") {
                                                            costs = JSON.parse(atk.attackCost);
                                                        } else if (Array.isArray(atk.attackCost)) {
                                                            costs = atk.attackCost;
                                                        }
                                                    } catch {
                                                        costs = [];
                                                    }
                                                    return costs.length > 0
                                                        ? costs.map((cost, i) => <EnergyIcon key={i} type={cost} />)
                                                        : <span className="text-gray-300"></span >;
                                                })()}
                                            </td>
                                            <td width="33%" style={{ textAlign: "center" }}>
                                                <span>{atk.attackName}</span>
                                            </td>

                                            <td width="33%" style={{ textAlign: "center" }}>
                                                <span>{atk.attackDamage ?? "N/A"}</span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="3">
                                                {atk.attackDescription}
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        ))
                    )}

                    {card.rule && (
                        <div className="section pokemon-tcg-text">
                            <span>  {card.rule}</span>
                        </div>
                    )}
                    <table width="100%" className="section">
                        <tr>
                            <td colSpan={2} style={{ textAlign: "center", verticalAlign: "middle" }}>
                                <Link to={`/cards/${card.setId}`}>
                                    <p>⬅️ Back</p>
                                </Link>
                            </td><td style={{ textAlign: "center", verticalAlign: "middle" }} >
                                {card.tcgPlayerUrl && (
                                    <p className="mt-4 text-right">
                                        <button
                                            onClick={() => window.open(card.tcgPlayerUrl, '_blank').focus}
                                            className="inline-flex items-center gap-1 text-gray-800 font-semibold underline hover:text-gray-600 transition-colors duration-200">
                                            View TCG Player Prices
                                        </button>
                                    </p>
                                )}
                            </td><td style={{ textAlign: "center", verticalAlign: "middle" }} >
                                <p className="mt-4 text-right">
                                    <button onClick={handleDownloadPdf} className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
                                        📥 Export PDF
                                    </button>
                                </p>
                            </td>                       

                        </tr></table>
                </div>
            </div>
        </div >
    );
}

function EnergyIcon({ type }) {
    if (!type) return null;
    const ICON_MAP = {
        fire: "fire.png",
        water: "water.png",
        grass: "grass.png",
        fairy: "fairy.png",
        lightning: "lightning.png",
        psychic: "psychic.png",
        fighting: "fighting.png",
        darkness: "darkness.png",
        steel: "steel.png",
        metal: "steel.png",
        dragon: "dragon.png",        
        colorless: "colorless.png",
    };
    const key = type.toLowerCase().trim();
    const filename = ICON_MAP[key];
    if (!filename) return <span className="text-gray-400">?</span>;
    const src = `/icons/${filename}`;
    return <img src={src} alt={type} className="w-6 h-6 inline-block" />;
}